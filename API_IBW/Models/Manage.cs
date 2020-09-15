using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_IBW.Models
{


    public class LeaveRequests
    {
        public string Color { get; set; }
        public decimal? PreLeaveBalance { get; set; }
        public decimal? PrePTOBalance { get; set; }

        public decimal? TotalLeaveBalance { get; set; }
        public decimal? PTOLeaveBalance { get; set; }
        public decimal? ActualPaidHours { get; set; }
        public string FromUserName { get; set; }
        public string ToUserName { get; set; }
        public string LeaveUserName { get; set; }
        public List<long?> UserJobIds { get; set; }
        public long LeaveRequestId { get; set; }
        public long? TypeOfAbsenceId { get; set; }
        public string TypeOfAbsence { get; set; }
        public long? LeaveReasonId { get; set; }
        public string LeaveReason { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public bool? IsFullDay { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsDeclined { get; set; }
        public bool IsDelete { get; set; }
        public long? UserId { get; set; }
        public long? AuthorisedById { get; set; }
        public string TimeDuration { get; set; }
        public string userName { get; set; }
        public int LeavesCount { get; set; }
        public bool IsSelected { get; set; }
        public string ReportingManager { get; set; }
        public List<int> LeaveIds { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public string Remarks { get; set; }
        public string CreatedDateString { get; set; }
        public string ActivityType { get; set; }
        public string FromDateString { get; set; }
        public string ToDateString { get; set; }
        public bool valuesUpdated { get; set; }
        public string Notes { get; set; }

    }
    public class GetLeaveRequests
    {
        public List<LookupOptions> AbsenceTypeList { get; set; }
        public List<LookupOptions> ActivityTypeList { get; set; }
        public List<IBWUsers> UserNameList { get; set; }
        public bool isReportingManager { get; set; }
        public bool IsManager { get; set; }
        public string ReportingManager { get; set; }
        public List<LeaveRequests> ActivityLeaveRequestsList { get; set; }
        public List<LeaveRequests> AssociateUsersLeaveRequestsList { get; set; }
        public List<LeaveRequests> TodayAssociateUsersLeaveRequestsList { get; set; }
        public List<LeaveRequests> TomorrowAssociateUsersLeaveRequestsList { get; set; }
        public List<LeaveRequests> ThisWeeksAssociateUsersLeaveRequestsList { get; set; }
        public List<LeaveRequests> ThisMonthsAssociateUsersLeaveRequestsList { get; set; }
        public List<LeaveRequests> AllLeavesForCalendar { get; set; }
        public List<IBWUsers> RMs { get; set; }
    }
}