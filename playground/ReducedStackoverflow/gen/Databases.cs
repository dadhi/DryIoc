using System.Data.Common;
using Framework;
using Shared;

namespace Databases
{
    public interface ICustomerDatabase
    {
    }


    public class CustomerDatabase : RepositoryBase
        , ICustomerDatabase
    {
        public CustomerDatabase(
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


    public class IDataRecordExtensions
    {
    }


    public class IDbCommandExtensions
    {
    }


    public interface IMasterDatabase
    {
    }


    public class MasterDatabase : RepositoryBase
        , IMasterDatabase
    {
        public MasterDatabase(
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