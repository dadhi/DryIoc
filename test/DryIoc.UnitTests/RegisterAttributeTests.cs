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
            Test_generating_the_object_graph_with_the_missing_services();
            Test_generating_the_object_graph();
            Can_register_service_with_tracking_disposable_reuse();

            return 3;
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

        public class B2
        {
            public readonly A A;
            public B2(A a) => A = a;
        }

        [Register<IA, A>(ReuseAs.Singleton)]
        [Register<B>(ReuseAs = ReuseAs.Scoped)]
        [Register<B2>(ReuseAs = ReuseAs.ScopedOrSingleton)]
        public static class DiConfig { }

        [Test]
        public void Test_generating_the_object_graph()
        {
            using var c = new Container();

            var count = c.RegisterByRegisterAttributes(typeof(DiConfig));

            using var scope = c.OpenScope();

            var b = scope.Resolve<B>();
            Assert.IsNotNull(b);
            Assert.IsInstanceOf<A>(b.A);

            var sb = new StringBuilder(4096);
            var containerForGen = c.GenerateCompileTimeContainerCSharpCode(sb,
                roots: null, // generates top level Resolve for all registered services
                namespaceUsings: new[] { nameof(UnitTests) },
                genCompileTimeContainerClassName: "MyCompTimeContainer");

            var code = sb.ToString();

            StringAssert.Contains("MyCompTimeContainer", code);
            StringAssert.Contains("new RegisterAttributeTests.A", code);
            StringAssert.Contains("new RegisterAttributeTests.B", code);
        }

        [Test]
        public void Test_generating_the_object_graph_with_the_missing_services()
        {
            using var c = new Container();

            var count = c.RegisterByRegisterAttributes(typeof(DiConfig));

            var sb = new StringBuilder(4096);
            var containerForGen = c.GenerateCompileTimeContainerCSharpCode(sb,
                roots: new[] { ServiceInfo.Of<B2>() },
                namespaceUsings: new[] { nameof(UnitTests) },
                genCompileTimeContainerClassName: "MyCompTimeContainer2");

            var code = sb.ToString();

            StringAssert.Contains("MyCompTimeContainer2", code);
            StringAssert.Contains("new RegisterAttributeTests.B2", code);
        }
    }
}
