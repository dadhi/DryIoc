using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class SelectConstructorWithAllResolvableArguments
    {
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

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<YetAnotherClient>());

            Assert.That(ex.Message, Is.StringStarting("Unable to find constructor with all resolvable parameters"));
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

        [Test]
        public void For_func_with_arguments_Constructor_containing_all_func_args_should_be_selected()
        {
            var container = new Container();
            container.Register<SomeClient>(withConstructor: ReflectionFactory.SelectConstructorWithAllResolvableArguments);

            var func = container.Resolve<Func<int, SomeService, SomeClient>>();

            Assert.That(func(3, null).Seed, Is.EqualTo(3));
        }

        [Test]
        public void For_func_with_arguments_Constructor_with_more_resolvable_arguments_should_be_preferred_over_less_or_no_args()
        {
            var container = new Container();
            container.Register<SomeClient>(withConstructor: ReflectionFactory.SelectConstructorWithAllResolvableArguments);
            container.Register<IDependency, SomeDependency>();

            var func = container.Resolve<Func<int, SomeClient>>();

            Assert.That(func(4).Dependency, Is.InstanceOf<SomeDependency>());
        }

        [Test]
        public void For_func_with_arguments_When_no_matching_constructor_found_Then_it_should_throw()
        {
            var container = new Container();
            container.Register<SomeClient>(withConstructor: ReflectionFactory.SelectConstructorWithAllResolvableArguments);

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<Func<string, SomeClient>>());

            Assert.That(ex.Message, Is.StringStarting("Unable to find constructor with all parameters matching Func signature"));
        }

        #region CUT

        public interface IDependency {}

        public class SomeDependency : IDependency {}

        public class SomeService {}

        public class SomeClient
        {
            public readonly int Seed = 1;
            public readonly SomeService Service;
            public readonly IDependency Dependency;

            // Won't be selected because constructor with IDependency will be selected first.
            public SomeClient() {}

            // Won't be selected because nor Int32 nor IService is registered in Container.
            public SomeClient(int seed, SomeService service)
            {
                Service = service;
                Seed = seed;
            }

            // Will be selected because IDependency is registered in Container.
            public SomeClient(IDependency dependency)
            {
                Dependency = dependency;
            }

            public SomeClient(int seed)
            {
                Seed = seed;
            }

            public SomeClient(IDependency dependency, int seed)
            {
                Dependency = dependency;
                Seed = seed;
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
            internal InternalClient() {}
        }

        #endregion
    }
}
