using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue315_Combining_RegisterDelegate_with_TrackingDisposableTransients_rule_throws_TargetParameterCountException : ITest
    {
        public int Run()
        {
            Test_RegisterDelegate();
            return 1;
        }

        [Test]
        public void Test_RegisterDelegate()
        {
            var container = new Container(rules => rules.WithTrackingDisposableTransients());
            container.RegisterDelegate<Foo>(c => new Bar());
            var foo = container.Resolve<Foo>();

            container.Dispose();
            Assert.IsTrue(container.IsDisposed);
        }

        public class Foo : IDisposable
        {
            public bool IsDisposed;
            public void Dispose() => IsDisposed = true;
        }

        public class Bar : Foo { }
    }
}