using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using Conn.Adapter;
using Conn.Service;
using RM;
using Data;
using Entities;
using Financials;
using Framework;
using Integrations;
using Logic;
using OrganizationBase;
using Organizations;
using ScheduledWork;
using Search;
using Shared;
using Pdf;
using Users;
using Users.Repositories;
using AccessRights = Entities.AccessRights;
using Activity = Entities.Activity;
using IOrganizationAddonService = Shop.IOrganizationAddonService;
using Item = Entities.Item;
using IUserService = Logic.IUserService;
using Task = Entities.Task;
using TimeZone = Shared.TimeZone;

namespace Web.Rest.API
{
    public class MyProject
    {
    }


    public class InternalXmlHelper
    {
    }


    public class RuntimeEnvironment
    {
    }


    public class SwaggerConfig
    {
        public SwaggerConfig(
        )
        {
        }
    }


    public class WebApiConfig
    {
    }


    public class RestApiIocModule
    {
    }

    public abstract class PsaApiControllerBase<TModel, TSearchCriteria> : ApiControllerBase
        where TModel : class, IEntity, new()
        where TSearchCriteria : SearchCriteriaBase
    {
        public PsaApiControllerBase()
        {
        }

        public PsaApiControllerBase(IContextService<IPsaContext> contextService,
            IRestSettingsService restSettingsService) : base(contextService)
        {
            _RestSettingsService = restSettingsService;
        }

        private IRestSettingsService _RestSettingsService;
    }


    public class ChallengeResult
        : IHttpActionResult
    {
        public ChallengeResult(
            string arg0,
            ApiController arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly ApiController field1;

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }


    public class ExternalLoginData
    {
        public ExternalLoginData(
        )
        {
        }
    }


    public abstract class ActivitiesControllerBase : PsaApiControllerBase<ActivityModel, ActivitySearchCriteria>
    {
        public ActivitiesControllerBase(
            IPsaContextService arg0,
            IRestSettingsService arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public abstract class ScheduledJobsController<TModel, T> : PsaApiControllerBase<TModel, CommonSearchCriteria>
        where TModel : ScheduledJobModel, new()
    {
        protected readonly IModelConverter<BackgroundTask> _ModelConverter;
        protected readonly IGuidService _GuidService;
        protected readonly IBackgroundTaskService _BackgroundTaskService;
        protected readonly IAuthorization<IPsaContext, BackgroundTask> _Authorization;

        public ScheduledJobsController(IContextService<IPsaContext> contextService,
            IRestSettingsService restSettingsService, IModelConverter<BackgroundTask> modelConverter,
            IGuidService guidService, IBackgroundTaskService backgroundTaskService,
            IAuthorization<IPsaContext, BackgroundTask> authorization) : base(contextService, restSettingsService)
        {
            _ModelConverter = modelConverter;
            _GuidService = guidService;
            _BackgroundTaskService = backgroundTaskService;
            _Authorization = authorization;
        }
    }


    public class UserLicensesController : ApiControllerBase
    {
        public UserLicensesController(
            IPsaContextService arg0,
            IUserLicensesService arg1,
            IUserLicenseModelConverter arg2,
            IOrderConfirmationModelConverter arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IUserLicensesService field1;
        public readonly IUserLicenseModelConverter field2;
        public readonly IOrderConfirmationModelConverter field3;
    }


    public class TimeEntrySuggestedRowsController : ApiControllerBase
    {
        public TimeEntrySuggestedRowsController(
            IPsaContextService arg0,
            ITimeEntrySuggestedRowModelConverter arg1,
            IGuidService arg2,
            IRestSettingsService arg3,
            ITimeEntrySuggestedRowService arg4,
            IUserService arg5,
            IUserWeeklyViewRowService arg6
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

        public readonly IPsaContextService field0;
        public readonly ITimeEntrySuggestedRowModelConverter field1;
        public readonly IGuidService field2;
        public readonly IRestSettingsService field3;
        public readonly ITimeEntrySuggestedRowService field4;
        public readonly IUserService field5;
        public readonly IUserWeeklyViewRowService field6;
    }


    public class WorkHourSuggestedRowsController : ApiControllerBase
    {
        public WorkHourSuggestedRowsController(
            IPsaContextService arg0,
            IWorkHourSuggestedRowModelConverter arg1,
            IGuidService arg2,
            IRestSettingsService arg3,
            IWorkHourSuggestedRowService arg4,
            IUserService arg5,
            IUserWeeklyViewRowService arg6
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

        public readonly IPsaContextService field0;
        public readonly IWorkHourSuggestedRowModelConverter field1;
        public readonly IGuidService field2;
        public readonly IRestSettingsService field3;
        public readonly IWorkHourSuggestedRowService field4;
        public readonly IUserService field5;
        public readonly IUserWeeklyViewRowService field6;
    }


    public class KpiFormulasController : PsaApiControllerBase<KpiFormulaModel, CommonSearchCriteria>
    {
        public KpiFormulasController(
            IPsaContextService arg0,
            IRestSettingsService arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class UserSettingsController : PsaApiControllerBase<UserSettingsModel, CommonSearchCriteria>
    {
        public UserSettingsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IUserSettingsService arg2,
            IUserSettingsModelConverter arg3,
            IGuidService arg4,
            IGlobalSettingsService arg5
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IUserSettingsService field2;
        public readonly IUserSettingsModelConverter field3;
        public readonly IGuidService field4;
        public readonly IGlobalSettingsService field5;
    }


    public class SecurityController : ApiControllerBase
    {
        public SecurityController(IContextService<IPsaContext> contextService) : base(contextService)
        {
        }
    }


    public class SoapApiKeysController : ApiControllerBase
    {
        public SoapApiKeysController(
            IPsaContextService arg0,
            IMasterUserRepository arg1,
            IPsaUserService arg2,
            ISoapApiKeyModelConverter arg3,
            IGuidService arg4
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IMasterUserRepository field1;
        public readonly IPsaUserService field2;
        public readonly ISoapApiKeyModelConverter field3;
        public readonly IGuidService field4;
    }


    public class NotificationSettingsController : ApiControllerBase
    {
        public NotificationSettingsController(
            IPsaContextService arg0,
            INotificationSettingsModelConverter arg1,
            IGuidService arg2,
            IUserService arg3
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly INotificationSettingsModelConverter field1;
        public readonly IGuidService field2;
        public readonly IUserService field3;
    }


    public class FlextimeAdjustmentsController : PsaApiControllerBase<FlextimeAdjustmentModel, CommonSearchCriteria>
    {
        public FlextimeAdjustmentsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IModelConverter<Workday> arg2,
            IGuidService arg3,
            IFlextimeAdjustmentService arg4
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<Workday> field2;
        public readonly IGuidService field3;
        public readonly IFlextimeAdjustmentService field4;
    }


    public class AuthorizedIpAddressesController : PsaApiControllerBase<AuthorizedIpAddressModel, CommonSearchCriteria>
    {
        public AuthorizedIpAddressesController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IModelConverter<AuthorizedIPAddress> arg2,
            IGuidService arg3,
            IAuthorizedIPAddressService arg4
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<AuthorizedIPAddress> field2;
        public readonly IGuidService field3;
        public readonly IAuthorizedIPAddressService field4;
    }


    public class BankAccountsController : PsaApiControllerBase<BankAccountModel, CommonSearchCriteria>
    {
        public BankAccountsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IModelConverter<BankAccount> arg2,
            IGuidService arg3,
            IBankAccountService arg4
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<BankAccount> field2;
        public readonly IGuidService field3;
        public readonly IBankAccountService field4;
    }


    public class ApiClientsController : PsaApiControllerBase<ApiClientModel, CommonSearchCriteria>
    {
        public ApiClientsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IApiClientService arg2,
            IModelConverter<ApiClient> arg3,
            IModelConverter<ApiClient> arg4,
            IGuidService arg5,
            IApiClientHandlerService arg6,
            IFeatureToggleService arg7
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
        }

        public readonly IApiClientService field2;
        public readonly IModelConverter<ApiClient> field3;
        public readonly IModelConverter<ApiClient> field4;
        public readonly IGuidService field5;
        public readonly IApiClientHandlerService field6;
        public readonly IFeatureToggleService field7;
    }


    public class OrganizationSettingsController : PsaApiControllerBase<OrganizationSettingsModel, CommonSearchCriteria>
    {
        public OrganizationSettingsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IOrganizationSettingsModelConverter arg2,
            IPsaOrganizationService arg3,
            ICaseService arg4
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IOrganizationSettingsModelConverter field2;
        public readonly IPsaOrganizationService field3;
        public readonly ICaseService field4;
    }


    public class LogosController : PsaApiControllerBase<LogoModel, BusinessUnitSearchCriteria>
    {
        public LogosController(
            IPsaContextService arg0,
            IFileService arg1,
            ILogoFileService arg2,
            IFileModelConverter arg3,
            IGuidService arg4,
            IFileDataService arg5,
            IRestSettingsService arg6,
            IRequestDataReaderService arg7,
            IOrganizationTrustedService arg8,
            ICurrentSessionService arg9,
            IOrganizationCompanyService arg10,
            IPsaOrganizationService arg11,
            IBusinessUnitService arg12
        )
        {
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

        public readonly ILogoFileService field2;
        public readonly IFileModelConverter field3;
        public readonly IGuidService field4;
        public readonly IFileDataService field5;
        public readonly IRestSettingsService field6;
        public readonly IRequestDataReaderService field7;
        public readonly IOrganizationTrustedService field8;
        public readonly ICurrentSessionService field9;
        public readonly IOrganizationCompanyService field10;
        public readonly IPsaOrganizationService field11;
        public readonly IBusinessUnitService field12;
    }

    public class
        ProjectMemberCostExceptionsController : PsaApiControllerBase<ProjectMemberCostExceptionModel,
            CommonSearchCriteria>
    {
        public ProjectMemberCostExceptionsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IUserCostPerCaseService arg2,
            IModelConverter<UserCostPerCase> arg3,
            IGuidService arg4
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IUserCostPerCaseService field2;
        public readonly IModelConverter<UserCostPerCase> field3;
        public readonly IGuidService field4;
    }


    public class OrganizationDetailsController : PsaApiControllerBase<OrganizationDetailsModel, CommonSearchCriteria>
    {
        public OrganizationDetailsController(
            IPsaContextService arg0,
            IGuidService arg1,
            IRestSettingsService arg2,
            IOrganizationDetailsModelConverter arg3,
            IPsaOrganizationService arg4,
            IAddressService arg5,
            IAccountService arg6,
            IOrganizationDetailsService arg7
        ) : base(arg0, arg2)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
        }

        public readonly IRestSettingsService field2;
        public readonly IOrganizationDetailsModelConverter field3;
        public readonly IPsaOrganizationService field4;
        public readonly IAddressService field5;
        public readonly IAccountService field6;
        public readonly IOrganizationDetailsService field7;
    }


    public class AddonsController : ApiControllerBase
    {
        public AddonsController(
            IPsaContextService arg0,
            IOrganizationAddonService arg1,
            IAddonModelConverter arg2,
            IAddonIdentifierConverter arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IOrganizationAddonService field1;
        public readonly IAddonModelConverter field2;
        public readonly IAddonIdentifierConverter field3;
    }


    public class TravelExpenseCountrySettingsController : ApiControllerBase
    {
        public TravelExpenseCountrySettingsController(
            IPsaContextService arg0,
            IProductCountrySettingsModelConverter arg1,
            IGuidService arg2,
            ICountryProductService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IProductCountrySettingsModelConverter field1;
        public readonly IGuidService field2;
        public readonly ICountryProductService field3;
    }


    public class ProductCountrySettingsController : ApiControllerBase
    {
        public ProductCountrySettingsController(
            IPsaContextService arg0,
            IProductCountrySettingsModelConverter arg1,
            IGuidService arg2,
            ICountryProductService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IProductCountrySettingsModelConverter field1;
        public readonly IGuidService field2;
        public readonly ICountryProductService field3;
    }

    public class CustomerCountrySettingsController : ApiControllerBase
    {
        public CustomerCountrySettingsController(
            IPsaContextService arg0,
            IModelConverter<AccountCountrySettings> arg1,
            IGuidService arg2,
            IAccountCountrySettingsService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IModelConverter<AccountCountrySettings> field1;
        public readonly IGuidService field2;
        public readonly IAccountCountrySettingsService field3;
    }


    public class KpiComparisonController : ApiControllerBase
    {
        public KpiComparisonController(
            IPsaContextService arg0,
            IKpiComparisonModelConverter arg1,
            IKpiComparisonRepository arg2,
            IRegistrationRepository arg3,
            IUserSeatService arg4
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IKpiComparisonModelConverter field1;
        public readonly IKpiComparisonRepository field2;
        public readonly IRegistrationRepository field3;
        public readonly IUserSeatService field4;
    }


    public class TravelExpenseReceiptsController : PsaApiControllerBase<ProjectTravelExpenseModel, ItemSearchCriteria>
    {
        public TravelExpenseReceiptsController(
            IPsaContextService arg0,
            ITravelExpenseReceiptModelConverter arg1,
            IGuidService arg2,
            ITravelExpenseReceiptService arg3,
            IRestSettingsService arg4,
            IFileModelConverter arg5,
            IItemFileService arg6,
            IProjectTravelExpenseModelConverter arg7
        ) : base(arg0, arg4)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
        }

        public readonly IGuidService field2;
        public readonly ITravelExpenseReceiptService field3;
        public readonly IRestSettingsService field4;
        public readonly IFileModelConverter field5;
        public readonly IItemFileService field6;
        public readonly IProjectTravelExpenseModelConverter field7;
    }


    public class
        TravelReimbursementStatusController : PsaApiControllerBase<TravelReimbursementStatusModel, CommonSearchCriteria>
    {
        public TravelReimbursementStatusController(
            IPsaContextService arg0,
            ITravelReimbursementStatusService arg1,
            IModelConverter<TravelReimbursementStatus> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base(arg0, arg4)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<TravelReimbursementStatus> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class
        TravelReimbursementsController : PsaApiControllerBase<TravelReimbursementModel,
            TravelReimbursementSearchCriteria>
    {
        public TravelReimbursementsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            ITravelReimbursementService arg2,
            ITravelReimbursementModelConverter arg3,
            IGuidService arg4
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly ITravelReimbursementService field2;
        public readonly ITravelReimbursementModelConverter field3;
        public readonly IGuidService field4;
    }


    public class ResourcingOverviewController : ApiControllerBase
    {
        public ResourcingOverviewController(
            IPsaContextService arg0,
            IResourcingOverviewModelConverter arg1,
            IGuidService arg2,
            IResourceAllocationService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IResourcingOverviewModelConverter field1;
        public readonly IGuidService field2;
        public readonly IResourceAllocationService field3;
    }


    public class
        TermsOfServiceApprovalsController : PsaApiControllerBase<TermsOfServiceApprovalModel, CommonSearchCriteria>
    {
        public TermsOfServiceApprovalsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IModelConverter<TermsOfServiceApproval> arg2,
            IGuidService arg3,
            ITermsOfServiceApprovalService arg4
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<TermsOfServiceApproval> field2;
        public readonly IGuidService field3;
        public readonly ITermsOfServiceApprovalService field4;
    }


    public class FinancialsController : ApiControllerBase
    {
        public FinancialsController(
            IPsaContextService arg0,
            IFinancialsIntegrationHandlerService arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IPsaContextService field0;
        public readonly IFinancialsIntegrationHandlerService field1;
    }


    public class MemberType
    {
    }


    public class CalendarGroupMembersController : ApiControllerBase
    {
        public CalendarGroupMembersController(
            IPsaContextService arg0,
            ISearchCriteriaService arg1,
            ISearchService arg2,
            ICalendarGroupMemberModelConverter arg3,
            IGuidService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly ISearchCriteriaService field1;
        public readonly ISearchService field2;
        public readonly ICalendarGroupMemberModelConverter field3;
        public readonly IGuidService field4;
    }


    public class ActivityParticipantsController : ActivitiesControllerBase
    {
        public ActivityParticipantsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IActivityService arg2,
            IActivityParticipantModelConverter arg3,
            IGuidService arg4,
            IActivityUserMemberService arg5,
            IUserService arg6,
            IActivityContactMemberService arg7,
            IActivityResourceMemberService arg8,
            ICalendarSyncActivityNonAppParticipantService arg9
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
            field9 = arg9;
        }

        public readonly IActivityService field2;
        public readonly IActivityParticipantModelConverter field3;
        public readonly IGuidService field4;
        public readonly IActivityUserMemberService field5;
        public readonly IUserService field6;
        public readonly IActivityContactMemberService field7;
        public readonly IActivityResourceMemberService field8;
        public readonly ICalendarSyncActivityNonAppParticipantService field9;
    }


    public class ActivitiesController : ActivitiesControllerBase
    {
        public ActivitiesController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IActivityService arg2,
            IModelConverter<Activity> arg3,
            IActivityRecurrenceModelConverter arg4,
            IGuidService arg5,
            ICalendarSyncDispatcherService arg6,
            IWorkdayHandlerService arg7,
            IActivityStatusService arg8
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
        }

        public readonly IActivityService field2;
        public readonly IModelConverter<Activity> field3;
        public readonly IActivityRecurrenceModelConverter field4;
        public readonly IGuidService field5;
        public readonly ICalendarSyncDispatcherService field6;
        public readonly IWorkdayHandlerService field7;
        public readonly IActivityStatusService field8;
    }


    public class PermissionProfilesController : PsaApiControllerBase<AccessRightProfileModel, CommonSearchCriteria>
    {
        public PermissionProfilesController(
            IPsaContextService arg0,
            IProfileService arg1,
            IModelConverter<Profile> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base(arg0, arg4)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<Profile> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class CalendarGroupsController : PsaApiControllerBase<CalendarGroupModel, CommonSearchCriteria>
    {
        public CalendarGroupsController(
            IPsaContextService arg0,
            IGuidService arg1,
            ISearchService arg2,
            ISearchCriteriaService arg3,
            ICalendarGroupModelConverter arg4,
            IRestSettingsService arg5,
            ICalendarGroupMemberModelConverter arg6
        ) : base(arg0, arg5)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly ISearchService field2;
        public readonly ISearchCriteriaService field3;
        public readonly ICalendarGroupModelConverter field4;
        public readonly IRestSettingsService field5;
        public readonly ICalendarGroupMemberModelConverter field6;
    }


    public class ResourcesController : PsaApiControllerBase<ResourceModel, CommonSearchCriteria>
    {
        public ResourcesController(
            IPsaContextService arg0,
            IGuidService arg1,
            IResourceService arg2,
            IModelConverter<Resource> arg3,
            IRestSettingsService arg4
        ) : base(arg0, arg4)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IResourceService field2;
        public readonly IModelConverter<Resource> field3;
        public readonly IRestSettingsService field4;
    }


    public class DemoDataController : ApiControllerBase
    {
        public DemoDataController(
            IDemoDataHandlerService arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IDemoDataHandlerService field0;
    }


    public class LinksController : ApiControllerBase
    {
        public LinksController(
            IPsaContextService arg0,
            ILinkService arg1,
            IModelConverter<Link> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly ILinkService field1;
        public readonly IModelConverter<Link> field2;
        public readonly IGuidService field3;
    }


    public class ReimbursedWorkHoursController : ApiControllerBase
    {
        public ReimbursedWorkHoursController(
            IPsaContextService arg0,
            IReimbursedHourService arg1,
            IModelConverter<HourForInvoice> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IReimbursedHourService field1;
        public readonly IModelConverter<HourForInvoice> field2;
        public readonly IGuidService field3;
    }


    public class ReimbursedProjectTravelExpensesController : ApiControllerBase
    {
        public ReimbursedProjectTravelExpensesController(
            IPsaContextService arg0,
            IReimbursedItemService arg1,
            IModelConverter<ItemForInvoice> arg2,
            IGuidService arg3,
            IProductService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IReimbursedItemService field1;
        public readonly IModelConverter<ItemForInvoice> field2;
        public readonly IGuidService field3;
        public readonly IProductService field4;
    }


    public class ReimbursedProjectFeesController : ApiControllerBase
    {
        public ReimbursedProjectFeesController(
            IPsaContextService arg0,
            IReimbursedItemService arg1,
            IModelConverter<ItemForInvoice> arg2,
            IGuidService arg3,
            IProductService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IReimbursedItemService field1;
        public readonly IModelConverter<ItemForInvoice> field2;
        public readonly IGuidService field3;
        public readonly IProductService field4;
    }


    public class ProjectsOverviewController : ApiControllerBase
    {
        public ProjectsOverviewController(
            IPsaContextService arg0,
            IProjectsOverviewModelConverter arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IPsaContextService field0;
        public readonly IProjectsOverviewModelConverter field1;
    }


    public class ContactRolesController : PsaApiControllerBase<ContactRoleModel, CommonSearchCriteria>
    {
        public ContactRolesController(
            IPsaContextService arg0,
            IContactRoleService arg1,
            IModelConverter<ContactRole> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base(arg0, arg4)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<ContactRole> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class CustomerMarketSegmentsController : ApiControllerBase
    {
        public CustomerMarketSegmentsController(
            IPsaContextService arg0,
            IAccountGroupMemberService arg1,
            IModelConverter<AccountGroupMemberEx> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IAccountGroupMemberService field1;
        public readonly IModelConverter<AccountGroupMemberEx> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class ProjectTotalFeesController : ApiControllerBase
    {
        public ProjectTotalFeesController(
            IPsaContextService arg0,
            IProjectTotalFeeModelConverter arg1,
            IGuidService arg2
        ) : base(arg0)
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IPsaContextService field0;
        public readonly IProjectTotalFeeModelConverter field1;
        public readonly IGuidService field2;
    }


    public class BillingInformationUpdateController : ApiControllerBase
    {
        public BillingInformationUpdateController(
            IPsaContextService arg0,
            IModelConverter<BillingInformationUpdate> arg1,
            IGuidService arg2,
            IInvoiceService arg3,
            IBillingInformationUpdateService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IModelConverter<BillingInformationUpdate> field1;
        public readonly IGuidService field2;
        public readonly IInvoiceService field3;
        public readonly IBillingInformationUpdateService field4;
    }


    public class KeywordsController : ApiControllerBase
    {
        public KeywordsController(
            IPsaContextService arg0,
            ITagService arg1,
            IKeywordModelConverter arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly ITagService field1;
        public readonly IKeywordModelConverter field2;
        public readonly IGuidService field3;
    }

    public class FlatRatesController : ApiControllerBase
    {
        public FlatRatesController(
            ITaskService arg0,
            IPsaContextService arg1,
            IModelConverter<TaskEx> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly ITaskService field0;
        public readonly IPsaContextService field1;
        public readonly IModelConverter<TaskEx> field2;
        public readonly IGuidService field3;
    }


    public class BusinessOverviewController : ApiControllerBase
    {
        public BusinessOverviewController(
            IPsaContextService arg0,
            IBusinessOverviewModelConverter arg1,
            ISalesProcessService arg2,
            IBusinessOverviewService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IBusinessOverviewModelConverter field1;
        public readonly ISalesProcessService field2;
        public readonly IBusinessOverviewService field3;
    }


    public class SpecialUserOptionsController : ApiControllerBase
    {
        public SpecialUserOptionsController(
            IPsaContextService arg0,
            IModelConverter<ListFilterValue> arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IPsaContextService field0;
        public readonly IModelConverter<ListFilterValue> field1;
    }


    public class UninvoicedProjectsController : PsaApiControllerBase<UninvoicedProjectModel, CaseSearchCriteria>
    {
        public UninvoicedProjectsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            ICaseService arg2,
            IModelConverter<Project> arg3,
            IGuidService arg4,
            IProjectsController arg5
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly ICaseService field2;
        public readonly IModelConverter<Project> field3;
        public readonly IGuidService field4;
        public readonly IProjectsController field5;
    }


    public class TeamProductivityController : ApiControllerBase
    {
        public TeamProductivityController(
            IPsaContextService arg0,
            ICaseMemberService arg1,
            ITeamProductivityModelConverter arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly ICaseMemberService field1;
        public readonly ITeamProductivityModelConverter field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class ProjectBillingCustomersController : ApiControllerBase
    {
        public ProjectBillingCustomersController(
            IPsaContextService arg0,
            IModelConverter<CaseBillingAccountEx> arg1,
            IGuidService arg2,
            ICaseBillingAccountService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IModelConverter<CaseBillingAccountEx> field1;
        public readonly IGuidService field2;
        public readonly ICaseBillingAccountService field3;
    }


    public class MarketSegmentsController : ApiControllerBase
    {
        public MarketSegmentsController(
            IPsaContextService arg0,
            IAccountGroupService arg1,
            IModelConverter<AccountGroup> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IAccountGroupService field1;
        public readonly IModelConverter<AccountGroup> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class ProjectProductsController : ApiControllerBase
    {
        public ProjectProductsController(
            IPsaContextService arg0,
            ICaseProductService arg1,
            IProjectProductModelConverter arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly ICaseProductService field1;
        public readonly IProjectProductModelConverter field2;
        public readonly IGuidService field3;
    }


    public class ScheduleOverviewController : ApiControllerBase
    {
        public ScheduleOverviewController(
            IPsaContextService arg0,
            IScheduleOverviewModelConverter arg1,
            IGuidService arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IPsaContextService field0;
        public readonly IScheduleOverviewModelConverter field1;
        public readonly IGuidService field2;
    }


    public class SharedDashboardAccessRightProfilesController : ApiControllerBase
    {
        public SharedDashboardAccessRightProfilesController(
            IPsaContextService arg0,
            IProfileDashboardService arg1,
            IModelConverter<ProfileDashboard> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IProfileDashboardService field1;
        public readonly IModelConverter<ProfileDashboard> field2;
        public readonly IGuidService field3;
    }


    public class SharedDashboardsController : ApiControllerBase
    {
        public SharedDashboardsController(
            IPsaContextService arg0,
            IDashboardService arg1,
            IModelConverter<Dashboard> arg2,
            IGuidService arg3,
            IModelConverter<ProfileDashboard> arg4,
            IProfileDashboardService arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IPsaContextService field0;
        public readonly IDashboardService field1;
        public readonly IModelConverter<Dashboard> field2;
        public readonly IGuidService field3;
        public readonly IModelConverter<ProfileDashboard> field4;
        public readonly IProfileDashboardService field5;
    }


    public class InvoicesOverviewController : ApiControllerBase
    {
        public InvoicesOverviewController(
            IPsaContextService arg0,
            IInvoicesOverviewModelConverter arg1,
            IGuidService arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IPsaContextService field0;
        public readonly IInvoicesOverviewModelConverter field1;
        public readonly IGuidService field2;
    }


    public class ProposalBillingPlanController : ApiControllerBase
    {
        public ProposalBillingPlanController(
            IPsaContextService arg0,
            IModelConverter<Item> arg1,
            IGuidService arg2,
            IItemService arg3,
            IOfferService arg4,
            IOfferTaskService arg5,
            IOfferSubtotalService arg6,
            IOfferItemService arg7,
            ITaxService arg8,
            IProductService arg9,
            ICountryProductService arg10
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
        }

        public readonly IPsaContextService field0;
        public readonly IModelConverter<Item> field1;
        public readonly IGuidService field2;
        public readonly IItemService field3;
        public readonly IOfferService field4;
        public readonly IOfferTaskService field5;
        public readonly IOfferSubtotalService field6;
        public readonly IOfferItemService field7;
        public readonly ITaxService field8;
        public readonly IProductService field9;
        public readonly ICountryProductService field10;
    }


    public class SalesOverviewController : ApiControllerBase
    {
        public SalesOverviewController(
            IPsaContextService arg0,
            ISalesOverviewModelConverter arg1,
            IGuidService arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IPsaContextService field0;
        public readonly ISalesOverviewModelConverter field1;
        public readonly IGuidService field2;
    }


    public class CustomersOverviewController : ApiControllerBase
    {
        public CustomersOverviewController(
            IPsaContextService arg0,
            ICustomersOverviewModelConverter arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IPsaContextService field0;
        public readonly ICustomersOverviewModelConverter field1;
    }


    public class ProposalProjectPlanController : ApiControllerBase
    {
        public ProposalProjectPlanController(
            IPsaContextService arg0,
            IModelConverter<Task> arg1,
            IGuidService arg2,
            ITaskService arg3,
            IOfferService arg4,
            IOfferTaskService arg5,
            IOfferSubtotalService arg6
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

        public readonly IPsaContextService field0;
        public readonly IModelConverter<Task> field1;
        public readonly IGuidService field2;
        public readonly ITaskService field3;
        public readonly IOfferService field4;
        public readonly IOfferTaskService field5;
        public readonly IOfferSubtotalService field6;
    }


    public class TimeEntryTypesController : ApiControllerBase
    {
        public TimeEntryTypesController(
            IPsaContextService arg0,
            IModelConverter<TimeEntryType> arg1,
            IGuidService arg2,
            IRestSettingsService arg3,
            ITimeEntryTypeService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IModelConverter<TimeEntryType> field1;
        public readonly IGuidService field2;
        public readonly IRestSettingsService field3;
        public readonly ITimeEntryTypeService field4;
    }


    public class CommunicationTypesController : ApiControllerBase
    {
        public CommunicationTypesController(
            IPsaContextService arg0,
            ICommunicationMethodService arg1,
            IModelConverter<CommunicationMethod> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly ICommunicationMethodService field1;
        public readonly IModelConverter<CommunicationMethod> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class ContactCommunicationsController : ApiControllerBase
    {
        public ContactCommunicationsController(
            IPsaContextService arg0,
            ICommunicatesWithRepository arg1,
            IModelConverter<ContactCommunicationMethod> arg2,
            IGuidService arg3,
            IContactCommunicationService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly ICommunicatesWithRepository field1;
        public readonly IModelConverter<ContactCommunicationMethod> field2;
        public readonly IGuidService field3;
        public readonly IContactCommunicationService field4;
    }


    public class FlextimeController : ApiControllerBase
    {
        public FlextimeController(
            IPsaContextService arg0,
            IFlextimeModelConverter arg1,
            IFlextimeManagerService arg2,
            IGuidService arg3,
            IUserService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IFlextimeModelConverter field1;
        public readonly IFlextimeManagerService field2;
        public readonly IGuidService field3;
        public readonly IUserService field4;
    }


    public class ProjectForecastsController : ApiControllerBase
    {
        public ProjectForecastsController(
            IPsaContextService arg0,
            IBillingPlanService arg1,
            IProjectForecastModelConverter arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IBillingPlanService field1;
        public readonly IProjectForecastModelConverter field2;
        public readonly IGuidService field3;
    }


    public class
        ResourceAllocationsController : PsaApiControllerBase<ResourceAllocationModel, ResourceAllocationSearchCriteria>
    {
        public ResourceAllocationsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IResourceAllocationService arg2,
            IResourceAllocationModelConverter arg3,
            IGuidService arg4
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IResourceAllocationService field2;
        public readonly IResourceAllocationModelConverter field3;
        public readonly IGuidService field4;
    }


    public class TemporaryProjectFeesController : ApiControllerBase
    {
        public TemporaryProjectFeesController(
            IPsaContextService arg0,
            IGuidService arg1,
            ITemporaryItemService arg2,
            IModelConverter<TemporaryItem> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IGuidService field1;
        public readonly ITemporaryItemService field2;
        public readonly IModelConverter<TemporaryItem> field3;
    }


    public class InvoiceTemplateSettingsController : ApiControllerBase
    {
        public InvoiceTemplateSettingsController(
            IPsaContextService arg0,
            IInvoiceTemplateConfigService arg1,
            IModelConverter<InvoiceTemplateConfig> arg2,
            IGuidService arg3,
            IInvoiceTemplateService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IInvoiceTemplateConfigService field1;
        public readonly IModelConverter<InvoiceTemplateConfig> field2;
        public readonly IGuidService field3;
        public readonly IInvoiceTemplateService field4;
    }


    public class TemporaryProjectTravelExpensesController : ApiControllerBase
    {
        public TemporaryProjectTravelExpensesController(
            IPsaContextService arg0,
            IGuidService arg1,
            ITemporaryItemService arg2,
            IModelConverter<TemporaryItem> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IGuidService field1;
        public readonly ITemporaryItemService field2;
        public readonly IModelConverter<TemporaryItem> field3;
    }


    public class TemporaryWorkHoursController : ApiControllerBase
    {
        public TemporaryWorkHoursController(
            IPsaContextService arg0,
            IGuidService arg1,
            ITemporaryHourService arg2,
            IModelConverter<TemporaryHour> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IGuidService field1;
        public readonly ITemporaryHourService field2;
        public readonly IModelConverter<TemporaryHour> field3;
    }


    public class InvoiceTemplatesController : ApiControllerBase
    {
        public InvoiceTemplatesController(
            IPsaContextService arg0,
            IGuidService arg1,
            IInvoiceTemplateService arg2,
            IModelConverter<InvoiceTemplate> arg3,
            IModelConverter<InvoiceTemplateConfig> arg4,
            IInvoiceTemplateConfigService arg5,
            IModelConverter<InvoiceTemplate> arg6,
            IInvoiceService arg7
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
        public readonly IGuidService field1;
        public readonly IInvoiceTemplateService field2;
        public readonly IModelConverter<InvoiceTemplate> field3;
        public readonly IModelConverter<InvoiceTemplateConfig> field4;
        public readonly IInvoiceTemplateConfigService field5;
        public readonly IModelConverter<InvoiceTemplate> field6;
        public readonly IInvoiceService field7;
    }


    public class WorkdaysController : ApiControllerBase
    {
        public WorkdaysController(
            IPsaContextService arg0,
            IUserService arg1,
            IWorkdayModelConverter arg2,
            IGuidService arg3,
            IWorkdayHandlerService arg4,
            IWorkdayService arg5,
            IWorkingDayService arg6
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

        public readonly IPsaContextService field0;
        public readonly IUserService field1;
        public readonly IWorkdayModelConverter field2;
        public readonly IGuidService field3;
        public readonly IWorkdayHandlerService field4;
        public readonly IWorkdayService field5;
        public readonly IWorkingDayService field6;
    }


    public class InvoiceSettingsController : ApiControllerBase
    {
        public InvoiceSettingsController(
            IPsaContextService arg0,
            IInvoiceConfigService arg1,
            IModelConverter<InvoiceConfig> arg2,
            IGuidService arg3,
            IInvoiceService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IInvoiceConfigService field1;
        public readonly IModelConverter<InvoiceConfig> field2;
        public readonly IGuidService field3;
        public readonly IInvoiceService field4;
    }


    public class OrganizationsController : ApiControllerBase
    {
        public OrganizationsController(
            IPsaContextService arg0,
            IOrganizationModelConverter arg1,
            IOrganizationTrustedService arg2,
            IOrganizationCompanyService arg3,
            IOrganizationAddonService arg4,
            ILanguageService arg5,
            IPsaOrganizationService arg6,
            IRegistrationRepository arg7,
            IPartnerService arg8,
            ISettingsService arg9,
            IFlextimeManagerService arg10
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

        public readonly IPsaContextService field0;
        public readonly IOrganizationModelConverter field1;
        public readonly IOrganizationTrustedService field2;
        public readonly IOrganizationCompanyService field3;
        public readonly IOrganizationAddonService field4;
        public readonly ILanguageService field5;
        public readonly IPsaOrganizationService field6;
        public readonly IRegistrationRepository field7;
        public readonly IPartnerService field8;
        public readonly ISettingsService field9;
        public readonly IFlextimeManagerService field10;
    }


    public class ProjectTaskStatusesController : ApiControllerBase
    {
        public ProjectTaskStatusesController(
            IPsaContextService arg0,
            IActivityStatusService arg1,
            IModelConverter<ActivityStatus> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IActivityStatusService field1;
        public readonly IModelConverter<ActivityStatus> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class ActivityTypesController : PsaApiControllerBase<ActivityTypeModel, CommonSearchCriteria>
    {
        public ActivityTypesController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IModelConverter<ActivityType> arg2,
            IGuidService arg3,
            IActivityTypeService arg4
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<ActivityType> field2;
        public readonly IGuidService field3;
        public readonly IActivityTypeService field4;
    }


    public class AddressesController : PsaApiControllerBase<AddressModel, CommonSearchCriteria>
    {
        public AddressesController(
            IPsaContextService arg0,
            IAddressService arg1,
            IAccountService arg2,
            ICompanyService arg3,
            IModelConverter<Address> arg4,
            IGuidService arg5,
            IRestSettingsService arg6
        ) : base(arg0, arg6)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IAccountService field2;
        public readonly ICompanyService field3;
        public readonly IModelConverter<Address> field4;
        public readonly IGuidService field5;
        public readonly IRestSettingsService field6;
    }


    public class DashboardPartsController : ApiControllerBase
    {
        public DashboardPartsController(
            IPsaContextService arg0,
            IDashboardPartService arg1,
            IModelConverter<DashboardPart> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IDashboardPartService field1;
        public readonly IModelConverter<DashboardPart> field2;
        public readonly IGuidService field3;
    }


    public class DashboardWithPartsController : ApiControllerBase
    {
        public DashboardWithPartsController(
            IPsaContextService arg0,
            IDashboardService arg1,
            IDashboardPartService arg2,
            IModelConverter<DashboardAndParts> arg3,
            IGuidService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IDashboardService field1;
        public readonly IDashboardPartService field2;
        public readonly IModelConverter<DashboardAndParts> field3;
        public readonly IGuidService field4;
    }


    public class DashboardsController : ApiControllerBase
    {
        public DashboardsController(
            IPsaContextService arg0,
            IDashboardService arg1,
            IModelConverter<Dashboard> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IDashboardService field1;
        public readonly IModelConverter<Dashboard> field2;
        public readonly IGuidService field3;
    }


    public class InvoiceRowsController : PsaApiControllerBase<InvoiceRowModel, InvoiceRowSearchCriteria>
    {
        public InvoiceRowsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IGuidService arg2,
            IInvoiceRowService arg3,
            IModelConverter<InvoiceRow> arg4,
            IInvoiceConfigService arg5,
            IInvoiceRowManagerService arg6
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly IGuidService field2;
        public readonly IInvoiceRowService field3;
        public readonly IModelConverter<InvoiceRow> field4;
        public readonly IInvoiceConfigService field5;
        public readonly IInvoiceRowManagerService field6;
    }


    public class InvoicesController : PsaApiControllerBase<InvoiceModel, InvoiceSearchCriteria>
    {
        public InvoicesController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IInvoiceService arg2,
            IInvoiceModelConverter arg3,
            IModelConverter<CreateInvoice> arg4,
            IGuidService arg5,
            ICaseService arg6,
            IInvoiceReimbursementService arg7,
            IInvoiceStatusService arg8,
            IInvoiceConfigService arg9,
            IHtmlSanitizerService arg10,
            IInvoiceTemplateConfigService arg11,
            IInvoiceCreationHandlerService arg12,
            ICustomFormulaService arg13
        ) : base(arg0, arg1)
        {
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

        public readonly IInvoiceService field2;
        public readonly IInvoiceModelConverter field3;
        public readonly IModelConverter<CreateInvoice> field4;
        public readonly IGuidService field5;
        public readonly ICaseService field6;
        public readonly IInvoiceReimbursementService field7;
        public readonly IInvoiceStatusService field8;
        public readonly IInvoiceConfigService field9;
        public readonly IHtmlSanitizerService field10;
        public readonly IInvoiceTemplateConfigService field11;
        public readonly IInvoiceCreationHandlerService field12;
        public readonly ICustomFormulaService field13;
    }


    public class HolidaysController : ApiControllerBase
    {
        public HolidaysController(
            IPsaContextService arg0,
            IWorkingDayExceptionService arg1,
            IModelConverter<WorkingDayException> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IWorkingDayExceptionService field1;
        public readonly IModelConverter<WorkingDayException> field2;
        public readonly IGuidService field3;
    }


    public class PermissionsController : ApiControllerBase
    {
        public PermissionsController(
            IPsaContextService arg0,
            IUserService arg1,
            IAccessRightsModelConverter arg2,
            IGuidService arg3,
            IAccessRightService arg4,
            IProfileService arg5,
            IProfileRightRepository arg6
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

        public readonly IPsaContextService field0;
        public readonly IUserService field1;
        public readonly IAccessRightsModelConverter field2;
        public readonly IGuidService field3;
        public readonly IAccessRightService field4;
        public readonly IProfileService field5;
        public readonly IProfileRightRepository field6;
    }


    public class QuickSearchController : ApiControllerBase
    {
        public QuickSearchController(
            IPsaContextService arg0,
            IQuickSearchService arg1,
            IQuickSearchResultModelConverter arg2,
            IRestSettingsService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IQuickSearchService field1;
        public readonly IQuickSearchResultModelConverter field2;
        public readonly IRestSettingsService field3;
    }


    public class ProposalTemplatesController : ApiControllerBase
    {
        public ProposalTemplatesController(
            IPsaContextService arg0,
            IGuidService arg1,
            IOfferTemplateService arg2,
            IModelConverter<Offer> arg3,
            IRestSettingsService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IGuidService field1;
        public readonly IOfferTemplateService field2;
        public readonly IModelConverter<Offer> field3;
        public readonly IRestSettingsService field4;
    }


    public class InvoiceTotalsController : ApiControllerBase
    {
        public InvoiceTotalsController(
            IPsaContextService arg0,
            IGuidService arg1,
            IInvoiceTaxBreakdownService arg2,
            IModelConverter<TaxBreakdown> arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IGuidService field1;
        public readonly IInvoiceTaxBreakdownService field2;
        public readonly IModelConverter<TaxBreakdown> field3;
    }


    public class ProposalTotalsController : ApiControllerBase
    {
        public ProposalTotalsController(
            IPsaContextService arg0,
            IGuidService arg1,
            IProposalTaxBreakdownService arg2,
            ICaseService arg3,
            IModelConverter<TaxBreakdown> arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IGuidService field1;
        public readonly IProposalTaxBreakdownService field2;
        public readonly ICaseService field3;
        public readonly IModelConverter<TaxBreakdown> field4;
    }


    public class ProposalWorkhoursController : PsaApiControllerBase<ProposalWorkhourModel, CommonSearchCriteria>
    {
        public ProposalWorkhoursController(
            IPsaContextService arg0,
            IOfferTaskService arg1,
            IModelConverter<OfferTask> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base(arg0, arg4)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<OfferTask> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class ProposalSubtotalsController : PsaApiControllerBase<ProposalSubtotalModel, CommonSearchCriteria>
    {
        public ProposalSubtotalsController(
            IPsaContextService arg0,
            IOfferSubtotalService arg1,
            IModelConverter<OfferSubtotal> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base(arg0, arg4)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<OfferSubtotal> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class ProposalFeesController : PsaApiControllerBase<ProposalFeeModel, CommonSearchCriteria>
    {
        public ProposalFeesController(
            IPsaContextService arg0,
            IOfferItemService arg1,
            IModelConverter<OfferItem> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base(arg0, arg4)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<OfferItem> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class ReportsController : PsaApiControllerBase<ReportModel, CommonSearchCriteria>
    {
        public ReportsController(
            IPsaContextService arg0,
            IReportService arg1,
            IModelConverter<Report> arg2,
            IGuidService arg5,
            IRestSettingsService arg6, 
            IAuditTrail<IReport> arg7,
            IUserSettingsService arg8
        ) : base(arg0, arg6)
        {
            field2 = arg2;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
        }

        public readonly IModelConverter<Report> field2;
        public readonly IGuidService field5;
        public readonly IRestSettingsService field6;
        public readonly IAuditTrail<IReport> field7;
        public readonly IUserSettingsService field8;
    }


    public class ProposalStatusesController : ApiControllerBase
    {
        public ProposalStatusesController(
            IPsaContextService arg0,
            IProposalStatusService arg1,
            IModelConverter<ProposalStatus> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IProposalStatusService field1;
        public readonly IModelConverter<ProposalStatus> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class InvoiceStatusesController : ApiControllerBase
    {
        public InvoiceStatusesController(
            IPsaContextService arg0,
            IInvoiceStatusService arg1,
            IModelConverter<InvoiceStatus> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IInvoiceStatusService field1;
        public readonly IModelConverter<InvoiceStatus> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class ProposalsController : PsaApiControllerBase<ProposalModel, OfferSearchCriteria>
    {
        public ProposalsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IOfferService arg2,
            IModelConverter<Offer> arg3,
            IGuidService arg4,
            IHtmlSanitizerService arg5
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IOfferService field2;
        public readonly IModelConverter<Offer> field3;
        public readonly IGuidService field4;
        public readonly IHtmlSanitizerService field5;
    }


    public class StatusHistoryController : ApiControllerBase
    {
        public StatusHistoryController(
            IPsaContextService arg0,
            ITaskStatusCommentService arg1,
            ICaseCommentService arg2,
            IModelConverter<CaseStatusHistory> arg3,
            IGuidService arg4,
            IRestSettingsService arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IPsaContextService field0;
        public readonly ITaskStatusCommentService field1;
        public readonly ICaseCommentService field2;
        public readonly IModelConverter<CaseStatusHistory> field3;
        public readonly IGuidService field4;
        public readonly IRestSettingsService field5;
    }


    public class PhaseStatusTypesController : PsaApiControllerBase<PhaseStatusTypeModel, CommonSearchCriteria>
    {
        public PhaseStatusTypesController(
            IPsaContextService arg0,
            ITaskStatusTypeService arg1,
            IModelConverter<TaskStatusType> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base(arg0, arg4)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<TaskStatusType> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class CostCentersController : ApiControllerBase
    {
        public CostCentersController(
            IPsaContextService arg0,
            ICostCenterService arg1,
            IModelConverter<CostCenter> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly ICostCenterService field1;
        public readonly IModelConverter<CostCenter> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class ProjectWorktypesController : ApiControllerBase
    {
        public ProjectWorktypesController(
            IPsaContextService arg0,
            ICaseWorkTypeService arg1,
            IProjectWorktypeModelConverter arg2,
            IGuidService arg3,
            IWorkTypeService arg4,
            IRestSettingsService arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IPsaContextService field0;
        public readonly ICaseWorkTypeService field1;
        public readonly IProjectWorktypeModelConverter field2;
        public readonly IGuidService field3;
        public readonly IWorkTypeService field4;
        public readonly IRestSettingsService field5;
    }


    public class PricelistVersionsController : ApiControllerBase
    {
        public PricelistVersionsController(
            IPsaContextService arg0,
            IPricelistVersionService arg1,
            IModelConverter<PricelistVersion> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IPricelistVersionService field1;
        public readonly IModelConverter<PricelistVersion> field2;
        public readonly IGuidService field3;
    }


    public class OvertimePricesController : ApiControllerBase
    {
        public OvertimePricesController(
            IPsaContextService arg0,
            ICaseService arg1,
            IOvertimePriceModelConverter arg2,
            IGuidService arg3,
            IOvertimePriceService arg4,
            IPricelistService arg5,
            IPricelistVersionService arg6
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

        public readonly IPsaContextService field0;
        public readonly ICaseService field1;
        public readonly IOvertimePriceModelConverter field2;
        public readonly IGuidService field3;
        public readonly IOvertimePriceService field4;
        public readonly IPricelistService field5;
        public readonly IPricelistVersionService field6;
    }


    public class AllUsersController : PsaApiControllerBase<UserBaseModel, UserSearchCriteria>
    {
        public AllUsersController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IUserService arg2,
            IUserSettingsModelConverter arg3,
            IGuidService arg4,
            IResourceAllocationService arg5
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IUserService field2;
        public readonly IUserSettingsModelConverter field3;
        public readonly IGuidService field4;
        public readonly IResourceAllocationService field5;
    }


    public class TimeEntriesController : PsaApiControllerBase<TimeEntryModel, TimeEntrySearchCriteria>
    {
        public TimeEntriesController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IModelConverter<TimeEntry> arg2,
            IGuidService arg3,
            ITimeEntryService arg4,
            IWorkdayHandlerService arg5
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IModelConverter<TimeEntry> field2;
        public readonly IGuidService field3;
        public readonly ITimeEntryService field4;
        public readonly IWorkdayHandlerService field5;
    }


    public class WorkTypesController : ApiControllerBase
    {
        public WorkTypesController(
            IPsaContextService arg0,
            IWorkTypeService arg1,
            IModelConverter<WorkType> arg2,
            IGuidService arg3,
            IModelConverter<WorktypeForCase> arg4,
            ICaseService arg5,
            IOrganizationCompanyService arg6,
            IRestSettingsService arg7
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
        public readonly IWorkTypeService field1;
        public readonly IModelConverter<WorkType> field2;
        public readonly IGuidService field3;
        public readonly IModelConverter<WorktypeForCase> field4;
        public readonly ICaseService field5;
        public readonly IOrganizationCompanyService field6;
        public readonly IRestSettingsService field7;
    }


    public class WorkHoursController : PsaApiControllerBase<WorkHourModel, HourSearchCriteria>
    {
        public WorkHoursController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IModelConverter<HourEx> arg2,
            IGuidService arg3,
            IHourService arg4,
            IHourRepository arg5,
            IInvoiceService arg6,
            IInvoiceConfigRepository arg7,
            IWorkdayHandlerService arg8,
            IInvoiceCaseRepository arg9,
            IModelConverter<CaseHour> arg10,
            ITaskRepository arg11,
            ICaseRepository arg12,
            IWorkTypeService arg13,
            IInvoiceHourHandlerService arg14,
            IWorkTypeRepository arg15
        ) : base(arg0, arg1)
        {
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

        public readonly IModelConverter<HourEx> field2;
        public readonly IGuidService field3;
        public readonly IHourService field4;
        public readonly IHourRepository field5;
        public readonly IInvoiceService field6;
        public readonly IInvoiceConfigRepository field7;
        public readonly IWorkdayHandlerService field8;
        public readonly IInvoiceCaseRepository field9;
        public readonly IModelConverter<CaseHour> field10;
        public readonly ITaskRepository field11;
        public readonly ICaseRepository field12;
        public readonly IWorkTypeService field13;
        public readonly IInvoiceHourHandlerService field14;
        public readonly IWorkTypeRepository field15;
    }


    public class ProjectWorkHourPricesController : ApiControllerBase
    {
        public ProjectWorkHourPricesController(
            IPsaContextService arg0,
            ICaseService arg1,
            IModelConverter<WorkPriceEx> arg2,
            IGuidService arg3,
            IWorkPriceService arg4,
            IPricelistService arg5,
            IPricelistVersionService arg6
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

        public readonly IPsaContextService field0;
        public readonly ICaseService field1;
        public readonly IModelConverter<WorkPriceEx> field2;
        public readonly IGuidService field3;
        public readonly IWorkPriceService field4;
        public readonly IPricelistService field5;
        public readonly IPricelistVersionService field6;
    }


    public class TravelPricesController : ApiControllerBase
    {
        public TravelPricesController(
            IPsaContextService arg0,
            ICaseService arg1,
            IModelConverter<ProductPriceEx> arg2,
            IGuidService arg3,
            IProductPriceService arg4,
            IPricelistService arg5,
            IPricelistVersionService arg6,
            IProductService arg7,
            IRestSettingsService arg8
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

        public readonly IPsaContextService field0;
        public readonly ICaseService field1;
        public readonly IModelConverter<ProductPriceEx> field2;
        public readonly IGuidService field3;
        public readonly IProductPriceService field4;
        public readonly IPricelistService field5;
        public readonly IPricelistVersionService field6;
        public readonly IProductService field7;
        public readonly IRestSettingsService field8;
    }


    public class WorkHourPricesController : ApiControllerBase
    {
        public WorkHourPricesController(
            IPsaContextService arg0,
            IModelConverter<WorkPriceEx> arg1,
            IGuidService arg2,
            IWorkPriceService arg3,
            IPricelistService arg4,
            IPricelistPriceService arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IPsaContextService field0;
        public readonly IModelConverter<WorkPriceEx> field1;
        public readonly IGuidService field2;
        public readonly IWorkPriceService field3;
        public readonly IPricelistService field4;
        public readonly IPricelistPriceService field5;
    }


    public class ProductPricesController : ApiControllerBase
    {
        public ProductPricesController(
            IPsaContextService arg0,
            ICaseService arg1,
            IModelConverter<ProductPriceEx> arg2,
            IGuidService arg3,
            IProductPriceService arg4,
            IPricelistService arg5,
            IPricelistVersionService arg6,
            IPricelistPriceService arg7,
            IRestSettingsService arg8
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

        public readonly IPsaContextService field0;
        public readonly ICaseService field1;
        public readonly IModelConverter<ProductPriceEx> field2;
        public readonly IGuidService field3;
        public readonly IProductPriceService field4;
        public readonly IPricelistService field5;
        public readonly IPricelistVersionService field6;
        public readonly IPricelistPriceService field7;
        public readonly IRestSettingsService field8;
    }


    public class
        ProjectRecurringFeeRulesController : PsaApiControllerBase<ProjectRecurringFeeRuleModel, ItemSearchCriteria>
    {
        public ProjectRecurringFeeRulesController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IRecurringItemService arg2,
            IProjectRecurringFeeModelConverter arg3,
            IGuidService arg4,
            IItemSalesAccountService arg5
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IRecurringItemService field2;
        public readonly IProjectRecurringFeeModelConverter field3;
        public readonly IGuidService field4;
        public readonly IItemSalesAccountService field5;
    }


    public class ProjectTravelExpensesController : PsaApiControllerBase<ProjectTravelExpenseModel, ItemSearchCriteria>
    {
        public ProjectTravelExpensesController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IItemService arg2,
            IProjectTravelExpenseModelConverter arg3,
            IGuidService arg4,
            IInvoiceService arg5,
            IWorkdayHandlerService arg6,
            IInvoiceCaseRepository arg7,
            IItemSalesAccountService arg8,
            IProductService arg9,
            IInvoiceConfigRepository arg10,
            IInvoiceItemHandlerService arg11
        ) : base(arg0, arg1)
        {
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

        public readonly IItemService field2;
        public readonly IProjectTravelExpenseModelConverter field3;
        public readonly IGuidService field4;
        public readonly IInvoiceService field5;
        public readonly IWorkdayHandlerService field6;
        public readonly IInvoiceCaseRepository field7;
        public readonly IItemSalesAccountService field8;
        public readonly IProductService field9;
        public readonly IInvoiceConfigRepository field10;
        public readonly IInvoiceItemHandlerService field11;
    }


    public class TravelExpensesController : ApiControllerBase
    {
        public TravelExpensesController(
            IPsaContextService arg0,
            IProductService arg1,
            IModelConverter<Product> arg2,
            IGuidService arg3,
            ICaseService arg4,
            ITaxService arg5,
            IRestSettingsService arg6
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

        public readonly IPsaContextService field0;
        public readonly IProductService field1;
        public readonly IModelConverter<Product> field2;
        public readonly IGuidService field3;
        public readonly ICaseService field4;
        public readonly ITaxService field5;
        public readonly IRestSettingsService field6;
    }


    public class ProjectFeesController : PsaApiControllerBase<ProjectFeeModel, ItemSearchCriteria>
    {
        public ProjectFeesController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IItemService arg2,
            IProjectFeeModelConverter arg3,
            IGuidService arg4,
            IInvoiceService arg5,
            IWorkdayHandlerService arg6,
            IInvoiceCaseRepository arg7,
            IItemSalesAccountService arg8,
            IInvoiceConfigRepository arg9,
            IInvoiceItemHandlerService arg10
        ) : base(arg0, arg1)
        {
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

        public readonly IItemService field2;
        public readonly IProjectFeeModelConverter field3;
        public readonly IGuidService field4;
        public readonly IInvoiceService field5;
        public readonly IWorkdayHandlerService field6;
        public readonly IInvoiceCaseRepository field7;
        public readonly IItemSalesAccountService field8;
        public readonly IInvoiceConfigRepository field9;
        public readonly IInvoiceItemHandlerService field10;
    }


    public class SalesReceivableAccountsController : ApiControllerBase
    {
        public SalesReceivableAccountsController(
            IPsaContextService arg0,
            ICurrencyService arg1,
            IModelConverter<CurrencyEx> arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IPsaContextService field0;
        public readonly ICurrencyService field1;
        public readonly IModelConverter<CurrencyEx> field2;
    }


    public class ValueAddedTaxesController : ApiControllerBase
    {
        public ValueAddedTaxesController(
            IPsaContextService arg0,
            ITaxService arg1,
            IModelConverter<Tax> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly ITaxService field1;
        public readonly IModelConverter<Tax> field2;
        public readonly IGuidService field3;
    }

    public class ProductsController : ApiControllerBase
    {
        public ProductsController(
            IPsaContextService arg0,
            IProductService arg1,
            IProductModelConverter arg2,
            IGuidService arg3,
            ICaseService arg4,
            IModelConverter<ProductForCase> arg5,
            IRestSettingsService arg6,
            ITaxService arg7
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
        public readonly IProductService field1;
        public readonly IProductModelConverter field2;
        public readonly IGuidService field3;
        public readonly ICaseService field4;
        public readonly IModelConverter<ProductForCase> field5;
        public readonly IRestSettingsService field6;
        public readonly ITaxService field7;
    }


    public class BusinessUnitsController : PsaApiControllerBase<BusinessUnitModel, BusinessUnitSearchCriteria>
    {
        public BusinessUnitsController(
            IPsaContextService arg0,
            IBusinessUnitService arg1,
            IModelConverter<BusinessUnit> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base(arg0, arg4)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<BusinessUnit> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class CollaborationNotesController : ApiControllerBase
    {
        public CollaborationNotesController(
            IPsaContextService arg0,
            INoteProviderService<AccountNote> arg1,
            IModelConverter<INote> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly INoteProviderService<AccountNote> field1;
        public readonly IModelConverter<INote> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class ContactsController : PsaApiControllerBase<ContactModel, ContactSearchCriteria>
    {
        public ContactsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IContactService arg2,
            IModelConverter<Contact> arg3,
            IGuidService arg4,
            IContactCommunicationService arg5
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IContactService field2;
        public readonly IModelConverter<Contact> field3;
        public readonly IGuidService field4;
        public readonly IContactCommunicationService field5;
    }


    public class CurrencyBasesController : ApiControllerBase
    {
        public CurrencyBasesController(
            IPsaContextService arg0,
            ICurrencyBaseService arg1,
            IModelConverter<CurrencyBase> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly ICurrencyBaseService field1;
        public readonly IModelConverter<CurrencyBase> field2;
        public readonly IGuidService field3;
    }


    public class CurrenciesController : ApiControllerBase
    {
        public CurrenciesController(
            IPsaContextService arg0,
            ICurrencyService arg1,
            IModelConverter<CurrencyEx> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly ICurrencyService field1;
        public readonly IModelConverter<CurrencyEx> field2;
        public readonly IGuidService field3;
    }


    public class CountriesController : ApiControllerBase
    {
        public CountriesController(
            IPsaContextService arg0,
            ICountryService arg1,
            IModelConverter<Country> arg2,
            ICountryRegionService arg3,
            IModelConverter<CountryRegion> arg4,
            IGuidService arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IPsaContextService field0;
        public readonly ICountryService field1;
        public readonly IModelConverter<Country> field2;
        public readonly ICountryRegionService field3;
        public readonly IModelConverter<CountryRegion> field4;
        public readonly IGuidService field5;
    }


    public class CustomersController : PsaApiControllerBase<CustomerModel, AccountSearchCriteria>
    {
        public CustomersController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IAccountService arg2,
            ICompanyService arg3,
            ICustomerModelConverter arg4,
            IGuidService arg5,
            IHtmlSanitizerService arg6,
            IFinancialsIntegrationHandlerService arg7
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
        }

        public readonly IAccountService field2;
        public readonly ICompanyService field3;
        public readonly ICustomerModelConverter field4;
        public readonly IGuidService field5;
        public readonly IHtmlSanitizerService field6;
        public readonly IFinancialsIntegrationHandlerService field7;
    }


    public class FileDataController : ApiControllerBase
    {
        public FileDataController(
            IPsaContextService arg0,
            IFileDataService arg1,
            IFileService arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IFileDataService field1;
        public readonly IFileService field2;
        public readonly IGuidService field3;
    }


    public class FilesController : PsaApiControllerBase<ProjectFileModel, CaseFileSearchCriteria>
    {
        public FilesController(
            IPsaContextService arg0,
            IFileService arg1,
            IFileModelConverter arg2,
            IGuidService arg3,
            IFileDataService arg4,
            ICaseFileService arg5,
            IOfferFileService arg6,
            IInvoiceFileService arg7,
            IPsaUserProfilePictureService arg8,
            IOrganizationTrustedService arg9,
            IUniqueUserService arg10,
            ICurrentSessionService arg11,
            IOfferService arg12,
            IRestSettingsService arg13,
            IItemFileService arg14,
            ITravelExpenseReceiptService arg15,
            Utilities.IImageResizer arg16
        ) : base(arg0, arg13)
        {
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
        }

        public readonly IFileModelConverter field2;
        public readonly IGuidService field3;
        public readonly IFileDataService field4;
        public readonly ICaseFileService field5;
        public readonly IOfferFileService field6;
        public readonly IInvoiceFileService field7;
        public readonly IPsaUserProfilePictureService field8;
        public readonly IOrganizationTrustedService field9;
        public readonly IUniqueUserService field10;
        public readonly ICurrentSessionService field11;
        public readonly IOfferService field12;
        public readonly IRestSettingsService field13;
        public readonly IItemFileService field14;
        public readonly ITravelExpenseReceiptService field15;
        public readonly Utilities.IImageResizer field16;
    }


    public class FormattingCulturesController : ApiControllerBase
    {
        public FormattingCulturesController(
            IPsaContextService arg0,
            IFormattingCultureService arg1,
            IModelConverter<FormatingCulture> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IFormattingCultureService field1;
        public readonly IModelConverter<FormatingCulture> field2;
        public readonly IGuidService field3;
    }


    public class IndustriesController : PsaApiControllerBase<IndustryModel, CommonSearchCriteria>
    {
        public IndustriesController(
            IPsaContextService arg0,
            IIndustryService arg1,
            IModelConverter<Industry> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base(arg0, arg4)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<Industry> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class LanguagesController : ApiControllerBase
    {
        public LanguagesController(
            IPsaContextService arg0,
            ILanguageService arg1,
            IModelConverter<Language> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly ILanguageService field1;
        public readonly IModelConverter<Language> field2;
        public readonly IGuidService field3;
    }


    public class LeadSourcesController : PsaApiControllerBase<LeadSourceModel, CommonSearchCriteria>
    {
        public LeadSourcesController(
            IPsaContextService arg0,
            ILeadSourceService arg1,
            IModelConverter<LeadSource> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base(arg0, arg4)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<LeadSource> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class MenuController : ApiControllerBase
    {
        public MenuController(
            IPsaContextService arg0,
            INavigationHistoryService arg1,
            IMenuModelConverter arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IPsaContextService field0;
        public readonly INavigationHistoryService field1;
        public readonly IMenuModelConverter field2;
    }


    public class PersonalSettingsController : ApiControllerBase
    {
        public PersonalSettingsController(
            IPsaContextService arg0,
            IPersonalSettingsService arg1,
            IUserService arg2,
            IGuidService arg3,
            IPersonalSettingsModelConverter arg4,
            ISettingsRepository arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IPsaContextService field0;
        public readonly IPersonalSettingsService field1;
        public readonly IUserService field2;
        public readonly IGuidService field3;
        public readonly IPersonalSettingsModelConverter field4;
        public readonly ISettingsRepository field5;
    }


    public class PhaseMembersController : ApiControllerBase
    {
        public PhaseMembersController(
            IPsaContextService arg0,
            ICaseMemberService arg1,
            ITaskMemberService arg2,
            IModelConverter<CaseMember> arg3,
            IGuidService arg4,
            ICaseService arg5,
            ITaskService arg6,
            IModelConverter<TaskMember> arg7,
            IRestSettingsService arg8,
            IResourceAllocationService arg9
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

        public readonly IPsaContextService field0;
        public readonly ICaseMemberService field1;
        public readonly ITaskMemberService field2;
        public readonly IModelConverter<CaseMember> field3;
        public readonly IGuidService field4;
        public readonly ICaseService field5;
        public readonly ITaskService field6;
        public readonly IModelConverter<TaskMember> field7;
        public readonly IRestSettingsService field8;
        public readonly IResourceAllocationService field9;
    }


    public class PhasesController : PsaApiControllerBase<PhaseModel, TaskSearchCriteria>
    {
        public PhasesController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            ITaskService arg2,
            IModelConverter<Task> arg3,
            IGuidService arg4,
            ITreeTaskService arg5,
            IModelConverter<TreeTask> arg6,
            IModelConverter<TreeTask> arg7,
            IUserTaskFavoriteService arg8
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
        }

        public readonly ITaskService field2;
        public readonly IModelConverter<Task> field3;
        public readonly IGuidService field4;
        public readonly ITreeTaskService field5;
        public readonly IModelConverter<TreeTask> field6;
        public readonly IModelConverter<TreeTask> field7;
        public readonly IUserTaskFavoriteService field8;
    }


    public class PricelistsController : ApiControllerBase
    {
        public PricelistsController(
            IPsaContextService arg0,
            IPricelistService arg1,
            IModelConverter<Pricelist> arg2,
            IGuidService arg3,
            ICaseService arg4,
            IRestSettingsService arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IPsaContextService field0;
        public readonly IPricelistService field1;
        public readonly IModelConverter<Pricelist> field2;
        public readonly IGuidService field3;
        public readonly ICaseService field4;
        public readonly IRestSettingsService field5;
    }


    public class OvertimesController : ApiControllerBase
    {
        public OvertimesController(
            IPsaContextService arg0,
            IOvertimeService arg1,
            IModelConverter<OverTime> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IOvertimeService field1;
        public readonly IModelConverter<OverTime> field2;
        public readonly IGuidService field3;
    }


    public class ProjectsController : PsaApiControllerBase<ProjectModel, CaseSearchCriteria>
        , IProjectsController
    {
        public ProjectsController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            ICaseService arg2,
            IGuidService arg4,
            IDataAnalyticsService arg5,
            IHtmlSanitizerService arg6
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly ICaseService field2;
        public readonly IGuidService field4;
        public readonly IDataAnalyticsService field5;
        public readonly IHtmlSanitizerService field6;
    }


    public class ProjectStatusTypesController : PsaApiControllerBase<ProjectStatusTypeModel, CommonSearchCriteria>
    {
        public ProjectStatusTypesController(
            IPsaContextService arg0,
            ICaseStatusTypeService arg1,
            IModelConverter<CaseStatusType> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base(arg0, arg4)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IModelConverter<CaseStatusType> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class SalesAccountsController : ApiControllerBase
    {
        public SalesAccountsController(
            IPsaContextService arg0,
            ISalesAccountService arg1,
            IModelConverter<SalesAccount> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly ISalesAccountService field1;
        public readonly IModelConverter<SalesAccount> field2;
        public readonly IGuidService field3;
    }


    public class SalesNotesController : ApiControllerBase
    {
        public SalesNotesController(
            IPsaContextService arg0,
            INoteProviderService<CaseNote> arg1,
            IModelConverter<INote> arg2,
            IGuidService arg3,
            IRestSettingsService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly INoteProviderService<CaseNote> field1;
        public readonly IModelConverter<INote> field2;
        public readonly IGuidService field3;
        public readonly IRestSettingsService field4;
    }


    public class ProductCategoriesController : ApiControllerBase
    {
        public ProductCategoriesController(
            IPsaContextService arg0,
            IProductCategoryService arg1,
            IModelConverter<ProductCategory> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IProductCategoryService field1;
        public readonly IModelConverter<ProductCategory> field2;
        public readonly IGuidService field3;
    }


    public class WorkContractsController : ApiControllerBase
    {
        public WorkContractsController(
            IPsaContextService arg0,
            IEmploymentService arg1,
            IModelConverter<Employment> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly IEmploymentService field1;
        public readonly IModelConverter<Employment> field2;
        public readonly IGuidService field3;
    }


    public class SalesStatusController : ApiControllerBase
    {
        public SalesStatusController(
            IPsaContextService arg0,
            ISalesStatusService arg1,
            IModelConverter<SalesStatus> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly ISalesStatusService field1;
        public readonly IModelConverter<SalesStatus> field2;
        public readonly IGuidService field3;
    }


    public class SalesStatusTypeController : ApiControllerBase
    {
        public SalesStatusTypeController(
            IPsaContextService arg0,
            ISalesProcessService arg1,
            IModelConverter<SalesProcess> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly ISalesProcessService field1;
        public readonly IModelConverter<SalesProcess> field2;
        public readonly IGuidService field3;
    }


    public class TimeZonesController : ApiControllerBase
    {
        public TimeZonesController(
            IPsaContextService arg0,
            ITimeZoneService arg1,
            IModelConverter<TimeZone> arg2,
            IGuidService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IPsaContextService field0;
        public readonly ITimeZoneService field1;
        public readonly IModelConverter<TimeZone> field2;
        public readonly IGuidService field3;
    }


    public class UsersController : PsaApiControllerBase<UserModel, UserSearchCriteria>
    {
        public UsersController(
            IPsaContextService arg0,
            IRestSettingsService arg1,
            IUserService arg2,
            IUserSettingsModelConverter arg3,
            IGuidService arg4,
            IFormattingCultureService arg5,
            IUserRepository arg6,
            IOrganizationTrustedService arg7,
            IUniqueUserService arg8,
            ITimeZoneService arg9
        ) : base(arg0, arg1)
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field7 = arg7;
            field8 = arg8;
            field9 = arg9;
        }

        public readonly IUserService field2;
        public readonly IUserSettingsModelConverter field3;
        public readonly IGuidService field4;
        public readonly IFormattingCultureService field5;
        public readonly IUserRepository field6;
        public readonly IOrganizationTrustedService field7;
        public readonly IUniqueUserService field8;
        public readonly ITimeZoneService field9;
    }


    public class AppRequest
    {
        public AppRequest(
        )
        {
        }
    }


    public class PsaApiControllerBase : ApiControllerBase
    {
        public PsaApiControllerBase(
            IContextService<IPsaContext> arg0,
            IRestSettingsService arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IRestSettingsService field1;
    }


    public class
        ScheduledWorkJobsController : ScheduledJobsController<ScheduledWorkJobModel,
            IScheduledWorkService>
    {
        public ScheduledWorkJobsController(
            IContextService<IPsaContext> arg0,
            IRestSettingsService arg1,
            IModelConverter<BackgroundTask> arg2,
            IGuidService arg3,
            IScheduledWorkTaskService arg4,
            IScheduledWorkTaskAuthorization arg5
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5)
        {
        }
    }


    public class PasswordChangeController : ApiControllerBase
    {
        public PasswordChangeController(
            IPsaContextService arg0,
            IGuidService arg1,
            IOrganizationTrustedService arg2,
            IUniqueUserService arg3,
            IUserPasswordService arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IGuidService field1;
        public readonly IOrganizationTrustedService field2;
        public readonly IUniqueUserService field3;
        public readonly IUserPasswordService field4;
    }


    public class UserInactivationInformationController : ApiControllerBase
    {
        public UserInactivationInformationController(
            IPsaContextService arg0,
            IGuidService arg1,
            IUserService arg2,
            IUserRepository arg3,
            IUserInactivationInformationModelConverter arg4
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IGuidService field1;
        public readonly IUserService field2;
        public readonly IUserRepository field3;
        public readonly IUserInactivationInformationModelConverter field4;
    }


    public class EmailAddressChangeController : ApiControllerBase
    {
        public EmailAddressChangeController(
            IPsaContextService arg0,
            IGuidService arg1,
            IChangeEmailAddressService arg2,
            IOrganizationTrustedService arg3,
            IUniqueUserService arg4,
            ITrustedOrganizationUserRepository arg5
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IPsaContextService field0;
        public readonly IGuidService field1;
        public readonly IChangeEmailAddressService field2;
        public readonly IOrganizationTrustedService field3;
        public readonly IUniqueUserService field4;
        public readonly ITrustedOrganizationUserRepository field5;
    }


    public class EmailController : ApiControllerBase
    {
        public EmailController(
            IPsaContextService arg0,
            IMailClient arg1,
            IGuidService arg2,
            IInvoiceService arg3,
            IPdfCreationHandlerService arg4,
            IOfferService arg5,
            IHourEmailService arg6,
            ITermsOfServiceEmailService arg7,
            IUserService arg8,
            IMasterUserRepository arg9,
            IUserEmailBuilder arg10
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

        public readonly IPsaContextService field0;
        public readonly IMailClient field1;
        public readonly IGuidService field2;
        public readonly IInvoiceService field3;
        public readonly IPdfCreationHandlerService field4;
        public readonly IOfferService field5;
        public readonly IHourEmailService field6;
        public readonly ITermsOfServiceEmailService field7;
        public readonly IUserService field8;
        public readonly IMasterUserRepository field9;
        public readonly IUserEmailBuilder field10;
    }


    public interface IProjectsController
    {
    }


    public class PdfController : ApiControllerBase
    {
        public PdfController(
            IPsaContextService arg0,
            IGuidService arg1,
            IPdfDocumentService arg2,
            IPdfCreationHandlerService arg3,
            IInvoiceService arg4,
            IOfferService arg5,
            ITravelReimbursementService arg6
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

        public readonly IPsaContextService field0;
        public readonly IGuidService field1;
        public readonly IPdfDocumentService field2;
        public readonly IPdfCreationHandlerService field3;
        public readonly IInvoiceService field4;
        public readonly IOfferService field5;
        public readonly ITravelReimbursementService field6;
    }


    public class OrganizationInfoModel
    {
        public OrganizationInfoModel(
        )
        {
        }
    }


    public class UserInfoModel
    {
        public UserInfoModel(
        )
        {
        }
    }


    public class LocalAccessTokenRequestModel
    {
        public LocalAccessTokenRequestModel(
        )
        {
        }
    }


    public class AuthenticationControllerBase : ApiControllerBase
    {
        public AuthenticationControllerBase()
        {
        }

        public AuthenticationControllerBase(
            IContextService<IPsaContext> arg0,
            ITokenService arg1,
            ICurrentSessionService arg2,
            IPersonalSettingsService arg3,
            IGuidService arg4,
            IOrganizationTrustedService arg5,
            IModelConverter<User> arg6,
            IRefreshTokenService arg7,
            IOrganizationAddonService arg8,
            IUniqueUserService arg9,
            IUniqueUserPhotoFileRepository arg10,
            IUserInfoTokenService arg11,
            IFeatureToggleService arg12,
            ILastSignInService arg13,
            IOrganizationWorkweekService arg14,
            IUserService arg15,
            ITermsOfServiceApprovalService arg16,
            IAccountService arg17,
            IConnClient arg18,
            IConnUserInfoService arg19,
            IConnUserService arg20,
            IConnUserApplicationsService arg21,
            IGlobalSettingsService arg22
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
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly ITokenService field1;
        public readonly ICurrentSessionService field2;
        public readonly IPersonalSettingsService field3;
        public readonly IGuidService field4;
        public readonly IOrganizationTrustedService field5;
        public readonly IModelConverter<User> field6;
        public readonly IRefreshTokenService field7;
        public readonly IOrganizationAddonService field8;
        public readonly IUniqueUserService field9;
        public readonly IUniqueUserPhotoFileRepository field10;
        public readonly IUserInfoTokenService field11;
        public readonly IFeatureToggleService field12;
        public readonly ILastSignInService field13;
        public readonly IOrganizationWorkweekService field14;
        public readonly IUserService field15;
        public readonly ITermsOfServiceApprovalService field16;
        public readonly IAccountService field17;
        public readonly IConnClient field18;
        public readonly IConnUserInfoService field19;
        public readonly IConnUserService field20;
        public readonly IConnUserApplicationsService field21;
        public readonly IGlobalSettingsService field22;
    }


    public class BearerAuthenticationController : AuthenticationControllerBase
    {
        public BearerAuthenticationController(
            IContextService<IPsaContext> arg0,
            ITokenService arg1,
            ICurrentSessionService arg2,
            IPersonalSettingsService arg3,
            IGuidService arg4,
            IOrganizationTrustedService arg5,
            IModelConverter<User> arg6,
            IRefreshTokenService arg7,
            IOrganizationAddonService arg8,
            IUniqueUserService arg9,
            IUniqueUserPhotoFileRepository arg10,
            IUserInfoTokenService arg11,
            IFeatureToggleService arg12,
            IFeatureToggleModelConverter arg13,
            ILastSignInService arg14,
            IOrganizationWorkweekService arg15,
            IUserService arg16,
            ITermsOfServiceApprovalService arg17,
            IPartnerRepository arg18,
            IAccountService arg19,
            IConnClient arg20,
            IConnUserInfoService arg21,
            IConnUserService arg22,
            IConnUserApplicationsService arg23,
            IGlobalSettingsService arg24
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg14, arg15, arg16,
            arg17, arg19, arg20, arg21, arg22, arg23, arg24)
        {
            field23 = arg23;
            field24 = arg24;
        }

        public readonly IConnUserApplicationsService field23;
        public readonly IGlobalSettingsService field24;
    }


    public class ExternalAuthenticationController : AuthenticationControllerBase
    {
        public ExternalAuthenticationController(
            IContextService<IPsaContext> arg0,
            IGoogleAuthenticationService arg1,
            ICurrentSessionService arg2,
            ITokenService arg3,
            IPersonalSettingsService arg4,
            IGuidService arg5,
            IOrganizationTrustedService arg6,
            IModelConverter<User> arg7,
            IRefreshTokenService arg8,
            IOrganizationAddonService arg9,
            IUniqueUserService arg10,
            IUniqueUserPhotoFileRepository arg11,
            IMasterUserRepository arg12,
            IForAuthUserRepository arg13,
            IUserInfoTokenService arg14,
            IAuthenticationService arg15,
            IFeatureToggleService arg16,
            ILastSignInService arg17,
            IOrganizationWorkweekService arg18,
            IUserService arg19,
            ITermsOfServiceApprovalService arg20,
            IUserPasswordService arg21,
            IAccountService arg22,
            IWhiteListService arg23,
            IRestSettingsService arg24,
            IConnClient arg25,
            IConnUserInfoService arg26,
            IConnUserService arg27,
            IConnUserApplicationsService arg28,
            IGlobalSettingsService arg29
        ) : base()
        {
            field23 = arg23;
            field24 = arg24;
            field25 = arg25;
            field26 = arg26;
            field27 = arg27;
            field28 = arg28;
            field29 = arg29;
        }

        public readonly IWhiteListService field23;
        public readonly IRestSettingsService field24;
        public readonly IConnClient field25;
        public readonly IConnUserInfoService field26;
        public readonly IConnUserService field27;
        public readonly IConnUserApplicationsService field28;
        public readonly IGlobalSettingsService field29;
    }


    public class DashboardAndParts
    {
        public DashboardAndParts(
        )
        {
        }
    }


    public class HeartBeatController : ApiControllerBase
    {
        public HeartBeatController(
            IHeartBeatService arg0,
            IConnClient arg1,
            IWebHookService arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IHeartBeatService field0;
        public readonly IConnClient field1;
        public readonly IWebHookService field2;
    }


    public class EnumModelBinder
        : IModelBinder
    {
        public EnumModelBinder(
        )
        {
        }

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext) =>
            throw new NotImplementedException();
    }


    public class MaximumValueAttribute : ValidationAttribute
    {
        public MaximumValueAttribute(
            double arg0,
            bool arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly double field0;
        public readonly bool field1;
    }


    public class MinimumValueAttribute : ValidationAttribute
    {
        public MinimumValueAttribute(
            double arg0,
            bool arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly double field0;
        public readonly bool field1;
    }


    public class AuthenticatedUser
    {
        public AuthenticatedUser(
            int? arg0,
            int arg1,
            int arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly int? field0;
        public readonly int field1;
        public readonly int field2;
    }


    public class CollectionWithTotalRowCount
    {
        public CollectionWithTotalRowCount(
        )
        {
        }
    }

    public class ApiControllerBase : ApiController
    {
        public ApiControllerBase()
        {
        }

        public ApiControllerBase(IContextService<IPsaContext> contextService)
        {
            _contextService = contextService;
        }

        protected IContextService<IPsaContext> _contextService;
    }


    public class DocumentationRequiredAttribute : Attribute
    {
        public DocumentationRequiredAttribute(
        )
        {
        }
    }


    public class DocumentationReadOnlyAttribute : Attribute
    {
        public DocumentationReadOnlyAttribute(
        )
        {
        }
    }


    public class CollectionWithHeader
    {
        public CollectionWithHeader(
        )
        {
        }
    }


    public interface IModelReader
        : IEntityReaderEx
    {
    }


    public class ModelBaseWithManageInfo : ModelBase
    {
        public ModelBaseWithManageInfo(
        )
        {
        }
    }


    public class ModelBaseReadOnly
        : IEntity
    {
        public ModelBaseReadOnly(
        )
        {
        }
    }


    public class ModelBase
        : IEntity
    {
        public ModelBase(
        )
        {
        }
    }


    public class ModelWithRequiredNameAndManageInfo : ModelBase
    {
        public ModelWithRequiredNameAndManageInfo(
        )
        {
        }
    }

    public interface IModelReader<TContext, TEntity> : IEntityReaderEx<TContext, TEntity> where TContext : IContext
    {
    }

    public class ModelReader<TContext, TEntity> : EntityReader<TContext, TEntity>, IModelReader<TContext, TEntity>
        where TContext : IContext
    {
    }


    public class ModelBaseWithRequiredGuid
        : IEntity
    {
        public ModelBaseWithRequiredGuid(
        )
        {
        }
    }


    public class ModelWithRequiredName : ModelBase
    {
        public ModelWithRequiredName(
        )
        {
        }
    }


    public class ModelWithName : ModelBase
    {
        public ModelWithName(
        )
        {
        }
    }


    public class UserClaims
    {
    }


    public interface IWebPartTypeInfo
    {
    }


    public interface IDashboardPartTypeInfo
        : IWebPartTypeInfo
    {
    }


    public class DashboardPartTypeFactory
    {
    }


    public class NoCacheFilterAttribute : ActionFilterAttribute
    {
        public NoCacheFilterAttribute(
        )
        {
        }
    }


    public class RequiredAddonAttribute : Attribute
    {
        public RequiredAddonAttribute(
            AddonIdentifier[] arg0,
            AddonOperatorType arg1
        ) : base()
        {
            field0 = arg0;
            field1a = arg1;
        }

        public RequiredAddonAttribute(
            AddonIdentifier arg0
        ) : base()
        {
            field0a = arg0;
        }

        public readonly AddonIdentifier[] field0;
        public readonly AddonIdentifier field0a;
        public readonly AddonOperatorType field1a;
    }


    public class AppAddonFilter : AuthorizationFilterBaseAttribute
    {
        public AppAddonFilter(
            IPsaContextService arg0,
            IAddonIdentifierConverter arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IPsaContextService field0;
        public readonly IAddonIdentifierConverter field1;
    }


    public class RequireHttpsAttribute : AuthorizationFilterBaseAttribute
    {
        public RequireHttpsAttribute(
        )
        {
        }
    }


    public class RequireRefererAttribute : AuthorizationFilterBaseAttribute
    {
        public RequireRefererAttribute(
        )
        {
        }
    }


    public class AuthorizationFilterBaseAttribute : AuthorizationFilterAttribute
    {
    }


    public class AppIgnoreTrialExpiredAttribute : AuthorizationFilterBaseAttribute
    {
        public AppIgnoreTrialExpiredAttribute(
        )
        {
        }
    }


    public class AppTrialExpiredAttribute : AuthorizationFilterBaseAttribute
    {
        public AppTrialExpiredAttribute(
            IPsaContextService arg0,
            IOrganizationAddonService arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IPsaContextService field0;
        public readonly IOrganizationAddonService field1;
    }


    public class AppTosApprovedAttribute : AuthorizationFilterBaseAttribute
    {
        public AppTosApprovedAttribute(
            IPsaContextService arg0,
            ITermsOfServiceApprovalService arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IPsaContextService field0;
        public readonly ITermsOfServiceApprovalService field1;
    }


    public class AppAuthorizeAttribute : AuthorizationFilterBaseAttribute
    {
        public AppAuthorizeAttribute(
            ICurrentSessionService arg0,
            IPsaContextService arg1,
            IOrganizationTrustedService arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICurrentSessionService field0;
        public readonly IPsaContextService field1;
        public readonly IOrganizationTrustedService field2;
    }


    public class WebApiApplication : HttpApplication
    {
        public WebApiApplication(
        )
        {
        }
    }


    public class AccessRightProfileModelConverter
        : IModelConverter<Profile>
    {
        public AccessRightProfileModelConverter(
        )
        {
        }
    }


    public interface IKeywordModelConverter
        : IModelConverter<TagEx>
    {
    }


    public class KeywordModelConverter
        : IKeywordModelConverter
    {
        public KeywordModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
    }


    public interface IOrderConfirmationModelConverter
    {
    }


    public class OrderConfirmationModelConverter
        : IOrderConfirmationModelConverter
    {
        public OrderConfirmationModelConverter(
            ICurrencyBaseService arg0,
            IGuidService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly ICurrencyBaseService field0;
        public readonly IGuidService field1;
    }


    public class UserLicenseModelConverter
        : IUserLicenseModelConverter
    {
        public UserLicenseModelConverter(
        )
        {
        }
    }


    public interface IUserLicenseModelConverter
    {
    }


    public class ScheduledJobModelConverter
        : IModelConverter<BackgroundTask>
    {
        public ScheduledJobModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class ActivityRecurrenceModelConverter
        : IActivityRecurrenceModelConverter
    {
        public ActivityRecurrenceModelConverter(
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


    public interface IAccessRightsModelConverter
        : IModelConverter<AccessRights>
    {
    }


    public class AccessRightModelConverter
        : IAccessRightsModelConverter
    {
        public AccessRightModelConverter(
            IPsaContextService arg0,
            IGuidService arg1,
            IInvoiceStatusService arg2,
            IProfileRepository arg3,
            IProfileRightRepository arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IPsaContextService field0;
        public readonly IGuidService field1;
        public readonly IInvoiceStatusService field2;
        public readonly IProfileRepository field3;
        public readonly IProfileRightRepository field4;
    }


    public interface ITimeEntrySuggestedRowModelConverter
    {
    }


    public class TimeEntrySuggestedRowModelConverter
        : ITimeEntrySuggestedRowModelConverter
    {
        public TimeEntrySuggestedRowModelConverter(
            IGuidService arg0,
            IPsaContextService arg1,
            IUserWeeklyViewRowService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
        public readonly IUserWeeklyViewRowService field2;
    }


    public interface IWorkHourSuggestedRowModelConverter
    {
    }


    public class WorkHourSuggestedRowModelConverter
        : IWorkHourSuggestedRowModelConverter
    {
        public WorkHourSuggestedRowModelConverter(
            IGuidService arg0,
            IPsaContextService arg1,
            IUserWeeklyViewRowService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
        public readonly IUserWeeklyViewRowService field2;
    }


    public interface IUserInactivationInformationModelConverter
    {
    }


    public class UserInactivationInformationModelConverter
        : IUserInactivationInformationModelConverter
    {
        public UserInactivationInformationModelConverter(
        )
        {
        }
    }

    public interface IUserSettingsModelConverter
        : IModelConverter<UserSettings>
    {
    }


    public class UserSettingsModelConverter
        : IUserSettingsModelConverter
    {
        public UserSettingsModelConverter(
            IGuidService arg0,
            IGlobalSettingsService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IGlobalSettingsService field1;
    }


    public interface ISoapApiKeyModelConverter
    {
    }


    public class SoapApiKeyModelConverter
        : ISoapApiKeyModelConverter
    {
        public SoapApiKeyModelConverter(
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

    public class AddonIdentifierConverter
        : IAddonIdentifierConverter
    {
        public AddonIdentifierConverter(
        )
        {
        }
    }


    public class AuthorizedIpAddressModelConverter
        : IModelConverter<AuthorizedIPAddress>
    {
        public AuthorizedIpAddressModelConverter(
            IPsaContextService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IPsaContextService field0;
    }


    public class AddonModelConverter
        : IAddonModelConverter
    {
        public AddonModelConverter(
            IAddonIdentifierConverter arg0
        )
        {
            field0 = arg0;
        }

        public readonly IAddonIdentifierConverter field0;
    }


    public interface IAddonIdentifierConverter
    {
    }


    public interface IAddonModelConverter
    {
    }


    public interface INotificationSettingsModelConverter
    {
    }


    public class NotificationSettingsModelConverter
        : INotificationSettingsModelConverter
    {
        public NotificationSettingsModelConverter(
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


    public interface IOrganizationSettingsModelConverter
    {
    }

    public class OrganizationSettingsModelConverter
        : IOrganizationSettingsModelConverter
    {
        public OrganizationSettingsModelConverter(
        )
        {
        }
    }


    public interface IOrganizationDetailsModelConverter
    {
    }


    public class OrganizationDetailsModelConverter
        : IOrganizationDetailsModelConverter
    {
        public OrganizationDetailsModelConverter(
            IGuidService arg0,
            IFormattingCultureService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IFormattingCultureService field1;
    }


    public class BankAccountModelConverter
        : IModelConverter<BankAccount>
    {
        public BankAccountModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class ApiClientModelConverter
        : IModelConverter<ApiClient>
    {
        public ApiClientModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class CountryRegionModelConverter
        : IModelConverter<CountryRegion>
    {
        public CountryRegionModelConverter(
            ITimeZoneService arg0,
            IGuidService arg1,
            IContextService<IPsaContext> arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ITimeZoneService field0;
        public readonly IGuidService field1;
        public readonly IContextService<IPsaContext> field2;
    }


    public interface IModelWithCreateParameterConverter<T>
    {
    }


    public interface ITravelExpenseReceiptModelConverter
    {
    }


    public class TravelExpenseReceiptModelConverter
        : ITravelExpenseReceiptModelConverter
    {
        public TravelExpenseReceiptModelConverter(
            IUserRepository arg0,
            ITaskRepository arg1,
            IProductRepository arg2,
            IGuidService arg3,
            IContextService<IPsaContext> arg4,
            ITravelExpenseReceiptService arg5
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IUserRepository field0;
        public readonly ITaskRepository field1;
        public readonly IProductRepository field2;
        public readonly IGuidService field3;
        public readonly IContextService<IPsaContext> field4;
        public readonly ITravelExpenseReceiptService field5;
    }


    public class ProjectMemberCostExceptionModelConverter
        : IModelConverter<UserCostPerCase>
    {
        public ProjectMemberCostExceptionModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class NoteModelConverterBase
    {
        public NoteModelConverterBase(
            IGuidService arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
    }


    public interface IProductCountrySettingsModelConverter
    {
    }


    public class ProductCountrySettingsModelConverter
        : IProductCountrySettingsModelConverter
    {
        public ProductCountrySettingsModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class CustomerCountrySettingsModelConverter
        : IModelConverter<AccountCountrySettings>
    {
        public CustomerCountrySettingsModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
    }


    public class FeatureToggleModelConverter
        : IFeatureToggleModelConverter
    {
        public FeatureToggleModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public interface ITravelReimbursementModelConverter
        : IModelConverter<TravelReimbursement>
    {
    }


    public class TravelReimbursementModelConverter
        : ITravelReimbursementModelConverter
    {
        public TravelReimbursementModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
    }


    public interface IFeatureToggleModelConverter
        : IModelConverter<FeatureToggle>
    {
    }


    public interface IPersonalSettingsModelConverter
        : IModelConverter<User>
    {
    }


    public class PersonalSettingsModelConverter
        : IPersonalSettingsModelConverter
    {
        public PersonalSettingsModelConverter(
            IContextService<IPsaContext> arg0,
            IGuidService arg1,
            ILanguageService arg2,
            ICountryService arg3,
            IFormattingCultureService arg4,
            ITimeZoneService arg5,
            IActivityTypeRepository arg6,
            ISettingsRepository arg7
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
        public readonly IGuidService field1;
        public readonly ILanguageService field2;
        public readonly ICountryService field3;
        public readonly IFormattingCultureService field4;
        public readonly ITimeZoneService field5;
        public readonly IActivityTypeRepository field6;
        public readonly ISettingsRepository field7;
    }


    public interface IResourcingOverviewModelConverter
    {
    }


    public class ResourcingOverviewModelConverter
        : IResourcingOverviewModelConverter
    {
        public ResourcingOverviewModelConverter(
            IPsaContextService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IPsaContextService field0;
    }


    public interface IResourceAllocationModelConverter
        : IModelConverter<ResourceAllocation>
    {
    }


    public class ResourceAllocationModelConverter
        : IResourceAllocationModelConverter
    {
        public ResourceAllocationModelConverter(
            IGuidService arg0,
            ICaseMemberService arg1,
            ICaseService arg2,
            IUserService arg3,
            IPsaContextService arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IGuidService field0;
        public readonly ICaseMemberService field1;
        public readonly ICaseService field2;
        public readonly IUserService field3;
        public readonly IPsaContextService field4;
    }


    public class CalendarGroupMemberModelConverter
        : ICalendarGroupMemberModelConverter
    {
        public CalendarGroupMemberModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
    }


    public class CalendarGroupModelConverter
        : ICalendarGroupModelConverter
    {
        public CalendarGroupModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
    }


    public interface ICalendarGroupMemberModelConverter
    {
    }


    public interface ICalendarGroupModelConverter
        : IModelConverter<CalendarGroupModelConverter>
    {
    }


    public interface IKpiComparisonModelConverter
    {
    }


    public class KpiComparisonModelConverter
        : IKpiComparisonModelConverter
    {
        public KpiComparisonModelConverter(
        )
        {
        }
    }


    public class TravelReimbursementStatusModelConverter
        : IModelConverter<TravelReimbursementStatus>
    {
        public TravelReimbursementStatusModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
    }


    public class ResourceModelConverter
        : IModelConverter<Resource>
    {
        public ResourceModelConverter(
        )
        {
        }
    }


    public interface IProjectTravelExpenseModelConverter
        : IModelConverter<Item>
    {
    }


    public class ProjectTravelExpenseModelConverter
        : IProjectTravelExpenseModelConverter
    {
        public ProjectTravelExpenseModelConverter(
            IGuidService arg0,
            IPsaContextService arg1,
            ITaskService arg2,
            IProductService arg3,
            IProductPriceService arg4,
            IPricelistService arg5
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
        public readonly ITaskService field2;
        public readonly IProductService field3;
        public readonly IProductPriceService field4;
        public readonly IPricelistService field5;
    }


    public class LinkModelConverter
        : IModelConverter<Link>
    {
        public LinkModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public interface IProjectsOverviewModelConverter
    {
    }


    public class ProjectsOverviewModelConverter
        : IProjectsOverviewModelConverter
    {
        public ProjectsOverviewModelConverter(
            IPsaContextService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IPsaContextService field0;
    }


    public class ReimbursedWorkHourModelConverter
        : IModelConverter<HourForInvoice>
    {
        public ReimbursedWorkHourModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class ReimbursedProjectTravelExpenseModelConverter
        : IModelConverter<ItemForInvoice>
    {
        public ReimbursedProjectTravelExpenseModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class SpecialUserOptionModelConverter
        : IModelConverter<ListFilterValue>
    {
        public SpecialUserOptionModelConverter(
            IContextService<IPsaContext> arg0,
            IGuidService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IGuidService field1;
    }


    public class ContactRoleModelConverter
        : IModelConverter<ContactRole>
    {
        public ContactRoleModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public interface IBusinessOverviewModelConverter
    {
    }


    public class BusinessOverviewModelConverter
        : IBusinessOverviewModelConverter
    {
        public BusinessOverviewModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public interface IProjectForecastModelConverter
        : IModelConverter<BillingPlan>
    {
    }


    public class ProjectForecastModelConverter
        : IProjectForecastModelConverter
    {
        public ProjectForecastModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public interface IInvoiceModelConverter
        : IModelConverter<Invoice>
    {
    }


    public class InvoiceModelConverter
        : IInvoiceModelConverter
    {
        public InvoiceModelConverter(
            IGuidService arg0,
            IInvoiceHtmlRepository arg1,
            IInvoiceConfigRepository arg2,
            IContactService arg3,
            IAccountService arg4,
            IHtmlSanitizerService arg5
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly IGuidService field0;
        public readonly IInvoiceHtmlRepository field1;
        public readonly IInvoiceConfigRepository field2;
        public readonly IContactService field3;
        public readonly IAccountService field4;
        public readonly IHtmlSanitizerService field5;
    }


    public interface IProjectTotalFeeModelConverter
    {
    }


    public class ProjectTotalFeeModelConverter
        : IProjectTotalFeeModelConverter
    {
        public ProjectTotalFeeModelConverter(
        )
        {
        }
    }


    public interface IOvertimePriceModelConverter
        : IModelConverter<OverTimePrice>
    {
    }


    public class OvertimePriceModelConverter
        : IOvertimePriceModelConverter
    {
        public OvertimePriceModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public interface ITeamProductivityModelConverter
        : IModelConverter<CaseMemberForTeamProductivity>
    {
    }


    public class TeamProductivityModelConverter
        : ITeamProductivityModelConverter
    {
        public TeamProductivityModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class ProjectHourExConverter
        : IModelConverter<HourEx>
    {
        public ProjectHourExConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class CustomerMarketSegmentModelConverter
        : IModelConverter<AccountGroupMemberEx>
    {
        public CustomerMarketSegmentModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public interface IProjectProductModelConverter
        : IModelConverter<CaseProduct>
    {
    }


    public class ProjectProductModelConverter
        : IProjectProductModelConverter
    {
        public ProjectProductModelConverter(
            IGuidService arg0,
            IProductService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IProductService field1;
    }


    public interface IScheduleOverviewModelConverter
    {
    }


    public class ScheduleOverviewModelConverter
        : IScheduleOverviewModelConverter
    {
        public ScheduleOverviewModelConverter(
        )
        {
        }
    }


    public class InvoiceTemplateWithSettingsModelConverter
        : IModelConverter<InvoiceTemplateWithSettingsModel>
    {
        public InvoiceTemplateWithSettingsModelConverter(
            IContextService<IPsaContext> arg0,
            IGuidService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IGuidService field1;
    }


    public class MarketSegmentModelConverter
        : IModelConverter<AccountGroup>
    {
        public MarketSegmentModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public interface IActivityRecurrenceModelConverter
    {
    }


    public class FlatRateModelConverter
        : IModelConverter<TaskEx>
    {
        public FlatRateModelConverter(
            IGuidService arg0,
            IPsaContextService arg1,
            ICaseService arg2,
            ICurrencyService arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
        public readonly ICaseService field2;
        public readonly ICurrencyService field3;
    }


    public class BillingInformationUpdateModelConverter
        : IModelConverter<BillingInformationUpdate>
    {
        public BillingInformationUpdateModelConverter(
            IContextService<IPsaContext> arg0,
            IGuidService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IGuidService field1;
    }


    public class ProjectBillingCustomerModelConverter
        : IModelConverter<CaseBillingAccountEx>
    {
        public ProjectBillingCustomerModelConverter(
            IContextService<IPsaContext> arg0,
            IGuidService arg1,
            ICaseBillingAccountService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IGuidService field1;
        public readonly ICaseBillingAccountService field2;
    }


    public class SharedDashboardAccessRightProfileModelConverter
        : IModelConverter<ProfileDashboard>
    {
        public SharedDashboardAccessRightProfileModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public interface IInvoicesOverviewModelConverter
    {
    }


    public class InvoicesOverviewModelConverter
        : IInvoicesOverviewModelConverter
    {
        public InvoicesOverviewModelConverter(
        )
        {
        }
    }


    public interface ISalesOverviewModelConverter
    {
    }


    public class SalesOverviewModelConverter
        : ISalesOverviewModelConverter
    {
        public SalesOverviewModelConverter(
        )
        {
        }
    }


    public class ProposalBillingPlanModelConverter
        : IModelConverter<ProposalBillingPlanModel>
    {
        public ProposalBillingPlanModelConverter(
            IContextService<IPsaContext> arg0,
            IGuidService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IGuidService field1;
    }


    public class CustomersOverviewModelConverter
        : ICustomersOverviewModelConverter
    {
        public CustomersOverviewModelConverter(
        )
        {
        }
    }


    public interface ICustomersOverviewModelConverter
    {
    }


    public class ProposalProjectPlanModelBaseConverter
        : IModelConverter<ProposalProjectPlanModel>
    {
        public ProposalProjectPlanModelBaseConverter(
            IContextService<IPsaContext> arg0,
            IGuidService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IGuidService field1;
    }


    public class TimeEntryTypeModelConverter
        : IModelConverter<TimeEntryType>
    {
        public TimeEntryTypeModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class ContactCommunicationModelConverter
        : IModelConverter<ContactCommunicationMethod>
    {
        public ContactCommunicationModelConverter(
            IGuidService arg0,
            IContactService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IContactService field1;
    }


    public class CommunicationTypeModelConverter
        : IModelConverter<CommunicationMethod>
    {
        public CommunicationTypeModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public interface IFlextimeModelConverter
    {
    }


    public class FlextimeModelConverter
        : IFlextimeModelConverter
    {
        public FlextimeModelConverter(
        )
        {
        }
    }


    public interface IOrganizationModelConverter
    {
    }


    public class OrganizationModelConverter
        : IOrganizationModelConverter
    {
        public OrganizationModelConverter(
            IGuidService arg0,
            IPsaContextService arg1,
            IOrganizationAddonService arg2,
            IUserService arg3,
            ITermsOfServiceApprovalService arg4,
            IOrganizationTrustedService arg5,
            IGlobalSettingsService arg6,
            IAddonIdentifierConverter arg7,
            IExternalIdentifierService arg8
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

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
        public readonly IOrganizationAddonService field2;
        public readonly IUserService field3;
        public readonly ITermsOfServiceApprovalService field4;
        public readonly IOrganizationTrustedService field5;
        public readonly IGlobalSettingsService field6;
        public readonly IAddonIdentifierConverter field7;
        public readonly IExternalIdentifierService field8;
    }


    public class InvoiceSettingsModelConverterBase
    {
        public InvoiceSettingsModelConverterBase(
            IContextService<IPsaContext> arg0,
            IGuidService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IGuidService field1;
    }

    public class TemporaryWorkHourModelConverter : IModelConverter<TemporaryHour>
    {
        public TemporaryWorkHourModelConverter(IContextService<IPsaContext> contextService, IGuidService guidService, IUserRepository userRepository, IOvertimeRepository overtimeRepository)
        {
        }
    }

    public class InvoiceTemplateSettingsModelConverter : InvoiceSettingsModelConverterBase
        , IModelConverter<InvoiceTemplateConfig>
    {
        public InvoiceTemplateSettingsModelConverter(
            IContextService<IPsaContext> arg0,
            IGuidService arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class InvoiceTemplateModelConverter
        : IModelConverter<InvoiceTemplate>
    {
        public InvoiceTemplateModelConverter(
            IContextService<IPsaContext> arg0,
            IGuidService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IGuidService field1;
    }


    public class ActivityParticipantModelConverter
        : IActivityParticipantModelConverter
    {
        public ActivityParticipantModelConverter(
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


    public interface IWorkdayModelConverter
    {
    }


    public class TemporaryProjectFeesModelConverter : IModelConverter<TemporaryItem>
    {
        public TemporaryProjectFeesModelConverter(
            IContextService<IPsaContext> arg0,
            IGuidService arg1
        )
        {
        }
    }


    public class TemporaryProjectTravelsModelConverter : IModelConverter<TemporaryProjectTravelExpenseModel>
    {
        public TemporaryProjectTravelsModelConverter(
            IContextService<IPsaContext> arg0,
            IGuidService arg1
        )
        {
        }
    }


    public class TermsOfServiceApprovalModelConverter
        : IModelConverter<TermsOfServiceApproval>
    {
        public TermsOfServiceApprovalModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
    }


    public class FinancialsClassModelConverter
        : IModelConverter<FinancialsClassModel>
    {
        public FinancialsClassModelConverter(
        )
        {
        }
    }


    public interface IActivityParticipantModelConverter
    {
    }


    public class InvoiceSettingsModelConverter : InvoiceSettingsModelConverterBase
        , IModelConverter<InvoiceConfig>
    {
        public InvoiceSettingsModelConverter(
            IContextService<IPsaContext> arg0,
            IGuidService arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class ProjectTaskStatusModelConverter
        : IModelConverter<ActivityStatus>
    {
        public ProjectTaskStatusModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class QuickSearchResultModelConverter
        : IQuickSearchResultModelConverter
    {
        public QuickSearchResultModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class ActivityModelConverter
        : IModelConverter<Activity>
    {
        public ActivityModelConverter(
            IPsaContextService arg0,
            IGuidService arg1,
            IHtmlSanitizerService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IPsaContextService field0;
        public readonly IGuidService field1;
        public readonly IHtmlSanitizerService field2;
    }


    public class ActivityTypeModelConverter
        : IModelConverter<ActivityType>
    {
        public ActivityTypeModelConverter(
        )
        {
        }
    }


    public class ConverterUtil
    {
        public ConverterUtil(
        )
        {
        }
    }


    public class CreateInvoiceModelConverter
        : IModelConverter<CreateInvoice>
    {
        public CreateInvoiceModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public interface IQuickSearchResultModelConverter
    {
    }


    public interface IInvoiceTaxBreakdownModelConverter
        : IModelConverter<TaxBreakdown>
    {
    }


    public class InvoiceTaxBreakdownModelConverter : TaxBreakdownModelConverterBase
        , IInvoiceTaxBreakdownModelConverter
    {
        public InvoiceTaxBreakdownModelConverter(
        )
        {
        }
    }


    public interface IProposalTaxBreakdownModelConverter
        : IModelConverter<ProposalTaxBreakdownModel>
    {
    }


    public class ProposalTaxBreakdownModelConverter : TaxBreakdownModelConverterBase
        , IProposalTaxBreakdownModelConverter
    {
        public ProposalTaxBreakdownModelConverter(
        )
        {
        }
    }


    public class TaxBreakdownModelConverterBase
    {
    }


    public interface IFileModelConverter
        : IModelConverter<FileModel>
    {
    }


    public class FileModelConverter
        : IFileModelConverter
    {
        public FileModelConverter(
            IGuidService arg0,
            IPsaContextService arg1,
            IOfferFileService arg2,
            IUserService arg3,
            IFileService arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
        public readonly IOfferFileService field2;
        public readonly IUserService field3;
        public readonly IFileService field4;
    }


    public class DashboardWithPartsModelConverter
        : IModelConverter<DashboardAndParts>
    {
        public DashboardWithPartsModelConverter(
            IGuidService arg0,
            IModelConverter<DashboardPart> arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IModelConverter<DashboardPart> field1;
    }


    public class DashboardModelConverter
        : IModelConverter<Dashboard>
    {
        public DashboardModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class DashboardPartModelConverter
        : IModelConverter<DashboardPart>
    {
        public DashboardPartModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
    }


    public class InvoiceRowModelConverter
        : IModelConverter<InvoiceRow>
    {
        public InvoiceRowModelConverter(
            IContextService<IPsaContext> arg0
        )
        {
            field0 = arg0;
        }

        public readonly IContextService<IPsaContext> field0;
    }


    public class HolidayModelConverter
        : IModelConverter<WorkingDayException>
    {
        public HolidayModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class ProposalTemplateModelConverter
        : IModelConverter<Offer>
    {
        public ProposalTemplateModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class ProposalWorkhourModelConverter
        : IModelConverter<OfferTask>
    {
        public ProposalWorkhourModelConverter(
            IGuidService arg0,
            IOfferItemService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IOfferItemService field1;
    }


    public class ProposalSubtotalModelConverter
        : IModelConverter<OfferSubtotal>
    {
        public ProposalSubtotalModelConverter(
            IGuidService arg0,
            IProductService arg1,
            IOfferSubtotalService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IGuidService field0;
        public readonly IProductService field1;
        public readonly IOfferSubtotalService field2;
    }


    public class ProposalStatusModelConverter
        : IModelConverter<ProposalStatus>
    {
        public ProposalStatusModelConverter(
            IGuidService arg0,
            IContextService<IPsaContext> arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IContextService<IPsaContext> field1;
    }


    public class InvoiceStatusModelConverter
        : IModelConverter<InvoiceStatus>
    {
        public InvoiceStatusModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class ProposalModelConverter
        : IModelConverter<ProposalModel>
    {
        public ProposalModelConverter(
            IGuidService arg0,
            IHtmlSanitizerService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IHtmlSanitizerService field1;
    }


    public class StatusHistoryModelConverter
        : IModelConverter<CaseStatusHistory>
    {
        public StatusHistoryModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
    }


    public class PhaseStatusTypeModelConverter
        : IModelConverter<TaskStatusType>
    {
        public PhaseStatusTypeModelConverter(
        )
        {
        }
    }


    public class CostCenterModelConverter
        : IModelConverter<CostCenter>
    {
        public CostCenterModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public interface IProjectWorktypeModelConverter
        : IModelConverter<CaseWorkType>
    {
    }


    public class ProjectWorkTypeModelConverter
        : IProjectWorktypeModelConverter
    {
        public ProjectWorkTypeModelConverter(
            IGuidService arg0,
            IWorkTypeService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IWorkTypeService field1;
    }


    public interface IProjectRecurringFeeModelConverter
        : IModelConverter<RecurringItem>
    {
    }


    public class ProjectRecurringFeeModelConverter
        : IProjectRecurringFeeModelConverter
    {
        public ProjectRecurringFeeModelConverter(
            IGuidService arg0,
            IPsaContextService arg1,
            IProductService arg2,
            IItemService arg3,
            IProductPriceService arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
        public readonly IProductService field2;
        public readonly IItemService field3;
        public readonly IProductPriceService field4;
    }


    public interface IProjectFeeModelConverter
        : IModelConverter<ProjectFeeModel>
    {
    }


    public class ProjectFeeModelConverter : ProjectFeeModelConverterBase
        , IProjectFeeModelConverter
    {
        public ProjectFeeModelConverter(
            IGuidService arg0,
            IPsaContextService arg1,
            IProductService arg2,
            IItemService arg3,
            IProductPriceService arg4,
            ITaskService arg5,
            IPricelistService arg6
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6)
        {
        }
    }


    public interface IProductModelConverter
        : IModelConverter<ProductModel>
    {
    }


    public class ProductModelConverter : ProductModelConverterBase,
        IProductModelConverter,
        IModelConverter<ProductForCase>
    {
        public ProductModelConverter(
            IGuidService arg0
        ) : base(arg0)
        {
        }
    }


    public class PhaseTreeFavoritePhaseModelConverter
        : IModelConverter<PhaseTreeFavoritePhaseModel>
    {
        public PhaseTreeFavoritePhaseModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
    }


    public class PhaseTreePhaseModelConverter
        : IModelConverter<TreeTask>
    {
        public PhaseTreePhaseModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
    }


    public class ProposalFeeModelConverter
        : IModelConverter<OfferItem>
    {
        public ProposalFeeModelConverter(
            IGuidService arg0,
            IOfferItemService arg1,
            IOfferService arg2,
            IItemService arg3,
            IProductService arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IGuidService field0;
        public readonly IOfferItemService field1;
        public readonly IOfferService field2;
        public readonly IItemService field3;
        public readonly IProductService field4;
    }


    public class TimeEntryModelConverter
        : IModelConverter<TimeEntry>
    {
        public TimeEntryModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
    }


    public class HourModelBaseConverter
        : IModelConverter<HourModelBase>
    {
        public HourModelBaseConverter(
            IGuidService arg0,
            IContextService<IPsaContext> arg1,
            IUserRepository arg2,
            IOvertimeRepository arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IGuidService field0;
        public readonly IContextService<IPsaContext> field1;
        public readonly IUserRepository field2;
        public readonly IOvertimeRepository field3;
    }


    public class WorkTypeModelConverter
        : IModelConverter<WorkType>, IModelConverter<WorktypeForCase>
    {
        public WorkTypeModelConverter(
            IGuidService arg0,
            ISalesAccountService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly ISalesAccountService field1;
    }

    public class WorkdayModelConverter : IModelConverter<Workday>, IWorkdayModelConverter
    {
        private readonly IGuidService _GuidService;

        public WorkdayModelConverter(IGuidService guidService)
        {
            _GuidService = guidService;
        }
    }

    public class PricelistVersionModelConverter
        : IModelConverter<PricelistVersion>
    {
        public PricelistVersionModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class ProjectWorkHourPriceModelConverter : WorkHourPriceModelConverter
    {
        public ProjectWorkHourPriceModelConverter(
            IGuidService arg0
        ) : base(arg0)
        {
        }
    }


    public class WorkHourPriceModelConverter
        : IModelConverter<WorkHourPriceModel>, IModelConverter<WorkPriceEx>
    {
        public WorkHourPriceModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class TravelPriceModelConverter : ProductPriceModelConverterBase
        , IModelConverter<ProductPriceEx>
    {
        public TravelPriceModelConverter(
            IGuidService arg0
        ) : base(arg0)
        {
        }
    }


    public class ProductPriceModelConverter : ProductPriceModelConverterBase
        , IModelConverter<ProductPriceModel>
    {
        public ProductPriceModelConverter(
            IGuidService arg0
        ) : base(arg0)
        {
        }
    }


    public class ProjectFeeModelConverterBase
    {
        public ProjectFeeModelConverterBase(
            IGuidService arg0,
            IPsaContextService arg1,
            IProductService arg2,
            IItemService arg3,
            IProductPriceService arg4,
            ITaskService arg5,
            IPricelistService arg6
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

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
        public readonly IProductService field2;
        public readonly IItemService field3;
        public readonly IProductPriceService field4;
        public readonly ITaskService field5;
        public readonly IPricelistService field6;
    }


    public class ProductModelConverterBase
    {
        public ProductModelConverterBase(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class ProductPriceModelConverterBase
    {
        public ProductPriceModelConverterBase(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class TravelExpenseModelConverter : ProductModelConverterBase
        , IModelConverter<Product>
    {
        public TravelExpenseModelConverter(
            IGuidService arg0
        ) : base(arg0)
        {
        }
    }


    public class SalesReceivableAccountModelConverter
        : IModelConverter<CurrencyEx>
    {
        public SalesReceivableAccountModelConverter(
        )
        {
        }
    }


    public class ValueAddedTaxModelConverter
        : IModelConverter<Tax>
    {
        public ValueAddedTaxModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly IPsaContextService field1;
    }


    public class OvertimeModelConverter
        : IModelConverter<OverTime>
    {
        public OvertimeModelConverter(
            IContextService<IPsaContext> arg0,
            IGuidService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IGuidService field1;
    }


    public class ProductCategoryModelConverter
        : IModelConverter<ProductCategory>
    {
        public ProductCategoryModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class BusinessUnitModelConverter
        : IModelConverter<BusinessUnit>
    {
        public BusinessUnitModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class CurrencyBaseModelConverter
        : IModelConverter<CurrencyBase>
    {
        public CurrencyBaseModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class CurrencyModelConverter
        : IModelConverter<Currency>
    {
        public CurrencyModelConverter(
            IContextService<IPsaContext> arg0,
            IGuidService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IGuidService field1;
    }


    public class SalesAccountModelConverter
        : IModelConverter<SalesAccount>
    {
        public SalesAccountModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class WorkContractModelConverter
        : IModelConverter<Employment>
    {
        public WorkContractModelConverter(
            IGuidService arg0,
            IOrganizationCompanyService arg1,
            IPsaContextService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IGuidService field0;
        public readonly IOrganizationCompanyService field1;
        public readonly IPsaContextService field2;
    }


    public class PhaseMemberModelConverter : 
            IModelConverter<CaseMember>,
            IModelConverter<TaskMember>
    {
        public PhaseMemberModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class CollaborationNoteModelConverter : NoteModelConverterBase
        , IModelConverter<INote>
    {
        public CollaborationNoteModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class ContactModelConverter
        : IModelConverter<Contact>
    {
        public ContactModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class CountryModelConverter
        : IModelConverter<Country>
    {
        public CountryModelConverter(
            ICurrencyBaseService arg0,
            ILanguageService arg1,
            ITimeZoneService arg2,
            IGuidService arg3,
            IContextService<IPsaContext> arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly ICurrencyBaseService field0;
        public readonly ILanguageService field1;
        public readonly ITimeZoneService field2;
        public readonly IGuidService field3;
        public readonly IContextService<IPsaContext> field4;
    }


    public class AddressModelConverter
        : IModelConverter<Address>
    {
        public AddressModelConverter(
            IGuidService arg0,
            IContextService<IPsaContext> arg1,
            ICountryRegionService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IGuidService field0;
        public readonly IContextService<IPsaContext> field1;
        public readonly ICountryRegionService field2;
    }


    public class CustomerModelConverter
        : ICustomerModelConverter
    {
        public CustomerModelConverter(
            IGuidService arg0,
            IAddressRepository arg1,
            IHtmlSanitizerService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IGuidService field0;
        public readonly IAddressRepository field1;
        public readonly IHtmlSanitizerService field2;
    }


    public class FormattingCultureModelConverter
        : IModelConverter<FormatingCulture>
    {
        public FormattingCultureModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class IndustryModelConverter
        : IModelConverter<Industry>
    {
        public IndustryModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public interface ICustomerModelConverter
        : IModelConverter<Customer>
    {
    }


    public interface IMenuModelConverter
    {
    }


    public class MenuModelConverter
        : IMenuModelConverter
    {
        public MenuModelConverter(
            IPsaContextService arg0,
            IGuidService arg1,
            IReportService arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IPsaContextService field0;
        public readonly IGuidService field1;
        public readonly IReportService field2;
    }

    public interface IModelConverter<T>
    {

    }

    public class ProjectWorkHourModelConverter : IModelConverter<CaseHour>
    {
        private readonly IGuidService _GuidService;

        public ProjectWorkHourModelConverter(IGuidService guidService)
        {
            _GuidService = guidService;
        }
    }


    public class ProjectModelConverter
        : IModelConverter<Project>
    {
        public ProjectModelConverter(
            IAccountService arg0,
            ICaseStatusTypeService arg1,
            IGuidService arg2,
            IContactRepository arg3,
            IHtmlSanitizerService arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly IAccountService field0;
        public readonly ICaseStatusTypeService field1;
        public readonly IGuidService field2;
        public readonly IContactRepository field3;
        public readonly IHtmlSanitizerService field4;
    }


    public class LanguageModelConverter
        : IModelConverter<Language>
    {
        public LanguageModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class LeadSourceModelConverter
        : IModelConverter<LeadSource>
    {
        public LeadSourceModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class PhaseModelConverter
        : IModelConverter<Task>
    {
        public PhaseModelConverter(
            IGuidService arg0,
            ITaskService arg1,
            ITaskStatusService arg2,
            ITaskStatusTypeService arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IGuidService field0;
        public readonly ITaskService field1;
        public readonly ITaskStatusService field2;
        public readonly ITaskStatusTypeService field3;
    }


    public class PricelistModelConverter
        : IModelConverter<Pricelist>
    {
        public PricelistModelConverter(
            ICurrencyService arg0,
            IGuidService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly ICurrencyService field0;
        public readonly IGuidService field1;
    }


    public class ProjectNoteModelConverter : NoteModelConverterBase
        , IModelConverter<NoteType>
    {
        public ProjectNoteModelConverter(
            IGuidService arg0,
            IPsaContextService arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class ProjectStatusTypeModelConverter
        : IModelConverter<CaseStatusType>
    {
        public ProjectStatusTypeModelConverter(
        )
        {
        }
    }


    public class SalesStatusHistoryModelConverter
        : IModelConverter<SalesStatus>
    {
        public SalesStatusHistoryModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class SalesStatusTypeModelConverter
        : IModelConverter<SalesProcess>
    {
        public SalesStatusTypeModelConverter(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class TimeZoneModelConverter
        : IModelConverter<TimeZone>
    {
        public TimeZoneModelConverter(
            IGuidService arg0,
            ITimeZoneService arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IGuidService field0;
        public readonly ITimeZoneService field1;
    }


    public class ChangeUserLicenseModel
    {
        public ChangeUserLicenseModel(
            int arg1
        )
        {
            field1 = arg1;
        }

        public readonly int field1;
    }


    public class ChangeUserLicensesResultModel
    {
        public ChangeUserLicensesResultModel(
            IEnumerable<UserLicenseModel> arg0,
            OrderConfirmationModel arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IEnumerable<UserLicenseModel> field0;
        public readonly OrderConfirmationModel field1;
    }


    public class OrderConfirmationModel
    {
        public OrderConfirmationModel(
            OrderConfirmationCurrency arg0,
            IEnumerable<OrderConfirmationRow> arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly OrderConfirmationCurrency field0;
        public readonly IEnumerable<OrderConfirmationRow> field1;
    }


    public class UserLicenseLimitedViewModel
    {
        public UserLicenseLimitedViewModel(
            int arg1
        )
        {
            field1 = arg1;
        }

        public readonly int field1;
    }


    public class UserLicenseModel
    {
        public UserLicenseModel(
            int arg1,
            int arg2,
            int arg3,
            int arg4
        )
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public readonly int field1;
        public readonly int field2;
        public readonly int field3;
        public readonly int field4;
    }


    public class CalendarSyncSettingsModel
    {
        public CalendarSyncSettingsModel(
        )
        {
        }
    }


    public class UsageModel
        : IEntity
    {
        public UsageModel(
        )
        {
        }
    }


    public class TimeEntrySuggestedRowModel : ModelBase
    {
        public TimeEntrySuggestedRowModel(
        )
        {
        }
    }


    public class ProfileRightsModel
    {
        public ProfileRightsModel(
        )
        {
        }
    }


    public class SalesCasePredictionModel
    {
        public SalesCasePredictionModel(
        )
        {
        }
    }


    public class WorkContractBaseModel : ModelBase
    {
        public WorkContractBaseModel(
        )
        {
        }
    }


    public class WorkHourSuggestedRowModel : ModelBase
    {
        public WorkHourSuggestedRowModel(
        )
        {
        }
    }


    public class CalendarSyncRequestModel
    {
        public CalendarSyncRequestModel(
        )
        {
        }
    }


    public class ReportSetFavoriteRequestModel
    {
        public ReportSetFavoriteRequestModel(
        )
        {
        }
    }


    public class ReportCopyRequestModel
    {
        public ReportCopyRequestModel(
        )
        {
        }
    }


    public class ProposalCopyRequestModel
    {
        public ProposalCopyRequestModel(
        )
        {
        }
    }


    public class SuggestedWorkHoursModel
    {
        public SuggestedWorkHoursModel(
        )
        {
        }
    }


    public class PhaseResourceAllocationHoursModel
    {
        public PhaseResourceAllocationHoursModel(
        )
        {
        }
    }


    public class WorkdayCopyRequestModel
    {
        public WorkdayCopyRequestModel(
        )
        {
        }
    }


    public class KpiFormulaModel : ModelWithName
    {
        public KpiFormulaModel(
        )
        {
        }
    }


    public class WorkweekSettingsModel
    {
        public WorkweekSettingsModel(
        )
        {
        }
    }


    public class UserSettingsModel : ModelBase
    {
        public UserSettingsModel(
        )
        {
        }
    }


    public class ReCaptchaVerificationModel
    {
        public ReCaptchaVerificationModel(
        )
        {
        }
    }


    public class ReCaptchaVerificationResponseModel
    {
        public ReCaptchaVerificationResponseModel(
        )
        {
        }
    }


    public class SoapApiAccessModel : ModelBase
    {
        public SoapApiAccessModel(
        )
        {
        }
    }


    public class UserInactivationInformationModel
    {
        public UserInactivationInformationModel(
        )
        {
        }
    }


    public class SoapApiKeyModel
    {
        public SoapApiKeyModel(
        )
        {
        }
    }


    public class EmailAddressChangeInformationModel
    {
        public EmailAddressChangeInformationModel(
        )
        {
        }
    }


    public class FlextimeAdjustmentModel : ModelBase
    {
        public FlextimeAdjustmentModel(
        )
        {
        }
    }


    public class AddonsModel
    {
        public AddonsModel(
            IEnumerable<AddonModel> arg0
        )
        {
            field0 = arg0;
        }

        public readonly IEnumerable<AddonModel> field0;
    }


    public class AddonModel
    {
        public AddonModel(
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


    public class EmailAddressChangeModel
    {
        public EmailAddressChangeModel(
        )
        {
        }
    }


    public class InvoiceKpiValueModel : ModelWithName
    {
        public InvoiceKpiValueModel(
        )
        {
        }
    }


    public class IpAddressModel
    {
        public IpAddressModel(
        )
        {
        }
    }


    public class AuthorizedIpAddressModel : ModelBase
    {
        public AuthorizedIpAddressModel(
        )
        {
        }
    }


    public class BankAccountModel : ModelBase
    {
        public BankAccountModel(
        )
        {
        }
    }


    public class ApiClientModel : ModelBaseWithManageInfo
    {
        public ApiClientModel(
        )
        {
        }
    }


    public class ApiClientSecretModel
    {
        public ApiClientSecretModel(
        )
        {
        }
    }


    public class BusinessUnitModelBase : ModelWithName
    {
        public BusinessUnitModelBase(
        )
        {
        }
    }


    public class OrganizationSettingsModel
        : IEntity
    {
        public OrganizationSettingsModel(
        )
        {
        }
    }


    public interface ICalendarGroupMemberModel
    {
    }


    public class CalendarGroupMemberModel : ModelWithName
        , ICalendarGroupMemberModel
    {
        public CalendarGroupMemberModel(
        )
        {
        }
    }


    public interface IProjectBaseModel
    {
    }


    public class ProjectModelBase : ModelWithRequiredNameAndManageInfo
        , IProjectBaseModel
    {
        public ProjectModelBase(
        )
        {
        }
    }


    public class OrganizationDetailsModel : ModelBase
    {
        public OrganizationDetailsModel(
        )
        {
        }
    }


    public class CountryRegionModel : ModelWithName
    {
        public CountryRegionModel(
        )
        {
        }
    }


    public class NotificationSettingsModel
    {
        public NotificationSettingsModel(
        )
        {
        }
    }


    public class ProjectMemberCostExceptionModel : ModelBase
    {
        public ProjectMemberCostExceptionModel(
        )
        {
        }
    }


    public class LogoModel : ModelBase
    {
        public LogoModel(
        )
        {
        }
    }


    public class NoteModelBase : ModelBase
    {
        public NoteModelBase(
        )
        {
        }
    }


    public class SimpleCurrencyModel : ModelWithName
    {
        public SimpleCurrencyModel(
        )
        {
        }
    }


    public class CustomerCountrySettingsModel : ModelBase
    {
        public CustomerCountrySettingsModel(
        )
        {
        }
    }


    public class FeatureToggleModel
    {
        public FeatureToggleModel(
        )
        {
        }
    }


    public class FileQuotaInformationModel
    {
        public FileQuotaInformationModel(
        )
        {
        }
    }


    public class ProjectTravelExpenseFileModel : FileModelBase
    {
        public ProjectTravelExpenseFileModel(
        )
        {
        }
    }


    public class KpiComparisonModel
    {
        public KpiComparisonModel(
        )
        {
        }
    }


    public class ProductCountrySettingsModelBase : ModelBase
    {
        public ProductCountrySettingsModelBase(
        )
        {
        }
    }


    public class ProductCountrySettingsModel : ProductCountrySettingsModelBase
    {
        public ProductCountrySettingsModel(
        )
        {
        }
    }


    public class TravelExpenseCountrySettingsModel : ProductCountrySettingsModelBase
    {
        public TravelExpenseCountrySettingsModel(
        )
        {
        }
    }


    public class TravelExpenseReceiptBaseModel : ModelBaseWithRequiredGuid
    {
        public TravelExpenseReceiptBaseModel(
        )
        {
        }
    }


    public class TravelExpenseReceiptModel : TravelExpenseReceiptBaseModel
    {
        public TravelExpenseReceiptModel(
        )
        {
        }
    }


    public class TravelExpenseReceiptDetailModel : TravelExpenseReceiptModel
    {
        public TravelExpenseReceiptDetailModel(
        )
        {
        }
    }


    public class TravelReimbursementStatusModel : ModelWithRequiredName
    {
        public TravelReimbursementStatusModel(
        )
        {
        }
    }


    public class TravelReimbursementModel : ModelBaseWithManageInfo
    {
        public TravelReimbursementModel(
        )
        {
        }
    }


    public class TermsOfServiceApprovalModel : ModelBase
    {
        public TermsOfServiceApprovalModel(
        )
        {
        }
    }


    public class PhaseCriteriaModel
    {
        public PhaseCriteriaModel(
        )
        {
        }
    }


    public class PhaseMembersFromBusinessUnitUsersModel
    {
        public PhaseMembersFromBusinessUnitUsersModel(
        )
        {
        }
    }


    public class ResourceAllocationProjectSummaryModel
    {
        public ResourceAllocationProjectSummaryModel(
        )
        {
        }
    }


    public class ResourcingOverviewModel
    {
        public ResourcingOverviewModel(
        )
        {
        }
    }


    public class ResourceAllocationCriteriaModel
    {
        public ResourceAllocationCriteriaModel(
        )
        {
        }
    }


    public class ResourceAllocationProjectCriteriaModel
    {
        public ResourceAllocationProjectCriteriaModel(
        )
        {
        }
    }


    public class ResourceAllocationPhaseCriteriaModel
    {
        public ResourceAllocationPhaseCriteriaModel(
        )
        {
        }
    }


    public class ResourceAllocationPhaseSummaryModel
    {
        public ResourceAllocationPhaseSummaryModel(
        )
        {
        }
    }


    public class ResourceAllocationUserAllocationModel
    {
        public ResourceAllocationUserAllocationModel(
        )
        {
        }
    }


    public class ResourceAllocationWorkloadTotalModel
    {
        public ResourceAllocationWorkloadTotalModel(
        )
        {
        }
    }


    public class ResourceAllocationWorkloadModel
    {
        public ResourceAllocationWorkloadModel(
        )
        {
        }
    }


    public class AbsenceModel : ModelBase
    {
        public AbsenceModel(
        )
        {
        }
    }


    public class FreeTextModel
        : IEntity
    {
        public FreeTextModel(
        )
        {
        }
    }


    public class MassDownloadModel
    {
        public MassDownloadModel(
        )
        {
        }
    }


    public class PersonModelBase : ModelBaseWithManageInfo
    {
        public PersonModelBase(
        )
        {
        }
    }


    public class FinancialsClassModel
    {
    }


    public class FinancialsCustomerClassModel : FinancialsClassModel
    {
        public FinancialsCustomerClassModel(
        )
        {
        }
    }


    public class FinancialsTravelExpenseClassModel : FinancialsClassModel
    {
        public FinancialsTravelExpenseClassModel(
        )
        {
        }
    }


    public class FinancialsProductClassModel : FinancialsClassModel
    {
        public FinancialsProductClassModel(
        )
        {
        }
    }


    public class FinancialsWorkTypeClassModel : FinancialsClassModel
    {
        public FinancialsWorkTypeClassModel(
        )
        {
        }
    }


    public class WorkHourEmailModel
    {
        public WorkHourEmailModel(
        )
        {
        }
    }


    public class CalendarGroupModel : ModelWithName
    {
        public CalendarGroupModel(
        )
        {
        }
    }


    public class DateRangeModel
        : IEntity
    {
        public DateRangeModel(
        )
        {
        }

        public DateRangeModel(
            TimePeriod arg0
        )
        {
            field0a = arg0;
        }

        public DateRangeModel(
            DateTime? arg0,
            DateTime? arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0a;
        public readonly DateTime? field0;
        public readonly DateTime? field1;
    }


    public class ResourceModel : ModelWithRequiredName
    {
        public ResourceModel(
        )
        {
        }
    }


    public class CalendarSyncResponseModel
    {
        public CalendarSyncResponseModel(
        )
        {
        }
    }


    public class LinkModel : ModelWithRequiredName
    {
        public LinkModel(
        )
        {
        }
    }


    public class ReimbursedWorkHourModel : ModelBase
    {
        public ReimbursedWorkHourModel(
        )
        {
        }
    }


    public class ReimbursedProjectTravelExpenseModel : ReimbursedProjectFeeModelBase
    {
        public ReimbursedProjectTravelExpenseModel(
        )
        {
        }
    }


    public class ReimbursedProjectFeeModel : ReimbursedProjectFeeModelBase
    {
        public ReimbursedProjectFeeModel(
        )
        {
        }
    }


    public class ReimbursedProjectFeeModelBase : ModelBase
    {
        public ReimbursedProjectFeeModelBase(
        )
        {
        }
    }


    public class ProjectFileCopyModel
    {
        public ProjectFileCopyModel(
        )
        {
        }
    }


    public class ActivityRecurrenceModel
    {
        public ActivityRecurrenceModel(
        )
        {
        }
    }


    public class ActivityParticipantModel : ModelBase
    {
        public ActivityParticipantModel(
        )
        {
        }
    }


    public class AccessRightProfileModel : ModelWithName
    {
        public AccessRightProfileModel(
        )
        {
        }
    }


    public class ProjectCopyModel
    {
        public ProjectCopyModel(
        )
        {
        }
    }


    public class MonthlyPlanModel
    {
        public MonthlyPlanModel(
        )
        {
        }
    }


    public class CustomerMonthlyPlanModel : MonthlyPlanModel
    {
        public CustomerMonthlyPlanModel(
        )
        {
        }
    }


    public class ProjectsToInvoiceCriteriaModel
    {
        public ProjectsToInvoiceCriteriaModel(
        )
        {
        }
    }


    public class ProjectsOverviewModel
    {
        public ProjectsOverviewModel(
        )
        {
        }
    }


    public class ContactRoleModel : ModelWithRequiredNameAndManageInfo
    {
        public ContactRoleModel(
        )
        {
        }
    }


    public class EmailModel
    {
        public EmailModel(
        )
        {
        }
    }


    public class TermsOfServiceApprovalEmailModel
    {
        public TermsOfServiceApprovalEmailModel(
        )
        {
        }
    }


    public class BillingInformationUpdateModel
        : IEntity
    {
        public BillingInformationUpdateModel(
        )
        {
        }
    }


    public class ProjectWorkHourModel : ModelWithName
    {
        public ProjectWorkHourModel(
        )
        {
        }
    }


    public class ProjectMonthlyPlanModel : MonthlyPlanModel
    {
        public ProjectMonthlyPlanModel(
        )
        {
        }
    }


    public class BusinessOverviewModel
    {
        public BusinessOverviewModel(
        )
        {
        }
    }


    public class MassDeleteModel
    {
        public MassDeleteModel(
        )
        {
        }
    }


    public class MassUpdateModel
    {
        public MassUpdateModel(
        )
        {
        }
    }


    public class ProjectTotalFeeModel
    {
        public ProjectTotalFeeModel(
        )
        {
        }
    }


    public class KeywordModel : ModelBaseWithManageInfo
    {
        public KeywordModel(
        )
        {
        }
    }


    public class CustomerMarketSegmentModel
    {
        public CustomerMarketSegmentModel(
        )
        {
        }
    }


    public class InvoiceTemplateWithSettingsModel : InvoiceTemplateModel
    {
        public InvoiceTemplateWithSettingsModel(
        )
        {
        }
    }


    public class DashboardModelBase : ModelWithName
    {
        public DashboardModelBase(
        )
        {
        }
    }


    public class FlatRateModel : ModelBase
    {
        public FlatRateModel(
        )
        {
        }
    }


    public class ProjectTotalForSalesCases
    {
        public ProjectTotalForSalesCases(
        )
        {
        }
    }


    public class ReportWithStatisticsModel : ReportModel
    {
        public ReportWithStatisticsModel(
        )
        {
        }
    }


    public class SharedReportModel : ReportWithDefinitionModel
    {
        public SharedReportModel(
        )
        {
        }
    }


    public class UninvoicedProjectModel : ProjectModelBase
    {
        public UninvoicedProjectModel(
        )
        {
        }
    }


    public class TeamProductivityModel : ModelBase
    {
        public TeamProductivityModel(
        )
        {
        }
    }


    public class ProjectBillingCustomerModel : ModelBase
    {
        public ProjectBillingCustomerModel(
        )
        {
        }
    }


    public class ProjectProductModel : ModelBase
    {
        public ProjectProductModel(
        )
        {
        }
    }


    public class ScheduleOverviewModel
    {
        public ScheduleOverviewModel(
        )
        {
        }
    }


    public class SharedDashboardAccessRightProfileModel : ModelBase
    {
        public SharedDashboardAccessRightProfileModel(
        )
        {
        }
    }


    public class SharedDashboardModel : DashboardModelBase
    {
        public SharedDashboardModel(
        )
        {
        }
    }


    public class InvoicesOverviewModel
    {
        public InvoicesOverviewModel(
        )
        {
        }
    }


    public class ProposalBillingPlanModel : ModelWithName
    {
        public ProposalBillingPlanModel(
        )
        {
        }
    }


    public class SalesOverviewModel
    {
        public SalesOverviewModel(
        )
        {
        }
    }


    public class CustomersOverviewModel
    {
        public CustomersOverviewModel(
        )
        {
        }
    }


    public class ProposalProjectPlanModelBase : ModelWithName
    {
        public ProposalProjectPlanModelBase(
        )
        {
        }
    }


    public class ReportWithDefinitionModel : ReportModel
    {
        public ReportWithDefinitionModel(
        )
        {
        }
    }


    public class TimeEntryTypeModel : ModelWithRequiredName
    {
        public TimeEntryTypeModel(
        )
        {
        }
    }


    public class ContactCommunicationModel : ModelBase
    {
        public ContactCommunicationModel(
        )
        {
        }
    }


    public class FileModelBase : ModelBase
    {
        public FileModelBase(
        )
        {
        }
    }


    public class ProjectFileModelWithData : ProjectFileModel
    {
        public ProjectFileModelWithData(
            ProjectFileModel arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly ProjectFileModel field0;
    }


    public class FlextimeModel
    {
        public FlextimeModel(
        )
        {
        }
    }


    public class HourModelBase : ModelBaseWithManageInfo
    {
        public HourModelBase(
        )
        {
        }
    }


    public class InvoiceSettingsModelBase : ModelBase
    {
        public InvoiceSettingsModelBase(
        )
        {
        }
    }


    public class InvoiceTemplateSettingsModel : InvoiceSettingsModelBase
    {
        public InvoiceTemplateSettingsModel(
        )
        {
        }
    }


    public class InvoiceTemplateModel : ModelBase
    {
        public InvoiceTemplateModel(
        )
        {
        }
    }


    public class InvoiceSettingsModel : InvoiceSettingsModelBase
    {
        public InvoiceSettingsModel(
        )
        {
        }
    }


    public class ProjectForecastModel : ModelBase
    {
        public ProjectForecastModel(
        )
        {
        }
    }


    public class ErrorModel
    {
        public ErrorModel(
        )
        {
        }
    }


    public class ModelWithErrors
    {
        public ModelWithErrors(
        )
        {
        }
    }


    public class SpecialUserOptionModel : ModelWithName
    {
        public SpecialUserOptionModel(
        )
        {
        }
    }


    public class ScheduledWorkErrorListReportController : PsaListReportController
    {
        public ScheduledWorkErrorListReportController(
        ): base ()
        {
        }
    }


    public class EmptyDataParameters
    {
        public EmptyDataParameters(
        )
        {
        }
    }


    public class ChartReportDefinitionModel : ReportWithFiltersDefinitionModel
    {
        public ChartReportDefinitionModel(
        )
        {
        }
    }


    public interface IReportFilterModel
    {
    }


    public class ReportFilterModel
        : IReportFilterModel
    {
        public ReportFilterModel(
        )
        {
        }
    }


    public class CrossTabReportDefinitionModelBase : ReportDefinitionModelBase
    {
    }


    public class ChartReportDataModel
    {
        public ChartReportDataModel(
            ChartReportResponse arg0
        )
        {
            field0 = arg0;
        }

        public readonly ChartReportResponse field0;
    }


    public class ChartReportMetadataModel : ReportWithFiltersMetadataModel
    {
        public ChartReportMetadataModel(
        )
        {
        }
    }


    public class ReportDefinitionAndDataParameterModel
    {
        public ReportDefinitionAndDataParameterModel(
        )
        {
        }
    }


    public class MatrixXAxisModel
    {
        public MatrixXAxisModel(
        )
        {
        }
    }


    public class MatrixXAxisMetadataModel
    {
        public MatrixXAxisMetadataModel(
        )
        {
        }
    }


    public class ChartDataFieldMetadataModel
    {
        public ChartDataFieldMetadataModel(
        )
        {
        }
    }


    public class ReportTypeModel
    {
        public ReportTypeModel(
        )
        {
        }
    }


    public class ReportWithFiltersDefinitionModel : ReportDefinitionModelBase
    {
    }


    public class ReportWithFiltersMetadataModel : ReportMetadataModelBase
    {
    }


    public class ResourceAllocationReportMetadataModel : ReportWithFiltersMetadataModel
    {
        public ResourceAllocationReportMetadataModel(
        )
        {
        }
    }


    public class ResourceAllocationReportDefinitionModel : ReportWithFiltersDefinitionModel
    {
        public ResourceAllocationReportDefinitionModel(
        )
        {
        }
    }


    public class TimeFrameModel
    {
        public TimeFrameModel(
        )
        {
        }
    }


    public class TimelineGraphReportDefinitionModel : ReportDefinitionModelBase
    {
        public TimelineGraphReportDefinitionModel(
        )
        {
        }
    }


    public class MatrixReportDefinitionModel : CrossTabReportDefinitionModelBase
    {
        public MatrixReportDefinitionModel(
        )
        {
        }
    }


    public class TimelineListReportDefinitionModel : CrossTabReportDefinitionModelBase
    {
        public TimelineListReportDefinitionModel(
        )
        {
        }
    }


    public class ReportSortFieldModel
    {
        public ReportSortFieldModel(
        )
        {
        }
    }


    public class ReportFieldModel
    {
        public ReportFieldModel(
        )
        {
        }
    }


    public class ReportParametersModel
    {
        public ReportParametersModel(
        )
        {
        }
    }


    public class ProjectFileModel : FileModelBase
    {
        public ProjectFileModel(
        )
        {
        }
    }


    public class ResourceAllocationModel : ModelBaseWithManageInfo
    {
        public ResourceAllocationModel(
        )
        {
        }
    }


    public class ResourceAllocationTotalModel
    {
        public ResourceAllocationTotalModel(
        )
        {
        }
    }


    public class InvoiceFileModel : FileModelBase
    {
        public InvoiceFileModel(
        )
        {
        }
    }


    public class ReportFieldMetadataModel
    {
        public ReportFieldMetadataModel(
        )
        {
        }
    }


    public class OrganizationModel : ModelWithName
    {
        public OrganizationModel(
        )
        {
        }
    }


    public class ProjectTaskStatusModel : ModelWithRequiredName
    {
        public ProjectTaskStatusModel(
        )
        {
        }
    }


    public class QuickSearchResultModel
    {
        public QuickSearchResultModel(
        )
        {
        }
    }


    public class ActivityModel : ModelWithRequiredNameAndManageInfo
    {
        public ActivityModel(
        )
        {
        }
    }


    public class ActivityTypeModel : ModelWithRequiredName
    {
        public ActivityTypeModel(
        )
        {
        }
    }


    public class AuthenticationModel
    {
        public AuthenticationModel(
        )
        {
        }
    }


    public class CreateInvoiceModel : ModelBase
    {
        public CreateInvoiceModel(
        )
        {
        }
    }


    public class InvoiceTaxBreakdownModel : TaxBreakdownModelBase
    {
        public InvoiceTaxBreakdownModel(
        )
        {
        }
    }


    public class TaxModel
    {
        public TaxModel(
        )
        {
        }
    }


    public class TaxBreakdownModelBase
    {
        public TaxBreakdownModelBase(
        )
        {
        }
    }


    public class ReportModel : ModelWithRequiredName
    {
        public ReportModel(
        )
        {
        }
    }

    public class ReportModelConverter<TReportModel> : IModelConverter<Report> where TReportModel : ReportModel, new()
    {
        protected readonly IGuidService GuidService;
        protected readonly ReportControllerFactory<IPsaContext> ReportControllerFactory;

        public ReportModelConverter(IGuidService guidService, ReportControllerFactory<IPsaContext> reportControllerFactory)
        {
            GuidService = guidService;
            ReportControllerFactory = reportControllerFactory;
        }
    }

    public class ReportModelConverter : ReportModelConverter<ReportModel>
    {
        public ReportModelConverter(IGuidService guidService, ReportControllerFactory<IPsaContext> reportControllerFactory) : base(guidService, reportControllerFactory)
        {
        }
    }

    public class ProposalFileModel : FileModelBase
    {
        public ProposalFileModel(
        )
        {
        }
    }


    public class DashboardWithPartsModel : DashboardModel
    {
        public DashboardWithPartsModel(
        )
        {
        }
    }


    public class DashboardPartModel : ModelWithName
    {
        public DashboardPartModel(
        )
        {
        }
    }


    public class ReportStateModel
    {
        public ReportStateModel(
        )
        {
        }
    }


    public class ListReportDataParameterModelWithReportState : ListReportDataParameterModel
    {
        public ListReportDataParameterModelWithReportState(
        )
        {
        }
    }


    public class ListReportDataParameterModelWithReportStateAndModifiers : ListReportDataParameterModelWithReportState
    {
        public ListReportDataParameterModelWithReportStateAndModifiers(
        )
        {
        }
    }


    public class ReportParametersWithDataModel : ReportParametersModel
    {
        public ReportParametersWithDataModel(
        )
        {
        }
    }


    public class DashboardModel : DashboardModelBase
    {
        public DashboardModel(
        )
        {
        }
    }


    public class ProposalTemplateModel : ProposalModelBase
    {
        public ProposalTemplateModel(
        )
        {
        }
    }


    public class ProposalSubtotalModel : ModelWithName
    {
        public ProposalSubtotalModel(
        )
        {
        }
    }


    public class ProposalTaxBreakdownModel : TaxBreakdownModelBase
    {
        public ProposalTaxBreakdownModel(
        )
        {
        }
    }


    public class TemporaryProjectFeeModel : TemporaryProjectFeeModelBase
    {
        public TemporaryProjectFeeModel(
        )
        {
        }
    }


    public class TemporaryProjectTravelExpenseModel : TemporaryProjectFeeModelBase
    {
        public TemporaryProjectTravelExpenseModel(
        )
        {
        }
    }


    public class TemporaryProjectFeeModelBase : ModelBase
    {
        public TemporaryProjectFeeModelBase(
        )
        {
        }
    }


    public class TemporaryWorkHourModel : HourModelBase
    {
        public TemporaryWorkHourModel(
        )
        {
        }
    }


    public class ProposalProjectPlanModel : ProposalProjectPlanModelBase
    {
        public ProposalProjectPlanModel(
        )
        {
        }
    }


    public class UserWithPhotoFileModelAndRequiredGuid : ModelBaseWithRequiredGuid
    {
        public UserWithPhotoFileModelAndRequiredGuid(
        )
        {
        }
    }


    public class UserWithNameAndPhotoFileModel : ModelWithName
    {
        public UserWithNameAndPhotoFileModel(
        )
        {
        }
    }


    public class UserWithFirstNameLastNameAndPhotoFileModel : UserWithNameAndPhotoFileModel
    {
        public UserWithFirstNameLastNameAndPhotoFileModel(
        )
        {
        }
    }


    public class UserWithFirstNameLastNamePhotoFileModelAndRequiredGuid : UserWithPhotoFileModelAndRequiredGuid
    {
        public UserWithFirstNameLastNamePhotoFileModelAndRequiredGuid(
        )
        {
        }
    }


    public class UserWithFirstNameLastNameModel : ModelBase
    {
        public UserWithFirstNameLastNameModel(
        )
        {
        }
    }


    public class UserWithFirstNameLastNameEmailModel : UserWithFirstNameLastNameModel
    {
        public UserWithFirstNameLastNameEmailModel(
        )
        {
        }
    }


    public class ResourcingAvailabilityUserBaseModel : UserBaseModel
    {
        public ResourcingAvailabilityUserBaseModel(
        )
        {
        }
    }


    public class UserBaseModel : PersonModelBase
    {
        public UserBaseModel(
        )
        {
        }
    }


    public class UserRightsModel
    {
        public UserRightsModel(
        )
        {
        }
    }


    public class InvoiceRowType
    {
    }


    public class InvoiceRowAccountingModel
        : IEntity
    {
        public InvoiceRowAccountingModel(
        )
        {
        }
    }


    public class InvoiceRowModel : ModelBaseWithManageInfo
    {
        public InvoiceRowModel(
        )
        {
        }
    }


    public class ListReportDataModel
    {
        public ListReportDataModel(
        )
        {
        }
    }


    public class WorkdayModel
    {
        public WorkdayModel(
        )
        {
        }
    }


    public class TimeEntryModel : ModelBase
    {
        public TimeEntryModel(
        )
        {
        }
    }


    public class WorktypeForProjectModel
    {
        public WorktypeForProjectModel(
        )
        {
        }
    }


    public class ReportActionParameterModel
    {
        public ReportActionParameterModel(
        )
        {
        }
    }


    public class ListReportDataParameterModel
    {
        public ListReportDataParameterModel(
        )
        {
        }
    }


    public class InvoiceModel : ModelBaseWithManageInfo
    {
        public InvoiceModel(
        )
        {
        }
    }


    public class InvoicesTotalModel
    {
        public InvoicesTotalModel(
        )
        {
        }
    }


    public class HolidayModel : ModelWithRequiredName
    {
        public HolidayModel(
        )
        {
        }
    }


    public class ProposalWorkhourModel : ModelWithName
    {
        public ProposalWorkhourModel(
        )
        {
        }
    }


    public class ProposalFeeModel : ModelWithName
    {
        public ProposalFeeModel(
        )
        {
        }
    }


    public class ProposalStatusModel : ModelWithRequiredName
    {
        public ProposalStatusModel(
        )
        {
        }
    }


    public class ProposalModelBase : ModelBaseWithManageInfo
    {
        public ProposalModelBase(
        )
        {
        }
    }


    public class ProposalModel : ProposalModelBase
    {
        public ProposalModel(
        )
        {
        }
    }


    public class InvoiceStatusModel : ModelWithRequiredName
    {
        public InvoiceStatusModel(
        )
        {
        }
    }


    public class PriceIncreaseModel
        : IEntity
    {
        public PriceIncreaseModel(
        )
        {
        }
    }


    public class StatusHistoryModel
    {
        public StatusHistoryModel(
        )
        {
        }
    }


    public class ProjectWorktypeModel : ModelBase
    {
        public ProjectWorktypeModel(
        )
        {
        }
    }


    public class PhaseStatusTypeModel : ModelBaseWithManageInfo
    {
        public PhaseStatusTypeModel(
        )
        {
        }
    }


    public class PhaseTreeFavoritePhaseModel : PhaseTreePhaseModelBase
    {
        public PhaseTreeFavoritePhaseModel(
        )
        {
        }
    }


    public class PhaseTreePhaseModelBase
    {
        public PhaseTreePhaseModelBase(
        )
        {
        }
    }


    public class PhaseTreePhaseModel : PhaseTreePhaseModelBase
    {
        public PhaseTreePhaseModel(
        )
        {
        }
    }


    public class PhaseStatusModel
        : IEntity
    {
        public PhaseStatusModel(
        )
        {
        }
    }


    public class WorkTypeModel : ModelWithRequiredName
    {
        public WorkTypeModel(
        )
        {
        }
    }


    public class WorkHourModel : HourModelBase
    {
        public WorkHourModel(
        )
        {
        }
    }


    public class CostCenterModel : ModelWithRequiredName
    {
        public CostCenterModel(
        )
        {
        }
    }


    public class PricelistVersionModel : ModelBase
    {
        public PricelistVersionModel(
        )
        {
        }
    }


    public class OvertimePriceModel : ModelBase
    {
        public OvertimePriceModel(
        )
        {
        }
    }


    public class ProjectWorkHourPriceModel : WorkHourPriceModel
    {
        public ProjectWorkHourPriceModel(
        )
        {
        }
    }


    public class WorkHourPriceModel : WorkHourPriceModelBase
    {
        public WorkHourPriceModel(
        )
        {
        }
    }


    public class WorkHourPriceModelBase : ModelBase
    {
        public WorkHourPriceModelBase(
        )
        {
        }
    }


    public class ProductPriceCollectionModel
    {
        public ProductPriceCollectionModel(
        )
        {
        }
    }


    public class ProductPriceModel : ProductPriceModelBase
    {
        public ProductPriceModel(
        )
        {
        }
    }


    public class TravelPriceModelCollectionModel
    {
        public TravelPriceModelCollectionModel(
        )
        {
        }
    }


    public class TravelPriceModel : ProductPriceModelBase
    {
        public TravelPriceModel(
        )
        {
        }
    }


    public class ProductPriceModelBase : ModelBase
    {
        public ProductPriceModelBase(
        )
        {
        }
    }


    public class ProductModelBase : ModelWithRequiredName
    {
        public ProductModelBase(
        )
        {
        }
    }


    public class ProjectFeeModelBase : ModelBaseWithManageInfo
    {
        public ProjectFeeModelBase(
        )
        {
        }
    }


    public class ProjectRecurringFeeRuleModel : ProjectFeeModelBase
    {
        public ProjectRecurringFeeRuleModel(
        )
        {
        }
    }


    public class ProjectTravelExpenseModel : ModelBaseWithManageInfo
    {
        public ProjectTravelExpenseModel(
        )
        {
        }
    }


    public class TravelExpenseModel : ProductModelBase
    {
        public TravelExpenseModel(
        )
        {
        }
    }


    public class FileModelWithData : FileModel
    {
        public FileModelWithData(
            FileModel arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly FileModel field0;
    }


    public class ProjectFeeModel : ProjectFeeModelBase
    {
        public ProjectFeeModel(
        )
        {
        }
    }


    public class ProjectFeeTotalModel
    {
        public ProjectFeeTotalModel(
        )
        {
        }
    }


    public class ProductForProjectModel
    {
        public ProductForProjectModel(
        )
        {
        }
    }


    public class SalesReceivableAccountModel
    {
        public SalesReceivableAccountModel(
        )
        {
        }
    }


    public class ValueAddedTaxModel : ModelBase
    {
        public ValueAddedTaxModel(
        )
        {
        }
    }


    public class OvertimeModel : ModelWithRequiredName
    {
        public OvertimeModel(
        )
        {
        }
    }


    public class ProductCategoryModel : ModelWithRequiredName
    {
        public ProductCategoryModel(
        )
        {
        }
    }


    public class BusinessUnitModel : BusinessUnitModelBase
    {
        public BusinessUnitModel(
        )
        {
        }
    }


    public class CollaborationNoteModel : NoteModelBase
    {
        public CollaborationNoteModel(
        )
        {
        }
    }


    public class ContactWithFirstNameLastName : ModelWithName
    {
        public ContactWithFirstNameLastName(
        )
        {
        }
    }


    public class ContactModel : PersonModelBase
    {
        public ContactModel(
        )
        {
        }
    }


    public class CurrencyBaseModel : ModelBaseWithRequiredGuid
    {
        public CurrencyBaseModel(
        )
        {
        }
    }


    public class CurrencyModel : ModelBase
    {
        public CurrencyModel(
        )
        {
        }
    }


    public class Credentials
    {
        public Credentials(
        )
        {
        }
    }


    public class AddressModel : ModelBase
    {
        public AddressModel(
        )
        {
        }
    }


    public class CustomerModel : ModelWithRequiredNameAndManageInfo
    {
        public CustomerModel(
        )
        {
        }
    }


    public class FileModel : FileModelBase
    {
        public FileModel(
        )
        {
        }
    }


    public class ProductModel : ProductModelBase
    {
        public ProductModel(
        )
        {
        }
    }


    public class ProjectChartCriteriaModel
    {
        public ProjectChartCriteriaModel(
        )
        {
        }
    }


    public class SalesAccountModel : ModelWithRequiredName
    {
        public SalesAccountModel(
        )
        {
        }
    }


    public class TimePeriodModel
    {
        public TimePeriodModel(
        )
        {
        }
    }


    public class ProjectChartReportParametersModel
    {
        public ProjectChartReportParametersModel(
        )
        {
        }
    }


    public class FormattingCultureModel : ModelWithName
    {
        public FormattingCultureModel(
        )
        {
        }
    }


    public class IndustryModel : ModelWithRequiredNameAndManageInfo
    {
        public IndustryModel(
        )
        {
        }
    }


    public class LanguageModel : ModelWithName
    {
        public LanguageModel(
        )
        {
        }
    }


    public class LeadSourceModel : ModelWithRequiredNameAndManageInfo
    {
        public LeadSourceModel(
        )
        {
        }
    }


    public class MenuModel
    {
        public MenuModel(
        )
        {
        }
    }


    public class PersonalSettingsModel
    {
        public PersonalSettingsModel(
        )
        {
        }
    }


    public class PhaseMemberModelWithResourcingAvailability : PhaseMemberModel
    {
        public PhaseMemberModelWithResourcingAvailability(
        )
        {
        }
    }


    public class PhaseMemberModel : ModelBase
    {
        public PhaseMemberModel(
        )
        {
        }
    }


    public class PhaseModel : ModelWithRequiredNameAndManageInfo
    {
        public PhaseModel(
        )
        {
        }
    }


    public class PhaseModelWithHierarchyInfo : PhaseModel
    {
        public PhaseModelWithHierarchyInfo(
        )
        {
        }
    }


    public class PricelistModel : ModelWithRequiredName
    {
        public PricelistModel(
        )
        {
        }
    }


    public class ProjectCriteriaModel
    {
        public ProjectCriteriaModel(
        )
        {
        }
    }


    public class ProjectModel : ProjectModelBase
    {
        public ProjectModel(
        )
        {
        }
    }


    public class ProjectStatusModel : ModelBaseReadOnly
    {
        public ProjectStatusModel(
        )
        {
        }
    }


    public class ProjectStatusTypeModel : ModelBaseWithManageInfo
    {
        public ProjectStatusTypeModel(
        )
        {
        }
    }


    public class SalesNoteModel : NoteModelBase
    {
        public SalesNoteModel(
        )
        {
        }
    }


    public class SalesStatusHistoryModel : ModelBase
    {
        public SalesStatusHistoryModel(
        )
        {
        }
    }


    public class SalesStatusModel : ModelBaseReadOnly
    {
        public SalesStatusModel(
        )
        {
        }
    }


    public class SalesStatusTypeModel : ModelWithRequiredName
    {
        public SalesStatusTypeModel(
        )
        {
        }
    }


    public class WorkContractModel : WorkContractBaseModel
    {
        public WorkContractModel(
        )
        {
        }
    }


    public class TimeZoneModel : ModelWithName
    {
        public TimeZoneModel(
        )
        {
        }
    }


    public class UserModel : PersonModelBase
    {
        public UserModel(
        )
        {
        }
    }


    public class GoogleOAuth2AuthenticationProvider
    {
        public GoogleOAuth2AuthenticationProvider(
        )
        {
        }
    }


    public class AccountListReportController : PsaListReportController
    {
        public AccountListReportController(
            IAccountService arg0,
            ICompanyService arg1,
            IIndustryService arg2,
            IAccountGroupMemberService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IAccountService field0;
        public readonly ICompanyService field1;
        public readonly IIndustryService field2;
        public readonly IAccountGroupMemberService field3;
    }

    public class ActivityStatusAction
    {
    }


    public class ActivityListReportController : PsaListReportController
    {
        public ActivityListReportController(
            IActivityService arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IActivityService field0;
    }


    public class CaseFileListReportController : PsaListReportController
    {
        public CaseFileListReportController(
        )
        {
        }
    }


    public class AddKeywordParameters
    {
        public AddKeywordParameters(
        )
        {
        }
    }


    public class BusinessUnitListReportController : PsaListReportController
    {
        public BusinessUnitListReportController(
        )
        {
        }
    }


    public class GenericStringListFilterType : SimpleListReportFilterHandler
    {
        public GenericStringListFilterType(
            string arg0, Data.ISearchDefinition<TContext> arg1,
            IGuidService arg2,
            string arg3,
            string arg4,
            string arg5,
            string arg6,
            bool arg9
        ) : base()
        {
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
            field9 = arg9;
        }

        public readonly IGuidService field2;
        public readonly string field3;
        public readonly string field4;
        public readonly string field5;
        public readonly string field6;
        public readonly bool field9;
    }


    public class GetAdditionalCriteriaDelegate
    {
        public GetAdditionalCriteriaDelegate(
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


    public class GetFixedCriteriaDelegate
    {
        public GetFixedCriteriaDelegate(
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


    public class GetAdditionalListFilterValuesDelegate
    {
        public GetAdditionalListFilterValuesDelegate(
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


    public class GenericIDListFilterType
    {
        public GenericIDListFilterType(
            string arg0, Data.ISearchDefinition<TContext> arg1,
            IGuidService arg2,
            string arg3,
            string arg4,
            string arg5,
            string arg6
        )
        {
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly Data.ISearchDefinition<TContext> field1;
        public readonly IGuidService field2;
        public readonly string field3;
        public readonly string field4;
        public readonly string field5;
        public readonly string field6;
    }


    public class ListReportDefinitionModel : ReportDefinitionModelBase
    {
        public ListReportDefinitionModel(
        )
        {
        }
    }


    public interface IReportDefinitionModel
    {
    }


    public class ReportDefinitionModelBase
        : IReportDefinitionModel
    {
    }


    public class TimeFrameMetadataModel
    {
        public TimeFrameMetadataModel(
        )
        {
        }
    }


    public class TimelineGraphReportMetadataModel : ReportMetadataModelBase
    {
        public TimelineGraphReportMetadataModel(
        )
        {
        }
    }


    public class MatrixReportMetadataModel : ReportMetadataModelBase
    {
        public MatrixReportMetadataModel(
        )
        {
        }
    }


    public class TimelineListReportMetadataModel : ReportMetadataModelBase
    {
        public TimelineListReportMetadataModel(
        )
        {
        }
    }


    public interface IReportMetadataModel
    {
    }


    public class ReportMetadataModelBase
        : IReportMetadataModel
    {
    }


    public class ListReportMetadataModel : ReportMetadataModelBase
    {
        public ListReportMetadataModel(
        )
        {
        }
    }


    public class OfferRowListReportController : PsaListReportController
    {
        public OfferRowListReportController(
        )
        {
        }
    }


    public class AccountAndBillingAccountReportColumn
    {
        public AccountAndBillingAccountReportColumn(
        )
        {
        }
    }


    public class AccountGroupsReportColumn
    {
        public AccountGroupsReportColumn(
        )
        {
        }
    }


    public class EmailAllowedListReportColumn
    {
        public EmailAllowedListReportColumn(
            string arg0
        )
        {
        }
    }

    public class ListFilterValue
    {
        public ListFilterValue(
        )
        {
        }

        public ListFilterValue(
            string arg1,
            string arg2
        )
        {
            field1 = arg1;
            field2 = arg2;
        }

        public readonly string field1;
        public readonly string field2;
    }


    public class ListFilterValueCollection
    {
        public ListFilterValueCollection(
        )
        {
        }
    }


    public class ListFilterMetadataModel : ReportFilterMetadataModel
    {
        public ListFilterMetadataModel(
        )
        {
        }
    }


    public class ListFilterModel : ReportFilterModel
    {
        public ListFilterModel(
        )
        {
        }
    }

    public class ReportFilterFactory
    {
    }


    public interface IReportFilterMetadataModel
    {
    }


    public class ReportFilterMetadataModel
        : IReportFilterMetadataModel
    {
        public ReportFilterMetadataModel(
        )
        {
        }
    }


    public class FilterTypeBase
        : IFilterType
    {
    }


    public interface IFilterType
    {
    }


    public class
        TimePeriodFilterType
    {
        public TimePeriodFilterType(
        )
        {
        }
    }


    public class ReportModifiers
    {
        public ReportModifiers(
        )
        {
        }
    }


    public class ReportParametersModelBase
    {
        public ReportParametersModelBase(
        )
        {
        }
    }


    public class ResourceAllocationTimelineReportController : PsaTimelineListReportController
    {
        public ResourceAllocationTimelineReportController(
        )
        {
        }
    }


    public class HourApprovalTimelineReportController : PsaTimelineListReportController
    {
        public HourApprovalTimelineReportController(
            IHourRepository arg0,
            IMailClient arg1,
            IAppSettings arg2,
            IHourEmailService arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IHourRepository field0;
        public readonly IMailClient field1;
        public readonly IAppSettings field2;
        public readonly IHourEmailService field3;
    }


    public class AccountOverviewReportController : AccountListReportController
    {
        public AccountOverviewReportController(
            IAccountService arg0,
            ICompanyService arg1,
            IIndustryService arg2,
            IAccountGroupMemberService arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
        }
    }


    public class HourOverviewReportController : UserListReportController
    {
        public HourOverviewReportController(
            IUserService arg0
        ) : base(arg0)
        {
        }
    }


    public class PurchaseOrderItemListReportController : PsaListReportController
    {
        public PurchaseOrderItemListReportController(
        )
        {
        }
    }


    public class UserManagementListReportController : UserListReportController
    {
        public UserManagementListReportController(
            IUserService arg0
        ) : base(arg0)
        {
        }
    }


    public class ItemTimelineReportController : PsaTimelineListReportController
    {
        public ItemTimelineReportController(
        )
        {
        }
    }


    public class HourTimelineReportController : PsaTimelineListReportController
    {
        public HourTimelineReportController(
        )
        {
        }
    }


    public class UserTimelineReportController : PsaTimelineListReportController
    {
        public UserTimelineReportController(
        )
        {
        }
    }


    public class CaseTimelineReportController : PsaTimelineListReportController
    {
        public CaseTimelineReportController(
        )
        {
        }
    }


    public class ContactListReportController : PsaListReportController
    {
        public ContactListReportController(
        )
        {
        }
    }


    public class ActionFactoryContainer<TReport> where TReport : IReport
    {
    }


    public class DrillDownClientAction : ClientAction
    {
        public DrillDownClientAction(
        )
        {
        }
    }


    public class DrillDownActionResponse : ActionResponse
    {
        public DrillDownActionResponse(
            ReportParametersModel arg0,
            string arg1
        ) : base()
        {
            field1 = arg1;
        }

        public readonly string field1;
    }


    public class ActionResponse
    {
        public ActionResponse(
            ClientAction arg0
        )
        {
        }

        public ActionResponse(
            ICollection<ClientError> arg0
        )
        {
            field0 = arg0;
        }

        public readonly ICollection<ClientError> field0;

        protected ActionResponse()
        {
        }
    }


    public class ClientAction
    {
        public ClientAction(
        )
        {
        }
    }


    public class ClientError
    {
        public ClientError(
        )
        {
        }
    }

    public class GridDataConverter
    {
        public GridDataConverter(
            LabelPurpose arg0
        )
        {
            field0 = arg0;
        }

        public readonly LabelPurpose field0;
    }


    public interface IReportColumnType
    {
    }


    public class KpiReportColumn
    {
        public KpiReportColumn(
        )
        {
        }
    }


    public class LinkListReportColumn
    {
        public LinkListReportColumn(
            string arg0
        ) : base()
        {
        }
    }


    public class NumericReportColumn
    {
        public NumericReportColumn(
        )
        {
        }
    }


    public class PictureFileReportColumn
    {
        public PictureFileReportColumn(
        )
        {
        }
    }


    public class ReportColumnFactory
    {
    }


    public class DataFieldGroupOptions
    {
        public DataFieldGroupOptions(
        )
        {
        }
    }


    public class ReportColumnType
        : IReportColumnType
    {
    }


    public class StringReportColumn
    {
        public StringReportColumn(
        )
        {
        }
    }


    public class WorkHoursReportColumn
    {
        public WorkHoursReportColumn(
        )
        {
        }
    }


    public class GridParametersModel : ReportParametersModelBase
    {
        public GridParametersModel(
        )
        {
        }
    }


    public interface IGridReportController<TContext> : IReportController<TContext> where TContext : IContext
    {
    }


    public interface IReportController<TContext> : IReportInfoBase<TContext> where TContext : IContext
    {
    }


    public class MassUpdateControlOptions
    {
        public MassUpdateControlOptions(
            string arg0,
            string arg1,
            string arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly string field0;
        public readonly string field1;
        public readonly string field2;
    }


    public class MassUpdateParameters
    {
        public MassUpdateParameters(
        )
        {
        }
    }


    public class GenericButtonControlOptions : MassUpdateControlOptions
    {
        public GenericButtonControlOptions(
            string arg0,
            string arg1,
            string arg2,
            string arg3,
            string arg4
        ) : base(arg0, arg1, arg2)
        {
            field3 = arg3;
            field4 = arg4;
        }

        public readonly string field3;
        public readonly string field4;
    }


    public class GenericDateControlOptions : MassUpdateControlOptions
    {
        public GenericDateControlOptions(
            string arg0,
            string arg1,
            string arg2,
            string arg3,
            string arg4,
            string arg5
        ) : base(arg0, arg1, arg2)
        {
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
        }

        public readonly string field3;
        public readonly string field4;
        public readonly string field5;
    }


    public class PopulateListItemsDelegate
    {
        public PopulateListItemsDelegate(
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


    public class GenericDropDownControlOptions : MassUpdateControlOptions
    {
        public GenericDropDownControlOptions(
            string arg0,
            string arg1,
            string arg2,
            string arg3,
            string arg4,
            string arg5,
            string arg6
        ) : base(arg0, arg1, arg2)
        {
            field3 = arg3;
            field4 = arg4;
            field5 = arg5;
            field6 = arg6;
        }

        public readonly string field3;
        public readonly string field4;
        public readonly string field5;
        public readonly string field6;
    }


    public class PsaTimelineListReportController
    {
        public PsaTimelineListReportController(
        )
        {
        }
    }

    public abstract class ReportController<TContext, TReportHandler, TReport, TDataParameters, TReportDefinitionModel> : ActionFactoryContainer<TReport>, IReportController<TContext>
        where TContext : IContext
        where TReportHandler : IReportHandler<TContext>
        where TReport : IReport
        where TReportDefinitionModel : IReportDefinitionModel, new()
    {
        private IContextService<TContext> _ContextService;
        private TReportHandler _ReportHandler;
        protected readonly ReportControllerFactory<TContext> ReportControllerFactory;

        protected ReportController(ReportControllerFactory<TContext> reportControllerFactory)
        {
            ReportControllerFactory = reportControllerFactory;
        }
    }

    public abstract class ListReportController<TContext> : ReportController<TContext, IListReportHandler<TContext>, ListReport, ListReportDataParameterModel, ListReportDefinitionModel>, IGridReportController<TContext> where TContext : IContext
    {
        public ListReportController(ReportControllerFactory<TContext> reportControllerFactory) : base(reportControllerFactory)
        {
        }
    }

    public class PsaListReportController : ListReportController<IPsaContext>
    {
        public PsaListReportController(
        ): base(null)
        {
        }
    }

    public class PsaReportFactory : ReportControllerFactory<IPsaContext>, IPsaReportFactoryService
    {
        public static readonly PsaReportFactory Factory = null;

        private readonly IGuidService _GuidService;
        private readonly IAccountService _AccountService;
        private readonly ICompanyService _CompanyService;
        private readonly IInvoiceService _InvoiceService;
        private readonly IUserService _UserService;
        private readonly IIndustryService _IndustryService;
        private readonly IInvoiceConfigService _InvoiceConfigService;
        private readonly IInvoiceStatusService _InvoiceStatusService;
        private readonly ICaseService _CaseService;
        private readonly IItemService _ItemService;
        private readonly IRecurringItemService _RecurringItemService;
        private readonly ITaskService _TaskService;
        private readonly ITimeEntryService _TimeEntryService;
        private readonly IActivityService _ActivityService;
        private readonly IHourRepository _HourRepository;
        private readonly IReportRepository _ReportRepository;
        private readonly IPsaReportingRepository _ReportingRepository;
        private readonly IHourEmailService _HourEmailService;
        private readonly ICaseTagService _CaseTagService;
        private readonly IAccountGroupMemberService _AccountGroupMemberService;
        public readonly ICurrencyService CurrencyService;
        public readonly ISalesProcessService SalesProcessService;
        public readonly IMailClient MailClient;
        public readonly IAppSettings AppSettings;

        private readonly PsaSearchFactory _SearchFactory;

        public PsaReportFactory(IPsaContextService contextService, IAccountService accountService,
            ICompanyService companyService, IInvoiceService invoiceService, IUserService userService,
            IIndustryService industryService, IInvoiceConfigService invoiceConfigService,
            IInvoiceStatusService invoiceStatusService, ICaseService caseService, IItemService itemService,
            IRecurringItemService recurringItemService, ITaskService taskService, ITimeEntryService timeEntryService,
            IActivityService activityService, IHourRepository hourRepository, IReportRepository reportRepository,
            IPsaReportingRepository reportingRepository, ISalesProcessService salesProcessService,
            IMailClient mailClient, IAppSettings AppSettings, IHourEmailService hourEmailService,
            ICaseTagService caseTagService, IAccountGroupMemberService accountGroupMemberService,
            ICurrencyService currencyService) : base(contextService)
        {
            _GuidService = new GuidService(contextService);
            _AccountService = accountService;
            _CompanyService = companyService;
            _InvoiceService = invoiceService;
            _UserService = userService;
            _IndustryService = industryService;
            _InvoiceConfigService = invoiceConfigService;
            _InvoiceStatusService = invoiceStatusService;
            _CaseService = caseService;
            _ItemService = itemService;
            _RecurringItemService = recurringItemService;
            _TaskService = taskService;
            _TimeEntryService = timeEntryService;
            _ActivityService = activityService;
            _HourRepository = hourRepository;
            _ReportRepository = reportRepository;
            _ReportingRepository = reportingRepository;
            SalesProcessService = salesProcessService;
            MailClient = mailClient;
            AppSettings = AppSettings;
            _HourEmailService = hourEmailService;
            _CaseTagService = caseTagService;
            _AccountGroupMemberService = accountGroupMemberService;
            CurrencyService = currencyService;
        }

        public IGuidService GuidService => _GuidService;
    }


    public abstract class ReportControllerFactory<TContext> : ReportFactory<TContext, IReportController<TContext>> where TContext : IContext
    {
        protected ReportControllerFactory(IPsaContextService contextService)
        {
        }
    }

    public class HourListReportController : PsaListReportController
    {
        public HourListReportController(
            IHourRepository arg0,
            IHourEmailService arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IHourRepository field0;
        public readonly IHourEmailService field1;
    }


    public class InvoiceListReportController : PsaListReportController
    {
        public InvoiceListReportController(
            IInvoiceService arg0,
            IInvoiceConfigService arg1,
            IInvoiceStatusService arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IInvoiceService field0;
        public readonly IInvoiceConfigService field1;
        public readonly IInvoiceStatusService field2;
    }


    public class InvoiceRowListReportController : PsaListReportController
    {
        public InvoiceRowListReportController(
            IInvoiceService arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IInvoiceService field0;
    }


    public class ItemListReportController : PsaListReportController
    {
        public ItemListReportController(
            IItemService arg0,
            IRecurringItemService arg1,
            ICaseService arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IItemService field0;
        public readonly IRecurringItemService field1;
        public readonly ICaseService field2;
    }


    public class OfferReportController : PsaListReportController
    {
        public OfferReportController(
        )
        {
        }
    }


    public class TaskListReportController : PsaListReportController
    {
        public TaskListReportController(
        )
        {
        }
    }


    public class TimeEntryReportController : PsaListReportController
    {
        public TimeEntryReportController(
            ICaseService arg0,
            ITaskService arg1,
            ITimeEntryService arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly ICaseService field0;
        public readonly ITaskService field1;
        public readonly ITimeEntryService field2;
    }


    public class TravelReimbursementListReportController : PsaListReportController
    {
        public TravelReimbursementListReportController(
        )
        {
        }
    }


    public class UserAccessGroup
    {
    }


    public class UserListReportController : PsaListReportController
    {
        public UserListReportController(
            IUserService arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly IUserService field0;
    }


    public class CalendarSyncDispatcherService
        : ICalendarSyncDispatcherService
    {
        public CalendarSyncDispatcherService(
            IContextService<IPsaContext> arg0,
            IGuidService arg1,
            IExchangeCalendarSyncProvider arg2,
            IGoogleCalendarSyncProvider arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly IContextService<IPsaContext> field0;
        public readonly IGuidService field1;
        public readonly IExchangeCalendarSyncProvider field2;
        public readonly IGoogleCalendarSyncProvider field3;
    }


    public interface IRequestDataReaderService
    {
    }


    public class RequestDataReaderService
        : IRequestDataReaderService
    {
        public RequestDataReaderService(
        )
        {
        }
    }


    public interface ICalendarSyncDispatcherService
    {
    }


    public interface IHtmlSanitizerService
    {
    }


    public class HtmlSanitizerService
        : IHtmlSanitizerService
    {
        public HtmlSanitizerService(
        )
        {
        }
    }


    public interface IWhiteListService
    {
    }


    public class WhiteListService
        : IWhiteListService
    {
        public WhiteListService(
        )
        {
        }
    }


    public class RestSettingsService
        : IRestSettingsService
    {
        public RestSettingsService(
        )
        {
        }
    }


    public class CurrentSessionService
        : ICurrentSessionService
    {
        public CurrentSessionService(
            IPsaContextStorage arg0
        )
        {
            field0 = arg0;
        }

        public readonly IPsaContextStorage field0;
    }


    public interface IRestSettingsService
    {
    }


    public interface ICurrentSessionService
    {
    }


    public interface IPersonalSettingsService
    {
    }


    public class PersonalSettingsService
        : IPersonalSettingsService
    {
        public PersonalSettingsService(
            IPersonalSettingsModelConverter arg0
        )
        {
            field0 = arg0;
        }

        public readonly IPersonalSettingsModelConverter field0;
    }


    public class ScheduledJobModel : ModelWithRequiredNameAndManageInfo
    {
        public ScheduledJobModel(
        )
        {
        }
    }


    public class ScheduledWorkJobModel : ScheduledJobModel
    {
        public ScheduledWorkJobModel(
        )
        {
        }
    }


    public class MyWebServices
    {
        public MyWebServices(
        )
        {
        }
    }


    public class ThreadSafeObjectProvider
    {
        public ThreadSafeObjectProvider(
        )
        {
        }
    }


    public class CustomSchemaFilterForDocumentationAttributes
    {
        public CustomSchemaFilterForDocumentationAttributes(
        )
        {
        }
    }


    public class CustomOperationFilter
    {
        public CustomOperationFilter(
        )
        {
        }
    }


    public class CustomDocumentFilter
    {
        public CustomDocumentFilter(
        )
        {
        }
    }


    public class UpdateModelDelegate
    {
        public UpdateModelDelegate(
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


    public class PhotoFileSize
    {
    }


    public class TravelReimbursementAttachmentSource
    {
    }


    public class PropertySettersWithCache
    {
        public PropertySettersWithCache(
            IGuidService arg0
        )
        {
            field0 = arg0;
        }

        public readonly IGuidService field0;
    }


    public class ModelCreationOrigin
    {
    }


    public class OrganizationStatusType
    {
    }


    public class ConnAuthenticationResponse
    {
        public ConnAuthenticationResponse(
        )
        {
        }
    }


    public class ExternalTokenModel
    {
        public ExternalTokenModel(
        )
        {
        }
    }


    public class WebHookStatusModel
    {
        public WebHookStatusModel(
        )
        {
        }
    }


    public class OutputArrayOptions
    {
    }


    public class StringCases
    {
    }

    public class StatusCodeMessageResult
        : IHttpActionResult
    {
        public StatusCodeMessageResult(
            HttpStatusCode arg0,
            string arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly HttpStatusCode field0;
        public readonly string field1;

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }


    public class GetContentDelegate
    {
        public GetContentDelegate(
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


    public class ReportTotalKpiLookoutParameters
    {
        public ReportTotalKpiLookoutParameters(
            ReportTotalKpiLookoutParameters arg0
        )
        {
            field0 = arg0;
        }

        public readonly ReportTotalKpiLookoutParameters field0;
    }


    public class ReportPartResult
    {
        public ReportPartResult(
        )
        {
        }
    }


    public class Parameters
    {
        public Parameters(
        )
        {
        }
    }


    public class AddonOperatorType
    {
    }

    public class CaseListReportController<TReportHandler> : PsaListReportController
        where TReportHandler : IListReportHandler<IPsaContext>
    {
        public CaseListReportController()
        {
        }
    }


    public class Mapping
    {
        public Mapping(
        )
        {
        }
    }


    public class OrderConfirmationCurrency : ModelWithName
    {
        public OrderConfirmationCurrency(
            string arg0,
            string arg1,
            string arg2,
            string arg3
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly string field0;
        public readonly string field1;
        public readonly string field2;
        public readonly string field3;
    }


    public class OrderConfirmationRow
    {
        public OrderConfirmationRow(
            int arg0,
            decimal arg1,
            string arg2,
            string arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly int field0;
        public readonly decimal field1;
        public readonly string field2;
        public readonly string field3;
    }


    public class ModelWithNameAndNumber : ModelWithName
    {
        public ModelWithNameAndNumber(
        )
        {
        }
    }


    public class CustomerRightModel
    {
        public CustomerRightModel(
        )
        {
        }
    }


    public class ProjectRightModel
    {
        public ProjectRightModel(
        )
        {
        }
    }


    public class ProjectJoinRightModel
    {
        public ProjectJoinRightModel(
        )
        {
        }
    }


    public class UserRightModel
    {
        public UserRightModel(
        )
        {
        }
    }


    public class OrganizationRightModel
    {
        public OrganizationRightModel(
        )
        {
        }
    }


    public class TravelReimbursementRightModel
    {
        public TravelReimbursementRightModel(
        )
        {
        }
    }


    public class SharingRightModel
    {
        public SharingRightModel(
        )
        {
        }
    }


    public class WorkHourApprovalRightModel
    {
        public WorkHourApprovalRightModel(
        )
        {
        }
    }


    public class ScheduleJobsRightModel
    {
        public ScheduleJobsRightModel(
        )
        {
        }
    }


    public class WorkHourPhase : ModelWithName
    {
        public WorkHourPhase(
        )
        {
        }
    }


    public class DateAndHourModel
    {
        public DateAndHourModel(
        )
        {
        }
    }


    public class KpiFormulaCategory
    {
    }


    public class AdditionalContextType
    {
    }


    public class SettingsContext
    {
    }


    public class QuickSearchOptions
    {
    }


    public class ProjectNameDisplayOptions
    {
    }


    public class ProjectSelectorOptions
    {
    }


    public class ModelWithNameAndCode : ModelBase
    {
        public ModelWithNameAndCode(
        )
        {
        }
    }


    public class CountryModel : ModelWithNameAndCode
    {
        public CountryModel(
        )
        {
        }
    }


    public class CountryRegionTimeZoneModel : ModelWithName
    {
        public CountryRegionTimeZoneModel(
        )
        {
        }
    }


    public class ProjectMemberCostExceptionProject : ModelWithName
    {
        public ProjectMemberCostExceptionProject(
        )
        {
        }
    }


    public class CustomerCountry : ModelWithName
    {
        public CustomerCountry(
        )
        {
        }
    }


    public class ProductCountryModel : ModelBaseWithRequiredGuid
    {
        public ProductCountryModel(
        )
        {
        }
    }


    public class ReceiptSource
    {
    }


    public class TravelExpenseReceiptCustomerModel : ModelWithName
    {
        public TravelExpenseReceiptCustomerModel(
        )
        {
        }
    }


    public class TravelExpenseReceiptProjectModel : ModelWithName
    {
        public TravelExpenseReceiptProjectModel(
        )
        {
        }
    }


    public class TravelExpenseReceiptPhaseModel : ModelWithName
    {
        public TravelExpenseReceiptPhaseModel(
        )
        {
        }
    }


    public class TosApprovalLanguageModel : ModelWithName
    {
        public TosApprovalLanguageModel(
        )
        {
        }
    }


    public class SalutationType
    {
    }


    public class WorkHourProject : ModelWithName
    {
        public WorkHourProject(
        )
        {
        }
    }


    public class RecurrenceFrequency
    {
    }


    public class DayOrdinal
    {
    }


    public class DayOrdinalOption
    {
    }


    public class Months
    {
    }


    public class DailyModel
    {
        public DailyModel(
        )
        {
        }
    }


    public class WeeklyModel
    {
        public WeeklyModel(
        )
        {
        }
    }


    public class MonthlyModel
    {
        public MonthlyModel(
        )
        {
        }
    }


    public class YearlyModel
    {
        public YearlyModel(
        )
        {
        }
    }


    public class RecurrencePattern
    {
        public RecurrencePattern(
        )
        {
        }
    }


    public class RecurrenceRange
    {
        public RecurrenceRange(
        )
        {
        }
    }


    public class ToProjectModel : ModelWithName
        , IProjectBaseModel
    {
        public ToProjectModel(
        )
        {
        }
    }


    public class Plan
    {
        public Plan(
        )
        {
        }
    }


    public class Month : Plan
    {
        public Month(
        )
        {
        }
    }


    public class BusinessOverviewSalesStatus : ModelWithName
    {
        public BusinessOverviewSalesStatus(
        )
        {
        }
    }


    public class BusinessOverviewInvoiceInfo
    {
        public BusinessOverviewInvoiceInfo(
        )
        {
        }
    }


    public class KeywordCategory
    {
    }


    public class MarketSegmentModel : ModelWithName
    {
        public MarketSegmentModel(
        )
        {
        }
    }


    public class BillingScheduleType
    {
    }


    public class FlatRateProjectModel
        : IEntity
    {
        public FlatRateProjectModel(
        )
        {
        }
    }


    public class ProjectCurrency : ModelWithName
    {
        public ProjectCurrency(
        )
        {
        }
    }


    public class SalesProgress
    {
    }


    public class ProjectCustomer : ModelBaseWithRequiredGuid
    {
        public ProjectCustomer(
        )
        {
        }
    }


    public class ProjectCostCenter : ModelWithName
    {
        public ProjectCostCenter(
        )
        {
        }
    }


    public class ProjectInvoiceTemplate : ModelBase
    {
        public ProjectInvoiceTemplate(
        )
        {
        }
    }


    public class ProjectPricelist : ModelWithName
    {
        public ProjectPricelist(
        )
        {
        }
    }


    public class BillingCustomerModel : ModelBaseWithRequiredGuid
    {
        public BillingCustomerModel(
        )
        {
        }
    }


    public class BillingAddressModel : ModelBase
    {
        public BillingAddressModel(
        )
        {
        }
    }


    public class TypeOfProduct
    {
    }


    public class ProposalRowType
    {
    }


    public class CommunicationTypeModel : ModelBaseWithRequiredGuid
    {
        public CommunicationTypeModel(
        )
        {
        }
    }


    public class FileCategory
    {
    }


    public class InvoiceClientAction : ClientAction
    {
        public InvoiceClientAction(
            string arg0,
            string arg1
        ) : base()
        {
            field0a = arg0;
            field1a = arg1;
        }

        public readonly string field0a;
        public readonly string field1a;
    }


    public class GetValuesDelegate
    {
        public GetValuesDelegate(
            object arg0,
            IntPtr arg1
        ) : base()
        {
            field0a = arg0;
            field1a = arg1;
        }

        public readonly object field0a;
        public readonly IntPtr field1a;
    }


    public class SeriesModel
    {
        public SeriesModel(
        )
        {
        }
    }


    public class AxisModel
    {
        public AxisModel(
        )
        {
        }
    }


    public class DataFieldModel
    {
        public DataFieldModel(
        )
        {
        }
    }


    public class SeriesMetadataModel
    {
        public SeriesMetadataModel(
        )
        {
        }
    }


    public class AllowedXAxis
    {
        public AllowedXAxis(
            string arg0,
            ICollection<XAxis> arg1,
            string arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly string field0;
        public readonly ICollection<XAxis> field1;
        public readonly string field2;
    }


    public class StackingGroupMetadataModel
    {
        public StackingGroupMetadataModel(
        )
        {
        }
    }


    public class AxisMetadataModel
    {
        public AxisMetadataModel(
        )
        {
        }
    }


    public class CompanyCountry : ModelWithName
    {
        public CompanyCountry(
        )
        {
        }
    }


    public class Workweek
    {
    }


    public class EntryFormat
    {
    }


    public class TreeDisplayMode
    {
    }


    public class CompanyModel : ModelWithName
    {
        public CompanyModel(
        )
        {
        }
    }


    public class DistributorModel : ModelWithName
    {
        public DistributorModel(
        )
        {
        }
    }


    public class Customer : ModelWithName
    {
        public Customer(
        )
        {
        }
    }


    public class ContactBase : ModelBase
    {
        public ContactBase(
        )
        {
        }
    }


    public class QuickSearchPersonModel : ModelBase
    {
        public QuickSearchPersonModel(
        )
        {
        }
    }


    public class CustomerContact : ContactBase
    {
        public CustomerContact(
        )
        {
        }
    }


    public class Project : ModelWithName
    {
        public Project(
        )
        {
        }
    }


    public class Invoice : ModelBase
    {
        public Invoice(
        )
        {
        }
    }


    public class Created : ModelBase
    {
        public Created(
        )
        {
        }
    }


    public class File : ModelWithName
    {
        public File(
        )
        {
        }
    }


    public class ActivityCategory
    {
    }


    public class ActivityCustomer : ModelWithName
    {
        public ActivityCustomer(
        )
        {
        }
    }


    public class ActivityActivityType : ModelBaseWithRequiredGuid
    {
        public ActivityActivityType(
        )
        {
        }
    }


    public class ActivityPhase : ModelWithName
    {
        public ActivityPhase(
        )
        {
        }
    }


    public class ActivityOwnerModel : UserWithPhotoFileModelAndRequiredGuid
    {
        public ActivityOwnerModel(
        )
        {
        }
    }


    public class ActivityTypeIcon
    {
    }


    public class AuthenticationOrganization : ModelWithName
    {
        public AuthenticationOrganization(
        )
        {
        }
    }


    public class AuthenticationCompany : ModelWithName
    {
        public AuthenticationCompany(
        )
        {
        }
    }


    public class AuthenticationUser : ModelWithName
    {
        public AuthenticationUser(
        )
        {
        }
    }


    public class UserApplication
    {
        public UserApplication(
        )
        {
        }
    }


    public class AuthenticationOrganizationSettings
    {
        public AuthenticationOrganizationSettings(
        )
        {
        }
    }


    public class AuthenticationCompanySettings
    {
        public AuthenticationCompanySettings(
        )
        {
        }
    }


    public class AuthenticationCountry : ModelWithName
    {
        public AuthenticationCountry(
        )
        {
        }
    }


    public class AuthenticationFormattingCulture : ModelWithName
    {
        public AuthenticationFormattingCulture(
        )
        {
        }
    }


    public class AuthenticationLanguage : ModelWithName
    {
        public AuthenticationLanguage(
        )
        {
        }
    }


    public class AuthenticationTimeZone : ModelWithName
    {
        public AuthenticationTimeZone(
        )
        {
        }
    }


    public class AuthenticationPersonalSettings
    {
        public AuthenticationPersonalSettings(
        )
        {
        }
    }


    public class CreateInvoiceProjectModel
    {
        public CreateInvoiceProjectModel(
        )
        {
        }
    }


    public class PrintSection
    {
    }


    public class CustomerPermissions
    {
    }


    public class ProjectPermissions
    {
    }


    public class ProjectJoinPermissions
    {
    }


    public class UserPermissions
    {
    }


    public class AdministratorPermissions
    {
    }


    public class WorkhourApprovalPermissions
    {
    }


    public class TravelReimbursementPermissions
    {
    }


    public class SharingPermissions
    {
    }


    public class TimeEntryProject : ModelWithName
    {
        public TimeEntryProject(
        )
        {
        }
    }


    public class InvoiceCustomer : ModelWithName
    {
        public InvoiceCustomer(
        )
        {
        }
    }


    public class InvoiceProjectModel : ModelWithName
    {
        public InvoiceProjectModel(
        )
        {
        }
    }


    public class InvoiceReceiverAddressModel
        : IEntity
    {
        public InvoiceReceiverAddressModel(
        )
        {
        }
    }


    public class InvoiceSenderAddressModel
        : IEntity
    {
        public InvoiceSenderAddressModel(
        )
        {
        }
    }


    public class RelatedInvoiceModel : ModelBase
    {
        public RelatedInvoiceModel(
        )
        {
        }
    }


    public class InvoiceLanguageModel : ModelWithName
    {
        public InvoiceLanguageModel(
        )
        {
        }
    }


    public class ProposalCustomer : ModelWithName
    {
        public ProposalCustomer(
        )
        {
        }
    }


    public class ProposalProject : ModelBaseWithRequiredGuid
    {
        public ProposalProject(
        )
        {
        }
    }


    public class CustomerContactPersonModel
        : IEntity
    {
        public CustomerContactPersonModel(
        )
        {
        }
    }


    public class ProposalBillingAddressModel : ModelBase
    {
        public ProposalBillingAddressModel(
        )
        {
        }
    }


    public class FixedPriceIncrease
    {
        public FixedPriceIncrease(
        )
        {
        }
    }


    public class PercentagePriceIncrease
    {
        public PercentagePriceIncrease(
        )
        {
        }
    }


    public class WorktypeModel : ModelBaseWithRequiredGuid
    {
        public WorktypeModel(
        )
        {
        }
    }


    public class IconType
    {
    }


    public class PhaseCustomer : ModelWithName
    {
        public PhaseCustomer(
        )
        {
        }
    }

    public class WorkHourCustomer : ModelWithName
    {
        public WorkHourCustomer(
        )
        {
        }
    }


    public class WorkHourInvoice : ModelBase
    {
        public WorkHourInvoice(
        )
        {
        }
    }


    public class BillableStatusType
    {
    }


    public class ExpenseType
    {
    }


    public class ProductType
    {
    }


    public class ProductCurrency : ModelWithName
    {
        public ProductCurrency(
        )
        {
        }
    }


    public class ProductSalesAccount : ModelWithName
    {
        public ProductSalesAccount(
        )
        {
        }
    }


    public class ProjectFeeCustomer : ModelWithName
    {
        public ProjectFeeCustomer(
        )
        {
        }
    }


    public class RecurrenceEndTypes
    {
    }


    public class ProjectSalesAccount : ModelWithName
    {
        public ProjectSalesAccount(
        )
        {
        }
    }

    public class ProjectTravelExpenseTravelReimbursementModel : ModelBase
    {
        public ProjectTravelExpenseTravelReimbursementModel(
        )
        {
        }
    }


    public class ProjectTravelExpenseInvoice : ModelBase
    {
        public ProjectTravelExpenseInvoice(
        )
        {
        }
    }


    public class ExpensesClass
    {
    }


    public class ProjectFeePhase : ModelWithName
    {
        public ProjectFeePhase(
        )
        {
        }
    }


    public class ProjectFeeInvoice : ModelBase
    {
        public ProjectFeeInvoice(
        )
        {
        }
    }


    public class BusinessUnitCostCenterModel : ModelWithName
    {
        public BusinessUnitCostCenterModel(
        )
        {
        }
    }


    public class ContactCustomer
        : IEntity
    {
        public ContactCustomer(
        )
        {
        }
    }


    public class CountryRegionsStatus
    {
    }


    public class CountryLanguage : ModelWithName
    {
        public CountryLanguage(
        )
        {
        }
    }


    public class CountryTimeZone : ModelWithName
    {
        public CountryTimeZone(
        )
        {
        }
    }


    public class AddressCountry : ModelWithName
    {
        public AddressCountry(
        )
        {
        }
    }


    public class NetFinancialsCustomerClass
        : IEntity
    {
        public NetFinancialsCustomerClass(
        )
        {
        }
    }


    public class CustomerCurrency : ModelWithName
    {
        public CustomerCurrency(
        )
        {
        }
    }


    public class CustomerInvoicingVat : ModelBase
    {
        public CustomerInvoicingVat(
        )
        {
        }
    }


    public class CustomerHeadquarterAddress : ModelBase
    {
        public CustomerHeadquarterAddress(
        )
        {
        }
    }


    public class Weekday
    {
    }


    public class UserCountry : ModelWithName
    {
        public UserCountry(
        )
        {
        }
    }


    public class UserFormattingCulture : ModelWithName
    {
        public UserFormattingCulture(
        )
        {
        }
    }


    public class UserLanguage : ModelWithName
    {
        public UserLanguage(
        )
        {
        }
    }


    public class UserTimeZone : ModelWithName
    {
        public UserTimeZone(
        )
        {
        }
    }


    public class ResourceAllocationAction
    {
    }


    public class PhaseMember : ModelWithName
    {
        public PhaseMember(
        )
        {
        }
    }


    public class PhaseProject : ModelBaseWithRequiredGuid
    {
        public PhaseProject(
        )
        {
        }
    }


    public class PricelistCurrency : ModelBaseWithRequiredGuid
    {
        public PricelistCurrency(
        )
        {
        }
    }


    public class UserWorkContractModel : WorkContractBaseModel
    {
        public UserWorkContractModel(
        )
        {
        }
    }


    public class UserKeywordModel
    {
        public UserKeywordModel(
        )
        {
        }
    }


    public class AccountClientAction : ClientAction
    {
        public AccountClientAction(
            string arg0,
            string arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly string field1;
    }


    public class AccountSegmentation
    {
        public AccountSegmentation(
        )
        {
        }
    }


    public class ActivityKey
    {
        public ActivityKey(
        )
        {
        }
    }


    public class ActivityClientAction : ClientAction
    {
        public ActivityClientAction(
            string arg0,
            string arg1,
            DateTime arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly string field0;
        public readonly string field1;
        public readonly DateTime field2;
    }


    public class OpenExternalFileClientAction : ClientAction
    {
        public OpenExternalFileClientAction(
            string arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly string field0;
    }


    public class OpenFileClientAction : ClientAction
    {
        public OpenFileClientAction(
            string arg0,
            string arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly string field1;
    }


    public class CaseFileParameters
    {
        public CaseFileParameters(
        )
        {
        }
    }


    public class CaseClientAction : ClientAction
    {
        public CaseClientAction(
            string arg0,
            string arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly string field1;
    }


    public class CustomPricingParameters
    {
        public CustomPricingParameters(
        )
        {
        }
    }


    public class GraphType
    {
        public GraphType(
        )
        {
        }
    }

    public class OfferClientAction : ClientAction
    {
        public OfferClientAction(
            string arg0,
            string arg1
        ) : base()
        {
            field0a = arg0;
            field1a = arg1;
        }

        public readonly string field0a;
        public readonly string field1a;
    }


    public class MassUpdateControl<TReport, TContext>
    {
    }
}