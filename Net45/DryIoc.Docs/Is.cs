/*cs
Wraps NUnit facilities required for examples to reduce the noise.
 */

using NUnit.Framework;

public class ExampleAttribute : TestAttribute { }

public static class Asserts
{
    public static void Is<TExpected>(this object actual) => Assert.IsInstanceOf<TExpected>(actual);
}
