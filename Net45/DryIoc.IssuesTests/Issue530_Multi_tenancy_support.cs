using System;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue530_Multi_tenancy_support
    {
        [Test]
        public void Func_of_scoped_thing_does_not_work_without_ambient_scope()
        {
            var c = new Container();

            c.Register<SingletonWithScopedFactory>(Reuse.Singleton);
            c.Register<ScopedThing>(Reuse.Scoped);

            var singleT = c.Resolve<SingletonWithScopedFactory>();

            using (c.OpenScope()) // unused scope is clear indication of the problem
            {
                Assert.Throws<ContainerException>(() => 
                    singleT.ScopedThing());
            }
        }

        [Test]
        public void No_cache_problem_between_Resolved_singleton_and_scoped()
        {
            var c = new Container();

            c.Register<IThing, SingleThing>(Reuse.Singleton);
            c.Register<IThing, ScopedThing>(Reuse.Scoped);

            Assert.IsInstanceOf<SingleThing>(c.Resolve<IThing>());

            using (var scope = c.OpenScope())
            {
                Assert.IsInstanceOf<ScopedThing>(scope.Resolve<IThing>());
            }

            Assert.IsInstanceOf<SingleThing>(c.Resolve<IThing>());
        }

        [Test]
        public void No_cache_problem_between_Injected_singleton_and_scoped()
        {
            var c = new Container();

            c.Register<IThing, SingleThing>(Reuse.Singleton);
            c.Register<IThing, ScopedThing>(Reuse.Scoped);
            c.Register<ThingUser>();

            Assert.IsInstanceOf<SingleThing>(c.Resolve<ThingUser>().Thing);

            using (var scope = c.OpenScope())
            {
                Assert.IsInstanceOf<ScopedThing>(scope.Resolve<ThingUser>().Thing);
            }

            Assert.IsInstanceOf<SingleThing>(c.Resolve<ThingUser>().Thing);
        }

        public interface IThing {}
        public class ScopedThing : IThing {}
        public class SingleThing : IThing {}

        public class ThingUser
        {
            public IThing Thing { get; }
            public ThingUser(IThing thing)
            {
                Thing = thing;
            }
        }

        public class SingletonWithScopedFactory
        {
            public Func<ScopedThing> ScopedThing { get; }
            public SingletonWithScopedFactory(Func<ScopedThing> scopedThing)
            {
                ScopedThing = scopedThing;
            }
        }

        [Test]
        public void Sample_based_registration_condition_and_resolution_scope()
        {
            var c = new Container();

            // register tenants with respective key + plus instruct tenant to open its scope
            c.Register<ITenant, GreenTenant>(serviceKey: TenantKey.Green, setup: Setup.With(openResolutionScope: true));
            c.Register<ITenant, BlueTenant>(serviceKey: TenantKey.Blue, setup: Setup.With(openResolutionScope: true));

            // green tenant services
            var greenSetup = Setup.With(null, req => IsTenant(req, TenantKey.Green));

            c.Register<ITransient, GreenTransient>(setup: greenSetup);
            c.Register<IScoped, GreenScoped>(Reuse.Scoped, setup: greenSetup);
            c.Register<ISingleton, GreenSingleton>(Reuse.ScopedTo<ITenant>(TenantKey.Green));

            // default services
            c.Register<ISomeController, SomeController>(Reuse.Scoped);
            c.Register<ITransient, DefaultTransient>();
            c.Register<IScoped, DefaultScoped>(Reuse.Scoped);
            c.Register<ISingleton, DefaultSingleton>(Reuse.Singleton);

            using (var reqScope = c.OpenScope())
            {
                GetAndAssertTenantServices(reqScope, TenantKey.Green, "Green");
                GetAndAssertTenantServices(reqScope, TenantKey.Blue, "Default");
            }

            using (var reqScope = c.OpenScope())
            {
                GetAndAssertTenantServices(reqScope, TenantKey.Green, "Green");
                GetAndAssertTenantServices(reqScope, TenantKey.Blue, "Default");
            }

            var allTransients = c.ResolveMany<ITransient>();
            // will be 1 default, because tenant transients can be resolved only through a tenant
            Assert.AreEqual(1, allTransients.Count());
        }

        private static void GetAndAssertTenantServices(IResolver resolver, TenantKey tenantKey, string expectedTypePart)
        {
            var tenant = resolver.Resolve<ITenant>(tenantKey);

            var controller = tenant.GetController(tenantKey);

            StringAssert.Contains(expectedTypePart, controller.Transient.GetType().Name);
            StringAssert.Contains(expectedTypePart, controller.Scoped.GetType().Name);
            StringAssert.Contains(expectedTypePart, controller.Singleton.GetType().Name);

            Assert.AreSame(controller.Singleton, controller.Transient.Singleton);

            Assert.AreNotSame(controller.Transient, controller.TransientFactory());
        }

        public enum TenantKey { Green, Blue }

        private static bool IsTenant(Request request, TenantKey tenantKey) =>
            request.CurrentScope != null && 
            request.CurrentScope.Any(scope => tenantKey.Equals((scope.Name as ResolutionScopeName)?.ServiceKey));

        #region Tenants

        public interface ITenant
        {
            ISomeController GetController(object ctx);
        }

        public class GreenTenant : ITenant
        {
            public IResolverContext ScopedResolver { get; }

            public GreenTenant(IResolverContext scopedResolver)
            {
                ScopedResolver = scopedResolver;
            }

            public ISomeController GetController(object ctx)
            {
                // can use ctx to route
                return ScopedResolver.Resolve<ISomeController>();
            }
        }

        public class BlueTenant : ITenant
        {
            public IResolverContext Resolver { get; }

            public BlueTenant(IResolverContext resolver)
            {
                Resolver = resolver;
            }

            public ISomeController GetController(object ctx)
            {
                return Resolver.Resolve<ISomeController>();
            }
        }

        #endregion

        #region Transients

        public interface ITransient
        {
            ISingleton Singleton { get; }
        }

        public class GreenTransient : ITransient
        {
            public ISingleton Singleton { get; }
            public GreenTransient(ISingleton singleton) { Singleton = singleton; }
        }

        public class BlueTransient : ITransient
        {
            public ISingleton Singleton { get; }
            public BlueTransient(ISingleton singleton) { Singleton = singleton; }
        }

        public class DefaultTransient : ITransient
        {
            public ISingleton Singleton { get; }
            public DefaultTransient(ISingleton singleton) { Singleton = singleton; }
        }

        #endregion

        #region Scoped services

        public interface IScoped { }
        public class GreenScoped : IScoped { }
        public class DefaultScoped : IScoped { }

        #endregion

        #region Singletons

        public interface ISingleton { }
        public class GreenSingleton : ISingleton { }
        public class DefaultSingleton : ISingleton { }

        #endregion

        public interface ISomeController
        {
            ITransient Transient { get; }
            IScoped Scoped { get; }
            ISingleton Singleton { get; }
            Func<ITransient> TransientFactory { get; }
        }

        public class SomeController : ISomeController
        {
            public ITransient Transient { get; }
            public IScoped Scoped { get; }
            public ISingleton Singleton { get; }
            public Func<ITransient> TransientFactory { get; }

            public SomeController(ITransient transient, IScoped scoped, ISingleton singleton,
                Func<ITransient> transientFactory)
            {
                Transient = transient;
                Scoped = scoped;
                Singleton = singleton;
                TransientFactory = transientFactory;
            }
        }
    }
}
