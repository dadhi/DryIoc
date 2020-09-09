using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    public class GHIssue233_Add_RegisterDelegate_with_parameters_returning_object_for_the_requested_runtime_known_service_type
    {
        [Test]
        public void Should_be_able_to_use_delegate_with_one_argument_returning_object()
        {
            var container = new Container();

            container.Register<A>();

            object CreateObjectB(A a) => new B(a);
            container.RegisterDelegate(typeof(B), (A a) => CreateObjectB(a));

            Assert.IsTrue(container.IsRegistered<B>());

            var b1 = container.Resolve<B>();
            var b2 = container.Resolve<B>();
            Assert.IsInstanceOf<B>(b1);
            Assert.IsInstanceOf<B>(b2);
        }

        [Test]
        public void Should_throw_if_delegate_with_one_argument_returning_Wrong_object()
        {
            var container = new Container();

            container.Register<A>();

            container.RegisterDelegate(typeof(B), (A a) => new Wrong());
            Assert.IsTrue(container.IsRegistered<B>());

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<B>());
            Assert.AreEqual(Error.NameOf(Error.RegisteredDelegateResultIsNotOfServiceType), ex.ErrorName);
        }

        class A { }
        class B
        {
            public readonly A A;
            public B(A a) { A = a; }
        }

        class Wrong {}
    }
}