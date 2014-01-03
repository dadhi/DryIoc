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
        public void Can_fill_generic_implmentation_type_args_based_on_base_type_args()
        {
            var openImplType = typeof(Buzz<,>);
            var closedBaseType = typeof(IFizz<int, Wrap<IFizz<Wrap<bool>, int>>>);

            var closedBaseTypeDefinition = closedBaseType.GetGenericTypeDefinition();
            
            var baseTypes = openImplType.GetImplementedTypes(TypeTools.ReturnBaseOpenGenerics.AsIs, TypeTools.IncludeSelf.Exclude);
            var openBaseType = Array.Find(baseTypes, type => type.GetGenericTypeDefinition() == closedBaseTypeDefinition);
            
            var targetTypeArgs = openImplType.GetGenericArguments();

            closedBaseType.GetGenericImplTypeArgsFromBaseType(openBaseType, ref targetTypeArgs);
            //Assert.That(Array.Exists(targetTypeArgs, type => type.IsGenericParameter), Is.False);
            CollectionAssert.AreEqual(new object[] { typeof(bool), typeof(int) }, targetTypeArgs);
        }

        [Test]
        public void Should_find_mismatch_between_closed_and_base_generic_args()
        {
            var openImplType = typeof(Buzz<,>);
            var baseTypeDef = typeof(IFizz<,>);
            var baseTypes = openImplType.GetImplementedTypes(TypeTools.ReturnBaseOpenGenerics.AsIs, TypeTools.IncludeSelf.Exclude);
            var openBaseType = Array.Find(baseTypes, type => type.GetGenericTypeDefinition() == baseTypeDef);
            var openBaseTypeArgs = openBaseType.GetGenericArguments();

            Assert.IsFalse(TypeTools.MatchClosedGenericWithBaseOpenGenericTypeArgs(typeof(IFizz<int, string>).GetGenericArguments(), openBaseTypeArgs));
            Assert.IsFalse(TypeTools.MatchClosedGenericWithBaseOpenGenericTypeArgs(typeof(IFizz<int, Wrap<string>>).GetGenericArguments(), openBaseTypeArgs));
            Assert.IsFalse(TypeTools.MatchClosedGenericWithBaseOpenGenericTypeArgs(typeof(IFizz<int, Wrap<IFizz<string, int>>>).GetGenericArguments(), openBaseTypeArgs));
            Assert.IsTrue(TypeTools.MatchClosedGenericWithBaseOpenGenericTypeArgs(typeof(IFizz<int, Wrap<IFizz<Wrap<string>, int>>>).GetGenericArguments(), openBaseTypeArgs));
        }

        [Test]
        public void Should_find_mismatch_between_closed_and_base_generic_args_With_multiple_different_closed_types_matched_to_single_open_arg()
        {
            var openImplType = typeof(Buzz<,>);
            var baseTypeDef = typeof(IFizz<,>);
            var baseTypes = openImplType.GetImplementedTypes(TypeTools.ReturnBaseOpenGenerics.AsIs, TypeTools.IncludeSelf.Exclude);
            var openBaseType = Array.Find(baseTypes, type => type.GetGenericTypeDefinition() == baseTypeDef);
            var openBaseTypeArgs = openBaseType.GetGenericArguments();

            // both INT and BOOL are matched with T2.
            Assert.IsFalse(TypeTools.MatchClosedGenericWithBaseOpenGenericTypeArgs(typeof(IFizz<int, Wrap<IFizz<Wrap<string>, bool>>>).GetGenericArguments(), openBaseTypeArgs));
        }

        [Test]
        public void Should_find_mismatch_between_closed_and_base_generic_args_With_closed_arg_in_the_middle()
        {
            var openImplType = typeof(BuzzInt<,>);
            var baseTypeDef = typeof(IFizz<,>);
            var baseTypes = openImplType.GetImplementedTypes(TypeTools.ReturnBaseOpenGenerics.AsIs, TypeTools.IncludeSelf.Exclude);
            var openBaseType = Array.Find(baseTypes, type => type.GetGenericTypeDefinition() == baseTypeDef);
            var openBaseTypeArgs = openBaseType.GetGenericArguments();

            Assert.IsTrue(TypeTools.MatchClosedGenericWithBaseOpenGenericTypeArgs(typeof(IFizz<IFizz<string, string>, bool>).GetGenericArguments(), openBaseTypeArgs));
            Assert.IsFalse(TypeTools.MatchClosedGenericWithBaseOpenGenericTypeArgs(typeof(IFizz<IFizz<string, Wrap<double>>, bool>).GetGenericArguments(), openBaseTypeArgs));
        }

        [Test]
        public void Should_find_mismatch_between_closed_and_base_generic_args_With_closed_generic_arg_in_the_middle()
        {
            var openImplType = typeof(BuzzWrapInt<,>);
            var baseTypeDef = typeof(IFizz<,>);
            var baseTypes = openImplType.GetImplementedTypes(TypeTools.ReturnBaseOpenGenerics.AsIs, TypeTools.IncludeSelf.Exclude);
            var openBaseType = Array.Find(baseTypes, type => type.GetGenericTypeDefinition() == baseTypeDef);
            var openBaseTypeArgs = openBaseType.GetGenericArguments();

            Assert.IsTrue(TypeTools.MatchClosedGenericWithBaseOpenGenericTypeArgs(typeof(IFizz<IFizz<string, Wrap<int>>, bool>).GetGenericArguments(), openBaseTypeArgs));
            Assert.IsFalse(TypeTools.MatchClosedGenericWithBaseOpenGenericTypeArgs(typeof(IFizz<IFizz<string, Wrap<double>>, bool>).GetGenericArguments(), openBaseTypeArgs));
        }

        [Test]
        public void Should_find_mismatch_between_closed_and_base_generic_args_With_different_generic_type_arg()
        {
            var openImplType = typeof(BuzzDiffArg<,>);
            var baseTypeDef = typeof(IFizz<,>);
            var baseTypes = openImplType.GetImplementedTypes(TypeTools.ReturnBaseOpenGenerics.AsIs, TypeTools.IncludeSelf.Exclude);
            var openBaseType = Array.Find(baseTypes, type => type.GetGenericTypeDefinition() == baseTypeDef);
            var openBaseTypeArgs = openBaseType.GetGenericArguments();

            Assert.IsTrue(TypeTools.MatchClosedGenericWithBaseOpenGenericTypeArgs(typeof(IFizz<Wrap<string>, bool>).GetGenericArguments(), openBaseTypeArgs));
            Assert.IsFalse(TypeTools.MatchClosedGenericWithBaseOpenGenericTypeArgs(typeof(IFizz<DifferentWrap<string>, bool>).GetGenericArguments(), openBaseTypeArgs));
        }

        [Test]
        public void Should_find_mismatch_between_closed_and_base_generic_args_With_different_arguments_number()
        {
            var openImplType = typeof(BuzzDiffArgCount<,>);
            var baseTypeDef = typeof(IFizz<,>);
            var baseTypes = openImplType.GetImplementedTypes(TypeTools.ReturnBaseOpenGenerics.AsIs, TypeTools.IncludeSelf.Exclude);
            var openBaseType = Array.Find(baseTypes, type => type.GetGenericTypeDefinition() == baseTypeDef);
            var openBaseTypeArgs = openBaseType.GetGenericArguments();

            Assert.IsTrue(TypeTools.MatchClosedGenericWithBaseOpenGenericTypeArgs(typeof(IFizz<Wrap<string>, bool>).GetGenericArguments(), openBaseTypeArgs));
            Assert.IsFalse(TypeTools.MatchClosedGenericWithBaseOpenGenericTypeArgs(typeof(IFizz<Wrap<string, double>, bool>).GetGenericArguments(), openBaseTypeArgs));
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

    public class BuzzInt<T1, T2> : IFizz<IFizz<T1, string>, T2> { }

    public class BuzzWrapInt<T1, T2> : IFizz<IFizz<T1, Wrap<int>>, T2> { }

    public class BuzzDiffArg<T1, T2> : IFizz<Wrap<T2>, T1> { }

    public class BuzzDiffArgCount<T1, T2> : IFizz<Wrap<T2>, T1> { }

    public class Wrap<T> { }
    public class Wrap<T1, T2> { }
    public class DifferentWrap<T> { }

    public class BuzzWithConstaints<T> : IBuzzWithConstraint<T> where T : IFizz<int, string> {}

    public interface IBuzzWithConstraint<T> where T : IFizz<int, string> {}

    #endregion
}
