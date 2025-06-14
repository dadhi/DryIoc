using NUnit.Framework;
using Example;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue101_Compile_time_generated_object_graph : ITest
    {
        public int Run()
        {
            Resolve_compile_time_generated_example_service();
            Emulate_compile_time_generated_example_service_in_runtime();
            Can_resolve_the_struct_service_in_runtime_interpreted_and_compiled();
            Can_resolve_the_keyed_struct_service_in_runtime_interpreted_and_compiled();
            Can_resolve_service_with_injected_dictionary();
            Resolve_compile_time_generated_example_service_with_the_rules();
            return 6;
        }

        [Test]
        public void Resolve_compile_time_generated_example_service()
        {
            var c = new Container(Rules.Default.WithCompileTimeContainer(CompileTimeContainer.Instance));
            c.Register<Example.RuntimeDependencyC>();

            var x = c.Resolve<Example.IService>();

            Assert.IsNotNull(x);
        }

        [Test]
        public void Emulate_compile_time_generated_example_service_in_runtime()
        {
            var container = new Container();

            container.Register<IService, MyService>();
            container.Register<IDependencyA, DependencyA>();
            container.Register(typeof(DependencyB<>), setup: Setup.With(asResolutionCall: true));

            //container.RegisterPlaceholder<RuntimeDependencyC>();

            var exprs = container.GenerateResolutionExpressions(ServiceInfo.Of<IService>());
            Assert.AreEqual(0, exprs.Errors.Count);
            Assert.AreEqual(1, exprs.Roots.Count);
            Assert.AreEqual(typeof(IService), exprs.Roots[0].Key.ServiceType);
            Assert.AreEqual(2, exprs.ResolveDependencies.Count);

            var resolvedDep = exprs.ResolveDependencies.First(kv => kv.Value != null);
            Assert.AreEqual(typeof(DependencyB<string>), resolvedDep.Key.ServiceType);

            var unresolvedDep = exprs.ResolveDependencies.First(kv => kv.Value == null);
            Assert.AreEqual(typeof(RuntimeDependencyC), unresolvedDep.Key.ServiceType);

            container.Register<Example.RuntimeDependencyC>();
            var x = container.Resolve<Example.IService>();

            Assert.IsNotNull(x);
        }

        [Test]
        public void Resolve_compile_time_generated_example_service_with_the_rules()
        {
            var c = new Container(Rules.Default.WithCompileTimeContainer(new TestCompileTimeContainer()));

            var x = c.Resolve<S2>();

            Assert.IsNotNull(x);
        }

        class S2 { }

        class TestCompileTimeContainer : ICompileTimeContainer
        {
            public ValueTuple<Type, object>[] GetResolutionRoots() => new ValueTuple<Type, object>[]
            {
                (typeof(S2), null)
            };

            public bool TryResolve(out object service, IResolverContext r, Type serviceType)
            {
                service = new S2();
                return true;
            }
            public bool TryResolve(out object service, IResolverContext r, Type serviceType, object serviceKey, Type requiredServiceType, Request preRequestParent, object[] args)
            {
                service = null;
                return false;
            }
            public IEnumerable<ResolveManyResult> ResolveMany(IResolverContext _, Type serviceType) => new[] { ResolveManyResult.Of(_ => new S2()) };
        }

        [Test]
        public void Can_resolve_the_struct_service_in_runtime_interpreted_and_compiled()
        {
            var c = new Container();
            c.Register<IA, A>();
            c.Register<BVal>();

            var b = c.Resolve<BVal>();
            var b1 = c.Resolve<BVal>();
            var b2 = c.Resolve<BVal>();

            Assert.IsNotNull(b);
            Assert.IsNotNull(b.A);
            Assert.IsNotNull(b1.A);
            Assert.IsNotNull(b2.A);
        }

        [Test]
        public void Can_resolve_the_keyed_struct_service_in_runtime_interpreted_and_compiled()
        {
            var c = new Container();
            c.Register<IA, A>();
            c.Register<BVal>(serviceKey: "1");

            var b = c.Resolve<BVal>(serviceKey: "1");
            var b1 = c.Resolve<BVal>(serviceKey: "1");
            var b2 = c.Resolve<BVal>(serviceKey: "1");

            Assert.IsNotNull(b);
            Assert.IsNotNull(b.A);
            Assert.IsNotNull(b1.A);
            Assert.IsNotNull(b2.A);
        }

        [Test]
        public void Can_resolve_service_with_injected_dictionary()
        {
            var c = new Container(Rules.Default.WithCompileTimeContainer(CompileTimeContainer.Instance));

            var x = c.Resolve<BaseAConsumer>();

            Assert.AreEqual(2, x.Addict.Count);
            Assert.IsInstanceOf<KeyedA>(x.Addict["keyed"]);
            Assert.IsInstanceOf<NonKeyedA>(x.Addict[DefaultKey.Of(0)]);
        }

        public interface IA { }

        public class A : IA { }

        // let's make it struct for fun
        public struct BVal
        {
            public readonly IA A;
            public BVal(IA a) => A = a;
        }
    }
}
