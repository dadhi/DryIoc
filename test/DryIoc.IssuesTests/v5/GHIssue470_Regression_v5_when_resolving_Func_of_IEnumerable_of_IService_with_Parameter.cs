using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue470_Regression_v5_when_resolving_Func_of_IEnumerable_of_IService_with_Parameter : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var c = new Container();

            c.Register<ServiceA>();
            c.Register<IServiceB, ServiceB1>();

            var a = c.Resolve<ServiceA>();
            Assert.IsInstanceOf<ServiceB1>(((IServiceB[])a.Services)[0]);
        }

        class ServiceA
        {
            public IEnumerable<IServiceB> Services;
            public ServiceA(Func<ServiceA, IEnumerable<IServiceB>> onStart)
            {
                Services = onStart(this);
            }
        }

        interface IServiceB {}
        class ServiceB1 : IServiceB {}
    }
}