using API_IBW.DB_Models;
using API_IBW.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace API_IBW.Business_Classes
{
    public class SchedulingMethods
    {
        private SchedulingDataContext _schedulingDB;
        private AdminDataContext _adminDB;
        private Admin _adminMethods;
        private QuotesDataContext _quoteDB;
        private Quotes _quoteMethods;
        private QuotesExtensionDataContext _quotesExtensionDB;
        public SchedulingMethods()
        {
            _schedulingDB = new SchedulingDataContext();
            _adminDB = new AdminDataContext();
            _adminMethods = new Admin();
            _quoteDB = new QuotesDataContext();
            _quoteMethods = new Quotes();
            _quotesExtensionDB = new QuotesExtensionDataContext();
        }

        #region Action Scheduling
        //Author        : Siddhant Chawade
        //Date          : 4th Jan 2020
        //Description   : To get scheduled action activity log
        public List<ScheduledActionActivityLog> GetSecheduledActionActivityLog(long? manageActionId)
        {
            List<ScheduledActionActivityLog> data = new List<ScheduledActionActivityLog>();
            try
            {
                data = _schedulingDB.sp_get_scheduled_action_activity_log(manageActionId).ToList().AsEnumerable().Select(x => new ScheduledActionActivityLog
                {
                    ManageActionId = x.int_manage_action_id,
                    DetailId = x.int_action_detail_id,
                    ScheduledDate = x.dt_scheduled_date,
                    AssignedToUserName = x.vc_assigned_to,
                    PlannedHours = x.dec_planned_hours,
                    AssignedHours = x.dec_assigned_hours,
                    WorkedHours = x.dec_timesheet_worked_hours == null ? x.dec_worked_hours : x.dec_timesheet_worked_hours,
                    RemainingHours = x.dec_timesheet_remaining_hours == null ? x.dec_assigned_hours : x.dec_timesheet_remaining_hours,
                    Status = x.vc_status,
                    CreatedByUserName = x.vc_created_by,
                    CreatedDate = x.dt_created_date
                }).ToList();
                foreach (var item in data)
                {
                    item.WorkedHours = item.WorkedHours == null ? 0 : item.WorkedHours;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }
        #region Grid View Tab
        //Author        : Siddhant Chawade
        //Date          : 30th Mar 2019
        //Description   : To get the list of actions scheduled to a user on a date
        public List<ActionScheduledToUser> ActionScheduledToUser(ActionScheduledToUser inputData)
        {
            List<ActionScheduledToUser> data = new List<ActionScheduledToUser>();
            try
            {
                //getting the list of all scheduled actions
                var list = _schedulingDB.sp_get_actions_scheduled_to_user(inputData.UserId, inputData.ScheduledDate, inputData.TaskId).Where(x => x.isQuoteDeleted == false).OrderBy(x => x.int_sequence).ToList();
                data = list.AsEnumerable().Select(x => new ActionScheduledToUser
                {
                    ActionDetailId = x.int_detail_id,
                    DueDate = x.dt_due_date == null ? "-" : Convert.ToDateTime(x.dt_due_date).ToString("yyyy-MM-dd"),
                    TaskId = x.int_task_id,
                    TaskName = x.vc_task_name,
                    QuoteNo = x.vc_quote_number,
                    AssignedHours = x.dec_assigned_hours,
                    ProjectManager = x.vc_project_manage_name,
                    TimeSheetStatus = x.vc_timesheet_status,
                    ActionDueDateOption = x.vc_due_date_option,
                    ScheduledDate = x.dt_schedule_date,
                    UserId = x.int_assigned_user_id,
                    UserName = x.vc_assigned_user_name
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }
        //Author        : Siddhant Chawade
        //Date          : 2nd Dec 2019
        //Description   : To get the list of actions for scheduling
        public List<ActionScheduling> GetActionSchedulingList(ActionScheduledToUser inputData)
        {
            List<ActionScheduling> data = new List<ActionScheduling>();
            try
            {
                long? taskId = inputData.TaskId;
                var userList = _schedulingDB.sp_get_users_job_code().OrderBy(x => x.vc_user_name).ToList();

                var list = _schedulingDB.sp_get_actions_for_scheduling(taskId).Where(x => x.bt_scheduled == false)
                     .Where(x =>
                    (inputData.FilterForm.QuoteId == null || inputData.FilterForm.QuoteId == x.int_quote_id) &&
                    (inputData.FilterForm.ProjectManagerIds == null || inputData.FilterForm.ProjectManagerIds.Count() == 0 || inputData.FilterForm.ProjectManagerIds.Contains(x.int_project_manager_id)) &&
                    (inputData.FilterForm.JobCodeIds == null || inputData.FilterForm.JobCodeIds.Count() == 0 || inputData.FilterForm.JobCodeIds.Contains(x.int_job_code_id)) &&
                    (inputData.FilterForm.KeyWord == null || JsonConvert.SerializeObject(x).ToLower().Contains(inputData.FilterForm.KeyWord.ToLower()))
                    ).ToList();

                List<long?> itemToRemove = new List<long?>();
                foreach (var item in list)
                {
                    if (item.bt_rescheduled == true && item.dec_scheduled_hours == 0)
                    {
                        itemToRemove.Add(item.int_ibw_manage_action_id);
                    }
                }

                list = list.Where(x => !itemToRemove.Contains(x.int_ibw_manage_action_id)).ToList();

                //list = list.Where(x => x.bt_rescheduled != true && x.dec_scheduled_hours != 0).ToList();
                //var list = _schedulingDB.sp_get_actions_for_scheduling(taskId).Where(x => x.bt_scheduled == false).ToList();

                data = list.AsEnumerable().Select(x => new ActionScheduling
                {
                    ManageActionId = x.int_ibw_manage_action_id,
                    TaskId = x.int_task_id,
                    TaskName = x.vc_task_name,
                    QuoteId = x.int_quote_id,
                    QuoteNo = x.vc_quote_number,
                    SiteId = x.int_site_id,
                    SiteName = x.vc_site_name,
                    SowId = x.int_sow_id,
                    SowName = x.vc_sow_name,
                    ActionId = x.int_action_id,
                    ActionName = x.vc_custom_action_name,
                    JobCodeId = x.int_job_code_id,
                    JobCode = x.vc_job_code,
                    JobCodeTitle = x.vc_job_title,
                    JobCodeStatus = x.bt_job_code_status,
                    PlannedHours = Convert.ToDecimal(x.dec_planned_hours),
                    CreatedDate = x.dt_created_date,
                    DueDate = x.dt_due_date,
                    DueDateOptionId = x.int_due_date_lookup_id,
                    DueDateOption = x.vc_due_date_option,
                    IsRemedialAction = x.bt_isRemedial_action,
                    TeamMemberId = x.int_team_member_id,
                    ProjectManagerId = x.int_project_manager_id,
                    ProjectManagerName = x.vc_project_manage_name,
                    TotalAssignedHours = 0,
                    TotalScheduledHours = x.dec_scheduled_hours,
                    UnScheduled = x.bt_unscheduled,
                    Rescheduled = x.bt_rescheduled,
                    IsProject = x.bt_project,
                    Notes = x.vc_notes,
                    Address = string.IsNullOrEmpty(x.vc_address) ? "-" : x.vc_address,
                    City = string.IsNullOrEmpty(x.vc_city) ? "-" : x.vc_city,
                    IsDueToday = x.dt_due_date != null && Convert.ToDateTime(x.dt_due_date).ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"),
                    DetailsList = list.Where(y => y.int_ibw_manage_action_id == x.int_ibw_manage_action_id).AsEnumerable().Select(y => new ActionSchedulingDetails
                    {
                        IsDuplicate = false,
                        AssignedHours = (dynamic)null,
                        AssignedUser = (dynamic)null,
                        ScheduleDate = (dynamic)null,
                        AssignUserList = userList.Where(z => z.int_job_code_master_id == x.int_job_code_id).ToList().AsEnumerable().Select(z => new AssignUser
                        {
                            UserId = z.int_user_id,
                            UserName = z.vc_user_name,
                            Status = z.bt_status
                        }).ToList()
                    }).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data.OrderByDescending(x => x.DueDate).ToList();
        }
        //Author        : Siddhant Chawade
        //Date          : 28th Jan 2020
        //Description   : To delete manage action
        public long? DeleteManageAction(long? manageActionId)
        {
            long? result = (dynamic)null;
            try
            {
                _schedulingDB.sp_delete_manage_action(manageActionId, ref result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        //Author        : Siddhant Chawade
        //Date          : 6th Dec 2019
        //Description   : To upsert due date of action
        public long? UpsertAcionDueDdate(ActionScheduling data)
        {
            long? result = (dynamic)null;
            try
            {
                _schedulingDB.sp_upsert_action_due_date(data.ManageActionId, data.DueDate, data.DueDateOptionId, data.IsRemedialAction, data.TeamMemberId, ref result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        //Author        : Siddhant Chawade
        //Date          : 7th Dec 2019
        //Description   : To save scheduled action details
        public long? SaveScheduledAcionDetails(List<ActionScheduling> data, long? createdBy)
        {
            long? result = (dynamic)null;
            try
            {
                if (data != null && data.Count > 0)
                {
                    foreach (var action in data)
                    {
                        if (action.DetailsList != null && action.DetailsList.Count > 0)
                        {
                            decimal? remaininghours = action.PlannedHours;
                            foreach (var detail in action.DetailsList)
                            {
                                _schedulingDB.sp_save_scheduled_action_details(action.ManageActionId, detail.ScheduleDate, detail.AssignedHours, detail.AssignedUser, createdBy, ref result);

                                //inserting activity log
                                if (remaininghours == action.PlannedHours)
                                    remaininghours = action.PlannedHours - detail.AssignedHours;
                                else
                                    remaininghours = remaininghours - detail.AssignedHours;
                                _schedulingDB.sp_insert_scheduled_action_activity_log(action.ManageActionId, result, detail.ScheduleDate, detail.AssignedUser, action.PlannedHours,
                                    detail.AssignedHours, 0, remaininghours, "Scheduled", createdBy);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }


        //Author        : Siddhant Chawade
        //Date          : 2nd Dec 2019
        //Description   : To get the list of actions for scheduling
        public List<ActionScheduledToUser> GetActionScheduledToUser(long? userId, DateTime? scheduledDate, long? taskId)
        {
            List<ActionScheduledToUser> data = new List<ActionScheduledToUser>();
            try
            {
                var list = _schedulingDB.sp_get_actions_scheduled_to_user(userId, scheduledDate, taskId).ToList();
                data = list.AsEnumerable().Select(x => new ActionScheduledToUser
                {
                    ActionId = x.int_ibw_manage_action_id,
                    ActionDetailId = x.int_detail_id,
                    UserId = x.int_assigned_user_id,
                    UserName = x.vc_assigned_user_name,
                    TaskName = x.vc_task_name,
                    ScheduledDate = x.dt_schedule_date,
                    QuoteNo = x.vc_quote_number,
                    SiteName = x.vc_site_name,
                    SowName = x.vc_sow_name,
                    ActionName = x.vc_custom_action_name,
                    JobCode = x.vc_job_code,
                    JobCodeTitle = x.vc_job_title,
                    PlannedHours = x.dec_planned_hours,
                    AssignedHours = x.dec_assigned_hours,
                    WorkedHours = x.int_worked_hours,
                    RemainingHours = x.dec_remaining_hours == null ? x.dec_assigned_hours : x.dec_remaining_hours,
                    CompletionDate = (dynamic)null
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }
        //Author        : Siddhant Chawade
        //Date          : 17th Dec 2019
        //Description   : To get the history of actions 
        public List<ActionHistory> GetActionHistory(ActionHistory actionHistory)
        {
            List<ActionHistory> data = new List<ActionHistory>();
            try
            {
                var list = _schedulingDB.sp_get_actions_history(actionHistory.ManageActionId).ToList();
                data = list.AsEnumerable().Select(x => new ActionHistory
                {
                    JobCode = x.vc_job_code,
                    JobCodeTitle = x.vc_job_title,
                    AssignedToUserName = x.vc_assigned_user,
                    PlannedHours = x.dec_planned_hours,
                    AssignedHours = x.dec_assigned_hours,
                    WorkedHours = x.dec_worked_hours == null ? 0 : x.dec_worked_hours,
                    RemainingHours = x.dec_remaining_hours == null ? x.dec_assigned_hours : x.dec_remaining_hours,
                    AssignedDate = x.dt_assigned_date,
                    //CompletionDate = x.dt_completion_date,
                    //Status = x.vc_status
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }
        #endregion

        #region Board Tab
        //Author        : Siddhant Chawade
        //Date          : 2nd Mar 2020
        //Description   : To update sequence of scehduled actions
        public long? UpdateScheduledActionsSequence(List<long?> data)
        {
            long? result = (dynamic)null;
            try
            {
                int sequence = 1;
                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        _schedulingDB.sp_update_scheduled_actions_sequence(item, sequence, ref result);
                        sequence++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        //Author        : Siddhant Chawade
        //Date          : 10th Dec 2019
        //Description   : To get the list of scheduled actions
        public List<ScheduledAction> GetScheduledActions(ActionScheduledToUser inputData)
        {
            List<ScheduledAction> data = new List<ScheduledAction>();
            List<AssignUser> Users = new List<AssignUser>();
            List<long?> UserIds = new List<long?>();
            List<DateTime> Dates = new List<DateTime>();
            DateTime today = DateTime.Today;
            DateTime currentDay = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            DateTime sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Sunday);
            DateTime saturday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday);
            bool actionExistsOnDate = false;
            long? taskId = inputData.TaskId;
            int viewType = inputData.ViewType;
            try
            {
                //for day view
                if (viewType == 1)
                {
                    Dates.Add(today);
                }
                //for this week view
                else if (viewType == 2)
                {
                    today = sunday;
                }
                //for last week view
                else if (viewType == 3)
                {
                    today = sunday.AddDays(-7);
                    saturday = saturday.AddDays(-7);
                }
                //for next week view
                else if (viewType == 4)
                {
                    today = sunday.AddDays(7);
                    saturday = saturday.AddDays(7);
                }
                if (viewType != 1)
                {
                    while (today <= saturday)
                    {
                        Dates.Add(today);
                        today = today.AddDays(1);
                    }
                }
                if (inputData.FromDate != null && inputData.ToDate != null)
                {
                    Dates = new List<DateTime>();
                    while (inputData.FromDate <= inputData.ToDate)
                    {
                        Dates.Add(Convert.ToDateTime(inputData.FromDate));
                        inputData.FromDate = Convert.ToDateTime(inputData.FromDate).AddDays(1);
                    }
                }
                //getting the list of all scheduled actions
                var list = _schedulingDB.sp_get_actions_scheduled_to_user(null, null, null)
                    .Where(x =>
                    (inputData.FilterForm.QuoteId == null || inputData.FilterForm.QuoteId == x.int_quote_id) &&
                    (inputData.FilterForm.ProjectManagerIds == null || inputData.FilterForm.ProjectManagerIds.Count == 0 || inputData.FilterForm.ProjectManagerIds.Contains(x.int_project_manager_id)) &&
                    (inputData.FilterForm.JobCodeIds == null || inputData.FilterForm.JobCodeIds.Count() == 0 || inputData.FilterForm.JobCodeIds.Contains(x.int_job_code_master_id)) &&
                    (inputData.FilterForm.KeyWord == null || JsonConvert.SerializeObject(x).ToLower().Contains(inputData.FilterForm.KeyWord.ToLower()))
                    ).ToList();
                if (list != null && list.Count > 0)
                {
                    //fetching the list of users from the scheduled actions
                    Users = list.Select(x => new AssignUser
                    {
                        UserId = x.int_assigned_user_id,
                        UserName = x.vc_assigned_user_name
                    }).ToList();
                    foreach (var user in Users.OrderBy(x => x.UserName).ToList())
                    {
                        actionExistsOnDate = false;
                        //to check if the user is already added to the list
                        if (!UserIds.Contains(user.UserId))
                        {
                            UserIds.Add(user.UserId);

                            decimal? totalHours = 0;
                            ScheduledAction scheduledAction = new ScheduledAction();
                            List<ActionScheduledOnDate> actionScheduledOnDateList = new List<ActionScheduledOnDate>();
                            scheduledAction.UserId = user.UserId;
                            scheduledAction.UserName = user.UserName;

                            //to get the list of scheduled actions assigned to user
                            var userActions = new List<sp_get_actions_scheduled_to_userResult>();
                            var currentTaskActionCount = list.Where(x => x.int_assigned_user_id == user.UserId && x.int_task_id == taskId && Dates.Contains(Convert.ToDateTime(x.dt_schedule_date))).ToList().Count;
                            if (currentTaskActionCount > 0)
                            {
                                userActions = list.Where(x => x.int_assigned_user_id == user.UserId).OrderBy(x => x.int_sequence).ToList();
                            }
                            else
                            {
                                userActions = list.Where(x => x.int_assigned_user_id == user.UserId && x.int_task_id == taskId).OrderBy(x => x.int_sequence).ToList();
                            }

                            if (userActions != null && userActions.Count > 0)
                            {
                                foreach (var date in Dates)
                                {
                                    totalHours = 0;
                                    ActionScheduledOnDate actionScheduledOnDate = new ActionScheduledOnDate();
                                    actionScheduledOnDate.Date = date;
                                    actionScheduledOnDate.strDate = date.DayOfWeek.ToString().Substring(0, 3) + " | " + Convert.ToDateTime(date).ToString("MM-dd");
                                    List<ActionScheduledToUser> userActionsList = new List<ActionScheduledToUser>();
                                    //to get the list of scheduled actions assigned to user and scheduled for this date
                                    var userActionsonDate = userActions.Where(x => x.dt_schedule_date == date).ToList();
                                    foreach (var action in userActionsonDate)
                                    {
                                        actionExistsOnDate = true;
                                        ActionScheduledToUser userAction = new ActionScheduledToUser();
                                        userAction.ActionId = action.int_ibw_manage_action_id;
                                        userAction.ActionDetailId = action.int_detail_id;
                                        userAction.QuoteNo = action.vc_quote_number;
                                        userAction.AssignedHours = action.dec_assigned_hours;
                                        userAction.ActionDueDateOption = action.vc_due_date_option;
                                        totalHours = totalHours + action.dec_assigned_hours;
                                        userAction.TimeSheetStatus = action.vc_timesheet_status;
                                        userAction.TaskName = action.vc_task_name;
                                        userAction.DueDate = action.dt_due_date == null ? "-" : Convert.ToDateTime(action.dt_due_date).ToString("yyyy-MM-dd");
                                        userAction.ProjectManager = action.vc_project_manage_name;
                                        userAction.Sequence = action.int_sequence;
                                        //adding action of list of actions of a user scheduled for a date
                                        userActionsList.Add(userAction);
                                    }
                                    actionScheduledOnDate.ActionsList = userActionsList.OrderBy(x => x.Sequence).ToList();
                                    actionScheduledOnDate.TotalHours = totalHours;
                                    actionScheduledOnDateList.Add(actionScheduledOnDate);
                                }
                            }
                            scheduledAction.ActionScheduledOnDate = actionScheduledOnDateList;
                            if (actionExistsOnDate)
                                data.Add(scheduledAction);
                        }
                    }
                }
                if (data.Count == 0)
                {
                    ScheduledAction scheduledAction = new ScheduledAction();
                    List<ActionScheduledOnDate> actionScheduledOnDateList = new List<ActionScheduledOnDate>();
                    scheduledAction.UserId = 0;
                    scheduledAction.UserName = string.Empty;
                    foreach (var date in Dates)
                    {
                        ActionScheduledOnDate actionScheduledOnDate = new ActionScheduledOnDate();
                        actionScheduledOnDate.Date = date;
                        actionScheduledOnDate.strDate = date.DayOfWeek.ToString().Substring(0, 3) + " | " + Convert.ToDateTime(date).ToString("MM-dd");
                        actionScheduledOnDateList.Add(actionScheduledOnDate);
                    }
                    scheduledAction.ActionScheduledOnDate = actionScheduledOnDateList;
                    data.Add(scheduledAction);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }

        //Author        : Siddhant Chawade
        //Date          : 11th Dec 2019
        //Description   : To get the details of scheduled actions
        public ScheduledActionDetail GetScheduledActionDetails(long? detailId, long? manageActionId)
        {
            ScheduledActionDetail data = new ScheduledActionDetail();
            try
            {
                var userList = _schedulingDB.sp_get_users_job_code().OrderBy(x => x.vc_user_name).ToList();
                var detail = _schedulingDB.sp_get_scheduled_actions_details_by_manage_action_id(manageActionId).FirstOrDefault();
                var details = _schedulingDB.sp_get_scheduled_actions_details(detailId).FirstOrDefault();
                var status = detailId == null ? "Unscheduled" : "Scheduled";
                if (details != null || detail != null)
                {
                    var log = _schedulingDB.sp_get_scheduled_action_activity_log(manageActionId == null ? details.int_ibw_manage_action_id : manageActionId).Where(x => x.int_action_detail_id == detailId).OrderByDescending(x => x.int_log_id).FirstOrDefault();
                    if (log != null)
                        status = log.vc_status;
                    data.ManageActionId = details == null ? detail.int_ibw_manage_action_id : details.int_ibw_manage_action_id;
                    data.ActionDetailId = details == null ? 0 : details.int_detail_id;
                    data.QuoteNo = details == null ? detail.vc_quote_number : details.vc_quote_number;
                    data.ProjectManagerName = details == null ? detail.vc_project_manage_name : details.vc_project_manage_name;
                    data.SiteName = details == null ? detail.vc_site_name : details.vc_site_name;
                    data.SowName = details == null ? detail.vc_sow_name : details.vc_sow_name;
                    data.TaskName = details == null ? detail.vc_task_name : details.vc_task_name;
                    data.ActionName = details == null ? detail.vc_custom_action_name : details.vc_custom_action_name;
                    data.JobCode = details == null ? detail.vc_job_code : details.vc_job_code;
                    data.JobCodeTitle = details == null ? detail.vc_job_title : details.vc_job_title;
                    data.PlannedHours = details == null ? detail.dec_planned_hours : details.dec_planned_hours;
                    data.AssignedHours = details == null ? detail.dec_assigned_hours : details.dec_assigned_hours;
                    data.WorkedHours = details == null ? detail.dec_worked_hours : (details.dec_worked_hours == null ? 0 : details.dec_worked_hours);
                    data.RecDate = details == null ? Convert.ToDateTime(detail.dt_rec_date).ToString("yyyy-MM-dd, hh:mm tt") : Convert.ToDateTime(details.dt_rec_date).ToString("yyyy-MM-dd, hh:mm tt");
                    data.DueDate = details == null ? detail.dt_due_date : details.dt_due_date;
                    data.ScheduleDate = details == null ? null : details.dt_schedule_date;
                    data.AssignedToUserId = details == null ? null : details.int_assigned_user_id;
                    data.AssignedToUserName = details == null ? "-" : details.vc_assigned_user;
                    data.AssignedOnDate = details == null ? null : details.dt_assigned_on_date;
                    data.AssignedByUserName = details == null ? "-" : details.vc_assigned_by;
                    data.Status = status;
                    data.Notes = details == null ? detail.vc_notes : details.vc_notes;
                    data.IsProject = details == null ? detail.bt_project : details.bt_project;
                    data.QuoteId = details == null ? detail.int_quote_id : details.int_quote_id;
                    data.CreatedBy = details == null ? detail.vc_created_by : details.vc_created_by;
                    data.AssignUserList = userList.Where(z => z.int_job_code_master_id == (details == null ? detail.int_job_code_master_id : details.int_job_code_master_id)).ToList().AsEnumerable().Select(z => new AssignUser
                    {
                        UserId = z.int_user_id,
                        UserName = z.vc_user_name,
                        Status = z.bt_status
                    }).ToList();
                    data.ActionComments = _schedulingDB.sp_get_scheduled_actions_comments(detailId).ToList().AsEnumerable().Select(x => new ActionComment
                    {
                        ActionCommentId = x.int_comment_id,
                        ActionDetailId = x.int_action_detail_id,
                        Comment = x.vc_comment,
                        CreatedById = x.int_created_by,
                        CretedByName = x.vc_created_by,
                        CreatedDate = x.dt_created_date,
                        UpdatedById = x.int_updated_by,
                        UpdatedByName = x.vc_updated_by,
                        UpdatedDate = x.dt_updated_date
                    }).ToList();
                    data.SplitWithUsers = _schedulingDB.sp_get_action_split_users(detailId).ToList().AsEnumerable().Select(x => new AssignUser
                    {
                        userAssignedHours = x.dec_assigned_hours,
                        UserName = x.vc_assigned_user_name
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }
        //Author        : Siddhant Chawade
        //Date          : 11th Dec 2019
        //Description   : To rescheduled actions 
        public long? RecheduledAction(ScheduledActionDetail data)
        {
            long? result = (dynamic)null;
            try
            {
                _schedulingDB.sp_reschedule_action(data.ActionDetailId, data.ScheduleDate, data.AssignedToUserId, data.AssignedByUserId, ref result);

                //inserting activity log
                var manageActionId = _schedulingDB.sp_get_actions_scheduled_to_user(null, null, null).Where(x => x.int_detail_id == data.ActionDetailId).FirstOrDefault().int_ibw_manage_action_id;
                var lastLog = _schedulingDB.sp_get_scheduled_action_activity_log(manageActionId).Where(x => x.int_action_detail_id == data.ActionDetailId).OrderByDescending(x => x.int_log_id).ToList().FirstOrDefault();
                if (lastLog != null)
                {
                    _schedulingDB.sp_insert_scheduled_action_activity_log(manageActionId, data.ActionDetailId, data.ScheduleDate, data.AssignedToUserId, lastLog.dec_planned_hours,
                        lastLog.dec_assigned_hours, lastLog.dec_worked_hours, lastLog.dec_remaining_hours, "Rescheduled", data.AssignedByUserId);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        //Author        : Siddhant Chawade
        //Date          : 11th Dec 2019
        //Description   : To upsert scheduled actions comment
        public long? UpsertScheduledActionComment(ActionComment data)
        {
            long? result = (dynamic)null;
            try
            {
                _schedulingDB.sp_upsert_scheduled_actions_comment(data.ActionCommentId, data.ActionDetailId, data.Comment, data.CreatedById, ref result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        //Author        : Siddhant Chawade
        //Date          : 11th Dec 2019
        //Description   : To delete scheduled actions comment
        public long? DeleteScheduledActionComment(ActionComment data)
        {
            long? result = (dynamic)null;
            try
            {
                _schedulingDB.sp_delete_scheduled_actions_comment(data.ActionCommentId, ref result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        //Author        : Siddhant Chawade
        //Date          : 18th Dec 2019
        //Description   : To deassign scheduled actions 
        public long? DeassignScheduledAction(ActionScheduledToUser data, long? createdBy)
        {
            long? result = (dynamic)null;
            try
            {
                //inserting activity log
                var manageActionId = _schedulingDB.sp_get_actions_scheduled_to_user(null, null, null).Where(x => x.int_detail_id == data.ActionDetailId).FirstOrDefault().int_ibw_manage_action_id;
                var lastLog = _schedulingDB.sp_get_scheduled_action_activity_log(manageActionId).Where(x => x.int_action_detail_id == data.ActionDetailId).OrderByDescending(x => x.int_log_id).ToList().FirstOrDefault();
                if (lastLog != null)
                {
                    _schedulingDB.sp_insert_scheduled_action_activity_log(manageActionId, data.ActionDetailId, lastLog.dt_scheduled_date, lastLog.int_assigned_to, lastLog.dec_planned_hours,
                        lastLog.dec_assigned_hours, lastLog.dec_worked_hours, lastLog.dec_remaining_hours, "Unassigned", createdBy);
                }

                _schedulingDB.sp_deassign_scheduled_action(data.ActionDetailId, ref result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        #endregion

        #region Summary Tab

        //Author        : Siddhant Chawade
        //Date          : 9th Dec 2019
        //Description   : To get the list of actions scheduled (summary)
        public List<ScheduledActionsSummary> GetScheduledActionsSummary(long? userId, DateTime? scheduledDate, ActionScheduledToUser inputData)
        {
            List<ScheduledActionsSummary> data = new List<ScheduledActionsSummary>();
            List<AssignUser> Users = new List<AssignUser>();
            List<long?> UserIds = new List<long?>();
            List<DateTime?> Dates = new List<DateTime?>();
            DateTime today = DateTime.Today;
            DateTime currentDay = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            DateTime sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Sunday);
            DateTime saturday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday);
            bool actionExistsOnDate = false;
            long? taskId = inputData.TaskId;
            int viewType = inputData.ViewType;
            try
            {
                //for this week view
                if (viewType == 2)
                {
                    today = sunday;
                }
                //for last week view
                else if (viewType == 3)
                {
                    today = sunday.AddDays(-7);
                    saturday = saturday.AddDays(-7);
                }
                //for next week view
                else if (viewType == 4)
                {
                    today = sunday.AddDays(7);
                    saturday = saturday.AddDays(7);
                }
                if (viewType != 1)
                {
                    while (today <= saturday)
                    {
                        Dates.Add(today);
                        today = today.AddDays(1);
                    }
                }
                var list = _schedulingDB.sp_get_actions_scheduled_to_user(userId, scheduledDate, taskId)
                    .Where(x =>
                    (inputData.FilterForm.QuoteId == null || inputData.FilterForm.QuoteId == x.int_quote_id) &&
                    (inputData.FilterForm.ProjectManagerIds == null || inputData.FilterForm.ProjectManagerIds.Count() == 0 || inputData.FilterForm.ProjectManagerIds.Contains(x.int_project_manager_id)) &&
                    (inputData.FilterForm.JobCodeIds == null || inputData.FilterForm.JobCodeIds.Count() == 0 || inputData.FilterForm.JobCodeIds.Contains(x.int_job_code_master_id)) &&
                    (inputData.FilterForm.KeyWord == null || JsonConvert.SerializeObject(x).ToLower().Contains(inputData.FilterForm.KeyWord.ToLower()))
                    ).ToList();
                if (list != null && list.Count > 0)
                {
                    Users = list.Select(x => new AssignUser
                    {
                        UserId = x.int_assigned_user_id,
                        UserName = x.vc_assigned_user_name
                    }).ToList();
                    foreach (var user in Users)
                    {
                        if (!UserIds.Contains(user.UserId))
                        {
                            actionExistsOnDate = false;
                            UserIds.Add(user.UserId);
                            decimal? mondayCount = 0;
                            decimal? tuesdayCount = 0;
                            decimal? wednesdayCount = 0;
                            decimal? thrusdayCount = 0;
                            decimal? fridayCount = 0;
                            decimal? saturdayCount = 0;
                            decimal? sundayCount = 0;
                            decimal? totalCount = 0;
                            ScheduledActionsSummary summary = new ScheduledActionsSummary();
                            var userActions = list.Where(x => x.int_assigned_user_id == user.UserId && Dates.Contains(x.dt_schedule_date)).ToList();
                            if (userActions != null && userActions.Count > 0)
                            {
                                foreach (var action in userActions)
                                {
                                    actionExistsOnDate = true;
                                    var day = Convert.ToDateTime(action.dt_schedule_date).DayOfWeek;
                                    if (day == DayOfWeek.Sunday)
                                        sundayCount = sundayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Monday)
                                        mondayCount = mondayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Tuesday)
                                        tuesdayCount = tuesdayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Wednesday)
                                        wednesdayCount = wednesdayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Thursday)
                                        thrusdayCount = thrusdayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Friday)
                                        fridayCount = fridayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Saturday)
                                        saturdayCount = saturdayCount + action.dec_assigned_hours;

                                    totalCount = totalCount + action.dec_assigned_hours;
                                }
                            }
                            summary.UserName = user.UserName;
                            summary.MondayCount = mondayCount;
                            summary.TuesdayCount = tuesdayCount;
                            summary.WednesdayCount = wednesdayCount;
                            summary.ThrusdayCount = thrusdayCount;
                            summary.FridayCount = fridayCount;
                            summary.SaturdayCount = saturdayCount;
                            summary.SundayCount = sundayCount;
                            summary.TotalCount = totalCount;
                            if (actionExistsOnDate)
                                data.Add(summary);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }


        #endregion

        #endregion
        #region Dashboard
        //Author        : Anji
        //Date          : 12th Dec 2019
        //Description   : To get the list of scheduled actions
        public List<ScheduledAction> GetScheduledActionsOfUser(long? userId, int viewType, DateTime? fromDate, DateTime? toDate)
        {
            List<ScheduledAction> data = new List<ScheduledAction>();
            List<AssignUser> Users = new List<AssignUser>();
            List<long?> UserIds = new List<long?>();
            List<DateTime> Dates = new List<DateTime>();
            DateTime today = DateTime.Today;
            DateTime currentDay = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            DateTime sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime saturday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 7);
            bool actionExistsOnDate = false;
            try
            {
                //for day view
                if (viewType == 1)
                {
                    Dates.Add(today);
                }
                //for this week view
                else if (viewType == 2)
                {
                    today = sunday;
                }
                //for last week view
                else if (viewType == 3)
                {
                    today = sunday.AddDays(-7);
                    saturday = saturday.AddDays(-7);
                }
                //for next week view
                else if (viewType == 4)
                {
                    today = sunday.AddDays(7);
                    saturday = saturday.AddDays(7);
                }
                if (viewType != 1)
                {
                    while (today <= saturday)
                    {
                        Dates.Add(today);
                        today = today.AddDays(1);
                    }
                }
                if (fromDate != null && toDate != null)
                {
                    Dates = new List<DateTime>();
                    while (fromDate <= toDate)
                    {
                        Dates.Add(Convert.ToDateTime(fromDate));
                        fromDate = Convert.ToDateTime(fromDate).AddDays(1);
                    }
                }
                //getting the list of all scheduled actions
                var list = _schedulingDB.sp_get_actions_scheduled_to_user(userId, null, null).Where(x => x.isQuoteDeleted == false).ToList();
                if (list != null && list.Count > 0)
                {
                    //fetching the list of users from the scheduled actions
                    Users = list.Select(x => new AssignUser
                    {
                        UserId = x.int_assigned_user_id,
                        UserName = x.vc_assigned_user_name
                    }).ToList();
                    foreach (var user in Users)
                    {
                        actionExistsOnDate = false;
                        //to check if the user is already added to the list
                        if (!UserIds.Contains(user.UserId))
                        {
                            UserIds.Add(user.UserId);

                            decimal? totalHours = 0;
                            ScheduledAction scheduledAction = new ScheduledAction();
                            List<ActionScheduledOnDate> actionScheduledOnDateList = new List<ActionScheduledOnDate>();
                            scheduledAction.UserId = user.UserId;
                            scheduledAction.UserName = user.UserName;

                            //to get the list of scheduled actions assigned to user
                            //var userActions = list.Where(x => x.int_assigned_user_id == user.UserId).ToList();
                            var userActions = list.Where(x => x.int_assigned_user_id == user.UserId).OrderBy(x => x.int_sequence).ToList();
                            if (userActions != null && userActions.Count > 0)
                            {
                                foreach (var date in Dates)
                                {
                                    totalHours = 0;
                                    ActionScheduledOnDate actionScheduledOnDate = new ActionScheduledOnDate();
                                    actionScheduledOnDate.Date = date;
                                    actionScheduledOnDate.strDate = date.DayOfWeek.ToString().Substring(0, 3) + " | " + Convert.ToDateTime(date).ToString("MM-dd");
                                    List<ActionScheduledToUser> userActionsList = new List<ActionScheduledToUser>();
                                    //to get the list of scheduled actions assigned to user and scheduled for this date
                                    var userActionsonDate = userActions.Where(x => x.dt_schedule_date == date).ToList();
                                    foreach (var action in userActionsonDate)
                                    {
                                        actionExistsOnDate = true;
                                        ActionScheduledToUser userAction = new ActionScheduledToUser();
                                        userAction.ActionId = action.int_ibw_manage_action_id;
                                        userAction.ActionDetailId = action.int_detail_id;
                                        userAction.QuoteNo = action.vc_quote_number;
                                        userAction.AssignedHours = action.dec_assigned_hours;
                                        userAction.ActionDueDateOption = action.vc_due_date_option;
                                        userAction.TimeSheetStatus = action.vc_timesheet_status;
                                        totalHours = totalHours + action.dec_assigned_hours;
                                        userAction.DueDate = Convert.ToDateTime(action.dt_due_date).ToString("yyyy-MM-dd");
                                        userAction.TaskName = action.vc_task_name;
                                        userAction.ProjectManager = action.vc_project_manage_name;
                                        //adding action of list of actions of a user scheduled for a date
                                        userActionsList.Add(userAction);
                                    }
                                    actionScheduledOnDate.ActionsList = userActionsList;
                                    actionScheduledOnDate.TotalHours = totalHours;
                                    actionScheduledOnDateList.Add(actionScheduledOnDate);
                                }
                            }
                            scheduledAction.ActionScheduledOnDate = actionScheduledOnDateList;
                            if (actionExistsOnDate)
                                data.Add(scheduledAction);
                        }
                    }
                }
                if (data.Count == 0)
                {
                    ScheduledAction scheduledAction = new ScheduledAction();
                    List<ActionScheduledOnDate> actionScheduledOnDateList = new List<ActionScheduledOnDate>();
                    scheduledAction.UserId = 0;
                    scheduledAction.UserName = string.Empty;
                    foreach (var date in Dates)
                    {
                        ActionScheduledOnDate actionScheduledOnDate = new ActionScheduledOnDate();
                        actionScheduledOnDate.Date = date;
                        actionScheduledOnDate.strDate = date.DayOfWeek.ToString().Substring(0, 3) + " | " + Convert.ToDateTime(date).ToString("MM-dd");
                        actionScheduledOnDateList.Add(actionScheduledOnDate);
                    }
                    scheduledAction.ActionScheduledOnDate = actionScheduledOnDateList;
                    data.Add(scheduledAction);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }
        //Author        : Anji
        //Date          : 12th Dec 2019
        //Description   : To get the list of scheduled actions
        public List<sp_get_actions_scheduled_to_userResult> GetScheduledActionsOfUserForMap(long? userId, int viewType, DateTime? scheduledDate)
        {
            List<DateTime> Dates = new List<DateTime>();
            DateTime today = DateTime.Today;
            DateTime currentDay = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            DateTime sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime saturday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 7);
            List<sp_get_actions_scheduled_to_userResult> actionsList = new List<sp_get_actions_scheduled_to_userResult>();
            try
            {
                //for day view
                if (viewType == 1)
                {
                    Dates.Add(today);
                }
                //for this week view
                else if (viewType == 2)
                {
                    today = sunday;
                }
                //for last week view
                else if (viewType == 3)
                {
                    today = sunday.AddDays(-7);
                    saturday = saturday.AddDays(-7);
                }
                //for next week view
                else if (viewType == 4)
                {
                    today = sunday.AddDays(7);
                    saturday = saturday.AddDays(7);
                }
                if (viewType != 1)
                {
                    while (today <= saturday)
                    {
                        Dates.Add(today);
                        today = today.AddDays(1);
                    }
                }
                //getting the list of all scheduled actions
                actionsList = _schedulingDB.sp_get_actions_scheduled_to_user(userId, null, null).Where(x => x.isQuoteDeleted == false).ToList();
                if (viewType != 0 && actionsList != null && actionsList.Count > 0)
                    actionsList = actionsList.Where(x => x.dt_schedule_date != null && Dates.Contains(x.dt_schedule_date.Value)).ToList();
                if (viewType == 0 && actionsList != null && actionsList.Count > 0)
                    actionsList = _schedulingDB.sp_get_actions_scheduled_to_user(userId, scheduledDate, null).Where(x => x.isQuoteDeleted == false).ToList();
                actionsList = _schedulingDB.sp_get_actions_scheduled_to_user(userId, scheduledDate, null).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return actionsList;
        }
        //Author        : Anji
        //Date          : 12th Dec 2019
        //Description   : To get the list of actions scheduled (summary)
        public List<ScheduledActionsSummary> GetUserScheduledActionsSummary(long? userId, int viewType, DateTime? scheduledDate, long? taskId)
        {
            List<DateTime> Dates = new List<DateTime>();
            DateTime today = DateTime.Today;
            DateTime currentDay = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            DateTime monday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 7);
            //for day view
            if (viewType == 1)
            {
                Dates.Add(today);
            }
            //for this week view
            else if (viewType == 2)
            {
                today = monday;
            }
            //for last week view
            else if (viewType == 3)
            {
                today = monday.AddDays(-7);
                sunday = sunday.AddDays(-7);
            }
            //for next week view
            else if (viewType == 4)
            {
                today = monday.AddDays(7);
                sunday = sunday.AddDays(7);
            }
            if (viewType != 1)
            {
                while (today <= sunday)
                {
                    Dates.Add(today);
                    today = today.AddDays(1);
                }
            }

            List<ScheduledActionsSummary> data = new List<ScheduledActionsSummary>();
            List<AssignUser> Users = new List<AssignUser>();
            List<long?> UserIds = new List<long?>();
            try
            {
                var list = _schedulingDB.sp_get_actions_scheduled_to_user(userId, scheduledDate, taskId).Where(x => x.isQuoteDeleted == false).ToList();
                if (list != null && list.Count > 0)
                {
                    Users = list.Select(x => new AssignUser
                    {
                        UserId = x.int_assigned_user_id,
                        UserName = x.vc_assigned_user_name
                    }).ToList();
                    foreach (var user in Users)
                    {
                        if (!UserIds.Contains(user.UserId))
                        {
                            UserIds.Add(user.UserId);
                            decimal? mondayCount = 0;
                            decimal? tuesdayCount = 0;
                            decimal? wednesdayCount = 0;
                            decimal? thrusdayCount = 0;
                            decimal? fridayCount = 0;
                            decimal? saturdayCount = 0;
                            decimal? sundayCount = 0;
                            decimal? totalCount = 0;
                            ScheduledActionsSummary summary = new ScheduledActionsSummary();
                            //var userActions = list.Where(x => x.int_assigned_user_id == user.UserId && (x.dt_schedule_date >= monday && x.dt_schedule_date <= sunday)).ToList();
                            var userActions = list.Where(x => x.int_assigned_user_id == user.UserId && x.dt_schedule_date != null && Dates.Contains(x.dt_schedule_date.Value)).ToList();
                            if (userActions != null && userActions.Count > 0)
                            {
                                foreach (var action in userActions)
                                {
                                    var day = Convert.ToDateTime(action.dt_schedule_date).DayOfWeek;
                                    if (day == DayOfWeek.Sunday)
                                        sundayCount = sundayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Monday)
                                        mondayCount = mondayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Tuesday)
                                        tuesdayCount = tuesdayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Wednesday)
                                        wednesdayCount = wednesdayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Thursday)
                                        thrusdayCount = thrusdayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Friday)
                                        fridayCount = fridayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Saturday)
                                        saturdayCount = saturdayCount + action.dec_assigned_hours;

                                    totalCount = totalCount + action.dec_assigned_hours;
                                }
                            }
                            summary.UserName = user.UserName;
                            summary.MondayCount = mondayCount;
                            summary.TuesdayCount = tuesdayCount;
                            summary.WednesdayCount = wednesdayCount;
                            summary.ThrusdayCount = thrusdayCount;
                            summary.FridayCount = fridayCount;
                            summary.SaturdayCount = saturdayCount;
                            summary.SundayCount = sundayCount;
                            summary.TotalCount = totalCount;
                            data.Add(summary);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }
        //Author        : Anji
        //Date          : 13th Dec 2019
        //Description   : To get the list of actions scheduled (summary)
        public List<ScheduledActionsSummary> GetPMScheduledActionsSummary(int viewType, long? projectManagerId)
        {
            List<DateTime> Dates = new List<DateTime>();
            DateTime today = DateTime.Today;
            DateTime currentDay = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            DateTime monday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 7);
            //for day view
            if (viewType == 1)
            {
                Dates.Add(today);
            }
            //for this week view
            else if (viewType == 2)
            {
                today = monday;
            }
            //for last week view
            else if (viewType == 3)
            {
                today = monday.AddDays(-7);
                sunday = sunday.AddDays(-7);
            }
            //for next week view
            else if (viewType == 4)
            {
                today = monday.AddDays(7);
                sunday = sunday.AddDays(7);
            }
            if (viewType != 1)
            {
                while (today <= sunday)
                {
                    Dates.Add(today);
                    today = today.AddDays(1);
                }
            }

            List<ScheduledActionsSummary> data = new List<ScheduledActionsSummary>();
            List<AssignUser> Users = new List<AssignUser>();
            List<long?> UserIds = new List<long?>();
            try
            {
                var list = _schedulingDB.sp_get_actions_scheduled_to_user(null, null, null).Where(x => x.isQuoteDeleted == false && (x.int_project_manager_id == projectManagerId || projectManagerId == null)).ToList();
                if (list != null && list.Count > 0)
                {
                    Users = list.Select(x => new AssignUser
                    {
                        UserId = x.int_assigned_user_id,
                        UserName = x.vc_assigned_user_name
                    }).ToList();
                    foreach (var user in Users)
                    {
                        if (!UserIds.Contains(user.UserId))
                        {
                            UserIds.Add(user.UserId);
                            decimal? mondayCount = 0;
                            decimal? tuesdayCount = 0;
                            decimal? wednesdayCount = 0;
                            decimal? thrusdayCount = 0;
                            decimal? fridayCount = 0;
                            decimal? saturdayCount = 0;
                            decimal? sundayCount = 0;
                            decimal? totalCount = 0;
                            ScheduledActionsSummary summary = new ScheduledActionsSummary();
                            //var userActions = list.Where(x => x.int_assigned_user_id == user.UserId && (x.dt_schedule_date >= monday && x.dt_schedule_date <= sunday)).ToList();
                            var userActions = list.Where(x => x.int_assigned_user_id == user.UserId && x.dt_schedule_date != null && Dates.Contains(x.dt_schedule_date.Value)).ToList();
                            if (userActions != null && userActions.Count > 0)
                            {
                                foreach (var action in userActions)
                                {
                                    var day = Convert.ToDateTime(action.dt_schedule_date).DayOfWeek;
                                    if (day == DayOfWeek.Sunday)
                                        sundayCount = sundayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Monday)
                                        mondayCount = mondayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Tuesday)
                                        tuesdayCount = tuesdayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Wednesday)
                                        wednesdayCount = wednesdayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Thursday)
                                        thrusdayCount = thrusdayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Friday)
                                        fridayCount = fridayCount + action.dec_assigned_hours;
                                    else if (day == DayOfWeek.Saturday)
                                        saturdayCount = saturdayCount + action.dec_assigned_hours;

                                    totalCount = totalCount + action.dec_assigned_hours;
                                }
                            }
                            summary.UserName = user.UserName;
                            summary.MondayCount = mondayCount;
                            summary.TuesdayCount = tuesdayCount;
                            summary.WednesdayCount = wednesdayCount;
                            summary.ThrusdayCount = thrusdayCount;
                            summary.FridayCount = fridayCount;
                            summary.SaturdayCount = saturdayCount;
                            summary.SundayCount = sundayCount;
                            summary.TotalCount = totalCount;
                            data.Add(summary);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data.Where(x => x.TotalCount > 0).ToList();
        }
        //Author        : Anji
        //Date          : 13 Dec 2019
        //Description   : To get the list of actions of PM
        public List<ActionScheduling> GetPMActions(PMDashboardFilter filterData)
        {
            List<ActionScheduling> data = new List<ActionScheduling>();
            try
            {
                DateTime? tempFromDate = null;
                DateTime? tempToDate = null;
                if (filterData.fromDate != null)
                    tempFromDate = new DateTime(DateTime.Parse(filterData.fromDate).Year, DateTime.Parse(filterData.fromDate).Month, DateTime.Parse(filterData.fromDate).Day, 0, 0, 0);
                if (filterData.toDate != null)
                    tempToDate = new DateTime(DateTime.Parse(filterData.toDate).Year, DateTime.Parse(filterData.toDate).Month, DateTime.Parse(filterData.toDate).Day, 23, 59, 59);

                var userList = _schedulingDB.sp_get_users_job_code().OrderBy(x => x.vc_user_name).ToList();

                //var list = _schedulingDB.sp_get_pm_actions(filterData.projectManagerId).Where(x => x.bt_approved != true || x.bt_unscheduled == true).
                //    Where(x=> (filterData.quoteId == null || x.int_quote_id ==filterData.quoteId ) &&
                //     (filterData.jobCodeId == null || x.int_job_code_id == filterData.jobCodeId) &&
                //     (filterData.staffId == null || filterData.staffId.Contains(x.int_assigned_user_id)) &&
                //     (tempFromDate == null || x.dt_created_date >= tempFromDate) &&
                //     (tempToDate == null || x.dt_created_date <= tempToDate) &&
                //     (filterData.keyWord == null || JsonConvert.SerializeObject(new ActionScheduling { CreatedDate= x.dt_created_date != null? Convert.ToDateTime(x.dt_created_date.Value.ToString("yyyy-mm-dd")):new DateTime(), QuoteNo= x.vc_quote_number, ProjectManagerName = x.vc_project_manage_name ,
                //     SiteName = x.vc_site_name, SowName = x.vc_sow_name, SiteStreetAddress = x.siteStreetAddress, City = x.cityName, TaskName =  x.vc_task_name, ActionName = x.vc_custom_action_name, JobCode = x.vc_job_code, PlannedHours = x.dec_planned_hours,
                //     strDueDate = x.dt_due_date != null? x.dt_due_date.Value.ToString("yyyy-mm-dd") :"Pending", AssignedOn = x.scheduleDate,
                //         AssignedHours = x.assignedHours,AssignedTo = x.assignedTo,
                //         ApprovalStatus = (x.int_detail_id != null && x.logHoursId != null)? (x.bt_approved == null? "Pending" : "Rejected"): "Scheduled"
                //     }).ToLower().Contains(filterData.keyWord.ToLower()))
                //    ).ToList();

                var list = _schedulingDB.sp_get_pm_actions(filterData.projectManagerId).Where(x => x.bt_approved != true || x.bt_unscheduled == true).
                    Where(x => (filterData.quoteId == null || x.int_quote_id == filterData.quoteId) &&
                     (filterData.jobCodeId == null || x.int_job_code_id == filterData.jobCodeId) &&
                     (filterData.staffId == null || filterData.staffId.Contains(x.int_assigned_user_id)) &&
                     (tempFromDate == null || x.dt_created_date >= tempFromDate) &&
                     (tempToDate == null || x.dt_created_date <= tempToDate)
                    ).ToList();

                data = list.AsEnumerable().Select(x => new ActionScheduling
                {
                    ProjectType = x.bt_project == true ? "project" : "quote",
                    ManageActionId = x.int_ibw_manage_action_id,
                    TaskId = x.int_task_id,
                    TaskName = x.vc_task_name,
                    QuoteId = x.int_quote_id,
                    QuoteNo = x.vc_quote_number,
                    SiteId = x.int_site_id,
                    SiteName = x.vc_site_name,
                    SowId = x.int_sow_id,
                    SowName = x.vc_sow_name,
                    SiteStreetAddress = x.siteStreetAddress,
                    SiteCity = x.cityName,
                    SiteState = x.stateName,
                    SiteCountry = x.countryName,
                    ActionId = x.int_action_id,
                    ActionName = x.vc_custom_action_name,
                    JobCodeId = x.int_job_code_id,
                    JobCode = x.vc_job_code,
                    JobCodeTitle = x.vc_job_title,
                    JobCodeStatus = x.bt_job_code_status,
                    PlannedHours = x.dec_planned_hours,
                    CreatedDate = x.dt_created_date,
                    CreatedOn = x.dt_created_date.Value.Date,
                    DueDate = x.dt_due_date,
                    DueDateOptionId = x.int_due_date_lookup_id,
                    DueDateOption = x.vc_due_date_option,
                    IsRemedialAction = x.bt_isRemedial_action,
                    TeamMemberId = x.int_team_member_id,
                    ProjectManagerId = x.int_project_manager_id,
                    ProjectManagerName = x.vc_project_manage_name,
                    TotalAssignedHours = 0,
                    TotalScheduledHours = x.dec_scheduled_hours,
                    UnScheduled = x.bt_unscheduled,
                    Rescheduled = x.bt_rescheduled,
                    IsProject = x.bt_project != null ? x.bt_project.Value : false,
                    Notes = x.vc_notes,
                    DetailsList = list.Where(y => y.int_ibw_manage_action_id == x.int_ibw_manage_action_id && y.bt_approved != true).Count() > 0 ? list.Where(y => y.int_ibw_manage_action_id == x.int_ibw_manage_action_id && y.bt_approved != true).AsEnumerable().Select(y => new ActionSchedulingDetails
                    {
                        IsDuplicate = false,
                        AssignedHours = y.assignedHours,
                        AssignedOn = y.scheduleDate,
                        AssignedTo = y.assignedTo,
                        AssignedBy = y.assignedBy,
                        AssignedUser = y.int_assigned_user_id,
                        DetailId = y.int_detail_id,
                        LogHoursId = y.logHoursId,
                        ActionCompletionStatus = y.actionCompletionStatus,
                        Approved = y.bt_approved,
                        AssignUserList = userList.Where(z => z.int_job_code_master_id == x.int_job_code_id).ToList().AsEnumerable().Select(z => new AssignUser
                        {
                            UserId = z.int_user_id,
                            UserName = z.vc_user_name,
                            Status = z.bt_status
                        }).ToList()
                    }).ToList() :
                    new List<ActionSchedulingDetails> { new ActionSchedulingDetails { } }
                    //DetailsList = x.bt_approved != true ? list.Where(y => y.int_ibw_manage_action_id == x.int_ibw_manage_action_id).AsEnumerable().Select(y => new ActionSchedulingDetails
                    //{
                    //    IsDuplicate = false,
                    //    AssignedHours = y.assignedHours,
                    //    AssignedOn = y.scheduleDate,
                    //    AssignedTo = y.assignedTo,
                    //    AssignedBy = y.assignedBy,
                    //    AssignedUser = y.int_assigned_user_id,
                    //    DetailId = y.int_detail_id,
                    //    LogHoursId = y.logHoursId,
                    //    ActionCompletionStatus = y.actionCompletionStatus,
                    //    Approved = y.bt_approved,
                    //    AssignUserList = userList.Where(z => z.int_job_code_master_id == x.int_job_code_id).ToList().AsEnumerable().Select(z => new AssignUser
                    //    {
                    //        UserId = z.int_user_id,
                    //        UserName = z.vc_user_name,
                    //        Status = z.bt_status
                    //    }).ToList()
                    //}).ToList() :
                    //new List<ActionSchedulingDetails> { new ActionSchedulingDetails { } }
                }).ToList();
                if (data != null)
                    data = data.GroupBy(x => x.ManageActionId).Select(y => y.First()).ToList();
                data = data.Where(x => filterData.keyWord == null ||
                (JsonConvert.SerializeObject(GetPropertyValues(new ActionScheduling
                {
                    TaskName = x.TaskName,
                    QuoteNo = x.QuoteNo,
                    SiteName = x.SiteName,
                    SowName = x.SowName,
                    SiteStreetAddress = x.SiteStreetAddress,
                    SiteCity = x.SiteCity,
                    ActionName = x.ActionName,
                    JobCode = x.JobCode,
                    PlannedHours = x.PlannedHours,
                    CreatedDate = x.CreatedDate,
                    Notes = x.DueDate != null ? x.DueDate.Value.ToString("yyyy-mm-dd") : "pending",
                    ProjectManagerName = x.ProjectManagerName,
                    SiteCountry = JsonConvert.SerializeObject(x.DetailsList.Select(y => new ActionSchedulingDetails
                    {
                        AssignedOn = y.AssignedOn,
                        AssignedHours = y.AssignedHours,
                        AssignedTo = y.AssignedTo,
                        ActionCompletionStatus = (y.DetailId != null && y.LogHoursId != null) ? (y.Approved == null ? "Pending" : "Rejected") : (y.DetailId != null && y.LogHoursId == null) ? "Scheduled" : ""
                    })).Replace("ScheduleDate", "")
                }), Formatting.Indented)
                ).ToLower().Contains(filterData.keyWord.ToLower())).ToList();

                //var strdata = data.Select(x => (JsonConvert.SerializeObject(GetPropertyValues(new ActionScheduling
                //{
                //    TaskName = x.TaskName,
                //    QuoteNo = x.QuoteNo,
                //    SiteName = x.SiteName,
                //    SowName = x.SowName,
                //    SiteStreetAddress = x.SiteStreetAddress,
                //    SiteCity = x.SiteCity,
                //    ActionName = x.ActionName,
                //    JobCode = x.JobCode,
                //    PlannedHours = x.PlannedHours,
                //    CreatedDate = x.CreatedDate,
                //    Notes = x.DueDate != null ? x.DueDate.Value.ToString("yyyy-mm-dd") : "pending",
                //    ProjectManagerName = x.ProjectManagerName

                //}), Formatting.Indented)) + "" +
                //(JsonConvert.SerializeObject(GetPropertyValues(x.DetailsList.Select(y => new ActionSchedulingDetails
                //{
                //    AssignedOn = y.AssignedOn,
                //    AssignedHours = y.AssignedHours,
                //    AssignedTo = y.AssignedTo,
                //    ActionCompletionStatus = (y.DetailId != null && y.LogHoursId != null) ? (y.Approved == null ? "Pending" : "Rejected") : (y.DetailId != null && y.LogHoursId == null) ? "Scheduled" : ""
                //})), Formatting.Indented)
                //).ToLower()).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data.OrderBy(x => x.DueDate).ToList();
        }
        public static IEnumerable<object> GetPropertyValues<T>(T input)
        {
            return input.GetType()
                .GetProperties()
                .Select(p => p.GetValue(input));
        }
        //Author        : Anji
        //Date          : 13 Dec 2019
        //Description   : To get the list of actions of PM
        public List<ActionScheduling> GetUnAssignedAndAssignedActionsMap(ActionScheduledToUser inputData)
        {
            List<DateTime> Dates = new List<DateTime>();
            DateTime today = DateTime.Today;
            DateTime currentDay = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            DateTime monday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 7);
            long? viewType = inputData.ViewType;
            long? taskId = inputData.TaskId;
            //for day view
            if (viewType == 1)
            {
                Dates.Add(today);
            }
            //for this week view
            else if (viewType == 2)
            {
                today = monday;
            }
            //for last week view
            else if (viewType == 3)
            {
                today = monday.AddDays(-7);
                sunday = sunday.AddDays(-7);
            }
            //for next week view
            else if (viewType == 4)
            {
                today = monday.AddDays(7);
                sunday = sunday.AddDays(7);
            }
            if (viewType != 1)
            {
                while (today <= sunday)
                {
                    Dates.Add(today);
                    today = today.AddDays(1);
                }
            }
            List<ActionScheduling> data = new List<ActionScheduling>();
            List<ActionScheduling> dataUnassigned = new List<ActionScheduling>();
            List<ActionScheduling> assigned = new List<ActionScheduling>();
            try
            {
                var userList = _schedulingDB.sp_get_users_job_code().OrderBy(x => x.vc_user_name).ToList();

                var actionsList = _schedulingDB.sp_get_pm_actions(null).Where(x => x.int_task_id == taskId).
                    Where(x =>
                    (inputData.FilterForm.QuoteId == null || inputData.FilterForm.QuoteId == x.int_quote_id) &&
                    (inputData.FilterForm.ProjectManagerIds == null || inputData.FilterForm.ProjectManagerIds.Count() == 0 || inputData.FilterForm.ProjectManagerIds.Contains(x.int_project_manager_id)) &&
                    (inputData.FilterForm.StaffIds == null || inputData.FilterForm.StaffIds.Count == 0 || inputData.FilterForm.StaffIds.Contains(x.int_assigned_user_id)) &&
                    (inputData.FilterForm.JobCodeIds == null || inputData.FilterForm.JobCodeIds.Count() == 0 || inputData.FilterForm.JobCodeIds.Contains(x.int_job_code_id)) &&
                    (inputData.FilterForm.KeyWord == null || JsonConvert.SerializeObject(x).ToLower().Contains(inputData.FilterForm.KeyWord.ToLower()))
                    ).ToList();
                if (actionsList != null)
                {
                    var unassignedActions = actionsList.Where(x => x.bt_scheduled == null || x.bt_scheduled == false);
                    dataUnassigned = unassignedActions.AsEnumerable().Select(x => new ActionScheduling
                    {
                        ManageActionId = x.int_ibw_manage_action_id,
                        TaskId = x.int_task_id,
                        TaskName = x.vc_task_name,
                        QuoteId = x.int_quote_id,
                        QuoteNo = x.vc_quote_number,
                        SiteId = x.int_site_id,
                        SiteName = x.vc_site_name,
                        SowId = x.int_sow_id,
                        SowName = x.vc_sow_name,
                        SiteStreetAddress = x.siteStreetAddress,
                        SiteCity = x.cityName,
                        SiteState = x.stateName,
                        SiteCountry = x.countryName,
                        ActionId = x.int_action_id,
                        ActionName = x.vc_custom_action_name,
                        JobCodeId = x.int_job_code_id,
                        JobCode = x.vc_job_code,
                        JobCodeTitle = x.vc_job_title,
                        JobCodeStatus = x.bt_job_code_status,
                        PlannedHours = x.dec_planned_hours,
                        CreatedDate = x.dt_created_date,
                        DueDate = x.dt_due_date,
                        DueDateOptionId = x.int_due_date_lookup_id,
                        DueDateOption = x.vc_due_date_option,
                        IsRemedialAction = x.bt_isRemedial_action,
                        TeamMemberId = x.int_team_member_id,
                        ProjectManagerId = x.int_project_manager_id,
                        ProjectManagerName = x.vc_project_manage_name,
                        TotalAssignedHours = 0,
                        DetailsList = unassignedActions.Where(y => y.int_ibw_manage_action_id == x.int_ibw_manage_action_id).AsEnumerable().Select(y => new ActionSchedulingDetails
                        {
                            IsDuplicate = false,
                            AssignedHours = y.assignedHours,
                            AssignedOn = y.scheduleDate,
                            AssignedTo = y.assignedTo,
                            AssignedBy = y.assignedBy,
                            AssignedUser = y.int_assigned_user_id,
                            AssignUserList = userList.Where(z => z.int_job_code_master_id == x.int_job_code_id).ToList().AsEnumerable().Select(z => new AssignUser
                            {
                                UserId = z.int_user_id,
                                UserName = z.vc_user_name,
                                Status = z.bt_status
                            }).ToList()
                        }).ToList()
                    }).ToList();
                    if (dataUnassigned != null)
                        dataUnassigned = dataUnassigned.GroupBy(x => x.ManageActionId).Select(y => y.First()).ToList();
                    var allAssigned = actionsList.Where(x => x.bt_scheduled == true && x.scheduleDate != "-" && x.scheduleDate != null).ToList();
                    var assignedActions = allAssigned.Where(x => Dates.Contains(Convert.ToDateTime(x.scheduleDate)));
                    assigned = assignedActions.AsEnumerable().Select(x => new ActionScheduling
                    {
                        ManageActionId = x.int_ibw_manage_action_id,
                        TaskId = x.int_task_id,
                        TaskName = x.vc_task_name,
                        QuoteId = x.int_quote_id,
                        QuoteNo = x.vc_quote_number,
                        SiteId = x.int_site_id,
                        SiteName = x.vc_site_name,
                        SowId = x.int_sow_id,
                        SowName = x.vc_sow_name,
                        SiteStreetAddress = x.siteStreetAddress,
                        SiteCity = x.cityName,
                        SiteState = x.stateName,
                        SiteCountry = x.countryName,
                        ActionId = x.int_action_id,
                        ActionName = x.vc_custom_action_name,
                        JobCodeId = x.int_job_code_id,
                        JobCode = x.vc_job_code,
                        JobCodeTitle = x.vc_job_title,
                        JobCodeStatus = x.bt_job_code_status,
                        PlannedHours = x.dec_planned_hours,
                        CreatedDate = x.dt_created_date,
                        DueDate = x.dt_due_date,
                        DueDateOptionId = x.int_due_date_lookup_id,
                        DueDateOption = x.vc_due_date_option,
                        IsRemedialAction = x.bt_isRemedial_action,
                        TeamMemberId = x.int_team_member_id,
                        ProjectManagerId = x.int_project_manager_id,
                        ProjectManagerName = x.vc_project_manage_name,
                        TotalAssignedHours = 0,
                        DetailsList = assignedActions.Where(y => y.int_ibw_manage_action_id == x.int_ibw_manage_action_id).AsEnumerable().Select(y => new ActionSchedulingDetails
                        {
                            IsDuplicate = false,
                            AssignedHours = y.assignedHours,
                            AssignedOn = y.scheduleDate,
                            AssignedTo = y.assignedTo,
                            AssignedBy = y.assignedBy,
                            AssignedUser = y.int_assigned_user_id,
                            AssignUserList = userList.Where(z => z.int_job_code_master_id == x.int_job_code_id).ToList().AsEnumerable().Select(z => new AssignUser
                            {
                                UserId = z.int_user_id,
                                UserName = z.vc_user_name,
                                Status = z.bt_status
                            }).ToList()
                        }).ToList()
                    }).ToList();
                    if (assigned != null)
                        assigned = assigned.GroupBy(x => x.ManageActionId).Select(y => y.First()).ToList();
                }
                data.AddRange(dataUnassigned);
                data.AddRange(assigned);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }

        public ActionScheduleFilter GetFilterInputs()
        {
            ActionScheduleFilter filter = new ActionScheduleFilter();
            filter.Quotes = _schedulingDB.GetAllQuotes().ToList();
            filter.JobCodes = _adminMethods.GetJobCodes();
            filter.Users = _adminMethods.GetUsers();
            filter.ProjectManagers = _adminDB.sp_getAllIBWUsers().Where(x => x.vc_role_name == "Project Manager" && x.bt_status == true).OrderBy(x => x.vc_user_name).ToList();
            return filter;
        }


        #endregion

        #region Bharath
        #region Expenses
        public Expenses getUnassignedExpenses(long expenseId)
        {
            var data = _schedulingDB.tbl_ibw_expenses.Where(x => x.int_expense_id == expenseId).FirstOrDefault();
            var refno = _schedulingDB.sp_get_all_quotes().Where(x => x.int_quote_id == data.int_quote_id).FirstOrDefault();
            var expCodes = _adminDB.GetExpenseCode().ToList();
            Expenses expenseData = new Expenses()
            {
                ExpenseId = data.int_expense_id,
                ExpenseCodeId = data.int_expense_code_id,
                TimesheetId = data.int_timesheet_id,
                Date = data.dt_log_date,
                RefId = data.int_quote_id,
                RefNo = refno != null ? refno.vc_quote_number : "-",
                SiteId = data.int_site_id,
                SowId = data.int_sow_id,
                TaskId = data.int_task_id,
                JobCodeId = data.int_jobcode_id,
                LimitType = _quoteDB.getAllExpenseCodesWithUnits().Where(l => l.int_expense_code_master_id == data.int_expense_code_id).Select(y => y.expenseLimitType).FirstOrDefault(),
                Rate = data.dec_rate,// _quoteDB.getAllExpenseCodesWithUnits().Where(l => l.int_expense_code_master_id == data.int_expense_code_id).Select(y => y.dec_rate).FirstOrDefault(),
                Quantity = data.int_quantity,
                IsReimbursable = data.bt_reimbursable,
                AttachmentRequired = expCodes.Where(y => y.int_expense_code_master_id == data.int_expense_code_id).FirstOrDefault().bt_attachment_required,
                GoogleFileId = data.vc_google_file_id,
                Description = data.vc_description
            };

            return expenseData;
        }
        public List<sp_get_user_timesheetResult> getTimeSheetsOfUser(long userId)
        {
            var data = _schedulingDB.sp_get_user_timesheet().Where(x => x.userId == userId).OrderByDescending(x => x.dt_created_date).ToList();
            return data;
        }


        public List<sp_get_pm_expensesResult> getExpensesOfUser(long userId)
        {
            var data = _schedulingDB.sp_get_pm_expenses().Where(x => x.userId == userId).OrderByDescending(x => x.dt_created_date).ToList();
            return data;
        }
        public long? deleteTimeSheetOfUser(bool isAssigned, long? timeSheetId)
        {
            long? result = null;
            List<string> deletedFilesList;
            if (isAssigned == true)
            {
                deletedFilesList = _schedulingDB.tbl_ibw_expense_documents.Where(x => x.int_action_log_hour_id == timeSheetId).Select(y => y.vc_google_drive_file_id).ToList();
            }
            else
            {
                List<long> expensesOfTimeSheet = _schedulingDB.tbl_ibw_expenses.Where(x => x.int_timesheet_id == timeSheetId).Select(x => x.int_expense_id).ToList();
                deletedFilesList = _schedulingDB.tbl_ibw_expense_documents.Where(x => expensesOfTimeSheet.Contains((long)x.int_expense_id)).Select(y => y.vc_google_drive_file_id).ToList();
            }
            if (deletedFilesList.Count > 0)
            {
                _quoteMethods.deleteGoogleDriveFilesfromList(deletedFilesList);

            }
            _schedulingDB.deleteTimeSheetOfUser(timeSheetId, isAssigned, ref result);
            return result;
        }
        public long? deleteExpenseOfUser(bool isAssigned, long? expenseid)
        {
            long? result = null;
            List<string> deletedExpensesFilesList;
            if (isAssigned == true)
            {
                deletedExpensesFilesList = _schedulingDB.tbl_ibw_expense_documents.Where(x => x.int_action_log_hour_expense_id == expenseid).Select(y => y.vc_google_drive_file_id).ToList();
            }
            else
            {
                deletedExpensesFilesList = _schedulingDB.tbl_ibw_expense_documents.Where(x => x.int_expense_id == expenseid).Select(y => y.vc_google_drive_file_id).ToList();

            }
            if (deletedExpensesFilesList.Count > 0)
            {
                _quoteMethods.deleteGoogleDriveFilesfromList(deletedExpensesFilesList);

            }
            _schedulingDB.deleteExpenseOfUser(expenseid, isAssigned, ref result);
            return result;
        }
        #endregion
        #region TimeSheets
        public string uploadActionLogFile(LogHoursFiles model, long? actionDetailId)
        {
            List<ExpenseCodes> expenseData = _adminMethods.GetExpenseCodes();
            var usersList = _adminMethods.GetAllIBWUsers();
            var allDetailsOfAction = _schedulingDB.getAllDetailsOfScheduledActions(actionDetailId).FirstOrDefault();
            string googleDriveFileId = "";
            List<DoocumentTypes> AllDocumentTypes = _adminDB.GetAllDocumentsType().Select(doc => new DoocumentTypes { DocumentId = doc.int_document_lookup_id, DocumentName = doc.vc_lookup_name }).ToList();
            if (model.UploadedFile != null)
            {
                if (_quoteMethods.setUpGoogleDriveFileForEntireQuoteStructure(allDetailsOfAction.int_quote_id, allDetailsOfAction.int_site_id, allDetailsOfAction.int_sow_id, allDetailsOfAction.int_ibw_budget_task_id, allDetailsOfAction.int_manage_action_id) != null)
                {
                    string actionGoogleDriveId = _quotesExtensionDB.tbl_ibw_manage_actions.Where(x => x.int_ibw_manage_action_id == allDetailsOfAction.int_manage_action_id).Select(y => y.vc_google_drive_file_id).FirstOrDefault();

                    if (model.isExpense == true)
                    {
                        string FileName = expenseData.Where(x => x.ExpenseCodeId == model.Id).Select(y => y.ExpenseCode).FirstOrDefault() + "-" +
                           usersList.Where(x => x.int_user_id == model.UserId).Select(y => y.vc_user_name + " " + y.vc_alias_name).FirstOrDefault();
                        googleDriveFileId = GoogleDriveFilesRepository.FileUploadInFolderDirect(actionGoogleDriveId, model.UploadedFile, FileName, model.UploadedFile.ContentType);

                        //var expenseDocumentsTable = _schedulingDB.tbl_ibw_expense_documents.Single(x=>x.int_action_log_hour_expense_id == item.)

                    }
                    else if (model.isExpense == false)
                    {
                        string FileName = AllDocumentTypes.Where(x => x.DocumentId == model.Id).Select(y => y.DocumentName).FirstOrDefault() + "-" +
                                                  usersList.Where(x => x.int_user_id == model.UserId).Select(y => y.vc_user_name + " " + y.vc_alias_name).FirstOrDefault();

                        googleDriveFileId = GoogleDriveFilesRepository.FileUploadInFolderDirect(actionGoogleDriveId, model.UploadedFile, FileName, model.UploadedFile.ContentType);

                    }
                }
            }
            return googleDriveFileId;
        }
        public LogHoursOverall getTimeSheet(long timeSheetId)
        {
            ActionOverviewForm actionForm = new ActionOverviewForm();
            LogHoursForm logHoursForm = new LogHoursForm();
            ExpensesDetails expensesForm = new ExpensesDetails();
            var expCodes = _adminDB.GetExpenseCode().ToList();
            var data = _schedulingDB.getTimeSheet(timeSheetId).ToList();
            var expenseData = _schedulingDB.getAllExpensesOfAssignedTimesheet(timeSheetId).ToList();
            actionForm = data.Select(x => new ActionOverviewForm
            {
                detailId = x.int_detail_id,
                manageActionId = x.int_manage_action_id,
                logHoursStatus = x.int_status,
            }).FirstOrDefault();
            logHoursForm = data.Select(y => new LogHoursForm
            {
                plannedHours = y.dec_planned_hours,
                workedHours = y.dec_worked_hours,
                remainingHours = y.dec_remaining_hours,
                completedDate = y.dt_completed_date,
                remarks = y.vc_remarks,
                includeExpenses = y.bt_include_expenses
            }).FirstOrDefault();
            expensesForm.expensesDetails = expenseData.Select(z => new ExpensesForm
            {
                expenseActionId = z.int_action_log_hours_expense_id,
                expenseCodeId = z.int_expense_code_id,
                expenseRate = z.dec_expense_rate,
                logHoursDate = z.dt_created_date,
                expenseReimbursable = z.bt_expense_isReimbursable,
                attachmentRequired = expCodes.Where(a => a.int_expense_code_master_id == z.int_expense_code_id).FirstOrDefault().bt_attachment_required,
                quantity = z.int_quanity,
                limitType = _quoteDB.getAllExpenseCodesWithUnits().Where(x => x.int_expense_code_master_id == z.int_expense_code_id).Select(y => y.expenseLimitType).FirstOrDefault(),
                expenseTotalAmount = z.dec_total_expense_cost,
                description = z.vc_description
            }).ToList();

            LogHoursOverall logHoursData = new LogHoursOverall()
            {
                actionForm = actionForm,
                logHoursForm = logHoursForm,
                expensesForm = expensesForm,
                actionLogHourId = timeSheetId
            };
            return logHoursData;
        }
        public Timesheet getUnAssignedTimeSheet(long timeSheetId)
        {
            Timesheet mainData = new Timesheet();
            var data = _schedulingDB.tbl_ibw_timesheets.Where(x => x.int_timesheet_id == timeSheetId).FirstOrDefault();
            var refno = _schedulingDB.sp_get_all_quotes().Where(x => x.int_quote_id == data.int_quote_id).FirstOrDefault();
            var expenseData = _schedulingDB.tbl_ibw_expenses.Where(x => x.int_timesheet_id == timeSheetId).ToList();
            var expCodes = _adminDB.GetExpenseCode().ToList();
            List<Expenses> expenseList = new List<Expenses>();
            expenseList = expenseData.Select(x => new Expenses
            {
                ExpenseId = x.int_expense_id,
                ExpenseCodeId = x.int_expense_code_id,
                TimesheetId = x.int_timesheet_id,
                Date = x.dt_log_date,
                RefId = x.int_quote_id,
                SiteId = x.int_site_id,
                SowId = x.int_sow_id,
                TaskId = x.int_task_id,
                JobCodeId = x.int_jobcode_id,
                LimitType = _quoteDB.getAllExpenseCodesWithUnits().Where(l => l.int_expense_code_master_id == x.int_expense_code_id).Select(y => y.expenseLimitType).FirstOrDefault(),
                Rate = x.dec_rate,// _quoteDB.getAllExpenseCodesWithUnits().Where(l => l.int_expense_code_master_id == x.int_expense_code_id).Select(y => y.dec_rate).FirstOrDefault(),
                Quantity = x.int_quantity,
                IsReimbursable = x.bt_reimbursable,
                AttachmentRequired = expCodes.Where(y => y.int_expense_code_master_id == x.int_expense_code_id).FirstOrDefault().bt_attachment_required,
                GoogleFileId = x.vc_google_file_id,
                Description = x.vc_description
            }).ToList();
            mainData = new Timesheet
            {
                TimesheetId = data.int_timesheet_id,
                Date = data.dt_log_date,
                RefId = data.int_quote_id,
                RefNo = refno != null ? refno.vc_quote_number : "-",
                SiteId = data.int_site_id,
                SowId = data.int_sow_id,
                TaskId = data.int_task_id,
                JobCodeId = data.int_jobcode_id,
                WorkedHours = data.dec_worked_hours,
                Remarks = data.vc_remarks,
                IncludeExpenses = data.bt_include_expense,
                ExpensesList = expenseList
            };
            return mainData;
        }
        public long? updateLogHours(TimeSheetModal model)
        {
            long? result = null;
            _schedulingDB.updateLogHours(model.TimeSheetId, model.TimeSheetStatus, model.WorkedHours, model.RemainingHours,
               model.CompletedDate, model.Remarks, model.IncludeExpenses, model.UserId, ref result);
            return result;
        }
        public ReviewTimesheetExpense GetActionTimesheetAndExpenses(long detailId)
        {
            try
            {
                ReviewTimesheetExpense TimesheetExpenses = new ReviewTimesheetExpense();
                TimesheetExpenses.ActionDetails = getAllDetailsOfScheduledAction(detailId);
                TimesheetExpenses.TimeSheet = _schedulingDB.GetUserActionLogHours(detailId).FirstOrDefault();
                TimesheetExpenses.Expenses = _schedulingDB.GetUserActionExpenses(TimesheetExpenses.TimeSheet.int_action_log_hour_id).ToList();
                return TimesheetExpenses;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public ScheduledActionDetails getAllDetailsOfScheduledAction(long detailId)
        {
            ScheduledActionDetails mainData = new ScheduledActionDetails();
            var data = _schedulingDB.getAllDetailsOfScheduledActions(detailId);
            mainData = data.Select(x => new ScheduledActionDetails
            {
                ActionName = x.vc_custom_action_name,
                ActionNotes = x.action_notes,
                ActionGoogleDriveId = x.action_folder_id,
                AssignedByUserId = x.int_assigned_user_id,
                AssignedByUserName = x.vc_assigned_by_user_name,
                AssignedToUserName = x.vc_assigned_for_user_name,
                AssignedHours = x.dec_assigned_hours,
                AssignedOn = x.dt_created_date,
                CompletionDate = x.dt_due_date,
                DocumentCategories = _adminDB.GetAllDocumentsTypeOfTask(x.int_task_id).Select(doc => new DoocumentTypes { DocumentId = doc.int_document_lookup_id, DocumentName = doc.vc_lookup_name }).ToList(),
                QuoteNo = x.vc_quote_number,
                QuoteId = x.int_quote_id,
                QuoteProject = x.bt_project == true ? "project" : "quote",
                DetailId = x.int_detail_id,
                TaskId = x.int_task_id,
                ManageActionId = x.int_manage_action_id,
                TaskName = x.vc_task_name,
                SiteName = x.vc_site_name,
                SowName = x.vc_sow_name,
                ScheduledDate = x.dt_schedule_date,
                PlannedHours = x.action_planned_hours,
                JobCode = x.vc_job_code,
                ProjectManagerId = x.int_project_manager_id,
                ProjectManagerName = x.vc_project_manager_name,
                RecDate = Convert.ToDateTime(x.dt_rec_date).ToString("yyyy-MM-dd, hh:mm tt"),
                CreatedBy = x.vc_created_by,
                SplitWithUsers = _schedulingDB.sp_get_action_split_users(detailId).ToList().AsEnumerable().Select(s => new AssignUser
                {
                    userAssignedHours = s.dec_assigned_hours,
                    UserName = s.vc_assigned_user_name
                }).ToList(),
                ActionSchedulingStatus = x.action_scheduling_status,
                JobCodeTitle = x.vc_job_title
            }).FirstOrDefault();
            return mainData;
        }
        //author:Sreebharath
        //date: 05/11/2019
        //to insert the job Codes with notes
        public long? UpsertLogHoursAction(LogHoursOverall model, List<DocumentCategory> docCategoryList)
        {
            try
            {
                long? result = null;
                DataTable dt = new DataTable();
                DataTable dt1 = new DataTable();

                dt.Columns.AddRange(new DataColumn[9] {
                    new DataColumn("int_action_log_hours_expense_id", typeof(int)),
                    new DataColumn("int_action_log_hour_id", typeof(int)),
                    new DataColumn("int_expense_code_master_id", typeof(int)),
                    new DataColumn("dec_expense_rate", typeof(decimal)),
                    new DataColumn("int_quantity", typeof(int)),
                    new DataColumn("dec_total_expense_cost", typeof(decimal)),
                    new DataColumn("bt_isReimbursable", typeof(bool)),
                    new DataColumn("vc_description", typeof(string)),
                    new DataColumn("vc_expense_google_drive_file_id", typeof(string))
                });
                dt1.Columns.AddRange(new DataColumn[1] {
                    new DataColumn("vc_document_google_drive_file_id", typeof(string))
                });
                if (docCategoryList.Count > 0)
                {
                    foreach (var row in docCategoryList)
                    {
                        string vc_document_google_drive_file_id = row.docGoogleDriveFileId;
                        dt1.Rows.Add(vc_document_google_drive_file_id);
                    }
                }
                if (model.expensesForm != null)
                {
                    foreach (var row in model.expensesForm.expensesDetails)
                    {
                        int int_action_log_hours_expense_id = Convert.ToInt32(row.expenseActionId);
                        int int_action_log_hour_id = Convert.ToInt32(model.actionLogHourId);
                        int int_expense_code_master_id = Convert.ToInt32(row.expenseCodeId);
                        decimal? dec_expense_rate = row.expenseRate;
                        int int_quantity = Convert.ToInt32(row.quantity);
                        decimal? dec_total_expense_cost = row.expenseTotalAmount;
                        bool? bt_isReimbursable = row.expenseReimbursable;
                        string vc_description = row.description;
                        string vc_expense_google_drive_file_id = row.expenseGoogleDriveFileId;
                        dt.Rows.Add(int_action_log_hours_expense_id, int_action_log_hour_id, int_expense_code_master_id, dec_expense_rate, int_quantity,
                            dec_total_expense_cost, bt_isReimbursable, vc_description, vc_expense_google_drive_file_id);
                    }
                }
                using (var con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    using (var cmd = new SqlCommand("upsertLogHoursWithExpenses", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        if (model.actionLogHourId != null)
                            cmd.Parameters.AddWithValue("@int_action_log_hour_id", model.actionLogHourId);
                        else
                            cmd.Parameters.AddWithValue("@int_action_log_hour_id", DBNull.Value);
                        cmd.Parameters.AddWithValue("@int_detail_id", model.actionForm.detailId);
                        cmd.Parameters.AddWithValue("@int_manage_action_id", model.actionForm.manageActionId);
                        cmd.Parameters.AddWithValue("@int_action_status", model.actionForm.logHoursStatus);
                        cmd.Parameters.AddWithValue("@dec_planned_hours", model.logHoursForm.plannedHours);
                        cmd.Parameters.AddWithValue("@dec_worked_hours", model.logHoursForm.workedHours);
                        cmd.Parameters.AddWithValue("@dec_remaining_hours", model.logHoursForm.remainingHours);
                        cmd.Parameters.AddWithValue("@dt_completed_date", model.logHoursForm.completedDate);
                        cmd.Parameters.AddWithValue("@vc_remarks", model.logHoursForm.remarks);
                        cmd.Parameters.AddWithValue("@array_expenses_details", model.expensesForm != null ? dt : null);
                        cmd.Parameters.AddWithValue("@array_document_ids", docCategoryList.Count > 0 ? dt1 : null);
                        cmd.Parameters.AddWithValue("@user_id", model.actionForm.userId);
                        cmd.Parameters.AddWithValue("@include_expenses", model.logHoursForm.includeExpenses);
                        //Add the output parameter to the command object
                        SqlParameter outPutParameter = new SqlParameter();
                        outPutParameter.ParameterName = "@result";
                        outPutParameter.SqlDbType = System.Data.SqlDbType.Int;
                        outPutParameter.Direction = System.Data.ParameterDirection.Output;
                        cmd.Parameters.Add(outPutParameter);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        result = Convert.ToInt32(outPutParameter.Value);
                        con.Close();
                    }

                    //if (docCategoryList.Count > 0)
                    //{
                    //    List<string> docsList = docCategoryList.Where(y=>y.docGoogleDriveFileId != null)
                    //        .Select(x => string.Format("(" + result.ToString() + ", {0})", x.docGoogleDriveFileId)).ToList();
                    //   // var qyy = docsList.Select(x => string.Format("(" + result.ToString() + ", {0})", x)).ToList();


                    //    string queryString = string.Join(",", docsList);

                    //    using (var cmd = new SqlCommand("insert into tbl_ibw_expense_documents(int_action_log_hour_id, vc_google_drive_file_id) values"+ queryString, con))
                    //    {
                    //        //  cmd.CommandType = CommandType.TableDirect;
                    //        con.Open();
                    //        cmd.ExecuteNonQuery();
                    //        con.Close();
                    //    }
                    //}


                    if (model.expensesForm != null && model.expensesForm.deletedExpenses.Count > 0)
                    {
                        var deletedExpensesList = _schedulingDB.tbl_ibw_expense_documents.Where(x => model.expensesForm.deletedExpenses.Contains(x.int_action_log_hour_expense_id)).Select(y => y.vc_google_drive_file_id).ToList();
                        _quoteMethods.deleteGoogleDriveFilesfromList(deletedExpensesList);

                        string totalExpensItems = "(" + string.Join(",", model.expensesForm.deletedExpenses) + ")";
                        using (var cmd = new SqlCommand("delete from tbl_ibw_expense_documents  where int_action_log_hour_expense_id in" + totalExpensItems + ";" + "delete from tbl_ibw_timesheet_expenses_notes where int_action_log_hours_expense_id in" + totalExpensItems + ";" + "delete from tbl_ibw_action_log_hours_expenses where int_action_log_hours_expense_id in" + totalExpensItems, con))
                        {
                            //  cmd.CommandType = CommandType.TableDirect;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }

                    }
                }



                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string uploadActionLogFileForExpense(ExpensesForm model, long? actionDetailId, long? userId, HttpPostedFile File)
        {
            List<ExpenseCodes> expenseData = _adminMethods.GetExpenseCodes();
            var usersList = _adminMethods.GetAllIBWUsers();
            var allDetailsOfAction = _schedulingDB.getAllDetailsOfScheduledActions(actionDetailId).FirstOrDefault();
            string googleDriveFileId = "";
            List<DoocumentTypes> AllDocumentTypes = _adminDB.GetAllDocumentsType().Select(doc => new DoocumentTypes { DocumentId = doc.int_document_lookup_id, DocumentName = doc.vc_lookup_name }).ToList();
            if (File != null)
            {
                if (_quoteMethods.setUpGoogleDriveFileForEntireQuoteStructure(allDetailsOfAction.int_quote_id, allDetailsOfAction.int_site_id, allDetailsOfAction.int_sow_id, allDetailsOfAction.int_ibw_budget_task_id, allDetailsOfAction.int_manage_action_id) != null)
                {
                    string actionGoogleDriveId = _quotesExtensionDB.tbl_ibw_manage_actions.Where(x => x.int_ibw_manage_action_id == allDetailsOfAction.int_manage_action_id).Select(y => y.vc_google_drive_file_id).FirstOrDefault();
                    string FileName = expenseData.Where(x => x.ExpenseCodeId == model.expenseCodeId).Select(y => y.ExpenseCode).FirstOrDefault() + "-" +
                    usersList.Where(x => x.int_user_id == userId).Select(y => y.vc_user_name + " " + y.vc_alias_name).FirstOrDefault();
                    googleDriveFileId = GoogleDriveFilesRepository.FileUploadInFolderDirect(actionGoogleDriveId, File, FileName, File.ContentType);

                }
            }
            return googleDriveFileId;
        }
        public string uploadActionLogFileDoc(DocumentCategory model, long? actionDetailId, long? userId, HttpPostedFile File)
        {
            List<ExpenseCodes> expenseData = _adminMethods.GetExpenseCodes();
            var usersList = _adminMethods.GetAllIBWUsers();
            var allDetailsOfAction = _schedulingDB.getAllDetailsOfScheduledActions(actionDetailId).FirstOrDefault();
            string googleDriveFileId = "";
            List<DoocumentTypes> AllDocumentTypes = _adminDB.GetAllDocumentsType().Select(doc => new DoocumentTypes { DocumentId = doc.int_document_lookup_id, DocumentName = doc.vc_lookup_name }).ToList();
            if (File != null)
            {
                if (_quoteMethods.setUpGoogleDriveFileForEntireQuoteStructure(allDetailsOfAction.int_quote_id, allDetailsOfAction.int_site_id, allDetailsOfAction.int_sow_id, allDetailsOfAction.int_ibw_budget_task_id, allDetailsOfAction.int_manage_action_id) != null)
                {
                    string actionGoogleDriveId = _quotesExtensionDB.tbl_ibw_manage_actions.Where(x => x.int_ibw_manage_action_id == allDetailsOfAction.int_manage_action_id).Select(y => y.vc_google_drive_file_id).FirstOrDefault();

                    string FileName = model.DocumentName + "-" + usersList.Where(x => x.int_user_id == userId).Select(y => y.vc_user_name + " " + y.vc_alias_name).FirstOrDefault();

                    googleDriveFileId = GoogleDriveFilesRepository.FileUploadInFolderDirect(actionGoogleDriveId, File, FileName, File.ContentType);

                }
            }
            return googleDriveFileId;
        }

        public bool uploadActionLogFiles(List<LogHoursFiles> model, long? actionDetailId)
        {
            List<ExpenseCodes> expenseData = _adminMethods.GetExpenseCodes();
            var usersList = _adminMethods.GetAllIBWUsers();
            var allDetailsOfAction = _schedulingDB.getAllDetailsOfScheduledActions(actionDetailId).FirstOrDefault();
            string googleDriveFileId = "";
            List<DoocumentTypes> AllDocumentTypes = _adminDB.GetAllDocumentsType().Select(doc => new DoocumentTypes { DocumentId = doc.int_document_lookup_id, DocumentName = doc.vc_lookup_name }).ToList();
            if (model.Count > 0)
            {
                if (_quoteMethods.setUpGoogleDriveFileForEntireQuoteStructure(allDetailsOfAction.int_quote_id, allDetailsOfAction.int_site_id, allDetailsOfAction.int_sow_id, allDetailsOfAction.int_ibw_budget_task_id, allDetailsOfAction.int_manage_action_id) != null)
                {
                    string actionGoogleDriveId = _quotesExtensionDB.tbl_ibw_manage_actions.Where(x => x.int_ibw_manage_action_id == allDetailsOfAction.int_manage_action_id).Select(y => y.vc_google_drive_file_id).FirstOrDefault();

                    foreach (var item in model)
                    {
                        if (item.isExpense == true)
                        {
                            string FileName = expenseData.Where(x => x.ExpenseCodeId == item.Id).Select(y => y.ExpenseCode).FirstOrDefault() + "-" +
                               usersList.Where(x => x.int_user_id == item.UserId).Select(y => y.vc_user_name + " " + y.vc_alias_name).FirstOrDefault();
                            googleDriveFileId = GoogleDriveFilesRepository.FileUploadInFolderDirect(actionGoogleDriveId, item.UploadedFile, FileName, item.UploadedFile.ContentType);

                        }
                        else if (item.isExpense == false)
                        {
                            string FileName = AllDocumentTypes.Where(x => x.DocumentId == item.Id).Select(y => y.DocumentName).FirstOrDefault() + "-" +
                                                      usersList.Where(x => x.int_user_id == item.UserId).Select(y => y.vc_user_name + " " + y.vc_alias_name).FirstOrDefault();

                            googleDriveFileId = GoogleDriveFilesRepository.FileUploadInFolderDirect(actionGoogleDriveId, item.UploadedFile, FileName, item.UploadedFile.ContentType);

                        }

                    }
                }
            }
            if (googleDriveFileId != "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public long? deAllocateScheduledAction(long? scheduledDetailId, long? createdBy)
        {
            try
            {
                //inserting activity log
                var manageActionId = _schedulingDB.sp_get_actions_scheduled_to_user(null, null, null).Where(x => x.int_detail_id == scheduledDetailId).FirstOrDefault().int_ibw_manage_action_id;
                var lastLog = _schedulingDB.sp_get_scheduled_action_activity_log(manageActionId).Where(x => x.int_action_detail_id == scheduledDetailId).OrderByDescending(x => x.int_log_id).ToList().FirstOrDefault();
                if (lastLog != null)
                {
                    _schedulingDB.sp_insert_scheduled_action_activity_log(manageActionId, scheduledDetailId, lastLog.dt_scheduled_date, lastLog.int_assigned_to, lastLog.dec_planned_hours,
                        lastLog.dec_assigned_hours, lastLog.dec_worked_hours, lastLog.dec_remaining_hours, "Not Yet Started", createdBy);
                }

                long? result = 0;
                _schedulingDB.deAllocateScheduledAction(scheduledDetailId, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        #endregion
        #endregion
        #region Timesheets & Expenses
        //Author        : Siddhant Chawade
        //Date          : 18th Feb 2020
        //Description   : To get the list of notes timesheet & expenses
        public List<TimesheetExpenseNotes> GetTimesheetExpenseNotes(long? id, bool isTimesheet, bool isAssigned, bool isApproval)
        {
            List<TimesheetExpenseNotes> data = new List<TimesheetExpenseNotes>();
            try
            {
                var list = _schedulingDB.sp_get_timesheet_expenses_notes(id, isTimesheet, isAssigned).OrderByDescending(x => x.int_note_id).ToList();
                list = isApproval ? list.Where(x => !string.IsNullOrEmpty(x.vc_pm_message)).ToList() : list.Where(x => !string.IsNullOrEmpty(x.vc_notes)).ToList();
                if (list != null && list.Count > 0)
                {
                    data = list.AsEnumerable().Select(x => new TimesheetExpenseNotes
                    {
                        Notes = isApproval ? x.vc_pm_message : x.vc_notes,
                        CreatedBy = x.vc_created_by,
                        CreatedDate = Convert.ToDateTime(x.dt_created_date).ToString("yyyy-MM-dd")
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }
        //Author        : Siddhant Chawade
        //Date          : 20th Dec 2019
        //Description   : To get the list of option for adding timesheet & expenses
        public TimesheetOptions GetTimesheetOptions(long? userId)
        {
            TimesheetOptions data = new TimesheetOptions();
            List<RefNo> refNos = new List<RefNo>();
            List<Site> sites = new List<Site>();
            List<SOW> sows = new List<SOW>();
            List<Tasks> tasks = new List<Tasks>();
            List<JobCodes> jobCodes = new List<JobCodes>();
            List<ExpenseCodes> expCodes = new List<ExpenseCodes>();
            try
            {
                var refNo = _schedulingDB.sp_get_all_quotes().ToList().AsEnumerable().Select(x => new RefNo
                {
                    RefId = x.int_quote_id,
                    RefNumber = x.vc_quote_number,
                    RefStage = x.vc_stage_name
                }).ToList();

                var site = _schedulingDB.sp_get_all_site_names().ToList().AsEnumerable().Select(x => new Site
                {
                    SiteId = x.int_site_id,
                    SiteName = x.vc_site_name,
                    RefId = x.int_quote_id
                }).ToList();

                sows = _schedulingDB.sp_get_all_sow_names().ToList().AsEnumerable().Select(x => new SOW
                {
                    SOWId = x.int_sow_id,
                    SOWName = x.vc_sow_name,
                    SiteId = x.int_site_id,
                    SOWStatusId = x.int_sow_status_id,
                    SOWStatusName = x.vc_sow_status
                }).ToList();

                //var userJobCodes = _adminDB.sp_getJobCodesOfUser().Where(x => x.int_user_id == userId && x.bt_delete == false).ToList();

                tasks = _adminMethods.GetTasks();
                if (userId == 1)
                {
                    jobCodes = _adminMethods.GetJobCodes();
                }
                else
                {
                    jobCodes = _adminDB.sp_getJobCodesOfUser().Where(x => x.int_user_id == userId && x.bt_delete == false).ToList().AsEnumerable().Select(x => new JobCodes
                    {
                        JobCodeId = x.int_job_code_master_id,
                        JobCode = x.vc_job_code,
                        JobCodeTitle = x.vc_job_title
                    }).ToList();
                }
                expCodes = _adminMethods.GetExpenseCodes().OrderBy(x => x.ExpenseCode).ToList();

                foreach (var item in site)
                {
                    if ((sows.Where(x => x.SiteId == item.SiteId).ToList().Count > 0))
                    {
                        sites.Add(item);
                    }
                }
                foreach (var item in refNo)
                {
                    if ((sites.Where(x => x.RefId == item.RefId).ToList().Count > 0))
                    {
                        refNos.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            data.RefNos = refNos;
            data.Sites = sites;
            data.Sows = sows;
            data.Tasks = tasks;
            data.JobCodes = jobCodes;
            data.ExpenseCodes = expCodes;
            return data;
        }


        //Author        : Siddhant Chawade
        //Date          : 21st Dec 2019
        //Description   : To save timesheet and expenses
        public long? SaveTimesheetExpenses(Timesheet data)
        {
            long? result = (dynamic)null;
            long? timesheetId = (dynamic)null;
            try
            {
                List<ExpenseCodes> expenseData = _adminMethods.GetExpenseCodes();
                if (data != null)
                {
                    if (data.RefId != null)
                    {
                        _schedulingDB.sp_upsert_timesheet(data.TimesheetId, data.Date, data.RefId, data.SiteId, data.SowId, data.TaskId, data.JobCodeId,
                            data.WorkedHours, data.Remarks, data.IncludeExpenses, data.UserId, ref result);
                        timesheetId = result;
                    }
                    if (data.ExpensesList != null && data.ExpensesList.Count > 0)
                    {
                        string userName = string.Empty;
                        var user = _adminMethods.GetAllIBWUsers().Where(x => x.int_user_id == data.UserId).FirstOrDefault();
                        if (user != null)
                        {
                            userName = user.vc_user_name + " " + user.vc_alias_name;
                        }
                        foreach (var exp in data.ExpensesList)
                        {
                            if (exp.googleDriveFile != null)
                            {
                                string fileName = expenseData.Where(x => x.ExpenseCodeId == exp.ExpenseCodeId).FirstOrDefault().ExpenseCode + "-" + userName;
                                //string filePath = Path.Combine(HttpContext.Current.Server.MapPath("~/QuotePurposeAttachments"),
                                //                    Path.GetFileName(exp.googleDriveFile.FileName));
                                //exp.googleDriveFile.SaveAs(filePath);
                                exp.GoogleFileId = GoogleDriveFilesRepository.FileUploadInFolderDirect(HelperMethods.Literals.GDriveExpensegFolderId, exp.googleDriveFile, fileName, exp.googleDriveFile.ContentType);
                                //if (File.Exists(filePath))
                                //    File.Delete(filePath);
                            }
                            exp.Date = data.Date != null ? data.Date : exp.Date;
                            exp.RefId = data.RefId != null ? data.RefId : exp.RefId;
                            exp.SiteId = data.SiteId != null ? data.SiteId : exp.SiteId;
                            exp.SowId = data.SowId != null ? data.SowId : exp.SowId;

                            //exp.RefId = exp.RefId != null ? exp.RefId : data.RefId;
                            //exp.SiteId = exp.SiteId != null ? exp.SiteId : data.SiteId;
                            //exp.SowId = exp.SowId != null ? exp.SowId : data.SowId;
                            exp.TaskId = data.TaskId != null ? data.TaskId : exp.TaskId;
                            //exp.TaskId = exp.TaskId != null ? exp.TaskId : data.TaskId;
                            exp.JobCodeId = exp.JobCodeId != null ? exp.JobCodeId : data.JobCodeId;
                            exp.UserId = exp.UserId != null ? exp.UserId : data.UserId;
                            exp.TimesheetId = exp.TimesheetId != null ? exp.TimesheetId : timesheetId;
                            exp.Total = exp.Rate * exp.Quantity;
                            _schedulingDB.sp_upsert_expense(exp.ExpenseId, exp.TimesheetId, exp.Date, exp.RefId, exp.SiteId, exp.SowId, exp.TaskId, exp.JobCodeId,
                                exp.ExpenseCodeId, exp.Quantity, exp.Rate, exp.Total, exp.IsReimbursable, exp.Description, exp.GoogleFileId, exp.UserId, ref result);
                        }
                        if (data.DeletedExpensesList != null && data.DeletedExpensesList.Count > 0)

                        {
                            //var datasss = _schedulingDB.tbl_ibw_expenses.ToList();
                            var deletedExpensesList = _schedulingDB.tbl_ibw_expense_documents.Where(x => data.DeletedExpensesList.Contains(x.int_expense_id) && x.vc_google_drive_file_id != null && x.vc_google_drive_file_id != "").Select(y => y.vc_google_drive_file_id).ToList();
                            _quoteMethods.deleteGoogleDriveFilesfromList(deletedExpensesList);
                            //var datasssss = _schedulingDB.tbl_ibw_expense_documents.Where(x => data.DeletedExpensesList.Contains(x.int_expense_id)).ToList();

                            using (var con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                            {
                                string totalExpensItems = "(" + string.Join(",", data.DeletedExpensesList) + ")";
                                string sqlCommand = "delete from tbl_ibw_expense_documents  where int_expense_id in" + totalExpensItems + ";" + "delete from tbl_ibw_timesheet_expenses_notes where int_expense_id in" + totalExpensItems + ";" + "delete from tbl_ibw_expense where int_expense_id in" + totalExpensItems;
                                using (var cmd = new SqlCommand(sqlCommand, con))
                                {
                                    //  cmd.CommandType = CommandType.TableDirect;
                                    con.Open();
                                    cmd.ExecuteNonQuery();
                                    con.Close();
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        //Author        : Siddhant Chawade
        //Date          : 23rd Dec 2019
        //Description   : To get the list timesheet
        public Timesheets GetTimesheets(long? userId, bool isSuperAdminMode)
        {
            Timesheets data = new Timesheets();
            List<Timesheet> timesheetList = new List<Timesheet>();
            List<Timesheet> thisWeekTimesheetList = new List<Timesheet>();
            List<Timesheet> nextWeekTimesheetList = new List<Timesheet>();
            List<DayHours> todayList = new List<DayHours>();
            List<DayHours> tomorrowList = new List<DayHours>();
            List<DayHours> thisWeekList = new List<DayHours>();
            List<DayHours> nextWeekList = new List<DayHours>();
            List<DayHours> thisWeekProjectList = new List<DayHours>();
            List<DayHours> nextWeekProjectList = new List<DayHours>();
            DateTime today = DateTime.Today;
            DateTime currentDay = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            DateTime sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime saturday = (DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday)).AddDays(1);
            List<DateTime?> thisWeekDates = new List<DateTime?>();
            List<DateTime?> nextWeekDates = new List<DateTime?>();

            var userHoursPerDay = userId == 1 ? 0 : _adminMethods.GetUsers().Where(x => x.userId == userId).FirstOrDefault().hoursPerDay;

            //getting list of reporting managers
            var managers = _adminDB.sp_getAllReportingManagers().ToList();

            //to get approved leaves of the user
            List<GetLeaveRequestsResult> LeaveRequests = _adminDB.GetLeaveRequests().Where(x => x.int_created_by == userId).ToList();
            ////if user is super admin or in super admin mode return leaves of all users
            //if (userId == 1 || isSuperAdminMode)
            //{

            //}
            ////if user is reporting manager return user's leaves and reportee's leaves
            //else if (managers.Exists(m => m.int_user_id == userId))
            //{
            //    LeaveRequests = LeaveRequests.Where(x => x.int_rm_id == userId || x.int_created_by == userId).ToList();
            //}
            ////else return only user's leaves
            //else
            //    LeaveRequests = LeaveRequests.Where(x => x.int_created_by == userId).ToList();
            data.Leaves = LeaveRequests;

            //to get list of holidays
            List<GetHolidaysResult> Holidays = _adminMethods.GetHolidays();
            data.Holidays = Holidays;
            try
            {
                //for this week view
                today = sunday;
                while (today <= saturday)
                {
                    thisWeekDates.Add(today);
                    today = today.AddDays(1);
                }
                //for last week view
                today = sunday.AddDays(-7);
                saturday = saturday.AddDays(-7);
                while (today <= saturday)
                {
                    nextWeekDates.Add(today);
                    today = today.AddDays(1);
                }

                timesheetList = _schedulingDB.sp_get_user_timesheet().Where(x => x.userId == userId).OrderBy(x => x.timesheetId).ToList().AsEnumerable().Select(x => new Timesheet
                {
                    TimesheetId = x.timesheetId,
                    Date = Convert.ToDateTime(x.completionDate),
                    RefNo = x.quoteNumber,
                    SiteName = x.siteName,
                    SowName = x.SOWName,
                    TaskName = x.taskName,
                    ActionName = x.actionName,
                    JobCode = x.jobCodeName,
                    JobCodeTitle = x.jobCodeTitle,
                    WorkedHours = Convert.ToDecimal(x.workedHours),
                    Approved = x.isApproved,
                    Status = x.completionStatus
                    //Remarks = x.n,
                    //IncludeExpenses = x.bt_include_expense
                }).ToList();

                var todayTimesheets = timesheetList.Where(x => x.Date == DateTime.Today).ToList();
                if (todayTimesheets.Count > 0)
                {
                    decimal? totalCount = 0;
                    List<string> RefNos = new List<string>();
                    foreach (var item in todayTimesheets)
                    {
                        if (RefNos.Contains(item.RefNo))
                        {
                            todayList.Where(x => x.Day == item.RefNo).FirstOrDefault().Hours = todayList.Where(x => x.Day == item.RefNo).FirstOrDefault().Hours + item.WorkedHours;
                            totalCount = totalCount + item.WorkedHours;
                        }
                        else
                        {
                            DayHours dayHour = new DayHours();
                            dayHour.Day = item.RefNo;
                            dayHour.Hours = item.WorkedHours;
                            RefNos.Add(item.RefNo);
                            todayList.Add(dayHour);
                            totalCount = totalCount + item.WorkedHours;
                        }
                    }
                    todayList.Add(new DayHours() { Day = "Total Hours Submitted", Hours = totalCount });
                }

                var tomorrowTimesheets = timesheetList.Where(x => x.Date == DateTime.Today.AddDays(-1)).ToList();
                if (tomorrowTimesheets.Count > 0)
                {
                    decimal? totalCount = 0;
                    List<string> RefNos = new List<string>();
                    foreach (var item in tomorrowTimesheets)
                    {
                        if (RefNos.Contains(item.RefNo))
                        {
                            tomorrowList.Where(x => x.Day == item.RefNo).FirstOrDefault().Hours = tomorrowList.Where(x => x.Day == item.RefNo).FirstOrDefault().Hours + item.WorkedHours;
                            totalCount = totalCount + item.WorkedHours;
                        }
                        else
                        {
                            DayHours dayHour = new DayHours();
                            dayHour.Day = item.RefNo;
                            dayHour.Hours = item.WorkedHours;
                            RefNos.Add(item.RefNo);
                            tomorrowList.Add(dayHour);
                            totalCount = totalCount + item.WorkedHours;
                        }
                    }
                    tomorrowList.Add(new DayHours() { Day = "Total Hours Submitted", Hours = totalCount });
                }

                thisWeekTimesheetList = timesheetList.Where(x => thisWeekDates.Contains(x.Date)).ToList();
                if (thisWeekTimesheetList.Count > 0)
                {
                    decimal? totalCount = 0;
                    List<string> RefNos = new List<string>();
                    foreach (var item in thisWeekTimesheetList)
                    {
                        if (RefNos.Contains(item.RefNo))
                        {
                            thisWeekProjectList.Where(x => x.Day == item.RefNo).FirstOrDefault().Hours = thisWeekProjectList.Where(x => x.Day == item.RefNo).FirstOrDefault().Hours + item.WorkedHours;
                            totalCount = totalCount + item.WorkedHours;
                        }
                        else
                        {
                            DayHours dayHour = new DayHours();
                            dayHour.Day = item.RefNo;
                            dayHour.Hours = item.WorkedHours;
                            RefNos.Add(item.RefNo);
                            thisWeekProjectList.Add(dayHour);
                            totalCount = totalCount + item.WorkedHours;
                        }
                    }
                    thisWeekProjectList.Add(new DayHours() { Day = "Total Hours Submitted", Hours = totalCount });
                }

                nextWeekTimesheetList = timesheetList.Where(x => nextWeekDates.Contains(x.Date)).ToList();
                if (nextWeekTimesheetList.Count > 0)
                {
                    decimal? totalCount = 0;
                    List<string> RefNos = new List<string>();
                    foreach (var item in nextWeekTimesheetList)
                    {
                        if (RefNos.Contains(item.RefNo))
                        {
                            nextWeekProjectList.Where(x => x.Day == item.RefNo).FirstOrDefault().Hours = nextWeekProjectList.Where(x => x.Day == item.RefNo).FirstOrDefault().Hours + item.WorkedHours;
                            totalCount = totalCount + item.WorkedHours;
                        }
                        else
                        {
                            DayHours dayHour = new DayHours();
                            dayHour.Day = item.RefNo;
                            dayHour.Hours = item.WorkedHours;
                            RefNos.Add(item.RefNo);
                            nextWeekProjectList.Add(dayHour);
                            totalCount = totalCount + item.WorkedHours;
                        }
                    }
                    nextWeekProjectList.Add(new DayHours() { Day = "Total Hours Submitted", Hours = totalCount });
                }

                thisWeekList = GetTimesheetWeekSummary(thisWeekDates, thisWeekTimesheetList, Holidays, LeaveRequests.ToList(), userHoursPerDay);
                nextWeekList = GetTimesheetWeekSummary(nextWeekDates, nextWeekTimesheetList, Holidays, LeaveRequests.ToList(), userHoursPerDay);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            data.TimesheetList = timesheetList.OrderBy(x => x.Date).ToList();
            data.ThisWeekTimesheetList = thisWeekTimesheetList.OrderBy(x => x.Date).ToList();
            data.NextWeeKTimesheetList = nextWeekTimesheetList.OrderBy(x => x.Date).ToList();
            data.Today = todayList;
            data.Tomorrow = tomorrowList;
            data.ThisWeek = thisWeekList;
            data.NextWeek = nextWeekList;
            data.ThisWeekProjects = thisWeekProjectList;
            data.NextWeekProjects = nextWeekProjectList;
            data.ThisWeekStart = thisWeekDates[0];
            data.ThisWeekEnd = thisWeekDates[thisWeekDates.Count - 1];
            data.LastWeekStart = nextWeekDates[0];
            data.LastWeekEnd = nextWeekDates[nextWeekDates.Count - 1];
            return data;
        }

        //Author        : Siddhant Chawade
        //Date          : 23rd Dec 2019
        //Description   : To get the list of timesheet summary by week
        public List<DayHours> GetTimesheetWeekSummary(List<DateTime?> Dates, List<Timesheet> timesheetList, List<GetHolidaysResult> Holidays, List<GetLeaveRequestsResult> LeaveRequests, decimal? hoursPerDay)
        {
            List<DayHours> data = new List<DayHours>();
            try
            {
                decimal? mondayCount = 0;
                decimal? tuesdayCount = 0;
                decimal? wednesdayCount = 0;
                decimal? thrusdayCount = 0;
                decimal? fridayCount = 0;
                decimal? saturdayCount = 0;
                decimal? sundayCount = 0;
                decimal? totalCount = 0;

                decimal? mondayLeave = 0;
                decimal? tuesdayLeave = 0;
                decimal? wednesdayLeave = 0;
                decimal? thrusdayLeave = 0;
                decimal? fridayLeave = 0;
                decimal? saturdayLeave = 0;
                decimal? sundayLeave = 0;
                decimal? totalLeave = 0;

                decimal? mondayHoliday = 0;
                decimal? tuesdayHoliday = 0;
                decimal? wednesdayHoliday = 0;
                decimal? thrusdayHoliday = 0;
                decimal? fridayHoliday = 0;
                decimal? saturdayHoliday = 0;
                decimal? sundayHoliday = 0;
                decimal? totalHoliday = 0;
                foreach (var date in Dates)
                {
                    bool isHoliday = false;
                    bool isLeave = false;
                    isHoliday = Holidays.Where(x => x.holidayDate == date).ToList().Count > 0 ? true : false;
                    isLeave = LeaveRequests.Where(x => ((x.dt_from_date == date && x.dt_to_date == null) || (x.dt_from_date <= date && x.dt_to_date != null && x.dt_to_date >= date))).ToList().Count > 0 ? true : false;

                    var leaveToday = LeaveRequests.Where(x => ((x.dt_from_date == date && x.dt_to_date == null) || (x.dt_from_date <= date && x.dt_to_date != null && x.dt_to_date >= date))).FirstOrDefault();
                    double? LeaveHours = 0;
                    if (isLeave)
                    {
                        LeaveHours = Convert.ToDouble(leaveToday.int_duration);
                        if (leaveToday.vc_absence_type == "Multiple Days")
                        {
                            var numOfDays = (Convert.ToDateTime(leaveToday.dt_to_date) - Convert.ToDateTime(leaveToday.dt_from_date)).TotalDays + 1;
                            LeaveHours = LeaveHours / numOfDays;
                        }
                    }

                    var day = Convert.ToDateTime(date).DayOfWeek;
                    if (day == DayOfWeek.Sunday)
                    {
                        if (isLeave)
                        {
                            sundayLeave = Convert.ToDecimal(LeaveHours);
                            totalLeave = totalLeave + sundayLeave;
                        }
                        if (isHoliday)
                        {
                            sundayHoliday = hoursPerDay;
                            totalHoliday = totalHoliday + hoursPerDay;
                        }
                    }
                    else if (day == DayOfWeek.Monday)
                    {
                        if (isLeave)
                        {
                            mondayLeave = Convert.ToDecimal(LeaveHours);
                            totalLeave = totalLeave + mondayLeave;
                        }
                        if (isHoliday)
                        {
                            mondayHoliday = hoursPerDay;
                            totalHoliday = totalHoliday + hoursPerDay;
                        }
                    }
                    else if (day == DayOfWeek.Tuesday)
                    {
                        if (isLeave)
                        {
                            tuesdayLeave = Convert.ToDecimal(LeaveHours);
                            totalLeave = totalLeave + tuesdayLeave;
                        }
                        if (isHoliday)
                        {
                            tuesdayHoliday = hoursPerDay;
                            totalHoliday = totalHoliday + hoursPerDay;
                        }
                    }
                    else if (day == DayOfWeek.Wednesday)
                    {
                        if (isLeave)
                        {
                            wednesdayLeave = Convert.ToDecimal(LeaveHours);
                            totalLeave = totalLeave + wednesdayLeave;
                        }
                        if (isHoliday)
                        {
                            wednesdayHoliday = hoursPerDay;
                            totalHoliday = totalHoliday + hoursPerDay;
                        }
                    }
                    else if (day == DayOfWeek.Thursday)
                    {
                        if (isLeave)
                        {
                            thrusdayLeave = Convert.ToDecimal(LeaveHours);
                            totalLeave = totalLeave + thrusdayLeave;
                        }
                        if (isHoliday)
                        {
                            thrusdayHoliday = hoursPerDay;
                            totalHoliday = totalHoliday + hoursPerDay;
                        }
                    }
                    else if (day == DayOfWeek.Friday)
                    {
                        if (isLeave)
                        {
                            fridayLeave = Convert.ToDecimal(LeaveHours);
                            totalLeave = totalLeave + fridayLeave;
                        }
                        if (isHoliday)
                        {
                            fridayHoliday = hoursPerDay;
                            totalHoliday = totalHoliday + hoursPerDay;
                        }
                    }
                    else if (day == DayOfWeek.Saturday)
                    {
                        if (isLeave)
                        {
                            saturdayLeave = Convert.ToDecimal(LeaveHours);
                            totalLeave = totalLeave + saturdayLeave;
                        }
                        if (isHoliday)
                        {
                            saturdayHoliday = hoursPerDay;
                            totalHoliday = totalHoliday + hoursPerDay;
                        }
                    }
                }
                if (timesheetList != null && timesheetList.Count > 0)
                {
                    foreach (var item in timesheetList)
                    {
                        var day = Convert.ToDateTime(item.Date).DayOfWeek;
                        if (day == DayOfWeek.Sunday)
                        {
                            sundayCount = sundayCount + item.WorkedHours;
                        }
                        else if (day == DayOfWeek.Monday)
                        {
                            mondayCount = mondayCount + item.WorkedHours;
                        }
                        else if (day == DayOfWeek.Tuesday)
                        {
                            tuesdayCount = tuesdayCount + item.WorkedHours;
                        }
                        else if (day == DayOfWeek.Wednesday)
                        {
                            wednesdayCount = wednesdayCount + item.WorkedHours;
                        }
                        else if (day == DayOfWeek.Thursday)
                        {
                            thrusdayCount = thrusdayCount + item.WorkedHours;
                        }
                        else if (day == DayOfWeek.Friday)
                        {
                            fridayCount = fridayCount + item.WorkedHours;
                        }
                        else if (day == DayOfWeek.Saturday)
                        {
                            saturdayCount = saturdayCount + item.WorkedHours;
                        }
                        totalCount = totalCount + item.WorkedHours;
                    }
                }
                data.Add(new DayHours() { Leaves = mondayLeave, Holidays = mondayHoliday, Day = "Mon | " + Convert.ToDateTime(Dates[0]).ToString("MM-dd"), Hours = mondayCount });
                data.Add(new DayHours() { Leaves = tuesdayLeave, Holidays = tuesdayHoliday, Day = "Tue | " + Convert.ToDateTime(Dates[1]).ToString("MM-dd"), Hours = tuesdayCount });
                data.Add(new DayHours() { Leaves = wednesdayLeave, Holidays = wednesdayHoliday, Day = "Wed | " + Convert.ToDateTime(Dates[2]).ToString("MM-dd"), Hours = wednesdayCount });
                data.Add(new DayHours() { Leaves = thrusdayLeave, Holidays = thrusdayHoliday, Day = "Thu | " + Convert.ToDateTime(Dates[3]).ToString("MM-dd"), Hours = thrusdayCount });
                data.Add(new DayHours() { Leaves = fridayLeave, Holidays = fridayHoliday, Day = "Fri | " + Convert.ToDateTime(Dates[4]).ToString("MM-dd"), Hours = fridayCount });
                data.Add(new DayHours() { Leaves = saturdayLeave, Holidays = saturdayHoliday, Day = "Sat | " + Convert.ToDateTime(Dates[5]).ToString("MM-dd"), Hours = saturdayCount });
                data.Add(new DayHours() { Leaves = sundayLeave, Holidays = sundayHoliday, Day = "Sun | " + Convert.ToDateTime(Dates[6]).ToString("MM-dd"), Hours = sundayCount });
                data.Add(new DayHours() { Leaves = totalLeave, Holidays = totalHoliday, Day = "Total Hours", Hours = totalCount });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }

        //Author        : Siddhant Chawade
        //Date          : 24th Dec 2019
        //Description   : To get the list of expenses
        public GetExpenses GetExpenses(long? userId, bool isSuperAdminMode)
        {
            GetExpenses data = new GetExpenses();
            List<Expenses> expenseList = new List<Expenses>();
            List<Expenses> thisWeekExpenseList = new List<Expenses>();
            List<Expenses> nextWeekExpenseList = new List<Expenses>();
            List<Expenses> thisMonthExpenseList = new List<Expenses>();
            List<Expenses> nextMonthExpenseList = new List<Expenses>();
            decimal? thisWeekSubmitted = 0;
            decimal? nextWeekSubmitted = 0;
            decimal? thisMonthSubmitted = 0;
            decimal? nextMonthSubmitted = 0;
            decimal? thisWeekApproved = 0;
            decimal? nextWeekApproved = 0;
            decimal? thisMonthApproved = 0;
            decimal? nextMonthApproved = 0;
            DateTime today = DateTime.Today;
            DateTime currentDay = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            DateTime sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime saturday = (DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday)).AddDays(1);
            List<DateTime?> thisWeekDates = new List<DateTime?>();
            List<DateTime?> nextWeekDates = new List<DateTime?>();

            //getting list of reporting managers
            var managers = _adminDB.sp_getAllReportingManagers().ToList();

            //to get approved leaves of the user
            List<GetLeaveRequestsResult> LeaveRequests = _adminDB.GetLeaveRequests().Where(x => x.is_approved == true).ToList();
            //if user is super admin or in super admin mode return leaves of all users
            if (userId == 1 || isSuperAdminMode)
            {

            }
            //if user is reporting manager return user's leaves and reportee's leaves
            else if (managers.Exists(m => m.int_user_id == userId))
            {
                LeaveRequests = LeaveRequests.Where(x => x.int_rm_id == userId || x.int_created_by == userId).ToList();
            }
            //else return only user's leaves
            else
                LeaveRequests = LeaveRequests.Where(x => x.int_created_by == userId).ToList();
            data.Leaves = LeaveRequests;

            //to get list of holidays
            List<GetHolidaysResult> Holidays = _adminMethods.GetHolidays();
            data.Holidays = Holidays;
            try
            {
                //for this week 
                today = sunday;
                while (today <= saturday)
                {
                    thisWeekDates.Add(today);
                    today = today.AddDays(1);
                }
                //for last week 
                today = sunday.AddDays(-7);
                saturday = saturday.AddDays(-7);
                while (today <= saturday)
                {
                    nextWeekDates.Add(today);
                    today = today.AddDays(1);
                }

                expenseList = _schedulingDB.sp_get_pm_expenses().Where(x => x.userId == userId).OrderBy(x => x.expenseId).ToList().AsEnumerable().Select(x => new Expenses
                {
                    ExpenseId = x.expenseId,
                    TimesheetId = x.timesheetId,
                    strDate = x.completionDate,
                    //RefId = x.int_quote_id,
                    RefNo = x.quoteNumber,
                    //SiteId = x.int_site_id,
                    SiteName = x.siteName,
                    //SowId = x.int_sow_id,
                    SowName = x.SOWName,
                    //TaskId = x.int_task_id,
                    TaskName = x.taskName,
                    ActionName = x.actionName,
                    //JobCodeId = x.int_jobcode_id,
                    JobCode = x.jobCodeName,
                    JobCodeTitle = x.jobCodeTitle,
                    //ExpenseCodeId = x.int_expense_code_id,
                    ExpenseCode = x.expenseCode,
                    LimitType = x.limitType,
                    Quantity = Convert.ToInt32(x.Quantity),
                    Total = x.Quantity * x.expenseRate,
                    IsReimbursable = x.isReimbursable == "Yes" ? true : false,
                    Status = "",
                    Approved = x.isApproved
                }).ToList();

                thisWeekExpenseList = expenseList.Where(x => x.strDate != null && x.strDate != "-" && thisWeekDates.Contains(Convert.ToDateTime(x.strDate))).ToList();
                foreach (var item in thisWeekExpenseList)
                {
                    thisWeekSubmitted = thisWeekSubmitted + item.Total;
                    if (item.Approved == true)
                        thisWeekApproved = thisWeekApproved + item.Total;
                }

                nextWeekExpenseList = expenseList.Where(x => x.strDate != null && x.strDate != "-" && nextWeekDates.Contains(Convert.ToDateTime(x.strDate))).ToList();
                foreach (var item in nextWeekExpenseList)
                {
                    nextWeekSubmitted = nextWeekSubmitted + item.Total;
                    if (item.Approved == true)
                        nextWeekApproved = nextWeekApproved + item.Total;
                }

                thisMonthExpenseList = expenseList.Where(x => x.strDate != null && x.strDate != "-" && Convert.ToDateTime(Convert.ToDateTime(x.strDate)).Month == DateTime.Now.Month && Convert.ToDateTime(Convert.ToDateTime(x.strDate)).Year == DateTime.Now.Year).ToList();
                foreach (var item in thisMonthExpenseList)
                {
                    thisMonthSubmitted = thisMonthSubmitted + item.Total;
                    if (item.Approved == true)
                        thisMonthApproved = thisMonthApproved + item.Total;
                }

                nextMonthExpenseList = expenseList.Where(x => x.strDate != null && x.strDate != "-" && Convert.ToDateTime(Convert.ToDateTime(x.strDate)).Month == DateTime.Now.AddMonths(-1).Month && Convert.ToDateTime(Convert.ToDateTime(x.strDate)).Year == DateTime.Now.AddMonths(-1).Year).ToList();
                foreach (var item in nextMonthExpenseList)
                {
                    nextMonthSubmitted = nextMonthSubmitted + item.Total;
                    if (item.Approved == true)
                        nextMonthApproved = nextMonthApproved + item.Total;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            data.ExpensesList = expenseList;
            data.ThisWeekExpensesList = thisWeekExpenseList.OrderBy(x => x.Date).ToList();
            data.NextWeekExpensesList = nextWeekExpenseList.OrderBy(x => x.Date).ToList();
            data.ThisMonthExpensesList = thisMonthExpenseList.OrderBy(x => x.Date).ToList();
            data.NextMonthExpensesList = nextMonthExpenseList.OrderBy(x => x.Date).ToList();
            data.ThisWeekExpensesSubmitted = thisWeekSubmitted;
            data.NextWeekExpensesSubmitted = nextWeekSubmitted;
            data.ThisMonthExpensesSubmitted = thisMonthSubmitted;
            data.NextMonthExpensesSubmitted = nextMonthSubmitted;
            data.ThisWeekExpensesAproved = thisWeekApproved;
            data.NextWeekExpensesAproved = nextWeekApproved;
            data.ThisMonthExpensesApproved = thisMonthApproved;
            data.NextMonthExpensesApproved = nextMonthApproved;
            data.ThisWeekStart = thisWeekDates[0];
            data.ThisWeekEnd = thisWeekDates[thisWeekDates.Count - 1];
            data.LastWeekStart = nextWeekDates[0];
            data.LastWeekEnd = nextWeekDates[nextWeekDates.Count - 1];
            data.ThisMonthStart = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
            data.ThisMonthEnd = DateTime.Today.AddDays(-(DateTime.Today.Day - 1)).AddMonths(1).AddDays(-1);
            data.LastMonthStart = DateTime.Today.AddDays(-(DateTime.Today.Day - 1)).AddMonths(-1);
            data.LastMonthEnd = DateTime.Today.AddDays(-(DateTime.Today.Day - 1)).AddDays(-1);
            return data;
        }
        #endregion
        #region PM-TM 
        public List<sp_get_user_timesheet_nonactionResult> GetNonActionTimeSheets(long? projectManagerId)
        {
            return _schedulingDB.sp_get_user_timesheet_nonaction(projectManagerId).OrderByDescending(x => x.dt_created_date).ToList();
        }
        public List<sp_get_pm_expenses_nonactionResult> GetNonActionExpenses(long? projectManagerId)
        {
            return _schedulingDB.sp_get_pm_expenses_nonaction(projectManagerId).OrderByDescending(x => x.dt_created_date).ToList();
        }

        public List<sp_get_user_timesheetResult> GetTimeSheets(long? projectManagerId)
        {
            return _schedulingDB.sp_get_user_timesheet().Where(x => (projectManagerId == null || x.projectManagerId == projectManagerId)).OrderByDescending(x => x.dt_created_date).ToList();
        }
        public List<sp_get_pm_expensesResult> GetPMExpenses(long? projectManagerId)
        {
            return _schedulingDB.sp_get_pm_expenses().Where(x => (projectManagerId == null || x.projectManagerId == projectManagerId)).OrderByDescending(x => x.dt_created_date).ToList();
        }
        public long? ApproveRejectTimesheetExpense(TimeAndExpenses data, long? userId)
        {
            try
            {
                long? result = null;
                DataTable dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[2] {
                    new DataColumn("Id", typeof(int)),
                    new DataColumn("isAssigned", typeof(bool)),
                });
                foreach (var row in data.TimeSheetExpenseItems)
                {
                    int id = Convert.ToInt32(row.Id);
                    bool isAssigned = Convert.ToBoolean(row.isAssigned);
                    dt.Rows.Add(id, isAssigned);
                }
                using (var con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    using (var cmd = new SqlCommand("sp_ApproveRejectTimesheetsExpenses", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@isApprove", data.IsApprove);
                        cmd.Parameters.AddWithValue("@TimesheetOrExpense", data.TimesheetOrExpense);
                        cmd.Parameters.AddWithValue("@items", dt);
                        cmd.Parameters.AddWithValue("@user_id", userId);
                        if (data.Remarks != null)
                        {
                            cmd.Parameters.AddWithValue("@pm_message", data.Remarks);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@pm_message", DBNull.Value);
                        }
                        //Add the output parameter to the command object
                        SqlParameter outPutParameter = new SqlParameter();
                        outPutParameter.ParameterName = "@result";
                        outPutParameter.SqlDbType = System.Data.SqlDbType.Int;
                        outPutParameter.Direction = System.Data.ParameterDirection.Output;
                        cmd.Parameters.Add(outPutParameter);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        result = Convert.ToInt32(outPutParameter.Value);
                    }
                }
                //FirebaseCloudMessaging.SendNotification(1, "Hello", "Timesheet Approved", "54545");
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public long? ApproveRejectLoggedHours(List<long> Ids, bool IsApprove, string TimesheetOrExpense, string Remarks, long? userId)
        {
            try
            {
                long? result = null;
                DataTable dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[2] {
                    new DataColumn("Id", typeof(int)),
                    new DataColumn("isAssigned", typeof(bool)),
                });
                foreach (var row in Ids)
                {
                    int id = Convert.ToInt32(row);
                    bool isAssigned = Convert.ToBoolean(true);
                    dt.Rows.Add(id, isAssigned);
                }
                using (var con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    using (var cmd = new SqlCommand("sp_ApproveRejectTimesheetsExpenses", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@isApprove", IsApprove);
                        cmd.Parameters.AddWithValue("@TimesheetOrExpense", TimesheetOrExpense);
                        cmd.Parameters.AddWithValue("@items", dt);
                        cmd.Parameters.AddWithValue("@user_id", userId);
                        if (Remarks != null)
                        {
                            cmd.Parameters.AddWithValue("@pm_message", Remarks);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@pm_message", DBNull.Value);
                        }
                        //Add the output parameter to the command object
                        SqlParameter outPutParameter = new SqlParameter();
                        outPutParameter.ParameterName = "@result";
                        outPutParameter.SqlDbType = System.Data.SqlDbType.Int;
                        outPutParameter.Direction = System.Data.ParameterDirection.Output;
                        cmd.Parameters.Add(outPutParameter);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        result = Convert.ToInt32(outPutParameter.Value);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool ApproveRejectLoggedHours(ReviewTimesheetExpense data, long? userId)
        {
            try
            {
                if (data.TimeSheet != null)
                    ApproveRejectLoggedHours(new List<long> { data.TimeSheet.int_action_log_hour_id }, data.IsApprove, "timesheet", data.PMRemarks, userId);
                if (data.Expenses != null && data.Expenses.Count > 0)
                    ApproveRejectLoggedHours(data.Expenses.Select(x => x.expenseId).ToList(), data.IsApprove, "expense", data.PMRemarks, userId);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}