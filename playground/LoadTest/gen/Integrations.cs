using RM;
using Data;
using Framework;
using Logic;
using Organizations;
using Shared;
using IUserService = Logic.IUserService;

namespace Integrations
{
    public class Configuration
    {
        public Configuration(
        )
        {
        }
    }


    public class LanguageMapping
    {
        public LanguageMapping(
        )
        {
        }
    }


    public class PaymentTermMapping
    {
        public PaymentTermMapping(
        )
        {
        }
    }


    public class PeriodKeyMapping
    {
        public PeriodKeyMapping(
        )
        {
        }
    }

    public class ConfigurationService<T>
    {
    }


    public class IntegrationsIocModule
    {
    }


    public interface IBusinessConfigurationService
    {
    }


    public class BusinessConfigurationService : ConfigurationService<Configuration>
        , IBusinessConfigurationService
    {
        public BusinessConfigurationService(
            IOrganizationAddonService arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IOrganizationAddonService field0;
    }


    public interface IFinancialsConfigurationService
    {
    }


    public class FinancialsConfigurationService : ConfigurationService<Configuration>
        , IFinancialsConfigurationService
    {
        public FinancialsConfigurationService(
            IOrganizationAddonService arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IOrganizationAddonService field0;
    }


    public interface IGlobalConfigurationService
    {
    }


    public class GlobalConfigurationService : ConfigurationService<Configuration>
        , IGlobalConfigurationService
    {
        public GlobalConfigurationService(
            IOrganizationAddonService arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IOrganizationAddonService field0;
    }


    public class OrganizationAddonExtensions
    {
    }


    public interface IOrganizationAddonService
    {
    }


    public class OrganizationAddonService
        : IOrganizationAddonService
    {
        public OrganizationAddonService(
        )
        {
        }
    }

    public class CalendarSyncProviderBase
        : ICalendarSyncProvider
    {
        public CalendarSyncProviderBase(
            IContextService<IPsaContext> arg0,
            IUserRepository arg1,
            IUserService arg2,
            ICalendarSyncService arg3,
            IActivityUserMemberService arg4,
            IActivityContactMemberService arg5,
            IContactRepository arg6,
            IActivityTypeRepository arg7,
            ITimeZoneService arg8,
            IGuidService arg9,
            IActivityService arg10
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
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IUserRepository field1;
        public readonly IUserService field2;
        public readonly ICalendarSyncService field3;
        public readonly IActivityUserMemberService field4;
        public readonly IActivityContactMemberService field5;
        public readonly IContactRepository field6;
        public readonly IActivityTypeRepository field7;
        public readonly ITimeZoneService field8;
        public readonly IGuidService field9;
        public readonly IActivityService field10;
    }


    public class ExchangeCalendarSyncProvider : CalendarSyncProviderBase
        , IExchangeCalendarSyncProvider
    {
        public ExchangeCalendarSyncProvider(
            IContextService<IPsaContext> arg0,
            IUserRepository arg1,
            IUserService arg2,
            ICalendarSyncService arg3,
            IActivityUserMemberService arg4,
            IActivityContactMemberService arg5,
            IContactRepository arg6,
            IActivityTypeRepository arg7,
            ITimeZoneService arg8,
            IGuidService arg9,
            IActivityService arg10
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10)
        {
        }
    }


    public class GoogleCalendarSyncProvider : CalendarSyncProviderBase
        , IGoogleCalendarSyncProvider
    {
        public GoogleCalendarSyncProvider(
            IContextService<IPsaContext> arg0,
            IUserRepository arg1,
            IUserService arg2,
            ICalendarSyncService arg3,
            IActivityUserMemberService arg4,
            IActivityContactMemberService arg5,
            IContactRepository arg6,
            IActivityTypeRepository arg7,
            ITimeZoneService arg8,
            IGuidService arg9,
            IActivityService arg10
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10)
        {
        }
    }


    public interface IExchangeCalendarSyncProvider
        : ICalendarSyncProvider
    {
    }


    public interface IGoogleCalendarSyncProvider
        : ICalendarSyncProvider
    {
    }


    public class PsaIntegrationIocModule
    {
    }


    public class SyncResult
    {
    }

    public partial class BaseIntegrationProvider<T>
        : IIntegrationProvider
    {
    }


    public partial class BaseIntegrationProvider<T, E> : BaseIntegrationProvider<T>
    {
    }


    public class BaseIntegrationSettings
    {
        public BaseIntegrationSettings(
        )
        {
        }
    }


    public interface ICalendarSyncProvider
        : IIntegrationProvider
    {
    }


    public class CalendarSyncSyncEventId
    {
    }


    public interface IIntegrationContext
    {
    }


    public interface IIntegrationProvider
    {
    }


    public interface IIntegrationSettingsService
    {
    }


    public interface IMobileSyncIntegrationProvider
        : IIntegrationProvider
    {
    }


    public class MobileSyncUser
    {
        public MobileSyncUser(
        )
        {
        }
    }
}