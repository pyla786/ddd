using API_IBW.Business_Classes;
using API_IBW.DB_Models;
using API_IBW.HelperMethods;
using API_IBW.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace API_IBW.Controllers
{
    public class SchedulingController : ApiController
    {
        HttpResponseMessage response;
        public SchedulingMethods _schMethods;

        public SchedulingController()
        {
            //creating required objects
            response = new HttpResponseMessage();
            _schMethods = new SchedulingMethods();
        }

        #region Action Scheduling
        //Author        : Siddhant Chawade
        //Date          : 4th Jan 2020
        //Description   : To get scheduled action activity log
        [Route("api/get-scheduled-actions-activity-log")]
        [HttpGet]
        public HttpResponseMessage GetSecheduledActionActivityLog(long? manageActionId)
        {
            Response<List<ScheduledActionActivityLog>> output = new Response<List<ScheduledActionActivityLog>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetSecheduledActionActivityLog(manageActionId);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }
        #region Grid View Tab
        //Author        : Siddhant Chawade
        //Date          : 30th Mar 2019
        //Description   : To get the list of actions scheduled to a user on a date
        [Route("api/get-actions-by-user-schdate")]
        [HttpPost]
        public HttpResponseMessage ActionScheduledToUser([FromBody]ActionScheduledToUser data)
        {
            Response<List<ActionScheduledToUser>> output = new Response<List<ActionScheduledToUser>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.ActionScheduledToUser(data);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }
        //Author        : Siddhant Chawade
        //Date          : 2nd Dec 2019
        //Description   : To get the list of actions for scheduling
        [Route("api/get-actions-for-scheduling")]
        [HttpPost]
        public HttpResponseMessage GetActionSchedulingList([FromBody]ActionScheduledToUser data)
        {
            Response<List<ActionScheduling>> output = new Response<List<ActionScheduling>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetActionSchedulingList(data);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }
        //Author        : Siddhant Chawade
        //Date          : 28th Jan 2020
        //Description   : To delete manage action
        [Route("api/delete-manage-action")]
        [HttpGet]
        public HttpResponseMessage DeleteManageAction(long? manageActionId)
        {
            Response<List<ActionHistory>> output = new Response<List<ActionHistory>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                if (_schMethods.DeleteManageAction(manageActionId) > 0)
                {
                    output.Status = true;
                    output.Message = "Action deleted successfully";
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong, please contact administrator";
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        //Author        : Siddhant Chawade
        //Date          : 2nd Dec 2019
        //Description   : To get the list of actions for scheduling
        [Route("api/upsert-action-due-date")]
        [HttpPost]
        public HttpResponseMessage UpsertAcionDueDdate([FromBody]ActionScheduling data)
        {
            Response<ActionScheduling> output = new Response<ActionScheduling>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                if (data != null)
                {
                    Logging.EventLog(headerUserId, "Attempt to update due date of an action");
                    if (_schMethods.UpsertAcionDueDdate(data) > 0)
                    {
                        output.Status = true;
                        output.Message = "Due date saved successfully";
                        Logging.EventLog(headerUserId, "Due date saved successfully");
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "Something went wrong";
                    }
                }
                else
                {
                    output.Status = false;
                    output.Message = "Invalid data";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            return response;
        }

        //Author        : Siddhant Chawade
        //Date          : 7th Dec 2019
        //Description   : To save scheduled action details
        [Route("api/save-scheduled-action-details")]
        [HttpPost]
        public HttpResponseMessage SaveScheduledAcionDetails([FromBody]List<ActionScheduling> data)
        {
            Response<ActionScheduling> output = new Response<ActionScheduling>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                if (data != null && data.Count > 0)
                {
                    Logging.EventLog(headerUserId, "Attempt to schedule actions");
                    if (_schMethods.SaveScheduledAcionDetails(data, headerUserId) > 0)
                    {
                        output.Status = true;
                        output.Message = "Actions scheduled successfully";
                        Logging.EventLog(headerUserId, "Actions scheduled successfully");
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "Something went wrong";
                    }
                }
                else
                {
                    output.Status = false;
                    output.Message = "Invalid data";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            return response;
        }

        //Author        : Siddhant Chawade
        //Date          : 2nd Dec 2019
        //Description   : To get the list of actions scheduled to a user on a specific date
        [Route("api/get-actions-scheduled-to-user")]
        [HttpPost]
        public HttpResponseMessage GetActionScheduledToUser([FromBody]ActionScheduledToUser data)
        {
            Response<List<ActionScheduledToUser>> output = new Response<List<ActionScheduledToUser>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetActionScheduledToUser(data.UserId, data.ScheduledDate, data.TaskId);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        //Author        : Siddhant Chawade
        //Date          : 17th Dec 2019
        //Description   : To get the history of actions 
        [Route("api/get-action-history")]
        [HttpPost]
        public HttpResponseMessage GetActionHistory([FromBody]ActionHistory data)
        {
            Response<List<ActionHistory>> output = new Response<List<ActionHistory>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetActionHistory(data);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        #endregion

        #region Board Tab
        //Author        : Siddhant Chawade
        //Date          : 2nd Mar 2020
        //Description   : To update sequence of scehduled actions
        [Route("api/update-scheduled-actions-sequence")]
        [HttpPost]
        public HttpResponseMessage UpdateScheduledActionsSequence([FromBody]List<long?> data)
        {
            Response<List<ScheduledAction>> output = new Response<List<ScheduledAction>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                if (_schMethods.UpdateScheduledActionsSequence(data) > 0)
                {
                    output.Status = true;
                    output.Message = "";
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong, please contact administrator";
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }
        //Author        : Siddhant Chawade
        //Date          : 10th Dec 2019
        //Description   : To get the list of scheduled actions
        [Route("api/get-scheduled-actions")]
        [HttpPost]
        public HttpResponseMessage GetScheduledActions([FromBody]ActionScheduledToUser data)
        {
            Response<List<ScheduledAction>> output = new Response<List<ScheduledAction>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetScheduledActions(data);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        //Author        : Siddhant Chawade
        //Date          : 11th Dec 2019
        //Description   : To get the details of scheduled actions
        [Route("api/get-scheduled-action-details")]
        [HttpGet]
        public HttpResponseMessage GetScheduledActionDetails(long? detailId, long? manageActionId)
        {
            Response<ScheduledActionDetail> output = new Response<ScheduledActionDetail>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetScheduledActionDetails(detailId, manageActionId);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        //Author        : Siddhant Chawade
        //Date          : 11th Dec 2019
        //Description   : To reschedule actions
        [Route("api/reschedule-action")]
        [HttpPost]
        public HttpResponseMessage RecheduledAction([FromBody]ScheduledActionDetail data)
        {
            Response<ScheduledActionDetail> output = new Response<ScheduledActionDetail>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                    data.AssignedByUserId = headerUserId;
                }
                if (data != null)
                {
                    if (_schMethods.RecheduledAction(data) > 0)
                    {
                        output.Status = true;
                        output.Message = "Saved successfully";
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "Something went wrong";
                    }
                }
                else
                {
                    output.Status = false;
                    output.Message = "Invalid data";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        //Author        : Siddhant Chawade
        //Date          : 11th Dec 2019
        //Description   : To upsert scheduled actions comment
        [Route("api/upsert-scheduled-action-comment")]
        [HttpPost]
        public HttpResponseMessage UpsertScheduledActionComment([FromBody]ActionComment data)
        {
            Response<ScheduledActionDetail> output = new Response<ScheduledActionDetail>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                    data.CreatedById = headerUserId;
                }
                if (data != null)
                {
                    if (_schMethods.UpsertScheduledActionComment(data) > 0)
                    {
                        output.Status = true;
                        output.Message = "Comment saved successfully";
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "Something went wrong";
                    }
                }
                else
                {
                    output.Status = false;
                    output.Message = "Invalid data";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        //Author        : Siddhant Chawade
        //Date          : 11th Dec 2019
        //Description   : To delete scheduled actions comment
        [Route("api/delete-scheduled-action-comment")]
        [HttpPost]
        public HttpResponseMessage DeleteScheduledActionComment([FromBody]ActionComment data)
        {
            Response<ScheduledActionDetail> output = new Response<ScheduledActionDetail>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                    data.CreatedById = headerUserId;
                }
                if (data != null && data.ActionCommentId != null)
                {
                    if (_schMethods.DeleteScheduledActionComment(data) > 0)
                    {
                        output.Status = true;
                        output.Message = "Comment deleted successfully";
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "Something went wrong";
                    }
                }
                else
                {
                    output.Status = false;
                    output.Message = "Invalid data";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        //Author        : Siddhant Chawade
        //Date          : 18th Dec 2019
        //Description   : To deassign scheduled actions 
        [Route("api/deassign-scheduled-action")]
        [HttpPost]
        public HttpResponseMessage DeassignScheduledAction([FromBody]ActionScheduledToUser data)
        {
            Response<ScheduledActionDetail> output = new Response<ScheduledActionDetail>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                if (data != null && data.ActionDetailId != null)
                {
                    if (_schMethods.DeassignScheduledAction(data, headerUserId) > 0)
                    {
                        output.Status = true;
                        output.Message = "Action unassigned successfully";
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "Something went wrong";
                    }
                }
                else
                {
                    output.Status = false;
                    output.Message = "Invalid data";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        #endregion

        #region Summary Tab

        //Author        : Siddhant Chawade
        //Date          : 9th Dec 2019
        //Description   : To get the list of actions scheduled (summary)
        [Route("api/get-scheduled-actions-summary")]
        [HttpPost]
        public HttpResponseMessage GetScheduledActionsSummary([FromBody]ActionScheduledToUser data)
        {
            Response<List<ScheduledActionsSummary>> output = new Response<List<ScheduledActionsSummary>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetScheduledActionsSummary(null, null, data);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        #endregion

        #endregion
        #region Dashboard
        [Route("api/get-filter-input")]
        [HttpGet]
        public HttpResponseMessage GetFilterInput()
        {
            Response<ActionScheduleFilter> response = new Response<ActionScheduleFilter>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }

                response.Status = true;
                response.Message = "";
                response.Data = _schMethods.GetFilterInputs();
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //Author        : Anji
        //Date          : 12th Dec 2019
        //Description   : To get the list of scheduled actions
        [Route("api/get-user-scheduled-actions")]
        [HttpPost]
        public HttpResponseMessage GetScheduledActionsOfUser([FromBody]ActionScheduledToUser data)
        {
            Response<List<ScheduledAction>> output = new Response<List<ScheduledAction>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetScheduledActionsOfUser(data.UserId, data.ViewType, data.FromDate, data.ToDate);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        //Author        : Anji
        //Date          : 12th Dec 2019
        //Description   : To get the list of scheduled actions
        [Route("api/get-user-scheduled-actions-map")]
        [HttpPost]
        public HttpResponseMessage GetScheduledActionsOfUserForMap([FromBody]ActionScheduledToUser data)
        {
            Response<List<sp_get_actions_scheduled_to_userResult>> output = new Response<List<sp_get_actions_scheduled_to_userResult>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetScheduledActionsOfUserForMap(data.UserId, data.ViewType, data.ScheduledDate);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }
        //Author        : Anji
        //Date          : 12th Dec 2019
        //Description   : To get the list of actions scheduled (summary)
        [Route("api/get-user-scheduled-actions-summary")]
        [HttpPost]
        public HttpResponseMessage GetUserScheduledActionsSummary([FromBody]ActionScheduledToUser data)
        {
            Response<List<ScheduledActionsSummary>> output = new Response<List<ScheduledActionsSummary>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetUserScheduledActionsSummary(data.UserId,data.ViewType,  null, data.TaskId);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }
        //Author        : Anji
        //Date          : 12th Dec 2019
        //Description   : To get the list of actions scheduled (summary)
        [Route("api/get-pm-scheduled-actions-summary")]
        [HttpPost]
        public HttpResponseMessage GetPMScheduledActionsSummary([FromBody]JObject model)
        {
            Response<List<ScheduledActionsSummary>> output = new Response<List<ScheduledActionsSummary>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                string viewType = model.Value<string>("viewType");
                string projectManagerId = model.Value<string>("projectManagerId");
                output.Data = _schMethods.GetPMScheduledActionsSummary(Convert.ToInt16(viewType), Convert.ToInt64(projectManagerId));

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }
        //Author        : Anji
        //Date          : 13th Dec 2019
        //Description   : To get the list of actions of PM
        [Route("api/get-pm-actions")]
        [HttpPost]
        public HttpResponseMessage GetPMAction([FromBody]PMDashboardFilter data)
        {
            Response<List<ActionScheduling>> output = new Response<List<ActionScheduling>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetPMActions(data);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                //output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }
        //Author        : Anji
        //Date          : 13th Dec 2019
        //Description   : To get the list of actions of PM
        [Route("api/get-unassigned-assigned-actions-map")]
        [HttpPost]
        public HttpResponseMessage GetUnAssignedAndAssignedActionsMap([FromBody]ActionScheduledToUser data)
        {
            Response<List<ActionScheduling>> output = new Response<List<ActionScheduling>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetUnAssignedAndAssignedActionsMap(data);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                //output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }
        #endregion


        #region Bharath
        #region Expenses
        //Author        : Sreebharath
        //Date          : 25th Dec 2019
        //Description   : To get all timesheets of user
        [Route("api/get-all-timesheets-of-user")]
        [HttpGet]
        public HttpResponseMessage GetTimeSheetsOfUser(long userId)
        {
            Response<List<sp_get_user_timesheetResult>> output = new Response<List<sp_get_user_timesheetResult>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.getTimeSheetsOfUser(userId);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }
        //Author        : Sreebharath
        //Date          : 25th Dec 2019
        //Description   : To get all expenses of user
        [Route("api/get-all-expenses-of-user")]
        [HttpGet]
        public HttpResponseMessage GetExpensesOfUser(long userId)
        {
            Response<List<sp_get_pm_expensesResult>> output = new Response<List<sp_get_pm_expensesResult>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.getExpensesOfUser(userId);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }


        //Author        : Sreebharath
        //Date          : 26th Dec 2019
        //Description   : To delete timesheet of user
        [Route("api/delete-timesheet-of-user")]
        [HttpDelete]
        public HttpResponseMessage DeleteTimeSheetOfUser(long? timesheetId, bool isAssigned)
        {
            Response<string> output = new Response<string>();
            try
            {
                if (_schMethods.deleteTimeSheetOfUser(isAssigned, timesheetId) > 0)
                {
                    output.Status = true;
                    output.Message = "Timesheet Deleted Successfully";
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something Went Wrong";
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        //Author        : Sreebharath
        //Date          : 26th Dec 2019
        //Description   : To delete expense of user
        [Route("api/delete-expense-of-user")]
        [HttpDelete]
        public HttpResponseMessage DeleteExpenseOfUser(long? expenseId, bool isAssigned)
        {
            Response<string> output = new Response<string>();
            try
            {
                if (_schMethods.deleteExpenseOfUser(isAssigned, expenseId) > 0)
                {
                    output.Status = true;
                    output.Message = "Expense Deleted Successfully";
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something Went Wrong";
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        //Author        : Sreebharath
        //Date          : 26th Dec 2019
        //Description   : To get the time sheet 
        [Route("api/get-time-sheet-assigned")]
        [HttpGet]
        public HttpResponseMessage GetTimeSheet(long timeSheetId)
        {
            Response<LogHoursOverall> output = new Response<LogHoursOverall>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.getTimeSheet(timeSheetId);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }


        //Author        : Sreebharath
        //Date          : 26th Dec 2019
        //Description   : To get the time sheet of anassigned
        [Route("api/get-time-sheet-unassigned")]
        [HttpGet]
        public HttpResponseMessage GetUnAssignedTimesheet(long timeSheetId)
        {
            Response<Timesheet> output = new Response<Timesheet>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.getUnAssignedTimeSheet(timeSheetId);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }


        //Author        : Sreebharath
        //Date          : 27th Dec 2019
        //Description   : To update log hours 
        [Route("api/update-log-hours")]
        [HttpPost]
        public HttpResponseMessage UpdateLogHours()
        {
            Response<TimeSheetModal> output = new Response<TimeSheetModal>();
            try
            {
                var httprequest = HttpContext.Current.Request;
                TimeSheetModal model = JObject.Parse(httprequest.Form["timeSheetForm"]).ToObject<TimeSheetModal>();

                List<LogHoursFiles> logHoursFileList = new List<LogHoursFiles>();
                bool isFilesSaved = false;
                if (httprequest.Files.Count > 0)
                {
                    for (var i = 0; i < httprequest.Files.Count; i++)
                    {
                        LogHoursFiles logHourFile = new LogHoursFiles();
                        LogHoursFiles logHoursFileDetails = JsonConvert.DeserializeObject<LogHoursFiles>(HttpUtility.UrlDecode(httprequest.Files.Keys[i]));
                        logHourFile.isExpense = logHoursFileDetails.isExpense;
                        logHourFile.Id = logHoursFileDetails.Id;
                        logHourFile.UploadedFile = httprequest.Files[i];
                        logHourFile.ActionGoogleDriveId = model.ActionGoogleDriveId;
                        logHourFile.UserId = model.UserId;
                        logHoursFileList.Add(logHourFile);
                    }
                    isFilesSaved = _schMethods.uploadActionLogFiles(logHoursFileList, model.ActionDetailId);
                }
                else
                {
                    isFilesSaved = true;
                }


                if (_schMethods.updateLogHours(model) > 0 && isFilesSaved == true)
                {
                    output.Status = true;
                    output.Message = "Timesheet Updated Successfully";
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something Went Wrong";
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }


        #endregion
        #region TimeSheets
        //Author        : Anji
        //Date          : 28th Jan 2020
        //Description   : To get action details along with timesheet and expenses
        [Route("api/get-action-timesheet-expenses")]
        [HttpGet]
        public HttpResponseMessage GetActionTimesheetAndExpenses(long detailId)
        {
            Response<ReviewTimesheetExpense> output = new Response<ReviewTimesheetExpense>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetActionTimesheetAndExpenses(detailId);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }
        //Author        : Sreebharath
        //Date          : 12th Dec 2019
        //Description   : To get all the details of the sheduled action
        [Route("api/get-all-details-of-scheduled-action")]
        [HttpGet]
        public HttpResponseMessage GetScheduledActionDetails(long detailId)
        {
            Response<ScheduledActionDetails> output = new Response<ScheduledActionDetails>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.getAllDetailsOfScheduledAction(detailId);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }



        //author:Sreebharath
        //date: 24-10-2018
        //Upsert Log hours form in timesheets and expenses
        [Route("api/upsert-log-hours")]
        [HttpPost]
        public HttpResponseMessage UpsertLogHours()
        {
            Response<List<Sites>> output = new Response<List<Sites>>();
            try
            {
                var httprequest = HttpContext.Current.Request;
                // var data55 = httprequest.Form["expensesForm"];
                //  System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;
                var data = httprequest.Form["docsForm"];
                ActionOverviewForm actionModel = JObject.Parse(httprequest.Form["actionForm"]).ToObject<ActionOverviewForm>();
                LogHoursForm logHoursModel = JObject.Parse(httprequest.Form["logHoursForm"]).ToObject<LogHoursForm>();
                DocumentCategoryList docsFormModal = JObject.Parse(httprequest.Form["docsForm"]).ToObject<DocumentCategoryList>();

                ExpensesDetails ExpensesModel = logHoursModel.includeExpenses == true ? JObject.Parse(httprequest.Form["expensesForm"]).ToObject<ExpensesDetails>() : null;
                long? ActionLogHourId = httprequest.Form["actionLogHourId"] == "" ? (dynamic)null : Convert.ToInt64(httprequest.Form["actionLogHourId"]);
                //var DeletedExpenses = JObject.Parse(httprequest.Form["deletedExpenses"]).ToObject<DeletedExpenses>();
                // var ff = DeletedExpenses.T;
                //ExpensesModel.deletedExpenses = DeletedExpenses;
                LogHoursOverall logHourOverallObj = new LogHoursOverall()
                {
                    actionForm = actionModel,
                    logHoursForm = logHoursModel,
                    expensesForm = ExpensesModel,
                    actionLogHourId = ActionLogHourId
                };
                List<LogHoursFiles> logHoursFileList = new List<LogHoursFiles>();
                if (httprequest.Files.Count > 0)
                {
                    if (ExpensesModel != null)
                    {
                        foreach (var item in ExpensesModel.expensesDetails)
                        {
                            if (item.expenseRandomString != null && item.expenseRandomString != "")
                            {
                                if (httprequest.Files[item.expenseRandomString] != null)
                                {
                                    item.expenseGoogleDriveFileId = _schMethods.uploadActionLogFileForExpense(item, actionModel.detailId, actionModel.userId, httprequest.Files[item.expenseRandomString]);
                                }
                            }
                        }
                    }

                    if (docsFormModal.DocumentCategories.Count > 0)
                    {
                        foreach (var docItem in docsFormModal.DocumentCategories)
                        {
                            if (docItem.docRandomString != null && docItem.docRandomString != "")
                            {
                                docItem.docGoogleDriveFileId = _schMethods.uploadActionLogFileDoc(docItem, actionModel.detailId, actionModel.userId, httprequest.Files[docItem.docRandomString]);
                            }
                        }
                    }

                    //for (var i = 0; i < httprequest.Files.Count; i++)
                    //{
                    //    LogHoursFiles logHourFile = new LogHoursFiles();
                    //    LogHoursFiles logHoursFileDetails = JsonConvert.DeserializeObject<LogHoursFiles>(HttpUtility.UrlDecode(httprequest.Files.Keys[i]));
                    //    logHourFile.isExpense = logHoursFileDetails.isExpense;
                    //    logHourFile.Id = logHoursFileDetails.Id;
                    //    logHourFile.UploadedFile = httprequest.Files[i];
                    //    logHourFile.ActionGoogleDriveId = actionModel.actionGoogleDriveId;
                    //    logHourFile.UserId = actionModel.userId;
                    //    logHoursFileList.Add(logHourFile);
                    //}
                    //  isFilesSaved = _schMethods.uploadActionLogFiles(logHoursFileList, actionModel.detailId);
                    //Task task = Task.Run(() => _schMethods.uploadActionLogFiles(logHoursFileList));
                }
                //else
                //{
                //    isFilesSaved = true;
                //}
                if (_schMethods.UpsertLogHoursAction(logHourOverallObj, docsFormModal.DocumentCategories) > 0)
                {
                    //if (_quotesMethods.upsertExpenses(model.expensesData, model.sowId) == true)
                    //{
                    output.Message = "Log Hours saved successfully";
                    output.Status = true;
                    //}
                }
                else
                {
                    output.Message = "Internal Server Error";
                    output.Status = false;
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //Author        : Sreebharath
        //Date          : 26th Dec 2019
        //Description   : To get the expenses of anassigned
        [Route("api/get-expenses-unassigned")]
        [HttpGet]
        public HttpResponseMessage GetExpenseData(long expenseId)
        {
            Response<Expenses> output = new Response<Expenses>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.getUnassignedExpenses(expenseId);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }


        //Author        : Sreebharath
        //Date          : 19th Dec 2019
        //Description   : To deallocate the scheduled action
        [Route("api/deallocate-cheduled-action")]
        [HttpPost]
        public HttpResponseMessage DeAllocateScheduledAction([FromUri]long detailId)
        {
            Response<ScheduledActionDetails> output = new Response<ScheduledActionDetails>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                if (_schMethods.deAllocateScheduledAction(detailId, headerUserId) > 0)
                {
                    output.Status = true;
                    output.Message = "Succesfulluy Saved";
                }
                else
                {
                    output.Status = false;
                    output.Message = "Internal Server Error";
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        #endregion
        #endregion
        #region Timesheets & Expenses
        //Author        : Sreebharath
        //Date          : 20-March-2020
        //Description   : To get timsheet details specific to user
        [Route("api/get-timesheets-user-specific")]
        [HttpGet]
        public HttpResponseMessage GetTimesheetsByUser(long userId, bool isSuperAdminMode)
        {
            Response<Timesheets> output = new Response<Timesheets>();
            try
            {
                output.Data = _schMethods.GetTimesheets(userId, isSuperAdminMode);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }
        //Author        : Siddhant Chawade
        //Date          : 24th Dec 2019
        //Description   : To get the list of expenses
        [Route("api/get-expenses-user-specific")]
        [HttpGet]
        public HttpResponseMessage GetExpensesByUser(long userId, bool isSuperAdminMode)
        {
            Response<GetExpenses> output = new Response<GetExpenses>();
            try
            {
                output.Data = _schMethods.GetExpenses(userId, isSuperAdminMode);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }
        //Author        : Siddhant Chawade
        //Date          : 18th Feb 2020
        //Description   : To get the list of notes timesheet & expenses
        [Route("api/get-timesheet-expense-notes")]
        [HttpGet]
        public HttpResponseMessage GetTimesheetExpenseNotes(long? id, bool isTimesheet, bool isAssigned, bool isApproval)
        {
            Response<List<TimesheetExpenseNotes>> output = new Response<List<TimesheetExpenseNotes>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetTimesheetExpenseNotes(id, isTimesheet, isAssigned, isApproval);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }
        //Author        : Siddhant Chawade
        //Date          : 20th Dec 2019
        //Description   : To get the list of option for adding timesheet & expenses
        [Route("api/get-timesheet-options")]
        [HttpGet]
        public HttpResponseMessage GetTimesheetOptions()
        {
            Response<TimesheetOptions> output = new Response<TimesheetOptions>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = true;
                output.Message = "";
                output.Data = _schMethods.GetTimesheetOptions(headerUserId);

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }

        //Author        : Siddhant Chawade
        //Date          : 21st Dec 2019
        //Description   : To save timesheet and expenses
        [Route("api/save-timesheet-expenses")]
        [HttpPost]
        public HttpResponseMessage SaveTimesheetExpenses()
        {
            Response<Timesheet> output = new Response<Timesheet>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                var httprequest = HttpContext.Current.Request;
                //  System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;
                var data = JObject.Parse(httprequest.Form["timesheetExpensesObject"]).ToObject<Timesheet>();
                if (httprequest.Files.Count > 0)
                {
                    for (var i = 0; i < httprequest.Files.Count; i++)
                    {
                        string indexNumber = httprequest.Files.Keys[i];

                        // actionItem.googleDriveFile = httprequest.Files[i];

                        //Expenses actionDetailsModel = JsonConvert.DeserializeObject<Expenses>(HttpUtility.UrlDecode(httprequest.Files.Keys[i]));
                        data.ExpensesList[Int32.Parse(indexNumber)].googleDriveFile = httprequest.Files[indexNumber];
                    }
                }
                if (data != null)
                {
                    data.UserId = headerUserId;
                    if (_schMethods.SaveTimesheetExpenses(data) > 0)
                    {
                        output.Message = "Saved successfully";
                        output.Status = true;
                    }
                    else
                    {
                        output.Message = "Something went wrong";
                        output.Status = false;
                    }
                }
                else
                {
                    output.Message = "Invalid data";
                    output.Status = false;
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //Author        : Siddhant Chawade
        //Date          : 23rd Dec 2019
        //Description   : To get the list timesheet
        [Route("api/get-timesheets")]
        [HttpGet]
        public HttpResponseMessage GetTimesheets(bool isSuperAdminMode)
        {
            Response<Timesheets> output = new Response<Timesheets>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Data = _schMethods.GetTimesheets(headerUserId, isSuperAdminMode);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //Author        : Siddhant Chawade
        //Date          : 24th Dec 2019
        //Description   : To get the list of expenses
        [Route("api/get-expenses")]
        [HttpGet]
        public HttpResponseMessage GetExpenses(bool isSuperAdminMode)
        {
            Response<GetExpenses> output = new Response<GetExpenses>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Data = _schMethods.GetExpenses(headerUserId, isSuperAdminMode);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        #endregion
        #region PM-TM
        //Author        : Anji
        //Date          : 18th Mar 2020
        //Description   : To get the hours logged by users and to be approved by PM
        [Route("api/get-pm-timesheets-nonaction")]
        [HttpGet]
        public HttpResponseMessage GetPMTimeSheetNonAction(long? projectManagerId)
        {
            Response<List<sp_get_user_timesheet_nonactionResult>> output = new Response<List<sp_get_user_timesheet_nonactionResult>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Data = _schMethods.GetNonActionTimeSheets(projectManagerId);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }
        //Author        : Anji
        //Date          : 25th Dec 2019
        //Description   : To get the hours logged by users and to be approved by PM
        [Route("api/get-pm-expenses-nonaction")]
        [HttpGet]
        public HttpResponseMessage GetPMExpensesNonAction(long? projectManagerId)
        {
            Response<List<sp_get_pm_expenses_nonactionResult>> output = new Response<List<sp_get_pm_expenses_nonactionResult>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Data = _schMethods.GetNonActionExpenses(projectManagerId);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }
        //Author        : Anji
        //Date          : 25th Dec 2019
        //Description   : To get the hours logged by users and to be approved by PM
        [Route("api/get-pm-timesheets")]
        [HttpGet]
        public HttpResponseMessage GetPMTimeSheet(long? projectManagerId)
        {
            Response<List<sp_get_user_timesheetResult>> output = new Response<List<sp_get_user_timesheetResult>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Data = _schMethods.GetTimeSheets(projectManagerId);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }
        //Author        : Anji
        //Date          : 25th Dec 2019
        //Description   : To get the hours logged by users and to be approved by PM
        [Route("api/get-pm-expenses")]
        [HttpGet]
        public HttpResponseMessage GetPMExpenses(long? projectManagerId)
        {
            Response<List<sp_get_pm_expensesResult>> output = new Response<List<sp_get_pm_expensesResult>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Data = _schMethods.GetPMExpenses(projectManagerId);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }
        //Author        : Anji
        //Date          : 26th Dec 2019
        //Description   : To approve or reject Time sheet or Expense
        [Route("api/approve-reject-timesheet-expense")]
        [HttpPost]
        public HttpResponseMessage ApproveRejectTimeSheetExpense([FromBody]TimeAndExpenses data)
        {
            Response<List<ActionScheduling>> output = new Response<List<ActionScheduling>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = false;
                output.Message = "";
                long? result = _schMethods.ApproveRejectTimesheetExpense(data, headerUserId);
                if (result > 0)
                {
                    output.Status = true;
                    output.Message = data.IsApprove ? "Selected entries has been approved successfully" : "Selected entry has been rejected successfully";
                }
                else
                {
                    output.Message = "Operation failed";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                //output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }
        //Author        : Anji
        //Date          : 26th Dec 2019
        //Description   : To approve or reject Time sheet or Expense
        [Route("api/approve-reject-action-timesheet-expense")]
        [HttpPost]
        public HttpResponseMessage ApproveRejectActionTimeSheetExpense([FromBody]ReviewTimesheetExpense data)
        {
            Response<List<string>> output = new Response<List<string>>();
            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                output.Status = false;
                output.Message = "";
                bool result = _schMethods.ApproveRejectLoggedHours(data, headerUserId);
                if (result)
                {
                    output.Status = true;
                    output.Message = data.IsApprove ? "Timesheet has been approved successfully" : "Timesheet has been rejected successfully";
                }
                else
                {
                    output.Message = "Operation failed";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                //output.Message = "Something went wrong, please contact administrator";
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { output }));
            }
            return response;
        }
        #endregion
    }
}
