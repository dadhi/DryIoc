/*cs
Wraps NUnit facilities required for examples to reduce the noise.
 */

using NUnit.Framework;

public class ExampleAttribute : TestAttribute { }

public static class Asserts
{
    public static void Is<TExpected>(this object actual) => Assert.IsInstanceOf<TExpected>(actual);
    public static void IsSameAs<T>(this T actual, T expected) => Assert.AreSame(expected, actual);
    public static void IsTrue(this bool actual) => Assert.IsTrue(actual);
}
