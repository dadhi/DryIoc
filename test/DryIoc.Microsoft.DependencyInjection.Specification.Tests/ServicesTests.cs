using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection.Specification;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using NUnit.Framework;

// uncomment when I want to copy some test here for testing.
//using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
//using Xunit;

namespace DryIoc.Microsoft.DependencyInjection.Specification.Tests
{
    public class ServicesTests : DependencyInjectionSpecificationTests, ITest
    {
        public int Run()
        {
            foreach (object[] lt in LifetimeCombinations)
                Resolve_single_service_with_multiple_registrations_should_resolve_the_same_way_as_microsoft_di(
                    (bool)lt[0], (ServiceLifetime)lt[1], (ServiceLifetime)lt[2], (Type)lt[3]);
            var testCount = LifetimeCombinations.Length;

            OpenGenericsWithIsService_DoubleTest();
            ServiceScopeFactoryIsSingleton_local();
            ScopesAreFlatNotHierarchical_local();
            testCount += 3;

            // DependencyInjectionSpecificationTests
            var createInstFuncs = CreateInstanceFuncs.SelectMany(xs => xs).Cast<CreateInstanceFunc>().ToArray();

            foreach (var f in createInstFuncs) TypeActivatorEnablesYouToCreateAnyTypeWithServicesEvenWhenNotInIocContainer(f);
            foreach (var f in createInstFuncs) TypeActivatorAcceptsAnyNumberOfAdditionalConstructorParametersToProvide(f);
            foreach (var f in createInstFuncs) TypeActivatorWorksWithStaticCtor(f);
            foreach (var f in createInstFuncs) TypeActivatorWorksWithCtorWithOptionalArgs(f);
            foreach (var f in createInstFuncs) TypeActivatorWorksWithCtorWithOptionalArgs_WithStructDefaults(f);
            foreach (var f in createInstFuncs) TypeActivatorCanDisambiguateConstructorsWithUniqueArguments(f);
            foreach (var f in createInstFuncs) TypeActivatorRequiresAllArgumentsCanBeAccepted(f);
            foreach (var f in createInstFuncs) TypeActivatorRethrowsOriginalExceptionFromConstructor(f);
            foreach (var f in createInstFuncs) TypeActivatorUsesMarkedConstructor(f);
            foreach (var f in createInstFuncs) TypeActivatorThrowsOnMultipleMarkedCtors(f);
            foreach (var f in createInstFuncs) TypeActivatorThrowsWhenMarkedCtorDoesntAcceptArguments(f);
            foreach (var f in createInstFuncs) UnRegisteredServiceAsConstructorParameterThrowsException(f);
            testCount += 12;

            var matchesData = ServiceContainerPicksConstructorWithLongestMatchesData;
            var matchesDataCount = 0;
            foreach (var d in matchesData)
            {
                ServiceContainerPicksConstructorWithLongestMatches((IServiceCollection)d[0], (TypeWithSupersetConstructors)d[1]);
                ++matchesDataCount;
            }
            testCount += matchesDataCount;

            ResolvesDifferentInstancesForServiceWhenResolvingEnumerable(
                typeof(IFakeService), typeof(FakeService), typeof(IFakeService), ServiceLifetime.Scoped);
            ResolvesDifferentInstancesForServiceWhenResolvingEnumerable(
                typeof(IFakeService), typeof(FakeService), typeof(IFakeService), ServiceLifetime.Singleton);
            ResolvesDifferentInstancesForServiceWhenResolvingEnumerable(
                typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>), typeof(IFakeOpenGenericService<IServiceProvider>), ServiceLifetime.Scoped);
            ResolvesDifferentInstancesForServiceWhenResolvingEnumerable(
                typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>), typeof(IFakeOpenGenericService<IServiceProvider>), ServiceLifetime.Singleton);
            testCount += 4;

            TypeActivatorCreateFactoryDoesNotAllowForAmbiguousConstructorMatches(typeof(string));
            TypeActivatorCreateFactoryDoesNotAllowForAmbiguousConstructorMatches(typeof(int));
            TypeActivatorCreateInstanceUsesLongestAvailableConstructor("", "IFakeService, string");
            TypeActivatorCreateInstanceUsesLongestAvailableConstructor(5, "IFakeService, int");
            GetServiceOrCreateInstanceRegisteredServiceTransient();
            GetServiceOrCreateInstanceRegisteredServiceSingleton();
            GetServiceOrCreateInstanceUnregisteredService();
            CreateInstance_WithAbstractTypeAndPublicConstructor_ThrowsCorrectException();
            CreateInstance_CapturesInnerException_OfTargetInvocationException();
            ServicesRegisteredWithImplementationTypeCanBeResolved();
            ServicesRegisteredWithImplementationType_ReturnDifferentInstancesPerResolution_ForTransientServices();
            ServicesRegisteredWithImplementationType_ReturnSameInstancesPerResolution_ForSingletons();
            ServiceInstanceCanBeResolved();
            TransientServiceCanBeResolvedFromProvider();
            TransientServiceCanBeResolvedFromScope();
            NonSingletonService_WithInjectedProvider_ResolvesScopeProvider(ServiceLifetime.Scoped);
            NonSingletonService_WithInjectedProvider_ResolvesScopeProvider(ServiceLifetime.Transient);
            SingletonServiceCanBeResolvedFromScope();
            SingleServiceCanBeIEnumerableResolved();
            MultipleServiceCanBeIEnumerableResolved();
            RegistrationOrderIsPreservedWhenServicesAreIEnumerableResolved();
            OuterServiceCanHaveOtherServicesInjected();
            FactoryServicesCanBeCreatedByGetService();
            FactoryServicesAreCreatedAsPartOfCreatingObjectGraph();
            LastServiceReplacesPreviousServices();
            SingletonServiceCanBeResolved();
            ServiceProviderRegistersServiceScopeFactory();
            ServiceScopeFactoryIsSingleton();
            ScopedServiceCanBeResolved();
            NestedScopedServiceCanBeResolved();
            ScopedServices_FromCachedScopeFactory_CanBeResolvedAndDisposed();
            ScopesAreFlatNotHierarchical();
            ServiceProviderIsDisposable();
            DisposingScopeDisposesService();
            SelfResolveThenDispose();
            SafelyDisposeNestedProviderReferences();
            SingletonServicesComeFromRootProvider();
            NestedScopedServiceCanBeResolvedWithNoFallbackProvider();
            OpenGenericServicesCanBeResolved();
            ConstrainedOpenGenericServicesCanBeResolved();
            ConstrainedOpenGenericServicesReturnsEmptyWithNoMatches();
            InterfaceConstrainedOpenGenericServicesCanBeResolved();
            AbstractClassConstrainedOpenGenericServicesCanBeResolved();
            ClosedServicesPreferredOverOpenGenericServices();
            ResolvingEnumerableContainingOpenGenericServiceUsesCorrectSlot();
            AttemptingToResolveNonexistentServiceReturnsNull();
            NonexistentServiceCanBeIEnumerableResolved();
            DisposesInReverseOrderOfCreation();
            ResolvesMixedOpenClosedGenericsAsEnumerable();
            ExplicitServiceRegisterationWithIsService();
            OpenGenericsWithIsService();
            ClosedGenericsWithIsService();
            IEnumerableWithIsServiceAlwaysReturnsTrue();
            BuiltInServicesWithIsServiceReturnsTrue();

            testCount += 54;

            return testCount;
        }

        protected override IServiceProvider CreateServiceProvider(IServiceCollection services) =>
            services.BuildDryIocServiceProvider();

        internal class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
        {
        }

        public static object[] LifetimeCombinations =
        {
          new object[] { false, ServiceLifetime.Singleton, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Singleton, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Singleton, ServiceLifetime.Scoped, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Transient, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Transient, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Transient, ServiceLifetime.Scoped, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Scoped, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Scoped, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Scoped, ServiceLifetime.Scoped, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Singleton, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Singleton, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Singleton, ServiceLifetime.Scoped, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Transient, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Transient, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Transient, ServiceLifetime.Scoped, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Scoped, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Scoped, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Scoped, ServiceLifetime.Scoped, typeof(ServiceB) },
        };

        [Test, TestCaseSource(nameof(LifetimeCombinations))]
        public void Resolve_single_service_with_multiple_registrations_should_resolve_the_same_way_as_microsoft_di(
            bool usingScope, ServiceLifetime firstLifetime, ServiceLifetime secondLifetime, Type expectedResolveType)
        {
            // arrange
            var collection = new ServiceCollection();
            collection.Add(ServiceDescriptor.Describe(typeof(IService), typeof(ServiceA), firstLifetime));
            collection.Add(ServiceDescriptor.Describe(typeof(IService), typeof(ServiceB), secondLifetime));

            IServiceProvider msProvider = ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(collection);
            IServiceProvider diProvider = collection.BuildDryIocServiceProvider();

            if (usingScope)
            {
                msProvider = msProvider.CreateScope().ServiceProvider;
                diProvider = diProvider.CreateScope().ServiceProvider;
            }

            // act
            var msService = msProvider.GetRequiredService<IService>();
            var dryiocService = diProvider.GetRequiredService<IService>();

            // assert
            Assert.IsInstanceOf(expectedResolveType, msService, "Microsoft changed the implementation");
            Assert.IsInstanceOf(expectedResolveType, dryiocService, "DryIoc resolves the requested type different than microsofts di implementation");
        }

        [Test]
        public void OpenGenericsWithIsService_DoubleTest()
        {
            if (!SupportsIServiceProviderIsService)
            {
                return;
            }

            // Arrange
            var collection = new TestServiceCollection();
            collection.AddTransient(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>));
            var provider = CreateServiceProvider(collection);

            // Act
            var serviceProviderIsService = provider.GetService<IServiceProviderIsService>();

            Assert.NotNull(serviceProviderIsService.IsService(typeof(IServiceProvider)));

            // Assert
            Assert.NotNull(serviceProviderIsService);
            Assert.True(serviceProviderIsService.IsService(typeof(IFakeOpenGenericService<IFakeService>)));
            Assert.False(serviceProviderIsService.IsService(typeof(IFakeOpenGenericService<>)));
        }

        [Test]
        public void ServiceScopeFactoryIsSingleton_local()
        {
            // Arrange
            var collection = new TestServiceCollection();
            var provider = CreateServiceProvider(collection);

            // Act
            var scopeFactory1 = provider.GetService<IServiceScopeFactory>();
            var scopeFactory2 = provider.GetService<IServiceScopeFactory>();
            using (var scope = provider.CreateScope())
            {
                var scopeFactory3 = scope.ServiceProvider.GetService<IServiceScopeFactory>();

                // Assert
                Assert.AreSame(scopeFactory1, scopeFactory2);
                Assert.AreSame(scopeFactory1, scopeFactory3);
            }
        }

        [Test]
        public void ScopesAreFlatNotHierarchical_local()
        {
            // Arrange
            var collection = new TestServiceCollection();
            collection.AddSingleton<IFakeSingletonService, FakeService>();
            var provider = CreateServiceProvider(collection);

            // Act
            var outerScope = provider.CreateScope();
            using var innerScope = outerScope.ServiceProvider.CreateScope();
            outerScope.Dispose();
            var innerScopedService = innerScope.ServiceProvider.GetService<IFakeSingletonService>();

            // Assert
            Assert.NotNull(innerScopedService);
        }

        public interface IService
        {
        }

        public class ServiceA : IService
        {
        }

        public class ServiceB : IService
        {
        }
    }
}
