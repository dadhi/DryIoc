using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue235_Add_RegisterDelegateMany_RegisterInstanceMany_UseMany_to_fill_the_API_gap
    {
        [Test]
        public void Should_register_all_instance_public_implemented_types_by_default()
        {
            var container = new Container();

            var x = new X();
            container.RegisterInstanceMany(x);

            Assert.IsTrue(container.IsRegistered<B>());
            Assert.IsTrue(container.IsRegistered<X>());
        }

        [Test]
        public void Should_register_all_instance_implemented_types_by_with_us_nonPublicTypes_parameter()
        {
            var container = new Container();

            var x = new X();
            container.RegisterInstanceMany(x, nonPublicServiceTypes: true);

            Assert.IsTrue(container.IsRegistered<A>());
            Assert.IsTrue(container.IsRegistered<B>());
            Assert.IsTrue(container.IsRegistered<X>());
        }

        [Test]
        public void Should_register_all_instance_explicitly_specified_types()
        {
            var container = new Container();

            var x = new X();
            container.RegisterInstanceMany(new[] { typeof(A), typeof(B) }, x);

            Assert.IsTrue(container.IsRegistered<A>());
            Assert.IsTrue(container.IsRegistered<B>());
        }

        [Test]
        public void Should_register_all_instance_public_types_implemented_by_provided_type()
        {
            var container = new Container();

            var x = new X();
            container.RegisterInstanceMany(typeof(X), x);

            Assert.IsTrue(container.IsRegistered<B>());
            Assert.IsTrue(container.IsRegistered<X>());
        }

        [Test]
        public void Should_use_the_same_instance_for_multiple_interfaces()
        {
            var container = new Container();

            var x = new X();
            
            using (var scope = container.OpenScope())
            {
                scope.UseMany(x, typeof(A), typeof(B));
                Assert.IsInstanceOf<X>(scope.Resolve<A>());
                Assert.IsInstanceOf<X>(scope.Resolve<B>());

                Assert.IsTrue(scope.IsUsed<A>());
                // Assert.IsTrue(container.IsRegisteredOrUsed<A>());
            }
        }

        internal interface A { }
        public interface B { }
        public class X : A, B { }
    }
}