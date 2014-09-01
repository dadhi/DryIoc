using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    public class IfUnresolvedTests
    {
        [Test]
        public void Resolving_with_IfUnresolved_ReturnNull_When_dependency_is_not_resolved_Should_return_null()
        {
            var container = new Container();
            container.Register<Client>();

            var client = container.Resolve<Client>(IfUnresolved.ReturnNull);

            Assert.IsNull(client);
        }

        public class Client
        {
            public IService Some { get; private set; }

            public Client(IService service)
            {
                Some = service;
            }
        }
    }
}
