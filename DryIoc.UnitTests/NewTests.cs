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
            var car = container.New<Car>(with: FactoryMethod.Of(() => CarFactory.Create(default(Wheels))));

            Assert.That(car.Wheels, Is.Not.Null);
            Assert.That(container.IsRegistered<Car>(), Is.False);
        }

        [Test]
        public void Should_be_able_to_decorated_created_type()
        {
            var container = new Container();
            container.Register<Wheels>();
            container.Register<Wheels>(
                with: FactoryMethod.Of(() => CarFactory.DecorateWheels(default(Wheels))), 
                setup: SetupDecorator.Default);

            var car = container.New<Car>();

            Assert.That(car.Wheels.Paint, Is.EqualTo("decorated"));
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
    }
}
