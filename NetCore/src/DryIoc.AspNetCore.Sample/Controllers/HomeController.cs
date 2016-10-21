// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNetCore.Mvc;
using DryIoc.AspNetCore.Sample.Models.Home;
using DryIoc.AspNetCore.Sample.Services;

namespace DryIoc.AspNetCore.Sample.Controllers
{
    public class HomeController : Controller
    {
        public IScopedService Scoped { get; }
        public ITransientService Transient { get; }

        public HomeController(IScopedService scoped, ITransientService transient, TransientService s)
        {
            Scoped = scoped;
            Transient = transient;
        }

        [Route("/")]
        [Route("home/index")]
        public IActionResult Index()
        {
            var model = new HomeModel
            {
                Message = 
                    $"Transient services are different: {Transient != Scoped.Transient}, " +
                    $"but Singletons are the same: {Transient.Singleton == Scoped.Singleton} "
            };

            return View(model);
        }

        // just for route test
        [Route("home/greet/{username}")]
        public IActionResult Greet(string username)
        {
            return Ok("Hi " + username);
        }
    }
}
