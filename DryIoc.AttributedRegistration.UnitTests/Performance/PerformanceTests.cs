using System;
using System.Diagnostics;
using DryIoc.AttributedRegistration.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.AttributedRegistration.UnitTests.Performance
{
	[TestFixture][Ignore]
	public class PerformanceTests
	{
		[Test]
		public void RegistrationPerformanceTest()
		{
			var currentAssembly = typeof(TransientService).Assembly;

			const int times = 1000;

			var stopwatch = Stopwatch.StartNew();

			for (int i = 0; i < times; i++)
			{
				var container = new Container();
				container.RegisterExported(currentAssembly);
			}

			stopwatch.Stop();

            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThanOrEqualTo(3000));
		}

		[Test]
		public void AttributeIsDefinedVsGetCustomAttributes()
		{
			const int times = 100000;
			var testType = typeof(TransientService);
			var attrType = typeof(ExportAttribute);

			var attributeIsDefined = Stopwatch.StartNew();
			for (int i = 0; i < times; i++)
			{
				Attribute.IsDefined(testType, attrType, false);
				//Attribute.GetCustomAttribute(testType, attrType, false);
			}
			attributeIsDefined.Stop();

			var getCustomAttributes = Stopwatch.StartNew();
			for (int i = 0; i < times; i++)
			{
				testType.GetCustomAttributes(attrType, false);
			}

			getCustomAttributes.Stop();

            Assert.That(attributeIsDefined.ElapsedMilliseconds, Is.LessThanOrEqualTo(getCustomAttributes.ElapsedMilliseconds));
		}
	}
}