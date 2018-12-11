using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using DryIocAttributes;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    using Request = DryIocAttributes.Request;

    public interface IService { }

    [ExportMany]
    public class Service : IService { }

    [ExportMany]
    public class AnotherService : IService { }

    public interface ITransientService
    {
    }

    [Export(typeof(ITransientService)), AsResolutionRoot]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class TransientService : ITransientService
    {
    }

    public interface ISingletonService
    {
    }

    [Export(typeof(ISingletonService)), AsResolutionRoot]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SingletonService : ISingletonService
    {
    }

    public interface IOpenGenericService<T>
    {
        T Value { get; }
    }

    [Export(typeof(IOpenGenericService<>))]
    public class SingletonOpenGenericService<T> : IOpenGenericService<T>
    {
        public T Value { get; set; }
    }

    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class TransientOpenGenericService<T>
    {
        public T Value { get; set; }
    }

    [Export]
    public class OpenGenericServiceWithTwoParameters<T1, T2>
    {
        public T1 Value1 { get; set; }
        public T2 Value2 { get; set; }
    }

    [Export, AsResolutionRoot]
    public class DependentService
    {
        public ITransientService TransientService { get; private set; }
        public ISingletonService SingletonService { get; private set; }
        public TransientOpenGenericService<string> TransientOpenGenericService { get; private set; }
        public OpenGenericServiceWithTwoParameters<bool, bool> OpenGenericServiceWithTwoParameters { get; set; }

        public DependentService(
            ITransientService transientService,
            ISingletonService singletonService,
            TransientOpenGenericService<string> transientOpenGenericService,
            OpenGenericServiceWithTwoParameters<bool, bool> openGenericServiceWithTwoParameters)
        {
            TransientService = transientService;
            SingletonService = singletonService;
            TransientOpenGenericService = transientOpenGenericService;
            OpenGenericServiceWithTwoParameters = openGenericServiceWithTwoParameters;
        }
    }

    public interface IServiceWithMultipleImplementations
    {
        string Message { get; }
    }

    [ExportWithDisplayName(typeof(IServiceWithMultipleImplementations), "One")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class OneTransientService : IServiceWithMultipleImplementations
    {
        public string Message { get; private set; }

        public OneTransientService(string message)
        {
            Message = message;
        }
    }

    [Export(typeof(IServiceWithMultipleImplementations))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AnotherTransientService : IServiceWithMultipleImplementations
    {
        public string Message { get; private set; }

        public AnotherTransientService(string message)
        {
            Message = message;
        }
    }

    public interface IServiceWithMetadata
    {
    }

    [ExportWithDisplayName(typeof(IServiceWithMetadata), "Up")]
    public class OneServiceWithMetadata : IServiceWithMetadata
    {
    }

    [ExportWithDisplayName(typeof(IServiceWithMetadata), "Down")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AnotherServiceWithMetadata : IServiceWithMetadata
    {
    }

    [Export(typeof(IServiceWithMetadata)), ViewWith(DisplayName = "Elsewhere")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class YetAnotherServiceWithMetadata : IServiceWithMetadata
    {
    }

    public class ViewMetadata
    {
        public string DisplayName { get; set; }
    }

    public interface IViewMetadata
    {
        string DisplayName { get; set; }
    }

    [MetadataAttribute]
    public class ExportWithDisplayNameAttribute : ExportAttribute, IViewMetadata
    {
        public ExportWithDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public ExportWithDisplayNameAttribute(Type contractType, string displayName) : base(contractType)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; set; }
    }

    [MetadataAttribute]
    public class ViewWithAttribute : Attribute, IViewMetadata
    {
        public string DisplayName { get; set; }
    }

    [Export, AsResolutionRoot]
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

    [Export, AsResolutionRoot]
    public class ServiceWithMultipleCostructorsAndOneImporting
    {
        public ITransientService Transient { get; private set; }
        public ISingletonService Singleton { get; private set; }

        public ServiceWithMultipleCostructorsAndOneImporting(ISingletonService singleton)
        {
            Singleton = singleton;
        }

        [ImportingConstructor]
        public ServiceWithMultipleCostructorsAndOneImporting(ITransientService transient)
        {
            Transient = transient;
        }
    }

    [Export, WithMetadata(1), AsResolutionRoot]
    public class SingleServiceWithMetadata
    {
    }

    [Export]
    public class ServiceWithImportedCtorParameter
    {
        public INamedService NamedDependency { get; set; }

        public ServiceWithImportedCtorParameter([Import("blah")]INamedService namedDependency)
        {
            NamedDependency = namedDependency;
        }
    }

    public interface INamedService
    {
    }

    [Export(typeof(INamedService))]
    public class NamedService : INamedService
    {
    }

    [Export("blah", typeof(INamedService))]
    public class AnotherNamedService : INamedService
    {
    }

    public class YetAnotherNamedService : INamedService
    {
    }

    public interface IFooService
    {
    }

    [ExportMany, AsResolutionRoot]
    public class DbMan : ISomeDb, IAnotherDb
    {
    }

    public interface IAnotherDb
    {
    }

    public interface ISomeDb
    {
    }

    [ExportMany(Except = new []{ typeof(IAnotherDb) })]
    public class DbMan<T> : ISomeDb<T>, IAnotherDb
    {
    }

    public interface ISomeDb<T>
    {
    }

    [Export, PartCreationPolicy(CreationPolicy.Any)]
    public class UseLazyEnumerable
    {
        public IEnumerable<Me> Mes { get; private set; }

        public UseLazyEnumerable(IEnumerable<Me> mes)
        {
            Mes = mes;
        }
    }

    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class Me {}

    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class LazyDepClient
    {
        public LazyDep Dep { get; private set; }
        public LazyDepClient(LazyDep dep)
        {
            Dep = dep;
        }
    }

    [Export, PartCreationPolicy(CreationPolicy.NonShared), OpenResolutionScope]
    public class LazyDep { }

    [InheritedExport]
    public interface IExportConditionInterface { }

    public abstract class ForParentImplementationAttribute : ExportConditionAttribute
    {
        public override bool Evaluate(Request request)
        {
            return request.Enumerate().Any(r => r.ImplementationType == _parentImplementationType);
        }

        private readonly Type _parentImplementationType;
        protected ForParentImplementationAttribute(Type parentImplementationType) { _parentImplementationType = parentImplementationType; }
    }

    public class ForImportCondition1ParentAttribute : ForParentImplementationAttribute
    {
        public ForImportCondition1ParentAttribute() : base(typeof(ImportConditionObject1)) { }
    }

    public class ForImportCondition2ParentAttribute : ForParentImplementationAttribute
    {
        public ForImportCondition2ParentAttribute() : base(typeof(ImportConditionObject2)) { }
    }

    public class ForImportCondition3ParentAttribute : ForParentImplementationAttribute
    {
        public ForImportCondition3ParentAttribute() : base(typeof(ImportConditionObject3)) { }
    }

    [ForImportCondition1Parent, AsResolutionCall]
    public class ExportConditionalObject1 : IExportConditionInterface { }

    [ForImportCondition2Parent]
    public class ExportConditionalObject2 : IExportConditionInterface { }

    [ForImportCondition3Parent, AsResolutionCall]
    public class ExportConditionalObject3 : IExportConditionInterface { }

    [Export, AsResolutionRoot]
    public class ImportConditionObject1
    {
        public IExportConditionInterface ExportConditionInterface { get; set; }
        public ImportConditionObject1(IExportConditionInterface exportConditionInterface)
        {
            ExportConditionInterface = exportConditionInterface;
        }
    }

    [Export, AsResolutionRoot]
    public class ImportConditionObject2
    {
        public IExportConditionInterface ExportConditionInterface { get; set; }
        public ImportConditionObject2(IExportConditionInterface exportConditionInterface)
        {
            ExportConditionInterface = exportConditionInterface;
        }
    }

    [Export, AsResolutionRoot]
    public class ImportConditionObject3
    {
        public IExportConditionInterface ExportConditionInterface { get; set; }
        public ImportConditionObject3(IExportConditionInterface exportConditionInterface)
        {
            ExportConditionInterface = exportConditionInterface;
        }
    }

    [Export, AsResolutionCall, TransientReuse]
    public class A {}

    [Export, AsResolutionRoot, TransientReuse]
    public class B
    {
        public readonly A A;

        public B(A a)
        {
            A = a;
        }
    }

    [Export, TransientReuse]
    public class ServiceWithReuseAttribute { }

    [Export, SingletonReuse]
    public class ServiceWithSingletonReuse { }

    [Export, CurrentScopeReuse]
    public class ServiceWithCurrentScopeReuse { }

    [Export, ResolutionScopeReuse]
    public class ServiceWithResolutionScopeReuse { }

    [Export, OpenResolutionScope]
    public class UserOfServiceWithResolutionScopeReuse
    {
        public ServiceWithResolutionScopeReuse One { get; set; }
        public ServiceWithResolutionScopeReuse Another { get; set; }

        public UserOfServiceWithResolutionScopeReuse(
            ServiceWithResolutionScopeReuse one,
            ServiceWithResolutionScopeReuse another)
        {
            One = one;
            Another = another;
        }
    }

    [Export, CurrentScopeReuse("ScopeA")]
    public class WithNamedCurrentScope { }

    [Export, WeaklyReferenced, AsResolutionRoot]
    public class WeaklyReferencedService { }

    [Export, PreventDisposal, AsResolutionRoot]
    public class PreventDisposalService : IDisposable
    {
        public bool IsDisposed;
        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [Export, SingletonReuse]
    public sealed class DTUser
    {
        public readonly DT Dt;
        public readonly DT2 Dt2;

        public DTUser(DT dt, DT2 dt2)
        {
            Dt = dt;
            Dt2 = dt2;
        }
    }

    [Export, TransientReuse, AllowDisposableTransient]
    public sealed class DT : IDisposable
    {
        public bool IsDisposed;

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [Export, TransientReuse, AllowDisposableTransient, TrackDisposableTransient]
    public sealed class DT2 : IDisposable
    {
        public bool IsDisposed;

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public class Daah
    {
        [Export("a")]
        public class Fooh<A>
        {
            public Fooh(A a) {}
        }

        [Export]
        public class FoohFactory<A>
        {
            [Export("b")]
            public static Fooh<A> Create(A a)
            {
                return new Fooh<A>(a);
            }
        }
    }

    [Export]
    public class A1 { }

    public interface IItem<T> {}

    [ExportMany]
    public class IntItem : IItem<int> { }

    [ExportMany]
    public class Item<B> : IItem<B> { }

    [ExportMany(ContractName = "root")]
    public class CompositeItem<T> : IItem<T>
    {
        public IItem<T>[] Items { get; set; }
        public CompositeItem(IEnumerable<IItem<T>> items)
        {
            Items = items.ToArray();
        }
    }

    public interface IAllOpts { }

    [ExportEx(typeof(IAllOpts),
        ContractKey = "a",
        IfAlreadyExported = IfAlreadyExported.Keep),
        CurrentScopeReuse,
        AsResolutionRoot]
    public class AllOpts : IAllOpts, IDisposable
    {
        public bool IsDisposed;

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [ExportEx(typeof(IAllOpts),
        ContractKey = "a",
        IfAlreadyExported = IfAlreadyExported.Keep),
        CurrentScopeReuse,
        AsResolutionRoot]
    public class AllOpts2 : IAllOpts
    {
    }

    [Export, CurrentScopeReuse(scopeName: "a"), AsResolutionRoot]
    public class NamedScopeService
    {
    }

    [Export]
    public class MultiCtorDep { }

    public class MultiCtorDepTwo { }

    [Export]
    public class MultiCtorSample
    {
        public MultiCtorDep Dep { get; private set; }

        public MultiCtorSample() { }

        public MultiCtorSample(Func<MultiCtorDep> getDep)
        {
            Dep = getDep();
        }
    }

    [Export, WithMetadata("a", 1), ExportMetadata("b", 2)]
    public class WithMetaKeyValue
    {
    }

    [Export, WithMetadata("a", 1)]
    public class WithWithMetadataOnlyKeyValue
    {
    }

    [Export, ExportMetadata("b", 2)]
    public class WithExportMetadataOnlyKeyValue
    {
    }

    public interface IOpGen<out T> { }
    
    [ExportMany, AsResolutionRoot]
    public class ClGen : IOpGen<string> { }

    [ExportMany, AsResolutionRoot]
    public class OpGen<T> : IOpGen<T> { }

    public class Aa { }
    public class Bb : Aa { }

    [ExportMany, AsResolutionRoot]
    public class ClGenA : IOpGen<Aa> { }

    [ExportMany, AsResolutionRoot]
    public class ClGenB : IOpGen<Bb> { }
}
