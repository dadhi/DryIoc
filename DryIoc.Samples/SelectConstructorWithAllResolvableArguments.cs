using System.Linq;
using NUnit.Framework;

namespace DryIoc.Samples
{
    [TestFixture]
    public class SelectConstructorWithAllResolvableArguments
    {
        [Test]
        public void Can_do_that_manually()
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

        [Test]
        public void Container_is_providing_method_for_that()
        {
            var container = new Container();
            container.Register<IDependency, SomeDependency>();

            container.Register<SomeClient>(withConstructor: ReflectionFactory.SelectConstructorWithAllResolvableArguments);

            var client = container.Resolve<SomeClient>();
            Assert.That(client.Dependency, Is.Not.Null);
        }

        [Test]
        public void Container_is_providing_method_for_that_Test_default_constructor()
        {
            var container = new Container();

            container.Register<SomeClient>(withConstructor: ReflectionFactory.SelectConstructorWithAllResolvableArguments);

            var client = container.Resolve<SomeClient>();
            Assert.That(client.Seed, Is.EqualTo(1));
        }

        [Test]
        public void If_single_constructor_available_Then_constructor_selection_is_not_applied()
        {
            var container = new Container();

            container.Register<AnotherClient>(withConstructor: ReflectionFactory.SelectConstructorWithAllResolvableArguments);

            var ex = Assert.Throws<ContainerException>(() => 
                container.Resolve<AnotherClient>());

            Assert.That(ex.Message, Is.StringStarting("Unable to resolve "));
        }

        [Test]
        public void If_multiple_constructor_available_and_nothing_selected_Then_it_should_throw_specific_error()
        {
            var container = new Container();

            container.Register<YetAnotherClient>(withConstructor: ReflectionFactory.SelectConstructorWithAllResolvableArguments);

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<YetAnotherClient>());

            Assert.That(ex.Message, Is.StringStarting("Unable to select constructor with all resolvable arguments "));
        }

        [Test]
        public void Could_specify_constructor_selection_as_default_strategy_for_all_registrations()
        {
            ReflectionFactory.DefaultConstructorSelector = ReflectionFactory.SelectConstructorWithAllResolvableArguments;
            try
            {
                var container = new Container();

                container.Register<SomeClient>();

                var client = container.Resolve<SomeClient>();
                Assert.That(client.Seed, Is.EqualTo(1));
            }
            finally
            {
                ReflectionFactory.DefaultConstructorSelector = null;
            }
        }

        [Test]
        public void What_if_no_public_constructors()
        {
            var container = new Container();

            container.Register<InternalClient>(withConstructor: ReflectionFactory.SelectConstructorWithAllResolvableArguments);

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<InternalClient>());

            Assert.That(ex.Message, Is.StringStarting("Unable to get constructor of "));
        }

        public interface IDependency { }

        public class SomeDependency : IDependency { }

        public class SomeClient
        {
            public readonly int Seed = 1;
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

        public class AnotherClient
        {
            public readonly IDependency Dependency;

            public AnotherClient(IDependency dependency)
            {
                Dependency = dependency;
            }
        }

        public class YetAnotherClient
        {
            public readonly IDependency Dependency;
            public readonly int Blah;

            public YetAnotherClient(IDependency dependency)
            {
                Dependency = dependency;
            }

            public YetAnotherClient(int blah)
            {
                Blah = blah;
            }
        }

        public class InternalClient
        {
            internal InternalClient() { }
        }
    }
}
