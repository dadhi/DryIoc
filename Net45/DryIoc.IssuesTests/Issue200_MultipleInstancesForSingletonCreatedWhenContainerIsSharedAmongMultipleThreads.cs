using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue200_MultipleInstancesForSingletonCreatedWhenContainerIsSharedAmongMultipleThreads
    {
        [Test]
        public void Inject_singleton_async_should_return_the_same_dependency()
        {
            var container = new Container();
            container.Register<U>();
            container.Register<S>(Reuse.Singleton);

            const int workerCount = 10;

            var tasks = new Task<U>[workerCount];
            for (var i = 0; i < workerCount; i++)
            {
                tasks[i] = Task.Run(() => container.Resolve<U>());
            }

            Task.WaitAll(tasks);

            for (var i = 1; i < tasks.Length; i++)
            {
                Assert.AreSame(tasks[i].Result.S, tasks[i - 1].Result.S);
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
