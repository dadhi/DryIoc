using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests.Playground
{
	[TestFixture]
    [Ignore]
	public class Experiments
	{
		[Test]
		public void Can_add_to_null_delegate()
		{
			// Arrange
			Func<int> f = null;
			f += () => 2;
			f += () => 1;

			// Act
			var x = f();

			Assert.That(x, Is.EqualTo(1));
		}

		[Test]
		public void Test_IsAssignableFrom_for_open_n_closed_generics()
		{
			var openGenericIface = typeof(IService<>);
			var closedGenericIface = typeof(IService<int>);

			var openGeneric = typeof(Service<>);
			var closedGeneric = typeof(Service<int>);

			Assert.IsFalse(closedGenericIface.IsAssignableFrom(openGeneric));
			Assert.IsFalse(openGenericIface.IsAssignableFrom(closedGeneric));
			Assert.IsFalse(openGenericIface.IsAssignableFrom(openGeneric));
			Assert.IsTrue(openGenericIface.IsAbstract);
		}

		[Test]
		public void Test_IsAssignableFromGenericType()
		{
			var iface = typeof(IFace<>);
			var myclass = typeof(MyClass<>);

			Assert.IsFalse(typeof(IFace<>).IsAssignableFrom(typeof(MyClass<int>)));
			var test = IsAssignableFromGenericType(iface, myclass);
			Assert.IsFalse(test);
		}

		[Test]
		public void TestGetPublicBaseClassesAndInterfaces()
		{
			var types = GetBaseClassesAndInterfaces(typeof(Base<int>)).ToArray();
			CollectionAssert.AreEqual(new[] { typeof(IFace<int>), typeof(IFace), typeof(IDisposable) }, types);

			types = GetBaseClassesAndInterfaces(typeof(MyClass<int>)).ToArray();
			CollectionAssert.AreEqual(new[] { typeof(Base<int>), typeof(IFace<int>), typeof(IFace), typeof(IDisposable) }, types);
		}

		public static IEnumerable<Type> GetBaseClassesAndInterfaces(Type type)
		{
			return type.BaseType == typeof(object)
				? type.GetInterfaces()
				: Enumerable
					.Repeat(type.BaseType, 1)
					.Concat(type.GetInterfaces())
					.Concat(GetBaseClassesAndInterfaces(type.BaseType))
					.Distinct();
		}

		public static IEnumerable<Type> GetSelfAndInherited(Type type)
		{
			var selfWithIfaces = Enumerable.Repeat(type, 1).Concat(type.GetInterfaces());
			return type.BaseType == typeof(object) ? selfWithIfaces
				: selfWithIfaces.Concat(GetSelfAndInherited(type.BaseType)).Distinct();
		}

		public static bool IsAssignableFromGenericType(Type source, Type target)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (target == null) throw new ArgumentNullException("target");

			if (!target.IsGenericTypeDefinition)
				return source.IsAssignableFrom(target);

			source = source.IsGenericType ? source.GetGenericTypeDefinition() : source;
			if (source == target)
				return true;

			var interfaces = target.GetInterfaces();
			if (interfaces.Any(type => source == type))
				return true;

			var targetBase = target.BaseType;
			if (targetBase != null)
				return IsAssignableFromGenericType(source, targetBase);

			return false;
		}

		public interface IFace<T> : IFace { }

		public interface IFace : IDisposable { }

		public class Base<T> : IFace<T>
		{
			public void Dispose()
			{
				throw new NotImplementedException();
			}
		}

		public class MyClass<T> : Base<T>, IFace<T>, IFace { }

		[Test]
		[Ignore]
		public void Func_vs_Expr_vs_Delegate()
		{
			const int times = 1000 * 1000;

			var funcTime = Measure(times, () => new Fuh(new Bar()));
			var delgTime = Measure(times, GetCtorDelg());
			var bodyTime = Measure(times, GetExprBody());
			var exprTime = Measure(times, GetExpr());

			Console.WriteLine("Func: " + funcTime);
			Console.WriteLine("Delg: " + delgTime);
			Console.WriteLine("Expr: " + exprTime);
			Console.WriteLine("Body: " + bodyTime);

			Assert.GreaterOrEqual(funcTime, delgTime);
			Assert.GreaterOrEqual(exprTime, bodyTime);
		}

		[Test]
		public void CreateInstanceWithDelegate()
		{
			var barCtor = typeof(Bar).GetConstructors()[0];
			var ctor = GetCtor<Func<object>>(barCtor);
			var bar = ctor();
			Assert.IsInstanceOf<Bar>(bar);
		}

		[Test]
		public void CreateInstanceWithDelegateWithParams()
		{
			var fuhCtor = typeof(Fuh).GetConstructors()[0];
			var ctor = GetCtor<Func<IBar, object>>(fuhCtor);
			var fuh = ctor(new Bar());
			Assert.IsInstanceOf<Fuh>(fuh);
		}

		private Func<object> GetExprBody()
		{
			var fuhCtor = typeof(Fuh).GetConstructors()[0];
			var barCtor = typeof(Bar).GetConstructors()[0];

			var barNew = Expression.New(barCtor, null);
			var barFac = Expression.Lambda(barNew, null);

			var fuhNew = Expression.New(fuhCtor, barFac.Body);
			var fuhFac = Expression.Lambda(fuhNew, null);

			var facExpr = Expression.Lambda<Func<object>>(fuhFac.Body, null);
			return facExpr.Compile();
		}

		private Func<object> GetCtorDelg()
		{
			var fuhCtor = typeof(Fuh).GetConstructors()[0];
			var barCtor = typeof(Bar).GetConstructors()[0];

			var barFunc = GetCtor<Func<object>>(barCtor);
			var fuhFunc = GetCtor<Func<IBar, object>>(fuhCtor);

			return () => fuhFunc((IBar)barFunc());
		}

		public static TDelegate GetCtor<TDelegate>(ConstructorInfo constructor)
		{
			if (constructor == null)
				throw new ArgumentNullException("constructor");

			var delegateType = typeof(TDelegate);

			// Validate the delegate return type
			var delegateInvoke = delegateType.GetMethod("Invoke");
			if (!delegateInvoke.ReturnType.IsAssignableFrom(constructor.DeclaringType))
				throw new InvalidOperationException("The return type of the delegate must match the constructors declaring type");

			// Validate the signatures
			var delegateArgs = delegateInvoke.GetParameters();
			var constructorArgs = constructor.GetParameters();
			if (delegateArgs.Length != constructorArgs.Length)
				throw new InvalidOperationException("The delegate signature does not match that of the constructor");

			for (var i = 0; i < delegateArgs.Length; i++)
			{
				if (delegateArgs[i].ParameterType != constructorArgs[i].ParameterType ||
					delegateArgs[i].IsOut)
					throw new InvalidOperationException("The delegate signature does not match that of the constructor");
			}

			// Create the dynamic method
			var constructorArgTypes = Array.ConvertAll(constructorArgs, p => p.ParameterType);
			var dynamicMethod = new DynamicMethod("CreateInstance", constructor.DeclaringType, constructorArgTypes, true);

			var il = dynamicMethod.GetILGenerator();
			for (var i = 0; i < constructorArgs.Length; i++)
				il.Emit(OpCodes.Ldarg, i);

			il.Emit(OpCodes.Newobj, constructor);
			il.Emit(OpCodes.Ret);

			return (TDelegate)(object)dynamicMethod.CreateDelegate(delegateType);
		}

		private Func<object> GetExpr()
		{
			var fooCtor = typeof(Fuh).GetConstructors()[0];
			var barCtor = typeof(Bar).GetConstructors()[0];

			var barNew = Expression.New(barCtor, null);
			var fooNew = Expression.New(fooCtor, barNew);

			var facExpr = Expression.Lambda<Func<object>>(fooNew, null);
			return facExpr.Compile();
		}

		private long Measure(int times, Func<object> factory)
		{
			var stopwatch = Stopwatch.StartNew();
			object ignored = null;
			for (int i = 0; i < times; i++)
			{
				ignored = factory();
			}
			stopwatch.Stop();
			return stopwatch.ElapsedMilliseconds;
		}
	}

	class Fuh : IFuh
	{
		public IBar Bar { get; set; }

		public Fuh(IBar bar)
		{
			Bar = bar;
		}
	}

	internal interface IFuh
	{
		IBar Bar { get; set; }
	}

	class Bar : IBar
	{

	}

	public interface IBar
	{
	}
}