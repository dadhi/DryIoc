using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    public class IfUnresolvedTests
    {
        [Test]
        public void Resolving_with_IfUnresolved_ReturnNull_When_dependency_is_not_resolved_Should_return_null()
        {
            var container = new Container();
            container.Register<Client>();

            var client = container.Resolve<Client>(IfUnresolved.ReturnDefault);

            Assert.IsNull(client);
        }

        [Test]
        public void Resolving_Enumerable_of_unregistered_service_with_ReturnNull_should_return_empty_array()
        {
            var container = new Container();

            var services = container.Resolve<Service[]>(IfUnresolved.ReturnDefault);

            Assert.AreEqual(0, services.Length);
        }

        [Test]
        public void Resolving_service_with_dependency_first_with_ReturnNull_then_with_Throw_should_throw()
        {
            var container = new Container();
            container.Register<Client>();

            container.Resolve<Client>(IfUnresolved.ReturnDefault);

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<Client>());

            StringAssert.Contains("Unable to resolve", ex.Message);
            StringAssert.Contains("parameter \"service\"", ex.Message);
        }

        [Test]
        public void Resolving_service_first_as_return_null_then_resolving_next_as_Throw_should_throw()
        {
            var container = new Container();

            var service = container.Resolve<Client>(IfUnresolved.ReturnDefault);
            Assert.IsNull(service);

            Assert.Throws<ContainerException>(() => 
                container.Resolve<Client>());
        }

        #region CUT

        public class Client
        {
            public IService Some { get; private set; }

            public Client(IService service)
            {
                Some = service;
            }
        }

        #endregion
    }
}
