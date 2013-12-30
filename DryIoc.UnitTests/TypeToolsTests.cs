using System;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class TypeToolsTests
    {
        [Test]
        public void GetSelfAndAllInheritedTypes_should_work_for_open_generic_types()
        {
            var types = typeof(Fuzz<>).GetImplementedTypes().ToArray();

            CollectionAssert.AreEqual(new[] { typeof(Fuzz<>), typeof(IBuzz), typeof(IFuzz<>), typeof(IFuzz), typeof(Buzz) }, types);
        }

        [Test]
        public void GetSelfAndAllInheritedTypes_should_work_for_class_nested_in_open_generic_types()
        {
            var types = typeof(Fuzz<>.NestedClazz).GetImplementedTypes().ToArray();

            CollectionAssert.AreEqual(new[] { typeof(Fuzz<>.NestedClazz), typeof(IFuzz<>), typeof(IFuzz) }, types);
        }

        [Test]
        public void Should_return_A_implementors()
        {
            var types = typeof(A).GetImplementedTypes();

            Assert.That(types, Is.EqualTo(new[] { typeof(A), typeof(IB), typeof(IA), typeof(B), typeof(C) }));
        }

        [Test]
        public void Should_return_B_implementors()
        {
            var types = typeof(B).GetImplementedTypes();

            Assert.That(types, Is.EqualTo(new[] { typeof(B), typeof(IB), typeof(C) }));
        }

        [Test]
        public void Can_fill_generic_type_args_based_on_implemented_type_args()
        {
            var openImplType = typeof(Buzz<,>);
            var closedBaseType = typeof(IFizz<int, Wrap<IFizz<Wrap<bool>, int>>>);

            var baseTypes = openImplType.GetImplementedTypes(TypeTools.ReturnBaseOpenGenerics.AsIs, TypeTools.IncludeSelf.Exclude);
            var closedBaseTypeDefinition = closedBaseType.GetGenericTypeDefinition();
            var openBaseType = Array.Find(baseTypes, type => type.GetGenericTypeDefinition() == closedBaseTypeDefinition);
            var targetTypeArgs = openImplType.GetGenericArguments();

            closedBaseType.FillTypeArgsForGenericImplementation(openBaseType, ref targetTypeArgs);
            //Assert.That(Array.Exists(targetTypeArgs, type => type.IsGenericParameter), Is.False);
            CollectionAssert.AreEqual(new object[] { typeof(bool), typeof(int) }, targetTypeArgs);
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

    public interface IFizz<T1, T2> { }

    public class Fizz<T2, T1> : IFizz<T1, T2> { }

    public class Buzz<T1, T2> : IFizz<T2, Wrap<IFizz<Wrap<T1>, T2>>> { }

    public class Wrap<T> { }

    #endregion
}
