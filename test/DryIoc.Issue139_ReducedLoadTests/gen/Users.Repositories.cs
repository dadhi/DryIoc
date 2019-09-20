using System.Data.Common;
using Conn.Adapter;
using Data;
using Databases;
using Framework;
using Organizations;
using Shared;

namespace Users.Repositories
{
    public interface IUniqueUserRepository
        : IEntityRepository
    {
    }

    public class EntityVersionRepository : RepositoryBase
        , IEntityVersionRepository
    {
        public EntityVersionRepository(
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


    public interface IEntityVersionRepository
    {
    }


    public interface IObsoleteUniqueUserRepository
    {
    }


    public class ObsoleteUniqueUserRepository
        : IObsoleteUniqueUserRepository
    {
        public ObsoleteUniqueUserRepository(
            IMasterDatabase arg0
        )
        {
            field0 = arg0;
        }

        public readonly IMasterDatabase field0;
    }


    public interface IUniqueUserToConnChangeNotifier
    {
    }


    public class UniqueUserToConnChangeNotifier
        : IUniqueUserToConnChangeNotifier
    {
        public UniqueUserToConnChangeNotifier(
            IUniqueUserVersionHandlerRepository arg0,
            IConnClient arg1,
            IConnClientService arg2,
            IConnUserService arg3,
            IUniqueUserToConnHelperService arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IUniqueUserVersionHandlerRepository field0;
        public readonly IConnClient field1;
        public readonly IConnClientService field2;
        public readonly IConnUserService field3;
        public readonly IUniqueUserToConnHelperService field4;
    }


    public interface IUniqueUserVersionHandlerRepository
    {
    }


    public class UniqueUserVersionHandlerRepository
        : IUniqueUserVersionHandlerRepository
    {
        public UniqueUserVersionHandlerRepository(
            IMasterDatabase arg0,
            IEntityVersionRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IMasterDatabase field0;
        public readonly IEntityVersionRepository field1;
    }


    public class UniqueUserUuidHandlerRepository
        : IUniqueUserRepository
    {
        public UniqueUserUuidHandlerRepository(
            IUniqueUserRepository arg0,
            IOrganizationUserReaderRepository arg1,
            IConnUserService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IUniqueUserRepository field0;
        public readonly IOrganizationUserReaderRepository field1;
        public readonly IConnUserService field2;
    }


    public class UniqueUserConnUpdateHandlerRepository : RepositoryBase
        , IUniqueUserRepository
    {
        public UniqueUserConnUpdateHandlerRepository(
            IContextService arg0,
            IConfiguration arg1,
            DbProviderFactory arg2,
            IUniqueUserRepository arg3,
            IOrganizationUserReaderRepository arg4,
            IUniqueUserToConnChangeNotifier arg5,
            IUniqueUserToConnHelperService arg6
        ) : base(arg0, arg1, arg2)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IContextService field0;
        public readonly IConfiguration field1;
        public readonly DbProviderFactory field2;
        public readonly IUniqueUserRepository field3;
        public readonly IOrganizationUserReaderRepository field4;
        public readonly IUniqueUserToConnChangeNotifier field5;
        public readonly IUniqueUserToConnHelperService field6;
    }


    public interface IUniqueUserPhotoFileRepository
    {
    }


    public class UniqueUserPhotoFileRepository : RepositoryBase
        , IUniqueUserPhotoFileRepository
    {
        public UniqueUserPhotoFileRepository(
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

    public class UniqueUserRepository : EntityRepository<UniqueUserFields, UniqueUser>
        , IUniqueUserRepository
    {
        public UniqueUserRepository(
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

    public interface IUniqueUserSettingsRepository
    {
    }


    public class UniqueUserSettingsRepository : RepositoryBase
        , IUniqueUserSettingsRepository
    {
        public UniqueUserSettingsRepository(
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
}