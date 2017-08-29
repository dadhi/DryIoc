using System;
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

            Assert.AreEqual(6, a2.I1);
            Assert.AreEqual(7, a2.I2);

            Assert.AreEqual(5, a1.I1);
        }

        public class A
        {
            public int I1 { get; private set; }
            public int I2 { get; private set; }

            public A(B b, int i1)
            {
                I1 = i1;
            }

            public A(B b, int i1, int i2)
            {
                I1 = i1;
                I2 = i2;
            }
        }

        public class B { }
    }
}
