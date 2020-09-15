using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace API_IBW.Models
{

    public class ActionsMaster
    {
        public long? actionMasterId { get; set; }
        public string actionMasterName { get; set; }
        public long? jobCodeId { get; set; }
        public string jobCode { get; set; }
        public string jobCodeTitle { get; set; }
        public decimal? jobCodeRate { get; set; }
        public bool isDeleted { get; set; }
        public long? createdBy { get; set; }
        public long? modifiedBy { get; set; }
        public bool status { get; set; }
        public long projectsCount { get; set; }
        public long quotesCount { get; set; }
    }
    public class Tasks
    {
        public long? TaskId { get; set; }
        public string TaskName { get; set; }
        public List<long?> JobCodeIds { get; set; }
        public List<string> JobCodes { get; set; }
        public List<string> Notes { get; set; }
        public long? positionIndex { get; set; }
        public long? UserId { get; set; }
        public bool? IsActive { get; set; }
        public List<JobCodes> jobCodeDetails { get; set; }
        public List<long?> documentTypes { get; set; }
        public List<string> documentTypeNames { get; set; }
    }
    public class JobCodes
    {
        public long? JobCodeId { get; set; }

        [Required]
        public string JobCode { get; set; }

        [Required]
        public string JobCodeTitle { get; set; }
        public string Notes { get; set; }

        [Required]
        public decimal? ChargeoutRate { get; set; }
        public int? AssociatedUsersCount { get; set; }

        public bool? IsActive { get; set; }
        public bool IsDelete { get; set; }

        public long? UserId { get; set; }
        public long? JobCodeCategoryId { get; set; }
        public string JobCodeCategory { get; set; }
    }

    public class ExpenseCodes
    {
        public long ExpenseCodeId { get; set; }
        public string ExpenseCode { get; set; }
        public string ExpenseLimitType { get; set; }
        public string ExpenseUnit { get; set; }
        public long? ExpenseUnitId { get; set; }
        public long? ExpenseLimitTypeId { get; set; }
        public decimal? ExpenseLimitAmount { get; set; }
        public decimal? Rate { get; set; }
        public bool? IsActive { get; set; }
        public bool isDefault { get; set; }
        public bool isReimbursable { get; set; }
        public bool IsDelete { get; set; }
        public long? UserId { get; set; }
        public bool? AttachmentRequired { get; set; }
    }
    public class CommonMasterData
    {
        public long CommonMasterDataId { get; set; }
        public string CommonMasterDataName { get; set; }
        public string CommonMasterDataCategory { get; set; }
        public int? AssociatedClientsCount { get; set; }
        public int? AssociatedQuestionsCount { get; set; }
        public bool? IsActive { get; set; }
        public long? UserId { get; set; }
        public long? dataTypeAssignedCount { get; set; }
    }
    public class AssetCategories
    {
        public long AssetCategoryId { get; set; }
        public string AssetCategoryName { get; set; }
        public string AssetCategoryPrefix { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDelete { get; set; }

        public long? UserId { get; set; }

    }
    public class ConfigQuestions
    {
        public long ConfigQuestionId { get; set; }
        public long? SurveyPurposeId { get; set; }
        public string SurveyPurposeName { get; set; }

        public string ConfigQuestion { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDelete { get; set; }
        public long? UserId { get; set; }

    }
    public class SelectedQuestions: ConfigQuestions
    {
        public bool selected { get; set; }
    }
    public class LeaveReasons
    {
        public long LeaveReasonId { get; set; }
        public long? TypeOfAbsenceId { get; set; }
        public string LeaveReason { get; set; }

        public string TypeOfAbsence { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDelete { get; set; }
        public long? UserId { get; set; }

    }
    public class Municipalities
    {
        public long? municipalityId { get; set; }
        public string municipalityName { get; set; }
        public bool? status { get; set; }
        public long? createdBy { get; set; }
        public long? modifiedBy { get; set; }
        public List<string> municipalitiesList { get; set; }
    }
    public class PotentialLevel
    {
        public long? potentialLevelId { get; set; }
        public string potentialLevelName { get; set; }
        public string colorCode { get; set; }
        public bool? status { get; set; }
        public long? createdBy { get; set; }
        public long? modifiedBy { get; set; }
    }

}