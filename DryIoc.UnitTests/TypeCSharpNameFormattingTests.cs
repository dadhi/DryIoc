using System;
using System.Collections.Generic;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
	[TestFixture]
	public class TypeCSharpNameFormattingTests
	{
		[Test]
		public void GetCSharpTypeName_should_return_correct_open_generic_type_name()
		{
			var name = typeof(OpenGenericServiceWithTwoParameters<,>).Print(t => t.Name);

            Assert.That(name, Is.EqualTo("OpenGenericServiceWithTwoParameters<,>"));
		}

		[Test]
		public void GetCSharpTypeName_should_return_correct_closed_generic_type_name()
		{
			var name = typeof(OpenGenericServiceWithTwoParameters<Lazy<IEnumerable<bool>>, Func<bool, TypeCSharpNameFormattingTests, string>>).Print();

            Assert.That(name, Is.EqualTo(
                "DryIoc.UnitTests.CUT.OpenGenericServiceWithTwoParameters<" + 
                    "DryIoc.Lazy<System.Collections.Generic.IEnumerable<System.Boolean>>, " + 
                    "System.Func<System.Boolean, DryIoc.UnitTests.TypeCSharpNameFormattingTests, System.String>" + 
                    ">"));
		}
	}
}