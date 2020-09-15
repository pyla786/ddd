using API_IBW.Business_Classes;
using API_IBW.DB_Models;
using API_IBW.HelperMethods;
using API_IBW.Models;
using API_IBW.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static API_IBW.Models.FCMModels;

namespace API_IBW.Controllers
{
    //[BasicAuthorization]
    [ExceptionHandler]
    public class AdminController : ApiController
    {
        HttpResponseMessage response;
        Admin adminMethods;
        private Login _login;
        //private long? headerUserId = null;
        public AdminController()
        {

            //creating required objects
            response = new HttpResponseMessage();
            adminMethods = new Admin();
            _login = new Login();
        }

        #region sree
        #region Alerts
        //author:Sreebharath 
        //date: 05/01/2020
        //To Get All Alert Notifications
        [Route("api/get-all-alert-notifications")]
        [HttpGet]
        public HttpResponseMessage getAllAlerts(long? userId)
        {
            EmployeeTypes empCodes = new EmployeeTypes();
            Response<List<Notification>> output = new Response<List<Notification>>();
            try
            {
                List<Notification> allNotificationsList = FirebaseCloudMessaging.getNotifcations(userId);
                if (allNotificationsList != null || allNotificationsList.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = allNotificationsList;
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }
        #endregion
        #region ActionMaster

        //author:Sreebharath 
        //date: 07/12/2019
        //Upsert and Delete Actions master data
        [Route("api/upsert-delete-action-master")]
        [HttpPost]
        public HttpResponseMessage upsertDeleteActionMaster(ActionsMaster model)
        {
            Response<long?> output = new Response<long?>();
            try
            {
                long? actionResult = adminMethods.upsertandDeleteActionsMaster(model);
                if (actionResult != null)
                {
                    output.Status = true;
                    output.Message = model.isDeleted == true ? "Action deleted successfully" : (model.actionMasterId == null ? "Action saved successfully" : "Action updated successfully");
                    output.Data = actionResult;
                }
                else
                {
                    output.Status = false;
                    output.Message = "Internal server error";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Sreebharath 
        //date: 07/12/2019
        //To get all the actions in master data
        [Route("api/get-all-actions-master-data")]
        [HttpGet]
        public HttpResponseMessage getAllActionsMasterData()
        {
            Response<List<ActionsMaster>> output = new Response<List<ActionsMaster>>();
            try
            {
                List<ActionsMaster> actionsList = adminMethods.getAllActionsInMasterData();
                if (actionsList.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = actionsList;
                }
                else
                {
                    output.Status = true;
                    output.Message = "";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        #endregion
        //author:Sreebharath 
        //date: 08/08/2019
        //To Get All User Roles
        [Route("api/get-all-job-codes")]
        [HttpGet]
        public HttpResponseMessage getAllJobCodes()
        {
            JobCodes jobCodes = new JobCodes();
            Response<List<AllJobCodes>> output = new Response<List<AllJobCodes>>();
            try
            {
                List<AllJobCodes> jobCodesList = adminMethods.GetAllJobCodes();
                if (jobCodesList != null || jobCodesList.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = jobCodesList;
                }
                else
                {
                    output.Status = false;
                    output.Message = "Internal server error";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                throw ex;
            }
            return response;
        }

        //author:Sreebharath 
        //date: 09/08/2019
        //To Get All Employee Types
        [Route("api/get-all-employee-types")]
        [HttpGet]
        public HttpResponseMessage getAllEmployeeTypes()
        {
            EmployeeTypes empCodes = new EmployeeTypes();
            Response<List<EmployeeTypes>> output = new Response<List<EmployeeTypes>>();
            try
            {
                List<EmployeeTypes> empTypes = adminMethods.GetEmployeeTypes();
                if (empTypes != null || empTypes.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = empTypes;
                }
                else
                {
                    output.Status = false;
                    output.Message = "Internal server error";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        #region Municipalities

        //author:Sreebharath 
        //date: 22/08/2019
        //To upsert municipalities
        [Route("api/upsert-municipalities")]
        [HttpPost]
        public HttpResponseMessage upsertMunicipalities(Municipalities model)
        {
            Response<Municipalities> output = new Response<Municipalities>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                if (model.municipalityId == null)
                {
                    //Logging event
                    Logging.EventLog(headerUserId, "Attempt to add new municipality");

                    if (adminMethods.MunicipalityNameValidation(model.municipalityName, model.municipalityId))
                    {
                        if (adminMethods.upsertMuniciplaties(model) != 0)
                        {
                            output.Status = true;
                            output.Message = "Municipality saved successfully";

                            Logging.EventLog(headerUserId, "Municipality added successfully");

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
                        output.Message = "Municipality name already exists";
                    }

                }
                else
                {
                    // Logging event
                    Logging.EventLog(headerUserId, "Attempt to update municipality");

                    if (adminMethods.MunicipalityNameValidation(model.municipalityName, model.municipalityId))
                    {
                        if (adminMethods.upsertMuniciplaties(model) != 0)
                        {
                            output.Status = true;
                            output.Message = "Municipality updated successfully";

                            Logging.EventLog(headerUserId, "Municipality details updated successfully");

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
                        output.Message = "Municipality name already exists";
                    }
                }


                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Sreebharath 
        //date: 30/08/2019
        //To change the status of municipality
        [Route("api/municipality-status")]
        [HttpPost]
        public HttpResponseMessage municipalityStatus(Municipalities model)
        {
            Response<Municipalities> output = new Response<Municipalities>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////
                //Logging event
                Logging.EventLog(headerUserId, "Attempt to update municipality status");
                if (adminMethods.upsertMuniciplaties(model) != 0)
                {
                    output.Status = true;
                    output.Message = "Status updated successfully";

                    Logging.EventLog(headerUserId, "Municipality status updated successfully");

                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Sreebharath 
        //date: 22/08/2019
        //To upsert municipalities
        [Route("api/municipalities-bulk-upload")]
        [HttpPost]
        public HttpResponseMessage municipalitiesBulkUpload(Municipalities model)
        {
            Response<Municipalities> output = new Response<Municipalities>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////
                //Logging event
                Logging.EventLog(headerUserId, "Attempt to upload municipalities in bulk");
                if (model.municipalitiesList.Count > 0)
                {
                    if (adminMethods.bulkUploadMunicipalities(model))
                    {
                        output.Status = true;
                        output.Message = "Municipalities saved successfully";

                        Logging.EventLog(headerUserId, "Municipalities uploaded successfully");

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
                    output.Message = "There are no municipalities to upload";
                }



                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Sreebharath 
        //date: 22/08/2019
        //To get all Municipalities
        [Route("api/get-all-municipalities")]
        [HttpGet]
        public HttpResponseMessage getAllMunicipalities()
        {
            Response<List<Municipalities>> output = new Response<List<Municipalities>>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////
                //Logging event
                // Logging.EventLog(headerUserId, "Attempt to view municipality");
                List<Municipalities> data = adminMethods.GetMunicipalities();
                if (data.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = data;
                }
                else
                {
                    output.Status = true;
                    output.Message = "";
                }
                return response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));

            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                return response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
        }

        //author:Sreebharath
        //date: 16-08-2019
        //To delete municipality
        [Route("api/delete-municipality")]
        [HttpPost]
        public HttpResponseMessage deleteMunicipality([FromBody]Municipalities model)
        {
            Response<Municipalities> output = new Response<Municipalities>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////
                //Logging event
                Logging.EventLog(headerUserId, "Attempt to delete municipality");
                if (adminMethods.deleteMunicipality(model) > 0)
                {
                    output.Status = true;
                    output.Message = "Municipality deleted successfully";

                    Logging.EventLog(headerUserId, "Municipality deleted successfully");

                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }

            return response;
        }

        #endregion

        #region Potential levels

        //author:Sreebharath 
        //date: 30/08/2019
        //To Change RFQ Potential Level Status
        [Route("api/rfq-potential-level-status")]
        [HttpPost]
        public HttpResponseMessage rfqPotentialLevelStatus(PotentialLevel model)
        {
            Response<PotentialLevel> output = new Response<PotentialLevel>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////
                //Logging event
                Logging.EventLog(headerUserId, "Attempt to update potential level status");


                if (adminMethods.upsertPotentialLevel(model) != 0)
                {
                    output.Status = true;
                    output.Message = "Status updated successfully";
                    //Logging event
                    Logging.EventLog(headerUserId, "Potential level status updated successfully");
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Sreebharath 
        //date: 22/08/2019
        //To upsert potential levels
        [Route("api/upsert-potential-levels")]
        [HttpPost]
        public HttpResponseMessage upsertPotentialLeveles(PotentialLevel model)
        {
            Response<PotentialLevel> output = new Response<PotentialLevel>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                if (model.potentialLevelId == null)
                {
                    //Logging event
                    Logging.EventLog(headerUserId, "Attempt to add new potential level");

                    if (adminMethods.potentialLevelNameandColorCodeValidation(model.potentialLevelName, model.potentialLevelId, model.colorCode))
                    {
                        if (adminMethods.upsertPotentialLevel(model) != 0)
                        {
                            output.Status = true;
                            output.Message = "Potential level saved successfully";

                            //Logging event
                            Logging.EventLog(headerUserId, "Potential level added successfully");
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
                        output.Message = "Potential level name or color code already exists";
                    }

                }
                else
                {
                    //Logging event
                    Logging.EventLog(headerUserId, "Attempt to update potential level");

                    if (adminMethods.potentialLevelNameandColorCodeValidation(model.potentialLevelName, model.potentialLevelId, model.colorCode))
                    {
                        if (adminMethods.upsertPotentialLevel(model) != 0)
                        {
                            output.Status = true;
                            output.Message = "Potential level updated successfully";
                            //Logging event
                            Logging.EventLog(headerUserId, "Potential level details updated successfully");
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
                        output.Message = "Potential level name or color code already exists";
                    }
                }


                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Sreebharath 
        //date: 22/08/2019
        //To get all potential levels
        [Route("api/get-all-potential-levels")]
        [HttpGet]
        public HttpResponseMessage getAllPotentialLevels()
        {
            Response<List<PotentialLevel>> output = new Response<List<PotentialLevel>>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                //Logging.EventLog(headerUserId, "Attempt to view potential level");
                List<PotentialLevel> data = adminMethods.GetAllPotentialLevels();
                if (data.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = data;
                }
                else
                {
                    output.Status = true;
                    output.Message = "";
                }
                return response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));

            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                return response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
        }

        //author:Sreebharath
        //date: 22-08-2019
        //To delete municipality
        [Route("api/delete-potential-level")]
        [HttpPost]
        public HttpResponseMessage deletePotentialLevel([FromBody]PotentialLevel model)
        {
            Response<Municipalities> output = new Response<Municipalities>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                //Logging event 
                Logging.EventLog(headerUserId, "Attempt to delete potential level");

                if (adminMethods.deletePotentialLevel(model) > 0)
                {
                    output.Status = true;
                    output.Message = "Potential Level deleted successfully";
                    //Logging event 
                    Logging.EventLog(headerUserId, "Potential level deleted successfully");
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }

            return response;
        }

        #endregion

        #region Users

        //author:Sreebharath 
        //date: 07/08/2019
        //To Insert Users and Update IBW Users with Job Roles
        [Route("api/upsert-ibw-user")]
        [HttpPost]
        public HttpResponseMessage UserUpsert([FromBody]IBWUsers model)
        {
            IBWUsers ibwUserData = new IBWUsers();
            Response<IBWUsers> output = new Response<IBWUsers>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                if (model.userId == null || model.userId == 0)
                {
                    //logging event
                    Logging.EventLog(headerUserId, "Attempt to add new user");

                    if (adminMethods.EmailValidation(model.email, model.userId))
                    {

                        long? userID = adminMethods.upsertUser(model);
                        if (userID != 0 || userID != null)
                        {


                            Response<string> emailConfirmation = _login.SendForgotPasswordEmail(model.email, true);
                            if (emailConfirmation.Status == true)
                            {
                                output.Status = true;
                                output.Message = "New user added successfully, and verification link has been sent to registered email";

                                //logging event
                                Logging.EventLog(headerUserId, "New user has been added successfully");
                            }
                            else
                            {
                                output.Status = false;
                                output.Message = "User added successfully, something wrong with email";

                                //logging event
                                Logging.EventLog(headerUserId, "New user has been added successfully");
                            }

                        }
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "User already exists with the given email";


                    }
                }
                else
                {
                    //logging event
                    Logging.EventLog(headerUserId, "Attempt to update user details");

                    if (adminMethods.EmailValidation(model.email, model.userId))
                    {
                        long? userID = adminMethods.upsertUser(model);
                        if (userID != null || userID != 0)
                        {
                            output.Status = true;
                            output.Message = "User updated successfully";

                            //to resend verification email
                            if (model.resendEmail == true)
                            {
                                Response<string> emailConfirmation = _login.SendForgotPasswordEmail(model.email, true);
                                if (emailConfirmation.Status == true)
                                {
                                    output.Status = true;
                                    output.Message = "User updated successfully, and verification link has been resent to registered email";
                                }
                                else
                                {
                                    output.Status = false;
                                    output.Message = "User updated successfully, something wrong with email";
                                }
                            }

                            //logging event
                            Logging.EventLog(headerUserId, "User details updated successfully");
                        }
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "User already exists with the given email";

                    }
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }

            return response;
        }
        //author:Sreebharath 
        //date: 11/08/2019
        //To get all project managers
        [Route("api/get-all-project-managers")]
        [HttpGet]
        public HttpResponseMessage getAllProjectManagers()
        {
            Response<List<IBWUsers>> output = new Response<List<IBWUsers>>();
            try
            {
                List<IBWUsers> data = adminMethods.getReportingManagersAndSuperAdmin().OrderBy(x=>x.userName).ToList();
                if (data.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = data;
                }
                else
                {
                    output.Status = true;
                    output.Message = "";
                }
                return response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));

            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                return response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
        }
        //To change the status of a user
        [Route("api/get-user-leave-count")]
        [HttpPost]
        public HttpResponseMessage GetUserLeaveCount([FromBody]JObject model)
        {
            Response<long> output = new Response<long>();
            try
            {

                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////
                string userId = model.Value<string>("userId");
                long result = adminMethods.GetUserLeaveCount(Convert.ToInt64(userId));
                output.Status = true;
                output.Message = "";
                output.Data = result;
               
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
        //author:Sreebharath 
        //date: 08/08/2019
        //To Get All User Roles
        [Route("api/get-all-user-roles-with-count")]
        [HttpGet]
        public HttpResponseMessage getAllUserRolesWithCount()
        {
            UserRoles userRole = new UserRoles();
            Response<List<UserPermissions>> output = new Response<List<UserPermissions>>();
            try
            {
                List<UserPermissions> usersRolesList = adminMethods.GetAllRolesWithCount();
                if (usersRolesList != null || usersRolesList.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = usersRolesList;
                }
                else
                {
                    output.Status = false;
                    output.Message = "Internal server error";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }
        //author:Sreebharath 
        //date: 07/08/2019
        //To Get All Users
        [Route("api/get-all-users")]
        [HttpGet]
        public HttpResponseMessage getAllUsers()
        {
            IBWUsers ibwUserData = new IBWUsers();
            Response<List<IBWUsers>> output = new Response<List<IBWUsers>>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                List<IBWUsers> usersList = adminMethods.GetUsers();
                if (usersList != null || usersList.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = usersList;

                    //logging event
                    //Logging.EventLog(headerUserId, "Attempt to view users");
                }
                else
                {
                    output.Status = false;
                    output.Message = "Internal server error";

                    //logging event
                    //Logging.EventLog(headerUserId, "Attempt to view users");
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Sreebharath 
        //date: 09/08/2019
        //To change the status of a user
        [Route("api/change-status")]
        [HttpPost]
        public HttpResponseMessage changeStatus(IBWUsers model)
        {
            Response<List<IBWUsers>> output = new Response<List<IBWUsers>>();
            try
            {

                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////
                if (model.status != null)
                {
                    Logging.EventLog(headerUserId, "Attempt to update user status");
                    if (model.status == true)
                    {
                        if (adminMethods.checkVerified(model.userId) == true)
                        {
                            var data = adminMethods.changeStatusAndDelete(model);
                            if (data != null)
                            {
                                output.Status = true;
                                output.Message = "Status updated successfully";

                                //logging event
                                Logging.EventLog(headerUserId, "User status has been updated successfully");
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
                            output.Message = "You can't change status without verifying email";
                        }
                    }
                    else
                    {
                        var data = adminMethods.changeStatusAndDelete(model);
                        if (data != null)
                        {
                            output.Status = true;
                            output.Message = "Status updated successfully";

                            //logging event
                            Logging.EventLog(headerUserId, "User status has been updated successfully");
                        }
                        else
                        {
                            output.Status = false;
                            output.Message = "Something went wrong";

                            //logging event
                            Logging.EventLog(headerUserId, "Attempt to update status");
                        }
                    }
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }
        //To change the status of a user
        [Route("api/user-superadmin-status-change")]
        [HttpPost]
        public HttpResponseMessage UserSuperAdminStatusChange(IBWUsers model)
        {
            Response<List<IBWUsers>> output = new Response<List<IBWUsers>>();
            try
            {

                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////
                if (model.isSuperAdmin != null)
                {
                    long? result = adminMethods.UserSuperAdminStatusChange(model);
                    if (result != 0)
                    {
                        output.Status = true;
                        if (model.isSuperAdmin == true)
                            output.Message = "You have successfully enabled super admin privileges for this user";
                        else
                            output.Message = "You have successfully disabled super admin privileges for this user";
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "Something went wrong";
                    }
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
        //To change the status of a user
        [Route("api/user-mode-change")]
        [HttpPost]
        public HttpResponseMessage UserModeChange([FromBody]JObject model)
        {
            Response<LoginResponse> output = new Response<LoginResponse>();
            try
            {

                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////
                string userId = model.Value<string>("userId");
                bool isSuperAdmin = model.Value<bool>("isSuperAdmin");
                long? result = adminMethods.ChangeUserMode(Convert.ToInt64(userId), isSuperAdmin);
                if (result != 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = new LoginResponse { UserDetails = _login.GetUserDetails(Convert.ToInt64(userId)), UserRights = _login.GetUserRights(Convert.ToInt64(userId)), Roles = _login.GetUserRoles(Convert.ToInt64(userId)).Select(x => x.vc_role_name).Distinct().ToList(), SettingDetails = _login.GetAllSettings() };  //_login.GetUserRights(Convert.ToInt64(userId));
                    //if (isSuperAdmin == true)
                    //    output.Message = "You have successfully enabled super admin privileges for this user";
                    //else
                    //    output.Message = "You have successfully disabled super admin privileges for this user";
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
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
        //author:Sreebharath 
        //date: 09/08/2019
        //To change the status of a user
        [Route("api/delete-user")]
        [HttpPost]
        public HttpResponseMessage deleteUser(IBWUsers model)
        {
            Response<List<IBWUsers>> output = new Response<List<IBWUsers>>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                //logging event
                Logging.EventLog(headerUserId, "Attempt to delete user account");

                model.isDelete = true;
                var data = adminMethods.changeStatusAndDelete(model);
                if (data != null)
                {
                    output.Status = true;
                    output.Message = "User deleted successfully";

                    //logging event
                    Logging.EventLog(headerUserId, "User account deleted successfully");
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        #endregion

        #region Permissions

        //author:Sreebharath 
        //date: 09/08/2019
        //To change the status of a user
        [Route("api/upsert-users-of-role")]
        [HttpPost]
        public HttpResponseMessage upsertUsersOfRole(UserPermissions model)
        {
            Response<List<UserPermissions>> output = new Response<List<UserPermissions>>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                var data = adminMethods.upsertUsersOfRole(model);
                if (data >= 0 || data == null)
                {
                    output.Status = true;
                    output.Message = "Role updated successfully";
                    //logging event
                    Logging.EventLog(headerUserId, "User list updated successfully");
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                    //logging event
                    Logging.EventLog(headerUserId, "Attempt to update user list");
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Sreebharath 
        //date: 11/08/2019
        //To get all screen permissions
        [Route("api/get-all-screen-permissions")]
        [HttpGet]
        public HttpResponseMessage getAllScreenPermissions(long roleId)
        {
            Response<List<screenPermissions>> output = new Response<List<screenPermissions>>();
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
                List<screenPermissions> data = adminMethods.GetScreenPermissions(roleId);
                Logging.EventLog(headerUserId, "Attempt to update permissions settings");
                if (data.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = data;
                }
                else
                {
                    output.Status = false;
                    output.Message = "No screen permissions to display";
                }
                return response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));

            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                return response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
        }

        //author:Sreebharath
        //date: 19-08-2019
        //Insert Permission Screens
        [Route("api/insert-permission-screens")]
        [HttpPost]
        public HttpResponseMessage insertScreenPermssions([FromBody]ScreenPermissionsList model)
        {
            Response<screenPermissions> output = new Response<screenPermissions>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                //logging event
                Logging.EventLog(headerUserId, "Attempt to configure permissions");

                var data = adminMethods.insertScreenPermissions(model);
                if (data > 0 || data == null)
                {
                    //logging event
                    Logging.EventLog(headerUserId, "Permissions have been configured successfully");
                    output.Status = true;
                    output.Message = "Permissions configured successfully";
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }


            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }

            return response;
        }
        //author:Sreebharath
        //date: 16-08-2019
        //To delete Permission role
        [Route("api/delete-role")]
        [HttpPost]
        public HttpResponseMessage deletePermissionRole([FromBody]UserPermissions model)
        {
            Response<IBWUsers> output = new Response<IBWUsers>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                //logging event
                Logging.EventLog(headerUserId, "Attempt to delete permission level");

                if (adminMethods.deletePermissionRole(model) > 0)
                {
                    //logging event
                    Logging.EventLog(headerUserId, "Permission level has been deleted successfully");

                    output.Status = true;
                    output.Message = "Role deleted successfully";
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }

            return response;
        }

        //author:Sreebharath
        //date: 16-08-2019
        //Permission Role Upsert 
        [Route("api/permission-role-upsert")]
        [HttpPost]
        public HttpResponseMessage permissionRoleUpsert([FromBody]UserPermissions model)
        {
            Response<IBWUsers> output = new Response<IBWUsers>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////


                if (model.roleMasterId == null)
                {
                    //Logging event
                    Logging.EventLog(headerUserId, "Attempt to add new permission level");

                    if (adminMethods.permissionNameValidation(model.roleMasterName, model.roleMasterId))
                    {
                        if (adminMethods.upsertPermissionRole(model) > 0)
                        {
                            output.Status = true;
                            output.Message = "Permission saved successfully";

                            //Logging event
                            Logging.EventLog(headerUserId, "New permission level has been added successfully");
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
                        output.Message = "Role name already exists";
                    }

                }
                else
                {
                    //Logging event
                    Logging.EventLog(headerUserId, "Attempt to update permission level");

                    if (adminMethods.permissionNameValidation(model.roleMasterName, model.roleMasterId))
                    {
                        if (adminMethods.upsertPermissionRole(model) > 0)
                        {
                            output.Status = true;
                            output.Message = "Permission updated successfully";
                            //Logging event
                            Logging.EventLog(headerUserId, "Permission level updated successfully");
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
                        output.Message = "Role name already exists";
                    }
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }

            return response;
        }

        //author:Sreebharath
        //date: 16-08-2019
        //Change Permission Role Status 
        [Route("api/permission-role-status")]
        [HttpPost]
        public HttpResponseMessage permissionRoleStatus([FromBody]UserPermissions model)
        {
            Response<IBWUsers> output = new Response<IBWUsers>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                //Logging event
                Logging.EventLog(headerUserId, "Attempt to update permission level status");


                if (adminMethods.upsertPermissionRole(model) > 0)
                {
                    output.Status = true;
                    output.Message = "Status updated successfully";

                    //Logging event
                    Logging.EventLog(headerUserId, "Permission level status has been updated successfully");
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }

            return response;
        }

        //author:Sreebharath 
        //date: 08/08/2019
        //To Get All User Roles
        [Route("api/get-all-user-roles")]
        [HttpGet]
        public HttpResponseMessage getAllUserRoles()
        {
            UserRoles userRole = new UserRoles();
            Response<List<UserRoles>> output = new Response<List<UserRoles>>();
            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                List<UserRoles> usersRolesList = adminMethods.GetUserRoles();

                //Logging event
                //Logging.EventLog(headerUserId, "Attempt to view permissions");

                if (usersRolesList != null || usersRolesList.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = usersRolesList;


                }
                else
                {
                    output.Status = false;
                    output.Message = "Internal server error";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        #endregion

        #endregion

        #region lookup
        //author:Mani
        //date: 22-Aug-2019
        //get lookup options 

        [Route("api/get-lookup-options")]
        [HttpGet]
        public HttpResponseMessage GetLookupOptions(LookupOptions model)
        {
            Response<LookupModel> response = new Response<LookupModel>();

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
                //Logging.EventLog(headerUserId, "Attempt to view lookup");
                LookupModel LookupOptions = adminMethods.GetLookups();
                if (LookupOptions.CodeMaster != null && LookupOptions.CodeMaster.Count() > 0)
                {
                    response.Status = true;
                    response.Message = "";
                    response.Data = LookupOptions;

                }
                else
                {
                    response.Status = false;
                    response.Message = "Fetching Lookup Options failed";
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }
        [Route("api/update-lookup")]
        [HttpPost]
        public HttpResponseMessage UpdateLookup(LookupOptions model)
        {
            Response<string> response = new Response<string>();

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
                if (model.LookupId == 0 || model.LookupId == null)
                {
                    Logging.EventLog(headerUserId, "Attempt to add new lookup");
                    Logging.EventLog(headerUserId, "Lookup added successfully");
                }
                if (model.LookupId == 0 || model.LookupId == null)
                {
                    Logging.EventLog(headerUserId, "Attempt to update lookup");
                    Logging.EventLog(headerUserId, "Lookup details updated successfully");
                }
                response = adminMethods.UpsertLookup(model, null);
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(new { response }));
            }
        }
        [Route("api/lookup-delete")]
        [HttpGet]
        public HttpResponseMessage DeleteLookup(long? lookupId)
        {
            Response<string> output = new Response<string>();
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
                Logging.EventLog(headerUserId, "Attempt to delete lookup");
                if (adminMethods.DeleteLookup(lookupId, null))
                {
                    output.Status = true;
                    output.Message = "Lookup deleted successfully";

                    Logging.EventLog(headerUserId, "Lookup deleted successfully");
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(output));
            }
            return response;
        }
        #endregion


        #region Mani
        #region Tasks

        //author:Mani
        //date: 14-Oct-2019
        //Task Upsert
        [Route("api/task-upsert")]
        [HttpPost]
        public HttpResponseMessage TaskUpsert(Tasks model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                response = adminMethods.TaskValidateUpsertAndGetResponse(response, model, headerUserId);
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = true;
                response.Message = response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            finally
            {
                //will use in near future
            }
        }
        //author:Mani
        //date: 05-Nov-2019
        //To change the sequnetial order in tasks
        [Route("api/change-sequence-order-tasks")]
        [HttpPost]
        public HttpResponseMessage changeSequenceOrderTasks(long? fromPosition, long? toPosition)
        {
            Response<long> output = new Response<long>();

            try
            {
                if (adminMethods.changeSequentialOrderTasks(fromPosition, toPosition) > 0)
                {
                    output.Message = "Tasks have been sequenced successfully.";
                    output.Status = true;
                }

                else
                {
                    output.Message = "Something Went Wrong";
                    output.Status = false;
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
                return response;
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = output.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            finally
            {
                //will use in near future
            }
        }

        //author:Sreebharath
        //date: 08-Nov-2019
        //To insert and uodate the Task with Job Codes and Notes using Cursor
        [Route("api/upsert-tasks-new")]
        [HttpPost]
        public HttpResponseMessage upsertTasksCursor(Tasks model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                response = adminMethods.TaskValidateUpsertAndGetResponse(response, model, headerUserId);
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            finally
            {
                //will use in near future
            }
        }

        //author:Mani
        //date: 14-Oct-2019
        //Leave Reason Get
        [Route("api/get-tasks")]
        [HttpGet]
        public HttpResponseMessage GetTasks()
        {
            Response<List<Tasks>> response = new Response<List<Tasks>>();

            try
            {

                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                List<Tasks> TasksData = adminMethods.GetTasks().ToList();


                if (TasksData.Count() > 0)
                {
                    response.Status = true;
                    response.Data = TasksData;
                    response.Message = "";
                    //Event logging
                    //Logging.EventLog(headerUserId, "Attempt to view leave reason");
                }
                else
                {
                    response.Status = true;
                    response.Message = "";
                    //Event logging
                    //Logging.EventLog(headerUserId, "Attempt to view leave reason");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            finally
            {
                //will use in near future
            }
        }
        //Author        : Siddhant Chawade
        //Date          : 28th Nov 2019
        //Description   : To update task Status
        [Route("api/update-task-status")]
        [HttpPost]
        public HttpResponseMessage UpdateTaskStatus(Tasks model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from headers
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                //////////////////

                if (adminMethods.UpdateTaskStatus(model.TaskId, model.IsActive) > 0)
                {
                    response.Status = true;
                    response.Message = "Status updated successfully";


                    Logging.EventLog(headerUserId, "Attempt to update task status");
                    Logging.EventLog(headerUserId, "Task status updated successfully");
                }
                else
                {
                    response.Status = false;
                    response.Message = "Updating status failed";


                    //logging event
                    Logging.EventLog(headerUserId, "Attempt to update task status");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }
        //author:Mani
        //date: 14-Oct-2019
        //delete task
        [Route("api/delete-task")]
        [HttpPost]
        public HttpResponseMessage DeleteTask(Tasks model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////
                if (adminMethods.DeleteTask(model.TaskId, model.UserId) > 0)
                {
                    response.Status = true;
                    response.Message = "Task deleted successfully";
                    //Event logging
                    //Logging.EventLog(headerUserId, "Attempt to delete leave reason");
                    //Logging.EventLog(headerUserId, "Leave reason deleted successfully");
                }
                else
                {
                    response.Status = false;
                    response.Message = "Deleting task failed";
                    //Event logging
                    //Logging.EventLog(headerUserId, "Attempt to delete leave reason");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            finally
            {
                //will use in near future
            }
        }

        #endregion
        #region jobCodes
        //Author        : Siddhant Chawade
        //Date          : 20th Jan 2020
        //Description   : To get the list of category wise job codes
        [Route("api/get-jobcode-with-category")]
        [HttpGet]
        public HttpResponseMessage GetJobCodesWithCategory(bool? isUserSpecific)
        {
            Response<List<AllJobCodesWithCategory>> response = new Response<List<AllJobCodesWithCategory>>();

            try
            {
                //Getting user id from headers
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }

                List<AllJobCodesWithCategory> JobCodesData = adminMethods.GetJobCodesWithCategory(isUserSpecific, headerUserId);

                if (JobCodesData.Count() > 0)
                {
                    response.Status = true;
                    response.Data = JobCodesData;
                    response.Message = "";
                }
                else
                {
                    response.Status = false;
                    response.Message = "";
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        //author:Mani
        //date: 08-Aug-2019
        //Job Code Upsert
        [Route("api/jobcode-upsert")]
        [HttpPost]
        public HttpResponseMessage JobCodeUpsert(JobCodes model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                response = adminMethods.JobCodeValidateUpsertAndGetResponse(response, model, headerUserId);


                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = true;
                response.Message = response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        //author:Mani
        //date: 08-Aug-2019
        //Job Code Get
        [Route("api/get-jobcode")]
        [HttpGet]
        public HttpResponseMessage GetJobCodes()
        {
            Response<List<JobCodes>> response = new Response<List<JobCodes>>();

            try
            {
                //Getting user id from headers
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }

                List<JobCodes> JobCodesData = adminMethods.GetJobCodes();

                if (JobCodesData.Count() > 0)
                {
                    response.Status = true;
                    response.Data = JobCodesData;
                    response.Message = "";

                    ////Event logging
                    //Logging.EventLog(headerUserId, "Attempt to view job codes");
                }
                else
                {
                    response.Status = false;
                    response.Message = "";

                    ////Event logging
                    //Logging.EventLog(headerUserId, "Attempt to view job codes");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        //author:Mani
        //date: 08-Aug-2019
        //Update Job Code Status

        [Route("api/update-job-code-status")]
        [HttpPost]
        public HttpResponseMessage UpdateJobCodesStatus(JobCodes model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from headers
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                //////////////////

                if (adminMethods.UpdateJobCodeStatus(model.JobCodeId, model.IsActive) > 0)
                {
                    response.Status = true;
                    response.Message = "Status updated successfully";


                    Logging.EventLog(headerUserId, "Attempt to update job code status");
                    Logging.EventLog(headerUserId, "Job code status updated successfully");
                }
                else
                {
                    response.Status = false;
                    response.Message = "Updating status failed";


                    //logging event
                    Logging.EventLog(headerUserId, "Attempt to update job code status");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }


        //author:Mani
        //date: 08-Aug-2019
        //delete Job Code 

        [Route("api/delete-job-code")]
        [HttpPost]
        public HttpResponseMessage DeleteJobCodes(JobCodes model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from headers
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                //////////////

                if (adminMethods.DeleteJobCode(model.JobCodeId) > 0)
                {
                    response.Status = true;
                    response.Message = "Job Code deleted successfully";

                    //logging event
                    Logging.EventLog(headerUserId, "Attempt to delete job code");
                    Logging.EventLog(headerUserId, "Job code deleted successfully");
                }
                else
                {
                    response.Status = false;
                    response.Message = "Deleting Job Code failed";

                    //logging event
                    Logging.EventLog(headerUserId, "Attempt to delete job code");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        #endregion

        #region Expense Codes

        //author:Mani
        //date: 20-Aug-2019
        //Expense Code Upsert
        [Route("api/expense-code-upsert")]
        [HttpPost]
        public HttpResponseMessage ExpenseCodeUpsert(ExpenseCodes model)
        {
            Response<bool> response = new Response<bool>();

            try
            {

                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                response = adminMethods.ExpenseCodeValidateUpsertAndGetResponse(response, model, headerUserId);

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = true;
                response.Message = response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        //author:Mani
        //date: 20-Aug-2019
        //Expense Codes Get
        [Route("api/get-expense-codes")]
        [HttpGet]
        public HttpResponseMessage GetExpenseCodes()
        {
            Response<List<ExpenseCodes>> response = new Response<List<ExpenseCodes>>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                List<ExpenseCodes> ExpenseCodesData = adminMethods.GetExpenseCodes();


                if (ExpenseCodesData.Count() > 0)
                {
                    response.Status = true;
                    response.Data = ExpenseCodesData;
                    response.Message = "";

                    //Event logging
                    //Logging.EventLog(headerUserId, "Attempt to view expense codes");
                }
                else
                {
                    response.Status = true;
                    response.Message = "";
                    //Event logging
                    //Logging.EventLog(headerUserId, "Attempt to view expense codes");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }


        //author:Mani
        //date: 20-Aug-2019
        //Update Expense Code Status

        [Route("api/update-expense-code-status")]
        [HttpPost]
        public HttpResponseMessage UpdateExpenseCodesStatus(ExpenseCodes model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                if (adminMethods.UpdateExpenseCodeStatus(model.ExpenseCodeId, model.IsActive) > 0)
                {
                    response.Status = true;
                    response.Message = "Status updated successfully";

                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to update expense code status");
                    Logging.EventLog(headerUserId, "Expense code status updated successfully");
                }
                else
                {
                    response.Status = false;
                    response.Message = "Updating status failed";

                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to update expense code status");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }


        //author:Mani
        //date: 20-Aug-2019
        //delete Expense Code 

        [Route("api/delete-expense-code")]
        [HttpPost]
        public HttpResponseMessage DeleteExpenseCode(ExpenseCodes model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                if (adminMethods.DeleteExpenseCode(model.ExpenseCodeId) > 0)
                {
                    response.Status = true;
                    response.Message = "Expense Code deleted successfully";

                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to delete expense code");
                    Logging.EventLog(headerUserId, "Expense code deleted successfully");

                }
                else
                {
                    response.Status = false;
                    response.Message = "Deleting Expense Code failed";

                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to delete expense code");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }


        #endregion

        #region Configure Kanban

        //author:Mani
        //date: 18-Aug-2019
        //Upsert kanban stage
        [Route("api/upsert-kanban-stage")]
        [HttpPost]
        public HttpResponseMessage UpsertKanbanStage(ConfigureKanban model)
        {
            Response<bool> responsetype = new Response<bool>();
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                if (Request.Headers.Contains("User"))
                {
                    string strUserId = Request.Headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                response = adminMethods.KanbanValidateUpsertAndGetResponse(response, model, headerUserId);

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        //author:Mani
        //date: 18-Aug-2019
        //Get kanban stages
        [Route("api/get-kanban-stages")]
        [HttpGet]
        public HttpResponseMessage GetKanbanStages(string KanbanTypeName)
        {
            Response<List<ConfigureKanban>> response = new Response<List<ConfigureKanban>>();

            try
            {

                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                //switch (KanbanTypeName.ToLower())
                //{
                //    case "quotes":
                //        //Event logging
                //        Logging.EventLog(headerUserId, "Attempt to view quotes kanban stages");
                //        break;
                //    case "projects":
                //        //Event logging
                //        Logging.EventLog(headerUserId, "Attempt to view projects kanban stages");
                //        break;
                //    case "sites":
                //        //Event logging
                //        Logging.EventLog(headerUserId, "Attempt to view sites kanban stages");
                //        break;
                //    case "sows":
                //        //Event logging
                //        Logging.EventLog(headerUserId, "Attempt to view sow kanban stages");
                //        break;
                //    case "tasks":
                //        //Event logging
                //        Logging.EventLog(headerUserId, "Attempt to view tasks kanban stages");
                //        break;
                //    default:

                //        break;
                //}

                List<ConfigureKanban> KanbanStepsData = adminMethods.GetKanbanStages().Where(x => x.KanbanTypeName.ToLower() == KanbanTypeName.ToLower()).ToList();


                if (KanbanStepsData.Count() > 0)
                {
                    response.Status = true;
                    response.Data = KanbanStepsData;
                    response.Message = "";


                }
                else
                {
                    response.Status = false;
                    response.Message = "";


                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        //author:Mani
        //date: 18-Aug-2019
        //Update kanban stage Status
        [Route("api/update-kanban-stage-status")]
        [HttpPost]
        public HttpResponseMessage UpdateKanbanStageStatus(ConfigureKanban model)
        {
            Response<bool> response = new Response<bool>();

            try
            {


                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////


                switch (model.KanbanTypeName.ToLower())
                {
                    case "quotes":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update quote stage status");
                        break;
                    case "projects":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update project stage status");
                        break;
                    case "sites":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update site status stage");
                        break;
                    case "sows":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update SOW stage status");
                        break;
                    case "tasks":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update task stage status");
                        break;
                    default:

                        break;
                }


                if (adminMethods.UpdateKanbanStageStatus(model.ConfigureKanbanId, model.IsActive) > 0)
                {
                    response.Status = true;
                    response.Message = "Status updated successfully";



                    switch (model.KanbanTypeName.ToLower())
                    {
                        case "quotes":
                            //Event logging
                            Logging.EventLog(headerUserId, "Quote stage status has been updated successfully");
                            break;
                        case "projects":
                            //Event logging
                            Logging.EventLog(headerUserId, "Project stage status has been updated successfully");
                            break;
                        case "sites":
                            //Event logging
                            Logging.EventLog(headerUserId, "Site stage status has been updated successfully");
                            break;
                        case "sows":
                            //Event logging
                            Logging.EventLog(headerUserId, "SOW stage status has been updated successfully");
                            break;
                        case "tasks":
                            //Event logging
                            Logging.EventLog(headerUserId, "Task stage status has been updated successfully");
                            break;
                        default:

                            break;
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = "Updating status failed";

                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to update status");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        //author:Mani
        //date: 08-Aug-2019
        //delete kanban stage
        [Route("api/delete-kanban-stage")]
        [HttpPost]
        public HttpResponseMessage DeleteKanbanStage(ConfigureKanban model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////


                switch (model.KanbanTypeName.ToLower())
                {
                    case "quotes":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to delete quote stage");
                        break;
                    case "projects":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to delete project stage");
                        break;
                    case "sites":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to delete site stage");
                        break;
                    case "sow":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to delete SOW stage");
                        break;
                    case "tasks":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to delete task stage");
                        break;
                    default:

                        break;
                }


                if (adminMethods.DeleteKanbanStage(model.ConfigureKanbanId) > 0)
                {
                    response.Status = true;
                    response.Message = model.KanbanTypeName + " kanban stage deleted successfully";

                    switch (model.KanbanTypeName.ToLower())
                    {
                        case "quotes":
                            //Event logging
                            Logging.EventLog(headerUserId, "Quote stage deleted successfully");
                            break;
                        case "projects":
                            //Event logging
                            Logging.EventLog(headerUserId, "Project stage deleted successfully");
                            break;
                        case "sites":
                            //Event logging
                            Logging.EventLog(headerUserId, "Site stage deleted successfully");
                            break;
                        case "sow":
                            //Event logging
                            Logging.EventLog(headerUserId, "SOW stage deleted successfully");
                            break;
                        case "tasks":
                            //Event logging
                            Logging.EventLog(headerUserId, "Task stage deleted successfully");
                            break;
                        default:

                            break;
                    }

                }
                else
                {
                    response.Status = false;
                    response.Message = "Deleting" + model.KanbanTypeName + " kanban stage failed";

                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        #endregion

        #region Common Master Data Tabs

        //author:Mani
        //date: 23-Aug-2019
        //Upsert kanban stage
        [Route("api/upsert-common-master-data")]
        [HttpPost]
        public HttpResponseMessage UpsertCommonMsterDta(CommonMasterData model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                response = adminMethods.CMDTabsValidateUpsertAndGetResponse(response, model, headerUserId);


                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = true;
                response.Message = response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        //author:Mani
        //date: 23-Aug-2019
        //Get common master data
        [Route("api/get-common-master-data")]
        [HttpGet]
        public HttpResponseMessage GetCommonMasterData(string commonMasterDataType)
        {
            Response<List<CommonMasterData>> response = new Response<List<CommonMasterData>>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                List<CommonMasterData> CommonMasterData = adminMethods.GetCommonMasterData();
                CommonMasterData = CommonMasterData.Where(x => x.CommonMasterDataCategory == commonMasterDataType).ToList();
                //switch (commonMasterDataType.ToLower())
                //{
                //    case "client types":

                //        //Event logging
                //        Logging.EventLog(headerUserId, "Attempt to view client types");
                //        break;

                //    case "project types":

                //        //Event logging
                //        Logging.EventLog(headerUserId, "Attempt to view project types");
                //        break;

                //    case "lead sources":

                //        //Event logging
                //        Logging.EventLog(headerUserId, "Attempt to view lead source");
                //        break;

                //    case "delay reasons":

                //        //Event logging
                //        Logging.EventLog(headerUserId, "Attempt to view delay reasons");
                //        break;

                //    case "survey purposes":

                //        //Event logging
                //        Logging.EventLog(headerUserId, "Attempt to view survey purpose");
                //        break;
                //    default:

                //        break;
                //}

                if (CommonMasterData.Count() > 0)
                {
                    response.Status = true;
                    response.Data = CommonMasterData;
                    response.Message = "";
                }
                else
                {
                    response.Status = true;
                    response.Message = "";
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        //author:Mani
        //date: 23-Aug-2019
        //Update common master data status
        [Route("api/update-common-master-data-status")]
        [HttpPost]
        public HttpResponseMessage UpdateCommonMasterDataStatus(CommonMasterData model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                string CommonMasterDataCategory = adminMethods.GetCommonMasterData().Where(x => x.CommonMasterDataId == model.CommonMasterDataId).Select(x => x.CommonMasterDataCategory).FirstOrDefault();

                switch (CommonMasterDataCategory.ToLower())
                {
                    case "client types":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update client type status");
                        break;
                    case "project types":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update project type status");
                        break;
                    case "lead sources":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update lead source status");
                        break;
                    case "delay reasons":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update delay reason status");
                        break;
                    case "survey purposes":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update survey purpose status");
                        break;
                    default:

                        break;
                }

                if (adminMethods.UpdateCommonMasterDataStatus(model.CommonMasterDataId, model.UserId, model.IsActive) > 0)
                {
                    response.Status = true;
                    response.Message = "Status updated successfully";

                    switch (CommonMasterDataCategory.ToLower())
                    {
                        case "client types":
                            //Event logging
                            Logging.EventLog(headerUserId, "Client type status updated successfully");
                            break;
                        case "project types":
                            //Event logging
                            Logging.EventLog(headerUserId, "Project type status updated successfully");
                            break;
                        case "lead sources":
                            //Event logging
                            Logging.EventLog(headerUserId, "Lead source status updated successfully");
                            break;
                        case "delay reasons":
                            //Event logging
                            Logging.EventLog(headerUserId, "Delay reason status updated successfully");
                            break;
                        case "survey purposes":
                            //Event logging
                            Logging.EventLog(headerUserId, "Survey purpose status updated successfully");
                            break;
                        default:

                            break;
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = "Updating status failed";
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        //author:Mani
        //date: 23-Aug-2019
        //delete common master data
        [Route("api/delete-common-master-data")]
        [HttpPost]
        public HttpResponseMessage DeleteCommonMasterData(CommonMasterData model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////


                switch (model.CommonMasterDataCategory.ToLower())
                {
                    case "client types":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to delete client type");
                        break;
                    case "project types":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to delete project type");
                        break;
                    case "lead sources":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to delete lead source");
                        break;
                    case "delay reasons":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to delete delay reason");
                        break;
                    case "survey purposes":
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to delete survey purpose");
                        break;
                    default:

                        break;
                }

                if (adminMethods.DeleteCommonMasterData(model.CommonMasterDataId, model.UserId) > 0)
                {
                    response.Status = true;
                    response.Message = model.CommonMasterDataCategory.Remove(model.CommonMasterDataCategory.Length - 1, 1) + " deleted successfully";
                    switch (model.CommonMasterDataCategory.ToLower())
                    {
                        case "client types":
                            //Event logging
                            Logging.EventLog(headerUserId, "Client type deleted successfully");
                            break;
                        case "project types":
                            //Event logging
                            Logging.EventLog(headerUserId, "Project type deleted successfully");
                            break;
                        case "lead sources":
                            //Event logging
                            Logging.EventLog(headerUserId, "Lead source deleted successfully");
                            break;
                        case "delay reasons":
                            //Event logging
                            Logging.EventLog(headerUserId, "Delay reason deleted successfully");
                            break;
                        case "survey purposes":
                            //Event logging
                            Logging.EventLog(headerUserId, "Survey purpose deleted successfully");
                            break;
                        default:

                            break;
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = "Deleting data failed";
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        #endregion

        #region Settings
        //author:Vamsi 
        //date: 17/03/2020
        //To get all country specific states
        [Route("api/get-all-country-specific-province-details")]
        [HttpGet]
        public IHttpActionResult GetCountrySpecificProvinceDetails()
        {
            Response<List<SettingsCountries>> response = new Response<List<SettingsCountries>>();
            try
            {
                //Getting user id from headers
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                List<SettingsCountries> countrystates = adminMethods.GetCountriesWithStates();
                if (countrystates.Count() > 0)
                {
                    response.Status = true;
                    response.Data = countrystates;
                    response.Message = "";
                }
                else
                {
                    response.Status = false;
                    response.Message = "";
                }

                return Ok(JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return InternalServerError(ex);
            }
        }

        //author:Mani
        //date: 17-Aug-2019
        //Settings Get
        [Route("api/get-settings")]
        [HttpGet]
        public HttpResponseMessage GetSettings()
        {
            Response<List<Settings>> response = new Response<List<Settings>>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                List<Settings> SettingsData = adminMethods.GetSettings();


                if (SettingsData.Count() > 0)
                {
                    response.Status = true;
                    response.Data = SettingsData;
                    response.Message = "";

                    //Event logging
                    //Logging.EventLog(headerUserId, "Attempt to view settings");
                }
                else
                {
                    response.Status = true;
                    response.Message = "";

                    //Event logging
                    //Logging.EventLog(headerUserId, "Attempt to view settings");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }


        //author: Mani
        //date: 17-Aug-2019
        //Update Settings

        [Route("api/update-setting-value")]
        [HttpPost]
        public HttpResponseMessage UpdateSettings(Settings model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                if (adminMethods.UpdateSettings(model.SettingId, model.Value, model.InputType) > 0)
                {
                    response.Status = true;
                    response.Message = "Setting updated successfully";

                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to update settings");
                    Logging.EventLog(headerUserId, "Settings updated successfully");
                }
                else
                {
                    response.Status = false;
                    response.Message = "Updating failed";

                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to update settings");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        #endregion

        #region Asset Categories


        //author:Mani
        //date: 24-Aug-2019
        //Asset Category Upsert
        [Route("api/asset-category-upsert")]
        [HttpPost]
        public HttpResponseMessage AssetCategoryUpsert(AssetCategories model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////


                response = adminMethods.AssetCategoryValidateUpsertAndGetResponse(response, model, headerUserId);

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = true;
                response.Message = response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        //author:Mani
        //date: 24-Aug-2019
        //Asset Category Get
        [Route("api/get-asset-category")]
        [HttpGet]
        public HttpResponseMessage GetAssetCategories()
        {
            Response<List<AssetCategories>> response = new Response<List<AssetCategories>>();

            try
            {

                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////


                List<AssetCategories> AssetCategoryData = adminMethods.GetAssetCategories();


                if (AssetCategoryData.Count() > 0)
                {
                    response.Status = true;
                    response.Data = AssetCategoryData;
                    response.Message = "";
                    //Event logging
                    //Logging.EventLog(headerUserId, "Attempt to view asset categories");
                }
                else
                {
                    response.Status = true;
                    response.Message = "";
                    //Event logging
                    //Logging.EventLog(headerUserId, "Attempt to view asset categories");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        //author:Mani
        //date: 24-Aug-2019
        //Update Asset Category Status

        [Route("api/update-asset-category-status")]
        [HttpPost]
        public HttpResponseMessage UpdateAssetCategoryStatus(AssetCategories model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                if (adminMethods.UpdateAssetCategoryStatus(model.AssetCategoryId, model.IsActive) > 0)
                {
                    response.Status = true;
                    response.Message = "Status updated successfully";
                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to update asset category status");
                    Logging.EventLog(headerUserId, "Asset category status updated successfully");
                }
                else
                {
                    response.Status = false;
                    response.Message = "Updating status failed";
                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to update asset category status");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }


        //author:Mani
        //date: 24-Aug-2019
        //delete Asset Category 

        [Route("api/delete-asset-category")]
        [HttpPost]
        public HttpResponseMessage DeleteAssetCategory(AssetCategories model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////


                if (adminMethods.DeleteAssetCategory(model.AssetCategoryId) > 0)
                {
                    response.Status = true;
                    response.Message = "Asset Category deleted successfully";
                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to delete asset category");
                    Logging.EventLog(headerUserId, "Asset category deleted successfully");

                }
                else
                {
                    response.Status = false;
                    response.Message = "Deleting Asset Category failed";
                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to delete asset category");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        #endregion

        #region Configure Questionnaire


        //author:Mani
        //date: 25-Aug-2019
        //Configure Questionnaire Upsert
        [Route("api/config-question-upsert")]
        [HttpPost]
        public HttpResponseMessage ConfigQuestionUpsert(ConfigQuestions model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////


                response = adminMethods.CQValidateUpsertAndGetResponse(response, model, headerUserId);

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = true;
                response.Message = response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        //author:Mani
        //date: 25-Aug-2019
        //Configured Questionns Get
        [Route("api/get-config-questions")]
        [HttpGet]
        public HttpResponseMessage GetConfigQuestions()
        {
            Response<List<ConfigQuestions>> response = new Response<List<ConfigQuestions>>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                List<ConfigQuestions> ConfigQuestionsData = adminMethods.GetConfigQuestions();
                //Event logging
                //Logging.EventLog(headerUserId, "Attempt to view question");

                if (ConfigQuestionsData.Count() > 0)
                {
                    response.Status = true;
                    response.Data = ConfigQuestionsData;
                    response.Message = "";
                }
                else
                {
                    response.Status = true;
                    response.Message = "";
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        //author:Mani
        //date: 25-Aug-2019
        //Configured Questionns Get
        [Route("api/get-quote-questions")]
        [HttpGet]
        public HttpResponseMessage GetQuoteQuestions(long? quoteId, long? SurveyPurposeId)
        {
            Response<List<SelectedQuestions>> response = new Response<List<SelectedQuestions>>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                List<SelectedQuestions> ConfigQuestionsData = adminMethods.GetConfigQuestions(quoteId, SurveyPurposeId);
                //Event logging
                //Logging.EventLog(headerUserId, "Attempt to view question");

                response.Status = true;
                response.Data = ConfigQuestionsData;
                response.Message = "";

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }
        //author:Mani
        //date: 25-Aug-2019
        //delete Configure Question

        [Route("api/delete-config-question")]
        [HttpPost]
        public HttpResponseMessage DeleteConfigQuestion(ConfigQuestions model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                //Event logging
                Logging.EventLog(headerUserId, "Attempt to delete question");

                if (adminMethods.DeleteConfigQuestion(model.ConfigQuestionId, model.UserId) > 0)
                {
                    response.Status = true;
                    response.Message = "Configured question deleted successfully";
                    //Event logging
                    Logging.EventLog(headerUserId, "Question deleted successfully");
                }
                else
                {
                    response.Status = false;
                    response.Message = "Deleting Asset Category failed";
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }

        #endregion

        #region Leave Reasons


        //author:Mani
        //date: 29-Aug-2019
        //Leave Reason Upsert
        [Route("api/leave-reason-upsert")]
        [HttpPost]
        public HttpResponseMessage LeaveReasonUpsert(LeaveReasons model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                response = adminMethods.LeaveReasonsValidateUpsertAndGetResponse(response, model, headerUserId);


                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = true;
                response.Message = response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            finally
            {
                //will use in near future
            }
        }

        //author:Mani
        //date: 29-Aug-2019
        //Leave Reason Get
        [Route("api/get-leave-reasons")]
        [HttpGet]
        public HttpResponseMessage GetLeaveReasons()
        {
            Response<List<LeaveReasons>> response = new Response<List<LeaveReasons>>();

            try
            {

                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                List<LeaveReasons> LeaveReasonsData = adminMethods.GetLeaveReasons();


                if (LeaveReasonsData.Count() > 0)
                {
                    response.Status = true;
                    response.Data = LeaveReasonsData;
                    response.Message = "";
                    //Event logging
                    //Logging.EventLog(headerUserId, "Attempt to view leave reason");
                }
                else
                {
                    response.Status = true;
                    response.Message = "";
                    //Event logging
                    //Logging.EventLog(headerUserId, "Attempt to view leave reason");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            finally
            {
                //will use in near future
            }
        }


        //author:Mani
        //date: 29-Aug-2019
        //delete Leave Reason
        [Route("api/delete-leave-reason")]
        [HttpPost]
        public HttpResponseMessage DeleteLeaveReason(LeaveReasons model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////
                if (adminMethods.DeleteLeaveReason(model.LeaveReasonId, model.UserId) > 0)
                {
                    response.Status = true;
                    response.Message = "Leave reason deleted successfully";
                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to delete leave reason");
                    Logging.EventLog(headerUserId, "Leave reason deleted successfully");
                }
                else
                {
                    response.Status = false;
                    response.Message = "Deleting leave reason failed";
                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to delete leave reason");
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            finally
            {
                //will use in near future
            }
        }



        #endregion



        #region Leave Request
        //Author Name   : Siddhant Chawade
        //Created Date  : 13th Nov 2019
        //Description   : Get Leaves Activity Log data by filter
        [Route("api/get-leave-activity-log-filter")]
        [HttpGet]
        public HttpResponseMessage GetLeaveActivityLogFilter(long UserId, long? AbsenceTypeId, long? LeaveReasonId, long? ActivityTypeId, long? UserNameId, string FromDate, string ToDate)
        {
            Response<GetLeaveRequests> response = new Response<GetLeaveRequests>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                GetLeaveRequests LeavesActivityLogData = adminMethods.GetLeaveActivityLogFilter(UserId, AbsenceTypeId, LeaveReasonId, ActivityTypeId, UserNameId, string.IsNullOrEmpty(FromDate) || FromDate == "null" || FromDate == "undefined" ? Convert.ToDateTime("1/1/1111") : Convert.ToDateTime(FromDate), string.IsNullOrEmpty(ToDate) || ToDate == "null" || ToDate == "undefined" ? Convert.ToDateTime("1/1/1111") : Convert.ToDateTime(ToDate));


                response.Status = true;
                response.Data = LeavesActivityLogData;
                response.Message = "";

                //Event logging
                Logging.EventLog(headerUserId, "");


                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
        }
        //author:Mani
        //date: 30-Aug-2019
        //Leave Request Upsert
        [Route("api/leave-request-upsert")]
        [HttpPost]
        public HttpResponseMessage LeaveRequestUpsert(LeaveRequests model)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                response = adminMethods.LeaveRequestValidateUpsertAndGetResponse(response, model, headerUserId);

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            finally
            {
                //will use in near future
            }
        }

        //author:Mani
        //date: 30-Aug-2019
        //Leave Request Get
        [Route("api/get-leave-requests")]
        [HttpGet]
        public HttpResponseMessage GetLeaveRequests(long? UserId, bool? isSuperAdmin, long? filterRMId, long? filterUserId, DateTime? filterFromDate, DateTime? filterToDate, int? type)
        {
            Response<GetLeaveRequests> response = new Response<GetLeaveRequests>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                GetLeaveRequests LeaveRequestsData = adminMethods.GetLeaveRequests(UserId, isSuperAdmin, filterRMId, filterUserId, filterFromDate, filterToDate, type);


                response.Status = true;
                response.Data = LeaveRequestsData;
                response.Message = "";

                //Event logging
                Logging.EventLog(headerUserId, "");


                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            finally
            {
                //will use in near future
            }
        }


        //author:Mani
        //date: 30-Aug-2019
        //delete Leave Request
        [Route("api/delete-leave-request")]
        [HttpPost]
        public HttpResponseMessage DeleteLeaveRequest(LeaveRequests model)
        {
            Response<bool> response = new Response<bool>();

            try
            {

                if (adminMethods.DeleteLeaveRequest(model) > 0)
                {
                    response.Status = true;
                    response.Message = "Leave request deleted successfully";

                }
                else
                {
                    response.Status = false;
                    response.Message = "Deleting leave request failed";
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            finally
            {
                //will use in near future
            }
        }

        //author:Mani
        //date: 01-Sep-2019
        //Review Leave Request
        [Route("api/review-leave-request")]
        [HttpPost]
        public HttpResponseMessage ReviewLeaveRequest(List<LeaveRequests> model)
        {
            Response<bool> response = new Response<bool>();

            try
            {

                if (model.Count > 1)
                {
                    if (adminMethods.ReviewLeaveRequest(model) > 0)
                    {
                        if ((bool)model.FirstOrDefault().IsApproved)
                        {
                            response.Status = true;
                            response.Message = "Leave requests have been reviewed and approved successfully";
                        }
                        else
                        {
                            response.Status = true;
                            response.Message = "Leave requests have been reviewed and declined successfully";
                        }

                    }
                    else
                    {
                        if ((bool)model.FirstOrDefault().IsApproved)
                        {
                            response.Status = true;
                            response.Message = "Leave requests approval have been failed";
                        }
                        else
                        {
                            response.Status = true;
                            response.Message = "Leave requests declination have been failed";
                        }
                    }
                }
                else if (model.Count == 1)
                {

                    if (adminMethods.ReviewLeaveRequest(model) > 0)
                    {
                        if ((bool)model.FirstOrDefault().IsApproved)
                        {
                            response.Status = true;
                            response.Message = "Leave request has been reviewed and approved successfully";
                        }
                        else
                        {
                            response.Status = true;
                            response.Message = "Leave request has been reviewed and declined successfully";
                        }

                    }
                    else
                    {
                        if ((bool)model.FirstOrDefault().IsApproved)
                        {
                            response.Status = true;
                            response.Message = "Leave request approval has been failed";
                        }
                        else
                        {
                            response.Status = true;
                            response.Message = "Leave request declination has been failed";
                        }
                    }

                    //}
                    //else
                    //{
                    //    //if (model.IsApproved == true)
                    //    //{
                    //    //    response.Status = false;
                    //    //    response.Message = "Approving leave request failed";
                    //    //}
                    //    //else
                    //    //{
                    //    //    response.Status = false;
                    //    //    response.Message = "Declining leave request failed";
                    //    //}
                    //}
                }
                else
                {
                    response.Status = false;
                    response.Message = "Something went wrong, please contact administrator";
                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            finally
            {
                //will use in near future
            }
        }

        //author:Mani
        //date: 21-Oct-2019
        //Get actual paid hours
        [Route("api/get-actual-paid-hours")]
        [HttpPost]
        public HttpResponseMessage GetActualHours(LeaveRequests model)
        {
            Response<int> response = new Response<int>();

            try
            {
                int duration = 0;


                int UserHoursPerDay = (int)adminMethods.GetAllIBWUsers().Where(x => x.int_user_id == model.UserId).Select(y => y.int_hrs_per_day).FirstOrDefault();

                if (model.TypeOfAbsence == "Full Day")
                {

                    duration = 1;
                    var holidaysList = adminMethods.GetHolidays().Select(x => x.holidayDate).ToList();
                    if (holidaysList.Exists(x => x == model.FromDate))
                        duration = 0;

                    if (model.FromDate.Value.DayOfWeek == DayOfWeek.Sunday || model.FromDate.Value.DayOfWeek == DayOfWeek.Saturday)
                        duration = 0;
                }
                else if (model.TypeOfAbsence == "Multiple Days")
                {

                    DateTime start = (DateTime)model.FromDate.Value.AddDays(1);
                    DateTime end = (DateTime)model.ToDate.Value.AddDays(1);

                    var Dateslist1 = Enumerable.Range(0, 1 + end.Subtract(start).Days).Select(day => start.AddDays(day)).ToList();
                    List<DateTime> WorkingDates = new List<DateTime>();
                    List<DateTime> ValidDates = new List<DateTime>();
                    var holidaysList = adminMethods.GetHolidays().Select(x => x.holidayDate).ToList();


                    WorkingDates = Dateslist1.Where(x => x.DayOfWeek != DayOfWeek.Sunday && x.DayOfWeek != DayOfWeek.Saturday).ToList();


                    foreach (DateTime date in WorkingDates)
                    {
                        if (!holidaysList.Exists(x => x.Value.Date == date.Date))
                            ValidDates.Add(date);
                    }

                    duration = ValidDates.Count();
                }


                response.Status = false;
                response.Message = "";
                response.Data = UserHoursPerDay * duration;
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {

                throw ex;

                response.Status = false;
                response.Message = "Something went wrong please contact administrator";
            }
            finally
            {
                //will use in near future
            }
        }


        //author:Mani
        //date: 24-Oct-2019
        //Leave Request Get
        [Route("api/get-leave-activity-log")]
        [HttpGet]
        public HttpResponseMessage GetLeaveActivityLog(long UserId)
        {
            Response<GetLeaveRequests> response = new Response<GetLeaveRequests>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////

                GetLeaveRequests LeavesActivityLogData = adminMethods.GetLeaveActivityLog(UserId);


                response.Status = true;
                response.Data = LeavesActivityLogData;
                response.Message = "";

                //Event logging
                Logging.EventLog(headerUserId, "");


                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            finally
            {
                //will use in near future
            }
        }

        //author:Mani
        //date: 25-Oct-2019
        //update Leave activity log read status
        [Route("api/update-leave-activity-log-read_status")]
        [HttpGet]
        public HttpResponseMessage UpdateActivityLogReadstatus(long UserId)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                //Getting user id from header
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                ///////////////////



                response.Status = true;
                response.Data = adminMethods.UpdateActivityLogReadstatus(UserId) > 0 ? true : false;
                response.Message = "";

                //Event logging
                Logging.EventLog(headerUserId, "");


                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new { response }));
            }
            finally
            {
                //will use in near future
            }
        }


        #endregion



        #endregion

        #region Asset
        [Route("api/get-assets")]
        [HttpGet]
        public HttpResponseMessage GetAssets()
        {
            Response<AssetModel> response = new Response<AssetModel>();
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
                //Logging.EventLog(headerUserId, "Attempt to view assets");
                AssetModel assets = new AssetModel();
                assets.Asset = adminMethods.GetAssets();
                assets.Categories = adminMethods.GetAssetCategoriesForMaster();
                response.Status = true;
                response.Message = "";
                response.Data = assets;
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(new { response }));
            }
        }
        [Route("api/get-assets-category-serial")]
        [HttpPost]
        public HttpResponseMessage GetAssetsCategorySerial([FromBody]JObject model)
        {
            Response<string> response = new Response<string>();
            try
            {
                string categoryName = model.Value<string>("categoryName");
                string categoryId = model.Value<string>("categoryId");
                string serialNumber = "";
                if (categoryName != null && categoryId != null)
                {
                    serialNumber = adminMethods.GetAssetCategoriesSerial(Convert.ToInt64(categoryId));
                    response.Status = true;
                    response.Data = serialNumber;
                    return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
                }
                else
                {
                    response.Status = false;
                    response.Data = "";
                    return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
                }
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(new { response }));
            }
        }
        [Route("api/update-asset")]
        [HttpPost]
        public HttpResponseMessage UpdateAsset(GetAssetsResult model)
        {
            Response<bool> response = new Response<bool>();

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
                if (model.assetId == 0 || model.assetId == null)
                {
                    Logging.EventLog(headerUserId, "Attempt to add new asset");
                    Logging.EventLog(headerUserId, "Asset added successfully");
                }

                else
                {
                    Logging.EventLog(headerUserId, "Attempt to update asset details");
                    Logging.EventLog(headerUserId, "Asset details updated successfully");

                }
                response = adminMethods.UpsertAsset(model, null);

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(new { response }));
            }
        }
        [Route("api/bulk-insert-assets")]
        [HttpPost]
        public HttpResponseMessage BulkInsertAssets(List<GetAssetsResult> assetsData)
        {
            Response<string> response = new Response<string>();
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
                Logging.EventLog(headerUserId, "Attempt to upload assets in bulk");
                if (adminMethods.BulkInsertAssets(assetsData, null))
                {
                    response.Status = true;
                    response.Message = "Assets added successfully.";
                    Logging.EventLog(headerUserId, "Assets uploaded successfully");
                }
                else
                {
                    response.Status = false;
                    response.Message = "Operation failed";
                }
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(new { response }));
            }
        }
        [Route("api/asset-status-change")]
        [HttpPost]
        public HttpResponseMessage AssetStatusChange(GetAssetsResult model)
        {
            Response<string> output = new Response<string>();
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
                Logging.EventLog(headerUserId, "Attempt to update asset status");
                if (adminMethods.ChangeAssetStatus(model, null))
                {
                    output.Status = true;
                    output.Message = "Status changed successfully";
                    Logging.EventLog(headerUserId, "Asset status updated successfully");

                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(output));
            }
            return response;
        }
        [Route("api/asset-delete")]
        [HttpGet]
        public HttpResponseMessage DeleteAsset(long? assetId)
        {
            Response<string> output = new Response<string>();
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
                Logging.EventLog(headerUserId, "Attempt to delete asset");
                if (adminMethods.DeleteAsset(assetId, null))
                {
                    output.Status = true;
                    output.Message = "Asset deleted successfully";
                    Logging.EventLog(headerUserId, "Asset deleted successfully");
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(output));
            }
            return response;
        }
        #endregion
        #region Holiday
        [Route("api/get-holidays")]
        [HttpGet]
        public HttpResponseMessage GetHolidays()
        {
            Response<List<GetHolidaysResult>> response = new Response<List<GetHolidaysResult>>();
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
                // Logging.EventLog(headerUserId, "Attempt to view important dates");
                List<GetHolidaysResult> holidays = new List<GetHolidaysResult>();
                holidays = adminMethods.GetHolidays();
                response.Status = true;
                response.Data = holidays;
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(new { response }));
            }
        }
        [Route("api/save-holidays")]
        [HttpPost]
        public HttpResponseMessage SaveHolidays(List<GetHolidaysResult> holidays)
        {
            Response<bool> response = new Response<bool>();

            try
            {
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;

                if (adminMethods.SaveHolidays(holidays, null))
                {

                    if (headers.Contains("User"))
                    {
                        string strUserId = headers.GetValues("User").First();
                        headerUserId = Convert.ToInt64(strUserId);
                    }
                    response.Status = true;
                    response.Message = "Holidays saved successfully";

                    Logging.EventLog(headerUserId, "Attempt to add new important date");
                    Logging.EventLog(headerUserId, "New important date added successfully");

                }
                else
                {
                    response.Status = false;
                    response.Message = "Operation failed";

                    Logging.EventLog(headerUserId, "Attempt to add new important date");

                }

                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = ex.ToString();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(new { response }));
            }
        }
        [Route("api/holiday-delete")]
        [HttpGet]
        public HttpResponseMessage HolidayDelete(long? holidayId)
        {
            Response<string> output = new Response<string>();
            try
            {
                if (adminMethods.DeleteHoliday(holidayId, null))
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
                    output.Message = "Holiday deleted successfully";


                    Logging.EventLog(headerUserId, "Attempt to delete important date");
                    Logging.EventLog(headerUserId, "Important date deleted successfully");
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(output));
            }
            return response;
        }
        #endregion
        #region EventAndErrorLog
        [Route("api/get-eventlog")]
        [HttpGet]
        public HttpResponseMessage GetEventLog()
        {
            Response<List<GetEventLogResult>> response = new Response<List<GetEventLogResult>>();
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
                // Logging.EventLog(headerUserId, "Attempt to view event log");
                List<GetEventLogResult> eventLog = new List<GetEventLogResult>();
                eventLog = adminMethods.GetEventLog();
                response.Status = true;
                response.Data = eventLog;
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(new { response }));
            }
        }
        [Route("api/get-errorlog")]
        [HttpGet]
        public HttpResponseMessage GetErrorLog()
        {
            Response<List<GetErrorLogResult>> response = new Response<List<GetErrorLogResult>>();
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
                // Logging.EventLog(headerUserId, "Attempt to view error log");
                List<GetErrorLogResult> errorLog = new List<GetErrorLogResult>();
                errorLog = adminMethods.GetErrorLog();
                response.Status = true;
                response.Data = errorLog;
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong, please contact administrator";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(new { response }));
            }
        }
        #endregion
        #region Clients

        //author:Shyam 
        //date: 16/08/2019
        //To Insert Users and Update IBW clients
        [Route("api/upsert-ibw-clients-all")]
        [HttpPost]
        public HttpResponseMessage ClientUpsert([FromBody]Clients model)
        {
            //Clients ibwClientData = new Clients();
            //List<Clients> clientsList = adminMethods.GetClients();
            //Address ibwAddress = new Address();
            Response<Clients> output = new Response<Clients>();

            try
            {
                if (model.clientID == null || model.clientID == 0)
                {
                    //if (clientsList.FindIndex(t => t.clientCode == model.clientCode) <= 0)
                    //{
                    Logging.EventLog(model.createdBy, "Attempt to add new client");

                    if (!adminMethods.VerifyClientName(model.clientID, model.clientName))
                    {
                        long? clientID = adminMethods.upsertClient(model);
                        if (clientID != 0 && clientID != null)
                        {
                            output.Status = true;
                            output.Message = "Client created successfully";
                            Logging.EventLog(model.createdBy, "New client has been added successfully");
                        }
                        //}
                        else
                        {
                            output.Status = false;
                            output.Message = "Client code already exists";
                        }
                    }
                    else {
                        output.Status = false;
                        output.Message = "Client name already exists";
                    }
                }
                else
                {
                    //if (clientsList.FindIndex(t => t.clientCode == model.clientCode && t.clientID != model.clientID) <= 0)
                    //{
                    Logging.EventLog(model.modifiedBy, "Attempt to update client details");
                    if (!adminMethods.VerifyClientName(model.clientID, model.clientName))
                    {
                        long? clientID = adminMethods.upsertClient(model);
                        if (clientID == null || clientID == 0)
                        {
                            output.Status = true;
                            output.Message = "Client updated successfully";
                            Logging.EventLog(model.modifiedBy, "Client details have been updated successfully");
                        }
                        //}
                        else
                        {
                            output.Status = false;
                            output.Message = "Client code already exists";
                        } 
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "Client name already exists";
                    }
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }

            return response;
        }
        [Route("api/verify-client-name")]
        [HttpGet]
        public HttpResponseMessage VerifyClientName(long? clientId, string clientName)
        {
            Response<bool> output = new Response<bool>();
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
                output.Data = adminMethods.VerifyClientName(clientId, clientName);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }
        //author:Shyam 
        //date: 17/08/2019
        //To Get All Clients
        [Route("api/get-all-clients")]
        [HttpGet]
        public HttpResponseMessage getAllClients()
        {
            Clients ibwClientData = new Clients();
            Response<List<Clients>> output = new Response<List<Clients>>();
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
                // Logging.EventLog(headerUserId, "Attempt to view clients grid view");

                List<Clients> clientsList = adminMethods.GetClients();
                if (clientsList != null && clientsList.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = clientsList;
                }
                else
                {
                    output.Status = false;
                    output.Data = new List<Clients>();
                    output.Message = "";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }
        [Route("api/get-client")]
        [HttpGet]
        public HttpResponseMessage getClient(long? clientId)
        {
            Clients ibwClientData = new Clients();
            Response<List<Clients>> output = new Response<List<Clients>>();
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
                // Logging.EventLog(headerUserId, "Attempt to view clients grid view");

                List<Clients> clientsList = adminMethods.GetClientById(clientId);
                if (clientsList != null && clientsList.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = clientsList;
                }
                else
                {
                    output.Status = false;
                    output.Data = new List<Clients>();
                    output.Message = "";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }
        //author:Shyam 
        //date: 17/08/2019
        //To delete client
        [Route("api/delete-client")]
        [HttpPost]
        public HttpResponseMessage DeleteClient([FromBody]Clients model)
        {
            Response<List<Clients>> output = new Response<List<Clients>>();
            try
            {
                Logging.EventLog(model.modifiedBy, "Attempt to delete a client");
                model.clientbtDelete = true;
                var data = adminMethods.ChangeClientState(model);
                if (data != null)
                {
                    output.Status = true;
                    output.Message = "Client deleted successfully";
                    Logging.EventLog(model.modifiedBy, "Client has been deleted successfully");
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Shyam 
        //date: 17/08/2019
        //To change client flag
        [Route("api/change-client-flag")]
        [HttpPost]
        public HttpResponseMessage ChangeClientFlag([FromBody]Clients model)
        {
            Response<List<Clients>> output = new Response<List<Clients>>();
            try
            {
                var data = adminMethods.ChangeClientState(model);

                Logging.EventLog(model.modifiedBy, "Attempt to make client as flagged/unflagged");

                if (data != null)
                {
                    output.Status = true;
                    output.Message = "Flag changed successfully";

                    Logging.EventLog(model.modifiedBy, "Client has been successfully marked as flagged/unflagged");

                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Shyam 
        //date: 17/08/2019
        //To change client status
        [Route("api/change-client-status")]
        [HttpPost]
        public HttpResponseMessage ChangeClientStatus([FromBody]Clients model)
        {
            Response<List<Clients>> output = new Response<List<Clients>>();
            try
            {
                Logging.EventLog(model.modifiedBy, "Attempt to update client status");

                var data = adminMethods.ChangeClientState(model);
                if (data != null)
                {
                    output.Status = true;
                    output.Message = "Status updated successfully";
                    Logging.EventLog(model.modifiedBy, "Client status updated successfully");
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Shyam 
        //date: 18/08/2019
        //To Get All Client Types
        [Route("api/get-client-types")]
        [HttpGet]
        public HttpResponseMessage getClientTypes()
        {
            Response<List<ClientTypes>> output = new Response<List<ClientTypes>>();
            try
            {
                List<ClientTypes> clientTypes = adminMethods.GetClientTypes();
                if (clientTypes != null || clientTypes.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = clientTypes;
                }
                else
                {
                    output.Status = false;
                    output.Data = new List<ClientTypes>();
                    output.Message = "";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Shyam 
        //date: 18/08/2019
        //To Get All Countries
        [Route("api/get-countries")]
        [HttpGet]
        public HttpResponseMessage getCountries()
        {
            Response<List<Countries>> output = new Response<List<Countries>>();
            try
            {
                List<Countries> countries = adminMethods.GetCountries();
                if (countries != null || countries.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = countries;
                }
                else
                {
                    output.Status = false;
                    output.Data = new List<Countries>();
                    output.Message = "";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Shyam 
        //date: 18/08/2019
        //To Get All States of a country
        [Route("api/get-states")]
        [HttpGet]
        public HttpResponseMessage getStates(long? id)
        {
            Response<List<States>> output = new Response<List<States>>();
            try
            {
                List<States> states = adminMethods.GetStates(id);
                if (states != null || states.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = states;
                }
                else
                {
                    output.Status = false;
                    output.Data = new List<States>();
                    output.Message = "";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Shyam 
        //date: 18/08/2019
        //To Get All muncipalities of a state
        [Route("api/get-muncipalities")]
        [HttpGet]
        public HttpResponseMessage getMuncipalities()
        {
            Response<List<Muncipalities>> output = new Response<List<Muncipalities>>();
            try
            {
                List<Muncipalities> muncipalities = adminMethods.GetMuncipalities();
                if (muncipalities != null || muncipalities.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = muncipalities;
                }
                else
                {
                    output.Status = false;
                    output.Data = new List<Muncipalities>();
                    output.Message = "";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Shyam 
        //date: 18/08/2019
        //To save grid columns
        [Route("api/save-gridcolumns")]
        [HttpPost]
        public HttpResponseMessage SaveGridColumns([FromBody]GridColumns model)
        {
            Response<string> output = new Response<string>();
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
                //Logging.EventLog(headerUserId, "Attempt to configure grid columns");

                long? rowsSaved = adminMethods.SaveGridColumns(model.gridColumnNames);

                //Logging.EventLog(headerUserId, "Grid columns configured successfully");
                if (rowsSaved != null)
                {
                    output.Status = true;
                    //output.Message = "Grid columns saved successfully";
                }
                else
                {
                    output.Status = false;
                    output.Message = "Internal server error";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Shyam 
        //date: 18/08/2019
        //To Get All muncipalities of a state
        [Route("api/get-gridcolumns")]
        [HttpGet]
        public HttpResponseMessage getGridColumns(long? id, string screenName)
        {
            Response<List<string>> output = new Response<List<string>>();
            try
            {
                List<string> gridColumns = adminMethods.GetGridColumns(id, screenName.Replace('_', ' '));
                if (gridColumns != null || gridColumns.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = gridColumns;
                }
                else
                {
                    output.Status = false;
                    output.Data = new List<string>();
                    output.Message = "";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }
        #endregion
        #region Contacts
        //author:Shyam 
        //date: 27/08/2019
        //To Insert Users and Update IBW contacts
        [Route("api/upsert-ibw-contacts-all")]
        [HttpPost]
        public HttpResponseMessage ContactUpsert([FromBody]Contacts model)
        {
            Contacts ibwContactsData = new Contacts();
            List<Contacts> contactsList = adminMethods.GetContacts();
            //Address ibwAddress = new Address();
            Response<Contacts> output = new Response<Contacts>();

            try
            {
                if (model.contactID == null || model.contactID == 0)
                {
                    Logging.EventLog(model.createdBy, "Attempt to add new contact");
                    if (adminMethods.ContactEmailValidation(model.contactEmail, model.clientID, null))
                    {
                        long? contactID = adminMethods.upsertContact(model);
                        if (contactID != 0 && contactID != null)
                        {
                            model.contactID = contactID;
                            output.Status = true;
                            output.Message = "Contact created successfully";
                            output.Data = model;
                            Logging.EventLog(model.createdBy, "New contact has been added successfully");
                        }
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "Contact already exists with the given email for the client";
                    }
                }
                else
                {
                    Logging.EventLog(model.modifiedBy, "Attempt to update contact details");
                    if (adminMethods.ContactEmailValidation(model.contactEmail, model.clientID, model.contactID))
                    {
                        long? contactID = adminMethods.upsertContact(model);
                        if (contactID == null || contactID == 0)
                        {
                            model.contactID = contactID;
                            output.Status = true;
                            output.Message = "Contact updated successfully";
                            output.Data = model;
                            Logging.EventLog(model.modifiedBy, "Contact details have been updated successfully");
                        }
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "Contact already exists with the given email for the client";
                    }
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }

            return response;
        }

        //author:Shyam 
        //date: 17/08/2019
        //To Get All Contacts
        [Route("api/get-all-contacts")]
        [HttpGet]
        public HttpResponseMessage getAllContacts()
        {
            Contacts ibwContactsData = new Contacts();
            Response<List<Contacts>> output = new Response<List<Contacts>>();
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
                // Logging.EventLog(headerUserId, "Attempt to view contacts");
                List<Contacts> contactsList = adminMethods.GetContacts();
                if (contactsList != null && contactsList.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = contactsList;
                }
                else
                {
                    output.Data = new List<Contacts>();
                    output.Status = false;
                    output.Message = "";
                }
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Shyam 
        //date: 17/08/2019
        //To change contact status
        [Route("api/change-contact-status")]
        [HttpPost]
        public HttpResponseMessage ChangeContactStatus([FromBody]Contacts model)
        {
            Response<List<Contacts>> output = new Response<List<Contacts>>();
            try
            {
                Logging.EventLog(model.modifiedBy, "Attempt to update contact status");
                var data = adminMethods.ChangeContactState(model);
                if (data != null)
                {
                    output.Status = true;
                    output.Message = "Status updated successfully";
                    Logging.EventLog(model.modifiedBy, "Contact status updated successfully");
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }

        //author:Shyam 
        //date: 17/08/2019
        //To delete client
        [Route("api/delete-contact")]
        [HttpPost]
        public HttpResponseMessage DeleteContact([FromBody]Contacts model)
        {
            Response<List<Contacts>> output = new Response<List<Contacts>>();
            try
            {
                Logging.EventLog(model.modifiedBy, "Attempt to delete a contact");
                model.contactbtDelete = true;
                var data = adminMethods.ChangeContactState(model);
                if (data != null)
                {
                    output.Status = true;
                    output.Message = "Contact deleted successfully";
                    Logging.EventLog(model.modifiedBy, "Contact has been deleted successfully");
                }
                else
                {
                    output.Status = false;
                    output.Message = "Something went wrong";
                }

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            return response;
        }
        #endregion

    }


}
