using System;

namespace DryIoc.WebApi.Owin.Sample.Services
{
    class ConsoleLogger : ILoggingService
    {
        public void Error(string message)
        {
            Console.WriteLine(message);
        }
    }
}