using System;
using System.Collections;
using System.Data.Common;
using Conn.Adapter;
using Data;
using Framework;
using Logic;
using OrganizationBase;
using Shared;
using Shop;
using Users;
using Users.Repositories;

namespace Organizations
{
    public interface IExternalIdentifierService
    {
    }


    public class ExternalIdentifierService
        : IExternalIdentifierService
    {
        public ExternalIdentifierService(
            IOrganizationSettingsRepository arg0,
            IOrganizationService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IOrganizationSettingsRepository field0;
        public readonly IOrganizationService field1;
    }


    public interface IExternallyOwnedOrganizationService
    {
    }


    public class ExternallyOwnedOrganizationService
        : IExternallyOwnedOrganizationService
    {
        public ExternallyOwnedOrganizationService(
            IOrganizationSettingsRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IOrganizationSettingsRepository field0;
    }


    public interface IInvoicingContactService
    {
    }


    public class InvoicingContactService
        : IInvoicingContactService
    {
        public InvoicingContactService(
            IOrganizationUserRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IOrganizationUserRepository field0;
    }


    public class InvoicingContacts
    {
        public InvoicingContacts(
        )
        {
        }
    }


    public interface IOrganization : IEntity
    {
    }


    public class Organization
        : IOrganization, IMasterOrganization
    {
        public Organization(
        )
        {
        }
    }


    public interface IOrganizationPermissionService
    {
    }

    public class OrganizationPermissionService : IOrganizationPermissionService
    {
        protected readonly IExternallyOwnedOrganizationService ExternallyOwnedOrganizationService;

        public OrganizationPermissionService(IExternallyOwnedOrganizationService externallyOwnedOrganizationService)
        {
            ExternallyOwnedOrganizationService = externallyOwnedOrganizationService;
        }
    }


    public interface IOrganizationSearch
    {
    }


    public class OrganizationSearch
        : IOrganizationSearch
    {
        public OrganizationSearch(
            IExternalIdentifierService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IExternalIdentifierService field0;
    }


    public interface IOrganizationService
    {
    }


    public class OrganizationService
        : IOrganizationService
    {
        public OrganizationService(
            IOrganizationRepository arg0,
            IOrganizationEmailService arg1,
            IExternallyOwnedOrganizationService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IOrganizationRepository field0;
        public readonly IOrganizationEmailService field1;
        public readonly IExternallyOwnedOrganizationService field2;
    }


    public class OrganizationSearchCriteria
    {
        public OrganizationSearchCriteria(
        )
        {
        }
    }

    public interface IEnvironmentClosedEmailBuilder
    {
    }


    public interface IEnvironmentOpenedEmailBuilder
    {
    }


    public interface IOrganizationEmailService
    {
    }


    public interface IOrganizationRepository : IEntityRepository<Organization>
    {
    }


    public class OrganizationRepository : RepositoryBase
        , IOrganizationRepository
    {
        public OrganizationRepository(
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


    public interface IOrganizationSettingsRepository
    {
    }


    public class OrganizationSettingsRepository : RepositoryBase
        , IOrganizationSettingsRepository
    {
        public OrganizationSettingsRepository(
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


    public partial interface IOrganizationUserRepository
    {
    }

    public interface ILastSignInService
    {
    }


    public interface IUser
    {
    }


    public interface IUserService
    {
    }

    public interface ILastSignInRepository
    {
    }


    public class LastSignInRepository : RepositoryBase
        , ILastSignInRepository
    {
        public LastSignInRepository(
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


    public class UserCounts
    {
        public UserCounts(
            int arg0,
            int arg1,
            int arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly int field0;
        public readonly int field1;
        public readonly int field2;
    }


    public interface IMasterUserRepository
    {
    }


    public class MasterUserRepository : OrganizationEntityRepository<MasterUserFields, MasterUser, IPsaContext>
        , IMasterUserRepository
    {
        public MasterUserRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }

    public interface IOrganizationUserReaderRepository
    {
    }


    public class OrganizationUserReaderRepository : RepositoryBase
        , IOrganizationUserReaderRepository
    {
        public OrganizationUserReaderRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public partial interface IOrganizationUserRepository
        : IEntityRepository<OrganizationUser>
    {
    }


    public class OrganizationUserRepository :
        OrganizationEntityRepository<OrganizationUserFields, OrganizationUser, IPsaContext>
        , IOrganizationUserRepository
    {
        public OrganizationUserRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
    }


    public interface IOrganizationUserService
    {
    }


    public class OrganizationUserService
        : IOrganizationUserService
    {
        public OrganizationUserService(
            IOrganizationUserRepository arg0,
            IUniqueUserRepository arg1,
            IUserRepository arg2,
            IPsaUserService arg3,
            IUserMemberCopyService arg4,
            IUserSeatService arg5
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IOrganizationUserRepository field0;
        public readonly IUniqueUserRepository field1;
        public readonly IUserRepository field2;
        public readonly IPsaUserService field3;
        public readonly IUserMemberCopyService field4;
        public readonly IUserSeatService field5;
    }

    public interface IUserSeatService
    {
    }

    public class UserSeatService : IUserSeatService
    {
        protected readonly IPsaContextService ContextService;
        protected readonly IAddonService AddonService;
        protected readonly IOrganizationAddonService OrganizationAddonService;
        protected readonly IMasterUserRepository MasterUserRepository;
        protected readonly IOrganizationService OrganizationService;
        protected readonly IExternallyOwnedOrganizationService ExternallyOwnedOrganizationService;

        public UserSeatService(IPsaContextService contextService, IAddonService addonService, IOrganizationAddonService organizationAddonService, IMasterUserRepository masterUserRepository, IOrganizationService organizationService, IExternallyOwnedOrganizationService externallyOwnedOrganizationService)
        {
            ContextService = contextService;
            AddonService = addonService;
            OrganizationAddonService = organizationAddonService;
            MasterUserRepository = masterUserRepository;
            OrganizationService = organizationService;
            ExternallyOwnedOrganizationService = externallyOwnedOrganizationService;
        }
    }



    public interface IPsaUserService
    {
    }


    public class PsaUserService
        : IPsaUserService, IGuidConverter
    {
        public PsaUserService(
            IContextService<IPsaContext> arg0,
            IUserValidator arg1,
            IUniqueUserService arg2,
            IOrganizationUserRepository arg3,
            IUserRepository arg4,
            IUserAuthorization arg5,
            IUserMemberCopyService arg6,
            IFeatureService arg7,
            IAuditTrail<User> arg8,
            IUserSeatService arg9,
            IUserEmailBuilder arg10,
            IMailClient arg11,
            IMasterUserRepository arg12,
            IUserDefaultsService arg13,
            IOrganizationService arg14,
            IExternallyOwnedOrganizationService arg15,
            IUserAnonymizer arg16
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
            field10 = arg10;
            field11 = arg11;
            field12 = arg12;
            field13 = arg13;
            field14 = arg14;
            field15 = arg15;
            field16 = arg16;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IUserValidator field1;
        public readonly IUniqueUserService field2;
        public readonly IOrganizationUserRepository field3;
        public readonly IUserRepository field4;
        public readonly IUserAuthorization field5;
        public readonly IUserMemberCopyService field6;
        public readonly IFeatureService field7;
        public readonly IAuditTrail<User> field8;
        public readonly IUserSeatService field9;
        public readonly IUserEmailBuilder field10;
        public readonly IMailClient field11;
        public readonly IMasterUserRepository field12;
        public readonly IUserDefaultsService field13;
        public readonly IOrganizationService field14;
        public readonly IExternallyOwnedOrganizationService field15;
        public readonly IUserAnonymizer field16;
    }

    public interface IUserEmailBuilder
    {
    }

    public class UserEmailBuilder : IUserEmailBuilder
    {
        protected readonly IPsaContextService ContextService;
        protected readonly IConnClientService ConnClientService;
        protected readonly IMasterOrganizationRepository MasterOrganizationRepository;
        protected readonly IDict Dictionary;
        protected readonly IAppSettings Settings;
        protected readonly IFeatureService FeatureService;
        protected readonly IEmailTemplateService EmailTemplateService;
        protected readonly IDistributorHelperService DistributorHelperService;

        public UserEmailBuilder(IPsaContextService contextService, IConnClientService ConnClientService, IMasterOrganizationRepository masterOrganizationRepository, IDict dictionary, IAppSettings settings, IFeatureService featureService, IEmailTemplateService emailTemplateService, IDistributorHelperService distributorHelperService)
        {
            ContextService = contextService;
            ConnClientService = ConnClientService;
            MasterOrganizationRepository = masterOrganizationRepository;
            Dictionary = dictionary;
            Settings = settings;
            FeatureService = featureService;
            EmailTemplateService = emailTemplateService;
            DistributorHelperService = distributorHelperService;
        }
    }


    public interface ITrustedOrganizationUserRepository
    {
    }


    public class TrustedOrganizationUserRepository : RepositoryBase
        , ITrustedOrganizationUserRepository
    {
        public TrustedOrganizationUserRepository(
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


    public interface IUserAnonymizer
    {
    }


    public interface IUserAuthorization
    {
    }


    public class UserAuthorization
        : IUserAuthorization
    {
        public UserAuthorization(
            IPsaContextService arg0,
            IUserRightsService arg1,
            IUserRepository arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IPsaContextService field0;
        public readonly IUserRightsService field1;
        public readonly IUserRepository field2;
    }


    public interface IUserMemberCopyService
    {
    }


    public class UserMemberCopyService
        : IUserMemberCopyService
    {
        public UserMemberCopyService(
        )
        {
        }
    }


    public interface IUserRepository
        : IEntityRepository<User>
    {
    }


    public class UniqueUserDecoratorForUserRepository
        : IUserRepository
    {
        public UniqueUserDecoratorForUserRepository(
            IUserRepository arg0,
            IOrganizationUserRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IUserRepository field0;
        public readonly IOrganizationUserRepository field1;
    }


    public interface IUserRepositoryController
    {
    }


    public class UserRepositoryController
        : IUserRepositoryController
    {
        public UserRepositoryController(
            IUserRepository arg0,
            IOrganizationUserRepository arg1,
            IUniqueUserRepository arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IUserRepository field0;
        public readonly IOrganizationUserRepository field1;
        public readonly IUniqueUserRepository field2;
    }


    public interface IUserRightsService
    {
    }


    public class UserRightsService
        : IUserRightsService
    {
        public UserRightsService(
        )
        {
        }
    }


    public interface IUserValidator
    {
    }


    public class UserValidator
        : IUserValidator
    {
        public UserValidator(
            IValidator<IUniqueUser> arg0,
            IUniqueUserToConnHelperService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IValidator<IUniqueUser> field0;
        public readonly IUniqueUserToConnHelperService field1;
    }

    public interface IValidator<T>
    {
    }

    public class LastSignInService
        : ILastSignInService
    {
        public LastSignInService(
            ILastSignInRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly ILastSignInRepository field0;
    }


    public class MasterUserFields : OrganizationUser, IOrganizationEntity
    {
        public MasterUserFields(
        )
        {
        }
    }


    public class MasterUser : MasterUserFields
        , IIdentifiableEntityWithOriginalState<MasterUser>, IIdentifiableEntityWithOriginalState<MasterUserFields>
    {
        public MasterUser(
        )
        {
        }
    }


    public class OrganizationUserFields : OrganizationEntity
    {
        public OrganizationUserFields(
        )
        {
        }
    }


    public class OrganizationUser : OrganizationUserFields
        , IOrganizationUser, IIdentifiableEntityWithOriginalState<OrganizationUser>,
        IIdentifiableEntityWithOriginalState<OrganizationUserFields>
    {
        public OrganizationUser(
        )
        {
        }
    }


    public class OrganizationUserWrapper
    {
        public OrganizationUserWrapper(
        )
        {
        }
    }


    public class UniqueUserDependencyServiceDecorator
        : IUniqueUserService
    {
        public UniqueUserDependencyServiceDecorator(
            IUniqueUserService arg0,
            IUniqueUserPhotoFileService arg1,
            ITrustedOrganizationUserRepository arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IUniqueUserService field0;
        public readonly IUniqueUserPhotoFileService field1;
        public readonly ITrustedOrganizationUserRepository field2;
    }


    public class UserFields : OrganizationEntity
    {
        public UserFields(
        )
        {
        }
    }


    public class UserInConn
    {
    }


    public class UserService
        : IUserService
    {
        public UserService(
            IUserRepositoryController arg0,
            IUserAuthorization arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IUserRepositoryController field0;
        public readonly IUserAuthorization field1;
    }


    public class PropertyInfoPair
    {
        public PropertyInfoPair(
        )
        {
        }
    }


    public class LoadUserRightsDelegate
    {
        public LoadUserRightsDelegate(
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

    public class UniqueUserToUserReplicator : RepositoryBase
       , IUniqueUserRepository
    {
        public UniqueUserToUserReplicator(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            IUniqueUserRepository arg3,
            ITrustedOrganizationUserRepository arg4,
            ICustomerDatabaseRepository arg5
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
        public readonly IUniqueUserRepository field3;
        public readonly ITrustedOrganizationUserRepository field4;
        public readonly ICustomerDatabaseRepository field5;
    }


    public class UserChangeBlockerWhenExternallyOwned
        : IUserRepository
    {
        public UserChangeBlockerWhenExternallyOwned(
            IUserRepository arg0,
            IExternallyOwnedOrganizationService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IUserRepository field0;
        public readonly IExternallyOwnedOrganizationService field1;
    }


    public class UserRepository : OrganizationEntityRepository<UserFields, User, IPsaContext>
        , IUserRepository
    {
        public UserRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public UserRepository(
            IContextService<IPsaContext> arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            ICustomerDatabaseRepository arg3,
            IMasterUserRepository arg4
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
        public readonly ICustomerDatabaseRepository field3;
        public readonly IMasterUserRepository field4;
    }


    public class ConnOrganizationUserRepository
        : IOrganizationUserRepository
    {
        public ConnOrganizationUserRepository(
            IOrganizationUserRepository arg0,
            IOrganizationUserReaderRepository arg1,
            IUniqueUserRepository arg2,
            IUniqueUserToConnChangeNotifier arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IOrganizationUserRepository field0;
        public readonly IOrganizationUserReaderRepository field1;
        public readonly IUniqueUserRepository field2;
        public readonly IUniqueUserToConnChangeNotifier field3;
    }


    public class AccessRightEntityContextComparer
        : IEqualityComparer
    {
        public AccessRightEntityContextComparer(
        )
        {
        }

        public bool Equals(object x, object y) => throw new NotImplementedException();

        public int GetHashCode(object obj) => throw new NotImplementedException();
    }


    public class AccessRights
    {
        public AccessRights(
        )
        {
        }
    }
}