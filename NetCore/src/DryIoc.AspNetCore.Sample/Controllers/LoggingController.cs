using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DryIoc.AspNetCore.Sample.Controllers
{
    public class LoggingController : Controller
    {
        private ILogger<LoggingController> _logger;

        public LoggingController(ILoggerFactory loggingFactory)
        {
            _logger = loggingFactory.CreateLogger<LoggingController>();
        }
    }
}
