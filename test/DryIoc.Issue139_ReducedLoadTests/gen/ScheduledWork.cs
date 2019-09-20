using Data;
using Framework;
using Logic;
using Shared;

namespace ScheduledWork
{
    public class ScheduledWorkIocModule
    {
    }


    public class DateSettingParser
    {
        public DateSettingParser(
        )
        {
        }
    }


    public interface IScheduledWorkService
    {
    }


    public class ScheduledWorkService
        : IScheduledWorkService
    {
        public ScheduledWorkService(
            IContextService<IPsaContext> arg0,
            IBackgroundTaskRepository arg1,
            IInvoiceService arg2,
            IIntegrationErrorService arg3,
            IInvoiceBillingAccountGrouperService arg4,
            IBackgroundTaskRunRepository arg5
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IBackgroundTaskRepository field1;
        public readonly IInvoiceService field2;
        public readonly IIntegrationErrorService field3;
        public readonly IInvoiceBillingAccountGrouperService field4;
        public readonly IBackgroundTaskRunRepository field5;
    }


    public class MiniProjectModel
    {
        public MiniProjectModel(
        )
        {
        }
    }


    public class JobInfo
    {
        public JobInfo(
        )
        {
        }
    }
}