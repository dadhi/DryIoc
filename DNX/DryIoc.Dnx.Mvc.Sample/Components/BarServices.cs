using System.ComponentModel.Composition;
using DryIocAttributes;
using Microsoft.AspNet.Http;

namespace Web.Components
{
    [Export, AsFactory]
    public sealed class BarSingletonService : ServiceBase, ISingletonService
    {
        private BarSingletonService() { }

        [Export]
        public static ISingletonService FactoryMethod() { return new BarSingletonService(); }
    }

    [Export, AsFactory]
    public sealed class BarPerRequestService : ServiceBase, IPerRequestService
    {
        private BarPerRequestService(ISingletonService singletonService)
        {
            SingletonService = singletonService;
        }

        [Export, WebRequestReuse]
        public static IPerRequestService FactoryMethod(ISingletonService singletonService)
        {
            return new BarPerRequestService(singletonService);
        }

        public ISingletonService SingletonService { get; private set; }
    }

    [Export, AsFactory]
    public sealed class BarTransientService : ServiceBase, ITransientService
    {
        private BarTransientService(IPerRequestService perRequestService)
        {
            PerRequestService = perRequestService;
        }

        [Export, TransientReuse]
        public static ITransientService FactoryMethod(IPerRequestService perRequestService)
        {
            return new BarTransientService(perRequestService);
        }

        public IPerRequestService PerRequestService { get; private set; }
    }

    [Export, AsFactory]
    public sealed class BarServiceHttpContext : ServiceBase
    {
        private BarServiceHttpContext(HttpContext httpContext, ISingletonService singletonService)
        {
            HttpContext = httpContext;
            SingletonService = singletonService;
        }

        [Export, WebRequestReuse]
        public static BarServiceHttpContext FactoryMethod(IHttpContextAccessor httpContextAccessor, ISingletonService singletonService)
        {
            return new BarServiceHttpContext(httpContextAccessor.HttpContext, singletonService);
        }

        public HttpContext HttpContext { get; private set; }

        public ISingletonService SingletonService { get; private set; }
    }
}
