using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class TypeCSharpNameFormattingTests : ITest
    {
        public int Run()
        {
            GetCSharpTypeName_should_return_correct_open_generic_type_name();
            GetCSharpTypeName_should_return_correct_closed_generic_type_name();
            GetCSharpTypeName_for_array_of_open_generics();
            return 3;
        }

        [Test]
        public void GetCSharpTypeName_should_return_correct_open_generic_type_name()
        {
            var name = typeof(OpenGenericServiceWithTwoParameters<,>).Print();

#if DEBUG
            Assert.That(name, Is.EqualTo("OpenGenericServiceWithTwoParameters<, >"));
#else
            Assert.That(name, Is.EqualTo("DryIoc.UnitTests.OpenGenericServiceWithTwoParameters<, >"));
#endif
        }

        [Test]
        public void GetCSharpTypeName_should_return_correct_closed_generic_type_name()
        {
            var result = typeof(
                    OpenGenericServiceWithTwoParameters<
                        Lazy<IEnumerable<bool>>,
                        Func<bool, TypeCSharpNameFormattingTests, string>>)
                .Print();

            var expected =
#if DEBUG
                "OpenGenericServiceWithTwoParameters<Lazy<IEnumerable<bool>>, Func<bool, TypeCSharpNameFormattingTests, string>>";
#else
                "DryIoc.UnitTests.OpenGenericServiceWithTwoParameters<" +
                    "Lazy<IEnumerable<bool>>, " +
                    "Func<bool, DryIoc.UnitTests.TypeCSharpNameFormattingTests, string>" +
                    ">";
#endif
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetCSharpTypeName_for_array_of_open_generics()
        {
            var result = typeof(OpenGenericServiceWithTwoParameters<int, bool>[]).Print();
            var expected =
#if DEBUG
                "OpenGenericServiceWithTwoParameters<int, bool>[]";
#else
                "DryIoc.UnitTests.OpenGenericServiceWithTwoParameters<int, bool>[]";
#endif
            Assert.AreEqual(expected, result);
        }
    }

    public class OpenGenericServiceWithTwoParameters<T1, T2>
    {
        public T1 Value1 { get; set; }
        public T2 Value2 { get; set; }
    }
}
