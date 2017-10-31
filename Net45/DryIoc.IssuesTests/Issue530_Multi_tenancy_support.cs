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

            c.Register<SingleThing>(Reuse.Singleton);
            c.Register<ScopedThing>(Reuse.Scoped);

            var singleT = c.Resolve<SingleThing>();

            using (var scope = c.OpenScope()) // unused scope is clear indication of the problem
            {
                Assert.Throws<ContainerException>(() => 
                    singleT.ScopedThing());
            }
        }

        public class ScopedThing {}

        public class SingleThing
        {
            public Func<ScopedThing> ScopedThing { get; }
            public SingleThing(Func<ScopedThing> scopedThing)
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
            var greenSetup = Setup.With(null, IsGreenTenant);

            c.Register<ITransient, GreenTransient>(setup: greenSetup);
            c.Register<IScoped, GreenScoped>(Reuse.Scoped, setup: greenSetup);
            c.Register<ISingleton, GreenSingleton>(Reuse.ScopedTo<ITenant>(TenantKey.Green));

            // default services
            c.Register<ISomeController, SomeController>(Reuse.Scoped);
            c.Register<ITransient, DefaultTransient>();
            c.Register<ISingleton, DefaultSingleton>();

            // set a tenant
            var context = "green";

            var tenant = context == "green"
                ? c.Resolve<ITenant>(TenantKey.Green)
                : c.Resolve<ITenant>(TenantKey.Blue);

            var controller = tenant.GetController(context);
            Assert.IsInstanceOf<GreenTransient>(controller.Transient);
            Assert.IsInstanceOf<GreenScoped>(controller.Scoped);
            Assert.IsInstanceOf<GreenSingleton>(controller.Singleton);

            var allTransients = c.ResolveMany<ITransient>();
            // will be 1 default, because tenant transients can be resolved only through a tenant
            Assert.AreEqual(1, allTransients.Count());
        }

        public enum TenantKey { Green, Blue }

        private static Func<Request, bool> IsGreenTenant = r =>
            r.CurrentScope != null && 
            r.CurrentScope.Any(scope => TenantKey.Green.Equals((scope.Name as ResolutionScopeName)?.ServiceKey));

        private static bool InBlueTenant(RequestInfo r) =>
            r.Parent.Any(p => TenantKey.Blue.Equals(p.ServiceKey));

        public interface ITenant
        {
            ISomeController GetController(object ctx);
        }

        public class GreenTenant : ITenant
        {
            public IResolverContext Resolver { get; }

            public GreenTenant(IResolverContext resolver)
            {
                Resolver = resolver;
            }

            public ISomeController GetController(object ctx)
            {
                // can use ctx to route
                return Resolver.Resolve<ISomeController>();
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

        // transients
        public interface ITransient { }
        public class GreenTransient : ITransient { }
        public class BlueTransient : ITransient { }
        public class DefaultTransient : ITransient { }

        // scoped
        public interface IScoped { }
        public class GreenScoped : IScoped { }

        // singletons
        public interface ISingleton { }
        public class GreenSingleton : ISingleton { }
        public class DefaultSingleton : ISingleton { }

        public interface ISomeController
        {
            ITransient Transient { get; }
            IScoped Scoped { get; }
            ISingleton Singleton { get; }
        }

        public class SomeController : ISomeController
        {
            public ITransient Transient { get; }
            public IScoped Scoped { get; }
            public ISingleton Singleton { get; }

            public SomeController(ITransient transient, IScoped scoped, ISingleton singleton)
            {
                Transient = transient;
                Scoped = scoped;
                Singleton = singleton;
            }
        }
    }

}
