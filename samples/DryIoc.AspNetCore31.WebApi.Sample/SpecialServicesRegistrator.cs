using DryIoc.AspNetCore31.WebApi.Sample.Services;
using DryIoc.MefAttributedModel;

namespace DryIoc.AspNetCore31.WebApi.Sample
{
    public static class SpecialServicesRegistrator
    {
        public static void Register(IRegistrator r)
        {
            // r.Register<IExportedService, ExportedService>();

            // Optionally using the MEF Exported services
            r.RegisterExports(typeof(ExportedService));
        }
    }
}