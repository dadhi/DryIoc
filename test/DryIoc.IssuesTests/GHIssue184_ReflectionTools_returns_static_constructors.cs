using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DryIoc.IssuesTests
{
    public class GHIssue184_ReflectionTools_returns_static_constructors
    {
        [Test]
        [TestCaseSource(nameof(SingleConstructors))]
        public void GetConstructorOrNull_DoesNotReturnStaticConstructor(Type type)
        {
            Assert.IsNull(
                type.GetConstructorOrNull());
        }

        [Test]
        [TestCaseSource(nameof(DoubleConstructors))]
        public void GetConstructorOrNull_ReturnsDoubleConstructorWithArguments(Type type)
        {
            Assert.IsInstanceOf<ConstructorInfo>(
                type.GetConstructorOrNull(typeof(object)));
        }

        [Test]
        [TestCaseSource(nameof(DoubleConstructors))]
        public void GetConstructorOrNull_ReturnsDoubleConstructorWithoutArguments(Type type)
        {
            var ctor = type.GetConstructorOrNull();

            Assert.IsInstanceOf<ConstructorInfo>(ctor);
            Assert.IsFalse(ctor.IsStatic);
        }

        [Test]
        [TestCaseSource(nameof(SingleConstructors))]
        public void GetConstructorOrNull_ReturnsSingleConstructorWithArguments(Type type)
        {
            Assert.IsInstanceOf<ConstructorInfo>(
                type.GetConstructorOrNull(typeof(object)));
        }

        [Test]
        [TestCaseSource(nameof(SingleConstructors))]
        public void GetSingleConstructorOrNull_ReturnsSingleConstructor(Type type)
        {
            Assert.IsInstanceOf<ConstructorInfo>(
                type.GetSingleConstructorOrNull());
        }

        [Test]
        [TestCaseSource(nameof(DoubleConstructors))]
        public void GetSingleConstructorOrNull_ReturnsNullForDoubleConstructor(Type type)
        {
            Assert.IsNull(
                type.GetSingleConstructorOrNull());
        }

        [Test]
        [TestCaseSource(nameof(SingleConstructors))]
        public void SingleConstructor_DoesNotThrowForStaticConstructor(Type type)
        {
            Assert.IsInstanceOf<ConstructorInfo>(
                type.SingleConstructor());
        }

        [Test]
        [TestCaseSource(nameof(DoubleConstructors))]
        public void SingleConstructor_ThrowsForDoubleConstructor(Type type)
        {
            Assert.Throws<ContainerException>(
                () => type.SingleConstructor());
        }

        private static readonly object[] SingleConstructors = new[]
        {
            typeof(SingleConstructorWithStaticConstructor1),
            typeof(SingleConstructorWithStaticConstructor2),
        };

        private static readonly object[] DoubleConstructors = new[]
        {
            typeof(DoubleConstructorWithStaticConstructor1),
            typeof(DoubleConstructorWithStaticConstructor2),
        };

        private class SingleConstructorWithStaticConstructor1
        {
            static SingleConstructorWithStaticConstructor1() { }
            public SingleConstructorWithStaticConstructor1(object param) { }
        }

        private class SingleConstructorWithStaticConstructor2
        {
            public SingleConstructorWithStaticConstructor2(object param) { }
            static SingleConstructorWithStaticConstructor2() { }
        }

        private class DoubleConstructorWithStaticConstructor1
        {
            static DoubleConstructorWithStaticConstructor1() { }
            public DoubleConstructorWithStaticConstructor1() { }
            public DoubleConstructorWithStaticConstructor1(object param) { }
        }

        private class DoubleConstructorWithStaticConstructor2
        {
            public DoubleConstructorWithStaticConstructor2() { }
            public DoubleConstructorWithStaticConstructor2(object param) { }
            static DoubleConstructorWithStaticConstructor2() { }
        }
    }
}
