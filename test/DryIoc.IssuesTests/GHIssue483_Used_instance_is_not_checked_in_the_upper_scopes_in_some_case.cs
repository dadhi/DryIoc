using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue483_Used_instance_is_not_checked_in_the_upper_scopes_in_some_case : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            using var c = new Container();

            c.Register<A>();

            using var s1 = c.OpenScope();
            var used = new U();
            s1.Use(used);

            using var s2 = s1.OpenScope();
            var used2 = new U2();
            s2.Use(used2);

            var a = s2.Resolve<A>();
            Assert.IsInstanceOf<U>(a.Used);
        }

        public class A
        {
            public U Used;
            public A(U used) => Used = used;
        }

        public class U {}

        public class U2 {}
    }
}