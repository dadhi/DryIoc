namespace Conn.Adapter
{
    public class ConnAuthenticationService
        : IConnAuthenticationService
    {
        public ConnAuthenticationService(
            IHttpClientFactory arg0,
            IAccessTokenClient arg1,
            IConnClient arg2,
            IConnErrorFactory arg3,
            IConnPublicApiClientFactory arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IHttpClientFactory field0;
        public readonly IAccessTokenClient field1;
        public readonly IConnClient field2;
        public readonly IConnErrorFactory field3;
        public readonly IConnPublicApiClientFactory field4;
    }


    public class ConnCertificateStorage
        : IConnCertificateStorage
    {
        public ConnCertificateStorage(
        )
        {
        }
    }


    public class ConnClient
        : IConnClient
    {
        public ConnClient(
            IAccessTokenClient arg0,
            IConnConfiguration arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IAccessTokenClient field0;
        public readonly IConnConfiguration field1;
    }


    public class ConnClientService
        : IConnClientService
    {
        public ConnClientService(
            IHttpClientFactory arg0,
            IAccessTokenClient arg1,
            IConnConfiguration arg2,
            IConnClient arg3,
            IConnErrorFactory arg4,
            IConnPublicApiClientFactory arg5
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IHttpClientFactory field0;
        public readonly IAccessTokenClient field1;
        public readonly IConnConfiguration field2;
        public readonly IConnClient field3;
        public readonly IConnErrorFactory field4;
        public readonly IConnPublicApiClientFactory field5;
    }


    public class ConnEmailChangeService
        : IConnEmailChangeService
    {
        public ConnEmailChangeService(
            IHttpClientFactory arg0,
            IAccessTokenClient arg1,
            IConnClient arg2,
            IConnErrorFactory arg3,
            IConnPublicApiClientFactory arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IHttpClientFactory field0;
        public readonly IAccessTokenClient field1;
        public readonly IConnClient field2;
        public readonly IConnErrorFactory field3;
        public readonly IConnPublicApiClientFactory field4;
    }


    public class ConnErrorFactory
        : IConnErrorFactory
    {
        public ConnErrorFactory(
        )
        {
        }
    }


    public class ConnPasswordPolicyService
        : IConnPasswordPolicyService
    {
        public ConnPasswordPolicyService(
            IHttpClientFactory arg0,
            IAccessTokenClient arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IHttpClientFactory field0;
        public readonly IAccessTokenClient field1;
    }


    public class ConnPublicApiClientFactory
        : IConnPublicApiClientFactory
    {
        public ConnPublicApiClientFactory(
            IAccessTokenClient arg0,
            IConnClient arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IAccessTokenClient field0;
        public readonly IConnClient field1;
    }


    public class ConnUserApplicationsService
        : IConnUserApplicationsService
    {
        public ConnUserApplicationsService(
            IHttpClientFactory arg0,
            IConnConfiguration arg1,
            IConnCertificateStorage arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IHttpClientFactory field0;
        public readonly IConnConfiguration field1;
        public readonly IConnCertificateStorage field2;
    }


    public class ConnUserInfoService
        : IConnUserInfoService
    {
        public ConnUserInfoService(
            IHttpClientFactory arg0
        )
        {
            field0 = arg0;
        }

        public readonly IHttpClientFactory field0;
    }


    public class ConnUserService
        : IConnUserService
    {
        public ConnUserService(
            IHttpClientFactory arg0,
            IAccessTokenClient arg1,
            IConnClient arg2,
            IConnErrorFactory arg3,
            IConnPublicApiClientFactory arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IHttpClientFactory field0;
        public readonly IAccessTokenClient field1;
        public readonly IConnClient field2;
        public readonly IConnErrorFactory field3;
        public readonly IConnPublicApiClientFactory field4;
    }


    public class ConnWebHookService
        : IConnWebHookService
    {
        public ConnWebHookService(
            IConnErrorFactory arg0,
            IConnPublicApiClientFactory arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IConnErrorFactory field0;
        public readonly IConnPublicApiClientFactory field1;
    }


    public interface IConnAuthenticationService
    {
    }


    public interface IConnCertificateStorage
    {
    }


    public interface IConnClient
    {
    }


    public interface IConnClientService
    {
    }


    public interface IConnEmailChangeService
    {
    }


    public interface IConnErrorFactory
    {
    }


    public interface IConnPasswordPolicyService
    {
    }


    public interface IConnPublicApiClientFactory
    {
    }


    public interface IConnUserApplicationsService
    {
    }


    public interface IConnUserInfoService
    {
    }


    public interface IConnUserService
    {
    }


    public interface IConnWebHookService
    {
    }


    public class AccessToken
    {
        public AccessToken(
        )
        {
        }

        public AccessToken(
            AccessToken arg0
        )
        {
            field0 = arg0;
        }

        public readonly AccessToken field0;
    }


    public class PasswordPolicy
    {
        public PasswordPolicy(
        )
        {
        }

        public PasswordPolicy(
            PasswordPolicy arg0
        )
        {
            field0 = arg0;
        }

        public readonly PasswordPolicy field0;
    }


    public class PendingChangedEmail
    {
        public PendingChangedEmail(
        )
        {
        }
    }


    public class UserInfo
    {
        public UserInfo(
        )
        {
        }

        public UserInfo(
            UserInfo arg0
        )
        {
            field0 = arg0;
        }

        public readonly UserInfo field0;
    }


    public class Application
    {
        public Application(
        )
        {
        }

        public Application(
            Application arg0
        )
        {
            field0 = arg0;
        }

        public readonly Application field0;
    }


    public class ConnGeneralResponseModel
    {
        public ConnGeneralResponseModel(
        )
        {
        }
    }


    public class ConnPendingChangedEmailResponseModel : ConnGeneralResponseModel
    {
        public ConnPendingChangedEmailResponseModel(
        )
        {
        }
    }


    public class ConnResponseCode
    {
    }


    public class ConnUserActivationResponseModel : ConnGeneralResponseModel
    {
        public ConnUserActivationResponseModel(
        )
        {
        }
    }


    public class ConnUserResponseModel : ConnGeneralResponseModel
    {
        public ConnUserResponseModel(
        )
        {
        }
    }


    public class ConnWebHookCreateResponseModel : ConnGeneralResponseModel
    {
        public ConnWebHookCreateResponseModel(
        )
        {
        }
    }


    public class ConnWebHooksGetResponseModel : ConnGeneralResponseModel
    {
        public ConnWebHooksGetResponseModel(
        )
        {
        }
    }


    public class WebHook
    {
        public WebHook(
        )
        {
        }

        public WebHook(
            GetWebHookOutputModel arg0
        )
        {
            field0 = arg0;
        }

        public readonly GetWebHookOutputModel field0;
    }

    public class GetWebHookOutputModel
    {
    }


    public class WebHookEvent
    {
    }


    public class CertificateType
    {
    }


    public class PublicApiErrorContentModel
    {
        public PublicApiErrorContentModel(
        )
        {
        }
    }


    public class ProxyApiErrorContentModel
    {
        public ProxyApiErrorContentModel(
        )
        {
        }
    }


    public class ApiOperation
    {
    }


    public class ErrorModel
    {
        public ErrorModel(
        )
        {
        }
    }


    public class Error
    {
        public Error(
        )
        {
        }
    }


    public class ProxyApiSimpleErrorModel
    {
        public ProxyApiSimpleErrorModel(
        )
        {
        }
    }
}