using System;
using System.Diagnostics;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests.Performance
{
	[TestFixture]
	public class ReflectionPerformanceTests
	{
		[Test]
		[Ignore]
		public void Compare_speed_of_IsGenericType_vs_stored_value_access()
		{
			const int times = 1000 * 1000;

			var testedType = typeof(Service<int>);
			var result = false;
			var timer = Stopwatch.StartNew();
			for (var i = 0; i < times; i++)
			{
				result = testedType.IsGenericType && !testedType.IsGenericTypeDefinition;
			}
			timer.Stop();
			var callingEachTime = timer.ElapsedMilliseconds;

			StoredFlag = result;
			timer = Stopwatch.StartNew();
			for (var i = 0; i < times; i++)
			{
				result = StoredFlag;
			}
			timer.Stop();
			var callingProperty = timer.ElapsedMilliseconds;

            Assert.That(result, Is.True);
            Assert.That(callingProperty * 10, Is.GreaterThan(callingEachTime));
		}

		[Test]
		[Ignore]
		public void Compare_speed_of_GetGenericTypeDefinition_vs_stored_value_access()
		{
			const int times = 1000 * 1000;

			var testedType = typeof(Service<int>);
			var result = default(Type);
			var timer = Stopwatch.StartNew();
			for (var i = 0; i < times; i++)
			{
				result = testedType.GetGenericTypeDefinition();
			}
			timer.Stop();
			var callingEachTime = timer.ElapsedMilliseconds;

			StoredType = result;
			timer = Stopwatch.StartNew();
			for (var i = 0; i < times; i++)
			{
				result = StoredType;
			}
			timer.Stop();
			var accessingStoredValue = timer.ElapsedMilliseconds;

            Assert.That(result, Is.InstanceOf(typeof(Service<>)));
            Assert.That(accessingStoredValue * 25, Is.GreaterThan(callingEachTime));
		}

		[Test]
		[Ignore]
		public void Compare_speed_of_GetGenericArguments_vs_stored_value_access()
		{
			const int times = 1000 * 1000;

			var testedType = typeof(Service<int>);
			var result = default(Type[]);
			var timer = Stopwatch.StartNew();
			for (var i = 0; i < times; i++)
			{
				result = testedType.GetGenericArguments();
			}
			timer.Stop();
			var callingEachTime = timer.ElapsedMilliseconds;

			StoredArgs = result;
			timer = Stopwatch.StartNew();
			for (var i = 0; i < times; i++)
			{
				result = StoredArgs;
			}
			timer.Stop();
			var accessingStoredValue = timer.ElapsedMilliseconds;
			
            Assert.That(result, Is.Not.Null);
            Assert.That(accessingStoredValue * 25, Is.GreaterThan(callingEachTime));
		}

		public bool StoredFlag { get; set; }
		public Type StoredType { get; set; }
		public Type[] StoredArgs { get; set; }
	}
}
