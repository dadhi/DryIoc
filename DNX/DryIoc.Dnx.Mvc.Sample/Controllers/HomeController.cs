using Microsoft.AspNet.Mvc;
using Web.Components;
using Web.Models.Home;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISingletonService _singleton1, _singleton2;
        private readonly IPerRequestService _perRequest1, _perRequest2;
        private readonly ITransientService _transinet1, _transinet2;
        private readonly FooServiceHttpContext _fooScopeInstance1, _fooScopeInstance2;
        private readonly BarServiceHttpContext _barScopeInstance1, _barScopeInstance2;

        public HomeController(
            ISingletonService singleton1, ISingletonService singleton2,
            IPerRequestService perRequest1, IPerRequestService perRequest2,
            ITransientService transinet1, ITransientService transinet2,
            FooServiceHttpContext fooScopeInstance1, FooServiceHttpContext fooScopeInstance2,
            BarServiceHttpContext barScopeInstance1, BarServiceHttpContext barScopeInstance2
        )
        {
            _singleton1 = singleton1;
            _singleton2 = singleton2;
            _perRequest1 = perRequest1;
            _perRequest2 = perRequest2;
            _transinet1 = transinet1;
            _transinet2 = transinet2;
            _fooScopeInstance1 = fooScopeInstance1;
            _fooScopeInstance2 = fooScopeInstance2;
            _barScopeInstance1 = barScopeInstance1;
            _barScopeInstance2 = barScopeInstance2;
        }

        public IActionResult Index()
        {
            return View(
                new IndexModel
                {
                    Singleton1Id = _singleton1.InstanceId,
                    Singleton2Id = _singleton2.InstanceId,
                    PerRequest1Id = _perRequest1.InstanceId,
                    PerRequest2Id = _perRequest2.InstanceId,
                    Transient1Id = _transinet1.InstanceId,
                    Transient2Id = _transinet2.InstanceId,
                    FooScopeInstance1Id = _fooScopeInstance1.InstanceId,
                    FooScopeInstance2Id = _fooScopeInstance2.InstanceId,
                    BarScopeInstance1Id = _barScopeInstance1.InstanceId,
                    BarScopeInstance2Id = _barScopeInstance2.InstanceId,
                });
        }
    }
}
