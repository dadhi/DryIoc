using System;
using System.Diagnostics;
using System.Web.Http;
using DryIoc;
using DryIoc.WebApi;

namespace LoadTest
{
    static class InvalidProgramExceptionTest
    {
        public static IContainer GetContainerForTest()
        {
            var container = new Container(rules => rules
                .WithoutInterpretationForTheFirstResolution()
                .WithoutUseInterpretation()
                .With(FactoryMethod.ConstructorWithResolvableArguments))
                .WithWebApi(new HttpConfiguration());

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
            Console.WriteLine("# Starting InvalidProgramException test");

            var controllerTypes = TestHelper.GetAllControllers();
            var container = GetContainerForTest();
            Console.WriteLine($"## Using container: {container}");

            Console.WriteLine("## Validating everything...");
            var sw = Stopwatch.StartNew();
            var validateResult = container.Validate();
            Console.WriteLine($"Validated in {sw.Elapsed.TotalMilliseconds} ms");


            if (validateResult.Length != 0)
            {
                throw new Exception(validateResult.ToString());
            }

            Console.WriteLine("### Resolving 1st time:");
            ResolveAllControllers(container, controllerTypes);
            Console.WriteLine("### Resolving 2nd time:");
            ResolveAllControllers(container, controllerTypes);
        }
    }
}
