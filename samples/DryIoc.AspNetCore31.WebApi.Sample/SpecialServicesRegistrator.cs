using DryIoc.AspNetCore31.WebApi.Sample.Services;

namespace DryIoc.AspNetCore31.WebApi.Sample
{
    public static class SpecialServicesRegistrator
    {
        public static void Register(IRegistrator r)
        {
            r.Register<IExportedService, ExportedService>();

            // optional: registering MEF Exported services
            //var assemblies = new[] { typeof(ExportedService).GetAssembly() };
            //r.RegisterExports(assemblies);
        }
    }
}