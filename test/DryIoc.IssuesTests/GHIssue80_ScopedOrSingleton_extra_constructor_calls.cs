using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue80_ScopedOrSingleton_extra_constructor_calls
    {
        [SetUp]
        public void SetUp()
        {
            TrackingDisposable.ConstructorCallsCount = 0;
            TrackingDisposable.DestructorCallsCount = 0;
        }

        [Test]
        public void Issue80_ScopedOrSingleton_SingletonCheck()
        {
            var container = new Container();
            var id = 1000;
            container.RegisterDelegate(_ => ++id);

            container.Register<TrackingDisposable>(Reuse.ScopedOrSingleton);

            Assert.AreEqual(1, container.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, container.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, container.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, TrackingDisposable.ConstructorCallsCount, "expected 1 CONSTRUCTOR CALL, BECAUSE SINGLETON");// value = 3
        }

        [Test]
        public void Issue80_ScopedOrSingleton()
        {
            var container = new Container();
            var id = 1000;
            container.RegisterDelegate(c => ++id);
            container.Register<TrackingDisposable>(Reuse.ScopedOrSingleton);

            var scope = container.OpenScope();

            Assert.AreEqual(1, container.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, container.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, container.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, TrackingDisposable.ConstructorCallsCount);

            Assert.AreEqual(2, scope.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(2, scope.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(2, scope.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(2, TrackingDisposable.ConstructorCallsCount);

            Assert.AreEqual(0, TrackingDisposable.DestructorCallsCount);

            scope.Dispose();
            Assert.AreEqual(1, TrackingDisposable.DestructorCallsCount);

            container.Dispose();
            Assert.AreEqual(2, TrackingDisposable.DestructorCallsCount);
        }

        [Test]
        public void Issue80_Scoped()
        {
            var container = new Container();
            var id = 1000;
            container.RegisterDelegate(c => ++id);
            container.Register<TrackingDisposable>(Reuse.Scoped);
            var scope1 = container.OpenScope();


            Assert.AreEqual(1, scope1.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, scope1.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, scope1.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, TrackingDisposable.ConstructorCallsCount);

            var scope2 = container.OpenScope();

            Assert.AreEqual(2, scope2.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(2, scope2.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(2, scope2.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(2, TrackingDisposable.ConstructorCallsCount);

            Assert.AreEqual(0, TrackingDisposable.DestructorCallsCount);

            scope1.Dispose();
            Assert.AreEqual(1, TrackingDisposable.DestructorCallsCount);

            scope2.Dispose();
            Assert.AreEqual(2, TrackingDisposable.DestructorCallsCount);
        }
    }

    public class TrackingDisposable : IDisposable
    {
        public TrackingDisposable(int i)
        {
            DebugLogger.Log($"~ctor {i}");
        }

        public static Int32 ConstructorCallsCount;
        public static Int32 DestructorCallsCount;
        public Int32 Value { get; set; } = ++ConstructorCallsCount;

        public void Dispose()
        {
            DebugLogger.Log($"~dispose {Value}");
            ++DestructorCallsCount;
        }
    }
    public class DebugLogger
    {
        public static List<String> Lines { get; set; } = new List<String>();
        public static void Log(string s)
        {
            Lines.Add(s);
            Debug.WriteLine(s);
        }

        public static string String()
        {
            return string.Join("\r\n", Lines);
        }
    }
}
