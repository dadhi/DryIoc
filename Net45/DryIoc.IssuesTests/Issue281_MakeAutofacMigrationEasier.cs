using System;
using Autofac;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue281_MakeAutofacMigrationEasier
    {
        [Test]
        public void Test_CustomDelegate_Autofac()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<A>();
            builder.RegisterType<B>();
            builder.RegisterType<C>();
            builder.RegisterType<D>();

            using (var container = builder.Build())
            {
                var d = container.Resolve<D>();
                var c = d.CreateC();

                Assert.IsNotNull(c.A);
                Assert.IsNotNull(c.B);
            }
        }

        [Test]
        public void Test_CustomDelegate_DryIoc()
        {
            var container = new Container();

            container.Register<A>();
            container.Register<B>();
            container.Register<C>();
            container.RegisterDelegate<C.Factory>(r => a => r.Resolve<Func<A, C>>()(a));
            container.Register<D>();

            var d = container.Resolve<D>();
            var c = d.CreateC();

            Assert.IsNotNull(c.A);
            Assert.IsNotNull(c.B);
        }

        [Test]
        public void Test_CustomDelegate_of_Singleton_in_DryIoc()
        {
            var container = new Container();

            container.Register<A>();
            container.Register<B>();
            container.Register<C>(Reuse.Singleton);
            container.RegisterDelegate<C.Factory>(r => a => r.Resolve<Func<A, C>>()(a));
            container.Register<D>();

            var d = container.Resolve<D>();
            var c = d.CreateC();
            Assert.AreSame(c, container.Resolve<C>());
        }

        public class A {}

        public class B {}

        public class C
        {
            public readonly A A;
            public readonly B B;

            public delegate C Factory(A a);

            public C(A a, B b)
            {
                A = a;
                B = b;
            }
        }

        public class D
        {
            private readonly A _a;
            private readonly C.Factory _factoryC;

            public D(A a, C.Factory factoryC)
            {
                _a = a;
                _factoryC = factoryC;
            }

            public C CreateC()
            {
                return _factoryC(_a);
            }
        }

        [Test]
        public void Test_TypedParameter_Autofac()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<OperationOne>().Named<IOperation>(nameof(OperationOne));
            builder.RegisterType<OperationTwo>().Named<IOperation>(nameof(OperationTwo));
            builder.RegisterType<Service>().As<IService>();

            using (var container = builder.Build())
            {
                var operation = container.ResolveNamed<IOperation>(nameof(OperationTwo));
                var service = container.Resolve<IService>(new TypedParameter(typeof(IOperation), operation));

                Assert.IsInstanceOf<OperationTwo>(((Service)service).Operation);
            }
        }

        [Test]
        public void Test_TypedParameter_DryIoc_option_one()
        {
            var container = new Container();

            container.Register<IOperation, OperationOne>(serviceKey: nameof(OperationOne));
            container.Register<IOperation, OperationTwo>(serviceKey: nameof(OperationTwo));
            container.Register<IService, Service>(made: Parameters.Of.Type<IOperation>(serviceKey: nameof(OperationTwo)));

            var service = container.Resolve<IService>();

            Assert.IsInstanceOf<OperationTwo>(((Service)service).Operation);
        }

        [Test]
        public void Test_TypedParameter_DryIoc_option_two()
        {
            var container = new Container();

            container.Register<IOperation, OperationOne>(serviceKey: nameof(OperationOne));
            container.Register<IOperation, OperationTwo>(serviceKey: nameof(OperationTwo));
            container.Register<IService, Service>();

            var service =
                container.Resolve<Func<IOperation, IService>>()(container.Resolve<IOperation>(nameof(OperationTwo)));

            Assert.IsInstanceOf<OperationTwo>(((Service)service).Operation);
        }

        public interface IOperation {}

        public class OperationOne : IOperation {}

        public class OperationTwo : IOperation {}

        public interface IService {}

        public class Service : IService
        {
            public readonly IOperation Operation;

            public Service(IOperation operation)
            {
                Operation = operation;
            }
        }

        [Test]
        public void Test_DryIoc_returns_the_same_singleton_prior_resolved_with_Func()
        {
            var container = new Container();

            container.Register<IService, Service>(Reuse.Singleton);
            container.Register<IOperation, OperationOne>();

            var serviceFactory = container.Resolve<Func<IOperation, IService>>();
            Assert.AreSame(serviceFactory(new OperationOne()), container.Resolve<IService>());
        }
    }
}
