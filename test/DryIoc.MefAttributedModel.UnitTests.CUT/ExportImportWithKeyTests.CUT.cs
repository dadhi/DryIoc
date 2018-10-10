using System.ComponentModel.Composition;
using DryIocAttributes;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    [Export]
    public class KeyClient
    {
        public IService Service { get; set; }

        public KeyClient([ImportEx(ServiceKey.One)]IService service)
        {
            Service = service;
        }
    }

    [ExportEx(ServiceKey.One, typeof(IService), IfAlreadyExported.Throw)]
    public class KeyService : IService { }

    [ExportMany(ContractKey = ServiceKey.OtherOne)]
    public class OtherKeyService : IService { }

    public enum ServiceKey { One, OtherOne }
}
