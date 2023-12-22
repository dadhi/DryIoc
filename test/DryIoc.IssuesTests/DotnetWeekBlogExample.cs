using System;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using DryIocAttributes;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class DotnetWeekBlogExample : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(Foo<,>), typeof(FooDecorator<>), typeof(X), typeof(Y));
            var foo = container.Resolve<IFoo<X, Y>>();
            StringAssert.Contains("decorated", foo.Message);
        }

        public interface IFoo<A, B>
        {
            string Message { get; set; }
        }

        [Export]
        public class X { }

        [Export]
        public class Y { }

        [Export(typeof(IFoo<,>))]
        class Foo<A, B> : IFoo<A, B>
        {
            public Foo(A a, B b) {}

            public string Message { get; set; }
        }

        [Export]
        public class Blah { }

        // AsFactory says that class has factory method(s) to Export 
        [Export]
        public class FooDecorator<B>
        {
            // Decorator or kind of instantiation middleware implemented as normal method
            // Method parameters are injected by container. Lazy, Func, etc are supported
            // AsDecorator instructs to use result of this method for IFoo<> service, decoratee foo is injected by container. 
            [Export, AsDecorator]
            public IFoo<A, B> AddMessage<A>(IFoo<A, B> foo, Func<A> a, Lazy<B> b)
            {
                foo.Message = "decorated with " + a() + " and " + b.Value;
                return foo;
            }
        }
    }
}
