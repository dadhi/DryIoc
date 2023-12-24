using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue26_DryIOCSingletonFailureTest : ITest
    {
        public int Run()
        {
            Passes();
            Fails();
            return 2;
        }

        private readonly Container _container;

        [Test]
        public void Passes()
        {
            var container = new Container();
            container.RegisterInstance(new Producer());
            container.Register<Consumer>(Reuse.Singleton);
            container.Resolve<Consumer>();
            //Console.WriteLine(1);
            Resolve();
            //Console.WriteLine(2);
            Resolve();
            //Console.WriteLine(3);
            Resolve();
            //Console.WriteLine(4);
            Resolve();
            //Console.WriteLine(5);
            Resolve();
            //Console.WriteLine(6);
            Resolve();
        }

        [Test]
        public void Fails()
        {
            var container = new Container();
            container.RegisterInstance(new Producer());
            container.Register<Consumer>(Reuse.Singleton);
            //Console.WriteLine(1);
            Resolve();
            //Console.WriteLine(2);
            Resolve();
            //Console.WriteLine(3);
            Resolve();
            //Console.WriteLine(4);
            Resolve();
            //Console.WriteLine(5);
            Resolve();
            //Console.WriteLine(6);
            Resolve();
        }

        private void Resolve()
        {
            using (var scope = _container.OpenScope())
            {
                scope.Resolve<Consumer>();
            }
        }

        public class Producer { }

        public class Consumer
        {
            public Producer Producer { get; private set; }

            public Consumer(Producer producer)
            {
                Producer = producer;
            }
        }
    }
}
