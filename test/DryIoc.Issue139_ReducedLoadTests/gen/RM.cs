using System.Data.Common;
using Data;
using Entities;
using Framework;
using OrganizationBase;
using Organizations;
using Shared;

namespace RM
{

    public class AccountCountrySettingsAuthorization : PsaEntityAuthorization<AccountCountrySettings>
    {
        public AccountCountrySettingsAuthorization(
            IContextService<IPsaContext> arg0,
            IAccountRepository arg1,
            IAuthorization<IPsaContext, Account> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IAccountRepository field1;
        public readonly IAuthorization<IPsaContext, Account> field2;
    }

    public class AccountGroupAuthorization : SettingsEntityAuthorization<AccountGroup>
    {
        public AccountGroupAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class AccountGroupMemberAuthorization : PsaEntityAuthorization<AccountGroupMember>
    {
        public AccountGroupMemberAuthorization(
            IContextService<IPsaContext> arg0,
            IAccountRepository arg1,
            IAuthorization<IPsaContext, Account> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IAccountRepository field1;
        public readonly IAuthorization<IPsaContext, Account> field2;
    }

    public class FileDataAuthorization : PsaEntityAuthorization<FileData>
    {
        public FileDataAuthorization(
            IContextService<IPsaContext> arg0,
            IAccountRepository arg1,
            IAuthorization<IPsaContext, Account> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IAccountRepository field1;
        public readonly IAuthorization<IPsaContext, Account> field2;
    }

    public class AccountPermissionLevel
    {
    }


    public class AddressAuthorization : PsaEntityAuthorization<Address>
    {
        public AddressAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class CommunicatesWithAuthorization : PsaEntityAuthorization<CommunicatesWith>
    {
        public CommunicatesWithAuthorization(
            IContextService<IPsaContext> arg0,
            IContactRepository arg1,
            IAuthorization<IPsaContext, Contact> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContactRepository field1;
        public readonly IAuthorization<IPsaContext, Contact> field2;
    }


    public class CompanyAuthorization : PsaEntityAuthorization<Company>
    {
        public CompanyAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class AccountCountrySettings : AccountCountrySettingsFields
        , IIdentifiableEntityWithOriginalState<AccountCountrySettingsFields>
    {
        public AccountCountrySettings(
        )
        {
        }
    }


    public class AccountGroup : AccountGroupFields
        , IIdentifiableEntityWithOriginalState<AccountGroupFields>
    {
        public AccountGroup(
        )
        {
        }

        public AccountGroup(
            AccountGroup arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly AccountGroup field0;
    }


    public class AccountGroupMemberEx : AccountGroupMember
    {
        public AccountGroupMemberEx(
        )
        {
        }
    }


    public class AccountWithOrganizationCompanyInfo : Account
    {
        public AccountWithOrganizationCompanyInfo(
        )
        {
        }
    }


    public class ActiveContact
    {
        public ActiveContact(
        )
        {
        }
    }


    public class Contact : ContactFields
        , INamedEntity, IIdentifiableEntityWithOriginalState<ContactFields>
    {
        public Contact(
        )
        {
        }
    }


    public class ContactCommunicationMethod : CommunicatesWith
    {
        public ContactCommunicationMethod(
        )
        {
        }
    }


    public class ContactWithEmail : Contact
    {
        public ContactWithEmail(
        )
        {
        }
    }


    public class AccountFields : OrganizationEntity
        , INamedEntity
    {
        public AccountFields(
        )
        {
        }
    }


    public class Account : AccountFields
        , IIdentifiableEntityWithOriginalState<AccountFields>
    {
        public Account(
        )
        {
        }
    }


    public class AccountCountrySettingsFields : OrganizationEntity
    {
        public AccountCountrySettingsFields(
        )
        {
        }
    }


    public class AccountGroupFields : OrganizationEntity
        , INamedEntity
    {
        public AccountGroupFields(
        )
        {
        }
    }


    public class AccountGroupMemberFields : OrganizationEntity
    {
        public AccountGroupMemberFields(
        )
        {
        }
    }


    public class AccountGroupMember : AccountGroupMemberFields
        , IIdentifiableEntityWithOriginalState<AccountGroupMemberFields>
    {
        public AccountGroupMember(
        )
        {
        }
    }


    public class CompanyFields : OrganizationEntity
        , INamedEntity
    {
        public CompanyFields(
        )
        {
        }
    }


    public class Company : CompanyFields
        , IIdentifiableEntityWithOriginalState<CompanyFields>
    {
        public Company(
        )
        {
        }
    }


    public class ContactFields : OrganizationEntity
    {
        public ContactFields(
        )
        {
        }
    }


    public class CommunicatesWithFields : OrganizationEntity
    {
        public CommunicatesWithFields(
        )
        {
        }
    }


    public class CommunicatesWith : CommunicatesWithFields
        , IIdentifiableEntityWithOriginalState<CommunicatesWithFields>
    {
        public CommunicatesWith(
        )
        {
        }
    }


    public interface IAccount
        : IOrganizationEntity, INamedEntity
    {
    }


    public class AccountCountrySettingsRepository : OrganizationEntityRepository<AccountCountrySettingsFields,
            AccountCountrySettings, IPsaContext>
        , IAccountCountrySettingsRepository
    {
        public AccountCountrySettingsRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
        public readonly ICustomerDatabaseRepository field3;
    }


    public class AccountGroupMemberRepository :
        OrganizationEntityRepository<AccountGroupMemberFields, AccountGroupMember, IPsaContext>
        , IAccountGroupMemberRepository
    {
        public AccountGroupMemberRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
        public readonly ICustomerDatabaseRepository field3;
    }


    public class AccountGroupRepository : OrganizationEntityRepository<AccountGroupFields, AccountGroup, IPsaContext>
        , IAccountGroupRepository
    {
        public AccountGroupRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
        public readonly ICustomerDatabaseRepository field3;
    }


    public class AccountRepository : OrganizationEntityRepository<AccountFields, Account, IPsaContext>
        , IAccountRepository
    {
        public AccountRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
        public readonly ICustomerDatabaseRepository field3;
    }


    public class AddressRepository : OrganizationEntityRepository<AddressFields, Address, IPsaContext>
        , IAddressRepository
    {
        public AddressRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
        public readonly ICustomerDatabaseRepository field3;
    }


    public class CommunicatesWithRepository :
        OrganizationEntityRepository<CommunicatesWithFields, CommunicatesWith, IPsaContext>
        , ICommunicatesWithRepository
    {
        public CommunicatesWithRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
        public readonly ICustomerDatabaseRepository field3;
    }


    public class CompanyRepository : OrganizationEntityRepository<CompanyFields, Company, IPsaContext>
        , ICompanyRepository
    {
        public CompanyRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1
        ) : base()
        {
        }
    }


    public class ContactRepository : OrganizationEntityRepository<ContactFields, Contact, IPsaContext>
        , IContactRepository
    {
        public ContactRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
        public readonly ICustomerDatabaseRepository field3;
    }


    public interface IAccountCountrySettingsRepository
        : IEntityRepository<AccountCountrySettings>
    {
    }


    public interface IAccountGroupMemberRepository
        : IEntityRepository<AccountGroupMember>
    {
    }


    public interface IAccountGroupRepository
        : IEntityRepository<AccountGroup>
    {
    }


    public interface IAccountRepository
        : IEntityRepository<Account>
    {
    }


    public interface IAddressRepository
        : IEntityRepository<Address>
    {
    }


    public interface ICommunicatesWithRepository
        : IEntityRepository<CommunicatesWith>
    {
    }


    public interface ICompanyRepository
        : IEntityRepository<Company>
    {
    }


    public interface IContactRepository
        : IEntityRepository<Contact>
    {
    }


    public class AccountCountrySettingsService : OrganizationEntityService<AccountCountrySettings,
            IAccountCountrySettingsRepository, User, IPsaContext>
        , IAccountCountrySettingsService
    {
        public AccountCountrySettingsService(
            IContextService<IPsaContext> arg0,
            IAccountCountrySettingsRepository arg1,
            IValidator<AccountCountrySettings> arg2,
            IAuthorization<IPsaContext, AccountCountrySettings> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IAccountCountrySettingsRepository field1;
        public readonly IValidator<AccountCountrySettings> field2;
        public readonly IAuthorization<IPsaContext, AccountCountrySettings> field3;
    }


    public abstract class OrganizationEntityService<TEntity, TRepository, TUser, TContext> : EntityService<TContext, TEntity, TRepository> where TRepository : IEntityRepository<TEntity>
        where TUser : IOrganizationUserBase
        where TContext : IOrganizationContextBase<TUser, IPsaCompany>
        where TEntity : IIdentifiableEntity
    {
        private IContextService<IPsaContext> arg0;

        protected OrganizationEntityService(IContextService<IPsaContext> arg0)
        {
            this.arg0 = arg0;
        }

        protected OrganizationEntityService(IContextService<TContext> contextService, TRepository repository, IValidator<TEntity> validator, IAuthorization<TContext, TEntity> authorization) : base(contextService, repository, validator, authorization)
        {
        }

        protected OrganizationEntityService()
        {
        }
    }

    public class AccountGroupMemberService :
        EntityService<IPsaContext, AccountGroupMember, IAccountGroupMemberRepository>
        , IAccountGroupMemberService
    {
        public AccountGroupMemberService(
            IContextService<IPsaContext> arg0,
            IAccountGroupMemberRepository arg1,
            IValidator<AccountGroupMember> arg2,
            IAuthorization<IPsaContext, AccountGroupMember> arg3,
            IAccountService arg4,
            IAuthorization<IPsaContext, Account> arg5,
            IAccountGroupService arg6
        ) : base(arg0, arg1, arg2, arg3)
        {
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IAccountService field4;
        public readonly IAuthorization<IPsaContext, Account> field5;
        public readonly IAccountGroupService field6;
    }


    public class AccountGroupService :
        OrganizationEntityService<AccountGroup, IAccountGroupRepository, User, IPsaContext>
        , IAccountGroupService
    {
        public AccountGroupService(
            IContextService<IPsaContext> arg0,
            IAccountGroupRepository arg1,
            IValidator<AccountGroup> arg2,
            IAuthorization<IPsaContext, AccountGroup> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IAccountGroupRepository field1;
        public readonly IValidator<AccountGroup> field2;
        public readonly IAuthorization<IPsaContext, AccountGroup> field3;
    }


    public class AccountService : EntityService<IPsaContext, Account, IAccountRepository>
        , IAccountService
    {
        public AccountService(
            IContextService<IPsaContext> arg0,
            IAccountRepository arg1,
            IValidator<Account> arg2,
            IAuthorization<IPsaContext, Account> arg3,
            ICompanyService arg4,
            IPsaOrganizationService arg5,
            IOrganizationCompanyRepository arg6
        ) : base(arg0, arg1, arg2, arg3)
        {
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly ICompanyService field4;
        public readonly IPsaOrganizationService field5;
        public readonly IOrganizationCompanyRepository field6;
    }

    public abstract class PsaEntityService<TEntity, TRepository> : OrganizationEntityService<TEntity, TRepository, User, IPsaContext>
        where TEntity : IOrganizationEntity
        where TRepository : IEntityRepository<TEntity>
    {
        protected PsaEntityService(IContextService<IPsaContext> contextService, TRepository repository, IValidator<TEntity> validator, IAuthorization<IPsaContext, TEntity> authorization) : base(contextService, repository, validator, authorization)
        {
        }
    }

    public class AddressService : PsaEntityService<Address, IAddressRepository>
        , IAddressService
    {
        public AddressService(
            IContextService<IPsaContext> arg0,
            IAddressRepository arg1,
            IValidator<Address> arg2,
            IAuthorization<IPsaContext, Address> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IAddressRepository field1;
        public readonly IValidator<Address> field2;
        public readonly IAuthorization<IPsaContext, Address> field3;
    }


    public class CompanyService : OrganizationEntityService<Company, ICompanyRepository, User, IPsaContext>
        , ICompanyService
    {
        public CompanyService(
            IContextService<IPsaContext> arg0,
            ICompanyRepository arg1,
            IValidator<Company> arg2,
            IAuthorization<IPsaContext, Company> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICompanyRepository field1;
        public readonly IValidator<Company> field2;
        public readonly IAuthorization<IPsaContext, Company> field3;
    }


    public interface IAccountCountrySettingsService
        : IEntityService<AccountCountrySettings>
    {
    }


    public interface IAccountGroupMemberService
        : IEntityService<AccountGroupMember>
    {
    }


    public interface IAccountGroupService
        : IEntityService<AccountGroup>
    {
    }


    public interface IAccountService
        : IEntityService<Account>
    {
    }


    public interface IAddressService
        : IEntityService<Address>
    {
    }


    public interface ICompanyService
        : IEntityService<Company>
    {
    }

    public interface ITaxRepository : IEntityRepository<Tax>
    {
    }

    public partial class TaxRepository : OrganizationEntityRepository<TaxFields, Tax, IPsaContext>, ITaxRepository
    {
        public TaxRepository(IContextService<IPsaContext> contextService, IConfiguration configuration, DbProviderFactory dbProviderFactory, ICustomerDatabaseRepository customerDatabaseRepository) : base(contextService, configuration, dbProviderFactory, customerDatabaseRepository)
        {
        }
    }

    public class TaxFields : IOrganizationEntity
    {
    }

    public class AccountCountrySettingsValidator : Validator<AccountCountrySettings>
    {
        public AccountCountrySettingsValidator(
            ITaxRepository arg0,
            IAccountCountrySettingsRepository arg1,
            IOrganizationCompanyRepository arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ITaxRepository field0;
        public readonly IAccountCountrySettingsRepository field1;
        public readonly IOrganizationCompanyRepository field2;
    }


    public class AccountGroupValidator : Validator<AccountGroup>
    {
        public AccountGroupValidator(
            IAccountGroupRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IAccountGroupRepository field0;
    }


    public class AccountValidator : Validator<Account>
    {
        public AccountValidator(
            IContextService<IPsaContext> arg0,
            IAccountRepository arg1,
            ILanguageService arg2,
            IOrganizationPermissionService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IAccountRepository field1;
        public readonly ILanguageService field2;
        public readonly IOrganizationPermissionService field3;
    }


    public class AddressValidator : Validator<Address>
    {
        public AddressValidator(
            IAddressRepository arg0,
            ICountryRegionService arg1,
            IAccountRepository arg2,
            ICompanyRepository arg3,
            IOrganizationPermissionService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IAddressRepository field0;
        public readonly ICountryRegionService field1;
        public readonly IAccountRepository field2;
        public readonly ICompanyRepository field3;
        public readonly IOrganizationPermissionService field4;
    }


    public class CommunicatesWithValidator : Validator<CommunicatesWith>
    {
        public CommunicatesWithValidator(
            ICommunicatesWithRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ICommunicatesWithRepository field0;
    }


    public class CompanyValidator : Validator<Company>
    {
        public CompanyValidator(
            IAccountRepository arg0,
            ICompanyRepository arg1,
            IOrganizationPermissionService arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IAccountRepository field0;
        public readonly ICompanyRepository field1;
        public readonly IOrganizationPermissionService field2;
    }


    public class ContactValidator : Validator<Contact>
    {
        public ContactValidator(
            IContactRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IContactRepository field0;
    }
}