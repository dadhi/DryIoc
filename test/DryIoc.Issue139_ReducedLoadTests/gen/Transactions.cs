using System;

namespace Transactions
{
    public interface ITransactionScope
        : IDisposable
    {
    }


    public class TransactionScope
        : ITransactionScope
    {
        public TransactionScope(
            bool arg0
        )
        {
        }

        public TransactionScope(
            TransactionTimeout arg0,
            bool arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TransactionTimeout field0;
        public readonly bool field1;

        public void Dispose()
        {
        }
    }


    public class TransactionTimeout
    {
    }


    public class TransactionUtil
    {
        public TransactionUtil(
        )
        {
        }
    }
}