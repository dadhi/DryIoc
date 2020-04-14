using System.Collections.Generic;
using DryIoc.AspNetCore31.WebApi.Sample.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DryIoc.AspNetCore31.WebApi.Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public IScopedService Scoped { get; }
        public ITransientService Transient { get; }
        public IExportedService Imported { get; }

        /// <summary> Example of injected property, <see cref="Startup.CreateMyPreConfiguredContainer"/> for how to setup this. </summary>
        public IScopedService ScopedServiceInjectedProperty { get; set; }

        public HomeController(ILogger<HomeController> logger,
            IScopedService scoped, ITransientService transient, IExportedService imported = null)
        {
            _logger = logger;
            Scoped = scoped;
            Transient = transient;
            Imported = imported;
        }

        [HttpGet]
        [Route("/")]
        [Route("/api")]
        [Route("/api/welcome")]
        public IEnumerable<string> Get()
        {
            _logger.LogInformation($"Injected service types: '{Transient?.GetType().Name}', '{Scoped?.GetType().Name}', '{Imported?.GetType().Name}'");
            return new[]
            {
                $"Transient services are different: {Transient != Scoped?.Transient}",
                $"Singletons are the same: {Transient?.Singleton == Scoped?.Singleton}",
                $"Exported service is successfully imported: {Imported != null}"
            };
        }

        [HttpGet]
        [Route("/greet/{name}")]
        [Route("/api/greet/{name}")]
        public string Get(string name)
        {
            _logger.LogInformation($"Greeted {name}, Injected property '{ScopedServiceInjectedProperty?.GetType().Name}'");
            return $"Hi {name}! Courtesy to injected property '{ScopedServiceInjectedProperty?.GetType().Name}'";
        }
    }
}
