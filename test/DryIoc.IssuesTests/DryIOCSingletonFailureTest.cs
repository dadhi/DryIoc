using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    /// <summary>
    /// Issue #26: Singleton creation from new scope fails
    /// </summary>
    [TestFixture]
    public class DryIOCSingletonFailureTest
    {
        private readonly Container _container;

        public DryIOCSingletonFailureTest()
        {
            _container = new Container();
            _container.RegisterInstance(new Producer());
            _container.Register<Consumer>(Reuse.Singleton);
        }

        [Test]
        public void Passes()
        {
            _container.Resolve<Consumer>();
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
