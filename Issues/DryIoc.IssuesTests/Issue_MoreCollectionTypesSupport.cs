using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    class Issue_MoreCollectionTypesSupport
    {
        [Test]
        public void I_can_resolve_array_implemented_collection_type_with_required_service_type()
        {
            var services = new IService[] { new Service() };
            Assert.That(services, Is.InstanceOf<ICollection<IService>>());
            Assert.That(services, Is.InstanceOf<IList<IService>>());
            Assert.That(services, Is.InstanceOf<IReadOnlyList<IService>>());
            Assert.That(services, Is.InstanceOf<IReadOnlyCollection<IService>>());

            var container = new Container();
            container.Register<IService, Service>();

            Assert.DoesNotThrow(() =>
            {
                container.Resolve<IService[]>();
                container.Resolve<ICollection<IService>>(typeof(IService[]));
                container.Resolve<IList<IService>>(typeof(IService[]));
                container.Resolve<IReadOnlyCollection<IService>>(typeof(IService[]));
                container.Resolve<IReadOnlyList<IService>>(typeof(IService[]));
            });
        }

        internal interface IService { }
        internal class Service : IService { }
    }
}
