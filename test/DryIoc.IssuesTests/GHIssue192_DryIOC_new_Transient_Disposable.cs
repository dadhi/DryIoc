using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue192_DryIOC_new_Transient_Disposable : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var container = new Container();
            container.Register<SomeClient>(
                reuse: Reuse.Transient,
                setup: Setup.With(allowDisposableTransient: true));

            // Works
            var client1 = container.Resolve<SomeClient>();

            // Fails
            var client2 = container.New<SomeClient>();
        }

        public interface IClient
        {
        }

        public class SomeClient : IClient, IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
