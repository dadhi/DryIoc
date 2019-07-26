using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.Serialization;
using CUsers.ApplicationServices;
using Conn.Adapter;
using RM;
using Data;
using Entities;
using Framework;
using Logic;
using OrganizationBase;
using Organizations;
using Shop;
using Users.Repositories;
using IOrganizationService = Organizations.IOrganizationService;
using IUser = Framework.IUser;
using Organization = Organizations.Organization;

namespace Shared
{

    public enum Phrase
    {
        Empty,
        Test,
        Foo,
        Bar
    }

    public class SettingsEntityAuthorization<TEntity> : PsaEntityAuthorization<TEntity> where TEntity : IOrganizationEntity
    {
        public SettingsEntityAuthorization(IContextService<IPsaContext> context) : base(context)
        {
        }
    }

    public class OrganizationCompanyAuthorization : SettingsEntityAuthorization<OrganizationCompany>
    {
        public OrganizationCompanyAuthorization(IContextService<IPsaContext> contextService) : base(contextService)
        {
        }
    }

    public interface IAuditTrailEntryRepository : IEntityRepository<AuditTrailEntry>
    {
    }

    public partial class AuditTrailEntryRepository : IAuditTrailEntryRepository
    {
    }

    public interface IGoogleAuthenticationService
    {
    }

    public class GoogleAuthenticationService : IGoogleAuthenticationService
    {
        protected readonly IForAuthUserRepository UserRepository;
        protected readonly IOrganizationAddonService OrganizationAddonService;

        public GoogleAuthenticationService(IForAuthUserRepository userRepository,
            IOrganizationAddonService organizationAddonService)
        {
            if (userRepository == null)
                throw new ArgumentNullException(nameof(userRepository));
            if (organizationAddonService == null)
                throw new ArgumentNullException(nameof(organizationAddonService));
            UserRepository = userRepository;
            OrganizationAddonService = organizationAddonService;
        }
    }

    public interface IForAuthUserRepository
    {
    }

    public class ForAuthUserRepository : RepositoryBase, IForAuthUserRepository
    {
        protected readonly IDict Dict;

        public ForAuthUserRepository(IContextService contextService, IConfiguration configuration,
            DbProviderFactory dbProviderFactory, IDict dict) : base(contextService, configuration,
            dbProviderFactory)
        {
            Dict = dict;
        }
    }


    public interface IChangeEmailAddressService
    {
    }

    public class ChangeEmailAddressService : IChangeEmailAddressService
    {
        protected readonly IConnEmailChangeService ConnEmailChangeService;
        protected readonly IConnUserService ConnUserService;
        protected readonly IGlobalGuidService GlobalGuidService;

        public ChangeEmailAddressService(IConnEmailChangeService ConnEmailChangeService,
            IConnUserService ConnUserService, IGlobalGuidService globalGuidService)
        {
            ConnEmailChangeService = ConnEmailChangeService;
            this.ConnUserService = ConnUserService;
            GlobalGuidService = globalGuidService;
        }
    }


    public interface IUserPasswordService
    {
    }

    public class FeatureService : IFeatureService
    {
        protected readonly IPsaContextService ContextService;

        public FeatureService(IPsaContextService contextService)
        {
            ContextService = contextService;
        }
    }

    public interface IFeatureService
    {
    }

    public class UserPasswordService : IUserPasswordService
    {
        protected readonly IConnClientService ConnClientService;
        protected readonly IConnPasswordPolicyService ConnPasswordPolicyService;
        protected readonly IConnUserService ConnUserService;
        protected readonly IUniqueUserRepository UniqueUserRepository;

        public UserPasswordService(IConnClientService ConnClientService,
            IConnPasswordPolicyService ConnPasswordPolicyService,
            IConnUserService ConnUserService, IUniqueUserRepository uniqueUserRepository)
        {
            ConnClientService = ConnClientService;
            ConnPasswordPolicyService = ConnPasswordPolicyService;
            ConnUserService = ConnUserService;
            UniqueUserRepository = uniqueUserRepository;
        }
    }

    public abstract class ReportFieldHandler<TReportField> : IReportFieldHandler where TReportField : IReportField
    {
        private readonly string _Identifier;
        private readonly ICollection<string> _SearchFields;
        private readonly GetTranslationDelegate _GetLabelDelegate;
        private readonly GetTranslationDelegate _GetShortLabelDelegate;
        private readonly GetTranslationDelegate _GetGroupDelegate;
        private bool _IsSortable;
        public readonly int? DefaultWidth;
        private readonly string _ColumnFormat;
        private readonly RowActionInfo _LinkAction;

        public ReportFieldHandler()
        {
        }

        public ReportFieldHandler(
            string identifier,
            Phrase labelPhrase,
            string searchFieldName,
            int defaultWidth = 0,
            RowActionInfo linkAction = null,
            bool isSortable = true
        )
        {
            _Identifier = identifier;
            if (searchFieldName == null)
                _SearchFields = new string[] {identifier};
            else if (searchFieldName != string.Empty)
                _SearchFields = new string[] {searchFieldName};
            DefaultWidth = defaultWidth;
            _LinkAction = linkAction;
            _IsSortable = isSortable;
        }

        public ReportFieldHandler(string identifier, Phrase labelPhrase, ICollection<string> searchFieldNames,
            Phrase shortLabelPhrase, Phrase groupPhrase, int? defaultWidth, RowActionInfo linkAction = null,
            string columnFormat = null, bool isSortable = true)
        {
            _Identifier = identifier;
            _SearchFields = searchFieldNames;
            DefaultWidth = defaultWidth;
            _ColumnFormat = columnFormat;
            _LinkAction = linkAction;
            _IsSortable = isSortable;
        }

        public ReportFieldHandler(string identifier, GetTranslationDelegate getLabelDelegate,
            string searchFieldName = null, GetTranslationDelegate getShortLabelDelegate = null,
            GetTranslationDelegate getGroupDelegate = null, int? defaultWidth = null, RowActionInfo linkAction = null,
            string columnFormat = null, bool isSortable = true)
        {
            _Identifier = identifier;
            if (searchFieldName == null)
                _SearchFields = new string[] {identifier};
            else if (searchFieldName != string.Empty)
                _SearchFields = new string[] {searchFieldName};
            _GetGroupDelegate = getGroupDelegate;
            _GetLabelDelegate = getLabelDelegate;
            if (getShortLabelDelegate != null)
                _GetShortLabelDelegate = getShortLabelDelegate;
            else
                _GetShortLabelDelegate = getLabelDelegate;
            DefaultWidth = defaultWidth;
            _LinkAction = linkAction;
            _ColumnFormat = columnFormat;
            _IsSortable = isSortable;
        }

        public ReportFieldHandler(string identifier, GetTranslationDelegate getLabelDelegate,
            ICollection<string> searchFieldNames, GetTranslationDelegate getShortLabelDelegate,
            GetTranslationDelegate getGroupDelegate, int? defaultWidth,
            RowActionInfo linkAction = null, string columnFormat = null, bool isSortable = true)
        {
            _Identifier = identifier;
            _SearchFields = searchFieldNames;
            _GetLabelDelegate = getLabelDelegate;
            if (getShortLabelDelegate != null)
                _GetShortLabelDelegate = getShortLabelDelegate;
            else
                _GetShortLabelDelegate = getLabelDelegate;
            _GetGroupDelegate = getGroupDelegate;
            DefaultWidth = defaultWidth;
            _LinkAction = linkAction;
            _ColumnFormat = columnFormat;
            _IsSortable = isSortable;
        }
    }

    public interface IApiClientService : IEntityService<ApiClient>
    {
    }

    public class ApiClientService : PsaEntityService<ApiClient, IApiClientRepository>, IApiClientService
    {
        public static Random Random = new Random(DateTime.Now.Millisecond);
        private static readonly object RandomLockObject = new object();

        public class ApiQuotaLimit
        {
            public long? PerMinute { get; set; }
            public long? PerSecond { get; set; }
            public long? PerHour { get; set; }
            public long? PerDay { get; set; }
            public long? PerWeek { get; set; }
        }

        private readonly IGlobalSettingsService _GlobalSettingsService;
        public ApiClientService(IContextService<IPsaContext> contextService, IApiClientRepository repository, IValidator<ApiClient> validator, IAuthorization<IPsaContext, ApiClient> authorization, IGlobalSettingsService globalSettingsService) : base(contextService, repository, validator, authorization)
        {
            _GlobalSettingsService = globalSettingsService;
        }
    }

    public class ApiClient : ApiClientFields, IIdentifiableEntity, IOrganizationEntity, IIdentifiableEntityWithOriginalState<ApiClientFields>
    {
    }

    public interface ICurrencyService : IEntityService<Currency>
    {
    }

    public class CurrencyService : PsaEntityService<Currency, ICurrencyRepository>, ICurrencyService
    {
        public CurrencyService(IContextService<IPsaContext> contextService, ICurrencyRepository repository, IValidator<Currency> validator, IAuthorization<IPsaContext, Currency> authorization) : base(contextService, repository, validator, authorization)
        {
        }
    }

    public abstract class AuthorizationBase<TContext, TEntity> where TContext : IContext
    {
        private readonly IContextService<TContext> _ContextService;

        public AuthorizationBase(IContextService<TContext> contextService)
        {
            _ContextService = contextService;
        }
    }

    public abstract class PsaEntityAuthorization<TEntity> : AuthorizationBase<IPsaContext, TEntity>,
        IAuthorization<IPsaContext, TEntity> where TEntity : IOrganizationEntity
    {
        public PsaEntityAuthorization(IContextService<IPsaContext> context) : base(context)
        {
        }
    }

    public interface IUserLicensesService
    {
    }

    public interface IPsaOrganizationService
    {
    }


    public class PsaOrganizationService : OrganizationServiceBase<IPsaContext, Organization, IOrganizationRepository>,
        IPsaOrganizationService
    {
        protected readonly IOrganizationAddonService OrganizationAddonService;
        private readonly IUserRepository _UserRepository;
        private readonly IMasterUserRepository _MasterUserRepository;
        private readonly ISettingsRepository _SettingsRepository;
        protected readonly IAuthenticationService AuthenticationService;
        protected readonly IDict Dict;
        private readonly IExternallyOwnedOrganizationService _ExternallyOwnedOrganizationService;

        public PsaOrganizationService(IContextService<IPsaContext> contextService, IOrganizationRepository repository,
            IValidator<Organization> validator, IAuthorization<IPsaContext, Organization> authorization,
            IOrganizationService organizationService,
            IOrganizationVatChangeService vatChangeService, IOrganizationNameChangeService nameChangeService,
            ICustomerDatabaseRepository customerDatabaseRepository, IOrganizationAddonService organizationAddonService,
            IUserRepository userRepository, IMasterUserRepository masterUserRepository,
            ISettingsRepository settingsRepository, IAuthenticationService authenticationService,
            IDict dict, IExternallyOwnedOrganizationService externallyOwnedOrganizationService) : base(
            contextService, repository, validator, authorization, organizationService, vatChangeService,
            nameChangeService, customerDatabaseRepository)
        {
            OrganizationAddonService = organizationAddonService;
            _UserRepository = userRepository;
            _MasterUserRepository = masterUserRepository;
            _SettingsRepository = settingsRepository;
            AuthenticationService = authenticationService;
            Dict = dict;
            _ExternallyOwnedOrganizationService = externallyOwnedOrganizationService;
        }
    }

    public class UserLicensesService : IUserLicensesService
    {
        protected readonly ICurrentUserService CurrentUserService;
        protected readonly IOrganizationAddonService OrganizationAddonService;
        protected readonly IMasterUserRepository MasterUserRepository;
        protected readonly IBillingService BillingService;
        protected readonly IBilledRowRepository BilledRowRepository;

        public UserLicensesService(ICurrentUserService currentUserService, IOrganizationAddonService organizationAddonService, IMasterUserRepository masterUserRepository, IBillingService billingService, IBilledRowRepository billedRowRepository)
        {
            CurrentUserService = currentUserService;
            OrganizationAddonService = organizationAddonService;
            MasterUserRepository = masterUserRepository;
            BillingService = billingService;
            BilledRowRepository = billedRowRepository;
        }
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IPsaContextService PsaContextService;
        private readonly ITimeService TimeService;

        public CurrentUserService(IPsaContextService psaContextService, ITimeService timeService)
        {
            PsaContextService = psaContextService;
            TimeService = timeService;
        }
    }

    public interface ICurrentUserService
    {
    }

    public interface IPsaUserProfilePictureService
    {
    }

    public class PsaUserProfilePictureService : IPsaUserProfilePictureService
    {
    }


    public interface IContextDataStorage<TContext> where TContext : IContext
    {
    }

    public interface IPsaContextStorage : IContextDataStorage<IPsaContext>
    {
    }

    public class PsaContextStorage : ContextDataStorage<IPsaContext>, IPsaContextStorage
    {
    }

    public class ContextDataStorage<TContext> : IContextDataStorage<TContext> where TContext : IContext
    {
    }

    public partial class OrganizationCompanyFields : OrganizationEntity, INamedEntity
    {
    }


    public partial class OrganizationCompany : OrganizationCompanyFields,
        IIdentifiableEntityWithOriginalState<OrganizationCompanyFields>
    {
    }


    public interface IOrganizationCompanyRepository : IEntityRepository<OrganizationCompany>
    {
    }

    public class OrganizationCompanyRepository : OrganizationEntityRepository<OrganizationCompanyFields, OrganizationCompany, IPsaContext>, IOrganizationCompanyRepository
    {
    }

    public interface IPsaCompany : ICompany
    {
    }

    public interface IOrganizationContextBase<TOrganizationUser, TCompany> : IContext<TOrganizationUser, TCompany>,
        ISharedContext where TOrganizationUser : IUser where TCompany : ICompany
    {
    }

    public abstract class
        EntityService<TContext, TEntity, TRepository> : EntityServiceBase<TContext, TEntity, TRepository>,
            IEntityService<TEntity>
        where TContext : IContext
        where TEntity : IIdentifiableEntity
        where TRepository : IEntityRepository<TEntity>
    {
        public EntityService(IContextService<TContext> contextService, TRepository repository,
            IValidator<TEntity> validator, IAuthorization<TContext, TEntity> authorization) : base()
        {
        }

        protected EntityService() : base()
        {
        }
    }

    public abstract class
        OrganizationEntityService<TEntity, TRepository, TUser> : EntityService<TContext, TEntity, TRepository>
        where TEntity : IOrganizationEntity
        where TRepository : IEntityRepository<TEntity>
        where TUser : IOrganizationUserBase
    {
        protected OrganizationEntityService() : base()
        {
        }

        protected OrganizationEntityService(IContextService<TContext> contextService, TRepository repository,
            IValidator<TEntity> validator, IAuthorization<TContext, TEntity> authorization) : base(contextService,
            repository, validator, authorization)
        {
        }
    }


    public interface IPsaContext
        : IOrganizationContextBase<User, IPsaCompany>
    {
    }

    public partial class RefreshTokenFields : OrganizationEntity
    {
    }

    public partial class RefreshTokenRepository : OrganizationEntityRepository<RefreshTokenFields, RefreshToken, IPsaContext>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(IContextService<IPsaContext> contextService, IConfiguration configuration, DbProviderFactory dbProviderFactory) : base(contextService, configuration, dbProviderFactory)
        {
        }
    }

    public interface IRefreshTokenService : IEntityService<RefreshToken>
    {
    }

    public interface IRefreshTokenRepository : IEntityRepository<RefreshToken>
    {
    }

    public class RefreshTokenAuthorization : PsaEntityAuthorization<RefreshToken>
    {
        public RefreshTokenAuthorization(IContextService<IPsaContext> contextService) : base(contextService)
        {
        }
    }

    public class RefreshTokenService : OrganizationEntityService<RefreshToken, IRefreshTokenRepository, User, IPsaContext>, IRefreshTokenService
    {
        public RefreshTokenService(IContextService<IPsaContext> contextService, IRefreshTokenRepository repository, IValidator<RefreshToken> validator, IAuthorization<IPsaContext, RefreshToken> authorization) : base(contextService, repository, validator, authorization)
        {
        }
    }

    public class RefreshToken : RefreshTokenFields, IIdentifiableEntity, IIdentifiableEntityWithOriginalState<RefreshTokenFields>
    {
    }

    public interface IApiClientRepository : IEntityRepository<ApiClient>
    {
    }

    public class ApiClientRepository : OrganizationEntityRepository<ApiClientFields, ApiClient, IPsaContext>, IApiClientRepository
    {
        public ApiClientRepository(IContextService<IPsaContext> contextService, IConfiguration configuration, DbProviderFactory dbProviderFactory) : base(contextService, configuration, dbProviderFactory)
        {
        }
    }

    public class ApiClientFields : IOrganizationEntity
    {
    }

    public interface ISettingsRepository : IEntityRepository<Settings>
    {
    }

    public partial class SettingsRepository : OrganizationEntityRepository<SettingsFields, Settings, IPsaContext>, ISettingsRepository
    {
        public SettingsRepository(IContextService<IPsaContext> contextService, IConfiguration configuration, DbProviderFactory dbProviderFactory, ICustomerDatabaseRepository customerDatabaseRepository) : base(contextService, configuration, dbProviderFactory, customerDatabaseRepository)
        {
        }
    }

    public interface IOrganizationCompanyService : IEntityService<OrganizationCompany>
    {
    }

    public class OrganizationCompanyService :
        OrganizationEntityService<OrganizationCompany, IOrganizationCompanyRepository, User>,
        IOrganizationCompanyService
    {
        private ITaxService _TaxService;

        public OrganizationCompanyService(IContextService<IPsaContext> contextService,
            IOrganizationCompanyRepository repository, IValidator<OrganizationCompany> validator,
            IAuthorization<IPsaContext, OrganizationCompany> authorization, ITaxService taxService
        ) : base()
        {
            _TaxService = taxService;
        }
    }

    public interface ITaxService
    {
    }

    public partial class Tax : TaxFields, IOrganizationEntity, IIdentifiableEntityWithOriginalState<TaxFields>
    {
    }

    public class TaxService : PsaEntityService<Tax, ITaxRepository>, ITaxService
    {
        private readonly ICountryTaxRepository _CountryTaxRepository;

        public TaxService(IContextService<IPsaContext> contextService, ITaxRepository repository, IValidator<Tax> validator, IAuthorization<IPsaContext, Tax> authorization, ICountryTaxRepository countryTaxRepository) : base(contextService, repository, validator, authorization)
        {
            _CountryTaxRepository = countryTaxRepository;
        }
    }

    public interface IFeatureToggleService
    {
    }

    public class FeatureToggleService : IFeatureToggleService
    {
        protected readonly IPsaContextService ContextService;
        private IFeatureToggleRepository _Repository;
        private IFeatureRepository _FeatureRepository;

        public FeatureToggleService(IPsaContextService contextService, IFeatureToggleRepository repository, IFeatureRepository featureRepository)
        {
            ContextService = contextService;
            _Repository = repository;
            _FeatureRepository = featureRepository;
        }
    }

    public class InternalXmlHelper
    {
    }

    public interface IUserInfoTokenService : IEntityService<UserInfoToken>
    {
    }

    public interface IUserInfoTokenRepository : IEntityRepository<UserInfoToken>
    {
    }

    public partial class UserInfoTokenFields : IdentifiableEntity
    {
    }

    public partial class UserInfoTokenRepository : EntityRepository<UserInfoTokenFields, UserInfoToken, IPsaContext>, IUserInfoTokenRepository
    {
        public UserInfoTokenRepository(IContextService<IPsaContext> contextService, IConfiguration configuration, DbProviderFactory dbProviderFactory) : base(contextService, configuration, dbProviderFactory)
        {
        }
    }

    public class UserInfoTokenService : SharedEntityService<UserInfoToken, IUserInfoTokenRepository>, IUserInfoTokenService
    {
        public UserInfoTokenService(IContextService<ISharedContext> contextService, IUserInfoTokenRepository repository, IValidator<UserInfoToken> validator, ISharedAuthorization<UserInfoToken> authorization) : base(contextService, repository, validator, authorization)
        {
        }
    }

    public class UserInfoToken : UserInfoTokenFields, IIdentifiableEntityWithOriginalState<UserInfoTokenFields>
    {
    }


    public interface ICurrency
    {
    }


    public interface IAppSettings
    {
    }


    public class AppSettings
        : IAppSettings
    {
        public AppSettings(
        )
        {
        }
    }


    public interface ISharedContext
        : IContext
    {
    }


    public class SimpleCurrency
        : ICurrency
    {
        public SimpleCurrency(
        )
        {
        }
    }


    public partial interface ISharedAuthorization
        : IAuthorization
    {
    }


    public interface IOrganizationContextScopeService
    {
    }

    public class OrganizationContextScopeService : IOrganizationContextScopeService
    {
        private readonly IPsaContextStorage _ContextStorage;
        private readonly IOrganizationRepository _OrganizationRepository;

        /// <summary>
        ///     ''' Initializes a new instance of the OrganizationContextScopeService class.
        ///     ''' </summary>
        ///     ''' <param name="contextStorage">A storage to use to storing the context.</param>
        public OrganizationContextScopeService(IPsaContextStorage contextStorage, IOrganizationRepository organizationRepository)
        {
            if (contextStorage == null)
                throw new ArgumentNullException("contextStorage");

            _ContextStorage = contextStorage;
            _OrganizationRepository = organizationRepository;
        }
    }

    public class TContext : IContext
    {
    }


    public class CustomFormulaGenericPartParameter
        : ICustomFormulaNumericPartParameter
    {
        public CustomFormulaGenericPartParameter(
        )
        {
        }

        public CustomFormulaGenericPartParameter(
            SearchFieldDataType arg0,
            int? arg1
        )
        {
        }

        public CustomFormulaGenericPartParameter(
            string arg0,
            Phrase arg1,
            SearchFieldDataType arg2,
            int? arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly string field0;
        public readonly Phrase field1;
        public readonly SearchFieldDataType field2;
        public readonly int? field3;
    }


    public class CustomFormulaHelper
    {
    }


    public class AdditionFunction
        : ICustomFormulaNumericPartParameter
    {
        public AdditionFunction(
        )
        {
        }

        public AdditionFunction(
            ICustomFormulaNumericPartParameter arg0,
            ICustomFormulaNumericPartParameter arg1,
            bool arg2,
            SearchFieldDataType arg3,
            int? arg4
        )
        {
            field4 = arg4;
        }

        public AdditionFunction(
            List<ICustomFormulaNumericPartParameter> arg0,
            bool arg1,
            SearchFieldDataType arg2,
            int? arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly List<ICustomFormulaNumericPartParameter> field0;
        public readonly bool field1;
        public readonly SearchFieldDataType field2;
        public readonly int? field3;
        public readonly int? field4;
    }


    public class ConstantValue
        : ICustomFormulaNumericPartParameter
    {
        public ConstantValue(
        )
        {
        }

        public ConstantValue(
            SearchFieldDataType arg0,
            int? arg1
        )
        {
        }

        public ConstantValue(
            decimal? arg0,
            SearchFieldDataType arg1,
            int? arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly decimal? field0;
        public readonly SearchFieldDataType field1;
        public readonly int? field2;
    }


    public class DivisionFunction
        : ICustomFormulaNumericPartParameter
    {
        public DivisionFunction(
        )
        {
        }

        public DivisionFunction(
            SearchFieldDataType arg0
        )
        {
        }

        public DivisionFunction(
            ICustomFormulaNumericPartParameter arg0,
            ICustomFormulaNumericPartParameter arg1,
            bool arg2,
            SearchFieldDataType arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly ICustomFormulaNumericPartParameter field0;
        public readonly ICustomFormulaNumericPartParameter field1;
        public readonly bool field2;
        public readonly SearchFieldDataType field3;
    }


    public class IsNullFunction
        : ICustomFormulaNumericPartParameter
    {
        public IsNullFunction(
        )
        {
        }

        public IsNullFunction(
            ICustomFormulaNumericPartParameter arg0,
            ICustomFormulaNumericPartParameter arg1,
            SearchFieldDataType arg2,
            int? arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly ICustomFormulaNumericPartParameter field0;
        public readonly ICustomFormulaNumericPartParameter field1;
        public readonly SearchFieldDataType field2;
        public readonly int? field3;
    }


    public class MultiplicationFunction
        : ICustomFormulaNumericPartParameter
    {
        public MultiplicationFunction(
        )
        {
        }

        public MultiplicationFunction(
            List<ICustomFormulaNumericPartParameter> arg0
        )
        {
            field0 = arg0;
        }

        public MultiplicationFunction(
            List<ICustomFormulaNumericPartParameter> arg0,
            bool arg1,
            SearchFieldDataType arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly List<ICustomFormulaNumericPartParameter> field0;
        public readonly bool field1;
        public readonly SearchFieldDataType field2;
    }


    public class PercentageFunction
        : ICustomFormulaNumericPartParameter
    {
        public PercentageFunction(
        )
        {
        }

        public PercentageFunction(
            ICustomFormulaNumericPartParameter arg0,
            ICustomFormulaNumericPartParameter arg1,
            bool arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICustomFormulaNumericPartParameter field0;
        public readonly ICustomFormulaNumericPartParameter field1;
        public readonly bool field2;
    }


    public class RangeFunction
        : ICustomFormulaNumericPartParameter
    {
        public RangeFunction(
        )
        {
        }

        public RangeFunction(
            SearchFieldDataType arg0,
            int? arg1
        )
        {
        }

        public RangeFunction(
            ICustomFormulaNumericPartParameter arg0,
            IEnumerable<Decimal> arg1,
            string arg2,
            SearchFieldDataType arg3,
            int? arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly ICustomFormulaNumericPartParameter field0;
        public readonly IEnumerable<Decimal> field1;
        public readonly string field2;
        public readonly SearchFieldDataType field3;
        public readonly int? field4;
    }


    public class RoundingFunction
        : ICustomFormulaNumericPartParameter
    {
        public RoundingFunction(
        )
        {
        }

        public RoundingFunction(
            SearchFieldDataType arg0,
            int? arg1
        )
        {
            field2 = arg0;
            field3 = arg1;
        }

        public RoundingFunction(
            ICustomFormulaNumericPartParameter arg0,
            int arg1,
            SearchFieldDataType arg2,
            int? arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly ICustomFormulaNumericPartParameter field0;
        public readonly int field1;
        public readonly SearchFieldDataType field2;
        public readonly int? field3;
    }


    public class SubtractionFunction
        : ICustomFormulaNumericPartParameter
    {
        public SubtractionFunction(
        )
        {
        }

        public SubtractionFunction(
            ICustomFormulaNumericPartParameter arg0,
            ICustomFormulaNumericPartParameter arg1,
            bool arg2,
            SearchFieldDataType arg3,
            int? arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly ICustomFormulaNumericPartParameter field0;
        public readonly ICustomFormulaNumericPartParameter field1;
        public readonly bool field2;
        public readonly SearchFieldDataType field3;
        public readonly int? field4;
    }


    public class CompositePhraseTranslationEntry : PhraseTranslationEntry
    {
        public CompositePhraseTranslationEntry(
            Phrase arg0,
            IDict arg1
        ) : base(arg0)
        {
            field1 = arg1;
        }

        public readonly IDict field1;
    }


    public class ContextHelp
    {
    }


    public class DictionaryCategory
    {
    }


    public class KeywordDictionary : Dictionary<String, String>
    {
        public KeywordDictionary(
        )
        {
        }
    }


    public class PhraseTranslationEntry
        : ITranslationEntry
    {
        public PhraseTranslationEntry(
            Phrase arg0
        )
        {
            field0 = arg0;
        }

        public readonly Phrase field0;
    }


    public class TranslationListOrder
    {
    }


    public class AddonFlag
    {
    }


    public class AddonIdentifier
    {
    }


    public class AddonIdentifierExtensions
    {
    }


    public class CountryRegionOption
    {
    }


    public class Country : CountryFields, IIdentifiableEntityWithOriginalState<CountryFields>
    {
        public Country(
        )
        {
        }
    }


    public class CurrencyBase : CurrencyBaseFields
        , INamedEntity, IIdentifiableEntityWithOriginalState<CurrencyBaseFields>
    {
        public CurrencyBase(
        )
        {
        }
    }


    public class CustomerDatabase : CustomerDatabaseFields
        , IIdentifiableEntityWithOriginalState<CustomerDatabaseFields>
    {
        public CustomerDatabase(
        )
        {
        }
    }


    public class FeatureToggleWithIdentifier : FeatureToggle
    {
        public FeatureToggleWithIdentifier(
        )
        {
        }
    }


    public class CustomerDatabaseFields : IdentifiableEntity
        , INamedEntity
    {
        public CustomerDatabaseFields(
        )
        {
        }
    }


    public class CountryFields : IdentifiableEntity
        , INamedEntity
    {
        public CountryFields(
        )
        {
        }
    }


    public class CountryRegionFields : IdentifiableEntity
        , INamedEntity
    {
        public CountryRegionFields(
        )
        {
        }
    }


    public class CountryRegion : CountryRegionFields
        , IIdentifiableEntityWithOriginalState<CountryRegionFields>
    {
        public CountryRegion(
        )
        {
        }
    }


    public class CountryTaxFields : IdentifiableEntity
    {
        public CountryTaxFields(
        )
        {
        }
    }


    public class CountryTax : CountryTaxFields
        , IIdentifiableEntityWithOriginalState<CountryTaxFields>
    {
        public CountryTax(
        )
        {
        }
    }


    public class CurrencyBaseFields : IdentifiableEntity
    {
        public CurrencyBaseFields(
        )
        {
        }
    }


    public class FeatureFields : IdentifiableEntity
    {
        public FeatureFields(
        )
        {
        }
    }


    public class Feature : FeatureFields
        , IIdentifiableEntityWithOriginalState<FeatureFields>
    {
        public Feature(
        )
        {
        }
    }


    public class FeatureToggleFields : IdentifiableEntity
    {
        public FeatureToggleFields(
        )
        {
        }
    }


    public class FeatureToggle : FeatureToggleFields
        , IIdentifiableEntityWithOriginalState<FeatureToggleFields>
    {
        public FeatureToggle(
        )
        {
        }
    }


    public class FormatingCultureFields : IdentifiableEntity
        , INamedEntity
    {
        public FormatingCultureFields(
        )
        {
        }
    }


    public class FormatingCulture : FormatingCultureFields
        , IIdentifiableEntityWithOriginalState<FormatingCultureFields>
    {
        public FormatingCulture(
        )
        {
        }
    }


    public class GlobalSettingsFields : IdentifiableEntity
    {
        public GlobalSettingsFields(
        )
        {
        }
    }


    public class GlobalSettings : GlobalSettingsFields
        , IIdentifiableEntityWithOriginalState<GlobalSettingsFields>
    {
        public GlobalSettings(
        )
        {
        }
    }


    public class KeywordFields : IdentifiableEntity
    {
        public KeywordFields(
        )
        {
        }
    }


    public class Keyword : KeywordFields
        , IIdentifiableEntityWithOriginalState<KeywordFields>
    {
        public Keyword(
        )
        {
        }
    }


    public class LanguageFields : IdentifiableEntity
        , INamedEntity
    {
        public LanguageFields(
        )
        {
        }
    }


    public partial class Language : LanguageFields
        , IIdentifiableEntityWithOriginalState<LanguageFields>
    {
        public Language(
        )
        {
        }
    }


    public class RightFields : IdentifiableEntity
        , INamedEntity
    {
        public RightFields(
        )
        {
        }
    }


    public class Right : RightFields
        , IIdentifiableEntityWithOriginalState<RightFields>
    {
        public Right(
        )
        {
        }
    }


    public class TimeZoneFields : IdentifiableEntity
        , INamedEntity
    {
        public TimeZoneFields(
        )
        {
        }
    }


    public class TranslationFields : IdentifiableEntity
    {
        public TranslationFields(
        )
        {
        }
    }


    public class Translation : TranslationFields
        , IIdentifiableEntityWithOriginalState<TranslationFields>
    {
        public Translation(
        )
        {
        }
    }


    public class HostingProviderFields : IdentifiableEntity
    {
        public HostingProviderFields(
        )
        {
        }
    }


    public class HostingProvider : HostingProviderFields
        , IIdentifiableEntityWithOriginalState<HostingProviderFields>
    {
        public HostingProvider(
        )
        {
        }
    }


    public class OrganizationHostingProviderFields : IdentifiableEntity
    {
        public OrganizationHostingProviderFields(
        )
        {
        }
    }


    public class OrganizationHostingProvider : OrganizationHostingProviderFields
        , IIdentifiableEntityWithOriginalState<OrganizationHostingProviderFields>
    {
        public OrganizationHostingProvider(
        )
        {
        }
    }


    public class HostingProviderSalesAccountFields : IdentifiableEntity
        , INamedEntity
    {
        public HostingProviderSalesAccountFields(
        )
        {
        }
    }


    public class HostingProviderSalesAccount : HostingProviderSalesAccountFields
        , IIdentifiableEntityWithOriginalState<HostingProviderSalesAccountFields>
    {
        public HostingProviderSalesAccount(
        )
        {
        }
    }


    public class PartnerFields : IdentifiableEntity
        , INamedEntity
    {
        public PartnerFields(
        )
        {
        }
    }


    public class Partner : PartnerFields
        , IIdentifiableEntityWithOriginalState<PartnerFields>
    {
        public Partner(
        )
        {
        }
    }


    public class PartnerEmailFields : IdentifiableEntity
    {
        public PartnerEmailFields(
        )
        {
        }
    }


    public class PartnerEmail : PartnerEmailFields
        , IIdentifiableEntityWithOriginalState<PartnerEmailFields>
    {
        public PartnerEmail(
        )
        {
        }
    }


    public class PartnerPricingModelSettingFields : IdentifiableEntity
    {
        public PartnerPricingModelSettingFields(
        )
        {
        }
    }


    public class PartnerPricingModelSetting : PartnerPricingModelSettingFields
        , IIdentifiableEntityWithOriginalState<PartnerPricingModelSettingFields>
    {
        public PartnerPricingModelSetting(
        )
        {
        }
    }


    public class RegistrationFields : IdentifiableEntity
    {
        public RegistrationFields(
        )
        {
        }
    }


    public class Registration : RegistrationFields
        , IIdentifiableEntityWithOriginalState<RegistrationFields>
    {
        public Registration(
        )
        {
        }
    }


    public class MasterOrganizationFields : IdentifiableEntity
        , INamedEntity
    {
        public MasterOrganizationFields(
        )
        {
        }
    }


    public class MasterOrganization : MasterOrganizationFields
        , IIdentifiableEntityWithOriginalState<MasterOrganizationFields>, IMasterOrganization
    {
        public MasterOrganization(
        )
        {
        }
    }


    public interface IDistributorSettings
    {
    }


    public class FinnishDistributorSettings
        : IDistributorSettings
    {
        public FinnishDistributorSettings(
        )
        {
        }
    }


    public interface IMasterOrganization
        : IIdentifiableEntity
    {
    }


    public class IncomeSource
    {
    }


    public class InvoicingSegment
    {
    }


    public class OrganizationAddons
    {
        public OrganizationAddons(
            AddonFlag arg0
        )
        {
            field0 = arg0;
        }

        public readonly AddonFlag field0;
    }


    public class OrganizationInfoForHostingProviderAccount
    {
        public OrganizationInfoForHostingProviderAccount(
        )
        {
        }
    }


    public class PricingModelEdition
    {
    }


    public class PricingModel
    {
        public PricingModel(
        )
        {
        }
    }


    public class TermsOfService
    {
        public TermsOfService(
        )
        {
        }
    }


    public class DataNotFoundException : AppExceptionEx
    {

        public DataNotFoundException(
            string arg0,
            Exception arg1
        ) : base(arg0, arg1)
        {
        }

        public DataNotFoundException(
            string arg0,
            Exception arg1,
            AppExceptionFlags arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class AppExceptionEx : AppException
    {
        public AppExceptionEx() { }

        public AppExceptionEx(
            string arg0,
            Exception arg1
        ) : base(arg0, arg1)
        {
        }

        public AppExceptionEx(
            string arg0,
            Exception arg1,
            AppExceptionFlags arg2
        ) : base(arg0, arg1, arg2)
        {
        }

        public AppExceptionEx(
            SerializationInfo arg0,
            StreamingContext arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class AppValidationExceptionEx : AppValidationException
    {
        public AppValidationExceptionEx(
            string arg0,
            Exception arg1
        ) : base(arg0, arg1)
        {
        }

        public AppValidationExceptionEx(
            string arg0,
            Exception arg1,
            AppExceptionFlags arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public interface IKpiLookoutHandler
    {
    }


    public interface IKpiLookoutParameters
    {
    }


    public class AppearanceReportFieldHandler : ReportFieldHandler<StringReportField>
    {
        public AppearanceReportFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            string arg3,
            string arg4,
            string arg5,
            RowActionInfo arg6,
            Phrase arg7,
            Phrase arg8,
            int? arg9
        )
        {
            field9 = arg9;
        }

        public readonly int? field9;
    }


    public class AuthorizedChartDataFieldFactory : AuthorizedIdentifiableEntityFactory<TContext, IChartDataFieldHandler>
    {
        public AuthorizedChartDataFieldFactory(
            ISearchDefinition<TContext> arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ISearchDefinition<TContext> field0;
    }

    public class AuthorizedIdentifiableEntityFactory<T1, T2>
    {
    }

    public class AuthorizedReportFieldFactory : AuthorizedIdentifiableEntityFactory<TContext, TReportFieldHandler>
    {
        public AuthorizedReportFieldFactory(
            ISearchDefinition<TContext> arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ISearchDefinition<TContext> field0;
    }

    public interface ISearchDefinition<T>
    {
    }

    public class TReportFieldHandler
    {
    }


    public class AuthorizedReportFilterFactory : AuthorizedIdentifiableEntityFactory<TContext, IReportFilterHandler>
    {
        public AuthorizedReportFilterFactory(
            ISearchDefinition<TContext> arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ISearchDefinition<TContext> field0;
    }


    public class AuthorizedStackingGroupFactory : AuthorizedIdentifiableEntityFactory<TContext, IChartStackingGroup>
    {
        public AuthorizedStackingGroupFactory(
            ISearchDefinition<TContext> arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ISearchDefinition<TContext> field0;
    }


    public class BooleanReportFilter : ReportFilter
    {
        public BooleanReportFilter(
            string arg0,
            bool? arg1
        ) : base(arg0, arg1)
        {
        }

        protected BooleanReportFilter(string arg0)
        {
        }
    }


    public class BooleanReportFilterHandler : ReportFilterHandler<BooleanReportFilter>
    {
        public BooleanReportFilterHandler(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            BindCriteriaDelegate<BooleanReportFilter> arg3,
            BindCriteriaDelegate<BooleanReportFilter> arg4,
            Phrase arg5,
            string arg6,
            bool? arg7,
            string arg8,
            bool arg9
        )
        {
        }

        protected BooleanReportFilterHandler()
        {
            throw new NotImplementedException();
        }
    }

    public class ReportFilterHandler<T>
    {
    }

    public class BindCriteriaDelegate<T>
    {
    }


    public class ChartDataFieldHandler
        : IChartDataFieldHandler
    {
        public ChartDataFieldHandler(
            string arg0,
            string arg2,
            string arg4,
            AggregateFunction arg6,
            bool arg7
        )
        {
            field0 = arg0;
            field2 = arg2;
            field4 = arg4;
            field6 = arg6;
            field7 = arg7;
        }

        public ChartDataFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            Phrase arg3,
            string arg4,
            AggregateFunction arg6,
            bool arg7
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field6 = arg6;
            field7 = arg7;
        }

        public readonly string field0;
        public readonly Phrase field1;
        public readonly string field2;
        public readonly Phrase field3;
        public readonly string field4;
        public readonly AggregateFunction field6;
        public readonly bool field7;
    }


    public class ChartEntityCategory : ChartAxisBase
    {
        public ChartEntityCategory(
            string arg0,
            string arg1,
            ICollection<IReportFilter> arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class ChartValueSeries : ChartSeriesBase
    {
        public ChartValueSeries(
            string arg0,
            string arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class CrossTabReport : ReportWithFilters
        , ICrossTabReport
    {
    }

    public interface ICurrencyRepository : IEntityRepository<Currency>
    {
    }

    public partial class CurrencyRepository : OrganizationEntityRepository<CurrencyFields, Currency, IPsaContext>, ICurrencyRepository
    {
        public CurrencyRepository(IContextService<IPsaContext> contextService, IConfiguration configuration, DbProviderFactory dbProviderFactory, ICustomerDatabaseRepository customerDatabaseRepository) : base(contextService, configuration, dbProviderFactory, customerDatabaseRepository)
        {
        }
    }

    public abstract class ReportWithFiltersHandler<TContext, TReport> : ReportHandler<TContext, TReport>,
        IReportWithFiltersHandler<TContext>
        where TContext : IContext
        where TReport : IReportWithFilters
    {
        protected ReportWithFiltersHandler(IContextService<TContext> contextService) : base(contextService)
        {
        }
    }


    public abstract class CrossTabReportHandler<TContext, TReport> : ReportWithFiltersHandler<TContext, TReport>
        where TContext : IContext
        where TReport : ICrossTabReport
    {
        private AuthorizedIdentifiableEntityFactory<TContext, IGroupReportFieldHandler> _AllRowFields;

        private AuthorizedIdentifiableEntityFactory<TContext, ICrossTabDataReportFieldHandler> _AllDataFields;
        // Private _AllFieldGroups As AuthorizedIdentifiableEntityFactory(Of TContext, IReportDataFieldGroup)

        protected CrossTabReportHandler(IContextService<TContext> contextService) : base(contextService)
        {
        }
    }


    public class CurrencyReportField : NumericReportField
    {
        public CurrencyReportField(
            string arg0,
            string arg1,
            int? arg2,
            bool arg4
        )
        {
        }
    }


    public class CurrencyReportFieldHandler
    {
        public CurrencyReportFieldHandler()
        {
        }

        public CurrencyReportFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            string arg3,
            Phrase arg4,
            string arg5,
            Phrase arg6,
            Phrase arg7,
            int? arg8,
            RowActionInfo arg9,
            TotalCalculationType arg10,
            CurrencyUsage arg11
        )
        {
        }
    }


    public class DatePeriod
    {
    }


    public class DatePartGroupField : ReportField
    {
        public DatePartGroupField(
            string arg0,
            DatePeriod arg1,
            string arg2,
            int? arg3
        ) : base()
        {
        }
    }


    public interface IFieldMultiplier
    {
    }


    public class DatePartGroupFieldHandler : ReportFieldHandler<DatePartGroupField>
        , IFieldMultiplier
    {
        public DatePartGroupFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            Phrase arg4,
            Phrase arg5,
            RowActionInfo arg6
        ) : base()
        {
        }
    }


    public class DateRangeReportField : ReportField
    {
        public DateRangeReportField(
            string arg0,
            string arg1
        ) : base()
        {
        }
    }


    public class DateRangeReportFieldHandler : ReportFieldHandler<TReportField>
    {
        public DateRangeReportFieldHandler(
            IContextService arg0,
            string arg1,
            Phrase arg2,
            string arg3,
            string arg4,
            IEnumerable<String> arg5,
            Phrase arg6,
            Phrase arg7,
            string arg8,
            DateConversion arg9,
            int? arg10,
            RowActionInfo arg11
        ) : base()
        {
            field9 = arg9;
            field10 = arg10;
            field11 = arg11;
        }

        public readonly DateConversion field9;
        public readonly int? field10;
        public readonly RowActionInfo field11;
    }

    public class TReportField : IReportField
    {
    }


    public class DateRangeReportFilter : ReportFilter
    {
        public DateRangeReportFilter(
            string arg0,
            TimePeriod arg1
        ) : base()
        {
        }
    }


    public class DateRangeReportFilterHandler : ReportFilterHandler<DateRangeReportFilter>
    {
        public DateRangeReportFilterHandler(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            BindCriteriaDelegate<DateRangeReportFilter> arg3,
            BindCriteriaDelegate<DateRangeReportFilter> arg4,
            Phrase arg5,
            string arg6,
            TimePeriod arg7,
            bool arg8
        ) : base()
        {
        }
    }


    public class DateReportField : ReportField
    {
        public DateReportField(
            string arg0,
            string arg1,
            int? arg2,
            bool arg4
        ) : base()
        {
        }
    }


    public class DateReportFieldHandler : ReportFieldHandler<TReportField>
    {
        public DateReportFieldHandler(
            IContextService arg0,
            string arg1,
            Phrase arg2,
            string arg3,
            IEnumerable<String> arg4,
            Phrase arg5,
            Phrase arg6,
            string arg7,
            DateConversion arg8,
            int? arg9,
            RowActionInfo arg10
        ) : base()
        {
            field9 = arg9;
            field10 = arg10;
        }

        public readonly int? field9;
        public readonly RowActionInfo field10;
    }


    public class GenericReportFieldHandler : ReportFieldHandler<StringReportField>
    {
        public GenericReportFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            string arg3,
            string arg4,
            IEnumerable<String> arg5,
            RowActionInfo arg6,
            Phrase arg7,
            Phrase arg8,
            int? arg9
        ) : base()
        {
            field9 = arg9;
        }

        public readonly int? field9;
    }


    public class IdGroupReportFieldHandler : ReportFieldHandler<StringReportField>, IGroupReportFieldHandler
    {
        public IdGroupReportFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            ICollection<String> arg3,
            GetRowDataDelegate arg4,
            Phrase arg5,
            Phrase arg6,
            int? arg7,
            RowActionInfo arg8
        ) : base()
        {
        }

        public IdGroupReportFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            string arg3,
            Phrase arg4,
            Phrase arg5,
            int? arg6,
            RowActionInfo arg7
        ) : base()
        {
        }

        protected IdGroupReportFieldHandler(RowFieldActionInfo<IPsaContext, WorkHourMatrixReport> arg0)
        {
        }

        protected IdGroupReportFieldHandler()
        {
        }

        public new bool Equals(object x, object y)
        {
            return true;
        }

        public int GetHashCode(object obj)
        {
            return 0;
        }
    }


    public class IDListReportFilter : ReportFilter
    {
        public IDListReportFilter(
            string arg0,
            IEnumerable<Int32> arg1,
            bool arg2
        ) : base(arg0)
        {
            field2 = arg2;
        }

        public IDListReportFilter(
            string arg0,
            int arg1,
            bool arg2
        ) : base(arg0)
        {
            field2 = arg2;
        }

        public readonly bool field2;

        protected IDListReportFilter()
        {
        }
    }


    public class IDListReportFilterHandler : ReportFilterHandler<IDListReportFilter>
    {
        public IDListReportFilterHandler(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            BindCriteriaDelegate<IDListReportFilter> arg3,
            BindCriteriaDelegate<IDListReportFilter> arg4,
            Phrase arg5,
            string arg6,
            string arg7,
            ICollection arg8,
            bool arg9,
            bool arg10
        ) : base()
        {
            field10 = arg10;
        }

        public IDListReportFilterHandler(
            string arg0,
            GetTranslationDelegate arg1,
            GetTranslationDelegate arg2,
            string arg3,
            ICollection arg4,
            BindCriteriaDelegate<IDListReportFilter> arg5,
            BindCriteriaDelegate<IDListReportFilter> arg6,
            string arg7,
            bool arg8,
            bool arg9
        ) : base()
        {
        }

        public IDListReportFilterHandler()
        {
        }

        public readonly bool field10;
    }


    public class KpiReportFieldHandler : ReportFieldHandler<NumericReportField>
        , INumericReportFieldHandler, ICrossTabDataReportFieldHandler
    {
        public KpiReportFieldHandler() { }

        public KpiReportFieldHandler(
            string arg0,
            string arg1,
            CreateFormulaParameterDelegate arg2,
            string arg3,
            string arg4,
            string arg5,
            string arg6,
            int? arg7,
            RowActionInfo arg8,
            bool arg10
        ) : base()
        {
            field10 = arg10;
        }

        public KpiReportFieldHandler(
            string arg0,
            Phrase arg1,
            CreateFormulaParameterDelegate arg2,
            GetTranslationDelegate arg3,
            Phrase arg4,
            Phrase arg5,
            string arg6,
            int? arg7,
            RowActionInfo arg8,
            bool arg10
        ) : base()
        {
            field10 = arg10;
        }

        public KpiReportFieldHandler(
            string arg0,
            GetTranslationDelegate arg1,
            CreateFormulaParameterDelegate arg2,
            GetTranslationDelegate arg3,
            GetTranslationDelegate arg4,
            GetTranslationDelegate arg5,
            string arg6,
            int? arg7,
            RowActionInfo arg8,
            bool arg10
        ) : base()
        {
            field10 = arg10;
        }

        public readonly bool field10;
    }


    public class KpiReportFilterHandler : NumericRangeReportFilterHandler
    {
        public KpiReportFilterHandler(
            string arg0,
            string arg1,
            GetTranslationDelegate arg2,
            CreateFormulaParameterDelegate arg3,
            string arg4,
            string arg5,
            string arg6,
            NumericRange arg7
        ) : base()
        {
        }

        public KpiReportFilterHandler(
            string arg0,
            GetTranslationDelegate arg1,
            GetTranslationDelegate arg2,
            CreateFormulaParameterDelegate arg3,
            GetTranslationDelegate arg4,
            string arg5,
            string arg6,
            NumericRange arg7
        ) : base()
        {
        }

        protected KpiReportFilterHandler()
        {
        }
    }

    public class NumericRangeReportFilterHandler
    {
    }

    public class NumericReportField : ReportField
    {
        public NumericReportField()
        {
        }

        public NumericReportField(
            string arg0,
            string arg1,
            int? arg2,
            bool arg4
        )
        {
        }
    }


    public class NumericReportFieldHandler : ReportFieldHandler
        , INumericReportFieldHandler, ICrossTabDataReportFieldHandler
    {
        public NumericReportFieldHandler() : base()
        {
        }

        public NumericReportFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            string arg3,
            Phrase arg4,
            string arg5,
            IEnumerable<String> arg6,
            Phrase arg7,
            Phrase arg8,
            string arg9,
            int? arg10,
            RowActionInfo arg11,
            TotalCalculationType arg12,
            string arg13,
            string arg14,
            bool arg15
        ) : base()
        {
            field13 = arg13;
            field15 = arg15;
        }

        public NumericReportFieldHandler(
            string arg0,
            GetTranslationDelegate arg1,
            string arg2,
            string arg3,
            GetTranslationDelegate arg4,
            IEnumerable<String> arg5,
            GetTranslationDelegate arg6,
            GetTranslationDelegate arg7,
            string arg8,
            int? arg9,
            RowActionInfo arg10,
            TotalCalculationType arg11,
            string arg12,
            string arg13,
            bool arg14
        ) : base()
        {
            field9 = arg9;
            field10 = arg10;
            field11 = arg11;
            field12 = arg12;
            field13 = arg13;
            field14 = arg14;
        }

        public readonly int? field9;
        public readonly RowActionInfo field10;
        public readonly TotalCalculationType field11;
        public readonly string field12;
        public readonly string field13;
        public readonly bool field14;
        public readonly bool field15;
    }


    public class PercentageReportField : NumericReportField
    {
        public PercentageReportField(
            string arg0,
            string arg1,
            int? arg2,
            bool arg4
        ) : base(arg0, arg1, arg2, arg4)
        {
        }
    }


    public class RatingReportFieldHandler : NumericReportFieldHandler
    {
        public RatingReportFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            IEnumerable<String> arg3,
            Phrase arg4,
            Phrase arg5,
            bool arg6
        ) : base()
        {
        }
    }


    public class RatioReportField : ReportField
    {
        public RatioReportField(
            string arg0,
            string arg1,
            int? arg2,
            bool arg4,
            bool arg5
        )
        {
            field5 = arg5;
        }

        public readonly bool field5;
    }


    public class ReportField
        : IReportField
    {
        public ReportField()
        {
        }

        public ReportField(
            string arg0,
            string arg1,
            int? arg2,
            bool arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field4 = arg4;
        }

        public readonly string field0;
        public readonly string field1;
        public readonly int? field2;
        public readonly bool field4;
    }


    public class ReportFieldHandler
        : IReportFieldHandler
    {
        public ReportFieldHandler()
        {
        }

        public ReportFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            Phrase arg3,
            Phrase arg4,
            int? arg5,
            RowActionInfo arg6,
            bool arg7
        )
        {
            field0 = arg0;
            field5 = arg5;
            field6 = arg6;
        }

        public ReportFieldHandler(
            string arg0,
            Phrase arg1,
            ICollection<String> arg2,
            Phrase arg3,
            Phrase arg4,
            int? arg5,
            RowActionInfo arg6,
            string arg7,
            bool arg8
        )
        {
            field0 = arg0;
            field2 = arg2;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
        }

        public ReportFieldHandler(
            string arg0,
            GetTranslationDelegate arg1,
            string arg2,
            GetTranslationDelegate arg3,
            GetTranslationDelegate arg4,
            int? arg5,
            RowActionInfo arg6,
            string arg7,
            bool arg8
        )
        {
            field0 = arg0;
            field1 = arg1;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
        }

        public ReportFieldHandler(
            string arg0,
            GetTranslationDelegate arg1,
            ICollection<String> arg2,
            GetTranslationDelegate arg3,
            GetTranslationDelegate arg4,
            int? arg5,
            RowActionInfo arg6,
            string arg7,
            bool arg8
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
        }

        public readonly string field0;
        public readonly GetTranslationDelegate field1;
        public readonly ICollection<String> field2;
        public readonly GetTranslationDelegate field3;
        public readonly GetTranslationDelegate field4;
        public readonly int? field5;
        public readonly RowActionInfo field6;
        public readonly string field7;
        public readonly bool field8;
    }


    public class ReportFilter
        : IReportFilter
    {
        public ReportFilter()
        {
        }

        public ReportFilter(
            string arg0,
            bool? arg1
        )
        {
            field0 = arg0;
        }

        public readonly string field0;

        protected ReportFilter(string arg0)
        {
        }
    }


    public class ReportFilterHandler
        : IReportFilterHandler
    {
        public ReportFilterHandler(
            string arg0,
            GetTranslationDelegate arg1,
            GetTranslationDelegate arg2,
            GetTranslationDelegate arg6,
            string arg7,
            bool arg8,
            string arg9
        )
        {
            field0 = arg0;
            field7 = arg7;
            field8 = arg8;
            field9 = arg9;
        }

        public ReportFilterHandler(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            string arg3,
            Phrase arg6,
            string arg7,
            bool arg8,
            string arg9
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
            field9 = arg9;
        }

        public readonly string field0;
        public readonly Phrase field1;
        public readonly Phrase field2;
        public readonly string field3;
        public readonly Phrase field6;
        public readonly string field7;
        public readonly bool field8;
        public readonly string field9;
    }


    public abstract class ReportHandler<TContext, TReport> : IReportHandler<TContext>
        where TContext : IContext
        where TReport : IReport
    {
        protected IContextService<TContext> _ContextService;

        protected ReportHandler(IContextService<TContext> contextService)
        {
            _ContextService = contextService;
        }
    }


    public class SalesStatusReportFilter : ReportFilter
    {
        public SalesStatusReportFilter(
            string arg0,
            IEnumerable<Int32> arg1,
            TimePeriod arg2,
            bool arg3
        ) : base(arg0)
        {
            field2 = arg2;
            field3 = arg3;
        }

        public SalesStatusReportFilter(string arg0, IEnumerable<int> arg1, TimePeriod arg2)
        {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
        }

        public readonly TimePeriod field2;
        public readonly bool field3;
        private string arg0;
        private IEnumerable<int> arg1;
        private TimePeriod arg2;
    }


    public class SalesStatusReportFilterHandler : ReportFilterHandler<SalesStatusReportFilter>
    {
        public SalesStatusReportFilterHandler()
        {
        }

        public SalesStatusReportFilterHandler(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            BindCriteriaDelegate<SalesStatusReportFilter> arg3,
            BindCriteriaDelegate<SalesStatusReportFilter> arg4,
            string arg5,
            string arg6,
            string arg7,
            bool arg8
        ) : base()
        {
        }
    }


    public class SimpleListReportFilter : ReportFilter
    {
        public SimpleListReportFilter(
            string arg0,
            IEnumerable<Object> arg1,
            bool arg2
        ) : base()
        {
            field2 = arg2;
        }

        public readonly bool field2;

        protected SimpleListReportFilter()
        {
        }
    }


    public class SimpleListReportFilterHandler : ReportFilterHandler<SimpleListReportFilter>
    {
        public SimpleListReportFilterHandler()
        {
        }

        public SimpleListReportFilterHandler(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            BindCriteriaDelegate<SimpleListReportFilter> arg3,
            BindCriteriaDelegate<SimpleListReportFilter> arg4,
            Phrase arg5,
            string arg6,
            string arg7,
            ICollection<Object> arg8,
            bool arg9,
            string arg10
        ) : base()
        {
            field10 = arg10;
        }

        public readonly string field10;
    }


    public class StringReportField : ReportField
    {
        public StringReportField(
            string arg0,
            string arg1,
            int? arg2,
            bool arg4
        ) : base()
        {
        }
    }

    public interface ITermsOfServiceApprovalRepository : IEntityRepository<TermsOfServiceApproval>
    {
    }

    public partial class TermsOfServiceApprovalRepository : OrganizationEntityRepository<TermsOfServiceApprovalFields, TermsOfServiceApproval, IPsaContext>, ITermsOfServiceApprovalRepository
    {
        public TermsOfServiceApprovalRepository(IContextService<IPsaContext> contextService, IConfiguration configuration, DbProviderFactory dbProviderFactory) : base(contextService, configuration, dbProviderFactory)
        {
        }
    }

    public class TermsOfServiceApprovalFields : IOrganizationEntity
    {
    }

    public class StringReportFieldHandler : ReportFieldHandler<StringReportField>
    {
        public StringReportFieldHandler() { }

        public StringReportFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            ICollection<String> arg3,
            Phrase arg4,
            Phrase arg5,
            GetRowDataDelegate arg6,
            int? arg7,
            RowActionInfo arg8,
            string arg9,
            bool arg10,
            string arg11
        ) : base()
        {
            field11 = arg11;
        }

        public StringReportFieldHandler(
            string arg0,
            GetTranslationDelegate arg1,
            string arg2,
            GetTranslationDelegate arg3,
            GetTranslationDelegate arg4,
            GetRowDataDelegate arg5,
            int? arg6,
            RowActionInfo arg7,
            string arg8,
            bool arg9,
            string arg10
        ) : base()
        {
            field9 = arg9;
            field10 = arg10;
        }

        public readonly bool field9;
        public readonly string field10;
        public readonly string field11;
    }


    public class StringReportFilter : ReportFilter
    {
        public StringReportFilter(
            string arg0,
            string arg1
        ) : base(arg0)
        {
        }
    }


    public class StringReportFilterHandler : ReportFilterHandler<StringReportFilter>
    {
        public StringReportFilterHandler(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            BindCriteriaDelegate<StringReportFilter> arg3,
            BindCriteriaDelegate<StringReportFilter> arg4,
            Phrase arg5,
            string arg6,
            string arg7,
            bool arg8,
            SearchCriteriaComparison arg9
        ) : base()
        {
        }
    }


    public class TimePeriodReportFilter : ReportFilter
    {
        public TimePeriodReportFilter(
            string arg0,
            TimePeriod arg1
        ) : base(arg0)
        {
        }

        protected TimePeriodReportFilter()
        {
        }
    }


    public class TimePeriodReportFilterHandler : ReportFilterHandler<TimePeriodReportFilter>
    {
        public TimePeriodReportFilterHandler(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            BindCriteriaDelegate<TimePeriodReportFilter> arg3,
            BindCriteriaDelegate<TimePeriodReportFilter> arg4,
            string arg5,
            Phrase arg6,
            string arg7,
            TimePeriod arg8,
            bool arg9
        ) : base()
        {
        }
    }


    public class TypeAndIDListReportFilter : ReportFilter
    {
        public TypeAndIDListReportFilter(
            string arg0,
            IEnumerable<TypeAndID> arg1,
            bool arg2
        ) : base(arg0)
        {
            field2 = arg2;
        }

        public TypeAndIDListReportFilter(
            string arg0,
            TypeAndID arg1,
            bool arg2
        ) : base(arg0)
        {
            field2 = arg2;
        }

        public readonly bool field2;
    }


    public class TypeAndIDListReportFilterHandler : ReportFilterHandler<TypeAndIDListReportFilter>
    {
        public TypeAndIDListReportFilterHandler(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            BindCriteriaDelegate<TypeAndIDListReportFilter> arg3,
            BindCriteriaDelegate<TypeAndIDListReportFilter> arg4,
            Phrase arg5,
            string arg6,
            string arg7,
            ICollection<TypeAndID> arg8,
            bool arg9,
            bool arg10
        ) : base()
        {
            field10 = arg10;
        }

        public TypeAndIDListReportFilterHandler(
            string arg0,
            GetTranslationDelegate arg1,
            GetTranslationDelegate arg2,
            string arg3,
            ICollection<TypeAndID> arg4,
            BindCriteriaDelegate<TypeAndIDListReportFilter> arg5,
            BindCriteriaDelegate<TypeAndIDListReportFilter> arg6,
            string arg7,
            bool arg8,
            bool arg9
        ) : base()
        {
        }

        public readonly bool field10;
    }


    public class WorkHoursReportFieldHandler : ReportFieldHandler<NumericReportField>
        , ICrossTabDataReportFieldHandler
    {
        public WorkHoursReportFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            IEnumerable<String> arg3,
            Phrase arg4,
            Phrase arg5,
            int? arg6,
            RowActionInfo arg7,
            TotalCalculationType arg8,
            string arg9,
            bool arg10
        ) : base()
        {
            field10 = arg10;
        }

        public WorkHoursReportFieldHandler(
            string arg0,
            GetTranslationDelegate arg1,
            string arg2,
            IEnumerable<String> arg3,
            GetTranslationDelegate arg4,
            GetTranslationDelegate arg5,
            int? arg6,
            RowActionInfo arg7,
            TotalCalculationType arg8,
            bool arg9
        ) : base()
        {
            field9 = arg9;
        }

        public readonly bool field9;
        public readonly bool field10;
    }


    public class CountryRepository : EntityRepository<CountryFields, Country>
        , ICountryRepository
    {
        public CountryRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public class CountryTaxRepository : EntityRepository<CountryTaxFields, CountryTax>
        , ICountryTaxRepository
    {
        public CountryTaxRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public class CustomerDatabaseRepository : EntityRepository<CustomerDatabaseFields, CustomerDatabase>
        , ICustomerDatabaseRepository
    {
        public CustomerDatabaseRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public class FeatureRepository : EntityRepository<FeatureFields, Feature>
        , IFeatureRepository
    {
        public FeatureRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public class FeatureToggleRepository : EntityRepository<FeatureToggleFields, FeatureToggle>
        , IFeatureToggleRepository
    {
        public FeatureToggleRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public class FormatingCultureRepository : EntityRepository<FormatingCultureFields, FormatingCulture>
        , IFormatingCultureRepository
    {
        public FormatingCultureRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public interface ICountryRegionRepository
        : IEntityRepository<CountryRegion>
    {
    }


    public class CountryRegionRepository : EntityRepository<CountryRegionFields, CountryRegion>
        , ICountryRegionRepository
    {
        public CountryRegionRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public interface ICurrencyBaseRepository
        : IEntityRepository<CurrencyBase>
    {
    }


    public class CurrencyBaseRepository : EntityRepository<CurrencyBaseFields, CurrencyBase>
        , ICurrencyBaseRepository
    {
        public CurrencyBaseRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public interface IFeatureRepository
        : IEntityRepository<Feature>
    {
    }


    public interface IFeatureToggleRepository
        : IEntityRepository<FeatureToggle>
    {
    }


    public interface IGlobalSettingsRepository
        : IEntityRepository<GlobalSettings>
    {
    }


    public class GlobalSettingsRepository : EntityRepository<GlobalSettingsFields, GlobalSettings>
        , IGlobalSettingsRepository
    {
        public GlobalSettingsRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public GlobalSettingsRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
        public readonly ICustomerDatabaseRepository field3;
    }


    public interface IKeywordRepository
        : IEntityRepository<Keyword>
    {
    }


    public class KeywordRepository : EntityRepository<KeywordFields, Keyword>
        , IKeywordRepository
    {
        public KeywordRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public interface IRightRepository
        : IEntityRepository<Right>
    {
    }


    public class RightRepository : EntityRepository<RightFields, Right>
        , IRightRepository
    {
        public RightRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public interface IHostingProviderSalesAccountRepository
        : IEntityRepository<HostingProviderSalesAccount>
    {
    }


    public class HostingProviderSalesAccountRepository :
        EntityRepository<HostingProviderSalesAccountFields, HostingProviderSalesAccount>
        , IHostingProviderSalesAccountRepository
    {
        public HostingProviderSalesAccountRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public class RegistrationRepository : EntityRepository<RegistrationFields, Registration>
        , IRegistrationRepository
    {
        public RegistrationRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public class MasterOrganizationRepository : EntityRepository<MasterOrganizationFields, MasterOrganization>
        , IMasterOrganizationRepository
    {
        public MasterOrganizationRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
        public readonly IOrganizationService field3;
        public readonly IOrganizationEventHandler field4;
    }


    public class LanguageRepository : EntityRepository<LanguageFields, Language>
        , ILanguageRepository
    {
        public LanguageRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public class TimeZoneRepository : EntityRepository<TimeZoneFields, TimeZone>
        , ITimeZoneRepository
    {
        public TimeZoneRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public class TranslationRepository : EntityRepository<TranslationFields, Translation>
        , ITranslationRepository
    {
        public TranslationRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public class HostingProviderRepository : EntityRepository<HostingProviderFields, HostingProvider>
        , IHostingProviderRepository
    {
        public HostingProviderRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public class OrganizationHostingProviderRepository :
        EntityRepository<OrganizationHostingProviderFields, OrganizationHostingProvider>
        , IOrganizationHostingProviderRepository
    {
        public OrganizationHostingProviderRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public class PartnerRepository : EntityRepository<PartnerFields, Partner>
        , IPartnerRepository
    {
        public PartnerRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public class PartnerEmailRepository : EntityRepository<PartnerEmailFields, PartnerEmail>
        , IPartnerEmailRepository
    {
        public PartnerEmailRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public class PartnerPricingModelSettingRepository :
        EntityRepository<PartnerPricingModelSettingFields, PartnerPricingModelSetting>
        , IPartnerPricingModelSettingRepository
    {
        public PartnerPricingModelSettingRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public interface ICountryRepository
        : IEntityRepository<Country>
    {
    }


    public interface ICountryTaxRepository
        : IEntityRepository<CountryTax>
    {
    }


    public interface ICustomerDatabaseRepository
        : IEntityRepository<Customer>
    {
    }

    public class Customer : IEntity
    {
    }


    public interface IFormatingCultureRepository
        : IEntityRepository<FormatingCulture>
    {
    }


    public interface IHostingProviderRepository
        : IEntityRepository<HostingProvider>
    {
    }


    public interface ILanguageRepository
        : IEntityRepository<Language>
    {
    }


    public interface IMasterOrganizationRepository
        : IEntityRepository<MasterOrganization>
    {
    }


    public interface IOrganizationHostingProviderRepository
        : IEntityRepository<OrganizationHostingProvider>
    {
    }


    public interface IPartnerEmailRepository
        : IEntityRepository<PartnerEmail>
    {
    }


    public interface IPartnerPricingModelSettingRepository
        : IEntityRepository<PartnerPricingModelSetting>
    {
    }


    public interface IPartnerRepository
        : IEntityRepository<Partner>
    {
    }


    public interface IRegistrationRepository
        : IEntityRepository<Registration>
    {
    }


    public class TimeZone : TimeZoneFields, IIdentifiableEntityWithOriginalState<TimeZoneFields>
    {
    }

    public interface ITimeZoneRepository
        : IEntityRepository<TimeZone>
    {
    }


    public interface ITranslationRepository
        : IEntityRepository<Translation>
    {
    }


    public class RightsOrganizationContext
    {
        public RightsOrganizationContext(
        )
        {
        }
    }


    public class RightsContextClass
    {
    }


    public class CountryRegionService : SharedEntityServiceWithCache<CountryRegion, ICountryRegionRepository>
        , ICountryRegionService
    {
        public CountryRegionService(
            IContextService<ISharedContext> arg0,
            ICountryRegionRepository arg1,
            IValidator<CountryRegion> arg2,
            ISharedAuthorization<CountryRegion> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<ISharedContext> field0;
        public readonly ICountryRegionRepository field1;
        public readonly IValidator<CountryRegion> field2;
        public readonly ISharedAuthorization<CountryRegion> field3;
    }


    public class CountryService : SharedEntityServiceWithCache<Country, ICountryRepository>
        , ICountryService
    {
        public CountryService(
            IContextService<ISharedContext> arg0,
            ICountryRepository arg1,
            IValidator<Country> arg2,
            ISharedAuthorization<Country> arg3
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<ISharedContext> field0;
        public readonly ICountryRepository field1;
        public readonly IValidator<Country> field2;
        public readonly ISharedAuthorization<Country> field3;
    }


    public class CurrencyBaseService : SharedEntityServiceWithCache<CurrencyBase, ICurrencyBaseRepository>
        , ICurrencyBaseService
    {
        public CurrencyBaseService(
            IContextService<ISharedContext> arg0,
            ICurrencyBaseRepository arg1,
            IValidator<CurrencyBase> arg2,
            ISharedAuthorization<CurrencyBase> arg3
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<ISharedContext> field0;
        public readonly ICurrencyBaseRepository field1;
        public readonly IValidator<CurrencyBase> field2;
        public readonly ISharedAuthorization<CurrencyBase> field3;
    }


    public class DistributorSettingsFactory
        : IDistributorSettingsFactory
    {
        public DistributorSettingsFactory(
        )
        {
        }
    }


    public class FormattingCultureService : SharedEntityServiceWithCache<FormatingCulture, IFormatingCultureRepository>
        , IFormattingCultureService
    {
        public FormattingCultureService(
            IContextService<ISharedContext> arg0,
            IFormatingCultureRepository arg1,
            IValidator<FormatingCulture> arg2,
            ISharedAuthorization<FormatingCulture> arg3
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<ISharedContext> field0;
        public readonly IFormatingCultureRepository field1;
        public readonly IValidator<FormatingCulture> field2;
        public readonly ISharedAuthorization<FormatingCulture> field3;
    }


    public class GlobalSettingsService : SharedEntityService<GlobalSettings, IGlobalSettingsRepository>
        , IGlobalSettingsService
    {
        public GlobalSettingsService(
            IContextService<ISharedContext> arg0,
            IGlobalSettingsRepository arg1,
            IValidator<GlobalSettings> arg2,
            ISharedAuthorization<GlobalSettings> arg3
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<ISharedContext> field0;
        public readonly IGlobalSettingsRepository field1;
        public readonly IValidator<GlobalSettings> field2;
        public readonly ISharedAuthorization<GlobalSettings> field3;
    }


    public class HostingProviderService : SharedEntityService<HostingProvider, IHostingProviderRepository>
        , IHostingProviderService
    {
        public HostingProviderService(
            IContextService<ISharedContext> arg0,
            IHostingProviderRepository arg1,
            IValidator<HostingProvider> arg2,
            ISharedAuthorization<HostingProvider> arg3
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<ISharedContext> field0;
        public readonly IHostingProviderRepository field1;
        public readonly IValidator<HostingProvider> field2;
        public readonly ISharedAuthorization<HostingProvider> field3;
    }


    public interface ICountryRegionService
        : IEntityService<CountryRegion>
    {
    }


    public interface ICountryService
        : IEntityService<Country>
    {
    }


    public interface ICurrencyBaseService
        : IEntityService<CurrencyBase>
    {
    }


    public interface IDistributorHelperService
    {
    }

    public class DistributorHelperService : IDistributorHelperService
    {
        private readonly IPartnerService _PartnerService;
        private readonly IDistributorSettingsFactory _DistributorSettingsFactory;

        public DistributorHelperService(IPartnerService partnerService, IDistributorSettingsFactory distributorSettingsFactory)
        {
            _PartnerService = partnerService;
            _DistributorSettingsFactory = distributorSettingsFactory;
        }
    }


    public interface IDistributorSettingsFactory
    {
    }


    public interface IFormattingCultureService
        : IEntityService<FormatingCulture>
    {
    }


    public interface IGlobalSettingsService
    {
    }


    public interface IHostingProviderService
        : IEntityService<HostingProvider>
    {
    }


    public interface ILanguageService
        : IEntityService<Language>
    {
    }

    public interface ISharedAuthorization<TEntity> : ISharedAuthorization<ISharedContext, TEntity> where TEntity : IEntity
    {
    }

    public interface ISharedAuthorization<TContext, TEntity> : IAuthorization<TContext, TEntity>
        where TContext : ISharedContext where TEntity : IEntity
    {
    }

    public abstract class
        SharedEntityService<TEntity, TRepository> : EntityService<ISharedContext, TEntity, TRepository>
        where TEntity : IIdentifiableEntity
        where TRepository : IEntityRepository<TEntity>
    {
        protected SharedEntityService(IContextService<ISharedContext> contextService, TRepository repository,
            IValidator<TEntity> validator,
            IAuthorization<ISharedContext, TEntity> authorization = null) : base(contextService, repository,
            validator, authorization ?? new SharedAuthorization<TEntity>(contextService))
        {
        }
    }

    public class SharedAuthorization<TEntity> : SharedAuthorization<ISharedContext, TEntity>, ISharedAuthorization<TEntity> where TEntity : IEntity
    {
        public SharedAuthorization(IContextService<ISharedContext> contextService) : base(contextService)
        {
        }
    }

    public class SharedAuthorization<TContext, TEntity> : ISharedAuthorization<TContext, TEntity> where TContext : ISharedContext where TEntity : IEntity
    {
        protected readonly IContextService<TContext> ContextService;

        public SharedAuthorization(IContextService<TContext> contextService)
        {
            ContextService = contextService;
        }

        public virtual bool CanCreate()
        {
            return false;
        }

        public virtual bool CanCreate(TEntity entity)
        {
            return false;
        }

        public virtual bool CanDelete(TEntity entity)
        {
            return false;
        }

        public virtual bool CanRead()
        {
            return true;
        }

        public virtual bool CanRead(TEntity entity)
        {
            return true;
        }

        public virtual bool CanUpdate(TEntity entity)
        {
            return false;
        }
    }

    public abstract class SharedEntityServiceWithCache<TEntity, TRepository> : SharedEntityService<TEntity, TRepository>
        where TEntity : IIdentifiableEntity
        where TRepository : IEntityRepository<TEntity>
    {
        // This is used as a base for cached reading of Country, CurrencyBase, FormattingCulture, Language, and TimeZone.

        private static IEnumerable<TEntity> _Entities;

        protected SharedEntityServiceWithCache(IContextService<ISharedContext> contextService, TRepository repository,
            IValidator<TEntity> validator,
            ISharedAuthorization<TEntity> authorization = null
        ) : base(contextService, repository, null, null)
        {
        }
    }


    public class LanguageService : SharedEntityServiceWithCache<Language, ILanguageRepository>
        , ILanguageService
    {
        public LanguageService(
            IContextService<ISharedContext> arg0,
            ILanguageRepository arg1,
            IValidator<Language> arg2,
            ISharedAuthorization<Language> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<ISharedContext> field0;
        public readonly ILanguageRepository field1;
        public readonly IValidator<Language> field2;
        public readonly ISharedAuthorization<Language> field3;
    }


    public class MailLogo
    {
    }


    public interface IMailContentBuilder
    {
    }


    public class MailContentBuilder
        : IMailContentBuilder
    {
        public MailContentBuilder(
        )
        {
        }
    }


    public interface IOrganizationVatChangeService
    {
    }


    public class PartnerService
        : IPartnerService
    {
        public PartnerService(
            IPartnerRepository arg0,
            IPartnerEmailRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IPartnerRepository field0;
        public readonly IPartnerEmailRepository field1;
    }


    public interface ITimeZoneService
        : IEntityService<TimeZone>
    {
    }


    public class TimeZoneService : SharedEntityServiceWithCache<TimeZone, ITimeZoneRepository>
        , ITimeZoneService
    {
        public TimeZoneService(
            IContextService<ISharedContext> arg0,
            ITimeZoneRepository arg1,
            IValidator<TimeZone> arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService<ISharedContext> field0;
        public readonly ITimeZoneRepository field1;
        public readonly IValidator<TimeZone> field2;
    }

    public class SharedEntityServiceWithCache
    {
    }


    public class Category
    {
        public Category(
            string arg0,
            DictionaryCategory arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly DictionaryCategory field1;
    }


    public partial class Language
        : ITranslator, IEntity, IIdentifiableEntityWithOriginalState<LanguageFields>
    {
        public Language(
            string arg0
        )
        {
        }

        public Language(
            int arg0
        )
        {
            field0 = arg0;
        }

        public readonly int field0;
    }


    public class ChartAxisHandlerBase
        : IChartAxisHandler
    {
        public ChartAxisHandlerBase()
        {
        }

        public ChartAxisHandlerBase(
            IContextService<TContext> arg0,
            string arg1,
            GetTranslationDelegate arg2,
            ICollection<ActionInfoWithParameters> arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field4 = arg4;
        }

        public ChartAxisHandlerBase(
            IContextService<TContext> arg0,
            string arg1,
            Phrase arg2,
            Phrase arg3,
            ICollection<ActionInfoWithParameters> arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<TContext> field0;
        public readonly string field1;
        public readonly Phrase field2;
        public readonly Phrase field3;
        public readonly ICollection<ActionInfoWithParameters> field4;
    }


    public class ActionInfo : ActionInfoWithParameters
    {
        public ActionInfo(
            string arg0,
            Phrase arg1,
            string arg2,
            ICollection<ActionParameter> arg3
        ) : base()
        {
        }
    }


    public class ChartSeriesHandlerBase
        : IChartSeriesHandler
    {
        public ChartSeriesHandlerBase(
            IContextService<TContext> arg0,
            string arg1,
            GetTranslationDelegate arg2,
            GetTranslationDelegate arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public ChartSeriesHandlerBase(
            IContextService<TContext> arg0,
            string arg1,
            Phrase arg2,
            Phrase arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<TContext> field0;
        public readonly string field1;
        public readonly Phrase field2;
        public readonly Phrase field3;
    }


    public class ChartTimeframeCategoryHandler : IChartCategoryAxisHandler
    {
        public ChartTimeframeCategoryHandler(
            IContextService<TContext> arg0,
            string arg1,
            GetTranslationDelegate arg2,
            GetTranslationDelegate arg3,
            ICollection<ActionInfoWithParameters> arg4,
            TimePeriod arg5
        ) : base()
        {
            field5 = arg5;
        }

        public ChartTimeframeCategoryHandler(
            IContextService<TContext> arg0,
            string arg1,
            Phrase arg2,
            Phrase arg3,
            ICollection<ActionInfoWithParameters> arg4,
            TimePeriod arg5
        ) : base()
        {
            field5 = arg5;
        }

        public readonly TimePeriod field5;
    }

    public class ProcessParameterDelegate
    {
        public ProcessParameterDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly object field0;
        public readonly IntPtr field1;
    }


    public class SubGroupRow
    {
        public SubGroupRow(
        )
        {
        }
    }


    public class TimePeriodWithName : TimePeriod
    {
        public TimePeriodWithName(
            string arg0,
            DateTime arg1,
            DateTime arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly DateTime field1;
        public readonly DateTime field2;
    }


    public class GetTimeFrameDelegate
    {
        public GetTimeFrameDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly object field0;
        public readonly IntPtr field1;
    }


    public class GetValueDelegate
    {
        public GetValueDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly object field0;
        public readonly IntPtr field1;
    }


    public class GuidAndNameEntity
    {
        public GuidAndNameEntity(
        )
        {
        }
    }


    public class IdAndNameEntity
    {
        public IdAndNameEntity(
        )
        {
        }
    }


    public class SettingSource
    {
    }


    public class EffectiveSetting
    {
        public EffectiveSetting(
        )
        {
        }
    }


    public class NorwegianDistributorSettings
        : IDistributorSettings
    {
        public NorwegianDistributorSettings(
        )
        {
        }
    }


    public class SwedishDistributorSettings
        : IDistributorSettings
    {
        public SwedishDistributorSettings(
        )
        {
        }
    }


    public class DutchDistributorSettings
        : IDistributorSettings
    {
        public DutchDistributorSettings(
        )
        {
        }
    }


    public class CheckActionRightsDelegate
    {
        public CheckActionRightsDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly object field0;
        public readonly IntPtr field1;
    }


    public class SeriesInfo
    {
        public SeriesInfo(
        )
        {
        }
    }
}