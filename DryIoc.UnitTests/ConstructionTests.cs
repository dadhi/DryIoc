using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ConstructionTests
    {
        [Test]
        public void Can_use_any_type_static_method_for_service_creation()
        {
            var container = new Container();
            container.Register<IService>(rules: InjectionRules.With(
                factoryMethod: typeof(ServiceFactory).GetDeclaredMethod("CreateService")));

            var service = container.Resolve<IService>();

            Assert.That(service.Message, Is.EqualTo("yep!"));
        }

        //[Test]
        //public void Can_use_static_method_for_service_creation()
        //{
        //    var container = new Container();
        //    container.Register<SomeService>(setup: Setup.With(
        //        //(t, _) => ConstructionInfo.Of(t.GetDeclaredMethod("Create"))));

        //    var service = container.Resolve<SomeService>();

        //    Assert.That(service.Message, Is.EqualTo("yes!"));
        //}

        //[Test]
        //public void Can_use_instance_method_for_service_creation()
        //{
        //    var container = new Container();
        //    container.Register<ServiceFactory>();
        //    container.Register<SomeService>(setup: Setup.With(
        //        (_, __) => ConstructionInfo.Of(typeof(ServiceFactory).GetDeclaredMethod("CreateService"), 
        //            r => r.Resolve<ServiceFactory>())));

        //    var service = container.Resolve<SomeService>();

        //    Assert.That(service.Message, Is.EqualTo("yep!"));
        //}

        //[Test]
        //public void Can_use_instance_method_with_resolved_parameter()
        //{
        //    var container = new Container();
        //    container.Register<ServiceFactory>();
        //    container.RegisterInstance("dah!");
        //    container.Register<SomeService>(setup: Setup.With(
        //        (_, r) => ConstructionInfo.Of(typeof(ServiceFactory).GetDeclaredMethod("CreateService", new[] { typeof(string) }),
        //            _r => _r.Resolve<ServiceFactory>())));

        //    var service = container.Resolve<SomeService>();

        //    Assert.That(service.Message, Is.EqualTo("dah!"));
        //}

        //[Test]
        //public void Should_throw_if_instance_factory_unresolved()
        //{
        //    var container = new Container();
        //    container.Register<SomeService>(setup: Setup.With(
        //        (_, r) => ConstructionInfo.Of(typeof(ServiceFactory).GetDeclaredMethod("CreateService"), rr => rr.Resolve<ServiceFactory>())));
            
        //    Assert.Throws<ContainerException>(() => 
        //        container.Resolve<SomeService>());
        //}

        //[Test]
        //public void Should_throw_if_instance_factory_is_null()
        //{
        //    var container = new Container();
        //    container.Register<SomeService>(setup: Setup.With(
        //        (_, r) => ConstructionInfo.Of(typeof(ServiceFactory).GetDeclaredMethod("CreateService"), __ => null)));

        //    Assert.Throws<ContainerException>(() =>
        //        container.Resolve<SomeService>());
        //}

        //[Test]
        //public void Should_return_null_if_instance_factory_is_not_registered_and_we_try_to_resolve_service()
        //{
        //    var container = new Container();
        //    container.Register<SomeService>(setup: Setup.With(
        //        (_, r) => ConstructionInfo.Of(typeof(ServiceFactory).GetDeclaredMethod("CreateService"), rr => rr.Resolve<ServiceFactory>())));

        //   var service = container.Resolve<SomeService>(IfUnresolved.ReturnDefault);

        //    Assert.That(service, Is.Null);
        //}

        //[Test]
        //public void What_if_factory_method_return_incompatible_type()
        //{
        //    var container = new Container();
        //    container.Register<SomeService>(setup: Setup.With(
        //        (t, _) => ConstructionInfo.Of(typeof(BadFactory).GetDeclaredMethod("Create"))));

        //    var ex = Assert.Throws<ContainerException>(() => 
        //        container.Resolve<SomeService>());
        //}

        #region CUT

        internal interface IService 
        {
            string Message { get; }
        }

        internal class SomeService : IService
        {
            public string Message { get; private set; }

            internal SomeService(string message)
            {
                Message = message;
            }

            public static SomeService Create()
            {
                return new SomeService("yes!");
            }
        }

        internal class ServiceFactory
        {
            public static IService CreateService()
            {
                return new SomeService("yep!");
            }

            public IService CreateService(string message)
            {
                return new SomeService(message);
            }
        }

        internal class BadFactory
        {
            public static string Create()
            {
                return "bad";
            }
        }

        internal class Generic<T>
        {
            public T X { get; private set; }

            public Generic(T x)
            {
                X = x;
            }
        }

        #endregion
    }
}
