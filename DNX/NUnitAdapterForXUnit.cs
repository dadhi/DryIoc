using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NUnit.Framework
{
    public class TestFixtureAttribute : Attribute {}

    public class TestAttribute : Xunit.FactAttribute {}

    public class ExplicitAttribute : Xunit.FactAttribute
    {
        public ExplicitAttribute(string reason = null)
        {
            Skip = "Explicit: " + reason;
        }
    }

    public static class Assert
    {
        public static void That(object @object, Action<object> assert, string reasonIgnored = null)
        {
            assert(@object);
        }

        public static void IsTrue(bool condition, string message = null)
        {
            Xunit.Assert.True(condition, message);
        }

        public static void IsFalse(bool condition, string message = null)
        {
            Xunit.Assert.False(condition, message);
        }

        public static void AreEqual(object expected, object actual, string message = null)
        {
            Xunit.Assert.Equal(expected, actual);
        }

        public static void AreNotEqual(object expected, object actual, string message = null)
        {
            Xunit.Assert.NotEqual(expected, actual);
        }

        public static void AreSame(object expected, object actual, string message = null)
        {
            Xunit.Assert.Same(expected, actual);
        }

        public static void AreNotSame(object expected, object actual, string message = null)
        {
            Xunit.Assert.NotSame(expected, actual);
        }

        public static void IsNull(object @object, string message = null)
        {
            Xunit.Assert.Null(@object);
        }

        public static void IsNotNull(object @object, string message = null)
        {
            Xunit.Assert.NotNull(@object);
        }

        public static void IsInstanceOf<T>(object @object, string message = null)
        {
            Xunit.Assert.IsAssignableFrom(typeof(T), @object);
        }

        public static T Throws<T>(Action action) where T : Exception
        {
            try
            {
                action();
                Fail(string.Format("Should have thrown {0}", typeof(T).Name));
                return null;
            }
            catch (T ex)
            {
                return ex;
            }
        }

        public static void DoesNotThrow(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Fail(string.Format("Should not have thrown {0}", ex));
            }
        }

        public static void Fail(string message = null)
        {
            var pass = "pass";
            if (string.IsNullOrWhiteSpace(message)) message = "fail";
            else if (message == "pass") pass = "test pass"; // unlikely, but...
            AreEqual(pass, message);
        }
    }

    public static class Is
    {
        public static Action<object> Null
        {
            get { return o => Assert.IsNull(o); }
        }

        public static Action<object> True
        {
            get { return o => Assert.IsTrue((bool)o); }
        }

        public static Action<object> False
        {
            get { return o => Assert.IsFalse((bool)o); }
        }

        public static Action<object> InstanceOf<T>()
        {
            return o => Assert.IsInstanceOf<T>(o);
        }

        public static Action<object> EqualTo(object other)
        {
            return o => Assert.AreEqual(other, o);
        }

        public static Action<object> SameAs(object other)
        {
            return o => Assert.AreSame(other, o);
        }

        public static Action<object> StringContaining(string substring)
        {
            return o => Assert.IsTrue(o.ToString().Contains(substring));
        }
        public static Action<object> StringStarting(string substring)
        {
            return o => Assert.AreEqual(0, o.ToString().IndexOf(substring, StringComparison.Ordinal));
        }

        public static class Not
        {
            public static Action<object> Null
            {
                get { return o => Assert.IsNotNull(o); }
            }

            public static Action<object> SameAs(object other)
            {
                return o => Assert.AreNotSame(o, other);
            }
        }
    }

    public static class StringAssert
    {
        public static void Contains(string substring, string source)
        {
            Assert.IsTrue(source.Contains(substring));
        }
    }

    public static class CollectionAssert
    {
        public static void AreEqual(IEnumerable aa, IEnumerable bb)
        {
            var a = aa.GetEnumerator();
            var b = bb.GetEnumerator();
            while (true)
            {
                var hasA = a.MoveNext();
                var hasB = b.MoveNext();
                if (!hasA || !hasB)
                {
                    Assert.IsFalse(hasB, "First collection has less elements than second.");
                    Assert.IsFalse(hasA, "First collection has more elements than second.");
                    break;
                }
                Assert.AreEqual(a.Current, b.Current);
            }
        }

        public static void AreEquivalent<T>(IEnumerable<T> aa, IEnumerable<T> bb)
        {
            foreach (var a in aa)
                Assert.IsTrue(bb.Contains(a));

            foreach (var b in bb)
                Assert.IsTrue(aa.Contains(b));
        }

        public static void IsSubsetOf<T>(IEnumerable<T> subset, IEnumerable<T> superset)
        {
            foreach (var a in subset)
                Assert.IsTrue(superset.Contains(a));
        }
    }
}
