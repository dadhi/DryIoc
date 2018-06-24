using System;
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
        public void Generate_asResolutionCall_dependency_should_not_contain_Resolve_call()
        {
            var container = new Container();

            container.Register<R>();
            container.Register<Dep>(setup: Setup.With(asResolutionCall: true));

            var exprs = container.GenerateResolutionExpressions(ServiceInfo.Of<R>());

            Assert.IsEmpty(exprs.Errors);
            Assert.AreEqual(1, exprs.Roots.Count);
            Assert.AreEqual(1, exprs.ResolveDependencies.Count);

            var depExpr = exprs.ResolveDependencies[0].ToString();
            Assert.IsFalse(depExpr.Contains(".Resolve "));
            Assert.IsTrue( depExpr.Contains("new "));
        }

        public class R
        {
            public readonly Dep Dep;
            public R(Dep d) { Dep = d; } 
        }

        public class Dep { }

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

            var depList = container.Rules.DependencyResolutionCallExprs.Value.Enumerate().ToArray();
            Assert.AreEqual(2, depList.Length);
        }

        [Test]
        public void Can_generate_expression_with_recursive_Lazy_dependency_by_hand()
        {
            IContainer container = new Container(rules => rules
                .WithoutEagerCachingSingletonForFasterAccess()
                .WithDependencyResolutionCallExpressions());

            container.Register<LazyUser>();
            container.Register<LazyDep>();

            var request = Request.Create(container, typeof(LazyUser));
            var factory = container.GetServiceFactoryOrDefault(request);
            factory.GetExpressionOrDefault(request);

            var depList = container.Rules.DependencyResolutionCallExprs.Value.Enumerate().ToArray();
            Assert.AreEqual(1, depList.Length);
        }

        [Test]
        public void Can_generate_expression_with_recursive_Lazy_dependency()
        {
            var container = new Container();

            container.Register<LazyUser>();
            container.Register<LazyDep>();

            var result = container.GenerateResolutionExpressions(ServiceInfo.Of<LazyUser>());

            Assert.AreEqual(1, result.Roots.Count);
            Assert.AreEqual(1, result.ResolveDependencies.Count);
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

            var depList = container.Rules.DependencyResolutionCallExprs.Value.Enumerate().ToArray();
            Assert.AreEqual(2, depList.Length);
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

            var result = container.GenerateResolutionExpressions(r => r.AsResolutionRoot);

            Assert.AreEqual(3, result.Roots.Count);
            Assert.AreEqual(2, result.ResolveDependencies.Count);
        }

        [Test]
        public void I_can_setup_to_throw_on_generating_expressions_with_runtime_state()
        {
            var container = new Container()
                .WithMefAttributedModel()
                .With(rules => rules.WithThrowIfRuntimeStateRequired());

            container.RegisterDelegate(resolver => "runtime state");

            var result = container.GenerateResolutionExpressions();

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(DryIoc.Error.StateIsRequiredToUseItem, result.Errors[0].Value.Error);
            Assert.AreEqual(0, result.Roots.Count);
            Assert.AreEqual(0, result.ResolveDependencies.Count);
        }

        [Test]
        public void Instance_and_Delegate_should_be_converted_to_Resolve_calls()
        {
            var container = new Container();

            container.RegisterDelegate(_ => "a string message");
            var n = new N();
            container.UseInstance(n);

            container.Register<K>(setup: Setup.With(asResolutionRoot: true));

            var result = container.GenerateResolutionExpressions(r => r.AsResolutionRoot);

            Assert.IsEmpty(result.Errors);
            Assert.AreEqual(1, result.Roots.Count);
            Assert.AreEqual(0, result.ResolveDependencies.Count);
        }

        public class K
        {
            public string M { get; }
            public N N { get; }

            public K(N n, string m)
            {
                M = m;
                N = n;
            }
        }

        public class N { }

        public class LazyUser
        {
            public LazyUser(Lazy<LazyDep> s) {}
        }

        public class LazyDep
        {
            public LazyDep(LazyUser u) {}
        }

        [Test]
        public void Can_specify_resolution_root_with_Service_type_and_key_instead_of_predicate()
        {
            var container = new Container();

            container.RegisterDelegate(_ => "a string message");
            var n = new N();
            container.UseInstance(n);

            container.Register(typeof(K<>));

            var result = container.GenerateResolutionExpressions(ServiceInfo.Of<K<N>>());

            Assert.IsEmpty(result.Errors);
            Assert.AreEqual(1, result.Roots.Count);
            Assert.AreEqual(0, result.ResolveDependencies.Count);
        }

        public class K<T>
        {
            public string M { get; }
            public T N { get; }

            public K(T n, string m)
            {
                M = m;
                N = n;
            }
        }

        [Test]
        public void No_errors_for_registered_placeholders()
        {
            var c = new Container();

            c.Register<M>();
            c.RegisterPlaceholder(typeof(IN));

            var result = c.GenerateResolutionExpressions(ServiceInfo.Of<M>());
            Assert.IsEmpty(result.Errors);
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
