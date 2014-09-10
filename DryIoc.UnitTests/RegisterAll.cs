using NUnit.Framework;

namespace DryIoc.UnitTests
{
	[TestFixture]
	public class RegisterAllTests
	{
		[Test]
		public void Can_register_single_registrations_for_all_public_types_implemented()
		{
			// Arrange
			var container = new Container();
			container.RegisterAll<Someberry>();

			Assert.That(container.IsRegistered<IBerry>(), Is.False);
			Assert.That(container.IsRegistered<IProduct>(), Is.True);
			Assert.That(container.IsRegistered<Someberry>(), Is.True);
		}

		[Test]
		public void Singleton_registered_with_multiple_interfaces_should_be_the_same()
		{
			// Arrange
			var container = new Container();
            container.RegisterAll<Someberry>(Reuse.Singleton);

			// Act
			var product = container.Resolve<IProduct>();
            var berry = container.Resolve<Someberry>();

			Assert.That(product, Is.SameAs(berry));
		}

	    public class Someberry : IBerry, IProduct
		{
		}

		public interface IProduct
		{
		}

	    internal interface IBerry
		{
		}
	}
}