using CUsers.Domain;
using Data;
using Logging;
using Shared;

namespace Background
{
    public class BackgroundExecutorService
        : IBackgroundExecutorService
    {
        public BackgroundExecutorService(
            IOrganizationContextScopeService arg0,
            ILogger arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IOrganizationContextScopeService field0;
        public readonly ILogger field1;
    }


    public class BackgroundIocModule
    {
    }


    public interface IBackgroundExecutorService
    {
    }


    public class ScopedBackgroundTask
    {
        public ScopedBackgroundTask(
            IOrganizationContextScopeService arg0,
            ILogger arg1,
            IMasterOrganizationRepository arg2,
            IUserRepository arg3,
            IBackgroundTaskRepository arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IOrganizationContextScopeService field0;
        public readonly ILogger field1;
        public readonly IMasterOrganizationRepository field2;
        public readonly IUserRepository field3;
        public readonly IBackgroundTaskRepository field4;
    }
}