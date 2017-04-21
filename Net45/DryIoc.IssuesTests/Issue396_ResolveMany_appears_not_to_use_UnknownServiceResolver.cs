using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue396_ResolveMany_appears_not_to_use_UnknownServiceResolver
    {
        [Test]
        public void Can_ResolveMany_of_not_registered_service_interface()
        {
            var implTypes = new[] { typeof(CustomRegistrationA), typeof(CustomRegistrationB) };

            var container = new Container().WithAutoFallbackResolution(implTypes);

            var xs = container.ResolveMany<ICustomRegistration>();

            CollectionAssert.AreEquivalent(implTypes, xs.Select(_ => _.GetType()));
        }

        public interface ICustomRegistration { }

        public class CustomRegistrationA : ICustomRegistration { }

        public class CustomRegistrationB : ICustomRegistration { }
    }
}
