using API_IBW.DB_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace API_IBW.Providers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class BasicAuthorization: AuthorizationFilterAttribute
    {
        private const string _authorizedToken = "AuthorizedToken";
        private const string _user = "User";
        public override void OnAuthorization(HttpActionContext filterContext)
        {
            string authorizedToken = string.Empty;
            string userId = string.Empty;

            try
            {
                var headerToken = filterContext.Request.Headers.SingleOrDefault
                                      (x => x.Key == _authorizedToken);
                var user = filterContext.Request.Headers.SingleOrDefault
                                      (x => x.Key == _user);


                if (headerToken.Key != null && user.Key != null)
                {
                    authorizedToken = Convert.ToString(headerToken.Value.SingleOrDefault());
                    userId = Convert.ToString(user.Value.SingleOrDefault());
                    //userAgent = Convert.ToString(filterContext.Request.Headers.UserAgent);
                    if(!IsAuthorize(authorizedToken, Convert.ToInt64(userId)))
                        filterContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

                }
                else
                {
                    filterContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                }
            }

            catch (Exception ex)
            {
                filterContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            base.OnAuthorization(filterContext);
        }
        private bool IsAuthorize(string authorizedToken, long? intUserId)
        {
            AdminDataContext _adminDB = new AdminDataContext();
            bool isAuthorizedRequest = false;
            List<tbl_ibw_user> users = new List<tbl_ibw_user>();
            try
            {
                users = _adminDB.tbl_ibw_users.Where(x => x.vc_auth_token == authorizedToken).ToList();
                if (users.Count > 0)
                    isAuthorizedRequest = true;
                
            }
            catch (Exception ex)
            {
                //throwing exception details
                throw;
            }
            return isAuthorizedRequest;
        }

    }
}