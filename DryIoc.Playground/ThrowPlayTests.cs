using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace DryIoc.Playground
{
	[TestFixture]
	public class ThrowPlayTests
	{
		[Test]
		[Ignore]
		public void Check_that_empty_params_array_is_NOT_the_same_in_multiple_calls()
		{
			Assert.That(GetEmptyParams(), Is.Not.SameAs(GetEmptyParams()));
		}

		public object[] GetEmptyParams(params object[] ps)
		{
			return ps;
		}

		[Test]
		[Ignore]
		public void Check_that_more_args_then_stated_in_Format_wont_throw()
		{
			Assert.DoesNotThrow(() => 
                string.Format("{0}", new object[] { 1, null, null }));
		}

		[Test]
		[Ignore]
		public void Check_that_params_array_created_if_omitted_parameters()
		{
			var time = TestDefaultParamsTime();
			Assert.That(time.Key, Is.GreaterThanOrEqualTo(time.Value));
		}

		public static KeyValuePair<long, long> TestDefaultParamsTime()
		{
			var a = 0;
			string arg = "" + a;
			var times = 1000 * 1000;

			var watch = Stopwatch.StartNew();

			for (int i = 0; i < times; i++)
			{
				Throw.ThrowIfNull(arg, "arg", "message");
			}

			var paramsTime = watch.ElapsedMilliseconds;
			watch.Reset();

			watch.Start();

			for (int i = 0; i < times; i++)
			{
				TestThrow.IfNullArg(arg, "arg");
			}

			var noParamsTime = watch.ElapsedMilliseconds;
			watch.Stop();

            return new KeyValuePair<long, long>(paramsTime, noParamsTime);
		}

        public static KeyValuePair<long, long> TestParamsTime()
		{
			var a = 0;
			var b = "" + a;
			var times = 1000 * 1000;

			var watch = Stopwatch.StartNew();

			for (int i = 0; i < times; i++)
			{
				TestThrow.IfNullArg(b, "b", "blah", null);
			}

			var paramsTime = watch.ElapsedMilliseconds;
			watch.Reset();

			watch.Start();

			for (int i = 0; i < times; i++)
			{
				TestThrow.IfNullArg(b, "b");
			}

			var noParamsTime = watch.ElapsedMilliseconds;
			watch.Stop();

            return new KeyValuePair<long, long>(paramsTime, noParamsTime);
		}
	}

	public static class TestThrow
	{
		public static T IfNullArg<T>(T value, string argName, string format, params object[] args) where T : class
		{
			if (value != null) return value;
			throw new ArgumentNullException(argName, String.Format(format, args));
		}

		public static T IfNullArg<T>(T value, string argName) where T : class
		{
			if (value != null) return value;
			throw new ArgumentNullException(argName);
		}
	}
}