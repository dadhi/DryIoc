using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue198_Open_generics_resolve_fails_if_there_is_a_static_constructor : ITest
    {
        public int Run()
        {
            Should_select_the_default_non_static_constructor();
            return 1;
        }

        [Test]
        public void Should_select_the_default_non_static_constructor()
        {
            var container = new Container();
            container.Register(typeof(ITest<>), typeof(Test<>), Reuse.Singleton);
            var resolved = container.Resolve<ITest<string>>();
            Assert.IsNotNull(resolved);
        }

        public interface ITest<T> { }
        public class Test<T> : ITest<T>
        {
            private static IEnumerable<T> Afield = new List<T>();
        }
    }
}
