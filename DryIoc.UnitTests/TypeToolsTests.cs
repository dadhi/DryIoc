using System.Linq;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class TypeToolsTests
    {
        [Test]
        public void GetImplementedTypes_should_work_for_open_generic_types()
        {
            var types = typeof(Fuzz<>).GetImplementedTypes()
                .Select(t => t.ContainsGenericParameters ? t.GetGenericTypeDefinition() : t);

            CollectionAssert.AreEqual(new[] { typeof(IBuzz), typeof(IFuzz<>), typeof(IFuzz), typeof(Buzz) }, types);
        }

        [Test]
        public void GetImplementedTypes_should_work_for_class_nested_in_open_generic_types()
        {
            var types = typeof(Fuzz<>.NestedClazz).GetImplementedTypes(TypeTools.IncludeTypeItself.AsFirst)
                .Select(t => t.ContainsGenericParameters ? t.GetGenericTypeDefinition() : t);

            CollectionAssert.AreEqual(new[] { typeof(Fuzz<>.NestedClazz), typeof(IFuzz<>), typeof(IFuzz) }, types);
        }

        [Test]
        public void Should_return_A_implementors()
        {
            var types = typeof(A).GetImplementedTypes();

            Assert.That(types, Is.EqualTo(new[] { typeof(IB), typeof(IA), typeof(B), typeof(C) }));
        }

        [Test]
        public void Should_return_B_implementors()
        {
            var types = typeof(B).GetImplementedTypes();

            Assert.That(types, Is.EqualTo(new[] { typeof(IB), typeof(C) }));
        }
    }

    #region CUT

    public class C { }

    public interface IB { }

    public class B : C, IB { }

    public interface IA { }

    public class A : B, IA { }

    public class Fuzz<T> : Buzz, IFuzz<T>
    {
        public class NestedClazz : IFuzz<T> { }
    }

    public class Buzz : IBuzz { }

    public interface IFuzz<T> : IFuzz { }

    public interface IFuzz { }

    public interface IBuzz { }

    #endregion
}
