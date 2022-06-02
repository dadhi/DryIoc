using System;
using System.Collections.Generic;
using System.Text;
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
            var name = new StringBuilder().Print(typeof(
                OpenGenericServiceWithTwoParameters<,>), t => t.Name).ToString();

            Assert.That(name, Is.EqualTo("OpenGenericServiceWithTwoParameters<,>"));
        }

        [Test]
        public void GetCSharpTypeName_should_return_correct_closed_generic_type_name()
        {
            var result = new StringBuilder().Print(typeof(
                    OpenGenericServiceWithTwoParameters<
                        Lazy<IEnumerable<bool>>,
                        Func<bool, TypeCSharpNameFormattingTests, string>>))
                .ToString();

            var expected =
#if DEBUG
                "OpenGenericServiceWithTwoParameters<Lazy<IEnumerable<Boolean>>, Func<Boolean, TypeCSharpNameFormattingTests, String>>";
#else
                "DryIoc.UnitTests.OpenGenericServiceWithTwoParameters<" +
                    "Lazy<IEnumerable<Boolean>>, " +
                    "Func<Boolean, DryIoc.UnitTests.TypeCSharpNameFormattingTests, String>" +
                    ">";
#endif
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetCSharpTypeName_for_array_of_open_generics()
        {
            var result = new StringBuilder().Print(typeof(OpenGenericServiceWithTwoParameters<int, bool>[])).ToString();
            var expected =
#if DEBUG
                "OpenGenericServiceWithTwoParameters<Int32, Boolean>[]";
#else
                "DryIoc.UnitTests.OpenGenericServiceWithTwoParameters<Int32, Boolean>[]";
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
