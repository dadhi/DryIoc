using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class InjectListOfDepsWithStringDeps
    {
        [Test]
        public void Test()
        {
            var di = new Container(Rules.Default.WithConcreteTypeDynamicRegistrations());

            di.RegisterInstance("a", serviceKey: "x");
            di.RegisterInstance("b", serviceKey: "y");

            di.Register(Made.Of(() => 
                GetBs(Arg.Of<Func<string, B>>(), Arg.Of<KeyValuePair<string, string>[]>())));

            var a = di.Resolve<A>();
            Assert.AreEqual(2, a.Bs.Count);
        }

        public static IList<B> GetBs(Func<string, B> getB, KeyValuePair<string, string>[] ss)
        {
            return ss.Select(s => getB(s.Value)).ToList();
        }

        public class B
        {
            public string Message { get; private set; }

            public B(string message)
            {
                Message = message;
            }
        }

        public class A
        {
            public IList<B> Bs { get; private set; }

            public A(IList<B> bs)
            {
                Bs = bs;
            }
        }
    }
}
