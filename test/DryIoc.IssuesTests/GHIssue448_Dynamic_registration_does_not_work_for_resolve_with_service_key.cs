
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue448_Dynamic_registration_does_not_work_for_resolve_with_service_key
    {
        [Test]
        public void Test1()
        {
            var container1 = new Container();
            container1.RegisterInstance<A>(new A());
            container1.RegisterInstance<B>(new B(), serviceKey: "key");

            IEnumerable<DynamicRegistration> provider(Type serviceType, object serviceKey) => new[]
            {
                new DynamicRegistration(
                    new DelegateFactory(_ => container1.Resolve(serviceType, serviceKey)),
                    serviceKey: serviceKey) // IMPORTANT you need to specify the key for the keyed dynamic registration
            };

            var container2 = new Container(rules => rules
                .WithDynamicRegistrationsAsFallback(DynamicRegistrationFlags.AsFallback | DynamicRegistrationFlags.Service, provider)
            );

            container1.Resolve<A>(); //WORKS
            container1.Resolve<B>("key"); //WORKS
            container2.Resolve<A>(); //WORKS
            container2.Resolve<B>("key"); //DOES NOT WORK
        }

        class A
        {
        }

        class B
        {
        }
    }
}
