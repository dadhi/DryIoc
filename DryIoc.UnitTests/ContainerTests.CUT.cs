using System;
using System.Collections.Generic;

namespace DryIoc.UnitTests.CUT
{
	public interface IService { }

	public interface IDependency { }

	public class ServiceWithInstanceCount : IService
	{
	    public static int InstanceCount { get; set; }

		public ServiceWithInstanceCount()
		{
			++InstanceCount;
		}
	}

    public class ServiceWithInstanceCountWithStringParam : IService
    {
        public static int InstanceCount { get; set; }

        public string Message { get; set; }

        public ServiceWithInstanceCountWithStringParam(string message)
        {
            Message = message;
            ++InstanceCount;
        }
    }

	public class Service : IService { }

	public class AnotherService : IService { }

	public class ServiceWithDependency : IService
	{
	    public IDependency Dependency { get; private set; }

	    public ServiceWithDependency(IDependency dependency)
		{
			Dependency = dependency;
		}
	}

	public class AnotherServiceWithDependency : IService
	{
	    public IDependency Dependency { get; private set; }

	    public AnotherServiceWithDependency(IDependency dependency)
		{
			Dependency = dependency;
		}
	}

	public class ServiceWithSingletonDependency : IService
	{
	    public ISingleton Singleton { get; private set; }

	    public ServiceWithSingletonDependency(ISingleton singleton)
		{
			Singleton = singleton;
		}
	}

	public class ServiceWithEnumerableDependencies : IService
	{
	    public IEnumerable<IDependency> Foos { get; private set; }

	    public ServiceWithEnumerableDependencies(IEnumerable<IDependency> foos)
		{
			Foos = foos;
		}
	}

    public class ServiceWithManyDependencies : IService
    {
        public IEnumerable<IDependency> Foos { get; private set; }

        public ServiceWithManyDependencies(Many<IDependency> foos)
        {
            Foos = foos.Items;
        }
    }

    public class ServiceWithLazyDependency : IService
	{
	    public Lazy<IDependency> LazyOne { get; private set; }

	    public ServiceWithLazyDependency(Lazy<IDependency> lazyOne)
		{
			LazyOne = lazyOne;
		}
	}

    public class AnotherServiceWithLazyDependency : IService
    {
        public Lazy<IDependency> LazyOne { get; private set; }

        public AnotherServiceWithLazyDependency(Lazy<IDependency> lazyOne)
        {
            LazyOne = lazyOne;
        }
    }

    public class ServiceClient
    {
        public IService Service { get; private set; }

        public ServiceClient(IService service)
        {
            Service = service;
        }
    }

    public interface IAbstractService { }

	public abstract class AbstractService : IAbstractService
	{
	}

	public class ServiceWithAbstractBaseClass : AbstractService
	{
	}

	public class Dependency : IDependency
	{
	}

	public class Foo1 : IDependency
	{
	}

	public class Foo2 : IDependency
	{
	}

	public class Foo3 : IDependency
	{
	}

	public interface ISingleton { }

	public class Singleton : ISingleton { }

	public class ServiceWithRecursiveDependency : IService
	{
		public IDependency Dependency { get; private set; }

		public ServiceWithRecursiveDependency(IDependency dependency)
		{
			Dependency = dependency;
		}
	}

	public class ServiceWithFuncOfRecursiveDependency : IService
	{
		public Func<IDependency> FooFactory { get; private set; }

		public ServiceWithFuncOfRecursiveDependency(Func<IDependency> fooFactory)
		{
			FooFactory = fooFactory;
		}
	}

	public class FooWithDependency : IDependency
	{
		public IService Service { get; private set; }

		public FooWithDependency(IService service)
		{
			Service = service;
		}
	}

	public class FooWithFuncOfDependency : IDependency
	{
		public Func<IService> ServiceFactory { get; private set; }

		public FooWithFuncOfDependency(Func<IService> serviceFactory)
		{
			ServiceFactory = serviceFactory;
		}
	}

	public class CloneableClass : ICloneable
	{
		public object Clone()
		{
			throw new NotImplementedException();
		}
	}

    public interface ICloneable {}

    public interface IService<T>
	{
		T Dependency { get; set; }
	}

	public class Service<T> : IService<T>
	{
		public T Dependency { get; set; }
	}

	public class ServiceWithGenericParameter<T> : IService<T>
	{
		public T Dependency { get; set; }
	}

	public class ServiceWithTwoGenericParameters<T1, T2>
	{
		public T1 Value1 { get; set; }
		public T2 Value2 { get; set; }
	}

	public class ServiceWithGenericDependency<T> : IService<T>
	{
	    public T Dependency { get; set; }

	    public ServiceWithGenericDependency(T dependency)
		{
			Dependency = dependency;
		}
	}

	public class ClosedGenericClass : IService<string>
	{
		public string Dependency { get; set; }
	}

	public class ServiceWithTwoParameters
	{
		public Service One { get; private set; }
		public AnotherService Another { get; private set; }

		public ServiceWithTwoParameters(Service one, AnotherService another)
		{
			One = one;
			Another = another;
		}
	}

	public class ServiceWithOnePrimitiveParameter : IService
	{
		public string Message { get; private set; }

		public ServiceWithOnePrimitiveParameter(string message)
		{
			Message = message;
		}
	}

	public class ServiceWithTwoPrimitiveParameters : IService
	{
		public string Message { get; private set; }
		public bool Flag { get; private set; }

		public ServiceWithTwoPrimitiveParameters(bool flag, string message)
		{
			Message = message;
			Flag = flag;
		}
	}

	public class ClientWithFuncAndInstanceDependency
	{
		public Func<string, IService> Factory { get; set; }
		public IService Instance { get; set; }

		public ClientWithFuncAndInstanceDependency(Func<string, IService> factory, IService instance)
		{
			Factory = factory;
			Instance = instance;
		}
	}

	public interface IServiceWithParameterAndDependency
	{
		Service Dependency { get; }
		bool Flag { get; }
	}

	public class ServiceWithParameterAndDependency : IServiceWithParameterAndDependency
	{
		public Service Dependency { get; private set; }
		public bool Flag { get; private set; }

		public ServiceWithParameterAndDependency(Service dependency, bool flag)
		{
			Dependency = dependency;
			Flag = flag;
		}
	}

	public class ServiceWithTwoParametersBothDependentOnSameService
	{
		public ServiceWithDependency One { get; private set; }
		public AnotherServiceWithDependency Another { get; private set; }

		public ServiceWithTwoParametersBothDependentOnSameService(ServiceWithDependency one, AnotherServiceWithDependency another)
		{
			One = one;
			Another = another;
		}
	}

	public class ServiceWithTwoDependenciesWithLazySingletonDependency
	{
		public ServiceWithLazyDependency One { get; private set; }
		public AnotherServiceWithLazyDependency Another { get; private set; }

		public ServiceWithTwoDependenciesWithLazySingletonDependency(ServiceWithLazyDependency one, AnotherServiceWithLazyDependency another)
		{
			One = one;
			Another = another;
		}
	}

	public class ServiceWithTwoDepenedenciesOfTheSameType
	{
		public Service One { get; set; }
		public Service Another { get; set; }

		public ServiceWithTwoDepenedenciesOfTheSameType(Service one, Service another)
		{
			One = one;
			Another = another;
		}
	}

	public class ServiceWithoutPublicConstructor
	{
		private ServiceWithoutPublicConstructor() { }
	}

	public class GenericOne<T>
	{
		public T Bla;

		public GenericOne(GenericOne<int> intOne)
		{
			IntOne = intOne;
		}

		protected GenericOne<int> IntOne { get; set; }
	}

	public class ServiceWithTwoSameGenericDependencies
	{
		public IService<int> Service1 { get; set; }
		public IService<int> Service2 { get; set; }

		public ServiceWithTwoSameGenericDependencies(IService<int> service1, IService<int> service2)
		{
			Service1 = service1;
			Service2 = service2;
		}
	}

	public class ServiceWithTwoSameFuncDependencies
	{
		public Func<IService> Factory1 { get; set; }
		public Func<IService> Factory2 { get; set; }

		public ServiceWithTwoSameFuncDependencies(Func<IService> factory1, Func<IService> factory2)
		{
			Factory1 = factory1;
			Factory2 = factory2;
		}
	}

	public class ClientWithServiceAndFuncOfServiceDependencies
	{
		public IService Service { get; set; }
		public Func<IService> Factory { get; set; }

		public ClientWithServiceAndFuncOfServiceDependencies(Func<IService> factory, IService service)
		{
			Service = service;
			Factory = factory;
		}
	}

    public class ComplexCreativity
    {
        public int Blah { get; set; }
        public string Name { get; set; }

        public ComplexCreativity(string name)
        {
            Name = name;
        }

        public ComplexCreativity(int blah)
        {
            Blah = blah;
        }

        public ComplexCreativity()
        {
        }
    }

    public interface ITransientService
    {
    }

    public interface ISingletonService
    {
    }

    public class ServiceWithMultipleCostructors
    {
        public ITransientService Transient { get; private set; }
        public ISingletonService Singleton { get; private set; }

        public ServiceWithMultipleCostructors(ISingletonService singleton)
        {
            Singleton = singleton;
        }

        public ServiceWithMultipleCostructors(ITransientService transient)
        {
            Transient = transient;
        }
    }

    public interface IBar { }

    public class Bar : IBar { }

    public interface IFuh
    {
        IBar Bar { get; set; }
    }

    public class Fuh : IFuh
    {
        public IBar Bar { get; set; }

        public Fuh(IBar bar)
        {
            Bar = bar;
        }
    }

    public class DisposableService : IService, IDisposable
    {
        public bool IsDisposed;

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public enum EnumKey { Some };

    public class Log {}

    public class Consumer
    {
        public Account Account { get; set; }
        public Log Log { get; set; }

        public Consumer(Account account, Log log)
        {
            Account = account;
            Log = log;
        }
    }

    public class Account
    {
        public Log Log { get; set; }

        public Account(Log log)
        {
            Log = log;
        }
    }
}
