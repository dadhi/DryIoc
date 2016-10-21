using DryIoc.AspNetCore.Sample.Services;

namespace DryIoc.AspNetCore.Sample
{
    public class CompositionRoot
    {
        // If you need the whole container then change parameter type from IRegistrator to IContainer
        public CompositionRoot(IRegistrator r) 
        { 
            r.Register<ISingletonService, SingletonService>(Reuse.Singleton);
            r.Register<ITransientService, TransientService>(Reuse.Transient);
            r.Register<IScopedService, ScopedService>(Reuse.InCurrentScope);
        }
    }
}