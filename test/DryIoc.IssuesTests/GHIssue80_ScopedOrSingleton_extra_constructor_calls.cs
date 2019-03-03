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
            TrackingDisposable.ConstructorIndex = 0;
            TrackingDisposable.DestructorIndex = 0;
        }

        [Test]
        public void DryIocTest_Issue80()
        {
            var container = new Container();
            var id = 1000;
            container.RegisterDelegate((c) => ++id);
            container.Register<TrackingDisposable>(Reuse.ScopedOrSingleton);
            var childContainer = container.OpenScope();


            Assert.AreEqual(1, container.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, container.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, container.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(3, TrackingDisposable.ConstructorIndex);

            Assert.AreEqual(4, childContainer.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(4, childContainer.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(4, childContainer.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(6, TrackingDisposable.ConstructorIndex);

            Assert.AreEqual(0, TrackingDisposable.DestructorIndex);

            DebugLogger.Log("extra calls #80 for scope; ignore IDisposable");
            Assert.Pass(DebugLogger.String());
        }

        [Test]
        public void DryIocTest_Issue81()
        {

            var container = new Container();
            var id = 1000;
            container.RegisterDelegate((c) => ++id);
            container.Register<TrackingDisposable>(Reuse.Scoped);
            var childContainer = container.OpenScope();


            Assert.AreEqual(1, childContainer.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, childContainer.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(1, childContainer.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(3, TrackingDisposable.ConstructorIndex);

            var childContainer2 = container.OpenScope();

            Assert.AreEqual(4, childContainer2.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(4, childContainer2.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(4, childContainer2.Resolve<TrackingDisposable>().Value);
            Assert.AreEqual(6, TrackingDisposable.ConstructorIndex);

            DebugLogger.Log("extra calls #81 for scope; ignore IDisposable");
            Assert.AreEqual(0, TrackingDisposable.DestructorIndex);

            Assert.Pass(DebugLogger.String());
        }

        [Test]
        public void Test_Scope_1()
        {
            var id = 1000;
            string s = "12345";

            var container = new Container();
            container.RegisterInstance(s);
            container.RegisterDelegate((c) => ++id);
            container.Register<TrackingDisposable>(Reuse.Scoped);
            container.Resolve<string>().Should().Be(s);

            var child = container.OpenScope();
            child.Resolve<TrackingDisposable>().Value.Should().Be(1);
            child.Resolve<TrackingDisposable>().Value.Should().Be(1);
            child.Resolve<TrackingDisposable>().Value.Should().Be(1);
            child.Resolve<string>().Should().Be(s);

            var child2 = child.OpenScope();

            child2.Resolve<TrackingDisposable>().Value.Should().Be(4);
            child2.Resolve<TrackingDisposable>().Value.Should().Be(4);
            child2.Resolve<TrackingDisposable>().Value.Should().Be(4);
            child2.Resolve<string>().Should().Be(s);

            var child3 = child2.OpenScope();

            child3.Resolve<TrackingDisposable>().Value.Should().Be(7);
            child3.Resolve<TrackingDisposable>().Value.Should().Be(7);
            child3.Resolve<TrackingDisposable>().Value.Should().Be(7);
            child3.Resolve<string>().Should().Be(s);

            child3.Dispose();

            child2.Resolve<string>().Should().Be(s);
            child2.Resolve<TrackingDisposable>().Value.Should().Be(4);
            child2.Resolve<TrackingDisposable>().Value.Should().Be(4);
            child2.Resolve<TrackingDisposable>().Value.Should().Be(4);

            child.Resolve<string>().Should().Be(s);
            child.Resolve<TrackingDisposable>().Value.Should().Be(1);
            child.Resolve<TrackingDisposable>().Value.Should().Be(1);
            child.Resolve<TrackingDisposable>().Value.Should().Be(1);

            container.Resolve<string>().Should().Be(s);

            var ms = container.With(rules => Rules.MicrosoftDependencyInjectionRules);
            var value = 6666;

            ms.Use(value);
            ms.Resolve<Int32>().Should().Be(value);
        }
    }

    public class TrackingDisposable : IDisposable
    {
        public TrackingDisposable(Int32 i)
        {
            DebugLogger.Log($"~ctor {i}");
        }

        public static Int32 ConstructorIndex;
        public static Int32 DestructorIndex;
        public Int32 Value { get; set; } = ++ConstructorIndex;

        public void Dispose()
        {
            DebugLogger.Log($"~dispose {Value}");
            ++DestructorIndex;
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
