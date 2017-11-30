using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using ExpressionToCodeLib.Unstable_v2_Api;
using NUnit.Framework;
using ImTools;

#if FEC_EXPRESSION_INFO
using Expr = FastExpressionCompiler.ExpressionInfo;
#else
using Expr = System.Linq.Expressions.Expression;
#endif

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class GenerateFactoryDelegateTests
    {
        [Test]
        public void Can_load_types_from_assembly_and_generate_some_resolutions()
        {
            var container = new Container().WithMef().With(rules => rules
                .WithoutEagerCachingSingletonForFasterAccess());

            var types = typeof(BirdFactory).GetAssembly().GetLoadedTypes();
            container.RegisterExports(types);

            var r = container.GetServiceRegistrations().FirstOrDefault(x => x.ServiceType == typeof(Chicken));
            var factoryExpr = container.Resolve<LambdaExpression>(r.OptionalServiceKey, IfUnresolved.Throw, r.ServiceType);

            Assert.DoesNotThrow(() => ExpressionStringify.With(true, true).ToCode(factoryExpr));
        }

        [Test]
        public void Generate_factory_delegate_for_exported_static_factory_method()
        {
            var container = new Container(rules => rules
                .WithoutEagerCachingSingletonForFasterAccess()
                .WithMefAttributedModel());

            container.RegisterExports(typeof(BirdFactory));

            var r = container.GetServiceRegistrations().FirstOrDefault(x => x.ServiceType == typeof(Chicken));
            var factoryExpr = container.Resolve<LambdaExpression>(r.OptionalServiceKey, IfUnresolved.Throw, r.ServiceType);

            Assert.DoesNotThrow(() => ExpressionStringify.With(true, true).ToCode(factoryExpr));
        }

        [Test]
        public void Can_generate_expression_for_all_ResolutionCalls()
        {
            IContainer container = new Container(rules => rules
                .WithMefAttributedModel()
                .WithoutEagerCachingSingletonForFasterAccess()
                .WithDependencyResolutionCallExpressions());

            container.RegisterExports(
                typeof(ImportConditionObject1),
                typeof(ImportConditionObject2),
                typeof(ImportConditionObject3),
                typeof(ExportConditionalObject1),
                typeof(ExportConditionalObject2),
                typeof(ExportConditionalObject3));

            var serviceRegistrations = container.GetServiceRegistrations()
                .Where(r => r.ServiceType.Name.Contains("Import"))
                .ToArray();

            var roots = ImHashMap<KV<Type, object>, Expr>.Empty;
            foreach (var r in serviceRegistrations)
            {
                var request = Request.Create(container, r.ServiceType, r.OptionalServiceKey, IfUnresolved.ReturnDefault);
                var factoryExpr = r.Factory.GetExpressionOrDefault(request);
                if (factoryExpr != null)
                    roots = roots.AddOrUpdate(new KV<Type, object>(r.ServiceType, r.OptionalServiceKey), factoryExpr);
            }

            var rootList = roots.Enumerate().ToArray();
            Assert.AreEqual(3, rootList.Length);

            var depList = container.Rules.DependencyResolutionCallExpressions.Value.Enumerate().ToArray();
            Assert.AreEqual(3, depList.Length);
        }

        [Test]
        public void Can_generate_expression_with_recursive_Lazy_dependency()
        {
            IContainer container = new Container(rules => rules
                .WithoutEagerCachingSingletonForFasterAccess()
                .WithDependencyResolutionCallExpressions());

            container.Register<LazyUser>();
            container.Register<LazyDep>();

            var request = Request.Create(container, typeof(LazyUser));
            container.GetServiceFactoryOrDefault(request).GetExpressionOrDefault(request);

            var depList = container.Rules.DependencyResolutionCallExpressions.Value.Enumerate().ToArray();
            Assert.AreEqual(1, depList.Length);
        }

        [Test]
        public void Can_generate_expression_for_all_ResolutionRoots()
        {
            IContainer container = new Container(rules => rules
                .WithMefAttributedModel()
                .WithoutEagerCachingSingletonForFasterAccess()
                .WithDependencyResolutionCallExpressions());

            container.RegisterExports(
                typeof(ImportConditionObject1),
                typeof(ImportConditionObject2),
                typeof(ImportConditionObject3),
                typeof(ExportConditionalObject1),
                typeof(ExportConditionalObject2),
                typeof(ExportConditionalObject3));

            var serviceRegistrations = container.GetServiceRegistrations()
                .Where(r => r.Factory.Setup.AsResolutionRoot)
                .ToArray();

            var roots = ImHashMap<KV<Type, object>, Expr>.Empty;
            foreach (var r in serviceRegistrations)
            {
                var request = Request.Create(container, r.ServiceType, r.OptionalServiceKey, IfUnresolved.ReturnDefault);
                var factoryExpr = r.Factory.GetExpressionOrDefault(request);
                if (factoryExpr != null)
                    roots = roots.AddOrUpdate(new KV<Type, object>(r.ServiceType, r.OptionalServiceKey), factoryExpr);
            }

            var rootList = roots.Enumerate().ToArray();
            Assert.AreEqual(3, rootList.Length);

            var depList = container.Rules.DependencyResolutionCallExpressions.Value.Enumerate().ToArray();
            Assert.AreEqual(3, depList.Length);
        }

        [Test]
        public void Can_generate_expression_for_all_ResolutionRoots_and_Calls_with_dedicated_method()
        {
            var container = new Container().WithMefAttributedModel();

            container.RegisterExports(
                typeof(ImportConditionObject1),
                typeof(ImportConditionObject2),
                typeof(ImportConditionObject3),
                typeof(ExportConditionalObject1),
                typeof(ExportConditionalObject2),
                typeof(ExportConditionalObject3));

            KeyValuePair<ServiceRegistrationInfo, Expression<FactoryDelegate>>[] resolutionsRoots;
            KeyValuePair<RequestInfo, Expression>[] resolutionCallDependencies;
            container.GenerateResolutionExpressions(out resolutionsRoots, out resolutionCallDependencies, ContainerTools.SetupAsResolutionRoots);

            Assert.AreEqual(3, resolutionsRoots.Length);
            Assert.AreEqual(3, resolutionCallDependencies.Length);
        }

        [Test]
        public void I_can_setup_to_throw_on_generating_expressions_with_runtime_state()
        {
            var container = new Container()
                .WithMefAttributedModel()
                .With(rules => rules.WithThrowIfRuntimeStateRequired());

            container.RegisterDelegate(resolver => "runtime state");

            KeyValuePair<ServiceRegistrationInfo, Expression<FactoryDelegate>>[] resolutionsRoots;
            KeyValuePair<RequestInfo, Expression>[] resolutionCallDependencies;
            var errors = container.GenerateResolutionExpressions(out resolutionsRoots, out resolutionCallDependencies);

            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual(DryIoc.Error.StateIsRequiredToUseItem, errors[0].Value.Error);
            Assert.AreEqual(0, resolutionsRoots.Length);
            Assert.AreEqual(0, resolutionCallDependencies.Length);
        }

        public class LazyUser
        {
            public LazyUser(Lazy<LazyDep> s) {}
        }

        public class LazyDep
        {
            public LazyDep(LazyUser u) {}
        }

        [Test]
        public void No_errors_for_registered_placeholders()
        {
            var c = new Container();

            c.Register<M>();
            c.RegisterPlaceholder(typeof(IN));

            KeyValuePair<ServiceRegistrationInfo, Expression<FactoryDelegate>>[] roots;
            KeyValuePair<RequestInfo, Expression>[] deps;
            var errors = c.GenerateResolutionExpressions(out roots, out deps, 
                r => r.ServiceType == typeof(M));
            Assert.IsEmpty(errors);
        }

        class M
        {
            public IN N;
            public M(IN n)
            {
                N = n;
            }
        }

        interface IN { }
    }
}
