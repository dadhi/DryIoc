using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue536_DryIoc_Exception_in_a_Constructor_of_a_Dependency_does_tunnel_through_Resolve_call : ITest
    {
        public int Run()
        {
            Test_dep_scoped_in_factory_delegate_should_rethrow_the_exception_WithoutUseInterpretation();
            Test_dep_scoped_in_registered_delegate_should_rethrow_the_exception_WithoutUseInterpretation();
            Test_dep_scoped_should_rethrow_the_exception_WithoutUseInterpretation();
            Test_root_singleton_should_rethrow_the_exception();
            Test_dep_singleton_should_rethrow_the_exception();
            Test_dep_scoped_should_rethrow_the_exception();
            return 6;
        }

        [Test]
        public void Test_root_singleton_should_rethrow_the_exception()
        {
            var container = new Container();

            container.Register<IInterfaceA, ClassA>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            Assert.Throws<ArgumentException>(() =>
                container.Resolve<IInterfaceA>());

            Assert.Throws<ArgumentException>(() =>
                container.Resolve<IInterfaceA>());
        }

        [Test]
        public void Test_dep_singleton_should_rethrow_the_exception()
        {
            var container = new Container();

            container.Register<B>();
            container.Register<IInterfaceA, ClassA>(Reuse.Singleton);

            Assert.Throws<ArgumentException>(() =>
                container.Resolve<B>());

            Assert.Throws<ArgumentException>(() =>
                container.Resolve<B>());
        }

        [Test]
        public void Test_dep_scoped_should_rethrow_the_exception()
        {
            var container = new Container();

            container.Register<B>();
            container.Register<IInterfaceA, ClassA>(Reuse.Scoped);

            using var scope = container.OpenScope();

            Assert.Throws<ArgumentException>(() =>
                scope.Resolve<B>());

            Assert.Throws<ArgumentException>(() =>
                scope.Resolve<B>());
        }

        [Test]
        public void Test_dep_scoped_should_rethrow_the_exception_WithoutUseInterpretation()
        {
            var container = new Container(Rules.Default.WithoutUseInterpretation());

            container.Register<B>();
            container.Register<IInterfaceA, ClassA>(Reuse.Scoped);

            using var scope = container.OpenScope();

            Assert.Throws<ArgumentException>(() =>
                scope.Resolve<B>());

            Assert.Throws<ArgumentException>(() =>
                scope.Resolve<B>());
        }

        [Test]
        public void Test_dep_scoped_in_registered_delegate_should_rethrow_the_exception_WithoutUseInterpretation()
        {
            var container = new Container(Rules.Default.WithoutUseInterpretation());

            container.RegisterDelegate<IInterfaceA, B>(a => new B(a));

            container.Register<IInterfaceA, ClassA>(Reuse.Scoped);

            using var scope = container.OpenScope();

            Assert.Throws<ArgumentException>(() =>
                scope.Resolve<B>());

            Assert.Throws<ArgumentException>(() =>
                scope.Resolve<B>());
        }

        [Test]
        public void Test_dep_scoped_in_factory_delegate_should_rethrow_the_exception_WithoutUseInterpretation()
        {
            var container = new Container(Rules.Default.WithoutUseInterpretation());

            container.RegisterDelegate(ctx => new B(ctx.Resolve<IInterfaceA>()));

            container.Register<IInterfaceA, ClassA>(Reuse.Scoped);

            using var scope = container.OpenScope();

            Assert.Throws<ArgumentException>(() =>
                scope.Resolve<B>());

            Assert.Throws<ArgumentException>(() =>
                scope.Resolve<B>());
        }

        public class B
        {
            public readonly IInterfaceA IntA;
            public B(IInterfaceA a) => IntA = a;
        }

        public interface IInterfaceA { }

        public class ClassA : IInterfaceA
        {
            public ClassA()
            {
                throw new ArgumentException("This is my error");
            }
        }
    }
}
