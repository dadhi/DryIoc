using DryIoc.AspNetCore.Sample.Services;
using DryIoc.MefAttributedModel;

namespace DryIoc.AspNetCore.Sample
{
    public class MyCompositionRoot
    {
        // If you need the whole container then change the parameter type from IRegistrator to IContainer
        public MyCompositionRoot(IRegistrator r)
        {
            r.Register<ISingletonService, SingletonService>(Reuse.Singleton);
            r.Register<ITransientService, TransientService>(Reuse.Transient);
            r.Register<IScopedService, ScopedService>(Reuse.InCurrentScope);

            // optional: registering MEF Exported services
            var assemblies = new[] { typeof(ExportedService).GetAssembly() };
            r.RegisterExports(assemblies);
        }
    }
}