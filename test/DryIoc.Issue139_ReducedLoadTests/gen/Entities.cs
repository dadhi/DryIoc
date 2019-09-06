using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using RM;
using Data;
using Framework;
using Logic;
using OrganizationBase;
using Shared;

namespace Entities
{
    public class InternalXmlHelper
    {
    }


    public class AccountKpi
    {
        public AccountKpi(
        )
        {
        }
    }


    public class BusinessUnitKpi
    {
        public BusinessUnitKpi(
        )
        {
        }
    }


    public class CaseKpi
    {
        public CaseKpi(
        )
        {
        }
    }


    public class ContactKpi
    {
        public ContactKpi(
        )
        {
        }
    }


    public class HourKpi
    {
        public HourKpi(
        )
        {
        }
    }


    public class InvoiceKpi
    {
        public InvoiceKpi(
        )
        {
        }
    }


    public class ItemKpi
    {
        public ItemKpi(
        )
        {
        }
    }


    public class OfferKpi
    {
        public OfferKpi(
        )
        {
        }
    }


    public class TaskKpi
    {
        public TaskKpi(
        )
        {
        }
    }


    public class TimeEntryKpi
    {
        public TimeEntryKpi(
        )
        {
        }
    }


    public class UserKpi
    {
        public UserKpi(
        )
        {
        }
    }


    public class WorkTimeKpi
    {
        public WorkTimeKpi(
        )
        {
        }
    }


    public class ActivityCategory
    {
        public ActivityCategory(
        )
        {
        }
    }


    public class AttachmentItem
    {
        public AttachmentItem(
        )
        {
        }
    }


    public class BilledStatus
    {
    }


    public class BillingInformationUpdate
    {
        public BillingInformationUpdate(
        )
        {
        }
    }


    public class BusinessOverviewFields : OrganizationEntity
    {
        public BusinessOverviewFields(
        )
        {
        }
    }


    public class CaseCopyInfo
    {
        public CaseCopyInfo(
        )
        {
        }
    }


    public class CaseOverviewSettings
    {
        public CaseOverviewSettings(
        )
        {
        }
    }


    public class CaseOverViewStats
    {
        public CaseOverViewStats(
        )
        {
        }
    }


    public class CaseRevenueAndInvoicingEx : CaseRevenueAndInvoicing
    {
        public CaseRevenueAndInvoicingEx(
        )
        {
        }
    }


    public class CaseRevenueAndInvoicing
    {
        public CaseRevenueAndInvoicing(
        )
        {
        }
    }


    public class CaseStatusHistory
    {
        public CaseStatusHistory(
        )
        {
        }
    }


    public class CustomWorkType
    {
        public CustomWorkType(
        )
        {
        }
    }


    public class FileAttachment : OrganizationEntity
    {
    }


    public class FileQuotaInfo
    {
        public FileQuotaInfo(
        )
        {
        }
    }


    public interface IActivity
    {
    }


    public class ActivityEx : Activity
        , IActivity
    {
        public ActivityEx(
        )
        {
        }
    }


    public class ParticipantType
    {
    }


    public interface IActivityParticipant
    {
    }


    public class ActivityContactMemberEx : ActivityContactMember
        , IActivityParticipant
    {
        public ActivityContactMemberEx(
        )
        {
        }
    }


    public interface IHour
    {
    }


    public class HourFields : OrganizationEntity
        , IHour
    {
        public HourFields(
        )
        {
        }
    }


    public interface INote
        : IOrganizationEntity
    {
    }


    public class AccountNote : AccountNoteFields
        , INote, IIdentifiableEntityWithOriginalState<AccountNoteFields>
    {
        public AccountNote(
        )
        {
        }
    }


    public class InvoiceLogEntryContext
    {
    }


    public interface ITagEntity
        : IOrganizationEntity
    {
    }


    public interface ITaskNameable
    {
    }


    public class TreeTask : TreeTaskFields
        , ITaskNameable, IIdentifiableEntityWithOriginalState<TreeTaskFields>
    {
        public TreeTask(
        )
        {
        }
    }


    public interface ITaxTotalPart
    {
    }


    public class InvoiceRowWithVat : InvoiceRow
        , ITaxTotalPart
    {
        public InvoiceRowWithVat(
            InvoiceRow arg0
        ) : base()
        {
            field0 = arg0;
        }

        public readonly InvoiceRow field0;
    }


    public class KPIComparisonFields : OrganizationEntity
    {
        public KPIComparisonFields(
        )
        {
        }
    }


    public class KPIComparison : KPIComparisonFields
        , IIdentifiableEntityWithOriginalState<KPIComparisonFields>
    {
        public KPIComparison(
        )
        {
        }
    }


    public class KPIComparisonHelper
    {
        public KPIComparisonHelper(
        )
        {
        }
    }


    public class OvertimePricing
    {
        public OvertimePricing(
        )
        {
        }
    }


    public class PdfVersionType
    {
    }


    public class ProductPricing
    {
        public ProductPricing(
        )
        {
        }
    }


    public class ProductWithCompanyInfo : ProductWithCountryInfo
    {
        public ProductWithCompanyInfo(
        )
        {
        }
    }


    public class ProductWithCountryInfo : Product
    {
        public ProductWithCountryInfo(
        )
        {
        }
    }


    public class ProfileRights
    {
        public ProfileRights(
        )
        {
        }
    }


    public class ResourceAllocationCaseFinancialForecast
    {
        public ResourceAllocationCaseFinancialForecast(
        )
        {
        }
    }


    public class ResourceAllocationHourHelper
    {
        public ResourceAllocationHourHelper(
        )
        {
        }
    }


    public class ResourceAllocationPhaseSummary
    {
        public ResourceAllocationPhaseSummary(
        )
        {
        }
    }


    public class ResourceAllocationUserAllocation
    {
        public ResourceAllocationUserAllocation(
        )
        {
        }
    }


    public class ResourceAllocationProjectSummary
    {
        public ResourceAllocationProjectSummary(
        )
        {
        }
    }


    public class ResourceAllocationTotalHelper
    {
        public ResourceAllocationTotalHelper(
        )
        {
        }
    }


    public class ResourceAllocationUserWorkloadTotal
    {
        public ResourceAllocationUserWorkloadTotal(
        )
        {
        }
    }


    public class ResourceAllocationWorkload
    {
        public ResourceAllocationWorkload(
        )
        {
        }
    }


    public class Absence
    {
        public Absence(
        )
        {
        }
    }


    public class PhaseResourceAllocationHours
    {
        public PhaseResourceAllocationHours(
        )
        {
        }
    }


    public class TaxBreakdown
    {
        public TaxBreakdown(
        )
        {
        }
    }


    public class TeamProductivitySettings
    {
        public TeamProductivitySettings(
        )
        {
        }

        public TeamProductivitySettings(
            ShareOfBillingDividend arg0,
            InvoicingSegment arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly ShareOfBillingDividend field0;
        public readonly InvoicingSegment field1;
    }


    public class ShareOfBillingDividend
    {
    }


    public class TermsOfServiceApprovalStatusType
    {
    }


    public class TimeEntrySuggestedRowFields : OrganizationEntity
    {
        public TimeEntrySuggestedRowFields(
        )
        {
        }
    }


    public class TimeEntrySuggestedRow : TimeEntrySuggestedRowFields
        , IIdentifiableEntityWithOriginalState<TimeEntrySuggestedRowFields>
    {
        public TimeEntrySuggestedRow(
        )
        {
        }
    }


    public class TravelExpenseReceiptMetadata
    {
        public TravelExpenseReceiptMetadata(
        )
        {
        }
    }


    public class TreeTaskFields : OrganizationEntity
    {
        public TreeTaskFields(
        )
        {
        }
    }


    public class WorkHourApprovalMode
    {
    }


    public class WorkHourEditMode
    {
    }


    public class WorkHourSuggestedRowFields : OrganizationEntity
    {
        public WorkHourSuggestedRowFields(
        )
        {
        }
    }


    public class WorkHourSuggestedRow : WorkHourSuggestedRowFields
        , IIdentifiableEntityWithOriginalState<WorkHourSuggestedRowFields>
    {
        public WorkHourSuggestedRow(
        )
        {
        }
    }


    public class WorkTypeWithCompanyAndUsageInfo : WorkTypeWithCompanyInfo
    {
        public WorkTypeWithCompanyAndUsageInfo(
        )
        {
        }
    }


    public class WorkTypeWithCompanyInfo : WorkType
    {
        public WorkTypeWithCompanyInfo(
        )
        {
        }
    }


    public class AccountUsage : Account
    {
        public AccountUsage(
        )
        {
        }
    }


    public class Activity : ActivityFields
        , IIdentifiableEntityWithOriginalState<ActivityFields>
    {
        public Activity(
        )
        {
        }
    }


    public class ActivityOtherMemberEx : CalendarSyncActivityNonAppParticipant
        , IActivityParticipant
    {
        public ActivityOtherMemberEx(
        )
        {
        }
    }


    public class ActivityResourceMemberEx : ActivityResourceMember
        , IActivityParticipant
    {
        public ActivityResourceMemberEx(
        )
        {
        }
    }


    public class ActivityUserMember : ActivityUserMemberFields
        , IIdentifiableEntityWithOriginalState<ActivityUserMemberFields>
    {
        public ActivityUserMember(
        )
        {
        }
    }


    public class ActivityUserMemberEx : ActivityUserMember
        , IActivityParticipant
    {
        public ActivityUserMemberEx(
        )
        {
        }
    }


    public class BackgroundTask : BackgroundTaskFields
        , IIdentifiableEntityWithOriginalState<BackgroundTaskFields>
    {
        public BackgroundTask(
        )
        {
        }
    }


    public class BillingPlan : BillingPlanFields
        , IIdentifiableEntityWithOriginalState<BillingPlanFields>
    {
        public BillingPlan(
        )
        {
        }
    }


    public class CalendarSyncActivityMapEx : CalendarSyncActivityMap
    {
        public CalendarSyncActivityMapEx(
        )
        {
        }
    }


    public class CalendarSyncDeviceEx : CalendarSyncDevice
    {
        public CalendarSyncDeviceEx(
        )
        {
        }
    }


    public class CalendarSyncParticipant
        : Utilities.IObjectDescription
    {
        public CalendarSyncParticipant(
        )
        {
        }

        public CalendarSyncParticipant(
            CalendarSyncParticipant arg0
        )
        {
            field0 = arg0;
        }

        public readonly CalendarSyncParticipant field0;
    }


    public class CalendarSyncUserCalendarEx : CalendarSyncUserCalendar
    {
        public CalendarSyncUserCalendarEx(
        )
        {
        }
    }


    public partial class Case : CaseFields
        , IIdentifiableEntityWithOriginalState<CaseFields>
    {
    }


    public class CaseBillingAccountEx : CaseBillingAccount
    {
        public CaseBillingAccountEx(
        )
        {
        }
    }


    public class CaseFile : CaseFileFields
        , IIdentifiableEntityWithOriginalState<CaseFileFields>
    {
        public CaseFile(
        )
        {
        }
    }


    public class CaseHour
    {
        public CaseHour(
        )
        {
        }
    }


    public class CaseMember : CaseMemberFields
        , INamedEntity, IIdentifiableEntityWithOriginalState<CaseMemberFields>
    {
        public CaseMember(
        )
        {
        }
    }


    public class CaseMemberForTeamProductivity : CaseMember
    {
        public CaseMemberForTeamProductivity(
        )
        {
        }
    }


    public class CaseNote : CaseNoteFields
        , INote, IIdentifiableEntityWithOriginalState<CaseNoteFields>
    {
        public CaseNote(
        )
        {
        }
    }


    public class CaseProductEx : CaseProduct
    {
        public CaseProductEx(
        )
        {
        }
    }


    public class CaseTag : CaseTagFields
        , ITagEntity, IIdentifiableEntityWithOriginalState<CaseTagFields>, ITagEntity<Case>
    {
        public CaseTag(
        )
        {
        }
    }


    public class CaseTagEx : CaseTag
    {
        public CaseTagEx(
        )
        {
        }
    }


    public class CaseTreeDisplayMode
    {
    }


    public class CaseSalesStatus
    {
    }


    public class CaseUsage : Case
    {
        public CaseUsage(
        )
        {
        }
    }


    public class CaseWorkHoursSummary
    {
        public CaseWorkHoursSummary(
        )
        {
        }
    }


    public class CaseWorkHoursSummaryCriteria
    {
        public CaseWorkHoursSummaryCriteria(
        )
        {
        }
    }


    public class CommunicationMethodType
    {
    }


    public class CompanyEx : Company
    {
        public CompanyEx(
        )
        {
        }
    }


    public class ContactTag : ContactTagFields
        , ITagEntity, IIdentifiableEntityWithOriginalState<ContactTagFields>, ITagEntity<Contact>
    {
        public ContactTag(
        )
        {
        }
    }


    public class ContactTagEx : ContactTag
    {
        public ContactTagEx(
        )
        {
        }
    }


    public class CostCenterRevenue : CostCenterRevenueFields
        , IIdentifiableEntityWithOriginalState<CostCenterRevenueFields>
    {
        public CostCenterRevenue(
        )
        {
        }
    }


    public class CostCenterRevenueAndInvoicing : CostCenterRevenueEx
    {
        public CostCenterRevenueAndInvoicing(
        )
        {
        }
    }


    public class CostCenterRevenueEx : CostCenterRevenue
    {
        public CostCenterRevenueEx(
        )
        {
        }
    }


    public class CountryProduct : CountryProductFields
        , IIdentifiableEntityWithOriginalState<CountryProductFields>
    {
        public CountryProduct(
        )
        {
        }
    }


    public class DashboardPart : DashboardPartFields
        , IIdentifiableEntityWithOriginalState<DashboardPartFields>
    {
        public DashboardPart(
        )
        {
        }
    }


    public class ExpenseClass
    {
    }


    public class FileTag : FileTagFields
        , ITagEntity, IIdentifiableEntityWithOriginalState<FileTagFields>, ITagEntity<File>
    {
        public FileTag(
        )
        {
        }
    }


    public class FileTagEx : FileTag
    {
        public FileTagEx(
        )
        {
        }
    }


    public class DashboardFields : OrganizationEntity
        , INamedEntity
    {
        public DashboardFields(
        )
        {
        }
    }


    public class Dashboard : DashboardFields
        , IIdentifiableEntityWithOriginalState<DashboardFields>
    {
        public Dashboard(
        )
        {
        }
    }


    public class DashboardPartFields : OrganizationEntity
        , INamedEntity
    {
        public DashboardPartFields(
        )
        {
        }
    }


    public class ProfileDashboardFields : OrganizationEntity
    {
        public ProfileDashboardFields(
        )
        {
        }
    }


    public class ProfileDashboard : ProfileDashboardFields
        , IIdentifiableEntityWithOriginalState<ProfileDashboardFields>
    {
        public ProfileDashboard(
        )
        {
        }
    }


    public class OrganizationSsoCompanyFields : OrganizationEntity
        , INamedEntity
    {
        public OrganizationSsoCompanyFields(
        )
        {
        }
    }


    public class OrganizationSsoCompany : OrganizationSsoCompanyFields
        , IIdentifiableEntityWithOriginalState<OrganizationSsoCompanyFields>
    {
        public OrganizationSsoCompany(
        )
        {
        }
    }


    public class OrganizationSSODomainFields : OrganizationEntity
    {
        public OrganizationSSODomainFields(
        )
        {
        }
    }


    public class OrganizationSSODomain : OrganizationSSODomainFields
        , IIdentifiableEntityWithOriginalState<OrganizationSSODomainFields>
    {
        public OrganizationSSODomain(
        )
        {
        }
    }


    public class AccountNoteFields : OrganizationEntity
    {
        public AccountNoteFields(
        )
        {
        }
    }


    public class ActivityFields : OrganizationEntity
        , INamedEntity
    {
        public ActivityFields(
        )
        {
        }
    }


    public class ActivityStatusFields : OrganizationEntity
        , INamedEntity
    {
        public ActivityStatusFields(
        )
        {
        }
    }


    public class ActivityStatus : ActivityStatusFields
        , IIdentifiableEntityWithOriginalState<ActivityStatusFields>
    {
        public ActivityStatus(
        )
        {
        }
    }


    public class ActivityTypeFields : OrganizationEntity
        , INamedEntity
    {
        public ActivityTypeFields(
        )
        {
        }
    }


    public class ActivityType : ActivityTypeFields
        , IIdentifiableEntityWithOriginalState<ActivityTypeFields>
    {
        public ActivityType(
        )
        {
        }
    }


    public class ActivityContactMemberFields : OrganizationEntity
    {
        public ActivityContactMemberFields(
        )
        {
        }
    }


    public class ActivityContactMember : ActivityContactMemberFields
        , IIdentifiableEntityWithOriginalState<ActivityContactMemberFields>
    {
        public ActivityContactMember(
        )
        {
        }
    }


    public class ActivityResourceMemberFields : OrganizationEntity
    {
        public ActivityResourceMemberFields(
        )
        {
        }
    }


    public class ActivityResourceMember : ActivityResourceMemberFields
        , IIdentifiableEntityWithOriginalState<ActivityResourceMemberFields>
    {
        public ActivityResourceMember(
        )
        {
        }
    }


    public class ActivityUserMemberFields : OrganizationEntity
    {
        public ActivityUserMemberFields(
        )
        {
        }
    }


    public class AuthorizedIPAddressFields : OrganizationEntity
    {
        public AuthorizedIPAddressFields(
        )
        {
        }
    }


    public class AuthorizedIPAddress : AuthorizedIPAddressFields
        , IIdentifiableEntityWithOriginalState<AuthorizedIPAddressFields>
    {
        public AuthorizedIPAddress(
        )
        {
        }
    }


    public class BackgroundTaskProgressFields : OrganizationEntity
    {
        public BackgroundTaskProgressFields(
        )
        {
        }
    }


    public class BackgroundTaskProgress : BackgroundTaskProgressFields
        , IIdentifiableEntityWithOriginalState<BackgroundTaskProgressFields>
    {
        public BackgroundTaskProgress(
        )
        {
        }
    }


    public class BackgroundTaskFields : OrganizationEntity
        , INamedEntity
    {
        public BackgroundTaskFields(
        )
        {
        }
    }


    public class BackgroundTaskRunFields : OrganizationEntity
    {
        public BackgroundTaskRunFields(
        )
        {
        }
    }


    public class BackgroundTaskRun : BackgroundTaskRunFields
        , IIdentifiableEntityWithOriginalState<BackgroundTaskRunFields>
    {
        public BackgroundTaskRun(
        )
        {
        }
    }


    public class BankAccountFields : OrganizationEntity
    {
        public BankAccountFields(
        )
        {
        }
    }


    public class BankAccount : BankAccountFields
        , IIdentifiableEntityWithOriginalState<BankAccountFields>
    {
        public BankAccount(
        )
        {
        }
    }


    public class BillingPlanFields : OrganizationEntity
    {
        public BillingPlanFields(
        )
        {
        }
    }


    public class BusinessUnitFields : OrganizationEntity
        , INamedEntity
    {
        public BusinessUnitFields(
        )
        {
        }
    }


    public class BusinessUnit : BusinessUnitFields
        , IIdentifiableEntityWithOriginalState<BusinessUnitFields>
    {
        public BusinessUnit(
        )
        {
        }
    }


    public class CalendarSyncActivityMapFields : OrganizationEntity
    {
        public CalendarSyncActivityMapFields(
        )
        {
        }
    }


    public class CalendarSyncActivityMap : CalendarSyncActivityMapFields
        , IIdentifiableEntityWithOriginalState<CalendarSyncActivityMapFields>
    {
        public CalendarSyncActivityMap(
        )
        {
        }
    }


    public class CalendarSyncActivityNonAppParticipantFields : OrganizationEntity
        , INamedEntity
    {
        public CalendarSyncActivityNonAppParticipantFields(
        )
        {
        }
    }


    public class CalendarSyncActivityNonAppParticipant : CalendarSyncActivityNonAppParticipantFields
        , IIdentifiableEntityWithOriginalState<CalendarSyncActivityNonAppParticipantFields>
    {
        public CalendarSyncActivityNonAppParticipant(
        )
        {
        }
    }


    public class CalendarSyncDeviceFields : OrganizationEntity
    {
        public CalendarSyncDeviceFields(
        )
        {
        }
    }


    public class CalendarSyncDevice : CalendarSyncDeviceFields
        , IIdentifiableEntityWithOriginalState<CalendarSyncDeviceFields>
    {
        public CalendarSyncDevice(
        )
        {
        }
    }


    public class CalendarSyncUserCalendarFields : OrganizationEntity
    {
        public CalendarSyncUserCalendarFields(
        )
        {
        }
    }


    public class CalendarSyncUserCalendar : CalendarSyncUserCalendarFields
        , IIdentifiableEntityWithOriginalState<CalendarSyncUserCalendarFields>
    {
        public CalendarSyncUserCalendar(
        )
        {
        }
    }


    public class CaseFields : OrganizationEntity
        , INamedEntity
    {
        public CaseFields(
        )
        {
        }
    }


    public class CaseBillingAccountFields : OrganizationEntity
    {
        public CaseBillingAccountFields(
        )
        {
        }
    }


    public class CaseBillingAccount : CaseBillingAccountFields
        , IIdentifiableEntityWithOriginalState<CaseBillingAccountFields>
    {
        public CaseBillingAccount(
        )
        {
        }
    }


    public class CaseCommentFields : OrganizationEntity
    {
        public CaseCommentFields(
        )
        {
        }
    }


    public class CaseComment : CaseCommentFields
        , IIdentifiableEntityWithOriginalState<CaseCommentFields>
    {
        public CaseComment(
        )
        {
        }
    }


    public class CaseFileFields : OrganizationEntity
    {
        public CaseFileFields(
        )
        {
        }
    }


    public class CaseMemberFields : OrganizationEntity
    {
        public CaseMemberFields(
        )
        {
        }
    }


    public class CaseNoteFields : OrganizationEntity
    {
        public CaseNoteFields(
        )
        {
        }
    }


    public class CaseProductFields : OrganizationEntity
    {
        public CaseProductFields(
        )
        {
        }
    }


    public class CaseProduct : CaseProductFields
        , IIdentifiableEntityWithOriginalState<CaseProductFields>
    {
        public CaseProduct(
        )
        {
        }
    }


    public class CaseStatusFields : OrganizationEntity
    {
        public CaseStatusFields(
        )
        {
        }
    }


    public class CaseStatus : CaseStatusFields
        , IIdentifiableEntityWithOriginalState<CaseStatusFields>
    {
        public CaseStatus(
        )
        {
        }
    }


    public class CaseStatusTypeFields : OrganizationEntity
        , INamedEntity
    {
        public CaseStatusTypeFields(
        )
        {
        }
    }


    public class CaseStatusType : CaseStatusTypeFields
        , IIdentifiableEntityWithOriginalState<CaseStatusTypeFields>
    {
        public CaseStatusType(
        )
        {
        }
    }


    public class CaseTagFields : OrganizationEntity
    {
        public CaseTagFields(
        )
        {
        }
    }


    public class CaseWorkTypeFields : OrganizationEntity
    {
        public CaseWorkTypeFields(
        )
        {
        }
    }


    public class CaseWorkType : CaseWorkTypeFields
        , IIdentifiableEntityWithOriginalState<CaseWorkTypeFields>
    {
        public CaseWorkType(
        )
        {
        }
    }


    public class CommunicationMethodFields : OrganizationEntity
        , INamedEntity
    {
        public CommunicationMethodFields(
        )
        {
        }
    }


    public class CommunicationMethod : CommunicationMethodFields
        , IIdentifiableEntityWithOriginalState<CommunicationMethodFields>
    {
        public CommunicationMethod(
        )
        {
        }
    }


    public class ContactRoleFields : OrganizationEntity
        , INamedEntity
    {
        public ContactRoleFields(
        )
        {
        }
    }


    public class ContactRole : ContactRoleFields
        , IIdentifiableEntityWithOriginalState<ContactRoleFields>
    {
        public ContactRole(
        )
        {
        }
    }


    public class ContactTagFields : OrganizationEntity
    {
        public ContactTagFields(
        )
        {
        }
    }


    public class CostAccountFields : OrganizationEntity
        , INamedEntity
    {
        public CostAccountFields(
        )
        {
        }
    }


    public class CostAccount : CostAccountFields
        , IIdentifiableEntityWithOriginalState<CostAccountFields>
    {
        public CostAccount(
        )
        {
        }
    }


    public class CostCenterFields : OrganizationEntity
        , INamedEntity
    {
        public CostCenterFields(
        )
        {
        }
    }


    public class CostCenter : CostCenterFields
        , IIdentifiableEntityWithOriginalState<CostCenterFields>
    {
        public CostCenter(
        )
        {
        }
    }


    public class CostCenterRevenueFields : OrganizationEntity
    {
        public CostCenterRevenueFields(
        )
        {
        }
    }


    public class CountryProductFields : OrganizationEntity
    {
        public CountryProductFields(
        )
        {
        }
    }


    public class CustomFormulaFields : OrganizationEntity
        , INamedEntity
    {
        public CustomFormulaFields(
        )
        {
        }
    }


    public class CustomFormula : CustomFormulaFields
        , IIdentifiableEntityWithOriginalState<CustomFormulaFields>
    {
        public CustomFormula(
        )
        {
        }
    }


    public class CustomFormulaSetFields : OrganizationEntity
        , INamedEntity
    {
        public CustomFormulaSetFields(
        )
        {
        }
    }


    public class CustomFormulaSet : CustomFormulaSetFields
        , IIdentifiableEntityWithOriginalState<CustomFormulaSetFields>
    {
        public CustomFormulaSet(
        )
        {
        }
    }


    public class ExtranetCaseContactFields : OrganizationEntity
    {
        public ExtranetCaseContactFields(
        )
        {
        }
    }


    public class ExtranetCaseContact : ExtranetCaseContactFields
        , IIdentifiableEntityWithOriginalState<ExtranetCaseContactFields>
    {
        public ExtranetCaseContact(
        )
        {
        }
    }


    public class ExtranetCaseInfoFields : OrganizationEntity
    {
        public ExtranetCaseInfoFields(
        )
        {
        }
    }


    public class ExtranetCaseInfo : ExtranetCaseInfoFields
        , IIdentifiableEntityWithOriginalState<ExtranetCaseInfoFields>
    {
        public ExtranetCaseInfo(
        )
        {
        }
    }


    public class FileFields : OrganizationEntity
        , INamedEntity
    {
        public FileFields(
        )
        {
        }
    }


    public class File : FileFields
        , IIdentifiableEntityWithOriginalState<FileFields>
    {
        public File(
        )
        {
        }
    }


    public class FileDownloadFields : OrganizationEntity
    {
        public FileDownloadFields(
        )
        {
        }
    }


    public class FileDownload : FileDownloadFields
        , IIdentifiableEntityWithOriginalState<FileDownloadFields>
    {
        public FileDownload(
        )
        {
        }
    }


    public class FileTagFields : OrganizationEntity
    {
        public FileTagFields(
        )
        {
        }
    }


    public class Hour : HourFields
        , IIdentifiableEntityWithOriginalState<HourFields>
    {
        public Hour(
        )
        {
        }
    }


    public class ImportFields : OrganizationEntity
    {
        public ImportFields(
        )
        {
        }
    }


    public class Import : ImportFields
        , IIdentifiableEntityWithOriginalState<ImportFields>
    {
        public Import(
        )
        {
        }
    }


    public class IndustryFields : OrganizationEntity
        , INamedEntity
    {
        public IndustryFields(
        )
        {
        }
    }


    public class Industry : IndustryFields
        , IIdentifiableEntityWithOriginalState<IndustryFields>
    {
        public Industry(
        )
        {
        }
    }


    public class IntegrationErrorFields : OrganizationEntity
    {
        public IntegrationErrorFields(
        )
        {
        }
    }


    public class IntegrationError : IntegrationErrorFields
        , IIdentifiableEntityWithOriginalState<IntegrationErrorFields>
    {
        public IntegrationError(
        )
        {
        }
    }


    public class InvoiceFields : OrganizationEntity
    {
        public InvoiceFields(
        )
        {
        }
    }


    public class Invoice : InvoiceFields
        , IIdentifiableEntityWithOriginalState<InvoiceFields>
    {
        public Invoice(
        )
        {
        }
    }


    public class InvoiceBankAccountFields : OrganizationEntity
    {
        public InvoiceBankAccountFields(
        )
        {
        }
    }


    public class InvoiceBankAccount : InvoiceBankAccountFields
        , IIdentifiableEntityWithOriginalState<InvoiceBankAccountFields>
    {
        public InvoiceBankAccount(
        )
        {
        }
    }


    public class InvoiceCaseFields : OrganizationEntity
    {
        public InvoiceCaseFields(
        )
        {
        }
    }


    public class InvoiceCase : InvoiceCaseFields
        , IIdentifiableEntityWithOriginalState<InvoiceCaseFields>
    {
        public InvoiceCase(
        )
        {
        }
    }


    public class InvoiceConfigFields : OrganizationEntity
    {
        public InvoiceConfigFields(
        )
        {
        }
    }


    public class InvoiceConfig : InvoiceConfigFields
        , IIdentifiableEntityWithOriginalState<InvoiceConfigFields>
    {
        public InvoiceConfig(
        )
        {
        }
    }


    public class InvoiceFileFields : OrganizationEntity
    {
        public InvoiceFileFields(
        )
        {
        }
    }


    public class InvoiceFile : InvoiceFileFields
        , IIdentifiableEntityWithOriginalState<InvoiceFileFields>
    {
        public InvoiceFile(
        )
        {
        }
    }


    public class InvoiceHTMLFields : OrganizationEntity
    {
        public InvoiceHTMLFields(
        )
        {
        }
    }


    public class InvoiceHTML : InvoiceHTMLFields
        , IIdentifiableEntityWithOriginalState<InvoiceHTMLFields>
    {
        public InvoiceHTML(
        )
        {
        }
    }


    public class InvoiceRowFields : OrganizationEntity
    {
        public InvoiceRowFields(
        )
        {
        }
    }


    public class InvoiceRow : InvoiceRowFields
        , IIdentifiableEntityWithOriginalState<InvoiceRowFields>
    {
        public InvoiceRow(
        )
        {
        }
    }


    public class InvoiceStatusFields : OrganizationEntity
        , INamedEntity
    {
        public InvoiceStatusFields(
        )
        {
        }
    }


    public class InvoiceStatus : InvoiceStatusFields
        , IIdentifiableEntityWithOriginalState<InvoiceStatusFields>
    {
        public InvoiceStatus(
        )
        {
        }
    }


    public class InvoiceStatusHistoryFields : OrganizationEntity
    {
        public InvoiceStatusHistoryFields(
        )
        {
        }
    }


    public class InvoiceStatusHistory : InvoiceStatusHistoryFields
        , IIdentifiableEntityWithOriginalState<InvoiceStatusHistoryFields>
    {
        public InvoiceStatusHistory(
        )
        {
        }
    }


    public class InvoiceTemplateFields : OrganizationEntity
    {
        public InvoiceTemplateFields(
        )
        {
        }
    }


    public class InvoiceTemplate : InvoiceTemplateFields
        , IIdentifiableEntityWithOriginalState<InvoiceTemplateFields>
    {
        public InvoiceTemplate(
        )
        {
        }
    }


    public class InvoiceTemplateConfigFields : OrganizationEntity
    {
        public InvoiceTemplateConfigFields(
        )
        {
        }
    }


    public class InvoiceTemplateConfig : InvoiceTemplateConfigFields
        , IIdentifiableEntityWithOriginalState<InvoiceTemplateConfigFields>
    {
        public InvoiceTemplateConfig(
        )
        {
        }
    }


    public class ItemFields : OrganizationEntity
        , INamedEntity
    {
        public ItemFields(
        )
        {
        }
    }


    public class Item : ItemFields
        , IIdentifiableEntityWithOriginalState<ItemFields>
    {
        public Item(
        )
        {
        }
    }


    public class ItemFileFields : OrganizationEntity
    {
        public ItemFileFields(
        )
        {
        }
    }


    public class ItemFile : ItemFileFields
        , IIdentifiableEntityWithOriginalState<ItemFileFields>
    {
        public ItemFile(
        )
        {
        }
    }


    public class ItemSalesAccountFields : OrganizationEntity
    {
        public ItemSalesAccountFields(
        )
        {
        }
    }


    public class ItemSalesAccount : ItemSalesAccountFields
        , IIdentifiableEntityWithOriginalState<ItemSalesAccountFields>
    {
        public ItemSalesAccount(
        )
        {
        }
    }


    public class LeadSourceFields : OrganizationEntity
        , INamedEntity
    {
        public LeadSourceFields(
        )
        {
        }
    }


    public class LeadSource : LeadSourceFields
        , IIdentifiableEntityWithOriginalState<LeadSourceFields>
    {
        public LeadSource(
        )
        {
        }
    }


    public class LinkFields : OrganizationEntity
        , INamedEntity
    {
        public LinkFields(
        )
        {
        }
    }


    public class Link : LinkFields
        , IIdentifiableEntityWithOriginalState<LinkFields>
    {
        public Link(
        )
        {
        }
    }


    public class NavigationHistoryFields : OrganizationEntity
    {
        public NavigationHistoryFields(
        )
        {
        }
    }


    public class NavigationHistory : NavigationHistoryFields
        , IIdentifiableEntityWithOriginalState<NavigationHistoryFields>
    {
        public NavigationHistory(
        )
        {
        }
    }


    public class OfferFields : OrganizationEntity
        , INamedEntity
    {
        public OfferFields(
        )
        {
        }
    }


    public class Offer : OfferFields
        , IIdentifiableEntityWithOriginalState<OfferFields>
    {
        public Offer(
        )
        {
        }
    }

    public class OfferTemplateFields : OrganizationEntity
        , INamedEntity
    {
        public OfferTemplateFields(
        )
        {
        }
    }


    public class OfferTemplate : OfferTemplateFields
        , IIdentifiableEntityWithOriginalState<OfferTemplateFields>
    {
        public OfferTemplate(
        )
        {
        }
    }


    public class OfferItemFields : OrganizationEntity
        , INamedEntity
    {
        public OfferItemFields(
        )
        {
        }
    }


    public class OfferItem : OfferItemFields
        , IIdentifiableEntityWithOriginalState<OfferItemFields>
    {
        public OfferItem(
        )
        {
        }
    }


    public class OfferSubtotalFields : OrganizationEntity
        , INamedEntity
    {
        public OfferSubtotalFields(
        )
        {
        }
    }


    public class OfferSubtotal : OfferSubtotalFields
        , IIdentifiableEntityWithOriginalState<OfferSubtotalFields>
    {
        public OfferSubtotal(
        )
        {
        }
    }


    public class OfferTaskFields : OrganizationEntity
        , INamedEntity
    {
        public OfferTaskFields(
        )
        {
        }
    }


    public class OfferTask : OfferTaskFields
        , IIdentifiableEntityWithOriginalState<OfferTaskFields>
    {
        public OfferTask(
        )
        {
        }
    }


    public class OrganizationCompanyProductFields : OrganizationEntity
    {
        public OrganizationCompanyProductFields(
        )
        {
        }
    }


    public class OrganizationCompanyProduct : OrganizationCompanyProductFields
        , IIdentifiableEntityWithOriginalState<OrganizationCompanyProductFields>
    {
        public OrganizationCompanyProduct(
        )
        {
        }
    }


    public class OrganizationCompanyWorkTypeFields : OrganizationEntity
    {
        public OrganizationCompanyWorkTypeFields(
        )
        {
        }
    }


    public class OrganizationCompanyWorkType : OrganizationCompanyWorkTypeFields
        , IIdentifiableEntityWithOriginalState<OrganizationCompanyWorkTypeFields>
    {
        public OrganizationCompanyWorkType(
        )
        {
        }
    }


    public class OverTimeFields : OrganizationEntity
        , INamedEntity
    {
        public OverTimeFields(
        )
        {
        }
    }


    public class OverTime : OverTimeFields
        , IIdentifiableEntityWithOriginalState<OverTimeFields>
    {
        public OverTime(
        )
        {
        }
    }


    public class OverTimePriceFields : OrganizationEntity
    {
        public OverTimePriceFields(
        )
        {
        }
    }


    public class OverTimePrice : OverTimePriceFields
        , IIdentifiableEntityWithOriginalState<OverTimePriceFields>
    {
        public OverTimePrice(
        )
        {
        }
    }


    public class PricelistFields : OrganizationEntity
        , INamedEntity
    {
        public PricelistFields(
        )
        {
        }
    }


    public class Pricelist : PricelistFields
        , IIdentifiableEntityWithOriginalState<PricelistFields>
    {
        public Pricelist(
        )
        {
        }
    }


    public class PricelistVersionFields : OrganizationEntity
    {
        public PricelistVersionFields(
        )
        {
        }
    }


    public class PricelistVersion : PricelistVersionFields
        , IIdentifiableEntityWithOriginalState<PricelistVersionFields>
    {
        public PricelistVersion(
        )
        {
        }
    }


    public class ProductFields : OrganizationEntity
        , INamedEntity
    {
        public ProductFields(
        )
        {
        }
    }


    public class Product : ProductFields
        , IIdentifiableEntityWithOriginalState<ProductFields>
    {
        public Product(
        )
        {
        }
    }


    public class ProductCategoryFields : OrganizationEntity
        , INamedEntity
    {
        public ProductCategoryFields(
        )
        {
        }
    }


    public class ProductCategory : ProductCategoryFields
        , IIdentifiableEntityWithOriginalState<ProductCategoryFields>
    {
        public ProductCategory(
        )
        {
        }
    }


    public class ProductCostAccountFields : OrganizationEntity
    {
        public ProductCostAccountFields(
        )
        {
        }
    }


    public class ProductCostAccount : ProductCostAccountFields
        , IIdentifiableEntityWithOriginalState<ProductCostAccountFields>
    {
        public ProductCostAccount(
        )
        {
        }
    }


    public class ProductPriceFields : OrganizationEntity
    {
        public ProductPriceFields(
        )
        {
        }
    }


    public class ProfileFields : OrganizationEntity
        , INamedEntity
    {
        public ProfileFields(
        )
        {
        }
    }


    public class Profile : ProfileFields
        , IIdentifiableEntityWithOriginalState<ProfileFields>
    {
        public Profile(
        )
        {
        }
    }


    public class ProfileRightFields : OrganizationEntity
    {
        public ProfileRightFields(
        )
        {
        }
    }


    public class ProfileRight : ProfileRightFields
        , IIdentifiableEntityWithOriginalState<ProfileRightFields>
    {
        public ProfileRight(
        )
        {
        }
    }


    public class ProfileSSOMappingFields : OrganizationEntity
    {
        public ProfileSSOMappingFields(
        )
        {
        }
    }


    public class ProfileSSOMapping : ProfileSSOMappingFields
        , IIdentifiableEntityWithOriginalState<ProfileSSOMappingFields>
    {
        public ProfileSSOMapping(
        )
        {
        }
    }


    public class ProposalStatusFields : OrganizationEntity
        , INamedEntity
    {
        public ProposalStatusFields(
        )
        {
        }
    }


    public class ProposalStatus : ProposalStatusFields
        , IIdentifiableEntityWithOriginalState<ProposalStatusFields>
    {
        public ProposalStatus(
        )
        {
        }
    }


    public class RecurringItemFields : OrganizationEntity
        , INamedEntity
    {
        public RecurringItemFields(
        )
        {
        }
    }


    public class RecurringItem : RecurringItemFields
        , IIdentifiableEntityWithOriginalState<RecurringItemFields>
    {
        public RecurringItem(
        )
        {
        }
    }


    public class ReimbursedHourFields : OrganizationEntity
    {
        public ReimbursedHourFields(
        )
        {
        }
    }


    public class ReimbursedHour : ReimbursedHourFields
        , IIdentifiableEntityWithOriginalState<ReimbursedHourFields>
    {
        public ReimbursedHour(
        )
        {
        }
    }


    public class ReimbursedItemFields : OrganizationEntity
        , INamedEntity
    {
        public ReimbursedItemFields(
        )
        {
        }
    }


    public class ReimbursedItem : ReimbursedItemFields
        , IIdentifiableEntityWithOriginalState<ReimbursedItemFields>
    {
        public ReimbursedItem(
        )
        {
        }
    }


    public class ReportFields : OrganizationEntity
    {
        public ReportFields(
        )
        {
        }
    }


    public class Report : ReportFields
        , IIdentifiableEntityWithOriginalState<ReportFields>
    {
        public Report(
        )
        {
        }
    }


    public class ResourceFields : OrganizationEntity
        , INamedEntity
    {
        public ResourceFields(
        )
        {
        }
    }


    public class Resource : ResourceFields
        , IIdentifiableEntityWithOriginalState<ResourceFields>
    {
        public Resource(
        )
        {
        }
    }


    public class ResourceAllocationFields : OrganizationEntity
    {
        public ResourceAllocationFields(
        )
        {
        }
    }


    public class ResourceAllocation : ResourceAllocationFields
        , IIdentifiableEntityWithOriginalState<ResourceAllocationFields>
    {
        public ResourceAllocation(
        )
        {
        }
    }


    public class SalesAccountFields : OrganizationEntity
        , INamedEntity
    {
        public SalesAccountFields(
        )
        {
        }
    }


    public class SalesAccount : SalesAccountFields
        , IIdentifiableEntityWithOriginalState<SalesAccountFields>
    {
        public SalesAccount(
        )
        {
        }
    }


    public class SalesProcessFields : OrganizationEntity
        , INamedEntity
    {
        public SalesProcessFields(
        )
        {
        }
    }


    public class SalesProcess : SalesProcessFields
        , IIdentifiableEntityWithOriginalState<SalesProcessFields>
    {
        public SalesProcess(
        )
        {
        }
    }


    public class SalesStatusFields : OrganizationEntity
    {
        public SalesStatusFields(
        )
        {
        }
    }


    public class SalesStatus : SalesStatusFields
        , IIdentifiableEntityWithOriginalState<SalesStatusFields>
    {
        public SalesStatus(
        )
        {
        }
    }


    public class SearchFields : OrganizationEntity
        , INamedEntity
    {
        public SearchFields(
        )
        {
        }
    }


    public class Search : SearchFields
        , IIdentifiableEntityWithOriginalState<SearchFields>
    {
        public Search(
        )
        {
        }
    }


    public class SearchCriteriaFields : OrganizationEntity
    {
        public SearchCriteriaFields(
        )
        {
        }
    }


    public class SearchCriteria : SearchCriteriaFields
        , IIdentifiableEntityWithOriginalState<SearchCriteriaFields>
    {
        public SearchCriteria(
        )
        {
        }
    }


    public class TagFields : OrganizationEntity
    {
        public TagFields(
        )
        {
        }
    }


    public class Tag : TagFields
        , IIdentifiableEntityWithOriginalState<TagFields>
    {
        public Tag(
        )
        {
        }
    }


    public class TaskFields : OrganizationEntity
        , INamedEntity
    {
        public TaskFields(
        )
        {
        }
    }


    public class Task : TaskFields
        , IIdentifiableEntityWithOriginalState<TaskFields>
    {
        public Task(
        )
        {
        }
    }


    public class TaskMemberFields : OrganizationEntity
    {
        public TaskMemberFields(
        )
        {
        }
    }


    public class TaskMember : TaskMemberFields
        , IIdentifiableEntityWithOriginalState<TaskMemberFields>
    {
        public TaskMember(
        )
        {
        }
    }


    public class TaskStatusFields : OrganizationEntity
    {
        public TaskStatusFields(
        )
        {
        }
    }


    public class TaskStatus : TaskStatusFields
        , IIdentifiableEntityWithOriginalState<TaskStatusFields>
    {
        public TaskStatus(
        )
        {
        }
    }


    public class TaskStatusCommentFields : OrganizationEntity
    {
        public TaskStatusCommentFields(
        )
        {
        }
    }


    public class TaskStatusComment : TaskStatusCommentFields
        , IIdentifiableEntityWithOriginalState<TaskStatusCommentFields>
    {
        public TaskStatusComment(
        )
        {
        }
    }


    public class TaskStatusTypeFields : OrganizationEntity
        , INamedEntity
    {
        public TaskStatusTypeFields(
        )
        {
        }
    }


    public class TaskStatusType : TaskStatusTypeFields
        , IIdentifiableEntityWithOriginalState<TaskStatusTypeFields>
    {
        public TaskStatusType(
        )
        {
        }
    }


    public class TemporaryHourFields : OrganizationEntity
        , IHour
    {
        public TemporaryHourFields(
        )
        {
        }
    }


    public class TemporaryHour : TemporaryHourFields
        , IIdentifiableEntityWithOriginalState<TemporaryHourFields>
    {
        public TemporaryHour(
        )
        {
        }
    }


    public class TemporaryItemFields : OrganizationEntity
    {
        public TemporaryItemFields(
        )
        {
        }
    }


    public class TemporaryItem : TemporaryItemFields
        , IIdentifiableEntityWithOriginalState<TemporaryItemFields>
    {
        public TemporaryItem(
        )
        {
        }
    }


    public class TimecardEventFields : OrganizationEntity
    {
        public TimecardEventFields(
        )
        {
        }
    }


    public class TimecardEvent : TimecardEventFields
        , IIdentifiableEntityWithOriginalState<TimecardEventFields>
    {
        public TimecardEvent(
        )
        {
        }
    }


    public class TimeEntryFields : OrganizationEntity
    {
        public TimeEntryFields(
        )
        {
        }
    }


    public class TimeEntry : TimeEntryFields
        , IIdentifiableEntityWithOriginalState<TimeEntryFields>
    {
        public TimeEntry(
        )
        {
        }
    }


    public class TimeEntryTypeFields : OrganizationEntity
        , INamedEntity
    {
        public TimeEntryTypeFields(
        )
        {
        }
    }


    public class TimeEntryType : TimeEntryTypeFields
        , IIdentifiableEntityWithOriginalState<TimeEntryTypeFields>
    {
        public TimeEntryType(
        )
        {
        }
    }


    public class TravelReimbursementFields : OrganizationEntity
    {
        public TravelReimbursementFields(
        )
        {
        }
    }


    public class TravelReimbursement : TravelReimbursementFields
        , IIdentifiableEntityWithOriginalState<TravelReimbursementFields>
    {
        public TravelReimbursement(
        )
        {
        }
    }


    public class TravelReimbursementStatusFields : OrganizationEntity
        , INamedEntity
    {
        public TravelReimbursementStatusFields(
        )
        {
        }
    }


    public class TravelReimbursementStatus : TravelReimbursementStatusFields
        , IIdentifiableEntityWithOriginalState<TravelReimbursementStatusFields>
    {
        public TravelReimbursementStatus(
        )
        {
        }
    }


    public class UsedScannerReceiptFields : OrganizationEntity
    {
        public UsedScannerReceiptFields(
        )
        {
        }
    }


    public class UsedScannerReceipt : UsedScannerReceiptFields
        , IIdentifiableEntityWithOriginalState<UsedScannerReceiptFields>
    {
        public UsedScannerReceipt(
        )
        {
        }
    }


    public class UserCostPerCaseFields : OrganizationEntity
    {
        public UserCostPerCaseFields(
        )
        {
        }
    }


    public class UserCostPerCase : UserCostPerCaseFields
        , IIdentifiableEntityWithOriginalState<UserCostPerCaseFields>
    {
        public UserCostPerCase(
        )
        {
        }
    }


    public class UserTagFields : OrganizationEntity
    {
        public UserTagFields(
        )
        {
        }
    }


    public class UserTag : UserTagFields
        , IIdentifiableEntityWithOriginalState<UserTagFields>, ITagEntity, ITagEntity<User>
    {
        public UserTag(
        )
        {
        }
    }


    public class UserTaskFavoriteFields : OrganizationEntity
    {
        public UserTaskFavoriteFields(
        )
        {
        }
    }


    public class UserTaskFavorite : UserTaskFavoriteFields
        , IIdentifiableEntityWithOriginalState<UserTaskFavoriteFields>
    {
        public UserTaskFavorite(
        )
        {
        }
    }


    public class UserWeeklyViewRowFields : OrganizationEntity
    {
        public UserWeeklyViewRowFields(
        )
        {
        }
    }


    public class UserWeeklyViewRow : UserWeeklyViewRowFields
        , IIdentifiableEntityWithOriginalState<UserWeeklyViewRowFields>
    {
        public UserWeeklyViewRow(
        )
        {
        }
    }


    public class UserSettingsFields : OrganizationEntity
    {
        public UserSettingsFields(
        )
        {
        }
    }


    public class UserSettings : UserSettingsFields
        , IIdentifiableEntityWithOriginalState<UserSettingsFields>
    {
        public UserSettings(
        )
        {
        }
    }


    public class WorkdayFields : OrganizationEntity
    {
        public WorkdayFields(
        )
        {
        }
    }


    public class Workday : WorkdayFields
        , IIdentifiableEntityWithOriginalState<WorkdayFields>
    {
        public Workday(
        )
        {
        }
    }


    public class WorkingDayExceptionFields : IdentifiableEntity
    {
        public WorkingDayExceptionFields(
        )
        {
        }
    }


    public class WorkingDayException : WorkingDayExceptionFields
        , IIdentifiableEntityWithOriginalState<WorkingDayExceptionFields>, IOrganizationEntity
    {
        public WorkingDayException(
        )
        {
        }
    }


    public class WorkPriceFields : OrganizationEntity
    {
        public WorkPriceFields(
        )
        {
        }
    }


    public class WorkPrice : WorkPriceFields
        , IIdentifiableEntityWithOriginalState<WorkPriceFields>
    {
        public WorkPrice(
        )
        {
        }
    }


    public class WorkTypeFields : OrganizationEntity
        , INamedEntity
    {
        public WorkTypeFields(
        )
        {
        }
    }


    public class WorkType : WorkTypeFields
        , IIdentifiableEntityWithOriginalState<WorkTypeFields>
    {
        public WorkType(
        )
        {
        }
    }


    public class WorkweekFields : IdentifiableEntity
    {
        public WorkweekFields(
        )
        {
        }
    }


    public class Workweek : WorkweekFields
        , IIdentifiableEntityWithOriginalState<WorkweekFields>
    {
        public Workweek(
        )
        {
        }
    }


    public class FileDataFields : OrganizationEntity
    {
        public FileDataFields(
        )
        {
        }
    }


    public class FileData : FileDataFields
        , IIdentifiableEntityWithOriginalState<FileDataFields>
    {
        public FileData(
        )
        {
        }
    }


    public class ApiLogEntryFields : IdentifiableEntity
    {
        public ApiLogEntryFields(
        )
        {
        }
    }


    public class ApiLogEntry : ApiLogEntryFields
        , IIdentifiableEntityWithOriginalState<ApiLogEntryFields>
    {
        public ApiLogEntry(
        )
        {
        }
    }


    public class RequestLogEntryFields : IdentifiableEntity
    {
        public RequestLogEntryFields(
        )
        {
        }
    }


    public class RequestLogEntry : RequestLogEntryFields
        , IIdentifiableEntityWithOriginalState<RequestLogEntryFields>
    {
        public RequestLogEntry(
        )
        {
        }
    }


    public class UserLogEntryFields : IdentifiableEntity
    {
        public UserLogEntryFields(
        )
        {
        }
    }


    public class UserLogEntry : UserLogEntryFields
        , IIdentifiableEntityWithOriginalState<UserLogEntryFields>
    {
        public UserLogEntry(
        )
        {
        }
    }


    public class WorkHourApproveStatus
    {
    }


    public class WorkHourBillableStatus
    {
    }


    public class HourEx : Hour
        , ITaskNameable
    {
        public HourEx(
        )
        {
        }
    }


    public class HourForInvoice : Hour
    {
        public HourForInvoice(
        )
        {
        }
    }


    public class InvoiceRowWithCategory : InvoiceRow
    {
        public InvoiceRowWithCategory(
        )
        {
        }
    }


    public class ItemForInvoice : Item
    {
        public ItemForInvoice(
        )
        {
        }
    }


    public class LogLevel
    {
    }


    public class NoteType
    {
    }


    public class OfferEx : Offer
    {
        public OfferEx(
        )
        {
        }
    }


    public class OfferFileFields : FileAttachment
    {
        public OfferFileFields(
        )
        {
        }
    }


    public class OfferFile : OfferFileFields
        , IIdentifiableEntityWithOriginalState<OfferFileFields>
    {
        public OfferFile(
        )
        {
        }
    }


    public class OfferRow
    {
        public OfferRow(
        )
        {
        }
    }


    public class OfferRowWithVat : OfferRow
        , ITaxTotalPart
    {
        public OfferRowWithVat(
            OfferRow arg0,
            decimal arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly OfferRow field0;
        public readonly decimal field1;
    }


    public class OfferTaskEx : OfferTask
    {
        public OfferTaskEx(
        )
        {
        }
    }


    public class PricelistUsage : Pricelist
    {
        public PricelistUsage(
        )
        {
        }
    }


    public class ProductForCase : ProductWithCountryInfo
    {
        public ProductForCase(
        )
        {
        }
    }


    public class ProductPriceEx : ProductPrice
    {
        public ProductPriceEx(ProductPrice arg0, string arg1) : base(arg0, arg1)
        {
        }
    }


    public class TermsOfServiceApproval : TermsOfServiceApprovalFields, IIdentifiableEntityWithOriginalState<TermsOfServiceApprovalFields>
    {

    }

    public class ProfileReportFields : OrganizationEntity
    {
        public ProfileReportFields(
        )
        {
        }
    }


    public class ProfileReport : ProfileReportFields
        , IIdentifiableEntityWithOriginalState<ProfileReportFields>
    {
        public ProfileReport(
        )
        {
        }
    }


    public class ProfileRightWithCodeAndContext : ProfileRight
        , IAccessRightEntity
    {
        public ProfileRightWithCodeAndContext(
        )
        {
        }

        public ProfileRightWithCodeAndContext(
            int arg0,
            int arg1,
            string arg2
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly int field0;
        public readonly int field1;
        public readonly string field2;
    }


    public class RecurringItemEx : RecurringItem
    {
        public RecurringItemEx(
        )
        {
        }
    }


    public class ReportCategory
    {
    }


    public class SalesProcessUsage : SalesProcess
    {
        public SalesProcessUsage(
        )
        {
        }
    }


    public class ScheduledWorkJobErrorInfo
        : IXmlStorable
    {
        public ScheduledWorkJobErrorInfo(
        )
        {
        }
    }


    public class ScheduledWorkTaskRunInfo
        : IXmlStorable
    {
        public ScheduledWorkTaskRunInfo(
        )
        {
        }
    }


    public class ScheduledWorkTaskSettings
        : IXmlStorable
    {
        public ScheduledWorkTaskSettings(
        )
        {
        }

        public ScheduledWorkTaskSettings(
            string arg0
        )
        {
            field0 = arg0;
        }

        public readonly string field0;
    }


    public class TagEx : Tag
    {
        public TagEx(
        )
        {
        }
    }


    public class TaskEx : Task
        , ITaskNameable
    {
        public TaskEx(
        )
        {
        }
    }


    public class TaxTotal
    {
        public TaxTotal(
        )
        {
        }
    }


    public class TeamProductivityParameters
    {
        public TeamProductivityParameters(
        )
        {
        }
    }


    public class TravelExpense : Item
    {
        public TravelExpense(
        )
        {
        }
    }


    public class Usage
    {
        public Usage(
        )
        {
        }
    }


    public class UserTagEx : UserTag
    {
        public UserTagEx(
        )
        {
        }
    }


    public class WorkDayInfo
    {
        public WorkDayInfo(
        )
        {
        }
    }


    public class WorkdaySummaryFields : OrganizationEntity
    {
        public WorkdaySummaryFields(
        )
        {
        }
    }


    public class WorkdaySummary : WorkdaySummaryFields
        , IIdentifiableEntityWithOriginalState<WorkdaySummaryFields>
    {
        public WorkdaySummary(
        )
        {
        }
    }


    public class WorkdaySummaryWithDate
    {
        public WorkdaySummaryWithDate(
        )
        {
        }
    }


    public class WorkPriceEx : WorkPrice
    {
        public WorkPriceEx(
        )
        {
        }
    }


    public class WorktypeForCase : WorkType
    {
        public WorktypeForCase(
        )
        {
        }
    }


    public class AccessRights
    {
        public AccessRights(
        )
        {
        }
    }


    public class DateTimeExtensions
    {
    }


    public class SettingsGroup
    {
        public SettingsGroup(
            Settings arg0,
            string arg1,
            bool arg2
        )
        {
            field1 = arg1;
            field2 = arg2;
        }

        public SettingsGroup(
            XmlDocument arg0,
            string arg1,
            bool arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly XmlDocument field0;
        public readonly string field1;
        public readonly bool field2;
    }


    public class Tools : Util
    {
        public Tools(
        )
        {
        }
    }


    public class Util
    {
        public Util(
        )
        {
        }
    }


    public class InvalidUserDataException : AppException
    {
        public InvalidUserDataException(
            string arg0,
            Exception arg1
        ) : base(arg0, arg1)
        {
        }

        public InvalidUserDataException(
            SerializationInfo arg0,
            StreamingContext arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class AppAddonNotActiveException : AppExceptionEx
    {
        public AppAddonNotActiveException(
            string arg0,
            AppExceptionFlags arg1
        ) : base()
        {
        }

        public AppAddonNotActiveException(
            string arg0,
            Exception arg1
        ) : base(arg0, arg1)
        {
        }

        public AppAddonNotActiveException(
            string arg0,
            Exception arg1,
            AppExceptionFlags arg2
        ) : base(arg0, arg1, arg2)
        {
        }

        public AppAddonNotActiveException(
            SerializationInfo arg0,
            StreamingContext arg1
        ) : base(arg0, arg1)
        {
        }
    }


    public class GenericSearchCriteria
    {
        public GenericSearchCriteria(
        )
        {
        }
    }


    public class AccountSearchCriteria : SearchCriteriaBase
    {
        public AccountSearchCriteria(
        )
        {
        }
    }


    public class ActivitySearchCriteria : SearchCriteriaBase
    {
        public ActivitySearchCriteria(
        )
        {
        }
    }


    public class BusinessUnitSearchCriteria : SearchCriteriaBase
    {
        public BusinessUnitSearchCriteria(
        )
        {
        }
    }


    public class CaseFileSearchCriteria : SearchCriteriaBase
    {
        public CaseFileSearchCriteria(
        )
        {
        }
    }


    public class CaseSearchCriteria : SearchCriteriaBase
    {
        public CaseSearchCriteria(
        )
        {
        }
    }


    public class ContactSearchCriteria : SearchCriteriaBase
    {
        public ContactSearchCriteria(
        )
        {
        }
    }


    public class CreateInvoicesSearchCriteria
    {
        public CreateInvoicesSearchCriteria(
        )
        {
        }
    }


    public class EmploymentSearchCriteria
        : ISearchCriteria
    {
        public EmploymentSearchCriteria(
        )
        {
        }
    }


    public class FileSearchCriteriaObsolete
    {
        public FileSearchCriteriaObsolete(
        )
        {
        }
    }


    public class HourSearchCriteria : SearchCriteriaBase
    {
        public HourSearchCriteria(
        )
        {
        }
    }


    public class InvoiceRowSearchCriteria : SearchCriteriaBase
    {
        public InvoiceRowSearchCriteria(
        )
        {
        }
    }


    public class InvoiceSearchCriteria : SearchCriteriaBase
    {
        public InvoiceSearchCriteria(
        )
        {
        }
    }


    public interface ISearchResultWithKpi
    {
    }


    public class ItemCategory
    {
    }


    public class ItemSearchCriteria : SearchCriteriaBase
    {
        public ItemSearchCriteria(
        )
        {
        }
    }


    public class OfferSearchCriteria : SearchCriteriaBase
    {
        public OfferSearchCriteria(
        )
        {
        }
    }


    public class ProductSearchCriteria
    {
        public ProductSearchCriteria(
        )
        {
        }
    }


    public class PurchaseOrderItemSearchCriteria : SearchCriteriaBase
    {
        public PurchaseOrderItemSearchCriteria(
        )
        {
        }
    }


    public class QuickSearchAccount
    {
        public QuickSearchAccount(
        )
        {
        }
    }


    public class QuickSearchCase
    {
        public QuickSearchCase(
        )
        {
        }
    }


    public class QuickSearchContact
    {
        public QuickSearchContact(
        )
        {
        }
    }


    public class QuickSearchFile
    {
        public QuickSearchFile(
        )
        {
        }
    }


    public class QuickSearchInvoice
    {
        public QuickSearchInvoice(
        )
        {
        }
    }


    public class ResourceAllocationSearchCriteria : SearchCriteriaBase
    {
        public ResourceAllocationSearchCriteria(
        )
        {
        }
    }


    public class ResourceSearchCriteria
    {
        public ResourceSearchCriteria(
        )
        {
        }
    }


    public class RevenueRecognitionSearchCriteria
    {
        public RevenueRecognitionSearchCriteria(
        )
        {
        }
    }


    public class TaskSearchCriteria : SearchCriteriaBase
    {
        public TaskSearchCriteria(
        )
        {
        }
    }


    public class TimeEntrySearchCriteria : SearchCriteriaBase
    {
        public TimeEntrySearchCriteria(
        )
        {
        }
    }


    public class TimelineSearchCriteria
    {
        public TimelineSearchCriteria(
        )
        {
        }

        public TimelineSearchCriteria(
            TimelineSearchCriteria arg0
        )
        {
            field0 = arg0;
        }

        public readonly TimelineSearchCriteria field0;
    }


    public class TravelReimbursementSearchCriteria : SearchCriteriaBase
    {
        public TravelReimbursementSearchCriteria(
        )
        {
        }
    }


    public class UserSearchCriteria : SearchCriteriaBase
    {
        public UserSearchCriteria(
        )
        {
        }
    }


    public class Cases
        : ICustomFormulaPartParameter
    {
        public Cases(
        )
        {
        }

        public Cases(
            bool? arg0
        )
        {
            field0 = arg0;
        }

        public readonly bool? field0;
    }


    public class IsProspect
        : ICustomFormulaNumericPartParameter
    {
        public IsProspect(
        )
        {
        }

        public IsProspect(
            bool? arg0
        )
        {
            field0 = arg0;
        }

        public readonly bool? field0;
    }


    public class CaseCount
        : ICustomFormulaNumericPartParameter
    {
        public CaseCount(
        )
        {
        }
    }


    public class AccountGroups
        : ICustomFormulaPartParameter
    {
        public AccountGroups(
        )
        {
        }

        public AccountGroups(
            int arg0
        )
        {
            field0 = arg0;
        }

        public readonly int field0;
    }


    public class BillingForecast
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public BillingForecast(
        )
        {
        }

        public BillingForecast(
            TimePeriod arg0,
            int? arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0;
        public readonly int? field1;
    }


    public class CaseTags
        : ICustomFormulaPartParameter
    {
        public CaseTags(
        )
        {
        }
    }


    public class ContactEmail
        : ICustomFormulaPartParameter
    {
        public ContactEmail(
        )
        {
        }
    }


    public class ContactPhoneNumber
        : ICustomFormulaPartParameter
    {
        public ContactPhoneNumber(
        )
        {
        }
    }


    public partial class Cost
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public readonly InvoicingSegment field0a;
        public readonly IncomeSource field1a;
        public readonly TimePeriod field2a;
        public readonly int? field3a;
    }


    public class ExpectedValue
        : ICustomFormulaNumericPartParameter
    {
        public ExpectedValue(
        )
        {
        }

        public ExpectedValue(
            int? arg0
        )
        {
            field0 = arg0;
        }

        public readonly int? field0;
    }


    public class ExpenseForecast
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public ExpenseForecast(
        )
        {
        }

        public ExpenseForecast(
            TimePeriod arg0,
            int? arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0;
        public readonly int? field1;
    }


    public partial class Invoicing
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public readonly InvoicingSegment field0a;
        public readonly IncomeSource field1a;
        public readonly TimePeriod field2a;
        public readonly int? field3a;
    }


    public class LaborExpenseForecast
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public LaborExpenseForecast(
        )
        {
        }

        public LaborExpenseForecast(
            TimePeriod arg0,
            int? arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0;
        public readonly int? field1;
    }


    public partial class PriceOfWorkHoursApproved
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public readonly TimePeriod field0a;
        public readonly int? field1a;
    }


    public partial class PriceOfWorkHoursBilled
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public PriceOfWorkHoursBilled(
            TimePeriod arg0,
            int? arg1,
            bool arg2
        )
        {
            field0a = arg0;
            field1a = arg1;
            field2a = arg2;
        }

        public readonly TimePeriod field0a;
        public readonly int? field1a;
        public readonly bool field2a;
    }


    public partial class PriceOfWorkHoursNotReviewed
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public readonly TimePeriod field0a;
        public readonly int? field1a;
    }


    public partial class ProductQuantity
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public ProductQuantity(
            ICollection<Int32> arg0,
            IncomeSource arg1,
            TimePeriod arg2
        )
        {
            field0a = arg0;
            field1a = arg1;
            field2a = arg2;
        }

        public readonly ICollection<Int32> field0a;
        public readonly IncomeSource field1a;
        public readonly TimePeriod field2a;
    }


    public class RevenueForecast
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public RevenueForecast(
        )
        {
        }

        public RevenueForecast(
            TimePeriod arg0,
            int? arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0;
        public readonly int? field1;
    }


    public class SalesCaseCount
        : ICustomFormulaNumericPartParameter
    {
        public SalesCaseCount(
            string arg0
        )
        {
            field0 = arg0;
        }

        public readonly string field0;
    }


    public class SalesMargin
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public SalesMargin(
        )
        {
        }

        public SalesMargin(
            IncomeSource arg0,
            TimePeriod arg1,
            int? arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IncomeSource field0;
        public readonly TimePeriod field1;
        public readonly int? field2;
    }


    public partial class SalesProgressExistence
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public SalesProgressExistence(
            string arg0
        )
        {
        }

        public SalesProgressExistence(
            TimePeriod arg0,
            string arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0;
        public readonly string field1;
    }


    public class SalesValue
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public SalesValue(
            string arg0
        )
        {
        }

        public SalesValue(
            TimePeriod arg0,
            string arg1,
            int? arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly TimePeriod field0;
        public readonly string field1;
        public readonly int? field2;
    }


    public class SalesValueForecast
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public SalesValueForecast(
            string arg0,
            bool arg1
        )
        {
        }

        public SalesValueForecast(
            TimePeriod arg0,
            string arg1,
            bool arg2,
            int? arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly TimePeriod field0;
        public readonly string field1;
        public readonly bool field2;
        public readonly int? field3;
    }


    public class Unbilled
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public Unbilled(
        )
        {
        }

        public Unbilled(
            InvoicingSegment arg0,
            bool? arg1,
            IncomeSource arg2,
            TimePeriod arg3,
            bool? arg4,
            int? arg5,
            bool? arg6
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

        public readonly InvoicingSegment field0;
        public readonly bool? field1;
        public readonly IncomeSource field2;
        public readonly TimePeriod field3;
        public readonly bool? field4;
        public readonly int? field5;
        public readonly bool? field6;
    }


    public partial class WorkHours
    {
        public WorkHours(
        )
        {
        }

        public WorkHours(
            BilledStatus arg0,
            bool? arg1,
            ICollection<Int32> arg2,
            ICollection<Int32> arg3,
            TimePeriod arg4,
            bool? arg5,
            bool? arg6
        )
        {
        }
    }


    public partial class WorkHoursBilled
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public WorkHoursBilled(
        )
        {
        }

        public WorkHoursBilled(
            TimePeriod arg0,
            bool arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0;
        public readonly bool field1;
    }


    public partial class WorkHoursValue
    {
        public WorkHoursValue(
        )
        {
        }

        public WorkHoursValue(
            BilledStatus arg0,
            ICollection<Int32> arg1,
            ICollection<Int32> arg2,
            TimePeriod arg3,
            bool? arg4,
            bool? arg5,
            int? arg6
        ) : base()
        {
            field6 = arg6;
        }

        public readonly int? field6;
    }


    public class CommunicationMethods
        : ICustomFormulaPartParameter
    {
        public CommunicationMethods(
        )
        {
        }

        public CommunicationMethods(
            int arg0
        )
        {
            field0 = arg0;
        }

        public readonly int field0;
    }


    public partial class ShareOfBillingByQuantityOfHours : CaseParameterBase
    {
        public ShareOfBillingByQuantityOfHours(
        )
        {
        }

        public ShareOfBillingByQuantityOfHours(
            InvoicingSegment arg0,
            int? arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly InvoicingSegment field0;
        public readonly int? field1;
    }


    public partial class ShareOfBillingByValue : CaseParameterBase
    {
        public ShareOfBillingByValue(
        )
        {
        }

        public ShareOfBillingByValue(
            InvoicingSegment arg0,
            int? arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly InvoicingSegment field0;
        public readonly int? field1;
    }

    public class ShareOfSalesMargin : CaseParameterBase
    {
        public ShareOfSalesMargin(
            bool arg0,
            int? arg1
        ) : base()
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly bool field0;
        public readonly int? field1;
    }


    public class WorkHoursBillable : WorkHoursBase
    {
        public WorkHoursBillable(
        )
        {
        }
    }


    public partial class Margin
        : ICustomFormulaNumericPartParameter
    {
        public Margin(
        )
        {
        }

        public Margin(
            InvoicingSegment arg0,
            InvoicingSegment arg1,
            IncomeSource arg2,
            int? arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly InvoicingSegment field0;
        public readonly InvoicingSegment field1;
        public readonly IncomeSource field2;
        public readonly int? field3;
    }


    public partial class ProductQuantity
        : ICustomFormulaNumericPartParameter
    {
        public ProductQuantity(
        )
        {
        }

        public ProductQuantity(
            ICollection<Int32> arg0,
            IncomeSource arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly ICollection<Int32> field0;
        public readonly IncomeSource field1;
    }


    public partial class Revenue
        : ICustomFormulaNumericPartParameter
    {
        public Revenue(
        )
        {
        }

        public Revenue(
            IncomeSource arg0,
            TimePeriod arg1,
            int? arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly IncomeSource field0;
        public readonly TimePeriod field1;
        public readonly int? field2;
    }


    public class ItemKpiBase
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public ItemKpiBase(
        )
        {
        }

        public ItemKpiBase(
            ICollection<Int32> arg0,
            IncomeSource arg1,
            TimePeriod arg2,
            ICollection<Int32> arg3,
            ExpenseClass arg4,
            bool? arg5,
            bool? arg6,
            bool? arg7,
            int? arg8
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

        public ItemKpiBase(ICollection<int> arg0, IncomeSource arg1, TimePeriod arg2, ICollection<int> arg3,
            ExpenseClass arg4, bool? arg5, bool? arg6, bool? arg7)
        {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
            this.arg4 = arg4;
            this.arg5 = arg5;
            this.arg6 = arg6;
            this.arg7 = arg7;
        }

        public readonly ICollection<Int32> field0;
        public readonly IncomeSource field1;
        public readonly TimePeriod field2;
        public readonly ICollection<Int32> field3;
        public readonly ExpenseClass field4;
        public readonly bool? field5;
        public readonly bool? field6;
        public readonly bool? field7;
        public readonly int? field8;
        private ICollection<int> arg0;
        private IncomeSource arg1;
        private TimePeriod arg2;
        private ICollection<int> arg3;
        private ExpenseClass arg4;
        private bool? arg5;
        private bool? arg6;
        private bool? arg7;
    }


    public class TotalExcludingVat
        : ICustomFormulaNumericPartParameter
    {
        public TotalExcludingVat(
        )
        {
        }

        public TotalExcludingVat(
            bool arg0,
            bool arg1,
            int? arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly bool field0;
        public readonly bool field1;
        public readonly int? field2;
    }


    public class TotalItemCost
        : ICustomFormulaNumericPartParameter
    {
        public TotalItemCost(
        )
        {
        }

        public TotalItemCost(
            int? arg0
        )
        {
            field0 = arg0;
        }

        public readonly int? field0;
    }


    public class TotalTaskCost
        : ICustomFormulaNumericPartParameter
    {
        public TotalTaskCost(
        )
        {
        }

        public TotalTaskCost(
            int? arg0
        )
        {
            field0 = arg0;
        }

        public readonly int? field0;
    }


    public class TotalVat
        : ICustomFormulaNumericPartParameter
    {
        public TotalVat(
            bool arg0,
            bool arg1,
            int? arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly bool field0;
        public readonly bool field1;
        public readonly int? field2;
    }


    public class ActivityCount
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public ActivityCount(
            TimePeriod arg0,
            bool? arg1,
            bool arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly TimePeriod field0;
        public readonly bool? field1;
        public readonly bool field2;
    }


    public partial class Cost
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public Cost(
        )
        {
        }

        public Cost(
            InvoicingSegment arg0,
            IncomeSource arg1,
            TimePeriod arg2,
            int? arg3
        )
        {
            field3 = arg3;
        }

        public readonly int? field3;
    }


    public partial class PriceOfWorkHoursApproved
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public PriceOfWorkHoursApproved(
            TimePeriod arg0,
            int? arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0;
        public readonly int? field1;
    }


    public partial class PriceOfWorkHoursBilled
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public PriceOfWorkHoursBilled(
            TimePeriod arg0,
            int? arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0;
        public readonly int? field1;
    }


    public partial class PriceOfWorkHoursNotReviewed
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public PriceOfWorkHoursNotReviewed(
        )
        {
        }

        public PriceOfWorkHoursNotReviewed(
            TimePeriod arg0,
            int? arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0;
        public readonly int? field1;
    }


    public class WorkEstimate
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public WorkEstimate(
        )
        {
        }

        public WorkEstimate(
            TimePeriod arg0,
            bool arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0;
        public readonly bool field1;
    }


    public partial class WorkHours
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public WorkHours(
            bool? arg0,
            bool? arg1,
            ICollection<Int32> arg2,
            TimePeriod arg3,
            bool? arg4,
            bool? arg5,
            bool arg6
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

        public readonly bool? field0;
        public readonly bool? field1;
        public readonly ICollection<Int32> field2;
        public readonly TimePeriod field3;
        public readonly bool? field4;
        public readonly bool? field5;
        public readonly bool field6;
    }

    public class AccountAmount
        : ICustomFormulaNumericPartParameter, IParameterWithTimePeriod
    {
        public AccountAmount(
        )
        {
        }

        public AccountAmount(
            TimePeriod arg0,
            bool? arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0;
        public readonly bool? field1;
    }


    public class ActivityParameterBase
        : ICustomFormulaNumericPartParameter, IParameterWithTimePeriod
    {
        public ActivityParameterBase(
        )
        {
        }

        public ActivityParameterBase(
            TimePeriod arg0,
            ActivityRelationToUser arg1,
            bool? arg2,
            ICollection<Int32> arg3
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
        }

        public readonly TimePeriod field0;
        public readonly ActivityRelationToUser field1;
        public readonly bool? field2;
        public readonly ICollection<Int32> field3;
    }


    public class CaseAmount : CaseParameterBase
    {
        public CaseAmount(
            TimePeriod arg0,
            CaseRelationToUser arg1,
            bool? arg2,
            ICollection<Int32> arg3,
            ICollection<Int32> arg4
        ) : base(arg0, arg1, arg2, arg3, arg4)
        {
        }
    }


    public class CaseParameterBase
        : ICustomFormulaNumericPartParameter, IParameterWithTimePeriod
    {
        public CaseParameterBase()
        {
        }

        public CaseParameterBase(
            TimePeriod arg0,
            CaseRelationToUser arg1,
            bool? arg2,
            ICollection<Int32> arg3,
            ICollection<Int32> arg4
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
            field3 = arg3;
            field4 = arg4;
        }

        public CaseParameterBase(TimePeriod arg0, bool arg1, int? arg2)
        {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
        }

        public CaseParameterBase(TimePeriod arg0, InvoicingSegment arg11, int? arg2)
        {
            this.arg0 = arg0;
            this.arg11 = arg11;
            this.arg2 = arg2;
        }

        public readonly TimePeriod field0;
        public readonly CaseRelationToUser field1;
        public readonly bool? field2;
        public readonly ICollection<Int32> field3;
        public readonly ICollection<Int32> field4;
        private TimePeriod arg0;
        private bool arg1;
        private int? arg2;
        private InvoicingSegment arg11;
    }


    public partial class Cost
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public Cost(
            InvoicingSegment arg0,
            TimePeriod arg1,
            int? arg2
        )
        {
            field0 = arg0;
            field1 = arg1;
            field2 = arg2;
        }

        public readonly InvoicingSegment field0;
        public readonly TimePeriod field1;
        public readonly int? field2;
    }

    public class EmploymentTotalHours
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public EmploymentTotalHours(
        )
        {
        }

        public EmploymentTotalHours(
            TimePeriod arg0
        )
        {
            field0 = arg0;
        }

        public readonly TimePeriod field0;
    }


    public class Flextime
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public Flextime(
        )
        {
        }

        public Flextime(
            TimePeriod arg0
        )
        {
            field0 = arg0;
        }

        public readonly TimePeriod field0;
    }


    public partial class Invoicing
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public Invoicing(
            TimePeriod arg0,
            int? arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0;
        public readonly int? field1;
    }


    public class ItemInvoicing : ItemKpiBase
    {
        public ItemInvoicing(
            ICollection<Int32> arg0,
            IncomeSource arg1,
            TimePeriod arg2,
            ICollection<Int32> arg3,
            ExpenseClass arg4,
            int? arg5,
            bool arg6
        ) : base()
        {
        }
    }


    public class ItemPrice : ItemKpiBase
    {
        public ItemPrice(
            ICollection<Int32> arg0,
            IncomeSource arg1,
            TimePeriod arg2,
            ICollection<Int32> arg3,
            ExpenseClass arg4,
            bool? arg5,
            bool? arg6,
            bool? arg7,
            int? arg8
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8)
        {
        }
    }


    public class LastLogin
        : ICustomFormulaPartParameter
    {
    }


    public class OutOfOfficeHours
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public OutOfOfficeHours(
        )
        {
        }

        public OutOfOfficeHours(
            TimePeriod arg0,
            ICollection<Int32> arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0;
        public readonly ICollection<Int32> field1;
    }


    public partial class ProductQuantity : ItemKpiBase
    {
        public ProductQuantity(
            ICollection<Int32> arg0,
            IncomeSource arg1,
            TimePeriod arg2,
            ICollection<Int32> arg3,
            bool? arg4,
            bool? arg5,
            bool? arg6
        ) : base()
        {
        }
    }


    public class ShareOfBilling : CaseParameterBase
    {
        public ShareOfBilling(
            TimePeriod arg0,
            bool arg1,
            int? arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class ShareOfBillingByCost : CaseParameterBase
    {
        public ShareOfBillingByCost(
            TimePeriod arg0,
            InvoicingSegment arg1,
            int? arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public partial class ShareOfBillingByQuantityOfHours : CaseParameterBase
    {
        public ShareOfBillingByQuantityOfHours(
            TimePeriod arg0,
            InvoicingSegment arg1,
            int? arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public partial class ShareOfBillingByValue : CaseParameterBase
    {
        public ShareOfBillingByValue(
            TimePeriod arg0,
            InvoicingSegment arg1,
            int? arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class ShareOfBillingPercentage : CaseParameterBase
    {
    }


    public class ShareOfBillingPerHour : CaseParameterBase
    {
        public ShareOfBillingPerHour(
            TimePeriod arg0,
            bool arg1,
            int? arg2
        ) : base(arg0, arg1, arg2)
        {
        }
    }


    public class TravelExpenseCost : ItemKpiBase
    {
        public TravelExpenseCost(
        )
        {
        }

        public TravelExpenseCost(
            ICollection<Int32> arg0,
            IncomeSource arg1,
            TimePeriod arg2,
            ICollection<Int32> arg3,
            ExpenseClass arg4,
            bool? arg5,
            bool? arg6,
            bool? arg7,
            int? arg8
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8)
        {
        }
    }


    public class TravelExpenseInvoicing : ItemInvoicing
    {
        public TravelExpenseInvoicing(
            ICollection<Int32> arg0,
            IncomeSource arg1,
            TimePeriod arg2,
            ICollection<Int32> arg3,
            ExpenseClass arg4,
            int? arg5,
            bool arg6
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6)
        {
        }
    }


    public class TravelExpensePrice : ItemPrice
    {
        public TravelExpensePrice(
            ICollection<Int32> arg0,
            IncomeSource arg1,
            TimePeriod arg2,
            ICollection<Int32> arg3,
            ExpenseClass arg4,
            bool? arg5,
            bool? arg6,
            bool? arg7,
            int? arg8
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8)
        {
        }
    }


    public class TravelExpenseQuantity : ItemKpiBase
    {
        public TravelExpenseQuantity(
            ICollection<Int32> arg0,
            IncomeSource arg1,
            TimePeriod arg2,
            ICollection<Int32> arg3,
            ExpenseClass arg4,
            bool? arg5,
            bool? arg6,
            bool? arg7
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7)
        {
        }
    }


    public class UserTags
        : ICustomFormulaPartParameter
    {
        public UserTags(
        )
        {
        }
    }


    public class ValuePerHour
        : IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public ValuePerHour(
            TimePeriod arg0,
            int? arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly TimePeriod field0;
        public readonly int? field1;
    }


    public class CaseRelationToUser
    {
    }


    public class ActivityRelationToUser
    {
    }


    public partial class WorkHours
    {
        public WorkHours(
            BilledStatus arg0,
            bool? arg1,
            ICollection<Int32> arg2,
            ICollection<Int32> arg3,
            TimePeriod arg4,
            bool? arg5,
            bool? arg6,
            bool? arg7,
            EntryType arg8,
            bool? arg9,
            bool arg10
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, null, null)
        {
            field10 = arg10;
        }

        public readonly bool field10;
    }


    public class GroupByProperty
    {
    }


    public partial class Criteria
    {
        public Criteria(
        )
        {
        }
    }


    public partial class WorkHours : WorkHoursBase
    {
        public WorkHours(
            BilledStatus arg0,
            bool? arg1,
            ICollection<Int32> arg2,
            ICollection<Int32> arg3,
            TimePeriod arg4,
            bool? arg5,
            bool? arg6,
            bool? arg7,
            bool? arg8,
            EntryType arg9
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9)
        {
        }
    }


    public class WorkHoursBase : GenericSearchCriteria
        , IParameterWithTimePeriod, ICustomFormulaNumericPartParameter
    {
        public WorkHoursBase(
        )
        {
        }

        public WorkHoursBase(
            BilledStatus arg0,
            bool? arg1,
            ICollection<Int32> arg2,
            ICollection<Int32> arg3,
            TimePeriod arg4,
            bool? arg5,
            bool? arg6,
            bool? arg7,
            bool? arg8,
            EntryType arg9
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

        public WorkHoursBase(ICollection<int> arg0, ICollection<int> arg1, TimePeriod arg2, bool? arg3)
        {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.arg3 = arg3;
        }

        public WorkHoursBase(BilledStatus arg01, bool? arg11, ICollection<int> arg21, ICollection<int> arg31,
            TimePeriod arg4, bool? arg5, bool? arg6)
        {
            this.arg01 = arg01;
            this.arg11 = arg11;
            this.arg21 = arg21;
            this.arg31 = arg31;
            this.arg4 = arg4;
            this.arg5 = arg5;
            this.arg6 = arg6;
        }

        public readonly BilledStatus field0;
        public readonly bool? field1;
        public readonly ICollection<Int32> field2;
        public readonly ICollection<Int32> field3;
        public readonly TimePeriod field4;
        public readonly bool? field5;
        public readonly bool? field6;
        public readonly bool? field7;
        public readonly bool? field8;
        public readonly EntryType field9;
        private ICollection<int> arg0;
        private ICollection<int> arg1;
        private TimePeriod arg2;
        private bool? arg3;
        private BilledStatus arg01;
        private bool? arg11;
        private ICollection<int> arg21;
        private ICollection<int> arg31;
        private TimePeriod arg4;
        private bool? arg5;
        private bool? arg6;
    }


    public partial class WorkHoursBilled : WorkHoursBase
    {
        public WorkHoursBilled(
            ICollection<Int32> arg0,
            ICollection<Int32> arg1,
            TimePeriod arg2,
            bool? arg3
        ) : base(arg0, arg1, arg2, arg3)
        {
        }
    }


    public class WorkHoursCost : WorkHoursBase
    {
        public WorkHoursCost(
        )
        {
        }

        public WorkHoursCost(
            BilledStatus arg0,
            bool? arg1,
            ICollection<Int32> arg2,
            ICollection<Int32> arg3,
            TimePeriod arg4,
            bool? arg5,
            bool? arg6
        ) : base(arg0, arg1, arg2, arg3, arg4, arg5, arg6)
        {
        }
    }


    public partial class WorkHoursInvoicing : WorkHoursBase
    {
        public WorkHoursInvoicing(
            ICollection<Int32> arg0,
            ICollection<Int32> arg1,
            TimePeriod arg2
        ) : base()
        {
        }
    }


    public partial class WorkHoursValue : WorkHoursBase
    {
        public WorkHoursValue(
            BilledStatus arg0,
            ICollection<Int32> arg1,
            ICollection<Int32> arg2,
            TimePeriod arg3,
            bool? arg4,
            bool? arg5
        ) : base()
        {
        }
    }


    public partial class WorkHoursValueWithDefaultPricelist : WorkHoursBase
    {
        public WorkHoursValueWithDefaultPricelist(
        )
        {
        }

        public WorkHoursValueWithDefaultPricelist(
            int? arg0
        ) : base()
        {
        }

        public WorkHoursValueWithDefaultPricelist(
            int? arg0,
            BilledStatus arg1,
            bool? arg2,
            ICollection<Int32> arg3,
            ICollection<Int32> arg4,
            TimePeriod arg5,
            bool? arg6,
            bool? arg7
        ) : base()
        {
        }
    }


    public class IncomeTypeEnum
    {
    }


    public class FinancialForecast
    {
        public FinancialForecast(
        )
        {
        }
    }


    public class DateAndHours
    {
        public DateAndHours(
        )
        {
        }
    }


    public class MemberStatus
    {
    }


    public class UserAccessRightLevel
    {
    }


    public class InvoiceSkin
    {
    }


    public class InvoiceGrouping
    {
    }


    public class BillingScheduleEnum
    {
    }


    public class RecurrenceEndTypeEnum
    {
    }


    public class ReportOwnership
    {
    }


    public class SalesStatusType
    {
    }


    public class PricleistType
    {
    }


    public class IncreaseType
    {
    }


    public partial class Case
        : IXmlStorable
    {
        public Case(
        )
        {
        }
    }


    public class DateSetting
    {
    }


    public partial class Criteria
    {
        public Criteria(
            string arg0,
            object arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly string field0;
        public readonly object field1;
    }

    public class EntryType
    {
    }


    public class By
    {
        public By(
        )
        {
        }
    }


    public class Period
    {
        public Period(
        )
        {
        }
    }


    public class Value
    {
        public Value(
        )
        {
        }
    }


    public class Graph
    {
        public Graph(
        )
        {
        }
    }
}