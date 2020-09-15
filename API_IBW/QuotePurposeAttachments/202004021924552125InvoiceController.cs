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
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using static API_IBW.Models.FCMModels;
using Rotativa;
using System.Net.Http.Headers;
using System.IO;
using Newtonsoft.Json;

namespace API_IBW.Controllers
{
    //[BasicAuthorization]
    [RoutePrefix("api/invoices")]
    [ExceptionHandler]
    public class InvoiceController : ApiController
    {
        //HttpResponseMessage response;
        Invoice invoiceMethods;
        Admin adminMethods;

        public object JsonExtensions { get; private set; }

        public InvoiceController()
        {
            //creating required objects
            //response = new HttpResponseMessage();
            invoiceMethods = new Invoice();
            adminMethods = new Admin();
        }
        
        //author:Vamsi 
        //date: 09/03/2020
        //To get all quote related invoice details
        [Route("get-quote-related-invoice-details")]
        [HttpPost]
        public IHttpActionResult getQuoterelatedInvoiceDetails(long quoteId, [FromBody]SelectedActions selectedactions)
        {
            Response<InvSitescontactsQuoteDetails> output = new Response<InvSitescontactsQuoteDetails>();
            InvSitescontactsQuoteDetails InvSitesListcontactsQuoteDetails = new InvSitescontactsQuoteDetails();
            List<ClientContacts> contacts = new List<ClientContacts>();
            InvQuoteDetails quotedetails = new InvQuoteDetails();
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
                List<InvSites> data = invoiceMethods.GetQuoteInvoicesDetails(quoteId, selectedactions);
                if (selectedactions != null)
                {
                    quotedetails = invoiceMethods.GetQuoteDetailsbyId(quoteId);
                    if (quotedetails != null && quotedetails.int_client_id != null)
                        contacts = invoiceMethods.GetContactDetailsbyClientId(quotedetails.int_client_id);
                }
                if (data.Count > 0 || quotedetails != null || contacts != null)
                {
                    output.Status = true;
                    output.Message = "";
                    InvSitesListcontactsQuoteDetails.sites = data;
                    if (quotedetails != null)
                        InvSitesListcontactsQuoteDetails.quotedetails = quotedetails;
                    if (contacts != null && contacts.Count() > 0)
                        InvSitesListcontactsQuoteDetails.contacts = contacts;
                    output.Data = InvSitesListcontactsQuoteDetails;
                }
                else
                {
                    output.Status = false;
                    output.Message = "";
                }
                //return response= Request.CreateResponse(HttpStatusCode.OK, (JObject.FromObject(output)));
                return Ok(JObject.FromObject(output));

            }
            catch (Exception ex)
            {
                output.Status = true;
                output.Message = ex.Message;
                //return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(output));
                return InternalServerError(ex);
            }
        }
        //author:Vamsi 
        //date: 10/03/2020
        //To insert or update invoice and invoice items for a particular quote
        [Route("upsert-invoice-and-invoice-items")]
        [HttpPost]
        public IHttpActionResult UpsertInvoiceandInvoiceItems([FromBody]UpsertInvoiceParameters parameters)
        {
            Response<List<ActionScheduling>> output = new Response<List<ActionScheduling>>();
            List<InvoiceItems> InvoiceItems = new List<InvoiceItems>();
            output.Status = false;
            output.Message = "";
            long? result = null;
            bool isinvoicepercentvalid = true;
            long isactionsavailable = 0;
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
                if (parameters.quotedetails != null)
                {
                    string invoiceuniquenesscheckmessage = invoiceMethods.CheckInvoiceNumberUniquness(parameters.quotedetails.int_quote_id, parameters.quotedetails.auto_invoice_number, parameters.quotedetails.manual_invoice_number);
                    if (invoiceuniquenesscheckmessage == "" && parameters.quotedetails.invoice_total != null && parameters.quotedetails.invoice_total > 0 && !string.IsNullOrEmpty(parameters.quotedetails.description) && ((parameters.quotedetails.is_existing_client == true && parameters.quotedetails.int_contact_id != null) || parameters.quotedetails.is_existing_client == false))
                    {
                        foreach (var site in parameters.sitesdetails)
                        {
                            foreach (var sow in site.sows)
                            {
                                if (sow.feestructure == "Fixed Rate" && sow.tasks[0].actions[0].isselect == true)
                                {
                                    if (sow.tasks[0].actions[0].percentinvoiced > 0)
                                    {
                                        isactionsavailable++;
                                        InvoiceItems InvoiceItem = new InvoiceItems();
                                        InvoiceItem.int_action_log_hour_id = null;
                                        InvoiceItem.int_budget_sow_id = sow.tasks[0].actions[0].int_budget_sow_id;
                                        InvoiceItem.int_timesheet_id = null;
                                        InvoiceItem.int_expense_id = null;
                                        InvoiceItem.dec_invoiced_percent = sow.tasks[0].actions[0].percentinvoiced;
                                        InvoiceItem.dec_invoiced_total = sow.tasks[0].actions[0].invoiced;
                                        InvoiceItems.Add(InvoiceItem);
                                    }
                                    else
                                    {
                                        isinvoicepercentvalid = false;
                                        goto breakloops;
                                    }
                                }
                                else
                                {
                                    foreach (var task in sow.tasks)
                                    {
                                        foreach (var action in task.actions.Where(x => x.isselect == true))
                                        {
                                            if (action.percentinvoiced > 0)
                                            {
                                                isactionsavailable++;
                                                InvoiceItems InvoiceItem = new InvoiceItems();
                                                InvoiceItem.int_action_log_hour_id = action.int_action_log_hour_id;
                                                InvoiceItem.int_action_expense_id = action.int_action_expense_id;
                                                InvoiceItem.int_budget_sow_id = null;
                                                InvoiceItem.int_timesheet_id = action.int_timesheet_id;
                                                InvoiceItem.int_expense_id = action.int_expense_id;
                                                InvoiceItem.dec_invoiced_percent = action.percentinvoiced;
                                                InvoiceItem.dec_invoiced_total = action.invoiced;
                                                InvoiceItems.Add(InvoiceItem);
                                            }
                                            else
                                            {
                                                isinvoicepercentvalid = false;
                                                goto breakloops;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    breakloops:
                        if (isinvoicepercentvalid && isactionsavailable > 0)
                        {
                            result = invoiceMethods.UpsertInvoiceItems(parameters.quotedetails, InvoiceItems, headerUserId);
                            if (result > 0)
                            {
                                GenerateInvoicePDF(parameters.quotedetails.auto_invoice_number,"",result, headerUserId);
                                output.Status = true;
                                output.Message = "Invoice details saved successfully";
                            }
                            else
                            {
                                output.Message = "Operation failed";
                            }
                        }
                        else if (!isinvoicepercentvalid)
                        {
                            output.Status = false;
                            output.Message = "Percent invoiced value of selected items must be greater than 0";
                        }
                        else
                        {
                            output.Status = false;
                            output.Message = "Please select an item to proceed further";
                        }
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = invoiceuniquenesscheckmessage != "" ? invoiceuniquenesscheckmessage : (string.IsNullOrEmpty(parameters.quotedetails.description)) ? "Please enter description" : ((parameters.quotedetails.invoice_total == null || parameters.quotedetails.invoice_total <= 0) ? "Invoice total should be greater than 0" : "Please select contact");
                    }
                }
                else
                {
                    output.Message = "Operation failed";
                }
            }
            catch (Exception ex)
            {
                output.Message = ex.Message;
                output.Status = false;
                //output.Message = "Something went wrong, please contact administrator";
                return InternalServerError(ex);
            }
            return Ok(JObject.FromObject(output));
        }
        //author:Vamsi 
        //date: 23/03/2020
        //To generate latest invoice pdf
        public void GenerateInvoicePDF(string vc_invoice_number, string invoice_date, long? invoiceId, long? userId)
        {
            InvoiceViewController controller = new InvoiceViewController();
            RouteData route = new RouteData();
            route.Values.Add("action", "Invoice"); // ActionName
            route.Values.Add("controller", "InvoiceView"); // Controller Name
            System.Web.Mvc.ControllerContext newContext = new
            System.Web.Mvc.ControllerContext(new HttpContextWrapper(System.Web.HttpContext.Current), route, controller);
            controller.ControllerContext = newContext;
            invoice_date = string.IsNullOrEmpty(invoice_date) ? DateTime.Now.ToString("yyyyMMdd") : invoice_date.Replace("-","");
            var actionPDF = controller.getinvoicePDFdatainbytes(invoiceId);
            var UserDetails = adminMethods.GetAllIBWUsers().Where(x => x.int_user_id == userId).FirstOrDefault();
            string invoicefilename = UserDetails != null ? vc_invoice_number + "-" + UserDetails.vc_user_name + " " + UserDetails.vc_alias_name + "-" + invoice_date : vc_invoice_number + "-" + invoice_date;
            string FileId = GoogleDriveFilesRepository.UploadInvoiceFiletoGoogleDrivefolder("", invoicefilename, actionPDF);
            string invoiceaccesslink = GoogleDriveFilesRepository.GetSharableLink(FileId);
            invoiceMethods.UpdateInvoiceGoogleDriveDetails(invoiceId, FileId, invoiceaccesslink);
        }
        //author:Vamsi 
        //date: 09/03/2020
        //To get all invoices & their item details
        [Route("get-invoice-items-details")]
        [HttpGet]
        public IHttpActionResult getInvoiceItemsDetails(long quoteId)
        {
            Response<List<Invoices>> output = new Response<List<Invoices>>();
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
                List<Invoices> data = invoiceMethods.GetInvoiceItemsDetails(quoteId);
                output.Status = true;
                output.Message = "";
                output.Data = data;
                //return response= Request.CreateResponse(HttpStatusCode.OK, (JObject.FromObject(output)));
                return Ok(JObject.FromObject(output));

            }
            catch (Exception ex)
            {
                output.Status = true;
                output.Message = ex.Message;
                //return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(output));
                return InternalServerError(ex);
            }
        }
        //author:Vamsi 
        //date: 09/03/2020
        //To get all invoices & their item details
        [Route("get-contact-details-by-client-id")]
        [HttpGet]
        public IHttpActionResult getContactDetailsbyClientId(long clientId)
        {
            Response<List<ClientContacts>> output = new Response<List<ClientContacts>>();
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
                List<ClientContacts> contacts = invoiceMethods.GetContactDetailsbyClientId(clientId);
                if (contacts.Count > 0)
                {
                    output.Status = true;
                    output.Message = "";
                    output.Data = contacts;
                }
                else
                {
                    output.Status = false;
                    output.Message = "";
                }
                //return response= Request.CreateResponse(HttpStatusCode.OK, (JObject.FromObject(output)));
                return Ok(JObject.FromObject(output));

            }
            catch (Exception ex)
            {
                output.Status = true;
                output.Message = ex.Message;
                //return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(output));
                return InternalServerError(ex);
            }
        }
        //author:Vamsi 
        //date: 09/03/2020
        //To get all invoices & their item details
        [Route("get-invoice-terms-and-conditions")]
        [HttpGet]
        public IHttpActionResult getInvoiceTermsandConditions()
        {
            Response<InvoiceTermsandConditions> output = new Response<InvoiceTermsandConditions>();
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
                InvoiceTermsandConditions invoiceTandC = invoiceMethods.GetInvoiceTandC();
                output.Status = true;
                output.Message = "";
                output.Data = invoiceTandC;
                //return response= Request.CreateResponse(HttpStatusCode.OK, (JObject.FromObject(output)));
                return Ok(JObject.FromObject(output));

            }
            catch (Exception ex)
            {
                output.Status = true;
                output.Message = ex.Message;
                //return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(output));
                return InternalServerError(ex);
            }
        }
        //author:Vamsi 
        //date: 17/03/2020
        //To update invoice terms & conditions
        [Route("update-invoice-terms-and-conditions")]
        [HttpPost]
        public IHttpActionResult updateInvoiceTermsandConditions([FromBody]InvoiceTermsandConditions InvoiceTandC)
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
                bool isinvoiceTandCupdated = invoiceMethods.UpdateInvoiceTandC(InvoiceTandC.termsandconditions, headerUserId);
                if (isinvoiceTandCupdated)
                {
                    output.Status = true;
                    output.Message = "Terms and conditions updated successfully";
                    output.Data = "";
                }
                else
                {
                    output.Status = false;
                    output.Message = "Operation failed";
                }
                //return response= Request.CreateResponse(HttpStatusCode.OK, (JObject.FromObject(output)));
                return Ok(JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = true;
                output.Message = ex.Message;
                //return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(output));
                return InternalServerError(ex);
            }
        }
        //author:Vamsi 
        //date: 18/03/2020
        //To delete invoice details
        [Route("delete-invoice-by-id")]
        [HttpGet]
        public IHttpActionResult deleteInvoiceById(long? invoiceId)
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
                bool isinvoicedeleted = invoiceMethods.DeleteInvoiceDetails(invoiceId, headerUserId);
                if (isinvoicedeleted)
                {
                    output.Status = true;
                    output.Message = "Invoice deleted successfully";
                    output.Data = "";
                }
                else
                {
                    output.Status = false;
                    output.Message = "Operation failed";
                }
                //return response= Request.CreateResponse(HttpStatusCode.OK, (JObject.FromObject(output)));
                return Ok(JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = true;
                output.Message = ex.Message;
                //return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(output));
                return InternalServerError(ex);
            }
        }
        //author:Vamsi 
        //date: 20/03/2020
        //To generate invoice pdf
        [Route("generate-invoice-pdf")]
        [HttpGet]
        public HttpResponseMessage generateinvoicePdf(long? invoiceid)
        {
            InvoiceViewController controller = new InvoiceViewController();
            RouteData route = new RouteData();
            route.Values.Add("action", "Invoice"); // ActionName
            route.Values.Add("controller", "InvoiceView"); // Controller Name
            System.Web.Mvc.ControllerContext newContext = new
            System.Web.Mvc.ControllerContext(new HttpContextWrapper(System.Web.HttpContext.Current), route, controller);
            controller.ControllerContext = newContext;
            var actionPDF = controller.getinvoicePDFdatainbytes(invoiceid);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(actionPDF);// new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "InvNo-First Name Last Name-Uploaded Date-timestamp.PDF";
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            return response;
        }
        //author:Vamsi 
        //date: 22/03/2020
        //To save invoice payment details
        [Route("save-invoice-payment-details")]
        [HttpPost]
        public IHttpActionResult SaveInvoicePaymentDetails()
        {
            Response<InvoicepaymentDetails> output = new Response<InvoicepaymentDetails>();
            try
            {
                long? headerUserId = null;
                HttpPostedFile Attachment=null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                var httprequest = HttpContext.Current.Request;
                //var data = JObject.Parse(httprequest.Form["InvoicePaymentDetailsObject"]).ToObject<InvoicepaymentDetails>();
                InvoicepaymentDetails data = JsonConvert.DeserializeObject<InvoicepaymentDetails>(httprequest.Form["InvoicePaymentDetailsObject"]);
                //var s = JsonExtensions.FromDelimitedJson<InvoicepaymentDetails>(new StringReader(httprequest.Form["InvoicePaymentDetailsObject"])).ToList();
                if (data.PaymentTypeId!=null && data.Amount!=null && data.Amount>0 && !string.IsNullOrEmpty(data.Comments) && httprequest.Files.Count > 0)
                {
                    if (httprequest.Files.Count > 0)
                    {
                        for (var i = 0; i < httprequest.Files.Count; i++)
                        {
                            Attachment = httprequest.Files[i];
                        }
                    }
                    if (data != null)
                    {
                        data.UserId = headerUserId;
                        if (invoiceMethods.UpdateInvoicePaymentDetails(data.InvoiceId, data.PaymentTypeId, data.Amount, data.Comments, data.UserId, Attachment) > 0)
                        {
                            string InvoiceFileId = invoiceMethods.GetInvoiceDetails(data.InvoiceId).InvoiceFileId;
                            GoogleDriveFilesRepository.DeleteFile(InvoiceFileId);
                            GenerateInvoicePDF(data.vc_invoice_number, data.vc_invoice_date, data.InvoiceId, headerUserId);
                            output.Message = "Invoice payment details saved successfully";
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
                }
                else
                {
                    output.Message = data.PaymentTypeId!=null? (data.Amount!=null && data.PaymentTypeId>0)? httprequest.Files.Count > 0 ? "Please enter comments" : "Please select file" : "Please enter amount" : "Please select payment type";
                    output.Status = false;
                }
                return Ok(JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                return InternalServerError(ex);
            }
        }
        //author:Vamsi 
        //date: 22/03/2020
        //To get invoice details by invoice id
        [Route("get-invoice-details-by-invoice-id")]
        [HttpGet]
        public IHttpActionResult getInvoiceDetailsbyInvoiceId(long invoiceId)
        {
            Response<InvoicePaymentdetailsandPaymentHistory> output = new Response<InvoicePaymentdetailsandPaymentHistory>();
            InvoicePaymentdetailsandPaymentHistory InvoicePaymentdetailsandPaymentHistory = new InvoicePaymentdetailsandPaymentHistory();
            InvoicepaymentDetails InvoicepaymentDetails = new InvoicepaymentDetails();
            List<InvoicePaymentHistory> InvoicePaymentHistory = new List<InvoicePaymentHistory>();
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
                InvoicepaymentDetails = invoiceMethods.GetInvoiceDetails(invoiceId);
                InvoicePaymentHistory = invoiceMethods.GetInvoicePaymentDetails(invoiceId);
                output.Status = true;
                output.Message = "";
                InvoicePaymentdetailsandPaymentHistory.InvoicepaymentDetails = InvoicepaymentDetails;
                InvoicePaymentdetailsandPaymentHistory.InvoicePaymentHistory = InvoicePaymentHistory;
                output.Data = InvoicePaymentdetailsandPaymentHistory;
                //return response= Request.CreateResponse(HttpStatusCode.OK, (JObject.FromObject(output)));
                return Ok(JObject.FromObject(output));
            }
            catch (Exception ex)
            {
                output.Status = false;
                output.Message = ex.Message;
                //return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(output));
                return InternalServerError(ex);
            }
        }
    }
}
