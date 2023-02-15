using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue557_WithFactorySelector_allows_to_Resolve_the_keyed_service_as_non_keyed : ITest
    {
        public int Run()
        {
            Test1();
            return 1;
        }

        [Test]
        public void Test1()
        {
            var rules = Rules.Default
                // .WithFactorySelector(Rules.SelectLastRegisteredFactory())
                .WithConcreteTypeDynamicRegistrations(reuse: Reuse.Transient)
                .With(Made.Of(FactoryMethod.ConstructorWithResolvableArguments))
                .WithFuncAndLazyWithoutRegistration()
                .WithTrackingDisposableTransients();

            var c = new Container(rules);

            c.Register(typeof(IServiceA), typeof(ServiceA), Reuse.Transient, serviceKey: "Test");

            var serviceA = c.Resolve<IServiceA>(IfUnresolved.ReturnDefault);
            Assert.IsNull(serviceA);
        }

        public interface IServiceA
        {
            string Text { get; }
        }

        public class ServiceA : IServiceA
        {
            public string Text { get; set; }
        }
    }
}
