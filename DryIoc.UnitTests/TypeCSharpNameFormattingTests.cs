using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
	[TestFixture]
	public class TypeCSharpNameFormattingTests
	{
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
			var name = new StringBuilder().Print(typeof(
                OpenGenericServiceWithTwoParameters<
                    Lazy<IEnumerable<bool>>, 
                    Func<bool, TypeCSharpNameFormattingTests, string>>))
                .ToString();

            Assert.That(name, Is.EqualTo(
                "DryIoc.UnitTests.OpenGenericServiceWithTwoParameters<" + 
                    "System.Lazy<System.Collections.Generic.IEnumerable<System.Boolean>>, " + 
                    "System.Func<System.Boolean, DryIoc.UnitTests.TypeCSharpNameFormattingTests, System.String>" + 
                    ">"));
		}
	}

    #region CUT

    public class OpenGenericServiceWithTwoParameters<T1, T2>
    {
        public T1 Value1 { get; set; }
        public T2 Value2 { get; set; }
    }

    #endregion
}