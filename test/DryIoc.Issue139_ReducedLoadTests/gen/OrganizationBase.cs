using System.Data.Common;
using Framework;
using Logic;
using Organizations;
using Shared;
using IUser = Framework.IUser;
using IUserRepository = CUsers.Domain.IUserRepository;

namespace OrganizationBase
{
    public class AccessRightEntity
        : IAccessRightEntity
    {
        public AccessRightEntity(
            string arg0,
            string arg1,
            string arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public AccessRightEntity(
            string arg0,
            string arg1,
            string arg2,
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
        public readonly string field2;
        public readonly string field3;
    }


    public class CaseRelationToUser
    {
    }


    public interface IAccessRightEntity
    {
    }


    public interface IOrganizationContextBase
        : IContext, ISharedContext
    {
    }


    public interface IOrganizationUser
        : IOrganizationUserBase
    {
    }


    public interface IOrganizationUserBase
        : IUser
    {
    }


    public interface IUserRights
    {
    }


    public class UserRights
        : IUserRights
    {
        public UserRights(
        )
        {
        }
    }


    public class TravelReimbursementRelationToUser
    {
    }


    public class UserRelationToUser
    {
    }


    public class SpecialRights
    {
    }


    public class AccountRight
    {
    }


    public class CaseRight
    {
    }


    public class CaseJoinRight
    {
    }


    public class UsersRight
    {
    }


    public class AdministratorRight
    {
    }


    public class WorkHourApprovalRight
    {
    }


    public class TravelReimbursementRight
    {
    }


    public class InvoiceStatusRight
    {
    }


    public class ScheduleJobsRight
    {
    }


    public class WorkHourRelationToUser
    {
    }


    public class Address : AddressFields
        , IIdentifiableEntityWithOriginalState<AddressFields>
    {
        public Address(
        )
        {
        }
    }


    public class AddressFormat
    {
    }


    public class AddressEx : Address
        , INamedEntity
    {
        public AddressEx(
        )
        {
        }
    }


    public class AuditTrailEntry : AuditTrailEntryFields
        , IIdentifiableEntityWithOriginalState<AuditTrailEntryFields>
    {
        public AuditTrailEntry(
        )
        {
        }
    }


    public class BillingAddress : BillingAddressFields
        , IIdentifiableEntityWithOriginalState<BillingAddressFields>
    {
        public BillingAddress(
        )
        {
        }
    }


    public class Currency : CurrencyFields
        , IIdentifiableEntityWithOriginalState<CurrencyFields>
    {
        public Currency(
        )
        {
        }
    }


    public class CurrencyEx : Currency
        , INamedEntity, ICurrency
    {
        public CurrencyEx(
        )
        {
        }
    }


    public class AuditTrailEntryFields : OrganizationEntity
    {
        public AuditTrailEntryFields(
        )
        {
        }
    }


    public class AddressFields : OrganizationEntity
    {
        public AddressFields(
        )
        {
        }
    }


    public class CurrencyFields : OrganizationEntity
    {
        public CurrencyFields(
        )
        {
        }
    }


    public class SettingsFields : OrganizationEntity
    {
        public SettingsFields(
        )
        {
        }
    }


    public class Settings : SettingsFields
        , IIdentifiableEntityWithOriginalState<SettingsFields>
    {
        public Settings(
        )
        {
        }
    }


    public class BillingAddressFields : OrganizationEntity
    {
        public BillingAddressFields(
        )
        {
        }
    }


    public class EInvoiceBillingFields : OrganizationEntity
    {
        public EInvoiceBillingFields(
        )
        {
        }
    }


    public class EInvoiceBilling : EInvoiceBillingFields
        , IIdentifiableEntityWithOriginalState<EInvoiceBillingFields>
    {
        public EInvoiceBilling(
        )
        {
        }
    }


    public class OrganizationFields : IdentifiableEntity
    {
        public OrganizationFields(
        )
        {
        }
    }


    public class InvoicingContact
    {
        public InvoicingContact(
        )
        {
        }
    }


    public interface IOrganizationEntity
        : IIdentifiableEntity
    {
    }


    public class MasterFile
        : IIdentifiableEntity
    {
        public MasterFile(
        )
        {
        }
    }


    public class MasterFileData
        : IIdentifiableEntity
    {
        public MasterFileData(
        )
        {
        }
    }


    public class OrganizationEntity : IdentifiableEntity
        , IOrganizationEntity
    {
    }


    public class GetSettingOptions
    {
    }


    public class SettingsHelper
    {
        public SettingsHelper(
        )
        {
        }
    }


    public interface IMasterFileDataRepository
    {
    }


    public class MasterFileDataRepository : RepositoryBase
        , IMasterFileDataRepository
    {
        public MasterFileDataRepository(
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


    public interface IMasterFileRepository
    {
    }


    public class MasterFileRepository : RepositoryBase
        , IMasterFileRepository
    {
        public MasterFileRepository(
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


    public interface IRepository<TOrganization, TEntity> : IWritableRepository<TEntity>
        where TOrganization : IIdentifiableEntity
        where TEntity : IOrganizationEntity
    {
    }


    public abstract class
        MasterDatabaseRepository<TOrganization, TEntityFields, TEntity> : WritableRepository<TEntityFields, TEntity>,
            IRepository<TOrganization, TEntity>
        where TOrganization : IIdentifiableEntity
        where TEntityFields : IOrganizationEntity, new()
        where TEntity : TEntityFields, IIdentifiableEntityWithOriginalState<TEntityFields>, new()
    {
        protected MasterDatabaseRepository(IContextService contextService, IConfiguration configuration,
            DbProviderFactory dbProviderFactory) : base(contextService, configuration, dbProviderFactory)
        {
        }
    }


    public interface IInvoicingContactService
    {
    }


    public class InvoicingContactService
        : IInvoicingContactService
    {
        public InvoicingContactService(
            IUserRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IUserRepository field0;
    }


    public interface IMasterFileService
    {
    }


    public class MasterFileService
        : IMasterFileService
    {
        public MasterFileService(
            IMasterFileRepository arg0,
            IMasterFileDataRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IMasterFileRepository field0;
        public readonly IMasterFileDataRepository field1;
    }


    public interface IOrganizationNameChangedMailBuilderService
    {
    }


    public class OrganizationNameChangedMailBuilderService
        : IOrganizationNameChangedMailBuilderService
    {
        public OrganizationNameChangedMailBuilderService(
            IContextService arg0,
            IAppSettings arg1,
            IResellerService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService field0;
        public readonly IAppSettings field1;
        public readonly IResellerService field2;
    }


    public interface IOrganizationNameChangeService
    {
    }


    public class OrganizationNameChangeService
        : IOrganizationNameChangeService
    {
        public OrganizationNameChangeService(
            IOrganizationNameChangedMailBuilderService arg0,
            IMailClient arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IOrganizationNameChangedMailBuilderService field0;
        public readonly IMailClient field1;
    }


    public interface IOrganizationService
    {
    }


    public class OrganizationService
        : IOrganizationService
    {
        public OrganizationService(
            IMasterOrganizationRepository arg0,
            ITimeService arg1,
            ITimeZoneService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IMasterOrganizationRepository field0;
        public readonly ITimeService field1;
        public readonly ITimeZoneService field2;
    }


    public interface IOrganizationVatChangedMailBuilderService
    {
    }


    public class OrganizationVatChangedMailBuilderService
        : IOrganizationVatChangedMailBuilderService
    {
        public OrganizationVatChangedMailBuilderService(
            IAppSettings arg0,
            IResellerService arg1,
            IInvoicingContactService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IAppSettings field0;
        public readonly IResellerService field1;
        public readonly IInvoicingContactService field2;
    }


    public interface IResellerService
    {
    }


    public class ResellerService
        : IResellerService
    {
        public ResellerService(
            IPartnerService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IPartnerService field0;
    }


    public abstract class
        OrganizationServiceBase<TContext, TOrganization, TRepository> : EntityServiceBase<TContext, TOrganization, TRepository>
        where TContext : IContext
        where TRepository : IEntityRepository<TOrganization>
        where TOrganization : class, IMasterOrganization
    {
        protected readonly Organizations.IOrganizationService OrganizationService;
        private readonly IOrganizationVatChangeService _VatChangeService;
        private readonly ICustomerDatabaseRepository _CustomerDatabaseRepository;
        private readonly IOrganizationNameChangeService _NameChangeService;

        protected OrganizationServiceBase(IContextService<TContext> contextService, TRepository repository,
            IValidator<TOrganization> validator, IAuthorization<TContext, TOrganization> authorization,
            Organizations.IOrganizationService organizationService, IOrganizationVatChangeService vatChangeService,
            IOrganizationNameChangeService nameChangeService, ICustomerDatabaseRepository customerDatabaseRepository) :
            base(contextService, repository, validator, authorization)
        {
            OrganizationService = organizationService;
            _VatChangeService = vatChangeService;
            _NameChangeService = nameChangeService;
            _CustomerDatabaseRepository = customerDatabaseRepository;
        }
    }


    public class OrganizationVatChangeService
        : IOrganizationVatChangeService
    {
        public OrganizationVatChangeService(
            IOrganizationVatChangedMailBuilderService arg0,
            IMailClient arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IOrganizationVatChangedMailBuilderService field0;
        public readonly IMailClient field1;
    }
}