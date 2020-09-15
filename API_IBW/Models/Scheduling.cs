using API_IBW.DB_Models;
using System;
using System.Collections.Generic;
using System.Web;

namespace API_IBW.Models
{
    public class DocumentCategoryList
    {
        public List<DocumentCategory> DocumentCategories { get; set; }
    }
    public class DocumentCategory
    {
        public long DocumentId { get; set; }
        public string DocumentName { get; set; }
        public string docRandomString { get; set; }
        public string docGoogleDriveFileId { get; set; }
    }
    public class ScheduledActionDetails : ActionScheduledToUser
    {
        public decimal? PlannedHours { get; set; }
        public string AssignedByUserName { get; set; }
        public string AssignedToUserName { get; set; }

        public long? AssignedByUserId { get; set; }
        public string ActionGoogleDriveId { get; set; }
        public List<AssignUser> SplitWithUsers { get; set; }
        public string ActionNotes { get; set; }
        public long DetailId { get; set; }
        public List<DoocumentTypes> DocumentCategories { get; set; }
        public long? QuoteId { get; set; }
        public string QuoteProject { get; set; }
        public long? ProjectManagerId { get; set; }
        public string ProjectManagerName { get; set; }
        public string RecDate { get; set; }
        public string CreatedBy { get; set; }
    }
    public class ActionScheduling
    {
        public long? ManageActionId { get; set; }
        public long? TaskId { get; set; }
        public string TaskName { get; set; }
        public long? QuoteId { get; set; }
        public string QuoteNo { get; set; }
        public long? SiteId { get; set; }
        public string SiteName { get; set; }
        public string SiteStreetAddress { get; set; }
        public string SiteCity { get; set; }
        public string SiteState { get; set; }
        public string SiteCountry { get; set; }
        public long? SowId { get; set; }
        public string SowName { get; set; }
        public long? ActionId { get; set; }
        public string ActionName { get; set; }
        public long? JobCodeId { get; set; }
        public string JobCode { get; set; }
        public string JobCodeTitle { get; set; }
        public bool? JobCodeStatus { get; set; }
        public decimal? PlannedHours { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? DueDate { get; set; }
        public long? DueDateOptionId { get; set; }
        public string DueDateOption { get; set; }
        public List<ActionSchedulingDetails> DetailsList { get; set; }
        public bool? IsRemedialAction { get; set; }
        public long? TeamMemberId { get; set; }
        public long? ProjectManagerId { get; set; }
        public string ProjectManagerName { get; set; }
        public int TotalAssignedHours { get; set; }
        public decimal? TotalScheduledHours { get; set; }
        public Boolean? UnScheduled { get; set; }
        public bool? Rescheduled { get; set; }
        public string ProjectType { get; set; }
        public bool IsProject { get; set; }
        public string Notes { get; set; }
        public bool IsDueToday { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
    }

    public class ActionSchedulingDetails
    {
        public bool IsDuplicate { get; set; }
        public decimal? AssignedHours { get; set; }
        public long? AssignedUser { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public List<AssignUser> AssignUserList { get; set; }
        public string AssignedOn { get; set; }
        public string AssignedTo { get; set; }
        public string AssignedBy { get; set; }
        public long? DetailId { get; set; }
        public long? LogHoursId { get; set; }
        public string ActionCompletionStatus { get; set; }
        public bool? Approved { get; set; }
    }

    public class AssignUser
    {
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public decimal? userAssignedHours { get; set; }
        public bool? Status { get; set; }
    }
    public class PMDashboardFilter
    {
        public long? projectManagerId { get; set; }
        public long? quoteId { get; set; }
        public List<long?> staffId { get; set; }
        public long? jobCodeId { get; set; }
        public string toDate { get; set; }
        public string fromDate { get; set; }
        public string keyWord { get; set; }
    }
    public class ActionScheduledToUser
    {
        public string TimeSheetStatus { get; set; }
        public FilterForm FilterForm { get; set; }
        public long? ActionId { get; set; }
        public long? ActionDetailId { get; set; }
        public long? UserId { get; set; }
        public long? TaskId { get; set; }
        public string UserName { get; set; }
        public string TaskName { get; set; }
        public string QuoteNo { get; set; }
        public string SiteName { get; set; }
        public string SowName { get; set; }
        public string ActionName { get; set; }
        public string JobCode { get; set; }
        public string JobCodeTitle { get; set; }
        public decimal? AssignedHours { get; set; }
        public decimal? WorkedHours { get; set; }
        public decimal? RemainingHours { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public int ViewType { get; set; }
        public DateTime? AssignedOn { get; set; }
        public string ActionSchedulingStatus { get; set; }
        public string ActionDueDateOption { get; set; }
        public long? ManageActionId { get; set; }
        public decimal? PlannedHours { get; set; }
        public string DueDate { get; set; }
        public string ProjectManager { get; set; }
        public long? Sequence { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class ScheduledActionsSummary
    {
        public string UserName { get; set; }
        public decimal? MondayCount { get; set; }
        public decimal? TuesdayCount { get; set; }
        public decimal? WednesdayCount { get; set; }
        public decimal? ThrusdayCount { get; set; }
        public decimal? FridayCount { get; set; }
        public decimal? SaturdayCount { get; set; }
        public decimal? SundayCount { get; set; }
        public decimal? TotalCount { get; set; }
    }

    public class ScheduledAction
    {
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public List<ActionScheduledOnDate> ActionScheduledOnDate { get; set; }
    }

    public class ActionScheduledOnDate
    {
        public DateTime Date { get; set; }
        public decimal? TotalHours { get; set; }
        public List<ActionScheduledToUser> ActionsList { get; set; }
        public string strDate { get; set; }
    }
    public class ActionHistory
    {
        public long? ActionId { get; set; }
        public string ActionName { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string JobCode { get; set; }
        public string JobCodeTitle { get; set; }
        public string AssignedToUserName { get; set; }
        public decimal? PlannedHours { get; set; }
        public decimal? WorkedHours { get; set; }
        public decimal? RemainingHours { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public long? TaskId { get; set; }
        public decimal? AssignedHours { get; set; }
        public string Status { get; set; }
        public long? ManageActionId { get; set; }
        public long? QuoteId { get; set; }
        public long? SiteId { get; set; }
        public long? SowId { get; set; }
    }
    public class ScheduledActionDetail
    {
        public string Notes { get; set; }
        public long? ManageActionId { get; set; }
        public long? ActionDetailId { get; set; }
        public string QuoteNo { get; set; }
        public string ProjectManagerName { get; set; }
        public string SiteName { get; set; }
        public string SowName { get; set; }
        public string TaskName { get; set; }
        public string ActionName { get; set; }
        public string JobCode { get; set; }
        public string JobCodeTitle { get; set; }
        public decimal? PlannedHours { get; set; }
        public decimal? AssignedHours { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public long? AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; }
        public DateTime? AssignedOnDate { get; set; }
        public long? AssignedByUserId { get; set; }
        public string AssignedByUserName { get; set; }
        public string Status { get; set; }
        public List<AssignUser> AssignUserList { get; set; }
        public List<ActionComment> ActionComments { get; set; }
        public List<AssignUser> SplitWithUsers { get; set; }
        public bool IsProject { get; set; }
        public long? QuoteId { get; set; }
        public decimal? WorkedHours { get; set; }
        public string RecDate { get; set; }
        public string CreatedBy { get; set; }
    }

    public class ActionComment
    {
        public long? ActionCommentId { get; set; }
        public long? ActionDetailId { get; set; }
        public string Comment { get; set; }
        public long? CreatedById { get; set; }
        public string CretedByName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long? UpdatedById { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
    public class LogHoursOverall
    {
        public long? actionLogHourId { get; set; }
        public ActionOverviewForm actionForm { get; set; }
        public LogHoursForm logHoursForm { get; set; }
        public ExpensesDetails expensesForm { get; set; }
    }

    public class ActionOverviewForm
    {
        public long? detailId { get; set; }
        public long? manageActionId { get; set; }
        public long? logHoursStatus { get; set; }
        public string actionGoogleDriveId { get; set; }
        public long? userId { get; set; }


    }
    public class LogHoursForm
    {
        public bool? includeExpenses { get; set; }
        public decimal? plannedHours { get; set; }
        public decimal? workedHours { get; set; }
        public decimal? remainingHours { get; set; }
        public DateTime? completedDate { get; set; }
        public string remarks { get; set; }

    }
    public class ExpensesDetails
    {
        public List<ExpensesForm> expensesDetails { get; set; }
        public List<long?> deletedExpenses { get; set; }
    }
    public class ExpensesForm
    {
        public long? expenseActionId { get; set; }
        public long? expenseCodeId { get; set; }
        public decimal? expenseRate { get; set; }
        public DateTime? logHoursDate { get; set; }
        public string limitType { get; set; }
        public bool expenseReimbursable { get; set; }
        public long? quantity { get; set; }
        public decimal? expenseTotalAmount { get; set; }
        public string description { get; set; }
        public bool? attachmentRequired { get; set; }
        public string expenseGoogleDriveFileId { get; set; }
        public string expenseRandomString { get; set; }
    }
    public class LogHoursFiles
    {
        public HttpPostedFile UploadedFile { get; set; }
        public bool isExpense { get; set; }
        public long Id { get; set; }
        public string ActionGoogleDriveId { get; set; }
        public long? UserId { get; set; }
    }

    public class DoocumentTypes
    {
        public long? DocumentId { get; set; }
        public string DocumentName { get; set; }
    }
    public class ActionScheduleFilter
    {
        public List<GetAllQuotesResult> Quotes { get; set; }
        public List<sp_getAllIBWUsersResult> ProjectManagers { get; set; }
        public List<IBWUsers> Users { get; set; }
        public List<JobCodes> JobCodes { get; set; }
    }
    public class FilterForm
    {
        public string KeyWord { get; set; }
        //public List<long?> QuoteIds { get; set; }
        public List<long?> ProjectManagerIds { get; set; }
        public List<long?> StaffIds { get; set; }
        public List<long?> JobCodeIds { get; set; }
        public long? QuoteId { get; set; }

    }
    public class TimeSheetModal
    {
        public bool? IncludeExpenses { get; set; }
        public long? TimeSheetStatus { get; set; }
        public long? TimeSheetId { get; set; }
        public decimal? WorkedHours { get; set; }
        public decimal? RemainingHours { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Remarks { get; set; }
        public bool isAssigned { get; set; }
        public long UserId { get; set; }
        public long? ManageActionId { get; set; }
        public long? ActionDetailId { get; set; }
        public long? PlannedHours { get; set; }
        public string ActionGoogleDriveId { get; set; }

    }
    public class ScheduledActionActivityLog
    {
        public long? ManageActionId { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public long? AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; }
        public decimal? PlannedHours { get; set; }
        public decimal? AssignedHours { get; set; }
        public decimal? RemainingHours { get; set; }
        public string Status { get; set; }
        public long? CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public decimal? WorkedHours { get; set; }
        public long? DetailId { get; set; }
    }
    #region Timesheet & Expenses
    public class TimesheetExpenseNotes
    {
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
    }
    public class RefNo
    {
        public long? RefId { get; set; }
        public string RefNumber { get; set; }
        public string RefStage { get; set; }
    }

    public class Site
    {
        public long? SiteId { get; set; }
        public string SiteName { get; set; }
        public long? RefId { get; set; }
    }

    public class SOW
    {
        public long? SOWId { get; set; }
        public string SOWName { get; set; }
        public long? SiteId { get; set; }
        public string SOWStatusName { get; set; }
        public long? SOWStatusId { get; set; }
    }

    public class TimesheetOptions
    {
        public List<RefNo> RefNos { get; set; }
        public List<Site> Sites { get; set; }
        public List<SOW> Sows { get; set; }
        public List<Tasks> Tasks { get; set; }
        public List<JobCodes> JobCodes { get; set; }
        public List<ExpenseCodes> ExpenseCodes { get; set; }
    }

    public class Timesheet
    {
        public long? TimesheetId { get; set; }
        public DateTime? Date { get; set; }
        public long? RefId { get; set; }
        public string RefNo { get; set; }
        public long? SiteId { get; set; }
        public string SiteName { get; set; }
        public long? SowId { get; set; }
        public string SowName { get; set; }
        public long? TaskId { get; set; }
        public string TaskName { get; set; }
        public long? JobCodeId { get; set; }
        public string JobCode { get; set; }
        public string JobCodeTitle { get; set; }
        public decimal? WorkedHours { get; set; }
        public string Remarks { get; set; }
        public bool? IncludeExpenses { get; set; }
        public List<Expenses> ExpensesList { get; set; }
        public long? UserId { get; set; }
        public string ActionName { get; set; }
        public string Status { get; set; }
        public bool? Approved { get; set; }
        public List<long?> DeletedExpensesList { get; set; }
    }

    public class Expenses
    {
        public string strDate { get; set; }
        public decimal? Rate { get; set; }
        public long? ExpenseId { get; set; }
        public long? ExpenseCodeId { get; set; }
        public string ExpenseCode { get; set; }
        public long? TimesheetId { get; set; }
        public DateTime? Date { get; set; }
        public long? RefId { get; set; }
        public string RefNo { get; set; }
        public long? SiteId { get; set; }
        public string SiteName { get; set; }
        public long? SowId { get; set; }
        public string SowName { get; set; }
        public long? TaskId { get; set; }
        public string TaskName { get; set; }
        public long? JobCodeId { get; set; }
        public string JobCode { get; set; }
        public string JobCodeTitle { get; set; }
        public string LimitType { get; set; }
        public int? Quantity { get; set; }
        public decimal? Total { get; set; }
        public bool? IsReimbursable { get; set; }
        public HttpPostedFile googleDriveFile { get; set; }
        public string GoogleFileId { get; set; }
        public string Description { get; set; }
        public long? UserId { get; set; }
        public string Status { get; set; }
        public string ActionName { get; set; }
        public bool? Approved { get; set; }
        public bool? AttachmentRequired { get; set; }
    }

    public class Timesheets
    {
        public List<Timesheet> TimesheetList { get; set; }
        public List<Timesheet> ThisWeekTimesheetList { get; set; }
        public List<Timesheet> NextWeeKTimesheetList { get; set; }
        public List<DayHours> Today { get; set; }
        public List<DayHours> Tomorrow { get; set; }
        public List<DayHours> ThisWeek { get; set; }
        public List<DayHours> NextWeek { get; set; }
        public DateTime? ThisWeekStart { get; set; }
        public DateTime? ThisWeekEnd { get; set; }
        public DateTime? LastWeekStart { get; set; }
        public DateTime? LastWeekEnd { get; set; }
        public List<DayHours> ThisWeekProjects { get; set; }
        public List<DayHours> NextWeekProjects { get; set; }
        public List<GetLeaveRequestsResult> Leaves { get; set; }
        public List<GetHolidaysResult> Holidays { get; set; }
    }

    public class DayHours
    {
        public string Day { get; set; }
        public decimal? Hours { get; set; }
        public decimal? Leaves { get; set; }
        public decimal? Holidays { get; set; }
    }

    public class GetExpenses
    {
        public List<Expenses> ExpensesList { get; set; }
        public List<Expenses> ThisWeekExpensesList { get; set; }
        public List<Expenses> NextWeekExpensesList { get; set; }
        public List<Expenses> ThisMonthExpensesList { get; set; }
        public List<Expenses> NextMonthExpensesList { get; set; }
        public decimal? ThisWeekExpensesSubmitted { get; set; }
        public decimal? NextWeekExpensesSubmitted { get; set; }
        public decimal? ThisMonthExpensesSubmitted { get; set; }
        public decimal? NextMonthExpensesSubmitted { get; set; }
        public decimal? ThisWeekExpensesAproved { get; set; }
        public decimal? NextWeekExpensesAproved { get; set; }
        public decimal? ThisMonthExpensesApproved { get; set; }
        public decimal? NextMonthExpensesApproved { get; set; }
        public DateTime? ThisWeekStart { get; set; }
        public DateTime? ThisWeekEnd { get; set; }
        public DateTime? LastWeekStart { get; set; }
        public DateTime? LastWeekEnd { get; set; }
        public DateTime? ThisMonthStart { get; set; }
        public DateTime? LastMonthStart { get; set; }
        public DateTime? ThisMonthEnd { get; set; }
        public DateTime? LastMonthEnd { get; set; }
        public List<GetLeaveRequestsResult> Leaves { get; set; }
        public List<GetHolidaysResult> Holidays { get; set; }
    }

    #endregion
    #region PM-TM
    public class TimeAndExpenses
    {
        public string TimesheetOrExpense { get; set; }
        public bool IsApprove { get; set; }
        public string Remarks { get; set; }
        public List<TimeAndExpensesItem> TimeSheetExpenseItems { get; set; }
    }
    public class TimeAndExpensesItem
    {
        public int Id { get; set; }
        public bool isAssigned { get; set; }
    }
    #endregion

    public class ReviewTimesheetExpense
    {
        public ScheduledActionDetails ActionDetails { get; set; }
        public GetUserActionLogHoursResult TimeSheet { get; set; }
        public List<GetUserActionExpensesResult> Expenses { get; set; }
        public bool IsApprove { get; set; }
        public string PMRemarks { get; set; }
        public string RecDate { get; set; }
    }
}