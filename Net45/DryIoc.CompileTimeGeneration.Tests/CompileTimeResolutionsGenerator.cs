
namespace DryIoc.CompileTimeGeneration.Tests
{
    using System;
    public static class GeneratedResolutions
    {
        public static HashTree<KV<Type, object>, FactoryDelegate> 
            Resolutions = HashTree<KV<Type, object>, FactoryDelegate>.Empty;

        static GeneratedResolutions()
        {

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.InitializerTests.InitializableService), null),
                (state, r, scope) => new DryIoc.UnitTests.InitializerTests.InitializableService());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ConstructionTests), null),
                (state, r, scope) => new DryIoc.UnitTests.ConstructionTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RegisterManyTests.AnotherBlah), null),
                (state, r, scope) => new DryIoc.UnitTests.RegisterManyTests.AnotherBlah());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CompositePatternTests.Circle), null),
                (state, r, scope) => new DryIoc.UnitTests.CompositePatternTests.Circle());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RegisterManyTests), null),
                (state, r, scope) => new DryIoc.UnitTests.RegisterManyTests());

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.Params using provided constructor selector.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ChildContainerTests.Orange), null),
                (state, r, scope) => new DryIoc.UnitTests.ChildContainerTests.Orange());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.ComplexCreativity), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.ComplexCreativity());

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.AnotherService), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.AnotherService());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.DecoratorConditionTests), null),
                (state, r, scope) => new DryIoc.UnitTests.DecoratorConditionTests());

/* Exception: ContainerException
----------------------
Unable to find constructor with all resolvable parameters when resolving DryIoc.UnitTests.RulesTests.Bla<>
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.RulesTests.Bla<>}.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IService but found many:
[DefaultKey.Of(0), {FactoryID=2295, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCount}];
[DefaultKey.Of(1), {FactoryID=2296, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCountWithStringParam}];
[DefaultKey.Of(2), {FactoryID=2297, ImplType=DryIoc.UnitTests.CUT.Service}];
[DefaultKey.Of(3), {FactoryID=2298, ImplType=DryIoc.UnitTests.CUT.AnotherService}];
[DefaultKey.Of(4), {FactoryID=2299, ImplType=DryIoc.UnitTests.CUT.ServiceWithDependency}];
[DefaultKey.Of(5), {FactoryID=2300, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithDependency}];
[DefaultKey.Of(6), {FactoryID=2301, ImplType=DryIoc.UnitTests.CUT.ServiceWithSingletonDependency}];
[DefaultKey.Of(7), {FactoryID=2302, ImplType=DryIoc.UnitTests.CUT.ServiceWithEnumerableDependencies}];
[DefaultKey.Of(8), {FactoryID=2303, ImplType=DryIoc.UnitTests.CUT.ServiceWithManyDependencies}];
[DefaultKey.Of(9), {FactoryID=2304, ImplType=DryIoc.UnitTests.CUT.ServiceWithLazyDependency}];
[DefaultKey.Of(10), {FactoryID=2305, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithLazyDependency}];
[DefaultKey.Of(11), {FactoryID=2313, ImplType=DryIoc.UnitTests.CUT.ServiceWithRecursiveDependency}];
[DefaultKey.Of(12), {FactoryID=2314, ImplType=DryIoc.UnitTests.CUT.ServiceWithFuncOfRecursiveDependency}];
[DefaultKey.Of(13), {FactoryID=2324, ImplType=DryIoc.UnitTests.CUT.ServiceWithOnePrimitiveParameter}];
[DefaultKey.Of(14), {FactoryID=2325, ImplType=DryIoc.UnitTests.CUT.ServiceWithTwoPrimitiveParameters}];
[DefaultKey.Of(15), {FactoryID=2340, ImplType=DryIoc.UnitTests.CUT.DisposableService}];
[DefaultKey.Of(16), {FactoryID=2453, ImplType=DryIoc.UnitTests.ReuseInCurrentScopeTests.IndependentService}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Singleton), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Singleton());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(System.Runtime.InteropServices._Attribute), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.ImportAttribute());

/* Exception: ContainerException
----------------------
Unable to resolve Object as parameter "metadata"
 in DryIoc.UnitTests.RulesTests.ImportWithMetadataAttribute: _Attribute {DefaultKey.Of(1)}
 in wrapper DryIoc.FactoryExpression<Object> {required: _Attribute, DefaultKey.Of(1)}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ContainerTests), null),
                (state, r, scope) => new DryIoc.UnitTests.ContainerTests());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IDependency but found many:
[DefaultKey.Of(0), {FactoryID=2308, ImplType=DryIoc.UnitTests.CUT.Dependency}];
[DefaultKey.Of(1), {FactoryID=2309, ImplType=DryIoc.UnitTests.CUT.Foo1}];
[DefaultKey.Of(2), {FactoryID=2310, ImplType=DryIoc.UnitTests.CUT.Foo2}];
[DefaultKey.Of(3), {FactoryID=2311, ImplType=DryIoc.UnitTests.CUT.Foo3}];
[DefaultKey.Of(4), {FactoryID=2315, ImplType=DryIoc.UnitTests.CUT.FooWithDependency}];
[DefaultKey.Of(5), {FactoryID=2316, ImplType=DryIoc.UnitTests.CUT.FooWithFuncOfDependency}];
[DefaultKey.Of(6), {FactoryID=2406, ImplType=DryIoc.UnitTests.LazyTests.BarDependency}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.Fuzz`1[T] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Context1), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Context1());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Foo2), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Foo2());

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.RegisterManyTests+Blah`2[T0,T1] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Dependency), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Dependency());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.EnumerableAndArrayTests), null),
                (state, r, scope) => new DryIoc.UnitTests.EnumerableAndArrayTests());

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.RulesTests+TransientOpenGenericService`1[T] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.IB), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.B());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.IB), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.A());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ReuseTests.ServiceWithResolutionAndSingletonDependencies), null),
                (state, r, scope) => new DryIoc.UnitTests.ReuseTests.ServiceWithResolutionAndSingletonDependencies(new DryIoc.UnitTests.ReuseTests.SingletonDep(new DryIoc.UnitTests.ReuseTests.ResolutionScopeDep()), new DryIoc.UnitTests.ReuseTests.ResolutionScopeDep()));

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.AppendableArrayTests), null),
                (state, r, scope) => new DryIoc.UnitTests.AppendableArrayTests());

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.OpenGenericServiceWithTwoParameters`2[T1,T2] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IBar), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Bar());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IService but found many:
[DefaultKey.Of(0), {FactoryID=2295, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCount}];
[DefaultKey.Of(1), {FactoryID=2296, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCountWithStringParam}];
[DefaultKey.Of(2), {FactoryID=2297, ImplType=DryIoc.UnitTests.CUT.Service}];
[DefaultKey.Of(3), {FactoryID=2298, ImplType=DryIoc.UnitTests.CUT.AnotherService}];
[DefaultKey.Of(4), {FactoryID=2299, ImplType=DryIoc.UnitTests.CUT.ServiceWithDependency}];
[DefaultKey.Of(5), {FactoryID=2300, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithDependency}];
[DefaultKey.Of(6), {FactoryID=2301, ImplType=DryIoc.UnitTests.CUT.ServiceWithSingletonDependency}];
[DefaultKey.Of(7), {FactoryID=2302, ImplType=DryIoc.UnitTests.CUT.ServiceWithEnumerableDependencies}];
[DefaultKey.Of(8), {FactoryID=2303, ImplType=DryIoc.UnitTests.CUT.ServiceWithManyDependencies}];
[DefaultKey.Of(9), {FactoryID=2304, ImplType=DryIoc.UnitTests.CUT.ServiceWithLazyDependency}];
[DefaultKey.Of(10), {FactoryID=2305, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithLazyDependency}];
[DefaultKey.Of(11), {FactoryID=2313, ImplType=DryIoc.UnitTests.CUT.ServiceWithRecursiveDependency}];
[DefaultKey.Of(12), {FactoryID=2314, ImplType=DryIoc.UnitTests.CUT.ServiceWithFuncOfRecursiveDependency}];
[DefaultKey.Of(13), {FactoryID=2324, ImplType=DryIoc.UnitTests.CUT.ServiceWithOnePrimitiveParameter}];
[DefaultKey.Of(14), {FactoryID=2325, ImplType=DryIoc.UnitTests.CUT.ServiceWithTwoPrimitiveParameters}];
[DefaultKey.Of(15), {FactoryID=2340, ImplType=DryIoc.UnitTests.CUT.DisposableService}];
[DefaultKey.Of(16), {FactoryID=2453, ImplType=DryIoc.UnitTests.ReuseInCurrentScopeTests.IndependentService}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IService but found many:
[DefaultKey.Of(0), {FactoryID=2295, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCount}];
[DefaultKey.Of(1), {FactoryID=2296, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCountWithStringParam}];
[DefaultKey.Of(2), {FactoryID=2297, ImplType=DryIoc.UnitTests.CUT.Service}];
[DefaultKey.Of(3), {FactoryID=2298, ImplType=DryIoc.UnitTests.CUT.AnotherService}];
[DefaultKey.Of(4), {FactoryID=2299, ImplType=DryIoc.UnitTests.CUT.ServiceWithDependency}];
[DefaultKey.Of(5), {FactoryID=2300, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithDependency}];
[DefaultKey.Of(6), {FactoryID=2301, ImplType=DryIoc.UnitTests.CUT.ServiceWithSingletonDependency}];
[DefaultKey.Of(7), {FactoryID=2302, ImplType=DryIoc.UnitTests.CUT.ServiceWithEnumerableDependencies}];
[DefaultKey.Of(8), {FactoryID=2303, ImplType=DryIoc.UnitTests.CUT.ServiceWithManyDependencies}];
[DefaultKey.Of(9), {FactoryID=2304, ImplType=DryIoc.UnitTests.CUT.ServiceWithLazyDependency}];
[DefaultKey.Of(10), {FactoryID=2305, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithLazyDependency}];
[DefaultKey.Of(11), {FactoryID=2313, ImplType=DryIoc.UnitTests.CUT.ServiceWithRecursiveDependency}];
[DefaultKey.Of(12), {FactoryID=2314, ImplType=DryIoc.UnitTests.CUT.ServiceWithFuncOfRecursiveDependency}];
[DefaultKey.Of(13), {FactoryID=2324, ImplType=DryIoc.UnitTests.CUT.ServiceWithOnePrimitiveParameter}];
[DefaultKey.Of(14), {FactoryID=2325, ImplType=DryIoc.UnitTests.CUT.ServiceWithTwoPrimitiveParameters}];
[DefaultKey.Of(15), {FactoryID=2340, ImplType=DryIoc.UnitTests.CUT.DisposableService}];
[DefaultKey.Of(16), {FactoryID=2453, ImplType=DryIoc.UnitTests.ReuseInCurrentScopeTests.IndependentService}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.UnitTests.CUT.ServiceWithOnePrimitiveParameter
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CUT.ServiceWithOnePrimitiveParameter}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.OpenGenericsTests+BananaSplit`2[T1,T2] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CompositePatternTests.IShape), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.CompositePatternTests.Circle());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CompositePatternTests.IShape), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.CompositePatternTests.Square());

/* Exception: ContainerException
----------------------
Recursive dependency is detected when resolving
DryIoc.UnitTests.CompositePatternTests.IShape {DefaultKey.Of(2), allow default} <--recursive
 in wrapper DryIoc.UnitTests.CompositePatternTests.IShape[] {allow default} as parameter "shapes"
 in DryIoc.UnitTests.CompositePatternTests.PolygonOfArray: DryIoc.UnitTests.CompositePatternTests.IShape {DefaultKey.Of(4), allow default}
 in wrapper IEnumerable<DryIoc.UnitTests.CompositePatternTests.IShape> as parameter "shapes"
 in DryIoc.UnitTests.CompositePatternTests.PolygonOfEnumerable: DryIoc.UnitTests.CompositePatternTests.IShape {DefaultKey.Of(2)} <--recursive
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CompositePatternTests.IShape, DefaultKey.Of(2)}.
*/

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

/* Exception: ContainerException
----------------------
Recursive dependency is detected when resolving
DryIoc.UnitTests.CompositePatternTests.IShape {DefaultKey.Of(4), allow default} <--recursive
 in wrapper IEnumerable<DryIoc.UnitTests.CompositePatternTests.IShape> {allow default} as parameter "shapes"
 in DryIoc.UnitTests.CompositePatternTests.PolygonOfEnumerable: DryIoc.UnitTests.CompositePatternTests.IShape {DefaultKey.Of(2), allow default}
 in wrapper DryIoc.UnitTests.CompositePatternTests.IShape[] as parameter "shapes"
 in DryIoc.UnitTests.CompositePatternTests.PolygonOfArray: DryIoc.UnitTests.CompositePatternTests.IShape {DefaultKey.Of(4)} <--recursive
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CompositePatternTests.IShape, DefaultKey.Of(4)}.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.CUT.EnumKey using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.UnitTests.CUT.ServiceWithInstanceCountWithStringParam
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CUT.ServiceWithInstanceCountWithStringParam}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.Buzz), null),
                (state, r, scope) => new DryIoc.UnitTests.Buzz());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RegisterManyTests.Someberry), null),
                (state, r, scope) => new DryIoc.UnitTests.RegisterManyTests.Someberry());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.SomeOperation), null),
                (state, r, scope) => new DryIoc.UnitTests.SomeOperation());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RefTests), null),
                (state, r, scope) => new DryIoc.UnitTests.RefTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.StronglyTypeConstructorAndParametersSpecTests), null),
                (state, r, scope) => new DryIoc.UnitTests.StronglyTypeConstructorAndParametersSpecTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.AsyncExecutionFlowScopeContextTests), null),
                (state, r, scope) => new DryIoc.UnitTests.AsyncExecutionFlowScopeContextTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ChildContainerTests.Melon), null),
                (state, r, scope) => new DryIoc.UnitTests.ChildContainerTests.Melon());

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.CUT.Service`1[T] is a generic type definition
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.ChildContainerTests.IFruit but found many:
[DefaultKey.Of(0), {FactoryID=2283, ImplType=DryIoc.UnitTests.ChildContainerTests.Orange}];
[DefaultKey.Of(1), {FactoryID=2284, ImplType=DryIoc.UnitTests.ChildContainerTests.Mango}];
[DefaultKey.Of(2), {FactoryID=2285, ImplType=DryIoc.UnitTests.ChildContainerTests.Melon}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.RegisterManyTests+AnotherBlah`1[T] is a generic type definition
*/

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.UnitTests.CUT.IService<>.
*/

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.UnitTests.CUT.IService<>.
*/

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.UnitTests.CUT.IService<>.
*/

/* Exception: ContainerException
----------------------
Unable to match service with open-generic DryIoc.UnitTests.OpenGenericsTests.BuzzDiffArgCount<,> implementing IFizz<Wrap<T2>, T1> when resolving DryIoc.UnitTests.OpenGenericsTests.IFizz<,>
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.OpenGenericsTests.IFizz<,>}.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.ILogger but found many:
[DefaultKey.Of(0), {FactoryID=2345, ImplType=DryIoc.UnitTests.CUT.FastLogger}];
[DefaultKey.Of(1), {FactoryID=2349, ImplType=DryIoc.UnitTests.CUT.Logger1}];
[DefaultKey.Of(2), {FactoryID=2350, ImplType=DryIoc.UnitTests.CUT.Logger2}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.ChildContainerTests.IFruit but found many:
[DefaultKey.Of(0), {FactoryID=2283, ImplType=DryIoc.UnitTests.ChildContainerTests.Orange}];
[DefaultKey.Of(1), {FactoryID=2284, ImplType=DryIoc.UnitTests.ChildContainerTests.Mango}];
[DefaultKey.Of(2), {FactoryID=2285, ImplType=DryIoc.UnitTests.ChildContainerTests.Melon}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.ILogger), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.FastLogger());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.ILogger), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Logger1());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.ILogger), DefaultKey.Of(2)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Logger2());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IService<Int32> but found many:
[DefaultKey.Of(0), {FactoryID=2318, ImplType=DryIoc.UnitTests.CUT.Service<>}];
[DefaultKey.Of(1), {FactoryID=2319, ImplType=DryIoc.UnitTests.CUT.ServiceWithGenericParameter<>}];
[DefaultKey.Of(2), {FactoryID=2321, ImplType=DryIoc.UnitTests.CUT.ServiceWithGenericDependency<>}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IService but found many:
[DefaultKey.Of(0), {FactoryID=2295, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCount}];
[DefaultKey.Of(1), {FactoryID=2296, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCountWithStringParam}];
[DefaultKey.Of(2), {FactoryID=2297, ImplType=DryIoc.UnitTests.CUT.Service}];
[DefaultKey.Of(3), {FactoryID=2298, ImplType=DryIoc.UnitTests.CUT.AnotherService}];
[DefaultKey.Of(4), {FactoryID=2299, ImplType=DryIoc.UnitTests.CUT.ServiceWithDependency}];
[DefaultKey.Of(5), {FactoryID=2300, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithDependency}];
[DefaultKey.Of(6), {FactoryID=2301, ImplType=DryIoc.UnitTests.CUT.ServiceWithSingletonDependency}];
[DefaultKey.Of(7), {FactoryID=2302, ImplType=DryIoc.UnitTests.CUT.ServiceWithEnumerableDependencies}];
[DefaultKey.Of(8), {FactoryID=2303, ImplType=DryIoc.UnitTests.CUT.ServiceWithManyDependencies}];
[DefaultKey.Of(9), {FactoryID=2304, ImplType=DryIoc.UnitTests.CUT.ServiceWithLazyDependency}];
[DefaultKey.Of(10), {FactoryID=2305, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithLazyDependency}];
[DefaultKey.Of(11), {FactoryID=2313, ImplType=DryIoc.UnitTests.CUT.ServiceWithRecursiveDependency}];
[DefaultKey.Of(12), {FactoryID=2314, ImplType=DryIoc.UnitTests.CUT.ServiceWithFuncOfRecursiveDependency}];
[DefaultKey.Of(13), {FactoryID=2324, ImplType=DryIoc.UnitTests.CUT.ServiceWithOnePrimitiveParameter}];
[DefaultKey.Of(14), {FactoryID=2325, ImplType=DryIoc.UnitTests.CUT.ServiceWithTwoPrimitiveParameters}];
[DefaultKey.Of(15), {FactoryID=2340, ImplType=DryIoc.UnitTests.CUT.DisposableService}];
[DefaultKey.Of(16), {FactoryID=2453, ImplType=DryIoc.UnitTests.ReuseInCurrentScopeTests.IndependentService}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.SomeOperation`1[T] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RegisterManyTests.ProductEater), null),
                (state, r, scope) => new DryIoc.UnitTests.RegisterManyTests.ProductEater(new DryIoc.UnitTests.RegisterManyTests.Someberry()));

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RegisterWithNonStringServiceKeyTests), null),
                (state, r, scope) => new DryIoc.UnitTests.RegisterWithNonStringServiceKeyTests());

/* Exception: ContainerException
----------------------
Unable to resolve Object as parameter "metadata"
 in DryIoc.UnitTests.RulesTests.ImportWithMetadataAttribute
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.RulesTests.ImportWithMetadataAttribute}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.FunkyChicken), null),
                (state, r, scope) => new DryIoc.UnitTests.FunkyChicken());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.IOperation but found many:
[DefaultKey.Of(0), {FactoryID=2369, ImplType=DryIoc.UnitTests.SomeOperation}];
[DefaultKey.Of(1), {FactoryID=2370, ImplType=DryIoc.UnitTests.AnotherOperation}];
[DefaultKey.Of(2), {FactoryID=2371, ImplType=DryIoc.UnitTests.ParameterizedOperation}];
[DefaultKey.Of(3), {FactoryID=2372, ImplType=DryIoc.UnitTests.MeasureExecutionTimeOperationDecorator}];
[DefaultKey.Of(4), {FactoryID=2374, ImplType=DryIoc.UnitTests.RetryOperationDecorator}];
[DefaultKey.Of(5), {FactoryID=2378, ImplType=DryIoc.UnitTests.LazyDecorator}];
[DefaultKey.Of(6), {FactoryID=2379, ImplType=DryIoc.UnitTests.FuncWithArgDecorator}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ThrowTests), null),
                (state, r, scope) => new DryIoc.UnitTests.ThrowTests());

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.CUT.ServiceWithGenericParameter`1[T] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ChildContainerTests.IFruit), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.ChildContainerTests.Orange());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ChildContainerTests.IFruit), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.ChildContainerTests.Mango());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ChildContainerTests.IFruit), DefaultKey.Of(2)),
                (state, r, scope) => new DryIoc.UnitTests.ChildContainerTests.Melon());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.PrintTests), null),
                (state, r, scope) => new DryIoc.UnitTests.PrintTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(System.Attribute), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.ImportAttribute());

/* Exception: ContainerException
----------------------
Unable to resolve Object as parameter "metadata"
 in DryIoc.UnitTests.RulesTests.ImportWithMetadataAttribute: Attribute {DefaultKey.Of(1)}
 in wrapper DryIoc.FactoryExpression<Object> {required: Attribute, DefaultKey.Of(1)}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments), null),
                (state, r, scope) => new DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IService but found many:
[DefaultKey.Of(0), {FactoryID=2295, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCount}];
[DefaultKey.Of(1), {FactoryID=2296, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCountWithStringParam}];
[DefaultKey.Of(2), {FactoryID=2297, ImplType=DryIoc.UnitTests.CUT.Service}];
[DefaultKey.Of(3), {FactoryID=2298, ImplType=DryIoc.UnitTests.CUT.AnotherService}];
[DefaultKey.Of(4), {FactoryID=2299, ImplType=DryIoc.UnitTests.CUT.ServiceWithDependency}];
[DefaultKey.Of(5), {FactoryID=2300, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithDependency}];
[DefaultKey.Of(6), {FactoryID=2301, ImplType=DryIoc.UnitTests.CUT.ServiceWithSingletonDependency}];
[DefaultKey.Of(7), {FactoryID=2302, ImplType=DryIoc.UnitTests.CUT.ServiceWithEnumerableDependencies}];
[DefaultKey.Of(8), {FactoryID=2303, ImplType=DryIoc.UnitTests.CUT.ServiceWithManyDependencies}];
[DefaultKey.Of(9), {FactoryID=2304, ImplType=DryIoc.UnitTests.CUT.ServiceWithLazyDependency}];
[DefaultKey.Of(10), {FactoryID=2305, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithLazyDependency}];
[DefaultKey.Of(11), {FactoryID=2313, ImplType=DryIoc.UnitTests.CUT.ServiceWithRecursiveDependency}];
[DefaultKey.Of(12), {FactoryID=2314, ImplType=DryIoc.UnitTests.CUT.ServiceWithFuncOfRecursiveDependency}];
[DefaultKey.Of(13), {FactoryID=2324, ImplType=DryIoc.UnitTests.CUT.ServiceWithOnePrimitiveParameter}];
[DefaultKey.Of(14), {FactoryID=2325, ImplType=DryIoc.UnitTests.CUT.ServiceWithTwoPrimitiveParameters}];
[DefaultKey.Of(15), {FactoryID=2340, ImplType=DryIoc.UnitTests.CUT.DisposableService}];
[DefaultKey.Of(16), {FactoryID=2453, ImplType=DryIoc.UnitTests.ReuseInCurrentScopeTests.IndependentService}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.MetadataDrivenFactory`2[TService,TMetadata] is a generic type definition
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.ILogger but found many:
[DefaultKey.Of(0), {FactoryID=2345, ImplType=DryIoc.UnitTests.CUT.FastLogger}];
[DefaultKey.Of(1), {FactoryID=2349, ImplType=DryIoc.UnitTests.CUT.Logger1}];
[DefaultKey.Of(2), {FactoryID=2350, ImplType=DryIoc.UnitTests.CUT.Logger2}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "name"
 in DryIoc.UnitTests.FuncTests.FullNameService
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.FuncTests.FullNameService}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ImportAttribute), null),
                (state, r, scope) => new DryIoc.UnitTests.ImportAttribute());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CompositePatternTests), null),
                (state, r, scope) => new DryIoc.UnitTests.CompositePatternTests());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IService but found many:
[DefaultKey.Of(0), {FactoryID=2295, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCount}];
[DefaultKey.Of(1), {FactoryID=2296, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCountWithStringParam}];
[DefaultKey.Of(2), {FactoryID=2297, ImplType=DryIoc.UnitTests.CUT.Service}];
[DefaultKey.Of(3), {FactoryID=2298, ImplType=DryIoc.UnitTests.CUT.AnotherService}];
[DefaultKey.Of(4), {FactoryID=2299, ImplType=DryIoc.UnitTests.CUT.ServiceWithDependency}];
[DefaultKey.Of(5), {FactoryID=2300, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithDependency}];
[DefaultKey.Of(6), {FactoryID=2301, ImplType=DryIoc.UnitTests.CUT.ServiceWithSingletonDependency}];
[DefaultKey.Of(7), {FactoryID=2302, ImplType=DryIoc.UnitTests.CUT.ServiceWithEnumerableDependencies}];
[DefaultKey.Of(8), {FactoryID=2303, ImplType=DryIoc.UnitTests.CUT.ServiceWithManyDependencies}];
[DefaultKey.Of(9), {FactoryID=2304, ImplType=DryIoc.UnitTests.CUT.ServiceWithLazyDependency}];
[DefaultKey.Of(10), {FactoryID=2305, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithLazyDependency}];
[DefaultKey.Of(11), {FactoryID=2313, ImplType=DryIoc.UnitTests.CUT.ServiceWithRecursiveDependency}];
[DefaultKey.Of(12), {FactoryID=2314, ImplType=DryIoc.UnitTests.CUT.ServiceWithFuncOfRecursiveDependency}];
[DefaultKey.Of(13), {FactoryID=2324, ImplType=DryIoc.UnitTests.CUT.ServiceWithOnePrimitiveParameter}];
[DefaultKey.Of(14), {FactoryID=2325, ImplType=DryIoc.UnitTests.CUT.ServiceWithTwoPrimitiveParameters}];
[DefaultKey.Of(15), {FactoryID=2340, ImplType=DryIoc.UnitTests.CUT.DisposableService}];
[DefaultKey.Of(16), {FactoryID=2453, ImplType=DryIoc.UnitTests.ReuseInCurrentScopeTests.IndependentService}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Foo1), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Foo1());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.B), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.B());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.B), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.A());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Bar), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Bar());

/* Exception: ContainerException
----------------------
Unable to resolve T0 as parameter "arg0"
 in DryIoc.UnitTests.WrapperWithTwoArgs<,>
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.WrapperWithTwoArgs<,>}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.ILogger but found many:
[DefaultKey.Of(0), {FactoryID=2345, ImplType=DryIoc.UnitTests.CUT.FastLogger}];
[DefaultKey.Of(1), {FactoryID=2349, ImplType=DryIoc.UnitTests.CUT.Logger1}];
[DefaultKey.Of(2), {FactoryID=2350, ImplType=DryIoc.UnitTests.CUT.Logger2}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.FunnyDuckling), null),
                (state, r, scope) => new DryIoc.UnitTests.FunnyDuckling());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.Metadata), null),
                (state, r, scope) => new DryIoc.UnitTests.Metadata());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IService but found many:
[DefaultKey.Of(0), {FactoryID=2295, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCount}];
[DefaultKey.Of(1), {FactoryID=2296, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCountWithStringParam}];
[DefaultKey.Of(2), {FactoryID=2297, ImplType=DryIoc.UnitTests.CUT.Service}];
[DefaultKey.Of(3), {FactoryID=2298, ImplType=DryIoc.UnitTests.CUT.AnotherService}];
[DefaultKey.Of(4), {FactoryID=2299, ImplType=DryIoc.UnitTests.CUT.ServiceWithDependency}];
[DefaultKey.Of(5), {FactoryID=2300, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithDependency}];
[DefaultKey.Of(6), {FactoryID=2301, ImplType=DryIoc.UnitTests.CUT.ServiceWithSingletonDependency}];
[DefaultKey.Of(7), {FactoryID=2302, ImplType=DryIoc.UnitTests.CUT.ServiceWithEnumerableDependencies}];
[DefaultKey.Of(8), {FactoryID=2303, ImplType=DryIoc.UnitTests.CUT.ServiceWithManyDependencies}];
[DefaultKey.Of(9), {FactoryID=2304, ImplType=DryIoc.UnitTests.CUT.ServiceWithLazyDependency}];
[DefaultKey.Of(10), {FactoryID=2305, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithLazyDependency}];
[DefaultKey.Of(11), {FactoryID=2313, ImplType=DryIoc.UnitTests.CUT.ServiceWithRecursiveDependency}];
[DefaultKey.Of(12), {FactoryID=2314, ImplType=DryIoc.UnitTests.CUT.ServiceWithFuncOfRecursiveDependency}];
[DefaultKey.Of(13), {FactoryID=2324, ImplType=DryIoc.UnitTests.CUT.ServiceWithOnePrimitiveParameter}];
[DefaultKey.Of(14), {FactoryID=2325, ImplType=DryIoc.UnitTests.CUT.ServiceWithTwoPrimitiveParameters}];
[DefaultKey.Of(15), {FactoryID=2340, ImplType=DryIoc.UnitTests.CUT.DisposableService}];
[DefaultKey.Of(16), {FactoryID=2453, ImplType=DryIoc.UnitTests.ReuseInCurrentScopeTests.IndependentService}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ContainerException
----------------------
Unable to find constructor with all resolvable parameters when resolving DryIoc.UnitTests.CUT.ServiceWithMultipleCostructors
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CUT.ServiceWithMultipleCostructors}.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ContextDependentResolutionTests.ILogger), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.ContextDependentResolutionTests.PlainLogger());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ContextDependentResolutionTests.ILogger), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.ContextDependentResolutionTests.FastLogger());

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.OpenGenericsTests+DoubleMultiNested`2[T1,T2] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.IfAlreadyRegisteredTests), null),
                (state, r, scope) => new DryIoc.UnitTests.IfAlreadyRegisteredTests());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IService but found many:
[DefaultKey.Of(0), {FactoryID=2295, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCount}];
[DefaultKey.Of(1), {FactoryID=2296, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCountWithStringParam}];
[DefaultKey.Of(2), {FactoryID=2297, ImplType=DryIoc.UnitTests.CUT.Service}];
[DefaultKey.Of(3), {FactoryID=2298, ImplType=DryIoc.UnitTests.CUT.AnotherService}];
[DefaultKey.Of(4), {FactoryID=2299, ImplType=DryIoc.UnitTests.CUT.ServiceWithDependency}];
[DefaultKey.Of(5), {FactoryID=2300, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithDependency}];
[DefaultKey.Of(6), {FactoryID=2301, ImplType=DryIoc.UnitTests.CUT.ServiceWithSingletonDependency}];
[DefaultKey.Of(7), {FactoryID=2302, ImplType=DryIoc.UnitTests.CUT.ServiceWithEnumerableDependencies}];
[DefaultKey.Of(8), {FactoryID=2303, ImplType=DryIoc.UnitTests.CUT.ServiceWithManyDependencies}];
[DefaultKey.Of(9), {FactoryID=2304, ImplType=DryIoc.UnitTests.CUT.ServiceWithLazyDependency}];
[DefaultKey.Of(10), {FactoryID=2305, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithLazyDependency}];
[DefaultKey.Of(11), {FactoryID=2313, ImplType=DryIoc.UnitTests.CUT.ServiceWithRecursiveDependency}];
[DefaultKey.Of(12), {FactoryID=2314, ImplType=DryIoc.UnitTests.CUT.ServiceWithFuncOfRecursiveDependency}];
[DefaultKey.Of(13), {FactoryID=2324, ImplType=DryIoc.UnitTests.CUT.ServiceWithOnePrimitiveParameter}];
[DefaultKey.Of(14), {FactoryID=2325, ImplType=DryIoc.UnitTests.CUT.ServiceWithTwoPrimitiveParameters}];
[DefaultKey.Of(15), {FactoryID=2340, ImplType=DryIoc.UnitTests.CUT.DisposableService}];
[DefaultKey.Of(16), {FactoryID=2453, ImplType=DryIoc.UnitTests.ReuseInCurrentScopeTests.IndependentService}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ContextDependentResolutionTests.FastLogger), null),
                (state, r, scope) => new DryIoc.UnitTests.ContextDependentResolutionTests.FastLogger());

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.OpenGenericsTests+Nested`1[T] is a generic type definition
*/

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.UnitTests.IOperationUser<>.
*/

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.UnitTests.IOperationUser<>.
*/

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.ContextDependentResolutionTests+Logger`1[T] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.InitializerTests.IInitializable), null),
                (state, r, scope) => new DryIoc.UnitTests.InitializerTests.InitializableService());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IDependency but found many:
[DefaultKey.Of(0), {FactoryID=2308, ImplType=DryIoc.UnitTests.CUT.Dependency}];
[DefaultKey.Of(1), {FactoryID=2309, ImplType=DryIoc.UnitTests.CUT.Foo1}];
[DefaultKey.Of(2), {FactoryID=2310, ImplType=DryIoc.UnitTests.CUT.Foo2}];
[DefaultKey.Of(3), {FactoryID=2311, ImplType=DryIoc.UnitTests.CUT.Foo3}];
[DefaultKey.Of(4), {FactoryID=2315, ImplType=DryIoc.UnitTests.CUT.FooWithDependency}];
[DefaultKey.Of(5), {FactoryID=2316, ImplType=DryIoc.UnitTests.CUT.FooWithFuncOfDependency}];
[DefaultKey.Of(6), {FactoryID=2406, ImplType=DryIoc.UnitTests.LazyTests.BarDependency}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.FastHandler), null),
                (state, r, scope) => new DryIoc.UnitTests.FastHandler());

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.UnitTests.IOperation<>.
*/

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.UnitTests.IOperation<>.
*/

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.UnitTests.IOperation<>.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RulesTests.FooHey), null),
                (state, r, scope) => new DryIoc.UnitTests.RulesTests.FooHey());

/* Exception: ContainerException
----------------------
Expecting single default registration of IOperation<T> but found many:
[DefaultKey.Of(0), {FactoryID=2375, ImplType=DryIoc.UnitTests.SomeOperation<>}];
[DefaultKey.Of(1), {FactoryID=2376, ImplType=DryIoc.UnitTests.RetryOperationDecorator<>}];
[DefaultKey.Of(2), {FactoryID=2377, ImplType=DryIoc.UnitTests.MeasureExecutionTimeOperationDecorator<>}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.YetAnotherClient), null),
                (state, r, scope) => new DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.YetAnotherClient(new DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.SomeDependency()));

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RulesTests.AnotherNamedService), null),
                (state, r, scope) => new DryIoc.UnitTests.RulesTests.AnotherNamedService());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.ServiceWithAbstractBaseClass), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.ServiceWithAbstractBaseClass());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.DiagnosticsTests), null),
                (state, r, scope) => new DryIoc.UnitTests.DiagnosticsTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.ISingleton), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Singleton());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.IfUnresolvedTests), null),
                (state, r, scope) => new DryIoc.UnitTests.IfUnresolvedTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.ServiceWithSingletonDependency), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.ServiceWithSingletonDependency(new DryIoc.UnitTests.CUT.Singleton()));

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RegisterManyTests.Blah), null),
                (state, r, scope) => new DryIoc.UnitTests.RegisterManyTests.Blah());

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.Fuzz`1+NestedClazz[T] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.InjectionRulesTests.ClientWithPropsAndFields), null),
                (state, r, scope) => new DryIoc.UnitTests.InjectionRulesTests.ClientWithPropsAndFields());

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.ContextDependentResolutionTests.ILogger but found many:
[DefaultKey.Of(0), {FactoryID=2355, ImplType=DryIoc.UnitTests.ContextDependentResolutionTests.PlainLogger}];
[DefaultKey.Of(1), {FactoryID=2356, ImplType=DryIoc.UnitTests.ContextDependentResolutionTests.FastLogger}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ResolveManyTests), null),
                (state, r, scope) => new DryIoc.UnitTests.ResolveManyTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.SomeClient), null),
                (state, r, scope) => new DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.SomeClient(new DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.SomeDependency()));

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Account), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Account(new DryIoc.UnitTests.CUT.Log()));

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.ServiceColors using provided constructor selector.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.ServiceWithInstanceCount), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.ServiceWithInstanceCount());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RegisterManyTests.IBlah), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.RegisterManyTests.Blah());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RegisterManyTests.IBlah), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.RegisterManyTests.AnotherBlah());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.AbstractService), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.ServiceWithAbstractBaseClass());

/* Exception: ContainerException
----------------------
Unable to resolve DryIoc.Container as parameter "container"
 in DryIoc.UnitTests.DynamicFactoryTests.DynamicFactory<>
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.DynamicFactoryTests.DynamicFactory<>}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IDependency but found many:
[DefaultKey.Of(0), {FactoryID=2308, ImplType=DryIoc.UnitTests.CUT.Dependency}];
[DefaultKey.Of(1), {FactoryID=2309, ImplType=DryIoc.UnitTests.CUT.Foo1}];
[DefaultKey.Of(2), {FactoryID=2310, ImplType=DryIoc.UnitTests.CUT.Foo2}];
[DefaultKey.Of(3), {FactoryID=2311, ImplType=DryIoc.UnitTests.CUT.Foo3}];
[DefaultKey.Of(4), {FactoryID=2315, ImplType=DryIoc.UnitTests.CUT.FooWithDependency}];
[DefaultKey.Of(5), {FactoryID=2316, ImplType=DryIoc.UnitTests.CUT.FooWithFuncOfDependency}];
[DefaultKey.Of(6), {FactoryID=2406, ImplType=DryIoc.UnitTests.LazyTests.BarDependency}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.FuncTests.TwoCtors), null),
                (state, r, scope) => new DryIoc.UnitTests.FuncTests.TwoCtors());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.C), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.C());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.C), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.B());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.C), DefaultKey.Of(2)),
                (state, r, scope) => new DryIoc.UnitTests.A());

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.OpenGenericsTests+DoubleNested`2[T1,T2] is a generic type definition
*/

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.UnitTests.IFuzz<>.
*/

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.UnitTests.IFuzz<>.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.ContextDependentResolutionTests.ILogger but found many:
[DefaultKey.Of(0), {FactoryID=2355, ImplType=DryIoc.UnitTests.ContextDependentResolutionTests.PlainLogger}];
[DefaultKey.Of(1), {FactoryID=2356, ImplType=DryIoc.UnitTests.ContextDependentResolutionTests.FastLogger}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.AsyncExecutionFlowScopeContextTests.<Scoped_service_should_Not_propagate_over_async_boundary_with_exec_flow_context>d__7 using provided constructor selector.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.UnregisterTests), null),
                (state, r, scope) => new DryIoc.UnitTests.UnregisterTests());

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.OpenGenericsTests+Wrap`1[T] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.InjectionRulesTests.Dep), null),
                (state, r, scope) => new DryIoc.UnitTests.InjectionRulesTests.Dep());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.LazyChicken), null),
                (state, r, scope) => new DryIoc.UnitTests.LazyChicken());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RegisterManyTests.IProduct), null),
                (state, r, scope) => new DryIoc.UnitTests.RegisterManyTests.Someberry());

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.UnitTests.OpenGenericsTests.IceCream<>.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.ServiceWithTwoDepenedenciesOfTheSameType), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.ServiceWithTwoDepenedenciesOfTheSameType(new DryIoc.UnitTests.CUT.Service(), new DryIoc.UnitTests.CUT.Service()));

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.WipeCacheTests), null),
                (state, r, scope) => new DryIoc.UnitTests.WipeCacheTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.CloneableClass), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.CloneableClass());

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.OpenGenericsTests+Wrap`2[T1,T2] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.FieldHolder), null),
                (state, r, scope) => new DryIoc.UnitTests.FieldHolder());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.KeyValuePairResolutionTests), null),
                (state, r, scope) => new DryIoc.UnitTests.KeyValuePairResolutionTests());

/* Exception: ContainerException
----------------------
Recursive dependency is detected when resolving
DryIoc.UnitTests.CompositePatternTests.IShape {DefaultKey.Of(2), allow default} <--recursive
 in wrapper IEnumerable<DryIoc.UnitTests.CompositePatternTests.IShape> as parameter "shapes"
 in DryIoc.UnitTests.CompositePatternTests.PolygonOfEnumerable <--recursive
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CompositePatternTests.PolygonOfEnumerable}.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.Measurer), null),
                (state, r, scope) => new DryIoc.UnitTests.Measurer());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IContext), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Context1());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IContext), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Context2());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.Guts), null),
                (state, r, scope) => new DryIoc.UnitTests.Guts());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.ContextDependentResolutionTests.ILogger but found many:
[DefaultKey.Of(0), {FactoryID=2355, ImplType=DryIoc.UnitTests.ContextDependentResolutionTests.PlainLogger}];
[DefaultKey.Of(1), {FactoryID=2356, ImplType=DryIoc.UnitTests.ContextDependentResolutionTests.FastLogger}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "dependency"
 in DryIoc.UnitTests.ContextDependentResolutionTests.StrUser
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.ContextDependentResolutionTests.StrUser}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Foo3), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Foo3());

/* Exception: ContainerException
----------------------
Unable to resolve Int32 as parameter "scopes"
 in DryIoc.UnitTests.ReuseTests.Soose
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.ReuseTests.Soose}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.TypeCSharpNameFormattingTests), null),
                (state, r, scope) => new DryIoc.UnitTests.TypeCSharpNameFormattingTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.ServiceWithTwoParameters), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.ServiceWithTwoParameters(new DryIoc.UnitTests.CUT.Service(), new DryIoc.UnitTests.CUT.AnotherService()));

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.OpenGenericsTests+Banana`1[T] is a generic type definition
*/

/* Exception: ContainerException
----------------------
Unable to resolve wrapper Meta<Func<IOperation<T>>, String> as parameter "getOperation"
 in DryIoc.UnitTests.OperationUser<>
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.OperationUser<>}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.UnitTests.RegisterManyTests.IBlah<,>.
*/

/* Exception: ContainerException
----------------------
Unable to match service with open-generic DryIoc.UnitTests.RegisterManyTests.AnotherBlah<> implementing IBlah<String, T> when resolving DryIoc.UnitTests.RegisterManyTests.IBlah<,> {DefaultKey.Of(1)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.RegisterManyTests.IBlah<,>, DefaultKey.Of(1)}.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RegisterManyTests.SomeEater), null),
                (state, r, scope) => new DryIoc.UnitTests.RegisterManyTests.SomeEater(new DryIoc.UnitTests.RegisterManyTests.Someberry()));

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IDependency), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Dependency());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IDependency), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Foo1());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IDependency), DefaultKey.Of(2)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Foo2());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IDependency), DefaultKey.Of(3)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Foo3());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IService but found many:
[DefaultKey.Of(0), {FactoryID=2295, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCount}];
[DefaultKey.Of(1), {FactoryID=2296, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCountWithStringParam}];
[DefaultKey.Of(2), {FactoryID=2297, ImplType=DryIoc.UnitTests.CUT.Service}];
[DefaultKey.Of(3), {FactoryID=2298, ImplType=DryIoc.UnitTests.CUT.AnotherService}];
[DefaultKey.Of(4), {FactoryID=2299, ImplType=DryIoc.UnitTests.CUT.ServiceWithDependency}];
[DefaultKey.Of(5), {FactoryID=2300, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithDependency}];
[DefaultKey.Of(6), {FactoryID=2301, ImplType=DryIoc.UnitTests.CUT.ServiceWithSingletonDependency}];
[DefaultKey.Of(7), {FactoryID=2302, ImplType=DryIoc.UnitTests.CUT.ServiceWithEnumerableDependencies}];
[DefaultKey.Of(8), {FactoryID=2303, ImplType=DryIoc.UnitTests.CUT.ServiceWithManyDependencies}];
[DefaultKey.Of(9), {FactoryID=2304, ImplType=DryIoc.UnitTests.CUT.ServiceWithLazyDependency}];
[DefaultKey.Of(10), {FactoryID=2305, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithLazyDependency}];
[DefaultKey.Of(11), {FactoryID=2313, ImplType=DryIoc.UnitTests.CUT.ServiceWithRecursiveDependency}];
[DefaultKey.Of(12), {FactoryID=2314, ImplType=DryIoc.UnitTests.CUT.ServiceWithFuncOfRecursiveDependency}];
[DefaultKey.Of(13), {FactoryID=2324, ImplType=DryIoc.UnitTests.CUT.ServiceWithOnePrimitiveParameter}];
[DefaultKey.Of(14), {FactoryID=2325, ImplType=DryIoc.UnitTests.CUT.ServiceWithTwoPrimitiveParameters}];
[DefaultKey.Of(15), {FactoryID=2340, ImplType=DryIoc.UnitTests.CUT.DisposableService}];
[DefaultKey.Of(16), {FactoryID=2453, ImplType=DryIoc.UnitTests.ReuseInCurrentScopeTests.IndependentService}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IService but found many:
[DefaultKey.Of(0), {FactoryID=2295, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCount}];
[DefaultKey.Of(1), {FactoryID=2296, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCountWithStringParam}];
[DefaultKey.Of(2), {FactoryID=2297, ImplType=DryIoc.UnitTests.CUT.Service}];
[DefaultKey.Of(3), {FactoryID=2298, ImplType=DryIoc.UnitTests.CUT.AnotherService}];
[DefaultKey.Of(4), {FactoryID=2299, ImplType=DryIoc.UnitTests.CUT.ServiceWithDependency}];
[DefaultKey.Of(5), {FactoryID=2300, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithDependency}];
[DefaultKey.Of(6), {FactoryID=2301, ImplType=DryIoc.UnitTests.CUT.ServiceWithSingletonDependency}];
[DefaultKey.Of(7), {FactoryID=2302, ImplType=DryIoc.UnitTests.CUT.ServiceWithEnumerableDependencies}];
[DefaultKey.Of(8), {FactoryID=2303, ImplType=DryIoc.UnitTests.CUT.ServiceWithManyDependencies}];
[DefaultKey.Of(9), {FactoryID=2304, ImplType=DryIoc.UnitTests.CUT.ServiceWithLazyDependency}];
[DefaultKey.Of(10), {FactoryID=2305, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithLazyDependency}];
[DefaultKey.Of(11), {FactoryID=2313, ImplType=DryIoc.UnitTests.CUT.ServiceWithRecursiveDependency}];
[DefaultKey.Of(12), {FactoryID=2314, ImplType=DryIoc.UnitTests.CUT.ServiceWithFuncOfRecursiveDependency}];
[DefaultKey.Of(13), {FactoryID=2324, ImplType=DryIoc.UnitTests.CUT.ServiceWithOnePrimitiveParameter}];
[DefaultKey.Of(14), {FactoryID=2325, ImplType=DryIoc.UnitTests.CUT.ServiceWithTwoPrimitiveParameters}];
[DefaultKey.Of(15), {FactoryID=2340, ImplType=DryIoc.UnitTests.CUT.DisposableService}];
[DefaultKey.Of(16), {FactoryID=2453, ImplType=DryIoc.UnitTests.ReuseInCurrentScopeTests.IndependentService}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IDependency), DefaultKey.Of(6)),
                (state, r, scope) => new DryIoc.UnitTests.LazyTests.BarDependency());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ServiceWithMetadata), null),
                (state, r, scope) => new DryIoc.UnitTests.ServiceWithMetadata());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ContextDependentResolutionTests.PlainLogger), null),
                (state, r, scope) => new DryIoc.UnitTests.ContextDependentResolutionTests.PlainLogger());

/* Exception: ContainerException
----------------------
Expecting single default registration of IOperationUser<T> but found many:
[DefaultKey.Of(0), {FactoryID=2367, ImplType=DryIoc.UnitTests.LogUserOps<>}];
[DefaultKey.Of(1), {FactoryID=2368, ImplType=DryIoc.UnitTests.OperationUser<>}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RulesTests.INamedService), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.RulesTests.NamedService());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RulesTests.INamedService), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.RulesTests.AnotherNamedService());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IDependency but found many:
[DefaultKey.Of(0), {FactoryID=2308, ImplType=DryIoc.UnitTests.CUT.Dependency}];
[DefaultKey.Of(1), {FactoryID=2309, ImplType=DryIoc.UnitTests.CUT.Foo1}];
[DefaultKey.Of(2), {FactoryID=2310, ImplType=DryIoc.UnitTests.CUT.Foo2}];
[DefaultKey.Of(3), {FactoryID=2311, ImplType=DryIoc.UnitTests.CUT.Foo3}];
[DefaultKey.Of(4), {FactoryID=2315, ImplType=DryIoc.UnitTests.CUT.FooWithDependency}];
[DefaultKey.Of(5), {FactoryID=2316, ImplType=DryIoc.UnitTests.CUT.FooWithFuncOfDependency}];
[DefaultKey.Of(6), {FactoryID=2406, ImplType=DryIoc.UnitTests.LazyTests.BarDependency}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.FuncTests), null),
                (state, r, scope) => new DryIoc.UnitTests.FuncTests());

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.OpenGenericsTests+Closed`1[T] is a generic type definition
*/

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RulesTests.NamedService), null),
                (state, r, scope) => new DryIoc.UnitTests.RulesTests.NamedService());

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.OpenGenericsTests+Open`1[T] is a generic type definition
*/

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.UnitTests.OpenGenericsTests.Open<>.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.ClosedGenericClass), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.ClosedGenericClass());

/* Exception: ContainerException
----------------------
Unable to resolve T as parameter "dependency"
 in DryIoc.UnitTests.CUT.ServiceWithGenericDependency<>
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CUT.ServiceWithGenericDependency<>}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.CUT.ServiceWithTwoGenericParameters`2[T1,T2] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.InitializerTests.ClientOfInitializableService), null),
                (state, r, scope) => new DryIoc.UnitTests.InitializerTests.ClientOfInitializableService(new DryIoc.UnitTests.InitializerTests.InitializableService()));

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.CUT.EnumKey using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.LazyTests.X using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.LazyTests.Y using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.ServiceColors using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.RulesTests.FooMetadata using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.CUT.ServiceWithoutPublicConstructor using provided constructor selector.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.A), null),
                (state, r, scope) => new DryIoc.UnitTests.A());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ReuseInCurrentScopeTests), null),
                (state, r, scope) => new DryIoc.UnitTests.ReuseInCurrentScopeTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.LazyTests), null),
                (state, r, scope) => new DryIoc.UnitTests.LazyTests());

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.CUT.EnumKey using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.LazyTests.X using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.LazyTests.Y using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.ServiceColors using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.RulesTests.FooMetadata using provided constructor selector.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.InjectionRulesTests), null),
                (state, r, scope) => new DryIoc.UnitTests.InjectionRulesTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.IDependency), null),
                (state, r, scope) => new DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.SomeDependency());

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.CUT.EnumKey using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.LazyTests.X using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.LazyTests.Y using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.ServiceColors using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.RulesTests.FooMetadata using provided constructor selector.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ReuseTests.ResolutionScopeDep), null),
                (state, r, scope) => new DryIoc.UnitTests.ReuseTests.ResolutionScopeDep());

/* Exception: ContainerException
----------------------
Recursive dependency is detected when resolving
DryIoc.UnitTests.CompositePatternTests.IShape {DefaultKey.Of(4), allow default} <--recursive
 in wrapper IEnumerable<DryIoc.UnitTests.CompositePatternTests.IShape> {allow default} as parameter "shapes"
 in DryIoc.UnitTests.CompositePatternTests.PolygonOfEnumerable: DryIoc.UnitTests.CompositePatternTests.IShape {DefaultKey.Of(2), allow default}
 in wrapper DryIoc.UnitTests.CompositePatternTests.IShape[] as parameter "shapes"
 in DryIoc.UnitTests.CompositePatternTests.PolygonOfArray <--recursive
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CompositePatternTests.PolygonOfArray}.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ChildContainerTests), null),
                (state, r, scope) => new DryIoc.UnitTests.ChildContainerTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.DisposableService), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.DisposableService());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ChildContainerTests.Mango), null),
                (state, r, scope) => new DryIoc.UnitTests.ChildContainerTests.Mango());

/* Exception: ContainerException
----------------------
Recursive dependency is detected when resolving
DryIoc.UnitTests.CUT.GenericOne<Int32> as parameter "intOne" <--recursive
 in DryIoc.UnitTests.CUT.GenericOne<Int32> as parameter "intOne" <--recursive
 in DryIoc.UnitTests.CUT.GenericOne<>
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CUT.GenericOne<>}.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.PropertyResolutionTests), null),
                (state, r, scope) => new DryIoc.UnitTests.PropertyResolutionTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.WrapperTests), null),
                (state, r, scope) => new DryIoc.UnitTests.WrapperTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.AnotherClient), null),
                (state, r, scope) => new DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.AnotherClient(new DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.SomeDependency()));

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IDependency but found many:
[DefaultKey.Of(0), {FactoryID=2308, ImplType=DryIoc.UnitTests.CUT.Dependency}];
[DefaultKey.Of(1), {FactoryID=2309, ImplType=DryIoc.UnitTests.CUT.Foo1}];
[DefaultKey.Of(2), {FactoryID=2310, ImplType=DryIoc.UnitTests.CUT.Foo2}];
[DefaultKey.Of(3), {FactoryID=2311, ImplType=DryIoc.UnitTests.CUT.Foo3}];
[DefaultKey.Of(4), {FactoryID=2315, ImplType=DryIoc.UnitTests.CUT.FooWithDependency}];
[DefaultKey.Of(5), {FactoryID=2316, ImplType=DryIoc.UnitTests.CUT.FooWithFuncOfDependency}];
[DefaultKey.Of(6), {FactoryID=2406, ImplType=DryIoc.UnitTests.LazyTests.BarDependency}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.DynamicFactoryTests.NotRegisteredService), null),
                (state, r, scope) => new DryIoc.UnitTests.DynamicFactoryTests.NotRegisteredService());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.TypeToolsTests), null),
                (state, r, scope) => new DryIoc.UnitTests.TypeToolsTests());

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.UnitTests.OpenGenericsTests.IDouble<,>.
*/

/* Exception: ContainerException
----------------------
Unable to match service with open-generic DryIoc.UnitTests.OpenGenericsTests.DoubleNested<,> implementing IDouble<Nested<T2>, T1> when resolving DryIoc.UnitTests.OpenGenericsTests.IDouble<,> {DefaultKey.Of(1)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.OpenGenericsTests.IDouble<,>, DefaultKey.Of(1)}.
*/

/* Exception: ContainerException
----------------------
Unable to match service with open-generic DryIoc.UnitTests.OpenGenericsTests.DoubleMultiNested<,> implementing IDouble<T2, Nested<IDouble<Nested<T1>, T2>>> when resolving DryIoc.UnitTests.OpenGenericsTests.IDouble<,> {DefaultKey.Of(2)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.OpenGenericsTests.IDouble<,>, DefaultKey.Of(2)}.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.DecoratorTests), null),
                (state, r, scope) => new DryIoc.UnitTests.DecoratorTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.SomeDependency), null),
                (state, r, scope) => new DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.SomeDependency());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IFuh), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Fuh(new DryIoc.UnitTests.CUT.Bar()));

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Fuh), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Fuh(new DryIoc.UnitTests.CUT.Bar()));

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.AnotherOperation), null),
                (state, r, scope) => new DryIoc.UnitTests.AnotherOperation());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.IHandler), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.FastHandler());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.IHandler), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.SlowHandler());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.IHandler but found many:
[DefaultKey.Of(0), {FactoryID=2363, ImplType=DryIoc.UnitTests.FastHandler}];
[DefaultKey.Of(1), {FactoryID=2364, ImplType=DryIoc.UnitTests.SlowHandler}];
[DefaultKey.Of(2), {FactoryID=2365, ImplType=DryIoc.UnitTests.LoggingHandlerDecorator}];
[DefaultKey.Of(3), {FactoryID=2496, ImplType=DryIoc.UnitTests.UnregisterTests.NullHandlerDecorator}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.IHandler), DefaultKey.Of(3)),
                (state, r, scope) => new DryIoc.UnitTests.UnregisterTests.NullHandlerDecorator());

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

/* Exception: ContainerException
----------------------
Unable to resolve Boolean as parameter "flag"
 in DryIoc.UnitTests.CUT.ServiceWithParameterAndDependency
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CUT.ServiceWithParameterAndDependency}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.NewTests), null),
                (state, r, scope) => new DryIoc.UnitTests.NewTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.InitializerTests), null),
                (state, r, scope) => new DryIoc.UnitTests.InitializerTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RulesTests.SomeService), null),
                (state, r, scope) => new DryIoc.UnitTests.RulesTests.SomeService());

/* Exception: ContainerException
----------------------
Unable to resolve Boolean as parameter "flag"
 in DryIoc.UnitTests.CUT.ServiceWithTwoPrimitiveParameters
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CUT.ServiceWithTwoPrimitiveParameters}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.IA), null),
                (state, r, scope) => new DryIoc.UnitTests.A());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.ICloneable), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.CloneableClass());

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.OpenGenericsTests+BuzzDiffArgCount`2[T1,T2] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.FastLogger), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.FastLogger());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.IOperation but found many:
[DefaultKey.Of(0), {FactoryID=2369, ImplType=DryIoc.UnitTests.SomeOperation}];
[DefaultKey.Of(1), {FactoryID=2370, ImplType=DryIoc.UnitTests.AnotherOperation}];
[DefaultKey.Of(2), {FactoryID=2371, ImplType=DryIoc.UnitTests.ParameterizedOperation}];
[DefaultKey.Of(3), {FactoryID=2372, ImplType=DryIoc.UnitTests.MeasureExecutionTimeOperationDecorator}];
[DefaultKey.Of(4), {FactoryID=2374, ImplType=DryIoc.UnitTests.RetryOperationDecorator}];
[DefaultKey.Of(5), {FactoryID=2378, ImplType=DryIoc.UnitTests.LazyDecorator}];
[DefaultKey.Of(6), {FactoryID=2379, ImplType=DryIoc.UnitTests.FuncWithArgDecorator}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RulesTests.FooBlah), null),
                (state, r, scope) => new DryIoc.UnitTests.RulesTests.FooBlah());

/* Exception: ContainerException
----------------------
Unable to resolve Object as parameter "param"
 in DryIoc.UnitTests.ParameterizedOperation
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.ParameterizedOperation}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.FunnyChicken), null),
                (state, r, scope) => new DryIoc.UnitTests.FunnyChicken());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ContextDependentResolutionTests), null),
                (state, r, scope) => new DryIoc.UnitTests.ContextDependentResolutionTests());

/* Exception: ContainerException
----------------------
Expecting single default registration of IOperation<T> but found many:
[DefaultKey.Of(0), {FactoryID=2375, ImplType=DryIoc.UnitTests.SomeOperation<>}];
[DefaultKey.Of(1), {FactoryID=2376, ImplType=DryIoc.UnitTests.RetryOperationDecorator<>}];
[DefaultKey.Of(2), {FactoryID=2377, ImplType=DryIoc.UnitTests.MeasureExecutionTimeOperationDecorator<>}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(System.IDisposable), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.DisposableService());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(System.IDisposable), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.ReuseTests.SomethingDisposable());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(System.IDisposable), DefaultKey.Of(2)),
                (state, r, scope) => new DryIoc.UnitTests.ReuseTests.A());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(System.IDisposable), DefaultKey.Of(3)),
                (state, r, scope) => new DryIoc.UnitTests.ReuseTests.B());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(System.IDisposable), DefaultKey.Of(4)),
                (state, r, scope) => new DryIoc.UnitTests.ReuseTests.SomeDep());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(System.IDisposable), DefaultKey.Of(5)),
                (state, r, scope) => new DryIoc.UnitTests.AsyncExecutionFlowScopeContextTests.SomeDep());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RequiredServiceTypeTests), null),
                (state, r, scope) => new DryIoc.UnitTests.RequiredServiceTypeTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Consumer), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Consumer(new DryIoc.UnitTests.CUT.Account(new DryIoc.UnitTests.CUT.Log()), new DryIoc.UnitTests.CUT.Log()));

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.OpenGenericsTests+IceCreamSource`1[T] is a generic type definition
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CompositePatternTests.Square), null),
                (state, r, scope) => new DryIoc.UnitTests.CompositePatternTests.Square());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ReuseTests), null),
                (state, r, scope) => new DryIoc.UnitTests.ReuseTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.MetadataTests), null),
                (state, r, scope) => new DryIoc.UnitTests.MetadataTests());

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.CUT.EnumKey using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.LazyTests.X using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.LazyTests.Y using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.ServiceColors using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.RulesTests.FooMetadata using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.AsyncExecutionFlowScopeContextTests.<Scoped_service_should_Not_propagate_over_async_boundary_with_exec_flow_context>d__7 using provided constructor selector.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.LazyEnumerableTests), null),
                (state, r, scope) => new DryIoc.UnitTests.LazyEnumerableTests());

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

/* Exception: ContainerException
----------------------
Unable to resolve DryIoc.Container as parameter "container"
 in DryIoc.UnitTests.DynamicFactoryTests.DynamicFactory<DryIoc.UnitTests.DynamicFactoryTests.NotRegisteredService> as parameter "parameter"
 in DryIoc.UnitTests.DynamicFactoryTests.ServiceWithNotRegisteredLazyParameter
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.DynamicFactoryTests.ServiceWithNotRegisteredLazyParameter}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: ArgumentException
----------------------
Type DryIoc.UnitTests.OpenGenericsTests+Double`2[T1,T2] is a generic type definition
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.IHandler but found many:
[DefaultKey.Of(0), {FactoryID=2363, ImplType=DryIoc.UnitTests.FastHandler}];
[DefaultKey.Of(1), {FactoryID=2364, ImplType=DryIoc.UnitTests.SlowHandler}];
[DefaultKey.Of(2), {FactoryID=2365, ImplType=DryIoc.UnitTests.LoggingHandlerDecorator}];
[DefaultKey.Of(3), {FactoryID=2496, ImplType=DryIoc.UnitTests.UnregisterTests.NullHandlerDecorator}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.IMeasurer), null),
                (state, r, scope) => new DryIoc.UnitTests.Measurer());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RulesTests.IFooService), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.RulesTests.FooHey());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RulesTests.IFooService), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.RulesTests.FooBlah());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Context2), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Context2());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Log), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Log());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.SlowHandler), null),
                (state, r, scope) => new DryIoc.UnitTests.SlowHandler());

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Logger1), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Logger1());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.IOperation but found many:
[DefaultKey.Of(0), {FactoryID=2369, ImplType=DryIoc.UnitTests.SomeOperation}];
[DefaultKey.Of(1), {FactoryID=2370, ImplType=DryIoc.UnitTests.AnotherOperation}];
[DefaultKey.Of(2), {FactoryID=2371, ImplType=DryIoc.UnitTests.ParameterizedOperation}];
[DefaultKey.Of(3), {FactoryID=2372, ImplType=DryIoc.UnitTests.MeasureExecutionTimeOperationDecorator}];
[DefaultKey.Of(4), {FactoryID=2374, ImplType=DryIoc.UnitTests.RetryOperationDecorator}];
[DefaultKey.Of(5), {FactoryID=2378, ImplType=DryIoc.UnitTests.LazyDecorator}];
[DefaultKey.Of(6), {FactoryID=2379, ImplType=DryIoc.UnitTests.FuncWithArgDecorator}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Service), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Service());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.IBuzz), null),
                (state, r, scope) => new DryIoc.UnitTests.Buzz());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.Logger2), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Logger2());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IService), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.ServiceWithInstanceCount());

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.UnitTests.CUT.ServiceWithInstanceCountWithStringParam: DryIoc.UnitTests.CUT.IService {DefaultKey.Of(1)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CUT.IService, DefaultKey.Of(1)}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IService), DefaultKey.Of(2)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.Service());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IService), DefaultKey.Of(3)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.AnotherService());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IDependency but found many:
[DefaultKey.Of(0), {FactoryID=2308, ImplType=DryIoc.UnitTests.CUT.Dependency}];
[DefaultKey.Of(1), {FactoryID=2309, ImplType=DryIoc.UnitTests.CUT.Foo1}];
[DefaultKey.Of(2), {FactoryID=2310, ImplType=DryIoc.UnitTests.CUT.Foo2}];
[DefaultKey.Of(3), {FactoryID=2311, ImplType=DryIoc.UnitTests.CUT.Foo3}];
[DefaultKey.Of(4), {FactoryID=2315, ImplType=DryIoc.UnitTests.CUT.FooWithDependency}];
[DefaultKey.Of(5), {FactoryID=2316, ImplType=DryIoc.UnitTests.CUT.FooWithFuncOfDependency}];
[DefaultKey.Of(6), {FactoryID=2406, ImplType=DryIoc.UnitTests.LazyTests.BarDependency}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IDependency but found many:
[DefaultKey.Of(0), {FactoryID=2308, ImplType=DryIoc.UnitTests.CUT.Dependency}];
[DefaultKey.Of(1), {FactoryID=2309, ImplType=DryIoc.UnitTests.CUT.Foo1}];
[DefaultKey.Of(2), {FactoryID=2310, ImplType=DryIoc.UnitTests.CUT.Foo2}];
[DefaultKey.Of(3), {FactoryID=2311, ImplType=DryIoc.UnitTests.CUT.Foo3}];
[DefaultKey.Of(4), {FactoryID=2315, ImplType=DryIoc.UnitTests.CUT.FooWithDependency}];
[DefaultKey.Of(5), {FactoryID=2316, ImplType=DryIoc.UnitTests.CUT.FooWithFuncOfDependency}];
[DefaultKey.Of(6), {FactoryID=2406, ImplType=DryIoc.UnitTests.LazyTests.BarDependency}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IService), DefaultKey.Of(6)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.ServiceWithSingletonDependency(new DryIoc.UnitTests.CUT.Singleton()));

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IService but found many:
[DefaultKey.Of(0), {FactoryID=2295, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCount}];
[DefaultKey.Of(1), {FactoryID=2296, ImplType=DryIoc.UnitTests.CUT.ServiceWithInstanceCountWithStringParam}];
[DefaultKey.Of(2), {FactoryID=2297, ImplType=DryIoc.UnitTests.CUT.Service}];
[DefaultKey.Of(3), {FactoryID=2298, ImplType=DryIoc.UnitTests.CUT.AnotherService}];
[DefaultKey.Of(4), {FactoryID=2299, ImplType=DryIoc.UnitTests.CUT.ServiceWithDependency}];
[DefaultKey.Of(5), {FactoryID=2300, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithDependency}];
[DefaultKey.Of(6), {FactoryID=2301, ImplType=DryIoc.UnitTests.CUT.ServiceWithSingletonDependency}];
[DefaultKey.Of(7), {FactoryID=2302, ImplType=DryIoc.UnitTests.CUT.ServiceWithEnumerableDependencies}];
[DefaultKey.Of(8), {FactoryID=2303, ImplType=DryIoc.UnitTests.CUT.ServiceWithManyDependencies}];
[DefaultKey.Of(9), {FactoryID=2304, ImplType=DryIoc.UnitTests.CUT.ServiceWithLazyDependency}];
[DefaultKey.Of(10), {FactoryID=2305, ImplType=DryIoc.UnitTests.CUT.AnotherServiceWithLazyDependency}];
[DefaultKey.Of(11), {FactoryID=2313, ImplType=DryIoc.UnitTests.CUT.ServiceWithRecursiveDependency}];
[DefaultKey.Of(12), {FactoryID=2314, ImplType=DryIoc.UnitTests.CUT.ServiceWithFuncOfRecursiveDependency}];
[DefaultKey.Of(13), {FactoryID=2324, ImplType=DryIoc.UnitTests.CUT.ServiceWithOnePrimitiveParameter}];
[DefaultKey.Of(14), {FactoryID=2325, ImplType=DryIoc.UnitTests.CUT.ServiceWithTwoPrimitiveParameters}];
[DefaultKey.Of(15), {FactoryID=2340, ImplType=DryIoc.UnitTests.CUT.DisposableService}];
[DefaultKey.Of(16), {FactoryID=2453, ImplType=DryIoc.UnitTests.ReuseInCurrentScopeTests.IndependentService}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IDependency but found many:
[DefaultKey.Of(0), {FactoryID=2308, ImplType=DryIoc.UnitTests.CUT.Dependency}];
[DefaultKey.Of(1), {FactoryID=2309, ImplType=DryIoc.UnitTests.CUT.Foo1}];
[DefaultKey.Of(2), {FactoryID=2310, ImplType=DryIoc.UnitTests.CUT.Foo2}];
[DefaultKey.Of(3), {FactoryID=2311, ImplType=DryIoc.UnitTests.CUT.Foo3}];
[DefaultKey.Of(4), {FactoryID=2315, ImplType=DryIoc.UnitTests.CUT.FooWithDependency}];
[DefaultKey.Of(5), {FactoryID=2316, ImplType=DryIoc.UnitTests.CUT.FooWithFuncOfDependency}];
[DefaultKey.Of(6), {FactoryID=2406, ImplType=DryIoc.UnitTests.LazyTests.BarDependency}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.CUT.IDependency but found many:
[DefaultKey.Of(0), {FactoryID=2308, ImplType=DryIoc.UnitTests.CUT.Dependency}];
[DefaultKey.Of(1), {FactoryID=2309, ImplType=DryIoc.UnitTests.CUT.Foo1}];
[DefaultKey.Of(2), {FactoryID=2310, ImplType=DryIoc.UnitTests.CUT.Foo2}];
[DefaultKey.Of(3), {FactoryID=2311, ImplType=DryIoc.UnitTests.CUT.Foo3}];
[DefaultKey.Of(4), {FactoryID=2315, ImplType=DryIoc.UnitTests.CUT.FooWithDependency}];
[DefaultKey.Of(5), {FactoryID=2316, ImplType=DryIoc.UnitTests.CUT.FooWithFuncOfDependency}];
[DefaultKey.Of(6), {FactoryID=2406, ImplType=DryIoc.UnitTests.LazyTests.BarDependency}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.UnitTests.CUT.ServiceWithOnePrimitiveParameter: DryIoc.UnitTests.CUT.IService {DefaultKey.Of(13)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CUT.IService, DefaultKey.Of(13)}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: ContainerException
----------------------
Unable to resolve Boolean as parameter "flag"
 in DryIoc.UnitTests.CUT.ServiceWithTwoPrimitiveParameters: DryIoc.UnitTests.CUT.IService {DefaultKey.Of(14)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CUT.IService, DefaultKey.Of(14)}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IService), DefaultKey.Of(15)),
                (state, r, scope) => new DryIoc.UnitTests.CUT.DisposableService());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IService), DefaultKey.Of(16)),
                (state, r, scope) => new DryIoc.UnitTests.ReuseInCurrentScopeTests.IndependentService());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.Brain), null),
                (state, r, scope) => new DryIoc.UnitTests.Brain());

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.InternalClient using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.RulesTests.INamedService but found many:
[DefaultKey.Of(0), {FactoryID=2473, ImplType=DryIoc.UnitTests.RulesTests.NamedService}];
[DefaultKey.Of(1), {FactoryID=2474, ImplType=DryIoc.UnitTests.RulesTests.AnotherNamedService}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.HashTreeTests), null),
                (state, r, scope) => new DryIoc.UnitTests.HashTreeTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.DelegateFactoryTests), null),
                (state, r, scope) => new DryIoc.UnitTests.DelegateFactoryTests());

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "x"
 in DryIoc.UnitTests.InjectionRulesTests.ClientWithStringParam
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.InjectionRulesTests.ClientWithStringParam}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.SomeService), null),
                (state, r, scope) => new DryIoc.UnitTests.SelectConstructorWithAllResolvableArguments.SomeService());

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.CUT.EnumKey using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.LazyTests.X using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.LazyTests.Y using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.ServiceColors using provided constructor selector.
*/

/* Exception: ContainerException
----------------------
Unable to get constructor of DryIoc.UnitTests.RulesTests.FooMetadata using provided constructor selector.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.CUT.IAbstractService), null),
                (state, r, scope) => new DryIoc.UnitTests.CUT.ServiceWithAbstractBaseClass());

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.ContextDependentResolutionTests.ILogger but found many:
[DefaultKey.Of(0), {FactoryID=2355, ImplType=DryIoc.UnitTests.ContextDependentResolutionTests.PlainLogger}];
[DefaultKey.Of(1), {FactoryID=2356, ImplType=DryIoc.UnitTests.ContextDependentResolutionTests.FastLogger}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RegisterManyTests.ISome), null),
                (state, r, scope) => new DryIoc.UnitTests.RegisterManyTests.Someberry());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.PropertyHolder), null),
                (state, r, scope) => new DryIoc.UnitTests.PropertyHolder());

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

/* Exception: ContainerException
----------------------
Unable to resolve Boolean as parameter "flag"
 in DryIoc.UnitTests.CUT.ServiceWithParameterAndDependency: DryIoc.UnitTests.CUT.IServiceWithParameterAndDependency
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.CUT.IServiceWithParameterAndDependency}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.IOperation), DefaultKey.Of(0)),
                (state, r, scope) => new DryIoc.UnitTests.SomeOperation());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.IOperation), DefaultKey.Of(1)),
                (state, r, scope) => new DryIoc.UnitTests.AnotherOperation());

/* Exception: ContainerException
----------------------
Unable to resolve Object as parameter "param"
 in DryIoc.UnitTests.ParameterizedOperation: DryIoc.UnitTests.IOperation {DefaultKey.Of(2)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.IOperation, DefaultKey.Of(2)}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.IOperation but found many:
[DefaultKey.Of(0), {FactoryID=2369, ImplType=DryIoc.UnitTests.SomeOperation}];
[DefaultKey.Of(1), {FactoryID=2370, ImplType=DryIoc.UnitTests.AnotherOperation}];
[DefaultKey.Of(2), {FactoryID=2371, ImplType=DryIoc.UnitTests.ParameterizedOperation}];
[DefaultKey.Of(3), {FactoryID=2372, ImplType=DryIoc.UnitTests.MeasureExecutionTimeOperationDecorator}];
[DefaultKey.Of(4), {FactoryID=2374, ImplType=DryIoc.UnitTests.RetryOperationDecorator}];
[DefaultKey.Of(5), {FactoryID=2378, ImplType=DryIoc.UnitTests.LazyDecorator}];
[DefaultKey.Of(6), {FactoryID=2379, ImplType=DryIoc.UnitTests.FuncWithArgDecorator}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.IOperation but found many:
[DefaultKey.Of(0), {FactoryID=2369, ImplType=DryIoc.UnitTests.SomeOperation}];
[DefaultKey.Of(1), {FactoryID=2370, ImplType=DryIoc.UnitTests.AnotherOperation}];
[DefaultKey.Of(2), {FactoryID=2371, ImplType=DryIoc.UnitTests.ParameterizedOperation}];
[DefaultKey.Of(3), {FactoryID=2372, ImplType=DryIoc.UnitTests.MeasureExecutionTimeOperationDecorator}];
[DefaultKey.Of(4), {FactoryID=2374, ImplType=DryIoc.UnitTests.RetryOperationDecorator}];
[DefaultKey.Of(5), {FactoryID=2378, ImplType=DryIoc.UnitTests.LazyDecorator}];
[DefaultKey.Of(6), {FactoryID=2379, ImplType=DryIoc.UnitTests.FuncWithArgDecorator}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

/* Exception: FormatException
----------------------
Input string was not in a correct format.
*/

/* Exception: ContainerException
----------------------
Expecting single default registration of DryIoc.UnitTests.IOperation but found many:
[DefaultKey.Of(0), {FactoryID=2369, ImplType=DryIoc.UnitTests.SomeOperation}];
[DefaultKey.Of(1), {FactoryID=2370, ImplType=DryIoc.UnitTests.AnotherOperation}];
[DefaultKey.Of(2), {FactoryID=2371, ImplType=DryIoc.UnitTests.ParameterizedOperation}];
[DefaultKey.Of(3), {FactoryID=2372, ImplType=DryIoc.UnitTests.MeasureExecutionTimeOperationDecorator}];
[DefaultKey.Of(4), {FactoryID=2374, ImplType=DryIoc.UnitTests.RetryOperationDecorator}];
[DefaultKey.Of(5), {FactoryID=2378, ImplType=DryIoc.UnitTests.LazyDecorator}];
[DefaultKey.Of(6), {FactoryID=2379, ImplType=DryIoc.UnitTests.FuncWithArgDecorator}].
Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.RulesTests), null),
                (state, r, scope) => new DryIoc.UnitTests.RulesTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.DynamicFactoryTests), null),
                (state, r, scope) => new DryIoc.UnitTests.DynamicFactoryTests());

/* Exception: ContainerException
----------------------
Unable to find constructor with all resolvable parameters when resolving DryIoc.UnitTests.OpenGenericsTests.LazyOne<>
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.UnitTests.OpenGenericsTests.LazyOne<>}.
*/

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ReuseWrapperTests), null),
                (state, r, scope) => new DryIoc.UnitTests.ReuseWrapperTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.OpenGenericsTests), null),
                (state, r, scope) => new DryIoc.UnitTests.OpenGenericsTests());

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.ReuseTests.SingletonDep), null),
                (state, r, scope) => new DryIoc.UnitTests.ReuseTests.SingletonDep(new DryIoc.UnitTests.ReuseTests.ResolutionScopeDep()));

            Resolutions = Resolutions.AddOrUpdate(new KV<Type, object>(
                typeof(DryIoc.UnitTests.Memory.NoMemoryLeaksTests), null),
                (state, r, scope) => new DryIoc.UnitTests.Memory.NoMemoryLeaksTests());
        } // end of static constructor
    }
}