using API_IBW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace API_IBW.Providers
{
    public class ExceptionHandler : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            string strUserId = string.Empty;
            Exception ex = context.Exception;
            long? userId = null;
            var uri = context.Request.RequestUri;
            var baseUrl = context.Request.RequestUri.GetLeftPart(UriPartial.Authority);
            
            var user = context.Request.Headers.SingleOrDefault(x => x.Key == "User");
            if (user.Key != null)
            {
                strUserId = Convert.ToString(user.Value.SingleOrDefault());
                userId = Convert.ToInt64(strUserId);
            }
            Logging.ErrorLog(userId, ex.Message + "Stack:" + ex.StackTrace, uri.LocalPath);

            //TO DO: handle exception and log it
            context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
    }
}