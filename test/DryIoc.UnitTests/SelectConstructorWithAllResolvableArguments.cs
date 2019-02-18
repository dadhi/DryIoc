using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class SelectConstructorWithAllResolvableArgumentTests
    {
        [Test]
        public void Container_is_providing_method_for_that_Test_default_constructor()
        {
            var container = new Container();

            container.Register<SomeClient>(made: FactoryMethod.ConstructorWithResolvableArguments);

            var client = container.Resolve<SomeClient>();
            Assert.That(client.Seed, Is.EqualTo(1));
        }

        [Test]
        public void If_single_constructor_available_Then_constructor_selection_is_not_applied()
        {
            var container = new Container();

            container.Register<AnotherClient>(made: FactoryMethod.ConstructorWithResolvableArguments);

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<AnotherClient>());

            StringAssert.StartsWith("Unable to resolve ", ex.Message);
        }

        [Test]
        public void If_multiple_constructor_available_and_nothing_selected_Then_it_should_throw_specific_error()
        {
            var container = new Container();

            container.Register<YetAnotherClient>(made: FactoryMethod.ConstructorWithResolvableArguments);

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<YetAnotherClient>());

            Assert.AreEqual(
                Error.NameOf(Error.UnableToFindCtorWithAllResolvableArgs),
                ex.ErrorName);
        }

        [Test]
        public void What_if_no_public_constructors()
        {
            var container = new Container();

            container.Register<InternalClient>(made: FactoryMethod.ConstructorWithResolvableArguments);

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<InternalClient>());

            StringAssert.StartsWith("Unable to get constructor of ", ex.Message);
        }

        [Test]
        public void For_func_with_arguments_Constructor_containing_all_func_args_should_be_selected()
        {
            var container = new Container();
            container.Register<SomeClient>(made: FactoryMethod.ConstructorWithResolvableArguments);

            var func = container.Resolve<Func<int, SomeService, SomeClient>>();

            Assert.AreEqual(3, func(3, null).Seed);
        }

        [Test]
        public void For_func_with_arguments_Constructor_with_more_resolvable_arguments_should_be_preferred_over_less_or_no_args()
        {
            var container = new Container();
            container.Register<SomeClient>(made: FactoryMethod.ConstructorWithResolvableArguments);
            container.Register<IDependency, SomeDependency>();

            var func = container.Resolve<Func<int, SomeClient>>();

            Assert.IsInstanceOf<SomeDependency>(func(4).Dependency);
        }

        [Test]
        public void For_func_with_arguments_When_it_should_the_first_matching_constructor()
        {
            var container = new Container();
            container.Register<SomeClient>(made: FactoryMethod.ConstructorWithResolvableArguments);

            var x = container.Resolve<Func<string, SomeClient>>();
            Assert.AreEqual(1, x("").Seed);
        }

        [Test]
        public void Could_specify_constructor_selection_as_default_strategy_for_all_Container_registrations()
        {
            var container = new Container(rules => 
                rules.With(FactoryMethod.ConstructorWithResolvableArguments));

            container.Register<SomeClient>();

            var client = container.Resolve<SomeClient>();
            Assert.That(client.Seed, Is.EqualTo(1));
        }

        [Test]
        public void When_using_all_arg_ctor_selector_implicitly_injected_services_should_be_honored()
        {
            var container = new Container(rules =>
                rules.With(FactoryMethod.ConstructorWithResolvableArguments));

            container.Register<Blah>();

            var blah = container.Resolve<Blah>();
            Assert.IsNotNull(blah.R);
        }

        [Test]
        public void When_using_all_arg_ctor_selector_implicit_custom_value_dependency_should_be_honored()
        {
            var container = new Container(rules =>
                rules.With(FactoryMethod.ConstructorWithResolvableArguments));

            container.Register<Blah>(made: Parameters.Of.Type<string>(_ => "a"));

            var blah = container.Resolve<Blah>();
            Assert.AreEqual("a", blah.S);
        }

        [Test]
        public void Can_specify_to_use_default_ctor()
        {
            var container = new Container();

            container.Register<Ab>(made: FactoryMethod.DefaultConstructor());

            var ab = container.Resolve<Ab>();
            Assert.IsNotNull(ab);
        }

        [Test]
        public void Can_specify_to_use_default_ctor_and_throw_correct_error_if_no_default_ctor()
        {
            var container = new Container();

            container.Register<Ac>(made: FactoryMethod.DefaultConstructor());

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<Ac>());

            Assert.AreEqual(
                Error.NameOf(Error.UnableToSelectCtor), 
                Error.NameOf(ex.Error));
        }

        [Test]
        public void Can_specify_to_use_default_ctor_and_throw_correct_error_if_no_impl_type()
        {
            var container = new Container();

            container.Register<BaseA>(made: FactoryMethod.DefaultConstructor());

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<BaseA>());

            Assert.AreEqual(
                Error.NameOf(Error.ImplTypeIsNotSpecifiedForAutoCtorSelection),
                Error.NameOf(ex.Error));
        }

        [Test]
        public void For_consistency_I_can_specify_FactoryMethod_to_be_a_constructor()
        {
            var container = new Container();

            container.Register<Ac>(made: Made.Of(FactoryMethod.Constructor()));

            var ac = container.WithDependencies(Parameters.Of.Type(_ => "x")).Resolve<Ac>();
            Assert.IsNotNull(ac);
        }

        [Test]
        public void Constructor_FactoryMethod_will_throw_for_not_defined_impl_type()
        {
            var container = new Container();

            container.Register<BaseA>(made: FactoryMethod.Constructor());

            var ex = Assert.Throws<ContainerException>(() => 
                container.Resolve<BaseA>());

            Assert.AreEqual(
                Error.NameOf(Error.ImplTypeIsNotSpecifiedForAutoCtorSelection),
                Error.NameOf(ex.Error));
        }

        [Test]
        public void AutoConstructoSelection_will_throw_for_not_defined_impl_type()
        {
            var container = new Container();

            container.Register<BaseA>(
                made: FactoryMethod.Constructor(mostResolvable: true, includeNonPublic: true));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<BaseA>());

            Assert.AreEqual(
                Error.NameOf(Error.ImplTypeIsNotSpecifiedForAutoCtorSelection),
                Error.NameOf(ex.Error));
        }

        #region CUT

        public interface INo { }

        public class Blah
        {
            public IResolver R { get; private set; }

            public int I { get; private set; }

            public string S { get; private set; }

            public Blah(IResolver r)
            {
                R = r;
            }

            public Blah(IResolver r, int i = 3)
            {
                R = r;
                I = i;
            }

            public Blah(INo no) { }

            public Blah(IResolver r, string s)
            {
                R = r;
                S = s;
            }
        }

        public interface IDependency { }

        public class SomeDependency : IDependency { }

        public class SomeService { }

        public class SomeClient
        {
            public readonly int Seed = 1;
            public readonly SomeService Service;
            public readonly IDependency Dependency;

            // Won't be selected because constructor with IDependency will be selected first.
            public SomeClient() { }

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
            internal InternalClient() { }
        }

        public class Ab
        {
            public Ab() {}
            public Ab(string x) {}
        }

        public class Ac
        {
            public Ac(string x) { }
        }

        public abstract class BaseA { }

        #endregion
    }
}
