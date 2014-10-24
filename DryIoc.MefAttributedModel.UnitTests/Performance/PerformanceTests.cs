using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests.Performance
{
	[TestFixture][Explicit("Not really a unit test")]
	public class PerformanceTests
	{
		[Test]
		public void RegistrationPerformanceTest()
		{
			var currentAssembly = typeof(TransientService).GetAssembly();

			const int times = 1000;

			var stopwatch = Stopwatch.StartNew();

			for (int i = 0; i < times; i++)
			{
				var container = new Container();
				container.RegisterExports(currentAssembly);
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
				testType.GetAttributes(attrType);
			}

			getCustomAttributes.Stop();

            Assert.That(attributeIsDefined.ElapsedMilliseconds, Is.LessThanOrEqualTo(getCustomAttributes.ElapsedMilliseconds));
		}
	}
}