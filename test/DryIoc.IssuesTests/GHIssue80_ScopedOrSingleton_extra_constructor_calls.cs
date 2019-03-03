using System;
using System.Collections.Generic;
using System.Diagnostics;
using FluentAssertions;
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
            container.RegisterDelegate((c) => ++id);

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
            container.RegisterDelegate((c) => ++id);
            container.Register<TrackingDisposable>(Reuse.ScopedOrSingleton);
            var childContainer = container.OpenScope();


            Assert.AreEqual(1, container.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, container.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, container.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(3, TrackingDisposable.ConstructorCallsCount);

            Assert.AreEqual(4, childContainer.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(4, childContainer.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(4, childContainer.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(6, TrackingDisposable.ConstructorCallsCount);

            Assert.AreEqual(0, TrackingDisposable.DestructorCallsCount);

            DebugLogger.Log("extra calls #80 for scope; ignore IDisposable");
            Assert.Fail(DebugLogger.String());
        }

        [Test]
        public void Issue80_Scoped()
        {

            var container = new Container();
            var id = 1000;
            container.RegisterDelegate((c) => ++id);
            container.Register<TrackingDisposable>(Reuse.Scoped);
            var childContainer = container.OpenScope();


            Assert.AreEqual(1, childContainer.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, childContainer.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, childContainer.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(3, TrackingDisposable.ConstructorCallsCount);

            var childContainer2 = container.OpenScope();

            Assert.AreEqual(4, childContainer2.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(4, childContainer2.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(4, childContainer2.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(6, TrackingDisposable.ConstructorCallsCount);

            DebugLogger.Log("extra calls #81 for scope; ignore IDisposable");
            Assert.AreEqual(0, TrackingDisposable.DestructorCallsCount);

            Assert.Fail(DebugLogger.String());
        }

    }

    public class TrackingDisposable : IDisposable
    {
        public TrackingDisposable(Int32 i)
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
