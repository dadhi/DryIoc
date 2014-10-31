using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    public class Issue81_SupportForOptionalParametersInConstructor
    {
        [Test]
        public void Should_automatically_specify_IfUnresolvedReturnDefault_for_optional_parameters()
        {
            var container = new Container();
            container.Register<Client>();

            var client = container.Resolve<Client>();

            Assert.That(client.Dep, Is.Null);
        }

        public class Client
        {
            public Dep Dep { get; set; }

            public Client(Dep dep = null)
            {
                Dep = dep;
            }
        }

        public class Dep { }
    }
}
