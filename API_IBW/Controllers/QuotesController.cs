using API_IBW.Business_Classes;
using API_IBW.DB_Models;
using API_IBW.HelperMethods;
using API_IBW.Models;
using API_IBW.Providers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace API_IBW.Controllers
{
    [ExceptionHandler]
    public class QuotesController : ApiController
    {
        HttpResponseMessage response;
        Admin adminMethods;
        Quotes _quotesMethods;
        private Login _login;
        //private long? headerUserId = null;
        public QuotesController()
        {
            //creating required objects
            response = new HttpResponseMessage();
            adminMethods = new Admin();
            _quotesMethods = new Quotes();
            _login = new Login();
        }
        #region addrfq
        [Route("api/get-all-sites")]
        [HttpGet]
        public HttpResponseMessage GetAllSites(bool isProject)
        {
            Response<List<GetAllSitesResult>> response = new Response<List<GetAllSitesResult>>();
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
                response.Data = _quotesMethods.GetAllSites(isProject);
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/add-rfq")]
        [HttpGet]
        public HttpResponseMessage AddRFQ(bool isProject)
        {
            Response<RFQ> response = new Response<RFQ>();
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
                response.Data = _quotesMethods.GetRFQData(isProject);
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/upsert-general-detail-quote")]
        [HttpPost]
        public HttpResponseMessage UpsertGeneralDetailsQuote([FromBody]Quote model)
        {
            Response<Quote> response = new Response<Quote>();
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
                if (!_quotesMethods.ProjectNumberVerification(model.legacyNumber))
                {
                    long? quoteId = _quotesMethods.UpsertGeneralDetailsQuotes(model);
                    if (quoteId != 0)
                    {
                        response.Status = true;
                        response.Message = "General details saved successfully.";
                        model.quoteId = quoteId;
                        response.Data = model;
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Failed to save general details.";
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = "Legacy number already exists";
                }



                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/upsert-address-quote")]
        [HttpPost]
        public HttpResponseMessage UpsertAddressQuote([FromBody]QuoteAddress model)
        {
            Response<QuoteAddress> response = new Response<QuoteAddress>();
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
                long? addressId = _quotesMethods.UpsertAddressQuotes(model);
                if (addressId != 0)
                {
                    response.Status = true;
                    response.Message = "Address details saved successfully.";
                    response.Data = model;
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to save address details.";
                }



                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/upsert-purpose")]
        [HttpPost]
        public HttpResponseMessage UpserPurpose([FromBody]Purpose purpose)
        {
            Response<long> response = new Response<long>();
            try
            {
                //string filePath = "";
                //string fileName = "";
                //var httpRequest = HttpContext.Current.Request;
                //if (httpRequest.Files.Count > 0)
                //{
                //    var postedFile = httpRequest.Files[0];
                //    string path = HttpContext.Current.Server.MapPath("~/QuotePurposeAttachments/");
                //    if (!Directory.Exists(path))
                //    {
                //        Directory.CreateDirectory(path);
                //    }
                //    fileName = postedFile.FileName;
                //    string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                //    filePath = path + timeStamp + postedFile.FileName;

                //    postedFile.SaveAs(filePath);
                //}
                //Purpose purpose = new Purpose();
                //purpose.quoteId = Convert.ToInt64(httpRequest.Form["quoteId"]);
                //purpose.purposeOfSurveyId = Convert.ToInt64(httpRequest.Form["purposeOfSurveyId"]);
                //purpose.details = httpRequest.Form["details"];
                //purpose.leadSourceId = Convert.ToInt64(httpRequest.Form["leadSourceId"]);
                //purpose.questionIds = httpRequest.Form["questionIds"];
                //purpose.createdBy = Convert.ToInt64(httpRequest.Form["createdBy"]);
                long? result = _quotesMethods.UpsertPurposeQuotes(Convert.ToInt64(purpose.quoteId), Convert.ToInt64(purpose.purposeOfSurveyId), purpose.details, "", "", Convert.ToInt64(purpose.leadSourceId), purpose.questionIds, Convert.ToInt64(purpose.createdBy));
                if (result != 0)
                {
                    response.Status = true;
                    response.Message = "Purpose details saved successfully.";
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to save purpose details.";
                }
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/upsert-purpose-file")]
        [HttpPost]
        public HttpResponseMessage UpserPurposeFile()
        {
            Response<long> response = new Response<long>();
            try
            {
                string filePath = "";
                string fileName = "";
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count > 0)
                {
                    var postedFile = httpRequest.Files[0];
                    string path = HttpContext.Current.Server.MapPath("~/QuotePurposeAttachments/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    fileName = postedFile.FileName;
                    string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                    filePath = path + timeStamp + postedFile.FileName;

                    postedFile.SaveAs(filePath);

                }
                string quoteId = httpRequest.Form["quoteId"];
                long? result = _quotesMethods.UpsertQuotePurposeAttachment(Convert.ToInt64(quoteId), fileName, filePath);
                if (result != 0)
                {
                    response.Status = true;
                    response.Message = "";
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to save purpose details.";
                }
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/save-rfq")]
        [HttpPost]
        public HttpResponseMessage SaveRFQ()
        {
            Response<RFQ> response = new Response<RFQ>();
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
                //string quoteId = model.Value<string>("quoteId");
                //string createdBy = model.Value<string>("createdBy");
                //string currentDateTime = model.Value<string>("currentDateTime");
                //string type = model.Value<string>("type");
                //string details = model.Value<string>("details");

                
                var httpRequest = HttpContext.Current.Request;
                

                string quoteId = httpRequest.Form["quoteId"];
                string createdBy = httpRequest.Form["createdBy"];
                string currentDateTime = httpRequest.Form["currentDateTime"];
                string type = httpRequest.Form["type"]; 
                string details = httpRequest.Form["details"]; 

                long? result = _quotesMethods.SaveRFQ(Convert.ToInt64(quoteId), Convert.ToInt64(createdBy), currentDateTime, type);
                if (result != 0)
                {

                    //_quotesMethods.CreateDefaultSiteAndSOW(Convert.ToInt64(quoteId), Convert.ToInt64(createdBy));
                    string quoteFolderGId = _quotesMethods.SaveFileInGDrive(Convert.ToInt64(quoteId), type);
                    _quotesMethods.UpdateQuoteGFolderId(Convert.ToInt64(quoteId), quoteFolderGId);

                    if (httpRequest.Files.Count > 0)
                    {
                        //string path = HttpContext.Current.Server.MapPath("~/QuotePurposeAttachments/");
                        //if (!Directory.Exists(path))
                        //{
                        //    Directory.CreateDirectory(path);
                        //}
                        for (int i = 0; i < httpRequest.Files.Count; i++)
                        {
                            //string filePath = "";
                            //string fileName = "";
                            var postedFile = httpRequest.Files[i];

                            //fileName = postedFile.FileName;
                            //string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                            //filePath = path + timeStamp + postedFile.FileName;

                            //postedFile.SaveAs(filePath);
                            _quotesMethods.SaveFileInGDriveFolder(Convert.ToInt64(quoteId), postedFile);
                        }
                    }

                    //bool IsDefault = type == "Project" ? false : true;
                    Sites site = new Sites()
                    {
                        siteId = null,
                        siteName = "Default",
                        quoteId = Convert.ToInt64(quoteId),
                        streetAddress = "",
                        cityName = "",
                        zipCode = "",
                        countryId = null,
                        stateId = null,
                        muncipalityId = null,
                        createdBy = Convert.ToInt64(createdBy),
                        isDefault = true
                    };
                    long? siteId = _quotesMethods.upsertSitesForQuotes(site);
                    Sow sow = new Sow() { sowId = null, sowName = "Default", sowTypeId = null, siteId = siteId, quoteId = Convert.ToInt64(quoteId), createdBy = Convert.ToInt64(createdBy), modifiedBy = Convert.ToInt64(createdBy), isApproved = false, isDefault = true };
                    long? SOWId = _quotesMethods.upsertSOWForQuotes(sow);
                    if (SOWId != null)
                    {
                        if (_quotesMethods.generateBudgetForQuote(Convert.ToInt64(quoteId), siteId, SOWId, Convert.ToInt64(createdBy), details))
                        {
                            response.Status = true;
                            response.Message = type == "Quote" ? "RFQ saved successfully" : "New project added successfully";
                        }
                        else
                        {
                            response.Status = false;
                            response.Message = "Budget Saving Failed.";
                        }
                    }

                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to save RFQ details.";
                }
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                response.Status = true;
                response.Message = ex.Message;
                return Request.CreateResponse(HttpStatusCode.InternalServerError, JObject.FromObject(response));
            }
        }
        [Route("api/rfq-status-change")]
        [HttpPost]
        public HttpResponseMessage RFQStstusChange([FromBody]JObject model)
        {
            Response<RFQ> response = new Response<RFQ>();
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
                string quoteId = model.Value<string>("int_quote_id");
                bool? status = Convert.ToBoolean(model.Value<string>("bt_status"));
                long? result = _quotesMethods.QuoteStatusChange(Convert.ToInt64(quoteId), status, headerUserId);
                if (result != 0)
                {
                    response.Status = true;
                    response.Message = "RFQ status changed successfully.";
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to change RFQ status details.";
                }
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/delete-rfq")]
        [HttpGet]
        public HttpResponseMessage DeleteRFQ(long? quoteId, string type)
        {
            Response<RFQ> response = new Response<RFQ>();
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
                //string quoteId = model.Value<string>("quoteId");
                //string createdBy = model.Value<string>("createdBy");
                long? result = _quotesMethods.DeleteQuote(quoteId, headerUserId);
                if (result != 0)
                {
                    response.Status = true;
                    if (type == "quote")
                        response.Message = "RFQ deleted successfully.";
                    else
                        response.Message = "Project deleted successfully.";
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to delete RFQ.";
                }
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/update-quote-potential")]
        [HttpPost]
        public HttpResponseMessage UpdateQuotePotential([FromBody]JObject model)
        {
            Response<RFQ> response = new Response<RFQ>();
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

                string quoteId = model.Value<string>("quoteId");
                string potentialId = model.Value<string>("potentialId");
                long? result = _quotesMethods.UpdateQuotePotential(Convert.ToInt64(quoteId), Convert.ToInt64(potentialId), headerUserId);
                if (result != 0)
                {
                    response.Status = true;
                    response.Message = "Quote potential update successfully.";
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to update quote potential.";
                }
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/get-quotes")]
        [HttpGet]
        public HttpResponseMessage GetQuotes(long? clientId)
        {
            Response<List<GetQuotesByIdResult>> response = new Response<List<GetQuotesByIdResult>>();
            try
            {
                //GoogleDriveFilesRepository.GetDriveFiles();
                //GoogleDriveFilesRepository.CreateFolder("Folder For Testing");
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                List<GetQuotesByIdResult> quotes = _quotesMethods.GetQuotes(null, clientId);
                response.Status = true;
                response.Message = "";
                response.Data = quotes;
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Route("api/get-quote-kanban-options")]
        [HttpGet]
        public HttpResponseMessage GetQuoteKanbanOptions(long? quoteKanbanId)
        {
            Response<List<GetQuoteKanbanOptionsResult>> response = new Response<List<GetQuoteKanbanOptionsResult>>();
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
                //string quoteId = model.Value<string>("quoteId");
                //string createdBy = model.Value<string>("createdBy");
                List<GetQuoteKanbanOptionsResult> result = _quotesMethods.GetQuoteKanbanOptions(quoteKanbanId);
                response.Status = true;
                response.Data = result;
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/update-quote-kanban-option")]
        [HttpPost]
        public HttpResponseMessage UpdateQuoteKanbanOption([FromBody]JObject model)
        {
            Response<RFQ> response = new Response<RFQ>();
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

                string optionId = model.Value<string>("optionId");
                string quoteKanbanId = model.Value<string>("quoteKanbanId");
                string option = model.Value<string>("option");
                if (_quotesMethods.GetQuoteKanbanOptions(Convert.ToInt64(quoteKanbanId)).Where(x => x.int_quote_kanban_option_id != Convert.ToInt64(optionId) && x.vc_option_name == option.Trim()).Count() <= 0)
                {
                    long? result = _quotesMethods.UpdateQuoteKanbanOption(Convert.ToInt64(optionId), Convert.ToInt64(quoteKanbanId), option, headerUserId);
                    if (result != 0)
                    {
                        response.Status = true;
                        response.Message = "Option saved successfully.";
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Failed to update quote kanban option.";
                    }
                }
                else {
                    response.Status = false;
                    response.Message = "Option already exists for this stage.";
                }
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/delete-quote-kanban-option")]
        [HttpGet]
        public HttpResponseMessage DeleteQuoteKanbanOption(long? optionId)
        {
            Response<RFQ> response = new Response<RFQ>();
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
                //string quoteId = model.Value<string>("quoteId");
                //string createdBy = model.Value<string>("createdBy");
                long? result = _quotesMethods.DeleteQuoteKanbanOption(optionId, headerUserId);
                if (result != 0)
                {
                    response.Status = true;
                    response.Message = "Option deleted successfully.";
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to delete RFQ.";
                }
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/update-quote-kanban-option-status")]
        [HttpPost]
        public HttpResponseMessage UpdateQuoteKanbanOptionStatus([FromBody]JObject model)
        {
            Response<RFQ> response = new Response<RFQ>();
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

                string optionId = model.Value<string>("optionId");
                bool status = model.Value<bool>("status");
                long? result = _quotesMethods.QuoteKanbanOptionStatusChange(Convert.ToInt64(optionId), Convert.ToBoolean(status), headerUserId);
                if (result != 0)
                {
                    response.Status = true;
                    response.Message = "Option status changed successfully.";
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to update quote kanban option.";
                }
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/update-quote-status-activity")]
        [HttpPost]
        public HttpResponseMessage UpdateQuoteStatusActivity([FromBody]JObject model)
        {
            Response<RFQ> response = new Response<RFQ>();
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

                string activityId = model.Value<string>("activityId");
                string quoteId = model.Value<string>("quoteId");
                string stageId = model.Value<string>("stageId");
                string stageName = model.Value<string>("stageName");
                string followUpDate = model.Value<string>("followUpDate");
                string currentDateTime = model.Value<string>("currentDateTime");
                string option = model.Value<string>("option");
                string remarks = model.Value<string>("remarks");
                string type = model.Value<string>("type");
                JArray jSOWs = model.Value<JArray>("sows");
                List<GetQuoteSOWsResult> sows = new List<GetQuoteSOWsResult>();
                if(jSOWs != null && jSOWs.Count >0 )
                    sows = jSOWs.ToObject<List<GetQuoteSOWsResult>>();
                long? optionId = null;
                if (option != null && option != "")
                    optionId = Convert.ToInt64(option);

                string projectDueDate = "";
                string projectSetupDueDate = "";
                if (stageName == "Awarded")
                {
                    projectDueDate = model.Value<string>("projectDueDate");
                    projectSetupDueDate = model.Value<string>("projectSetupDueDate");
                }
                    long? result = _quotesMethods.UpdateQuoteStatusActivity(Convert.ToInt64(activityId), Convert.ToInt64(quoteId), Convert.ToInt64(stageId), followUpDate, currentDateTime, optionId, remarks, projectDueDate, projectSetupDueDate, headerUserId, type);
                if (result != 0)
                {
                    if (stageName == "Awarded")
                    {
                        _quotesMethods.UpdateSOWApproveStatus(sows);
                        _quotesMethods.UpdateGDriveFolderNameQuoteToProject(Convert.ToInt64(quoteId));
                        //var manageActionParents = _quotesMethods.getManageActionParentIds(result);
                        //_quotesMethods.setUpGoogleDriveFileForEntireQuoteStructure(Convert.ToInt64(quoteId), manageActionParents.SiteID, manageActionParents.SowID, manageActionParents.BudgetTaskID, manageActionParents.ManageActionID);
                    }

                    response.Status = true;
                    if (type == "quote")
                        response.Message = "Quote status updated successfully.";
                    if (type == "project")
                        response.Message = "Project status updated successfully.";
                }
                else
                {
                    response.Status = false;
                    response.Message = "Failed to update quote status.";
                }
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/get-quote-status-activity")]
        [HttpGet]
        public HttpResponseMessage GetQuoteStatusActivity(long? activityId)
        {
            Response<GetQuoteStatusActivitiesResult> response = new Response<GetQuoteStatusActivitiesResult>();
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
                //string quoteId = model.Value<string>("quoteId");
                //string createdBy = model.Value<string>("createdBy");
                GetQuoteStatusActivitiesResult result = _quotesMethods.GetQuoteStatusActivity(activityId);
                response.Status = true;
                response.Data = result;
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/get-quote-status-activities")]
        [HttpGet]
        public HttpResponseMessage GetQuoteStatusActivitiesByQuoteId(long? quoteId)
        {
            Response<List<GetQuoteStatusActivitiesByQuoteIdResult>> response = new Response<List<GetQuoteStatusActivitiesByQuoteIdResult>>();
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
                //string quoteId = model.Value<string>("quoteId");
                //string createdBy = model.Value<string>("createdBy");
                List<GetQuoteStatusActivitiesByQuoteIdResult> result = _quotesMethods.GetQuoteStatusActivitiesByQuoteId(quoteId);
                response.Status = true;
                response.Data = result;
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/get-quote-sows")]
        [HttpGet]
        public HttpResponseMessage GetQuoteSOWs(long? quoteId)
        {
            Response<List<GetQuoteSOWsResult>> response = new Response<List<GetQuoteSOWsResult>>();
            try
            {
                List<GetQuoteSOWsResult> result = _quotesMethods.GetQuoteSOWs(quoteId);
                response.Status = true;
                response.Data = result;
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region Properties
        [Route("api/quote-properties-file-upload")]
        [HttpPost]
        public HttpResponseMessage QuotePropertiesFileUpload()
        {
            Response<long> response = new Response<long>();
            try
            {
                
                var httpRequest = HttpContext.Current.Request;
                string quoteId = httpRequest.Form["quoteId"];
                if (httpRequest.Files.Count > 0)
                {
                    string path = HttpContext.Current.Server.MapPath("~/QuotePurposeAttachments/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    for (int i = 0; i < httpRequest.Files.Count; i++)
                    {
                        //string filePath = "";
                        //string fileName = "";
                        var postedFile = httpRequest.Files[i];

                        //fileName = postedFile.FileName;
                        //string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                        //filePath = path + timeStamp + postedFile.FileName;

                        //postedFile.SaveAs(filePath);
                        _quotesMethods.SaveFileInGDriveFolder(Convert.ToInt64(quoteId), postedFile);
                    }

                }
                response.Status = true;
                response.Message = "";
                //string result = _quotesMethods.SaveFileInGDriveFolder(Convert.ToInt64(quoteId), filePath);
                //if (result != null)
                //{
                //    response.Status = true;
                //    response.Message = "";
                //}
                //else
                //{
                //    response.Status = false;
                //    response.Message = "";
                //}
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/get-quote-details")]
        [HttpGet]
        public HttpResponseMessage GetQuoteDetails(long? quoteId)
        {
            Response<QuoteDetails> response = new Response<QuoteDetails>();
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

                QuoteDetails quote = _quotesMethods.GetQuoteDetails(quoteId);
                if (quote != null && quote.Quote != null)
                {
                    response.Status = true;
                    response.Message = "";
                    response.Data = quote;
                }
                else {
                    response.Status = false;
                    response.Message = "";
                }
                
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Route("api/update-quote-details")]
        [HttpPost]
        public HttpResponseMessage UpdateQuoteDetails([FromBody]UpdateQuote modal)
        {
            Response<long> response = new Response<long>();
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
                bool isClientContactExists = false;
                if (modal.contactDetailsId == 2) {
                    isClientContactExists = _quotesMethods.IsClientContactExists(modal.clientId, modal.ContactId, modal.conatctEmail);
                }
                if (!isClientContactExists)
                {
                    if (!_quotesMethods.ProjectNumberVerification(modal.legacyNumber, modal.quoteId))
                    {
                        long? result = _quotesMethods.UpdateQuoteDetails(modal, headerUserId);
                        if (result != 0)
                        {
                            response.Status = true;

                            response.Message = modal.type == "quote" ? "Quote details saved successfully" : "Project details saved successfully";
                        }
                        else
                        {
                            response.Status = false;
                            response.Message = "Failed to save quote details.";
                        }
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Legacy number already exists.";
                    }
                }
                else {
                    response.Status = false;
                    response.Message = "Contact email already exists";
                }
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region Projects
        [Route("api/get-projects")]
        [HttpGet]
        public HttpResponseMessage GetProjects(long? clientId)
        {
            Response<List<GetProjectsByIdResult>> response = new Response<List<GetProjectsByIdResult>>();
            try
            {
                //GoogleDriveFilesRepository.GetDriveFiles();
                //GoogleDriveFilesRepository.CreateFolder("Folder For Testing");
                long? headerUserId = null;
                var re = Request;
                var headers = re.Headers;
                if (headers.Contains("User"))
                {
                    string strUserId = headers.GetValues("User").First();
                    headerUserId = Convert.ToInt64(strUserId);
                }
                List<GetProjectsByIdResult> quotes = _quotesMethods.GetProject(null, clientId);
                response.Status = true;
                response.Message = "";
                response.Data = quotes;
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(response));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Bharath

        #region Budget

        //author:Sreebharath
        //date: 17-10-2018
        //Get Budget Tree
        [Route("api/get-budget-tree")]
        [HttpGet]
        public HttpResponseMessage GetBudgetTree(long? quoteId)
        {
            Response<List<Sites>> output = new Response<List<Sites>>();
            try
            {
                var data = _quotesMethods.getAllSitesOfQuote(quoteId);
                if (data.Count > 0)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "";
                    output.Status = true;
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
        //date: 19-10-2018
        //Get Manage Tree
        [Route("api/get-manage-tree")]
        [HttpGet]
        public HttpResponseMessage GetManageTree(long? quoteId)
        {
            Response<List<Sites>> output = new Response<List<Sites>>();
            try
            {
                var data = _quotesMethods.getManageTreeSites(quoteId);
                if (data.Count > 0)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "";
                    output.Status = true;
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
        //date: 25-10-2018
        //Get Budget Data for SOW
        [Route("api/get-budget-data-sow")]
        [HttpGet]
        public HttpResponseMessage GetBudgetDataSOW(long? sowId)
        {
            Response<Budget> output = new Response<Budget>();
            try
            {
                var data = _quotesMethods.getBudgetData(sowId, false);
                if (data != null)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "";
                    output.Status = true;
                    output.Data = null;
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
        //date: 24-10-2018
        //Upsert Budget
        [Route("api/upsert-budget")]
        [HttpPost]
        public HttpResponseMessage upsertBudgetQuote(Budget model)
        {
            Response<List<Sites>> output = new Response<List<Sites>>();
            try
            {
                var data = _quotesMethods.upsertBudgetForSowQuote(model);
                if (data > 0)
                {
                    if (_quotesMethods.upsertExpenses(model.expensesData, model.sowId) == true)
                    {
                        output.Message = "Budget saved successfully";
                        output.Status = true;
                    }

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

        //author:Sreebharath
        //date: 28-10-2019
        //Get Expense Codes With Units
        [Route("api/get-expense-codes-with-units")]
        [HttpGet]
        public HttpResponseMessage getExpenseCodesWithUnits()
        {
            Response<List<getAllExpenseCodesWithUnitsResult>> output = new Response<List<getAllExpenseCodesWithUnitsResult>>();
            try
            {
                var data = _quotesMethods.getAllExpenseCodesWithUnits();
                if (data.Count > 0)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "No Expense Codes Found";
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



        //author:Sreebharath
        //date: 30-10-2019
        //Get All Saved Expenses
        [Route("api/get-all-saved-expenses")]
        [HttpGet]
        public HttpResponseMessage getAllSavedExpenses(long? sowId)
        {
            Response<List<expenses>> output = new Response<List<expenses>>();
            try
            {
                var data = _quotesMethods.getAllSavedExpenses(sowId);
                if (data.Count > 0)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "";
                    output.Status = true;
                    output.Data = null;
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

        #region Sites

        //author:Sreebharath
        //date: 04-02-2018
        //Get SOW Reference Number
        [Route("api/get-site-reference-number")]
        [HttpGet]
        public HttpResponseMessage getSiteReferenceNumber(long? quoteId)
        {
            Response<string> output = new Response<string>();
            try
            {
                string referenceNumber = _quotesMethods.GetSiteSerialNumber(quoteId);
                output.Message = "";
                output.Status = true;
                output.Data = referenceNumber;
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
        //date: 16/10/2019
        //To Insert and Update the Sites in Quotes
        [Route("api/upsert-sites-quotes")]
        [HttpPost]
        public HttpResponseMessage UpsertSitesQuotes([FromBody]Sites model)
        {
            Response<long?> output = new Response<long?>();
            try
            {
                if (model.siteId == null || model.siteId == 0)
                {
                    Logging.EventLog(model.createdBy, "Attempt to add new site in the Quotes");
                    if (_quotesMethods.siteNameValidation(model.quoteId, model.siteId, model.siteName) == true)
                    {
                        long? siteId = _quotesMethods.upsertSitesForQuotes(model);
                        if (siteId != 0 && siteId != null)
                        {
                            output.Status = true;
                            output.Data = siteId;
                            output.Message = "New site added successfully";
                            Logging.EventLog(model.createdBy, "New site has been added successfully");
                        }
                        else
                        {
                            output.Status = false;
                            output.Message = "Something Went Wrong";
                        }
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "Site name already exists";
                    }

                }
                else
                {
                    Logging.EventLog(model.modifiedBy, "Attempt to update Site details");
                    if (_quotesMethods.siteNameValidation(model.quoteId, model.siteId, model.siteName) == true)
                    {
                        long? siteID = _quotesMethods.upsertSitesForQuotes(model);
                        if (siteID != null || siteID != 0)
                        {
                            output.Status = true;
                            output.Data = model.siteId;
                            output.Message = "Site details updated successfully";
                            Logging.EventLog(model.modifiedBy, "Client details have been updated successfully");
                        }
                        else
                        {
                            output.Status = false;
                            output.Message = "Something Went Wrong";
                        }
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "Site name already exists";
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
        //date: 31-10-2019
        //Get Budget Amount for the site
        [Route("api/get-budget-amount-site")]
        [HttpGet]
        public HttpResponseMessage getBudgetAmountSite(long siteId)
        {
            Response<Sites> output = new Response<Sites>();
            try
            {
                var data = _quotesMethods.getTotalBudgetOfSite(siteId);
                if (data != null)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "Something Went Wrong";
                    output.Status = true;
                    output.Data = null;
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
        //date: 18-10-2018
        //Delete Site in Quotes
        [Route("api/delete-site-quote")]
        [HttpPost]
        public HttpResponseMessage deleteSiteInQuote(Sites model)
        {
            Response<List<Sites>> output = new Response<List<Sites>>();
            try
            {
                if (_quotesMethods.getAllSOWOfQuote(model.quoteId, model.siteId).Count > 0)
                {
                    output.Message = "You can’t delete this site, until you delete all SOW’s of this site";
                    output.Status = false;
                }
                else
                {
                    long? data = _quotesMethods.deleteSiteInQuotes(model);
                    if (data > 0)
                    {
                        output.Message = "Site deleted successfully";
                        output.Status = true;
                    }
                    else
                    {
                        output.Message = "Something Went Wrong";
                        output.Status = false;
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



        //author:Sreebharath
        //date: 17-10-2018
        //Get All Sites in Quotes
        [Route("api/get-all-sites-in-quotes")]
        [HttpGet]
        public HttpResponseMessage GetAllSitesInQuotes(long? quoteId)
        {
            Response<List<Sites>> output = new Response<List<Sites>>();
            try
            {
                var data = _quotesMethods.getAllSitesQuote(quoteId);
                if (data.Count > 0)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "";
                    output.Status = true;
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



        #endregion

        #region SOW
        //author:Sreebharath
        //date: 06-02-2020
        //To update the status of SOW
        [Route("api/update-sow-status")]
        [HttpPost]
        public HttpResponseMessage updateSOWStatus(Sow model)
        {
            Response<string> output = new Response<string>();
            try
            {
                long? updatedResult = _quotesMethods.UpdateQuoteStatusActivity(null, model.quoteId, model.sowStatus, null, null, model.statusOptionId, model.remarks,"","", model.createdBy, "sow", model.sowId);
                if (updatedResult > 0)
                {
                    output.Message = "SOW status updated successfully";
                    output.Status = true;
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

        //author:Sreebharath
        //date: 04-02-2018
        //Get SOW Reference Number
        [Route("api/get-sow-reference-number")]
        [HttpGet]
        public HttpResponseMessage getSOWReferenceNumber(long? quoteId, long? siteId)
        {
            Response<string> output = new Response<string>();
            try
            {
                string referenceNumber = _quotesMethods.GetSOWSerialNumber(quoteId, siteId);
                output.Message = "";
                output.Status = true;
                output.Data = referenceNumber;
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
        //date: 16/10/2019
        //To Insert and Update the SOW in Quotes
        [Route("api/upsert-sow-quotes")]
        [HttpPost]
        public HttpResponseMessage UpsertSOWQuotes([FromBody]Sow model)
        {
            Response<long?> output = new Response<long?>();
            try
            {
                if (model.sowId == null || model.sowId == 0)
                {
                    Logging.EventLog(model.createdBy, "Attempt to add new SOW in the Quotes");
                    if (_quotesMethods.sowNameValidation(model.quoteId, model.siteId, model.sowId, model.sowName) == true)
                    {
                        long? sowID = _quotesMethods.upsertSOWForQuotes(model);
                        if (sowID != 0 && sowID != null)
                        {
                            output.Status = true;
                            output.Data = sowID;
                            output.Message = "New SOW added successfully";
                            Logging.EventLog(model.createdBy, "New SOW has been added successfully");
                        }
                        else
                        {
                            output.Status = false;
                            output.Message = "Something Went Wrong";
                        }
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "SOW already exists in this site";
                    }

                }
                else
                {
                    Logging.EventLog(model.modifiedBy, "Attempt to update SOW details");
                    if (_quotesMethods.sowNameValidation(model.quoteId, model.siteId, model.sowId, model.sowName) == true)
                    {
                        long? sowId = _quotesMethods.upsertSOWForQuotes(model);
                        if (sowId != null || sowId != 0)
                        {
                            output.Status = true;
                            output.Data = model.sowId;
                            output.Message = "SOW details updated successfully";
                            Logging.EventLog(model.modifiedBy, "Client details have been updated successfully");
                        }
                        else
                        {
                            output.Status = false;
                            output.Message = "Something Went Wrong";
                        }
                    }
                    else
                    {
                        output.Status = false;
                        output.Message = "SOW already exists in this site";
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
        //date: 17-10-2018
        //Delete Sow in Quotes
        [Route("api/delete-sow-quote")]
        [HttpPost]
        public HttpResponseMessage deleteSowInQuote(Sow model)
        {
            Response<List<Sites>> output = new Response<List<Sites>>();
            try
            {
                long? data = _quotesMethods.deleteSowInQuotes(model);
                if (data > 0)
                {
                    output.Message = "SOW deleted successfully";
                    output.Status = true;
                }
                else
                {
                    output.Message = "Something Went Wrong";
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




        #endregion

        #endregion

        #region Manage
        //author:Sreebharath
        //date: 19-11-2019
        //Get Manage Data for SOW
        [Route("api/get-sites-and-sows-budget")]
        [HttpGet]
        public HttpResponseMessage getAllSitesNSowsOfBudget(long? quoteId)
        {
            Response<IEnumerable<ActionsCreate>> output = new Response<IEnumerable<ActionsCreate>>();
            try
            {
                var data = _quotesMethods.getAllSiteNSowsOfBudget(quoteId);
                if (data != null)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "";
                    output.Status = true;
                    output.Data = null;
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
        //date: 24-03-2020
        //Get Saved Tasks of Budget SOW
        [Route("api/get-saved-tasks-budget-sow")]
        [HttpGet]
        public HttpResponseMessage getSavedTaskBudgetSow(long? budgetSowId)
        {
            Response<IEnumerable<TaskBudget>> output = new Response<IEnumerable<TaskBudget>>();
            try
            {
                var data = _quotesMethods.getSavedTasksInBudget(budgetSowId);
                if (data != null)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "";
                    output.Status = true;
                    output.Data = null;
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
        //date: 19-11-2019
        //Get Manage Data for SOW
        [Route("api/get-sites-and-sows-manageaction")]
        [HttpGet]
        public HttpResponseMessage getSitesAndSowsManageAction(long? quoteId)
        {
            Response<IEnumerable<ActionsCreate>> output = new Response<IEnumerable<ActionsCreate>>();
            try
            {
                var data = _quotesMethods.getAllSitesAndSowOfManageAction(quoteId);
                if (data != null)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "";
                    output.Status = true;
                    output.Data = null;
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
        //date: 28-02-2020
        //Get Saved Tasks of manage SOW
        [Route("api/get-saved-tasks-mage-sow")]
        [HttpGet]
        public HttpResponseMessage getSavedTasksOfManageSow(long? manageSowId)
        {
            Response<IEnumerable<TaskManage>> output = new Response<IEnumerable<TaskManage>>();
            try
            {
                var data = _quotesMethods.getSavedTasksInManage(manageSowId, true);
                if (data != null)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "";
                    output.Status = true;
                    output.Data = null;
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
        //date: 28-02-2020
        //Insert  action in manage globally
        [Route("api/insert-action-manage-globally")]
        [HttpPost]
        public HttpResponseMessage insertActionInManageGlobally()
        {
            Response<long> output = new Response<long>();
            try
            {
                var httprequest = HttpContext.Current.Request;
                var manageModel = JObject.Parse(httprequest.Form["managaAction"]).ToObject<ActionsCreate>();
                bool isFileExists = false;
                if (httprequest.Files.Count > 0)
                {
                    for (var i = 0; i < httprequest.Files.Count; i++)
                    {
                        manageModel.googleDriveFile = httprequest.Files[i];
                        isFileExists = true;
                    }
                }

                var data = _quotesMethods.insertActionInManageGlobally(manageModel, isFileExists);
                if (data > 0)
                {

                    output.Message = "Action saved successfully";
                    output.Status = true;

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
        //author:Sreebharath
        //date: 19-11-2019
        //Get Manage Data for SOW
        [Route("api/get-manange-data-sow")]
        [HttpGet]
        public HttpResponseMessage getManageDataSow(long? sowId)
        {
            Response<Budget> output = new Response<Budget>();
            try
            {
                var data = _quotesMethods.getBudgetData(sowId, true);
                if (data != null)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "";
                    output.Status = true;
                    output.Data = null;
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
        //date: 24-10-2018
        //Upsert Budget

        [Route("api/upsert-manage")]
        [HttpPost]
        public HttpResponseMessage upsertManageQuote()
        {
            Response<List<Sites>> output = new Response<List<Sites>>();
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
                var manageModel = JObject.Parse(httprequest.Form["finalObjectManage"]).ToObject<Manage>();
                //var manageModel = JObject.Parse(httprequest.Form["finalObjectManage"]).ToObject<Manage>();
                if (httprequest.Files.Count > 0)
                {
                    //for (var i = 0; i < httprequest.Files.Count; i++)
                    //{
                    // actionDetails actionDetailsModel = JsonConvert.DeserializeObject<actionDetails>(HttpUtility.UrlDecode(httprequest.Files.Keys[i]));

                    foreach (TaskManage taskItem in manageModel.taskData)
                    {
                        foreach (ActionsManage actionItem in taskItem.actionData)
                        {
                            if (actionItem.actionFileUniqueId != null && actionItem.actionFileUniqueId != "")
                            {
                                actionItem.googleDriveFile = httprequest.Files[actionItem.actionFileUniqueId];
                                //  manageModel.taskData[actionDetailsModel.taskIndex].actionData[actionDetailsModel.actionIndex].googleDriveFile = httprequest.Files[i];
                            }
                        }
                    }
                    //}
                }
                var data = _quotesMethods.upsertManageOfSow(manageModel, headerUserId);
                if (data > 0)
                {
                    //if (_quotesMethods.upsertExpenses(model.expensesData, model.sowId) == true)
                    //{
                    output.Message = "Manage saved successfully";
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

        //author:Sreebharath
        //date: 29-11-2019
        //Get Manage Data for SOW
        [Route("api/get-saved-manage-data-sow")]
        [HttpGet]
        public HttpResponseMessage GetManageDataSOW(long? sowId)
        {
            Response<Manage> output = new Response<Manage>();
            try
            {
                var data = _quotesMethods.getSavedManageData(sowId);
                if (data != null)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "";
                    output.Status = true;
                    output.Data = null;
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
        //date: 22-12-2019
        //Get expenses of action
        [Route("api/get-expenses-of-action")]
        [HttpGet]
        public HttpResponseMessage getExpensesOfAction(long? sowId)
        {
            Response<List<ExpensesManage>> output = new Response<List<ExpensesManage>>();
            try
            {
                var data = _quotesMethods.getExpensesInManage(sowId);
                if (data != null)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "";
                    output.Status = true;
                    output.Data = null;
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


        #endregion

        #region Log Hours
        //author:Sreebharath
        //date: 19-11-2019
        //Get all logged hours of the quote
        [Route("api/get-all-log-hours-by-quote")]
        [HttpGet]
        public HttpResponseMessage getAllLogHoursOfQuote(long quoteId)
        {
            Response<List<LogHours>> output = new Response<List<LogHours>>();
            try
            {
                var data = _quotesMethods.getTimeSheetsOfQuote(quoteId);
                if (data != null)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "";
                    output.Status = true;
                    output.Data = null;
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
        //date: 19-11-2019
        //Get all the logged expenese of the quote
        [Route("api/get-all-log-expenses-by-quote")]
        [HttpGet]
        public HttpResponseMessage getAllLoggedExpensesOfQuote(long quoteId)
        {
            Response<List<ActionExpenses>> output = new Response<List<ActionExpenses>>();
            try
            {
                var data = _quotesMethods.getExpensesOfQuote(quoteId);
                if (data != null)
                {
                    output.Message = "";
                    output.Data = data;
                    output.Status = true;
                }
                else
                {
                    output.Message = "";
                    output.Status = true;
                    output.Data = null;
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


        #endregion
        #endregion
    }
}
