using System.Reflection;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class Playground
    {
        [Test]
        public void Reflection_tests()
        {
            MethodBase ctor = GetType().GetConstructorOrNull();
            Assert.IsTrue(ctor.IsConstructor);

            MethodBase method = GetType().GetDeclaredMethod("Reflection_tests");
            Assert.IsFalse(method.IsConstructor);
        }
    }
}
