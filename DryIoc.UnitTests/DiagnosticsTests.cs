using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
	public class DiagnosticsTests
	{
		[Test]
		public void Factory_expression_returns_expression_used_by_Container_to_create_service()
		{
			var container = new Container();
			container.Register<ServiceWithDependency>();
			container.Register<IDependency, Dependency>(Reuse.Singleton);

			var service = container.Resolve<FactoryExpression<ServiceWithDependency>>();

			Assert.That(service.Value.ToString(), Is.StringContaining("=> new ServiceWithDependency("));
		}
	}
}