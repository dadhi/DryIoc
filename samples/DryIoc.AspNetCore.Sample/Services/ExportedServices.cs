using System.ComponentModel.Composition;

namespace DryIoc.AspNetCore.Sample.Services
{
    //[Export(typeof(IExportedService))]
    public class ExportedService : IExportedService
    {
        public ITransientService Transient { get; }

        public ExportedService(ITransientService transient)
        {
            Transient = transient;
        }
    }
}
