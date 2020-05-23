using System;
using System.Diagnostics;
using System.Web.Http;
using DryIoc;
using DryIoc.WebApi;

namespace LoadTest
{
    static class SplitDependencyGraphTest
    {

        public static IContainer GetContainerForTest(int depth)
        {
            var container = new Container((rules) =>
                rules
                    .WithDependencyCountInLambdaToSplitBigObjectGraph(depth)
                    .WithoutInterpretationForTheFirstResolution()
                    .WithoutUseInterpretation()
                    .With(FactoryMethod.ConstructorWithResolvableArguments)
            ).WithWebApi(new HttpConfiguration());

            Registrations.RegisterTypes(container, true);

            return container;
        }

        private static void ResolveAllControllers(IContainer container, Type[] controllerTypes)
        {
            Console.WriteLine($"Starting resolving all {controllerTypes.Length} controllers...");
            var sw = Stopwatch.StartNew();
            foreach (var controllerType in controllerTypes)
            {
                using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
                {
                    var controller = scope.Resolve(controllerType);

                    if (controller == null)
                    {
                        throw new Exception("Invalid result!");
                    }
                }
            }
            
            Console.WriteLine($"Finished resolving controllers in '{sw.Elapsed.TotalMilliseconds}' ms");
            sw.Stop();
        }

        public static void Start()
        {
            Console.WriteLine("# Starting Object Graph split test");

            var controllerTypes = TestHelper.GetAllControllers();

            for (var depth = 5; depth < 2000; depth+=200)
            {
                Console.WriteLine("## DependencyCount - " + depth);

                var container = GetContainerForTest(depth);

                Console.WriteLine("### Resolving 1st time:");
                ResolveAllControllers(container, controllerTypes);
                Console.WriteLine("### Resolving 2nd time:");
                ResolveAllControllers(container, controllerTypes);
            }
        }
    }
}
