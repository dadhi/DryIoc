using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class NewTests
    {
        [Test]
        public void Should_be_able_to_create_type_without_registering_in_container()
        {
            var container = new Container();
            container.Register<Wheels>();
            var car = container.New<Car>();

            Assert.That(car.Wheels, Is.Not.Null);
            Assert.That(container.IsRegistered<Car>(), Is.False);
        }

        [Test]
        public void Should_be_able_to_create_type_with_factory_method()
        {
            var container = new Container();
            container.Register<Wheels>();
            var car = container.New<Car>(Made.Of(() => CarFactory.Create(default(Wheels))));

            Assert.That(car.Wheels, Is.Not.Null);
            Assert.That(container.IsRegistered<Car>(), Is.False);
        }

        [Test]
        public void Should_be_able_to_decorate_created_type()
        {
            var container = new Container();
            container.Register<Wheels>();
            container.Register(Made.Of(() => CarFactory.DecorateWheels(default(Wheels))), setup: Setup.Decorator);

            var car = container.New<Car>();

            Assert.That(car.Wheels.Paint, Is.EqualTo("decorated"));
        }

        [Test]
        public void Should_be_able_to_create_Func_wrapper_of_type()
        {
            var container = new Container();
            container.Register<Wheels>();

            var getCar = container.New<Func<Car>>();
            Assert.IsNotNull(getCar());
        }

        [Test]
        public void Should_be_able_to_create_Func_with_argument_of_type()
        {
            var container = new Container();

            var getCar = container.New<Func<Wheels, Car>>();
            Assert.IsNotNull(getCar(new Wheels()));
        }

        [Test]
        public void Should_be_able_to_create_Lazy_with_argument_of_type()
        {
            var container = new Container();
            container.Register<Wheels>();

            var car = container.New<Lazy<Car>>();
            Assert.IsNotNull(car.Value);
        }

        [Test]
        public void Can_new_instance_of_runtime_known_type()
        {
            var container = new Container();
            container.Register<Wheels>();
            var car = (Car)container.New(typeof(Car));

            Assert.That(car.Wheels, Is.Not.Null);
            Assert.That(container.IsRegistered<Car>(), Is.False);
        }

        [Test]
        public void New_is_unable_to_create_open_generic_and_should_Throw_instead()
        {
            var container = new Container();
            var ex = Assert.Throws<ContainerException>(() => 
                container.New(typeof(DoorFor<>)));

            Assert.AreEqual(Error.ResolvingOpenGenericServiceTypeIsNotPossible, ex.Error);
        }

        internal class Wheels
        {
            public string Paint;            
        }

        internal class Car
        {
            public Wheels Wheels { get; private set; }

            public Car(Wheels wheels)
            {
                Wheels = wheels;
            }
        }

        internal static class CarFactory
        {
            public static Car Create(Wheels wheels)
            {
                return new Car(wheels);
            }

            public static Wheels DecorateWheels(Wheels wheels)
            {
                wheels.Paint = "decorated";
                return wheels;
            }
        }

        internal class DoorFor<T>
        {
        }
    }
}
