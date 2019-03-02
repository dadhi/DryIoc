using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue577_InconsistentResolutionAndCacheAnomaly
    {
        private static readonly Type[] ExpectedStrategies = new[]
        {
            typeof(UnknownStrategy),
            typeof(PartiallyKnownStrategy),
            typeof(IdentityStrategy<TransferImportItem>),
            typeof(SimpleStrategyStrategy<TransferImportItem>),
            typeof(FallbackStrategy<TransferImportItem>)
        }.OrderBy(x => x.FullName).ToArray();

        private static readonly Type[] ExpectedStrategiesWithSmallerDependencyTree = new[]
        {
            typeof(UnknownNoDependencies),
            typeof(PartiallyKnownNoDependencies),
            typeof(IdentityStrategy<TransferImportItem>),
            typeof(SimpleStrategyStrategy<TransferImportItem>),
            typeof(FallbackStrategy<TransferImportItem>)
        }.OrderBy(x => x.FullName).ToArray();

        [Test]
        public void ResolvingEagerWithSimplifiedRegistrationYieldsInstancesOfTheExpectedTypes()
        {
            DoResolutionTest(CreateContainerWithSimplifiedRegistration, ExpectedStrategies);
        }

        [Test]
        public void ResolvingEagerWithSmallerDependencyTreeYieldsInstancesOfTheExpectedTypes()
        {
            DoResolutionTest(CreateContainerWithSmallerDependencyTree,
                ExpectedStrategiesWithSmallerDependencyTree);
        }

        [Test]
        public void ResolvingEagerYieldsInstancesOfTheExpectedTypes()
        {
            DoResolutionTest(CreateContainerWithoutLazy, ExpectedStrategies);
        }

        [Test]
        public void ResolvingLazyYieldsInstancesOfTheExpectedTypes()
        {
            DoResolutionTest(CreateContainerWithLazy, ExpectedStrategies);
        }

        private static void DoResolutionTest(Func<IContainer> createContainer, Type[] expectedTypes)
        {
            using (var container = createContainer())
            {
                var r = container.Resolve<IStrategySelector<TransferImportItem>>();
                var resolvedTypes = r.Strategies.Select(x => x.GetType()).OrderBy(x => x.FullName).ToArray();
                Assert.AreEqual(expectedTypes, resolvedTypes);
            }
        }

        #region Setups

        public static IContainer CreateContainerWithoutLazy()
        {
            var container = CreateContainer();
            RegisterStrategies(container);

            container.Register(typeof(IStrategySelector<>), typeof(DefaultStrategySelector<>));

            return container;
        }

        public static IContainer CreateContainerWithLazy()
        {
            var container = CreateContainer();
            RegisterStrategies(container);

            container.Register(typeof(IStrategySelector<>), typeof(LazyStrategySelector<>));

            return container;
        }

        public static IContainer CreateContainerWithSmallerDependencyTree()
        {
            var container = CreateContainer();

            container.RegisterMany(new[]
                {
                    typeof(IStrategy<TransferImportItem>),
                    typeof(IIdentity<TransferImportItem>)
                },
                typeof(IdentityStrategy<TransferImportItem>));

            container.Register<IStrategy<TransferImportItem>, UnknownNoDependencies>();
            container.Register<IStrategy<TransferImportItem>, PartiallyKnownNoDependencies>();
            container.Register(typeof(IStrategy<>), typeof(FallbackStrategy<>));

            container.RegisterMany(
                new[] { typeof(IStrategy<>), typeof(ISimpleStrategy<>) },
                typeof(SimpleStrategyStrategy<>));

            container.Register(typeof(IStrategySelector<>), typeof(DefaultStrategySelector<>));

            return container;
        }

        private static void RegisterStrategies(IContainer container)
        {
            container.RegisterMany(new[]
                {
                    typeof(IStrategy<TransferImportItem>),
                    typeof(IIdentity<TransferImportItem>)
                },
                typeof(IdentityStrategy<TransferImportItem>));

            container.Register<IStrategy<TransferImportItem>, UnknownStrategy>();
            container.Register<IStrategy<TransferImportItem>, PartiallyKnownStrategy>();
            container.Register(typeof(IStrategy<>), typeof(FallbackStrategy<>));

            container.RegisterMany(
                new[] { typeof(IStrategy<>), typeof(ISimpleStrategy<>) },
                typeof(SimpleStrategyStrategy<>));
        }

        public static IContainer CreateContainerWithSimplifiedRegistration()
        {
            var container = CreateContainer();

            container.Register(typeof(IStrategySelector<>), typeof(DefaultStrategySelector<>));

            container.RegisterMany(new[]
                {
                    typeof(IStrategy<TransferImportItem>),
                    typeof(IIdentity<TransferImportItem>)
                },
                typeof(IdentityStrategy<TransferImportItem>));
            container.Register<IStrategy<TransferImportItem>, UnknownStrategy>();
            container.Register<IStrategy<TransferImportItem>, PartiallyKnownStrategy>();
            container.Register(typeof(IStrategy<TransferImportItem>), typeof(FallbackStrategy<TransferImportItem>));
            container.Register(typeof(IStrategy<DirectCollectionImportItem>), typeof(FallbackStrategy<DirectCollectionImportItem>));
            container.Register(typeof(IStrategy<TransferImportItem>), typeof(SimpleStrategyStrategy<TransferImportItem>));
            container.Register(typeof(IStrategy<DirectCollectionImportItem>), typeof(SimpleStrategyStrategy<DirectCollectionImportItem>));
            return container;
        }

        private static IContainer CreateContainer()
        {
            var container = new Container(rules => rules
                .WithDefaultReuse(Reuse.ScopedOrSingleton)
                .With(FactoryMethod.ConstructorWithResolvableArguments));

            container.RegisterMany(
                new[] { typeof(IUnitOfWork), typeof(IStoredProceduresProvider<IStoredProcedures>) },
                typeof(DefaultUnitOfWork),
                made: Made.Of(() => DefaultUnitOfWork.CreateUnitOfWork(Arg.Of<ICurrentUser>(IfUnresolved.ReturnDefault))));

            container.Register(Made.Of(r => ServiceInfo.Of<IStoredProceduresProvider<IStoredProcedures>>(), f => f.StoredProcedures()));

            container.Register<IBEntityRepository, BEntityRepository>();
            container.Register<IAEntityRepository, AEntityRepository>();
            container.RegisterInstance<ICurrentUser>(new SystemCurrentUser());
            container.Register<IAService, AService>();

            container.Register<IExPostValidator, ExPostValidator>();
            return container;
        }


        #endregion

        #region Types

        public interface IUnitOfWork : IDisposable { }
        public interface IContext : IUnitOfWork { }

        public interface IDbContext : IContext { }

        private interface IBackgroundJobDbContext : IDbContext { }

        public sealed class DefaultUnitOfWork : IUnitOfWork, IStoredProceduresProvider<IStoredProcedures>
        {
            public static DefaultUnitOfWork CreateUnitOfWork(ICurrentUser currentUser)
            {
                return new DefaultUnitOfWork(new DefaultContext("constr") { CurrentUser = currentUser });
            }

            public DefaultUnitOfWork(IDbContext context)
            {
                _context = context;
            }

            private readonly IDbContext _context;
            public IContext Context => _context;
            public void Dispose() { }

            IStoredProcedures IStoredProceduresProvider<IStoredProcedures>.StoredProcedures()
            {
                var provider = Context as IStoredProceduresProvider<IStoredProcedures>;
                return provider?.StoredProcedures();
            }
        }

        public interface ICurrentUser { }

        private class SystemCurrentUser : ICurrentUser { }


        private sealed class DefaultContext : IBackgroundJobDbContext, IStoredProceduresProvider<IStoredProcedures>
        {
            public IStoredProcedures StoredProcedures() => new StoredProcedures(this);
            public ICurrentUser CurrentUser { get; set; }
            public DefaultContext(string contextName) { }
            static DefaultContext() { }
            public void Dispose() { }
        }

        private interface IStoredProcedures { }

        private class StoredProcedures : IStoredProcedures
        {
            public StoredProcedures(DefaultContext dbContext)
            {
                if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
            }
        }

        private interface IImportItem { }
        private interface ICollectionImportItem : IImportItem { }
        private interface IImportItemB : IImportItem { }
        private abstract class ImportItem : IImportItem { }

        private abstract class ImportItemB : ImportItem, IImportItemB { }

        private class TransferImportItem : ImportItemB { }

        private class DirectCollectionImportItem : ImportItemB, ICollectionImportItem { }

        private interface IAEntityRepository { }

        private class AEntityRepository : IAEntityRepository
        {
            public AEntityRepository(IUnitOfWork unitOfWork, IStoredProcedures storedProcedures) { }
        }

        private interface IBEntityRepository { }

        private class BEntityRepository : IBEntityRepository
        {
            public BEntityRepository(IUnitOfWork unitOfWork)
            {
                if (unitOfWork == null) 
                    throw new ArgumentNullException(nameof(unitOfWork));
            }
        }

        private interface IExPostValidator { }

        private interface IAService { }


        private class AService : IAService
        {
            public AService(IAEntityRepository repository, IUnitOfWork unitOfWork)
            {
                if (repository == null)
                    throw new ArgumentNullException(nameof(repository));
                if (unitOfWork == null)
                    throw new ArgumentNullException(nameof(unitOfWork));
            }
        }

        private class ExPostValidator : IExPostValidator
        {
            public ExPostValidator(IBEntityRepository ibEntityRepository, IAService iaService)
            {
                if (ibEntityRepository == null)
                    throw new ArgumentNullException(nameof(ibEntityRepository));
                if (iaService == null)
                    throw new ArgumentNullException(nameof(iaService));
            }
        }

        private interface IIdentity<in TImportItem> : IStrategy<TImportItem> where TImportItem : class, IImportItemB { }

        private interface IStoredProceduresProvider<out TStoredProcedures>
        {
            TStoredProcedures StoredProcedures();
        }

        private interface IStrategy<in TImportItem> where TImportItem : class, IImportItem { }

        private interface IStrategySelector<in TImportItem> where TImportItem : class, IImportItem
        {
            IEnumerable<IStrategy<TImportItem>> Strategies { get; }
        }

        private class FallbackStrategy<TImportItem> : IStrategy<TImportItem> where TImportItem : ImportItem { }

        private interface ISimpleStrategy<in TImportItem> : IStrategy<TImportItem> where TImportItem : class, IImportItem { }

        private class SimpleStrategyStrategy<TImportItem> : ISimpleStrategy<TImportItem> where TImportItem : ImportItem
        {
            public SimpleStrategyStrategy(IExPostValidator exPostValidator)
            {
                if (exPostValidator == null)
                    throw new ArgumentNullException(nameof(exPostValidator));
            }
        }

        private class IdentityStrategy<TImportItem> :
            IIdentity<TImportItem> where TImportItem : class, IImportItemB
        {
            public IdentityStrategy(IExPostValidator exPostValidator)
            {
                if (exPostValidator == null)
                    throw new ArgumentNullException(nameof(exPostValidator));
            }
        }

        private class PartiallyKnownStrategy : IStrategy<TransferImportItem>
        {
            public PartiallyKnownStrategy(IExPostValidator exPostValidator)
            {
                if (exPostValidator == null)
                    throw new ArgumentNullException(nameof(exPostValidator));
            }
        }

        private class UnknownStrategy : IStrategy<TransferImportItem>
        {
            public UnknownStrategy(IExPostValidator exPostValidator)
            {
                if (exPostValidator == null)
                    throw new ArgumentNullException(nameof(exPostValidator));
            }
        }

        private class UnknownNoDependencies : IStrategy<TransferImportItem> { }

        private class PartiallyKnownNoDependencies : IStrategy<TransferImportItem> { }

        private class DefaultStrategySelector<TImportItem> : IStrategySelector<TImportItem> where TImportItem : ImportItem
        {
            public IEnumerable<IStrategy<TImportItem>> Strategies { get; }
            public DefaultStrategySelector(IEnumerable<IStrategy<TImportItem>> strategies)
            {
                Strategies = strategies.ThrowIfNull();
            }
        }
        private class LazyStrategySelector<TImportItem> : IStrategySelector<TImportItem> where TImportItem : ImportItem
        {
            public IEnumerable<IStrategy<TImportItem>> Strategies { get; }
            public LazyStrategySelector(LazyEnumerable<IStrategy<TImportItem>> strategies)
            {
                Strategies = strategies.ThrowIfNull().ToList();
            }
        }
        #endregion Types
    }
}