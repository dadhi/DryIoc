using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue_Register_null_string
    {
        [Test]
        public void Test()
        {
            var rules = Rules.Default.WithConcreteTypeDynamicRegistrations(reuse: Reuse.Transient)
                .With(Made.Of(FactoryMethod.ConstructorWithResolvableArguments))
                .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Replace)
                .WithFuncAndLazyWithoutRegistration()
                .WithTrackingDisposableTransients()
                .WithoutFastExpressionCompiler()
                .WithFactorySelector(Rules.SelectLastRegisteredFactory());

            var c = new Container(rules);

            c.Register<A>();

            // Both variants work with DryIoc v5 (preview) but failing with the v4.8.4
            //c.RegisterDelegate<string>(() => null, setup: Setup.With(asResolutionCall: true));
            c.RegisterInstance<string>(null, setup: Setup.With(asResolutionCall: true));

            var s = c.Resolve<string>();
            Assert.AreEqual(null, s);

            var a = c.Resolve<A>();
            Assert.AreEqual(null, a.S);

            var a2 = c.Resolve<A>(new object[] { "Hello World" });
            Assert.AreEqual("Hello World", a2.S);

            c.RegisterInstance<string>(null);
            var a3 = c.Resolve<A>();
            Assert.AreEqual(null, a3.S);

            c.RegisterInstance<string>("Hello New Registration");
            var a4 = c.Resolve<A>();
            Assert.AreEqual("Hello New Registration", a4.S);

            c.RegisterDelegate<string>(() => "Hello Another Registration");
            var a5 = c.Resolve<A>();
            Assert.AreEqual("Hello Another Registration", a5.S);
        }

        class A
        {
            public string S;
            public A(string s) => S = s;
        }
    }
}
