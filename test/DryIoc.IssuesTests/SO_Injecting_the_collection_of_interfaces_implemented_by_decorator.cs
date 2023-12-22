using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class SO_Injecting_the_collection_of_interfaces_implemented_by_decorator : ITest
    {
        public int Run()
        {
            Answer();
            return 1;
        }

        [Test]
        public void Answer()
        {
            var c = new Container();

            c.Register<IService, Service>(Reuse.Singleton);
            c.RegisterMany<CachingService>(Reuse.Singleton); // registers both CashingService and IFlushable with the same implementing instance
            c.RegisterDelegate<CachingService, IService>(cs => cs.GetDecoratedService(), setup: Setup.Decorator);

            var s = c.Resolve<IService>();
            Assert.IsNotNull(s);
            var cs = c.Resolve<CachingService>();
            Assert.IsTrue(cs.ServiceDecorated); // check the service indeed is decorated
            
            var f = c.Resolve<IFlushable>();
            Assert.AreSame(cs, f); // check that the flushable and caching service are the same instance
        }

        public interface IService { }

        public class Service : IService { }

        // no need to implement IService for the decorator, we may use its method instead
        public class CachingService : IFlushable
        {
            public readonly IService Service;
            public bool ServiceDecorated;

            public CachingService(IService service) => Service = service;

            public IService GetDecoratedService()
            {
                ServiceDecorated = true; // do something with decorated service
                return Service; 
            }

            public void Flush() { }
        }

        public interface IFlushable
        {
            public void Flush();
        }
    }
}
