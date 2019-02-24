using System;
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

        [Explicit("Modifies static state which is side-effect which may affect other tests run.")]
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
        public void Given_implementation_with_one_constructor_primitive_parameter_Possible_to_resolve_service_as_Func_of_one_parameter()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithOnePrimitiveParameter));

            var func = container.Resolve<Func<string, ServiceWithOnePrimitiveParameter>>();
            var service = func("hi");

            Assert.That(service.Message, Is.EqualTo("hi"));
        }

        [Test]
        public void Given_implementation_with_many_constructor_primitive_parameters_Possible_to_resolve_service_as_Func_of_many_parameters()
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

            Assert.AreSame(one, another);
            Assert.IsTrue(another.Flag);
            Assert.AreEqual("one", another.Message);
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
        public void Given_implementation_with_two_constructor_parameters_Possible_to_resolve_service_as_Func_of_one_parameter_with_another_provided_by_Conrainer()
        {
            var container = new Container();
            container.Register(typeof(IServiceWithParameterAndDependency), typeof(ServiceWithParameterAndDependency));
            container.Register(typeof(Service));

            var func = container.Resolve<Func<bool, IServiceWithParameterAndDependency>>();
            var service = func(true);

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
        public void Resolving_service_with_recursive_dependency_with_Func_on_road_should_throw()
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

        [Test]
        public void Can_resolve_sub_dependency_with_func_argument()
        {
            var container = new Container();
            container.Register<ClassWithDepAndSubDep>();
            container.Register<DepWithSubDep>();

            var func = container.Resolve<Func<string, ClassWithDepAndSubDep>>();
            var service = func("Hey");

            Assert.That(service.Dep.Message, Is.EqualTo("Hey"));
        }

        [Test]
        public void Can_reuse_func_argument_for_dependency_and_sub_dependency()
        {
            var container = new Container();
            container.Register<X>();
            container.Register<Y>();

            var getX = container.Resolve<Func<A, X>>();
            var a = new A();
            var x = getX(a);

            Assert.AreSame(x.A, x.Y.A);
        }

        [Test]
        public void Func_correctly_handles_IfUnresolvedReturnDefault_for_unregistered_service()
        {
            var container = new Container();

            var getX = container.Resolve<Func<X>>(IfUnresolved.ReturnDefault);

            Assert.IsNull(getX);
        }

        [Test]
        public void Func_correctly_handles_IfUnresolvedReturnDefault_for_unregistered_dependency()
        {
            var container = new Container();
            container.Register<X>();

            var getX = container.Resolve<Func<X>>(IfUnresolved.ReturnDefault);

            Assert.IsNull(getX);
        }

        [Test]
        public void Reuse_func_arguments_in_nested_dependencies()
        {
            var container = new Container();

            container.Register<AA>();
            container.Register<BB>();

            var getAA = container.Resolve<Func<ILogger, AA>>();

            var logger = new SomeLogger();
            var aa = getAA(logger);
            Assert.AreSame(logger, aa.Logger);
            Assert.AreSame(aa.Bb.Logger, aa.Logger);
        }

        [Test]
        public void Should_not_reuse_func_arguments_in_the_same_service()
        {
            var container = new Container();

            container.Register<AA>();
            container.Register<BB, BBB>(Made.Of(() => new BBB(Arg.Of<ILogger>(), Arg.Of<ILogger>(IfUnresolved.ReturnDefault))));

            var getAA = container.Resolve<Func<ILogger, AA>>();

            var logger = new SomeLogger();
            var aa = getAA(logger);
            Assert.IsNull(((BBB)aa.Bb).OtherLogger);
            Assert.AreSame(logger, aa.Logger);
            Assert.AreSame(aa.Bb.Logger, aa.Logger);
        }

        [Test]
        public void For_singleton_can_use_func_without_args_or_just_resolve_after_func_with_args()
        {
            var container = new Container();

            container.Register<BB>(Reuse.Singleton);

            var logger = new SomeLogger();
            var bb = container.Resolve<Func<ILogger, BB>>().Invoke(logger);

            var bb2 = container.Resolve<Func<BB>>().Invoke();
            Assert.AreSame(bb, bb2);

            var bb3 = container.Resolve<BB>();
            Assert.AreSame(bb, bb3);
        }

        [Test]
        public void Can_propagate_args_through_resolution_call()
        {
            var c = new Container();

            c.Register<F>();
            c.Register<L>();

            var l = c.Resolve<Func<string, L>>();

            Assert.AreEqual("hey", l("hey").F.S);
        }

        [Test]
        public void Can_both_provide_args_and_resolve_as_Func_with_args()
        {
            var c = new Container();

            c.Register<SS>();

            var f = c.Resolve<Func<string, SS>>(new object[] { "b" });
            var ss = f("a");

            Assert.AreEqual("a", ss.A);
            Assert.AreEqual("b", ss.B);
        }

        [Test]
        public void Can_supply_fresh_args_in_multiple_resolve_call_Using_the_rule_for_ignoring_Reuse()
        {
            var c = new Container(r => r.WithIgnoringReuseForFuncWithArgs());

            c.Register<SS>(Reuse.Singleton);

            var ss = c.Resolve<Func<string, SS>>(new object[] { "b" })("a");
            Assert.AreEqual("a", ss.A);
            Assert.AreEqual("b", ss.B);

            var ss2 = c.Resolve<SS>(new object[] { "x", "y" });
            Assert.AreEqual("x", ss2.A);
            Assert.AreEqual("y", ss2.B);
        }

        [Test]
        public void Can_supply_fresh_args_in_different_open_scopes()
        {
            var c = new Container();
            //var c = new Container(r => r.WithIgnoringReuseForFuncWithArgs());

            c.Register<SS>(Reuse.ScopedTo("1", "2"));

            using (var scope = c.OpenScope("1"))
            {
                var ss = scope.Resolve<Func<string, SS>>(new object[] { "b" })("a");
                Assert.AreEqual("a", ss.A);
                Assert.AreEqual("b", ss.B);
            }

            using (var scope = c.OpenScope("1"))
            {
                var ss = scope.Resolve<SS>(new object[] { "x", "y" });
                Assert.AreEqual("x", ss.A);
                Assert.AreEqual("y", ss.B);
            }

            using (var scope = c.OpenScope("2"))
            {
                var ss = scope.Resolve<SS>(new object[] { "__", "_ _" });
                Assert.AreEqual("__", ss.A);
                Assert.AreEqual("_ _", ss.B);
            }
        }

        #region CUT

        class SS
        {
            public string A { get; }
            public string B { get; }

            public SS(string a, string b)
            {
                A = a;
                B = b;
            }
        }

        class F
        {
            public string S { get; }
            public F(string s)
            {
                S = s;
            }
        }

        class L
        {
            public F F => _f.Value;

            public L(Lazy<F> f)
            {
                _f = f;
            }

            private Lazy<F> _f;
        }

        public class BB
        {
            public ILogger Logger { get; set; }
            public BB(ILogger logger)
            {
                Logger = logger;
            }
        }

        public class BBB : BB
        {
            public ILogger OtherLogger { get; set; }

            public BBB(ILogger logger, ILogger otherLogger) : base(logger)
            {
                OtherLogger = otherLogger;
            }
        }

        public class AA
        {
            public BB Bb { get; set; }
            public ILogger Logger { get; set; }
            public AA(BB bb, ILogger logger)
            {
                Bb = bb;
                Logger = logger;
            }
        }

        public class SomeLogger : ILogger { }

        internal class X
        {
            public A A { get; private set; }
            public Y Y { get; private set; }

            public X(A a, Y y)
            {
                A = a;
                Y = y;
            }
        }

        internal class A { }

        internal class Y
        {
            public A A { get; private set; }
            public Y(A a)
            {
                A = a;
            }
        }

        internal class ClassWithDepAndSubDep
        {
            public DepWithSubDep Dep { get; private set; }
            public ClassWithDepAndSubDep(DepWithSubDep dep)
            {
                Dep = dep;
            }
        }

        internal class DepWithSubDep
        {
            public string Message { get; private set; }
            public DepWithSubDep(string message)
            {
                Message = message;
            }
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

        #endregion
    }
}
