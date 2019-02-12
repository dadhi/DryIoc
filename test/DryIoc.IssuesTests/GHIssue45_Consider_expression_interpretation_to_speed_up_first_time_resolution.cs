using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    class GHIssue45_Consider_expression_interpretation_to_speed_up_first_time_resolution
    {
        [Test]
        public void Test()
        {
            var container = new Container();

            container.Register<ScopedBlah>(Reuse.Scoped);

            container.Register<Parameter1>(Reuse.Transient);
            container.Register<Parameter2>(Reuse.Singleton);

            using (var scope = container.OpenScope())
            {
                var blah = scope.Resolve<ScopedBlah>();
                Assert.IsNotNull(blah);
            }
        }

        internal class Parameter1 { }
        internal class Parameter2 { }
        internal class Parameter3 { }

        internal class ScopedBlah
        {
            public Parameter1 Parameter1 { get; }
            public Parameter2 Parameter2 { get; }

            public ScopedBlah(Parameter1 parameter1, Parameter2 parameter2)
            {
                Parameter1 = parameter1;
                Parameter2 = parameter2;
            }
        }

        [Test]
        public void Interpreting_scoped_registered_delegates()
        {
            var container = new Container();

            container.Register<R>(Reuse.Scoped);

            container.RegisterDelegate(_ => new X(), Reuse.Scoped);
            container.RegisterDelegate(_ => new Y(), Reuse.Scoped);
            container.RegisterDelegate(_ => 42, Reuse.Scoped);
            container.Register<S>(Reuse.Scoped);

            using (var scope = container.OpenScope())
            {
                var r = scope.Resolve<R>();

                Assert.IsNotNull(r.X);
                Assert.IsNotNull(r.Y);
                Assert.AreEqual(42, r.Unknown);
            }
        }

        public class R
        {
            public readonly X X;
            public readonly Y Y;
            public readonly int Unknown;
            public readonly S S;

            public R(X x, Y y, int _42, S s)
            {
                S = s;
                X = x;
                Y = y;
                Unknown = _42;
            }
        }

        public class X { }
        public class Y { }

        public struct S
        {
            public S(int _42) { }
        }
    }
}
