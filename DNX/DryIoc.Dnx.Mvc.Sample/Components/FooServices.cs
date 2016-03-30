using System.ComponentModel.Composition;
using System.Diagnostics;
using DryIocAttributes;
using Microsoft.AspNet.Http;

namespace Web.Components
{
    [Export, WebRequestReuse]
    public sealed class FooServiceHttpContext : ServiceBase
    {
        public FooServiceHttpContext(IHttpContextAccessor context)
        {
            Debug.Assert(context?.HttpContext != null, "Suddenly null HttpContext");
        }
    }
}