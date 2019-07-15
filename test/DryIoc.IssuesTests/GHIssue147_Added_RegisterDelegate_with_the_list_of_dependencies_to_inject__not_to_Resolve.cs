using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue147_Added_RegisterDelegate_with_the_list_of_dependencies_to_inject__not_to_Resolve
    {
        [Test]
        public void Test()
        {
            var container = new Container();

            Func<D1, S> f = d1 => new S(d1);

            container.Register<S>(made: Made.Of(FactoryMethod.Of(f.GetType().GetMethod("Invoke"), f)));
            container.Register<D1>();

            var s = container.Resolve<S>();

            Assert.IsNotNull(s);
        }

        public class S
        {
            public D1 D1 { get; }

            public S(D1 d1)
            {
                D1 = d1;
            }
        }

        public class D1 { }
    }
}
