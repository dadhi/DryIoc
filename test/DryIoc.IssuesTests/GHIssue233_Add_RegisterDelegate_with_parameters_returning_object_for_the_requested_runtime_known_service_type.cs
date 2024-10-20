using DryIoc.FastExpressionCompiler.LightExpression;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue233_Add_RegisterDelegate_with_parameters_returning_object_for_the_requested_runtime_known_service_type : ITest
    {
        public int Run()
        {
            // Should_throw_if_delegate_with_one_argument_returning_Wrong_object_WithoutUseInterpretation();
            Should_be_able_to_register_delegate_with_runtime_service_type_with_one_argument_returning_object();
            Should_throw_if_delegate_with_one_argument_returning_Wrong_object();
            Should_be_able_to_register_delegate_with_runtime_service_type_with_two_arguments_returning_object();
            For_expression_generation_Should_be_able_to_register_delegate_with_runtime_service_type_with_two_arguments_returning_object();
            return 4;
        }

        [Test]
        public void Should_be_able_to_register_delegate_with_runtime_service_type_with_one_argument_returning_object()
        {
            var container = new Container();

            container.Register<A>();

            object CreateObjectB(A a) => new B(a);
            container.RegisterDelegate(typeof(B), (A a) => CreateObjectB(a));

            Assert.IsTrue(container.IsRegistered<B>());

            // call multiple times to trigger interpretation and compilation
            var x1 = container.Resolve<B>();
            var x2 = container.Resolve<B>();
            Assert.IsInstanceOf<B>(x1);
            Assert.IsInstanceOf<B>(x2);
        }

        [Test]
        public void Should_throw_if_delegate_with_one_argument_returning_Wrong_object()
        {
            var container = new Container();

            container.Register<A>();

            container.RegisterDelegate(typeof(B), (A a) => new NotB());
            Assert.IsTrue(container.IsRegistered<B>());

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<B>());
            Assert.AreEqual(Error.NameOf(Error.NoConversionOperatorFoundWhenInterpretingTheConvertExpression), ex.ErrorName);
        }

        [Test]
        public void Should_throw_if_delegate_with_one_argument_returning_Wrong_object_WithoutUseInterpretation()
        {
            var container = new Container(Rules.Default.WithoutUseInterpretation());

            container.Register<A>();

            container.RegisterDelegate(typeof(B), (A a) => new NotB());
            Assert.IsTrue(container.IsRegistered<B>());

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<B>());
            Assert.AreEqual(Error.NameOf(Error.RegisteredDelegateResultIsNotOfServiceType), ex.ErrorName);
        }

        [Test]
        public void Should_be_able_to_register_delegate_with_runtime_service_type_with_two_arguments_returning_object()
        {
            var container = new Container();

            container.Register<A>();
            container.Register<B>();

            object CreateObjectB(A a, B b) => new C(a, b);
            container.RegisterDelegate(typeof(C), (A a, B b) => CreateObjectB(a, b));

            Assert.IsTrue(container.IsRegistered<C>());

            // call multiple times to trigger interpretation and compilation
            var x1 = container.Resolve<C>();
            var x2 = container.Resolve<C>();
            Assert.IsInstanceOf<C>(x1);
            Assert.IsInstanceOf<C>(x2);
        }

        [Test]
        public void For_expression_generation_Should_be_able_to_register_delegate_with_runtime_service_type_with_two_arguments_returning_object()
        {
            var container = new Container(Rules.Default.WithExpressionGenerationSettingsOnly());

            container.Register<A>();
            container.Register<B>();
            container.RegisterDelegate(typeof(C), (A a, B b) => new C(a, b));
            container.Register<D>();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<LambdaExpression>(typeof(D)));

            Assert.AreEqual(Error.NameOf(Error.StateIsRequiredToUseItem), ex.ErrorName);
        }

        class D
        {
            public C C;
            public D(C c) => C = c;
        }

        class A { }
        class B
        {
            public readonly A A;
            public B(A a) { A = a; }
        }

        class C
        {
            public A A;
            public B B;
            public C(A a, B b) => (A, B) = (a, b);
        }

        class NotB { }
    }
}