using DryIoc;
using DryIoc.MefAttributedModel;
using Microsoft.AspNet.Http;
using Web.Components;

namespace Web
{
    public static class Bootstrap
    {
        public static void RegisterServices(IRegistrator registrator)
        {
            registrator.Register<ISingletonService>(Reuse.Singleton, 
                Made.Of(() => BarSingletonService.FactoryMethod()));
            registrator.Register<IPerRequestService>(Reuse.InWebRequest,
                Made.Of(() => BarPerRequestService.FactoryMethod(Arg.Of<ISingletonService>())));
            registrator.Register<ITransientService>(Reuse.Transient, 
                Made.Of(() => BarTransientService.FactoryMethod(Arg.Of<IPerRequestService>())));

            registrator.Register<FooServiceHttpContext>(Reuse.InWebRequest);
            registrator.Register<BarServiceHttpContext>(Made.Of(
                () => BarServiceHttpContext.FactoryMethod(Arg.Of<IHttpContextAccessor>(), Arg.Of<ISingletonService>())), 
                Reuse.InWebRequest);
        }

        public static void RegisterExportedServices(IRegistrator registrator)
        {
            registrator.RegisterExports(
                typeof(BarSingletonService), 
                typeof(BarPerRequestService), 
                typeof(BarTransientService), 
                typeof(FooServiceHttpContext), 
                typeof(BarServiceHttpContext)
            );
        }
    }
}
