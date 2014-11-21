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

            MethodBase method = GetType().GetDeclaredMethodOrNull("Reflection_tests");
            Assert.IsFalse(method.IsConstructor);

            var methodInfos = typeof(X).GetTypeInfo().DeclaredMethods.ToArrayOrSelf();
        }

        class Y
        {
            public void Me() { }
        }

        class X : Y
        {
            public void Yours() { }
        }
    }
}
