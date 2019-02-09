
using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue63_Func_wrapper_resolving
    {
        [Test]
        public void Test()
        {
            var container = new Container(Rules.Default
                .WithUnknownServiceResolvers(request => new DelegateFactory(_ => null)));

            container.Register<A>(); //UNCOMMENT THIS LINE TO GET EXCEPTION

            var fFunc = container.Resolve<Func<IFoo>>();

            var f0 = fFunc(); //can be resolved as null because of unknown service resolver

            container.Register<IFoo, Foo1>(Reuse.Singleton, 
                ifAlreadyRegistered: IfAlreadyRegistered.Replace, 
                setup: Setup.With(asResolutionCall: true));

            var f1 = container.Resolve<IFoo>();
            var f2 = fFunc();

            Assert.IsNull(f0);

            Assert.IsNotNull(f2); // EXCEPTION

            Assert.AreEqual(f1, f2);
        }

        internal interface IFoo { }
        internal class Foo1 : IFoo { }
        internal class A
        {
            public A(Func<IFoo> func) { }
        }
    }
}
