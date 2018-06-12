using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue579_VerifyResolutions_strange_behaviour
    {
        [Test]
        public void Minimal_test()
        {
            var container = new Container(rules => rules.WithoutThrowIfDependencyHasShorterReuseLifespan());
            container.Register<fiosEntities>(Reuse.InWebRequest, setup: Setup.With(openResolutionScope: true));
            var errors = container.Validate();
            Assert.IsEmpty(errors);
        }

        [Test]
        public void Close_to_real_test()
        {
            var container = new Container();

            container.Register<fiosEntities>(Reuse.InWebRequest, setup: Setup.With(openResolutionScope: true));
            //container.Register(typeof(IGenericExecutor<>), typeof(GenericExecutor<>), Reuse.Singleton);
            container.Register<IBaseParams, BaseParams>(Reuse.Singleton);
            //container.Register<IDataCheck, DataCheck>(Reuse.Singleton);
            //container.Register<IReportUtilities, ReportUtilities>(Reuse.Singleton);
            //container.Register<IResponseProvider, JsonResponseProvider>(Reuse.Singleton);
            //container.Register<IRelationshipsService, RelationshipsService>(Reuse.Singleton);
            //container.Register<IBusinessService, BusinessService>(Reuse.Singleton);
            //container.Register<ISurveysService, SurveysService>(Reuse.Singleton);
            //container.Register<IGenericService, GenericService>(Reuse.Singleton);
            //container.Register<IGenericDataService, GenericDataService>(Reuse.Singleton);
            //container.Register<IAccountsService, AccountsService>(Reuse.Singleton);
            //container.Register<IApplicationsService, ApplicationsService>(Reuse.Singleton);
            //container.Register<IInformationService, InformationService>(Reuse.Singleton);
            //container.Register<IReferenceDataService, ReferenceDataService>(Reuse.Singleton);
            //container.Register<IReportsService, ReportsService>(Reuse.Singleton);
            //container.Register<ITechnologiesService, TechnologiesService>(Reuse.Singleton);
            //container.Register<IUiService, UiService>(Reuse.Singleton);

            var errors = container.Validate();
            Assert.IsEmpty(errors);
        }
    }

    public class fiosEntities { }

    public interface IGenericExecutor<TSvc> { }

    public class GenericExecutor<TSvc> : IGenericExecutor<TSvc> { }

    public interface IBaseParams
    {
        fiosEntities Db { get; }

        IDataCheck DataCheck { get; }

        IResponseProvider ResponseProvider { get; }

        IRelationshipsService RelationshipsService { get; }

        IGenericService GenericService { get; }

        IUiService UiService { get; }
    }

    public class BaseParams : IBaseParams
    {
        private readonly Func<fiosEntities> _getDb;

        public fiosEntities Db => _getDb();

        public IDataCheck DataCheck { get; }

        public IResponseProvider ResponseProvider { get; }

        public IRelationshipsService RelationshipsService { get; }

        public IGenericService GenericService { get; }

        public IUiService UiService { get; }

        public BaseParams(
            Func<fiosEntities> db,
            IDataCheck dataCheck,
            IResponseProvider responseProvider,
            IRelationshipsService relationshipsService,
            IGenericService genericService,
            IUiService uiService)
        {

            _getDb = db;
            DataCheck = dataCheck;
            ResponseProvider = responseProvider;
            RelationshipsService = relationshipsService;
            GenericService = genericService;
            UiService = uiService;
        }
    }

    public interface IDataCheck
    {
    }

    public class DataCheck : IDataCheck
    {
        private readonly Func<fiosEntities> _getDb;

        public DataCheck(Func<fiosEntities> getDb)
        {
            _getDb = getDb;
        }
    }

    public interface IResponseProvider
    {
    }

    public class JsonResponseProvider : IResponseProvider
    {
    }

    public interface IRelationshipsService
    {
    }

    public class RelationshipsService : BaseService, IRelationshipsService
    {
        private readonly IGenericDataService _genericDataService;
        private readonly IGenericExecutor<RelationshipsService> _genericExecutor;

        public RelationshipsService(
            Func<fiosEntities> getDb,
            IGenericDataService genericDataService,
            IGenericExecutor<RelationshipsService> genericExecutor)
            : base(getDb)
        {

            _genericDataService = genericDataService;
            _genericExecutor = genericExecutor;
        }
    }

    public abstract class BaseService
    {
        protected fiosEntities Db => _getDb();

        private readonly Func<fiosEntities> _getDb;

        protected BaseService(Func<fiosEntities> getDb)
        {
            _getDb = getDb;
        }
    }

    public interface IGenericService
    {
    }

    public interface IUiService
    {
    }

    public interface IReportUtilities
    {
    }

    public class ReportUtilities : IReportUtilities
    {
        private readonly Func<fiosEntities> _getDb;
        private readonly ITechnologiesService _technologiesService;
        private readonly IRelationshipsService _relationshipsService;

        public ReportUtilities(Func<fiosEntities> getDb, ITechnologiesService technologiesService, IRelationshipsService relationshipsService)
        {
            _getDb = getDb;
            _technologiesService = technologiesService;
            _relationshipsService = relationshipsService;
        }
    }

    public interface ITechnologiesService
    {
    }

    public interface IGenericDataService
    {
    }

    public interface IBusinessService
    {
    }

    public class BusinessService : BaseService, IBusinessService
    {
        private readonly IRelationshipsService _relationshipService;

        public BusinessService(Func<fiosEntities> db, IRelationshipsService relationshipService) : base(db)
        {
            _relationshipService = relationshipService;
        }
    }

    public interface ISurveysService
    {
    }

    public class SurveysService : BaseService, ISurveysService
    {
        private static readonly List<string> HierarchicalTables;

        private readonly IRelationshipsService _relationshipService;

        public SurveysService(Func<fiosEntities> db, IRelationshipsService relationshipsService)
            : base(db)
        {
            _relationshipService = relationshipsService;
        }
    }

    /// <summary>
    /// Performs operations over the database for entities from generic type
    /// </summary>
    public class GenericService : BaseService, IGenericService
    {
        private const string IsDeletedPropertyName = "IsDeleted";
        private readonly IRelationshipsService _relationshipService;

        public GenericService(Func<fiosEntities> getDb, IRelationshipsService relationshipService)
            : base(getDb)
        {
            _relationshipService = relationshipService;
        }
    }

    public class GenericDataService : BaseService, IGenericDataService
    {
        public GenericDataService(Func<fiosEntities> getDb)
            : base(getDb)
        {
        }
    }

    public interface IAccountsService
    {
    }

    public class AccountsService : BaseService, IAccountsService
    {
        public AccountsService(Func<fiosEntities> db) : base(db)
        {
        }
    }

    public interface IApplicationsService
    {
    }

    public class ApplicationsService : BaseService, IApplicationsService
    {
        private readonly IRelationshipsService _relationshipService;

        public ApplicationsService(Func<fiosEntities> db, IRelationshipsService relationshipService) : base(db)
        {
            _relationshipService = relationshipService;
        }
    }

    public interface IInformationService
    {
    }

    public class InformationService : BaseService, IInformationService
    {
        private readonly IRelationshipsService _relationshipService;

        public InformationService(Func<fiosEntities> db, IRelationshipsService relationshipService) : base(db)
        {
            _relationshipService = relationshipService;
        }
    }

    public interface IReferenceDataService
    {
    }

    public class ReferenceDataService : BaseService, IReferenceDataService
    {
        public ReferenceDataService(Func<fiosEntities> db) : base(db)
        {
        }
    }

    public interface IReportsService
    {
    }

    public class ReportsService : BaseService, IReportsService
    {
        public ReportsService(Func<fiosEntities> db) : base(db)
        {
        }
    }

    public class TechnologiesService : BaseService, ITechnologiesService
    {
        private readonly IRelationshipsService _relationshipsService;

        public TechnologiesService(Func<fiosEntities> db, IRelationshipsService relationshipsService)
            : base(db)
        {
            _relationshipsService = relationshipsService;
        }
    }

    public class UiService : BaseService, IUiService
    {
        private readonly IRelationshipsService _relationshipsService;
        private readonly ITechnologiesService _technologiesService;

        public UiService(Func<fiosEntities> getDb, IRelationshipsService relationshipsService, ITechnologiesService technologiesService)
            : base(getDb)
        {

            _relationshipsService = relationshipsService;
            _technologiesService = technologiesService;
        }
    }
}
