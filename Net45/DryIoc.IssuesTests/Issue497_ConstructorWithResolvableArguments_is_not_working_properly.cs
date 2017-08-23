using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue497_ConstructorWithResolvableArguments_is_not_working_properly
    {
        [Test]
        public void Test()
        {
            var c = new Container(Rules.Default.With(FactoryMethod.ConstructorWithResolvableArguments));

            c.Register<A>();
            c.Register<B>();

            var a2 = c.Resolve<Func<int, int, A>>()(6, 7); // this works
            var a1 = c.Resolve<Func<int, A>>()(5); // this does NOT work
        }

        public class A
        {
            public A(B b, int i1) { }

            public A(B b, int i1, int i2) { }
        }

        public class B { }
    }
}
