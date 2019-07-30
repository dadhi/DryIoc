using Framework;
using Integrations;
using Logic;
using Organizations;
using Shared;
using Users.Repositories;

namespace Financials
{
    public class FinancialsAddonIocModule
    {
    }


    public class FinancialsIntegrationHandlerService
        : IFinancialsIntegrationHandlerService, ITokenChangeListener
    {
        public FinancialsIntegrationHandlerService(
            IContextService<IPsaContext> arg0,
            IFinancialsConfigurationService arg1,
            IPartnerRepository arg2,
            ICountryService arg3,
            IUniqueUserSettingsRepository arg5,
            IOrganizationSettingsRepository arg6,
            IGlobalGuidService arg7,
            IUserRepository arg8,
            IPsaUserService arg9,
            IAccessRightService arg10
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
            field9 = arg9;
            field10 = arg10;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IFinancialsConfigurationService field1;
        public readonly IPartnerRepository field2;
        public readonly ICountryService field3;
        public readonly IUniqueUserSettingsRepository field5;
        public readonly IOrganizationSettingsRepository field6;
        public readonly IGlobalGuidService field7;
        public readonly IUserRepository field8;
        public readonly IPsaUserService field9;
        public readonly IAccessRightService field10;
    }

    public interface ITokenChangeListener
    {
    }


    public interface IFinancialsIntegrationHandlerService
    {
    }


    public class FinancialsNotAuthenticatedException : AppExceptionEx
    {
        public FinancialsNotAuthenticatedException(
            string arg0,
            AppExceptionFlags arg1
        ) : base()
        {
        }
    }


    public class CacheType
    {
    }
}