using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue554_Allow_Register_an_open_generic_service_type_with_closed_implementation_to_enable_variance
    {
        [Test]
        public void Can_Register_with_open_generic_service_type_and_Resolve_variance_compatible_type()
        {
            var c = new Container();

            c.Register(typeof(IQuery<,>), typeof(FakeRepo));

            var x = c.Resolve<IQuery<string, object>>(); // resolve reverse type of what implemented by FakeRepo
            Assert.IsNotNull(x);
        }

        [Test]
        public void Can_RegisterMany_with_open_generic_service_type_and_Resolve_variance_compatible_type()
        {
            var c = new Container();

            c.RegisterMany<FakeRepo>();

            Assert.Throws<ContainerException>(() => 
                c.Resolve<IQuery<string, object>>()); // resolve reverse type of what implemented by FakeRepo
        }

        public interface IQuery<in T, out R> { }

        public class FakeRepo : IQuery<object, string>
        {
        }
    }
}
