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

            c.Register<IServiceB, ServiceB>();
            c.Register<IDep, Dep>();
            c.Register<ServiceA>();

            var a = c.Resolve<ServiceA>();
            var b = ((IServiceB[])a.Services)[0] as ServiceB;
            Assert.IsNotNull(b);
            Assert.AreSame(c, b.R);
            Assert.IsInstanceOf<Dep>(b.D);
        }

        class ServiceA
        {
            public IEnumerable<IServiceB> Services;
            public ServiceA(Func<ServiceA, IEnumerable<IServiceB>> onStart)
            {
                Services = onStart(this);
            }
        }

        interface IDep { }
        class Dep : IDep { }

        interface IServiceB { }
        class ServiceB : IServiceB
        {
            public IResolver R;
            public IDep D;
            public ServiceB(IResolver r, IDep d)
            {
                R = r;
                D = d;
            }
        }
    }
}