using DryIoc.AspNetCore.Sample.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static System.Environment;

namespace DryIoc.AspNetCore.Sample.Controllers
{
    public class LoggingController : Controller
    {
        private readonly ILogger<LoggingController> _logger;

        //[Import]
        public ITransientService Transient { get; set; }

        public LoggingController(ILoggerFactory loggingFactory, ITransientService Transient)
        {
            _logger = loggingFactory.CreateLogger<LoggingController>();
        }

        [Route("logging")]
        public IActionResult Index()
        {
            _logger.LogInformation("Hello from the Logging.");
            return Ok($"Everything is fine." + NewLine +
                "Transient property is injected: {Transient != null}");
        }
    }
}
