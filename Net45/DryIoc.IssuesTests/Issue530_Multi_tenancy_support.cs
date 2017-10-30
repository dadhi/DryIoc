using System;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue530_Multi_tenancy_support
    {
        [Test]
        public void Test_func_of_scoped_thing()
        {
            var c = new Container();

            c.Register<SingleThing>(Reuse.Singleton);
            c.Register<ScopedThing>(Reuse.Scoped);

            var singleT = c.Resolve<SingleThing>();

            using (var scope = c.OpenScope())
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
        [TestCase("blue")]
        [TestCase("green")]
        public void Sample_based_registration_condition_and_resolution_scope(string context)
        {
            var c = new Container();

            c.RegisterMany<GreenTenant>(setup: Setup.With(openResolutionScope: true));
            c.RegisterMany<BlueTenant>(setup: Setup.With(openResolutionScope: true));

            // green tenant
            c.Register<ITransient, GreenTransient>(setup: Setup.With(condition: IsGreenTenant));

            // default services
            c.Register<ITransient, DefaultTransient>();
            c.Register<ISomeController, SomeController>(Reuse.Scoped);

            var tenant = context == "green"
                ? c.Resolve<ITenant, GreenTenant>()
                : c.Resolve<ITenant, BlueTenant>();

            var controller = tenant.GetController(context);
            Assert.IsNotNull(controller.Transient);

            var allTransients = c.ResolveMany<ITransient>();
            // will be 1 default, because tenant transients can be resolved only through a tenant
            Assert.AreEqual(1, allTransients.Count());
        }

        private static bool IsGreenTenant(RequestInfo r) =>
            r.Parent.Enumerate().Any(p => p.ImplementationType == typeof(GreenTenant));

        private static bool InBlueTenant(RequestInfo r) =>
            r.Parent.Enumerate().Any(p => p.ImplementationType == typeof(BlueTenant));

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

        public interface ITransient { }
        public class GreenTransient : ITransient { }
        public class BlueTransient : ITransient { }
        public class DefaultTransient : ITransient { }

        public interface ISomeController
        {
            ITransient Transient { get; }
        }

        public class SomeController : ISomeController
        {
            public ITransient Transient { get; }

            public SomeController(ITransient transient)
            {
                Transient = transient;
            }
        }
    }

}
