using API_IBW.Business_Classes;
using API_IBW.DB_Models;
using API_IBW.HelperMethods;
using API_IBW.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using API_IBW.Models;

namespace API_IBW.Controllers
{
    [ExceptionHandler]
    public class LoginController : ApiController
    {
        private Login _login = new Business_Classes.Login();
        public HttpResponseMessage Login([FromBody]JObject model)
        {
            try
            {
                Logging.EventLog(null, "User attempted to login to the application");
                //Creating instance for response.
                HttpResponseMessage response = new HttpResponseMessage();
                Response<LoginResponse> output = new Response<LoginResponse>();

                //Getting all the values from model.
                string userName = model.Value<string>("email");
                string password = model.Value<string>("password");


                //invoking custom data method
                var userData = _login.UserAuthentication(userName, password);

                if (userData != null)
                {
                    //check whether the user is active or not
                    if (userData.bt_status == true)
                    {

                        //verify the user's password
                        if (userData.vc_password == Cryptography.Encrypt(password))
                        {
                            var accessToken = Guid.NewGuid().ToString("n");
                            userData.vc_auth_token = accessToken;
                            //save access token in database
                            _login.UpdateUserAuthToken(userData.int_user_id, accessToken);


                            output.Message = "Successfully Logged In.";
                            output.Status = true;
                            output.Data = new LoginResponse { UserDetails = userData, UserRights = _login.GetUserRights(userData.int_user_id), Roles = _login.GetUserRoles(userData.int_user_id).Select(x => x.vc_role_name).Distinct().ToList(), SettingDetails = _login.GetAllSettings() };
                            response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
                            Logging.EventLog(userData.int_user_id, "User has successfully logged in to the application");
                        }
                        else
                        {
                            output.Message = "Login failed due to incorrect password.";
                            output.Status = false;

                            response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
                            Logging.EventLog(null, "User has failed to login to the application");
                        }

                    }
                    else
                    {
                        output.Message = "Login failed. User is not active.";
                        output.Status = false;
                        response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
                        Logging.EventLog(null, "User has failed to login to the application");
                    }
                }
                else
                {
                    output.Message = "Login failed. User not exist.";
                    output.Status = false;
                    response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
                    Logging.EventLog(null, "User has failed to login to the application");
                }



                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        //public HttpResponseMessage Login([FromBody]JObject model)
        //{
        //    Logging.EventLog(null, "User attempted to login to the application");
        //    //Creating instance for response.
        //    HttpResponseMessage response = new HttpResponseMessage();
        //    Response<LoginResponse> output = new Response<LoginResponse>();
        //    //try
        //    //{
        //    //Getting all the values from model.
        //    string userName = model.Value<string>("email");
        //    string password = model.Value<string>("password");


        //    //invoking custom data method
        //    var userData = _login.UserAuthentication(userName, password);

        //    if (userData != null)
        //    {
        //        //check whether the user is active or not
        //        if (userData.bt_status == true)
        //        {

        //            //verify the user's password
        //            if (userData.vc_password == Cryptography.Encrypt(password))
        //            {
        //                var accessToken = Guid.NewGuid().ToString("n");
        //                userData.vc_auth_token = accessToken;
        //                //save access token in database
        //                _login.UpdateUserAuthToken(userData.int_user_id, accessToken);


        //                output.Message = "Successfully Logged In.";
        //                output.Status = true;
        //                output.Data = new LoginResponse { UserDetails = userData, UserRights = _login.GetUserRights(userData.int_user_id), Roles = _login.GetUserRoles(userData.int_user_id).Select(x => x.vc_role_name).Distinct().ToList() };
        //                Logging.EventLog(userData.int_user_id, "User has successfully logged in to the application");
        //                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
        //            }
        //            else
        //            {
        //                Logging.EventLog(null, "User has failed to login to the application");
        //                output.Message = "Login failed due to incorrect password.";
        //                output.Status = false;

        //                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
        //            }

        //        }
        //        else
        //        {
        //            Logging.EventLog(null, "User has failed to login to the application");
        //            output.Message = "Login failed. User is not active.";
        //            output.Status = false;
        //            response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
        //        }
        //    }
        //    else
        //    {
        //        Logging.EventLog(null, "User has failed to login to the application");
        //        output.Message = "Login failed. User not exist.";
        //        output.Status = false;
        //        response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
        //    }

        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    output.Message = ex.Message;
        //    //    output.Status = false;

        //    //    response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, JObject.FromObject(output));
        //    //}

        //    return response;
        //}

        //public HttpResponseMessage Login([FromBody]JObject model)
        //{
        //    //Creating instance for response.
        //    HttpResponseMessage response = new HttpResponseMessage();
        //    Response<sp_getAllIBWUsersResult> output = new Response<sp_getAllIBWUsersResult>();
        //    try
        //    {
        //        //Getting all the values from model.
        //        string userName = model.Value<string>("email");
        //        string password = model.Value<string>("password");


        //        //invoking custom data method
        //        var userData = _login.UserAuthentication(userName, password);

        //        if (userData != null)
        //        {
        //            //check whether the user is active or not
        //            if (userData.bt_status == true)
        //            {

        //                //verify the user's password
        //                if (userData.vc_password == Cryptography.Encrypt(password))
        //                {
        //                    var accessToken = Guid.NewGuid().ToString("n");
        //                    userData.vc_auth_token = accessToken;
        //                    //save access token in database
        //                    _login.UpdateUserAuthToken(userData.int_user_id, accessToken);


        //                    output.Message = "Successfully Logged In.";
        //                    output.Status = true;
        //                    //output.Data = new LoginResponse { UserDetails = userData,UserRights = _login.GetUserRights(userData.int_user_id) };
        //                    output.Data = userData;
        //                    response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
        //                }
        //                else
        //                {
        //                    output.Message = "Login failed due to incorrect password.";
        //                    output.Status = false;

        //                    response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
        //                }

        //            }
        //            else
        //            {
        //                output.Message = "Login failed. User is not active.";
        //                output.Status = false;
        //                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
        //            }
        //        }
        //        else
        //        {
        //            output.Message = "Login failed. User not exist.";
        //            output.Status = false;
        //            response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        output.Message = ex.Message;
        //        output.Status = false;

        //        response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, JObject.FromObject(output));
        //    }

        //    return response;
        //}
        [Route("api/send-forgot-password")]
        public HttpResponseMessage SendForgotPasswordEmail([FromBody]JObject model)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            Response<string> output = new Response<string>();
            try
            {
                Logging.EventLog(null, "User attempted to reset password");
                //Getting all the values from model.
                string vcEmail = model.Value<string>("email");
                output = _login.SendForgotPasswordEmail(vcEmail, false);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                Logging.EventLog(null, "User failed to reset password ");
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, JObject.FromObject(output));
            }
            return response;
        }
        [Route("api/get-user-detail")]
        [HttpPost]
        public HttpResponseMessage GetUserDetails([FromBody]JObject model)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            Response<sp_getAllIBWUsersResult> output = new Response<sp_getAllIBWUsersResult>();
            try
            {
                //Getting all the values from model.
                
                string userId = model.Value<string>("userId");
                output.Status = true;
                output.Data = _login.GetUserDetails(Convert.ToInt64(userId));
                Logging.EventLog(Convert.ToInt64(userId), "Attempt to update profile details");
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, JObject.FromObject(output));
            }
            return response;
        }
        [Route("api/get-user-leave-detail")]
        [HttpPost]
        public HttpResponseMessage GetUserLeaveDetails([FromBody]JObject model)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            Response<sp_getAllIBWUsersResult> output = new Response<sp_getAllIBWUsersResult>();
            try
            {
                //Getting all the values from model.

                string userId = model.Value<string>("userId");
                output.Status = true;
                output.Data = _login.GetUserDetails(Convert.ToInt64(userId));
                //Logging.EventLog(Convert.ToInt64(userId), "Attempt to update profile details");
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, JObject.FromObject(output));
            }
            return response;
        }
        [Route("api/forgot-password")]
        public HttpResponseMessage ForgotPassword([FromBody]JObject model)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            Response<long> output = new Response<long>();
            try
            {
                //Getting all the values from model.
                string authToken = model.Value<string>("authToken");
                output = _login.ForgotPassword(authToken);
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                Logging.EventLog(null, "User failed to reset password");
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, JObject.FromObject(output));
            }
            return response;
        }
        [Route("api/update-profile")]
        public HttpResponseMessage UpdateProfile([FromBody]JObject model)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            Response<string> output = new Response<string>();
            try
            {
                //Getting all the values from model.
                string userId = model.Value<string>("userId");
                string email = model.Value<string>("email");
                string userName = model.Value<string>("userName");
                string aliasName = model.Value<string>("aliasName");
                string phoneNumber = model.Value<string>("phoneNumber");
                output = _login.UpdateProfile(Convert.ToInt64(userId), email, userName, aliasName, phoneNumber);
                Logging.EventLog(Convert.ToInt64(userId), "Profile details upated successfully");
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, JObject.FromObject(output));
            }
            return response;
        }
        [Route("api/update-password")]
        public HttpResponseMessage UpdatePassword([FromBody]JObject model)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            Response<long> output = new Response<long>();
            try
            {
                Logging.EventLog(null, "User has successfully reset password");
                //Getting all the values from model.
                string userId = model.Value<string>("userId");
                string password = model.Value<string>("password");
                output = _login.UpdatePassword(password, Convert.ToInt64(userId));
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                Logging.EventLog(null, "User failed to reset password");
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, JObject.FromObject(output));
            }
            return response;
        }
        [Route("api/change-password")]
        public HttpResponseMessage ChangePassword([FromBody]JObject model)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            Response<long> output = new Response<long>();
            try
            {
                //Getting all the values from model.
                string userId = model.Value<string>("userId");
                string password = model.Value<string>("password");
                string newpassword = model.Value<string>("newpassword");
                output = _login.ChangePassword(newpassword, password, Convert.ToInt64(userId));
                Logging.EventLog(Convert.ToInt64(userId), "Password has been changed successfully");
                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, JObject.FromObject(output));
            }
            return response;
        }
        [Route("api/logout")]
        [HttpGet]
        public HttpResponseMessage Logout(long? userId)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                //Getting user id 
                //string userId = string.Empty;
                //var user = Request.Headers.SingleOrDefault
                //                          (x => x.Key == "User");
                //userId = Convert.ToString(user.Value.SingleOrDefault());
                //long id = Convert.ToInt64(Cryptography.Decrypt(userId));
                //_login.UserLogOut(Convert.ToInt64(userId));
                _login.UserLogOut(Convert.ToInt64(userId));
                Logging.EventLog(Convert.ToInt64(userId), "User has successfully logged out");
                Response<tbl_ibw_user> output = new Response<tbl_ibw_user>() { Status = true, Message = "Logged out successfully" };

                response = Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                var output = new
                {
                    error = true,
                    message = ex.Message
                };
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, JObject.FromObject(output));
            }
            return response;
        }
        #region Bharath
        [Route("api/save-fcm-token")]
        [HttpPost]
        public HttpResponseMessage saveFCMToken([FromBody]FCMTokenModel model)
        {
            Response<string> output = new Response<string>();
            try
            {
                if (_login.insertFCMToken(model) > 0)
                {
                    output.Message = "";
                    output.Status = true;
                    output.Data = null;
                }
                else
                {
                    output.Message = "Failed To Save FCM Token";
                    output.Status = false;
                }
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                Logging.EventLog(null, "Failed to Save FCM Token");
                output.Status = false;
                output.Message = ex.Message;
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, JObject.FromObject(output));
            }
        }

        [Route("api/delete-fcm-token")]
        [HttpPost]
        public HttpResponseMessage deleteFCMToken([FromBody]FCMTokenModel model)
        {
            Response<string> output = new Response<string>();
            try
            {
                if (_login.deleteFCMToken(model) > 0)
                {
                    output.Message = "";
                    output.Status = true;
                    output.Data = null;
                }
                else
                {
                    output.Message = "Failed To Delete FCM Token";
                    output.Status = false;
                }
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                Logging.EventLog(null, "Failed to Save FCM Token");
                output.Status = false;
                output.Message = ex.Message;
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, JObject.FromObject(output));
            }
        }
        #endregion

    }
}
