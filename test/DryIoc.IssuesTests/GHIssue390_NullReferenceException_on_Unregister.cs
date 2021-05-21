using NUnit.Framework;
using System.Linq;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue390_NullReferenceException_on_Unregister : ITest
    {
        public int Run() 
        { 
            Test1();
            return 1;
        }

        [Test]
        public void Test1()
        {
            var implementationType1 = typeof(A);
            var implementationType2 = typeof(B);
            var interfaceType       = typeof(ITargetInterface);

            var container = new Container();
            container.Register(interfaceType, implementationType1, serviceKey: "foo");
            container.Register(interfaceType, implementationType2, serviceKey: "bar");

            container.Unregister(interfaceType, serviceKey: "foo");

            Assert.Throws<ContainerException>(() => 
                container.Resolve(interfaceType, "foo"));

            var x = container.Resolve(interfaceType, "foo", IfUnresolved.ReturnDefault);
            Assert.IsNull(x);

            var a = container.ResolveMany(interfaceType, ResolveManyBehavior.AsFixedArray);
            Assert.IsInstanceOf<B>(a.Single());

            container.ClearCache(interfaceType);
        }

        interface ITargetInterface {}
        class A : ITargetInterface {}
        class B : ITargetInterface {}
    }
}