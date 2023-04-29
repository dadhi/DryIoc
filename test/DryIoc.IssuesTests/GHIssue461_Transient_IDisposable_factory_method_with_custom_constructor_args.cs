using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue461_Transient_IDisposable_factory_method_with_custom_constructor_args : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            using var c = new Container();

            // Resolver should receive type name of original requester
            c.Register<LifetimeResolver>(
                made: Parameters.Of.Type<string>(r => r.Parent.Parent.ImplementationType.Name),
                setup: Setup.With(trackDisposableTransient: true));

            c.Register<Lifetime>(Reuse.Scoped,
                made: Made.Of(r => ServiceInfo.Of<LifetimeResolver>(), f => f.GetValue()),
                setup: Setup.With(asResolutionCall: true));

            c.Register<A>();
            c.Register<B>();
            c.Register<C>();

            using var aScope = c.OpenScope();
            var a = aScope.Resolve<A>();

            using var bScope = c.OpenScope();
            var b = bScope.Resolve<B>();

            Assert.AreEqual("A", a.Lifetime.Id);
            Assert.AreEqual("B", b.Lifetime.Id);
            Assert.AreEqual("B", b.C.Lifetime.Id);
        }

        public class LifetimeResolver : IDisposable
        {
            private string _id;
            public LifetimeResolver(string id) => _id = id;

            private Lifetime _lifetime;
            public Lifetime GetValue()
            {
                if (_lifetime is null)
                    _lifetime = new Lifetime(_id);
                return _lifetime;
            }

            public void Dispose() => _lifetime?.Terminate();
        }

        public class Lifetime
        {
            public string Id;
            public Lifetime(string id) => Id = id;
            internal void Terminate() {}
        }

        public abstract class L
        {
            public Lifetime Lifetime { get; }
            protected L(Lifetime l) => Lifetime = l;
        }
        public class A : L
        {
            public A(Lifetime aLifetime) : base(aLifetime) { }
        }

        public class B : L
        {
            public C C { get; }
            public B(Lifetime bLifetime, C c) : base(bLifetime) => C = c;
        }

        public class C
        {
            public Lifetime Lifetime { get; }
            public C(Lifetime l) => Lifetime = l;
        }
    }
}