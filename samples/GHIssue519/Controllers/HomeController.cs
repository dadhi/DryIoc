using Microsoft.AspNetCore.Mvc;

namespace test.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}