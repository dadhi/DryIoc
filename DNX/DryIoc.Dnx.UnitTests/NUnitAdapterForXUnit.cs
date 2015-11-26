using System;
using System.Collections;

namespace NUnit.Framework
{
    public class TestFixtureAttribute : Attribute {}

    public class TestAttribute : Xunit.FactAttribute {}

    public static class Assert
    {
        public static void IsTrue(bool condition, string message = null)
        {
            Xunit.Assert.True(condition, message);
        }

        public static void IsFalse(bool condition, string message = null)
        {
            Xunit.Assert.False(condition, message);
        }

        public static void AreEqual<T>(T expected, T actual, string message = null)
        {
            Xunit.Assert.Equal<T>(expected, actual);
        }

        public static void AreNotEqual<T>(T expected, T actual, string message = null)
        {
            Xunit.Assert.NotEqual<T>(expected, actual);
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
            Xunit.Assert.IsType(typeof(T), @object);
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
    }
}
