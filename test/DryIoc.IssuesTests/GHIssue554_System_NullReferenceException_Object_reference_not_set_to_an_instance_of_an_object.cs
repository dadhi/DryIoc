using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue554_System_NullReferenceException_Object_reference_not_set_to_an_instance_of_an_object : ITest
    {
        public int Run()
        {
            Test1();
            return 1;
        }

        [Test]
        public void Test1()
        {
            using (var container = new Container()) 
            {
                container.Register<IDisposable, MyDisposable>(Reuse.ScopedOrSingleton);
                var scope = container.OpenScope() as IContainer;
                var child = scope.CreateChild();
                child.Resolve<IDisposable>();
            }
        }

        public class MyDisposable : IDisposable
        {
            public void Dispose() {}
        }
    }
}
