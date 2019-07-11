using System;
using System.Net.Http;

namespace Conn
{
    public class AccessToken
    {
        public AccessToken(
        )
        {
        }
    }


    public class AccessTokenCache
        : IAccessTokenCache
    {
        public AccessTokenCache(
        )
        {
        }
    }


    public class AccessTokenCacheClient
        : IAccessTokenClient
    {
        public AccessTokenCacheClient(
            IAccessTokenClient arg0,
            IAccessTokenCache arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IAccessTokenClient field0;
        public readonly IAccessTokenCache field1;
    }


    public class AccessTokenClient
        : IAccessTokenClient
    {
        public AccessTokenClient(
            IConnConfiguration arg0
        )
        {
            field0 = arg0;
        }

        public readonly IConnConfiguration field0;
    }


    public class GetIsUserAllowedToChangeEmailAddressLink
    {
        public GetIsUserAllowedToChangeEmailAddressLink(
        )
        {
        }
    }


    public class GetPasswordPolicyLink
    {
        public GetPasswordPolicyLink(
        )
        {
        }
    }


    public class GetUserApplications
    {
        public GetUserApplications(
        )
        {
        }
    }


    public class GetUserInfoLink
    {
        public GetUserInfoLink(
        )
        {
        }
    }


    public class GetUserLink
    {
        public GetUserLink(
        )
        {
        }
    }


    public class HttpClient
        : IHttpClient
    {
        public HttpClient(
            HttpClient arg0
        )
        {
            field0 = arg0;
        }

        public readonly HttpClient field0;

        public void Dispose()
        {
        }
    }


    public class HttpClientFactory
        : IHttpClientFactory
    {
        public HttpClientFactory(
            IConnConfiguration arg0
        )
        {
            field0 = arg0;
        }

        public readonly IConnConfiguration field0;
    }


    public interface IAccessTokenCache
    {
    }


    public interface IAccessTokenClient
    {
    }


    public interface IHttpClient
        : IDisposable
    {
    }


    public interface IHttpClientFactory
    {
    }


    public class ImageHelper
    {
        public ImageHelper(
        )
        {
        }
    }


    public interface IPublicUserService
    {
    }


    public class PublicUserService
        : IPublicUserService
    {
        public PublicUserService(
            IAccessTokenClient arg0,
            IHttpClientFactory arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IAccessTokenClient field0;
        public readonly IHttpClientFactory field1;
    }


    public interface IUserClient
    {
    }


    public class UserClient
        : IUserClient
    {
        public UserClient(
        )
        {
        }
    }


    public interface IUserHttpClientFactory
    {
    }


    public class UserHttpClientFactory
        : IUserHttpClientFactory
    {
        public UserHttpClientFactory(
            IAccessTokenClient arg0,
            IHttpClientFactory arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IAccessTokenClient field0;
        public readonly IHttpClientFactory field1;
    }


    public interface IUserService
    {
    }


    public class UserService
        : IUserService
    {
        public UserService(
            IAccessTokenClient arg0,
            IHttpClientFactory arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IAccessTokenClient field0;
        public readonly IHttpClientFactory field1;
    }


    public interface IConnConfiguration
    {
    }


    public class ConnConfiguration
        : IConnConfiguration
    {
        public ConnConfiguration(
        )
        {
        }
    }


    public class JpegPhotoHelper
    {
        public JpegPhotoHelper(
        )
        {
        }
    }


    public class JsonContent : StringContent
    {
        public JsonContent(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class PasswordPolicy
    {
        public PasswordPolicy(
        )
        {
        }
    }


    public class PatchUserLink
    {
        public PatchUserLink(
        )
        {
        }
    }


    public class PostAuthenticationLink
    {
        public PostAuthenticationLink(
        )
        {
        }
    }


    public class PostRequestAuthorizedChangeEmailAddressLink
    {
        public PostRequestAuthorizedChangeEmailAddressLink(
        )
        {
        }
    }


    public class PostRequestChangeEmailAddressLink
    {
        public PostRequestChangeEmailAddressLink(
        )
        {
        }
    }


    public class PostRequestChangeUnactivatedUsersEmailAddressLink
    {
        public PostRequestChangeUnactivatedUsersEmailAddressLink(
        )
        {
        }
    }


    public class PostResetPasswordLink
    {
        public PostResetPasswordLink(
        )
        {
        }
    }


    public class PostUserActivationRequestLink
    {
        public PostUserActivationRequestLink(
        )
        {
        }
    }


    public class PostUserLink
    {
        public PostUserLink(
        )
        {
        }
    }


    public class PutUserLink
    {
        public PutUserLink(
        )
        {
        }
    }


    public class PutUserUnlinkClientLink
    {
        public PutUserUnlinkClientLink(
        )
        {
        }
    }


    public class User
    {
        public User(
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
    }


    public class Api
    {
    }


    public class Application
    {
        public Application(
        )
        {
        }
    }


    public class UserApplications
    {
        public UserApplications(
        )
        {
        }
    }


    public partial class Response
    {
        public Response(
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
}