using System;
using System.Text;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class RegisterAttributeTests : ITest
    {
        public int Run()
        {
            Test_generating_the_object_graph();
            Can_register_service_with_tracking_disposable_reuse();

            return 2;
        }

        [Test]
        public void Can_register_service_with_tracking_disposable_reuse()
        {
            var c = new Container();

            var count = c.RegisterByRegisterAttributes(typeof(Registrations));
            Assert.AreEqual(1, count);

            var ad = c.Resolve<ID>();
            Assert.IsNotNull(ad);

            c.Dispose();
            Assert.IsTrue(((AD)ad).IsDisposed);
        }

        [Register<ID, AD>(TrackDisposableTransient = DisposableTracking.TrackDisposableTransient)]
        public static class Registrations { }

        public interface ID { }

        public class AD : ID, IDisposable
        {
            public bool IsDisposed;
            public void Dispose() => IsDisposed = true;
        }

        public interface IA { }
        public class A : IA { }

        public class B
        {
            public readonly IA A;
            public B(IA a) => A = a;
        }

        [Register<IA, A>(ReuseAs.Singleton)]
        [Register<B>(ReuseAs = ReuseAs.Scoped)]
        public static class DiConfig { }

        [Test]
        public void Test_generating_the_object_graph()
        {
            using var c = new Container(
            // rules => rules.WithExpressionGeneration()
            );

            var count = c.RegisterByRegisterAttributes(typeof(DiConfig));
            Assert.AreEqual(2, count);

            using var scope = c.OpenScope();

            var b = scope.Resolve<B>();
            Assert.IsNotNull(b);
            Assert.IsInstanceOf<A>(b.A);

            var sb = new StringBuilder(4096);
            var containerForGen = c.GenerateCompileTimeContainerCSharpCode(sb,
                roots: [], // generates top level Resolve for all registered services
                namespaceUsings: [nameof(UnitTests)],
                genCompileTimeContainerClassName: "MyCompTimeContainer");

            var code = sb.ToString();

            StringAssert.Contains("MyCompTimeContainer", code);
            StringAssert.Contains("new RegisterAttributeTests.A", code);
            StringAssert.Contains("new RegisterAttributeTests.B", code);
        }
    }
}
