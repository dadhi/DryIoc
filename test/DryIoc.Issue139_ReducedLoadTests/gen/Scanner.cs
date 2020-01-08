using System.ComponentModel;
using System.Runtime.Serialization;
using Framework;

namespace Scanner
{
    public interface IScannerService
    {
    }


    public class MetadataFromScanner
    {
        public MetadataFromScanner(
        )
        {
        }
    }


    public class ErrorModel
    {
        public ErrorModel(
        )
        {
        }
    }

    public interface ISettings
    {
    }


    public class Settings
        : ISettings
    {
        public Settings(
        )
        {
        }
    }


    public class ScannerService
        : IScannerService
    {
        public ScannerService(
            ISettings arg0,
            IContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly ISettings field0;
        public readonly IContextService field1;
    }


    public class PhotoStatusDto
    {
    }


    public class AuthenticationFault
        : IExtensibleDataObject, INotifyPropertyChanged
    {
        public AuthenticationFault(
        )
        {
        }

        public ExtensionDataObject ExtensionData { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }


    public class PhotoMetadataDto
        : IExtensibleDataObject, INotifyPropertyChanged
    {
        public PhotoMetadataDto(
        )
        {
        }

        public ExtensionDataObject ExtensionData { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }


    public interface IAppPhotoService
    {
    }


    public class AppPhotoServiceClient : ClientBase<IAppPhotoService>
        , IAppPhotoService
    {
        public AppPhotoServiceClient(
        )
        {
        }

        public AppPhotoServiceClient(
            string arg0
        ) : base()
        {
        }

        public AppPhotoServiceClient(
            string arg0,
            string arg1
        ) : base()
        {
        }

        public AppPhotoServiceClient(
            string arg0,
            EndpointAddress arg1
        ) : base()
        {
            field1 = arg1;
        }

        public AppPhotoServiceClient(
            Binding arg0,
            EndpointAddress arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly Binding field0;
        public readonly EndpointAddress field1;
    }

    public class Binding
    {
    }

    public class EndpointAddress
    {
    }

    public class ClientBase<T>
    {
    }


    public interface IAppPhotoServiceChannel
        : IAppPhotoService, IClientChannel
    {
    }

    public interface IClientChannel
    {
    }
}