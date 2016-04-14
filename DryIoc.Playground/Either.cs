using System;
using System.Reflection;
using NUnit.Framework;

namespace Playground
{
    [TestFixture]
    public class EitherTests
    {
        [Test]
        public void Test_Either_of_Property_or_FieldInfo()
        {
            var some = new Some(false);
            var field = some.GetType().GetField("X");
            var property = some.GetType().GetProperty("Y");
            Assert.NotNull(field);
            Assert.NotNull(property);

            Either<PropertyInfo, FieldInfo> info = field;
            var infoType = info.Is(p => p.PropertyType, f => f.FieldType);

            Assert.That(infoType, Is.EqualTo(typeof(bool)));
        }

        [Test]
        public void Either_of_three_cases_using_nesting()
        {
            var some = new Some(false);
            var field = some.GetType().GetField("X");
            var parameter = some.GetType().GetConstructors()[0].GetParameters()[0];
            Assert.NotNull(field);
            Assert.NotNull(parameter);

            var info = Either<ParameterInfo, Either<PropertyInfo, FieldInfo>>.Of(parameter);
            var name = info.Is(_ => _.Name, _ => _.Name, _ => _.Name);

            Assert.AreEqual("a", name);
        }

        public class Some
        {
            public bool X;
            public string Y { get; set; }

            public Some(bool a)
            {
                X = a;
            }
        }
    }


    public static class Either
    {
        public static T Is<A, B, C, T>(
            this Either<A, Either<B, C>> source,
            Func<A, T> a = null, Func<B, T> b = null, Func<C, T> c = null)
        {
            return source.Is(a, bc => bc.Is(b, c));
        }
    }

    public abstract class Either<A, B>
    {
        public static implicit operator Either<A, B>(A a)
        {
            return Of(a);
        }

        public static implicit operator Either<A, B>(B b)
        {
            return Of(b);
        }

        public static Either<A, B> Of(A a)
        {
            return new CaseA(a);
        }

        public static Either<A, B> Of(B b)
        {
            return new CaseB(b);
        }

        public abstract T Is<T>(Func<A, T> a = null, Func<B, T> b = null);

        private sealed class CaseA : Either<A, B>
        {
            private readonly A _item;
            public CaseA(A item) { _item = item; }

            public override T Is<T>(Func<A, T> a = null, Func<B, T> b = null)
            {
                return a == null ? default(T) : a(_item);
            }
        }

        private sealed class CaseB : Either<A, B>
        {
            private readonly B _item;
            public CaseB(B item) { _item = item; }

            public override T Is<T>(Func<A, T> a = null, Func<B, T> b = null)
            {
                return b == null ? default(T) : b(_item);
            }
        }
    }
}
