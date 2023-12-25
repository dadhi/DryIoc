using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue337_Singleton_is_created_twice : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var container = new Container(r => r.WithoutThrowOnRegisteringDisposableTransient());

            container.Register<IA, A>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var ts = new Task[8];
            for (int i = 0; i < ts.Length; i++)
            {
                ts[i] = Task.Run(() => container.Resolve<IA>());
            }

            Task.WaitAll(ts);
            Assert.AreEqual(1, A.Counter);
        }

        interface IA {}

        class A : IA
        { 
            public static int Counter;
            public A() 
            {
                Thread.Sleep(100);
                Console.WriteLine(++Counter);
            }
        }

    }
}