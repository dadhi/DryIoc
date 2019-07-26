using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using DryIoc;
using DryIoc.WebApi;
using Logic;

namespace LoadTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new HttpConfiguration();

            /*
             * Following container setup will cause process to fail to Stack Overflow exception
             *
             * Make Release build and run .exe file from command line,
             * if it does not fail you can increase threadCount and iterations  integers to increase pressure
             *
             * Reproduces https://github.com/dadhi/DryIoc/issues/139
             */
            var container = new Container(rules => rules.With(FactoryMethod.ConstructorWithResolvableArguments)).WithWebApi(config);

            Registrations.RegisterTypes(container, true);

            var httpControllerType = typeof(IHttpController);

            // Get Controllers which would normally be used for routing web requests
            var controllers = Assembly.GetExecutingAssembly().GetLoadedTypes()
                .Where((t) =>
                    !t.IsAbstract && !t.IsInterface && !t.Name.Contains("Base") &&
                    httpControllerType.IsAssignableFrom(t))
                .ToArray();


            StartTest(controllers, container);
        }

        public static void StartTest(Type[] controllerTypes, IContainer container)
        {
            Console.WriteLine("-- Starting Load test --");

            var threadCount = 32;
            var iterations = 100;
            var i = 0;

            var controllers = controllerTypes;

            using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
            {
                scope.Resolve(controllers[148]);
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            var ts = stopWatch.Elapsed;
            Console.WriteLine("-- Load Test Result --");
            Console.WriteLine($"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");
        }
    }
}