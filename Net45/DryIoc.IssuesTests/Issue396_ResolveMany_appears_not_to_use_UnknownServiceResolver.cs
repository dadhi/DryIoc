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

            var container = new Container().WithAutoFallbackDynamicRegistrations(implTypes);

            var xs = container.ResolveMany<ICustomRegistration>();

            CollectionAssert.AreEquivalent(implTypes, xs.Select(_ => _.GetType()));
        }

        public interface ICustomRegistration { }

        public class CustomRegistrationA : ICustomRegistration { }

        public class CustomRegistrationB : ICustomRegistration { }

        [Test]
        public void Can_ResolveMany_of_not_registered_service_generic_interface()
        {
            var implTypes = new[] { typeof(CustomRegistrationA<>), typeof(CustomRegistrationB<>) };

            var container = new Container().WithAutoFallbackDynamicRegistrations(implTypes);

            var xs = container.ResolveMany<ICustomRegistration<string>>().ToArray();

            Assert.IsInstanceOf<CustomRegistrationA<string>>(xs[0]);
            Assert.IsInstanceOf<CustomRegistrationB<string>>(xs[1]);
        }

        public interface ICustomRegistration<T> { }

        public class CustomRegistrationA<T> : ICustomRegistration<T> { }

        public class CustomRegistrationB<T> : ICustomRegistration<T> { }

        [Test]
        public void Selects_only_valid_non_generic_impl_for_non_generic_service()
        {
            var implTypes = new[] { typeof(CustomRegistrationA<>), typeof(CustomRegistrationB) };

            var container = new Container().WithAutoFallbackDynamicRegistrations(implTypes);

            var xs = container.ResolveMany<ICustomRegistration>().ToArray();

            Assert.AreEqual(1, xs.Length);
            Assert.IsInstanceOf<CustomRegistrationB>(xs[0]);
        }


        [Test]
        public void I_should_be_able_to_specify_reuse()
        {
            var implTypes = new[] { typeof(CustomRegistrationA<>), typeof(CustomRegistrationB<>) };

            var container = new Container().WithAutoFallbackDynamicRegistrations(
                implTypes,
                factory: (serviceType, key, implType) =>
                {
                    if (serviceType == typeof(ICustomRegistration<string>))
                        return new ReflectionFactory(implType, Reuse.Singleton);
                    return new ReflectionFactory(implType);
                });

            var x1 = container.ResolveMany<ICustomRegistration<string>>().ToArray();
            var x2 = container.ResolveMany<ICustomRegistration<string>>().ToArray();

            Assert.AreSame(x1[0], x2[0]);
        }
    }
}
