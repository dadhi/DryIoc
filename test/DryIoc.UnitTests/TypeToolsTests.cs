using System.Linq;
using DryIoc.FastExpressionCompiler.LightExpression;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class TypeToolsTests : ITest
    {
        public int Run()
        {
            GetImplementedTypes_should_work_for_open_generic_types();
            GetImplementedTypes_should_work_for_class_nested_in_open_generic_type_with_include_SourceType_option();
            GetImplementedTypes_should_work_for_class_nested_in_open_generic_type_with_include_ObjectType_option();
            GetImplementedTypes_should_work_for_class_nested_in_open_generic_type_with_both_SourceType_ObjectType_options();
            Should_return_A_implementors();
            Should_return_B_implementors();
            IsCompilerGenerated_returns_false_for_string_type();
            IsCompilerGenerated_returns_true_for_anonymous_type();
            return 8;
        }

        [Test]
        public void GetImplementedTypes_should_work_for_open_generic_types()
        {
            var types = typeof(Fuzz<>).GetImplementedTypes().Select(t => t.GetGenericDefinitionOrNull() ?? t);

            CollectionAssert.AreEqual(new[] { typeof(IBuzz), typeof(IFuzz<>), typeof(IFuzz), typeof(Buzz) }, types);
        }

        [Test]
        public void GetImplementedTypes_should_work_for_class_nested_in_open_generic_type_with_include_SourceType_option()
        {
            var types = typeof(Fuzz<>.NestedClazz).GetImplementedTypes(ReflectionTools.AsImplementedType.SourceType)
                .Select(t => t.GetGenericDefinitionOrNull() ?? t);

            CollectionAssert.AreEqual(new[] { typeof(Fuzz<>.NestedClazz), typeof(IFuzz<>), typeof(IFuzz) }, types);
        }

        [Test]
        public void GetImplementedTypes_should_work_for_class_nested_in_open_generic_type_with_include_ObjectType_option()
        {
            var types = typeof(Fuzz<>.NestedClazz).GetImplementedTypes(ReflectionTools.AsImplementedType.ObjectType)
                .Select(t => t.GetGenericDefinitionOrNull() ?? t);

            CollectionAssert.AreEqual(new[] { typeof(IFuzz<>), typeof(IFuzz), typeof(object) }, types);
        }

        [Test]
        public void GetImplementedTypes_should_work_for_class_nested_in_open_generic_type_with_both_SourceType_ObjectType_options()
        {
            var types = typeof(Fuzz<>.NestedClazz).GetImplementedTypes(ReflectionTools.AsImplementedType.ObjectType | ReflectionTools.AsImplementedType.SourceType)
                .Select(t => t.GetGenericDefinitionOrNull() ?? t);

            CollectionAssert.AreEqual(new[] { typeof(Fuzz<>.NestedClazz), typeof(IFuzz<>), typeof(IFuzz), typeof(object) }, types);
        }

        [Test]
        public void Should_return_A_implementors()
        {
            var types = typeof(A).GetImplementedTypes(ReflectionTools.AsImplementedType.ObjectType);

            Assert.That(types, Is.EqualTo(new[] { typeof(IB), typeof(IA), typeof(B), typeof(C), typeof(object) }));
        }

        [Test]
        public void Should_return_B_implementors()
        {
            var types = typeof(B).GetImplementedTypes(ReflectionTools.AsImplementedType.ObjectType);

            Assert.That(types, Is.EqualTo(new[] { typeof(IB), typeof(C), typeof(object) }));
        }

        [Test]
        public void IsCompilerGenerated_returns_false_for_string_type()
        {
            Assert.That(typeof(string).IsCompilerGenerated(), Is.False);
        }

        [Test]
        public void IsCompilerGenerated_returns_true_for_anonymous_type()
        {
            Assert.That(new { }.GetType().IsCompilerGenerated(), Is.True);
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
