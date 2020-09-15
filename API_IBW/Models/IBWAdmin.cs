using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_IBW.Models
{
    public class AllJobCodesWithCategory
    {
        public long? JobCodeCategoryId { get; set; }
        public string JobCodeCategory { get; set; }
        public List<AllJobCodes> JobCodes { get; set; }
    }
    //author: Sreebharath
    //date: 07-08/2019
    //For User Details Model
    public class IBWUsers
    {
        public int? leavesCount { get; set; }
        public int? schActionsCount { get; set; }
        public int? timesheetsCount { get; set; }
        public int? asRMCount { get; set; }
        public int? asPMAMCount { get; set; }
        public long? userId { get; set; }
        public string userName { get; set; }
        public string aliasName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public long? employmentTypeId { get; set; }
        public long? reportingManagerId { get; set; }
        public string reportingManagerName { get; set; }
        public bool? isVerified { get; set; }
        public decimal? eligibleVacationDays { get; set; }
        public decimal? openingLeaveBalance { get; set; }
        public bool? status { get; set; }
        public long? createdBy { get; set; }
        public DateTime createdDate { get; set; }
        public long? modifiedBy { get; set; }
        public DateTime modifiedDate { get; set; }
        public long? permissionRoleId { get; set; }
        public string permissionLevelName { get; set; }
        public List<AllJobCodes> jobCodeSet { get; set; }
        public List<long> jobCodes { get; set; }
        public List<string> jobShortCode { get; set; }
        public string jobCodesStringSeperated { get; set; }
        public string permissionLevelStringSeperated { get; set; }
        public List<long> permissionRoleIds { get; set; }
        public List<UserRoles> PermissionRoles { get; set; }
        public decimal? hoursPerDay { get; set; }
        public bool? isDelete { get; set; }
        public bool? isSuperAdmin { get; set; }
        public decimal? totalLeaveBalance { get; set; }
        public string startDate { get; set; }
        public bool? resendEmail { get; set; }
        public decimal? ptoLeaveBalance { get; set; }
    }




    //author: Sreebharath
    //date: 07-08/2019
    //Model for User Roles
    public class UserRoles
    {
        public long? roleMasterId { get; set; }
        public string roleName { get; set; }
        public bool? status { get; set; }
    }
    //author: Sreebharath
    //date: 07-08/2019
    //Model for Employee Tyoes
    public class EmployeeTypes
    {
        public long employeeTypeId { get; set; }
        public string employeeTypeName { get; set; }
    }

    //author: Sreebharath
    //date: 07-08/2019
    //Model for Employee Tyoes
    public class AllJobCodes
    {
        public long? JobCodeCategoryId { get; set; }
        public string JobCodeCategory { get; set; }
        public long? jobCodeId { get; set; }
        public string jobCode { get; set; }
        public string jobCodeTitle { get; set; }
        public long? jobRate { get; set; }
        public bool? status { get; set; }
    }

    #region Clients and Contacts

    //author: Shyam
    //date: 14/08/2019
    //Model for clients
    public class Clients
    {
        public long? clientID { get; set; }
        public string clientName { get; set; }
        public string clientCode { get; set; }
        public string clientEmail { get; set; }
        public string clientPhone { get; set; }
        public string clientType { get; set; }
        public long? clientTypeID { get; set; }
        public long? clientAddressID { get; set; }
        public long? clientCountryID { get; set; }
        public string clientCountry { get; set; }
        public long? clientMuncipalityID { get; set; }
        public string clientMuncipality { get; set; }
        public long? clientStateID { get; set; }
        public string clientState { get; set; }
        public string clientCity { get; set; }
        public string clientZip { get; set; }
        public string clientStreetAddress { get; set; }
        public bool? clientbtFlag { get; set; }
        public bool? clientbtStatus { get; set; }
        public bool? clientbtDelete { get; set; }
        public long? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public DateTime? activityDate { get; set; }
        public long? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
        public long? clientContacts { get; set; }
        public long? quotesCount { get; set; }
        public long? projectsCount { get; set; }
    }

    //author: Shyam
    //date: 27/08/2019
    //Model for contacts
    public class Contacts
    {
        public long? contactID { get; set; }
        public long? clientID { get; set; }
        public string clientName { get; set; }
        public string contactName { get; set; }
        public string contactJobTitle { get; set; }
        public string contactEmail { get; set; }
        public string contactPhone { get; set; }
        public string contactCountry { get; set; }
        public long? contactAddressID { get; set; }
        public long? contactCountryID { get; set; }
        public long? contactMuncipalityID { get; set; }
        public string contactMuncipality { get; set; }
        public long? contactStateID { get; set; }
        public string contactState { get; set; }
        public string contactCity { get; set; }
        public string contactZip { get; set; }
        public string contactStreetAddress { get; set; }
        public bool? contactIsPrimaryContact { get; set; }
        public bool? contactIsBillingContact { get; set; }
        //public bool? clientbtFlag { get; set; }
        public bool? contactbtStatus { get; set; }
        public bool? contactbtDelete { get; set; }
        public long? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public DateTime? activityDate { get; set; }
        public long? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
        public bool? isAddressCopiedFromClient { get; set; }
        public long? quotesCount { get; set; }
    }

    //author: Shyam
    //date: 18-08/2019
    //Model for Client Types
    public class ClientTypes
    {
        public long? clientTypeId { get; set; }
        public string clientTypeName { get; set; }
    }

    //author: Shyam
    //date: 18-08/2019
    //Model for Grid Columns
    public class GridColumns
    {
        public List<string> gridColumnNames { get; set; }
    }

    //author: Shyam
    //date: 18-08/2019
    //Model for Countries
    public class Countries
    {
        public long? countryId { get; set; }
        public string countryName { get; set; }
    }

    //author: Shyam
    //date: 18-08/2019
    //Model for States
    public class States
    {
        public long? stateId { get; set; }
        public string stateName { get; set; }
        public long? countryID { get; set; }
    }

    //author: Shyam
    //date: 18-08/2019
    //Model for Muncipalities
    public class Muncipalities
    {
        public long? muncipalityId { get; set; }
        public string muncipalityName { get; set; }
        public long? stateID { get; set; }
    }
    #endregion
}