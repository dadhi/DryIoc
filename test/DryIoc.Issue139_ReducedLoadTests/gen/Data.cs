using System;
using System.Collections.Generic;
using System.Data.Common;
using RM;
using Entities;
using Framework;
using Logic;
using OrganizationBase;
using Search;
using Shared;
using Activity = Entities.Activity;
using ActivityCount = Entities.ActivityCount;
using File = Entities.File;
using Invoice = Entities.Invoice;
using Item = Entities.Item;
using Organization = Framework.Organization;
using SearchCriteria = Search.SearchCriteria;
using Workweek = Entities.Workweek;

namespace Data
{
    public interface IAuditTrail<T>
    {
    }

    public class AuditTrail<T> : IAuditTrail<T>
    {
        public static readonly IAuditTrail<T> Definition = new AuditTrail<T>();
    }


    public abstract class
        OrganizationEntityRepository<TEntityFields, TEntity, TContext> : EntityRepository<TEntityFields, TEntity>
        where TEntityFields : IOrganizationEntity, new()
        where TEntity : TEntityFields, IIdentifiableEntityWithOriginalState<TEntityFields>
        where TContext : IOrganizationContextBase<User, IPsaCompany>
    {
        protected readonly ICustomerDatabaseRepository CustomerDatabaseRepository;

        public OrganizationEntityRepository() : base()
        {
        }

        protected OrganizationEntityRepository(IContextService<TContext> contextService, IConfiguration configuration,
            DbProviderFactory dbProviderFactory) : base(contextService, configuration, dbProviderFactory)
        {
        }

        protected OrganizationEntityRepository(IContextService<TContext> contextService, IConfiguration configuration,
            DbProviderFactory dbProviderFactory, ICustomerDatabaseRepository customerDatabaseRepository) : base(
            contextService, configuration, dbProviderFactory)
        {
            CustomerDatabaseRepository = customerDatabaseRepository;
        }
    }

    public abstract class EntityRepository<TEntityFields, TEntity> : RepositoryBase, IEntityRepository<TEntity>
        where TEntityFields : IIdentifiableEntity
        where TEntity : TEntityFields, IIdentifiableEntityWithOriginalState<TEntityFields>
    {
        public EntityRepository() : base()
        {
        }

        public EntityRepository(IContextService contextService, IConfiguration configuration,
            DbProviderFactory dbProviderFactory) : base(contextService, configuration, dbProviderFactory)
        {
        }
    }

    public class ActivityAuditTrail : AuditTrail<Activity>
    {
    }


    public class BillingPlanAuditTrail : AuditTrail<BillingPlan>
    {
    }


    public class CaseAuditTrail : AuditTrail<Case>
    {
    }

    public class EmploymentAuditTrail : AuditTrail<Employment>
    {
    }

    public class RightAuditTrail : AuditTrail<Right>
    {
    }


    public class CaseMemberAuditTrail : AuditTrail<CaseMember>
    {
    }


    public class CommunicatesWithAuditTrail : AuditTrail<CommunicatesWith>
    {
    }


    public class ContactAuditTrail : AuditTrail<Contact>
    {
    }


    public class CostCenterRevenueAuditTrail : AuditTrail<CostCenterRevenue>
    {
    }


    public class ExportAuditTrail : AuditTrail<IReport>
    {
    }


    public class InvoiceAuditTrail : AuditTrail<Invoice>
    {
    }


    public class InvoiceCaseAuditTrail : AuditTrail<InvoiceCase>
    {
    }

    public class UserAuditTrail : AuditTrail<User>
    {
    }


    public class InvoiceRowAuditTrail : AuditTrail<InvoiceRow>
    {
    }


    public class ItemAuditTrail : AuditTrail<Item>
    {
    }


    public class TaskAuditTrail : AuditTrail<Task>
    {
    }


    public class TaskMemberAuditTrail : AuditTrail<TaskMember>
    {
    }


    public class AccountFormulaHandler : PsaFormulaHandler
    {
        public AccountFormulaHandler(
            string arg0,
            string arg1
        ) : base(arg0)
        {
            Field1 = arg1;
        }

        public readonly string Field1;
    }


    public class BusinessUnitFormulaHandler : PsaFormulaHandler
    {
        public BusinessUnitFormulaHandler(
            string arg0,
            string arg1
        ) : base(arg0)
        {
            Field1 = arg1;
        }

        public readonly string Field1;
    }


    public class CaseFormulaHandler : PsaFormulaHandler
    {
        public CaseFormulaHandler(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class ContactFormulaHandler : PsaFormulaHandler
    {
        public ContactFormulaHandler(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class HourFormulaHandler : PsaFormulaHandler
    {
        public HourFormulaHandler(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class InvoiceFormulaHandler : PsaFormulaHandler
    {
        public InvoiceFormulaHandler(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class ItemFormulaHandler
    {
        public ItemFormulaHandler(
        )
        {
        }
    }


    public class OfferFormulaHandler : PsaFormulaHandler
    {
        public OfferFormulaHandler(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class TaskFormulaHandler : PsaFormulaHandler
    {
        public TaskFormulaHandler(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class TimeEntryFormulaHandler
    {
        public TimeEntryFormulaHandler(
        )
        {
        }
    }


    public class UserFormulaHandler : PsaFormulaHandler
    {
        public UserFormulaHandler(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class WorktimeFormulaHandler
    {
        public WorktimeFormulaHandler(
        )
        {
        }
    }


    public class AccessRightsHelper
        : IAccessRightsHelper
    {
        public AccessRightsHelper(
            IRightRepository arg0
        )
        {
            Field0 = arg0;
        }

        public readonly IRightRepository Field0;
    }


    public interface IAccessRightsHelper
    {
    }


    public class PsaDataIocModule
    {
    }

    public interface ISqlFormulaHandler<TContext> : ICustomFormulaHandler where TContext : IContext
    {
    }

    public abstract class CustomFormulaDecimalPart<TContext> : CustomFormulaPartBase<TContext> where TContext : IContext
    {
        public CustomFormulaDecimalPart(AggregateFunction? defaultAggregateFunction = AggregateFunction.Sum) : base()
        {
        }
    }

    public abstract class CustomFormulaDecimalPart<TContext, TParameter> : CustomFormulaDecimalPart<TContext>
        where TContext : IContext
    {
        protected TParameter Parameter;
        private readonly Phrase _description;

        public CustomFormulaDecimalPart(Phrase description, TParameter parameter, string sqlSynonym = null,
            AggregateFunction? defaultAggregateFunction = AggregateFunction.Sum) : base(defaultAggregateFunction)
        {
            _description = description;
            Parameter = parameter;
        }

        protected CustomFormulaDecimalPart()
        {
        }
    }

    public abstract class
        CustomFormulaDecimalPart<TContext, TFormulaHandler, TParameter> : CustomFormulaDecimalPart<TContext, TParameter>
        where TContext : IContext
        where TFormulaHandler : ISqlFormulaHandler<TContext>
    {
        public CustomFormulaDecimalPart(Phrase description, TParameter parameter, string sqlSynonym = null,
            AggregateFunction? defaultAggregateFunction = AggregateFunction.Sum) : base(description, parameter,
            sqlSynonym, defaultAggregateFunction)
        {
        }

        protected CustomFormulaDecimalPart() : base()
        {
        }
    }


    public abstract class
        PsaCustomFormulaDecimalPart<TFormulaHandler, TParameter> : CustomFormulaDecimalPart<IPsaContext, TFormulaHandler, TParameter>
        where TFormulaHandler : class, ISqlFormulaHandler<IPsaContext>
    {
        public PsaCustomFormulaDecimalPart(Phrase description, TParameter parameter, string sqlSynonym = null,
            AggregateFunction defaultAggregateFunction = AggregateFunction.Sum) : base(description, parameter,
            sqlSynonym, defaultAggregateFunction)
        {
        }

        protected PsaCustomFormulaDecimalPart(string description)
        {
        }

        protected PsaCustomFormulaDecimalPart(PriceOfWorkHoursNotReviewed description, string parameter) : base()
        {
        }

        protected PsaCustomFormulaDecimalPart(UnitCost description, string parameter)
        {
        }

        protected PsaCustomFormulaDecimalPart(CaseCount description, string parameter)
        {
        }

        protected PsaCustomFormulaDecimalPart()
        {
        }
    }


    public partial class PsaCustomFormulaStringPart : CustomFormulaStringPart<IPsaContext>
    {
        public PsaCustomFormulaStringPart(
        )
        {
        }
    }


    public abstract class PsaCustomFormulaStringPart<TParameter> : PsaCustomFormulaStringPart
        where TParameter : class, ICustomFormulaPartParameter
    {
        private TParameter _parameter;
        private readonly Phrase _description;

        public PsaCustomFormulaStringPart(Phrase description, TParameter parameter, string sqlSynonym = null) : base()
        {
            _description = description;
            _parameter = parameter;
        }
    }


    public class PsaFormulaHandler : SqlFormulaHandler<IPsaContext>
    {
        public PsaFormulaHandler(
            string arg0
        )
        {
        }
    }


    public class AccountNoteRepository : OrganizationEntityRepository<AccountNoteFields, AccountNote, IPsaContext>
        , IAccountNoteRepository
    {
        public AccountNoteRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ActivityContactMemberRepository : OrganizationEntityRepository<ActivityContactMemberFields,
            ActivityContactMember, IPsaContext>
        , IActivityContactMemberRepository
    {
        public ActivityContactMemberRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ActivityRepository : OrganizationEntityRepository<ActivityFields, Activity, IPsaContext>
        , IActivityRepository
    {
        public ActivityRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ActivityResourceMemberRepository : OrganizationEntityRepository<ActivityResourceMemberFields,
            ActivityResourceMember, IPsaContext>
        , IActivityResourceMemberRepository
    {
        public ActivityResourceMemberRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ActivityStatusRepository :
        OrganizationEntityRepository<ActivityStatusFields, ActivityStatus, IPsaContext>
        , IActivityStatusRepository
    {
        public ActivityStatusRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ActivityTypeRepository : OrganizationEntityRepository<ActivityTypeFields, ActivityType, IPsaContext>
        , IActivityTypeRepository
    {
        public ActivityTypeRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ActivityUserMemberRepository :
        OrganizationEntityRepository<ActivityUserMemberFields, ActivityUserMember, IPsaContext>
        , IActivityUserMemberRepository
    {
        public ActivityUserMemberRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class AuditTrailRepository :
        OrganizationEntityRepository<AuditTrailEntryFields, AuditTrailEntry, IPsaContext>
        , IAuditTrailRepository
    {
        public AuditTrailRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class AuthorizedIpAddressRepository :
        OrganizationEntityRepository<AuthorizedIPAddressFields, AuthorizedIPAddress, IPsaContext>
        , IAuthorizedIpAddressRepository
    {
        public AuthorizedIpAddressRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class BackgroundTaskRepository :
        OrganizationEntityRepository<BackgroundTaskFields, BackgroundTask, IPsaContext>
        , IBackgroundTaskRepository
    {
        public BackgroundTaskRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class BackgroundTaskRunRepository :
        OrganizationEntityRepository<BackgroundTaskRunFields, BackgroundTaskRun, IPsaContext>
        , IBackgroundTaskRunRepository
    {
        public BackgroundTaskRunRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class BankAccountRepository : OrganizationEntityRepository<BankAccountFields, BankAccount, IPsaContext>
        , IBankAccountRepository
    {
        public BankAccountRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class BillingPlanRepository : OrganizationEntityRepository<BillingPlanFields, BillingPlan, IPsaContext>
        , IBillingPlanRepository
    {
        public BillingPlanRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class BusinessOverviewRepository :
        OrganizationEntityRepository<BusinessOverviewFields, BusinessOverview, IPsaContext>
        , IBusinessOverviewRepository
    {
        public BusinessOverviewRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class BusinessUnitRepository : OrganizationEntityRepository<BusinessUnitFields, BusinessUnit, IPsaContext>
        , IBusinessUnitRepository
    {
        public BusinessUnitRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CalendarSyncActivityMapRepository : OrganizationEntityRepository<CalendarSyncActivityMapFields,
            CalendarSyncActivityMap, IPsaContext>
        , ICalendarSyncActivityMapRepository
    {
        public CalendarSyncActivityMapRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CalendarSyncActivityNonAppParticipantRepository : OrganizationEntityRepository<
            CalendarSyncActivityNonAppParticipantFields, CalendarSyncActivityNonAppParticipant, IPsaContext>
        , ICalendarSyncActivityNonAppParticipantRepository
    {
        public CalendarSyncActivityNonAppParticipantRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CalendarSyncDeviceRepository :
        OrganizationEntityRepository<CalendarSyncDeviceFields, CalendarSyncDevice, IPsaContext>
        , ICalendarSyncDeviceRepository
    {
        public CalendarSyncDeviceRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CalendarSyncUserCalendarRepository : OrganizationEntityRepository<CalendarSyncUserCalendarFields,
            CalendarSyncUserCalendar, IPsaContext>
        , ICalendarSyncUserCalendarRepository
    {
        public CalendarSyncUserCalendarRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CaseBillingAccountRepository :
        OrganizationEntityRepository<CaseBillingAccountFields, CaseBillingAccount, IPsaContext>
        , ICaseBillingAccountRepository
    {
        public CaseBillingAccountRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CaseCommentRepository : OrganizationEntityRepository<CaseCommentFields, CaseComment, IPsaContext>
        , ICaseCommentRepository
    {
        public CaseCommentRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CaseFileRepository : OrganizationEntityRepository<CaseFileFields, CaseFile, IPsaContext>
        , ICaseFileRepository
    {
        public CaseFileRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CaseMemberRepository : OrganizationEntityRepository<CaseMemberFields, CaseMember, IPsaContext>
        , ICaseMemberRepository
    {
        public CaseMemberRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CaseNoteRepository : OrganizationEntityRepository<CaseNoteFields, CaseNote, IPsaContext>
        , ICaseNoteRepository
    {
        public CaseNoteRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CaseProductRepository : OrganizationEntityRepository<CaseProductFields, CaseProduct, IPsaContext>
        , ICaseProductRepository
    {
        public CaseProductRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CaseRepository : OrganizationEntityRepository<CaseFields, Case, IPsaContext>
        , ICaseRepository
    {
        public CaseRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3,
            ITimeEntryRepository arg4,
            IOrganizationCompanyRepository arg5
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
            Field4 = arg4;
            Field5 = arg5;
        }

        public CaseRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
        public readonly ITimeEntryRepository Field4;
        public readonly IOrganizationCompanyRepository Field5;
    }


    public class CaseStatusRepository : OrganizationEntityRepository<CaseStatusFields, CaseStatus, IPsaContext>
        , ICaseStatusRepository
    {
        public CaseStatusRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CaseStatusTypeRepository :
        OrganizationEntityRepository<CaseStatusTypeFields, CaseStatusType, IPsaContext>
        , ICaseStatusTypeRepository
    {
        public CaseStatusTypeRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CaseTagRepository : OrganizationEntityRepository<CaseTagFields, CaseTag, IPsaContext>
        , ICaseTagRepository
    {
        public CaseTagRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CaseWorkTypeRepository : OrganizationEntityRepository<CaseWorkTypeFields, CaseWorkType, IPsaContext>
        , ICaseWorkTypeRepository
    {
        public CaseWorkTypeRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CommunicationMethodRepository :
        OrganizationEntityRepository<CommunicationMethodFields, CommunicationMethod, IPsaContext>
        , ICommunicationMethodRepository
    {
        public CommunicationMethodRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ContactRoleRepository : OrganizationEntityRepository<ContactRoleFields, ContactRole, IPsaContext>
        , IContactRoleRepository
    {
        public ContactRoleRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ContactTagRepository : OrganizationEntityRepository<ContactTagFields, ContactTag, IPsaContext>
        , IContactTagRepository
    {
        public ContactTagRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CostCenterRepository : OrganizationEntityRepository<CostCenterFields, CostCenter, IPsaContext>
        , ICostCenterRepository
    {
        public CostCenterRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CostCenterRevenueRepository :
        OrganizationEntityRepository<CostCenterRevenueFields, CostCenterRevenue, IPsaContext>
        , ICostCenterRevenueRepository
    {
        public CostCenterRevenueRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CountryProductRepository :
        OrganizationEntityRepository<CountryProductFields, CountryProduct, IPsaContext>
        , ICountryProductRepository
    {
        public CountryProductRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class CustomFormulaRepository : OrganizationEntityRepository<CustomFormulaFields, CustomFormula, IPsaContext>
        , ICustomFormulaRepository
    {
        public CustomFormulaRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class DashboardPartRepository : OrganizationEntityRepository<DashboardPartFields, DashboardPart, IPsaContext>
        , IDashboardPartRepository
    {
        public DashboardPartRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class DashboardRepository : OrganizationEntityRepository<DashboardFields, Dashboard, IPsaContext>
        , IDashboardRepository
    {
        public DashboardRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class EmploymentRepository : OrganizationEntityRepository<EmploymentFields, Employment, IPsaContext>
        , IEmploymentRepository
    {
        public EmploymentRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }

    public class EmploymentFields : IOrganizationEntity
    {
    }


    public class ExtranetCaseContactRepository :
        OrganizationEntityRepository<ExtranetCaseContactFields, ExtranetCaseContact, IPsaContext>
        , IExtranetCaseContactRepository
    {
        public ExtranetCaseContactRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ExtranetCaseInfoRepository :
        OrganizationEntityRepository<ExtranetCaseInfoFields, ExtranetCaseInfo, IPsaContext>
        , IExtranetCaseInfoRepository
    {
        public ExtranetCaseInfoRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class FileDataRepository : OrganizationEntityRepository<FileDataFields, FileData, IPsaContext>
        , IFileDataRepository
    {
        public FileDataRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class FileDownloadRepository : OrganizationEntityRepository<FileDownloadFields, FileDownload, IPsaContext>
        , IFileDownloadRepository
    {
        public FileDownloadRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class FileRepository : OrganizationEntityRepository<FileFields, File, IPsaContext>
        , IFileRepository
    {
        public FileRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class FileTagRepository : OrganizationEntityRepository<FileTagFields, FileTag, IPsaContext>
        , IFileTagRepository
    {
        public FileTagRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public interface IBackgroundTaskProgressRepository
        : IEntityRepository<BackgroundTaskProgress>
    {
    }


    public class BackgroundTaskProgressRepository : OrganizationEntityRepository<BackgroundTaskProgressFields,
            BackgroundTaskProgress, IPsaContext>
        , IBackgroundTaskProgressRepository
    {
        public BackgroundTaskProgressRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public interface ICostAccountRepository
        : IEntityRepository<CostAccount>
    {
    }


    public class CostAccountRepository : OrganizationEntityRepository<CostAccountFields, CostAccount, IPsaContext>
        , ICostAccountRepository
    {
        public CostAccountRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public interface ICustomFormulaSetRepository
        : IEntityRepository<CustomFormulaSet>
    {
    }


    public class CustomFormulaSetRepository :
        OrganizationEntityRepository<CustomFormulaSetFields, CustomFormulaSet, IPsaContext>
        , ICustomFormulaSetRepository
    {
        public CustomFormulaSetRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public interface IImportRepository
        : IEntityRepository<Import>
    {
    }


    public class ImportRepository : OrganizationEntityRepository<ImportFields, Import, IPsaContext>
        , IImportRepository
    {
        public ImportRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public interface IProductCostAccountRepository
        : IEntityRepository<ProductCostAccount>
    {
    }


    public class ProductCostAccountRepository :
        OrganizationEntityRepository<ProductCostAccountFields, ProductCostAccount, IPsaContext>
        , IProductCostAccountRepository
    {
        public ProductCostAccountRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public interface IProfileSsoMappingRepository
        : IEntityRepository<ProfileSSOMapping>
    {
    }


    public class ProfileSsoMappingRepository :
        OrganizationEntityRepository<ProfileSSOMappingFields, ProfileSSOMapping, IPsaContext>
        , IProfileSsoMappingRepository
    {
        public ProfileSsoMappingRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ReportRepository : OrganizationEntityRepository<ReportFields, Report, IPsaContext>
        , IReportRepository
    {
        public ReportRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ProfileDashboardRepository :
        OrganizationEntityRepository<ProfileDashboardFields, ProfileDashboard, IPsaContext>
        , IProfileDashboardRepository
    {
        public ProfileDashboardRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class HourRepository : OrganizationEntityRepository<HourFields, Hour, IPsaContext>
        , IHourRepository
    {
        public HourRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class IndustryRepository : OrganizationEntityRepository<IndustryFields, Industry, IPsaContext>
        , IIndustryRepository
    {
        public IndustryRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class IntegrationErrorRepository :
        OrganizationEntityRepository<IntegrationErrorFields, IntegrationError, IPsaContext>
        , IIntegrationErrorRepository
    {
        public IntegrationErrorRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class InvoiceRepository : OrganizationEntityRepository<InvoiceFields, Invoice, IPsaContext>
        , IInvoiceRepository
    {
        public InvoiceRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class InvoiceBankAccountRepository :
        OrganizationEntityRepository<InvoiceBankAccountFields, InvoiceBankAccount, IPsaContext>
        , IInvoiceBankAccountRepository
    {
        public InvoiceBankAccountRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class InvoiceCaseRepository : OrganizationEntityRepository<InvoiceCaseFields, InvoiceCase, IPsaContext>
        , IInvoiceCaseRepository
    {
        public InvoiceCaseRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class InvoiceConfigRepository : OrganizationEntityRepository<InvoiceConfigFields, InvoiceConfig, IPsaContext>
        , IInvoiceConfigRepository
    {
        public InvoiceConfigRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
            //ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            //Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class InvoiceFileRepository : OrganizationEntityRepository<InvoiceFileFields, InvoiceFile, IPsaContext>
        , IInvoiceFileRepository
    {
        public InvoiceFileRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class InvoiceHtmlRepository : OrganizationEntityRepository<InvoiceHTMLFields, InvoiceHTML, IPsaContext>
        , IInvoiceHtmlRepository
    {
        public InvoiceHtmlRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class InvoiceRowRepository : OrganizationEntityRepository<InvoiceRowFields, InvoiceRow, IPsaContext>
        , IInvoiceRowRepository
    {
        public InvoiceRowRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class InvoiceStatusRepository : OrganizationEntityRepository<InvoiceStatusFields, InvoiceStatus, IPsaContext>
        , IInvoiceStatusRepository
    {
        public InvoiceStatusRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class InvoiceStatusHistoryRepository :
        OrganizationEntityRepository<InvoiceStatusHistoryFields, InvoiceStatusHistory, IPsaContext>
        , IInvoiceStatusHistoryRepository
    {
        public InvoiceStatusHistoryRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class InvoiceTemplateRepository :
        OrganizationEntityRepository<InvoiceTemplateFields, InvoiceTemplate, IPsaContext>
        , IInvoiceTemplateRepository
    {
        public InvoiceTemplateRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class InvoiceTemplateConfigRepository : OrganizationEntityRepository<InvoiceTemplateConfigFields,
            InvoiceTemplateConfig, IPsaContext>
        , IInvoiceTemplateConfigRepository
    {
        public InvoiceTemplateConfigRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ItemRepository : OrganizationEntityRepository<ItemFields, Item, IPsaContext>
        , IItemRepository
    {
        public ItemRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ItemFileRepository : OrganizationEntityRepository<ItemFileFields, ItemFile, IPsaContext>
        , IItemFileRepository
    {
        public ItemFileRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ItemSalesAccountRepository :
        OrganizationEntityRepository<ItemSalesAccountFields, ItemSalesAccount, IPsaContext>
        , IItemSalesAccountRepository
    {
        public ItemSalesAccountRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class LeadSourceRepository : OrganizationEntityRepository<LeadSourceFields, LeadSource, IPsaContext>
        , ILeadSourceRepository
    {
        public LeadSourceRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class LinkRepository : OrganizationEntityRepository<LinkFields, Link, IPsaContext>
        , ILinkRepository
    {
        public LinkRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class NavigationHistoryRepository :
        OrganizationEntityRepository<NavigationHistoryFields, NavigationHistory, IPsaContext>
        , INavigationHistoryRepository
    {
        public NavigationHistoryRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class OfferRepository : OrganizationEntityRepository<OfferFields, Offer, IPsaContext>
        , IOfferRepository
    {
        public OfferRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class OfferFileRepository : OrganizationEntityRepository<OfferFileFields, OfferFile, IPsaContext>
        , IOfferFileRepository
    {
        public OfferFileRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class OfferItemRepository : OrganizationEntityRepository<OfferItemFields, OfferItem, IPsaContext>
        , IOfferItemRepository
    {
        public OfferItemRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class OfferSubtotalRepository : OrganizationEntityRepository<OfferSubtotalFields, OfferSubtotal, IPsaContext>
        , IOfferSubtotalRepository
    {
        public OfferSubtotalRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class OfferTaskRepository : OrganizationEntityRepository<OfferTaskFields, OfferTask, IPsaContext>
        , IOfferTaskRepository
    {
        public OfferTaskRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class OrganizationCompanyProductRepository : OrganizationEntityRepository<OrganizationCompanyProductFields,
            OrganizationCompanyProduct, IPsaContext>
        , IOrganizationCompanyProductRepository
    {
        public OrganizationCompanyProductRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class OrganizationCompanyWorkTypeRepository : OrganizationEntityRepository<OrganizationCompanyWorkTypeFields,
            OrganizationCompanyWorkType, IPsaContext>
        , IOrganizationCompanyWorkTypeRepository
    {
        public OrganizationCompanyWorkTypeRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class OverTimeRepository : OrganizationEntityRepository<OverTimeFields, OverTime, IPsaContext>
        , IOvertimeRepository
    {
        public OverTimeRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class OverTimePriceRepository : OrganizationEntityRepository<OverTimePriceFields, OverTimePrice, IPsaContext>
        , IOvertimePriceRepository
    {
        public OverTimePriceRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class PricelistRepository : OrganizationEntityRepository<PricelistFields, Pricelist, IPsaContext>
        , IPricelistRepository
    {
        public PricelistRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class PricelistVersionRepository :
        OrganizationEntityRepository<PricelistVersionFields, PricelistVersion, IPsaContext>
        , IPricelistVersionRepository
    {
        public PricelistVersionRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ProductRepository : OrganizationEntityRepository<ProductFields, Product, IPsaContext>
        , IProductRepository
    {
        public ProductRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ProductCategoryRepository :
        OrganizationEntityRepository<ProductCategoryFields, ProductCategory, IPsaContext>
        , IProductCategoryRepository
    {
        public ProductCategoryRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ProductPriceRepository : OrganizationEntityRepository<ProductPriceFields, ProductPrice, IPsaContext>
        , IProductPriceRepository
    {
        public ProductPriceRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ProfileRepository : OrganizationEntityRepository<ProfileFields, Profile, IPsaContext>
        , IProfileRepository
    {
        public ProfileRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ProfileRightRepository : OrganizationEntityRepository<ProfileRightFields, ProfileRight, IPsaContext>
        , IProfileRightRepository
    {
        public ProfileRightRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ProposalStatusRepository :
        OrganizationEntityRepository<ProposalStatusFields, ProposalStatus, IPsaContext>
        , IProposalStatusRepository
    {
        public ProposalStatusRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class RecurringItemRepository : OrganizationEntityRepository<RecurringItemFields, RecurringItem, IPsaContext>
        , IRecurringItemRepository
    {
        public RecurringItemRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ReimbursedHourRepository :
        OrganizationEntityRepository<ReimbursedHourFields, ReimbursedHour, IPsaContext>
        , IReimbursedHourRepository
    {
        public ReimbursedHourRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ReimbursedItemRepository :
        OrganizationEntityRepository<ReimbursedItemFields, ReimbursedItem, IPsaContext>
        , IReimbursedItemRepository
    {
        public ReimbursedItemRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ResourceRepository : OrganizationEntityRepository<ResourceFields, Resource, IPsaContext>
        , IResourceRepository
    {
        public ResourceRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ResourceAllocationRepository :
        OrganizationEntityRepository<ResourceAllocationFields, ResourceAllocation, IPsaContext>
        , IResourceAllocationRepository
    {
        public ResourceAllocationRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class SalesAccountRepository : OrganizationEntityRepository<SalesAccountFields, SalesAccount, IPsaContext>
        , ISalesAccountRepository
    {
        public SalesAccountRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class SalesProcessRepository : OrganizationEntityRepository<SalesProcessFields, SalesProcess, IPsaContext>
        , ISalesProcessRepository
    {
        public SalesProcessRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class SalesStatusRepository : OrganizationEntityRepository<SalesStatusFields, SalesStatus, IPsaContext>
        , ISalesStatusRepository
    {
        public SalesStatusRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class SearchRepository : OrganizationEntityRepository<SearchFields, Entities.Search, IPsaContext>
        , ISearchRepository
    {
        public SearchRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class SearchCriteriaRepository :
        OrganizationEntityRepository<SearchCriteriaFields, Entities.SearchCriteria, IPsaContext>
        , ISearchCriteriaRepository
    {
        public SearchCriteriaRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class TagRepository : OrganizationEntityRepository<TagFields, Tag, IPsaContext>
        , ITagRepository
    {
        public TagRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class TaskRepository : OrganizationEntityRepository<TaskFields, Task, IPsaContext>
        , ITaskRepository
    {
        public TaskRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class TaskMemberRepository : OrganizationEntityRepository<TaskMemberFields, TaskMember, IPsaContext>
        , ITaskMemberRepository
    {
        public TaskMemberRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class TaskStatusRepository : OrganizationEntityRepository<TaskStatusFields, TaskStatus, IPsaContext>
        , ITaskStatusRepository
    {
        public TaskStatusRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class TaskStatusCommentRepository :
        OrganizationEntityRepository<TaskStatusCommentFields, TaskStatusComment, IPsaContext>
        , ITaskStatusCommentRepository
    {
        public TaskStatusCommentRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class TaskStatusTypeRepository :
        OrganizationEntityRepository<TaskStatusTypeFields, TaskStatusType, IPsaContext>
        , ITaskStatusTypeRepository
    {
        public TaskStatusTypeRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class TemporaryHourRepository : OrganizationEntityRepository<TemporaryHourFields, TemporaryHour, IPsaContext>
        , ITemporaryHourRepository
    {
        public TemporaryHourRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class TemporaryItemRepository : OrganizationEntityRepository<TemporaryItemFields, TemporaryItem, IPsaContext>
        , ITemporaryItemRepository
    {
        public TemporaryItemRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class TimecardEventRepository : OrganizationEntityRepository<TimecardEventFields, TimecardEvent, IPsaContext>
        , ITimecardEventRepository
    {
        public TimecardEventRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class TimeEntryRepository : OrganizationEntityRepository<TimeEntryFields, TimeEntry, IPsaContext>
        , ITimeEntryRepository
    {
        public TimeEntryRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class TimeEntryTypeRepository : OrganizationEntityRepository<TimeEntryTypeFields, TimeEntryType, IPsaContext>
        , ITimeEntryTypeRepository
    {
        public TimeEntryTypeRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class TravelReimbursementRepository :
        OrganizationEntityRepository<TravelReimbursementFields, TravelReimbursement, IPsaContext>
        , ITravelReimbursementRepository
    {
        public TravelReimbursementRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class TravelReimbursementStatusRepository : OrganizationEntityRepository<TravelReimbursementStatusFields,
            TravelReimbursementStatus, IPsaContext>
        , ITravelReimbursementStatusRepository
    {
        public TravelReimbursementStatusRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class UsedScannerReceiptRepository :
        OrganizationEntityRepository<UsedScannerReceiptFields, UsedScannerReceipt, IPsaContext>
        , IUsedScannerReceiptRepository
    {
        public UsedScannerReceiptRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class UserCostPerCaseRepository :
        OrganizationEntityRepository<UserCostPerCaseFields, UserCostPerCase, IPsaContext>
        , IUserCostPerCaseRepository
    {
        public UserCostPerCaseRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class UserTagRepository : OrganizationEntityRepository<UserTagFields, UserTag, IPsaContext>
        , IUserTagRepository
    {
        public UserTagRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class UserTaskFavoriteRepository :
        OrganizationEntityRepository<UserTaskFavoriteFields, UserTaskFavorite, IPsaContext>
        , IUserTaskFavoriteRepository
    {
        public UserTaskFavoriteRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class UserWeeklyViewRowRepository :
        OrganizationEntityRepository<UserWeeklyViewRowFields, UserWeeklyViewRow, IPsaContext>
        , IUserWeeklyViewRowRepository
    {
        public UserWeeklyViewRowRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class UserSettingsRepository : OrganizationEntityRepository<UserSettingsFields, UserSettings, IPsaContext>
        , IUserSettingsRepository
    {
        public UserSettingsRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class WorkdayRepository : OrganizationEntityRepository<WorkdayFields, Workday, IPsaContext>
        , IWorkdayRepository
    {
        public WorkdayRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class WorkPriceRepository : OrganizationEntityRepository<WorkPriceFields, WorkPrice, IPsaContext>
        , IWorkPriceRepository
    {
        public WorkPriceRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class WorkTypeRepository : OrganizationEntityRepository<WorkTypeFields, WorkType, IPsaContext>
        , IWorkTypeRepository
    {
        public WorkTypeRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class ApiLogEntryRepository : LogEntryRepositoryBase<ApiLogEntryFields, ApiLogEntry>
        , IApiLogEntryRepository
    {
        public ApiLogEntryRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
        }
    }


    public class RequestLogEntryRepository : LogEntryRepositoryBase<RequestLogEntryFields, RequestLogEntry>
        , IRequestLogEntryRepository
    {
        public RequestLogEntryRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
        }
    }


    public class UserLogEntryRepository : LogEntryRepositoryBase<UserLogEntryFields, UserLogEntry>
        , IUserLogEntryRepository
    {
        public UserLogEntryRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
        }
    }


    public class HeartBeatRepository : RepositoryBase
        , IHeartBeatRepository
    {
        public HeartBeatRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
        }

        public readonly IContextService Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
    }


    public interface IAccountNoteRepository
        : IEntityRepository<AccountNote>
    {
    }


    public interface IActivityContactMemberRepository
        : IEntityRepository<ActivityContactMember>
    {
    }


    public interface IActivityRepository
        : IEntityRepository<Activity>
    {
    }


    public interface IActivityResourceMemberRepository
        : IEntityRepository<ActivityResourceMember>
    {
    }


    public interface IActivityStatusRepository
        : IEntityRepository<ActivityStatus>
    {
    }


    public interface IActivityTypeRepository
        : IEntityRepository<ActivityType>
    {
    }


    public interface IActivityUserMemberRepository
        : IEntityRepository<ActivityUserMember>
    {
    }


    public interface IApiLogEntryRepository
        : IEntityRepository<ApiLogEntry>
    {
    }


    public interface IAuditTrailRepository
        : IEntityRepository<AuditTrailEntry>
    {
    }


    public interface IAuthorizedIpAddressRepository
        : IEntityRepository<AuthorizedIPAddress>
    {
    }


    public interface IBackgroundTaskRepository
        : IEntityRepository<BackgroundTask>
    {
    }


    public interface IBackgroundTaskRunRepository
        : IEntityRepository<BackgroundTaskRun>
    {
    }


    public interface IBankAccountRepository
        : IEntityRepository<BankAccount>
    {
    }


    public class ForecastMonthlyGrouping
    {
    }


    public interface IBillingPlanRepository
        : IEntityRepository<BillingPlan>
    {
    }


    public interface IBusinessOverviewRepository
        : IEntityRepository<BusinessOverview>
    {
    }


    public interface IBusinessUnitRepository
        : IEntityRepository<BusinessUnit>
    {
    }


    public interface ICalendarSyncActivityMapRepository
        : IEntityRepository<CalendarSyncActivityMap>
    {
    }


    public interface ICalendarSyncActivityNonAppParticipantRepository
        : IEntityRepository<CalendarSyncActivityNonAppParticipant>
    {
    }


    public interface ICalendarSyncDeviceRepository
        : IEntityRepository<CalendarSyncDevice>
    {
    }


    public interface ICalendarSyncUserCalendarRepository
        : IEntityRepository<CalendarSyncUserCalendar>
    {
    }


    public interface ICaseBillingAccountRepository
        : IEntityRepository<CaseBillingAccount>
    {
    }


    public interface ICaseCommentRepository
        : IEntityRepository<CaseComment>
    {
    }


    public interface ICaseFileRepository
        : IEntityRepository<CaseFile>
    {
    }


    public interface ICaseMemberRepository
        : IEntityRepository<CaseMember>
    {
    }


    public interface ICaseNoteRepository
        : IEntityRepository<CaseNote>
    {
    }


    public interface ICaseProductRepository
        : IEntityRepository<CaseProduct>
    {
    }


    public interface ICaseRepository
        : IEntityRepository<Case>
    {
    }


    public interface ICaseStatusRepository
        : IEntityRepository<CaseStatus>
    {
    }


    public interface ICaseStatusTypeRepository
        : IEntityRepository<CaseStatusType>
    {
    }


    public interface ICaseTagRepository
        : IEntityRepository<CaseTag>
    {
    }


    public interface ICaseWorkTypeRepository
        : IEntityRepository<CaseWorkType>
    {
    }


    public interface ICommunicationMethodRepository
        : IEntityRepository<CommunicationMethod>
    {
    }


    public interface IContactRoleRepository
        : IEntityRepository<ContactRole>
    {
    }


    public interface IContactTagRepository
        : IEntityRepository<ContactTag>
    {
    }


    public interface ICostCenterRepository
        : IEntityRepository<CostCenter>
    {
    }


    public interface ICostCenterRevenueRepository
        : IEntityRepository<CostCenterRevenue>
    {
    }


    public interface ICountryProductRepository
        : IEntityRepository<CountryProduct>
    {
    }


    public interface ICustomFormulaRepository
        : IEntityRepository<CustomFormula>
    {
    }


    public interface IDashboardPartRepository
        : IEntityRepository<DashboardPart>
    {
    }


    public interface IDashboardRepository
        : IEntityRepository<Dashboard>
    {
    }


    public interface IEmploymentRepository
        : IEntityRepository<Employment>
    {
    }


    public interface IExtranetCaseContactRepository
        : IEntityRepository<ExtranetCaseContact>
    {
    }


    public interface IExtranetCaseInfoRepository
        : IEntityRepository<ExtranetCaseInfo>
    {
    }


    public interface IFileDataRepository
        : IEntityRepository<FileData>
    {
    }


    public interface IFileDownloadRepository
        : IEntityRepository<FileDownload>
    {
    }


    public interface IFileRepository
        : IEntityRepository<File>
    {
    }


    public interface IFileTagRepository
        : IEntityRepository<FileTag>
    {
    }


    public interface IHeartBeatRepository
    {
    }


    public interface IHourRepository
        : IEntityRepository<Hour>
    {
    }


    public interface IIndustryRepository
        : IEntityRepository<Industry>
    {
    }


    public interface IIntegrationErrorRepository
        : IEntityRepository<IntegrationError>
    {
    }


    public interface IInvoiceBankAccountRepository
        : IEntityRepository<InvoiceBankAccount>
    {
    }


    public interface IInvoiceCaseRepository
        : IEntityRepository<InvoiceCase>
    {
    }


    public interface IInvoiceConfigRepository
        : IEntityRepository<InvoiceConfig>
    {
    }


    public interface IInvoiceFileRepository
        : IEntityRepository<InvoiceFile>
    {
    }


    public interface IInvoiceHtmlRepository
        : IEntityRepository<InvoiceHTML>
    {
    }


    public interface IInvoiceRepository
        : IEntityRepository<Invoice>
    {
    }


    public interface IInvoiceRowRepository
        : IEntityRepository<InvoiceRow>
    {
    }


    public interface IInvoiceStatusHistoryRepository
        : IEntityRepository<InvoiceStatusHistory>
    {
    }


    public interface IInvoiceStatusRepository
        : IEntityRepository<InvoiceStatus>
    {
    }


    public interface IInvoiceTemplateConfigRepository
        : IEntityRepository<InvoiceTemplateConfig>
    {
    }


    public interface IInvoiceTemplateRepository
        : IEntityRepository<InvoiceTemplate>
    {
    }


    public interface IItemFileRepository
        : IEntityRepository<ItemFile>
    {
    }


    public interface IItemRepository
        : IEntityRepository<Item>
    {
    }


    public interface IItemSalesAccountRepository
        : IEntityRepository<ItemSalesAccount>
    {
    }

    public interface ISearchDefinition<TContext> : IIdentifiable where TContext : IContext
    {

    }

    public interface ISearchDefinition<TContext, TSearchCriteria> : ISearchDefinition<TContext>
        where TContext : IContext
        where TSearchCriteria : ISearchCriteria
    {
    }

    public interface ISearchDefinition<TContext, TSearchCriteria, TEntity> : ISearchDefinition<TContext, TSearchCriteria>
        where TContext : IContext
        where TSearchCriteria : ISearchCriteria
        where TEntity : IIdentifiableEntity
    {
    }

    public interface IKpiComparisonRepository
        : IEntityRepository<KPIComparison>
    {
    }

    public class PsaGenericSearchDefinition<TEntity> : GenericSearchDefinition<IPsaContext, TEntity> where TEntity : IIdentifiableEntity
    {
        public PsaGenericSearchDefinition(IDatabaseConnionResolver<IPsaContext> identifier,
            string databaseConnionResolver) { }

        public PsaGenericSearchDefinition(string identifier, IDatabaseConnionResolver<IPsaContext> databaseConnionResolver, string database = null, AddSearchFieldDelegate<IPsaContext> addAdditionalSearchFields = null) : base(identifier, databaseConnionResolver, database, addAdditionalSearchFields)
        {
        }

        protected PsaGenericSearchDefinition(IDatabaseConnionResolver<IPsaContext> identifier) : base()
        {
        }

        protected PsaGenericSearchDefinition()
        {
            throw new NotImplementedException();
        }
    }

    public class GenericSearchDefinition<TContext, TEntity> : SearchDefinitionBase<TContext, CommonSearchCriteria>, ISearchDefinition<TContext, CommonSearchCriteria, TEntity>
        where TContext : IContext where TEntity : IIdentifiableEntity
    {
        protected class JoinedTable
        {
            public string JoinedTableIdColumn;
            public string ForeignKeyTable;
            public string ForeignKeyColumn;
            public bool JoinOrganization;
            public string CustomJoin;
        }

        private readonly string _identifier;
        private IDatabaseConnionResolver<TContext> _databaseConnionResolver;
        private ProjectDatabase _database;

        private List<Utilities.PropertyAccessor> _properties;
        protected readonly List<JoinedTable> JoinedTables = new List<JoinedTable>();

        public GenericSearchDefinition(string identifier, IDatabaseConnionResolver<TContext> databaseConnionResolver, string database = null, AddSearchFieldDelegate<TContext> addAdditionalSearchFields = null) : base()
        {
        }

        protected GenericSearchDefinition()
        {
        }
    }

    public class AddSearchFieldDelegate<TContext> where TContext : IContext
    {
    }

    public class KpiComparisonRepository : EntityRepository<KPIComparisonFields, KPIComparison, IPsaContext>
        , IKpiComparisonRepository
    {
        public KpiComparisonRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public interface ILeadSourceRepository
        : IEntityRepository<LeadSource>
    {
    }


    public interface ILinkRepository
        : IEntityRepository<Link>
    {
    }


    public interface INavigationHistoryRepository
        : IEntityRepository<NavigationHistory>
    {
    }


    public interface IOfferFileRepository
        : IEntityRepository<OfferFile>
    {
    }


    public interface IOfferItemRepository
        : IEntityRepository<OfferItem>
    {
    }


    public interface IOfferRepository
        : IEntityRepository<Offer>
    {
    }


    public interface IOfferSubtotalRepository
        : IEntityRepository<OfferSubtotal>
    {
    }


    public interface IOfferTaskRepository
        : IEntityRepository<OfferTask>
    {
    }


    public interface IOrganizationCompanyProductRepository
        : IEntityRepository<OrganizationCompanyProduct>
    {
    }


    public interface IOrganizationCompanyWorkTypeRepository
        : IEntityRepository<OrganizationCompanyWorkType>
    {
    }


    public interface IOvertimePriceRepository
        : IEntityRepository<OverTimePrice>
    {
    }


    public interface IOvertimeRepository
        : IEntityRepository<OverTime>
    {
    }


    public interface IPricelistRepository
        : IEntityRepository<Pricelist>
    {
    }


    public interface IPricelistVersionRepository
        : IEntityRepository<PricelistVersion>
    {
    }


    public interface IProductCategoryRepository
        : IEntityRepository<ProductCategory>
    {
    }


    public interface IProductPriceRepository
        : IEntityRepository<ProductPrice>
    {
    }


    public interface IProductRepository
        : IEntityRepository<Product>
    {
    }


    public interface IProfileDashboardRepository
        : IEntityRepository<ProfileDashboard>
    {
    }


    public interface IProfileReportRepository
        : IEntityRepository<ProfileReport>
    {
    }


    public class ProfileReportRepository : OrganizationEntityRepository<ProfileReportFields, ProfileReport, IPsaContext>
        , IProfileReportRepository
    {
        public ProfileReportRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public interface IProfileRepository
        : IEntityRepository<Profile>
    {
    }


    public interface IProfileRightRepository
        : IEntityRepository<ProfileRight>
    {
    }


    public interface IProposalStatusRepository
        : IEntityRepository<ProposalStatus>
    {
    }


    public interface IPsaReportingRepository
    {
    }


    public class PsaReportingRepository : RepositoryBase<IPsaContext>
        , IPsaReportingRepository
    {
        public PsaReportingRepository(
            IPsaContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3,
            ICurrencyService arg4
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
            Field4 = arg4;
        }

        public readonly IPsaContextService Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
        public readonly ICurrencyService Field4;
    }


    public interface IQuickSearchRepository
    {
    }


    public class QuickSearchRepository : RepositoryBase
        , IQuickSearchRepository
    {
        public QuickSearchRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public interface IRecurringItemRepository
        : IEntityRepository<RecurringItem>
    {
    }


    public interface IReimbursedHourRepository
        : IEntityRepository<ReimbursedHour>
    {
    }


    public interface IReimbursedItemRepository
        : IEntityRepository<ReimbursedItem>
    {
    }


    public interface IReportRepository
        : IEntityRepository<Report>
    {
    }


    public interface IRequestLogEntryRepository
        : IEntityRepository<RequestLogEntry>
    {
    }


    public interface IResourceAllocationRepository
        : IEntityRepository<ResourceAllocation>
    {
    }


    public interface IResourceRepository
        : IEntityRepository<Resource>
    {
    }


    public interface ISalesAccountRepository
        : IEntityRepository<SalesAccount>
    {
    }


    public interface ISalesProcessRepository
        : IEntityRepository<SalesProcess>
    {
    }


    public interface ISalesStatusRepository
        : IEntityRepository<SalesStatus>
    {
    }


    public interface ISearchCriteriaRepository
        : IEntityRepository<Entities.SearchCriteria>
    {
    }


    public interface ISearchRepository
        : IEntityRepository<Entities.Search>
    {
    }


    public interface ITagRepository
        : IEntityRepository<Tag>
    {
    }


    public interface ITaskMemberRepository
        : IEntityRepository<TaskMember>
    {
    }


    public interface ITaskRepository
        : IEntityRepository<Task>
    {
    }


    public interface ITaskStatusCommentRepository
        : IEntityRepository<TaskStatusComment>
    {
    }


    public interface ITaskStatusRepository
        : IEntityRepository<TaskStatus>
    {
    }


    public interface ITaskStatusTypeRepository
        : IEntityRepository<TaskStatusType>
    {
    }


    public interface ITemporaryHourRepository
        : IEntityRepository<TemporaryHour>
    {
    }


    public interface ITemporaryItemRepository
        : IEntityRepository<TemporaryItem>
    {
    }


    public interface ITimecardEventRepository
        : IEntityRepository<TimecardEvent>
    {
    }


    public interface ITimeEntryRepository
        : IEntityRepository<TimeEntry>
    {
    }


    public interface ITimeEntrySuggestedRowRepository
        : IEntityRepository<TimeEntrySuggestedRow>
    {
    }


    public class TimeEntrySuggestedRowRepository : OrganizationEntityRepository<TimeEntrySuggestedRowFields,
            TimeEntrySuggestedRow, IPsaContext>
        , ITimeEntrySuggestedRowRepository
    {
        public TimeEntrySuggestedRowRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public interface ITimeEntryTypeRepository
        : IEntityRepository<TimeEntryType>
    {
    }


    public interface ITravelReimbursementRepository
        : IEntityRepository<TravelReimbursement>
    {
    }


    public interface ITravelReimbursementStatusRepository
        : IEntityRepository<TravelReimbursementStatus>
    {
    }


    public interface ITreeTaskRepository
        : IEntityRepository<TreeTask>
    {
    }


    public class TreeTaskRepository : OrganizationEntityRepository<TreeTaskFields, TreeTask, IPsaContext>
        , ITreeTaskRepository
    {
        public TreeTaskRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3,
            IAuthorization<IPsaContext, User> arg4
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
            Field4 = arg4;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
        public readonly IAuthorization<IPsaContext, User> Field4;
    }


    public interface IUsedScannerReceiptRepository
        : IEntityRepository<UsedScannerReceipt>
    {
    }


    public interface IUserCostPerCaseRepository
        : IEntityRepository<UserCostPerCase>
    {
    }


    public interface IUserLogEntryRepository
        : IEntityRepository<UserLogEntry>
    {
    }


    public interface IUserSettingsRepository
        : IEntityRepository<UserSettings>
    {
    }


    public interface IUserTagRepository
        : IEntityRepository<UserTag>
    {
    }


    public interface IUserTaskFavoriteRepository
        : IEntityRepository<UserTaskFavorite>
    {
    }


    public interface IUserWeeklyViewRowRepository
        : IEntityRepository<UserWeeklyViewRow>
    {
    }


    public interface IWorkdayRepository
        : IEntityRepository<Workday>
    {
    }


    public interface IWorkdaySummaryRepository
        : IEntityRepository<WorkdaySummary>
    {
    }


    public class WorkdaySummaryRepository :
        OrganizationEntityRepository<WorkdaySummaryFields, WorkdaySummary, IPsaContext>
        , IWorkdaySummaryRepository
    {
        public WorkdaySummaryRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public interface IWorkHourSuggestedRowRepository
        : IEntityRepository<WorkHourSuggestedRow>
    {
    }


    public class WorkHourSuggestedRowRepository :
        OrganizationEntityRepository<WorkHourSuggestedRowFields, WorkHourSuggestedRow, IPsaContext>
        , IWorkHourSuggestedRowRepository
    {
        public WorkHourSuggestedRowRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public interface IWorkingDayExceptionRepository
        : IEntityRepository<WorkingDayException>
    {
    }


    public class WorkingDayExceptionRepository :
        EntityRepository<WorkingDayExceptionFields, WorkingDayException, IPsaContext>
        , IWorkingDayExceptionRepository
    {
        public WorkingDayExceptionRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public interface IWorkPriceRepository
        : IEntityRepository<WorkPrice>
    {
    }


    public interface IWorkTypeRepository
        : IEntityRepository<WorkType>
    {
    }


    public interface IWorkweekRepository
        : IEntityRepository<Workweek>
    {
    }


    public class WorkweekRepository : EntityRepository<WorkweekFields, Workweek, IPsaContext>
        , IWorkweekRepository
    {
        public WorkweekRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class LogEntryRepositoryBase<TEntityFields, TEntity> : EntityRepository<TEntityFields, TEntity, IPsaContext>
        where TEntityFields : IIdentifiableEntity, new() where TEntity : TEntityFields, IIdentifiableEntityWithOriginalState<TEntityFields>, new()
    {
        public LogEntryRepositoryBase(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base(arg0, arg1, arg2)
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
        }

        public readonly IContextService<IPsaContext> Field0;
        public readonly IConfiguration Field1;
        public readonly DbProviderFactory Field2;
        public readonly ICustomerDatabaseRepository Field3;
    }


    public class AccountGroupIdSearchFieldDefinition : PsaIdSearchFieldDefinition<SearchCriteria>
        , IInnerSearchFieldDefinition
    {
        public AccountGroupIdSearchFieldDefinition(
            int? arg0,
            string arg1,
            string arg2,
            bool arg3,
            string arg4
        ) : base()
        {
        }
    }

    public class PsaIdSearchFieldDefinition<T>
    {
        public PsaIdSearchFieldDefinition()
        {
        }

        protected PsaIdSearchFieldDefinition(string arg0, string s, CheckUserRightsDelegate<IPsaContext> checkUserRightsDelegate)
        {
        }
    }


    public class AccountGroupSearchDefinition : PsaGenericSearchDefinition<AccountGroup>
    {
        public AccountGroupSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class AccountSearchDefinition : PsaSearchDefinitionBase<AccountSearchCriteria, Account>
    {
        public AccountSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, AccountSearchCriteria>> fields) : base(fields)
        {
        }

        public override SearchResponse<Account> GetEntities(IPsaContext context, ISearchRequest<AccountSearchCriteria> request) => throw new NotImplementedException();
    }


    public class ActivityGuidSearchFieldDefinition : IdSearchFieldDefinition<IPsaContext, SearchCriteria>
    {
        public ActivityGuidSearchFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            string arg3,
            string arg4,
            CheckUserRightsDelegate<IPsaContext> arg5,
            ISqlExpressionModifier<IPsaContext> arg6,
            SearchFieldOption arg7,
            int? arg8,
            string arg9
        ) : base()
        {
        }
    }

    public class IdSearchFieldDefinition<T1, T2>
    {
    }

    public class ActivitySearchDefinition : PsaSearchDefinitionBase<ActivitySearchCriteria>
    {
        public ActivitySearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, ActivitySearchCriteria>> fields) : base(fields)
        {
        }
    }


    public class ActivityTypeSearchDefinition : PsaGenericSearchDefinition<ActivityType>
    {
        public ActivityTypeSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class AddressSearchDefinition : PsaGenericSearchDefinition<Address>
    {
        public AddressSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ApiClientSearchDefinition : PsaGenericSearchDefinition<ApiClient>
    {
        public ApiClientSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class AuthorizedIpAddressSearchDefinition : PsaGenericSearchDefinition<AuthorizedIPAddress>
    {
        public AuthorizedIpAddressSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class BackgroundTaskSearchDefinition : PsaGenericSearchDefinition<BackgroundTask>
    {
        public BackgroundTaskSearchDefinition(IDatabaseConnionResolver<IPsaContext> arg0)
        {
        }

        public BackgroundTaskSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0,
            string arg1
        ) : base(arg0)
        {
        }
    }


    public class BankAccountSearchDefinition : PsaGenericSearchDefinition<BankAccount>
    {
        public BankAccountSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class BusinessUnitSearchDefinition : PsaSearchDefinitionBase<BusinessUnitSearchCriteria, BusinessUnit>
    {
        public BusinessUnitSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, BusinessUnitSearchCriteria>> fields) : base(fields)
        {
        }

        public override SearchResponse<BusinessUnit> GetEntities(IPsaContext context, ISearchRequest<BusinessUnitSearchCriteria> request) => throw new NotImplementedException();
    }


    public class CalendarGroupMemberSearchDefinition : PsaGenericSearchDefinition<Entities.SearchCriteria>
    {
        public CalendarGroupMemberSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class CalendarGroupSearchDefinition : PsaGenericSearchDefinition<Entities.Search>
    {
        public CalendarGroupSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class CaseFileSearchDefinition : PsaSearchDefinitionBase<CaseFileSearchCriteria>
    {
        public CaseFileSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, CaseFileSearchCriteria>> fields) : base(fields)
        {
        }
    }


    public class CaseRelationToUserFieldDefinition : ISqlSearchFieldDefinition, ISqlSearchFieldRegisterEntry
    {
        public CaseRelationToUserFieldDefinition(
            string arg0,
            bool arg1,
            string arg2
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
        }

        public readonly string Field0;
        public readonly bool Field1;
        public readonly string Field2;
    }

    internal interface ISqlSearchFieldDefinition
    {
    }

    public class CaseSearchDefinition : PsaSearchDefinitionBase<CaseSearchCriteria, Case>
    {
        public CaseSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, CaseSearchCriteria>> fields) : base(fields)
        {
        }

        public override SearchResponse<Case> GetEntities(IPsaContext context, ISearchRequest<CaseSearchCriteria> request) => throw new NotImplementedException();
    }


    public class CaseStatusTypeSearchDefinition : PsaGenericSearchDefinitionWithManageInfo<CaseStatusType>
    {
        public CaseStatusTypeSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }

    public class PsaGenericSearchDefinitionWithManageInfo<T>
    {
        private IDatabaseConnionResolver<IPsaContext> _arg0;

        public PsaGenericSearchDefinitionWithManageInfo(IDatabaseConnionResolver<IPsaContext> arg0)
        {
            _arg0 = arg0;
        }
    }


    public class CaseToInvoiceSearchDefinition : CaseSearchDefinition
    {
        public CaseToInvoiceSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, CaseSearchCriteria>> fields) : base(fields)
        {
        }
    }


    public class ContactRoleSearchDefinition : PsaGenericSearchDefinition<ContactRole>
    {
        public ContactRoleSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ContactSearchDefinition : PsaSearchDefinitionBase<ContactSearchCriteria, Contact>
    {
        public ContactSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, ContactSearchCriteria>> fields) : base(fields)
        {
        }

        public override SearchResponse<Contact> GetEntities(IPsaContext context, ISearchRequest<ContactSearchCriteria> request) => throw new NotImplementedException();
    }


    public class CustomFormulaSearchDefinition : PsaGenericSearchDefinition<CustomFormula>
    {
        public CustomFormulaSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class EmploymentSearchDefinition : PsaGenericSearchDefinition<Employment>
    {
        public EmploymentSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class HourSearchDefinition : PsaSearchDefinitionBase<HourSearchCriteria, Hour>
    {
        public HourSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, HourSearchCriteria>> fields) : base(fields)
        {
        }

        public override SearchResponse<Hour> GetEntities(IPsaContext context, ISearchRequest<HourSearchCriteria> request) => throw new NotImplementedException();
    }


    public class IndustrySearchDefinition : PsaGenericSearchDefinition<Industry>
    {
        public IndustrySearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class IntegrationErrorSearchDefinition : PsaGenericSearchDefinition<IntegrationError>
    {
        public IntegrationErrorSearchDefinition(IDatabaseConnionResolver<IPsaContext> arg0) : base()
        {
        }

        public IntegrationErrorSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0,
            string arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class InvoiceCaseSearchDefinition : PsaGenericSearchDefinition<InvoiceCase>
    {
        public InvoiceCaseSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class InvoiceRowSearchDefinition : PsaSearchDefinitionBase<InvoiceRowSearchCriteria, InvoiceRow>
    {
        public InvoiceRowSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, InvoiceRowSearchCriteria>> fields) : base(fields)
        {
        }

        public override SearchResponse<InvoiceRow> GetEntities(IPsaContext context, ISearchRequest<InvoiceRowSearchCriteria> request) => throw new NotImplementedException();
    }


    public class InvoiceSearchDefinition : PsaSearchDefinitionBase<InvoiceSearchCriteria, Invoice>
    {
        public InvoiceSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, InvoiceSearchCriteria>> fields) : base(fields)
        {
        }

        public override SearchResponse<Invoice> GetEntities(IPsaContext context, ISearchRequest<InvoiceSearchCriteria> request) => throw new NotImplementedException();
    }


    public class ItemSearchDefinition : PsaSearchDefinitionBase<ItemSearchCriteria, Item>
    {
        public ItemSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, ItemSearchCriteria>> fields) : base(fields)
        {
        }

        public override SearchResponse<Item> GetEntities(IPsaContext context, ISearchRequest<ItemSearchCriteria> request) => throw new NotImplementedException();
    }


    public class LeadSourceSearchDefinition : PsaGenericSearchDefinition<LeadSource>
    {
        public LeadSourceSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class OfferItemSearchDefinition : PsaGenericSearchDefinition<OfferItem>
    {
        public OfferItemSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class OfferRowSearchDefinition : PsaSearchDefinitionBase<CommonSearchCriteria>
    {
        public OfferRowSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, CommonSearchCriteria>> fields) : base(fields)
        {
        }
    }


    public class OfferSearchDefinition : PsaSearchDefinitionBase<OfferSearchCriteria, Offer>
    {
        public OfferSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, OfferSearchCriteria>> fields) : base(fields)
        {
        }

        public override SearchResponse<Offer> GetEntities(IPsaContext context, ISearchRequest<OfferSearchCriteria> request) => throw new NotImplementedException();
    }


    public class OfferSubtotalSearchDefinition : PsaGenericSearchDefinition<OfferSubtotal>
    {
        public OfferSubtotalSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class OfferTaskSearchDefinition : PsaGenericSearchDefinition<OfferTask>
    {
        public OfferTaskSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class OrganizationSearchDefinition : PsaGenericSearchDefinition<Organization>
    {
        public OrganizationSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ProductSearchDefinition : PsaGenericSearchDefinition<Product>
    {
        public ProductSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ProfileReportSearchDefinition : PsaGenericSearchDefinition<ProfileReport>
    {
        public ProfileReportSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ProfileSearchDefinition : PsaGenericSearchDefinitionWithManageInfo<Profile>
    {
        public ProfileSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class PsaSearchFactory : SearchFactory<IPsaContext>
    {
        public PsaSearchFactory(IContextService<TContext> arg0, IDatabaseConnionResolver<TContext> arg1) : base(arg0, arg1)
        {
        }
    }

    public class SearchFactory<T> : SearchFactory
    {
        public SearchFactory(IContextService<TContext> arg0, IDatabaseConnionResolver<TContext> arg1) : base(arg0, arg1)
        {
        }
    }

    public class PurchaseOrderItemSearchDefinition : PsaSearchDefinitionBase<PurchaseOrderItemSearchCriteria>
    {
        public PurchaseOrderItemSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, PurchaseOrderItemSearchCriteria>> fields) : base(fields)
        {
        }
    }


    public class ReportSearchDefinition : PsaGenericSearchDefinition<Report>
    {
        public ReportSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ResourceAllocationSearchDefinition : PsaSearchDefinitionBase<ResourceAllocationSearchCriteria>
    {
        public ResourceAllocationSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, ResourceAllocationSearchCriteria>> fields) : base(fields)
        {
        }
    }


    public class ResourceSearchDefinition : PsaGenericSearchDefinition<Resource>
    {
        public ResourceSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class SalesProcessIdFilterFieldDefinition : SearchFieldDefinition<IPsaContext, SearchCriteria>
    {
        public SalesProcessIdFilterFieldDefinition(
        ) : base()
        {
        }
    }


    public class SalesStatusTimeStampSearchFieldDefinition : SearchFieldDefinition<IPsaContext, SearchCriteria>
    {
        public SalesStatusTimeStampSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class ScheduledWorkErrorSearchDefinition : IntegrationErrorSearchDefinition
    {
        public ScheduledWorkErrorSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ScheduledWorkTaskSearchDefinition : BackgroundTaskSearchDefinition
    {
        public ScheduledWorkTaskSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class TagIdSearchFieldDefinition : PsaIdSearchFieldDefinition<SearchCriteria>
    {
        public TagIdSearchFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            string arg3,
            string arg4,
            CheckUserRightsDelegate<IPsaContext> arg5
        ) : base()
        {
        }
    }


    public class TagKeywordSearchFieldDefinition : SearchFieldDefinition<IPsaContext, SearchCriteria>
    {
        public TagKeywordSearchFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            string arg3,
            CheckUserRightsDelegate<IPsaContext> arg4
        ) : base()
        {
        }
    }


    public class TaskSearchDefinition : PsaSearchDefinitionBase<TaskSearchCriteria, Task>
    {
        public TaskSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, TaskSearchCriteria>> fields) : base(fields)
        {
        }

        public override SearchResponse<Task> GetEntities(IPsaContext context, ISearchRequest<TaskSearchCriteria> request) => throw new NotImplementedException();
    }


    public class TaskStatusTypeSearchDefinition : PsaGenericSearchDefinitionWithManageInfo<TaskStatusType>
    {
        public TaskStatusTypeSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class TermsOfServiceApprovalSearchDefinition : PsaGenericSearchDefinition<TermsOfServiceApproval>
    {
        public TermsOfServiceApprovalSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class TimeEntrySearchDefinition : PsaSearchDefinitionBase<TimeEntrySearchCriteria, TimeEntry>
    {
        public TimeEntrySearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, TimeEntrySearchCriteria>> fields) : base(fields)
        {
        }

        public override SearchResponse<TimeEntry> GetEntities(IPsaContext context, ISearchRequest<TimeEntrySearchCriteria> request) => throw new NotImplementedException();
    }


    public class TravelReimbursementAccountIdSearchFieldDefinition : PsaIdSearchFieldDefinition<SearchCriteria>
    {
        public TravelReimbursementAccountIdSearchFieldDefinition(
            string arg0,
            string arg1,
            CheckUserRightsDelegate<IPsaContext> arg2
        ) : base()
        {
        }
    }


    public class TravelReimbursementCaseIdSearchFieldDefinition : PsaIdSearchFieldDefinition<SearchCriteria>
    {
        public TravelReimbursementCaseIdSearchFieldDefinition(
            string arg0,
            string arg1,
            CheckUserRightsDelegate<IPsaContext> arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class TravelReimbursementRelationToUserFieldDefinition
        : ISqlSearchFieldDefinition, ISqlSearchFieldRegisterEntry
    {
        public TravelReimbursementRelationToUserFieldDefinition(
            string arg0
        )
        {
            Field0 = arg0;
        }

        public readonly string Field0;
    }


    public class TravelReimbursementSearchDefinition : PsaSearchDefinitionBase<TravelReimbursementSearchCriteria>
    {
        public TravelReimbursementSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, TravelReimbursementSearchCriteria>> fields) : base(fields)
        {
        }
    }


    public class TreeRootTaskIdSearchFieldDefinition : PsaIdSearchFieldDefinition<SearchCriteria>
    {
        public TreeRootTaskIdSearchFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            CheckUserRightsDelegate<IPsaContext> arg3,
            ISqlExpressionModifier<IPsaContext> arg4
        ) : base()
        {
        }
    }


    public class UserCostPerCaseSearchDefinition : PsaGenericSearchDefinition<UserCostPerCase>
    {
        public UserCostPerCaseSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class UserIdSearchFieldDefinition : PsaIdSearchFieldDefinition<SearchCriteria>
    {
        public UserIdSearchFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            string arg3,
            string arg4,
            CheckUserRightsDelegate<IPsaContext> arg5,
            ISqlExpressionModifier<IPsaContext> arg6,
            SearchFieldOption arg7
        ) : base()
        {
        }
    }


    public partial class UserRelationToUserFieldDefinition
    {
        public UserRelationToUserFieldDefinition(
        )
        {
        }
    }


    public partial class UserRelationToUserFieldDefinition : ISqlSearchFieldDefinition, ISqlSearchFieldRegisterEntry
    {
        public UserRelationToUserFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            string arg3,
            bool arg4
        ) : base()
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
            Field3 = arg3;
            Field4 = arg4;
        }

        public readonly string Field0;
        public readonly string Field1;
        public readonly string Field2;
        public readonly string Field3;
        public readonly bool Field4;
    }


    public class UserSearchDefinition : PsaSearchDefinitionBase<UserSearchCriteria, User>
    {
        public UserSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, UserSearchCriteria>> fields) : base(fields)
        {
        }

        public override SearchResponse<User> GetEntities(IPsaContext context, ISearchRequest<UserSearchCriteria> request) => throw new NotImplementedException();
    }


    public class UserSettingsSearchDefinition : PsaGenericSearchDefinition<UserSettings>
    {
        public UserSettingsSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class WorkdaySearchDefinition : PsaGenericSearchDefinition<Workday>
    {
        public WorkdaySearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class WorkHourRelationToUserFieldDefinition
        : ISqlSearchFieldDefinition, ISqlSearchFieldRegisterEntry
    {
        public WorkHourRelationToUserFieldDefinition(
            string arg0,
            bool arg1
        )
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly string Field0;
        public readonly bool Field1;
    }


    public class WorkTypeSearchDefinition : PsaGenericSearchDefinition<WorkType>
    {
        public WorkTypeSearchDefinition(
            IDatabaseConnionResolver<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public partial class AccountGroups : CustomFormulaStringPart<IPsaContext, AccountGroups>
    {
        public AccountGroups(
            AccountGroups arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }

    public class CustomFormulaStringPart<T1, T2>
    {
        public CustomFormulaStringPart(string arg1)
        {
        }
    }

    public partial class CaseCount : PsaCustomFormulaDecimalPart<AccountFormulaHandler, CaseCount>
    {
        public CaseCount(
            CaseCount arg0,
            string arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class Cases : CustomFormulaEntityAccessInfoPart<IPsaContext, Cases, CaseAccessInfo>
    {
        public Cases(
            Cases arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }

    public class CaseAccessInfo : IEntityAccessInfo<IPsaContext>
    {
    }


    public class IsProspect : PsaCustomFormulaDecimalPart<AccountFormulaHandler, IsProspect>
    {
        public IsProspect(
            IsProspect arg0,
            string arg1,
            AggregateFunction arg2
        ) : base(arg1)
        {
        }
    }

    public class BillingForecast : PsaCustomFormulaDecimalPart<CaseFormulaHandler, BillingForecast>
    {

        public BillingForecast(
            BillingForecast arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class CaseTags : CustomFormulaStringPart<IPsaContext, CaseTags>
    {
        public CaseTags(
            CaseTags arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class ContactEmail : CustomFormulaStringPart<IPsaContext, ContactEmail>
    {
        public ContactEmail(
            ContactEmail arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class ContactPhoneNumber : CustomFormulaStringPart<IPsaContext, ContactPhoneNumber>
    {
        public ContactPhoneNumber(
            ContactPhoneNumber arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }

    public class ExpectedValue : PsaCustomFormulaDecimalPart<CaseFormulaHandler, ExpectedValue>
    {
        public ExpectedValue(
            ExpectedValue arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class ExpenseForecast : PsaCustomFormulaDecimalPart<CaseFormulaHandler, ExpenseForecast>
    {
        public ExpenseForecast(
            ExpenseForecast arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class LaborExpenseForecast : PsaCustomFormulaDecimalPart<CaseFormulaHandler, LaborExpenseForecast>
    {
        public LaborExpenseForecast(
            LaborExpenseForecast arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class LateActivityCount : PsaCustomFormulaDecimalPart<CaseFormulaHandler, ActivityCount>
    {
        public LateActivityCount(
            ActivityCount arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class ProductPrice : ProductPriceFields, IEntity, IIdentifiableEntityWithOriginalState<ProductPriceFields>
    {
        public ProductPrice(
            ProductPrice arg0,
            string arg1
        ) : base()
        {
        }
    }


    public class ProductQuantity : PsaCustomFormulaDecimalPart<CaseFormulaHandler, Entities.ProductQuantity>
    {
        public ProductQuantity(
            Entities.ProductQuantity arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class Revenue : PsaCustomFormulaDecimalPart<CaseFormulaHandler, Entities.Revenue>
    {
        public Revenue(
            Entities.Revenue arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class RevenueForecast : PsaCustomFormulaDecimalPart<CaseFormulaHandler, RevenueForecast>
    {
        public RevenueForecast(
            RevenueForecast arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class SalesCaseCount : PsaCustomFormulaDecimalPart<CaseFormulaHandler, SalesCaseCount>
    {
        public SalesCaseCount(
        ) : base()
        {
        }

        public SalesCaseCount(
            SalesCaseCount arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class SalesMargin : PsaCustomFormulaDecimalPart<CaseFormulaHandler, Entities.SalesMargin>
    {
        public SalesMargin(
        ) : base()
        {
        }

        public SalesMargin(
            Entities.SalesMargin arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class SalesProgressExistence : PsaCustomFormulaDecimalPart<CaseFormulaHandler, Entities.SalesProgressExistence>
    {
        public SalesProgressExistence(
            Entities.SalesProgressExistence arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class SalesValue : PsaCustomFormulaDecimalPart<CaseFormulaHandler, SalesValue>
    {
        public SalesValue(
            SalesValue arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class SalesValueForecast : PsaCustomFormulaDecimalPart<CaseFormulaHandler, SalesValueForecast>
    {
        public SalesValueForecast(
            SalesValueForecast arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class TodayActivityCount : PsaCustomFormulaDecimalPart<CaseFormulaHandler, ActivityCount>
    {
        public TodayActivityCount(
            ActivityCount arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class Unbilled : PsaCustomFormulaDecimalPart<CaseFormulaHandler, Entities.Unbilled>
    {
        public Unbilled(
            Entities.Unbilled arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class WorkEstimate : PsaCustomFormulaDecimalPart<CaseFormulaHandler, Entities.WorkEstimate>
    {
        public WorkEstimate(
            Entities.WorkEstimate arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class WorkHoursBilled : PsaCustomFormulaDecimalPart<CaseFormulaHandler, Entities.WorkHoursBilled>
    {
        public WorkHoursBilled(
            Entities.WorkHoursBilled arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }

    public abstract class CustomFormulaEntityAccessInfoPart<TContext, TParameter, TEntity> : CustomFormulaXmlPart<TContext, TParameter>
        where TContext : IContext
        where TEntity : IEntityAccessInfo<TContext>
    {
        public CustomFormulaEntityAccessInfoPart(string description) : base()
        { }

        public CustomFormulaEntityAccessInfoPart(Phrase description, TParameter parameter, string sqlSynonym = null) : base(description, sqlSynonym)
        {
        }

        protected CustomFormulaEntityAccessInfoPart()
        {
        }
    }


    public class CommunicationMethods : CustomFormulaEntityAccessInfoPart<IPsaContext, CommunicationMethods,
        CommunicationMethodAccessInfo>
    {
        public CommunicationMethods(
        ) : base()
        {
        }

        public CommunicationMethods(
            CommunicationMethods arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class ShareOfBilling : TeamProductivityPart<ShareOfBilling>, ICustomFormulaNumericPartParameter
    {
        public ShareOfBilling(
            ShareOfBilling arg0,
            string arg1,
            TeamProductivitySettings arg2
        ) : base(arg1)
        {
        }
    }


    public class ShareOfBillingByCost : PsaCustomFormulaDecimalPart<HourFormulaHandler, ShareOfBillingByCost>
    {
        public ShareOfBillingByCost(
        ) : base()
        {
        }

        public ShareOfBillingByCost(
            ShareOfBillingByCost arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class
        ShareOfBillingByQuantityOfHours : PsaCustomFormulaDecimalPart<HourFormulaHandler,
            Entities.ShareOfBillingByQuantityOfHours>
    {
        public ShareOfBillingByQuantityOfHours(
        ) : base()
        {
        }

        public ShareOfBillingByQuantityOfHours(
            Entities.ShareOfBillingByQuantityOfHours arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class ShareOfBillingByValue : PsaCustomFormulaDecimalPart<HourFormulaHandler, Entities.ShareOfBillingByValue>
    {
        public ShareOfBillingByValue(
        ) : base()
        {
        }

        public ShareOfBillingByValue(
            Entities.ShareOfBillingByValue arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class ShareOfMargin : TeamProductivityPart<ShareOfMargin>, ICustomFormulaNumericPartParameter
    {
        public ShareOfMargin(
            ShareOfMargin arg0,
            string arg1,
            TeamProductivitySettings arg2
        ) : base(arg1)
        {
        }
    }


    public class ShareOfSalesMargin : TeamProductivityPart<ShareOfSalesMargin>, ICustomFormulaNumericPartParameter
    {
        public ShareOfSalesMargin(
            ShareOfSalesMargin arg0,
            string arg1,
            TeamProductivitySettings arg2
        ) : base(arg1)
        {
        }
    }


    public abstract class TeamProductivityPart<TParameter> : PsaCustomFormulaDecimalPart<HourFormulaHandler, TParameter> where TParameter : class, ICustomFormulaNumericPartParameter
    {
        private TeamProductivitySettings _teamProductivitySettings;
        private string _arg1;

        public TeamProductivityPart(Phrase description, TParameter parameter, string sqlSynonym = null, TeamProductivitySettings teamProductivitySettings = null) : base(description, parameter, sqlSynonym)
        {
            _teamProductivitySettings = teamProductivitySettings;
        }

        protected TeamProductivityPart(string arg1)
        {
            _arg1 = arg1;
        }
    }



    public class UnitCost : PsaCustomFormulaDecimalPart<HourFormulaHandler, UnitCost>
    {
        public UnitCost(
        ) : base()
        {
        }

        public UnitCost(
            UnitCost arg0,
            string arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class UnitPrice : PsaCustomFormulaDecimalPart<HourFormulaHandler, UnitPrice>
    {
        public UnitPrice(
        ) : base()
        {
        }

        public UnitPrice(
            UnitPrice arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class AccessRights
    {
        public AccessRights(
        )
        {
        }
    }


    public partial class WorkHours : PsaCustomFormulaDecimalPart<CaseFormulaHandler, Entities.WorkHours>
    {
        public WorkHours(
        ) : base()
        {
        }

        public WorkHours(
            Entities.WorkHours arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class WorkHoursBillable : PsaCustomFormulaDecimalPart<HourFormulaHandler, WorkHoursBillable>
    {
        public WorkHoursBillable(
        ) : base()
        {
        }

        public WorkHoursBillable(
            WorkHoursBillable arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public partial class WorkHoursValue : PsaCustomFormulaDecimalPart<HourFormulaHandler, Entities.WorkHoursValue>
    {
        public WorkHoursValue(
        ) : base()
        {
        }

        public WorkHoursValue(
            Entities.WorkHoursValue arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class Invoicing : PsaCustomFormulaDecimalPart<InvoiceFormulaHandler, Entities.Invoicing>
    {
        public Invoicing(
        ) : base()
        {
        }

        public Invoicing(
            Entities.Invoicing arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public partial class Margin : PsaCustomFormulaDecimalPart<InvoiceFormulaHandler, Entities.Margin>
    {
        public Margin(
        ) : base()
        {
        }

        public Margin(
            Entities.Margin arg0,
            string arg1
        ) : base(arg1)
        {
        }
    }


    public class ReadTimelineDetailParameter
    {
        public ReadTimelineDetailParameter(
            TimelineSearchCriteria arg0,
            DateTime? arg1,
            int? arg2
        )
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
        }

        public readonly TimelineSearchCriteria Field0;
        public readonly DateTime? Field1;
        public readonly int? Field2;
    }


    public class AccountChangedSinceOptions
    {
    }


    public class AccountChangedSinceCriteria
    {
        public AccountChangedSinceCriteria(
            DateTime arg0,
            AccountChangedSinceOptions arg1
        )
        {
            Field0 = arg0;
            Field1 = arg1;
        }

        public readonly DateTime Field0;
        public readonly AccountChangedSinceOptions Field1;
    }


    public class LanguageTable
        : IJoinedTable
    {
        public LanguageTable(
        )
        {
        }
    }


    public class AccountCountrySettingsTable
        : IJoinedTable
    {
        public AccountCountrySettingsTable(
        )
        {
        }
    }


    public class SearchedTextExSearchFieldDefinition : SearchFieldDefinition<IPsaContext, AccountSearchCriteria>
    {
        public SearchedTextExSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public partial class PostalCodeFieldDefinition : SearchFieldDefinition<IPsaContext, SearchCriteria>
    {
        public PostalCodeFieldDefinition(
        ) : base()
        {
        }
    }


    public partial class OpenCasesSearchFieldDefinition : EntityAccessInfoListFieldDefinition<IPsaContext, AccountSearchCriteria, CaseAccessInfo>
    {
    }


    public partial class ActivitySearchFieldDefinition : EntityAccessInfoListFieldDefinition<IPsaContext, AccountSearchCriteria, ActivityAccessInfo>
    {
        public ActivitySearchFieldDefinition() : base()
        {
        }
    }

    public class ActivityAccessInfo : IEntityAccessInfo<IPsaContext>
    {
    }


    public partial class ActivityExistanceSearchFieldDefinition : SearchFieldDefinition<IPsaContext, AccountSearchCriteria>
    {
        public ActivityExistanceSearchFieldDefinition(
            string arg0
        ) : base()
        {
        }
    }


    public class ProjectTaskCountSearchFieldDefinition : SearchFieldDefinition<IPsaContext, AccountSearchCriteria>
    {
        public ProjectTaskCountSearchFieldDefinition(
            string arg0,
            bool? arg1
        ) : base(arg1)
        {
        }
    }


    public class NextOpenActivityDateSearchFieldDefinition : SearchFieldDefinition<IPsaContext, AccountSearchCriteria>
    {
        public NextOpenActivityDateSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class SalesCaseCountSearchFieldDefinition : SearchFieldDefinition<IPsaContext, AccountSearchCriteria>
    {
        public SalesCaseCountSearchFieldDefinition(
            string arg0,
            bool? arg1,
            string arg2
        ) : base()
        {
        }
    }


    public class AccountChangedExSearchFieldDefinition : SearchFieldDefinition<IPsaContext, AccountSearchCriteria>
    {
        public AccountChangedExSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class ActivityStatusTranslatedSearchFieldDefinition : ActivitySearchFieldDefinition
    {
        public ActivityStatusTranslatedSearchFieldDefinition(
        )
        {
        }
    }


    public class ContactPhoneSearchFieldDefinition : ActivitySearchFieldDefinition
    {
        public ContactPhoneSearchFieldDefinition(
        )
        {
        }
    }


    public class ActivityTypeSearchFieldDefinition : ActivitySearchFieldDefinition
    {
        public ActivityTypeSearchFieldDefinition(
        )
        {
        }
    }


    public class ActivityNameSearchFieldDefinition : ActivitySearchFieldDefinition
    {
        public ActivityNameSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class ActivityNotesSearchFieldDefinition : ActivitySearchFieldDefinition
    {
        public ActivityNotesSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class ActivityLocationSearchFieldDefinition : ActivitySearchFieldDefinition
    {
        public ActivityLocationSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class ActivityTypeNameSearchFieldDefinition : ActivitySearchFieldDefinition
    {
        public ActivityTypeNameSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public partial class ActivitySearchFieldDefinition : EntityAccessInfoListFieldDefinition<IPsaContext, AccountSearchCriteria, ActivityAccessInfo>
    {
        public ActivitySearchFieldDefinition(
            string arg0,
            SearchFieldDataType arg1,
            string arg2,
            string arg3,
            string arg4,
            CheckUserRightsDelegate<IPsaContext> arg5,
            ISqlExpressionModifier<IPsaContext> arg6,
            SearchFieldOption arg7,
            SearchCriteriaComparison arg8,
            AggregateFunction arg9
        ) : base()
        {
        }
    }


    public class ParticipantIdSearchFieldDefinition : UserIdSearchFieldDefinition<ActivitySearchCriteria>
    {
        public ParticipantIdSearchFieldDefinition(
        )
        {
        }
    }


    public class OverlappedPeriodWithoutEndDateChangeSearchFieldDefinition : OverlappedPeriodSearchFieldDefinition
    {
        public OverlappedPeriodWithoutEndDateChangeSearchFieldDefinition(
        )
        {
        }
    }

    public class OverlappedPeriodSearchFieldDefinition
    {
    }


    public class ContactIdSearchFieldDefinition : UserIdSearchFieldDefinition<ActivitySearchCriteria>
    {
        public ContactIdSearchFieldDefinition(
        )
        {
        }
    }


    public class ResourceIdSearchFieldDefinition : UserIdSearchFieldDefinition<ActivitySearchCriteria>
    {
        public ResourceIdSearchFieldDefinition(
        )
        {
        }
    }

    public class UserIdSearchFieldDefinition<T>
    {
    }

    public class
        TaskIdIncludingChildTasksSearchFieldDefinition : SearchFieldDefinition<IPsaContext, ActivitySearchCriteria>
    {
        public TaskIdIncludingChildTasksSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class CalendarGroupSearchFieldDefinition : SearchFieldDefinition<IPsaContext, ActivitySearchCriteria>
    {
        public CalendarGroupSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class CountryRegionIdOrNameSearchFieldDefinition : SearchFieldDefinition<IPsaContext, CommonSearchCriteria>
    {
        public CountryRegionIdOrNameSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public partial class SearchedTextSearchFieldDefinition : SearchFieldDefinition<IPsaContext, BusinessUnitSearchCriteria>
    {
    }


    public partial class PostalCodeFieldDefinition : SearchFieldDefinition<IPsaContext, SearchCriteria>
    {
    }


    public partial class OpenCasesSearchFieldDefinition : EntityAccessInfoListFieldDefinition<IPsaContext, AccountSearchCriteria, CaseAccessInfo>
    {
        public OpenCasesSearchFieldDefinition(
        ) : base()
        {
        }
    }

    public class FileTypeSearchFieldDefinition : SearchFieldDefinition<IPsaContext, CaseFileSearchCriteria>
    {
        public FileTypeSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class SalesProcessStatusCriteria
    {
        public SalesProcessStatusCriteria(
        )
        {
        }
    }


    public class InvoiceTemplateIdSearchFieldDefinition : PsaIdSearchFieldDefinition<CaseSearchCriteria>
    {
        public InvoiceTemplateIdSearchFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            CheckUserRightsDelegate<IPsaContext> arg3,
            ISqlExpressionModifier<IPsaContext> arg4,
            SearchFieldOption arg5
        ) : base()
        {
        }
    }


    public class InvoiceTemplateSearchFieldDefinition : SearchFieldDefinition<IPsaContext, CaseSearchCriteria>
        , IInnerSearchFieldDefinition
    {
        public InvoiceTemplateSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class CaseExtendedNameSearchFieldDefinition : SearchFieldDefinition<IPsaContext, SearchCriteria>
        , IInnerSearchFieldDefinition
    {
        public CaseExtendedNameSearchFieldDefinition(
            string arg0
        ) : base()
        {
        }
    }


    public class DefaultWorkTypeSearchFieldDefinition : SearchFieldDefinition<IPsaContext, CaseSearchCriteria>
        , IInnerSearchFieldDefinition
    {
        public DefaultWorkTypeSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class DefaultWorkTypeIdSearchFieldDefinition : SearchFieldDefinition<IPsaContext, CaseSearchCriteria>
        , IInnerSearchFieldDefinition
    {
        public DefaultWorkTypeIdSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class PricelistIdSearchFieldDefinition : PsaIdSearchFieldDefinition<CaseSearchCriteria>
    {
        public PricelistIdSearchFieldDefinition(
            string arg0,
            string arg1,
            string arg2,
            CheckUserRightsDelegate<IPsaContext> arg3,
            ISqlExpressionModifier<IPsaContext> arg4,
            SearchFieldOption arg5
        ) : base()
        {
        }
    }


    public partial class ActivitySearchFieldDefinition : EntityAccessInfoListFieldDefinition<IPsaContext, AccountSearchCriteria, ActivityAccessInfo>
    {
        public ActivitySearchFieldDefinition(
            string arg0
        ) : base(arg0)
        {
        }
    }


    public class TaskIdSearchFieldDefinition : SearchFieldDefinition<IPsaContext, CaseSearchCriteria>
    {
        public TaskIdSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class
        SalesCaseInNeedOfAttentionSearchFieldDefinition : SearchFieldDefinition<IPsaContext, CaseSearchCriteria>
    {
        public SalesCaseInNeedOfAttentionSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class RecurringItemExistenceSearchFieldDefinition : SearchFieldDefinition<IPsaContext, CaseSearchCriteria>
    {
        public RecurringItemExistenceSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class CanEditSearchFieldDefinition : SearchFieldDefinition<IPsaContext, CaseSearchCriteria>
    {
        public CanEditSearchFieldDefinition(
        ) : base()
        {
        }
    }


    public class SalesProcessStatusFilterFieldDefinition : SearchFieldDefinition<IPsaContext, CaseSearchCriteria>
    {
        public SalesProcessStatusFilterFieldDefinition(
            string arg0
        ) : base()
        {
        }
    }


    public class UserWorkHourEntryValue
    {
        public UserWorkHourEntryValue(
            int arg0,
            DateTime? arg1,
            DateTime? arg2
        )
        {
            Field0 = arg0;
            Field1 = arg1;
            Field2 = arg2;
        }

        public readonly int Field0;
        public readonly DateTime? Field1;
        public readonly DateTime? Field2;
    }


    public class UserWorkHourEntrySearchFieldDefinition : SearchFieldDefinition<IPsaContext, TaskSearchCriteria>
    {
        public UserWorkHourEntrySearchFieldDefinition(
        ) : base()
        {
        }
    }
}