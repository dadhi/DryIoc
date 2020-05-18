using System;
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
