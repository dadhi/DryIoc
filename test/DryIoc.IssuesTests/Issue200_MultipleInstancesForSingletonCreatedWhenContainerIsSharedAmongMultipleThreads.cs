using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue200_MultipleInstancesForSingletonCreatedWhenContainerIsSharedAmongMultipleThreads : ITest
    {
        public int Run()
        {
            Inject_singleton_async_should_return_the_same_dependency();
            return 1;
        }

        [Test]
        public void Inject_singleton_async_should_return_the_same_dependency()
        {
            var container = new Container();
            container.Register<U>();
            container.Register<S>(Reuse.Singleton);

            const int workerCount = 4;
            const int repeatTimes = 5;

            for (var i = 0; i < repeatTimes; i++)
            {
                var tasks = new Task<U>[workerCount];
                for (var j = 0; j < workerCount; j++)
                {
                    tasks[j] = Task.Run(async () =>
                    {
                        await Task.Delay(5);
                        return container.Resolve<U>();
                    });
                }

                Task.WaitAll(tasks);

                for (var j = 1; j < tasks.Length; j++)
                    Assert.AreSame(tasks[j].Result.S, tasks[j - 1].Result.S);
            }
        }

        public class S { }

        public class U
        { 
            public S S { get; private set; }

            public U(S s)
            {
                S = s;
            }
        }
    }
}
