using System.Linq;
using NUnit.Framework;

namespace DryIoc.Samples
{
    [TestFixture]
    public class SelectConstructorWithAllResolvableArguments
    {
        [Test]
        public void Can_do_that()
        {
            var container = new Container();
            container.Register<IDependency, SomeDependency>();

            container.Register<SomeClient>(withConstructor: (type, request, registry) =>
            {
                var ctors = type.GetConstructors();
                if (ctors.Length == 0)
                    return null;

                if (ctors.Length == 1)
                    return ctors[0];

                var ctor = ctors
                    .Select(c => new { Ctor = c, Params = c.GetParameters() })
                    .OrderByDescending(x => x.Params.Length)
                    .FirstOrDefault(x => x.Params.All(p => registry.ResolveFactory(request.Push(p, registry), IfUnresolved.ReturnNull) != null));

                return ctor.ThrowIfNull("Unable to select constructor with all resolvable arguments when resolving {0}", request).Ctor;
            });

            var client = container.Resolve<SomeClient>();
            Assert.That(client.Dependency, Is.Not.Null);
        }

        public interface IDependency { }

        public class SomeDependency : IDependency { }

        public class SomeClient
        {
            public readonly int Seed;
            public readonly IService Service;
            public readonly IDependency Dependency;

            // Won't be selected because constructor with IDependency will be selected first.
            public SomeClient() { }

            // Won't be selected because nor Int32 nor IService is registered in Container.
            public SomeClient(int seed, IService service)
            {
                Service = service;
                Seed = seed;
            }

            // Will be selected because IDependency is registered in Container.
            public SomeClient(IDependency dependency)
            {
                Dependency = dependency;
            }
        }
    }
}
