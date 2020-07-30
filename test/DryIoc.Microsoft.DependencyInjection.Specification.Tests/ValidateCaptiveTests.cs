using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace DryIoc.Microsoft.DependencyInjection.Specification.Tests
{
    [TestFixture]
    public class ValidateCaptiveTests
    {
        [Test]
        public void ServiceCollection_AddScoped_dependency_in_a_Singleton_ISNOT_Validated_as_captive_dependency()
        {
            var services = new ServiceCollection();
            services.AddScoped<Foo>();      // Actually a `ScopedOrSingleton` due MS.DI convention
            services.AddSingleton<Bar>();
            services.AddScoped<Buz>();      // Actually a `ScopedOrSingleton` due MS.DI convention

            // These two line are "presumably" done by the framework
            var providerFactory = new DryIocAdapter.DryIocServiceProviderFactory();
            var provider = providerFactory.CreateServiceProvider(providerFactory.CreateBuilder(services));

            // Getting back the underlying DryIoc container to use its functions (it is always implicitly available).
            var container = provider.GetRequiredService<IContainer>();

            var errors = container.Validate(ServiceInfo.Of<Foo>());

            // No errors (!) is because due MS.DI conventions `AddScoped` adds a service as a `ScopedOrSingleton`,
            // which is perfectly fine to have inside the a `Singleton`
            Assert.AreEqual(0, errors.Length);
        }

        [Test]
        public void DryIoc_own_Register_ReuseScope_dependency_in_a_Singleton_IS_Validated_as_captive_dependency()
        {
            var services = new ServiceCollection();
            services.AddScoped<Foo>();      // Actually a `ScopedOrSingleton` due MS.DI convention
            services.AddSingleton<Bar>();

            // These two line are "presumably" done by the framework
            var providerFactory = new DryIocAdapter.DryIocServiceProviderFactory();
            var provider = providerFactory.CreateServiceProvider(providerFactory.CreateBuilder(services));

            // Getting back the underlying DryIoc container to use its functions (it is always implicitly available).
            var container = provider.GetRequiredService<IContainer>();
            
            // IMPORTANT line
            container.Register<Buz>(Reuse.Scoped);

            var errors = container.Validate(ServiceInfo.Of<Foo>());

            Assert.AreEqual(1, errors.Length);
            var error = errors[0].Value;
            Assert.AreEqual(Error.NameOf(Error.DependencyHasShorterReuseLifespan), error.ErrorName);

            /* Exception message:
            
            code: Error.DependencyHasShorterReuseLifespan; 
            message: Dependency Buz as parameter "buz" (IsSingletonOrDependencyOfSingleton) with reuse Scoped {Lifespan=100} has a shorter lifespan than its parent's Singleton Bar as parameter "bar" FactoryId=145 (IsSingletonOrDependencyOfSingleton)
              in Resolution root Scoped Foo FactoryId=144
              from container without scope
             with Rules with {UsedForValidation} and without {ImplicitCheckForReuseMatchingScope, EagerCachingSingletonForFasterAccess} with TotalDependencyCountInLambdaToSplitBigObjectGraph=2147483647
            If you know what you're doing you may disable this error with the rule `new Container(rules => rules.WithoutThrowIfDependencyHasShorterReuseLifespan())`.
            */
        }

        public class Foo
        {
            public Foo(Bar bar) { }
        }

        public class Bar
        {
            public Bar(Buz buz) { }
        }

        public class Buz
        {
        }
    }
}
