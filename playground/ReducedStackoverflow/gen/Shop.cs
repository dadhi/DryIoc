using System;
using System.Collections.Generic;
using System.Data.Common;
using Data;
using Framework;
using Logic;
using OrganizationBase;
using Organizations;
using Shared;
using IOrganizationService = OrganizationBase.IOrganizationService;

namespace Shop
{
    public class AddonService
        : IAddonService
    {
        public AddonService(
            IAddonRepository arg0,
            IAddonCountryService arg1,
            IAddonDependencyService arg2,
            IDefaultAddonsService arg3,
            IAddonPricingModelContentRepository arg4,
            IAddonSettingRepository arg5
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IAddonRepository field0;
        public readonly IAddonCountryService field1;
        public readonly IAddonDependencyService field2;
        public readonly IDefaultAddonsService field3;
        public readonly IAddonPricingModelContentRepository field4;
        public readonly IAddonSettingRepository field5;
    }


    public class BillingService
        : IBillingService
    {
        public BillingService(
            IBillingRuleRepository arg0,
            IOrganizationBillingRuleService arg1,
            IOrganizationService arg2,
            IOrganizationPaymentPeriodService arg3,
            IBillingDatesResolver arg4,
            IOrganizationBillingCurrencyService arg5,
            IOrganizationPricingService arg6,
            IAddonService arg7,
            IOrganizationBillingService arg8
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
        }

        public readonly IBillingRuleRepository field0;
        public readonly IOrganizationBillingRuleService field1;
        public readonly IOrganizationService field2;
        public readonly IOrganizationPaymentPeriodService field3;
        public readonly IBillingDatesResolver field4;
        public readonly IOrganizationBillingCurrencyService field5;
        public readonly IOrganizationPricingService field6;
        public readonly IAddonService field7;
        public readonly IOrganizationBillingService field8;
    }


    public class ChangeEditionService
        : IChangeEditionService
    {
        public ChangeEditionService(
            IOrganizationAddonService arg0,
            IAddonService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IOrganizationAddonService field0;
        public readonly IAddonService field1;
    }


    public class AddonCountryService
        : IAddonCountryService
    {
        public AddonCountryService(
            IAddonCountryRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IAddonCountryRepository field0;
    }


    public class AddonDependencyService
        : IAddonDependencyService
    {
        public AddonDependencyService(
            IAddonRepository arg0,
            IAddonDependencyRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IAddonRepository field0;
        public readonly IAddonDependencyRepository field1;
    }


    public class AddonInformationEmailBuilder
        : IAddonInformationEmailBuilder
    {
        public AddonInformationEmailBuilder(
            IShopUserService arg0,
            IAppSettings arg1,
            IMailContentBuilder arg2,
            IDistributorHelperService arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IShopUserService field0;
        public readonly IAppSettings field1;
        public readonly IMailContentBuilder field2;
        public readonly IDistributorHelperService field3;
    }


    public class AddonRules
    {
    }


    public class AddonRuleService
        : IAddonRuleService
    {
        public AddonRuleService(
            IAddonRuleRepository arg0,
            IAddonRuleRowRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IAddonRuleRepository field0;
        public readonly IAddonRuleRowRepository field1;
    }


    public class AddonVatService
        : IAddonVatService
    {
        public AddonVatService(
            IAddonVatRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IAddonVatRepository field0;
    }


    public class BillingDatesResolver
        : IBillingDatesResolver
    {
        public BillingDatesResolver(
            ITimeZoneService arg0,
            ITimeService arg1,
            IBilledRowRepository arg2,
            IPartnerService arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly ITimeZoneService field0;
        public readonly ITimeService field1;
        public readonly IBilledRowRepository field2;
        public readonly IPartnerService field3;
    }


    public class BillingRuleChangesResolver
        : IBillingRuleChangesResolver
    {
        public BillingRuleChangesResolver(
            IAddonRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IAddonRepository field0;
    }


    public class CreditNoteEmailBuilder
        : ICreditNoteEmailBuilder
    {
        public CreditNoteEmailBuilder(
            IMasterOrganizationRepository arg0,
            IMasterUserRepository arg1,
            IDict arg2,
            ILanguageService arg3,
            ICurrencyBaseRepository arg4,
            IAppSettings arg5,
            IAddonCurrencyBankAccountRepository arg6,
            IBilledPaymentPdfBuilder arg7,
            IBilledRowRepository arg8,
            IDistributorHelperService arg9
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
        }

        public readonly IMasterOrganizationRepository field0;
        public readonly IMasterUserRepository field1;
        public readonly IDict field2;
        public readonly ILanguageService field3;
        public readonly ICurrencyBaseRepository field4;
        public readonly IAppSettings field5;
        public readonly IAddonCurrencyBankAccountRepository field6;
        public readonly IBilledPaymentPdfBuilder field7;
        public readonly IBilledRowRepository field8;
        public readonly IDistributorHelperService field9;
    }


    public class DistributorService
        : IDistributorService
    {
        public DistributorService(
            IPartnerRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IPartnerRepository field0;
    }


    public interface IAddonCountryService
    {
    }


    public interface IAddonDependencyService
    {
    }


    public interface IAddonInformationEmailBuilder
    {
    }


    public interface IAddonRuleService
    {
    }


    public interface IAddonVatService
    {
    }


    public interface IBillingDatesResolver
    {
    }


    public interface IBillingRuleChangesResolver
    {
    }


    public interface ICreditNoteEmailBuilder
    {
    }


    public interface IDistributorService
    {
    }


    public interface IOrderConfirmationEmailBuilder
    {
    }


    public class OrderConfirmationEmailBuilder
        : IOrderConfirmationEmailBuilder
    {
        public OrderConfirmationEmailBuilder(
            IPartnerService arg0,
            IUserRepository arg1,
            IMasterOrganizationRepository arg2,
            IAddonRepository arg3,
            IAddonSettingRepository arg4,
            ICurrencyBaseRepository arg5,
            ILanguageRepository arg6,
            IDict arg7,
            Utilities.IHttpUtility arg8,
            IAppSettings arg9,
            IMailContentBuilder arg10,
            IBilledPaymentRepository arg11,
            IBillingAddressRepository arg12,
            IAddonActivationRepository arg13,
            IBilledRowRepository arg14,
            IBillingRuleChangesResolver arg15
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
        }

        public readonly IPartnerService field0;
        public readonly IUserRepository field1;
        public readonly IMasterOrganizationRepository field2;
        public readonly IAddonRepository field3;
        public readonly IAddonSettingRepository field4;
        public readonly ICurrencyBaseRepository field5;
        public readonly ILanguageRepository field6;
        public readonly IDict field7;
        public readonly Utilities.IHttpUtility field8;
        public readonly IAppSettings field9;
        public readonly IMailContentBuilder field10;
        public readonly IBilledPaymentRepository field11;
        public readonly IBillingAddressRepository field12;
        public readonly IAddonActivationRepository field13;
        public readonly IBilledRowRepository field14;
        public readonly IBillingRuleChangesResolver field15;
    }


    public interface IOrganizationBalanceDueService
    {
    }


    public class OrganizationBalanceDueService
        : IOrganizationBalanceDueService
    {
        public OrganizationBalanceDueService(
            IBilledRowRepository arg0,
            IDict arg1,
            IBillingDatesResolver arg2,
            IShopUserService arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IBilledRowRepository field0;
        public readonly IDict field1;
        public readonly IBillingDatesResolver field2;
        public readonly IShopUserService field3;
    }


    public interface IOrganizationBillingCurrencyService
    {
    }


    public class OrganizationBillingCurrencyService
        : IOrganizationBillingCurrencyService
    {
        public OrganizationBillingCurrencyService(
            IBillingRuleRepository arg0,
            ICurrencyBaseRepository arg1,
            IAddonCountryCurrencyRepository arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IBillingRuleRepository field0;
        public readonly ICurrencyBaseRepository field1;
        public readonly IAddonCountryCurrencyRepository field2;
    }


    public interface IOrganizationBillingRuleService
    {
    }


    public class OrganizationBillingRuleService
        : IOrganizationBillingRuleService
    {
        public OrganizationBillingRuleService(
            IBillingRuleRepository arg0,
            IBillingRuleRowRepository arg1,
            IBilledRowRepository arg2,
            IAddonService arg3,
            IOrganizationPricingService arg4,
            IAddonVatService arg5,
            ICountryRepository arg6,
            IOrganizationPaymentPeriodService arg7,
            IOrganizationBalanceDueService arg8,
            IOrganizationAddonService arg9,
            IMasterOrganizationRepository arg10
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

        public readonly IBillingRuleRepository field0;
        public readonly IBillingRuleRowRepository field1;
        public readonly IBilledRowRepository field2;
        public readonly IAddonService field3;
        public readonly IOrganizationPricingService field4;
        public readonly IAddonVatService field5;
        public readonly ICountryRepository field6;
        public readonly IOrganizationPaymentPeriodService field7;
        public readonly IOrganizationBalanceDueService field8;
        public readonly IOrganizationAddonService field9;
        public readonly IMasterOrganizationRepository field10;
    }


    public interface IOrganizationBillingService
    {
    }


    public class OrganizationBillingService
        : IOrganizationBillingService
    {
        public OrganizationBillingService(
            IBilledPaymentRepository arg0,
            IBilledRowRepository arg1,
            IBillingDatesResolver arg2,
            ITimeZoneService arg3,
            ITimeService arg4,
            IHostingProviderService arg5,
            IMasterOrganizationRepository arg6,
            IShopUserService arg7
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
        }

        public readonly IBilledPaymentRepository field0;
        public readonly IBilledRowRepository field1;
        public readonly IBillingDatesResolver field2;
        public readonly ITimeZoneService field3;
        public readonly ITimeService field4;
        public readonly IHostingProviderService field5;
        public readonly IMasterOrganizationRepository field6;
        public readonly IShopUserService field7;
    }


    public interface IOrganizationPaymentPeriodService
    {
    }


    public class OrganizationPaymentPeriodService
        : IOrganizationPaymentPeriodService
    {
        public OrganizationPaymentPeriodService(
            IBillingRuleRepository arg0,
            IBillingRuleRowRepository arg1,
            IPartnerPricingModelSettingRepository arg2,
            IShopUserService arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IBillingRuleRepository field0;
        public readonly IBillingRuleRowRepository field1;
        public readonly IPartnerPricingModelSettingRepository field2;
        public readonly IShopUserService field3;
    }


    public interface IOrganizationPricingService
    {
    }


    public class OrganizationPricingService
        : IOrganizationPricingService
    {
        public OrganizationPricingService(
            IPricingService arg0,
            IAddonRepository arg1,
            IAddonActivationRepository arg2,
            IOrganizationBillingCurrencyService arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPricingService field0;
        public readonly IAddonRepository field1;
        public readonly IAddonActivationRepository field2;
        public readonly IOrganizationBillingCurrencyService field3;
    }


    public interface IPricingService
    {
    }


    public class PricingService
        : IPricingService
    {
        public PricingService(
            IAddonService arg0,
            IAddonPriceRepository arg1,
            IAddonCountryCurrencyRepository arg2,
            ICountryRepository arg3,
            ICurrencyBaseRepository arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IAddonService field0;
        public readonly IAddonPriceRepository field1;
        public readonly IAddonCountryCurrencyRepository field2;
        public readonly ICountryRepository field3;
        public readonly ICurrencyBaseRepository field4;
    }


    public interface IReimbursedFileQuotaService
    {
    }


    public class ReimbursedFileQuotaService
        : IReimbursedFileQuotaService
    {
        public ReimbursedFileQuotaService(
            IBilledRowRepository arg0,
            IAddonService arg1,
            IOrganizationPricingService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IBilledRowRepository field0;
        public readonly IAddonService field1;
        public readonly IOrganizationPricingService field2;
    }


    public interface IResellerService
    {
    }


    public class ResellerService
        : IResellerService
    {
        public ResellerService(
            IPartnerRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IPartnerRepository field0;
    }


    public class Addons
    {
    }


    public class DefaultAddonsService
        : IDefaultAddonsService
    {
        public DefaultAddonsService(
            IAddonTrialModeDefaultRepository arg0,
            IAddonPricingModelDefaultRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IAddonTrialModeDefaultRepository field0;
        public readonly IAddonPricingModelDefaultRepository field1;
    }


    public interface IDefaultAddonsService
    {
    }


    public class DoNothingBilledPaymentPdfBuilder
        : IBilledPaymentPdfBuilder
    {
        public DoNothingBilledPaymentPdfBuilder(
        )
        {
        }
    }


    public class ExportImportAddonIdentifier
    {
    }


    public class AddonCategory
    {
    }


    public class AddonPriceRule
    {
    }


    public class Addon : AddonFields
        , IIdentifiableEntityWithOriginalState<AddonFields>
    {
        public Addon(
        )
        {
        }
    }


    public class AddonActivationStatus
    {
    }


    public class AddonCurrencyBankAccount : AddonCurrencyBankAccountFields
        , IIdentifiableEntityWithOriginalState<AddonCurrencyBankAccountFields>
    {
        public AddonCurrencyBankAccount(
        )
        {
        }
    }


    public class AddonEx : Addon
    {
        public AddonEx(
        )
        {
        }
    }


    public class AddonExEx : AddonEx
    {
        public AddonExEx(
        )
        {
        }
    }


    public class AddonGroupCode
    {
        public AddonGroupCode(
        )
        {
        }
    }


    public class AddonPriceEx : AddonPrice
    {
        public AddonPriceEx(
        )
        {
        }
    }


    public class AddonPricingModelContent
    {
        public AddonPricingModelContent(
        )
        {
        }
    }


    public class AddonPricingModelDefault
    {
        public AddonPricingModelDefault(
        )
        {
        }
    }


    public class AddonRule : AddonRuleFields
        , IIdentifiableEntityWithOriginalState<AddonRuleFields>
    {
        public AddonRule(
        )
        {
        }
    }


    public class BilledPaymentStatus
    {
    }


    public class BilledPaymentType
    {
    }


    public class BilledPaymentPaymentType
    {
    }


    public class BilledPayment : BilledPaymentFields
        , IIdentifiableEntityWithOriginalState<BilledPaymentFields>
    {
        public BilledPayment(
        )
        {
        }
    }


    public class BilledPaymentEx : BilledPayment
    {
        public BilledPaymentEx(
        )
        {
        }
    }


    public class BilledRow : BilledRowFields
        , IIdentifiableEntityWithOriginalState<BilledRowFields>
    {
        public BilledRow(
        )
        {
        }
    }


    public class BilledRowEx : BilledRow
    {
        public BilledRowEx(
        )
        {
        }
    }


    public class BillingRuleStatus
    {
    }


    public class BillingRuleType
    {
    }


    public class BillingRule : BillingRuleFields
        , IIdentifiableEntityWithOriginalState<BillingRuleFields>
    {
        public BillingRule(
        )
        {
        }
    }


    public class BillingRuleRow : BillingRuleRowFields
        , IIdentifiableEntityWithOriginalState<BillingRuleRowFields>
    {
        public BillingRuleRow(
        )
        {
        }
    }


    public class BillingRuleRowType
    {
    }


    public class ExportImportAddon
    {
        public ExportImportAddon(
        )
        {
        }
    }


    public class AddonFields : IdentifiableEntity
    {
        public AddonFields(
        )
        {
        }
    }


    public class AddonActivationFields : OrganizationEntity
    {
        public AddonActivationFields(
        )
        {
        }
    }


    public class AddonActivation : AddonActivationFields
        , IIdentifiableEntityWithOriginalState<AddonActivationFields>
    {
        public AddonActivation(
        )
        {
        }
    }


    public class AddonCountryFields : IdentifiableEntity
    {
        public AddonCountryFields(
        )
        {
        }
    }


    public class AddonCountry : AddonCountryFields
        , IIdentifiableEntityWithOriginalState<AddonCountryFields>, IEntityRepository
    {
        public AddonCountry(
        )
        {
        }
    }


    public class AddonCountryCurrencyFields : IdentifiableEntity
    {
        public AddonCountryCurrencyFields(
        )
        {
        }
    }


    public class AddonCountryCurrency : AddonCountryCurrencyFields
        , IIdentifiableEntityWithOriginalState<AddonCountryCurrencyFields>, IEntityRepository
    {
        public AddonCountryCurrency(
        )
        {
        }
    }


    public class AddonCurrencyBankAccountFields : IdentifiableEntity
    {
        public AddonCurrencyBankAccountFields(
        )
        {
        }
    }


    public class AddonDependencyFields : IdentifiableEntity
    {
        public AddonDependencyFields(
        )
        {
        }
    }


    public class AddonDependency : AddonDependencyFields
        , IIdentifiableEntityWithOriginalState<AddonDependencyFields>, IEntityRepository
    {
        public AddonDependency(
        )
        {
        }
    }


    public class AddonPriceFields : IdentifiableEntity
    {
        public AddonPriceFields(
        )
        {
        }
    }


    public class AddonPrice : AddonPriceFields
        , IIdentifiableEntityWithOriginalState<AddonPriceFields>
    {
        public AddonPrice(
        )
        {
        }
    }


    public class AddonRuleFields : IdentifiableEntity
    {
        public AddonRuleFields(
        )
        {
        }
    }


    public class AddonRuleRowFields : IdentifiableEntity
    {
        public AddonRuleRowFields(
        )
        {
        }
    }


    public class AddonRuleRow : AddonRuleRowFields
        , IIdentifiableEntityWithOriginalState<AddonRuleRowFields>
    {
        public AddonRuleRow(
        )
        {
        }
    }


    public class AddonSettingFields : IdentifiableEntity
    {
        public AddonSettingFields(
        )
        {
        }
    }


    public class AddonSetting : AddonSettingFields
        , IIdentifiableEntityWithOriginalState<AddonSettingFields>
    {
        public AddonSetting(
        )
        {
        }
    }


    public class AddonTrialModeDefaultFields : IdentifiableEntity
    {
        public AddonTrialModeDefaultFields(
        )
        {
        }
    }


    public class AddonTrialModeDefault : AddonTrialModeDefaultFields
        , IIdentifiableEntityWithOriginalState<AddonTrialModeDefaultFields>
    {
        public AddonTrialModeDefault(
        )
        {
        }
    }


    public class AddonVatFields : IdentifiableEntity
    {
        public AddonVatFields(
        )
        {
        }
    }


    public class AddonVat : AddonVatFields
        , IIdentifiableEntityWithOriginalState<AddonVatFields>
    {
        public AddonVat(
        )
        {
        }
    }


    public class AddonGroupFields : IdentifiableEntity
        , INamedEntity
    {
        public AddonGroupFields(
        )
        {
        }
    }


    public class AddonGroup : AddonGroupFields
        , IIdentifiableEntityWithOriginalState<AddonGroupFields>
    {
        public AddonGroup(
        )
        {
        }
    }


    public class OrganizationAddonFields : OrganizationEntity
    {
        public OrganizationAddonFields(
        )
        {
        }
    }


    public class OrganizationAddon : OrganizationAddonFields
        , IIdentifiableEntityWithOriginalState<OrganizationAddonFields>
    {
        public OrganizationAddon(
        )
        {
        }
    }


    public class BilledPaymentFields : OrganizationEntity
    {
        public BilledPaymentFields(
        )
        {
        }
    }


    public class BilledRowFields : OrganizationEntity
    {
        public BilledRowFields(
        )
        {
        }
    }


    public class BillingRuleFields : OrganizationEntity
    {
        public BillingRuleFields(
        )
        {
        }
    }


    public class BillingRuleRowFields : OrganizationEntity
    {
        public BillingRuleRowFields(
        )
        {
        }
    }


    public class OrganizationResellerHistoryFields : OrganizationEntity
    {
        public OrganizationResellerHistoryFields(
        )
        {
        }
    }


    public class OrganizationResellerHistory : OrganizationResellerHistoryFields
        , IIdentifiableEntityWithOriginalState<OrganizationResellerHistoryFields>
    {
        public OrganizationResellerHistory(
        )
        {
        }
    }


    public class RecurringCharge : BillingRule
    {
        public RecurringCharge(
        )
        {
        }
    }


    public interface IAddonService
    {
    }


    public interface IBilledPaymentPdfBuilder
    {
    }


    public class AddonWithQuantity
    {
        public AddonWithQuantity(
            AddonIdentifier arg0,
            int arg1
        )
        {
            field1 = arg1;
        }

        public AddonWithQuantity(
            string arg0,
            int arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly int field1;
    }


    public interface IBillingService
    {
    }


    public interface IChangeEditionService
    {
    }


    public interface IOrganizationAddonService
    {
    }


    public class OrganizationAddonService : IOrganizationAddonService
    {
        public OrganizationAddonService(
            //IMasterOrganizationRepository arg0,
            //IAddonRepository arg1,
            //IAddonService arg2,
            //IAddonDependencyService arg3,
            //IBillingDatesResolver arg5,
            //IBilledRowRepository arg6,
            //IOrganizationPricingService arg7,
            //IOrganizationPaymentPeriodService arg8,
            //IOrganizationBillingCurrencyService arg10,
            //IAddonSettingRepository arg11,
            //IExternallyOwnedOrganizationService arg12,
            //IAddonRuleService arg13,
            //IAddonActivationRepository arg14,
            //IOrganizationAddonRepository arg15
        )
        {
            //field5 = arg5;
            //field6 = arg6;
            //field7 = arg7;
            //field8 = arg8;
            //field10 = arg10;
            //field11 = arg11;
            //field12 = arg12;
            //field13 = arg13;
            //field14 = arg14;
            //field15 = arg15;
        }

        public readonly IMasterOrganizationRepository field0;
        public readonly IAddonRepository field1;
        public readonly IAddonService field2;
        public readonly IAddonDependencyService field3;
        public readonly IBillingDatesResolver field5;
        public readonly IBilledRowRepository field6;
        public readonly IOrganizationPricingService field7;
        public readonly IOrganizationPaymentPeriodService field8;
        public readonly IOrganizationBillingRuleService field9;
        public readonly IOrganizationBillingCurrencyService field10;
        public readonly IAddonSettingRepository field11;
        public readonly IExternallyOwnedOrganizationService field12;
        public readonly IAddonRuleService field13;
        public readonly IAddonActivationRepository field14;
        public readonly IOrganizationAddonRepository field15;
    }


    public interface IOrganizationResellerService
    {
    }


    public class OrganizationResellerService
        : IOrganizationResellerService
    {
        public OrganizationResellerService(
            IDistributorService arg0,
            IResellerService arg1,
            IOrganizationService arg2,
            IOrganizationResellerHistoryRepository arg3,
            IOrganizationBillingRuleService arg4,
            IOrganizationPaymentPeriodService arg5,
            IPartnerPricingModelSettingRepository arg6
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IDistributorService field0;
        public readonly IResellerService field1;
        public readonly IOrganizationService field2;
        public readonly IOrganizationResellerHistoryRepository field3;
        public readonly IOrganizationBillingRuleService field4;
        public readonly IOrganizationPaymentPeriodService field5;
        public readonly IPartnerPricingModelSettingRepository field6;
    }


    public interface IReimburseBilledPaymentService
    {
    }


    public class ReimburseBilledPaymentService
        : IReimburseBilledPaymentService
    {
        public ReimburseBilledPaymentService(
            IBilledPaymentRepository arg0,
            IBilledRowRepository arg1,
            IHostingProviderService arg2,
            IOrganizationBillingRuleService arg3,
            IOrderConfirmationEmailBuilder arg4,
            IMailClient arg5,
            ITimeZoneService arg6,
            ITimeService arg7,
            ICreditNoteEmailBuilder arg8,
            IAddonRepository arg9,
            IReimbursedFileQuotaService arg10
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

        public readonly IBilledPaymentRepository field0;
        public readonly IBilledRowRepository field1;
        public readonly IHostingProviderService field2;
        public readonly IOrganizationBillingRuleService field3;
        public readonly IOrderConfirmationEmailBuilder field4;
        public readonly IMailClient field5;
        public readonly ITimeZoneService field6;
        public readonly ITimeService field7;
        public readonly ICreditNoteEmailBuilder field8;
        public readonly IAddonRepository field9;
        public readonly IReimbursedFileQuotaService field10;
    }


    public interface IAppApiGuidEncryptionMethodSettingService
    {
    }


    public class AppApiGuidEncryptionMethodSettingService
        : IAppApiGuidEncryptionMethodSettingService
    {
        public AppApiGuidEncryptionMethodSettingService(
            IAddonRepository arg0,
            IOrganizationAddonRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IAddonRepository field0;
        public readonly IOrganizationAddonRepository field1;
    }


    public interface IShopUserService
    {
    }

    public class ShopUserService : IShopUserService { }

    public class AddonNode
    {
        public AddonNode(
            Addon arg0
        )
        {
            field0 = arg0;
        }

        public readonly Addon field0;
    }


    public class AddonsAndPrices : List<PriceEntry>
    {
        public AddonsAndPrices(
            CurrencyBase arg0
        ) : base()
        {
        }
    }


    public class AddonStatus
    {
        public AddonStatus(
            string arg0,
            bool arg1,
            bool arg2,
            bool arg3,
            IEnumerable<String> arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly string field0;
        public readonly bool field1;
        public readonly bool field2;
        public readonly bool field3;
        public readonly IEnumerable<String> field4;
    }


    public class BalanceDue : List<PriceEntry>
    {
        public BalanceDue(
            CurrencyBase arg0,
            BillingDates arg1
        )
        {
            field1 = arg1;
        }

        public readonly BillingDates field1;
    }


    public class BalanceDueSummary : List<BalanceDueSummaryRow>
    {
        public BalanceDueSummary(
            CurrencyBase arg0
        )
        {
        }
    }


    public class BalanceDueSummaryRow
    {
        public BalanceDueSummaryRow(
            string arg0,
            decimal arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly decimal field1;
    }


    public class BillingDates
    {
        public BillingDates(
        )
        {
        }
    }


    public class BillingRuleChange
    {
        public BillingRuleChange(
        )
        {
        }
    }


    public class BillingRuleWithRows : BillingRule
    {
        public BillingRuleWithRows(
        )
        {
        }
    }


    public class PaymentPeriod
    {
        public PaymentPeriod(
            int arg0,
            decimal arg1,
            decimal? arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly int field0;
        public readonly decimal field1;
        public readonly decimal? field2;
    }


    public class PriceEntry
    {
        public PriceEntry(
            Addon arg0,
            decimal arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public PriceEntry(
            Addon arg0,
            decimal arg1,
            int arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly Addon field0;
        public readonly decimal field1;
        public readonly int field2;
    }


    public class PriceType
    {
    }


    public class ShopContext
    {
        public ShopContext(
            ShopUserType arg0
        )
        {
            field0 = arg0;
        }

        public readonly ShopUserType field0;
    }


    public class ShopUserType
    {
    }


    public struct UserSeats
    {
        public UserSeats(
            int arg0,
            int? arg1,
            int? arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly int field0;
        public readonly int? field1;
        public readonly int? field2;
    }


    public class AddonActivationRepository :
        MasterDatabaseRepository<MasterOrganization, AddonActivationFields, AddonActivation>
        , IAddonActivationRepository
    {
        public AddonActivationRepository(
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


    public class AddonActivationAppApiGuidRuleDecorator
        : IAddonActivationRepository
    {
        public AddonActivationAppApiGuidRuleDecorator(
            IAddonActivationRepository arg0,
            IAppApiGuidEncryptionMethodSettingService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IAddonActivationRepository field0;
        public readonly IAppApiGuidEncryptionMethodSettingService field1;
    }


    public class AddonCurrencyBankAccountRepository :
        EntityRepository<AddonCurrencyBankAccountFields, AddonCurrencyBankAccount>
        , IAddonCurrencyBankAccountRepository
    {
        public AddonCurrencyBankAccountRepository(
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


    public class AddonPriceRepository : EntityRepository<AddonPriceFields, AddonPrice>
        , IAddonPriceRepository
    {
        public AddonPriceRepository(
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


    public class AddonPricingModelContentRepository : RepositoryBase
        , IAddonPricingModelContentRepository
    {
        public AddonPricingModelContentRepository(
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


    public class AddonPricingModelDefaultRepository : RepositoryBase
        , IAddonPricingModelDefaultRepository
    {
        public AddonPricingModelDefaultRepository(
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


    public class AddonRepository : EntityRepository<AddonFields, Addon>
        , IAddonRepository
    {
        public AddonRepository(
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


    public class AddonSettingRepository : EntityRepository<AddonSettingFields, AddonSetting>
        , IAddonSettingRepository
    {
        public AddonSettingRepository(
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


    public class BilledRowRepository : MasterDatabaseRepository<MasterOrganization, BilledRowFields, BilledRow>
        , IBilledRowRepository
    {
        public BilledRowRepository(
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


    public class BillingAddressRepository :
        MasterDatabaseRepository<MasterOrganization, BillingAddressFields, BillingAddress>
        , IBillingAddressRepository
    {
        public BillingAddressRepository(
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


    public class BillingRuleRepository : MasterDatabaseRepository<MasterOrganization, BillingRuleFields, BillingRule>
        , IBillingRuleRepository
    {
        public BillingRuleRepository(
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


    public class BillingRuleRowRepository :
        MasterDatabaseRepository<MasterOrganization, BillingRuleRowFields, BillingRuleRow>
        , IBillingRuleRowRepository
    {
        public BillingRuleRowRepository(
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


    public interface IAddonCountryRepository
        : IEntityRepository<AddonCountry>
    {
    }


    public class AddonCountryRepository : EntityRepository<AddonCountryFields, AddonCountry>
        , IAddonCountryRepository
    {
        public AddonCountryRepository(
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


    public interface IAddonCountryCurrencyRepository
        : IEntityRepository<AddonCountryCurrency>
    {
    }


    public class AddonCountryCurrencyRepository : EntityRepository<AddonCountryCurrencyFields, AddonCountryCurrency>
        , IAddonCountryCurrencyRepository
    {
        public AddonCountryCurrencyRepository(
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


    public interface IAddonDependencyRepository
        : IEntityRepository<AddonDependency>
    {
    }


    public class AddonDependencyRepository : EntityRepository<AddonDependencyFields, AddonDependency>
        , IAddonDependencyRepository
    {
        public AddonDependencyRepository(
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


    public interface IAddonRuleRepository
        : IEntityRepository<AddonRule>
    {
    }


    public class AddonRuleRepository : EntityRepository<AddonRuleFields, AddonRule>
        , IAddonRuleRepository
    {
        public AddonRuleRepository(
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


    public interface IAddonRuleRowRepository
        : IEntityRepository<AddonRuleRow>
    {
    }


    public class AddonRuleRowRepository : EntityRepository<AddonRuleRowFields, AddonRuleRow>
        , IAddonRuleRowRepository
    {
        public AddonRuleRowRepository(
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


    public interface IAddonTrialModeDefaultRepository
        : IEntityRepository<AddonTrialModeDefault>
    {
    }


    public class AddonTrialModeDefaultRepository : EntityRepository<AddonTrialModeDefaultFields, AddonTrialModeDefault>
        , IAddonTrialModeDefaultRepository
    {
        public AddonTrialModeDefaultRepository(
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


    public interface IAddonVatRepository
        : IEntityRepository<AddonVat>
    {
    }


    public class AddonVatRepository : EntityRepository<AddonVatFields, AddonVat>
        , IAddonVatRepository
    {
        public AddonVatRepository(
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


    public interface IAddonGroupRepository
        : IEntityRepository<AddonGroup>
    {
    }


    public class AddonGroupRepository : EntityRepository<AddonGroupFields, AddonGroup>
        , IAddonGroupRepository
    {
        public AddonGroupRepository(
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


    public class OrganizationAddonRepository :
        MasterDatabaseRepository<MasterOrganization, OrganizationAddonFields, OrganizationAddon>
        , IOrganizationAddonRepository
    {
        public OrganizationAddonRepository(
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


    public class BilledPaymentRepository :
        MasterDatabaseRepository<MasterOrganization, BilledPaymentFields, BilledPayment>
        , IBilledPaymentRepository
    {
        public BilledPaymentRepository(
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


    public class OrganizationResellerHistoryRepository : MasterDatabaseRepository<MasterOrganization,
            OrganizationResellerHistoryFields, OrganizationResellerHistory>
        , IOrganizationResellerHistoryRepository
    {
        public OrganizationResellerHistoryRepository(
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


    public interface IAddonActivationRepository
        : IRepository<MasterOrganization, BilledPayment>
    {
    }


    public interface IAddonCurrencyBankAccountRepository
        : IEntityRepository<AddonCurrencyBankAccount>
    {
    }


    public interface IAddonPriceRepository
        : IEntityRepository<AddonPrice>
    {
    }


    public interface IAddonPricingModelContentRepository
    {
    }


    public interface IAddonPricingModelDefaultRepository
    {
    }


    public interface IAddonRepository
        : IEntityRepository<Addon>
    {
    }


    public interface IAddonSettingRepository
        : IEntityRepository<AddonSetting>
    {
    }


    public interface IBilledPaymentRepository
        : IRepository<MasterOrganization, BilledPayment>
    {
    }


    public interface IBilledRowRepository
        : IRepository<MasterOrganization, BilledRow>
    {
    }


    public interface IBillingAddressRepository
        : IWritableRepository<BillingAddress>
    {
    }


    public interface IBillingRuleRepository
        : IRepository<MasterOrganization, BillingRule>
    {
    }


    public interface IBillingRuleRowRepository
        : IRepository<MasterOrganization, BillingRuleRow>
    {
    }


    public interface IOrganizationAddonRepository
        : IRepository<MasterOrganization, OrganizationAddon>
    {
    }


    public interface IOrganizationResellerHistoryRepository
        : IRepository<MasterOrganization, OrganizationResellerHistory>
    {
    }
}