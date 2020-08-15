using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue303_Open_Generic_Singleton_do_not_provide_same_instance_for_Resolve_and_ResolveMany
    {
        [Test]
        public void Test()
        {
            var container = new Container();
            //container.Register<A<string>>(Reuse.Singleton, made: FactoryMethod.ConstructorWithResolvableArguments);
            container.Register(typeof(B<>), Reuse.Singleton, made: FactoryMethod.ConstructorWithResolvableArguments);

            //var a1 = container.Resolve<A<string>>();
            //var a2 = container.ResolveMany<A<string>>().Single();
            //Assert.AreEqual(a1, a2); // working

            var b1 = container.Resolve<B<string>>();
            var b2 = container.ResolveMany<B<string>>().Single();
            Assert.AreEqual(b1, b2);
        }

        private class A<T>
        {
        }
        private class B<T>
        {
        }
    }
}
