using System.ComponentModel.Composition;
using DryIocAttributes;
using Microsoft.AspNet.Http;

/***
 ** Set of services with public constructor
 ***/
namespace Web.Components
{
    public sealed class FooSingletonService : ServiceBase, ISingletonService { }
    public sealed class FooPerRequestService : ServiceBase, IPerRequestService { }
    public sealed class FooTransientService : ServiceBase, ITransientService { }

    public sealed class FooServiceHttpContext : ServiceBase
    {
        public FooServiceHttpContext(HttpContext context) { }
    }
}