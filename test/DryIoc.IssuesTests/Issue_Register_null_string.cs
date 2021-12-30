using System;
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
            var a = c.Resolve<A>();

            var a2 = c.Resolve<A>(new object[] { "Hello World" });

            c.RegisterInstance<string>(null);
            var a3 = c.Resolve<A>();

            c.RegisterInstance<string>("Hello New Registration");
            var a4 = c.Resolve<A>();

            c.RegisterDelegate<string>(() => "Hello Another Registration");
            var a5 = c.Resolve<A>();

            Console.WriteLine("Hello s: " + (s ?? "null"));
            Console.WriteLine("Hello a.S: " + (a.S ?? "null"));
            Console.WriteLine("Hello a2.S: " + (a2.S ?? "null"));
            Console.WriteLine("Hello a3.S: " + (a3.S ?? "null"));

            Console.WriteLine("Hello a4.S: " + (a4.S ?? "null"));
            Console.WriteLine("Hello a5.S: " + (a5.S ?? "null"));
        }

        class A
        {
            public string S;
            public A(string s) => S = s;
        }
    }
}
