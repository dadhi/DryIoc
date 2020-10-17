using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue337_Singleton_is_created_twice
    {
        [Test]
        public void Test()
        {
            var container = new Container();
            container.Register<A>(Reuse.Singleton);

            var ts = new Task[8];
            for (int i = 0; i < ts.Length; i++)
            {
                ts[i] = Task.Run(() => container.Resolve<A>());
            }

            Task.WaitAll(ts);
        }

        class A 
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