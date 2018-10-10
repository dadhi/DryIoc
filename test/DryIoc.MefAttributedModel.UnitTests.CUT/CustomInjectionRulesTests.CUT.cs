using System.ComponentModel.Composition;
using DryIocAttributes;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    [Export]
    public class ClientWithPrimitiveParameter
    {
        public IService Service { get; private set; }
        public string Message { get; private set; }

        public ClientWithPrimitiveParameter([ImportEx(ServiceKey.One)]IService service, string message)
        {
            Service = service;
            Message = message;
        }
    }

    [Export]
    public class ClientWithServiceAndPrimitiveProperty
    {
        [ImportEx(ServiceKey.One)]
        public IService Service { get; set; }

        public string Message { get; set; }
    } 
}
