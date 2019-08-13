using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using DryIoc;
using DryIoc.WebApi;

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
            //var container = new Container(rules => rules
            //    // With UseInterpretation it completes without error in 28 sec
            //    //.WithUseInterpretation()
            //    .With(FactoryMethod.ConstructorWithResolvableArguments))
            //    .WithWebApi(config);

            //Registrations.RegisterTypes(container, true);

            // The same SO exception as above with `singletonDecorators: true`
            //var container = new Container(rules => rules.With(FactoryMethod.ConstructorWithResolvableArguments)).WithWebApi(config);
            //Registrations.RegisterTypes(container, false);

            /*
             * This is another variation to run into Stack Overflow exception even when using WithoutFastExpressionCompiler -config
             * Seems like in master branches its now throwing:
             * System.InvalidOperationException: 'variable 'r' of type 'DryIoc.IResolverContext' referenced from scope '', but it is not defined'
             * previously I was able to reproduce Stack Overflow exception this way
             *
             */
            // WORKS:
            //Release mode - CPU: Core i7 8750H(12 threads), RAM: 16Gb
            //    -- Load Test Result--
            //  00:04:42.45

            // 13.08.2019 - 00:03:45.58

            var container = new Container(rules => rules
                .WithoutFastExpressionCompiler()
                .With(FactoryMethod.ConstructorWithResolvableArguments))
                .WithWebApi(config);
            Registrations.RegisterTypes(container, false);


            // This setup config WORKS, but uses a lot of memory
            //Release mode - CPU: Core i7 8750H(12 threads), RAM: 16Gb
            //  --Load Test Result --
            //  00:01:27.69

            // After centralized cache and fan-keyed resolution cache
            //  00:02:54.82

            // 13.08.2019 - still holds!

            //var container = new Container(rules => rules
            //    .WithoutFastExpressionCompiler()
            //    .With(FactoryMethod.ConstructorWithResolvableArguments))
            //    .WithWebApi(config);
            //Registrations.RegisterTypes(container, true);

            // Validate IoC registrations
            var results = container.Validate();
            if (results.Length > 0)
            {
                throw new Exception(results.ToString());
            }
            Console.WriteLine("No IoC Validation errors detected");

            var httpControllerType = typeof(IHttpController);

            // Get Controllers which would normally be used for routing web requests
            var controllers = Assembly.GetExecutingAssembly().GetLoadedTypes()
                .Where((t) =>
                    !t.IsAbstract && !t.IsInterface && !t.Name.Contains("Base") &&
                    httpControllerType.IsAssignableFrom(t))
                .ToArray();

            // Make sure all controllers can be resolved
            ResolveAllControllersOnce(container, controllers);

            ForceGarbageCollector();

            StartTest(controllers, container);
        }

        public static void StartTest(Type[] controllerTypes, IContainer container)
        {
            Console.WriteLine("-- Starting Load test --");

            var threadCount = 32;
            var iterations = 10;
            var i = 0;
            var threads = new Thread[threadCount];

            // Create threads
            for (i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(delegate()
                {
                    var controllers = controllerTypes;
                    var controllersCount = controllers.Length;

                    for (var j = 0; j < iterations; j++)
                    {
                        for (var k = 0; k < controllersCount; k++)
                        {
                            // Simulate WebAPI loop, open scope resolve and repeat
                            using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
                            {
                                scope.Resolve(controllers[k]);
                            }
                        }
                    }
                });
            }


            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // Start all
            for (i = 0; i < threadCount; i++)
            {
                threads[i].Start();
            }

            // Join all
            for (i = 0; i < threadCount; i++)
            {
                threads[i].Join();
            }

            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            var ts = stopWatch.Elapsed;
            Console.WriteLine("-- Load Test Result --");
            Console.WriteLine($"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");
        }

        static void ResolveAllControllersOnce(IContainer container, Type[] controllers)
        {
            using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
            {
                foreach (var controller in controllers)
                {
                    var t = scope.Resolve(controller);

                    Console.WriteLine(t.GetType().Name + " - resolved");
                }
            }

            Console.WriteLine("");
            Console.WriteLine("All Controllers in test resolved");
            Console.WriteLine("");
        }

        static void ForceGarbageCollector()
        {
            GC.Collect(0, GCCollectionMode.Forced, true);
            GC.Collect(1, GCCollectionMode.Forced, true);
            GC.Collect(2, GCCollectionMode.Forced, true);
        }
    }
}