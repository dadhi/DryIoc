using System.Linq;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class SugarTests
    {
        [Test]
        public void GetSelfAndAllInheritedTypes_should_work_for_open_generic_types()
        {
            var types = typeof(Fuzz<>).GetSelfAndImplemented().ToArray();

            CollectionAssert.AreEqual(new[] { typeof(Fuzz<>), typeof(IBuzz), typeof(IFuzz<>), typeof(IFuzz), typeof(Buzz) }, types);
        }

        [Test]
        public void GetSelfAndAllInheritedTypes_should_work_for_class_nested_in_open_generic_types()
        {
            var types = typeof(Fuzz<>.NestedClazz).GetSelfAndImplemented().ToArray();

            CollectionAssert.AreEqual(new[] { typeof(Fuzz<>.NestedClazz), typeof(IFuzz<>), typeof(IFuzz) }, types);
        }

        [Test]
        public void Should_return_A_implementors()
        {
            var types = typeof(A).GetSelfAndImplemented();

            Assert.That(types, Is.EqualTo(new[] { typeof(A), typeof(IB), typeof(IA), typeof(B), typeof(C) }));
        }

        [Test]
        public void Should_return_B_implementors()
        {
            var types = typeof(B).GetSelfAndImplemented();

            Assert.That(types, Is.EqualTo(new[] { typeof(B), typeof(IB), typeof(C) }));
        }
    }

    public class C { }

    public interface IB { }

    public class B : C, IB { }

    public interface IA { }

    public class A : B, IA { }

    class Fuzz<T> : Buzz, IFuzz<T>
    {
        public class NestedClazz : IFuzz<T>
        {
        }
    }

    internal class Buzz : IBuzz
    {
    }

    interface IFuzz<T> : IFuzz
    {
    }

    interface IFuzz { }

    interface IBuzz { }
}
