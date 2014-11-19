using System;
using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class FuncTests
    {
        [Test]
        public void Resolving_as_Func_should_produce_FuncOfService()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var func = container.Resolve(typeof(Func<IService>));

            Assert.That(func, Is.InstanceOf<Func<IService>>());
        }

        [Test]
        public void Resolving_as_Func_should_throw_for_not_registered_service()
        {
            var container = new Container();

            Assert.Throws<ContainerException>(
                () => container.Resolve<Func<IService>>());
        }

        [Test]
        public void Func_itself_is_transient()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var first = container.Resolve<Func<IService>>();
            var second = container.Resolve<Func<IService>>();

            Assert.That(first, Is.Not.SameAs(second));
        }

        [Test]
        public void Given_registered_transient_Resolved_Func_should_create_new_instances()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var func = container.Resolve<Func<IService>>();

            var one = func();
            var another = func();
            Assert.That(one, Is.Not.Null);
            Assert.That(one, Is.Not.SameAs(another));
        }

        [Test]
        public void Given_registered_singleton_Resolved_Func_should_create_same_instances()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service), Reuse.Singleton);

            var func = container.Resolve<Func<IService>>();

            var one = func();
            var another = func();
            Assert.That(one, Is.Not.Null);
            Assert.That(one, Is.SameAs(another));
        }

        [Test]
        public void Given_registered_singleton_Resolving_service_dependency_as_is_and_as_Func_should_provide_same_instances()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service), Reuse.Singleton);
            container.Register(typeof(ClientWithServiceAndFuncOfServiceDependencies));

            var client = container.Resolve<ClientWithServiceAndFuncOfServiceDependencies>();

            Assert.That(client.Service, Is.SameAs(client.Factory()));
        }

        [Test]
        public void Given_registered_singleton_Resolving_as_Func_should_NOT_create_service_instance_until_Func_is_invoked()
        {
            var container = new Container();
            container.Register<ServiceWithInstanceCount>(Reuse.Singleton);
            ServiceWithInstanceCount.InstanceCount = 0;

            var func = container.Resolve<Func<ServiceWithInstanceCount>>();
            Assert.That(ServiceWithInstanceCount.InstanceCount, Is.EqualTo(0));

            func.Invoke();
            Assert.That(ServiceWithInstanceCount.InstanceCount, Is.EqualTo(1));
        }

        [Test]
        public void Given_implementation_with_one_constuctor_primitive_parameter_Possible_to_resolve_service_as_Func_of_one_parameter()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithOnePrimitiveParameter));

            var func = container.Resolve<Func<string, ServiceWithOnePrimitiveParameter>>();
            var service = func("hi");

            Assert.That(service.Message, Is.EqualTo("hi"));
        }

        [Test]
        public void Given_implementation_with_many_constuctor_primitive_parameters_Possible_to_resolve_service_as_Func_of_many_parameters()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithTwoPrimitiveParameters));

            var func = container.Resolve<Func<bool, string, ServiceWithTwoPrimitiveParameters>>();
            var service = func(true, "hey");

            Assert.That(service.Flag, Is.True);
            Assert.That(service.Message, Is.EqualTo("hey"));
        }

        [Test]
        public void Given_registered_singleton_Resolved_Func_of_one_parameter_should_create_same_instances_Where_created_first_will_determine_parameter_value()
        {
            var container = new Container();
            container.Register<ServiceWithOnePrimitiveParameter>(Reuse.Singleton);

            var func = container.Resolve<Func<string, ServiceWithOnePrimitiveParameter>>();
            var one = func.Invoke("one");
            var another = func.Invoke("another");

            Assert.That(one, Is.SameAs(another));
            Assert.That(another.Message, Is.EqualTo("one"));
        }

        [Test]
        public void Given_registered_singleton_Resolved_Func_of_many_parameters_should_create_same_instances_Where_created_first_will_determine_parameters_value()
        {
            var container = new Container();
            container.Register<ServiceWithTwoPrimitiveParameters>(Reuse.Singleton);

            var func = container.Resolve<Func<bool, string, ServiceWithTwoPrimitiveParameters>>();
            var one = func(true, "one");
            var another = func(false, "another");

            Assert.That(one, Is.SameAs(another));
            Assert.That(another.Flag, Is.True);
            Assert.That(another.Message, Is.EqualTo("one"));
        }

        [Test]
        public void Given_registered_service_Resolving_service_dependency_Func_with_parameter_and_as_is_should_provide_same_instances()
        {
            var container = new Container();
            container.Register(typeof(ClientWithFuncAndInstanceDependency));
            container.RegisterInstance("I am a string");
            container.Register(typeof(IService), typeof(ServiceWithOnePrimitiveParameter), Reuse.Singleton);

            var client = container.Resolve<ClientWithFuncAndInstanceDependency>();

            Assert.That(client.Factory("blah"), Is.SameAs(client.Instance));
        }

        [Test]
        public void Given_implementation_with_two_constuctor_parameters_Possible_to_resolve_service_as_Func_of_one_parameter_with_another_provided_by_Conrainer()
        {
            var container = new Container();
            container.Register(typeof(IServiceWithParameterAndDependency), typeof(ServiceWithParameterAndDependency));
            container.Register(typeof(Service));

            var func = container.Resolve<Func<bool, IServiceWithParameterAndDependency>>();
            var service = func(true);

            Assert.That(service.Flag, Is.True);
            Assert.That(service.Dependency, Is.Not.Null);
        }

        [Test][Ignore]
        public void Can_resolve_Func_with_args_of_Wrapper()
        {
            var container = new Container();
            container.Register<IServiceWithParameterAndDependency, ServiceWithParameterAndDependency>();
            container.Register(typeof(Service));

            var func = container.Resolve<Func<bool, Lazy<IServiceWithParameterAndDependency>>>();
            var service = func(true).Value;

            Assert.That(service.Flag, Is.True);
            Assert.That(service.Dependency, Is.Not.Null);
        }

        [Test][Ignore]
        public void Can_resolve_Func_with_args_of_LazyEnumerable()
        {
            var container = new Container();
            container.Register<IServiceWithParameterAndDependency, ServiceWithParameterAndDependency>();
            container.Register(typeof(Service));

            var func = container.Resolve<Func<bool, LazyEnumerable<IServiceWithParameterAndDependency>>>();
            var service = func(true).First();

            Assert.That(service.Flag, Is.True);
            Assert.That(service.Dependency, Is.Not.Null);
        }

        [Test]
        public void Constructor_will_use_func_argument_only_once_for_first_ctor_parameter()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithTwoDepenedenciesOfTheSameType));
            container.Register(typeof(Service));

            var func = container.Resolve<Func<Service, ServiceWithTwoDepenedenciesOfTheSameType>>();
            var freeParameter = new Service();
            var service = func(freeParameter);

            Assert.That(service.Another, Is.Not.SameAs(freeParameter));
        }

        [Test]
        public void Given_constructor_with_two_parameters_Possible_to_resolve_Func_with_swapped_parameter_positions()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithParameterAndDependency));
            container.Register(typeof(Service));

            var func = container.Resolve<Func<bool, Service, ServiceWithParameterAndDependency>>();
            var parameter = new Service();
            var service = func(true, parameter);

            Assert.That(service.Dependency, Is.SameAs(parameter));
            Assert.That(service.Flag, Is.True);
        }

        [Test]
        public void Given_constructor_with_two_parameters_Resolving_Func_twice_with_one_BUT_different_parameter_should_work()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithTwoParameters));
            container.Register(typeof(Service));
            container.Register(typeof(AnotherService), typeof(AnotherService));

            var firstFunc = container.Resolve<Func<Service, ServiceWithTwoParameters>>();
            var one = new Service();
            var first = firstFunc(one);

            Assert.That(first.One, Is.SameAs(one));

            var secondFunc = container.Resolve<Func<AnotherService, ServiceWithTwoParameters>>();
            var another = new AnotherService();
            var second = secondFunc(another);

            Assert.That(second.Another, Is.SameAs(another));
        }

        [Test]
        public void Possible_to_resolve_Func_or_Func()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var funcOfFunc = container.Resolve<Func<Func<IService>>>();

            Assert.That(funcOfFunc, Is.InstanceOf<Func<Func<IService>>>());
            Assert.That(funcOfFunc()(), Is.InstanceOf<Service>());
        }

        [Test]
        public void Possible_to_resolve_Func_of_Lazy()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var funcOfLazy = container.Resolve<Func<Lazy<IService>>>();

            Assert.That(funcOfLazy, Is.InstanceOf<Func<Lazy<IService>>>());
            Assert.That(funcOfLazy().Value, Is.InstanceOf<Service>());
        }

        [Test]
        public void Resolving_service_with_recursive_Func_dependency_should_throw()
        {
            var container = new Container();

            container.Register(typeof(IDependency), typeof(FooWithDependency));
            container.Register(typeof(IService), typeof(ServiceWithFuncOfRecursiveDependency));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IService>());
        }

        [Test]
        public void Resolving_service_with_resursive_dependency_with_Func_on_road_should_throw()
        {
            var container = new Container();

            container.Register(typeof(IDependency), typeof(FooWithFuncOfDependency));
            container.Register(typeof(IService), typeof(ServiceWithRecursiveDependency));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IService>());
        }

        [Test]
        public void Possible_to_register_and_resolve_delegate_as_instance()
        {
            var container = new Container();
            Func<int, string> toString = i => i.ToString();
            container.RegisterInstance(toString);

            var func = container.Resolve<Func<int, string>>();

            Assert.That(func, Is.SameAs(toString));
        }

        [Test]
        public void Possible_to_register_Instance_of_Func_of_argument_and_Resolve_it_as_Func()
        {
            var container = new Container();

            Func<string, TwoCtors> getTwo = s => new TwoCtors(s);
            container.RegisterInstance(getTwo);

            var getTwoResolved = container.Resolve<Func<string, TwoCtors>>();

            Assert.That(getTwoResolved("cool").Message, Is.EqualTo("cool"));
        }

        [Test]
        public void Resolving_func_with_some_unused_args_should_not_throw()
        {
            var container = new Container();
            container.Register<ServiceWithDependency>();
            container.Register<IDependency, Dependency>();

            var func = container.Resolve<Func<string, ServiceWithDependency>>();

            Assert.That(func(null), Is.InstanceOf<ServiceWithDependency>());
        }

        [Test]
        public void Resolving_Func_of_delegate_factory_should_not_throw_The_func_argument_will_not_be_used()
        {
            var container = new Container();
            container.RegisterDelegate(r => new Service());

            var func = container.Resolve<Func<int, Service>>();

            Assert.That(func(0), Is.InstanceOf<Service>());
        }

        [Test]
        public void One_func_argument_should_be_used_only_once_for_one_parameter()
        {
            var container = new Container();
            container.Register<FullNameService>();

            var func = container.Resolve<Func<string, string, FullNameService>>();
            var service = func("Oh", "Yeah");

            Assert.That(service.FullName, Is.EqualTo("Oh Yeah"));
        }

        public class TwoCtors
        {
            public string Message { get; set; }

            public TwoCtors()
                : this("Hey!")
            {
            }

            public TwoCtors(string message)
            {
                Message = message;
            }
        }

        public class FullNameService
        {
            public string FullName { get { return _name + " " + _surname; } }

            public FullNameService(string name, string surname)
            {
                _name = name;
                _surname = surname;
            }

            private readonly string _name;
            private readonly string _surname;
        }
    }
}
