using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class StronglyTypeConstructorAndParametersSpecTests
    {
        [Test]
        public void Specify_default_constructor_without_reflection()
        {
            var container = new Container();

            container.Register(Made.Of(() => new Burger()));

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Null);
        }

        [Test]
        public void Specifying_constructor_of_wrong_type_should_throw()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() => 
            container.Register<NewBurger>(made: Made.Of(() => new Burger())));

            Assert.AreEqual(ex.Error, Error.RegisteredFactoryMethodResultTypesIsNotAssignableToImplementationType);
        }

        [Test]
        public void Specify_constructor_with_params_without_reflection()
        {
            var container = new Container();

            container.Register(Made.Of(() => new Burger(Arg.Of<ICheese>())));
            container.Register<ICheese, BlueCheese>();

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Not.Null);
        }                       

        [Test]
        public void Specify_parameter_ifUnresolved_behavior_without_reflection()
        {
            var container = new Container();

            container.Register(Made.Of(() => new Burger(Arg.Of<ICheese>(IfUnresolved.ReturnDefault))));

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Null);
        }

        [Test]
        public void Specify_service_key_and_required_service_type_for_parameter()
        {
            var container = new Container();

            container.Register<BlueCheese>(serviceKey: "a");
            container.RegisterInstance("King");

            container.Register(Made.Of(() => new Burger("King", Arg.Of<BlueCheese>("a"))));

            var burger = container.Resolve<Burger>();
            Assert.AreEqual("King", burger.Name);
        }

        [Test]
        public void Specify_static_factory_method()
        {
            var container = new Container();

            container.Register(Made.Of(() => Burger.Create()));

            Assert.IsNotNull(container.Resolve<Burger>());
        }

        [Test]
        public void Specify_for_factory_method_service_key_and_required_service_type_for_parameter()
        {
            var container = new Container();

            container.Register<BlueCheese>(serviceKey: "a");
            container.RegisterInstance("King");

            container.Register(Made.Of(() => Burger.Create("King", Arg.Of<BlueCheese>("a"))));

            var burger = container.Resolve<Burger>();
            Assert.AreEqual("King", burger.Name);
        }

        [Test]
        public void Can_specify_properties_and_fields_together_with_constructor()
        {
            var container = new Container();
            container.Register(Made.Of(() => 
                new Burger { Name = "x", Cheese = Arg.Of<BlueCheese>() }));

            container.Register<BlueCheese>();

            var burger = container.Resolve<Burger>();

            Assert.AreEqual("x", burger.Name);
            Assert.IsInstanceOf<BlueCheese>(burger.Cheese);
        }

        [Test]
        public void Can_specify_custom_value_via_variable_for_a_property()
        {
            var container = new Container();

            var x = "King";
            container.Register(Made.Of(() => new Burger { Name = x }));

            var burger = container.Resolve<Burger>();

            Assert.AreEqual("King", burger.Name);
        }

        [Test]
        public void Can_handle_default_parameters()
        {
            var container = new Container();
            container.Register<IBurger, Burger>(Made.Of(() => new Burger(Arg.Of<string>("key"), Arg.Of<int>())));
            container.RegisterInstance("King", serviceKey: "key");

            var burger = container.Resolve<IBurger>();
            Assert.IsNotNull(burger);
        }

        [Test]
        public void Can_handle_default_parameters_with_explicit_specification()
        {
            var container = new Container();
            container.Register<IBurger, Burger>(Made.Of(() => new Burger(Arg.Of<string>("key"), Arg.Of<int>(IfUnresolved.ReturnDefault))));
            container.RegisterInstance("King", serviceKey: "key");

            var burger = container.Resolve<IBurger>();

            Assert.AreEqual(3, burger.Size);
        }

        [Test]
        public void Can_use_enum_service_key()
        {
            var container = new Container();
            container.Register<IBurger, Burger>(Made.Of(() => new Burger(Arg.Of<ICheese>(BurgerCheese.Blue))));
            container.Register<ICheese, BlueCheese>(serviceKey: BurgerCheese.Blue);

            var burger = container.Resolve<IBurger>();

            Assert.IsInstanceOf<BlueCheese>(burger.Cheese);
        }

        enum BurgerCheese {  Blue } 

        [Test]
        public void Arg_method_do_nothing_and_just_return_default_arg_value()
        {
            Assert.AreEqual(default(ICheese), Arg.Of<ICheese>());
            Assert.AreEqual(default(ICheese), Arg.Of<ICheese>(IfUnresolved.Throw));
            Assert.AreEqual(default(ICheese), Arg.Of<ICheese>("key"));
            Assert.AreEqual(default(ICheese), Arg.Of<ICheese>(IfUnresolved.Throw, "key"));
        }

        [Test]
        public void Can_specify_default_parameter()
        {
            var container = new Container();
            container.Register(Made.Of(() => new D(Arg.Of("d", IfUnresolved.ReturnDefault))));

            var d = container.Resolve<D>();

            Assert.AreEqual("d", d.S);
        }

        [Test]
        public void Can_specify_default_parameter_and_service_key()
        {
            var container = new Container();
            container.Register(Made.Of(() => new D(Arg.Of("d", IfUnresolved.ReturnDefault, "someKey"))));

            var d = container.Resolve<D>();

            Assert.AreEqual("d", d.S);
        }

        [Test]
        public void If_default_parameter_is_specified_Then_IfUnresolved_should_be_set_to_ReturnDefault()
        {
            var container = new Container();
            container.Register(Made.Of(() => new D(Arg.Of("d", 
                IfUnresolved.Throw))));
            
            var d = container.Resolve<D>();

            Assert.AreEqual("d", d.S);
        }

        [Test]
        public void I_should_be_able_to_specify_custom_constant_value_instead_of_Arg()
        {
            var container = new Container();
            container.Register(Made.Of(() => new D("d")));

            var d = container.Resolve<D>();

            Assert.AreEqual("d", d.S);
        }

        public class D
        {
            public string S { get; private set; }

            public D(string s)
            {
                S = s;
            }
        }

        internal interface ICheese { }

        internal class BlueCheese : ICheese { }

        internal interface IBurger 
        {
            string Name { get; }
            int Size { get; }
            ICheese Cheese { get; }
        }

        internal class Burger : IBurger
        {
            public string Name { get; set; }

            public int Size { get; set; }
            public const int DEFAULT_SIZE = 3;

            public ICheese Cheese { get; set; }

            public Burger() { }

            public Burger(ICheese cheese)
            {
                Cheese = cheese;
            }

            public Burger(string name, ICheese cheese)
            {
                Name = name;
                Cheese = cheese;
            }

            public Burger(string name, int size = DEFAULT_SIZE)
            {
                Name = name;
                Size = size;
            }

            public static Burger Create()
            {
                return new Burger();
            }

            public static Burger Create(string name, ICheese cheese)
            {
                return new Burger(name, cheese);
            }

            public int Number { get; private set; }
            public static Burger CreateMany(ICheese cheese, int number = 1)
            {
                return new Burger("default", cheese) { Number = number };
            }
        }

        internal class NewBurger : Burger {}

        [Test]
        public void Can_specify_to_use_property()
        {
            var container = new Container();
            container.Register<FooFactory>(Reuse.Singleton);
            container.Register(Made.Of(r => ServiceInfo.Of<FooFactory>(), factory => factory.FooX));

            var foo = container.Resolve<IFoo>();

            Assert.IsNotNull(foo);
        }

        [Test]
        public void Can_specify_to_use_field()
        {
            var container = new Container();
            container.Register<FooFactory>(Reuse.Singleton);
            container.Register(Made.Of(
                    r => ServiceInfo.Of<FooFactory>(IfUnresolved.ReturnDefault), 
                    factory => factory.Blah));

            var blah = container.Resolve<Blah>(IfUnresolved.ReturnDefault);

            Assert.IsNotNull(blah);
        }

        public interface IFoo {}
        internal class Foo : IFoo {}
        internal class Blah {}

        internal class FooFactory
        {
            public readonly IFoo FooX;
            public Blah Blah { get; private set; }

            public FooFactory()
            {
                FooX = new Foo();
                Blah = new Blah();
            }
        }

        [Test]
        public void Can_specify_property_custom_value_with_factory_spec()
        {
            var container = new Container();
            container.Register<IA, A>(Made.Of(() => new A { ImplType = Arg.Index<Type>(0) }, r => r.ServiceType));

            var a = container.Resolve<IA>();

            Assert.AreEqual(typeof(IA), a.ImplType);
        }

        [Test]
        public void Will_throw_for_custom_value_with_factory_spec_But_without_value_provider()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() => 
                container.Register<IA, A>(Made.Of(() => new A { ImplType = Arg.Index<Type>(0) })));

            Assert.AreEqual(Error.ArgValueIndexIsProvidedButNoArgValues, ex.Error);
        }

        [Test]
        public void Can_use_cast_to_specific_return_type_in_factory_expression()
        {
            var container = new Container();

            container.Register(Made.Of(() => (IA)GetAObject()));

            Assert.IsInstanceOf<A>(container.Resolve<IA>());
        }

        [Test]
        public void Can_specify_requirement_service_type_for_the_wrapper()
        {
            var container = new Container();
            container.Register<A>();
            container.Register<MyWrapper>(setup: Setup.Wrapper);

            container.Register(Made.Of(() => new UseMyWrapper(Arg.Of<MyWrapper, A>())));

            container.Resolve<UseMyWrapper>();
        }

        [Test]
        public void Should_throw_with_enough_info_to_find_culprit_MadeOf()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Register(Made.Of(() => new UseMyWrapper(new MyWrapper(new object())))));

            Assert.AreEqual(
                Error.NameOf(Error.UnexpectedExpressionInsteadOfConstantInMadeOf),
                Error.NameOf(ex.Error));
        }

        public class MyWrapper {
            public readonly object Service;
            public MyWrapper(object service)
            {
                Service = service;
            }
        }

        public class UseMyWrapper {
            public readonly MyWrapper Wr;
            public UseMyWrapper(MyWrapper wr)
            {
                Wr = wr;
            }
        }

        public static object GetAObject()
        {
            return new A();
        }

        public interface IA 
        {
            Type ImplType { get; }
        }

        public class A : IA
        {
            public Type ImplType { get; set; }
        }
    }
}
