using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue544_WithTrackingDisposableTransients_may_downgrade_Singletons_to_Transients
    {
        public interface IA { }
        public class A : IA { }
        class B : IA { public B(IA aa) { } }

        public interface IE : IDisposable { }
        public class E : IE { public void Dispose() { } }
        class F : IE { public F(IE ee) { } public void Dispose() { } }

        class C : IE { public C(IE[] ee) { } public void Dispose() { } }

        [Test]
        public void Decorator_CanHave_SingletonReuse()
        {
            var container = new Container(r => r.WithTrackingDisposableTransients());

            // base transient service
            container.Register<IA, A>(Reuse.Transient);

            // decorator that pins it as singleton
            container.Register<IA, B>(Reuse.Singleton, setup: Setup.Decorator);

            var b1 = container.Resolve<IA>();
            var b2 = container.Resolve<IA>();
            Assert.AreSame(b1, b2);
        }

        [Test]
        public void Decorator_CanHave_SingletonReuse_AlsoForDisposables()
        {
            var container = new Container(r => r.WithTrackingDisposableTransients());

            // base transient service
            container.Register<IE, E>(Reuse.Transient);

            // DECORATOR that pins them as singleton
            container.Register<IE, F>(Reuse.Singleton, setup: Setup.Decorator);

            var f1 = container.Resolve<IE>();
            var f2 = container.Resolve<IE>();

            Assert.IsInstanceOf<F>(f1);

            // BUG: when 'adapter' is active then the net effect is: NOT SAME
            // it seems Decorator-Singleton was downgraded to Transient !!
            Assert.AreSame(f1, f2);
        }

        [Test]
        public void Composite_PreservesSingletonReuse()
        {
            var container = new Container(r => r
                .WithFactorySelector(Rules.SelectLastRegisteredFactory())
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithTrackingDisposableTransients());

            // base transient service
            container.Register<IE, E>(Reuse.Transient);

            // COMPOSITE that pins them as singletons
            container.Register<IE, C>(Reuse.Singleton);

            var c1 = container.Resolve<IE>();
            var c2 = container.Resolve<IE>();
            Assert.IsInstanceOf<C>(c1);
            Assert.IsInstanceOf<C>(c2);
            Assert.AreSame(c1, c2);
        }
    }
}
