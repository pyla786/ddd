using API_IBW.DB_Models;
using API_IBW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_IBW.Business_Classes
{
    public class ReportsMethods
    {
        private SchedulingDataContext _schedulingDB;
        private Admin _adminMethods;
        private AdminDataContext _adminDB;

        public ReportsMethods()
        {
            _schedulingDB = new SchedulingDataContext();
            _adminMethods = new Admin();
            _adminDB = new AdminDataContext();
        }

        #region Payroll Bi-Weekly

        //Author        : Siddhant Chawade
        //Date          : 17th Feb 2020
        //Description   : To get payroll bi weekly report
        public PayrollBiWeekly GetPayrollBiWeekly(long? managerId, long? UserId, DateTime? fromDate, DateTime? toDate)
        {
            PayrollBiWeekly data = new PayrollBiWeekly();
            List<UserPayroll> UserPayroll = new List<UserPayroll>();
            try
            {
                //assigning dates if dates are null
                if (fromDate == null || toDate == null)
                {
                    toDate = DateTime.Now.Date;
                    fromDate = DateTime.Now.AddDays(-13).Date;
                }

                //list of summary of all users
                List<string> summary = new List<string>();
                //summary.Add("Summary of All Users");
                for (int i = 1; i <= 23; i++)
                    summary.Add("0.00");

                //list of headers
                List<string> headers = new List<string>();
                //headers.Add("First Name");
                //headers.Add("Last Name");
                headers.Add("Time Type");

                //to add dates in headers
                List<DateTime?> dates = new List<DateTime?>();
                while (fromDate <= toDate)
                {
                    if (dates.Count == 7)
                    {
                        headers.Add("Week Totals");
                        headers.Add("Regular Total");
                        headers.Add("OT Total");
                    }
                    dates.Add(fromDate);
                    headers.Add(Convert.ToDateTime(fromDate).ToString("yyyy-MM-dd"));
                    fromDate = Convert.ToDateTime(fromDate).AddDays(1);
                }

                //adding other columns in header
                headers.Add("Week Totals");
                headers.Add("Regular Total");
                headers.Add("OT Total");
                headers.Add("Bi-Week Total Hours");
                headers.Add("Bi-Week Regular Total");
                headers.Add("Bi-Week OT Total");

                //fetching list of users
                var users = _adminMethods.GetUsers().OrderBy(x => x.userName).ToList();
                data.Users = users.AsEnumerable().Select(x => new IBWUsers
                {
                    userId = x.userId,
                    userName = x.userName + " " + x.aliasName
                }).ToList();

                var managers = users.Where(x => x.asRMCount > 0).ToList();
                data.ReportingManagers = managers.AsEnumerable().Select(x => new IBWUsers
                {
                    userId = x.userId,
                    userName = x.userName + " " + x.aliasName
                }).ToList();

                //applying reporting manager filter
                if (managerId != null)
                    users = users.Where(x => x.reportingManagerId == managerId).ToList();

                //applying user filter
                if (UserId != null)
                    users = users.Where(x => x.userId == UserId).ToList();

                //fetching list of approved timesheets of all users
                var hours = _schedulingDB.sp_get_user_timesheet().Where(x => x.isApproved == true).ToList();
                //fetching the list of holidays
                var holidays = _adminMethods.GetHolidays();
                //fetching the list of leaves
                var leaves = _adminDB.GetLeaveRequests().ToList();
                if (users != null && users.Count > 0)
                {
                    foreach (var user in users)
                    {
                        UserPayroll userPayroll = new UserPayroll();

                        userPayroll.UserId = user.userId;
                        userPayroll.FirstName = user.userName;
                        userPayroll.LastName = user.aliasName;
                        //list of regular hours
                        List<string> regular = new List<string>();
                        //list of holiday hours
                        List<string> holiday = new List<string>();
                        //list of leave hours
                        List<string> leave = new List<string>();
                        //list of total hours
                        List<string> total = new List<string>();

                        //variables for calculating totals
                        double? regularTotal = 0.00;
                        double? regularTotal1 = 0.00;
                        double? regularTotal2 = 0.00;

                        double? holidayTotal = 0.00;
                        double? holidayTotal1 = 0.00;
                        double? holidayTotal2 = 0.00;

                        double? leaveTotal = 0.00;
                        double? leaveTotal1 = 0.00;
                        double? leaveTotal2 = 0.00;

                        double? totalTotal = 0.00;
                        double? totalTotal1 = 0.00;
                        double? totalTotal2 = 0.00;

                        double? ot = 0.00;
                        double? ot1 = 0.00;
                        double? ot2 = 0.00;

                        regular.Add("Regular Hours");
                        holiday.Add("Statutory Holiday");
                        leave.Add("Leave");
                        total.Add("Total");

                        int i = 0;
                        foreach (var date in dates)
                        {
                            bool isHoliday = false;
                            bool isLeave = false;
                            isHoliday = holidays.Where(x => x.holidayDate == date).ToList().Count > 0 ? true : false;
                            isLeave = leaves.Where(x => x.int_created_by == user.userId && x.is_approved == true && ((x.dt_from_date == date && x.dt_to_date == null) || (x.dt_from_date <= date && x.dt_to_date != null && x.dt_to_date >= date))).ToList().Count > 0 ? true : false;

                            double? thisDayHours = 0;
                            //getting list of approved timesheets for current date
                            var currentDayHours = hours.Where(x => x.userId == user.userId && Convert.ToDateTime(x.completionDate) == date).ToList();
                            if (currentDayHours != null && currentDayHours.Count > 0)
                            {
                                //calculating sum of worked hours 
                                thisDayHours = currentDayHours.Sum(x => Convert.ToDouble(x.workedHours));

                                //calculating summary of all users
                                summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + thisDayHours).ToString("0.00");

                                //adding to holiday list if current date is a holiday
                                if (isHoliday)
                                {
                                    double? holidayHours = Convert.ToDouble(user.hoursPerDay);
                                    regular.Add(Convert.ToDouble(thisDayHours).ToString("0.00"));
                                    holiday.Add(Convert.ToDouble(holidayHours).ToString("0.00"));
                                    leave.Add("0.00");
                                    total.Add(Convert.ToDouble(thisDayHours).ToString("0.00"));
                                    //adding to week 1 total
                                    if (regular.Count <= 8)
                                    {
                                        regularTotal1 = regularTotal1 + thisDayHours;
                                        holidayTotal1 = holidayTotal1 + holidayHours;
                                        totalTotal1 = totalTotal1 + thisDayHours;
                                    }
                                    //adding to week 2 total
                                    else
                                    {
                                        regularTotal2 = regularTotal2 + thisDayHours;
                                        holidayTotal2 = holidayTotal2 + holidayHours;
                                        totalTotal2 = totalTotal2 + thisDayHours;
                                    }
                                }
                                //adding to leave list if current date is a leave
                                else if (isLeave)
                                {
                                    var leaveToday = leaves.Where(x => x.int_created_by == user.userId && x.is_approved == true && ((x.dt_from_date == date && x.dt_to_date == null) || (x.dt_from_date <= date && x.dt_to_date != null && x.dt_to_date >= date))).FirstOrDefault();
                                    double? LeaveHours = Convert.ToDouble(leaveToday.int_duration);
                                    if (leaveToday.vc_absence_type == "Multiple Days")
                                    {
                                        var numOfDays = (Convert.ToDateTime(leaveToday.dt_to_date) - Convert.ToDateTime(leaveToday.dt_from_date)).TotalDays + 1;
                                        LeaveHours = LeaveHours / numOfDays;
                                    }
                                    regular.Add(Convert.ToDouble(thisDayHours).ToString("0.00"));
                                    holiday.Add("0.00");
                                    leave.Add(Convert.ToDouble(LeaveHours).ToString("0.00"));
                                    total.Add(Convert.ToDouble(thisDayHours).ToString("0.00"));
                                    //adding to week 1 total
                                    if (regular.Count <= 8)
                                    {
                                        regularTotal1 = regularTotal1 + thisDayHours;
                                        leaveTotal1 = leaveTotal1 + LeaveHours;
                                        totalTotal1 = totalTotal1 + thisDayHours;
                                    }
                                    //adding to week 2 total
                                    else
                                    {
                                        regularTotal2 = regularTotal2 + thisDayHours;
                                        leaveTotal2 = leaveTotal2 + LeaveHours;
                                        totalTotal2 = totalTotal2 + thisDayHours;
                                    }
                                }
                                //adding to regular list if current date is neither a holiday nor a leave
                                else
                                {
                                    regular.Add(Convert.ToDouble(thisDayHours).ToString("0.00"));
                                    holiday.Add("0.00");
                                    leave.Add("0.00");
                                    total.Add(Convert.ToDouble(thisDayHours).ToString("0.00"));
                                    //adding to week 1 total
                                    if (regular.Count <= 8)
                                    {
                                        regularTotal1 = regularTotal1 + thisDayHours;
                                        totalTotal1 = totalTotal1 + thisDayHours;
                                    }
                                    //adding to week 2 total
                                    else
                                    {
                                        regularTotal2 = regularTotal2 + thisDayHours;
                                        totalTotal2 = totalTotal2 + thisDayHours;
                                    }
                                }
                            }
                            else
                            {
                                regular.Add("0.00");
                                holiday.Add("0.00");
                                leave.Add("0.00");
                                total.Add("0.00");
                            }
                            //caculating total hours and OT hours for week 1
                            if (regular.Count == 8)
                            {


                                //calculating summary of all users
                                i++;
                                summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + regularTotal1 + holidayTotal1 + leaveTotal1).ToString("0.00");

                                regular.Add(Convert.ToDouble(regularTotal1).ToString("0.00"));
                                holiday.Add(Convert.ToDouble(holidayTotal1).ToString("0.00"));
                                leave.Add(Convert.ToDouble(leaveTotal1).ToString("0.00"));
                                total.Add(Convert.ToDouble(regularTotal1 + holidayTotal1 + leaveTotal1).ToString("0.00"));

                                //if hours are grater than 44 per week, consider as OT
                                ot1 = totalTotal1 > 44 ? totalTotal1 - 44 : 0;
                                totalTotal1 = totalTotal1 > 44 ? 44 : totalTotal1;

                                i++;
                                summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + (regularTotal1 > 44 ? 44 : regularTotal1) + holidayTotal1 + leaveTotal1).ToString("0.00");

                                regular.Add(Convert.ToDouble(regularTotal1 > 44 ? 44 : regularTotal1).ToString("0.00"));
                                holiday.Add(Convert.ToDouble(holidayTotal1).ToString("0.00"));
                                leave.Add(Convert.ToDouble(leaveTotal1).ToString("0.00"));
                                total.Add(Convert.ToDouble((regularTotal1 > 44 ? 44 : regularTotal1) + holidayTotal1 + leaveTotal1).ToString("0.00"));

                                i++;
                                summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + ot1).ToString("0.00");

                                regular.Add(Convert.ToDouble(regularTotal1 > 44 ? regularTotal1 - 44 : 0).ToString("0.00"));
                                holiday.Add("-");
                                leave.Add("-");
                                total.Add(Convert.ToDouble(ot1).ToString("0.00"));
                            }
                            //caculating total hours and OT hours for week 2 and sum of w1 and w2
                            if (regular.Count == 18)
                            {

                                //calculating summary of all users
                                i++;
                                summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + regularTotal2 + holidayTotal2 + leaveTotal2).ToString("0.00");

                                regular.Add(Convert.ToDouble(regularTotal2).ToString("0.00"));
                                holiday.Add(Convert.ToDouble(holidayTotal2).ToString("0.00"));
                                leave.Add(Convert.ToDouble(leaveTotal2).ToString("0.00"));
                                total.Add(Convert.ToDouble(regularTotal2 + holidayTotal2 + leaveTotal2).ToString("0.00"));

                                //if hours are grater than 44 per week, consider as OT
                                ot2 = totalTotal2 > 44 ? totalTotal2 - 44 : 0;
                                //totalTotal2 = totalTotal2 > 44 ? 44 : totalTotal2;

                                i++;
                                summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + (regularTotal2 > 44 ? 44 : regularTotal2) + holidayTotal2 + leaveTotal2).ToString("0.00");

                                regular.Add(Convert.ToDouble(regularTotal2 > 44 ? 44 : regularTotal2).ToString("0.00"));
                                holiday.Add(Convert.ToDouble(holidayTotal2).ToString("0.00"));
                                leave.Add(Convert.ToDouble(leaveTotal2).ToString("0.00"));
                                total.Add(Convert.ToDouble((regularTotal2 > 44 ? 44 : regularTotal2) + holidayTotal2 + leaveTotal2).ToString("0.00"));

                                i++;
                                summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + ot2).ToString("0.00");

                                regular.Add(Convert.ToDouble(regularTotal2 > 44 ? regularTotal2 - 44 : 0).ToString("0.00"));
                                holiday.Add("-");
                                leave.Add("-");
                                total.Add(Convert.ToDouble(ot2).ToString("0.00"));

                                //calculating summary of all users
                                i++;
                                summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + regularTotal1 + regularTotal2 + holidayTotal1 + holidayTotal2 + leaveTotal1 + leaveTotal2).ToString("0.00");

                                regular.Add(Convert.ToDouble(regularTotal1 + regularTotal2).ToString("0.00"));
                                holiday.Add(Convert.ToDouble(holidayTotal1 + holidayTotal2).ToString("0.00"));
                                leave.Add(Convert.ToDouble(leaveTotal1 + leaveTotal2).ToString("0.00"));
                                total.Add(Convert.ToDouble(regularTotal1 + regularTotal2 + holidayTotal1 + holidayTotal2 + leaveTotal1 + leaveTotal2).ToString("0.00"));

                                regularTotal = regularTotal1 + regularTotal2;
                                holidayTotal = holidayTotal1 + holidayTotal2;
                                leaveTotal = leaveTotal1 + leaveTotal2;
                                totalTotal = totalTotal1 + totalTotal2 > 88 ? 88 : totalTotal1 + totalTotal2;
                                ot = ot1 + ot2;

                                i++;
                                summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + ((regularTotal1 > 44 ? 44 : regularTotal1) + (regularTotal2 > 44 ? 44 : regularTotal2)) + holidayTotal + leaveTotal).ToString("0.00");

                                regularTotal = (regularTotal1 > 44 ? 44 : regularTotal1) + (regularTotal2 > 44 ? 44 : regularTotal2);

                                regular.Add(Convert.ToDouble((regularTotal1 > 44 ? 44 : regularTotal1) + (regularTotal2 > 44 ? 44 : regularTotal2) > 88 ? 88 : regularTotal).ToString("0.00"));
                                holiday.Add(Convert.ToDouble(holidayTotal).ToString("0.00"));
                                leave.Add(Convert.ToDouble(leaveTotal).ToString("0.00"));
                                total.Add(Convert.ToDouble(regularTotal + holidayTotal + leaveTotal).ToString("0.00"));

                                i++;
                                summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + ot).ToString("0.00");

                                regular.Add(Convert.ToDouble(ot).ToString("0.00"));
                                holiday.Add("-");
                                leave.Add("-");
                                total.Add(Convert.ToDouble(ot).ToString("0.00"));
                            }
                            i++;
                        }

                        userPayroll.Regular = regular;
                        userPayroll.Holiday = holiday;
                        userPayroll.Leave = leave;
                        userPayroll.Total = total;

                        UserPayroll.Add(userPayroll);
                    }
                }

                data.Headers = headers;
                data.Summary = summary;
                data.UserPayrolls = UserPayroll;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }
        //Author        : Siddhant Chawade
        //Date          : 17th Feb 2020
        //Description   : To get payroll bi weekly report
        public PayrollBiWeekly GetExpensesBiWeekly(long? managerId, long? UserId, DateTime? fromDate, DateTime? toDate)
        {
            PayrollBiWeekly data = new PayrollBiWeekly();
            List<UserPayroll> UserPayroll = new List<UserPayroll>();
            try
            {
                //assigning dates if dates are null
                if (fromDate == null || toDate == null)
                {
                    toDate = DateTime.Now.Date;
                    fromDate = DateTime.Now.AddDays(-13).Date;
                }

                //list of summary of all users
                List<string> summary = new List<string>();
                //summary.Add("Summary of All Users");
                for (int i = 1; i <= 17; i++)
                    summary.Add("0.00");

                //list of headers
                List<string> headers = new List<string>();
                //headers.Add("First Name");
                //headers.Add("Last Name");
                //headers.Add("Time Type");

                //to add dates in headers
                List<DateTime?> dates = new List<DateTime?>();
                while (fromDate <= toDate)
                {
                    if (dates.Count == 7)
                    {
                        headers.Add("Week Totals");
                        //headers.Add("Week Total - OT");
                    }
                    dates.Add(fromDate);
                    headers.Add(Convert.ToDateTime(fromDate).ToString("yyyy-MM-dd"));
                    fromDate = Convert.ToDateTime(fromDate).AddDays(1);
                }

                //adding other columns in header
                headers.Add("Week Totals");
                //headers.Add("Bi-Week Total - OT");
                headers.Add("Bi-Week Total");
                //headers.Add("Pay Period Total - OT");

                //fetching list of users
                var users = _adminMethods.GetUsers().OrderBy(x => x.userName).ToList();
                data.Users = users.AsEnumerable().Select(x => new IBWUsers
                {
                    userId = x.userId,
                    userName = x.userName + " " + x.aliasName
                }).ToList();

                var managers = users.Where(x => x.asRMCount > 0).ToList();
                data.ReportingManagers = managers.AsEnumerable().Select(x => new IBWUsers
                {
                    userId = x.userId,
                    userName = x.userName + " " + x.aliasName
                }).ToList();

                //applying reporting manager filter
                if (managerId != null)
                    users = users.Where(x => x.reportingManagerId == managerId).ToList();

                //applying user filter
                if (UserId != null)
                    users = users.Where(x => x.userId == UserId).ToList();

                //fetching list of approved and reimbursable expenses of all users
                var exp = _schedulingDB.sp_get_pm_expenses().Where(x => x.isApproved == true && x.isReimbursable == "Yes").ToList();
                //fetching the list of holidays
                //var holidays = _adminMethods.GetHolidays();
                //fetching the list of leaves
                //var leaves = _adminDB.GetLeaveRequests().ToList();
                if (users != null && users.Count > 0)
                {
                    foreach (var user in users)
                    {
                        UserPayroll userPayroll = new UserPayroll();

                        userPayroll.UserId = user.userId;
                        userPayroll.FirstName = user.userName;
                        userPayroll.LastName = user.aliasName;
                        //list of regular hours
                        List<string> regular = new List<string>();
                        //list of holiday hours
                        //List<string> holiday = new List<string>();
                        //list of leave hours
                        //List<string> leave = new List<string>();
                        //list of total hours
                        //List<string> total = new List<string>();

                        //variables for calculating totals
                        double? regularTotal = 0.00;
                        double? regularTotal1 = 0.00;
                        double? regularTotal2 = 0.00;

                        //double? holidayTotal = 0.00;
                        //double? holidayTotal1 = 0.00;
                        //double? holidayTotal2 = 0.00;

                        //double? leaveTotal = 0.00;
                        //double? leaveTotal1 = 0.00;
                        //double? leaveTotal2 = 0.00;

                        //double? totalTotal = 0.00;
                        //double? totalTotal1 = 0.00;
                        //double? totalTotal2 = 0.00;

                        //double? ot = 0.00;
                        //double? ot1 = 0.00;
                        //double? ot2 = 0.00;

                        //regular.Add("Regular Hours");
                        //holiday.Add("Statutory Holiday");
                        //leave.Add("Leave");
                        //total.Add("Total");

                        int i = 0;
                        foreach (var date in dates)
                        {
                            //bool isHoliday = false;
                            //bool isLeave = false;
                            //isHoliday = holidays.Where(x => x.holidayDate == date).ToList().Count > 0 ? true : false;
                            //isLeave = leaves.Where(x => x.int_created_by == user.userId && x.is_approved == true && ((x.dt_from_date == date && x.dt_to_date == null) || (x.dt_from_date <= date && x.dt_to_date != null && x.dt_to_date >= date))).ToList().Count > 0 ? true : false;

                            //getting list of approved timesheets for current date
                            var currentDayHours = exp.Where(x => x.userId == user.userId && x.completionDate != "-" && Convert.ToDateTime(x.completionDate) == date).ToList();
                            if (currentDayHours != null && currentDayHours.Count > 0)
                            {
                                //calculating sum of worked hours 
                                double? thisDayExp = currentDayHours.Sum(x => Convert.ToDouble(x.Quantity * x.expenseRate));

                                //calculating summary of all users
                                summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + thisDayExp).ToString("0.00");
                                regular.Add(Convert.ToDouble(thisDayExp).ToString("0.00"));

                                //adding to week 1 total
                                if (regular.Count <= 7)
                                {
                                    regularTotal1 = regularTotal1 + thisDayExp;
                                }
                                //adding to week 2 total
                                else
                                {
                                    regularTotal2 = regularTotal2 + thisDayExp;
                                }

                                ////adding to holiday list if current date is a holiday
                                //if (isHoliday)
                                //{
                                //    regular.Add("0.00");
                                //    holiday.Add(Convert.ToDouble(thisDayExp).ToString("0.00"));
                                //    leave.Add("0.00");
                                //    total.Add(Convert.ToDouble(thisDayExp).ToString("0.00"));
                                //    //adding to week 1 total
                                //    if (regular.Count <= 8)
                                //    {
                                //        holidayTotal1 = holidayTotal1 + thisDayExp;
                                //        totalTotal1 = totalTotal1 + thisDayExp;
                                //    }
                                //    //adding to week 2 total
                                //    else
                                //    {
                                //        holidayTotal2 = holidayTotal2 + thisDayExp;
                                //        totalTotal2 = totalTotal2 + thisDayExp;
                                //    }
                                //}
                                ////adding to leave list if current date is a leave
                                //else if (isLeave)
                                //{
                                //    regular.Add("0.00");
                                //    holiday.Add("0.00");
                                //    leave.Add(Convert.ToDouble(thisDayExp).ToString("0.00"));
                                //    total.Add(Convert.ToDouble(thisDayExp).ToString("0.00"));
                                //    //adding to week 1 total
                                //    if (regular.Count <= 8)
                                //    {
                                //        leaveTotal1 = leaveTotal1 + thisDayExp;
                                //        totalTotal1 = totalTotal1 + thisDayExp;
                                //    }
                                //    //adding to week 2 total
                                //    else
                                //    {
                                //        leaveTotal2 = leaveTotal2 + thisDayExp;
                                //        totalTotal2 = totalTotal2 + thisDayExp;
                                //    }
                                //}
                                ////adding to regular list if current date is neither a holiday nor a leave
                                //else
                                //{
                                //    regular.Add(Convert.ToDouble(thisDayExp).ToString("0.00"));
                                //    holiday.Add("0.00");
                                //    leave.Add("0.00");
                                //    total.Add(Convert.ToDouble(thisDayExp).ToString("0.00"));
                                //    //adding to week 1 total
                                //    if (regular.Count <= 8)
                                //    {
                                //        regularTotal1 = regularTotal1 + thisDayExp;
                                //        totalTotal1 = totalTotal1 + thisDayExp;
                                //    }
                                //    //adding to week 2 total
                                //    else
                                //    {
                                //        regularTotal2 = regularTotal2 + thisDayExp;
                                //        totalTotal2 = totalTotal2 + thisDayExp;
                                //    }
                                //}
                            }
                            else
                            {
                                regular.Add("0.00");
                                //holiday.Add("0.00");
                                //leave.Add("0.00");
                                //total.Add("0.00");
                            }
                            //caculating total hours and OT hours for week 1
                            if (regular.Count == 7)
                            {
                                ////if hours are grater than 44 per week, consider as OT
                                //ot1 = totalTotal1 > 44 ? totalTotal1 - 44 : 0;
                                //totalTotal1 = totalTotal1 > 44 ? 44 : totalTotal1;


                                //calculating summary of all users
                                i++;
                                summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + regularTotal1).ToString("0.00");
                                //i++;
                                //summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + ot1).ToString("0.00");

                                regular.Add(Convert.ToDouble(regularTotal1).ToString("0.00"));
                                //holiday.Add(Convert.ToDouble(holidayTotal1).ToString("0.00"));
                                //leave.Add(Convert.ToDouble(leaveTotal1).ToString("0.00"));
                                //total.Add(Convert.ToDouble((totalTotal1 > 44 ? 44 : totalTotal1)).ToString("0.00"));

                                //regular.Add("-");
                                //holiday.Add("-");
                                //leave.Add("-");
                                //total.Add(Convert.ToDouble(ot1).ToString("0.00"));
                            }
                            //caculating total hours and OT hours for week 2 and sum of w1 and w2
                            if (regular.Count == 15)
                            {
                                ////if hours are grater than 44 per week, consider as OT
                                //ot2 = totalTotal2 > 44 ? totalTotal2 - 44 : 0;
                                //totalTotal2 = totalTotal2 > 44 ? 44 : totalTotal2;

                                //calculating summary of all users
                                i++;
                                summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + regularTotal2).ToString("0.00");
                                //i++;
                                //summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + ot2).ToString("0.00");

                                regular.Add(Convert.ToDouble(regularTotal2).ToString("0.00"));
                                //holiday.Add(Convert.ToDouble(holidayTotal2).ToString("0.00"));
                                //leave.Add(Convert.ToDouble(leaveTotal2).ToString("0.00"));
                                //total.Add(Convert.ToDouble((totalTotal2 > 44 ? 44 : totalTotal2)).ToString("0.00"));

                                //regular.Add("-");
                                //holiday.Add("-");
                                //leave.Add("-");
                                //total.Add(Convert.ToDouble(ot2).ToString("0.00"));

                                regularTotal = regularTotal1 + regularTotal2;
                                //holidayTotal = holidayTotal1 + holidayTotal2;
                                //leaveTotal = leaveTotal1 + leaveTotal2;
                                //totalTotal = totalTotal1 + totalTotal2 > 88 ? 88 : totalTotal1 + totalTotal2;
                                //ot = ot1 + ot2;

                                //calculating summary of all users
                                i++;
                                summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + regularTotal).ToString("0.00");
                                //i++;
                                //summary[i] = Convert.ToDouble(Convert.ToDouble(summary[i]) + ot).ToString("0.00");

                                regular.Add(Convert.ToDouble(regularTotal).ToString("0.00"));
                                //holiday.Add(Convert.ToDouble(holidayTotal).ToString("0.00"));
                                //leave.Add(Convert.ToDouble(leaveTotal).ToString("0.00"));
                                //total.Add(Convert.ToDouble((totalTotal > 44 ? 44 : totalTotal)).ToString("0.00"));

                                //regular.Add("-");
                                //holiday.Add("-");
                                //leave.Add("-");
                                //total.Add(Convert.ToDouble(ot).ToString("0.00"));
                            }
                            i++;
                        }

                        userPayroll.Regular = regular;
                        //userPayroll.Holiday = holiday;
                        //userPayroll.Leave = leave;
                        //userPayroll.Total = total;

                        UserPayroll.Add(userPayroll);
                    }
                }

                data.Headers = headers;
                data.Summary = summary;
                data.UserPayrolls = UserPayroll;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }

        #endregion
    }
}