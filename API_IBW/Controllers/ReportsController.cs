using API_IBW.Business_Classes;
using API_IBW.HelperMethods;
using API_IBW.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API_IBW.Controllers
{
    public class ReportsController : ApiController
    {
        HttpResponseMessage response;
        public ReportsMethods _reportsMethods;

        public ReportsController()
        {
            response = new HttpResponseMessage();
            _reportsMethods = new ReportsMethods();
        }

        #region Payroll Bi-Weekly

        //Author        : Siddhant Chawade
        //Date          : 17th Feb 2020
        //Description   : To get payroll bi weekly report
        [Route("api/get-payroll-biweekly-report")]
        [HttpGet]
        public HttpResponseMessage GetPayrollBiWeekly(long? managerId, long? UserId, DateTime? fromDate, DateTime? toDate, int isTimesheet)
        {
            Response<PayrollBiWeekly> output = new Response<PayrollBiWeekly>();
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
                output.Data = isTimesheet == 1 ? _reportsMethods.GetPayrollBiWeekly(managerId, UserId, fromDate, toDate) : _reportsMethods.GetExpensesBiWeekly(managerId, UserId, fromDate, toDate);

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
    }
}
