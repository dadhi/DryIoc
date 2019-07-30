using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using Background;
using Conn.Adapter;
using RM;
using CurrencyRounding;
using Data;
using Entities;
using Framework;
using Scanner;
using OrganizationBase;
using Organizations;
using Search;
using Shared;
using Shop;
using Pdf;
using Users;
using Utilities;
using File = Entities.File;
using IInvoicingContactService = Organizations.IInvoicingContactService;
using Invoice = Entities.Invoice;
using IOrganizationService = CUsers.ApplicationServices.IOrganizationService;
using Organization = Organizations.Organization;
using SearchCriteria = Search.SearchCriteria;
using Settings = OrganizationBase.Settings;

namespace Logic
{
    public class InternalXmlHelper
    {
    }


    public interface IPartnerService
    {
    }


    public class PartnerService
        : IPartnerService
    {
        public PartnerService(
            IPartnerRepository arg0,
            IPartnerEmailRepository arg1,
            IOrganizationRepository arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IPartnerRepository field0;
        public readonly IPartnerEmailRepository field1;
        public readonly IOrganizationRepository field2;
    }


    public class AutoTag
    {
        public AutoTag(
            string arg0,
            Phrase arg1,
            Func<String> arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly string field0;
        public readonly Phrase field1;
        public readonly Func<String> field2;
    }


    public class BaseAutoTags
    {
        public BaseAutoTags() { }

        public BaseAutoTags(
            IContextService<IPsaContext> arg0,
            IAccountRepository arg1,
            IBusinessUnitService arg2,
            IAddressRepository arg3,
            ICompanyRepository arg4,
            IOrganizationCompanyRepository arg5
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
        public readonly IAccountRepository field1;
        public readonly IBusinessUnitService field2;
        public readonly IAddressRepository field3;
        public readonly ICompanyRepository field4;
        public readonly IOrganizationCompanyRepository field5;
    }


    public class InvoiceFooterAutoTags : BaseAutoTags
    {
        public InvoiceFooterAutoTags(
            BusinessUnit arg0,
            IContextService<IPsaContext> arg1,
            ICountryRepository arg2,
            IBusinessUnitService arg3,
            IAccountRepository arg4,
            IAddressRepository arg5,
            ICompanyRepository arg6,
            IOrganizationCompanyRepository arg7
        ) : base()
        {
            field6 = arg6;
            field7 = arg7;
        }

        public readonly ICompanyRepository field6;
        public readonly IOrganizationCompanyRepository field7;
    }


    public class InvoiceFooterTags
    {
        public InvoiceFooterTags(
        )
        {
        }
    }


    public class InvoiceNoteAutoTags : BaseAutoTags
    {
        public InvoiceNoteAutoTags(
            Invoice arg0,
            IContextService<IPsaContext> arg1,
            IAccountRepository arg2,
            IBusinessUnitService arg3,
            IAddressRepository arg4,
            ICompanyRepository arg5,
            ICaseRepository arg6,
            IUserRepository arg7,
            IOrganizationCompanyRepository arg8
        ) : base()
        {
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
        }

        public readonly ICaseRepository field6;
        public readonly IUserRepository field7;
        public readonly IOrganizationCompanyRepository field8;
    }


    public class InvoiceNoteTags
    {
        public InvoiceNoteTags(
        )
        {
        }
    }


    public class OfferNoteAutoTags : BaseAutoTags
    {
        public OfferNoteAutoTags(
            Case arg0,
            IContextService<IPsaContext> arg1,
            IAccountRepository arg2,
            IBusinessUnitService arg3,
            IAddressRepository arg4,
            ICompanyRepository arg5,
            IOrganizationCompanyRepository arg6
        ) : base()
        {
            field6 = arg6;
        }

        public readonly IOrganizationCompanyRepository field6;
    }


    public class OfferNoteTags
    {
        public OfferNoteTags(
        )
        {
        }
    }


    public class MailBuilder
    {
        public MailBuilder(
        )
        {
        }
    }


    public class WHApprovalLogicHandler
    {
        public WHApprovalLogicHandler(
        )
        {
        }
    }


    public interface IVatNumberValidator
    {
    }


    public class VatNumberValidator
        : IVatNumberValidator
    {
        public VatNumberValidator(
            IContextService<IPsaContext> arg0,
            ICountryRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICountryRepository field1;
    }


    public class TaskNameBuilder
    {
        public TaskNameBuilder(
            ITaskNameable arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly ITaskNameable field0;
        public readonly IPsaContextService field1;
    }


    public class AllowAllAuthorization
        : IAuthorization
    {
        public AllowAllAuthorization(
        )
        {
        }
    }


    public class AuthorizationBase
    {
    }


    public class PsaEntityAllowAllAuthorization<TEntity> : PsaEntityAuthorization<TEntity> where TEntity : IOrganizationEntity
    {
        public PsaEntityAllowAllAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }

    public class SettingsAuthorization<TEntity> : AuthorizationBase<IPsaContext, TEntity>
        , IAuthorization
    {
        public SettingsAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public interface IContainerAccessor
    {
    }


    public interface ITokenService
    {
    }


    public class TokenService
        : ITokenService
    {
        public TokenService(
            IOrganizationTrustedService arg0,
            IUserInfoTokenService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IOrganizationTrustedService field0;
        public readonly IUserInfoTokenService field1;
    }


    public class OrganizationSettingsService<TEntity, TRepository> : OrganizationEntityService<TEntity, TRepository, User, IPsaContext> where TRepository : IEntityRepository<TEntity> where TEntity : IOrganizationEntity
    {
        public OrganizationSettingsService(IContextService<IPsaContext> arg0) : base(arg0)
        {
        }

        public OrganizationSettingsService(IContextService<IPsaContext> contextService, TRepository repository, IValidator<TEntity> validator, IAuthorization<IPsaContext, TEntity> authorization) : base(contextService, repository, validator, authorization)
        {
        }
    }


    public interface IPdfCreationHandlerService
    {
    }


    public class PdfCreationHandlerService
        : IPdfCreationHandlerService
    {
        public PdfCreationHandlerService(
            IInvoiceFileService arg1,
            IPdfProposalDocumentService arg5,
            IOfferService arg8
        )
        {
        }
    }


    public interface IPdfInvoiceDocumentService
    {
    }


    public class PdfInvoiceDocumentService : PdfDocumentServiceBase
        , IPdfInvoiceDocumentService
    {
        public PdfInvoiceDocumentService(
            IContextService<IPsaContext> arg0,
            IInvoiceFileService arg9
        ) : base(arg0)
        {
        }
    }


    public interface IPdfProposalDocumentService
    {
    }


    public class PdfProposalDocumentService : PdfDocumentServiceBase
        , IPdfProposalDocumentService
    {
        public PdfProposalDocumentService(
            IContextService<IPsaContext> arg0,
            IOfferService arg5,
            ICaseService arg6,
            IProposalTaxBreakdownService arg12
        ) : base(arg0)
        {

        }
    }


    public interface IPdfTravelReimbursementDocumentService
    {
    }


    public class PdfTravelReimbursementDocumentService : PdfDocumentServiceBase
        , IPdfTravelReimbursementDocumentService
    {
        public PdfTravelReimbursementDocumentService(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1,
            IFileService arg2,
            IFileDataService arg3,
            IBusinessUnitService arg4,
            IFormatingCultureRepository arg5,
            IUserRepository arg6,
            IItemFileRepository arg7,
            IImageResizer arg8
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
        }

        public readonly ICurrencyService field1;
        public readonly IFileService field2;
        public readonly IFileDataService field3;
        public readonly IBusinessUnitService field4;
        public readonly IFormatingCultureRepository field5;
        public readonly IUserRepository field6;
        public readonly IItemFileRepository field7;
        public readonly IImageResizer field8;
    }


    public class PdfDocumentServiceBase
    {
        public PdfDocumentServiceBase(
            IContextService<IPsaContext> arg0
        )
        {
            field0 = arg0;
        }

        public readonly IContextService<IPsaContext> field0;
    }


    public class PsaIocModule
    {
    }


    public class TelerikRecurrenceRule
    {
        public TelerikRecurrenceRule(
            string arg0
        )
        {
            field0 = arg0;
        }

        public readonly string field0;
    }


    public class CustomFormulaAuthorization
        : IAuthorization<IPsaContext, CustomFormula>
    {
        public CustomFormulaAuthorization(
        )
        {
        }
    }


    public class CustomFormulaService :
        OrganizationEntityService<CustomFormula, ICustomFormulaRepository, User, IPsaContext>
        , ICustomFormulaService
    {
        public CustomFormulaService(
            IContextService<IPsaContext> arg0,
            ICustomFormulaRepository arg1,
            IValidator<CustomFormula> arg2,
            IAuthorization<IPsaContext, CustomFormula> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICustomFormulaRepository field1;
        public readonly IValidator<CustomFormula> field2;
        public readonly IAuthorization<IPsaContext, CustomFormula> field3;
    }


    public class CustomFormulaSetAuthorization
        : IAuthorization<IPsaContext, CustomFormulaSet>
    {
        public CustomFormulaSetAuthorization(
        )
        {
        }
    }


    public class CustomFormulaSetService :
        OrganizationEntityService<CustomFormulaSet, ICustomFormulaSetRepository, User, IPsaContext>
        , ICustomFormulaSetService
    {
        public CustomFormulaSetService(
            IContextService<IPsaContext> arg0,
            ICustomFormulaSetRepository arg1,
            IValidator<CustomFormulaSet> arg2,
            IAuthorization<IPsaContext, CustomFormulaSet> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICustomFormulaSetRepository field1;
        public readonly IValidator<CustomFormulaSet> field2;
        public readonly IAuthorization<IPsaContext, CustomFormulaSet> field3;
    }


    public interface ICustomFormulaService
        : IEntityService<CustomFormula>
    {
    }


    public interface ICustomFormulaSetService
        : IEntityService<CustomFormulaSet>
    {
    }


    public interface IKpiGeneratorService
    {
    }


    public class KpiGeneratorService
        : IKpiGeneratorService
    {
        public KpiGeneratorService(
            IDict arg0,
            ICustomFormulaService arg1,
            ICustomFormulaSetService arg2,
            IOrganizationRepository arg3,
            ICurrencyRepository arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IDict field0;
        public readonly ICustomFormulaService field1;
        public readonly ICustomFormulaSetService field2;
        public readonly IOrganizationRepository field3;
        public readonly ICurrencyRepository field4;
    }


    public class BillingManagerFactory
        : IBillingManagerFactory
    {
        public BillingManagerFactory(
        )
        {
        }
    }


    public class BillingManager
        : IBillingManager
    {
        public BillingManager(
        )
        {
        }
    }


    public interface IBillingManagerFactory
    {
    }


    public interface IBillingManager
    {
    }


    public interface IAddonInfo
    {
    }


    public class CreatedOrganizationContext
    {
        public CreatedOrganizationContext(
            Registration arg0,
            User arg1,
            Organization arg2,
            Company arg3,
            Address arg4,
            OrganizationInfoForHostingProviderAccount arg5
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly Registration field0;
        public readonly User field1;
        public readonly Organization field2;
        public readonly Company field3;
        public readonly Address field4;
        public readonly OrganizationInfoForHostingProviderAccount field5;
    }


    public class CreatedOrganizationContextFactory
        : ICreatedOrganizationContextFactory
    {
        public CreatedOrganizationContextFactory(
            IAccountRepository arg0,
            ICompanyRepository arg1,
            IAddressRepository arg2,
            IMasterOrganizationRepository arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IAccountRepository field0;
        public readonly ICompanyRepository field1;
        public readonly IAddressRepository field2;
        public readonly IMasterOrganizationRepository field3;
    }


    public class CreateOrganizationDefaultsService
        : ICreateOrganizationDefaultsService
    {
        public CreateOrganizationDefaultsService(
            ICountryService arg0,
            IPricelistService arg1,
            IAccountNavigationHistoryService arg2,
            IDefaultCaseOverviewSettingsService arg3,
            IWorkHourOverviewReportService arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly ICountryService field0;
        public readonly IPricelistService field1;
        public readonly IAccountNavigationHistoryService field2;
        public readonly IDefaultCaseOverviewSettingsService field3;
        public readonly IWorkHourOverviewReportService field4;
    }


    public class DefaultCaseOverviewSettingsService
        : IDefaultCaseOverviewSettingsService
    {
        public DefaultCaseOverviewSettingsService(
            IOrganizationSettingsService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IOrganizationSettingsService field0;
    }


    public class HostingOrganizationAccountService
        : IHostingOrganizationAccountService
    {
        public HostingOrganizationAccountService(
            IAccountRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IAccountRepository field0;
    }


    public class HostingOrganizationAddressService
        : IHostingOrganizationAddressService
    {
        public HostingOrganizationAddressService(
            ICompanyRepository arg0,
            IAddressRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly ICompanyRepository field0;
        public readonly IAddressRepository field1;
    }


    public class HostingOrganizationCaseSalesStatusService
        : IHostingOrganizationCaseSalesStatusService
    {
        public HostingOrganizationCaseSalesStatusService(
            ISalesProcessRepository arg0,
            ICountryRepository arg1,
            ICaseRepository arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ISalesProcessRepository field0;
        public readonly ICountryRepository field1;
        public readonly ICaseRepository field2;
    }


    public class HostingOrganizationCaseService
        : IHostingOrganizationCaseService
    {
        public HostingOrganizationCaseService(
            ICaseRepository arg0,
            IUserRepository arg1,
            ICountryRepository arg2,
            IBusinessUnitRepository arg3,
            ICostCenterRepository arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly ICaseRepository field0;
        public readonly IUserRepository field1;
        public readonly ICountryRepository field2;
        public readonly IBusinessUnitRepository field3;
        public readonly ICostCenterRepository field4;
    }


    public class HostingOrganizationCaseTagService
        : IHostingOrganizationCaseTagService
    {
        public HostingOrganizationCaseTagService(
            ITagRepository arg0,
            ICaseTagRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly ITagRepository field0;
        public readonly ICaseTagRepository field1;
    }


    public class HostingOrganizationCompanyService
        : IHostingOrganizationCompanyService
    {
        public HostingOrganizationCompanyService(
            IIndustryRepository arg0,
            ICompanyRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IIndustryRepository field0;
        public readonly ICompanyRepository field1;
    }


    public class HostingOrganizationContactService
        : IHostingOrganizationContactService
    {
        public HostingOrganizationContactService(
            IContactRepository arg0,
            ICommunicationMethodRepository arg1,
            ICommunicatesWithRepository arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContactRepository field0;
        public readonly ICommunicationMethodRepository field1;
        public readonly ICommunicatesWithRepository field2;
    }


    public class HostingOrganizationContext
    {
        public HostingOrganizationContext(
            Organization arg0,
            Account arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly Organization field0;
        public readonly Account field1;
    }


    public class HostingOrganizationCreateCustomerService
        : IHostingOrganizationCreateCustomerService
    {
        public HostingOrganizationCreateCustomerService(
            IOrganizationHostingProviderService arg0,
            IOrganizationContextScopeFactory arg1,
            IAccountRepository arg2,
            IHostingOrganizationCustomerService arg3,
            IHostingOrganizationSalesCaseService arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IOrganizationHostingProviderService field0;
        public readonly IOrganizationContextScopeFactory field1;
        public readonly IAccountRepository field2;
        public readonly IHostingOrganizationCustomerService field3;
        public readonly IHostingOrganizationSalesCaseService field4;
    }


    public class HostingOrganizationCustomerNameService
        : IHostingOrganizationCustomerNameService
    {
        public HostingOrganizationCustomerNameService(
            IAccountRepository arg0,
            ICompanyRepository arg1,
            IOrganizationHostingProviderRepository arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IAccountRepository field0;
        public readonly ICompanyRepository field1;
        public readonly IOrganizationHostingProviderRepository field2;
    }


    public class HostingOrganizationCustomerService
        : IHostingOrganizationCustomerService
    {
        public HostingOrganizationCustomerService(
            IHostingOrganizationCustomerNameService arg0,
            IHostingOrganizationCompanyService arg1,
            IHostingOrganizationAccountService arg2,
            IHostingOrganizationAddressService arg3,
            IHostingOrganizationContactService arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IHostingOrganizationCustomerNameService field0;
        public readonly IHostingOrganizationCompanyService field1;
        public readonly IHostingOrganizationAccountService field2;
        public readonly IHostingOrganizationAddressService field3;
        public readonly IHostingOrganizationContactService field4;
    }


    public class HostingOrganizationSalesCaseService
        : IHostingOrganizationSalesCaseService
    {
        public HostingOrganizationSalesCaseService(
            IHostingOrganizationCaseService arg0,
            IHostingOrganizationCaseTagService arg1,
            IHostingOrganizationCaseSalesStatusService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IHostingOrganizationCaseService field0;
        public readonly IHostingOrganizationCaseTagService field1;
        public readonly IHostingOrganizationCaseSalesStatusService field2;
    }


    public interface ICreatedOrganizationContextFactory
    {
    }


    public interface ICreateOrganizationDefaultsService
    {
    }


    public interface IDefaultCaseOverviewSettingsService
    {
    }


    public interface IHostingOrganizationAccountService
    {
    }


    public interface IHostingOrganizationAddressService
    {
    }


    public interface IHostingOrganizationCaseSalesStatusService
    {
    }


    public interface IHostingOrganizationCaseService
    {
    }


    public interface IHostingOrganizationCaseTagService
    {
    }


    public interface IHostingOrganizationCompanyService
    {
    }


    public interface IHostingOrganizationContactService
    {
    }


    public interface IHostingOrganizationCreateCustomerService
    {
    }


    public interface IHostingOrganizationCustomerNameService
    {
    }


    public interface IHostingOrganizationCustomerService
    {
    }


    public interface IHostingOrganizationSalesCaseService
    {
    }


    public interface ILinkOrganizationToHostingProviderService
    {
    }


    public class LinkOrganizationToHostingProviderService
        : ILinkOrganizationToHostingProviderService
    {
        public LinkOrganizationToHostingProviderService(
            ICreatedOrganizationContextFactory arg0,
            IHostingOrganizationCreateCustomerService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly ICreatedOrganizationContextFactory field0;
        public readonly IHostingOrganizationCreateCustomerService field1;
    }


    public interface IOrganizationContextScopeFactory
    {
    }


    public class OrganizationContextScopeFactory
        : IOrganizationContextScopeFactory
    {
        public OrganizationContextScopeFactory(
            IMasterOrganizationRepository arg0,
            IUserRepository arg1,
            IOrganizationRepository arg2,
            IOrganizationContextScopeService arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IMasterOrganizationRepository field0;
        public readonly IUserRepository field1;
        public readonly IOrganizationRepository field2;
        public readonly IOrganizationContextScopeService field3;
    }


    public interface IOrganizationHostingProviderService
    {
    }


    public class OrganizationHostingProviderService
        : IOrganizationHostingProviderService
    {
        public OrganizationHostingProviderService(
            IOrganizationHostingProviderRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IOrganizationHostingProviderRepository field0;
    }


    public interface IRegistrationEmailBuilder
    {
    }


    public class RegistrationEmailBuilder
        : IRegistrationEmailBuilder
    {
        public RegistrationEmailBuilder(
            IPartnerService arg0,
            IMailContentBuilder arg1,
            IAppSettings arg2,
            IHttpUtility arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPartnerService field0;
        public readonly IMailContentBuilder field1;
        public readonly IAppSettings field2;
        public readonly IHttpUtility field3;
    }


    public interface ISendRegistrationEmailsService
    {
    }


    public class SendRegistrationEmailsService
        : ISendRegistrationEmailsService
    {
        public SendRegistrationEmailsService(
            ISendRegistrationEmailToUserService arg0,
            ISendRegistrationEmailToSalesService arg1,
            IRegistrationEmailBuilder arg2,
            IMailClient arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly ISendRegistrationEmailToUserService field0;
        public readonly ISendRegistrationEmailToSalesService field1;
        public readonly IRegistrationEmailBuilder field2;
        public readonly IMailClient field3;
    }


    public interface ISendRegistrationEmailToSalesService
    {
    }


    public class SendRegistrationEmailToSalesService
        : ISendRegistrationEmailToSalesService
    {
        public SendRegistrationEmailToSalesService(
            IAppSettings arg0,
            IMailClient arg1,
            ICountryService arg2,
            ICountryRegionService arg3,
            ICurrencyService arg4,
            ILanguageService arg5,
            ITimeZoneService arg6,
            IOrganizationContextScopeFactory arg7
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

        public readonly IAppSettings field0;
        public readonly IMailClient field1;
        public readonly ICountryService field2;
        public readonly ICountryRegionService field3;
        public readonly ICurrencyService field4;
        public readonly ILanguageService field5;
        public readonly ITimeZoneService field6;
        public readonly IOrganizationContextScopeFactory field7;
    }


    public interface ISendRegistrationEmailToUserService
    {
    }


    public class SendRegistrationEmailToUserService
        : ISendRegistrationEmailToUserService
    {
        public SendRegistrationEmailToUserService(
            IDict arg0,
            IEmailTemplateService arg1,
            IHttpUtility arg2,
            IConnClientService arg3,
            IDistributorHelperService arg4,
            IMailClient arg5,
            IAppSettings arg6
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

        public readonly IDict field0;
        public readonly IEmailTemplateService field1;
        public readonly IHttpUtility field2;
        public readonly IConnClientService field3;
        public readonly IDistributorHelperService field4;
        public readonly IMailClient field5;
        public readonly IAppSettings field6;
    }


    public interface IAccountGroupIDReportFilterHandler
    {
    }


    public class AccountListReportHandler : PsaListReportHandler
    {

        protected AccountListReportHandler(IContextService<IPsaContext> contextService, ICurrencyService currencyService, IGuidService guidService) : base(contextService, currencyService, guidService)
        {
        }
    }



public class AccountOverviewReportHandler : AccountListReportHandler
    {
        protected AccountOverviewReportHandler(IContextService<IPsaContext> contextService, ICurrencyService currencyService, IGuidService guidService) : base(contextService, currencyService, guidService)
        {
        }
    }


    public class ActivityListReportHandler : PsaListReportHandler
    {
        public ActivityListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class AuditTrailListReportHandler : PsaListReportHandler
    {
        public AuditTrailListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class BusinessUnitListReportHandler : PsaListReportHandler
    {
        public BusinessUnitListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class CaseFileListReportHandler : PsaListReportHandler
    {
        public CaseFileListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class CaseFinancialOverviewReportHandler : CaseListReportHandler
    {
        public CaseFinancialOverviewReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class CaseListReportHandler : PsaListReportHandler
    {
        public CaseListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class CaseToInvoiceSchedulerFilterHandler : CaseToInvoiceListReportHandler
    {
    }

    public class CaseToInvoiceListReportHandler
    {
    }


    public class CaseOverviewReportHandler : CaseListReportHandler
    {
        public CaseOverviewReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class CaseTimelineReportHandler : PsaTimelineReportHandler
    {
        public CaseTimelineReportHandler(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1,
            IGuidService arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class HourApprovalTimelineReportHandler : PsaTimelineReportHandler
    {
        public HourApprovalTimelineReportHandler(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1,
            IGuidService arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class HourTimelineReportHandler : PsaTimelineReportHandler
    {
        public HourTimelineReportHandler(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1,
            IGuidService arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class ItemTimelineReportHandler : PsaTimelineReportHandler
    {
        public ItemTimelineReportHandler(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1,
            IGuidService arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class ResourceAllocationTimelineReportHandler : PsaTimelineReportHandler
    {
        public ResourceAllocationTimelineReportHandler(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1,
            IGuidService arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class UserTimelineReportHandler : PsaTimelineReportHandler
    {
        public UserTimelineReportHandler(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1,
            IGuidService arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class CommunicationMethodReportField : ReportField, IReportField
    {
        public CommunicationMethodReportField(
            int? arg0,
            string arg1,
            int? arg2,
            Nullable<AggregateFunction> arg3,
            bool arg4
        ) : base()
        {
        }
    }


    public class CommunicationMethodReportFieldHandler : LinkListReportFieldHandler<IPsaContext, CommunicationMethodAccessInfo, CommunicationMethodReportField>
    {
        public CommunicationMethodReportFieldHandler(
            int arg0,
            string arg1,
            CreateCommunicationMethodParameterDelegate arg2,
            GetTranslationDelegate arg3,
            int? arg4
        ) : base()
        {
        }
    }

    public class CommunicationMethodAccessInfo : IEntityAccessInfo<IPsaContext>
    {
    }


    public class ContactListReportHandler : PsaListReportHandler
    {
        public ContactListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class EmailAllowedLinkListReportFieldHandler : LinkListReportFieldHandler
    {
        public EmailAllowedLinkListReportFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            string arg3,
            Phrase arg4,
            Phrase arg5,
            int? arg6
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6)
        {
        }
    }


    public class HourListReportHandler : PsaListReportHandler
    {
        public HourListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class HourOverviewReportHandler : UserListReportHandler
    {
        public HourOverviewReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class InvoiceListReportHandler : PsaListReportHandler
    {
        public InvoiceListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class InvoiceRowListReportHandler : PsaListReportHandler
    {
        public InvoiceRowListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public interface IPsaReportFactoryService
        : IReportFactoryService<IPsaContext>
    {
    }


    public class ItemListReportHandler : PsaListReportHandler
    {
        public ItemListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public interface IWorkHourOverviewReportHandler
    {
    }


    public class WorkHourOverviewReportHandler
        : IWorkHourOverviewReportHandler
    {
        public WorkHourOverviewReportHandler(
        )
        {
        }
    }


    public class OfferReportHandler : PsaListReportHandler
    {
        public OfferReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class OfferRowListReportHandler : PsaListReportHandler
    {
        public OfferRowListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class PictureFileReportFieldHandler : ReportFieldHandler<StringReportField>
    {
        public PictureFileReportFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            string arg3,
            string arg4,
            string arg5,
            string arg6,
            RowActionInfo arg7,
            Phrase arg8,
            Phrase arg9,
            int? arg10
        ) : base()
        {
            field9 = arg9;
            field10 = arg10;
        }

        public readonly Phrase field9;
        public readonly int? field10;
    }


    public class PsaActions
    {
        public PsaActions(
        )
        {
        }
    }


    public class PsaDrillDownReportActionInfo<TReport> : DrillDownReportActionInfo
    {
        public PsaDrillDownReportActionInfo(
            string arg0,
            Phrase arg1,
            ICollection<String> arg2,
            ICollection<Parameter> arg3,
            CheckRowActionRightsDelegate<IPsaContext> arg4,
            string arg5
        ) : base()
        {
        }

        protected PsaDrillDownReportActionInfo()
        {
        }
    }


    public class PsaEntityListDataFieldGroup<TEntity> : EntityListDataFieldGroup<IPsaContext, SearchCriteria, TEntity> where TEntity : IIdentifiableEntity
    {
        public PsaEntityListDataFieldGroup(
            IGuidService arg0,
            ICrossTabReport arg1,
            string arg2,
            string arg3,
            string arg4,
            bool arg6,
            bool arg7,
            bool arg8,
            string arg9,
            string arg10
        ) : base()
        {
        }

        public PsaEntityListDataFieldGroup(
            IGuidService arg0,
            ICrossTabReport arg1,
            XElement arg2,
            string arg3,
            string arg4,
            string arg5,
            bool arg7,
            string arg8,
            string arg9
        ) : base()
        {
        }

        public PsaEntityListDataFieldGroup()
        {
        }
    }


    public class PsaListDrillDownReportActionInfo : PsaDrillDownReportActionInfo<ListReport>
    {
        public PsaListDrillDownReportActionInfo(
            string arg0,
            Phrase arg1,
            ICollection<String> arg2,
            ICollection<Parameter> arg3,
            CheckRowActionRightsDelegate<IPsaContext> arg4,
            string arg5
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5)
        {
        }

        protected PsaListDrillDownReportActionInfo()
        {
        }
    }

    public abstract class ListReportHandler<TContext> : ReportWithFiltersHandler<TContext, ListReport>, IListReportHandler<TContext> where TContext : IContext
    {

        protected ListReportHandler(IContextService<TContext> contextService) : base(contextService)
        {
        }
    }


    public class PsaListReportHandler : ListReportHandler<IPsaContext>
    {
        public PsaListReportHandler(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1,
            IGuidService arg2
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICurrencyService field1;
        public readonly IGuidService field2;
    }


    public class PsaRowActionInfo : RowActionInfo<IPsaContext>
    {
        public PsaRowActionInfo(
            string arg0,
            string arg1,
            string arg2,
            ICollection<String> arg3,
            ICollection<Parameter> arg4,
            ICollection<KeyValuePair<string, object>> arg5,
            CheckRowActionRightsDelegate<IPsaContext> arg6
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6)
        {
        }
    }

    public abstract class TimelineReportHandler<TContext> : CrossTabReportHandler<TContext, ICrossTabReport> where TContext : IContext
    {
        protected TimelineReportHandler(IContextService<TContext> contextService) : base(contextService)
        {
        }
    }

    public class PsaTimelineReportHandler : TimelineReportHandler<IPsaContext>
    {
        public PsaTimelineReportHandler(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1,
            IGuidService arg2
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICurrencyService field1;
        public readonly IGuidService field2;
    }


    public class PurchaseOrderItemListReportHandler : PsaListReportHandler
    {
        public PurchaseOrderItemListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class RepertoryChartReport : ChartReport
    {
        public RepertoryChartReport(
        )
        {
        }
    }


    public class RepertoryChartReportHandler : PsaChartReportHandler<RepertoryChartReport>
    {
        public RepertoryChartReportHandler(IContextService<IPsaContext> contextService, ICurrencyService currencyService, IGuidService guidService) : base(contextService, currencyService, guidService)
        {
        }
    }


    public class ResourceAllocationByUserGraphReport : ReportWithFilters
    {
        public ResourceAllocationByUserGraphReport(
        )
        {
        }
    }


    public class ResourceAllocationByUserGraphReportHandler : ReportWithFiltersHandler<IPsaContext, ResourceAllocationByUserGraphReport>
    {
        public ResourceAllocationByUserGraphReportHandler(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
            field0 = arg0;
        }

        public readonly IContextService<IPsaContext> field0;
    }


    public class ResourceAllocationOverviewGraphReport : ChartReport
    {
        public ResourceAllocationOverviewGraphReport(
        )
        {
        }

        public ResourceAllocationOverviewGraphReport(
            ResourceAllocationSearchCriteria arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ResourceAllocationSearchCriteria field0;
    }


    public class ResourceAllocationOverviewGraphReportHandler : ResourceAllocationOverviewGraphReportHandlerBase<ResourceAllocationOverviewGraphReport>
    {
        public ResourceAllocationOverviewGraphReportHandler(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1,
            IGuidService arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class ResourceAllocationOverviewGraphReportHandlerBase<TReport> : PsaChartReportHandler<TReport> where TReport : IChartReport
    {
        public ResourceAllocationOverviewGraphReportHandlerBase(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1,
            IGuidService arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class ResourceAllocationOverviewInHoursGraphReport : ChartReport
    {
        public ResourceAllocationOverviewInHoursGraphReport(
        )
        {
        }

        public ResourceAllocationOverviewInHoursGraphReport(
            ResourceAllocationSearchCriteria arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ResourceAllocationSearchCriteria field0;
    }


    public class ResourceAllocationOverviewInHoursGraphReportHandler : ResourceAllocationOverviewGraphReportHandlerBase<
        ResourceAllocationOverviewInHoursGraphReport>
    {
        public ResourceAllocationOverviewInHoursGraphReportHandler(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1,
            IGuidService arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class ScheduledWorkErrorListReportHandler : PsaListReportHandler
    {
        public ScheduledWorkErrorListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class TaskListReportHandler : PsaListReportHandler
    {
        public TaskListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class TimeEntryListReportHandler : PsaListReportHandler
    {
        public TimeEntryListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class TimelineGraphReport : ReportWithFilters
    {
        public TimelineGraphReport(
        )
        {
        }
    }


    public class TimelineGraphReportHandler : ReportWithFiltersHandler<IPsaContext, TimelineGraphReport>
    {
        public TimelineGraphReportHandler(
            IContextService<IPsaContext> arg0,
            IPsaReportingRepository arg1,
            IGuidService arg2,
            ICurrencyService arg3,
            ISalesProcessService arg4
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IPsaReportingRepository field1;
        public readonly IGuidService field2;
        public readonly ICurrencyService field3;
        public readonly ISalesProcessService field4;
    }


    public class TravelReimbursementListReportHandler : PsaListReportHandler
    {
        public TravelReimbursementListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class UserListReportHandler : PsaListReportHandler
    {
        public UserListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class UserManagementListReportHandler : UserListReportHandler
    {
        public UserManagementListReportHandler(IContextService<IPsaContext> arg0, ICurrencyService arg1, IGuidService arg2) : base(arg0, arg1, arg2)
        {
        }
    }


    public class WorkCapacityMatrixReportHandler : WorkHourMatrixReportHandlerBase<WorkCapacityMatrixReport>
    {
        public WorkCapacityMatrixReportHandler(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1
        ) : base(arg0, arg1)
        {
        }
    }

    public class WorkCapacityMatrixReport
    {
    }


    public class WorkHourGroupedMatrixReport : WorkHourMatrixReportBase
    {
        public WorkHourGroupedMatrixReport(
        ) : base()
        {
        }
    }


    public class WorkHourGroupedMatrixReportHandler : WorkHourMatrixReportHandlerBase<WorkHourGroupedMatrixReport>
    {
        public WorkHourGroupedMatrixReportHandler(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class WorkHourMatrixReport : WorkHourMatrixReportBase
    {
        public WorkHourMatrixReport(
        )
        {
        }
    }


    public interface IReportDataFieldGroupWithTimePeriod
        : IMatrixReportDataFieldGroup
    {
    }


    public class TimeFrameDataFieldGroup
        : IReportDataFieldGroupWithTimePeriod
    {
        public TimeFrameDataFieldGroup(
            ICrossTabReport arg0,
            string arg1,
            TimePeriod arg2
        )
        {
            field0 = arg0;
            field2 = arg2;
        }

        public TimeFrameDataFieldGroup(
            ICrossTabReport arg0,
            XElement arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly ICrossTabReport field0;
        public readonly XElement field1;
        public readonly TimePeriod field2;
    }


    public class WorkHourMatrixReportBase : CrossTabReport
    {
        public WorkHourMatrixReportBase(
            string arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly string field0;

        protected WorkHourMatrixReportBase()
        {
        }
    }


    public class WorkHourMatrixReportHandler : WorkHourMatrixReportHandlerBase<WorkHourMatrixReport>
    {
        public WorkHourMatrixReportHandler(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class WorkHourMatrixReportHandlerBase<TReport> : MatrixReportHandler<IPsaContext, TReport>
    {
        public WorkHourMatrixReportHandlerBase(
            IContextService<IPsaContext> arg0,
            ICurrencyService arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICurrencyService field1;
    }

    public class MatrixReportHandler<T1, T2>
    {
    }

    public class DailyEmploymentContractSearchDefinition : SearchDefinitionBase<IPsaContext, EmploymentSearchCriteria>
    {
    }



    public class
        DailyResourceAllocationSearchDefinition : SearchDefinitionBase<IPsaContext, ResourceAllocationSearchCriteria>
    {
    }


    public interface IIdentityAdapter
    {
    }


    public class Roles
    {
        public Roles(
        )
        {
        }
    }


    public class AccessRightService
        : IAccessRightService
    {
        public AccessRightService(
            IAccessRightsHelper arg0,
            IInvoiceStatusService arg2,
            IRightRepository arg3,
            IProfileRightRepository arg4, 
            IAuditTrail<Right> arg5,
            IAccessRightsHelperValidator arg6
        )
        {
            field0 = arg0;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field6 = arg6;
        }

        public readonly IAccessRightsHelper field0;
        public readonly IInvoiceStatusService field2;
        public readonly IRightRepository field3;
        public readonly IProfileRightRepository field4;
        public readonly IAccessRightsHelperValidator field6;
    }


    public class AccountNavigationHistoryService
        : IAccountNavigationHistoryService
    {
        public AccountNavigationHistoryService(
            IAccountService arg0,
            INavigationHistoryRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IAccountService field0;
        public readonly INavigationHistoryRepository field1;
    }


    public class AccountNoteService : OrganizationEntityService<AccountNote, IAccountNoteRepository, User, IPsaContext>
        , IAccountNoteService
    {
        public AccountNoteService(
            IContextService<IPsaContext> arg0,
            IAccountNoteRepository arg1,
            IValidator<AccountNote> arg2,
            IAuthorization<IPsaContext, AccountNote> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IAccountNoteRepository field1;
        public readonly IValidator<AccountNote> field2;
        public readonly IAuthorization<IPsaContext, AccountNote> field3;
    }


    public class ActivityContactMemberService : OrganizationEntityService<ActivityContactMember,
            IActivityContactMemberRepository, User, IPsaContext>
        , IActivityContactMemberService
    {
        public ActivityContactMemberService(
            IContextService<IPsaContext> arg0,
            IActivityContactMemberRepository arg1,
            IValidator<ActivityContactMember> arg2,
            IAuthorization<IPsaContext, ActivityContactMember> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IActivityContactMemberRepository field1;
        public readonly IValidator<ActivityContactMember> field2;
        public readonly IAuthorization<IPsaContext, ActivityContactMember> field3;
    }


    public class ActivityEmailService
        : IActivityEmailService
    {
        public ActivityEmailService(
            IMailClient arg0,
            IAppSettings arg1,
            IEmailTemplateService arg2,
            IUserRepository arg3,
            ITimeZoneService arg4,
            IDict arg5,
            IMasterOrganizationRepository arg6,
            ITaskRepository arg7,
            IAccountRepository arg8,
            IActivityTypeRepository arg9,
            IDistributorHelperService arg10,
            IActivityService arg11,
            IActivityRepository arg12
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
        }

        public readonly IMailClient field0;
        public readonly IAppSettings field1;
        public readonly IEmailTemplateService field2;
        public readonly IUserRepository field3;
        public readonly ITimeZoneService field4;
        public readonly IDict field5;
        public readonly IMasterOrganizationRepository field6;
        public readonly ITaskRepository field7;
        public readonly IAccountRepository field8;
        public readonly IActivityTypeRepository field9;
        public readonly IDistributorHelperService field10;
        public readonly IActivityService field11;
        public readonly IActivityRepository field12;
    }


    public class BillingPlanAuditTrailServiceDecorator
        : IBillingPlanService
    {
        public BillingPlanAuditTrailServiceDecorator(
            IBillingPlanService arg0,
            IContextService<IPsaContext> arg1,
            IBillingPlanRepository arg2, IAuditTrail<BillingPlan> arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IBillingPlanService field0;
        public readonly IContextService<IPsaContext> field1;
        public readonly IBillingPlanRepository field2;
        public readonly IAuditTrail<BillingPlan> field3;
    }


    public class BillingPlanService : OrganizationEntityService<BillingPlan, IBillingPlanRepository, User, IPsaContext>
        , IBillingPlanService
    {
        public BillingPlanService(
            IContextService<IPsaContext> arg0,
            IBillingPlanRepository arg1,
            IValidator<BillingPlan> arg2,
            IAuthorization<IPsaContext, BillingPlan> arg3,
            IItemRepository arg4,
            IRecurringItemRepository arg5,
            ITaskRepository arg6,
            ICaseRepository arg7, IAuditTrail<BillingPlan> arg8,
            IResourceAllocationService arg9,
            IFeatureToggleService arg10
        ) : base()
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
        public readonly IBillingPlanRepository field1;
        public readonly IValidator<BillingPlan> field2;
        public readonly IAuthorization<IPsaContext, BillingPlan> field3;
        public readonly IItemRepository field4;
        public readonly IRecurringItemRepository field5;
        public readonly ITaskRepository field6;
        public readonly ICaseRepository field7;
        public readonly IAuditTrail<BillingPlan> field8;
        public readonly IResourceAllocationService field9;
        public readonly IFeatureToggleService field10;
    }


    public class BusinessUnitService :
        OrganizationEntityService<BusinessUnit, IBusinessUnitRepository, User, IPsaContext>
        , IBusinessUnitService
    {
        public BusinessUnitService(
            IContextService<IPsaContext> arg0,
            IBusinessUnitRepository arg1,
            IValidator<BusinessUnit> arg2,
            IAuthorization<IPsaContext, BusinessUnit> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IBusinessUnitRepository field1;
        public readonly IValidator<BusinessUnit> field2;
        public readonly IAuthorization<IPsaContext, BusinessUnit> field3;
    }


    public class EmploymentService : OrganizationEntityService<Employment, IEmploymentRepository, User, IPsaContext>
        , IEmploymentService
    {
        public EmploymentService(
            IContextService<IPsaContext> arg0,
            IEmploymentRepository arg1,
            IValidator<Employment> arg2,
            IAuthorization<IPsaContext, Employment> arg3,
            IFlextimeManagerService arg4,
            IUserRepository arg5, IAuditTrail<Employment> arg6
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IEmploymentRepository field1;
        public readonly IValidator<Employment> field2;
        public readonly IAuthorization<IPsaContext, Employment> field3;
        public readonly IFlextimeManagerService field4;
        public readonly IUserRepository field5;
        public readonly IAuditTrail<Employment> field6;
    }


    public class FlextimeAdjustmentService : IFlextimeAdjustmentService
    {
        public FlextimeAdjustmentService(
            IContextService<IPsaContext> arg0,
            IWorkdayRepository arg1,
            IValidator<Workday> arg2,
            IAuthorization<IPsaContext, Workday> arg3,
            IHourRepository arg4,
            IUserRepository arg5,
            IFlextimeManagerService arg6,
            IUserAuthorization arg7
        ) : base()
        {
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
        }

        public readonly IUserRepository field5;
        public readonly IFlextimeManagerService field6;
        public readonly IUserAuthorization field7;
    }


    public class HourService : OrganizationEntityService<Hour, IHourRepository, User, IPsaContext>
        , IHourService
    {
        public HourService(
            IContextService<IPsaContext> arg0,
            IHourRepository arg1,
            IValidator<Hour> arg2,
            IAuthorization<IPsaContext, Hour> arg3,
            ICaseRepository arg4,
            ITaskService arg5,
            IWorkTypeRepository arg6,
            ICurrencyRepository arg7,
            IUserService arg8,
            ITimecardEventRepository arg9,
            IFlextimeManagerService arg10,
            ITemporaryHourService arg11,
            IReimbursedHourRepository arg12,
            IOrganizationCompanyRepository arg13,
            IUserRepository arg14
        ) : base()
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
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IHourRepository field1;
        public readonly IValidator<Hour> field2;
        public readonly IAuthorization<IPsaContext, Hour> field3;
        public readonly ICaseRepository field4;
        public readonly ITaskService field5;
        public readonly IWorkTypeRepository field6;
        public readonly ICurrencyRepository field7;
        public readonly IUserService field8;
        public readonly ITimecardEventRepository field9;
        public readonly IFlextimeManagerService field10;
        public readonly ITemporaryHourService field11;
        public readonly IReimbursedHourRepository field12;
        public readonly IOrganizationCompanyRepository field13;
        public readonly IUserRepository field14;
    }


    public class ItemService : OrganizationEntityService<Item, IItemRepository, User, IPsaContext>
        , IItemService
    {
        public ItemService(
            IContextService<IPsaContext> arg0,
            IItemRepository arg1,
            IValidator<Item> arg2,
            IAuthorization<IPsaContext, Item> arg3,
            ICaseRepository arg4,
            IProductRepository arg5,
            ICountryProductRepository arg6,
            ITaxRepository arg7,
            IItemSalesAccountService arg8,
            IScannerService arg9,
            ICurrencyService arg10,
            IWorkdayService arg11,
            ITaskRepository arg12,
            ITemporaryItemService arg13,
            IOfferItemRepository arg14,
            IReimbursedItemRepository arg15,
            IUserRepository arg16,
            ICaseMemberService arg17,
            ICostCenterRepository arg18,
            IOrganizationCompanyRepository arg19,
            IFileService arg20,
            IAuditTrail<Item> arg21,
            ITreeTaskService arg22,
            IItemFileRepository arg23
        ) : base()
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
            field17 = arg17;
            field18 = arg18;
            field19 = arg19;
            field20 = arg20;
            field21 = arg21;
            field22 = arg22;
            field23 = arg23;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IItemRepository field1;
        public readonly IValidator<Item> field2;
        public readonly IAuthorization<IPsaContext, Item> field3;
        public readonly ICaseRepository field4;
        public readonly IProductRepository field5;
        public readonly ICountryProductRepository field6;
        public readonly ITaxRepository field7;
        public readonly IItemSalesAccountService field8;
        public readonly IScannerService field9;
        public readonly ICurrencyService field10;
        public readonly IWorkdayService field11;
        public readonly ITaskRepository field12;
        public readonly ITemporaryItemService field13;
        public readonly IOfferItemRepository field14;
        public readonly IReimbursedItemRepository field15;
        public readonly IUserRepository field16;
        public readonly ICaseMemberService field17;
        public readonly ICostCenterRepository field18;
        public readonly IOrganizationCompanyRepository field19;
        public readonly IFileService field20;
        public readonly IAuditTrail<Item> field21;
        public readonly ITreeTaskService field22;
        public readonly IItemFileRepository field23;
    }


    public class LeadSourceService : OrganizationEntityService<LeadSource, ILeadSourceRepository, User, IPsaContext>
        , ILeadSourceService
    {
        public LeadSourceService(
            IContextService<IPsaContext> arg0,
            ILeadSourceRepository arg1,
            IValidator<LeadSource> arg2,
            IAuthorization<IPsaContext, LeadSource> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ILeadSourceRepository field1;
        public readonly IValidator<LeadSource> field2;
        public readonly IAuthorization<IPsaContext, LeadSource> field3;
    }


    public class RecurringItemService :
        OrganizationEntityService<RecurringItem, IRecurringItemRepository, User, IPsaContext>
        , IRecurringItemService
    {
        public RecurringItemService(
            IContextService<IPsaContext> arg0,
            IRecurringItemRepository arg1,
            IValidator<RecurringItem> arg2,
            IAuthorization<IPsaContext, RecurringItem> arg3,
            ICaseRepository arg4,
            IItemSalesAccountService arg5,
            ICurrencyService arg6,
            IItemRepository arg7,
            IProductRepository arg8,
            ICostCenterRepository arg9,
            ITreeTaskService arg10
        ) : base()
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
        public readonly IRecurringItemRepository field1;
        public readonly IValidator<RecurringItem> field2;
        public readonly IAuthorization<IPsaContext, RecurringItem> field3;
        public readonly ICaseRepository field4;
        public readonly IItemSalesAccountService field5;
        public readonly ICurrencyService field6;
        public readonly IItemRepository field7;
        public readonly IProductRepository field8;
        public readonly ICostCenterRepository field9;
        public readonly ITreeTaskService field10;
    }


    public class ReimbursedHourService :
        OrganizationEntityService<ReimbursedHour, IReimbursedHourRepository, User, IPsaContext>
        , IReimbursedHourService
    {
        public ReimbursedHourService(
            IContextService<IPsaContext> arg0,
            IReimbursedHourRepository arg1,
            IValidator<ReimbursedHour> arg2,
            IAuthorization<IPsaContext, ReimbursedHour> arg3,
            IHourRepository arg4,
            IInvoiceRowRepository arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IReimbursedHourRepository field1;
        public readonly IValidator<ReimbursedHour> field2;
        public readonly IAuthorization<IPsaContext, ReimbursedHour> field3;
        public readonly IHourRepository field4;
        public readonly IInvoiceRowRepository field5;
    }


    public class ReimbursedItemService :
        OrganizationEntityService<ReimbursedItem, IReimbursedItemRepository, User, IPsaContext>
        , IReimbursedItemService
    {
        public ReimbursedItemService(
            IContextService<IPsaContext> arg0,
            IReimbursedItemRepository arg1,
            IValidator<ReimbursedItem> arg2,
            IAuthorization<IPsaContext, ReimbursedItem> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IReimbursedItemRepository field1;
        public readonly IValidator<ReimbursedItem> field2;
        public readonly IAuthorization<IPsaContext, ReimbursedItem> field3;
    }


    public class SalesProcessService :
        OrganizationEntityService<SalesProcess, ISalesProcessRepository, User, IPsaContext>
        , ISalesProcessService
    {
        public SalesProcessService(
            IContextService<IPsaContext> arg0,
            ISalesProcessRepository arg1,
            IValidator<SalesProcess> arg2,
            IAuthorization<IPsaContext, SalesProcess> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ISalesProcessRepository field1;
        public readonly IValidator<SalesProcess> field2;
        public readonly IAuthorization<IPsaContext, SalesProcess> field3;
    }



    public class TermsOfServiceApprovalService : OrganizationEntityService<TermsOfServiceApproval,
            ITermsOfServiceApprovalRepository, User, IPsaContext>
        , ITermsOfServiceApprovalService
    {
        public TermsOfServiceApprovalService(
            IContextService<IPsaContext> arg0,
            ITermsOfServiceApprovalRepository arg1,
            IValidator<TermsOfServiceApproval> arg2,
            IAuthorization<IPsaContext, TermsOfServiceApproval> arg3,
            IOrganizationService arg4,
            IOrganizationService arg5,
            IPsaOrganizationService arg6,
            IExternallyOwnedOrganizationService arg7
        ) : base()
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

        public readonly IContextService<IPsaContext> field0;
        public readonly ITermsOfServiceApprovalRepository field1;
        public readonly IValidator<TermsOfServiceApproval> field2;
        public readonly IAuthorization<IPsaContext, TermsOfServiceApproval> field3;
        public readonly IOrganizationService field4;
        public readonly IOrganizationService field5;
        public readonly IPsaOrganizationService field6;
        public readonly IExternallyOwnedOrganizationService field7;
    }


    public class TermsOfServiceEmailService
        : ITermsOfServiceEmailService
    {
        public TermsOfServiceEmailService(
            IContextService<IPsaContext> arg0,
            IMailClient arg1,
            IAppSettings arg2,
            ILanguageService arg3,
            IEmailTemplateService arg4,
            IUserRepository arg5,
            IDistributorHelperService arg6
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

        public readonly IContextService<IPsaContext> field0;
        public readonly IMailClient field1;
        public readonly IAppSettings field2;
        public readonly ILanguageService field3;
        public readonly IEmailTemplateService field4;
        public readonly IUserRepository field5;
        public readonly IDistributorHelperService field6;
    }


    public class AuthorizedIPAddressValidator : Validator<AuthorizedIPAddress>
    {
        public AuthorizedIPAddressValidator(
            IAuthorizedIpAddressRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IAuthorizedIpAddressRepository field0;
    }


    public class CaseBillingAccountValidator : Validator<CaseBillingAccount>
    {
        public CaseBillingAccountValidator(
            IContextService<IPsaContext> arg0,
            ICaseBillingAccountRepository arg1,
            ICaseRepository arg2,
            IAccountRepository arg3,
            IInvoiceRepository arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICaseBillingAccountRepository field1;
        public readonly ICaseRepository field2;
        public readonly IAccountRepository field3;
        public readonly IInvoiceRepository field4;
    }


    public class EmploymentValidator : Validator<Employment>
    {
        public EmploymentValidator(
            IContextService<IPsaContext> arg0,
            IEmploymentRepository arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IEmploymentRepository field1;
    }


    public class ItemValidator : Validator<Item>
    {
        public ItemValidator(
            IContextService<IPsaContext> arg0,
            IItemRepository arg1,
            ITaskRepository arg2,
            ICaseRepository arg3,
            ITaxRepository arg4,
            ITravelReimbursementStatusRepository arg5,
            IProductService arg6,
            ICaseProductService arg7,
            ICaseMemberService arg8,
            ITaskMemberService arg9,
            IInvoiceRepository arg10,
            IInvoiceStatusRepository arg11,
            IInvoiceCaseRepository arg12,
            ICurrencyRepository arg13,
            ITravelReimbursementRepository arg14
        ) : base()
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
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IItemRepository field1;
        public readonly ITaskRepository field2;
        public readonly ICaseRepository field3;
        public readonly ITaxRepository field4;
        public readonly ITravelReimbursementStatusRepository field5;
        public readonly IProductService field6;
        public readonly ICaseProductService field7;
        public readonly ICaseMemberService field8;
        public readonly ITaskMemberService field9;
        public readonly IInvoiceRepository field10;
        public readonly IInvoiceStatusRepository field11;
        public readonly IInvoiceCaseRepository field12;
        public readonly ICurrencyRepository field13;
        public readonly ITravelReimbursementRepository field14;
    }


    public class RecurringItemValidator : Validator<RecurringItem>
    {
        public RecurringItemValidator(
            IProductService arg0,
            IRecurringItemRepository arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IProductService field0;
        public readonly IRecurringItemRepository field1;
    }


    public class WorkdayHandlerService
        : IWorkdayHandlerService
    {
        public WorkdayHandlerService(
            IContextService<IPsaContext> arg0,
            IFlextimeManagerService arg1,
            IWorkdayRepository arg2,
            IEmploymentService arg3,
            IWorkingDayExceptionService arg4,
            IWorkingDayService arg5
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
        public readonly IFlextimeManagerService field1;
        public readonly IWorkdayRepository field2;
        public readonly IEmploymentService field3;
        public readonly IWorkingDayExceptionService field4;
        public readonly IWorkingDayService field5;
    }


    public class WorkdayService : OrganizationEntityService<Workday, IWorkdayRepository, User, IPsaContext>
        , IWorkdayService
    {
        public WorkdayService(
            IContextService<IPsaContext> arg0,
            IWorkdayRepository arg1,
            IValidator<Workday> arg2,
            IAuthorization<IPsaContext, Workday> arg3,
            IHourRepository arg4
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IWorkdayRepository field1;
        public readonly IValidator<Workday> field2;
        public readonly IAuthorization<IPsaContext, Workday> field3;
        public readonly IHourRepository field4;
    }


    public class ActivityResourceMemberService : OrganizationEntityService<ActivityResourceMember,
            IActivityResourceMemberRepository, User, IPsaContext>
        , IActivityResourceMemberService
    {
        public ActivityResourceMemberService(
            IContextService<IPsaContext> arg0,
            IActivityResourceMemberRepository arg1,
            IValidator<ActivityResourceMember> arg2,
            IAuthorization<IPsaContext, ActivityResourceMember> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IActivityResourceMemberRepository field1;
        public readonly IValidator<ActivityResourceMember> field2;
        public readonly IAuthorization<IPsaContext, ActivityResourceMember> field3;
    }


    public class ActivityService : OrganizationEntityService<Activity, IActivityRepository, User, IPsaContext>
        , IActivityService
    {
        public ActivityService(
            IContextService<IPsaContext> arg0,
            IActivityRepository arg1,
            IValidator<Activity> arg2,
            IAuthorization<IPsaContext, Activity> arg3,
            IActivityUserMemberService arg4,
            IActivityContactMemberService arg5,
            IActivityResourceMemberService arg6,
            ICalendarSyncActivityNonAppParticipantService arg7,
            ICaseRepository arg8,
            IActivityTypeRepository arg9,
            IActivityStatusRepository arg10,
            IUserRepository arg11,
            IFlextimeManagerService arg12,
            IActivityUserMemberRepository arg13,
            IActivityContactMemberRepository arg14,
            IActivityResourceMemberRepository arg15,
            ICalendarSyncActivityNonAppParticipantRepository arg16, IAuditTrail<Activity> arg17,
            IBackgroundExecutorService arg18,
            IUserService arg19
        ) : base(arg0, arg1, arg2, arg3)
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
            field17 = arg17;
            field18 = arg18;
            field19 = arg19;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IActivityRepository field1;
        public readonly IValidator<Activity> field2;
        public readonly IAuthorization<IPsaContext, Activity> field3;
        public readonly IActivityUserMemberService field4;
        public readonly IActivityContactMemberService field5;
        public readonly IActivityResourceMemberService field6;
        public readonly ICalendarSyncActivityNonAppParticipantService field7;
        public readonly ICaseRepository field8;
        public readonly IActivityTypeRepository field9;
        public readonly IActivityStatusRepository field10;
        public readonly IUserRepository field11;
        public readonly IFlextimeManagerService field12;
        public readonly IActivityUserMemberRepository field13;
        public readonly IActivityContactMemberRepository field14;
        public readonly IActivityResourceMemberRepository field15;
        public readonly ICalendarSyncActivityNonAppParticipantRepository field16;
        public readonly IAuditTrail<Activity> field17;
        public readonly IBackgroundExecutorService field18;
        public readonly IUserService field19;
    }


    public class ActivityStatusService :
        OrganizationEntityService<ActivityStatus, IActivityStatusRepository, User, IPsaContext>
        , IActivityStatusService
    {
        public ActivityStatusService(
            IContextService<IPsaContext> arg0,
            IActivityStatusRepository arg1,
            IValidator<ActivityStatus> arg2,
            IAuthorization<IPsaContext, ActivityStatus> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IActivityStatusRepository field1;
        public readonly IValidator<ActivityStatus> field2;
        public readonly IAuthorization<IPsaContext, ActivityStatus> field3;
    }


    public class ActivityTypeService :
        OrganizationEntityService<ActivityType, IActivityTypeRepository, User, IPsaContext>
        , IActivityTypeService
    {
        public ActivityTypeService(
            IContextService<IPsaContext> arg0,
            IActivityTypeRepository arg1,
            IValidator<ActivityType> arg2,
            IAuthorization<IPsaContext, ActivityType> arg3,
            IFlextimeManagerService arg4
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IActivityTypeRepository field1;
        public readonly IValidator<ActivityType> field2;
        public readonly IAuthorization<IPsaContext, ActivityType> field3;
        public readonly IFlextimeManagerService field4;
    }


    public class ActivityUserMemberService : OrganizationEntityService<ActivityUserMember, IActivityUserMemberRepository
            , User, IPsaContext>
        , IActivityUserMemberService
    {
        public ActivityUserMemberService(
            IContextService<IPsaContext> arg0,
            IActivityUserMemberRepository arg1,
            IValidator<ActivityUserMember> arg2,
            IAuthorization<IPsaContext, ActivityUserMember> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IActivityUserMemberRepository field1;
        public readonly IValidator<ActivityUserMember> field2;
        public readonly IAuthorization<IPsaContext, ActivityUserMember> field3;
    }


    public class ApiClientHandlerService
        : IApiClientHandlerService
    {
        public ApiClientHandlerService(
            IContextService<IPsaContext> arg0,
            IPsaUserService arg1,
            IUserRepository arg2,
            ICountryRepository arg3,
            IApiClientService arg4,
            IAccessRightService arg5,
            IProfileService arg6
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

        public readonly IContextService<IPsaContext> field0;
        public readonly IPsaUserService field1;
        public readonly IUserRepository field2;
        public readonly ICountryRepository field3;
        public readonly IApiClientService field4;
        public readonly IAccessRightService field5;
        public readonly IProfileService field6;
    }


    public class AttachmentGrouper
    {
        public AttachmentGrouper(
            List<ItemForInvoice> arg0,
            IEnumerable<ItemForInvoice> arg1,
            IEnumerable<HourForInvoice> arg2,
            IEnumerable<TaskEx> arg3,
            decimal? arg4,
            string arg5,
            List<InvoiceRowWithCategory> arg6,
            ICurrencyRounding arg7
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

        public readonly List<ItemForInvoice> field0;
        public readonly IEnumerable<ItemForInvoice> field1;
        public readonly IEnumerable<HourForInvoice> field2;
        public readonly IEnumerable<TaskEx> field3;
        public readonly decimal? field4;
        public readonly string field5;
        public readonly List<InvoiceRowWithCategory> field6;
        public readonly ICurrencyRounding field7;
    }


    public class AuditTrailService :
        OrganizationEntityService<AuditTrailEntry, IAuditTrailRepository, User, IPsaContext>
        , IAuditTrailService
    {
        public AuditTrailService(
            IContextService<IPsaContext> arg0,
            IAuditTrailRepository arg1,
            IValidator<AuditTrailEntry> arg2,
            IAuthorization<IPsaContext, AuditTrailEntry> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IAuditTrailRepository field1;
        public readonly IValidator<AuditTrailEntry> field2;
        public readonly IAuthorization<IPsaContext, AuditTrailEntry> field3;
    }


    public class AccountNoteAuthorization : NoteAuthorization<AccountNote>
    {
        public AccountNoteAuthorization(
            IContextService<IPsaContext> arg0,
            IAccountRepository arg1,
            IAuthorization<IPsaContext, Account> arg2,
            ICaseRepository arg3,
            IAuthorization<IPsaContext, Case> arg4
        ) : base(arg0, arg1, arg2, arg3, arg4)
        {
        }
    }


    public class ActivityAuthorization : PsaEntityAuthorization<Activity>
    {
        public ActivityAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IActivityTypeRepository arg2,
            IUserRepository arg3,
            IAuthorization<IPsaContext, Case> arg4,
            IAccountRepository arg5,
            IAuthorization<IPsaContext, Account> arg6,
            IActivityUserMemberRepository arg7,
            IActivityRepository arg8
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
        }

        public readonly ICaseRepository field1;
        public readonly IActivityTypeRepository field2;
        public readonly IUserRepository field3;
        public readonly IAuthorization<IPsaContext, Case> field4;
        public readonly IAccountRepository field5;
        public readonly IAuthorization<IPsaContext, Account> field6;
        public readonly IActivityUserMemberRepository field7;
        public readonly IActivityRepository field8;
    }

    public class OrganizationAuthorization : AuthorizationBase<IPsaContext, Organization>, IAuthorization<IPsaContext, Organization>
    {
        public OrganizationAuthorization(IContextService<IPsaContext> contextService) : base(contextService)
        {
        }
    }


    public class ActivityContactMemberAuthorization : PsaEntityAuthorization<ActivityContactMember>
    {
        public ActivityContactMemberAuthorization(
            IContextService<IPsaContext> arg0,
            IActivityRepository arg1,
            IAuthorization<IPsaContext, Activity> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IActivityRepository field1;
        public readonly IAuthorization<IPsaContext, Activity> field2;
    }


    public class ActivityResourceMemberAuthorization : PsaEntityAuthorization<ActivityResourceMember>
    {
        public ActivityResourceMemberAuthorization(
            IContextService<IPsaContext> arg0,
            IActivityRepository arg1,
            IAuthorization<IPsaContext, Activity> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IActivityRepository field1;
        public readonly IAuthorization<IPsaContext, Activity> field2;
    }


    public class ActivityStatusAuthorization : SettingsEntityAuthorization<ActivityStatus>
    {
        public ActivityStatusAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ActivityTypeAuthorization : SettingsEntityAuthorization<ActivityType>
    {
        public ActivityTypeAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ActivityUserMemberAuthorization : PsaEntityAuthorization<ActivityUserMember>
    {
        public ActivityUserMemberAuthorization(
            IContextService<IPsaContext> arg0,
            IActivityRepository arg1,
            IAuthorization<IPsaContext, Activity> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IActivityRepository field1;
        public readonly IAuthorization<IPsaContext, Activity> field2;
    }


    public class AuditTrailEntryAuthorization : SettingsEntityAuthorization<AuditTrailEntry>
    {
        public AuditTrailEntryAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class AuthorizedIPAddressAuthorization : SettingsEntityAuthorization<AuthorizedIPAddress>
    {
        public AuthorizedIPAddressAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class BackgroundTaskAuthorization : PsaEntityAuthorization<BackgroundTask>
    {
        public BackgroundTaskAuthorization(
            IContextService<IPsaContext> arg0,
            IAuthorization<IPsaContext, User> arg1,
            IUserRepository arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IAuthorization<IPsaContext, User> field1;
        public readonly IUserRepository field2;
    }

    public class FileDownloadAuthorization : PsaEntityAuthorization<FileDownload>
    {
        public FileDownloadAuthorization(
            IContextService<IPsaContext> arg0,
            IAuthorization<IPsaContext, User> arg1,
            IUserRepository arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IAuthorization<IPsaContext, User> field1;
        public readonly IUserRepository field2;
    }

    public class CurrencyAuthorization : PsaEntityAuthorization<Currency>
    {
        public CurrencyAuthorization(
            IContextService<IPsaContext> arg0,
            IAuthorization<IPsaContext, User> arg1,
            IUserRepository arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IAuthorization<IPsaContext, User> field1;
        public readonly IUserRepository field2;
    }


    public class BankAccountAuthorization : SettingsEntityAuthorization<BankAccount>
    {
        public BankAccountAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class BillingPlanAuthorization : PsaEntityAuthorization<BillingPlan>
    {
        public BillingPlanAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1
        ) : base(arg0)
        {
            field1 = arg1;
        }

        public readonly ICaseRepository field1;
    }


    public class BusinessOverviewAuthorization : AllowAllAuthorization<IPsaContext, BusinessOverview>
    {
        public BusinessOverviewAuthorization(
        )
        {
        }
    }

    public class AllowAllAuthorization<TContext, TEntity> : IAuthorization<TContext, TEntity> where TContext : IContext where TEntity : IEntity
    {
    }

    public class BusinessUnitAuthorization : SettingsEntityAuthorization<BusinessUnit>
    {
        public BusinessUnitAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class
        CalendarSyncActivityNonAppParticipantAuthorization : PsaEntityAllowAllAuthorization<CalendarSyncActivityNonAppParticipant>
    {
        public CalendarSyncActivityNonAppParticipantAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class CaseAuthorization : PsaEntityAuthorization<Case>
    {
        public CaseAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class CaseBillingAccountAuthorization : PsaEntityAuthorization<CaseBillingAccount>
    {
        public CaseBillingAccountAuthorization(
            IContextService<IPsaContext> arg0,
            IAuthorization<IPsaContext, Case> arg1,
            IAuthorization<IPsaContext, Account> arg2,
            ICaseRepository arg3,
            IAccountRepository arg4
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IAuthorization<IPsaContext, Case> field1;
        public readonly IAuthorization<IPsaContext, Account> field2;
        public readonly ICaseRepository field3;
        public readonly IAccountRepository field4;
    }


    public class CaseCommentAuthorization : PsaEntityAuthorization<CaseComment>
    {
        public CaseCommentAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
    }


    public class CaseFileAuthorization : PsaEntityAuthorization<CaseFile>
    {
        public CaseFileAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
    }


    public class CaseMemberAuthorization : PsaEntityAuthorization<CaseMember>
    {
        public CaseMemberAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseMemberRepository arg1,
            ICaseRepository arg2,
            IAuthorization<IPsaContext, Case> arg3
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly ICaseMemberRepository field1;
        public readonly ICaseRepository field2;
        public readonly IAuthorization<IPsaContext, Case> field3;
    }

    public class AccountAuthorization : PsaEntityAuthorization<Account>
    {
        public AccountAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }

    public class ApiClientAuthorization : PsaEntityAuthorization<ApiClient>
    {
        public ApiClientAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }

    public class CaseNoteAuthorization : NoteAuthorization<CaseNote>
    {
        public CaseNoteAuthorization(
            IContextService<IPsaContext> arg0,
            IAccountRepository arg1,
            IAuthorization<IPsaContext, Account> arg2,
            ICaseRepository arg3,
            IAuthorization<IPsaContext, Case> arg4
        ) : base(arg0, arg1, arg2, arg3, arg4)
        {
        }
    }


    public class CaseProductAuthorization : PsaEntityAuthorization<CaseProduct>
    {
        public CaseProductAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
    }


    public class CaseStatusAuthorization : PsaEntityAuthorization<CaseStatus>
    {
        public CaseStatusAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
    }


    public class CaseStatusTypeAuthorization : SettingsEntityAuthorization<CaseStatusType>
    {
        public CaseStatusTypeAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class CaseTagAuthorization : TagBaseAuthorization<Case, CaseTag>
    {
        public CaseTagAuthorization(
            IContextService<IPsaContext> arg0,
            IAuthorization<IPsaContext, Case> arg1,
            ICaseRepository arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class CaseWorkTypeAuthorization : PsaEntityAuthorization<CaseWorkType>
    {
        public CaseWorkTypeAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
    }


    public class CommunicationMethodAuthorization : SettingsEntityAuthorization<CommunicationMethod>
    {
        public CommunicationMethodAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ContactAuthorization : PsaEntityAuthorization<Contact>
    {
        public ContactAuthorization(
            IContextService<IPsaContext> arg0,
            IAccountRepository arg1,
            IAuthorization<IPsaContext, Account> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IAccountRepository field1;
        public readonly IAuthorization<IPsaContext, Account> field2;
    }


    public class ContactRoleAuthorization : SettingsEntityAuthorization<ContactRole>
    {
        public ContactRoleAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ContactTagAuthorization : TagBaseAuthorization<Contact, ContactTag>
    {
        public ContactTagAuthorization(
            IContextService<IPsaContext> arg0,
            IAuthorization<IPsaContext, Contact> arg1,
            IContactRepository arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class CostCenterAuthorization : SettingsEntityAuthorization<CostCenter>
    {
        public CostCenterAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class CostCenterRevenueAuthorization : PsaEntityAuthorization<CostCenterRevenue>
    {
        public CostCenterRevenueAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
    }


    public class CountryProductAuthorization : SettingsEntityAuthorization<CountryProduct>
    {
        public CountryProductAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class DashboardAuthorization : PsaEntityAuthorization<Dashboard>
    {
        public DashboardAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class DashboardPartAuthorization : PsaEntityAuthorization<DashboardPart>
    {
        public DashboardPartAuthorization(
            IContextService<IPsaContext> arg0,
            IAuthorization<IPsaContext, Dashboard> arg1,
            IDashboardRepository arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IAuthorization<IPsaContext, Dashboard> field1;
        public readonly IDashboardRepository field2;
    }


    public class EmploymentAuthorization : PsaEntityAuthorization<Employment>
    {
        public EmploymentAuthorization(
            IContextService<IPsaContext> arg0,
            IUserRepository arg1
        ) : base(arg0)
        {
            field1 = arg1;
        }

        public readonly IUserRepository field1;
    }


    public class FileAuthorization : PsaEntityAuthorization<File>
    {
        public FileAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseFileRepository arg1,
            IAuthorization<IPsaContext, CaseFile> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseFileRepository field1;
        public readonly IAuthorization<IPsaContext, CaseFile> field2;
    }


    public class FileDataAuthorization : AllowAllAuthorization<IPsaContext, FileData>
    {
        public FileDataAuthorization(
        )
        {
        }
    }


    public class FileTagAuthorization : TagBaseAuthorization<File, FileTag>
    {
        public FileTagAuthorization(
            IContextService<IPsaContext> arg0,
            IAuthorization<IPsaContext, File> arg1,
            IFileRepository arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class HourAuthorization : PsaEntityAuthorization<Hour>
    {
        public HourAuthorization(
            IContextService<IPsaContext> arg0,
            IHourRepository arg1,
            IInvoiceRepository arg2,
            IAuthorization<IPsaContext, Invoice> arg3,
            IAuthorization<IPsaContext, User> arg4,
            IUserRepository arg5,
            ICaseRepository arg6,
            IAuthorization<IPsaContext, Case> arg7
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
        }

        public readonly IHourRepository field1;
        public readonly IInvoiceRepository field2;
        public readonly IAuthorization<IPsaContext, Invoice> field3;
        public readonly IAuthorization<IPsaContext, User> field4;
        public readonly IUserRepository field5;
        public readonly ICaseRepository field6;
        public readonly IAuthorization<IPsaContext, Case> field7;
    }


    public class IndustryAuthorization : SettingsEntityAuthorization<Industry>
    {
        public IndustryAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class IntegrationErrorAuthorization : PsaEntityAllowAllAuthorization<IntegrationError>
    {
        public IntegrationErrorAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class InvoiceAuthorization : PsaEntityAuthorization<Invoice>
    {
        public InvoiceAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class InvoiceConfigAuthorization : PsaEntityAuthorization<InvoiceConfig>
    {
        public InvoiceConfigAuthorization(
            IContextService<IPsaContext> arg0,
            IInvoiceRepository arg1,
            IAuthorization<IPsaContext, Invoice> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IInvoiceRepository field1;
        public readonly IAuthorization<IPsaContext, Invoice> field2;
    }


    public class InvoiceFileAuthorization : PsaEntityAuthorization<InvoiceFile>
    {
        public InvoiceFileAuthorization(
            IContextService<IPsaContext> arg0,
            IInvoiceRepository arg1,
            IAuthorization<IPsaContext, Invoice> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IInvoiceRepository field1;
        public readonly IAuthorization<IPsaContext, Invoice> field2;
    }


    public interface IInvoiceHandlersAuthorization
    {
    }


    public class InvoiceHandlersAuthorization : AuthorizationBase<IPsaContext, Object>
        , IInvoiceHandlersAuthorization
    {
        public InvoiceHandlersAuthorization(
            IContextService<IPsaContext> arg0,
            IAuthorization<IPsaContext, Invoice> arg1
        ) : base(arg0)
        {
            field1 = arg1;
        }

        public readonly IAuthorization<IPsaContext, Invoice> field1;
    }


    public class InvoiceHtmlAuthorization : PsaEntityAuthorization<InvoiceHTML>
    {
        public InvoiceHtmlAuthorization(
            IContextService<IPsaContext> arg0,
            IInvoiceRepository arg1,
            IAuthorization<IPsaContext, Invoice> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IInvoiceRepository field1;
        public readonly IAuthorization<IPsaContext, Invoice> field2;
    }


    public class InvoiceRowAuthorization : PsaEntityAuthorization<InvoiceRow>
    {
        public InvoiceRowAuthorization(
            IContextService<IPsaContext> arg0,
            IInvoiceRepository arg1,
            IAuthorization<IPsaContext, Invoice> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IInvoiceRepository field1;
        public readonly IAuthorization<IPsaContext, Invoice> field2;
    }


    public class InvoiceStatusAuthorization : SettingsEntityAuthorization<InvoiceStatus>
    {
        public InvoiceStatusAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class InvoiceTemplateAuthorization : SettingsEntityAuthorization<InvoiceTemplate>
    {
        public InvoiceTemplateAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class InvoiceTemplateConfigAuthorization : SettingsEntityAuthorization<InvoiceTemplateConfig>
    {
        public InvoiceTemplateConfigAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public interface IScheduledWorkTaskAuthorization
        : IAuthorization<IPsaContext, BackgroundTask>
    {
    }


    public class ScheduledWorkTaskAuthorization : BackgroundTaskAuthorization
        , IScheduledWorkTaskAuthorization
    {
        public ScheduledWorkTaskAuthorization(
            IContextService<IPsaContext> arg0,
            IAuthorization<IPsaContext, User> arg1,
            IUserRepository arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class ItemAuthorization : PsaEntityAuthorization<Item>
    {
        public ItemAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2,
            ITravelReimbursementRepository arg3,
            IAuthorization<IPsaContext, TravelReimbursement> arg4
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
        public readonly ITravelReimbursementRepository field3;
        public readonly IAuthorization<IPsaContext, TravelReimbursement> field4;
    }


    public class ItemFileAuthorization : PsaEntityAuthorization<ItemFile>
    {
        public ItemFileAuthorization(
            IContextService<IPsaContext> arg0,
            IItemRepository arg1,
            IAuthorization<IPsaContext, Item> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IItemRepository field1;
        public readonly IAuthorization<IPsaContext, Item> field2;
    }


    public class ItemSalesAccountAuthorization : PsaEntityAuthorization<ItemSalesAccount>
    {
        public ItemSalesAccountAuthorization(
            IContextService<IPsaContext> arg0,
            IItemRepository arg1,
            IAuthorization<IPsaContext, Item> arg2,
            IRecurringItemRepository arg3,
            IAuthorization<IPsaContext, RecurringItem> arg4
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IItemRepository field1;
        public readonly IAuthorization<IPsaContext, Item> field2;
        public readonly IRecurringItemRepository field3;
        public readonly IAuthorization<IPsaContext, RecurringItem> field4;
    }


    public class LeadSourceAuthorization : SettingsEntityAuthorization<LeadSource>
    {
        public LeadSourceAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class LinkAuthorization : PsaEntityAuthorization<Link>
    {
        public LinkAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class NavigationHistoryAuthorization : PsaEntityAllowAllAuthorization<NavigationHistory>
    {
        public NavigationHistoryAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class NoteAuthorization<TEntity> : PsaEntityAuthorization<TEntity> where TEntity : IOrganizationEntity
    {
        public NoteAuthorization(
            IContextService<IPsaContext> arg0,
            IAccountRepository arg1,
            IAuthorization<IPsaContext, Account> arg2,
            ICaseRepository arg3,
            IAuthorization<IPsaContext, Case> arg4
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IAccountRepository field1;
        public readonly IAuthorization<IPsaContext, Account> field2;
        public readonly ICaseRepository field3;
        public readonly IAuthorization<IPsaContext, Case> field4;
    }


    public class OfferAuthorization : PsaEntityAuthorization<Offer>
    {
        public OfferAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
    }


    public class OfferFileAuthorization : PsaEntityAuthorization<OfferFile>
    {
        public OfferFileAuthorization(
            IContextService<IPsaContext> arg0,
            IOfferRepository arg1,
            IAuthorization<IPsaContext, Offer> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IOfferRepository field1;
        public readonly IAuthorization<IPsaContext, Offer> field2;
    }


    public class OfferItemAuthorization : PsaEntityAuthorization<OfferItem>
    {
        public OfferItemAuthorization(
            IContextService<IPsaContext> arg0,
            IOfferRepository arg1,
            IAuthorization<IPsaContext, Offer> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IOfferRepository field1;
        public readonly IAuthorization<IPsaContext, Offer> field2;
    }


    public class OfferSubtotalAuthorization : PsaEntityAuthorization<OfferSubtotal>
    {
        public OfferSubtotalAuthorization(
            IContextService<IPsaContext> arg0,
            IOfferRepository arg1,
            IAuthorization<IPsaContext, Offer> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IOfferRepository field1;
        public readonly IAuthorization<IPsaContext, Offer> field2;
    }


    public class OfferTaskAuthorization : PsaEntityAuthorization<OfferTask>
    {
        public OfferTaskAuthorization(
            IContextService<IPsaContext> arg0,
            IOfferRepository arg1,
            IAuthorization<IPsaContext, Offer> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IOfferRepository field1;
        public readonly IAuthorization<IPsaContext, Offer> field2;
    }


    public class OrganizationCompanyWorkTypeAuthorization : SettingsEntityAuthorization<OrganizationCompanyWorkType>
    {
        public OrganizationCompanyWorkTypeAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class OrganizationSsoCompanyAuthorization : SettingsEntityAuthorization<OrganizationSsoCompany>
    {
        public OrganizationSsoCompanyAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class OvertimeAuthorization : SettingsEntityAuthorization<OverTime>
    {
        public OvertimeAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class OvertimePriceAuthorization : PsaEntityAuthorization<OverTimePrice>
    {
        public OvertimePriceAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1
        ) : base(arg0)
        {
            field1 = arg1;
        }

        public readonly ICaseRepository field1;
    }


    public class PricelistAuthorization : PsaEntityAuthorization<Pricelist>
    {
        public PricelistAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
    }


    public class PricelistVersionAuthorization : SettingsEntityAuthorization<PricelistVersion>
    {
        public PricelistVersionAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ProductAuthorization : SettingsEntityAuthorization<Product>
    {
        public ProductAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ProductCategoryAuthorization : SettingsEntityAuthorization<ProductCategory>
    {
        public ProductCategoryAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ProductPriceAuthorization : PsaEntityAuthorization<ProductPrice>
    {
        public ProductPriceAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1
        ) : base(arg0)
        {
            field1 = arg1;
        }

        public readonly ICaseRepository field1;
    }


    public class ProfileAuthorization : PsaEntityAuthorization<Profile>
    {
        public ProfileAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ProfileDashboardAuthorization : PsaEntityAuthorization<ProfileDashboard>
    {
        public ProfileDashboardAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ProposalStatusAuthorization : SettingsEntityAuthorization<ProposalStatus>
    {
        public ProposalStatusAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class RecurringItemAuthorization : PsaEntityAuthorization<RecurringItem>
    {
        public RecurringItemAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
    }


    public class ReimbursedHourAuthorization : PsaEntityAuthorization<ReimbursedHour>
    {
        public ReimbursedHourAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            ITaskRepository arg2,
            IAuthorization<IPsaContext, Case> arg3
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly ICaseRepository field1;
        public readonly ITaskRepository field2;
        public readonly IAuthorization<IPsaContext, Case> field3;
    }


    public class ReimbursedItemAuthorization : PsaEntityAuthorization<ReimbursedItem>
    {
        public ReimbursedItemAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
    }


    public class ReportAuthorization : PsaEntityAllowAllAuthorization<Report>
    {
        public ReportAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class ResourceAllocationAuthorization : PsaEntityAllowAllAuthorization<ResourceAllocation>
    {
        public ResourceAllocationAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2,
            ICaseMemberRepository arg3,
            IUserRepository arg4,
            IUserAuthorization arg5
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
        public readonly ICaseMemberRepository field3;
        public readonly IUserRepository field4;
        public readonly IUserAuthorization field5;
    }


    public class ResourceAuthorization : SettingsEntityAuthorization<Resource>
    {
        public ResourceAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class SalesAccountAuthorization : SettingsEntityAuthorization<SalesAccount>
    {
        public SalesAccountAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class SalesProcessAuthorization : SettingsEntityAuthorization<SalesProcess>
    {
        public SalesProcessAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class SalesStatusAuthorization : PsaEntityAuthorization<SalesStatus>
    {
        public SalesStatusAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
    }


    public class SearchAuthorization : PsaEntityAuthorization<Entities.Search>
    {
        public SearchAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class SearchCriteriaAuthorization : PsaEntityAuthorization<Entities.SearchCriteria>
    {
        public SearchCriteriaAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class SettingsAuthorization : PsaEntityAuthorization<Settings>
    {
        public SettingsAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class TagAuthorization : SettingsEntityAuthorization<Tag>
    {
        public TagAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }

    public class TTaggedEntity : IEntity
    {
    }

    public interface ITagEntity<TTaggedEntity> : IOrganizationEntity where TTaggedEntity : IOrganizationEntity
    {
    }

    public abstract class TagBaseAuthorization<TTaggedEntity, TTagEntity> : PsaEntityAuthorization<TTagEntity>
        where TTaggedEntity : IOrganizationEntity
        where TTagEntity : ITagEntity<TTaggedEntity>
    {
        private readonly IAuthorization<IPsaContext, TTaggedEntity> _TaggedEntityAuthorization;
        private readonly IEntityRepository<TTaggedEntity> _TaggedEntityRepository;

        public TagBaseAuthorization(IContextService<IPsaContext> context, IAuthorization<IPsaContext, TTaggedEntity> taggedEntityAuthorization, IEntityRepository<TTaggedEntity> taggedEntityRepository) : base(context)
        {
            _TaggedEntityAuthorization = taggedEntityAuthorization;
            _TaggedEntityRepository = taggedEntityRepository;
        }
    }


    public class TTagEntity : IEntity
    {
    }

    public class TaskAuthorization : PsaEntityAuthorization<Task>
    {
        public TaskAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
    }


    public class TaskMemberAuthorization : PsaEntityAuthorization<TaskMember>
    {
        public TaskMemberAuthorization(
            IContextService<IPsaContext> arg0,
            ITaskRepository arg1,
            IAuthorization<IPsaContext, Task> arg2,
            ICaseRepository arg3
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly ITaskRepository field1;
        public readonly IAuthorization<IPsaContext, Task> field2;
        public readonly ICaseRepository field3;
    }


    public class TaskStatusAuthorization : PsaEntityAuthorization<TaskStatus>
    {
        public TaskStatusAuthorization(
            IContextService<IPsaContext> arg0,
            ITaskRepository arg1,
            IAuthorization<IPsaContext, Task> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ITaskRepository field1;
        public readonly IAuthorization<IPsaContext, Task> field2;
    }


    public class TaskStatusCommentAuthorization : PsaEntityAuthorization<TaskStatusComment>
    {
        public TaskStatusCommentAuthorization(
            IContextService<IPsaContext> arg0,
            ITaskRepository arg1,
            ITaskStatusRepository arg2,
            IAuthorization<IPsaContext, Task> arg3
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly ITaskRepository field1;
        public readonly ITaskStatusRepository field2;
        public readonly IAuthorization<IPsaContext, Task> field3;
    }


    public class TaskStatusTypeAuthorization : SettingsEntityAuthorization<TaskStatusType>
    {
        public TaskStatusTypeAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class TaxAuthorization : SettingsEntityAuthorization<Tax>
    {
        public TaxAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class TemporaryHourAuthorization : PsaEntityAllowAllAuthorization<TemporaryHour>
    {
        public TemporaryHourAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class TemporaryItemAuthorization : PsaEntityAllowAllAuthorization<TemporaryItem>
    {
        public TemporaryItemAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class TermsOfServiceApprovalAuthorization : PsaEntityAuthorization<TermsOfServiceApproval>
    {
        public TermsOfServiceApprovalAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class TimeEntryAuthorization : PsaEntityAllowAllAuthorization<TimeEntry>
    {
        public TimeEntryAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class TimeEntrySuggestedRowAuthorization : PsaEntityAuthorization<TimeEntrySuggestedRow>
    {
        public TimeEntrySuggestedRowAuthorization(
            IContextService<IPsaContext> arg0,
            IUserRepository arg1,
            IAuthorization<IPsaContext, User> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IUserRepository field1;
        public readonly IAuthorization<IPsaContext, User> field2;
    }


    public class TimeEntryTypeAuthorization : SettingsEntityAuthorization<TimeEntryType>
    {
        public TimeEntryTypeAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class TravelReimbursementAuthorization : PsaEntityAuthorization<TravelReimbursement>
    {
        public TravelReimbursementAuthorization(
            IContextService<IPsaContext> arg0,
            IUserRepository arg1
        ) : base(arg0)
        {
            field1 = arg1;
        }

        public readonly IUserRepository field1;
    }


    public class TravelReimbursementStatusAuthorization : SettingsEntityAuthorization<TravelReimbursementStatus>
    {
        public TravelReimbursementStatusAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class TreeTaskAuthorization : PsaEntityAuthorization<TreeTask>
    {
        public TreeTaskAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class UserAuthorization : PsaEntityAuthorization<User>
    {
        public UserAuthorization(
            IContextService<IPsaContext> arg0,
            IUserRepository arg1
        ) : base(arg0)
        {
            field1 = arg1;
        }

        public readonly IUserRepository field1;
    }

    public class UniqueUserAuthorization : PsaEntityAuthorization<UniqueUser>
    {
        public UniqueUserAuthorization(
            IContextService<IPsaContext> arg0,
            IUserRepository arg1
        ) : base(arg0)
        {
            field1 = arg1;
        }

        public readonly IUserRepository field1;
    }


    public class UserCostperCaseAuthorization : SettingsEntityAuthorization<UserCostPerCase>
    {
        public UserCostperCaseAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
    }


    public class UserSettingsAuthorization : PsaEntityAuthorization<UserSettings>
    {
        public UserSettingsAuthorization(
            IContextService<IPsaContext> arg0,
            IUserRepository arg1
        ) : base(arg0)
        {
            field1 = arg1;
        }

        public readonly IUserRepository field1;
    }


    public class UserTagAuthorization : TagBaseAuthorization<User, UserTag>
    {
        public UserTagAuthorization(
            IContextService<IPsaContext> arg0,
            IAuthorization<IPsaContext, User> arg1,
            IUserRepository arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class UserTaskFavoriteAuthorization : PsaEntityAllowAllAuthorization<UserTaskFavorite>
    {
        public UserTaskFavoriteAuthorization(
            IContextService<IPsaContext> arg0,
            ITaskRepository arg1,
            IAuthorization<IPsaContext, Task> arg2,
            IUserRepository arg3,
            IAuthorization<IPsaContext, User> arg4
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly ITaskRepository field1;
        public readonly IAuthorization<IPsaContext, Task> field2;
        public readonly IUserRepository field3;
        public readonly IAuthorization<IPsaContext, User> field4;
    }


    public class UserWeeklyViewRowAuthorization : PsaEntityAuthorization<UserWeeklyViewRow>
    {
        public UserWeeklyViewRowAuthorization(
            IContextService<IPsaContext> arg0,
            IUserRepository arg1,
            IAuthorization<IPsaContext, User> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IUserRepository field1;
        public readonly IAuthorization<IPsaContext, User> field2;
    }


    public class WorkdayAuthorization : PsaEntityAllowAllAuthorization<Workday>
    {
        public WorkdayAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class WorkHourSuggestedRowAuthorization : PsaEntityAuthorization<WorkHourSuggestedRow>
    {
        public WorkHourSuggestedRowAuthorization(
            IContextService<IPsaContext> arg0,
            IUserRepository arg1,
            IAuthorization<IPsaContext, User> arg2
        ) : base(arg0)
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IUserRepository field1;
        public readonly IAuthorization<IPsaContext, User> field2;
    }


    public class WorkingDayExceptionAuthorization : AuthorizationBase<IPsaContext, WorkingDayException>
        , IAuthorization
    {
        public WorkingDayExceptionAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class WorkPriceAuthorization : PsaEntityAuthorization<WorkPrice>
    {
        public WorkPriceAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1
        ) : base(arg0)
        {
            field1 = arg1;
        }

        public readonly ICaseRepository field1;
    }


    public class WorkingDayAuthorization : PsaEntityAuthorization<WorkingDayException>
    {
        public WorkingDayAuthorization(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1
        ) : base(arg0)
        {
            field1 = arg1;
        }

        public readonly ICaseRepository field1;
    }


    public class WorkTypeAuthorization : SettingsEntityAuthorization<WorkType>
    {
        public WorkTypeAuthorization(
            IContextService<IPsaContext> arg0
        ) : base(arg0)
        {
        }
    }


    public class AuthorizedIPAddressService : OrganizationEntityService<AuthorizedIPAddress,
            IAuthorizedIpAddressRepository, User, IPsaContext>
        , IAuthorizedIPAddressService
    {
        public AuthorizedIPAddressService(
            IContextService<IPsaContext> arg0,
            IAuthorizedIpAddressRepository arg1,
            IValidator<AuthorizedIPAddress> arg2,
            IAuthorization<IPsaContext, AuthorizedIPAddress> arg3,
            IOrganizationAddonService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IAuthorizedIpAddressRepository field1;
        public readonly IValidator<AuthorizedIPAddress> field2;
        public readonly IAuthorization<IPsaContext, AuthorizedIPAddress> field3;
        public readonly IOrganizationAddonService field4;
    }


    public class BackgroundTaskService :
        OrganizationEntityService<BackgroundTask, IBackgroundTaskRepository, User, IPsaContext>
        , IBackgroundTaskService
    {
        public BackgroundTaskService(
            IContextService<IPsaContext> arg0,
            IBackgroundTaskRepository arg1,
            IValidator<BackgroundTask> arg2,
            IAuthorization<IPsaContext, BackgroundTask> arg3,
            IBackgroundExecutorService arg4,
            IBackgroundTaskRunRepository arg5
        ) : base()
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
        public readonly IValidator<BackgroundTask> field2;
        public readonly IAuthorization<IPsaContext, BackgroundTask> field3;
        public readonly IBackgroundExecutorService field4;
        public readonly IBackgroundTaskRunRepository field5;
    }


    public class BankAccountService : OrganizationEntityService<BankAccount, IBankAccountRepository, User, IPsaContext>
        , IBankAccountService
    {
        public BankAccountService(
            IContextService<IPsaContext> arg0,
            IBankAccountRepository arg1,
            IValidator<BankAccount> arg2,
            IAuthorization<IPsaContext, BankAccount> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IBankAccountRepository field1;
        public readonly IValidator<BankAccount> field2;
        public readonly IAuthorization<IPsaContext, BankAccount> field3;
    }


    public class BillingInformationUpdateService
        : IBillingInformationUpdateService
    {
        public BillingInformationUpdateService(
            IAccountService arg0,
            ICaseService arg1,
            IInvoiceCaseRepository arg2,
            IContactService arg3,
            IUserService arg4,
            ICaseBillingAccountService arg5
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IAccountService field0;
        public readonly ICaseService field1;
        public readonly IInvoiceCaseRepository field2;
        public readonly IContactService field3;
        public readonly IUserService field4;
        public readonly ICaseBillingAccountService field5;
    }

    public class BusinessOverviewService :
        OrganizationEntityService<BusinessOverview, IBusinessOverviewRepository, User, IPsaContext>
        , IBusinessOverviewService
    {
        public BusinessOverviewService(
            IContextService<IPsaContext> arg0,
            IBusinessOverviewRepository arg1,
            IValidator<BusinessOverview> arg2,
            IAuthorization<IPsaContext, BusinessOverview> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IBusinessOverviewRepository field1;
        public readonly IValidator<BusinessOverview> field2;
        public readonly IAuthorization<IPsaContext, BusinessOverview> field3;
    }


    public class CalendarSyncActivityNonAppParticipantService : OrganizationEntityService<
            CalendarSyncActivityNonAppParticipant, ICalendarSyncActivityNonAppParticipantRepository, User,
            IPsaContext>
        , ICalendarSyncActivityNonAppParticipantService
    {
        public CalendarSyncActivityNonAppParticipantService(
            IContextService<IPsaContext> arg0,
            ICalendarSyncActivityNonAppParticipantRepository arg1,
            IValidator<CalendarSyncActivityNonAppParticipant> arg2,
            IAuthorization<IPsaContext, CalendarSyncActivityNonAppParticipant> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICalendarSyncActivityNonAppParticipantRepository field1;
        public readonly IValidator<CalendarSyncActivityNonAppParticipant> field2;
        public readonly IAuthorization<IPsaContext, CalendarSyncActivityNonAppParticipant> field3;
    }


    public class CalendarSyncService
        : ICalendarSyncService
    {
        public CalendarSyncService(
            IContextService<IPsaContext> arg0,
            ICalendarSyncDeviceRepository arg1,
            ICalendarSyncUserCalendarRepository arg2,
            ICalendarSyncActivityMapRepository arg3,
            ICalendarSyncActivityNonAppParticipantRepository arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICalendarSyncDeviceRepository field1;
        public readonly ICalendarSyncUserCalendarRepository field2;
        public readonly ICalendarSyncActivityMapRepository field3;
        public readonly ICalendarSyncActivityNonAppParticipantRepository field4;
    }


    public class CaseBillingAccountService : OrganizationEntityService<CaseBillingAccount, ICaseBillingAccountRepository
            , User, IPsaContext>
        , ICaseBillingAccountService
    {
        public CaseBillingAccountService(
            IContextService<IPsaContext> arg0,
            ICaseBillingAccountRepository arg1,
            IValidator<CaseBillingAccount> arg2,
            IAuthorization<IPsaContext, CaseBillingAccount> arg3,
            ICaseRepository arg4,
            IInvoiceRepository arg5, IAuditTrail<Case> arg6
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICaseBillingAccountRepository field1;
        public readonly IValidator<CaseBillingAccount> field2;
        public readonly IAuthorization<IPsaContext, CaseBillingAccount> field3;
        public readonly ICaseRepository field4;
        public readonly IInvoiceRepository field5;
        public readonly IAuditTrail<Case> field6;
    }


    public class CaseCommentService : OrganizationEntityService<CaseComment, ICaseCommentRepository, User, IPsaContext>
        , ICaseCommentService
    {
        public CaseCommentService(
            IContextService<IPsaContext> arg0,
            ICaseCommentRepository arg1,
            IValidator<CaseComment> arg2,
            IAuthorization<IPsaContext, CaseComment> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICaseCommentRepository field1;
        public readonly IValidator<CaseComment> field2;
        public readonly IAuthorization<IPsaContext, CaseComment> field3;
    }


    public class CaseCopyHandlerService
        : ICaseCopyHandlerService
    {
        public CaseCopyHandlerService(
            IContextService<IPsaContext> arg0,
            ITaskMemberRepository arg1,
            IFileService arg2,
            IItemService arg3,
            IItemSalesAccountRepository arg4,
            ISalesAccountRepository arg5,
            IOrganizationRepository arg6,
            ITaskService arg7,
            ICaseMemberService arg8,
            IActivityService arg9,
            ICaseProductRepository arg10,
            ICostCenterRepository arg11,
            IRecurringItemService arg12,
            ICaseWorkTypeService arg13,
            IUserRepository arg14,
            ICaseRepository arg15,
            ITagService arg16,
            ICaseTagRepository arg17
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
            field17 = arg17;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITaskMemberRepository field1;
        public readonly IFileService field2;
        public readonly IItemService field3;
        public readonly IItemSalesAccountRepository field4;
        public readonly ISalesAccountRepository field5;
        public readonly IOrganizationRepository field6;
        public readonly ITaskService field7;
        public readonly ICaseMemberService field8;
        public readonly IActivityService field9;
        public readonly ICaseProductRepository field10;
        public readonly ICostCenterRepository field11;
        public readonly IRecurringItemService field12;
        public readonly ICaseWorkTypeService field13;
        public readonly IUserRepository field14;
        public readonly ICaseRepository field15;
        public readonly ITagService field16;
        public readonly ICaseTagRepository field17;
    }


    public class CaseFileService : OrganizationEntityService<CaseFile, ICaseFileRepository, User, IPsaContext>
        , ICaseFileService
    {
        public CaseFileService(
            IContextService<IPsaContext> arg0,
            ICaseFileRepository arg1,
            IValidator<CaseFile> arg2,
            IAuthorization<IPsaContext, CaseFile> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICaseFileRepository field1;
        public readonly IValidator<CaseFile> field2;
        public readonly IAuthorization<IPsaContext, CaseFile> field3;
    }


    public class CaseMemberService : OrganizationEntityService<CaseMember, ICaseMemberRepository, User, IPsaContext>
        , ICaseMemberService
    {
        public CaseMemberService(
            IPsaContextService arg0,
            ICaseMemberRepository arg1,
            IValidator<CaseMember> arg2,
            IAuthorization<IPsaContext, CaseMember> arg3,
            IAuthorization<IPsaContext, Case> arg4,
            ITaskMemberRepository arg5,
            ITaskRepository arg6,
            IAuditTrailEntryRepository arg7
        ) : base()
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

        public readonly IPsaContextService field0;
        public readonly ICaseMemberRepository field1;
        public readonly IValidator<CaseMember> field2;
        public readonly IAuthorization<IPsaContext, CaseMember> field3;
        public readonly IAuthorization<IPsaContext, Case> field4;
        public readonly ITaskMemberRepository field5;
        public readonly ITaskRepository field6;
        public readonly IAuditTrailEntryRepository field7;
    }

    public interface INoteService<TEntity> : IEntityService<TEntity> where TEntity : INote, IOrganizationEntity
    {
    }


    public class CaseNoteService : OrganizationEntityService<CaseNote, ICaseNoteRepository, User, IPsaContext>
        , ICaseNoteService
    {
        public CaseNoteService(
            IContextService<IPsaContext> arg0,
            ICaseNoteRepository arg1,
            IValidator<CaseNote> arg2,
            IAuthorization<IPsaContext, CaseNote> arg3,
            INoteService<AccountNote> arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICaseNoteRepository field1;
        public readonly IValidator<CaseNote> field2;
        public readonly IAuthorization<IPsaContext, CaseNote> field3;
        public readonly INoteService<AccountNote> field4;
    }


    public class CaseProductService : OrganizationEntityService<CaseProduct, ICaseProductRepository, User, IPsaContext>
        , ICaseProductService
    {
        public CaseProductService(
            IContextService<IPsaContext> arg0,
            IValidator<CaseProduct> arg1,
            IAuthorization<IPsaContext, CaseProduct> arg2,
            ICaseProductRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IValidator<CaseProduct> field1;
        public readonly IAuthorization<IPsaContext, CaseProduct> field2;
        public readonly ICaseProductRepository field3;
    }


    public class CaseService : OrganizationEntityService<Case, ICaseRepository, User, IPsaContext>
        , ICaseService
    {
        public CaseService(
            IPsaContextService arg0,
            ICaseRepository arg1,
            IAuthorization<IPsaContext, Case> arg2,
            IValidator<Case> arg3,
            IPricelistService arg4,
            IPricelistVersionService arg5,
            IWorkPriceRepository arg6,
            IProductPriceRepository arg7,
            IOvertimePriceRepository arg8,
            ICaseMemberService arg9,
            ISalesStatusService arg10,
            IUserRepository arg11,
            IAccountService arg12,
            ITaskService arg13,
            IContactService arg14,
            IProductService arg15,
            ICaseWorkTypeService arg16,
            IWorkTypeService arg17,
            IRecurringItemService arg18,
            ICostCenterRevenueService arg19,
            IResourceAllocationRepository arg20,
            IBillingPlanRepository arg21,
            ICaseTagRepository arg22,
            IInvoiceCaseRepository arg23,
            ISalesStatusRepository arg24,
            ICaseNoteRepository arg25,
            ITimecardEventRepository arg26,
            ITaskRepository arg27,
            ICaseMemberRepository arg28,
            ICaseWorkTypeRepository arg29,
            ICaseProductRepository arg30,
            ICaseCommentRepository arg31,
            ICaseStatusRepository arg32,
            IExtranetCaseContactRepository arg33,
            IExtranetCaseInfoRepository arg34,
            ISalesProcessRepository arg35,
            ICaseStatusService arg36,
            IWorkPriceService arg37,
            ICaseBillingAccountService arg38,
            IAddressRepository arg39,
            IActivityService arg40,
            IBusinessUnitService arg41,
            ICostCenterRepository arg42,
            ICaseCopyHandlerService arg43,
            IHourRepository arg44,
            IItemRepository arg45,
            IRecurringItemRepository arg46, IAuditTrail<Case> arg47,
            IActivityContactMemberRepository arg49,
            IUserWeeklyViewRowRepository arg50
        ) : base()
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
            field17 = arg17;
            field18 = arg18;
            field19 = arg19;
            field20 = arg20;
            field21 = arg21;
            field22 = arg22;
            field23 = arg23;
            field24 = arg24;
            field25 = arg25;
            field26 = arg26;
            field27 = arg27;
            field28 = arg28;
            field29 = arg29;
            field30 = arg30;
            field31 = arg31;
            field32 = arg32;
            field33 = arg33;
            field34 = arg34;
            field35 = arg35;
            field36 = arg36;
            field37 = arg37;
            field38 = arg38;
            field39 = arg39;
            field40 = arg40;
            field41 = arg41;
            field42 = arg42;
            field43 = arg43;
            field44 = arg44;
            field45 = arg45;
            field46 = arg46;
            field47 = arg47;
            field49 = arg49;
            field50 = arg50;
        }

        public readonly IPsaContextService field0;
        public readonly ICaseRepository field1;
        public readonly IAuthorization<IPsaContext, Case> field2;
        public readonly IValidator<Case> field3;
        public readonly IPricelistService field4;
        public readonly IPricelistVersionService field5;
        public readonly IWorkPriceRepository field6;
        public readonly IProductPriceRepository field7;
        public readonly IOvertimePriceRepository field8;
        public readonly ICaseMemberService field9;
        public readonly ISalesStatusService field10;
        public readonly IUserRepository field11;
        public readonly IAccountService field12;
        public readonly ITaskService field13;
        public readonly IContactService field14;
        public readonly IProductService field15;
        public readonly ICaseWorkTypeService field16;
        public readonly IWorkTypeService field17;
        public readonly IRecurringItemService field18;
        public readonly ICostCenterRevenueService field19;
        public readonly IResourceAllocationRepository field20;
        public readonly IBillingPlanRepository field21;
        public readonly ICaseTagRepository field22;
        public readonly IInvoiceCaseRepository field23;
        public readonly ISalesStatusRepository field24;
        public readonly ICaseNoteRepository field25;
        public readonly ITimecardEventRepository field26;
        public readonly ITaskRepository field27;
        public readonly ICaseMemberRepository field28;
        public readonly ICaseWorkTypeRepository field29;
        public readonly ICaseProductRepository field30;
        public readonly ICaseCommentRepository field31;
        public readonly ICaseStatusRepository field32;
        public readonly IExtranetCaseContactRepository field33;
        public readonly IExtranetCaseInfoRepository field34;
        public readonly ISalesProcessRepository field35;
        public readonly ICaseStatusService field36;
        public readonly IWorkPriceService field37;
        public readonly ICaseBillingAccountService field38;
        public readonly IAddressRepository field39;
        public readonly IActivityService field40;
        public readonly IBusinessUnitService field41;
        public readonly ICostCenterRepository field42;
        public readonly ICaseCopyHandlerService field43;
        public readonly IHourRepository field44;
        public readonly IItemRepository field45;
        public readonly IRecurringItemRepository field46;
        public readonly IAuditTrail<Case> field47;
        public readonly IActivityContactMemberRepository field49;
        public readonly IUserWeeklyViewRowRepository field50;
    }


    public class CaseStatusService : OrganizationEntityService<CaseStatus, ICaseStatusRepository, User, IPsaContext>
        , ICaseStatusService
    {
        public CaseStatusService(
            IContextService<IPsaContext> arg0,
            ICaseStatusRepository arg1,
            IValidator<CaseStatus> arg2,
            IAuthorization<IPsaContext, CaseStatus> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICaseStatusRepository field1;
        public readonly IValidator<CaseStatus> field2;
        public readonly IAuthorization<IPsaContext, CaseStatus> field3;
    }


    public class CaseStatusTypeService :
        OrganizationEntityService<CaseStatusType, ICaseStatusTypeRepository, User, IPsaContext>
        , ICaseStatusTypeService
    {
        public CaseStatusTypeService(
            IContextService<IPsaContext> arg0,
            ICaseStatusTypeRepository arg1,
            IValidator<CaseStatusType> arg2,
            IAuthorization<IPsaContext, CaseStatusType> arg3,
            ICaseStatusRepository arg4,
            ICaseRepository arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICaseStatusTypeRepository field1;
        public readonly IValidator<CaseStatusType> field2;
        public readonly IAuthorization<IPsaContext, CaseStatusType> field3;
        public readonly ICaseStatusRepository field4;
        public readonly ICaseRepository field5;
    }


    public class CaseTagService : OrganizationEntityService<CaseTag, ICaseTagRepository, User, IPsaContext>
        , ICaseTagService
    {
        public CaseTagService(
            IContextService<IPsaContext> arg0,
            ICaseTagRepository arg1,
            IValidator<CaseTag> arg2,
            IAuthorization<IPsaContext, CaseTag> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICaseTagRepository field1;
        public readonly IValidator<CaseTag> field2;
        public readonly IAuthorization<IPsaContext, CaseTag> field3;
    }


    public class CaseWorkTypeService :
        OrganizationEntityService<CaseWorkType, ICaseWorkTypeRepository, User, IPsaContext>
        , ICaseWorkTypeService
    {
        public CaseWorkTypeService(
            IContextService<IPsaContext> arg0,
            IValidator<CaseWorkType> arg1,
            IAuthorization<IPsaContext, CaseWorkType> arg2,
            ICaseWorkTypeRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IValidator<CaseWorkType> field1;
        public readonly IAuthorization<IPsaContext, CaseWorkType> field2;
        public readonly ICaseWorkTypeRepository field3;
    }


    public class CommunicationMethodService : OrganizationEntityService<CommunicationMethod,
            ICommunicationMethodRepository, User, IPsaContext>
        , ICommunicationMethodService
    {
        public CommunicationMethodService(
            IContextService<IPsaContext> arg0,
            ICommunicationMethodRepository arg1,
            IValidator<CommunicationMethod> arg2,
            IAuthorization<IPsaContext, CommunicationMethod> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICommunicationMethodRepository field1;
        public readonly IValidator<CommunicationMethod> field2;
        public readonly IAuthorization<IPsaContext, CommunicationMethod> field3;
    }


    public class ContactAnonymizer
        : IContactAnonymizer
    {
        public ContactAnonymizer(
            IContactRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IContactRepository field0;
    }


    public class ContactAuditTrailServiceDecorator
        : IContactService
    {
        public ContactAuditTrailServiceDecorator(
            IContextService<IPsaContext> arg0,
            IContactService arg1,
            IContactRepository arg2, IAuditTrail<Contact> arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IContactService field1;
        public readonly IContactRepository field2;
        public readonly IAuditTrail<Contact> field3;
    }


    public class ContactCommunicationAuditTrailServiceDecorator
        : IContactCommunicationService
    {
        public ContactCommunicationAuditTrailServiceDecorator(
            IContactCommunicationService arg0,
            ICommunicatesWithRepository arg1, IAuditTrail<CommunicatesWith> arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContactCommunicationService field0;
        public readonly ICommunicatesWithRepository field1;
        public readonly IAuditTrail<CommunicatesWith> field2;
    }


    public class ContactCommunicationService :
        OrganizationEntityService<CommunicatesWith, ICommunicatesWithRepository, User, IPsaContext>
        , IContactCommunicationService
    {
        public ContactCommunicationService(
            IContextService<IPsaContext> arg0,
            ICommunicatesWithRepository arg1,
            IValidator<CommunicatesWith> arg2,
            IAuthorization<IPsaContext, CommunicatesWith> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICommunicatesWithRepository field1;
        public readonly IValidator<CommunicatesWith> field2;
        public readonly IAuthorization<IPsaContext, CommunicatesWith> field3;
    }


    public class ContactRoleService : OrganizationEntityService<ContactRole, IContactRoleRepository, User, IPsaContext>
        , IContactRoleService
    {
        public ContactRoleService(
            IContextService<IPsaContext> arg0,
            IContactRoleRepository arg1,
            IValidator<ContactRole> arg2,
            IAuthorization<IPsaContext, ContactRole> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IContactRoleRepository field1;
        public readonly IValidator<ContactRole> field2;
        public readonly IAuthorization<IPsaContext, ContactRole> field3;
    }


    public class SatisfactionLevelType
    {
    }


    public class ContactService : OrganizationEntityService<Contact, IContactRepository, User, IPsaContext>
        , IContactService
    {
        public ContactService(
            IContextService<IPsaContext> arg0,
            IContactRepository arg1,
            IValidator<Contact> arg2,
            IAuthorization<IPsaContext, Contact> arg3,
            ICommunicatesWithRepository arg4,
            ICaseRepository arg5,
            IContactAnonymizer arg6
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IContactRepository field1;
        public readonly IValidator<Contact> field2;
        public readonly IAuthorization<IPsaContext, Contact> field3;
        public readonly ICommunicatesWithRepository field4;
        public readonly ICaseRepository field5;
        public readonly IContactAnonymizer field6;
    }


    public class ContactTagService : OrganizationEntityService<ContactTag, IContactTagRepository, User, IPsaContext>
        , IContactTagService
    {
        public ContactTagService(
            IContextService<IPsaContext> arg0,
            IContactTagRepository arg1,
            IValidator<ContactTag> arg2,
            IAuthorization<IPsaContext, ContactTag> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IContactTagRepository field1;
        public readonly IValidator<ContactTag> field2;
        public readonly IAuthorization<IPsaContext, ContactTag> field3;
    }


    public class CostCenterRevenueService : OrganizationEntityService<CostCenterRevenue, ICostCenterRevenueRepository,
            User, IPsaContext>
        , ICostCenterRevenueService
    {
        public CostCenterRevenueService(
            IContextService<IPsaContext> arg0,
            ICostCenterRevenueRepository arg1,
            IValidator<CostCenterRevenue> arg2,
            IAuthorization<IPsaContext, CostCenterRevenue> arg3, IAuditTrail<CostCenterRevenue> arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICostCenterRevenueRepository field1;
        public readonly IValidator<CostCenterRevenue> field2;
        public readonly IAuthorization<IPsaContext, CostCenterRevenue> field3;
        public readonly IAuditTrail<CostCenterRevenue> field4;
    }


    public class CostCenterService : OrganizationEntityService<CostCenter, ICostCenterRepository, User, IPsaContext>
        , ICostCenterService
    {
        public CostCenterService(
            IContextService<IPsaContext> arg0,
            ICostCenterRepository arg1,
            IValidator<CostCenter> arg2,
            IAuthorization<IPsaContext, CostCenter> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICostCenterRepository field1;
        public readonly IValidator<CostCenter> field2;
        public readonly IAuthorization<IPsaContext, CostCenter> field3;
    }


    public class CountryProductService : PsaEntityService<CountryProduct, ICountryProductRepository>
        , ICountryProductService
    {
        public CountryProductService(
            IContextService<IPsaContext> arg0,
            ICountryProductRepository arg1,
            IValidator<CountryProduct> arg2,
            IAuthorization<IPsaContext, CountryProduct> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICountryProductRepository field1;
        public readonly IValidator<CountryProduct> field2;
        public readonly IAuthorization<IPsaContext, CountryProduct> field3;
    }


    public class AccountService : EntityService<IPsaContext, Account, IAccountRepository>, IAccountService
    {
        private readonly ICompanyService _CompanyService;
        private readonly IPsaOrganizationService _OrganizationService;
        private readonly IOrganizationCompanyRepository _OrganizationCompanyRepository;

        public AccountService(IContextService<IPsaContext> contextService, IAccountRepository repository, IValidator<Account> validator, IAuthorization<IPsaContext, Account> authorization, ICompanyService companyService, IPsaOrganizationService organizationService, IOrganizationCompanyRepository organizationCompanyRepository) : base(contextService, repository, validator, authorization)
        {
            _CompanyService = companyService;
            _OrganizationService = organizationService;
            _OrganizationCompanyRepository = organizationCompanyRepository;
        }
    }

    public class DashboardPartService :
        OrganizationEntityService<DashboardPart, IDashboardPartRepository, User, IPsaContext>
        , IDashboardPartService
    {
        public DashboardPartService(
            IContextService<IPsaContext> arg0,
            IDashboardPartRepository arg1,
            IValidator<DashboardPart> arg2,
            IAuthorization<IPsaContext, DashboardPart> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IDashboardPartRepository field1;
        public readonly IValidator<DashboardPart> field2;
        public readonly IAuthorization<IPsaContext, DashboardPart> field3;
    }


    public class DashboardService : OrganizationEntityService<Dashboard, IDashboardRepository, User, IPsaContext>
        , IDashboardService
    {
        public DashboardService(
            IContextService<IPsaContext> arg0,
            IDashboardRepository arg1,
            IValidator<Dashboard> arg2,
            IAuthorization<IPsaContext, Dashboard> arg3,
            IDashboardPartRepository arg4,
            IUserRepository arg5,
            IProfileDashboardService arg6
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IDashboardRepository field1;
        public readonly IValidator<Dashboard> field2;
        public readonly IAuthorization<IPsaContext, Dashboard> field3;
        public readonly IDashboardPartRepository field4;
        public readonly IUserRepository field5;
        public readonly IProfileDashboardService field6;
    }


    public class DataAnalyticsHttpService
        : IDataAnalyticsHttpService
    {
        public DataAnalyticsHttpService(
        )
        {
        }
    }


    public class DataAnalyticsService
        : IDataAnalyticsService
    {
        public DataAnalyticsService(
            IPsaContextService arg0,
            IFeatureToggleService arg1,
            IDataAnalyticsHttpService arg2,
            IDataAnalyticsUserPermissionService arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IFeatureToggleService field1;
        public readonly IDataAnalyticsHttpService field2;
        public readonly IDataAnalyticsUserPermissionService field3;
    }


    public class DataAnalyticsUserPermissionService
        : IDataAnalyticsUserPermissionService
    {
        public DataAnalyticsUserPermissionService(
        )
        {
        }
    }


    public class DemoDataHandlerService
        : IDemoDataHandlerService
    {
        public DemoDataHandlerService(
            IContextService<IPsaContext> arg0,
            IAccountService arg1,
            IContactService arg2,
            ICaseService arg3,
            ITaskService arg4,
            IHourService arg5,
            IProductService arg6,
            ICountryProductService arg7,
            IItemService arg8,
            IInvoiceService arg9,
            IInvoiceStatusService arg10,
            IUserService arg11,
            IBusinessUnitService arg12,
            IWorkTypeService arg13,
            IAddressService arg14,
            ICompanyService arg15,
            ICommunicationMethodService arg16,
            ICommunicatesWithRepository arg17,
            ISalesProcessService arg18,
            ICaseStatusTypeService arg19,
            ILanguageService arg20,
            ILeadSourceService arg21,
            IPricelistService arg22,
            IActivityService arg23,
            IActivityTypeService arg24,
            ICaseMemberService arg25,
            IEmploymentService arg26
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
            field17 = arg17;
            field18 = arg18;
            field19 = arg19;
            field20 = arg20;
            field21 = arg21;
            field22 = arg22;
            field23 = arg23;
            field24 = arg24;
            field25 = arg25;
            field26 = arg26;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IAccountService field1;
        public readonly IContactService field2;
        public readonly ICaseService field3;
        public readonly ITaskService field4;
        public readonly IHourService field5;
        public readonly IProductService field6;
        public readonly ICountryProductService field7;
        public readonly IItemService field8;
        public readonly IInvoiceService field9;
        public readonly IInvoiceStatusService field10;
        public readonly IUserService field11;
        public readonly IBusinessUnitService field12;
        public readonly IWorkTypeService field13;
        public readonly IAddressService field14;
        public readonly ICompanyService field15;
        public readonly ICommunicationMethodService field16;
        public readonly ICommunicatesWithRepository field17;
        public readonly ISalesProcessService field18;
        public readonly ICaseStatusTypeService field19;
        public readonly ILanguageService field20;
        public readonly ILeadSourceService field21;
        public readonly IPricelistService field22;
        public readonly IActivityService field23;
        public readonly IActivityTypeService field24;
        public readonly ICaseMemberService field25;
        public readonly IEmploymentService field26;
    }


    public class EmailBuilderService
        : IEmailBuilderService
    {
        public EmailBuilderService(
        )
        {
        }
    }


    public class EnvironmentClosedEmailBuilder
        : IEnvironmentClosedEmailBuilder
    {
        public EnvironmentClosedEmailBuilder(
            IPartnerService arg0,
            IMailContentBuilder arg1,
            IAppSettings arg2,
            IHttpUtility arg3,
            IUserRepository arg4,
            IMasterOrganizationRepository arg5,
            IContextService arg6
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

        public readonly IPartnerService field0;
        public readonly IMailContentBuilder field1;
        public readonly IAppSettings field2;
        public readonly IHttpUtility field3;
        public readonly IUserRepository field4;
        public readonly IMasterOrganizationRepository field5;
        public readonly IContextService field6;
    }


    public class EnvironmentOpenedEmailBuilder
        : IEnvironmentOpenedEmailBuilder
    {
        public EnvironmentOpenedEmailBuilder(
            IEmailTemplateService arg0,
            IDict arg1,
            IAppSettings arg2,
            IUserRepository arg3,
            IMasterOrganizationRepository arg4,
            IConnClientService arg5,
            IDistributorHelperService arg6
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

        public readonly IEmailTemplateService field0;
        public readonly IDict field1;
        public readonly IAppSettings field2;
        public readonly IUserRepository field3;
        public readonly IMasterOrganizationRepository field4;
        public readonly IConnClientService field5;
        public readonly IDistributorHelperService field6;
    }


    public class FileDataService : OrganizationEntityService<FileData, IFileDataRepository, User, IPsaContext>
        , IFileDataService
    {
        public FileDataService(
            IContextService<IPsaContext> arg0,
            IFileDataRepository arg1,
            IValidator<FileData> arg2,
            IAuthorization<IPsaContext, FileData> arg3,
            IScannerService arg5,
            IFileDownloadService arg6
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            //field5 = arg5;
            //field6 = arg6;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IFileDataRepository field1;
        public readonly IValidator<FileData> field2;
        public readonly IAuthorization<IPsaContext, FileData> field3;
        public readonly IScannerService field5;
        public readonly IFileDownloadService field6;
    }


    public class FileDownloadService : OrganizationSettingsService<FileDownload, IFileDownloadRepository>
        , IFileDownloadService
    {
        public FileDownloadService(
            IContextService<IPsaContext> arg0,
            IFileDownloadRepository arg1,
            IValidator<FileDownload> arg2,
            IAuthorization<IPsaContext, FileDownload> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IFileDownloadRepository field1;
        public readonly IValidator<FileDownload> field2;
        public readonly IAuthorization<IPsaContext, FileDownload> field3;
    }


    public class FileTagService : OrganizationEntityService<FileTag, IFileTagRepository, User, IPsaContext>
        , IFileTagService
    {
        public FileTagService(
            IContextService<IPsaContext> arg0,
            IFileTagRepository arg1,
            IValidator<FileTag> arg2,
            IAuthorization<IPsaContext, FileTag> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IFileTagRepository field1;
        public readonly IValidator<FileTag> field2;
        public readonly IAuthorization<IPsaContext, FileTag> field3;
    }


    public class FlextimeContext
    {
        public FlextimeContext(
            User arg0
        )
        {
            field0 = arg0;
        }

        public readonly User field0;
    }


    public class FlextimeManagerService
        : IFlextimeManagerService
    {
        public FlextimeManagerService(
            IContextService<IPsaContext> arg0,
            IEmploymentRepository arg1,
            IActivityRepository arg2,
            IUserRepository arg3,
            IActivityTypeRepository arg4,
            IWorkdayRepository arg5,
            IWorkdaySummaryRepository arg6,
            IUserAuthorization arg7,
            IWorkingDayService arg8
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

        public readonly IContextService<IPsaContext> field0;
        public readonly IEmploymentRepository field1;
        public readonly IActivityRepository field2;
        public readonly IUserRepository field3;
        public readonly IActivityTypeRepository field4;
        public readonly IWorkdayRepository field5;
        public readonly IWorkdaySummaryRepository field6;
        public readonly IUserAuthorization field7;
        public readonly IWorkingDayService field8;
    }


    public class GoogleDriveExceptionFactory
    {
        public GoogleDriveExceptionFactory(
        )
        {
        }
    }


    public class GoogleDriveSettings
        : IGoogleDriveSettings
    {
        public GoogleDriveSettings(
            ISettingsService arg0
        )
        {
            field0 = arg0;
        }

        public readonly ISettingsService field0;
    }

    public interface IGoogleDriveSettings
    {
    }


    public class GuidService
        : IGuidService
    {
        public GuidService(
            IContextService<IPsaContext> arg0
        )
        {
            field0 = arg0;
        }

        public readonly IContextService<IPsaContext> field0;
    }


    public class HeartBeatService
        : IHeartBeatService
    {
        public HeartBeatService(
            IHeartBeatRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IHeartBeatRepository field0;
    }


    public class HourEmailService
        : IHourEmailService
    {
        public HourEmailService(
            IContextService<IPsaContext> arg0,
            IEmailBuilderService arg1,
            IUserRepository arg2,
            ITaskRepository arg3,
            IMailClient arg4,
            IHourService arg5,
            IDistributorHelperService arg6,
            IMasterOrganizationRepository arg7
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

        public readonly IContextService<IPsaContext> field0;
        public readonly IEmailBuilderService field1;
        public readonly IUserRepository field2;
        public readonly ITaskRepository field3;
        public readonly IMailClient field4;
        public readonly IHourService field5;
        public readonly IDistributorHelperService field6;
        public readonly IMasterOrganizationRepository field7;
    }


    public interface IAccessRightService
    {
    }


    public interface IContactAnonymizer
    {
    }


    public interface IDemoDataHandlerService
    {
    }


    public interface IInvoiceBreakdownService
    {
    }


    public class InvoiceBreakdownService
        : IInvoiceBreakdownService
    {
        public InvoiceBreakdownService(
            IContextService<IPsaContext> arg0,
            IItemService arg1,
            IInvoiceRepository arg2,
            IInvoiceRowService arg3,
            ITaskRepository arg4,
            IHourService arg5,
            IInvoiceHelperService arg6
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

        public readonly IContextService<IPsaContext> field0;
        public readonly IItemService field1;
        public readonly IInvoiceRepository field2;
        public readonly IInvoiceRowService field3;
        public readonly ITaskRepository field4;
        public readonly IHourService field5;
        public readonly IInvoiceHelperService field6;
    }


    public class IndustryService : OrganizationEntityService<Industry, IIndustryRepository, User, IPsaContext>
        , IIndustryService
    {
        public IndustryService(
            IContextService<IPsaContext> arg0,
            IIndustryRepository arg1,
            IValidator<Industry> arg2,
            IAuthorization<IPsaContext, Industry> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IIndustryRepository field1;
        public readonly IValidator<Industry> field2;
        public readonly IAuthorization<IPsaContext, Industry> field3;
    }


    public class IntegrationErrorService :
        OrganizationEntityService<IntegrationError, IIntegrationErrorRepository, User, IPsaContext>
        , IIntegrationErrorService
    {
        public IntegrationErrorService(
            IContextService<IPsaContext> arg0,
            IIntegrationErrorRepository arg1,
            IValidator<IntegrationError> arg2,
            IAuthorization<IPsaContext, IntegrationError> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IIntegrationErrorRepository field1;
        public readonly IValidator<IntegrationError> field2;
        public readonly IAuthorization<IPsaContext, IntegrationError> field3;
    }


    public interface IBillingInformationUpdateService
    {
    }


    public interface IAccountNavigationHistoryService
    {
    }


    public interface IActivityContactMemberService
        : IEntityService<ActivityContactMember>
    {
    }


    public interface IActivityEmailService
    {
    }


    public interface IActivityResourceMemberService
        : IEntityService<ActivityResourceMember>
    {
    }


    public class RecurringActivityFilter
    {
    }


    public interface IActivityService
        : IEntityService<Activity>
    {
    }


    public interface IActivityStatusService
        : IEntityService<ActivityStatus>
    {
    }


    public interface IActivityTypeService
        : IEntityService<ActivityType>
    {
    }


    public interface IActivityUserMemberService
        : IEntityService<ActivityUserMember>
    {
    }


    public interface IApiClientHandlerService
    {
    }


    public interface IAuditTrailService
        : IEntityService<AuditTrailEntry>
    {
    }


    public interface IAuthorizedIPAddressService
        : IEntityService<AuthorizedIPAddress>
    {
    }


    public interface IBackgroundTaskService
        : IEntityService<BackgroundTask>
    {
    }


    public interface IBankAccountService
        : IEntityService<BankAccount>
    {
    }


    public interface IBillingPlanService
        : IEntityService<BillingPlan>
    {
    }


    public interface IBusinessOverviewService
        : IEntityService<BusinessOverview>
    {
    }


    public interface IBusinessUnitService
        : IEntityService<BusinessUnit>
    {
    }


    public interface ICalendarSyncActivityNonAppParticipantService
        : IEntityService<CalendarSyncActivityNonAppParticipant>
    {
    }


    public interface ICalendarSyncService
    {
    }


    public interface ICaseBillingAccountService
        : IEntityService<CaseBillingAccount>
    {
    }


    public interface ICaseCommentService
        : IEntityService<CaseComment>
    {
    }


    public interface ICaseCopyHandlerService
    {
    }


    public interface ICaseFileService
        : IEntityService<CaseFile>
    {
    }


    public interface ICaseMemberService
        : IEntityService<CaseMember>
    {
    }


    public interface ICaseNoteService
        : INoteProviderService<CaseNote>, INoteService<CaseNote>
    {
    }


    public interface IAccountNoteService
        : INoteProviderService<AccountNote>, INoteService<AccountNote>
    {
    }

    public interface ICaseProductService
        : IEntityService<CaseProduct>
    {
    }


    public interface ICaseService
        : IEntityService<CaseService>
    {
    }


    public interface ICaseStatusService
        : IEntityService<CaseStatus>
    {
    }


    public interface ICaseStatusTypeService
        : IEntityService<CaseStatusType>
    {
    }


    public interface ICaseTagService
        : ITagEntityService<CaseTag>
    {
    }


    public interface ICaseWorkTypeService
        : IEntityService<CaseWorkType>
    {
    }


    public interface ICommunicationMethodService
        : IEntityService<CommunicationMethod>
    {
    }


    public interface IContactCommunicationService
    {
    }


    public interface IContactRoleService
        : IEntityService<ContactRole>
    {
    }


    public interface IContactService
        : IEntityService<Contact>
    {
    }


    public interface IContactTagService
        : ITagEntityService<ContactTag>
    {
    }


    public interface ICostCenterRevenueService
        : IEntityService<CostCenterRevenue>
    {
    }


    public interface ICostCenterService
        : IEntityService<CostCenter>
    {
    }


    public interface ICountryProductService
        : IEntityService<CountryProduct>
    {
    }


    public interface IDashboardPartService
        : IEntityService<DashboardPart>
    {
    }


    public interface IDashboardService
        : IEntityService<Dashboard>
    {
    }


    public interface IDataAnalyticsHttpService
    {
    }


    public interface IDataAnalyticsService
    {
    }


    public interface IDataAnalyticsUserPermissionService
    {
    }


    public interface IEmailBuilderService
    {
    }


    public interface IEmploymentService
        : IEntityService<Employment>
    {
    }


    public interface IFileDataService
        : IEntityService<FileData>
    {
    }


    public interface IFileDownloadService
        : IEntityService<FileDownload>
    {
    }


    public interface IFileTagService
        : ITagEntityService<FileTag>
    {
    }


    public interface IFlextimeAdjustmentService
        : IEntityService<Flextime>
    {
    }


    public interface IFlextimeManagerService
    {
    }


    public interface IGuidService
    {
    }


    public interface IHeartBeatService
    {
    }


    public interface IHourEmailService
    {
    }


    public interface IHourService
        : IEntityService<Hour>
    {
    }


    public interface IIndustryService
        : IEntityService<Industry>
    {
    }


    public interface IIntegrationErrorService
        : IEntityService<IntegrationError>
    {
    }


    public interface IInvoiceBillingAccountGrouperService
    {
    }


    public class InvoiceBillingAccountGrouperService
        : IInvoiceBillingAccountGrouperService
    {
        public InvoiceBillingAccountGrouperService(
            ICaseRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly ICaseRepository field0;
    }


    public interface IInvoiceConfigService
        : IEntityService<InvoiceConfig>
    {
    }


    public class InvoiceConfigService :
        OrganizationEntityService<InvoiceConfig, IInvoiceConfigRepository, User, IPsaContext>
        , IInvoiceConfigService
    {
        public InvoiceConfigService(
            IContextService<IPsaContext> arg0,
            IInvoiceConfigRepository arg1,
            IValidator<InvoiceConfig> arg2,
            IAuthorization<IPsaContext, InvoiceConfig> arg3,
            IInvoiceHtmlRepository arg4,
            IInvoiceRepository arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceConfigRepository field1;
        public readonly IValidator<InvoiceConfig> field2;
        public readonly IAuthorization<IPsaContext, InvoiceConfig> field3;
        public readonly IInvoiceHtmlRepository field4;
        public readonly IInvoiceRepository field5;
    }


    public interface IInvoiceCreationHandlerService
    {
    }


    public class InvoiceCreationHandlerService
        : IInvoiceCreationHandlerService
    {
        public InvoiceCreationHandlerService(
            IContextService<IPsaContext> arg0,
            IInvoiceRepository arg1,
            IInvoiceStatusService arg2,
            IInvoiceCaseRepository arg3,
            IInvoiceConfigRepository arg4,
            IInvoiceHtmlRepository arg5,
            IInvoiceDetailsHelperService arg6,
            IInvoiceHelperService arg7,
            IAccountService arg8,
            ICompanyService arg9,
            IBusinessUnitRepository arg10,
            IOrganizationCompanyRepository arg11,
            ICountryService arg12,
            IInvoiceTemplateService arg13,
            IInvoiceHandlersAuthorization arg14,
            IRecurringItemService arg15,
            IValidator<Invoice> arg16, IAuditTrail<Invoice> arg17, IAuditTrail<InvoiceCase> arg18,
            ICurrencyService arg19
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
            field17 = arg17;
            field18 = arg18;
            field19 = arg19;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceRepository field1;
        public readonly IInvoiceStatusService field2;
        public readonly IInvoiceCaseRepository field3;
        public readonly IInvoiceConfigRepository field4;
        public readonly IInvoiceHtmlRepository field5;
        public readonly IInvoiceDetailsHelperService field6;
        public readonly IInvoiceHelperService field7;
        public readonly IAccountService field8;
        public readonly ICompanyService field9;
        public readonly IBusinessUnitRepository field10;
        public readonly IOrganizationCompanyRepository field11;
        public readonly ICountryService field12;
        public readonly IInvoiceTemplateService field13;
        public readonly IInvoiceHandlersAuthorization field14;
        public readonly IRecurringItemService field15;
        public readonly IValidator<Invoice> field16;
        public readonly IAuditTrail<Invoice> field17;
        public readonly IAuditTrail<InvoiceCase> field18;
        public readonly ICurrencyService field19;
    }


    public interface IInvoiceDeletionHandlerService
    {
    }


    public class InvoiceDeletionHandlerService
        : IInvoiceDeletionHandlerService
    {
        public InvoiceDeletionHandlerService(
            IContextService<IPsaContext> arg0,
            IInvoiceRepository arg1,
            IBusinessUnitService arg2,
            IInvoiceBankAccountRepository arg3,
            IRevenueRecognitionService arg4,
            ITaskRepository arg5,
            IHourRepository arg6,
            IItemRepository arg7,
            IReimbursedHourRepository arg8,
            IReimbursedItemRepository arg9,
            IInvoiceConfigService arg10,
            IOrganizationRepository arg11,
            ICaseRepository arg12,
            IInvoiceCaseRepository arg13,
            IInvoiceRowRepository arg14,
            IInvoiceHelperService arg15,
            IInvoiceHtmlService arg16,
            IBusinessUnitRepository arg17,
            IPricelistRepository arg18
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
            field17 = arg17;
            field18 = arg18;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceRepository field1;
        public readonly IBusinessUnitService field2;
        public readonly IInvoiceBankAccountRepository field3;
        public readonly IRevenueRecognitionService field4;
        public readonly ITaskRepository field5;
        public readonly IHourRepository field6;
        public readonly IItemRepository field7;
        public readonly IReimbursedHourRepository field8;
        public readonly IReimbursedItemRepository field9;
        public readonly IInvoiceConfigService field10;
        public readonly IOrganizationRepository field11;
        public readonly ICaseRepository field12;
        public readonly IInvoiceCaseRepository field13;
        public readonly IInvoiceRowRepository field14;
        public readonly IInvoiceHelperService field15;
        public readonly IInvoiceHtmlService field16;
        public readonly IBusinessUnitRepository field17;
        public readonly IPricelistRepository field18;
    }


    public interface IInvoiceDetailsHelperService
    {
    }


    public class InvoiceDetailsHelperService
        : IInvoiceDetailsHelperService
    {
        public InvoiceDetailsHelperService(
            IContextService<IPsaContext> arg0,
            IAddressService arg1,
            IContactService arg2,
            IUserRepository arg3,
            ICountryService arg4,
            ICountryRegionService arg5,
            ICurrencyService arg6,
            IBusinessUnitService arg7,
            ICaseBillingAccountService arg8
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

        public readonly IContextService<IPsaContext> field0;
        public readonly IAddressService field1;
        public readonly IContactService field2;
        public readonly IUserRepository field3;
        public readonly ICountryService field4;
        public readonly ICountryRegionService field5;
        public readonly ICurrencyService field6;
        public readonly IBusinessUnitService field7;
        public readonly ICaseBillingAccountService field8;
    }


    public interface IInvoiceFileService
        : IEntityService<InvoiceFile>
    {
    }


    public class InvoiceFileService : OrganizationEntityService<InvoiceFile, IInvoiceFileRepository, User, IPsaContext>
        , IInvoiceFileService
    {
        public InvoiceFileService(
            IFileService arg4
        ) : base()
        {

        }
    }


    public interface IInvoiceHelperService
    {
    }


    public class InvoiceHelperService
        : IInvoiceHelperService
    {
        public InvoiceHelperService(
            IContextService<IPsaContext> arg0,
            ILanguageService arg1,
            IBusinessUnitService arg2,
            IAccountRepository arg3,
            ICompanyService arg4,
            IAddressService arg5,
            ITaskRepository arg6,
            IInvoiceCaseRepository arg7,
            ICurrencyRoundingFactory arg8,
            ICurrencyService arg9
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

        public readonly IContextService<IPsaContext> field0;
        public readonly ILanguageService field1;
        public readonly IBusinessUnitService field2;
        public readonly IAccountRepository field3;
        public readonly ICompanyService field4;
        public readonly IAddressService field5;
        public readonly ITaskRepository field6;
        public readonly IInvoiceCaseRepository field7;
        public readonly ICurrencyRoundingFactory field8;
        public readonly ICurrencyService field9;
    }


    public interface IInvoiceHourHandlerService
    {
    }


    public class InvoiceHourHandlerService : InvoiceHandlerServiceBase
        , IInvoiceHourHandlerService
    {
        public InvoiceHourHandlerService(
            IContextService<IPsaContext> arg0,
            IHourRepository arg1,
            IInvoiceHelperService arg2,
            IUserService arg3,
            ITaskRepository arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IHourRepository field1;
        public readonly IInvoiceHelperService field2;
        public readonly IUserService field3;
        public readonly ITaskRepository field4;
    }


    public interface IInvoiceHtmlService
        : IEntityService<InvoiceHTML>
    {
    }


    public class InvoiceHtmlService : OrganizationEntityService<InvoiceHTML, IInvoiceHtmlRepository, User, IPsaContext>
        , IInvoiceHtmlService
    {
        public InvoiceHtmlService(
            IContextService<IPsaContext> arg0,
            IInvoiceHtmlRepository arg1,
            IValidator<InvoiceHTML> arg2,
            IAuthorization<IPsaContext, InvoiceHTML> arg3,
            IInvoiceConfigService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceHtmlRepository field1;
        public readonly IValidator<InvoiceHTML> field2;
        public readonly IAuthorization<IPsaContext, InvoiceHTML> field3;
        public readonly IInvoiceConfigService field4;
    }


    public interface IInvoiceItemHandlerService
    {
    }


    public class InvoiceItemHandlerService : InvoiceHandlerServiceBase
        , IInvoiceItemHandlerService
    {
        public InvoiceItemHandlerService(
            IContextService<IPsaContext> arg0,
            IItemService arg1,
            IInvoiceHelperService arg2,
            IUserService arg3,
            IRecurringItemService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IItemService field1;
        public readonly IInvoiceHelperService field2;
        public readonly IUserService field3;
        public readonly IRecurringItemService field4;
    }


    public interface IInvoiceReimbursementService
    {
    }


    public class InvoiceReimbursementService
        : IInvoiceReimbursementService
    {
        public InvoiceReimbursementService(
            IContextService<IPsaContext> arg0,
            IInvoiceStatusService arg1,
            IInvoiceHelperService arg2,
            ICaseService arg3,
            IReimbursedItemService arg4,
            IReimbursedHourService arg5,
            IInvoiceConfigService arg6,
            IHourService arg7,
            ITaskService arg8,
            IItemService arg9,
            IInvoiceService arg10,
            IInvoiceRowService arg11,
            IInvoiceBankAccountRepository arg12,
            IRevenueRecognitionService arg13,
            IDict arg14,
            IAccountService arg15
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

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceStatusService field1;
        public readonly IInvoiceHelperService field2;
        public readonly ICaseService field3;
        public readonly IReimbursedItemService field4;
        public readonly IReimbursedHourService field5;
        public readonly IInvoiceConfigService field6;
        public readonly IHourService field7;
        public readonly ITaskService field8;
        public readonly IItemService field9;
        public readonly IInvoiceService field10;
        public readonly IInvoiceRowService field11;
        public readonly IInvoiceBankAccountRepository field12;
        public readonly IRevenueRecognitionService field13;
        public readonly IDict field14;
        public readonly IAccountService field15;
    }


    public interface IInvoiceRowGroupHandlerService
    {
    }


    public class InvoiceRowGroupHandlerService
        : IInvoiceRowGroupHandlerService
    {
        public InvoiceRowGroupHandlerService(
            IInvoiceRowHandlerService arg0,
            IInvoiceRowService arg1,
            IInvoiceRowItemGroupService arg2,
            IInvoiceHelperService arg3,
            IInvoiceRowHourGroupService arg4,
            IHourRepository arg5,
            IItemRepository arg6
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

        public readonly IInvoiceRowHandlerService field0;
        public readonly IInvoiceRowService field1;
        public readonly IInvoiceRowItemGroupService field2;
        public readonly IInvoiceHelperService field3;
        public readonly IInvoiceRowHourGroupService field4;
        public readonly IHourRepository field5;
        public readonly IItemRepository field6;
    }


    public interface IInvoiceRowHandlerService
    {
    }


    public class InvoiceRowHandlerService
        : IInvoiceRowHandlerService
    {
        public InvoiceRowHandlerService(
            IContextService<IPsaContext> arg0,
            ICostCenterRepository arg1,
            ITaxService arg2,
            ICurrencyRepository arg3,
            ITaskRepository arg4,
            IWorkTypeRepository arg5,
            ISalesAccountRepository arg6,
            IInvoiceHelperService arg7
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

        public readonly IContextService<IPsaContext> field0;
        public readonly ICostCenterRepository field1;
        public readonly ITaxService field2;
        public readonly ICurrencyRepository field3;
        public readonly ITaskRepository field4;
        public readonly IWorkTypeRepository field5;
        public readonly ISalesAccountRepository field6;
        public readonly IInvoiceHelperService field7;
    }


    public interface IInvoiceRowHourGroupService
    {
    }


    public class InvoiceRowHourGroupService
        : IInvoiceRowHourGroupService
    {
        public InvoiceRowHourGroupService(
            IInvoiceHelperService arg0,
            IInvoiceRowHandlerService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IInvoiceHelperService field0;
        public readonly IInvoiceRowHandlerService field1;
    }


    public interface IInvoiceRowItemGroupService
    {
    }


    public class InvoiceRowItemGroupService
        : IInvoiceRowItemGroupService
    {
        public InvoiceRowItemGroupService(
            IInvoiceHelperService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IInvoiceHelperService field0;
    }


    public interface IInvoiceRowManagerService
    {
    }


    public class InvoiceRowManagerService
        : IInvoiceRowManagerService
    {
        public InvoiceRowManagerService(
            IItemRepository arg0,
            IHourRepository arg1,
            ITaxService arg2,
            IInvoiceRowGroupHandlerService arg3,
            IInvoiceRowHandlerService arg4,
            ITaskRepository arg5,
            IInvoiceRowService arg6,
            IInvoiceConfigService arg7,
            IInvoiceItemHandlerService arg8,
            IInvoiceHourHandlerService arg9,
            IInvoiceHelperService arg10,
            IInvoiceStatusRepository arg11,
            IReimbursedHourRepository arg12,
            IInvoiceRepository arg13,
            IReimbursedItemRepository arg14
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
        }

        public readonly IItemRepository field0;
        public readonly IHourRepository field1;
        public readonly ITaxService field2;
        public readonly IInvoiceRowGroupHandlerService field3;
        public readonly IInvoiceRowHandlerService field4;
        public readonly ITaskRepository field5;
        public readonly IInvoiceRowService field6;
        public readonly IInvoiceConfigService field7;
        public readonly IInvoiceItemHandlerService field8;
        public readonly IInvoiceHourHandlerService field9;
        public readonly IInvoiceHelperService field10;
        public readonly IInvoiceStatusRepository field11;
        public readonly IReimbursedHourRepository field12;
        public readonly IInvoiceRepository field13;
        public readonly IReimbursedItemRepository field14;
    }


    public interface IInvoiceRowService
        : IEntityService<InvoiceRow>
    {
    }


    public class InvoiceRowService : OrganizationEntityService<InvoiceRow, IInvoiceRowRepository, User, IPsaContext>
        , IInvoiceRowService
    {
        public InvoiceRowService(
            IContextService<IPsaContext> arg0,
            IInvoiceRowRepository arg1,
            IValidator<InvoiceRow> arg2,
            IAuthorization<IPsaContext, InvoiceRow> arg3,
            ITaxRepository arg4,
            IHourRepository arg5,
            ITaskRepository arg6,
            IItemRepository arg7,
            IItemSalesAccountRepository arg8,
            ICostCenterRepository arg9,
            ISalesAccountRepository arg10,
            IInvoiceRepository arg11,
            IInvoiceConfigRepository arg12, IAuditTrail<InvoiceRow> arg13
        ) : base()
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
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceRowRepository field1;
        public readonly IValidator<InvoiceRow> field2;
        public readonly IAuthorization<IPsaContext, InvoiceRow> field3;
        public readonly ITaxRepository field4;
        public readonly IHourRepository field5;
        public readonly ITaskRepository field6;
        public readonly IItemRepository field7;
        public readonly IItemSalesAccountRepository field8;
        public readonly ICostCenterRepository field9;
        public readonly ISalesAccountRepository field10;
        public readonly IInvoiceRepository field11;
        public readonly IInvoiceConfigRepository field12;
        public readonly IAuditTrail<InvoiceRow> field13;
    }


    public interface IInvoiceService
        : IEntityService<Invoice>
    {
    }


    public class InvoiceService : OrganizationEntityService<Invoice, IInvoiceRepository, User, IPsaContext>
        , IInvoiceService
    {
        public InvoiceService(
            IContextService<IPsaContext> arg0,
            IValidator<Invoice> arg1,
            IAuthorization<IPsaContext, Invoice> arg2,
            IInvoiceRepository arg3,
            ICountryRepository arg4,
            IAccountRepository arg5,
            IBusinessUnitService arg6,
            IAddressRepository arg7,
            ICompanyRepository arg8,
            IInvoiceStatusService arg9,
            ICurrencyService arg10,
            IReferenceNumberService arg11,
            IInvoiceBankAccountRepository arg12,
            IRevenueRecognitionService arg13,
            IInvoiceConfigService arg14,
            IOrganizationRepository arg15,
            ICaseRepository arg16,
            IInvoiceRowManagerService arg17,
            IInvoiceItemHandlerService arg18,
            IInvoiceCreationHandlerService arg19,
            IInvoiceDeletionHandlerService arg20,
            IInvoiceHourHandlerService arg21,
            IInvoiceTaskHandlerService arg22,
            IInvoiceHelperService arg23,
            IContactRepository arg24,
            IInvoiceTemplateConfigService arg25,
            IInvoiceCaseRepository arg26,
            IUserRepository arg27,
            IInvoiceHtmlService arg28,
            IInvoiceTemplateService arg29,
            ICaseBillingAccountService arg30,
            IBusinessUnitRepository arg31, IAuditTrail<InvoiceCase> arg32, IAuditTrail<Invoice> arg33,
            IInvoiceRowService arg34,
            IContactService arg35,
            IOrganizationCompanyRepository arg36,
            IAccountCountrySettingsRepository arg37,
            IInvoiceBillingAccountGrouperService arg38
        ) : base()
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
            field17 = arg17;
            field18 = arg18;
            field19 = arg19;
            field20 = arg20;
            field21 = arg21;
            field22 = arg22;
            field23 = arg23;
            field24 = arg24;
            field25 = arg25;
            field26 = arg26;
            field27 = arg27;
            field28 = arg28;
            field29 = arg29;
            field30 = arg30;
            field31 = arg31;
            field32 = arg32;
            field33 = arg33;
            field34 = arg34;
            field35 = arg35;
            field36 = arg36;
            field37 = arg37;
            field38 = arg38;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IValidator<Invoice> field1;
        public readonly IAuthorization<IPsaContext, Invoice> field2;
        public readonly IInvoiceRepository field3;
        public readonly ICountryRepository field4;
        public readonly IAccountRepository field5;
        public readonly IBusinessUnitService field6;
        public readonly IAddressRepository field7;
        public readonly ICompanyRepository field8;
        public readonly IInvoiceStatusService field9;
        public readonly ICurrencyService field10;
        public readonly IReferenceNumberService field11;
        public readonly IInvoiceBankAccountRepository field12;
        public readonly IRevenueRecognitionService field13;
        public readonly IInvoiceConfigService field14;
        public readonly IOrganizationRepository field15;
        public readonly ICaseRepository field16;
        public readonly IInvoiceRowManagerService field17;
        public readonly IInvoiceItemHandlerService field18;
        public readonly IInvoiceCreationHandlerService field19;
        public readonly IInvoiceDeletionHandlerService field20;
        public readonly IInvoiceHourHandlerService field21;
        public readonly IInvoiceTaskHandlerService field22;
        public readonly IInvoiceHelperService field23;
        public readonly IContactRepository field24;
        public readonly IInvoiceTemplateConfigService field25;
        public readonly IInvoiceCaseRepository field26;
        public readonly IUserRepository field27;
        public readonly IInvoiceHtmlService field28;
        public readonly IInvoiceTemplateService field29;
        public readonly ICaseBillingAccountService field30;
        public readonly IBusinessUnitRepository field31;
        public readonly IAuditTrail<InvoiceCase> field32;
        public readonly IAuditTrail<Invoice> field33;
        public readonly IInvoiceRowService field34;
        public readonly IContactService field35;
        public readonly IOrganizationCompanyRepository field36;
        public readonly IAccountCountrySettingsRepository field37;
        public readonly IInvoiceBillingAccountGrouperService field38;
    }


    public interface IInvoiceStatusService
        : IEntityService<InvoiceStatus>
    {
    }


    public class InvoiceStatusService :
        OrganizationEntityService<InvoiceStatus, IInvoiceStatusRepository, User, IPsaContext>
        , IInvoiceStatusService
    {
        public InvoiceStatusService(
            IContextService<IPsaContext> arg0,
            IInvoiceStatusRepository arg1,
            IValidator<InvoiceStatus> arg2,
            IAuthorization<IPsaContext, InvoiceStatus> arg3,
            IInvoiceStatusHistoryRepository arg4,
            IUserRepository arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceStatusRepository field1;
        public readonly IValidator<InvoiceStatus> field2;
        public readonly IAuthorization<IPsaContext, InvoiceStatus> field3;
        public readonly IInvoiceStatusHistoryRepository field4;
        public readonly IUserRepository field5;
    }


    public interface IInvoiceTaskHandlerService
    {
    }


    public class InvoiceTaskHandlerService : InvoiceHandlerServiceBase
        , IInvoiceTaskHandlerService
    {
        public InvoiceTaskHandlerService(
            IContextService<IPsaContext> arg0,
            ITaskRepository arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITaskRepository field1;
    }


    public interface IInvoiceTaxBreakdownService
    {
    }


    public class InvoiceTaxBreakdownService
        : IInvoiceTaxBreakdownService
    {
        public InvoiceTaxBreakdownService(
            IInvoiceService arg0,
            IInvoiceRowService arg1,
            IInvoiceConfigService arg2,
            ITaxTotalService arg3,
            IInvoiceHelperService arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IInvoiceService field0;
        public readonly IInvoiceRowService field1;
        public readonly IInvoiceConfigService field2;
        public readonly ITaxTotalService field3;
        public readonly IInvoiceHelperService field4;
    }


    public interface IInvoiceTemplateConfigService
        : IEntityService<InvoiceTemplateConfig>
    {
    }


    public class InvoiceTemplateConfigService : OrganizationEntityService<InvoiceTemplateConfig,
            IInvoiceTemplateConfigRepository, User, IPsaContext>
        , IInvoiceTemplateConfigService
    {
        public InvoiceTemplateConfigService(
            IContextService<IPsaContext> arg0,
            IInvoiceTemplateConfigRepository arg1,
            IValidator<InvoiceTemplateConfig> arg2,
            IAuthorization<IPsaContext, InvoiceTemplateConfig> arg3,
            ICountryService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceTemplateConfigRepository field1;
        public readonly IValidator<InvoiceTemplateConfig> field2;
        public readonly IAuthorization<IPsaContext, InvoiceTemplateConfig> field3;
        public readonly ICountryService field4;
    }


    public interface IInvoiceTemplateService
        : IEntityService<InvoiceTemplate>
    {
    }


    public class InvoiceTemplateService :
        OrganizationEntityService<InvoiceTemplate, IInvoiceTemplateRepository, User, IPsaContext>
        , IInvoiceTemplateService
    {
        public InvoiceTemplateService(
            IContextService<IPsaContext> arg0,
            IInvoiceTemplateRepository arg1,
            IValidator<InvoiceTemplate> arg2,
            IAuthorization<IPsaContext, InvoiceTemplate> arg3,
            ICaseService arg4,
            IInvoiceConfigService arg5,
            IInvoiceHtmlRepository arg6,
            IAccountRepository arg7,
            IBusinessUnitService arg8,
            IInvoiceTemplateConfigService arg9
        ) : base()
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

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceTemplateRepository field1;
        public readonly IValidator<InvoiceTemplate> field2;
        public readonly IAuthorization<IPsaContext, InvoiceTemplate> field3;
        public readonly ICaseService field4;
        public readonly IInvoiceConfigService field5;
        public readonly IInvoiceHtmlRepository field6;
        public readonly IAccountRepository field7;
        public readonly IBusinessUnitService field8;
        public readonly IInvoiceTemplateConfigService field9;
    }


    public interface IScheduledWorkTaskService
        : IBackgroundTaskService
    {
    }


    public class ScheduledWorkTaskService : BackgroundTaskService
        , IScheduledWorkTaskService
    {
        public ScheduledWorkTaskService(
            IContextService<IPsaContext> arg0,
            IBackgroundTaskRepository arg1,
            IScheduledWorkTaskValidator arg2,
            IScheduledWorkTaskAuthorization arg3,
            IBackgroundExecutorService arg4,
            IBackgroundTaskRunRepository arg5
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5)
        {
        }
    }


    public interface IItemFileService
        : IEntityService<ItemFile>
    {
    }


    public class ItemFileService : OrganizationEntityService<ItemFile, IItemFileRepository, User, IPsaContext>
        , IItemFileService
    {
        public ItemFileService(
            IContextService<IPsaContext> arg0,
            IValidator<ItemFile> arg1,
            IAuthorization<IPsaContext, ItemFile> arg2,
            IItemFileRepository arg3,
            IFileService arg4,
            IItemRepository arg5,
            ITravelReimbursementRepository arg6,
            ITravelReimbursementStatusRepository arg7
        ) : base()
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

        public readonly IContextService<IPsaContext> field0;
        public readonly IValidator<ItemFile> field1;
        public readonly IAuthorization<IPsaContext, ItemFile> field2;
        public readonly IItemFileRepository field3;
        public readonly IFileService field4;
        public readonly IItemRepository field5;
        public readonly ITravelReimbursementRepository field6;
        public readonly ITravelReimbursementStatusRepository field7;
    }


    public interface IItemSalesAccountService
        : IEntityService<ItemSalesAccount>
    {
    }


    public class ItemSalesAccountService :
        OrganizationEntityService<ItemSalesAccount, IItemSalesAccountRepository, User, IPsaContext>
        , IItemSalesAccountService
    {
        public ItemSalesAccountService(
            IContextService<IPsaContext> arg0,
            IItemSalesAccountRepository arg1,
            IValidator<ItemSalesAccount> arg2,
            IAuthorization<IPsaContext, ItemSalesAccount> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IItemSalesAccountRepository field1;
        public readonly IValidator<ItemSalesAccount> field2;
        public readonly IAuthorization<IPsaContext, ItemSalesAccount> field3;
    }


    public interface IItemService
        : IEntityService<Item>
    {
    }


    public interface ILeadSourceService
        : IEntityService<LeadSource>
    {
    }


    public interface ILinkService
        : IEntityService<Link>
    {
    }


    public class LinkService : OrganizationEntityService<Link, ILinkRepository, User, IPsaContext>
        , ILinkService
    {
        public LinkService(
            IContextService<IPsaContext> arg0,
            ILinkRepository arg1,
            IValidator<Link> arg2,
            IAuthorization<IPsaContext, Link> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ILinkRepository field1;
        public readonly IValidator<Link> field2;
        public readonly IAuthorization<IPsaContext, Link> field3;
    }


    public interface ILogoFileService
    {
    }


    public class LogoFileService
        : ILogoFileService
    {
        public LogoFileService(
            IFileService arg0,
            IPsaOrganizationService arg1,
            IBusinessUnitService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IFileService field0;
        public readonly IPsaOrganizationService field1;
        public readonly IBusinessUnitService field2;
    }


    public interface INavigationHistoryService
        : IEntityService<NavigationHistory>
    {
    }


    public class NavigationHistoryService : OrganizationEntityService<NavigationHistory, INavigationHistoryRepository,
            User, IPsaContext>
        , INavigationHistoryService
    {
        public NavigationHistoryService(
            IContextService<IPsaContext> arg0,
            INavigationHistoryRepository arg1,
            IValidator<NavigationHistory> arg2,
            IAuthorization<IPsaContext, NavigationHistory> arg3,
            IReportService arg4,
            IGuidService arg5
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly INavigationHistoryRepository field1;
        public readonly IValidator<NavigationHistory> field2;
        public readonly IAuthorization<IPsaContext, NavigationHistory> field3;
        public readonly IReportService field4;
        public readonly IGuidService field5;
    }


    public interface INoteProviderService<T>
    {
    }


    public interface IOrganizationCompanyWorkTypeService
        : IEntityService<OrganizationCompanyWorkType>
    {
    }


    public class OrganizationCompanyWorkTypeService : OrganizationSettingsService<OrganizationCompanyWorkType,
            IOrganizationCompanyWorkTypeRepository>
        , IOrganizationCompanyWorkTypeService
    {
        public OrganizationCompanyWorkTypeService(
            IContextService<IPsaContext> arg0,
            IOrganizationCompanyWorkTypeRepository arg1,
            IValidator<OrganizationCompanyWorkType> arg2,
            IAuthorization<IPsaContext, OrganizationCompanyWorkType> arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IOrganizationCompanyWorkTypeRepository field1;
        public readonly IValidator<OrganizationCompanyWorkType> field2;
        public readonly IAuthorization<IPsaContext, OrganizationCompanyWorkType> field3;
    }


    public interface IOrganizationDetailsService
    {
    }


    public class OrganizationDetailsService
        : IOrganizationDetailsService
    {
        public OrganizationDetailsService(
            IContextService<IPsaContext> arg0,
            IPsaOrganizationService arg1,
            IOrganizationCompanyService arg2,
            IAccountService arg3,
            ICompanyService arg4,
            IAddressService arg5
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
        public readonly IPsaOrganizationService field1;
        public readonly IOrganizationCompanyService field2;
        public readonly IAccountService field3;
        public readonly ICompanyService field4;
        public readonly IAddressService field5;
    }


    public interface IOrganizationSettingsService
    {
    }


    public partial class OrganizationSettingsService
        : IOrganizationSettingsService
    {
        public OrganizationSettingsService(
            IPsaContextService arg0,
            ISettingsRepository arg1,
            IPsaOrganizationService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IPsaContextService field0;
        public readonly ISettingsRepository field1;
        public readonly IPsaOrganizationService field2;
    }


    public interface IOrganizationTrustedService
    {
    }

    public interface IPsaLoginService
    {
    }

    public class PsaLoginService : IPsaLoginService
    {
        private readonly IMasterUserRepository _MasterUserRepository;
        private readonly IMasterOrganizationRepository _MasterOrganizationRepository;

        public PsaLoginService(IMasterUserRepository masterUserRepository, IMasterOrganizationRepository masterOrganizationRepository)
        {
            _MasterUserRepository = masterUserRepository;
            _MasterOrganizationRepository = masterOrganizationRepository;
        }
    }

    public class OrganizationTrustedService
        : IOrganizationTrustedService
    {
        public OrganizationTrustedService(
            IContextService<IPsaContext> arg0,
            IOrganizationRepository arg1,
            IUserRepository arg2,
            IPsaLoginService arg3,
            IOrganizationAddonService arg4,
            ICalendarSyncUserCalendarRepository arg5,
            ICurrencyRepository arg6,
            IOrganizationCompanyRepository arg7,
            IMasterUserRepository arg8,
            IMasterOrganizationRepository arg9,
            IPartnerRepository arg10,
            IExternallyOwnedOrganizationService arg11
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
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IOrganizationRepository field1;
        public readonly IUserRepository field2;
        public readonly IPsaLoginService field3;
        public readonly IOrganizationAddonService field4;
        public readonly ICalendarSyncUserCalendarRepository field5;
        public readonly ICurrencyRepository field6;
        public readonly IOrganizationCompanyRepository field7;
        public readonly IMasterUserRepository field8;
        public readonly IMasterOrganizationRepository field9;
        public readonly IPartnerRepository field10;
        public readonly IExternallyOwnedOrganizationService field11;
    }


    public interface IOrganizationWorkweekService
    {
    }


    public class OrganizationWorkweekService
        : IOrganizationWorkweekService
    {
        public OrganizationWorkweekService(
            IWorkweekRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IWorkweekRepository field0;
    }


    public interface IOvertimePriceService
        : IEntityService<OverTimePrice>
    {
    }


    public class OvertimePriceService :
        OrganizationEntityService<OverTimePrice, IOvertimePriceRepository, User, IPsaContext>
        , IOvertimePriceService
    {
        public OvertimePriceService(
            IContextService<IPsaContext> arg0,
            IOvertimePriceRepository arg1,
            IValidator<OverTimePrice> arg2,
            IAuthorization<IPsaContext, OverTimePrice> arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IOvertimePriceRepository field1;
        public readonly IValidator<OverTimePrice> field2;
        public readonly IAuthorization<IPsaContext, OverTimePrice> field3;
    }


    public interface IOvertimeService
        : IEntityService<OverTime>
    {
    }


    public class OvertimeService : OrganizationEntityService<OverTime, IOvertimeRepository, User, IPsaContext>
        , IOvertimeService
    {
        public OvertimeService(
            IContextService<IPsaContext> arg0,
            IOvertimeRepository arg1,
            IValidator<OverTime> arg2,
            IAuthorization<IPsaContext, OverTime> arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IOvertimeRepository field1;
        public readonly IValidator<OverTime> field2;
        public readonly IAuthorization<IPsaContext, OverTime> field3;
    }


    public interface IPricelistVersionService
        : IEntityService<PricelistVersion>
    {
    }


    public class PricelistVersionService :
        OrganizationEntityService<PricelistVersion, IPricelistVersionRepository, User, IPsaContext>
        , IPricelistVersionService
    {
        public PricelistVersionService(
            IContextService<IPsaContext> arg0,
            IPricelistVersionRepository arg1,
            IValidator<PricelistVersion> arg2,
            IAuthorization<IPsaContext, PricelistVersion> arg3,
            IWorkPriceService arg4,
            IProductPriceService arg5,
            IOvertimePriceService arg6,
            IPricelistPriceService arg7,
            IPricelistVersionRuleService arg8,
            IPricelistRepository arg9,
            ICaseRepository arg10,
            IItemService arg11
        ) : base(arg0)
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
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IPricelistVersionRepository field1;
        public readonly IValidator<PricelistVersion> field2;
        public readonly IAuthorization<IPsaContext, PricelistVersion> field3;
        public readonly IWorkPriceService field4;
        public readonly IProductPriceService field5;
        public readonly IOvertimePriceService field6;
        public readonly IPricelistPriceService field7;
        public readonly IPricelistVersionRuleService field8;
        public readonly IPricelistRepository field9;
        public readonly ICaseRepository field10;
        public readonly IItemService field11;
    }


    public interface IProductCategoryService
        : IEntityService<ProductCategory>
    {
    }


    public class ProductCategoryService :
        OrganizationEntityService<ProductCategory, IProductCategoryRepository, User, IPsaContext>
        , IProductCategoryService
    {
        public ProductCategoryService(
            IContextService<IPsaContext> arg0,
            IProductCategoryRepository arg1,
            IValidator<ProductCategory> arg2,
            IAuthorization<IPsaContext, ProductCategory> arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IProductCategoryRepository field1;
        public readonly IValidator<ProductCategory> field2;
        public readonly IAuthorization<IPsaContext, ProductCategory> field3;
    }


    public interface IProductPriceService
        : IEntityService<ProductPrice>
    {
    }


    public class ProductPriceService :
        OrganizationEntityService<ProductPrice, IProductPriceRepository, User, IPsaContext>
        , IProductPriceService
    {
        public ProductPriceService(
            IContextService<IPsaContext> arg0,
            IValidator<ProductPrice> arg1,
            IAuthorization<IPsaContext, ProductPrice> arg2,
            IProductPriceRepository arg3,
            IItemService arg4,
            IProductService arg5
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IValidator<ProductPrice> field1;
        public readonly IAuthorization<IPsaContext, ProductPrice> field2;
        public readonly IProductPriceRepository field3;
        public readonly IItemService field4;
        public readonly IProductService field5;
    }


    public interface IProductService
        : IEntityService<Product>
    {
    }


    public class ProductService : OrganizationEntityService<Product, IProductRepository, User, IPsaContext>
        , IProductService
    {
        public ProductService(
            IContextService<IPsaContext> arg0,
            IProductRepository arg1,
            IValidator<Product> arg2,
            IAuthorization<IPsaContext, Product> arg3,
            ITaxRepository arg4,
            IItemRepository arg5,
            ICurrencyRepository arg6
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IProductRepository field1;
        public readonly IValidator<Product> field2;
        public readonly IAuthorization<IPsaContext, Product> field3;
        public readonly ITaxRepository field4;
        public readonly IItemRepository field5;
        public readonly ICurrencyRepository field6;
    }


    public interface IProfileDashboardService
        : IEntityService<ProfileDashboard>
    {
    }


    public class ProfileDashboardService :
        OrganizationEntityService<ProfileDashboard, IProfileDashboardRepository, User, IPsaContext>
        , IProfileDashboardService
    {
        public ProfileDashboardService(
            IContextService<IPsaContext> arg0,
            IProfileDashboardRepository arg1,
            IValidator<ProfileDashboard> arg2,
            IAuthorization<IPsaContext, ProfileDashboard> arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IProfileDashboardRepository field1;
        public readonly IValidator<ProfileDashboard> field2;
        public readonly IAuthorization<IPsaContext, ProfileDashboard> field3;
    }


    public interface IProfileService
        : IEntityService<Profile>
    {
    }


    public class ProfileService : OrganizationEntityService<Profile, IProfileRepository, User, IPsaContext>
        , IProfileService
    {
        public ProfileService(
            IContextService<IPsaContext> arg0,
            IProfileRepository arg1,
            IValidator<Profile> arg2,
            IAuthorization<IPsaContext, Profile> arg3,
            IProfileRightRepository arg4,
            IAccessRightsHelper arg5,
            IAccessRightService arg6,
            IInvoiceStatusRepository arg7
        ) : base(arg0)
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

        public readonly IContextService<IPsaContext> field0;
        public readonly IProfileRepository field1;
        public readonly IValidator<Profile> field2;
        public readonly IAuthorization<IPsaContext, Profile> field3;
        public readonly IProfileRightRepository field4;
        public readonly IAccessRightsHelper field5;
        public readonly IAccessRightService field6;
        public readonly IInvoiceStatusRepository field7;
    }


    public interface IProposalTaxBreakdownService
    {
    }


    public class ProposalTaxBreakdownService
        : IProposalTaxBreakdownService
    {
        public ProposalTaxBreakdownService(
            IContextService<IPsaContext> arg0,
            IOfferService arg1,
            ITaxService arg2,
            ICaseService arg6
        )
        {
        }
    }


    public interface IQuickSearchService
    {
    }


    public class QuickSearchService
        : IQuickSearchService
    {
        public QuickSearchService(
            IContextService<IPsaContext> arg0,
            IQuickSearchRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IQuickSearchRepository field1;
    }


    public interface IRecurringItemService
        : IEntityService<RecurringItem>
    {
    }


    public interface IReimbursedHourService
        : IEntityService<ReimbursedHour>
    {
    }


    public interface IReimbursedItemService
        : IEntityService<ReimbursedItem>
    {
    }


    public interface IReportService
        : IEntityService<Report>
    {
    }

    public class ReportService : OrganizationEntityService<Report, IReportRepository, User, IPsaContext>
        , IReportService
    {
        public ReportService(
            IContextService<IPsaContext> arg0,
            IReportRepository arg1,
            IValidator<Report> arg2,
            IAuthorization<IPsaContext, Report> arg3,
            ISettingsService arg4,
            IReportFactoryService<IPsaContext> arg5,
            IProfileReportRepository arg6
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IReportRepository field1;
        public readonly IValidator<Report> field2;
        public readonly IAuthorization<IPsaContext, Report> field3;
        public readonly ISettingsService field4;
        public readonly IReportFactoryService<IPsaContext> field5;
        public readonly IProfileReportRepository field6;
    }


    public interface IResourceAllocationService
        : IEntityService<ResourceAllocation>
    {
    }


    public class ResourceAllocationService : OrganizationEntityService<ResourceAllocation, IResourceAllocationRepository
            , User, IPsaContext>
        , IResourceAllocationService
    {
        public ResourceAllocationService(
            IContextService<IPsaContext> arg0,
            IResourceAllocationRepository arg1,
            IValidator<ResourceAllocation> arg2,
            IAuthorization<IPsaContext, ResourceAllocation> arg3,
            ITaskRepository arg4,
            IEmploymentService arg5,
            IOrganizationWorkweekService arg6,
            IWorkingDayExceptionService arg7,
            IGuidService arg8,
            IHourRepository arg9,
            IOrganizationCompanyRepository arg10,
            ICaseRepository arg11,
            IUserCostPerCaseService arg12,
            ICurrencyRepository arg13
        ) : base(arg0)
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
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IResourceAllocationRepository field1;
        public readonly IValidator<ResourceAllocation> field2;
        public readonly IAuthorization<IPsaContext, ResourceAllocation> field3;
        public readonly ITaskRepository field4;
        public readonly IEmploymentService field5;
        public readonly IOrganizationWorkweekService field6;
        public readonly IWorkingDayExceptionService field7;
        public readonly IGuidService field8;
        public readonly IHourRepository field9;
        public readonly IOrganizationCompanyRepository field10;
        public readonly ICaseRepository field11;
        public readonly IUserCostPerCaseService field12;
        public readonly ICurrencyRepository field13;
    }


    public interface IResourceService
        : IEntityService<Resource>
    {
    }


    public class ResourceService : OrganizationEntityService<Resource, IResourceRepository, User, IPsaContext>
        , IResourceService
    {
        public ResourceService(
            IContextService<IPsaContext> arg0,
            IResourceRepository arg1,
            IValidator<Resource> arg2,
            IAuthorization<IPsaContext, Resource> arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IResourceRepository field1;
        public readonly IValidator<Resource> field2;
        public readonly IAuthorization<IPsaContext, Resource> field3;
    }


    public interface ISalesAccountService
        : IEntityService<SalesAccount>
    {
    }


    public class SalesAccountService :
        OrganizationEntityService<SalesAccount, ISalesAccountRepository, User, IPsaContext>
        , ISalesAccountService
    {
        public SalesAccountService(
            IContextService<IPsaContext> arg0,
            ISalesAccountRepository arg1,
            IValidator<SalesAccount> arg2,
            IAuthorization<IPsaContext, SalesAccount> arg3,
            IItemRepository arg4,
            IProductRepository arg5,
            IWorkTypeRepository arg6
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ISalesAccountRepository field1;
        public readonly IValidator<SalesAccount> field2;
        public readonly IAuthorization<IPsaContext, SalesAccount> field3;
        public readonly IItemRepository field4;
        public readonly IProductRepository field5;
        public readonly IWorkTypeRepository field6;
    }


    public interface ISalesProcessService
        : IEntityService<SalesProcess>
    {
    }


    public interface ISalesStatusService
        : IEntityService<SalesStatus>
    {
    }


    public class SalesStatusService : OrganizationEntityService<SalesStatus, ISalesStatusRepository, User, IPsaContext>
        , ISalesStatusService
    {
        public SalesStatusService(
            IContextService<IPsaContext> arg0,
            ISalesStatusRepository arg1,
            IValidator<SalesStatus> arg2,
            IAuthorization<IPsaContext, SalesStatus> arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ISalesStatusRepository field1;
        public readonly IValidator<SalesStatus> field2;
        public readonly IAuthorization<IPsaContext, SalesStatus> field3;
    }


    public interface ISearchCriteriaService
        : IEntityService<Entities.SearchCriteria>
    {
    }


    public class SearchCriteriaService :
        OrganizationEntityService<Entities.SearchCriteria, ISearchCriteriaRepository, User, IPsaContext>
        , ISearchCriteriaService
    {
        public SearchCriteriaService(
            IContextService<IPsaContext> arg0,
            ISearchCriteriaRepository arg1,
            IValidator<Entities.SearchCriteria> arg2,
            IAuthorization<IPsaContext, Entities.SearchCriteria> arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ISearchCriteriaRepository field1;
        public readonly IValidator<Entities.SearchCriteria> field2;
        public readonly IAuthorization<IPsaContext, Entities.SearchCriteria> field3;
    }


    public interface ISearchService
        : IEntityService<Entities.Search>
    {
    }


    public class SearchService : OrganizationEntityService<Entities.Search, ISearchRepository, User, IPsaContext>
        , ISearchService
    {
        public SearchService(
            IContextService<IPsaContext> arg0,
            ISearchRepository arg1,
            IValidator<Entities.Search> arg2,
            IAuthorization<IPsaContext, Entities.Search> arg3,
            ISearchCriteriaService arg4
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ISearchRepository field1;
        public readonly IValidator<Entities.Search> field2;
        public readonly IAuthorization<IPsaContext, Entities.Search> field3;
        public readonly ISearchCriteriaService field4;
    }


    public interface ISettingsService
        : IEntityService<Settings>
    {
    }


    public class SettingsService : OrganizationEntityService<Settings, ISettingsRepository, User, IPsaContext>
        , ISettingsService
    {
        public SettingsService(
            IContextService<IPsaContext> arg0,
            ISettingsRepository arg1,
            IValidator<Settings> arg2,
            IAuthorization<IPsaContext, Settings> arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ISettingsRepository field1;
        public readonly IValidator<Settings> field2;
        public readonly IAuthorization<IPsaContext, Settings> field3;
    }


    public interface ITagEntityService<T>
        : IEntityService<T>
    {
    }


    public interface ITagService
        : IEntityService<Tag>
    {
    }


    public class TagService : OrganizationEntityService<Tag, ITagRepository, User, IPsaContext>
        , ITagService
    {
        public TagService(
            IContextService<IPsaContext> arg0,
            ITagRepository arg1,
            IValidator<Tag> arg2,
            IAuthorization<IPsaContext, Tag> arg3,
            ICaseTagService arg4,
            IContactTagService arg5,
            IFileTagService arg6,
            IUserTagService arg7,
            IAuthorization<IPsaContext, File> arg8,
            IAuthorization<IPsaContext, Case> arg9,
            IAuthorization<IPsaContext, User> arg10,
            IAuthorization<IPsaContext, Contact> arg11,
            IFileRepository arg12,
            ICaseRepository arg13,
            IUserRepository arg14,
            IContactRepository arg15
        ) : base(arg0)
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

        public readonly IContextService<IPsaContext> field0;
        public readonly ITagRepository field1;
        public readonly IValidator<Tag> field2;
        public readonly IAuthorization<IPsaContext, Tag> field3;
        public readonly ICaseTagService field4;
        public readonly IContactTagService field5;
        public readonly IFileTagService field6;
        public readonly IUserTagService field7;
        public readonly IAuthorization<IPsaContext, File> field8;
        public readonly IAuthorization<IPsaContext, Case> field9;
        public readonly IAuthorization<IPsaContext, User> field10;
        public readonly IAuthorization<IPsaContext, Contact> field11;
        public readonly IFileRepository field12;
        public readonly ICaseRepository field13;
        public readonly IUserRepository field14;
        public readonly IContactRepository field15;
    }


    public interface ITaskMemberService
        : IEntityService<TaskMember>
    {
    }


    public class TaskMemberService : OrganizationEntityService<TaskMember, ITaskMemberRepository, User, IPsaContext>
        , ITaskMemberService
    {
        public TaskMemberService(
            IContextService<IPsaContext> arg0,
            ITaskMemberRepository arg1,
            IValidator<TaskMember> arg2,
            IAuthorization<IPsaContext, TaskMember> arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITaskMemberRepository field1;
        public readonly IValidator<TaskMember> field2;
        public readonly IAuthorization<IPsaContext, TaskMember> field3;
    }


    public interface ITaskService
        : IEntityService<Task>
    {
    }


    public class TaskService : OrganizationEntityService<Task, ITaskRepository, User, IPsaContext>
        , ITaskService
    {
        public TaskService(
            IContextService<IPsaContext> arg0,
            ITaskRepository arg1,
            IValidator<Task> arg2,
            IAuthorization<IPsaContext, Task> arg3,
            ICaseNoteService arg4,
            IResourceAllocationService arg5,
            IPricelistService arg6,
            ICaseMemberService arg7,
            ITaskMemberService arg8,
            IUserRepository arg9,
            ITaxService arg10,
            ICaseRepository arg11, IAuditTrail<Task> arg12, IAuditTrail<Case> arg13, IAuditTrail<CaseMember> arg14
            //IAuditTrailEntryRepository arg15,
            //ITreeTaskService arg16
        ) : base()
        {
        }
    }


    public interface ITaskStatusCommentService
        : IEntityService<TaskStatusComment>
    {
    }


    public class TaskStatusCommentService : OrganizationEntityService<TaskStatusComment, ITaskStatusCommentRepository,
            User, IPsaContext>
        , ITaskStatusCommentService
    {
        public TaskStatusCommentService(
            IContextService<IPsaContext> arg0,
            ITaskStatusCommentRepository arg1,
            IValidator<TaskStatusComment> arg2,
            IAuthorization<IPsaContext, TaskStatusComment> arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITaskStatusCommentRepository field1;
        public readonly IValidator<TaskStatusComment> field2;
        public readonly IAuthorization<IPsaContext, TaskStatusComment> field3;
    }


    public interface ITaskStatusService
        : IEntityService<TaskStatus>
    {
    }


    public class TaskStatusService : OrganizationEntityService<TaskStatus, ITaskStatusRepository, User, IPsaContext>
        , ITaskStatusService
    {
        public TaskStatusService(
            IContextService<IPsaContext> arg0,
            ITaskStatusRepository arg1,
            IValidator<TaskStatus> arg2,
            IAuthorization<IPsaContext, TaskStatus> arg3,
            ITaskStatusCommentService arg4
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITaskStatusRepository field1;
        public readonly IValidator<TaskStatus> field2;
        public readonly IAuthorization<IPsaContext, TaskStatus> field3;
        public readonly ITaskStatusCommentService field4;
    }


    public interface ITaskStatusTypeService
        : IEntityService<TaskStatusType>
    {
    }


    public class TaskStatusTypeService :
        OrganizationEntityService<TaskStatusType, ITaskStatusTypeRepository, User, IPsaContext>
        , ITaskStatusTypeService
    {
        public TaskStatusTypeService(
            IContextService<IPsaContext> arg0,
            ITaskStatusTypeRepository arg1,
            IValidator<TaskStatusType> arg2,
            IAuthorization<IPsaContext, TaskStatusType> arg3,
            ITaskStatusService arg4,
            ITaskStatusRepository arg5,
            ITaskRepository arg6
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITaskStatusTypeRepository field1;
        public readonly IValidator<TaskStatusType> field2;
        public readonly IAuthorization<IPsaContext, TaskStatusType> field3;
        public readonly ITaskStatusService field4;
        public readonly ITaskStatusRepository field5;
        public readonly ITaskRepository field6;
    }


    public interface ITaxTotalService
    {
    }


    public class TaxTotalService
        : ITaxTotalService
    {
        public TaxTotalService(
        )
        {
        }
    }


    public interface ITemporaryHourService
        : IEntityService<TemporaryHour>
    {
    }


    public class TemporaryHourService :
        OrganizationEntityService<TemporaryHour, ITemporaryHourRepository, User, IPsaContext>
        , ITemporaryHourService
    {
        public TemporaryHourService(
            IContextService<IPsaContext> arg0,
            ITemporaryHourRepository arg1,
            IValidator<TemporaryHour> arg2,
            IAuthorization<IPsaContext, TemporaryHour> arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITemporaryHourRepository field1;
        public readonly IValidator<TemporaryHour> field2;
        public readonly IAuthorization<IPsaContext, TemporaryHour> field3;
    }


    public interface ITemporaryItemService
        : IEntityService<TemporaryItem>
    {
    }


    public class TemporaryItemService :
        OrganizationEntityService<TemporaryItem, ITemporaryItemRepository, User, IPsaContext>
        , ITemporaryItemService
    {
        public TemporaryItemService(
            IContextService<IPsaContext> arg0,
            ITemporaryItemRepository arg1,
            IValidator<TemporaryItem> arg2,
            IAuthorization<IPsaContext, TemporaryItem> arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITemporaryItemRepository field1;
        public readonly IValidator<TemporaryItem> field2;
        public readonly IAuthorization<IPsaContext, TemporaryItem> field3;
    }


    public interface ITermsOfServiceApprovalService
        : IEntityService<TermsOfServiceApproval>
    {
    }


    public interface ITermsOfServiceEmailService
    {
    }


    public interface ITimeEntryService
        : IEntityService<TimeEntry>
    {
    }


    public class TimeEntryService : OrganizationEntityService<TimeEntry, ITimeEntryRepository, User, IPsaContext>
        , ITimeEntryService
    {
        public TimeEntryService(
            IContextService<IPsaContext> arg0,
            ITimeEntryRepository arg1,
            IValidator<TimeEntry> arg2,
            IAuthorization<IPsaContext, TimeEntry> arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITimeEntryRepository field1;
        public readonly IValidator<TimeEntry> field2;
        public readonly IAuthorization<IPsaContext, TimeEntry> field3;
    }


    public interface ITimeEntrySuggestedRowService
        : IEntityService<TimeEntrySuggestedRow>
    {
    }


    public class TimeEntrySuggestedRowService : OrganizationEntityService<TimeEntrySuggestedRow,
            ITimeEntrySuggestedRowRepository, User, IPsaContext>
        , ITimeEntrySuggestedRowService
    {
        public TimeEntrySuggestedRowService(
            IContextService<IPsaContext> arg0,
            ITimeEntrySuggestedRowRepository arg1,
            IValidator<TimeEntrySuggestedRow> arg2,
            IAuthorization<IPsaContext, TimeEntrySuggestedRow> arg3,
            IGlobalSettingsService arg4
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITimeEntrySuggestedRowRepository field1;
        public readonly IValidator<TimeEntrySuggestedRow> field2;
        public readonly IAuthorization<IPsaContext, TimeEntrySuggestedRow> field3;
        public readonly IGlobalSettingsService field4;
    }


    public interface ITimeEntryTypeService
        : IEntityService<TimeEntryType>
    {
    }


    public class TimeEntryTypeService :
        OrganizationEntityService<TimeEntryType, ITimeEntryTypeRepository, User, IPsaContext>
        , ITimeEntryTypeService
    {
        public TimeEntryTypeService(
            IContextService<IPsaContext> arg0,
            ITimeEntryTypeRepository arg1,
            IValidator<TimeEntryType> arg2,
            IAuthorization<IPsaContext, TimeEntryType> arg3,
            ITimeEntryService arg4
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITimeEntryTypeRepository field1;
        public readonly IValidator<TimeEntryType> field2;
        public readonly IAuthorization<IPsaContext, TimeEntryType> field3;
        public readonly ITimeEntryService field4;
    }


    public interface ITravelExpenseReceiptService
    {
    }


    public class TravelExpenseReceiptService
        : ITravelExpenseReceiptService
    {
        public TravelExpenseReceiptService(
            IContextService<IPsaContext> arg0,
            IScannerService arg1,
            IIntegrationErrorService arg2,
            IGuidService arg3,
            IItemFileService arg4,
            IFileRepository arg5,
            IItemService arg6,
            ICaseRepository arg7,
            IPricelistService arg8,
            ITaskRepository arg9,
            ITaxRepository arg10,
            IProductRepository arg11,
            IUsedScannerReceiptRepository arg12,
            IUserRepository arg13,
            ITimeZoneService arg14
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
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IScannerService field1;
        public readonly IIntegrationErrorService field2;
        public readonly IGuidService field3;
        public readonly IItemFileService field4;
        public readonly IFileRepository field5;
        public readonly IItemService field6;
        public readonly ICaseRepository field7;
        public readonly IPricelistService field8;
        public readonly ITaskRepository field9;
        public readonly ITaxRepository field10;
        public readonly IProductRepository field11;
        public readonly IUsedScannerReceiptRepository field12;
        public readonly IUserRepository field13;
        public readonly ITimeZoneService field14;
    }


    public interface ITravelReimbursementService
        : IEntityService<TravelReimbursement>
    {
    }


    public class TravelReimbursementService : PsaEntityService<TravelReimbursement, ITravelReimbursementRepository>
        , ITravelReimbursementService
    {
        public TravelReimbursementService(
            IContextService<IPsaContext> arg0,
            ITravelReimbursementRepository arg1,
            IValidator<TravelReimbursement> arg2,
            IAuthorization<IPsaContext, TravelReimbursement> arg3,
            ICurrencyRepository arg4,
            ITravelReimbursementStatusRepository arg5,
            IBusinessUnitRepository arg6,
            IOrganizationRepository arg7,
            IItemRepository arg8,
            IUserRepository arg9
        ) : base(arg0, arg1, arg2, arg3)
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

        public readonly IContextService<IPsaContext> field0;
        public readonly ITravelReimbursementRepository field1;
        public readonly IValidator<TravelReimbursement> field2;
        public readonly IAuthorization<IPsaContext, TravelReimbursement> field3;
        public readonly ICurrencyRepository field4;
        public readonly ITravelReimbursementStatusRepository field5;
        public readonly IBusinessUnitRepository field6;
        public readonly IOrganizationRepository field7;
        public readonly IItemRepository field8;
        public readonly IUserRepository field9;
    }


    public interface ITravelReimbursementStatusService
        : IEntityService<TravelReimbursementStatus>
    {
    }


    public class TravelReimbursementStatusService :
        PsaEntityService<TravelReimbursementStatus, ITravelReimbursementStatusRepository>
        , ITravelReimbursementStatusService
    {
        public TravelReimbursementStatusService(
            IContextService<IPsaContext> arg0,
            ITravelReimbursementStatusRepository arg1,
            IValidator<TravelReimbursementStatus> arg2,
            IAuthorization<IPsaContext, TravelReimbursementStatus> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITravelReimbursementStatusRepository field1;
        public readonly IValidator<TravelReimbursementStatus> field2;
        public readonly IAuthorization<IPsaContext, TravelReimbursementStatus> field3;
    }


    public interface ITreeTaskService
        : IEntityService<TreeTask>
    {
    }


    public class TreeTaskService : OrganizationEntityService<TreeTask, ITreeTaskRepository, User, IPsaContext>
        , ITreeTaskService
    {
        public TreeTaskService(
            IContextService<IPsaContext> arg0,
            ITreeTaskRepository arg1,
            IValidator<TreeTask> arg2,
            IAuthorization<IPsaContext, TreeTask> arg3,
            IUserRepository arg4,
            ITaskRepository arg5
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITreeTaskRepository field1;
        public readonly IValidator<TreeTask> field2;
        public readonly IAuthorization<IPsaContext, TreeTask> field3;
        public readonly IUserRepository field4;
        public readonly ITaskRepository field5;
    }


    public interface IUserCostPerCaseService
        : IEntityService<UserCostPerCase>
    {
    }


    public class UserCostPerCaseService :
        OrganizationEntityService<UserCostPerCase, IUserCostPerCaseRepository, User, IPsaContext>
        , IUserCostPerCaseService
    {
        public UserCostPerCaseService(
            IContextService<IPsaContext> arg0,
            IUserCostPerCaseRepository arg1,
            IValidator<UserCostPerCase> arg2,
            IAuthorization<IPsaContext, UserCostPerCase> arg3,
            ICaseRepository arg4,
            IHourRepository arg5
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IUserCostPerCaseRepository field1;
        public readonly IValidator<UserCostPerCase> field2;
        public readonly IAuthorization<IPsaContext, UserCostPerCase> field3;
        public readonly ICaseRepository field4;
        public readonly IHourRepository field5;
    }


    public interface IUserService
        : IEntityService<User>
    {
    }


    public class UserService : OrganizationEntityService<User, IUserRepository, User, IPsaContext>
        , IUserService
    {
        public UserService(
            IContextService<IPsaContext> arg0,
            IUserRepository arg1,
            IValidator<User> arg2,
            IAuthorization<IPsaContext, User> arg3,
            IMasterUserRepository arg4,
            IOrganizationCompanyRepository arg5,
            ILanguageService arg6,
            IBusinessUnitService arg7,
            IEmploymentService arg8,
            IAddressService arg9,
            IOrganizationAddonService arg10,
            IBillingManagerFactory arg11,
            IProfileService arg12,
            IUserCostPerCaseRepository arg13,
            ITimeZoneService arg14,
            ICountryRegionRepository arg15,
            ICountryRepository arg16,
            IPsaUserService arg17
        ) : base(arg0, arg1, arg2, arg3)
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
            field17 = arg17;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IUserRepository field1;
        public readonly IValidator<User> field2;
        public readonly IAuthorization<IPsaContext, User> field3;
        public readonly IMasterUserRepository field4;
        public readonly IOrganizationCompanyRepository field5;
        public readonly ILanguageService field6;
        public readonly IBusinessUnitService field7;
        public readonly IEmploymentService field8;
        public readonly IAddressService field9;
        public readonly IOrganizationAddonService field10;
        public readonly IBillingManagerFactory field11;
        public readonly IProfileService field12;
        public readonly IUserCostPerCaseRepository field13;
        public readonly ITimeZoneService field14;
        public readonly ICountryRegionRepository field15;
        public readonly ICountryRepository field16;
        public readonly IPsaUserService field17;
    }


    public interface IUserSettingsService
        : IEntityService<UserSettings>
    {
    }


    public class UserSettingsService :
        OrganizationEntityService<UserSettings, IUserSettingsRepository, User, IPsaContext>
        , IUserSettingsService
    {
        public UserSettingsService(
            IContextService<IPsaContext> arg0,
            IUserSettingsRepository arg1,
            IValidator<UserSettings> arg2,
            IAuthorization<IPsaContext, UserSettings> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IUserSettingsRepository field1;
        public readonly IValidator<UserSettings> field2;
        public readonly IAuthorization<IPsaContext, UserSettings> field3;
    }


    public interface IUserTagService
        : ITagEntityService<UserTag>
    {
    }


    public class UserTagService : OrganizationEntityService<UserTag, IUserTagRepository, User, IPsaContext>
        , IUserTagService
    {
        public UserTagService(
            IContextService<IPsaContext> arg0,
            IUserTagRepository arg1,
            IValidator<UserTag> arg2,
            IAuthorization<IPsaContext, UserTag> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IUserTagRepository field1;
        public readonly IValidator<UserTag> field2;
        public readonly IAuthorization<IPsaContext, UserTag> field3;
    }


    public interface IUserTaskFavoriteService
        : IEntityService<UserTaskFavorite>
    {
    }


    public class UserTaskFavoriteService :
        OrganizationEntityService<UserTaskFavorite, IUserTaskFavoriteRepository, User, IPsaContext>
        , IUserTaskFavoriteService
    {
        public UserTaskFavoriteService(
            IContextService<IPsaContext> arg0,
            IUserTaskFavoriteRepository arg1,
            IValidator<UserTaskFavorite> arg2,
            IAuthorization<IPsaContext, UserTaskFavorite> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IUserTaskFavoriteRepository field1;
        public readonly IValidator<UserTaskFavorite> field2;
        public readonly IAuthorization<IPsaContext, UserTaskFavorite> field3;
    }


    public interface IUserWeeklyViewRowService
        : IEntityService<UserWeeklyViewRow>
    {
    }


    public class UserWeeklyViewRowService : OrganizationEntityService<UserWeeklyViewRow, IUserWeeklyViewRowRepository,
            User, IPsaContext>
        , IUserWeeklyViewRowService
    {
        public UserWeeklyViewRowService(
            IContextService<IPsaContext> arg0,
            IUserWeeklyViewRowRepository arg1,
            IValidator<UserWeeklyViewRow> arg2,
            IAuthorization<IPsaContext, UserWeeklyViewRow> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IUserWeeklyViewRowRepository field1;
        public readonly IValidator<UserWeeklyViewRow> field2;
        public readonly IAuthorization<IPsaContext, UserWeeklyViewRow> field3;
    }


    public interface IWorkdayHandlerService
    {
    }


    public interface IWorkdayService
        : IEntityService<Workday>
    {
    }


    public interface IWorkHourOverviewReportService
    {
    }


    public class WorkHourOverviewReportService
        : IWorkHourOverviewReportService
    {
        public WorkHourOverviewReportService(
            IOrganizationSettingsService arg0,
            IWorkHourOverviewReportHandler arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IOrganizationSettingsService field0;
        public readonly IWorkHourOverviewReportHandler field1;
    }


    public interface IWorkHourSuggestedRowService
        : IEntityService<WorkHourSuggestedRow>
    {
    }


    public class WorkHourSuggestedRowService : OrganizationEntityService<WorkHourSuggestedRow,
            IWorkHourSuggestedRowRepository, User, IPsaContext>
        , IWorkHourSuggestedRowService
    {
        public WorkHourSuggestedRowService(
            IContextService<IPsaContext> arg0,
            IWorkHourSuggestedRowRepository arg1,
            IValidator<WorkHourSuggestedRow> arg2,
            IAuthorization<IPsaContext, WorkHourSuggestedRow> arg3,
            IGlobalSettingsService arg4,
            IResourceAllocationService arg5
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IWorkHourSuggestedRowRepository field1;
        public readonly IValidator<WorkHourSuggestedRow> field2;
        public readonly IAuthorization<IPsaContext, WorkHourSuggestedRow> field3;
        public readonly IGlobalSettingsService field4;
        public readonly IResourceAllocationService field5;
    }


    public interface IWorkingDayExceptionService
        : IEntityService<WorkingDayException>
    {
    }


    public class WorkingDayExceptionService :
        EntityService<IPsaContext, WorkingDayException, IWorkingDayExceptionRepository>
        , IWorkingDayExceptionService
    {
        public WorkingDayExceptionService(
            IContextService<IPsaContext> arg0,
            IWorkingDayExceptionRepository arg1,
            IValidator<WorkingDayException> arg2,
            IAuthorization<IPsaContext, WorkingDayException> arg3,
            IFlextimeManagerService arg4
        ) : base(arg0, arg1, arg2, arg3)
        {
            field4 = arg4;
        }

        public readonly IFlextimeManagerService field4;
    }


    public interface IWorkingDayService
    {
    }


    public class WorkingDayService
        : IWorkingDayService
    {
        public WorkingDayService(
            IWorkingDayExceptionRepository arg0,
            IOrganizationWorkweekService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IWorkingDayExceptionRepository field0;
        public readonly IOrganizationWorkweekService field1;
    }


    public interface IWorkPriceService
        : IEntityService<WorkPrice>
    {
    }


    public class WorkPriceService : OrganizationEntityService<WorkPrice, IWorkPriceRepository, User, IPsaContext>
        , IWorkPriceService
    {
        public WorkPriceService(
            IContextService<IPsaContext> arg0,
            IWorkPriceRepository arg1,
            IValidator<WorkPrice> arg2,
            IAuthorization<IPsaContext, WorkPrice> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IWorkPriceRepository field1;
        public readonly IValidator<WorkPrice> field2;
        public readonly IAuthorization<IPsaContext, WorkPrice> field3;
    }


    public interface IWorkTypeService
        : IEntityService<WorkType>
    {
    }


    public class WorkTypeService : OrganizationSettingsService<WorkType, IWorkTypeRepository>
        , IWorkTypeService
    {
        public WorkTypeService(
            IContextService<IPsaContext> arg0,
            IWorkTypeRepository arg1,
            IValidator<WorkType> arg2,
            IAuthorization<IPsaContext, WorkType> arg3,
            ITaskRepository arg4,
            ICaseRepository arg5,
            IHourRepository arg6,
            ICaseWorkTypeRepository arg7,
            IWorkPriceService arg8,
            IUserRepository arg9
        ) : base(arg0, arg1, arg2, arg3)
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

        public readonly IContextService<IPsaContext> field0;
        public readonly IWorkTypeRepository field1;
        public readonly IValidator<WorkType> field2;
        public readonly IAuthorization<IPsaContext, WorkType> field3;
        public readonly ITaskRepository field4;
        public readonly ICaseRepository field5;
        public readonly IHourRepository field6;
        public readonly ICaseWorkTypeRepository field7;
        public readonly IWorkPriceService field8;
        public readonly IUserRepository field9;
    }


    public class InvoiceHandlerServiceBase
    {
    }


    public class ScannerContextService
        : IContextService
    {
        public ScannerContextService(
            IPsaContextService arg0,
            IGuidService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IPsaContextService field0;
        public readonly IGuidService field1;
    }


    public class FileService : OrganizationEntityService<File, IFileRepository, User, IPsaContext>
        , IFileService
    {
        public FileService(
            IContextService<IPsaContext> arg0,
            IFileRepository arg1,
            IValidator<File> arg2,
            IAuthorization<IPsaContext, File> arg3,
            IUserRepository arg4,
            IFileDownloadService arg5,
            IFileDataService arg6,
            ICaseFileService arg9,
            IFileTagService arg10,
            ICaseNoteService arg11,
            IOfferFileService arg14
        ) : base()
        {
        }
    }


    public class FileRepositoryType
    {
    }


    public class FileCategory
    {
    }


    public class FileStorageInfo
    {
        public FileStorageInfo(
            FileCategory arg0,
            string arg1,
            FileRepositoryType arg2
        )
        {
            field2 = arg2;
        }

        public FileStorageInfo(
            Case arg0,
            FileRepositoryType arg1
        )
        {
            field1 = arg1;
        }

        public FileStorageInfo(
            User arg0,
            FileRepositoryType arg1
        )
        {
            field1 = arg1;
        }

        public FileStorageInfo(
            Invoice arg0,
            FileRepositoryType arg1
        )
        {
            field1 = arg1;
        }

        public FileStorageInfo(
            Item arg0,
            FileRepositoryType arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly Item field0;
        public readonly FileRepositoryType field1;
        public readonly FileRepositoryType field2;
    }


    public interface IFileService
        : IEntityService<File>
    {
    }


    public interface IOfferFileService
        : IEntityService<OfferFile>
    {
    }


    public class OfferFileService : OrganizationEntityService<OfferFile, IOfferFileRepository, User, IPsaContext>
        , IOfferFileService
    {
        public OfferFileService(
            IContextService<IPsaContext> arg0,
            IValidator<OfferFile> arg1,
            IAuthorization<IPsaContext, OfferFile> arg2
            //IOfferFileRepository arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            //field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IValidator<OfferFile> field1;
        public readonly IAuthorization<IPsaContext, OfferFile> field2;
        public readonly IOfferFileRepository field3;
    }


    public interface IOfferItemService
        : IEntityService<OfferItem>
    {
    }


    public class OfferItemService : OrganizationEntityService<OfferItem, IOfferItemRepository, User, IPsaContext>
        , IOfferItemService
    {
        public OfferItemService(
            IContextService<IPsaContext> arg0,
            IOfferItemRepository arg1,
            IValidator<OfferItem> arg2,
            IPricelistVersionService arg6,
            IAuthorization<IPsaContext, Offer> arg12
        ) : base()
        {
        }
    }


    public interface IOfferService
        : IEntityService<Offer>
    {
    }


    public class OfferService : OrganizationEntityService<Offer, IOfferRepository, User, IPsaContext>
        , IOfferService
    {
        public OfferService(
            IContextService<IPsaContext> arg0,
            IOfferRepository arg3,
            IProposalStatusService arg4,
            ITaskService arg5,
            ICaseService arg6,
            IPricelistService arg7,
            IInvoiceService arg12,
            IProductPriceService arg18,
            IPricelistVersionService arg19,
            IOfferItemService arg22,
            ICompanyRepository arg28
        ) : base(arg0)
        {
        }
    }


    public interface IOfferSubtotalService
        : IEntityService<OfferSubtotal>
    {
    }


    public class OfferSubtotalService :
        OrganizationEntityService<OfferSubtotal, IOfferSubtotalRepository, User, IPsaContext>
        , IOfferSubtotalService
    {
        public OfferSubtotalService(
            IContextService<IPsaContext> arg0,
            IOfferSubtotalRepository arg1,
            IValidator<OfferSubtotal> arg2,
            IAuthorization<IPsaContext, OfferSubtotal> arg3,
            IOfferItemRepository arg4,
            IOfferTaskRepository arg5
        ) : base(arg0, arg1, arg2, arg3)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IOfferSubtotalRepository field1;
        public readonly IValidator<OfferSubtotal> field2;
        public readonly IAuthorization<IPsaContext, OfferSubtotal> field3;
        public readonly IOfferItemRepository field4;
        public readonly IOfferTaskRepository field5;
    }


    public interface IOfferTaskService
        : IEntityService<OfferTask>
    {
    }


    public class OfferTaskService : OrganizationEntityService<OfferTask, IOfferTaskRepository, User, IPsaContext>
        , IOfferTaskService
    {
        public OfferTaskService(
            IContextService<IPsaContext> arg0,
            IOfferTaskRepository arg1,
            IValidator<OfferTask> arg2,
            IAuthorization<IPsaContext, OfferTask> arg3,
            ICaseRepository arg4,
            ICurrencyRepository arg5,
            IOfferRepository arg6,
            IOfferSubtotalRepository arg7
        ) : base(arg0, arg1, arg2, arg3)
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

        public readonly IContextService<IPsaContext> field0;
        public readonly IOfferTaskRepository field1;
        public readonly IValidator<OfferTask> field2;
        public readonly IAuthorization<IPsaContext, OfferTask> field3;
        public readonly ICaseRepository field4;
        public readonly ICurrencyRepository field5;
        public readonly IOfferRepository field6;
        public readonly IOfferSubtotalRepository field7;
    }


    public interface IOfferTemplateService
        : IEntityService<Offer>
    {
    }


    public class OfferTemplateService : OrganizationEntityService<Offer, IOfferRepository, User, IPsaContext>
        , IOfferTemplateService
    {
        public OfferTemplateService(
            IContextService<IPsaContext> arg0,
            IValidator<Offer> arg1,
            IAuthorization<IPsaContext, Offer> arg2,
            IOfferRepository arg3,
            IOfferService arg4
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IValidator<Offer> field1;
        public readonly IAuthorization<IPsaContext, Offer> field2;
        public readonly IOfferRepository field3;
        public readonly IOfferService field4;
    }


    public interface IProposalStatusService
        : IEntityService<ProposalStatus>
    {
    }


    public class ProposalStatusService :
        OrganizationEntityService<ProposalStatus, IProposalStatusRepository, User, IPsaContext>
        , IProposalStatusService
    {
        public ProposalStatusService(
            IContextService<IPsaContext> arg0,
            IProposalStatusRepository arg1,
            IValidator<ProposalStatus> arg2,
            IAuthorization<IPsaContext, ProposalStatus> arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
        }
    }


    public interface IReferenceNumberService
    {
    }


    public class ReferenceNumberService
        : IReferenceNumberService
    {
        public ReferenceNumberService(
            IContextService<IPsaContext> arg0,
            IBusinessUnitRepository arg1,
            IAccountRepository arg2,
            ICountryService arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IBusinessUnitRepository field1;
        public readonly IAccountRepository field2;
        public readonly ICountryService field3;
    }


    public interface IRevenueRecognitionService
    {
    }


    public class RevenueRecognitionService
        : IRevenueRecognitionService
    {
        public RevenueRecognitionService(
            IContextService<IPsaContext> arg0,
            IInvoiceRowService arg1,
            IBillingPlanService arg2,
            ITaskRepository arg3,
            ICostCenterRepository arg4,
            ICostCenterRevenueService arg5,
            IInvoiceRepository arg6,
            IItemRepository arg7,
            IHourRepository arg8,
            IInvoiceCaseRepository arg9,
            IInvoiceStatusService arg10
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
        public readonly IInvoiceRowService field1;
        public readonly IBillingPlanService field2;
        public readonly ITaskRepository field3;
        public readonly ICostCenterRepository field4;
        public readonly ICostCenterRevenueService field5;
        public readonly IInvoiceRepository field6;
        public readonly IItemRepository field7;
        public readonly IHourRepository field8;
        public readonly IInvoiceCaseRepository field9;
        public readonly IInvoiceStatusService field10;
    }


    public class OfferSubtotalRow
    {
        public OfferSubtotalRow(
        )
        {
        }
    }


    public class OrganizationEmailService
        : IOrganizationEmailService
    {
        public OrganizationEmailService(
            IEnvironmentOpenedEmailBuilder arg0,
            IEnvironmentClosedEmailBuilder arg1,
            IMailClient arg2,
            IEmailTemplateService arg3,
            IMasterOrganizationRepository arg4,
            IAppSettings arg5,
            IInvoicingContactService arg6,
            IDict arg7,
            IDistributorHelperService arg8,
            IContextService arg9,
            IPsaContextService arg10
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

        public readonly IEnvironmentOpenedEmailBuilder field0;
        public readonly IEnvironmentClosedEmailBuilder field1;
        public readonly IMailClient field2;
        public readonly IEmailTemplateService field3;
        public readonly IMasterOrganizationRepository field4;
        public readonly IAppSettings field5;
        public readonly IInvoicingContactService field6;
        public readonly IDict field7;
        public readonly IDistributorHelperService field8;
        public readonly IContextService field9;
        public readonly IPsaContextService field10;
    }

    public interface IEmailTemplateService
    {
    }
    
    public class EmailTemplateService : IEmailTemplateService { }

    public interface IPricelistPriceService
    {
    }


    public class PricelistPriceService
        : IPricelistPriceService
    {
        public PricelistPriceService(
            IPricelistRepository arg0
        )
        {
            field0 = arg0;
        }

        public readonly IPricelistRepository field0;
    }


    public interface IPricelistService
        : IEntityService<Pricelist>
    {
    }


    public class PricelistService : OrganizationEntityService<Pricelist, IPricelistRepository, User, IPsaContext>
        , IPricelistService
    {
        public PricelistService(
            IContextService<IPsaContext> arg0,
            IValidator<Pricelist> arg1,
            IAuthorization<IPsaContext, Pricelist> arg2,
            IPricelistRepository arg3,
            IWorkPriceRepository arg4,
            IProductPriceRepository arg5,
            IWorkTypeService arg6,
            ICurrencyService arg7,
            IWorkPriceService arg8,
            IProductPriceService arg9,
            IOvertimePriceService arg10,
            IPricelistVersionService arg11,
            IProductService arg12,
            ICaseRepository arg13,
            IItemService arg14
        ) : base(arg0)
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
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IValidator<Pricelist> field1;
        public readonly IAuthorization<IPsaContext, Pricelist> field2;
        public readonly IPricelistRepository field3;
        public readonly IWorkPriceRepository field4;
        public readonly IProductPriceRepository field5;
        public readonly IWorkTypeService field6;
        public readonly ICurrencyService field7;
        public readonly IWorkPriceService field8;
        public readonly IProductPriceService field9;
        public readonly IOvertimePriceService field10;
        public readonly IPricelistVersionService field11;
        public readonly IProductService field12;
        public readonly ICaseRepository field13;
        public readonly IItemService field14;
    }


    public interface IPricelistVersionRuleService
    {
    }


    public class PricelistVersionRuleService
        : IPricelistVersionRuleService
    {
        public PricelistVersionRuleService(
            IContextService<IPsaContext> arg0,
            IPricelistVersionRepository arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IPricelistVersionRepository field1;
    }

    public interface IPsaUserPhotoFileService
    {
    }


    public class PsaUserPhotoFileService
        : IPsaUserPhotoFileService
    {
        public PsaUserPhotoFileService(
            IUserRepository arg0,
            IOrganizationContextScopeService arg1,
            IOrganizationRepository arg2,
            IFileService arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IUserRepository field0;
        public readonly IOrganizationContextScopeService field1;
        public readonly IOrganizationRepository field2;
        public readonly IFileService field3;
    }

    public class UserAnonymizer
        : IUserAnonymizer
    {
        public UserAnonymizer(
            IPsaContextService arg0,
            IDict arg1,
            IUserRepository arg2,
            IOrganizationUserRepository arg3,
            IUserTagRepository arg4,
            IFileService arg5
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IPsaContextService field0;
        public readonly IDict field1;
        public readonly IUserRepository field2;
        public readonly IOrganizationUserRepository field3;
        public readonly IUserTagRepository field4;
        public readonly IFileService field5;
    }


    public class UserDefaultsService
        : IUserDefaultsService
    {
        public UserDefaultsService(
            IPsaContextService arg0,
            IUserRepository arg1,
            IWorkTypeRepository arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IPsaContextService field0;
        public readonly IUserRepository field1;
        public readonly IWorkTypeRepository field2;
    }


    public class AccessRightsHelperValidator
        : IAccessRightsHelperValidator
    {
        public AccessRightsHelperValidator(
            IContextService<IPsaContext> arg0,
            IInvoiceStatusRepository arg1,
            IProfileRightRepository arg2,
            IUserRepository arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceStatusRepository field1;
        public readonly IProfileRightRepository field2;
        public readonly IUserRepository field3;
    }


    public class AccountNoteValidator : Validator<AccountNote>
    {
        public AccountNoteValidator(
            IContextService<IPsaContext> arg0,
            IAccountNoteRepository arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IAccountNoteRepository field1;
    }


    public class AccountValidator
    {
        public AccountValidator(
            IContextService<IPsaContext> arg0,
            IAccountRepository arg1,
            IPricelistRepository arg2,
            ILanguageService arg3,
            IOrganizationPermissionService arg4
        ) : base()
        {
            field4 = arg4;
        }

        public readonly IOrganizationPermissionService field4;

        private AccountValidator() 
        {
        }
    }


    public class ActivityContactMemberValidator : Validator<ActivityContactMember>
    {
        public ActivityContactMemberValidator(
            IActivityContactMemberRepository arg0,
            IActivityRepository arg1,
            IContactRepository arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IActivityContactMemberRepository field0;
        public readonly IActivityRepository field1;
        public readonly IContactRepository field2;
    }


    public class ActivityResourceMemberValidator : Validator<ActivityResourceMember>
    {
        public ActivityResourceMemberValidator(
            IActivityResourceMemberRepository arg0,
            IActivityRepository arg1,
            IResourceRepository arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IActivityResourceMemberRepository field0;
        public readonly IActivityRepository field1;
        public readonly IResourceRepository field2;
    }


    public class ActivityStatusValidator : Validator<ActivityStatus>
    {
        public ActivityStatusValidator(
            IActivityStatusRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IActivityStatusRepository field0;
    }


    public class ActivityTypeValidator : Validator<ActivityType>
    {
        public ActivityTypeValidator(
            IContextService<IPsaContext> arg0,
            IActivityTypeRepository arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IActivityTypeRepository field1;
    }


    public class ActivityUserMemberValidator : Validator<ActivityUserMember>
    {
        public ActivityUserMemberValidator(
            IActivityUserMemberRepository arg0,
            IActivityRepository arg1,
            IUserRepository arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IActivityUserMemberRepository field0;
        public readonly IActivityRepository field1;
        public readonly IUserRepository field2;
    }


    public class ActivityValidator : Validator<Activity>
    {
        public ActivityValidator(
            IActivityRepository arg0,
            IActivityTypeRepository arg1,
            ICaseRepository arg2,
            IContextService<IPsaContext> arg3,
            IHourRepository arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IActivityRepository field0;
        public readonly IActivityTypeRepository field1;
        public readonly ICaseRepository field2;
        public readonly IContextService<IPsaContext> field3;
        public readonly IHourRepository field4;
    }


    public class BackgroundTaskValidator : Validator<BackgroundTask>
    {
        public BackgroundTaskValidator(
            IBackgroundTaskRepository arg0,
            IBackgroundExecutorService arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IBackgroundTaskRepository field0;
        public readonly IBackgroundExecutorService field1;
    }


    public class BankAccountValidator : Validator<BankAccount>
    {
        public BankAccountValidator(
            IBankAccountRepository arg0,
            IBusinessUnitRepository arg1,
            IOrganizationCompanyRepository arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IBankAccountRepository field0;
        public readonly IBusinessUnitRepository field1;
        public readonly IOrganizationCompanyRepository field2;
    }


    public class BusinessUnitValidator : Validator<BusinessUnit>
    {
        public BusinessUnitValidator(
            IContextService<IPsaContext> arg0,
            IBusinessUnitRepository arg1,
            IOrganizationCompanyRepository arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IBusinessUnitRepository field1;
        public readonly IOrganizationCompanyRepository field2;
    }


    public class CaseFileValidator : Validator<CaseFile>
    {
        public CaseFileValidator(
            IInvoiceFileRepository arg0,
            IInvoiceStatusRepository arg1,
            IOfferFileRepository arg2,
            IProposalStatusRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IInvoiceFileRepository field0;
        public readonly IInvoiceStatusRepository field1;
        public readonly IOfferFileRepository field2;
        public readonly IProposalStatusRepository field3;
    }


    public class CaseMemberValidator : Validator<CaseMember>
    {
        public CaseMemberValidator(
            IContextService<IPsaContext> arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IContextService<IPsaContext> field0;
    }


    public class CaseNoteValidator : Validator<CaseNote>
    {
        public CaseNoteValidator(
            IContextService<IPsaContext> arg0,
            ICaseNoteRepository arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICaseNoteRepository field1;
    }


    public class CaseProductValidator : Validator<CaseProduct>
    {
        public CaseProductValidator(
            ICaseProductRepository arg0,
            ICaseRepository arg1,
            IProductRepository arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseProductRepository field0;
        public readonly ICaseRepository field1;
        public readonly IProductRepository field2;
    }


    public class CaseStatusTypeValidator : Validator<CaseStatusType>
    {
        public CaseStatusTypeValidator(
            ICaseStatusTypeRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ICaseStatusTypeRepository field0;
    }


    public class CaseTagValidator : TagBaseValidator<Case, CaseTag>
    {
        public CaseTagValidator(
            ICaseTagRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ICaseTagRepository field0;
    }

    public class TagBaseValidator<T1, T2>
    {
    }

    public class CaseValidator : Validator<Case>
    {
        public CaseValidator(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            IAccountService arg2,
            IUserRepository arg3,
            IContactService arg4,
            IBusinessUnitRepository arg5,
            IInvoiceCaseRepository arg6
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ICaseRepository field1;
        public readonly IAccountService field2;
        public readonly IUserRepository field3;
        public readonly IContactService field4;
        public readonly IBusinessUnitRepository field5;
        public readonly IInvoiceCaseRepository field6;
    }


    public class CaseWorkTypeValidator : Validator<CaseWorkType>
    {
        public CaseWorkTypeValidator(
            ICaseWorkTypeRepository arg0,
            ICaseRepository arg1,
            IWorkTypeRepository arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseWorkTypeRepository field0;
        public readonly ICaseRepository field1;
        public readonly IWorkTypeRepository field2;
    }


    public class CommunicationMethodValidator : Validator<CommunicationMethod>
    {
        public CommunicationMethodValidator(
            ICommunicationMethodRepository arg0,
            ICommunicatesWithRepository arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly ICommunicationMethodRepository field0;
        public readonly ICommunicatesWithRepository field1;
    }


    public class ContactRoleValidator : Validator<ContactRole>
    {
        public ContactRoleValidator(
            IContactRoleRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IContactRoleRepository field0;
    }


    public class CostCenterValidator : Validator<CostCenter>
    {
        public CostCenterValidator(
            ICostCenterRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ICostCenterRepository field0;
    }


    public class CountryProductValidator : Validator<CountryProduct>
    {
        public CountryProductValidator(
            ICountryProductRepository arg0,
            IOrganizationCompanyRepository arg1,
            ITaxRepository arg2,
            IProductRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly ICountryProductRepository field0;
        public readonly IOrganizationCompanyRepository field1;
        public readonly ITaxRepository field2;
        public readonly IProductRepository field3;
    }


    public class FileValidator : Validator<File>
    {
        public FileValidator(
            IInvoiceFileRepository arg0,
            IInvoiceStatusRepository arg1,
            IOfferFileRepository arg2,
            IProposalStatusRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IInvoiceFileRepository field0;
        public readonly IInvoiceStatusRepository field1;
        public readonly IOfferFileRepository field2;
        public readonly IProposalStatusRepository field3;
    }


    public abstract partial class HourBaseValidator<TEntityFields, TEntity> : Validator<TEntity>
        where TEntityFields : IIdentifiableEntity, IHour
        where TEntity : TEntityFields, IIdentifiableEntityWithOriginalState<TEntityFields>
    {
        protected readonly IContextService<IPsaContext> _ContextService;
        protected readonly IUserRepository _UserRepository;
        protected readonly IOvertimeRepository _OvertimeRepository;
        protected readonly IEmploymentRepository _EmploymentRepository;
        protected readonly IWorkdayRepository _WorkdayRepository;

        protected IPsaContext Context => null;

        public HourBaseValidator(IContextService<IPsaContext> contextService, IUserRepository userRepository, IOvertimeRepository overtimeRepository, IEmploymentRepository employmentRepository, IWorkdayRepository workdayRepository) : base()
        {
            _ContextService = contextService;
            _UserRepository = userRepository;
            _OvertimeRepository = overtimeRepository;
            _EmploymentRepository = employmentRepository;
            _WorkdayRepository = workdayRepository;
        }

        protected HourBaseValidator()
        {
        }
    }


    public interface IAccessRightsHelperValidator
    {
    }


    public class InvoiceConfigValidator : Validator<InvoiceConfig>
    {
        public InvoiceConfigValidator(
            IInvoiceConfigRepository arg0,
            IInvoiceRepository arg1,
            IInvoiceStatusRepository arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IInvoiceConfigRepository field0;
        public readonly IInvoiceRepository field1;
        public readonly IInvoiceStatusRepository field2;
    }


    public class InvoiceFileValidator : Validator<InvoiceFile>
    {
        public InvoiceFileValidator(
            IContextService<IPsaContext> arg0,
            IInvoiceFileRepository arg1,
            IInvoiceRepository arg2,
            IInvoiceStatusRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceFileRepository field1;
        public readonly IInvoiceRepository field2;
        public readonly IInvoiceStatusRepository field3;
    }


    public class InvoiceHtmlValidator : Validator<InvoiceHTML>
    {
        public InvoiceHtmlValidator(
            IContextService<IPsaContext> arg0,
            IInvoiceHtmlRepository arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceHtmlRepository field1;
    }


    public class InvoiceRowValidator : Validator<InvoiceRow>
    {
        public InvoiceRowValidator(
            IContextService<IPsaContext> arg0,
            IInvoiceRowRepository arg1,
            IInvoiceRepository arg2,
            IInvoiceStatusRepository arg3,
            ISalesAccountRepository arg4,
            ICostCenterRepository arg5,
            ICurrencyRepository arg6,
            ITaxRepository arg7,
            IInvoiceConfigRepository arg8
        ) : base()
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

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceRowRepository field1;
        public readonly IInvoiceRepository field2;
        public readonly IInvoiceStatusRepository field3;
        public readonly ISalesAccountRepository field4;
        public readonly ICostCenterRepository field5;
        public readonly ICurrencyRepository field6;
        public readonly ITaxRepository field7;
        public readonly IInvoiceConfigRepository field8;
    }


    public class InvoiceStatusValidator : Validator<InvoiceStatus>
    {
        public InvoiceStatusValidator(
            IContextService<IPsaContext> arg0,
            IInvoiceStatusRepository arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceStatusRepository field1;
    }


    public class InvoiceTemplateConfigValidator : Validator<InvoiceTemplateConfig>
    {
        public InvoiceTemplateConfigValidator(
        )
        {
        }
    }


    public class InvoiceTemplateValidator : Validator<InvoiceTemplate>
    {
        public InvoiceTemplateValidator(
            IInvoiceTemplateRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IInvoiceTemplateRepository field0;
    }


    public class InvoiceValidator : Validator<Invoice>
    {
        public InvoiceValidator(
            IContextService<IPsaContext> arg0,
            IInvoiceRepository arg1,
            IBusinessUnitService arg2,
            IInvoiceStatusService arg3,
            IOrganizationRepository arg4,
            IContactRepository arg5,
            ILanguageRepository arg6,
            IFormatingCultureRepository arg7
        ) : base()
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

        public readonly IContextService<IPsaContext> field0;
        public readonly IInvoiceRepository field1;
        public readonly IBusinessUnitService field2;
        public readonly IInvoiceStatusService field3;
        public readonly IOrganizationRepository field4;
        public readonly IContactRepository field5;
        public readonly ILanguageRepository field6;
        public readonly IFormatingCultureRepository field7;
    }


    public interface IScheduledWorkTaskValidator
        : IValidator<BackgroundTask>
    {
    }


    public class ScheduledWorkTaskValidator : BackgroundTaskValidator
        , IScheduledWorkTaskValidator
    {
        public ScheduledWorkTaskValidator(
            IBackgroundTaskRepository arg0,
            IBackgroundExecutorService arg1,
            IInvoiceStatusService arg2
        ) : base(arg0, arg1)
        {
            field2 = arg2;
        }

        public readonly IInvoiceStatusService field2;
        public void Validate() => throw new NotImplementedException();

        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }


    public class LinkValidator : Validator<Link>
    {
        public LinkValidator(
            ILinkRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ILinkRepository field0;
    }


    public class OfferFileValidator : Validator<OfferFile>
    {
        public OfferFileValidator(
            IOfferFileRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IOfferFileRepository field0;
    }


    public class OfferItemValidator : Validator<OfferItem>
    {
        public OfferItemValidator(
            IContextService<IPsaContext> arg0,
            IOfferItemRepository arg1,
            IOfferRepository arg2,
            ITaxRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IOfferItemRepository field1;
        public readonly IOfferRepository field2;
        public readonly ITaxRepository field3;
    }


    public class OfferSubtotalValidator : Validator<OfferSubtotal>
    {
        public OfferSubtotalValidator(
            IOfferSubtotalRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IOfferSubtotalRepository field0;
    }


    public class OfferTaskValidator : Validator<OfferTask>
    {
        public OfferTaskValidator(
            IOfferTaskRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IOfferTaskRepository field0;
    }


    public class OfferValidator : Validator<Offer>
    {
        public OfferValidator(
            IOfferRepository arg0,
            ICaseRepository arg1,
            IContactRepository arg2,
            IOfferItemRepository arg3,
            IOfferTaskRepository arg4,
            IOfferFileRepository arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IOfferRepository field0;
        public readonly ICaseRepository field1;
        public readonly IContactRepository field2;
        public readonly IOfferItemRepository field3;
        public readonly IOfferTaskRepository field4;
        public readonly IOfferFileRepository field5;
    }


    public class OverTimePriceValidator : Validator<OverTimePrice>
    {
        public OverTimePriceValidator(
            IOvertimePriceRepository arg0,
            ICaseRepository arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IOvertimePriceRepository field0;
        public readonly ICaseRepository field1;
    }


    public class OvertimeValidator : Validator<OverTime>
    {
        public OvertimeValidator(
            IOvertimeRepository arg0,
            IOvertimePriceRepository arg1,
            IHourRepository arg2,
            IReimbursedHourRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IOvertimeRepository field0;
        public readonly IOvertimePriceRepository field1;
        public readonly IHourRepository field2;
        public readonly IReimbursedHourRepository field3;
    }


    public class PricelistValidator : Validator<Pricelist>
    {
        public PricelistValidator(
            IPricelistRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IPricelistRepository field0;
    }


    public class ProductCategoryValidator : Validator<ProductCategory>
    {
        public ProductCategoryValidator(
            IProductCategoryRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IProductCategoryRepository field0;
    }


    public class ProductPriceValidator : Validator<ProductPrice>
    {
        public ProductPriceValidator(
            IProductPriceRepository arg0,
            IPricelistRepository arg1,
            ICaseRepository arg2,
            IProductRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IProductPriceRepository field0;
        public readonly IPricelistRepository field1;
        public readonly ICaseRepository field2;
        public readonly IProductRepository field3;
    }


    public class ProductValidator : Validator<Product>
    {
        public ProductValidator(
            IContextService<IPsaContext> arg0,
            ITaxRepository arg1,
            IItemRepository arg2,
            IProductRepository arg3,
            ICurrencyRepository arg4,
            IWorkTypeRepository arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITaxRepository field1;
        public readonly IItemRepository field2;
        public readonly IProductRepository field3;
        public readonly ICurrencyRepository field4;
        public readonly IWorkTypeRepository field5;
    }


    public class ProfileDashboardValidator : Validator<ProfileDashboard>
    {
        public ProfileDashboardValidator(
        )
        {
        }
    }


    public class ProfileValidator : Validator<Profile>
    {
        public ProfileValidator(
            IContextService<IPsaContext> arg0,
            IProfileRepository arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IProfileRepository field1;
    }


    public class ProposalStatusValidator : Validator<ProposalStatus>
    {
        public ProposalStatusValidator(
            IContextService<IPsaContext> arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IContextService<IPsaContext> field0;
    }


    public class ReportValidator : Validator<Report>
    {
        public ReportValidator(
            IReportRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IReportRepository field0;
    }


    public class ResourceAllocationValidator : Validator<ResourceAllocation>
    {
        public ResourceAllocationValidator(
            IResourceAllocationRepository arg0,
            ITaskRepository arg1,
            ICaseMemberRepository arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IResourceAllocationRepository field0;
        public readonly ITaskRepository field1;
        public readonly ICaseMemberRepository field2;
    }


    public class ResourceValidator : Validator<Resource>
    {
        public ResourceValidator(
            IResourceRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IResourceRepository field0;
    }


    public class SalesAccountValidator : Validator<SalesAccount>
    {
        public SalesAccountValidator(
            ISalesAccountRepository arg0,
            IItemRepository arg1,
            IProductRepository arg2,
            IWorkTypeRepository arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly ISalesAccountRepository field0;
        public readonly IItemRepository field1;
        public readonly IProductRepository field2;
        public readonly IWorkTypeRepository field3;
    }


    public class SalesProcessValidator : Validator<SalesProcess>
    {
        public SalesProcessValidator(
            ISalesProcessRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ISalesProcessRepository field0;
    }


    public class SearchCriteriaValidator : Validator<Entities.SearchCriteria>
    {
        public SearchCriteriaValidator(
            IContextService<IPsaContext> arg0,
            ISearchCriteriaRepository arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ISearchCriteriaRepository field1;
    }


    public class SearchValidator : Validator<Entities.Search>
    {
        public SearchValidator(
            ISearchRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ISearchRepository field0;
    }


    public class TagBaseValidator : Validator<TTagEntity>
    {
        public TagBaseValidator(
        )
        {
        }
    }


    public class TagValidator : Validator<Tag>
    {
        public TagValidator(
            ITagRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ITagRepository field0;
    }


    public class TaskStatusTypeValidator : Validator<TaskStatusType>
    {
        public TaskStatusTypeValidator(
            ITaskStatusTypeRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ITaskStatusTypeRepository field0;
    }


    public class TaskValidator : Validator<Task>
    {
        public TaskValidator(
            ITaskRepository arg0,
            IHourRepository arg1,
            IActivityRepository arg2,
            IItemRepository arg3,
            ITimeEntryRepository arg4,
            ICaseRepository arg5,
            ICaseWorkTypeRepository arg6,
            IWorkTypeRepository arg7
        ) : base()
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

        public readonly ITaskRepository field0;
        public readonly IHourRepository field1;
        public readonly IActivityRepository field2;
        public readonly IItemRepository field3;
        public readonly ITimeEntryRepository field4;
        public readonly ICaseRepository field5;
        public readonly ICaseWorkTypeRepository field6;
        public readonly IWorkTypeRepository field7;
    }


    public class TemporaryHourValidator : HourBaseValidator<TemporaryHourFields, TemporaryHour>
    {
        public TemporaryHourValidator(
            IContextService<IPsaContext> arg0,
            IUserRepository arg1,
            IOvertimeRepository arg2,
            IEmploymentRepository arg3,
            IWorkdayRepository arg4
        ) : base(arg0, arg1, arg2, arg3, arg4)
        {
        }
    }


    public class TemporaryItemValidator : Validator<TemporaryItem>
    {
        public TemporaryItemValidator(
            IContextService<IPsaContext> arg0,
            ITemporaryItemRepository arg1,
            IUserRepository arg2,
            IProductRepository arg3,
            IEmploymentRepository arg4,
            IWorkdayRepository arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITemporaryItemRepository field1;
        public readonly IUserRepository field2;
        public readonly IProductRepository field3;
        public readonly IEmploymentRepository field4;
        public readonly IWorkdayRepository field5;
    }


    public class TermsOfServiceApprovalValidator : Validator<TermsOfServiceApproval>
    {
        public TermsOfServiceApprovalValidator(
        )
        {
        }
    }


    public class TimeEntryTypeValidator : Validator<TimeEntryType>
    {
        public TimeEntryTypeValidator(
            ITimeEntryRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ITimeEntryRepository field0;
    }


    public class TimeEntryValidator : Validator<TimeEntry>
    {
        public TimeEntryValidator(
            ITaskService arg0,
            ICaseService arg1,
            ITimeEntryRepository arg2,
            ICaseMemberService arg3,
            ITaskMemberService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly ITaskService field0;
        public readonly ICaseService field1;
        public readonly ITimeEntryRepository field2;
        public readonly ICaseMemberService field3;
        public readonly ITaskMemberService field4;
    }


    public class TravelReimbursementValidator : Validator<TravelReimbursement>
    {
        public TravelReimbursementValidator(
            ITravelReimbursementRepository arg0,
            IContextService<IPsaContext> arg1,
            ITravelReimbursementStatusRepository arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ITravelReimbursementRepository field0;
        public readonly IContextService<IPsaContext> field1;
        public readonly ITravelReimbursementStatusRepository field2;
    }


    public class UserCostPerCaseValidator : Validator<UserCostPerCase>
    {
        public UserCostPerCaseValidator(
            IContextService<IPsaContext> arg0,
            IUserCostPerCaseRepository arg1,
            ICaseMemberRepository arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IUserCostPerCaseRepository field1;
        public readonly ICaseMemberRepository field2;
    }


    public class UserSettingsValidator : Validator<UserSettings>
    {
        public UserSettingsValidator(
            IContextService<IPsaContext> arg0,
            IUserSettingsRepository arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IUserSettingsRepository field1;
    }


    public class UserTaskFavoriteValidator : Validator<UserTaskFavorite>
    {
        public UserTaskFavoriteValidator(
            ITaskService arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ITaskService field0;
    }


    public class UserValidator : Validator<User>
    {
        public UserValidator(
            IContextService<IPsaContext> arg0,
            IUserRepository arg1,
            ICountryService arg2,
            ILanguageService arg3,
            ITimeZoneService arg4,
            ICountryRegionService arg5,
            IUserAuthorization arg6
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IUserRepository field1;
        public readonly ICountryService field2;
        public readonly ILanguageService field3;
        public readonly ITimeZoneService field4;
        public readonly ICountryRegionService field5;
        public readonly IUserAuthorization field6;
    }


    public class UserWeeklyViewRowValidator : Validator<UserWeeklyViewRow>
    {
        public UserWeeklyViewRowValidator(
            IUserWeeklyViewRowRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IUserWeeklyViewRowRepository field0;
    }


    public class WorkdayValidator : Validator<Workday>
    {
        public WorkdayValidator(
            IWorkdayRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IWorkdayRepository field0;
    }


    public class WorkHourValidator : HourBaseValidator<HourFields, Hour>
    {
        public WorkHourValidator(
            IContextService<IPsaContext> arg0,
            ICaseRepository arg1,
            ITaskRepository arg2,
            ICaseMemberRepository arg3,
            ITaskMemberRepository arg4,
            IWorkTypeRepository arg5,
            IUserRepository arg6,
            IWorkdayRepository arg7,
            IOvertimeRepository arg8,
            IEmploymentRepository arg9,
            IInvoiceStatusRepository arg10,
            IInvoiceCaseRepository arg11,
            IInvoiceRepository arg12,
            IWorkdayService arg13
        ) : base()
        {
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
            field9 = arg9;
            field10 = arg10;
            field11 = arg11;
            field12 = arg12;
            field13 = arg13;
        }

        public readonly IWorkTypeRepository field5;
        public readonly IUserRepository field6;
        public readonly IWorkdayRepository field7;
        public readonly IOvertimeRepository field8;
        public readonly IEmploymentRepository field9;
        public readonly IInvoiceStatusRepository field10;
        public readonly IInvoiceCaseRepository field11;
        public readonly IInvoiceRepository field12;
        public readonly IWorkdayService field13;
    }


    public class WorkingDayExceptionValidator : Validator<WorkingDayException>
    {
        public WorkingDayExceptionValidator(
            IWorkingDayExceptionRepository arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IWorkingDayExceptionRepository field0;
    }


    public class WorkPriceValidator : Validator<WorkPrice>
    {
        public WorkPriceValidator(
            IContextService<IPsaContext> arg0,
            IWorkPriceRepository arg1,
            ICaseRepository arg2,
            IPricelistRepository arg3,
            IPricelistVersionRepository arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IWorkPriceRepository field1;
        public readonly ICaseRepository field2;
        public readonly IPricelistRepository field3;
        public readonly IPricelistVersionRepository field4;
    }


    public class WorkTypeValidator : Validator<WorkType>
    {
        public WorkTypeValidator(
            IContextService<IPsaContext> arg0,
            IWorkTypeRepository arg1,
            IProductRepository arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IWorkTypeRepository field1;
        public readonly IProductRepository field2;
    }


    public partial class Logo
    {
    }


    public class PdfFileInfo
    {
        public PdfFileInfo(
        )
        {
        }
    }


    public class CategoryGroup
    {
        public CategoryGroup(
        )
        {
        }
    }


    public class ElementPosition
    {
        public ElementPosition(
        )
        {
        }
    }


    public class ExpenseComparer
        : IComparer
    {
        public ExpenseComparer(
            IPsaContext arg0,
            string arg1,
            CultureInfo arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IPsaContext field0;
        public readonly string field1;
        public readonly CultureInfo field2;
        public int Compare(object x, object y) => throw new NotImplementedException();
    }


    public class KpiFormulaContext
    {
    }


    public class CreateAccountGroupsParameterDelegate
    {
        public CreateAccountGroupsParameterDelegate(
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


    public class AuditTrailEntrySearchCriteria : SearchCriteriaBase
    {
        public AuditTrailEntrySearchCriteria(
        )
        {
        }
    }

    public abstract class PsaSearchDefinitionBase<TSearchCriteria> : SearchDefinitionBase<IPsaContext, TSearchCriteria> where TSearchCriteria : ISearchCriteria, new()
    {
        private readonly ICustomerDatabaseRepository _CustomerDatabaseRepository;

        public ICustomerDatabaseRepository CustomerDatabaseRepository
        {
            get
            {
                if (_CustomerDatabaseRepository == null) { }
                return _CustomerDatabaseRepository;
            }
        }

        public PsaSearchDefinitionBase(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, TSearchCriteria>> fields)
        {
        }
    }

    public abstract class PsaSearchDefinitionBase<TSearchCriteria, TEntity> : PsaSearchDefinitionBase<TSearchCriteria>, ISearchDefinition<IPsaContext, TSearchCriteria, TEntity>
        where TSearchCriteria : ISearchCriteria, new()
        where TEntity : IOrganizationEntity
    {
        public PsaSearchDefinitionBase(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, TSearchCriteria>> fields) : base(fields)
        {
        }

        public abstract SearchResponse<TEntity> GetEntities(IPsaContext context, ISearchRequest<TSearchCriteria> request);
    }



    public class AuditTrailSearchDefinition : PsaSearchDefinitionBase<AuditTrailEntrySearchCriteria, AuditTrailEntry>
    {
        public AuditTrailSearchDefinition(ICollection<ISqlSearchFieldRegisterEntry<IPsaContext, AuditTrailEntrySearchCriteria>> fields) : base(fields)
        {
        }

        public override SearchResponse<AuditTrailEntry> GetEntities(IPsaContext context, ISearchRequest<AuditTrailEntrySearchCriteria> request) => throw new NotImplementedException();
    }

    public partial class HourDrillDown : PsaDrillDownReportActionInfo<ICrossTabReport>
    {
        public HourDrillDown(
            string arg1,
            Phrase arg2,
            string arg3,
            bool? arg4,
            bool? arg5,
            bool arg6,
            string arg7
        ) : base()
        {
        }
    }


    public partial class HourDrillDown : PsaDrillDownReportActionInfo<ICrossTabReport>
    {
        public HourDrillDown(
            CaseTimelineReportHandler arg0,
            string arg1,
            bool? arg2,
            bool? arg3,
            bool arg4,
            string arg5,
            Phrase arg6,
            string arg7
        ) : base()
        {
            s = arg4;
            d = arg5;
            field6 = arg6;
            field7 = arg7;
        }

        public bool? s;
        public string d;
        public Phrase field6;
        public string field7;
    }


    public class InvoicingDrillDown : CaseDrillDownBase
    {
        public InvoicingDrillDown(
            CaseTimelineReportHandler arg0,
            string arg1,
            Phrase arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public partial class CaseDrillDownBase : PsaDrillDownReportActionInfo<ICrossTabReport>
    {
        public CaseDrillDownBase(
            CaseTimelineReportHandler arg0,
            string arg1,
            Phrase arg2
        ) : base()
        {
        }
    }


    public partial class HourDrillDown : PsaDrillDownReportActionInfo<ICrossTabReport>
    {
        public HourDrillDown(
            HourApprovalTimelineReportHandler arg0,
            string arg1,
            bool? arg2,
            bool? arg3,
            bool arg4,
            bool? arg5,
            string arg6,
            Phrase arg7,
            ICollection<Parameter> arg8,
            string arg9
        ) : base()
        {
            field9 = arg9;
        }

        public readonly string field6a;
        public readonly Phrase field7a;
        public readonly ICollection<Parameter> field8a;
        public readonly string field9;
    }


    public partial class HourDrillDown : PsaDrillDownReportActionInfo<ICrossTabReport>
    {
        public HourDrillDown(
            HourTimelineReportHandler arg0,
            string arg1,
            bool? arg2,
            bool? arg3,
            bool arg4,
            bool? arg5,
            string arg6,
            Phrase arg7,
            string arg8
        ) : base()
        {
        }
    }


    public class ItemDrillDown : PsaDrillDownReportActionInfo<ICrossTabReport>
    {
        public ItemDrillDown(
            ItemTimelineReportHandler arg0,
            bool? arg1,
            bool arg2,
            bool? arg3,
            string arg4,
            Phrase arg5,
            string arg6,
            ICollection<Parameter> arg7
        ) : base()
        {
            field6 = arg6;
            field7 = arg7;
        }

        public readonly string field6;
        public readonly ICollection<Parameter> field7;
    }


    public class ResourceAllocationDrillDown : PsaDrillDownReportActionInfo<ICrossTabReport>
    {
        public ResourceAllocationDrillDown(
            ResourceAllocationTimelineReportHandler arg0,
            string arg1,
            Phrase arg2
        ) : base()
        {
        }
    }


    public partial class HourDrillDown : PsaDrillDownReportActionInfo<ICrossTabReport>
    {
        public HourDrillDown(
            ResourceAllocationTimelineReportHandler arg0,
            string arg1,
            Phrase arg2
        ) : base()
        {
        }
    }


    public class ExpensesFromItemsDrillDown : UserDrillDownBase
    {
        public ExpensesFromItemsDrillDown(
            UserTimelineReportHandler arg0,
            string arg1,
            Phrase arg2
        ) : base()
        {
        }
    }


    public partial class HourDrillDown : PsaDrillDownReportActionInfo<ICrossTabReport>
    {
        public HourDrillDown(
            UserTimelineReportHandler arg0,
            string arg1,
            bool? arg2,
            bool? arg3,
            bool? arg4,
            bool arg5,
            string arg6,
            Phrase arg7,
            string arg8
        ) : base()
        {
            field8 = arg8;
        }

        public readonly string field8;
    }


    public abstract class UserDrillDownBase : PsaListDrillDownReportActionInfo
    {
        protected UserListReportHandler ReportHandler;

        public UserDrillDownBase(UserListReportHandler reportHandler, string identifier, Phrase titlePhrase, ICollection<string> requiredSearchFields = null, ICollection<Parameter> parameters = null) : base()
        {
            ReportHandler = reportHandler;
        }

        protected UserDrillDownBase() : base()
        {
        }
    }


    public class CreateCommunicationMethodParameterDelegate
    {
        public CreateCommunicationMethodParameterDelegate(
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


    public class AddCriteriaDelegate
    {
        public AddCriteriaDelegate(
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


    public class LinkListReportFieldHandler : LinkListReportFieldHandler<IPsaContext, TEntityAccessInfo>
    {
        public LinkListReportFieldHandler() { }

        public LinkListReportFieldHandler(
            string arg0,
            Phrase arg1,
            string arg2,
            string arg3,
            Phrase arg4,
            Phrase arg5,
            int? arg7,
            bool arg8
        ) : base()
        {
        }

        protected LinkListReportFieldHandler(string arg0, Phrase phrase, string s, string arg3, Phrase arg4, Phrase arg5, int? arg6)
        {
        }
    }

    public class TEntityAccessInfo
    {
    }

    public class LinkListReportFieldHandler<T1, T2>
    {
    }

    public abstract class LinkListReportFieldHandler<TContext, TEntityAccessInfo, TReportField> : ReportFieldHandler<TReportField>
    where TContext : IContext
    where TEntityAccessInfo : IEntityAccessInfo<TContext>
    where TReportField : IReportField
    {
        public delegate string GetValueDelegate(TEntityAccessInfo info);

        public readonly string SearchFieldName;
        private readonly GetValueDelegate _GetValue;
        private readonly string _UIType;

        public LinkListReportFieldHandler(string identifier, Phrase labelPhrase, string uiType, string searchFieldName = null, Phrase? shortLabelPhrase = null, Phrase groupPhrase = Phrase.Empty, GetValueDelegate getValue = null, int defaultWidth = 0, bool isSortable = true) : base()
        {
        }

        public LinkListReportFieldHandler(string identifier, GetTranslationDelegate getLabelDelegate, string uiType, string searchFieldName = null, GetTranslationDelegate getShortLabelDelegate = null, GetTranslationDelegate getGroupDelegate = null, GetValueDelegate getValue = null, int? defaultWidth = 0, bool isSortable = true) : base(identifier, getLabelDelegate, searchFieldName: searchFieldName, getShortLabelDelegate: getShortLabelDelegate, getGroupDelegate: getGroupDelegate, defaultWidth: defaultWidth, isSortable: isSortable)
        {
            _UIType = uiType;
        }

        protected LinkListReportFieldHandler()
        {
        }
    }

    public class NumericKpiReportFieldHandler : KpiReportFieldHandler
    {
        public NumericKpiReportFieldHandler(
            string arg0,
            Phrase arg1,
            CreateFormulaParameterDelegate arg2,
            GetTranslationDelegate arg3,
            Phrase arg4,
            Phrase arg5,
            string arg6,
            int? arg7,
            RowActionInfo arg8
        ) : base()
        {
        }
    }


    public class BaseCurrencyKpiReportFieldHandler : KpiReportFieldHandler
        , ICurrencyReportFieldHandler
    {
        public BaseCurrencyKpiReportFieldHandler(
            IContextService<IPsaContext> arg0,
            string arg1,
            Phrase arg2,
            CreateFormulaParameterDelegate arg3,
            GetTranslationDelegate arg4,
            Phrase arg5,
            Phrase arg6,
            int? arg7,
            RowActionInfo arg8,
            TotalCalculationType arg9
        ) : base()
        {
        }
    }


    public class KpiReportFieldHandlerOld : KpiReportFieldHandler
    {
        public KpiReportFieldHandlerOld(
            CustomFormula arg0,
            ICustomFormulaHandler arg1,
            IPsaContext arg2,
            string arg3
        ) : base()
        {
        }
    }


    public class KpiReportFieldOld : NumericReportField
    {
        public KpiReportFieldOld(
            int arg0,
            string arg1,
            int? arg2,
            Nullable<AggregateFunction> arg3,
            bool arg4
        ) : base()
        {
        }
    }


    public class IDListReportFilterHandlerOld : IDListReportFilterHandler
    {
        public IDListReportFilterHandlerOld(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            BindCriteriaDelegate<IDListReportFilter> arg3,
            BindCriteriaDelegate<IDListReportFilter> arg4,
            Phrase arg5,
            string arg6,
            string arg7,
            ICollection arg8,
            bool arg9,
            bool arg10
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10)
        {
        }

        public IDListReportFilterHandlerOld(
            string arg0,
            GetTranslationDelegate arg1,
            GetTranslationDelegate arg2,
            string arg3,
            ICollection arg4,
            BindCriteriaDelegate<IDListReportFilter> arg5,
            BindCriteriaDelegate<IDListReportFilter> arg6,
            string arg7,
            bool arg8,
            bool arg9
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9)
        {
        }

        protected IDListReportFilterHandlerOld()
        {
        }
    }


    public class SimpleListReportFilterOld : SimpleListReportFilter
    {
        public SimpleListReportFilterOld(
            string arg0,
            IEnumerable<Object> arg1
        ) : base()
        {
        }
    }


    public class SimpleListReportFilterHandlerOld : SimpleListReportFilterHandler
    {
        public SimpleListReportFilterHandlerOld(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            BindCriteriaDelegate<SimpleListReportFilter> arg3,
            BindCriteriaDelegate<SimpleListReportFilter> arg4,
            Phrase arg5,
            string arg6,
            string arg7,
            ICollection<Object> arg8,
            bool arg9
        ) : base()
        {
        }
    }


    public class AccountGroupIDReportFilterHandlerOld : IDListReportFilterHandlerOld
        , IAccountGroupIDReportFilterHandler
    {
        public AccountGroupIDReportFilterHandlerOld(
            int arg0,
            string arg1,
            GetTranslationDelegate arg2
        ) : base()
        {
        }
    }


    public class TimePeriodReportFilterOld : TimePeriodReportFilter
    {
        public TimePeriodReportFilterOld(
            string arg0,
            TimePeriod arg1,
            Nullable<AggregateFunction> arg2
        ) : base()
        {
        }
    }


    public class TimePeriodReportFilterHandlerOld : TimePeriodReportFilterHandler
    {
        public TimePeriodReportFilterHandlerOld(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            BindCriteriaDelegate<TimePeriodReportFilter> arg3,
            BindCriteriaDelegate<TimePeriodReportFilter> arg4,
            string arg5,
            Phrase arg6,
            string arg7,
            TimePeriod arg8,
            bool arg9
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9)
        {
        }
    }


    public class NumericRangeReportFilterHandlerOld : NumericRangeReportFilterHandler
    {
        public NumericRangeReportFilterHandlerOld(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            GetTranslationDelegate arg3,
            BindCriteriaDelegate<NumericRangeReportFilter> arg4,
            BindCriteriaDelegate<NumericRangeReportFilter> arg5,
            Phrase arg6,
            string arg7,
            string arg8,
            NumericRange arg9,
            bool arg10
        ) : base()
        {
        }

        public NumericRangeReportFilterHandlerOld(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            Phrase arg3,
            BindCriteriaDelegate<NumericRangeReportFilter> arg4,
            BindCriteriaDelegate<NumericRangeReportFilter> arg5,
            Phrase arg6,
            string arg7,
            string arg8,
            NumericRange arg9,
            bool arg10
        ) : base()
        {
        }
    }


    public class KpiReportFilterHandlerOld : KpiReportFilterHandler
    {
        public KpiReportFilterHandlerOld(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            CreateFormulaParameterDelegate arg3,
            Phrase arg4,
            string arg5,
            string arg6,
            NumericRange arg7
        ) : base()
        {
        }

        public KpiReportFilterHandlerOld(
            string arg0,
            GetTranslationDelegate arg1,
            GetTranslationDelegate arg2,
            CreateFormulaParameterDelegate arg3,
            GetTranslationDelegate arg4,
            string arg5,
            string arg6,
            NumericRange arg7
        ) : base()
        {
        }

        protected KpiReportFilterHandlerOld()
        {
        }
    }


    public class BaseCurrencyKpiReportFilterHandlerOld : KpiReportFilterHandlerOld
    {
        public BaseCurrencyKpiReportFilterHandlerOld(
            IContextService<IPsaContext> arg0,
            string arg1,
            Phrase arg2,
            Phrase arg3,
            CreateFormulaParameterDelegate arg4,
            GetTranslationDelegate arg5,
            string arg6,
            NumericRange arg7
        ) : base()
        {
        }
    }


    public class BaseCurrencyRangeReportFilterHandlerOld : BaseCurrencyRangeReportFilterHandler
    {
        public BaseCurrencyRangeReportFilterHandlerOld(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            GetTranslationDelegate arg3,
            string arg4,
            NumericRange arg5
        ) : base()
        {
        }
    }


    public class StringReportFilterOld : StringReportFilter
    {
        public StringReportFilterOld(
            string arg0,
            string arg1,
            Nullable<AggregateFunction> arg2
        ) : base(arg0, arg1)
        {
        }
    }


    public class StringReportFilterHandlerOld : StringReportFilterHandler
    {
        public StringReportFilterHandlerOld(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            BindCriteriaDelegate<StringReportFilter> arg3,
            BindCriteriaDelegate<StringReportFilter> arg4,
            Phrase arg5,
            string arg6,
            string arg7,
            bool arg8,
            SearchCriteriaComparison arg9
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9)
        {
        }
    }


    public class BooleanReportFilterOld : BooleanReportFilter
    {
        public BooleanReportFilterOld(
            string arg0,
            string arg1,
            string arg2,
            bool? arg3
        ) : base(arg0)
        {
            field2 = arg2;
            field3 = arg3;
        }

        public readonly string field2;
        public readonly bool? field3;
    }


    public class BooleanReportFilterHandlerOld : BooleanReportFilterHandler
    {
        public BooleanReportFilterHandlerOld(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            string arg3,
            string arg4,
            BindCriteriaDelegate<BooleanReportFilter> arg5,
            BindCriteriaDelegate<BooleanReportFilter> arg6,
            Phrase arg7,
            string arg8,
            bool? arg9,
            string arg10,
            bool arg11
        ) : base()
        {
            field10 = arg10;
            field11 = arg11;
        }

        public readonly string field10;
        public readonly bool field11;
    }


    public class SalesStatusReportFilterOld : SalesStatusReportFilter
    {
        public SalesStatusReportFilterOld(
            string arg0,
            IEnumerable<Int32> arg1,
            TimePeriod arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class SalesStatusReportFilterHandlerOld : SalesStatusReportFilterHandler
    {
        public SalesStatusReportFilterHandlerOld(
            string arg0,
            Phrase arg1,
            Phrase arg2,
            BindCriteriaDelegate<SalesStatusReportFilter> arg3,
            BindCriteriaDelegate<SalesStatusReportFilter> arg4,
            string arg5,
            string arg6,
            string arg7
        ) : base()
        {
        }
    }


    public class ZoomDrillDown : DrillDownReportActionInfo<IPsaContext, RepertoryChartReport>
    {
        public ZoomDrillDown(
            RepertoryChartReportHandler arg0,
            string arg1,
            Phrase arg2
        ) : base()
        {
        }
    }


    public class ChartDataFieldHandler
    {
        public ChartDataFieldHandler() { }

        public ChartDataFieldHandler(
            string arg0,
            GetTranslationDelegate arg1,
            RepertoryChartReportHandler arg2,
            string arg3,
            GetTranslationDelegate arg4,
            string arg5,
            CurrencyUsage arg6,
            ICurrencyService arg7,
            AggregateFunction arg8,
            bool arg9
        ) : base()
        {
        }

        public ChartDataFieldHandler(
            string arg0,
            Phrase arg1,
            RepertoryChartReportHandler arg2,
            string arg3,
            Phrase arg4,
            string arg5,
            CurrencyUsage arg6,
            ICurrencyService arg7,
            AggregateFunction arg8,
            bool arg9
        ) : base()
        {
        }
    }


    public class OverTimeCategoryHandler : ChartEntityCategoryListHandler<IPsaContext, RepertoryChartReport, OverTime>
    {
        public OverTimeCategoryHandler(
            IContextService<IPsaContext> arg0,
            string arg1,
            Phrase arg2,
            Phrase arg3
        ) : base()
        {
        }
    }


    public class UserCategoryHandler : ChartEntityCategoryListHandler<IPsaContext, RepertoryChartReport, User>
    {
        public UserCategoryHandler(
            IContextService<IPsaContext> arg0,
            string arg1,
            Phrase arg2,
            Phrase arg3
        ) : base()
        {
        }
    }


    public class WorkHourSeries : ChartValueSeriesHandler<IPsaContext, RepertoryChartReport, HourSearchCriteria>
    {
        public WorkHourSeries(
            IContextService<IPsaContext> arg0,
            RepertoryChartReportHandler arg1
        ) : base()
        {
        }
    }


    public class WorkTypeCategoryHandler : ChartEntityCategoryListHandler<IPsaContext, RepertoryChartReport, WorkType>
    {
        public WorkTypeCategoryHandler(
            IContextService<IPsaContext> arg0,
            string arg1,
            Phrase arg2,
            Phrase arg3
        ) : base()
        {
        }
    }


    public class Employment : EmploymentFields, IIdentifiableEntityWithOriginalState<EmploymentFields>
    {
        public Employment(
        )
        {
        }
    }


    public class HourValue
    {
        public HourValue(
        )
        {
        }
    }


    public partial class HourDrillDown<TReport> : PsaDrillDownReportActionInfo<ICrossTabReport> where TReport : IChartReport
    {
        public HourDrillDown(
            ResourceAllocationOverviewGraphReportHandlerBase<TReport> arg0,
            string arg1,
            Phrase arg2,
            string arg3,
            bool? arg4,
            bool? arg5,
            bool? arg6,
            bool arg7
        ) : base()
        {
            field5 = arg5;
        }

        public readonly bool? field5;
    }


    public class ResourceAllocationDrillDown<TReport> : DrillDownReportActionInfo where TReport : IChartReport
    {
        public ResourceAllocationDrillDown(
            ResourceAllocationOverviewGraphReportHandlerBase<TReport> arg0,
            string arg1,
            Phrase arg2,
            bool arg3
        ) : base()
        {
        }
    }


    public class ActivitiesDrillDown<TReport> : DrillDownReportActionInfo where TReport : IChartReport
    {
        public ActivitiesDrillDown(
            ResourceAllocationOverviewGraphReportHandlerBase<TReport> arg0,
            string arg1,
            Phrase arg2,
            string arg3,
            bool? arg4,
            bool arg5,
            bool arg6
        ) : base()
        {
            field5 = arg5;
            field6 = arg6;
        }

        public readonly bool field5;
        public readonly bool field6;
    }


    public class ZoomDrillDown<TReport> : DrillDownReportActionInfo where TReport : IChartReport
    {
        public ZoomDrillDown(
            ResourceAllocationOverviewGraphReportHandlerBase<TReport> arg0,
            string arg1,
            Phrase arg2
        ) : base()
        {
        }
    }


    public class InfoReportFieldHandler : StringReportFieldHandler
    {
        public InfoReportFieldHandler(
            string arg0,
            Phrase arg1,
            GetDataDelegate arg2,
            Phrase arg3,
            RowActionInfo arg4,
            bool arg5,
            string arg6
        )
        {
        }
    }


    public class ReportField
    {
        public ReportField(
        )
        {
        }
    }


    public class ChartSeries
    {
        public ChartSeries(
        )
        {
        }
    }


    public class Response
    {
        public Response(
        )
        {
        }
    }


    public class CaseDrillDown : PsaDrillDownReportActionInfo<TimelineGraphReport>
    {
        public CaseDrillDown(
            TimelineGraphReportHandler arg0,
            string arg1
        ) : base()
        {
        }
    }


    public partial class HourDrillDown : PsaDrillDownReportActionInfo<ICrossTabReport>
    {
        public HourDrillDown(
            UserListReportHandler arg0,
            string arg1,
            Phrase arg2,
            string arg3,
            bool? arg4,
            bool? arg5,
            bool? arg6
        ) : base()
        {
            a = arg4;
            b = arg5;
        }

        public bool? a;
        public bool? b;
    }


    public class EntryTypeDrillDown : UserDrillDownBase
    {
        public EntryTypeDrillDown(
            UserListReportHandler arg0,
            string arg1,
            string arg2
        ) : base()
        {
        }
    }


    public class TimeEntryDrillDown : UserDrillDownBase
    {
        public TimeEntryDrillDown(
            UserListReportHandler arg0,
            string arg1
        ) : base()
        {
        }
    }


    public class OutOfOfficeDrillDown : UserDrillDownBase
    {
        public OutOfOfficeDrillDown(
            UserListReportHandler arg0,
            string arg1
        ) : base()
        {
        }
    }


    public class EntityListDataFieldGroup<TEntity> : PsaEntityListDataFieldGroup<TEntity>
        , IReportDataFieldGroupWithTimePeriod where TEntity : IIdentifiableEntity
    {
        public EntityListDataFieldGroup(
            IGuidService arg0,
            ICrossTabReport arg1,
            TimePeriod arg2,
            string arg3,
            string arg4,
            string arg5,
            bool arg7,
            bool arg8,
            bool arg9,
            string arg10
        ) : base()
        {
        }

        public EntityListDataFieldGroup(
            IGuidService arg0,
            ICrossTabReport arg1,
            XElement arg2,
            string arg3,
            string arg4,
            string arg5,
            bool arg7,
            string arg8
        ) : base()
        {
        }
    }

    public abstract class EntityListDataFieldGroup<TContext, TSearchCriteria, TEntity> : IMatrixReportDataFieldGroup
    where TContext : IContext
    where TEntity : IIdentifiableEntity
    {
        public readonly string IdSearchField;
        public readonly string EntityIdSearchField;
        public readonly string EntityNameSearchField;
        protected readonly AddCriteriaDelegate AddCriteria;
        private ICollection<SearchResponseRow> _Entities;
        public readonly bool ForMatrix;
        private bool _AddTotal;
        protected readonly IGuidService GuidService;
        protected readonly ICrossTabReport Report;
        private bool _IsCumulative;
        private string _NodeName;
        private string _XAxis;

        public delegate void AddCriteriaDelegate(TSearchCriteria criteria);

        public EntityListDataFieldGroup(IGuidService guidService, ICrossTabReport report, string idSearchField = null, string entityIdSearchField = "ID", string entityNameSearchField = "Name", AddCriteriaDelegate addCriteria = null, bool forMatrix = true, bool addTotal = true, bool isCumulative = false, string xAxis = null, string nodeName = null)
        {
            GuidService = guidService;
            Report = report;
            if (string.IsNullOrEmpty(idSearchField))
                IdSearchField = string.Format("{0}ID", typeof(TEntity).Name);
            else
                IdSearchField = idSearchField;
            if (string.IsNullOrEmpty(xAxis))
                _XAxis = typeof(TEntity).Name;
            else
                _XAxis = xAxis;
            if (string.IsNullOrEmpty(nodeName))
                _NodeName = typeof(TEntity).Name;
            else
                _NodeName = nodeName;
            EntityIdSearchField = entityIdSearchField;
            EntityNameSearchField = entityNameSearchField;
            AddCriteria = addCriteria;
            ForMatrix = forMatrix;
            _AddTotal = addTotal;
            _IsCumulative = isCumulative;
        }

        public EntityListDataFieldGroup(IGuidService guidService, ICrossTabReport report, XElement xml, string idSearchField = null, ICollection<SearchRequestCriteriaField> criterias = null, string entityIdSearchField = "ID", string entityNameSearchField = "Name", AddCriteriaDelegate addCriteria = null, bool forMatrix = true, string xAxis = null, string nodeName = null)
        {
            GuidService = guidService;
            Report = report;
            if (string.IsNullOrEmpty(idSearchField))
                IdSearchField = string.Format("{0}ID", typeof(TEntity).Name);
            else
                IdSearchField = idSearchField;
            if (string.IsNullOrEmpty(xAxis))
                _XAxis = typeof(TEntity).Name;
            else
                _XAxis = xAxis;
            if (string.IsNullOrEmpty(nodeName))
                _NodeName = typeof(TEntity).Name;
            else
                _NodeName = nodeName;
            EntityIdSearchField = entityIdSearchField;
            EntityNameSearchField = entityNameSearchField;
            AddCriteria = addCriteria;
            ForMatrix = forMatrix;
        }

        protected EntityListDataFieldGroup()
        {
        }

        public bool IsCumulative => _IsCumulative;

        public bool AddTotal => _AddTotal;

        public string XAxis => _XAxis;

        public string XAxisSearchField => IdSearchField;

        public string Identifier => _XAxis;
    }


    public class CaseIdGroupReportFieldHandler : IdGroupReportFieldHandler
    {
        public CaseIdGroupReportFieldHandler(
            RowFieldActionInfo<IPsaContext, WorkHourMatrixReport> arg0
        ) : base(arg0)
        {
        }
    }

    public class RowFieldActionInfo<T, T1>
    {
    }


    public partial class HourDrillDown<TReport> : PsaDrillDownReportActionInfo<ICrossTabReport> where TReport : IChartReport
    {
        public HourDrillDown(
            WorkHourMatrixReportHandlerBase<TReport> arg0,
            string arg1,
            bool? arg2,
            bool? arg3,
            bool arg4,
            bool? arg5,
            string arg6,
            Phrase arg7,
            string arg8
        ) : base()
        {
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
        }

        public readonly string field6;
        public readonly Phrase field7;
        public readonly string field8;
    }

    public class AddOns
    {
        public AddOns(
        )
        {
        }
    }


    public class UsersHours
    {
        public UsersHours(
        )
        {
        }
    }


    public class DateHours
    {
        public DateHours(
        )
        {
        }
    }


    public partial class Logo
    {
    }


    public class AccountGroupInfo
    {
        public AccountGroupInfo(
        )
        {
        }
    }


    public class InvoiceRowHour
    {
        public InvoiceRowHour(
        )
        {
        }

        public InvoiceRowHour(
            string arg0,
            string arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly string field1;
    }


    public class InvoiceRowItem
    {
        public InvoiceRowItem(
        )
        {
        }

        public InvoiceRowItem(
            string arg0,
            string arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly string field1;
    }


    public class InvoiceGrouping
    {
    }


    public class InvoiceItemType
    {
    }


    public class InvoiceRowCategory
    {
    }


    public class CreateInvoice
    {
        public CreateInvoice(
        )
        {
        }
    }


    public class PrintToPdf
    {
    }


    public class ReferenceNumberCalculationType
    {
    }


    public class ResourceAllocationAction
    {
    }


    public class WorkloadHelper
    {
        public WorkloadHelper(
        )
        {
        }
    }


    public class Ownership
    {
        public Ownership(
        )
        {
        }
    }


    public class ContextName
    {
        public ContextName(
        )
        {
        }
    }


    public class GroupByType
    {
    }


    public partial class RequiredIfFinancialsAddOnEnabledValidationRule : RequiredValidationRule<Product>
    {
        public RequiredIfFinancialsAddOnEnabledValidationRule(
            string arg0,
            ITranslationEntry arg1,
            IContextService<IPsaContext> arg2
        ) : base(arg0, arg1)
        {
            field2 = arg2;
        }

        public readonly IContextService<IPsaContext> field2;
    }


    public class WorkingDayExceptionDateComparer
        : IComparer
    {
        public WorkingDayExceptionDateComparer(
        )
        {
        }

        public int Compare(object x, object y) => throw new NotImplementedException();
    }


    public class WorkingDayExceptionComparerImpl
        : IComparer
    {
        public WorkingDayExceptionComparerImpl(
        )
        {
        }

        public int Compare(object x, object y) => throw new NotImplementedException();
    }


    public class ActivityDrillDown : DrillDownReportActionInfo<IPsaContext, RepertoryChartReport>
    {
        public ActivityDrillDown(
            RepertoryChartReportHandler arg0,
            string arg1,
            Phrase arg2,
            bool arg3
        ) : base()
        {
        }
    }


    public class Reader
        : IEntityReader
    {
        public Reader(
            string arg0
        )
        {
            field0 = arg0;
        }

        public readonly string field0;
    }


    public class GetDataDelegate
    {
        public GetDataDelegate(
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
}