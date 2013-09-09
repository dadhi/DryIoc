using System;
using System.Diagnostics;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests.AttributedRegistration
{
	[TestFixture]
	public class PerformanceTests
	{
		[Test]
		[Ignore]
		public void RegistrationPerformanceTest()
		{
			var currentAssembly = typeof(FooConsumer).Assembly;

			const int times = 1000;

			var stopwatch = Stopwatch.StartNew();

			for (int i = 0; i < times; i++)
			{
				var container = new Container();
				container.ScanAndRegisterExports(currentAssembly);
			}

			stopwatch.Stop();

            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThanOrEqualTo(1000));
		}

		[Test]
		[Ignore]
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