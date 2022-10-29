using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue536_DryIoc_Exception_in_a_Constructor_of_a_Dependency_does_tunnel_through_Resolve_call : ITest
    {
        public int Run()
        {
            Test_root_singleton_should_rethrow_the_exception();
            return 2;
        }

        [Test]
        public void Test_root_singleton_should_rethrow_the_exception()
        {
            var container = new Container();

            container.Register<IInterfaceA, ClassA>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            Assert.Throws<ArgumentException>(() =>
                container.Resolve<IInterfaceA>());

            Assert.Throws<ArgumentException>(() =>
                container.Resolve<IInterfaceA>());
        }

        public interface IInterfaceA { }

        public class ClassA : IInterfaceA
        {
            public ClassA()
            {
                throw new ArgumentException("This is my error");
            }
        }
    }
}
