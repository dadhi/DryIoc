using Microsoft.AspNetCore.Http;

namespace DryIoc.AspNetCore.Sample.Components
{
    public class UserContext : IUserContext
    {
        public string UserName { get; }

        public UserContext(IHttpContextAccessor httpContext)
        {
            UserName = "Contextual me #" + httpContext?.HttpContext;
        }
    }
}
