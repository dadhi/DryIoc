using System;
using System.Collections.Generic;
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

        [Test]
        public void Should_find_mismatch_between_closed_and_base_generic_args()
        {
            Assert.IsFalse(MatchOpenImplWithClosedBaseTypeArgs(typeof(Buzz<,>), typeof(IFizz<int, string>)));
            Assert.IsFalse(MatchOpenImplWithClosedBaseTypeArgs(typeof(Buzz<,>), typeof(IFizz<int, Wrap<string>>)));
            Assert.IsFalse(MatchOpenImplWithClosedBaseTypeArgs(typeof(Buzz<,>), typeof(IFizz<int, Wrap<IFizz<string, int>>>)));
            Assert.IsTrue(MatchOpenImplWithClosedBaseTypeArgs(typeof(Buzz<,>), typeof(IFizz<int, Wrap<IFizz<Wrap<string>, int>>>)));
        }

        [Test]
        public void Should_find_mismatch_between_closed_and_base_generic_args_With_multiple_different_closed_types_matched_to_single_open_arg()
        {
            // both INT and BOOL are matched with T2.
            Assert.IsFalse(MatchOpenImplWithClosedBaseTypeArgs(typeof(Buzz<,>), typeof(IFizz<int, Wrap<IFizz<Wrap<string>, bool>>>)));
        }

        [Test]
        public void Should_find_mismatch_between_closed_and_base_generic_args_With_closed_arg_in_the_middle()
        {
            Assert.IsTrue(MatchOpenImplWithClosedBaseTypeArgs(typeof(BuzzInt<,>), typeof(IFizz<IFizz<string, string>, bool>)));
            Assert.IsFalse(MatchOpenImplWithClosedBaseTypeArgs(typeof(BuzzInt<,>), typeof(IFizz<IFizz<string, Wrap<double>>, bool>)));
        }

        [Test]
        public void Should_find_mismatch_between_closed_and_base_generic_args_With_closed_generic_arg_in_the_middle()
        {
            Assert.IsTrue(MatchOpenImplWithClosedBaseTypeArgs(typeof(BuzzWrapInt<,>), typeof(IFizz<IFizz<string, Wrap<int>>, bool>)));
            Assert.IsFalse(MatchOpenImplWithClosedBaseTypeArgs(typeof(BuzzWrapInt<,>), typeof(IFizz<IFizz<string, Wrap<double>>, bool>)));
        }

        [Test]
        public void Should_find_mismatch_between_closed_and_base_generic_args_With_different_generic_type_arg()
        {
            Assert.IsTrue(MatchOpenImplWithClosedBaseTypeArgs(typeof(BuzzDiffArg<,>), typeof(IFizz<Wrap<string>, bool>)));
            Assert.IsFalse(MatchOpenImplWithClosedBaseTypeArgs(typeof(BuzzDiffArg<,>), typeof(IFizz<DifferentWrap<string>, bool>)));
        }

        [Test]
        public void Should_find_mismatch_between_closed_and_base_generic_args_With_different_arguments_number()
        {
            Assert.IsTrue(MatchOpenImplWithClosedBaseTypeArgs(typeof(BuzzDiffArgCount<,>), typeof(IFizz<Wrap<string>, bool>)));
            Assert.IsFalse(MatchOpenImplWithClosedBaseTypeArgs(typeof(BuzzDiffArgCount<,>), typeof(IFizz<Wrap<string, double>, bool>)));
        }

        private static bool MatchOpenImplWithClosedBaseTypeArgs(Type openImplType, Type closedBaseType)
        {
            var baseTypeDefinition = closedBaseType.GetGenericTypeDefinition();
            var baseTypes = openImplType.GetImplementedTypes();
            var openBaseType = Array.Find(baseTypes, t => t.ContainsGenericParameters && t.GetGenericTypeDefinition() == baseTypeDefinition);
            var openBaseTypeArgs = openBaseType.GetGenericArguments();

            IDictionary<string, Type> ignored = new Dictionary<string, Type>();
            return TypeTools.MatchBaseOpenWithClosedGenericTypeArgs(openBaseTypeArgs, closedBaseType.GetGenericArguments(), ref ignored);
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

    public class BuzzWithConstaints<T> : IBuzzWithConstraint<T> where T : IFizz<int, string> { }

    public interface IBuzzWithConstraint<T> where T : IFizz<int, string> { }

    #endregion
}
