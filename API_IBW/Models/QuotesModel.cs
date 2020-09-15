using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace API_IBW.Models
{
    public class ActionsCreate : ActionsManage
    {
        public long? budgetSowId { get; set; }
        public long? taskBudgetId { get; set; }
        public long? sowId { get; set; }
        public long? manageSowId { get; set; }
        public long? siteId { get; set; }
        public long? quoteId { get; set; }
        public string siteName { get; set; }
        public string sowNumber { get; set; }
        public string sowName { get; set; }
        public long createdBy { get; set; }
    }
    public class QuoteModel
    {
    }

    public class Sites
    {
        public bool isDefault { get; set; }
        public long? siteId { get; set; }
        public string siteName { get; set; }
        public string label { get; set; }
        public long? quoteId { get; set; }
        public string quoteNumber { get; set; }
        public long? countryId { get; set; }
        public string countryName { get; set; }
        public long? muncipalityId { get; set; }
        public long? addressId { get; set; }
        public string muncipalityName { get; set; }
        public long? stateId { get; set; }
        public string stateName { get; set; }
        public string cityName { get; set; }
        public string zipCode { get; set; }
        public string googleDriveFolderId { get; set; }
        public string streetAddress { get; set; }
        public long? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public long? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
        public string iconClass { get; set; }
        public bool isSite { get; set; }
        public bool selectable { get; set; }
        public decimal? totalBudgetAmount { get; set; }
        public decimal? totalQuotedAmount { get; set; }
        public decimal? totalExpensesAmount { get; set; }
        public decimal? totalBudgetHours { get; set; }
        public List<Sow> children { get; set; }
        public string siteSerialNumber { get; set; }
    }

    public class Sow
    {
        public long? sowId { get; set; }
        public string sowName { get; set; }
        public string label { get; set; }
        public long? sowTypeId { get; set; }
        public long? quoteId { get; set; }
        public bool isSite { get; set; }
        public long? siteId { get; set; }
        public string siteName { get; set; }
        public string iconClass { get; set; }
        public long? createdBy { get; set; }
        public long? modifiedBy { get; set; }
        public string googleDriveFolderId { get; set; }
        public string styleClass { get; set; }
        public string iconStyle { get; set; }
        public bool isSavedInBudget { get; set; }
        public bool? isApproved { get; set; }
        public DateTime createdDate { get; set; }
        public DateTime modifiedDate { get; set; }
        public bool isDefault { get; set; } = false;
        public string sowNumber { get; set; }
        public long? sowStatus { get; set; }
        public long? statusOptionId { get; set; }
        public string remarks { get; set; }
        public long? sowInvoiceType { get; set; }
        public string sowDescription { get; set; }
        public string projectTypeName { get; set; }
        public string projectStatusName { get; set; }
        public string feeStructureName { get; set; }
    }
    public class Budget
    {
        public bool isDefault { get; set; }
        public long? budgetId { get; set; }
        public long? quoteId { get; set; }
        public long? siteId { get; set; }
        public long? sowId { get; set; }
        public bool? isSubmitted { get; set; }
        public decimal? totalBudgetCostSOW { get; set; }
        public decimal? totalBudgetHoursSOW { get; set; }
        public decimal? totalQuotedCostSOW { get; set; }
        public decimal? totalExpensesCost { get; set; }
        public long? createdBy { get; set; }
        public long? modifiedBy { get; set; }
        public List<TaskBudget> taskandJobCodesData { get; set; }
        public List<expenses> expensesData { get; set; }
        [AllowHtml]
        public string manageNotes { get; set; }
    }
    public class TaskBudget
    {
        public long? taskBudgetId { get; set; }
        public long? budgetId { get; set; }
        public long? taskId { get; set; }
        public string taskName { get; set; }
        public decimal? totalBudgetCostSOW { get; set; }
        public decimal? totalBudgetHoursSOW { get; set; }
        public decimal? totalQuotedCostSOW { get; set; }
        public List<JobCodeBudget> Data { get; set; }
        public long? taskManageId { get; set; }
    }
    public class JobCodeBudget
    {
        public long? jobCodeBudgetId { get; set; }
        public long? taskBudgetId { get; set; }
        public long? jobCodeId { get; set; }
        public decimal? plannedHours { get; set; }
        public decimal? rate { get; set; }
        public decimal? total { get; set; }
        public string notes { get; set; }
        public bool? isUserAdded { get; set; }
        public bool? isDeleted { get; set; }
    }


    public class Manage
    {
        public long? manageId { get; set; }
        public long? quoteId { get; set; }
        public long? siteId { get; set; }
        public long? sowId { get; set; }
        public bool? isSubmitted { get; set; }
        public decimal? totalActualCost { get; set; }
        public decimal? totalActualHours { get; set; }
        public decimal? totalPlannedCost { get; set; }
        public decimal? totalPlannedHours { get; set; }
        public decimal? totalBudgetAmount { get; set; }
        public decimal? totalExpenseCost { get; set; }
        public decimal? totalBudgetHours { get; set; }
        public decimal? totalQuotedAmount { get; set; }
        public long? createdBy { get; set; }
        public long? modifiedBy { get; set; }
        public List<TaskManage> taskData { get; set; }
    }
    public class actionDetails
    {
        public string actionFileUniqueId { get; set; }

    }
    public class TaskManage
    {
        public long? taskManageId { get; set; }
        public long? taskBudgetId { get; set; }
        public long? manageId { get; set; }
        public long? taskId { get; set; }
        public string taskName { get; set; }
        public decimal? totalPlannedCost { get; set; }
        public decimal? totalPlannedHours { get; set; }
        public decimal? totalActualCost { get; set; }
        public decimal? totalActualHours { get; set; }
        public decimal? totalBudetAmount { get; set; }
        public decimal? totalBudgetHours { get; set; }
        public decimal? totalQuotedAmount { get; set; }
        public List<ActionsManage> actionData { get; set; }
    }
    public class ActionsManage
    {
        public long? actionManageId { get; set; }
        public long? actionId { get; set; }
        public string actionName { get; set; }
        public string jobCodeName { get; set; }
        public long? taskManageId { get; set; }
        public long? jobCodeId { get; set; }
        public decimal? plannedHours { get; set; }
        public decimal? jobCodeRate { get; set; }
        public decimal? plannedCost { get; set; }
        public decimal? actualHours { get; set; }
        public decimal? actualCost { get; set; }
        public DateTime? dueDate { get; set; }
        public long? dueDateOption { get; set; }
        public string googleDriveFileId { get; set; }
        public string mimeType { get; set; }
        public HttpPostedFile googleDriveFile { get; set; }
        [AllowHtml]
        public string notes { get; set; }
        public bool? isUserAdded { get; set; }
        public bool? isDeleted { get; set; }
        public bool isRemedial { get; set; }
        public long? teamMemberId { get; set; }
        public bool? isScheduled { get; set; }
        public long? scheduledActionsCount { get; set; }
        public string actionFileUniqueId { get; set; }
    }
    public class expenses
    {
        public long? expenseBudgetId { get; set; }
        public long? expenseBudgetCustomId { get; set; }
        public long? expenseCodeId { get; set; }
        public string expenseCodeName { get; set; }
        public string expenseUnit { get; set; }
        public decimal? expenseRate { get; set; }
        public decimal? quantity { get; set; }
        public decimal? total { get; set; }
        public bool? isDeleted { get; set; }
        public long? createdBy { get; set; }
        public long? modifiedBy { get; set; }
    }
    public class ExpensesManage : expenses
    {
        public long actionLogHourExpenseId { get; set; }
        public long? actionLogHourId { get; set; }
        public string limitType { get; set; }
        public string isDefault { get; set; }
        public string description { get; set; }
        public bool? expenseCodeStatus { get; set; }
        public string actionFolderId { get; set; }
        public bool? expenseCodeIsDeleted { get; set; }
    }

    public class LogHours
    {
        public long? logHourId { get; set; }
        public long? quoteID { get; set; }
        public long? siteId { get; set; }
        public string siteName { get; set; }
        public long? sowId { get; set; }
        public string sowName { get; set; }
        public long? taskId { get; set; }
        public string taskName { get; set; }
        public long? manageActionId { get; set; }
        public string manageActionName { get; set; }
        public string jobCode { get; set; }
        public string jobCodeTitle { get; set; }
        public bool? status { get; set; }
        public string updatedBy { get; set; }
        public string updatedDate { get; set; }
        public string dueDate { get; set; }
        public string loggedDate { get; set; }
        public string loggedBy { get; set; }
        public string logHoursStatus { get; set; }
        public string logHourRemarks { get; set; }
        public string googleDriveId { get; set; }
        public decimal? plannedHours { get; set; }
        public decimal? workedHours { get; set; }
        public decimal? remainingHours { get; set; }
        public string pmMessage { get; set; }
        public int isAssigned { get; set; }
        public string projectManager { get; set; }
        public int? notesCount { get; set; }
    }


    public class ActionExpenses : LogHours
    {
        public long? expenseCodeId { get; set; }
        public string expenseCode { get; set; }
        public string limitType { get; set; }
        public decimal? expenseRate { get; set; }
        public long? quantity { get; set; }
        public decimal? totalExpenseCost { get; set; }
        public bool isReimbursable { get; set; }
        public string expenseDescription { get; set; }
        public long? expenseId { get; set; }
        public long? siteId { get; set; }
        public long? sowId { get; set; }
    }

}