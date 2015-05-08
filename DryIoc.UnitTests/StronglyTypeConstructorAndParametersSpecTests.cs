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

            container.Register<Burger>(made: Made.Of(() => new Burger()));

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Null);
        }

        [Test]
        public void Specifying_constructor_of_wrong_type_should_throw()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() => 
            container.Register<NewBurger>(made: Made.Of(() => new Burger())));

            Assert.AreEqual(ex.Error, Error.MadeOfTypeNotAssignableToImplementationType);
        }

        [Test]
        public void Specify_constructor_with_params_without_reflection()
        {
            var container = new Container();

            container.Register<Burger>(made: Made.Of(() => new Burger(default(ICheese))));
            container.Register<ICheese, BlueCheese>();

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Not.Null);
        }                       

        [Test]
        public void Specify_parameter_ifUnresolved_behavior_without_reflection()
        {
            var container = new Container();

            container.Register<Burger>(
                made: Made.Of(() => new Burger(Arg.Of<ICheese>(IfUnresolved.ReturnDefault))));

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Null);
        }

        [Test]
        public void Specify_service_key_and_required_service_type_for_parameter()
        {
            var container = new Container();

            container.Register<BlueCheese>(serviceKey: "a");
            container.RegisterInstance("King");

            container.Register<Burger>(
                made: Made.Of(() => new Burger("King", Arg.Of<BlueCheese>("a"))));

            var burger = container.Resolve<Burger>();
            Assert.AreEqual("King", burger.Name);
        }

        [Test]
        public void Specify_static_factory_method()
        {
            var container = new Container();

            container.Register<Burger>(made: Made.Of(() => Burger.Create()));

            Assert.NotNull(container.Resolve<Burger>());
        }

        [Test]
        public void Specify_for_factory_method_service_key_and_required_service_type_for_parameter()
        {
            var container = new Container();

            container.Register<BlueCheese>(serviceKey: "a");
            container.RegisterInstance("King");

            container.Register<Burger>(
                made: Made.Of(() => Burger.Create("King", Arg.Of<BlueCheese>("a"))));

            var burger = container.Resolve<Burger>();
            Assert.AreEqual("King", burger.Name);
        }

        [Test]
        public void Can_specify_properties_and_fields_together_with_constructor()
        {
            var container = new Container();
            container.Register<Burger>(
                made: Made.Of(() => new Burger { Name = "", Cheese = Arg.Of<BlueCheese>() }));

            container.RegisterInstance("King");
            container.Register<BlueCheese>();

            var burger = container.Resolve<Burger>();

            Assert.AreEqual("King", burger.Name);
            Assert.IsInstanceOf<BlueCheese>(burger.Cheese);
        }

        [Test]
        public void Can_handle_default_parameters()
        {
            var container = new Container();
            container.Register<IBurger, Burger>(made: Made.Of(() => new Burger(Arg.Of<string>("key"), default(int))));
            container.RegisterInstance("King", serviceKey: "key");

            var burger = container.Resolve<IBurger>();

            Assert.AreEqual(3, burger.Size);
        }

        [Test]
        public void Can_handle_default_parameters_with_explicit_specification()
        {
            var container = new Container();
            container.Register<IBurger, Burger>(made: Made.Of(() => new Burger(Arg.Of<string>("key"), Arg.Of<int>(IfUnresolved.ReturnDefault))));
            container.RegisterInstance("King", serviceKey: "key");

            var burger = container.Resolve<IBurger>();

            Assert.AreEqual(3, burger.Size);
        }

        [Test]
        public void Can_use_enum_service_key()
        {
            var container = new Container();
            container.Register<IBurger, Burger>(made: Made.Of(() => new Burger(Arg.Of<ICheese>(BurgerCheese.Blue))));
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
            container.Register<IFoo>(made: Made.Of(r => ServiceInfo.Of<FooFactory>(), factory => factory.Foo));

            var foo = container.Resolve<IFoo>();

            Assert.IsNotNull(foo);
        }

        [Test]
        public void Can_specify_to_use_field()
        {
            var container = new Container();
            container.Register<FooFactory>(Reuse.Singleton);
            container.Register<Blah>(
                made: Made.Of(
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
            public readonly IFoo Foo;
            public Blah Blah { get; private set; }

            public FooFactory()
            {
                Foo = new Foo();
                Blah = new Blah();
            }
        }
    }
}
