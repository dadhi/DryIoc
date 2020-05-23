using DryIoc.AspNetCore31.WebApi.Sample.Services;

namespace DryIoc.AspNetCore31.WebApi.Sample
{
    public static class BasicServicesRegistrator
    {
        public static void Register(IRegistrator r)
        {
            r.Register<ISingletonService, SingletonService>(Reuse.Singleton);
            r.Register<ITransientService, TransientService>(Reuse.Transient);
            r.Register<IScopedService, ScopedService>(Reuse.InCurrentScope);
        }
    }
}
