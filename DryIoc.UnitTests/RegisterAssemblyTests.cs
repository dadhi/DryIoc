using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class RegisterAssemblyTests
    {
        [Test]
        public void Can_register_service_with_implementations_found_in_assemblies()
        {
            var container = new Container();

            container.RegisterFromAssemblies<IBlah>();

            var services = container.Resolve<IBlah[]>();

            CollectionAssert.AreEquivalent(
                new[] { typeof(Blah), typeof(AnotherBlah) },
                services.Select(s => s.GetType()));
        }

        [Test]
        public void Can_register_genric_service_with_implementations_found_in_assemblies()
        {
            var container = new Container();

            container.RegisterFromAssemblies(typeof(IBlah<,>));

            var services = container.Resolve<IBlah<string, bool>[]>();

            CollectionAssert.AreEquivalent(
                new[] { typeof(Blah<string, bool>), typeof(AnotherBlah<bool>) },
                services.Select(s => s.GetType()));
        }

        public interface IBlah { }
        public class Blah : IBlah { }
        public class AnotherBlah : IBlah { }

        public interface IBlah<T0, T1> { }
        public class Blah<T0, T1> : IBlah<T0, T1> { }
        public class AnotherBlah<T> : IBlah<string, T> { }
    }

    public static class MyClass
    {
        public static void RegisterFromAssemblies(this IRegistrator registrator, Type serviceType, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                assemblies = new[] { serviceType.GetAssembly() };

            var implementations = assemblies.SelectMany(Polyfill.GetTypesFrom)
                .Where(type => IsImplementationOf(type, serviceType)).ToArray();

            for (var i = 0; i < implementations.Length; ++i)
                registrator.Register(serviceType, implementations[i]);
        }
        
        public static void RegisterFromAssemblies<TService>(this IRegistrator registrator, params Assembly[] assemblies)
        {
            registrator.RegisterFromAssemblies(typeof(TService), assemblies);
        }

        private static bool IsImplementationOf(Type candidate, Type serviceType)
        {
            if (candidate.IsAbstract() || !serviceType.IsPublicOrNestedPublic())
                return false;

            if (candidate == serviceType)
                return true;

            var implementedTypes = candidate.GetImplementedTypes();
            
            var found = !serviceType.IsOpenGeneric() 
                ? implementedTypes.Contains(serviceType) 
                : implementedTypes.Any(t => t.GetGenericDefinitionOrNull() == serviceType);
            
            return found;
        }
    }
}
