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

        [Test]
        public void Passes()
        {
            var container = new Container();
            container.RegisterInstance(new Producer());
            container.Register<Consumer>(Reuse.Singleton);
            container.Resolve<Consumer>();
            Resolve();
            Resolve();
            Resolve();
            Resolve();
            Resolve();
            Resolve();

            void Resolve()
            {
                using (var scope = container.OpenScope())
                {
                    scope.Resolve<Consumer>();
                }
            }
        }

        [Test]
        public void Fails()
        {
            var container = new Container();
            container.RegisterInstance(new Producer());
            container.Register<Consumer>(Reuse.Singleton);
            Resolve();
            Resolve();
            Resolve();
            Resolve();
            Resolve();
            Resolve();

            void Resolve()
            {
                using (var scope = container.OpenScope())
                {
                    scope.Resolve<Consumer>();
                }
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
