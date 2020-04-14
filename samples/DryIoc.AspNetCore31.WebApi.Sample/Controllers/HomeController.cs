using System.Collections.Generic;
using DryIoc.AspNetCore31.WebApi.Sample.Services;
using Microsoft.AspNetCore.Mvc;

namespace DryIoc.AspNetCore31.WebApi.Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        public IScopedService Scoped { get; }
        public ITransientService Transient { get; }
        public IExportedService Imported { get; }

        public HomeController(IScopedService scoped, ITransientService transient, IExportedService imported = null)
        {
            Scoped = scoped;
            Transient = transient;
            Imported = imported;
        }

        [HttpGet]
        [Route("/")]
        [Route("/api")]
        [Route("/api/welcome")]
        public IEnumerable<string> Get() => new[]
        {
            $"Transient services are different: {Transient != Scoped.Transient}",
            $"Singletons are the same: {Transient.Singleton == Scoped.Singleton}",
            $"Exported service is successfully imported: {Imported != null}"
        };

        [HttpGet]
        [Route("/api/greet/{name}")]
        public string Get(string name) => $"Hi {name}!";
    }
}
