using System;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class RegisterManyTests
    {
        [Test]
        public void Can_register_single_registrations_for_all_public_types_implemented()
        {
            var container = new Container();
            container.RegisterMany<Someberry>();

            Assert.That(container.IsRegistered<IBerry>(), Is.False);
            Assert.That(container.IsRegistered<IProduct>(), Is.True);
            Assert.That(container.IsRegistered<Someberry>(), Is.True);
        }

        [Test]
        public void Singleton_registered_with_multiple_interfaces_should_be_the_same()
        {
            var container = new Container();
            container.RegisterMany<Someberry>(Reuse.Singleton);

            var product = container.Resolve<IProduct>();
            var berry = container.Resolve<Someberry>();

            Assert.That(product, Is.SameAs(berry));
        }

        [Test]
        public void Non_optimized_singleton_registered_with_multiple_interfaces_should_be_the_same()
        {
            var container = new Container(r => r.WithoutEagerCachingSingletonForFasterAccess());
            container.RegisterMany<Someberry>(Reuse.Singleton);

            var product = container.Resolve<IProduct>();
            //var productExpr = container.Resolve<FactoryExpression<IProduct>>();

            var berry = container.Resolve<ISome>();
            //var someExpr = container.Resolve<FactoryExpression<ISome>>();

            Assert.That(product, Is.SameAs(berry));
        }

        [Test]
        public void Non_optimized_singleton_injected_as_different_interfaces_should_be_the_same()
        {
            var container = new Container(r => r.WithoutEagerCachingSingletonForFasterAccess());
            container.RegisterMany<Someberry>(Reuse.Singleton);
            container.Register<SomeEater>();
            container.Register<ProductEater>();

            var some = container.Resolve<SomeEater>();
            var product = container.Resolve<ProductEater>();

            Assert.AreSame(some.Some, product.Product);
        }

        public class Someberry : IBerry, ISome, IProduct { }

        public interface IProduct { }
        public interface ISome { }

        internal interface IBerry { }

        public class SomeEater
        {
            public readonly ISome Some;
            public SomeEater(ISome some) { Some = some; }
        }

        public class ProductEater
        {
            public readonly IProduct Product;
            public ProductEater(IProduct product) { Product = product; }
        }

        [Test]
        public void Can_register_service_with_implementations_found_in_assemblies()
        {
            var container = new Container();

            container.RegisterMany(new[] { typeof(Blah), typeof(AnotherBlah) });

            var services = container.Resolve<IBlah[]>();

            CollectionAssert.AreEquivalent(
                new[] { typeof(Blah), typeof(AnotherBlah) },
                services.Select(s => s.GetType()));
        }

        [Test]
        public void Can_register_generic_service_Only_with_its_implementations_found_in_assemblies()
        {
            var container = new Container();

            container.RegisterMany(new[] { typeof(IBlah<,>).GetAssembly() }, typeof(IBlah<,>).Equals);

            var services = container.Resolve<IBlah<string, bool>[]>();

            CollectionAssert.AreEquivalent(
                new[] { typeof(Blah<string, bool>), typeof(AnotherBlah<bool>) },
                services.Select(s => s.GetType()));
        }

        public interface IBlah { }
        public class Blah : IBlah { }
        public class AnotherBlah : IBlah { }

        public interface IBlah<T0, T1> { }
        public class Blah<T0, T1> : IBlah<T0, T1> { }
        public class AnotherBlah<T> : IBlah<string, T> { }

        [Test]
        public void Can_register_something_from_assembly_as_singleton()
        {
            var container = new Container();

            container.RegisterMany(new[] { GetType().GetAssembly() },
                (r, serviceTypes, implType) => // for only A and its implementations
                {
                    if (serviceTypes.IndexOf(typeof(A)) != -1)
                        r.Register(typeof(A), implType, Reuse.Singleton);
                });
        }

        internal class A { }

        [Test]
        public void Register_many_should_skip_Compiler_generated_classes()
        {
            var container = new Container();

            container.RegisterMany(new[] { GetType().GetAssembly() }, (r, serviceTypes, implType) =>
            {
                Assert.False(implType.FullName.Contains("_DisplayClass"));
            });
        }

        internal class MyClass
        {
            public string Message;

            public void Handle(string message)
            {
                Message = message;
            }
        }

        internal class MyHandler
        {
            public Action Handler { get; private set; }

            public MyHandler(MyClass handler)
            {
                Handler = () => handler.Handle("Nope!");
            }
        }

        [Test]
        public void Can_get_all_service_registrations()
        {
            var container = new Container();
            container.RegisterMany(new[] { GetType().GetAssembly() }, (r, types, type) =>
            {
                if (type.GetAllConstructors().Count() == 1)
                    r.RegisterMany(types, type, Reuse.Singleton);
            });

            var registrations = container.GetServiceRegistrations().Select(r => r.ServiceType).ToArray();
            CollectionAssert.Contains(registrations, typeof(RegisterManyTests));
        }

        [Test]
        public void Can_register_internal_implementations()
        {
            var container = new Container(r => r
                .With(FactoryMethod.ConstructorWithResolvableArguments));

            container.RegisterMany(new[] { typeof(InternalMe).GetAssembly() }, nonPublicServiceTypes: true, 
                action: (registrator, types, type) => registrator.RegisterMany(types, type, Reuse.Singleton));
            
            var service = container.Resolve<IPublicMe>();

            Assert.IsInstanceOf<InternalMe>(service);
        }

        internal interface IPublicMe { }
        internal class InternalMe : IPublicMe { }

        [Test]
        public void Can_register_mapping_to_registered_service()
        {
            var container = new Container();
            container.Register<Y>(Reuse.Singleton);

            container.RegisterMapping<X, Y>();

            Assert.AreSame(container.Resolve<X>(), container.Resolve<Y>());
        }

        [Test]
        public void Can_register_mapping_to_not_assignable_service_of_the_same_implementation()
        {
            var container = new Container();
            container.Register<I, Y>(Reuse.Singleton);

            container.RegisterMapping<X, I>();

            Assert.AreSame(container.Resolve<I>(), container.Resolve<X>());
        }

        [Test]
        public void Register_mapping_should_throw_if_factory_is_not_found()
        {
            var container = new Container();
            container.Register<Y>();

            var ex = Assert.Throws<ContainerException>(() =>
                container.RegisterMapping<X, Y>(registeredServiceKey: "a"));

            Assert.AreEqual(Error.RegisterMappingNotFoundRegisteredService, ex.Error);
        }

        [Test]
        public void Register_mapping_should_throw_if_new_service_type_is_not_compatible_with_registered_implementation()
        {
            var container = new Container();
            container.Register<Y>(Reuse.Singleton);

            var ex = Assert.Throws<ContainerException>(() =>
                container.RegisterMapping(typeof(IDisposable), typeof(Y)));

            Assert.AreEqual(Error.RegisterImplementationNotAssignableToServiceType, ex.Error);
        }

        public interface I {}
        public class X {}
        public class Y : X, I {}
    }
}