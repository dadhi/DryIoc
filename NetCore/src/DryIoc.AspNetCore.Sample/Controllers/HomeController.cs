// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

using DryIoc.AspNetCore.Sample.Components;
using Microsoft.AspNetCore.Mvc;
using DryIoc.AspNetCore.Sample.Models.Home;

namespace DryIoc.AspNetCore.Sample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserContext _user;

        public HomeController(IUserContext user)
        {
            _user = user;
        }

        [Route("/")]
        [Route("home/index")]
        public IActionResult Index()
        {
            return View(_user);
        }

        [Route("home/greet/{username}")]
        public IActionResult Greet(string username)
        {
            return Ok(new Greeting { Username = username });
        }
    }
}
