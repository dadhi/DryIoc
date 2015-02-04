
namespace DryIoc.CompileTimeGeneration.Tests
{
    using System;
    public static class GeneratedResolutions
    {
        public static HashTree<KV<Type, object>, FactoryDelegate> 
            Resolutions = HashTree<KV<Type, object>, FactoryDelegate>.Empty;

        static GeneratedResolutions()
        {
            Type serviceType;
            object serviceKey;

            serviceType = typeof(DryIoc.UnitTests.CUT.IService);
            serviceKey = DefaultKey.Of(0);
            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(serviceType, serviceKey),
                (state, r, scope) => new ServiceWithInstanceCount());

            serviceType = typeof(DryIoc.UnitTests.CUT.IService);
            serviceKey = DefaultKey.Of(2);
            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(serviceType, serviceKey),
                (state, r, scope) => new Service());

            serviceType = typeof(DryIoc.UnitTests.CUT.IService);
            serviceKey = DefaultKey.Of(3);
            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(serviceType, serviceKey),
                (state, r, scope) => new AnotherService());

            serviceType = typeof(DryIoc.UnitTests.CUT.IService);
            serviceKey = DefaultKey.Of(4);
            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(serviceType, serviceKey),
                (state, r, scope) => new ServiceWithDependency(new LazyTests.BarDependency()));

            serviceType = typeof(DryIoc.UnitTests.CUT.IService);
            serviceKey = DefaultKey.Of(5);
            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(serviceType, serviceKey),
                (state, r, scope) => new AnotherServiceWithDependency(new LazyTests.BarDependency()));

            serviceType = typeof(DryIoc.UnitTests.CUT.IService);
            serviceKey = DefaultKey.Of(6);
            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(serviceType, serviceKey),
                (state, r, scope) => new ServiceWithSingletonDependency(new Singleton()));

            serviceType = typeof(DryIoc.UnitTests.CUT.IService);
            serviceKey = DefaultKey.Of(7);
            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(serviceType, serviceKey),
                (state, r, scope) => new ServiceWithEnumerableDependencies(new IDependency[] { new Dependency(), new Foo1(), new Foo2(), new Foo3(), new FooWithDependency(new ReuseInCurrentScopeTests.IndependentService()), new FooWithFuncOfDependency(() => new ReuseInCurrentScopeTests.IndependentService()), new LazyTests.BarDependency() }));

            serviceType = typeof(DryIoc.UnitTests.CUT.IService);
            serviceKey = DefaultKey.Of(8);
            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(serviceType, serviceKey),
                (state, r, scope) => new ServiceWithManyDependencies(new LazyEnumerable<IDependency>(r.Resolver.ResolveMany(typeof(IDependency), (object)state.Get(7), typeof(IDependency), (object)state.Get(7)).Cast())));

            serviceType = typeof(DryIoc.UnitTests.CUT.IService);
            serviceKey = DefaultKey.Of(9);
            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(serviceType, serviceKey),
                (state, r, scope) => new ServiceWithLazyDependency(new Lazy<IDependency>(() => r.Resolver.Resolve(null, IfUnresolved.ReturnDefault, default(Type)))));

            serviceType = typeof(DryIoc.UnitTests.CUT.IService);
            serviceKey = DefaultKey.Of(10);
            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(serviceType, serviceKey),
                (state, r, scope) => new AnotherServiceWithLazyDependency(new Lazy<IDependency>(() => r.Resolver.Resolve(null, IfUnresolved.ReturnDefault, default(Type)))));

            serviceType = typeof(DryIoc.UnitTests.CUT.IService);
            serviceKey = DefaultKey.Of(11);
            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(serviceType, serviceKey),
                (state, r, scope) => new ServiceWithRecursiveDependency(new LazyTests.BarDependency()));

            serviceType = typeof(DryIoc.UnitTests.CUT.IService);
            serviceKey = DefaultKey.Of(12);
            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(serviceType, serviceKey),
                (state, r, scope) => new ServiceWithFuncOfRecursiveDependency(() => new LazyTests.BarDependency()));

            serviceType = typeof(DryIoc.UnitTests.CUT.IService);
            serviceKey = DefaultKey.Of(15);
            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(serviceType, serviceKey),
                (state, r, scope) => new DisposableService());

            serviceType = typeof(DryIoc.UnitTests.CUT.IService);
            serviceKey = DefaultKey.Of(16);
            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(serviceType, serviceKey),
                (state, r, scope) => new ReuseInCurrentScopeTests.IndependentService());
