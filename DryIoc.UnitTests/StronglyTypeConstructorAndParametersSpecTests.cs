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

            container.Register2(with: Impl.Of(() => new Burger()));

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Null);
        }

        [Test]
        public void Specify_constructor_with_params_without_reflection()
        {
            var container = new Container();

            container.Register<Burger>(with: Impl.Of(() => new Burger(default(ICheese))));
            container.Register<ICheese, BlueCheese>();

            var burger = container.Resolve<Burger>();
            Assert.That(burger.Cheese, Is.Not.Null);
        }

        [Test]
        public void Specify_parameter_ifUnresolved_behavior_without_reflection()
        {
            var container = new Container();

            container.Register<Burger>(
                with: Impl.Of(() => new Burger(Arg.Of<ICheese>(IfUnresolved.ReturnDefault))));

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
                with: Impl.Of(() => new Burger("King", Arg.Of<BlueCheese>("a"))));

            var burger = container.Resolve<Burger>();
            Assert.AreEqual("King", burger.Name);
        }

        [Test]
        public void Specify_static_factory_method()
        {
            var container = new Container();

            container.Register<Burger>(with: Impl.Of(() => Burger.Create()));

            Assert.NotNull(container.Resolve<Burger>());
        }

        [Test]
        public void Specify_for_factory_method_service_key_and_required_service_type_for_parameter()
        {
            var container = new Container();

            container.Register<BlueCheese>(serviceKey: "a");
            container.RegisterInstance("King");

            container.Register2(
                with: Impl.Of(() => Burger.Create("King", Arg.Of<BlueCheese>("a"))));

            var burger = container.Resolve<Burger>();
            Assert.AreEqual("King", burger.Name);
        }

        [Test]
        public void Can_specify_properties_and_fields_together_with_constructor()
        {
            var container = new Container();
            container.Register2(with: Impl.Of(() => new Burger { Name = "", Cheese = Arg.Of<BlueCheese>() }));

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
            container.Register2<IBurger, Burger>(with: Impl.Of(() => new Burger(Arg.Of<string>("key"), default(int))));
            container.RegisterInstance("King", serviceKey: "key");

            var burger = container.Resolve<IBurger>();

            Assert.AreEqual(3, burger.Size);
        }

        [Test]
        public void Can_handle_default_parameters_with_explicit_specification()
        {
            var container = new Container();
            container.Register2<IBurger, Burger>(with: Impl.Of(() => new Burger(Arg.Of<string>("key"), Arg.Of<int>(IfUnresolved.ReturnDefault))));
            container.RegisterInstance("King", serviceKey: "key");

            var burger = container.Resolve<IBurger>();

            Assert.AreEqual(3, burger.Size);
        }

        internal interface ICheese { }

        internal class BlueCheese : ICheese { }

        internal interface IBurger 
        {
            string Name { get; }
            int Size { get; }
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
    }
}
