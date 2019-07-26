using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using RM;
using Data;
using Logging;
using Logic;
using Organizations;
using Shared;
using IUserRepository = CUsers.Domain.IUserRepository;

namespace Mail.Service
{
    public class MailService
    {
        public MailService(
        )
        {
        }
    }


    public interface IInboundEmail
    {
    }


    public class InboundEmailAdapter
        : IInboundEmail
    {
        public InboundEmailAdapter(
            InboundEmail arg0
        )
        {
            field0 = arg0;
        }

        public readonly InboundEmail field0;
    }

    public class InboundEmail
    {
    }


    public interface IIncomingEmailHandler
    {
    }


    public class AppIncomingEmailHandler
        : IIncomingEmailHandler
    {
        public AppIncomingEmailHandler(
            IOrganizationRepository arg0,
            IMasterOrganizationRepository arg1,
            IOrganizationContextScopeService arg2,
            IUserRepository arg3,
            IContactRepository arg4,
            ICaseRepository arg5,
            ITaskRepository arg6,
            IActivityService arg7,
            IFileService arg8
        )
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
        }

        public readonly IMasterOrganizationRepository field1;
        public readonly IOrganizationContextScopeService field2;
        public readonly IUserRepository field3;
        public readonly IContactRepository field4;
        public readonly ICaseRepository field5;
        public readonly ITaskRepository field6;
        public readonly IActivityService field7;
        public readonly IFileService field8;
    }


    public class IncomingMailController : ApiController
    {
        public IncomingMailController(
            IIncomingEmailHandler arg0,
            ILogger arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IIncomingEmailHandler field0;
        public readonly ILogger field1;
    }


    public class AuthenticationFailureResult
        : IHttpActionResult
    {
        public AuthenticationFailureResult(
            string arg0,
            HttpRequestMessage arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly HttpRequestMessage field1;

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken) =>
            throw new System.NotImplementedException();
    }


    public class ServiceKeyAuthentication : ActionFilterAttribute
        , IAuthenticationFilter
    {
        public ServiceKeyAuthentication(
        )
        {
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken) =>
            throw new System.NotImplementedException();

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken) =>
            throw new System.NotImplementedException();
    }
}