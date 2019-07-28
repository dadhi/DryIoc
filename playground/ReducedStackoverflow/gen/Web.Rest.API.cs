using System.Web.Http;
using Framework;
using Logic;
using Shared;

namespace Web.Rest.API
{
    public class ApiControllerBase : ApiController
    {
        public ApiControllerBase()
        {
        }

        public ApiControllerBase(IContextService<IPsaContext> contextService)
        {
            _contextService = contextService;
        }

        protected IContextService<IPsaContext> _contextService;
    }

    public class EmailController : ApiControllerBase
    {
        public EmailController(
            IPdfCreationHandlerService arg4
        ) : base()
        {
        }
    }
}