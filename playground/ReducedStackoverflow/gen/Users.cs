using Conn.Adapter;
using Framework;
using OrganizationBase;
using Organizations;
using Shared;
using Users.Repositories;

namespace Users
{
    public interface IUserDefaultsService
    {
    }

    public interface IUniqueUserService : IEntityService<UniqueUser>
    {
    }

    public interface IObsoleteUniqueUserService
    {
    }


    public class ObsoleteUniqueUserService
        : IObsoleteUniqueUserService
    {
        public ObsoleteUniqueUserService(
            IObsoleteUniqueUserRepository arg0,
            IUniqueUserSettingsRepository arg1,
            IUniqueUserPhotoFileService arg2,
            IConnClientService arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IObsoleteUniqueUserRepository field0;
        public readonly IUniqueUserSettingsRepository field1;
        public readonly IUniqueUserPhotoFileService field2;
        public readonly IConnClientService field3;
    }


    public interface IUniqueUser
    {
    }


    public interface IUniqueUserDefaultsService
    {
    }


    public class UniqueUserDefaultsService
        : IUniqueUserDefaultsService
    {
        public UniqueUserDefaultsService(
            ICountryService arg0,
            ICountryRegionService arg1,
            ILanguageService arg2,
            IContextService arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly ICountryService field0;
        public readonly ICountryRegionService field1;
        public readonly ILanguageService field2;
        public readonly IContextService field3;
    }




    public interface IUniqueUserPhotoFileService
    {
    }


    public class UniqueUserPhotoFileService
        : IUniqueUserPhotoFileService
    {
        public UniqueUserPhotoFileService(
            IMasterFileService arg0,
            IUniqueUserPhotoFileRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IMasterFileService field0;
        public readonly IUniqueUserPhotoFileRepository field1;
    }








    public interface IUniqueUserToConnHelperService
    {
    }


    public class UniqueUserToConnHelperService
        : IUniqueUserToConnHelperService
    {
        public UniqueUserToConnHelperService(
            ILanguageService arg0,
            ICountryService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly ILanguageService field0;
        public readonly ICountryService field1;
    }


    public class UniqueUserFields : IdentifiableEntity
    {
        public UniqueUserFields(
        )
        {
        }
    }


    public class UniqueUser : UniqueUserFields
        , IUniqueUser, IIdentifiableEntityWithOriginalState<UniqueUserFields>, IOrganizationEntity
    {
        public UniqueUser(
        )
        {
        }
    }


    public class UniqueUserPhotoFile
        : IIdentifiableEntity
    {
        public UniqueUserPhotoFile(
        )
        {
        }
    }


    public class UniqueUserService
        : IUniqueUserService
    {
        public UniqueUserService(
            IUniqueUserRepository arg0,
            //IAuthorization<IContext, UniqueUser> arg1,
            IUniqueUserDefaultsService arg2,
            IValidator<UniqueUser> arg3
        )
        {
            field0 = arg0;
            //this.field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IUniqueUserRepository field0;
        public readonly IAuthorization<IContext, UniqueUser> field1;
        public readonly IUniqueUserDefaultsService field2;
        public readonly IValidator<UniqueUser> field3;
    }


    public class UniqueUserValidator : Validator<UniqueUser>
    {
        public UniqueUserValidator(
            IContextService arg0,
            IFormattingCultureService arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService field0;
        public readonly IFormattingCultureService field1;
    }


    public class UserForConn
    {
        public UserForConn(
        )
        {
        }
    }
}