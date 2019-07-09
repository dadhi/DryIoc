using NUnit.Framework;
using RealisticUnitOfWork;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue44_Real_world_benchmark_the_unit_work_or_say_the_controller_resolution
    {
        [Test]
        public void CreateContainerAndRegisterServices_Then_FirstTimeOpenScopeAndResolve()
        {
            var container = new Container();

            var x = container.PrepareDryIoc().Measure();

            Assert.IsInstanceOf<R>(x);
        }

        [Test]
        public void OpenScopeAndResolve()
        {
            var container = new Container().PrepareDryIoc();
            var x1 = container.Measure();

            var x2 = container.Measure();

            Assert.IsNotNull(x1);
            Assert.IsNotNull(x2);
        }
    }

    public static class Realistic_unit_of_work_slash_web_controller_example
    {
        public static IContainer PrepareDryIoc(this IContainer container)
        {
            // register dummy scoped and singletons services to populate resolution cache and scopes to be close to reality
            RegisterDummyPopulation(container);

            // register graph for benchmarking starting with scoped R (root) / Controller
            container.Register<R>(Reuse.Scoped);

            container.Register<Scoped1>(Reuse.Scoped);
            container.Register<Scoped2>(Reuse.Scoped);

            container.Register<Trans1>(Reuse.Transient);
            container.Register<Trans2>(Reuse.Transient);

            container.Register<Single1>(Reuse.Singleton);
            container.Register<Single2>(Reuse.Singleton);

            container.RegisterDelegate(r => 
                new ScopedFac1(r.Resolve<Scoped1>(), r.Resolve<Scoped3>(), r.Resolve<Single1>(), r.Resolve<SingleObj1>()),
                Reuse.Scoped);

            container.RegisterDelegate(r => 
                new ScopedFac2(r.Resolve<Scoped2>(), r.Resolve<Scoped4>(), r.Resolve<Single2>(), r.Resolve<SingleObj2>()),
                Reuse.Scoped);

            container.RegisterInstance(new SingleObj1());
            container.RegisterInstance(new SingleObj2());

            // level 2
            container.Register<Scoped3>(Reuse.Scoped);
            container.Register<Scoped4>(Reuse.Scoped);

            container.Register<Scoped12>(Reuse.Scoped);
            container.Register<Scoped22>(Reuse.Scoped);

            container.Register<Single12>(Reuse.Singleton);
            container.Register<Single22>(Reuse.Singleton);

            container.Register<Trans12>(Reuse.Transient);
            container.Register<Trans22>(Reuse.Transient);

            container.RegisterDelegate(
                r => new ScopedFac12(r.Resolve<Scoped13>(), r.Resolve<Single1>(), r.Resolve<SingleObj13>()),
                Reuse.Scoped);
            container.RegisterDelegate(
                r => new ScopedFac22(r.Resolve<Scoped23>(), r.Resolve<Single2>(), r.Resolve<SingleObj23>()),
                Reuse.Scoped);

            container.RegisterInstance(new SingleObj12());
            container.RegisterInstance(new SingleObj22());

            // level 3
            container.Register<Scoped13>(Reuse.Scoped);
            container.Register<Scoped23>(Reuse.Scoped);

            container.Register<Single13>(Reuse.Singleton);
            container.Register<Single23>(Reuse.Singleton);

            container.Register<Trans13>(Reuse.Transient);
            container.Register<Trans23>(Reuse.Transient);

            container.RegisterDelegate(
                r => new ScopedFac13(r.Resolve<Single1>(), r.Resolve<Scoped14>(), r.Resolve<ScopedFac14>()),
                Reuse.Scoped);

            container.RegisterDelegate(
                r => new ScopedFac23(r.Resolve<Single2>(), r.Resolve<Scoped24>(), r.Resolve<ScopedFac24>()),
                Reuse.Scoped);

            container.RegisterInstance(new SingleObj13());
            container.RegisterInstance(new SingleObj23());

            // level 4
            container.Register<Scoped14>(Reuse.Scoped);
            container.Register<Scoped24>(Reuse.Scoped);

            container.Register<Single14>(Reuse.Singleton);
            container.Register<Single24>(Reuse.Singleton);

            container.Register<Trans14>(Reuse.Transient);
            container.Register<Trans24>(Reuse.Transient);

            container.RegisterDelegate(r => new ScopedFac14(), Reuse.Scoped);
            container.RegisterDelegate(r => new ScopedFac24(), Reuse.Scoped);

            container.RegisterInstance(new SingleObj14());
            container.RegisterInstance(new SingleObj24());

            ResolveDummyPopulation(container);
            return container;
        }

        public static object Measure(this IContainer container)
        {
            using (var scope = container.OpenScope())
                return scope.Resolve<R>();
        }

        private static void RegisterDummyPopulation(IContainer container)
        {
            container.Register<D1>(Reuse.Scoped);
            container.Register<D2>(Reuse.Scoped);
            container.Register<D3>(Reuse.Scoped);
            container.Register<D4>(Reuse.Scoped);
            container.Register<D5>(Reuse.Scoped);
            container.Register<D6>(Reuse.Scoped);
            container.Register<D7>(Reuse.Scoped);
            container.Register<D8>(Reuse.Scoped);
            container.Register<D9>(Reuse.Scoped);
            container.Register<D10>(Reuse.Scoped);
            container.Register<D11>(Reuse.Scoped);
            container.Register<D12>(Reuse.Scoped);

            container.Register<D13>(Reuse.Singleton);
            container.Register<D14>(Reuse.Singleton);
            container.Register<D15>(Reuse.Singleton);
            container.Register<D16>(Reuse.Singleton);
            container.Register<D17>(Reuse.Singleton);
            container.Register<D18>(Reuse.Singleton);
            container.Register<D19>(Reuse.Singleton);
            container.Register<D20>(Reuse.Singleton);
        }

        public static object ResolveDummyPopulation(IContainer container)
        {
            using (var scope = container.OpenScope())
            {
                scope.Resolve<D1>();
                scope.Resolve<D2>();
                scope.Resolve<D3>();
                scope.Resolve<D4>();
                scope.Resolve<D5>();
                scope.Resolve<D6>();
                scope.Resolve<D7>();
                scope.Resolve<D8>();
                scope.Resolve<D9>();
                scope.Resolve<D10>();
                scope.Resolve<D11>();
                scope.Resolve<D12>();

                scope.Resolve<D13>();
                scope.Resolve<D14>();
                scope.Resolve<D15>();
                scope.Resolve<D16>();
                scope.Resolve<D17>();
                scope.Resolve<D18>();
                scope.Resolve<D19>();
                return scope.Resolve<D20>();
            }
        }
    }
}
