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

        [Test]
        public void Should_keep_specified_default_parameter_value()
        {
            var container = new Container();
            container.Register<SomeService>();
            container.Register<Dep>();

            var service = container.Resolve<SomeService>();

            Assert.That(service.Count, Is.EqualTo(3));
        }

        internal class Client
        {
            public Dep Dep { get; private set; }

            public Client(Dep dep = null)
            {
                Dep = dep;
            }
        }

        internal class Dep { }

        internal class SomeService
        {
            public Dep Dep { get; private set; }
            public int Count { get; private set; }

            public SomeService(Dep dep, int count = 3)
            {
                Dep = dep;
                Count = count;
            }
        }
    }
}
