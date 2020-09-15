using API_IBW.DB_Models;
using API_IBW.HelperMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API_IBW.Models;

namespace API_IBW.Business_Classes
{
    public class LoginResponse
    {
        public sp_getAllIBWUsersResult UserDetails { get; set; }
        public List<sp_getRightsOfUserResult> UserRights { get; set; }
        public List<string> Roles { get; set; }
        public List<getAllSettingsResult> SettingDetails { get; set; }
    }

    public class Login
    {
        #region Bharath

        public long? insertFCMToken(FCMTokenModel model)
        {
            long? result = null;
            _adminDB.insertFCMToken(model.UserID, model.FCMToken, ref result);
            // string[] deviceTokens =  { model.FCMToken };
            // FirebaseCloudMessaging.SendPushNotifications(deviceTokens, "Testing From API", "FHFHFHFHFHFHFHFHFHf");
            return result;
        }

        public long? deleteFCMToken(FCMTokenModel model)
        {
            long? result = null;
            _adminDB.deleteFCMToken(model.FCMToken, ref result);
            return result;
        }
        #endregion
        private AdminDataContext _adminDB = new AdminDataContext();

        public sp_getAllIBWUsersResult UserAuthentication(string email, string passsword)
        {
            try
            {
                return _adminDB.sp_getAllIBWUsers().Where(x => x.vc_email.ToLower() == email.ToLower() && x.bt_delete == false).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }
       
        public List<getAllSettingsResult> GetAllSettings()
        {
            return _adminDB.getAllSettings().ToList();
        }
        public List<sp_getRightsOfUserResult> GetUserRights(long userId)
        {
            return _adminDB.sp_getRightsOfUser(userId).ToList();
        }
        public List<sp_getRoleOfUserResult> GetUserRoles(long userId)
        {
            try
            {
                return _adminDB.sp_getRoleOfUser().Where(x => x.int_user_id == userId).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void UpdateUserAuthToken(long userId, string authToken)
        {
            try
            {
                var user = _adminDB.tbl_ibw_users.Where(x => x.int_user_id == userId).SingleOrDefault();
                user.vc_auth_token = authToken;
                _adminDB.SubmitChanges();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public sp_getAllIBWUsersResult GetUserDetails(long? userId)
        {
            try
            {
                return _adminDB.sp_getAllIBWUsers().Where(x => x.int_user_id == userId).FirstOrDefault();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public Response<string> SendForgotPasswordEmail(string email, bool isNew)
        {
            bool status = false;
            string message = string.Empty;
            try
            {
                string senderEmail = string.Empty;
                var settings = _adminDB.GetSettings().Where(x => x.vc_setting_name == "Notification Email").FirstOrDefault();
                if (settings != null)
                {
                    senderEmail = settings.vc_value;
                }
                var userData = _adminDB.tbl_ibw_users.Where(x => x.vc_email.ToLower() == email.ToLower() && x.bt_delete == false).FirstOrDefault();
                if (userData != null)
                {
                    Random rnd = new Random(99999999);
                    //string authToken = Cryptography.Encrypt(userData.intUserId.ToString());
                    var authToken = Guid.NewGuid().ToString("n");

                    if (isNew == true)
                    {
                        string strquery = Literals.WebUrl + "/#/set-password/" + authToken;
                        userData.vc_auth_token = authToken;
                        userData.dt_verificationlink_sent_on = DateTime.Now;
                        _adminDB.SubmitChanges();
                        StringBuilder strBody = new StringBuilder();
                        strBody.Append("Hello " + userData.vc_user_name + " " + userData.vc_alias_name + "," + "<br />");
                        strBody.Append("<br/>");
                        strBody.Append("<p>");
                        strBody.Append("You have been registered as a user with Azimuth.");
                        strBody.Append("<br/>");
                        strBody.Append("<br/>");
                        strBody.Append("To continue you must complete this verification process, <a href = " + strquery + " > click here </a> to setup your new password. <br/> Please note, this link will expire in 24 hrs and cannot be reused.");
                        strBody.Append("<br/>");
                        //strBody.Append("<br/>");
                        //strBody.Append("<a href = " + strquery + " > Click  </a>  to Verify and Set Your Password.");
                        //strBody.Append("<br/>");
                        //strBody.Append("<br/>");
                        //strBody.Append("Please note this link to set the password will expire in 24hrs. It is a one-time link and cannot be reused.");
                        //strBody.Append("<br/>");
                        strBody.Append("<br/>");
                        strBody.Append("Thanks,");
                        strBody.Append("<br/>");
                        strBody.Append(Literals.EmailSenderTeamName + ".");
                        strBody.Append("<br/>");
                        strBody.Append("</p>");
                        Mails mail = new Mails();
                        //mail.SendMail(userData.vcEmail, userData.vcFirstName + " " + userData.vcLastName, strquery, "Reset Password");
                        Task.Run(async () =>
                        {
                            await mail.SendMail(senderEmail, email, userData.vc_alias_name, strBody.ToString(), "Azimuth: New User Verification");
                        });
                        status = true;
                        message = "Set Password Link has been sent to Email, Please Set Password using the link.";
                    }
                    else
                    {
                        if (userData.bt_verified == true)
                        {
                            string strquery = Literals.WebUrl + "/#/reset-password/" + authToken;
                            userData.vc_auth_token = authToken;
                            userData.dt_verificationlink_sent_on = DateTime.Now;
                            _adminDB.SubmitChanges();
                            StringBuilder strBody = new StringBuilder();
                            strBody.Append("Hello " + userData.vc_user_name + " " + userData.vc_alias_name + "," + "<br />");
                            strBody.Append("<br/>");
                            strBody.Append("<p>");
                            strBody.Append("<a href = " + strquery + " > Click here </a> to reset your account password. <br/> Please note this link will expire in 24 hrs and cannot be reused.");
                            strBody.Append("<br/>");
                            strBody.Append("<br/>");
                            //strBody.Append("<a href = " + strquery + " > Click </a> to Recovery Password.");
                            //strBody.Append("<br/>");
                            //strBody.Append("<br/>");
                            //strBody.Append("Please note this link to set the password will expire in 24hrs. It is a one time link and cannot be reused.");
                            //strBody.Append("<br/>");
                            //strBody.Append("<br/>");
                            strBody.Append("Thanks,");
                            strBody.Append("<br/>");
                            strBody.Append(Literals.EmailSenderTeamName + ".");
                            strBody.Append("<br/>");
                            strBody.Append("</p>");
                            Mails mail = new Mails();
                            Task.Run(async () =>
                            {
                                await mail.SendMail(senderEmail, email, userData.vc_alias_name, strBody.ToString(), "Azimuth: Password Recovery Email");
                            });
                            status = true;
                            message = "Password reset link has been sent to Email, please reset password using the link.";
                        }
                        else
                        {
                            status = false;
                            message = "Email has not verified yet";
                        }
                    }

                }
                else
                {
                    status = false;
                    message = "User not exist with the Email.";
                }
            }
            catch (Exception ex)
            {
                status = false;
                message = ex.Message;
            }
            //returning data
            return new HelperMethods.Response<string>
            {
                Status = status,
                Message = message
            };
        }   
        //public Response<string> SendForgotPasswordEmail(string email, bool isNew)
        //{
        //    bool status = false;
        //    string message = string.Empty;
        //    try
        //    {
        //        var userData = _adminDB.tbl_ibw_users.Where(x => x.vc_email == email && x.bt_delete == false).FirstOrDefault();
        //        if (userData != null)
        //        {
        //            Random rnd = new Random(99999999);
        //            //string authToken = Cryptography.Encrypt(userData.intUserId.ToString());
        //            var authToken = Guid.NewGuid().ToString("n");

        //            if (isNew == true)
        //            {
        //                string strquery = Literals.ServiceWebUrl + "/#/set-password/" + authToken;
        //                userData.vc_auth_token = authToken;
        //                userData.dt_verificationlink_sent_on = DateTime.Now;
        //                _adminDB.SubmitChanges();
        //                StringBuilder strBody = new StringBuilder();
        //                strBody.Append("Hello " + userData.vc_alias_name + "," + "<br />");
        //                strBody.Append("<br/>");
        //                strBody.Append("<p>");
        //                strBody.Append("Thank you for registering with us. Please click below link to complete verification process and setup account password.");
        //                strBody.Append("<br/>");
        //                strBody.Append("<br/>");
        //                strBody.Append("<a href = " + strquery + " > Click  </a>  to Verify and Set Your Password.");
        //                strBody.Append("<br/>");
        //                strBody.Append("<br/>");
        //                strBody.Append("Please note this link to set the password will expire in 24hrs. It is a one-time link and cannot be reused.");
        //                strBody.Append("<br/>");
        //                strBody.Append("<br/>");
        //                strBody.Append("Regards,");
        //                strBody.Append("<br/>");
        //                strBody.Append(Literals.EmailSenderName);
        //                strBody.Append("<br/>");
        //                strBody.Append("</p>");
        //                Mails mail = new Mails();
        //                //mail.SendMail(userData.vcEmail, userData.vcFirstName + " " + userData.vcLastName, strquery, "Reset Password");
        //                Task.Run(async () =>
        //                {
        //                    await mail.SendMail(email, userData.vc_alias_name, strBody.ToString(), "New User Email Verification");
        //                });
        //                status = true;
        //                message = "Set Password Link has been sent to Email, Please Set Password using the link.";
        //            }
        //            else
        //            {
        //                if (userData.bt_verified == true)
        //                {
        //                    string strquery = Literals.ServiceWebUrl + "/#/reset-password/" + authToken;
        //                    userData.vc_auth_token = authToken;
        //                    userData.dt_verificationlink_sent_on = DateTime.Now;
        //                    _adminDB.SubmitChanges();
        //                    StringBuilder strBody = new StringBuilder();
        //                    strBody.Append("Hello " + userData.vc_alias_name + "," + "<br />");
        //                    strBody.Append("<br/>");
        //                    strBody.Append("<p>");
        //                    strBody.Append("Please click below link to recovery account password.");
        //                    strBody.Append("<br/>");
        //                    strBody.Append("<br/>");
        //                    strBody.Append("<a href = " + strquery + " > Click </a> to Recovery Password.");
        //                    strBody.Append("<br/>");
        //                    strBody.Append("<br/>");
        //                    strBody.Append("Please note this link to set the password will expire in 24hrs. It is a one time link and cannot be reused.");
        //                    strBody.Append("<br/>");
        //                    strBody.Append("<br/>");
        //                    strBody.Append("Regards,");
        //                    strBody.Append("<br/>");
        //                    strBody.Append(Literals.EmailSenderName);
        //                    strBody.Append("<br/>");
        //                    strBody.Append("</p>");
        //                    Mails mail = new Mails();
        //                    Task.Run(async () =>
        //                    {
        //                        await mail.SendMail(email, userData.vc_alias_name, strBody.ToString(), "Password Recovery Email");
        //                    });
        //                    status = true;
        //                    message = "Password reset link has been sent to Email, please reset password using the link.";
        //                }
        //                else
        //                {
        //                    status = false;
        //                    message = "Email has not verified yet";
        //                }
        //            }

        //        }
        //        else
        //        {
        //            status = false;
        //            message = "User not exist with the Email.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        status = false;
        //        message = ex.Message;
        //    }
        //    //returning data
        //    return new HelperMethods.Response<string>
        //    {
        //        Status = status,
        //        Message = message
        //    };
        //}

        //public Response<string> SendForgotPasswordEmail(string email)
        //{
        //    bool status = false;
        //    string message = string.Empty;
        //    try
        //    {
        //        var userData = _adminDB.tbl_ibw_users.Where(x => x.vc_email == email && x.bt_delete == false).FirstOrDefault();
        //        if (userData != null)
        //        {
        //            Random rnd = new Random(99999999);
        //            //string authToken = Cryptography.Encrypt(userData.intUserId.ToString());
        //            var authToken = Guid.NewGuid().ToString("n");
        //            string strquery = Literals.ServiceWebUrl + "/#/reset-password/" + authToken;
        //            userData.vc_auth_token = authToken;
        //            _adminDB.SubmitChanges();
        //            StringBuilder strBody = new StringBuilder();
        //            strBody.Append("Hello " + userData.vc_alias_name + "," + "<br />");
        //            strBody.Append("<br/>");
        //            strBody.Append("<p>");
        //            strBody.Append("Did you forget your password to sign into IBW Surveyors ? Let 's get you a new one. You have requested a new password for " + email);
        //            strBody.Append("<br/>");
        //            strBody.Append("<a href = " + strquery + " > Click here </a> to reset Password.");
        //            strBody.Append("<br/>");
        //            strBody.Append("<br/>");
        //            strBody.Append("Thanks,");
        //            strBody.Append("<br/>");
        //            strBody.Append(Literals.EmailSenderName);
        //            strBody.Append("<br/>");
        //            strBody.Append("</p>");

        //            Mails mail = new Mails();
        //            //mail.SendMail(userData.vcEmail, userData.vcFirstName + " " + userData.vcLastName, strquery, "Reset Password");
        //             Task.Run(async () => {
        //                 await mail.SendMail(email, userData.vc_alias_name, strBody.ToString(), "Fogot password");
        //            });
        //            status = true;
        //            message = "Password reset link has been sent to Email, please reset password using the link.";
        //        }
        //        else
        //        {
        //            status = false;
        //            message = "User not exist with the Email.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        status = false;
        //        message = ex.Message;
        //    }
        //    //returning data
        //    return new HelperMethods.Response<string>
        //    {
        //        Status = status,
        //        Message = message
        //    };
        //}
        public Response<long> ForgotPassword(string authToken)
        {
            try
            {
                Response<long> output = new Response<long>();
                var user = _adminDB.tbl_ibw_users.Where(x => x.vc_auth_token == authToken && x.bt_delete != true).FirstOrDefault();
                if (user != null && user.dt_verificationlink_sent_on.Value.AddDays(1) >= DateTime.Now)
                {
                    output = new Response<long> { Status = true, Message = "", Data = user.int_user_id };
                }
                else
                {
                    output = new Response<long> { Status = false, Message = "This link is no longer active, please contact administrator." };
                }
                return output;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public Response<string> UpdateProfile(long userId, string email, string userName, string aliasName, string phoneNumber)
        {
            Response<string> output = new Response<string>();
            List<tbl_ibw_user> users = new List<tbl_ibw_user>();
            tbl_ibw_user user = new tbl_ibw_user();
            user = _adminDB.tbl_ibw_users.Where(x => x.int_user_id == userId && x.bt_delete != true).FirstOrDefault();
            if (user != null)
            {
                users = _adminDB.tbl_ibw_users.Where(x => x.int_user_id != userId && x.vc_email == email && x.bt_delete != true).ToList();
                if (users.Count == 0)
                {
                    user.vc_email = email;
                    user.vc_user_name = userName;
                    user.vc_alias_name = aliasName;
                    user.vc_phone_number = phoneNumber;
                    _adminDB.SubmitChanges();
                    output = new Response<string> { Status = true, Message = "Profile has been updated successfully" };
                }
                else
                {
                    output = new Response<string> { Status = false, Message = "Email already exists with other user." };
                }

            }
            else
            {
                output = new Response<string> { Status = false, Message = "Failed to update Profile" };
            }
            return output;
        }

        public Response<long> UpdatePassword(string password, long userId)
        {
            Response<long> output = new Response<long>();
            var user = _adminDB.tbl_ibw_users.Where(x => x.int_user_id == userId && x.bt_delete != true).FirstOrDefault();
            if (user != null)
            {
                user.vc_password = Cryptography.Encrypt(password);
                user.bt_verified = true;
                user.bt_status = true;
                user.vc_auth_token = null;
                _adminDB.SubmitChanges();
                output = new Response<long> { Status = true, Message = "Password has been updated successfully", Data = user.int_user_id };
            }
            else
            {
                output = new Response<long> { Status = false, Message = "Failed to update password" };
            }
            return output;
        }

        public Response<long> ChangePassword(string newPassword, string password, long userId)
        {
            Response<long> output = new Response<long>();
            var user = _adminDB.tbl_ibw_users.Where(x => x.int_user_id == userId && x.bt_delete != true).FirstOrDefault();
            if (user != null)
            {
                if (user.vc_password == Cryptography.Encrypt(password))
                {
                    user.vc_password = Cryptography.Encrypt(newPassword);
                    _adminDB.SubmitChanges();
                    output = new Response<long> { Status = true, Message = "Password has been updated successfully", Data = user.int_user_id };
                }
                else
                {
                    output = new Response<long> { Status = false, Message = "Incorrect Current Password.", Data = user.int_user_id };
                }
            }
            else
            {
                output = new Response<long> { Status = false, Message = "Failed to update password" };
            }
            return output;
        }
        public void UserLogOut(long userId)
        {
            var user = _adminDB.tbl_ibw_users.Where(x => x.int_user_id == userId).SingleOrDefault();
            user.vc_auth_token = null;
            _adminDB.SubmitChanges();
        }
    }
}