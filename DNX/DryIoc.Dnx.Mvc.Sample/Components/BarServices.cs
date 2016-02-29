using Microsoft.AspNet.Http;

/***
 ** Set of services created by static factory method
 ***/
namespace Web.Components
{
    public sealed class BarSingletonService : ServiceBase, ISingletonService
    {
        private BarSingletonService() { }
        public static BarSingletonService FactoryMethod() { return new BarSingletonService(); }
    }

    public sealed class BarPerRequestService : ServiceBase, IPerRequestService
    {
        private BarPerRequestService(ISingletonService singletonService)
        {
            SingletonService = singletonService;
        }

        public static BarPerRequestService FactoryMethod(ISingletonService singletonService)
        {
            return new BarPerRequestService(singletonService);
        }

        public ISingletonService SingletonService { get; private set; }
    }

    public sealed class BarTransientService : ServiceBase, ITransientService
    {
        private BarTransientService(IPerRequestService perRequestService)
        {
            PerRequestService = perRequestService;
        }

        public static BarTransientService FactoryMethod(IPerRequestService perRequestService)
        {
            return new BarTransientService(perRequestService);
        }

        public IPerRequestService PerRequestService { get; private set; }
    }

    public sealed class BarServiceHttpContext : ServiceBase
    {
        private BarServiceHttpContext(HttpContext httpContext, ISingletonService singletonService)
        {
            HttpContext = httpContext;
            SingletonService = singletonService;
        }

        public static BarServiceHttpContext FactoryMethod(HttpContext httpContext, ISingletonService singletonService)
        {
            return new BarServiceHttpContext(httpContext, singletonService);
        }

        public HttpContext HttpContext { get; private set; }
        public ISingletonService SingletonService { get; private set; }
    }
}
