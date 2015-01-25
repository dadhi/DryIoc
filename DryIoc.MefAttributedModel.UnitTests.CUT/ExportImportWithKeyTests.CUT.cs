using System.ComponentModel.Composition;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    [Export]
    public class KeyClient
    {
        public IService Service { get; set; }

        public KeyClient([ImportWithKey(ServiceKey.One)]IService service)
        {
            Service = service;
        }
    }

    [ExportWithKey(ServiceKey.One, typeof(IService))]
    public class KeyService : IService { }

    [ExportMany(ContractKey = ServiceKey.OtherOne)]
    public class OtherKeyService : IService { }

    public enum ServiceKey { One, OtherOne }
}
