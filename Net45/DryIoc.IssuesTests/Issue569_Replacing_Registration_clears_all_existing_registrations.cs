using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue569_Replacing_Registration_clears_all_existing_registrations
    {
        [Test]
        public void Replacing_default_registration_with_presence_of_keyed_should_not_replace_the_keyed()
        {
            var c = new Container();

            c.Register<X, A>(serviceKey: "blah");
            c.Register<X, B>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            Assert.IsInstanceOf<A>(c.Resolve<X>("blah"));

            c.Register<X, A>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            Assert.IsInstanceOf<A>(c.Resolve<X>("blah"));
        }

        [Test]
        public void Original_case()
        {
            var container = new Container();

            container.Register(typeof(X), typeof(A),
                made: Made.Of(FactoryMethod.ConstructorWithResolvableArguments),
                ifAlreadyRegistered: IfAlreadyRegistered.Replace,
                serviceKey: "Foo");

            container.Register(typeof(X), typeof(A),
                made: Made.Of(FactoryMethod.ConstructorWithResolvableArguments),
                ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var registrations = container.GetServiceRegistrations().Where(r => r.ServiceType == typeof(X));
            Assert.AreEqual(2, registrations.Count());

            // replace keyed, still should be 2
            container.Register(typeof(X), typeof(B),
                made: Made.Of(FactoryMethod.ConstructorWithResolvableArguments),
                ifAlreadyRegistered: IfAlreadyRegistered.Replace,
                serviceKey: "Foo");

            registrations = container.GetServiceRegistrations().Where(r => r.ServiceType == typeof(X));
            Assert.AreEqual(2, registrations.Count());

            // replace default, still should be 2, always 2
            container.Register(typeof(X), typeof(B),
                made: Made.Of(FactoryMethod.ConstructorWithResolvableArguments),
                ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            registrations = container.GetServiceRegistrations().Where(r => r.ServiceType == typeof(X));
            Assert.AreEqual(2, registrations.Count());
        }

        public interface X { }
        public class A : X { }
        public class B : X { }
    }
}
