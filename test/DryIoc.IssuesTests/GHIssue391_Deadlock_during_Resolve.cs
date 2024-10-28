using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue391_Deadlock_during_Resolve : ITest
    {
        public int Run()
        {
            Test_non_generic_with_Decorator(); // actually related to the #598
            Test_non_generic();
            Test_open_generic();
            return 3;
        }

        [Test]
        public void Test_non_generic()
        {
            var container = new Container(rules => rules
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithoutEagerCachingSingletonForFasterAccess()
                .WithoutThrowOnRegisteringDisposableTransient()
                .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Replace));

            container.Register(typeof(A), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));
            container.RegisterMany(new[] { typeof(A), typeof(IA) }, typeof(A), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));

            container.Register(typeof(B), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));
            container.RegisterMany(new[] { typeof(B), typeof(IB) }, typeof(B), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));

            container.Register(typeof(C), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));
            container.RegisterMany(new[] { typeof(C), typeof(IC) }, typeof(C), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));

            // the missing dependency
            // container.Register<ID, D>(Reuse.Singleton);

            // A -> B -> C -> D(missing)
            //   \----->

            Assert.Throws<ContainerException>(() => container.Resolve<IA>());

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<IA>());
            Assert.AreSame(Error.NameOf(Error.WaitForScopedServiceIsCreatedTimeoutExpired), ex.ErrorName);

            var m = ex.TryGetDetails(container);
            StringAssert.Contains("A,", m);
            StringAssert.DoesNotContain("IA,", m);
        }

        [Test]
        public void Test_non_generic_with_Decorator()
        {
            var container = new Container(rules => rules
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithoutEagerCachingSingletonForFasterAccess()
                .WithoutThrowOnRegisteringDisposableTransient()
                .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Replace));

            container.Register<IA, AD>(Reuse.Singleton, setup: Setup.Decorator);

            container.Register(typeof(A), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));
            container.RegisterMany(new[] { typeof(A), typeof(IA) }, typeof(A), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));

            container.Register(typeof(B), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));
            container.RegisterMany(new[] { typeof(B), typeof(IB) }, typeof(B), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));

            container.Register(typeof(C), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));
            container.RegisterMany(new[] { typeof(C), typeof(IC) }, typeof(C), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));

            // the missing dependency
            // container.Register<ID, D>(Reuse.Singleton);

            // A -> B -> C -> D(missing)
            //   \----->

            Assert.Throws<ContainerException>(() => container.Resolve<IA>());

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<IA>());
            Assert.AreSame(Error.NameOf(Error.WaitForScopedServiceIsCreatedTimeoutExpired), ex.ErrorName);

            var m = ex.TryGetDetails(container);
            StringAssert.Contains("DecoratorType=", m);
            StringAssert.Contains("ServiceType=", m);
            StringAssert.Contains("IA", m);
        }

        [Test]
        public void Test_open_generic()
        {
            var container = new Container(rules => rules
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithoutEagerCachingSingletonForFasterAccess()
                .WithoutThrowOnRegisteringDisposableTransient()
                .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Replace));

            container.Register(typeof(A<>), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));
            container.RegisterMany(new[] { typeof(A<>), typeof(IA<>) }, typeof(A<>), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));

            container.Register(typeof(B), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));
            container.RegisterMany(new[] { typeof(B), typeof(IB) }, typeof(B), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));

            container.Register(typeof(C), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));
            container.RegisterMany(new[] { typeof(C), typeof(IC) }, typeof(C), Reuse.Singleton, setup: Setup.With(asResolutionCall: true));

            // the missing dependency
            // container.Register<ID, D>(Reuse.Singleton);

            // A -> B -> C -> D(missing)
            //   \----->

            Assert.Throws<ContainerException>(() => container.Resolve<IA<B>>());

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<IA<B>>());
            Assert.AreSame(Error.NameOf(Error.WaitForScopedServiceIsCreatedTimeoutExpired), ex.ErrorName);

            var m = ex.TryGetDetails(container);
            StringAssert.Contains("A<>", m);
        }

        public interface IA { }
        public interface IA<TB> { }
        public interface IB { }
        public interface IC { }
        public interface ID { }

        public class A : IA
        {
            public IB B;
            public IC C;
            public A(IC c, IB b)
            {
                B = b;
                C = c;
            }
        }

        public class AD : IA
        {
            public IA A;
            public AD(IA a) => A = a;
        }

        public class A<TB> : IA<TB>
        {
            public TB B;
            public IC C;
            public A(IC c, TB b)
            {
                B = b;
                C = c;
            }
        }

        public class B : IB
        {
            public IC C;
            public B(IC c) => C = c;
        }

        public class C : IC
        {
            public ID D;
            public C(ID d) => D = d;
        }

        public class D : ID { }
    }
}