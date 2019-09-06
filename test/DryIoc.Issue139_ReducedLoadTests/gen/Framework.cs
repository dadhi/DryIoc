using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Conn.Adapter;
using Data;
using Entities;
using Logic;
using OrganizationBase;
using Organizations;
using Search;
using Shared;
using Users;
using Users.Repositories;
using Utilities;

namespace Framework
{

    public interface IEntityService<TEntity>
    {
    }

    public interface IAuthorization<TContext, TEntity> where TContext : IContext where TEntity : IEntity
    {
    }


    public class Conversions
    {
    }


    public class ProjectData
    {
    }


    public class Utils
    {
    }


    public class StandardModuleAttribute : Attribute
    {
        public StandardModuleAttribute(
        )
        {
        }
    }


    public class Embedded : Attribute
    {
        public Embedded(
        )
        {
        }
    }


    public class InternalXmlHelper
    {
    }


    public class ApiQuotaService
        : IQuotaService
    {
        public ApiQuotaService(
        )
        {
        }
    }


    public class ContextDataStorage
        : IContextDataStorage
    {
        public ContextDataStorage(
        )
        {
        }
    }


    public interface ICompany
    {
    }


    public class Organization : OrganizationFields
        , IIdentifiableEntityWithOriginalState<Organization>, ICompany, IMasterOrganization
    {
        public Organization(
        )
        {
        }

        public Organization(
            MasterOrganization arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly MasterOrganization field0;
    }


    public interface IContext
    {
    }


    public abstract class OrganizationContextBase<TUser, TCompany> : IOrganizationContextBase<TUser, TCompany>
        where TUser : IOrganizationUserBase
        where TCompany : ICompany
    {
        public OrganizationContextBase(TUser user, OrganizationAddons addons, ITranslator translator = null,
            TimeZoneInfo TimeZoneInfo = null
        )
        {
        }
    }


    public interface IContextDataStorage
    {
    }

    public interface IPsaContextService : IContextService<IPsaContext>, IContextService<ISharedContext>
    {
    }

    public class PsaContextService
        : IPsaContextService
    {
        public PsaContextService(
            IPsaContextStorage arg0
        )
        {
            field0 = arg0;
        }

        public readonly IPsaContextStorage field0;
        public IPsaContext Context { get; }
    }


    public interface IContextService
    {
    }


    public interface IContextService<T> : IContextService
    {
    }

    public class PsaDatabaseConnionResolver : IDatabaseConnionResolver<IPsaContext>
    {
        protected readonly ICustomerDatabaseRepository CustomerDatabaseRepository;

        public PsaDatabaseConnionResolver(ICustomerDatabaseRepository customerDatabaseRepository)
        {
            CustomerDatabaseRepository = customerDatabaseRepository;
        }

        public string GetDatabaseConnion(IPsaContext context, ProjectDatabase database)
        {
            return null;
        }
    }


    public interface IDatabaseConnionResolver<TContext>
    {
    }


    public class ProjectDatabase
    {
    }


    public interface IQuotaService
    {
    }


    public interface ITimeService
    {
    }

    public interface IEntityRepository<TEntity> where TEntity : IEntity
    {
    }

    public partial interface IExtendedDataReader : IDataReader
    {
        new void GetValue(string name);

        new DateTime GetDateTime(string name);

        new decimal GetDecimal(string name);

        new Int32 GetInt32(string name);

        new Int64 GetInt64(string name);

        bool? GetNullableBoolean(int ordinal);

        bool? GetNullableBoolean(string name);

        DateTime? GetNullableDateTime(int ordinal);

        DateTime? GetNullableDateTime(string name);

        DateTime? GetNullableDateTimeEx(string name);

        decimal? GetNullableDecimal(int ordinal);

        decimal? GetNullableDecimal(string name);

        decimal? GetNullableDecimalEx(string name, decimal? defaultValue = null);

        Int32? GetNullableInt32(int ordinal);

        Int32? GetNullableInt32(string name);

        Int32? GetNullableInt32Ex(string name, int? defaultValue);

        Int64? GetNullableInt64(int ordinal);

        Int64? GetNullableInt64(string name);

        new int GetOrdinal(string name, bool throwExceptionIfNotExists = true);

        new string GetString(string name);

        string GetStringEx(string name, string defaultValue = null);

        new bool IsDBNull(string name);

        new bool GetBoolean(string name);

        SqlDataReader Reader { get; }
    }

    public class TimeService
        : ITimeService
    {
        public TimeService(
        )
        {
        }
    }


    public interface IUser
    {
    }


    public class User : UserFields, IUniqueUser, IIdentifiableEntityWithOriginalState<User>, IOrganizationUser,
        INamedEntity, IUser, IIdentifiableEntityWithOriginalState<UserFields>, IOrganizationUserBase
    {
        public User(
        )
        {
        }
    }


    public class ApiCredentials
        : ICredentials
    {
        public ApiCredentials(
        )
        {
        }
    }


    public class Authentication
    {
        public Authentication(
        )
        {
        }
    }


    public interface IAuthenticationService
    {
    }


    public interface ICredentials
    {
    }


    public interface ITokenAuthenticationService
        : IAuthenticationService
    {
    }

    public class AuthenticationService : IAuthenticationService
    {
        protected readonly IConnAuthenticationService ConnAuthenticationService;
        protected readonly IUniqueUserRepository UniqueUserRepository;
        protected readonly IForAuthUserRepository AuthUserRepository;

        public AuthenticationService(IConnAuthenticationService ConnAuthenticationService, IUniqueUserRepository uniqueUserRepository, IForAuthUserRepository authUserRepository)
        {
            ConnAuthenticationService = ConnAuthenticationService;
            UniqueUserRepository = uniqueUserRepository;
            AuthUserRepository = authUserRepository;
        }
    }

    public interface IUserTokenService
    {
    }


    public class TokenCredentials
        : ICredentials
    {
        public TokenCredentials(
        )
        {
        }
    }


    public class UserPasswordCredentials
        : ICredentials
    {
        public UserPasswordCredentials(
        )
        {
        }
    }


    public class AllowAllAuthorization
        : IAuthorization
    {
        public AllowAllAuthorization(
        )
        {
        }
    }


    public class AuthorizationBase
    {
        public AuthorizationBase(
            IContextService<TContext> arg0
        )
        {
            field0 = arg0;
        }

        public readonly IContextService<TContext> field0;
    }

    public delegate bool CheckUserRightsDelegate<TContext>(TContext context) where TContext : IContext;


    public class CheckUserRightsDelegate
    {
        public CheckUserRightsDelegate(
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


    public class AuthorizedEntity<TContext, TEntity> where TContext : IContext
    {
        private readonly CheckUserRightsDelegate<TContext> _CheckUserRightsDelegate;
        public readonly TEntity Entity;

        public AuthorizedEntity(TEntity entity, CheckUserRightsDelegate<TContext> checkUserRights)
        {
            _CheckUserRightsDelegate = checkUserRights;
            Entity = entity;
        }
    }


    public class AuthorizedEntityFactory<TContext, TEntity> : IAuthorizedEntityFactory<TContext, TEntity>
        where TContext : IContext
    {
            protected readonly Dictionary<string, AuthorizedEntity<TContext, TEntity>> _Entities = new Dictionary<string, AuthorizedEntity<TContext, TEntity>>();
    }


    public class AuthorizedIdentifiableEntityFactory<TContext, TEntity> : AuthorizedEntityFactory<TContext, TEntity>
        where TContext : IContext
        where TEntity : IIdentifiable
    {
    }


    public interface IAuthorization
    {
    }


    public interface IAuthorizedEntityFactory<TContext, TEntity> where TContext : IContext
    {
    }


    public class BigInt
    {
        public BigInt(
            int arg0
        )
        {
            field0 = arg0;
        }

        public BigInt(
            int arg0,
            int arg1
        )
        {
            field0 = arg0;
        }

        public BigInt(
            int arg0,
            Int64 arg1
        )
        {
            field0 = arg0;
        }

        public BigInt(
            int arg0,
            double arg1
        )
        {
            field0 = arg0;
        }

        public BigInt(
            int arg0,
            UInt16 arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public BigInt(
            int arg0,
            UInt32 arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public BigInt(
            int arg0,
            UInt64 arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly int field0;
        public readonly UInt64 field1;
    }


    public class EncodingMode
    {
    }


    public class Coder
    {
    }


    public class Configuration
        : IConfiguration
    {
        public Configuration(
        )
        {
        }
    }


    public class DateConversion
    {
    }


    public class DateConversionExtensions
    {
    }


    public class HandleEntityDelegate
    {
        public HandleEntityDelegate(
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


    public class DependencyResolver
    {
    }


    public class DictionaryItem
    {
        public DictionaryItem(
        )
        {
        }

        public DictionaryItem(
            string arg0
        )
        {
        }

        public DictionaryItem(
            string arg0,
            string arg1,
            string arg2
        )
        {
            field1 = arg1;
            field2 = arg2;
        }

        public DictionaryItem(
            int? arg0,
            string arg1,
            string arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly int? field0;
        public readonly string field1;
        public readonly string field2;
    }


    public class DictionaryItemList : List<DictionaryItem>
    {
        public DictionaryItemList(
        )
        {
        }
    }


    public class EntityAccessInfo
        : IEntityAccessInfo
    {
        public EntityAccessInfo(
        )
        {
        }

        public EntityAccessInfo(
            int? arg0,
            string arg1,
            string arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly int? field0;
        public readonly string field1;
        public readonly string field2;
    }


    public class ExcludedLimit
    {
    }


    public class GlobalGuidService
        : IGlobalGuidService
    {
        public GlobalGuidService(
        )
        {
        }
    }


    public interface IConfiguration
    {
    }


    public interface IEntityAccessInfo
    {
    }


    public interface IEntityAccessInfo<TContext> : IEntityAccessInfo
    {
    }


    public interface IGlobalGuidService
    {
    }


    public interface IGuidConverter
    {
    }


    public interface IIdentifiable
    {
    }


    public class ActionInfo
        : IIdentifiable
    {
        public ActionInfo()
        {
        }

        public ActionInfo(
            string arg0,
            string arg1,
            string arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly string field0;
        public readonly string field1;
        public readonly string field2;
    }


    public interface IMailClient
    {
    }


    public class MailClient
        : IMailClient
    {
        public MailClient(
        )
        {
        }
    }


    public interface IXmlStorable
    {
    }


    public class TimePeriod
        : IXmlStorable
    {
        public TimePeriod(
        )
        {
        }

        public TimePeriod(
            string arg0
        )
        {
        }

        public TimePeriod(
            TimePeriod arg0
        )
        {
            field0 = arg0;
        }

        public readonly TimePeriod field0;
    }


    public class MailOptions
    {
        public MailOptions(
        )
        {
        }
    }


    public class NumericRange
    {
        public NumericRange(
        )
        {
        }

        public NumericRange(
            decimal? arg0,
            decimal? arg1,
            ExcludedLimit arg2,
            bool arg3
        )
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public NumericRange(
            NumericRange arg0
        )
        {
            field0 = arg0;
        }

        public readonly NumericRange field0;
        public readonly decimal? field1;
        public readonly ExcludedLimit field2;
        public readonly bool field3;
    }


    public class ObjectDescriptionPair : IObjectDescription
    {
        public ObjectDescriptionPair(
            object arg0,
            object arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly object field0;
        public readonly object field1;
    }


    public class OperationInfoHolder
    {
        public OperationInfoHolder(
        )
        {
        }
    }


    public class RatioValue
    {
        public RatioValue(
            decimal? arg0,
            decimal? arg1,
            decimal? arg2
        ) : base()
        {
            field1 = arg1;
            field2 = arg2;
        }

        public RatioValue(
            decimal? arg0,
            decimal? arg1
        ) : base()
        {
            field1 = arg1;
        }

        public RatioValue(
        ) : base()
        {
        }

        public readonly decimal? field1;
        public readonly decimal? field2;
    }


    public class SerializableSqlHierarchyId
    {
        public SerializableSqlHierarchyId(
        )
        {
        }
    }


    public class SimpleObjectDescription
        : IObjectDescription
    {
        public SimpleObjectDescription(
            object arg0
        )
        {
            field0 = arg0;
        }

        public readonly object field0;
    }


    public class TypeAndID
    {
        public TypeAndID(
        )
        {
        }

        public TypeAndID(
            string arg0,
            int? arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly int? field1;
    }


    public enum AggregateFunction
    {
        None,
        Sum,
        Avg,
        Max,
        Min,
        Count,
        CountDistinct
    }


    public class FormulaPartDescriptionLevel
    {
    }


    public interface ICustomFormulaHandler
    {
    }

    public abstract class SqlFormulaHandler<TContext> : ISqlFormulaHandler<TContext> where TContext : IContext
    {
    }

    public class SqlFormulaHandler
        : ISqlFormulaHandler
    {
        public SqlFormulaHandler()
        {
        }

        public SqlFormulaHandler(
            string arg0
        )
        {
            field0 = arg0;
        }

        public readonly string field0;
    }


    public interface ICustomFormulaNumericPartParameter
        : ICustomFormulaPartParameter
    {
    }


    public class CaseCount
        : ICustomFormulaNumericPartParameter
    {
        public CaseCount(
        )
        {
        }

        public CaseCount(
            bool? arg0
        )
        {
            field0 = arg0;
        }

        public readonly bool? field0;
    }


    public interface ICustomFormulaPartParameter
    {
    }


    public class AccountGroups
        : ICustomFormulaPartParameter
    {
        public AccountGroups(
        )
        {
        }

        public AccountGroups(
            int arg0
        )
        {
            field0 = arg0;
        }

        public readonly int field0;
    }


    public interface IFormulaPartTypeInfo
    {
    }

    public delegate ICustomFormulaPart CreateCustomFormulaPartDelegate<TContext>() where TContext : IContext;

    public delegate ICustomFormulaPartParameter CreateCustomFormulaParameterDelegate(bool forExposing);

    public class FormulaPartTypeInfo
        : IFormulaPartTypeInfo
    {
        public FormulaPartTypeInfo(
            Type arg0,
            Type arg1,
            string arg2,
            Phrase arg3,
            bool arg4
        )
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public FormulaPartTypeInfo(
            Type arg0,
            CreateCustomFormulaParameterDelegate arg1,
            string arg2,
            Phrase arg3,
            bool arg4
        )
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public FormulaPartTypeInfo(
            CreateCustomFormulaPartDelegate<TContext> arg0,
            CreateCustomFormulaParameterDelegate arg1,
            string arg2,
            Phrase arg3,
            bool arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly CreateCustomFormulaPartDelegate<TContext> field0;
        public readonly CreateCustomFormulaParameterDelegate field1;
        public readonly string field2;
        public readonly Phrase field3;
        public readonly bool field4;
    }


    public interface IParameterWithTimePeriod
    {
    }


    public class ActivityCount
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public ActivityCount(
            TimePeriod arg0,
            bool? arg1,
            string arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly TimePeriod field0;
        public readonly bool? field1;
        public readonly string field2;
    }


    public class CreateFormulaParameterDelegate
    {
        public CreateFormulaParameterDelegate(
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


    public class KpiFieldInfo
    {
        public KpiFieldInfo(
            string arg0,
            string arg1,
            string arg2,
            CreateFormulaParameterDelegate arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly string field0;
        public readonly string field1;
        public readonly string field2;
        public readonly CreateFormulaParameterDelegate field3;
    }


    public class CompositeTranslationEntry
        : ITranslationEntry
    {
        public CompositeTranslationEntry(
            string arg0,
            IDict arg1
        )
        {
        }

        public CompositeTranslationEntry(
            ITranslationEntry arg0,
            IDict arg1
        )
        {
            field0 = arg0;
        }

        public CompositeTranslationEntry(
            ITranslationEntry arg0,
            KeyValuePair<String, ITranslationEntry> arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly ITranslationEntry field0;
        public readonly KeyValuePair<String, ITranslationEntry> field1;
    }


    public class ErrorTranslationEntry : CompositeTranslationEntry
    {
        public ErrorTranslationEntry(
            string arg0,
            IDict arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public interface IDict
    {
    }


    public class Dict
        : ITranslator, IDict
    {
        public static Dict Current()
        {
            return new Dict();
        }
    }


    public interface ITranslationEntry
    {
    }


    public class GetStringDelegate
    {
        public GetStringDelegate(
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


    public class GetTranslationDelegate
    {
        public GetTranslationDelegate(
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


    public interface ITranslator
    {
    }


    public class TranslationEntry
        : ITranslationEntry
    {
        public TranslationEntry(
            string arg0
        )
        {
        }

        public TranslationEntry(
            GetTranslationDelegate arg0,
            string arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly GetTranslationDelegate field0;
        public readonly string field1;
    }


    public class TranslationEntryCollection
        : ITranslationEntry
    {
        public TranslationEntryCollection(
            IEnumerable<ITranslationEntry> arg0
        )
        {
            field0 = arg0;
        }

        public readonly IEnumerable<ITranslationEntry> field0;
    }


    public class UntranslatedEntry
        : ITranslationEntry
    {
        public UntranslatedEntry(
            string arg0,
            string arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly string field1;
    }


    public class IdentifiableEntity
        : IIdentifiableEntity
    {
    }


    public interface IEntity
    {
    }


    public interface IIdentifiableEntity
        : IEntity
    {
    }


    public interface IIdentifiableEntityWithOriginalState<T, E>
        : IIdentifiableEntityWithOriginalState<T>
    {
    }


    public interface IIdentifiableEntityWithOriginalState<T>
    {
    }


    public class BusinessOverview : BusinessOverviewFields
        , IIdentifiableEntityWithOriginalState<BusinessOverview>,
        IIdentifiableEntityWithOriginalState<BusinessOverviewFields>
    {
        public BusinessOverview(
        )
        {
        }
    }


    public interface INamedEntity
        : IIdentifiableEntity
    {
    }


    public class ProductWithCompanyAndUsageInfo : ProductWithCompanyInfo
    {
        public ProductWithCompanyAndUsageInfo(
        )
        {
        }
    }


    public interface IPropertyChange
    {
    }

    public interface IPropertyChange<T> : IPropertyChange
    {
    }


    public interface IQuotaEntity
    {
    }


    public class QuotaEntity
        : IQuotaEntity
    {
        public QuotaEntity(
        )
        {
        }
    }


    public class PropertyCache
    {
    }


    public class PropertyChange<T>
        : IPropertyChange<T>
    {
        public PropertyChange(
            string arg0,
            T arg1,
            T arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public PropertyChange(
            string arg0,
            T arg1,
            T arg2,
            Type arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly string field0;
        public readonly T field1;
        public readonly T field2;
        public readonly Type field3;
    }


    public class PropertyEnumerator
        : IEnumerator, IEnumerable
    {
        public PropertyEnumerator(
            Type arg0
        )
        {
            field0 = arg0;
        }

        public readonly Type field0;
        public bool MoveNext() => throw new NotImplementedException();

        public void Reset() => throw new NotImplementedException();

        public object Current { get; }
        public IEnumerator GetEnumerator() => throw new NotImplementedException();
    }


    public class DuplicateValueException : AppDatabaseException
    {
        public DuplicateValueException(
        )
        {
        }

        public DuplicateValueException(
            string arg0,
            Exception arg1
        ) : base(arg0, arg1)
        {
        }

        public DuplicateValueException(
            SerializationInfo arg0,
            StreamingContext arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public interface IAppException
        : ITranslationEntry
    {
    }


    public class NotAuthorizedException : AppException
    {
        public NotAuthorizedException(
            string arg0,
            Exception arg1
        ) : base(arg0, arg1)
        {
        }

        public NotAuthorizedException(
            string arg0,
            Exception arg1,
            AppExceptionFlags arg2
        ) : base(arg0, arg1, arg2)
        {
        }

        public NotAuthorizedException(
            SerializationInfo arg0,
            StreamingContext arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class AppDatabaseException : AppException
    {
        public AppDatabaseException()
        {
        }

        public AppDatabaseException(
            string arg0,
            Exception arg1
        ) : base(arg0, arg1)
        {
        }

        public AppDatabaseException(
            SerializationInfo arg0,
            StreamingContext arg1
        ) : base(arg0, arg1)
        {
        }
    }

    public abstract class EntityRepository<TEntityFields, TEntity, TContext> : EntityRepository<TEntityFields, TEntity>
        where TEntityFields : IIdentifiableEntity, new()
        where TEntity : TEntityFields, IIdentifiableEntityWithOriginalState<TEntityFields>, new()
        where TContext : IContext
    {
        protected EntityRepository(IContextService<TContext> contextService, IConfiguration configuration,
            DbProviderFactory dbProviderFactory) : base(contextService, configuration, dbProviderFactory)
        {
        }
    }

    public class AppException : Exception
        , IAppException
    {
        public AppException()
        {
        }

        public AppException(
            string arg0,
            Exception arg1
        ) : base(arg0, arg1)
        {
        }

        public AppException(
            string arg0,
            Exception arg1,
            AppExceptionFlags arg2
        ) : base(arg0, arg1)
        {
            field2 = arg2;
        }

        public AppException(
            SerializationInfo arg0,
            StreamingContext arg1
        ) : base(arg0, arg1)
        {
        }

        public readonly AppExceptionFlags field2;
    }


    public class AppExceptionFlags
    {
    }


    public class AppValidationException : AppException
    {
        public AppValidationException(
            string arg0,
            Exception arg1
        ) : base(arg0, arg1)
        {
        }

        public AppValidationException(
            string arg0,
            Exception arg1,
            AppExceptionFlags arg2
        ) : base(arg0, arg1, arg2)
        {
        }

        public readonly AppExceptionFlags field3;
    }


    public class IDbCommandExtensions
    {
    }


    public class ActionInfoWithParameters : ActionInfo
    {
        public ActionInfoWithParameters()
        {
        }

        public ActionInfoWithParameters(
            string arg0,
            string arg1,
            string arg2,
            ICollection<ActionParameter> arg3
        ) : base(arg0, arg1, arg2)
        {
            field3 = arg3;
        }

        public readonly ICollection<ActionParameter> field3;
    }


    public class ActionModel
    {
        public ActionModel(
        )
        {
        }
    }


    public class ActionParameter
    {
        public ActionParameter(
        )
        {
        }

        public ActionParameter(
            string arg0,
            object arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly object field1;
    }


    public class ActionParameters
    {
        public ActionParameters(
        )
        {
        }
    }


    public class ChartReport : ReportWithFilters
        , IChartReport
    {
    }


    public class ChartReportResponse
    {
        public ChartReportResponse(
        )
        {
        }

        public interface IAxis
        {
        }
    }


    public class ChartCategoryAxis
        : IAxis, ChartReportResponse.IAxis
    {
        public ChartCategoryAxis(
        )
        {
        }
    }


    public class ChartCategory
    {
        public ChartCategory(
        )
        {
        }
    }


    public class ChartLinearAxis
        : IAxis
    {
        public ChartLinearAxis(
        )
        {
        }
    }


    public class ChartSeriesData
    {
        public ChartSeriesData(
        )
        {
        }
    }


    public class ChartSeries
    {
        public ChartSeries(
        )
        {
        }
    }


    public class ChartSeriesXAxis
    {
        public ChartSeriesXAxis(
            string arg0,
            ICollection<XAxis> arg1,
            string arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly string field0;
        public readonly ICollection<XAxis> field1;
        public readonly string field2;
    }

    public delegate bool CheckRowActionRightsDelegate<TContext>(TContext context, SearchResponseRow row)
        where TContext : IContext;

    public class CurrencyUsage
    {
    }


    public class DrillDownActionModel : ActionModel
    {
        public DrillDownActionModel(
        )
        {
        }
    }

    public class RowActionInfo<TContext> : RowActionInfo where TContext : IContext
    {
        private readonly CheckRowActionRightsDelegate<TContext> _CheckActionRights;

        public RowActionInfo()
        {
        }

        public RowActionInfo(string identifier, string titlePhrase, string type,
            ICollection<string> requiredSearchFields, ICollection<Parameter> rowParameters,
            ICollection<KeyValuePair<string, object>> otherParameters,
            CheckRowActionRightsDelegate<TContext> checkActionRights) : base()
        {
            _CheckActionRights = checkActionRights;
        }
    }

    public interface IDrillDownReportAction<TContext, TReport>
        where TContext : IContext
        where TReport : IReport
    {
        /// <summary>
        ///     ''' Returns report to be used as drill down. It can be of any kind, but tipically it is an list report
        ///     ''' </summary>
        ///     ''' <param name="context"></param>
        ///     ''' <param name="parameters"></param>
        ///     ''' <returns></returns>
        ///     ''' <remarks></remarks>
        IReport GetDrillDownReport(TContext context, TReport report, ActionParameters parameters);
    }

    public abstract class DrillDownReportActionInfo<TContext, TReport> : RowActionInfo<TContext>,
        IDrillDownReportAction<TContext, TReport>
        where TContext : IContext
        where TReport : IReport
    {
        public DrillDownReportActionInfo(string identifier, string titlePhrase,
            ICollection<string> requiredSearchFields, ICollection<Parameter> parameters) :
            base()
        {
        }

        protected DrillDownReportActionInfo()
        {
        }

        /// <summary>
        ///     ''' Returns report to be used as drill down. It can be of any kind, but tipically it is an list report
        ///     ''' </summary>
        ///     ''' <param name="context"></param>
        ///     ''' <param name="parameters"></param>
        ///     ''' <returns></returns>
        ///     ''' <remarks></remarks>
        public virtual IReport GetDrillDownReport(TContext context, TReport report, ActionParameters parameters)
        {
            return null;
        }
    }


    public class DrillDownReportActionInfo : RowActionInfo
        , IDrillDownReportAction
    {
        public DrillDownReportActionInfo()
        {
        }

        public DrillDownReportActionInfo(
            string arg0,
            string arg1,
            ICollection<String> arg2,
            ICollection<Parameter> arg3,
            CheckRowActionRightsDelegate<TContext> arg4,
            string arg5
        ) : base(arg0)
        {
        }
    }


    public class ExportedColumnInfo
    {
        public ExportedColumnInfo(
            string arg0,
            GetRowDataDelegate arg1,
            SearchFieldDataType arg2,
            string arg3,
            string arg4,
            TotalCalculationType arg5,
            int? arg6,
            GetRowDataDelegate arg7,
            GetTranslationDelegate arg8,
            GetRowDataDelegate arg9
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
            field9 = arg9;
        }

        public readonly string field0;
        public readonly GetRowDataDelegate field1;
        public readonly SearchFieldDataType field2;
        public readonly string field3;
        public readonly string field4;
        public readonly TotalCalculationType field5;
        public readonly int? field6;
        public readonly GetRowDataDelegate field7;
        public readonly GetTranslationDelegate field8;
        public readonly GetRowDataDelegate field9;
    }


    public interface IBatchActionHandler
    {
    }


    public class PsaBatchActionHandlerBase
        : IBatchActionHandler
    {
        public PsaBatchActionHandlerBase(
            IContextService<IPsaContext> arg0
        )
        {
            field0 = arg0;
        }

        public readonly IContextService<IPsaContext> field0;
    }


    public interface IChartAxis
    {
    }


    public class ChartAxisBase
        : IChartAxis
    {
        public ChartAxisBase(
            string arg0,
            string arg1,
            ICollection<IReportFilter> arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly string field0;
        public readonly string field1;
        public readonly ICollection<IReportFilter> field2;
    }


    public interface IChartAxisHandler
        : IIdentifiable
    {
    }

    public abstract partial class ChartReportHandler<TContext, TReport>
        where TContext : IContext
        where TReport : IChartReport
    {
        public abstract class ChartAxisHandlerBase<TAxis, TResponseAxis> : IChartAxisHandler
            where TAxis : IChartAxis
            where TResponseAxis : ChartReportResponse.IAxis
        {
            private readonly string _Identifier;
            private readonly GetTranslationDelegate _GetLabelDelegate;
            private readonly GetTranslationDelegate _GetGroupDelegate;
            private AuthorizedIdentifiableEntityFactory<TContext, ActionInfoWithParameters> _AllActions;
            protected readonly IContextService<TContext> ContextService;

            public ChartAxisHandlerBase(IContextService<TContext> contextService, string identifier,
                GetTranslationDelegate getLabelDelegate, GetTranslationDelegate getGroupDelegate = null,
                ICollection<ActionInfoWithParameters> actions = null
            )
            {
                ContextService = contextService;
                _Identifier = identifier;
                _GetLabelDelegate = getLabelDelegate;
                _GetGroupDelegate = getGroupDelegate;
                if (actions != null && actions.Count > 0)
                {
                }
            }

            public ChartAxisHandlerBase(IContextService<TContext> contextService, string identifier, Phrase labelPhrase,
                Phrase groupPhrase, ICollection<ActionInfoWithParameters> actions = null
            )
            {
                ContextService = contextService;
                _Identifier = identifier;
            }
        }
    }


    public abstract partial class ChartReportHandler<TContext, TReport>
        where TContext : IContext
        where TReport : IChartReport
    {
        protected ChartReportHandler(IContextService<IPsaContext> contextService)
        {
        }

        public abstract class
            ChartEntityCategoryListHandler<TEntity> :
                ChartAxisHandlerBase<ChartEntityCategory<TEntity>, ChartCategoryAxis>,
                IChartCategoryAxisHandler where TEntity : IIdentifiableEntity
        {
            public ChartEntityCategoryListHandler(IContextService<TContext> contextService, string identifier,
                GetTranslationDelegate getLabelDelegate, GetTranslationDelegate getGroupDelegate = null
            ) : base(contextService, identifier, getLabelDelegate, getGroupDelegate: getGroupDelegate)
            {
            }

            public ChartEntityCategoryListHandler(IContextService<TContext> contextService, string identifier,
                Phrase labelPhrase, Phrase groupPhrase = Phrase.Empty
            ) : base(contextService, identifier, labelPhrase, groupPhrase: groupPhrase)
            {
            }
        }
    }

    public class ChartEntityCategory<TEntity> : IChartAxis where TEntity : IIdentifiableEntity
    {
    }

    public class
        ActivityTypeCategoryHandler : ChartEntityCategoryListHandler<IPsaContext, RepertoryChartReport, ActivityType>
    {
        public ActivityTypeCategoryHandler(
            IContextService<IPsaContext> arg0,
            string arg1,
            Phrase arg2,
            Phrase arg3
        ) : base()
        {
        }
    }

    public class ChartEntityCategoryListHandler<T1, T2, T3>
    {
    }

    public interface IChartCategoryAxisHandler
        : IChartAxisHandler
    {
    }


    public class ChartCategoryItem
    {
        public ChartCategoryItem(
        )
        {
        }
    }


    public interface IChartDataField
        : IXmlStorable
    {
    }


    public class ChartDataField
        : IChartDataField
    {
        public ChartDataField(
            string arg0,
            string arg1,
            string arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly string field0;
        public readonly string field1;
        public readonly string field2;
    }


    public interface IChartDataFieldHandler
        : IIdentifiable
    {
    }


    public class PsaChartDataFieldHandler : Logic.ChartDataFieldHandler
    {
        public PsaChartDataFieldHandler(
            string arg0,
            GetTranslationDelegate arg1,
            string arg3,
            GetTranslationDelegate arg4,
            string arg5,
            CurrencyUsage arg6,
            ICurrencyService arg7,
            AggregateFunction arg8,
            bool arg9
        ) : base()
        {
            field8 = arg8;
            field9 = arg9;
        }

        public PsaChartDataFieldHandler(
            string arg0,
            Phrase arg1,
            string arg3,
            Phrase arg4,
            string arg5,
            CurrencyUsage arg6,
            ICurrencyService arg7,
            AggregateFunction arg8,
            bool arg9
        ) : base()
        {
            field8 = arg8;
            field9 = arg9;
        }

        public readonly AggregateFunction field8;
        public readonly bool field9;
    }


    public interface IChartReport
        : IReportWithFilters
    {
    }


    public abstract class PsaChartReportHandler<TReport> : ChartReportHandler<IPsaContext, TReport>
        where TReport : IChartReport
    {
        public readonly ICurrencyService CurrencyService;
        protected readonly IGuidService GuidService;

        public PsaChartReportHandler(IContextService<IPsaContext> contextService, ICurrencyService currencyService,
            IGuidService guidService) : base(contextService)
        {
            CurrencyService = currencyService;
            GuidService = guidService;
        }
    }


    public interface IChartReportHandler
    {
        ICollection<IChartSeriesHandler> GetAvailableSeries();
        IChartSeriesHandler GetAvailableSeries(string identifier);
        ICollection<IChartAxisHandler> GetAvailableXAxes();
        IChartAxisHandler GetAvailableXAxis(string identifier);
        ICollection<IChartAxisHandler> GetAvailableYAxes();
        IChartAxisHandler GetAvailableYAxis(string identifier);
    }

    public interface IChartReportHandler<TContext, TReport> : IReportWithFiltersHandler<TContext>, IChartReportHandler
        where TContext : IContext
        where TReport : IChartReport
    {
        ChartReportResponse GetData(TReport report);

        TReport CreateReport(XElement xml);
    }


    public interface IChartSeries
    {
    }


    public class ChartSeriesBase
        : IChartSeries
    {
        public ChartSeriesBase(
            string arg0,
            string arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly string field1;
    }


    public interface IChartSeriesHandler
        : IIdentifiable
    {
    }


    public class ActivitySeries : ChartValueSeriesHandler<IPsaContext, RepertoryChartReport, ActivitySearchCriteria>
    {
        public ActivitySeries(
            IContextService<IPsaContext> arg0,
            RepertoryChartReportHandler arg1
        ) : base()
        {
        }
    }

    public class ChartValueSeriesHandler<T, T1, T2>
    {
    }


    public interface IChartStackingGroup
        : IIdentifiable
    {
    }


    public class ChartStackingGroup
        : IChartStackingGroup
    {
        public ChartStackingGroup(
            string arg0,
            GetTranslationDelegate arg1,
            string arg2,
            bool arg4
        )
        {
            field0 = arg0;
            field2 = arg2;
            field4 = arg4;
        }

        public ChartStackingGroup(
            string arg0,
            Phrase arg1,
            string arg2,
            bool arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field4 = arg4;
        }

        public readonly string field0;
        public readonly Phrase field1;
        public readonly string field2;
        public readonly bool field4;
    }


    public interface ICrossTabDataReportFieldHandler
        : IReportFieldHandler
    {
    }


    public class BaseCurrencyReportFieldHandler : CurrencyReportFieldHandler
        , ICurrencyReportFieldHandler
    {
        public BaseCurrencyReportFieldHandler(
            IContextService<IPsaContext> arg0,
            string arg1,
            Phrase arg2,
            string arg3,
            string arg4,
            Phrase arg5,
            Phrase arg6,
            Phrase arg7,
            int? arg8,
            RowActionInfo arg9,
            TotalCalculationType arg10
        ) : base()
        {
        }
    }

    public interface IReportWithFilters : IReport
    {
    }

    public interface ICrossTabReport
        : IReportWithFilters
    {
    }


    public interface ICrossTabReportHandler<TContext> : IReportWithFiltersHandler<TContext> where TContext : IContext
    {
    }


    public interface ICurrencyReportFieldHandler
        : INumericReportFieldHandler
    {
    }


    public interface IDrillDownReportAction
    {
    }


    public interface IGroupReportFieldHandler
        : IReportFieldHandler, IEqualityComparer
    {
    }


    public class UserIdGroupReportFieldHandler : IdGroupReportFieldHandler
    {
        public UserIdGroupReportFieldHandler(
        ) : base()
        {
        }
    }


    public interface IListReport
        : IReportWithFilters
    {
    }


    public class ListReport : ReportWithFilters
        , IListReport
    {
        public ListReport(
            string arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly string field0;
    }


    public interface IListReportHandler<TContext> : IReportWithFiltersHandler<TContext> where TContext : IContext
    {
    }


    public interface IMatrixReportDataFieldGroup
        : IReportDataFieldGroup
    {
    }


    public class EntityListDataFieldGroup
        : IMatrixReportDataFieldGroup
    {
        public EntityListDataFieldGroup(
            IGuidService arg0,
            ICrossTabReport arg1,
            string arg2,
            string arg3,
            string arg4,
            bool arg6,
            bool arg7,
            bool arg8,
            string arg9,
            string arg10
        )
        {
            field0 = arg0;
            field1 = arg1;
            field8 = arg8;
            field9 = arg9;
            field10 = arg10;
        }

        public EntityListDataFieldGroup(
            IGuidService arg0,
            ICrossTabReport arg1,
            XElement arg2,
            string arg3,
            ICollection<SearchRequestCriteriaField> arg4,
            string arg5,
            string arg6,
            bool arg8,
            string arg9,
            string arg10
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field8 = arg8;
            field9 = arg9;
            field10 = arg10;
        }

        public readonly IGuidService field0;
        public readonly ICrossTabReport field1;
        public readonly XElement field2;
        public readonly string field3;
        public readonly ICollection<SearchRequestCriteriaField> field4;
        public readonly string field5;
        public readonly string field6;
        public readonly bool field8;
        public readonly string field9;
        public readonly string field10;
    }


    public interface INumericRangeReportFilter
        : IReportFilter
    {
    }


    public class NumericRangeReportFilterOld
    {
        public NumericRangeReportFilterOld(
            string arg0,
            NumericRange arg1
        )
        {
        }
    }


    public interface INumericRangeReportFilterHandler
        : IReportFilterHandler
    {
    }


    public class BaseCurrencyRangeReportFilterHandler : NumericRangeReportFilterHandler
    {
        public BaseCurrencyRangeReportFilterHandler(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            GetTranslationDelegate arg3,
            string arg4,
            NumericRange arg5,
            bool arg6
        ) : base()
        {
        }

        public BaseCurrencyRangeReportFilterHandler(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            string arg3,
            BindCriteriaDelegate<NumericRangeReportFilter> arg4,
            BindCriteriaDelegate<NumericRangeReportFilter> arg5,
            string arg6,
            NumericRange arg7,
            bool arg8
        ) : base()
        {
        }

        public BaseCurrencyRangeReportFilterHandler(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            Phrase arg3,
            BindCriteriaDelegate<NumericRangeReportFilter> arg4,
            BindCriteriaDelegate<NumericRangeReportFilter> arg5,
            string arg6,
            NumericRange arg7,
            bool arg8
        ) : base()
        {
        }

        protected BaseCurrencyRangeReportFilterHandler()
        {
            throw new NotImplementedException();
        }
    }

    public class NumericRangeReportFilter
    {
    }


    public interface INumericReportFieldHandler
        : IReportFieldHandler
    {
    }


    public interface IReport
    {
    }


    public interface IReportDataFieldGroup
        : IIdentifiable, IXmlStorable
    {
    }


    public class TimelineReportDataFieldGroup
        : IReportDataFieldGroup
    {
        public TimelineReportDataFieldGroup(
            TimelineReportTimeFrame arg0,
            string arg1
        )
        {
            field1 = arg1;
        }

        public TimelineReportDataFieldGroup(
            XElement arg0,
            string arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly XElement field0;
        public readonly string field1;
    }


    public class DataFieldSubGroup
    {
        public DataFieldSubGroup(
        )
        {
        }
    }


    public interface IReportFactoryService<TContext> where TContext : IContext
    {
    }


    public interface IReportField
        : IXmlStorable
    {
    }


    public class AccountGroupsReportField : Logic.ReportField, IReportField
    {
        public AccountGroupsReportField(
            int? arg0,
            string arg1,
            int? arg2,
            AggregateFunction arg3,
            bool arg4
        ) : base()
        {
        }
    }


    public interface IReportFieldHandler
        : IIdentifiable
    {
    }


    public class AccountGroupsReportFieldHandler : ReportFieldHandler<AccountGroupsReportField>
    {
        public AccountGroupsReportFieldHandler(
            int arg0,
            string arg1,
            CreateAccountGroupsParameterDelegate arg2,
            GetTranslationDelegate arg3,
            int? arg4
        ) : base()
        {
        }
    }


    public interface IReportFilter
        : IXmlStorable
    {
    }


    public class IDListReportFilterOld : IDListReportFilter
    {
        public IDListReportFilterOld(
            string arg0,
            IEnumerable<Int32> arg1
        ) : base()
        {
        }
    }


    public class BindCriteriaDelegate
    {
        public BindCriteriaDelegate(
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


    public interface IReportFilterHandler
        : IIdentifiable
    {
    }


    public class AccountGroupIDReportFilterHandler : IDListReportFilterHandler
        , IAccountGroupIDReportFilterHandler
    {
        public AccountGroupIDReportFilterHandler(
            int arg0,
            string arg1,
            GetTranslationDelegate arg2
        ) : base()
        {
        }
    }


    public interface IReportHandler<TContext> : IIdentifiable where TContext : IContext
    {
    }


    public interface IReportWithFiltersHandler<TContext> : IReportHandler<TContext> where TContext : IContext
    {
    }


    public class LabelPurpose
    {
    }


    public interface IReportInfoBase<TContext> where TContext : IContext
    {
    }


    public abstract class ReportFactory<TContext, TReportInfo>
        where TContext : IContext
        where TReportInfo : IReportInfoBase<TContext>
    {
    }



    public class ReportTotalKpiLookoutParameters
    {
        public ReportTotalKpiLookoutParameters(
        )
        {
        }

        public ReportTotalKpiLookoutParameters(
            XElement arg0
        )
        {
            field0 = arg0;
        }

        public readonly XElement field0;
    }


    public class ReportWithFilters
        : IReportWithFilters
    {
        public ReportWithFilters(
        )
        {
        }
    }


    public class RowActionInfo : ActionInfo
    {
        protected RowActionInfo()
        {
        }

        public RowActionInfo(
            string arg0,
            string arg1,
            string arg2,
            ICollection<String> arg3,
            ICollection<Parameter> arg4,
            ICollection arg5
        ) : base(arg0, arg1, arg2)
        {
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly ICollection<String> field3;
        public readonly ICollection<Parameter> field4;
        public readonly ICollection field5;

        protected RowActionInfo(string arg0)
        {
        }
    }


    public class CheckRowActionRightsDelegate
    {
        public CheckRowActionRightsDelegate(
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


    public class TimelineReportTimeFrame
    {
        public TimelineReportTimeFrame(
            string arg0,
            DateTime? arg1,
            DateTime? arg2,
            int? arg3,
            string arg4,
            int? arg5,
            string arg6,
            bool arg7
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
        }

        public readonly string field0;
        public readonly DateTime? field1;
        public readonly DateTime? field2;
        public readonly int? field3;
        public readonly string field4;
        public readonly int? field5;
        public readonly string field6;
        public readonly bool field7;
    }


    public class TotalCalculationType
    {
    }


    public class ConnionManager
    {
    }


    public class DataReader
        : IExtendedDataReader
    {
        public DataReader(
            SqlDataReader arg0
        )
        {
            field0 = arg0;
        }

        public readonly SqlDataReader field0;

        protected DataReader()
        {
        }

        public void Dispose() => throw new NotImplementedException();

        public string GetName(int i) => throw new NotImplementedException();

        public string GetDataTypeName(int i) => throw new NotImplementedException();

        public Type GetFieldType(int i) => throw new NotImplementedException();

        public object GetValue(int i) => throw new NotImplementedException();

        public int GetValues(object[] values) => throw new NotImplementedException();

        public int GetOrdinal(string name) => throw new NotImplementedException();

        public bool GetBoolean(int i) => throw new NotImplementedException();

        public byte GetByte(int i) => throw new NotImplementedException();

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) =>
            throw new NotImplementedException();

        public char GetChar(int i) => throw new NotImplementedException();

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) =>
            throw new NotImplementedException();

        public Guid GetGuid(int i) => throw new NotImplementedException();

        public short GetInt16(int i) => throw new NotImplementedException();

        public int GetInt32(int i) => throw new NotImplementedException();

        public long GetInt64(int i) => throw new NotImplementedException();

        public float GetFloat(int i) => throw new NotImplementedException();

        public double GetDouble(int i) => throw new NotImplementedException();

        public string GetString(int i) => throw new NotImplementedException();

        public decimal GetDecimal(int i) => throw new NotImplementedException();

        public DateTime GetDateTime(int i) => throw new NotImplementedException();

        public IDataReader GetData(int i) => throw new NotImplementedException();

        public bool IsDBNull(int i) => throw new NotImplementedException();

        public int FieldCount { get; }

        public object this[int i] => throw new NotImplementedException();

        public object this[string name] => throw new NotImplementedException();

        public void Close() => throw new NotImplementedException();

        public DataTable GetSchemaTable() => throw new NotImplementedException();

        public bool NextResult() => throw new NotImplementedException();

        public bool Read() => throw new NotImplementedException();

        public void GetValue(string name)
        {
        }

        public DateTime GetDateTime(string name)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(string name)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(string name)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(string name)
        {
            throw new NotImplementedException();
        }

        public bool? GetNullableBoolean(int ordinal)
        {
            throw new NotImplementedException();
        }

        public bool? GetNullableBoolean(string name)
        {
            throw new NotImplementedException();
        }

        public DateTime? GetNullableDateTime(int ordinal)
        {
            throw new NotImplementedException();
        }

        public DateTime? GetNullableDateTime(string name)
        {
            throw new NotImplementedException();
        }

        public DateTime? GetNullableDateTimeEx(string name)
        {
            throw new NotImplementedException();
        }

        public decimal? GetNullableDecimal(int ordinal)
        {
            throw new NotImplementedException();
        }

        public decimal? GetNullableDecimal(string name)
        {
            throw new NotImplementedException();
        }

        public decimal? GetNullableDecimalEx(string name, decimal? defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public int? GetNullableInt32(int ordinal)
        {
            throw new NotImplementedException();
        }

        public int? GetNullableInt32(string name)
        {
            throw new NotImplementedException();
        }

        public int? GetNullableInt32Ex(string name, int? defaultValue)
        {
            throw new NotImplementedException();
        }

        public long? GetNullableInt64(int ordinal)
        {
            throw new NotImplementedException();
        }

        public long? GetNullableInt64(string name)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name, bool throwExceptionIfNotExists = true)
        {
            throw new NotImplementedException();
        }

        public string GetString(string name)
        {
            throw new NotImplementedException();
        }

        public string GetStringEx(string name, string defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(string name)
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean(string name)
        {
            throw new NotImplementedException();
        }

        public int Depth { get; }
        public bool IsClosed { get; }
        public int RecordsAffected { get; }

        public SqlDataReader Reader => throw new NotImplementedException();
    }


    public class DBHelper
    {
        public DBHelper(
        )
        {
        }
    }


    public interface IEntityRepository
    {
    }


    public partial interface IExtendedDataReader
        : IDataReader
    {
    }


    public interface IWritableRepository<TEntity>
    {
    }


    public class QueryHints
    {
    }


    public abstract class RepositoryBase
    {
        public enum Operand
        {
            // Equals
            LessThanOrEquals,
            GreaterThanOrEquals
        }


        protected readonly IContextService ContextService;
        protected readonly IConfiguration Configuration;
        protected readonly DbProviderFactory DbProviderFactory;

        protected RepositoryBase()
        {
        }

        protected RepositoryBase(IContextService contextService, IConfiguration configuration,
            DbProviderFactory dbProviderFactory)
        {
            ContextService = contextService;
            Configuration = configuration;
            DbProviderFactory = dbProviderFactory;
        }
    }

    public abstract class RepositoryBase<TContext> : RepositoryBase where TContext : IContext
    {
        protected RepositoryBase(IContextService<TContext> contextService, IConfiguration configuration,
            DbProviderFactory dbProviderFactory) : base(contextService, configuration, dbProviderFactory)
        {
        }
    }


    public abstract class WritableRepository<TEntityFields, TEntity> : RepositoryBase
        where TEntityFields : IIdentifiableEntity, new()
        where TEntity : TEntityFields, IIdentifiableEntityWithOriginalState<TEntityFields>, new()
    {
        protected WritableRepository(IContextService contextService, IConfiguration configuration,
            DbProviderFactory dbProviderFactory) : base(contextService, configuration, dbProviderFactory)
        {
        }
    }

    public class DateConversionMode
    {
    }


    public class DateConversionAttribute : Attribute
    {
        public DateConversionAttribute(
            DateConversionMode arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly DateConversionMode field0;
    }


    public class FieldNameAttribute : PropertyAttributeBase
    {
        public FieldNameAttribute(
            string arg0,
            bool arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly bool field1;
    }


    public class IdentifierAttribute : FieldNameAttribute
    {
        public IdentifierAttribute(
            string arg0,
            bool arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class IgnoreModelPropertyAttribute : PropertyAttributeBase
    {
        public IgnoreModelPropertyAttribute(
        )
        {
        }
    }


    public class IpAddressAttribute : PropertyAttributeBase
    {
        public IpAddressAttribute(
        )
        {
        }
    }

    public interface IContext<TUser, TCompany> : IContext
        where TUser : IUser
        where TCompany : ICompany
    {
    }

    public interface IPropertyAttribute
    {
    }


    public class NullConversionAttribute : PropertyAttributeBase
    {
        public NullConversionAttribute(
            object arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly object field0;
    }


    public class PropertyAttributeBase : Attribute
        , IPropertyAttribute
    {
    }


    public class PropertyFieldNameAttribute : PropertyAttributeBase
    {
        public PropertyFieldNameAttribute(
            string arg0,
            string arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly string field1;
    }


    public class EntityIDCriteriaEntry
        : ICriteriaEntry
    {
        public EntityIDCriteriaEntry(
            string arg0,
            string arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly string field1;
    }

    public interface IEntityReader<TContext, TEntity> where TContext : IContext
    {
        void Read(TContext context, TEntity entity, INamedPropertyContainer valueContainer);
    }

    public interface IEntityReaderEx<TContext, TEntity> : IEntityReader<TContext, TEntity> where TContext : IContext
    {
    }

    public class EntityReader<TContext> where TContext : IContext
    {
    }

    public class EntityReader<TContext, TEntity> : EntityReader<TContext>, IEntityReaderEx<TContext, TEntity>
        where TContext : IContext
    {
        public EntityReader()
        {
        }

        public EntityReader(ReadDelegate readDelegate)
        {
            _ReadDelegate = readDelegate;
        }

        private readonly ReadDelegate _ReadDelegate;

        public void Read(TContext context, TEntity entity, INamedPropertyContainer valueContainer)
        {
        }
    }


    public class FieldSortInfo
    {
        public FieldSortInfo(
        )
        {
        }

        public FieldSortInfo(
            string arg0,
            SortDirection arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly SortDirection field1;
    }


    public interface ICriteriaEntry
    {
    }


    public interface IEntityReader
    {
    }


    public interface IEntityReaderEx
        : IEntityReader
    {
    }


    public interface INamedPropertyContainer
    {
    }


    public class SearchResponseRow : Dictionary<String, Object>
        , INamedPropertyContainer
    {
        public SearchResponseRow(
        )
        {
        }
    }


    public interface ISearchCriteria
    {
    }


    public class SearchCriteriaBase
        : ISearchCriteria
    {
    }


    public interface ISearchDefinition
        : IIdentifiable
    {
    }

    public interface ISearchFieldDefinition
    {
    }


    public class EntityAccessInfoListFieldDefinition : IInnerSearchFieldDefinition
    {
        public EntityAccessInfoListFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            SearchFieldDataType arg6
        ) : base()
        {
        }
    }

    public interface ISearchRequest
    {
        ICollection<SearchRequestField> Fields { get; }
        ICollection<SearchRequestSortField> SortFields { get; }

        int? FirstRowIndex { get; }
        int? NumberOfRows { get; }
        SearchRequestOption Options { get; }
        IDict Parameters { get; }

        string Collate { get; }
    }

    public interface ISearchRequest<TSearchCriteria> : ISearchRequest where TSearchCriteria : ISearchCriteria
    {
        TSearchCriteria Criteria { get; }
    }


    public class SearchRequest
        : ISearchRequest
    {
        public SearchRequest(
            ICollection<SearchRequestField> arg1,
            ICollection<SearchRequestSortField> arg2,
            int? arg3,
            int? arg4,
            SearchRequestOption arg5,
            IDict arg6,
            string arg7
        )
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
        }

        public readonly ICollection<SearchRequestField> field1;
        public readonly ICollection<SearchRequestSortField> field2;
        public readonly int? field3;
        public readonly int? field4;
        public readonly SearchRequestOption field5;
        public readonly IDict field6;
        public readonly string field7;
        public ICollection<SearchRequestField> Fields { get; }
        public ICollection<SearchRequestSortField> SortFields { get; }
        public int? FirstRowIndex { get; }
        public int? NumberOfRows { get; }
        public SearchRequestOption Options { get; }
        public IDict Parameters { get; }
        public string Collate { get; }
    }


    public interface ISearchResponse
    {
    }


    public class SearchResponse<TRecordType>
        : ISearchResponse
    {
        public SearchResponse(
        )
        {
        }
    }


    public class GetJoinedTableDelegate
    {
        public GetJoinedTableDelegate(
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


    public class TableJoinType
    {
    }


    public interface IJoinedTable
    {
    }


    public interface ITableToJoinCollection
    {
    }


    public class JoinedTable
        : IJoinedTable
    {
        public JoinedTable(
            string arg0,
            string arg1,
            string arg2,
            TableJoinType arg3,
            ICollection arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly string field0;
        public readonly string field1;
        public readonly string field2;
        public readonly TableJoinType field3;
        public readonly ICollection field4;
    }


    public class TableToJoinCollection
        : ITableToJoinCollection
    {
        public TableToJoinCollection(
        )
        {
        }
    }


    public class MultiplierSearchRequestField : SearchRequestField
    {
        public MultiplierSearchRequestField(
            string arg0,
            string arg1,
            ICollection arg2,
            AggregateFunction arg3,
            SearchRequestFieldOption arg4,
            ICustomFormulaPartParameter arg5
        ) : base()
        {
            field4 = arg4;
        }

        public MultiplierSearchRequestField(
            SearchRequestField arg0,
            string arg1,
            ICollection arg2
        ) : base()
        {
        }

        public readonly AggregateFunction field3;
        public readonly SearchRequestFieldOption field4;
    }


    public class RowDataConverter
    {
        public RowDataConverter(
        )
        {
        }
    }


    public class SearchCriteriaComparison
    {
    }


    public class SearchFieldDataType
    {
    }


    public class SearchFieldOption
    {
    }


    public class SearchFilterEntry
    {
        public SearchFilterEntry(
        )
        {
        }
    }


    public class SearchRequestCriteriaField
    {
        public SearchRequestCriteriaField(
            string arg0,
            object arg1,
            SearchCriteriaComparison arg2,
            AggregateFunction arg3,
            ICustomFormulaPartParameter arg4,
            string arg5
        )
        {
            field0 = arg0;
            field5 = arg5;
        }

        public SearchRequestCriteriaField(
            string arg0,
            TimePeriod arg1,
            AggregateFunction arg2,
            ICustomFormulaPartParameter arg3,
            string arg4
        )
        {
            field0 = arg0;
            field3 = arg3;
            field4 = arg4;
        }

        public SearchRequestCriteriaField(
            string arg0,
            NumericRange arg1,
            AggregateFunction arg2,
            ICustomFormulaPartParameter arg3,
            string arg4
        )
        {
            field0 = arg0;
            field3 = arg3;
            field4 = arg4;
        }

        public SearchRequestCriteriaField(
            string arg0,
            IEnumerable arg1,
            AggregateFunction arg2,
            ICustomFormulaPartParameter arg3,
            string arg4
        )
        {
            field0 = arg0;
            field3 = arg3;
            field4 = arg4;
        }

        public SearchRequestCriteriaField(
            string arg0,
            LogicalOperator arg1,
            string arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly string field0;
        public readonly LogicalOperator field1;
        public readonly string field2;
        public readonly ICustomFormulaPartParameter field3;
        public readonly string field4;
        public readonly string field5;
    }


    public class LogicalOperator
    {
    }


    public class SearchRequestField
    {
        public SearchRequestField()
        {
        }

        public SearchRequestField(
            string arg0,
            AggregateFunction arg1,
            SearchRequestFieldOption arg2,
            ICustomFormulaPartParameter arg3
        )
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public SearchRequestField(
            SearchRequestField arg0
        )
        {
            field0 = arg0;
        }

        public readonly SearchRequestField field0;
        public readonly AggregateFunction field1;
        public readonly SearchRequestFieldOption field2;
        public readonly ICustomFormulaPartParameter field3;
    }


    public class SearchRequestFieldEx : SearchRequestField
    {
        public SearchRequestFieldEx(
            string arg0,
            string arg1,
            AggregateFunction arg2,
            SearchRequestFieldOption arg3,
            ICustomFormulaPartParameter arg4
        ) : base()
        {
            field4 = arg4;
        }

        public readonly ICustomFormulaPartParameter field4;
    }


    public class SearchRequestFieldOption
    {
    }


    public class SearchRequestOption
    {
    }


    public class SearchRequestSortField
    {
        public SearchRequestSortField(
            SearchRequestField arg0,
            SortDirection arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly SearchRequestField field0;
        public readonly SortDirection field1;
    }


    public class SortField
    {
        public SortField(
            string arg0,
            SortDirection arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly SortDirection field1;
    }


    public class GetRowDataDelegate
    {
        public GetRowDataDelegate(
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


    public class CustomPropertySetter
        : IPropertySetter
    {
        public CustomPropertySetter(
            ICollection<String> arg1
        )
        {
            field1 = arg1;
        }

        public readonly ICollection<String> field1;
    }


    public class DateTimeOffsetSetter
    {
        public DateTimeOffsetSetter(
            PropertyInfo arg0,
            string arg1
        )
        {
        }
    }


    public class EnumParser
    {
        public EnumParser(
            Type arg0
        )
        {
            field0 = arg0;
        }

        public readonly Type field0;
    }


    public class EnumSetter
    {
        public EnumSetter(
            PropertyInfo arg0,
            string arg1
        ) : base()
        {
        }
    }


    public class IdentifierSetter : IIdentifierSetter
    {
        public IdentifierSetter(
            PropertyInfo arg0,
            string arg1
        )
        {
        }
    }


    public interface IIdentifierSetter
    {
    }


    public class IpAddressSetter
    {
        public IpAddressSetter(
            PropertyInfo arg0,
            string arg1
        )
        {
        }
    }


    public interface IPropertySetter
    {
    }


    public class NullConversionSetter
    {
        public NullConversionSetter(
            PropertyInfo arg0,
            string arg1
        )
        {
        }
    }


    public class SimplePropertySetter
        : IPropertySetter
    {
        public SimplePropertySetter(
            PropertyInfo arg0,
            string arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly PropertyInfo field0;
        public readonly string field1;
    }


    public class EntityPropertySetter
        : IPropertySetter
    {
        public EntityPropertySetter(
            PropertyInfo arg0,
            string arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly PropertyInfo field0;
        public readonly string field1;
    }


    public class SortDirection
    {
    }


    public class TimePeriodCriteriaEntry
        : ICriteriaEntry
    {
        public TimePeriodCriteriaEntry(
            string arg0,
            TimePeriod arg1
        )
        {
            field0 = arg0;
        }

        public TimePeriodCriteriaEntry(
            string arg0,
            DateTime? arg1,
            DateTime arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly string field0;
        public readonly DateTime? field1;
        public readonly DateTime field2;
    }

    public abstract class EntityServiceBase<TContext, TEntity, TRepository>
        where TContext : IContext
        where TEntity : IIdentifiableEntity
        where TRepository : IEntityRepository<TEntity>
    {
        protected readonly IContextService<TContext> ContextService;
        protected readonly TRepository Repository;
        protected readonly IValidator<TEntity> Validator;
        protected readonly IAuthorization<TContext, TEntity> Authorization;


        public EntityServiceBase(IContextService<TContext> contextService, TRepository repository,
            IValidator<TEntity> validator, IAuthorization<TContext, TEntity> authorization)
        {
            ContextService = contextService;
            Repository = repository;
            Validator = validator;
            Authorization = authorization;
        }

        protected EntityServiceBase()
        {
        }
    }


    public interface IPasswordGenerator
    {
    }


    public class PasswordGenerator
        : IPasswordGenerator
    {
        public PasswordGenerator(
        )
        {
        }
    }


    public class DatabasePropertyAttribute : Attribute
    {
        public DatabasePropertyAttribute(
        )
        {
        }
    }


    public class ForeignPropertyAttribute : DatabasePropertyAttribute
    {
        public ForeignPropertyAttribute(
        )
        {
        }
    }


    public class AuditTrailAttribute : DatabasePropertyAttribute
    {
        public AuditTrailAttribute(
        )
        {
        }
    }


    public class GeneratedAttribute : AuditTrailAttribute
    {
        public GeneratedAttribute(
        )
        {
        }
    }


    public class DatabaseEntityAttribute : Attribute
    {
        public DatabaseEntityAttribute(
        )
        {
        }
    }


    public class ForeignKeyAttribute : Attribute
    {
        public ForeignKeyAttribute(
            string arg0,
            string arg1,
            string arg2,
            string arg3,
            string arg4,
            string arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly string field0;
        public readonly string field1;
        public readonly string field2;
        public readonly string field3;
        public readonly string field4;
        public readonly string field5;
    }


    public class OrganizationForeignKeyAttribute : ForeignKeyAttribute
    {
        public OrganizationForeignKeyAttribute(
            string arg0,
            string arg1,
            string arg2,
            string arg3,
            string arg4,
            string arg5
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5)
        {
        }
    }


    public class FixedGroupSort
    {
        public FixedGroupSort(
        )
        {
        }
    }


    public class GenericValidationRule<TEntity> : ValidationRuleBase<TEntity>
    {
        public GenericValidationRule(
            string arg1,
            ITranslationEntry arg2
        ) : base(arg1, arg2)
        {
            field2 = arg2;
        }

        public readonly ITranslationEntry field2;
    }


    public class IPAddressValidationRule<TEntity> : ValidationRuleBase<TEntity>
    {
        public IPAddressValidationRule(
            string arg0,
            ITranslationEntry arg1,
            ITranslationEntry arg2
        ) : base(arg0, arg1)
        {
            field2 = arg2;
        }

        public readonly ITranslationEntry field2;
    }


    public interface IValidationError
        : ITranslationEntry
    {
    }


    public class ValidationError : CompositeTranslationEntry
        , IValidationError
    {
        public ValidationError(
            ITranslationEntry arg0,
            KeyValuePair<String, ITranslationEntry> arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public interface IValidationRule<T>
    {
    }

    public class RegexValidationRule<TEntity> : ValidationRuleBase<TEntity>
    {
        public RegexValidationRule(
            string arg0,
            ITranslationEntry arg1,
            Regex arg2,
            ITranslationEntry arg3
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
        }

        public readonly Regex field2;
        public readonly ITranslationEntry field3;
    }


    public class RequiredOnUpdateValidationRule<TEntity> : RequiredValidationRule<TEntity>
    {
        public RequiredOnUpdateValidationRule(
            string arg0,
            ITranslationEntry arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class RequiredValidationRule<TEntity> : ValidationRuleBase<TEntity>
    {
        public RequiredValidationRule(
            string arg0,
            ITranslationEntry arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class StringValidationRule<TEntity> : ValidationRuleBase<TEntity>
    {
        public StringValidationRule(
            string arg0,
            ITranslationEntry arg1,
            int arg2,
            int arg3
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
        }

        public readonly int field2;
        public readonly int field3;
    }


    public class ValidationRuleBase<T>
        : IValidationRule<T>
    {
        public ValidationRuleBase(
            string arg0,
            ITranslationEntry arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly ITranslationEntry field1;
    }


    public class ValidationRuleCollection
        : IEnumerable
    {
        public ValidationRuleCollection(
        )
        {
        }

        public IEnumerator GetEnumerator() => throw new NotImplementedException();
    }


    public class Validator<TEntity>
        : IValidator<TEntity>
    {
        public Validator(
        )
        {
        }
    }


    public class PrivateImplementationDetails
    {
    }


    public class ContextIdentity : GenericIdentity
    {
        public ContextIdentity(string name) : base(name)
        {
        }

        public ContextIdentity(string name, string type) : base(name, type)
        {
        }

        protected ContextIdentity(GenericIdentity identity) : base(identity)
        {
        }
    }


    public class Name
    {
        public Name(
        )
        {
        }
    }


    public interface IAxis
    {
    }


    public class XAxis
    {
        public XAxis(
            string arg0,
            GetTranslationDelegate arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly GetTranslationDelegate field1;
    }


    public class Parameter
    {
        public Parameter(
        )
        {
        }

        public Parameter(
            string arg0,
            string arg1,
            GetRowDataDelegate arg2,
            string arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly string field0;
        public readonly string field1;
        public readonly GetRowDataDelegate field2;
        public readonly string field3;
    }


    public class TimeFrameType
    {
        public TimeFrameType(
        )
        {
        }
    }


    public class TimeFrame
    {
        public TimeFrame(
        )
        {
        }
    }


    public class GetOrganizationIdDelegate
    {
        public GetOrganizationIdDelegate(
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


    public class Operand
    {
    }


    public class DataReaderNamedPropertyContainer
        : INamedPropertyContainer
    {
        public DataReaderNamedPropertyContainer(
            IDataReader arg0
        )
        {
            field0 = arg0;
        }

        public readonly IDataReader field0;
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


    public class ReadDelegate
    {
        public ReadDelegate(
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


    public class ConverterInfo
    {
        public ConverterInfo(
            string arg0,
            GetRowDataDelegate arg1,
            bool arg2,
            string arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly string field0;
        public readonly GetRowDataDelegate field1;
        public readonly bool field2;
        public readonly string field3;
    }


    public class SetPropertyDelegate
    {
        public SetPropertyDelegate(
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


    public class Group
    {
        public Group(
            string arg0
        )
        {
            field0 = arg0;
        }

        public readonly string field0;
    }


    public class ValidateEntityDelegate
    {
        public ValidateEntityDelegate(
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
}