using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    public class Issue80_SupportWrappersForAllInterfacesImplementedByArray : ITest
    {
        public int Run()
        {
            I_can_resolve_array_implemented_collection_type_with_required_service_type();
            return 1;
        }

        [Test]
        public void I_can_resolve_array_implemented_collection_type_with_required_service_type()
        {
            var container = new Container();
            container.Register<IService, Service>();

            Assert.DoesNotThrow(() =>
            {
                container.Resolve<IService[]>();
                container.Resolve<ICollection<IService>>();
                container.Resolve<IList<IService>>();
                container.Resolve<IReadOnlyCollection<IService>>();
                container.Resolve<IReadOnlyList<IService>>();
            });
        }

        internal interface IService { }
        internal class Service : IService { }
    }
}
