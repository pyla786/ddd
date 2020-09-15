using API_IBW.DB_Models;
using API_IBW.HelperMethods;
using API_IBW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace API_IBW.Business_Classes
{
    public class Quotes
    {
        private AdminDataContext _adminDB;
        private QuotesDataContext _quotesDB;
        private QuotesExtensionDataContext _quotesExtensionDB;
        private Admin _adminMethods;
        private SchedulingDataContext _schedulingDB;
        private static string mColumnLetters = "zabcdefghijklmnopqrstuvwxyz";

        public Quotes()
        {
            _adminDB = new AdminDataContext();
            _quotesDB = new QuotesDataContext();
            _quotesExtensionDB = new QuotesExtensionDataContext();
            _adminMethods = new Admin();
            _schedulingDB = new SchedulingDataContext();
        }
        #region add rfq and grid
        public List<GetAllSitesResult> GetAllSites(bool isProject)
        {
            return _quotesDB.GetAllSites(isProject).ToList();
        }

        public RFQ GetRFQData(bool isProject)
        {
            RFQ rfq = new RFQ();
            rfq.QuoteNumber = GetQuoteSerial(isProject);
            rfq.QuoteReceivedDate = DateTime.Now.ToString("yyyy-MM-dd");
            rfq.ProjectTypes = _adminMethods.GetCommonMasterData().Where(x => x.CommonMasterDataCategory == "Project Types" && x.IsActive == true).OrderBy(x => x.CommonMasterDataName).ToList();
            rfq.Clients = _adminMethods.GetClients().OrderBy(x => x.clientName).ToList();
            rfq.ProjectManagers = _adminDB.sp_getAllIBWUsers().Where(x => x.vc_role_name == "Project Manager" && x.bt_status == true).OrderBy(x => x.vc_user_name).ToList();
            rfq.AccountManagers = _adminDB.sp_getAllIBWUsers().Where(x => x.vc_role_name == "Account Manager" && x.bt_status == true).OrderBy(x => x.vc_user_name).ToList();

            return rfq;
        }
        public string GetQuoteSerial(bool isProject)
        {
            string serialNum = "";
            List<GetQuotesResult> quotes = new List<GetQuotesResult>();
            quotes = _quotesDB.GetQuotes().ToList();
            if (quotes.Count > 0)
            {
                string latestSerialNum = quotes.Select(x => x.vc_quote_number).FirstOrDefault();
                string[] arrayLatestSerialNum = latestSerialNum.Split('-');
                int incrementedNum = Convert.ToInt32(arrayLatestSerialNum[1]) + 1;
                while(quotes.Where(x=> x.vc_legacy_number == "R" + "-" + incrementedNum.ToString("D" + 6) ||
                x.vc_legacy_number == "r" + "-" + incrementedNum.ToString("D" + 6) ||
                x.vc_legacy_number ==  "A" + "-" + incrementedNum.ToString("D" + 6) || 
                x.vc_legacy_number == "a" + "-" + incrementedNum.ToString("D" + 6)).Count() > 0)
                {
                    ++incrementedNum;
                }


                serialNum = "R" + "-" + incrementedNum.ToString("D" + 6);
            }
            else
            {
                serialNum = Literals.FirstQuoteNumber;  //"Q-000001";
            }
            if (isProject)
                serialNum = serialNum.Replace('R', 'A');
            return serialNum;
        }
        public bool ProjectNumberVerification(string legacyNumber)
        {
            bool isExists = false;
            if (legacyNumber != null && legacyNumber.Trim() != "")
            {
                List<GetAllQuotesResult> quotes = new List<GetAllQuotesResult>();
                quotes = _schedulingDB.GetAllQuotes().ToList();
                if (quotes.Count > 0)
                {
                    if (quotes.Where(x => x.vc_quote_number.ToLower() == legacyNumber.Trim().ToLower() || (x.vc_legacy_number != null && x.vc_legacy_number.ToLower() == legacyNumber.Trim().ToLower())).Count() > 0)
                        isExists = true;
                }
            }
            return isExists;
        }
        public bool ProjectNumberVerification(string legacyNumber, long? quoteId)
        {
            bool isExists = false;
            if (legacyNumber != null && legacyNumber.Trim() != "")
            {
                List<GetAllQuotesResult> quotes = new List<GetAllQuotesResult>();
                quotes = _schedulingDB.GetAllQuotes().ToList();
                if (quotes.Count > 0)
                {
                    if (quotes.Where(x => (x.int_quote_id != quoteId) && (x.vc_quote_number == legacyNumber.Trim() || x.vc_legacy_number == legacyNumber.Trim())).Count() > 0)
                        isExists = true;
                }
            }
            return isExists;
        }
        public long? UpsertGeneralDetailsQuotes(Quote quote)
        {
            long? result = 0;
            quote.legacyNumber = quote.legacyNumber != null ? quote.legacyNumber.Trim() : quote.legacyNumber;
            _quotesDB.UpsertGenerealDetailsQuote(quote.quoteId, quote.contactTypeId, quote.quoteNumber,
                quote.CommonMasterDataId, quote.clientId, quote.contactId, quote.projectManagerId,
                quote.accountManagerId, quote.receivedDate, quote.dueDate, quote.quotePreparationDueDate, quote.legacyNumber, quote.isProject,
                quote.createdBy, ref result);
            return result;
        }
        public long? UpsertAddressQuotes(QuoteAddress address)
        {
            long? result = 0;
            _quotesDB.UpsertAddressQuote(address.quoteId, address.address, address.city,
                address.postal, address.countryId, address.stateId, address.municipalityId, address.legalDescription,
                address.createdBy, ref result);
            return result;
        }
        public long? UpsertPurposeQuotes(long? quoteId, long? purposeOfSurveyId, string details, string fileName, string filePath, long? leadSourceId, string questionIds, long? createdBy)
        {
            if (questionIds != "")
                questionIds = questionIds + "#";
            long? result = 0;
            if (leadSourceId == 0)
                leadSourceId = null;
            _quotesDB.UpsertPurposeQuote(quoteId, purposeOfSurveyId, details, fileName, filePath, leadSourceId, questionIds, createdBy, ref result);
            return result;
        }
        public long? UpsertQuotePurposeAttachment(long? quoteId, string fileName, string filePath)
        {
            long? result = 0;
            _quotesDB.UpsertQuotePurposeAttachment(quoteId, fileName, filePath, ref result);
            return result;
        }
        public string SaveFileInGDrive(long? quoteId, string type)
        {
            try
            {
                string quoteFolderGId = null;
                List<GetAllQuotesResult> quotes = new List<GetAllQuotesResult>();
                quotes = _schedulingDB.GetAllQuotes().Where(x => x.int_quote_id == quoteId).ToList();
                var quote = quotes.FirstOrDefault();
                string parentFolderId = Literals.GDriveQuoteFolderId; // "1AZ_fcwJNl7KMAYRntSh-sqnPzL5p3cWi";
                if (type == "Project")
                {
                    parentFolderId = Literals.GDriveProjectgFolderId; //"1kNR-chP1w36qR859wc6lQB06PUJSYCM_";
                }
                quoteFolderGId = GoogleDriveFilesRepository.CreateFolder(quote.QuoteRefNum, parentFolderId);
                if (quoteFolderGId != null)
                {
                    //string siteParentId = GoogleDriveFilesRepository.CreateFolder("Default Site", quoteFolderGId);
                    //GoogleDriveFilesRepository.CreateFolder("Default SOW", siteParentId);
                    if (quote.vc_purpose_file_path != null && quote.vc_purpose_file_path != "")
                    {
                        GoogleDriveFilesRepository.FileUploadInFolder(quoteFolderGId, quote.vc_purpose_file_path);
                    }
                }
                return quoteFolderGId;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public string SaveFileInGDriveFolder(long? quoteId, string filePath)
        {
            try
            {
                string quoteFolderGId = null;
                List<GetQuotesByIdResult> quotes = new List<GetQuotesByIdResult>();
                quotes = GetQuotes(quoteId, null);
                var quote = quotes.FirstOrDefault();
                quoteFolderGId = quote.vc_GDrive_file_id;
                if (quoteFolderGId != null)
                {
                    GoogleDriveFilesRepository.FileUploadInFolder(quoteFolderGId, filePath);
                }
                return quoteFolderGId;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public string SaveFileInGDriveFolder(long? quoteId, HttpPostedFile attachment)
        {
            try
            {
                string quoteFolderGId = null;
                List<GetQuoteDetailsByIdResult> quotes = new List<GetQuoteDetailsByIdResult>();
                quotes = _quotesExtensionDB.GetQuoteDetailsById(quoteId).ToList();
                var quote = quotes.FirstOrDefault();
                quoteFolderGId = quote.GDriveFileId;
                if (quoteFolderGId != null)
                {
                    //GoogleDriveFilesRepository.FileUploadInFolder(quoteFolderGId, filePath);
                    string GoogleFileId = GoogleDriveFilesRepository.FileUploadInFolderDirect(quoteFolderGId, attachment, attachment.FileName, attachment.ContentType);
                }
                return quoteFolderGId;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void UpdateQuoteGFolderId(long? quoteId, string folderId)
        {
            long? result = 0;
            _quotesDB.UpdateQuoteGFolderId(quoteId, folderId, ref result);
        }
        public void CreateDefaultSiteAndSOW(long? quoteId, long? userId)
        {
            long? result = 0;
            _quotesDB.DefaultSiteAndSOW(quoteId, userId, ref result);
        }
        public long? SaveRFQ(long? quoteId, long? updatedBy, string currentDateTime, string type)
        {
            try
            {
                long? result = 0;
                string refNum = GetQuoteSerial(type == "Project" ? true : false);
                _quotesExtensionDB.UpdateQuoteStatus(quoteId, true, null, null, true, currentDateTime, currentDateTime, refNum, updatedBy, type, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public long? QuoteStatusChange(long? quoteId, bool? status, long? updatedBy)
        {
            long? result = 0;
            _quotesExtensionDB.UpdateQuoteStatus(quoteId, null, true, null, status, "", "", "", updatedBy, null, ref result);
            return result;
        }
        public long? DeleteQuote(long? quoteId, long? updatedBy)
        {
            List<GetQuotesByIdResult> quotes = new List<GetQuotesByIdResult>();
            quotes = _quotesExtensionDB.GetQuotesById(quoteId,null).ToList();
            if (quotes.Count > 0)
            {
                GoogleDriveFilesRepository.DeleteFile(quotes.FirstOrDefault().vc_GDrive_file_id);
            }
            long? result = 0;
            _quotesExtensionDB.UpdateQuoteStatus(quoteId, null, null, true, true, "", "", "", updatedBy, null, ref result);
            return result;
        }
        public List<GetQuotesByIdResult> GetQuotes(long? quoteId, long? clientId)
        {
            List<GetQuotesByIdResult> quotes = new List<GetQuotesByIdResult>();
            quotes = _quotesExtensionDB.GetQuotesById(quoteId, clientId).ToList();
            return quotes;
        }
        public long? UpdateQuotePotential(long? quoteId, long? potentilaId, long? updatedBy)
        {
            long? result = 0;
            _quotesDB.UpdateQuotePotential(quoteId, potentilaId, updatedBy, ref result);
            return result;
        }
        public List<GetQuoteKanbanOptionsResult> GetQuoteKanbanOptions(long? quoteKanbanId)
        {
            List<GetQuoteKanbanOptionsResult> quoteKanbanOptions = new List<GetQuoteKanbanOptionsResult>();
            quoteKanbanOptions = _quotesDB.GetQuoteKanbanOptions(quoteKanbanId).ToList();
            return quoteKanbanOptions;
        }
        public long? UpdateQuoteKanbanOption(long? optionId, long? kanbanId, string option, long? updatedBy)
        {
            long? result = 0;
            _quotesDB.UpsertQuoteKanbanOption(optionId, kanbanId, option, updatedBy, ref result);
            return result;
        }
        public long? QuoteKanbanOptionStatusChange(long? optionId, bool? status, long? updatedBy)
        {
            long? result = 0;
            _quotesDB.QuoteKanbanOptionStatusChange(optionId, status, updatedBy, ref result);
            return result;
        }
        public long? DeleteQuoteKanbanOption(long? optionId, long? updatedBy)
        {
            long? result = 0;
            _quotesDB.DeleteQuoteKanbanOption(optionId, updatedBy, ref result);
            return result;
        }
        public long? UpdateQuoteStatusActivity(long? activityId, long? quoteId, long? stageId, string followUpDate, string currentDateTime, long? optionId, string remarks, string projectDueDate, string projectSetupDueDate, long? updatedBy, string type, long? sowId = null)
        {
            long? result = 0;
            _quotesExtensionDB.UpdateQuoteStatusActivity(activityId, quoteId, stageId, followUpDate, currentDateTime, optionId, remarks, updatedBy, sowId, type, projectDueDate, projectSetupDueDate, ref result);
            return result;
        }
        public void UpdateSOWApproveStatus(List<GetQuoteSOWsResult> sows)
        {
            long? result = 0;
            foreach (var item in sows)
                _quotesDB.UpdateSOWApproveStatus(item.int_sow_id, item.bt_isApproved, ref result);
        }
        public GetQuoteStatusActivitiesResult GetQuoteStatusActivity(long? activityId)
        {
            GetQuoteStatusActivitiesResult QuoteStatusActivity = new GetQuoteStatusActivitiesResult();
            QuoteStatusActivity = _quotesDB.GetQuoteStatusActivities(activityId).FirstOrDefault();
            return QuoteStatusActivity;
        }
        public List<GetQuoteStatusActivitiesByQuoteIdResult> GetQuoteStatusActivitiesByQuoteId(long? quoteId)
        {
            List<GetQuoteStatusActivitiesByQuoteIdResult> QuoteStatusActivities = new List<GetQuoteStatusActivitiesByQuoteIdResult>();
            QuoteStatusActivities = _quotesDB.GetQuoteStatusActivitiesByQuoteId(quoteId).ToList();
            QuoteStatusActivities.ForEach(x => x.strCreatedOn = getDateTimeString(x.strCreatedOn));
            return QuoteStatusActivities;
        }
        public List<GetQuoteSOWsResult> GetQuoteSOWs(long? quoteId)
        {
            return _quotesDB.GetQuoteSOWs(quoteId).ToList();
        }
        public string getDateTimeString(string dateTimeString)
        {
            try
            {
                if (dateTimeString != "")
                {
                    string date = dateTimeString.Split(' ')[0];
                    string time = dateTimeString.Split(' ')[1];
                    string hours = "";
                    string MeridiemType = "";
                    hours = time.Split(':')[0];
                    if (Convert.ToInt16(hours) > 12)
                    {
                        hours = (Convert.ToInt16(hours) - 12).ToString();
                        MeridiemType = "PM";
                    }
                    else
                    {
                        MeridiemType = "AM";
                    }
                    time = hours + ':' + time.Split(':')[1] + " " + MeridiemType;
                    dateTimeString = date + ',' + " " + time;
                }


                return dateTimeString;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
        #region properties 
        public QuoteDetails GetQuoteDetails(long? quoteId)
        {
            QuoteDetails quoteDetails = new QuoteDetails();
            quoteDetails.Quote = _quotesExtensionDB.GetQuoteDetailsById(quoteId).FirstOrDefault();
            quoteDetails.Questionnaire = _quotesDB.GetQuoteQuestions(quoteId).ToList();
            return quoteDetails;
        }
        public bool IsClientContactExists(long? clientId, long? contactId, string contactEmail)
        {
            bool isExists = true;
            long count = _adminMethods.GetContacts().Where(x => x.clientID == clientId && x.contactEmail == contactEmail && x.contactID != contactId).Count();
            if (count <= 0)
                isExists = false;
            return isExists;
        }
        public long? UpdateQuoteDetails(UpdateQuote modal, long? userId)
        {
            long? result = 0;
            modal.quostionId = modal.quostionId + "#";
            _quotesExtensionDB.UpdateQuoteDetails(modal.quoteId, modal.contactDetailsId, modal.contactDetailsName,
                modal.projectTypeId, modal.clientId, modal.projectManagerId, modal.accountManagerId, modal.strDueDate, modal.legacyNumber,
                modal.strQuotedDate, modal.ContactId, modal.contactJobTitle, modal.ContactName, modal.conatctEmail, modal.contactPhone,
                modal.contactStreetAddress, modal.contactCity, modal.contactPostalCode, modal.contactCountryId, modal.contactStateId,
                modal.contactMunicipalityId, modal.projectStreetAddress, modal.projectCity, modal.projectPostalCode, modal.projectCountryId,
                modal.projectStateId, modal.projectMunicipalityId, modal.purposeOfSurveyId, modal.purposeDetails, modal.legalDescription,modal.gotoLink, modal.leadSourceId, modal.quostionId,
                userId, ref result);
            return result;
        }
        #endregion
        #region Projects
        public List<GetProjectsByIdResult> GetProject(long? quoteId, long? clientId)
        {
            List<GetProjectsByIdResult> projects = new List<GetProjectsByIdResult>();
            projects = _quotesExtensionDB.GetProjectsById(quoteId, clientId).ToList();
            return projects;
        }
        public string UpdateGDriveFolderNameQuoteToProject(long? quoteId)
        {
            try
            {
                string quoteFolderGId = null;
                List<GetQuoteDetailsByIdResult> quotes = new List<GetQuoteDetailsByIdResult>();
                quotes = _quotesExtensionDB.GetQuoteDetailsById(quoteId).ToList();
                var quote = quotes.FirstOrDefault();
                quoteFolderGId = quote.GDriveFileId;
                if (quoteFolderGId != null)
                {
                    //string projectNumber = quote.quoteNumber.Replace('R', 'A');
                    GoogleDriveFilesRepository.UpdateFolderName(quote.quoteNumber, quoteFolderGId);
                    GoogleDriveFilesRepository.MoveFiles(quoteFolderGId, Literals.GDriveProjectgFolderId);
                }
                return quoteFolderGId;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Bharath

        #region Budget
        public bool generateBudgetForQuote(long? QuoteId, long? SiteId, long? SOWId, long? userID, string details)
        {
            List<JobCodes> jobCodesList = _adminMethods.GetJobCodes();

            //long? budgetID = 0;
            //_quotesDB.upsertBudgetOfSOW(budgetData.budgetId, budgetData.sowId, budgetData.totalQuotedCostSOW, budgetData.totalBudgetCostSOW, budgetData.totalBudgetHoursSOW, budgetData.totalExpensesCost,
            //       budgetData.isSubmitted, budgetData.createdBy, budgetData.modifiedBy, ref budgetID);

            List<TaskBudget> taskList = _adminMethods.GetTasks().Select(x => new TaskBudget
            {
                taskBudgetId = null,
                budgetId = null,
                taskId = x.TaskId,
                taskName = x.TaskName,
                totalBudgetCostSOW = 0,
                totalBudgetHoursSOW = 0,
                totalQuotedCostSOW = 0,
                Data = x.jobCodeDetails.Select(y => new JobCodeBudget
                {
                    jobCodeBudgetId = null,
                    taskBudgetId = null,
                    jobCodeId = y.JobCodeId,
                    plannedHours = 0,
                    rate = jobCodesList.Where(l => l.JobCodeId == y.JobCodeId).Select(j => j.ChargeoutRate).FirstOrDefault(),
                    total = 0,
                    notes = y.Notes,
                    isUserAdded = false,
                    isDeleted = false
                }).ToList()
            }).ToList();

            List<expenses> expensesList = getAllExpenseCodesWithUnits().Where(x => x.bt_isDefault == true && x.bt_status == true).Select(ex => new expenses
            {
                expenseBudgetId = null,
                expenseBudgetCustomId = null,
                expenseCodeId = ex.int_expense_code_master_id,
                expenseCodeName = ex.vc_expense_code,
                expenseUnit = ex.expenseUnit,
                expenseRate = ex.dec_rate,
                quantity = 0,
                total = 0,
                isDeleted = false,
                createdBy = userID
            }).ToList();

            Budget budgetData = new Budget()
            {
                quoteId = QuoteId,
                siteId = SiteId,
                sowId = SOWId,
                budgetId = null,
                isDefault = true,
                totalBudgetCostSOW = 0,
                totalBudgetHoursSOW = 0,
                totalQuotedCostSOW = 0,
                totalExpensesCost = 0,
                createdBy = userID,
                taskandJobCodesData = taskList,
                manageNotes = details
            };
            if (upsertBudgetForSowQuote(budgetData) > 0)
            {
                return upsertExpenses(expensesList, SOWId);
            }
            else
            {
                return false;
            }
        }


        public List<Sites> getAllSitesOfQuote(long? quoteId)
        {
            try
            {
                var data = _quotesExtensionDB.getAllSitesOfQuote(quoteId).OrderByDescending(x => x.int_site_id).ThenBy(y => y.vc_site_name).ToList();
                List<Sites> AllSites = new List<Sites>();
                foreach (var item in data)
                {
                    Sites eachSitedata = new Sites();
                    eachSitedata.siteId = item.int_site_id;
                    eachSitedata.siteName = item.vc_site_name;
                    eachSitedata.label = (item.vc_site_number != null ? item.vc_site_number : "") + " " + item.vc_site_name;
                    eachSitedata.quoteId = item.int_quote_id;
                    eachSitedata.quoteNumber = item.QuoteNumber;
                    eachSitedata.iconClass = "fa fa-map-marker cs-map-icon";
                    eachSitedata.streetAddress = item.vc_site_street_address;
                    eachSitedata.stateId = item.int_state_id;
                    eachSitedata.countryId = item.int_county_id;
                    eachSitedata.selectable = true;
                    eachSitedata.siteSerialNumber = item.vc_site_number;
                    eachSitedata.isSite = true;
                    eachSitedata.isDefault = item.bt_isDefault;
                    eachSitedata.cityName = item.vc_city;
                    eachSitedata.zipCode = item.vc_zip_or_postal;
                    eachSitedata.muncipalityId = item.int_municipality_id;
                    eachSitedata.googleDriveFolderId = item.vc_google_drive_folder_id;
                    eachSitedata.addressId = item.int_address_id;
                    eachSitedata.children = getAllSOWOfQuote(eachSitedata.quoteId, eachSitedata.siteId);
                    AllSites.Add(eachSitedata);
                }
                return AllSites;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<Sow> getAllSOWOfQuote(long? quoteId, long? siteId)
        {
            try
            {
                var data = _quotesExtensionDB.getAllSowOfQuoteandSite(quoteId, siteId).OrderByDescending(x => x.int_sow_id).ThenBy(x => x.vc_sow_name).ToList();
                List<Sow> AllSOWs = new List<Sow>();
                foreach (var item in data)
                {
                    Sow eachSowdata = new Sow();
                    eachSowdata.sowId = item.int_sow_id;
                    eachSowdata.sowName = item.vc_sow_name;
                    eachSowdata.label = (item.vc_sow_number != null ? item.vc_sow_number : "") + " " + item.vc_sow_name;
                    eachSowdata.isSite = false;
                    eachSowdata.sowTypeId = item.int_sow_type_id;
                    eachSowdata.siteId = item.int_site_id;
                    eachSowdata.siteName = item.vc_site_name;
                    eachSowdata.quoteId = item.int_quote_id;
                    eachSowdata.googleDriveFolderId = item.vc_google_drive_folder_id;
                    eachSowdata.isApproved = item.bt_isApproved;
                    eachSowdata.sowNumber = item.vc_sow_number;
                    eachSowdata.isSavedInBudget = _quotesDB.getBudgetDetailsOfSOW(item.int_sow_id).ToList().Count > 0 ? true : false;
                    eachSowdata.isDefault = item.bt_isDefault;
                    eachSowdata.sowStatus = item.int_sow_status_id;
                    eachSowdata.sowInvoiceType = item.int_sow_invoice_type_id;
                    eachSowdata.sowDescription = item.vc_sow_description;
                    eachSowdata.projectTypeName = item.vc_project_type_name;
                    eachSowdata.projectStatusName = item.vc_stage_name;
                    eachSowdata.feeStructureName = item.vc_invoice_type_name;
                    eachSowdata.iconClass = "fa fa-briefcase";
                    eachSowdata.iconStyle = _adminMethods.GetKanbanStages().Where(x => x.ConfigureKanbanId == item.int_sow_status_id).Select(y => y.ColorCode).FirstOrDefault();
                    eachSowdata.styleClass = eachSowdata.isApproved == true ? "" : "awardedClass";
                    AllSOWs.Add(eachSowdata);
                }
                return AllSOWs;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public bool upsertExpenses(List<expenses> model, long? sowId)
        {
            try
            {
                long? result = null;
                if (model.Count > 0)
                {
                    foreach (var item in model)
                    {
                        _quotesDB.upsertExpensesBudget(item.expenseBudgetId, item.expenseBudgetCustomId, sowId, item.expenseCodeId, item.expenseCodeName,
                            item.expenseUnit, item.expenseRate, item.quantity, item.total, item.isDeleted, item.createdBy, item.modifiedBy, ref result);
                    }
                    if (result > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //Author : Sreebharath 
        //Date : 28-10-2019
        //Get All Expense Codes With unit and Limit Type
        public List<getAllExpenseCodesWithUnitsResult> getAllExpenseCodesWithUnits()
        {
            try
            {
                return _quotesDB.getAllExpenseCodesWithUnits().ToList(); ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Sreebharath 
        //Date : 31-10-2019
        //Get the Entire Budget for the Site
        public Sites getTotalBudgetOfSite(long siteId)
        {
            try
            {
                Sites siteData = new Sites();
                var data = _quotesDB.getTotalBudgetForSite(siteId).ToList();
                siteData.totalBudgetAmount = data.Sum(x => x.dec_budget_amount);
                siteData.totalQuotedAmount = data.Sum(x => x.dec_quoted_amount);
                siteData.totalExpensesAmount = data.Sum(x => x.dec_expenses_amount);
                siteData.totalBudgetHours = data.Sum(x => x.dec_budget_hrs);
                return siteData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Sreebharath 
        //Date : 28-10-2019
        //Get All the Saved Expenses
        public List<expenses> getAllSavedExpenses(long? sowId)
        {
            try
            {
                var data = _quotesDB.getAllSavedExpenses(sowId).ToList();
                List<expenses> expensesList = new List<expenses>();
                foreach (var item in data)
                {
                    expenses eachExpense = new expenses();
                    eachExpense.expenseBudgetCustomId = item.int_ibw_expenses_budget_custom_id;
                    eachExpense.expenseBudgetId = item.int_expenses_budget_id;
                    eachExpense.expenseCodeId = item.int_expense_id;
                    eachExpense.expenseCodeName = item.vc_expense_code;
                    eachExpense.expenseRate = item.dec_expense_rate;
                    eachExpense.quantity = item.int_quantity;
                    eachExpense.total = item.dec_total;
                    eachExpense.expenseUnit = item.vc_limit_type_name;
                    expensesList.Add(eachExpense);
                }
                return expensesList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Sreebharath 
        //Date : 24-10-2019
        //Tpo upsert budget for Sow
        public long? upsertBudgetForSowQuote(Budget model)
        {
            try
            {
                long? result = 0;
                long? jobCodeResult = 0;
                _quotesDB.upsertBudgetOfSOW(model.budgetId, model.sowId, model.totalQuotedCostSOW, model.totalBudgetCostSOW, model.totalBudgetHoursSOW, model.totalExpensesCost,
                    model.isSubmitted, model.createdBy, model.modifiedBy, ref result);
                if (result > 0)
                {
                    if (model.taskandJobCodesData.Count > 0)
                    {
                        foreach (var taskItem in model.taskandJobCodesData)
                        {
                            long? taskResult = 0;
                            if (taskItem.taskBudgetId == null)
                            {

                                if (setUpGoogleDriveFileForEntireQuoteStructure(model.quoteId, model.siteId, model.sowId) != null)
                                {
                                    string sowGoogeDriveFolderId = getAllSOWOfQuote(model.quoteId, model.siteId).Where(x => x.siteId == model.siteId && x.sowId == model.sowId).Select(y => y.googleDriveFolderId).FirstOrDefault();
                                    string taskGoogleDriveId = GoogleDriveFilesRepository.CreateFolder(taskItem.taskName, sowGoogeDriveFolderId);
                                    _quotesDB.upsertTasksinSOWBudget(taskItem.taskBudgetId, result, taskItem.taskId, taskItem.taskName, taskItem.totalQuotedCostSOW,
                                    taskItem.totalBudgetCostSOW, taskItem.totalBudgetHoursSOW, taskGoogleDriveId, ref taskResult);
                                }

                            }
                            else
                            {
                                if (setUpGoogleDriveFileForEntireQuoteStructure(model.quoteId, model.siteId, model.sowId, taskItem.taskBudgetId) != null)
                                {
                                    _quotesDB.upsertTasksinSOWBudget(taskItem.taskBudgetId, result, taskItem.taskId, taskItem.taskName, taskItem.totalQuotedCostSOW,
                                    taskItem.totalBudgetCostSOW, taskItem.totalBudgetHoursSOW, null, ref taskResult);
                                }
                            }

                            if (taskResult > 0)
                                {
                                if (taskItem.Data.Count > 0)
                                {
                                    foreach (var jobCodeItem in taskItem.Data)
                                    {
                                        _quotesDB.upsertJobCodesInTaskBudget(jobCodeItem.jobCodeBudgetId, taskResult, jobCodeItem.jobCodeId, jobCodeItem.plannedHours, jobCodeItem.total,
                                            jobCodeItem.rate, jobCodeItem.isUserAdded, jobCodeItem.notes, jobCodeItem.isDeleted, ref jobCodeResult);
                                    }
                                }
                                else
                                {
                                    jobCodeResult = taskResult;
                                }
                            }
                        }
                    }
                }
                if (jobCodeResult > 0)
                {
                    if (model.isDefault && model.budgetId == null)
                    {
                        long? defaultActionResult = 0;
                        _quotesExtensionDB.automateActionForDefaultSOW(model.quoteId, model.sowId, result, model.manageNotes, model.createdBy, ref defaultActionResult);
                        if (defaultActionResult > 0)
                        {
                            var taskData = _quotesDB.getTaskDetailsOfManageAction(defaultActionResult).FirstOrDefault();
                            if (setUpGoogleDriveFileForEntireQuoteStructure(model.quoteId, model.siteId, model.sowId, taskData.int_ibw_budget_task_id, defaultActionResult) != null)
                            {
                                //string actionFileName = DateTime.Now.ToString("yyyyMMdd") + "-" + taskData.vc_custom_action_name + "-" + DateTime.Now.ToString("HH:mm");
                                //string manageactionFolderId =  GoogleDriveFilesRepository.CreateFolder(actionFileName, taskData.task_google_drive_folder_id);
                                //_quotesDB.tbl_ibw_manage_actions.Single(action => action.int_ibw_manage_action_id == defaultActionResult).vc_google_drive_file_id = manageactionFolderId;
                                return jobCodeResult;

                            }
                            //_quotesDB.SubmitChanges();
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    return jobCodeResult;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //This Method is used to get the budget data in the budget as well as in Manage
        public Budget getBudgetData(long? sowId, bool isManage)
        {
            var item = _quotesDB.getBudgetDetailsOfSOW(sowId).FirstOrDefault();
            if (item != null)
            {
                Budget budget = new Budget();
                budget.budgetId = item.int_budget_sow_id;
                budget.sowId = item.int_sow_id;
                budget.totalQuotedCostSOW = item.dec_quoted_amount;
                budget.totalBudgetCostSOW = item.dec_budget_amount;
                budget.totalExpensesCost = item.dec_expenses_amount;
                budget.totalBudgetHoursSOW = item.dec_budget_hrs;
                budget.isSubmitted = item.bt_isSubmitted;
                budget.taskandJobCodesData = getTasksinSOWBudget(budget.budgetId, isManage);
                return budget;
            }
            else
            {
                return null;
            }
        }

        public List<Sites> getManageTreeSites(long? quoteId)
        {
            try
            {
                var data = _quotesExtensionDB.getSavedBudgetInManage(quoteId).ToList();
                var distintSiteIds = data.GroupBy(x => x.int_site_id).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.int_site_id).ThenBy(y => y.vc_site_name).ToList();
                List<Sites> AllSites = new List<Sites>();
                foreach (var item in distintSiteIds)
                {
                    Sites eachSitedata = new Sites();
                    eachSitedata.siteId = item.int_site_id;
                    eachSitedata.siteName = item.vc_site_name;
                    eachSitedata.label = (item.vc_site_number != null ? item.vc_site_number : "") + " " + item.vc_site_name;
                    eachSitedata.quoteId = item.int_quote_id;
                    eachSitedata.iconClass = "fa fa-map-marker cs-map-icon cs-blue";
                    eachSitedata.isSite = true;
                    eachSitedata.selectable = false;
                    eachSitedata.children = getSowsOfSitesInManage(eachSitedata.quoteId, eachSitedata.siteId);
                    AllSites.Add(eachSitedata);
                }
                return AllSites;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Sow> getSowsOfSitesInManage(long? quoteId, long? siteId)
        {
            try
            {
                var data = _quotesExtensionDB.getSavedBudgetInManage(quoteId).Where(x => x.int_site_id == siteId).OrderByDescending(x => x.int_sow_id).ThenBy(x => x.vc_sow_name).ToList();
                List<Sow> AllSOWs = new List<Sow>();
                foreach (var item in data)
                {
                    Sow eachSowdata = new Sow();
                    eachSowdata.sowId = item.int_sow_id;
                    eachSowdata.sowName = item.vc_sow_name;
                    eachSowdata.label = (item.vc_sow_number != null ? item.vc_sow_number : "") + " " + item.vc_sow_name;
                    eachSowdata.isSite = false;
                    eachSowdata.siteId = item.int_site_id;
                    eachSowdata.siteName = item.vc_site_name;
                    eachSowdata.quoteId = item.int_quote_id;
                    eachSowdata.isApproved = item.bt_isApproved;
                    eachSowdata.sowDescription = item.vc_sow_description;
                    eachSowdata.projectTypeName = item.vc_project_type_name;
                    eachSowdata.projectStatusName = item.vc_stage_name;
                    eachSowdata.feeStructureName = item.vc_invoice_type_name;
                    eachSowdata.iconClass = "fa fa-briefcase";
                    eachSowdata.iconStyle = _adminMethods.GetKanbanStages().Where(x => x.ConfigureKanbanId == item.int_sow_status_id).Select(y => y.ColorCode).FirstOrDefault();
                    eachSowdata.styleClass = eachSowdata.isApproved == true ? "" : "awardedClass";
                    AllSOWs.Add(eachSowdata);
                }
                return AllSOWs;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Sites> getAllSitesQuote(long? quoteId)
        {
            try
            {
                var data = _quotesExtensionDB.getAllSitesOfQuote(quoteId).ToList();

                // var data = _quotesDB.getAllSitesinQuotes().ToList();
                List<Sites> AllSites = new List<Sites>();
                foreach (var item in data)
                {
                    Sites eachSitedata = new Sites();
                    eachSitedata.siteId = item.int_site_id;
                    eachSitedata.siteName = item.vc_site_name;
                    eachSitedata.label = item.vc_site_name;
                    eachSitedata.quoteId = item.int_quote_id;
                    eachSitedata.addressId = item.int_address_id;
                    eachSitedata.children = getAllSOWOfQuote(eachSitedata.quoteId, eachSitedata.siteId);
                    AllSites.Add(eachSitedata);
                }
                return AllSites;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<TaskBudget> getTasksinSOWBudget(long? budgetSOWId, bool isManage)
        {
            List<TaskBudget> tasksList = new List<TaskBudget>();
            var data = _quotesDB.getTasksInBudgetSow(budgetSOWId).ToList();
            foreach (var item in data)
            {
                TaskBudget taskItem = new TaskBudget();
                taskItem.taskBudgetId = item.int_task_in_sow_id;
                taskItem.budgetId = item.int_ibw_budget_sow_id;
                taskItem.taskId = item.int_task_id;
                taskItem.taskName = item.vc_task_name;
                taskItem.totalBudgetHoursSOW = item.dec_budget_hrs;
                taskItem.totalBudgetCostSOW = item.dec_budget_amount;
                taskItem.totalQuotedCostSOW = item.dec_quoted_amount;
                if (isManage == false)
                {
                    taskItem.Data = getJoCodesInTasks(taskItem.taskBudgetId);
                }
                tasksList.Add(taskItem);
            }
            return tasksList;
        }

        public List<JobCodeBudget> getJoCodesInTasks(long? taskInSowId)
        {
            List<JobCodeBudget> jobCodeList = new List<JobCodeBudget>();
            var data = _quotesDB.getJobCodeDataInTasks(taskInSowId).ToList();
            foreach (var item in data)
            {
                JobCodeBudget jobCodeItem = new JobCodeBudget();
                jobCodeItem.jobCodeBudgetId = item.int_jobCode_in_task_id;
                jobCodeItem.taskBudgetId = item.int_task_in_sow_id;
                jobCodeItem.jobCodeId = item.int_jobCode_id;
                jobCodeItem.plannedHours = item.dec_budget_hrs;
                jobCodeItem.rate = item.dec_job_code_rate;
                jobCodeItem.total = item.dec_budget_amt;
                jobCodeItem.notes = item.vc_notes;
                jobCodeItem.isDeleted = false;
                jobCodeList.Add(jobCodeItem);
            }
            return jobCodeList;
        }
        #region Sites

        public string GetSiteSerialNumber(long? QuoteId)
        {
            string serialNum = "";
            List<tbl_ibw_site> allSites = _quotesExtensionDB.tbl_ibw_sites.Where(x => x.int_quote_id == QuoteId && x.vc_site_number != null).ToList();
            if (allSites.Count > 0)
            {
                string latestSerialNum = allSites.OrderByDescending(x => x.int_site_id).Select(y => y.vc_site_number).FirstOrDefault();
                int incrementedNum = Convert.ToInt32(latestSerialNum) + 1;
                serialNum = incrementedNum.ToString("D" + 3);
            }
            else
            {
                serialNum = Literals.FirstSiteNumber;  //"001";
            }
            return serialNum;
        }

        //Author : Sreebharath 
        //Date : 16-10-2019
        //Upsert Sites for Quotes
        public long? upsertSitesForQuotes(Sites model)
        {
            try
            {
                if (model.siteId == null)
                {
                    if (setUpGoogleDriveFileForEntireQuoteStructure(model.quoteId) != null)
                    {
                        model.siteSerialNumber = GetSiteSerialNumber(model.quoteId);
                        string quoteGoogeDriveFolderId = _quotesExtensionDB.GetQuoteDetailsById(model.quoteId).Select(x => x.GDriveFileId).FirstOrDefault();
                        model.googleDriveFolderId = GoogleDriveFilesRepository.CreateFolder(model.siteSerialNumber + " " + model.siteName, quoteGoogeDriveFolderId);
                    }
                }
                else
                {
                    var sitedata = getAllSitesOfQuote(model.quoteId).Where(x => x.siteId == model.siteId).FirstOrDefault();
                    if (sitedata.siteName != model.siteName)
                    {
                        if (setUpGoogleDriveFileForEntireQuoteStructure(model.quoteId, model.siteId) != null)
                        {
                            var siteInfo = getAllSitesOfQuote(model.quoteId).Where(x => x.siteId == model.siteId).FirstOrDefault();
                            string SiteSerialNumber = model.siteSerialNumber != null ? model.siteSerialNumber + " " : "";
                            GoogleDriveFilesRepository.UpdateFolderName(SiteSerialNumber + " " + model.siteName, siteInfo.googleDriveFolderId);
                        }
                    }
                }
                long? result = null;
                _quotesExtensionDB.upsertSitesQuotes(model.siteId, model.siteName, model.quoteId, model.streetAddress, model.cityName, model.zipCode, model.countryId, model.stateId,
                    model.muncipalityId, model.googleDriveFolderId, model.siteSerialNumber, model.createdBy, model.modifiedBy, model.isDefault, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public List<Sites> getAllSitesOfQuote()
        {
            try
            {
                var data = _quotesDB.getAllSitesinQuotes().ToList();
                List<Sites> AllSites = new List<Sites>();
                foreach (var item in data)
                {
                    Sites eachSitedata = new Sites();
                    eachSitedata.siteId = item.int_site_id;
                    eachSitedata.siteName = item.vc_site_name;
                    eachSitedata.label = item.vc_site_name;
                    eachSitedata.quoteId = item.int_quote_id;
                    eachSitedata.addressId = item.int_address_id;
                    eachSitedata.children = getAllSOWOfQuote(eachSitedata.quoteId, eachSitedata.siteId);
                    AllSites.Add(eachSitedata);
                }
                return AllSites;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Sreebharath 
        //Date : 17-10-2019
        //Site Name Validation for not allowing duplicates
        public bool siteNameValidation(long? quoteId, long? siteId, string siteName)
        {
            try
            {
                List<getAllSitesOfQuoteResult> allSitesList;
                if (siteId == null)
                {
                    allSitesList = _quotesExtensionDB.getAllSitesOfQuote(quoteId).Where(x => x.vc_site_name.ToUpper() == siteName.ToUpper() && x.bt_delete == false).ToList();
                }
                else
                {
                    allSitesList = _quotesExtensionDB.getAllSitesOfQuote(quoteId).Where(x => x.vc_site_name.ToUpper() == siteName.ToUpper() && x.int_site_id != siteId && x.bt_delete == false).ToList();
                }
                if (allSitesList.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;

                }
            }
            catch { throw; }
        }

        //Author : Sreebharath 
        //Date : 18-10-2019
        //Delete Site in Quotes
        public long? deleteSiteInQuotes(Sites model)
        {
            try
            {
                string siteGoogeDriveFolderId = getAllSitesOfQuote(model.quoteId).Where(x => x.siteId == model.siteId).Select(y => y.googleDriveFolderId).FirstOrDefault();
                GoogleDriveFilesRepository.DeleteFile(siteGoogeDriveFolderId);
                long? result = null;
                _quotesDB.deleteSiteInQuote(model.siteId, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region SOW
        public int ColumnIndexByName(string column)
        {
            int retVal = 0;
            string col = column.ToUpper();
            for (int iChar = col.Length - 1; iChar >= 0; iChar--)
            {
                char colPiece = col[iChar];
                int colNum = colPiece - 64;
                retVal = retVal + colNum * (int)Math.Pow(26, col.Length - (iChar + 1));
            }
            return retVal;
        }
        // Convert 0 based index to Column name
        public string ColumnNameByIndex(int ColumnIndex)
        {
            int ModOf26, Subtract;
            StringBuilder NumberInLetters = new StringBuilder();
            ColumnIndex += 1; // A is the first column, but for calculation it's number is 1 and not 0. however, Index is alsways zero-based.
            while (ColumnIndex > 0)
            {
                if (ColumnIndex <= 26)
                {
                    ModOf26 = ColumnIndex;
                    NumberInLetters.Insert(0, mColumnLetters.Substring(ModOf26, 1));
                    ColumnIndex = 0;
                }
                else
                {
                    ModOf26 = ColumnIndex % 26;
                    Subtract = (ModOf26 == 0) ? 26 : ModOf26;
                    ColumnIndex = (ColumnIndex - Subtract) / 26;
                    NumberInLetters.Insert(0, mColumnLetters.Substring(ModOf26, 1));
                }
            }
            return NumberInLetters.ToString().ToUpper();
        }
        public string GetSOWSerialNumber(long? QuoteId, long? siteId)
        {
            string serialNum = "";
            List<tbl_ibw_sow> allSows = _quotesExtensionDB.tbl_ibw_sows.Where(x => x.int_quote_id == QuoteId && x.int_site_id == siteId && x.vc_sow_number != null).ToList();
            if (allSows.Count > 0)
            {
                string latestSerialNum = allSows.OrderByDescending(x => x.int_sow_id).Select(y => y.vc_sow_number).FirstOrDefault();

                //int incrementedNum = ColumnIndexByName("AZ") + 1;
                int incrementedNum = ColumnIndexByName(latestSerialNum);
                // serialNum = incrementedNum.ToString("D" + 3);
                serialNum = ColumnNameByIndex(incrementedNum);
            }
            else
            {
                serialNum = Literals.FirstSOWNumber;  //"A";
            }

            return serialNum;
        }

        //Author : Sreebharath 
        //Date : 16-10-2019
        //Upsert SOW for Quotes
        public long? upsertSOWForQuotes(Sow model)
        {
            try
            {

                if (model.sowId == null)
                {
                    if (setUpGoogleDriveFileForEntireQuoteStructure(model.quoteId, model.siteId) != null)
                    {
                        string siteGoogeDriveFolderId = getAllSitesOfQuote(model.quoteId).Where(x => x.siteId == model.siteId).Select(y => y.googleDriveFolderId).FirstOrDefault();
                        model.sowNumber = GetSOWSerialNumber(model.quoteId, model.siteId);


                        model.googleDriveFolderId = GoogleDriveFilesRepository.CreateFolder(model.sowNumber + " " + model.sowName, siteGoogeDriveFolderId);
                    }
                }
                else
                {
                    var sowData = _quotesExtensionDB.getAllSowOfQuoteandSite(model.quoteId, model.siteId).Where(x => x.int_sow_id == model.sowId).FirstOrDefault();
                    if (sowData.vc_sow_name != model.sowName)
                    {
                        if (setUpGoogleDriveFileForEntireQuoteStructure(model.quoteId, model.siteId, model.sowId) != null)
                        {
                            var sowInfo = _quotesExtensionDB.getAllSowOfQuoteandSite(model.quoteId, model.siteId).Where(x => x.int_sow_id == model.sowId).FirstOrDefault();

                            //string latestSowNumber = "";
                            //if (model.sowNumber.Contains("#"))
                            //{
                            //    string[] arrayLatestSerialNum = model.sowNumber.Split('#');

                            //    latestSowNumber = arrayLatestSerialNum[1];
                            //}
                            //else
                            //{
                            //    latestSowNumber = model.sowNumber;
                            // }
                            GoogleDriveFilesRepository.UpdateFolderName((model.sowNumber != null ? model.sowNumber : "") + " " + model.sowName, sowInfo.vc_google_drive_folder_id);

                        }
                    }
                    if (sowData.int_sow_status_id != model.sowStatus)
                    {
                        long? statusActivityLogId = UpdateQuoteStatusActivity(null, model.quoteId, model.sowStatus, null, null, model.statusOptionId, model.remarks, "", "", model.modifiedBy, "sow", model.sowId);

                    }
                }
                long? result = null;
                _quotesExtensionDB.upsertSOWQuotes(model.sowId, model.sowName, model.sowTypeId, model.siteId, model.quoteId, model.sowNumber, model.createdBy, model.modifiedBy, model.googleDriveFolderId, model.sowDescription, model.isApproved, model.isDefault, model.sowInvoiceType, model.sowStatus, ref result);


                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        //Author : Sreebharath 
        //Date : 17-10-2019
        //SOW Name Validation for not allowing duplicates
        public bool sowNameValidation(long? quoteId, long? siteId, long? sowId, string sowName)
        {
            try
            {
                List<getAllSowOfQuoteandSiteResult> allSowList;
                if (sowId == null)
                {
                    allSowList = _quotesExtensionDB.getAllSowOfQuoteandSite(quoteId, siteId).Where(x => x.vc_sow_name.ToUpper() == sowName.ToUpper() && x.bt_delete == false).ToList();
                }
                else
                {
                    allSowList = _quotesExtensionDB.getAllSowOfQuoteandSite(quoteId, siteId).Where(x => x.vc_sow_name.ToUpper() == sowName.ToUpper() && x.int_sow_id != sowId && x.bt_delete == false).ToList();
                }
                if (allSowList.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;

                }
            }
            catch { throw; }
        }


        //Author : Sreebharath 
        //Date : 18-10-2019
        //Delete SOW in Quotes
        public long? deleteSowInQuotes(Sow model)
        {
            try
            {
                string sowGoogeDriveFolderId = getAllSOWOfQuote(model.quoteId, model.siteId).Where(x => x.sowId == model.sowId).Select(y => y.googleDriveFolderId).FirstOrDefault();
                if (sowGoogeDriveFolderId != "" && sowGoogeDriveFolderId != null)
                {
                    GoogleDriveFilesRepository.DeleteFile(sowGoogeDriveFolderId);
                }
                long? result = null;
                _quotesDB.deleteSOWInQuote(model.sowId, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #endregion

        #region Manage
        public List<ActionsCreate> getAllSiteNSowsOfBudget(long? QuoteId)
        {
            List<ActionsCreate> sitesandSowList = _quotesExtensionDB.getAllSavedSitesNSowInBudget(QuoteId).Select(x => new ActionsCreate
            {
                quoteId = x.int_quote_id,
                siteId = x.int_site_id,
                siteName = x.vc_site_name,
                sowId = x.int_sow_id,
                sowName = x.vc_sow_name,
                sowNumber = x.vc_sow_number,
                budgetSowId = x.int_budget_sow_id,
                manageSowId = x.int_manage_sow_id
            }).ToList();
            return sitesandSowList;
        }
        public List<TaskBudget> getSavedTasksInBudget(long? budgetSowId)
        {
            List<TaskBudget> tasksList = new List<TaskBudget>();
            var data = _quotesExtensionDB.getSavedTasksInBudget(budgetSowId).ToList();
            foreach (var item in data)
            {
                TaskBudget taskItem = new TaskBudget();
                taskItem.taskManageId = item.int_ibw_manage_task_id;
                taskItem.taskBudgetId = item.int_task_in_sow_id;
                taskItem.taskId = item.int_task_id;
                taskItem.taskName = item.vc_task_name;
                tasksList.Add(taskItem);
            }
            return tasksList;
        }
        public long? insertActionInManageGlobally(ActionsCreate model, bool isFileExists)
        {
            bool? isUploaded = setUpGoogleDriveFileForEntireQuoteStructure(model.quoteId, model.siteId, model.sowId, model.taskBudgetId);
            string googleDriveUploadedFileId = "";
            var userDetails = _adminDB.sp_getAllIBWUsers().Where(x => x.int_user_id == model.createdBy).FirstOrDefault();
            if (isUploaded != null)
            {
                var taskBudgetData = _quotesExtensionDB.tbl_tasks_in_sow_budgets.Where(x => x.int_task_in_sow_id == model.taskBudgetId).FirstOrDefault();
                string actionFileName = DateTime.Now.ToString("yyyyMMdd") + "-" + model.actionName + "-" + DateTime.Now.ToString("HH:mm");
                model.googleDriveFileId = GoogleDriveFilesRepository.CreateFolder(actionFileName, taskBudgetData.vc_google_drive_folder_id);
                if (isFileExists == true)
                {
                    googleDriveUploadedFileId = GoogleDriveFilesRepository.FileUploadInFolderDirect(model.googleDriveFileId, model.googleDriveFile, (userDetails.vc_user_name + " " + userDetails.vc_alias_name), null);

                }
                long? actionResult = null;
                if (model.manageSowId != null && model.manageSowId != 0)
                {
                    _quotesExtensionDB.insertActionInManageGlobally(model.manageSowId, model.taskManageId, model.actionId, model.jobCodeId, model.actionName, model.jobCodeName, model.jobCodeRate,
                                  model.plannedHours, model.plannedCost, model.dueDate, model.dueDateOption, 0, 0, model.notes, googleDriveUploadedFileId, model.googleDriveFileId, model.isRemedial, model.teamMemberId, model.createdBy, ref actionResult);
                    return actionResult;
                }
                else
                {
                    _quotesExtensionDB.saveManageTaskAndAction(model.quoteId, model.sowId, model.budgetSowId, model.taskBudgetId, model.actionId, model.actionName, model.jobCodeId, model.jobCodeName, model.jobCodeRate,
                             model.plannedHours, model.plannedCost, model.dueDate, model.dueDateOption, model.notes, googleDriveUploadedFileId, model.googleDriveFileId, model.isRemedial, model.teamMemberId, model.createdBy, ref actionResult);
                    return actionResult;
                }

            }
            else
            {
                return 0;
            }
        }
        public List<ActionsCreate> getAllSitesAndSowOfManageAction(long? QuoteId)
        {
            List<ActionsCreate> sitesandSowList = _quotesDB.getAllSitesNSowsInManageOfQuote(QuoteId).Select(x => new ActionsCreate
            {
                quoteId = x.int_quote_id,
                siteId = x.int_site_id,
                siteName = x.vc_site_name,
                sowId = x.int_sow_id,
                sowName = x.vc_sow_name,
                sowNumber = x.vc_sow_number,
                manageSowId = x.int_manage_sow_id
            }).ToList();
            return sitesandSowList;
        }


        public getManageActionParentsResult getManageActionParentIds(long? manageActionId)
        {
            return _quotesDB.getManageActionParents(manageActionId).FirstOrDefault();
        }
        //Author : Sreebharath 
        //Date : 26-11-2019
        //Tpo upsert Manage for Sow
        public long? upsertManageOfSow(Manage model, long? userId)
        {
            try
            {
                long? result = 0;
                long? ActionResult = 0;
                //  string sowGoogeDriveFolderId = getAllSOWOfQuote(model.quoteId, model.siteId).Where(x => x.siteId == model.siteId && x.sowId == model.sowId).Select(y => y.googleDriveFolderId).FirstOrDefault();
                var userDetails = _adminDB.sp_getAllIBWUsers().Where(x => x.int_user_id == model.createdBy).FirstOrDefault();
                _quotesDB.upsertManageSOW(model.manageId, model.sowId, model.totalActualCost, model.totalActualHours, model.totalPlannedCost, model.totalPlannedHours,
                    model.isSubmitted, model.createdBy, model.modifiedBy, ref result);
                if (result > 0)
                {
                    if (model.taskData.Count > 0)
                    {
                        foreach (var taskItem in model.taskData)
                        {
                            long? taskResult = 0;
                            _quotesDB.upsertTasksinManage(taskItem.taskManageId, result, taskItem.taskId, taskItem.taskName, taskItem.totalPlannedCost, taskItem.totalActualCost, taskItem.taskBudgetId,
                                taskItem.totalPlannedHours, taskItem.totalActualHours, ref taskResult);
                            if (taskResult > 0)
                            {
                                if (taskItem.actionData.Count > 0)
                                {
                                    foreach (var actionItem in taskItem.actionData.Where(x => x.isDeleted == false))
                                    {
                                        string actionFileName = "";
                                        if (actionItem.actionManageId == null)
                                        {

                                            if (setUpGoogleDriveFileForEntireQuoteStructure(model.quoteId, model.siteId, model.sowId, taskItem.taskBudgetId, actionItem.actionManageId) != null)
                                            {
                                                actionFileName = DateTime.Now.ToString("yyyyMMdd") + "-" + actionItem.actionName + "-" + DateTime.Now.ToString("hh:mm tt");
                                                string taskGoogeDriveFolderId = _quotesExtensionDB.tbl_tasks_in_sow_budgets.Where(x => x.int_task_in_sow_id == taskItem.taskBudgetId).Select(y => y.vc_google_drive_folder_id).FirstOrDefault();
                                                actionItem.googleDriveFileId = GoogleDriveFilesRepository.CreateFolder(actionFileName, taskGoogeDriveFolderId);
                                            }
                                        }
                                        else
                                        {

                                            if (setUpGoogleDriveFileForEntireQuoteStructure(model.quoteId, model.siteId, model.sowId, taskItem.taskBudgetId, actionItem.actionManageId) != null)
                                            {
                                                var actionData = _quotesExtensionDB.tbl_ibw_manage_actions.Where(x => x.int_ibw_manage_action_id == actionItem.actionManageId).FirstOrDefault();
                                                actionFileName = DateTime.Parse(actionData.dt_created_date.ToString()).ToString("yyyyMMdd") + "-" + actionItem.actionName + "-" + DateTime.Parse(actionData.dt_created_date.ToString()).ToString("hh:mm tt");
                                                GoogleDriveFilesRepository.UpdateFolderName(actionFileName, actionData.vc_google_drive_file_id);
                                            }
                                        }

                                        string googleFileId = "";
                                        if (actionItem.googleDriveFile != null)
                                        {
                                            if (setUpGoogleDriveFileForEntireQuoteStructure(model.quoteId, model.siteId, model.sowId, taskItem.taskBudgetId, actionItem.actionManageId) != null)
                                            {
                                                //  string actionGoogleDriveFileId = _quotesDB.tbl_ibw_manage_actions.Where(x => x.int_ibw_manage_action_id == actionItem.actionManageId).Select(y => y.vc_google_drive_file_id).FirstOrDefault();
                                                googleFileId = GoogleDriveFilesRepository.FileUploadInFolderDirect(actionItem.googleDriveFileId, actionItem.googleDriveFile, (userDetails.vc_user_name + " " + userDetails.vc_alias_name), actionItem.mimeType);
                                            }

                                        }
                                        _quotesExtensionDB.upsertActionsInManage(actionItem.actionManageId, taskResult, actionItem.actionId, actionItem.jobCodeId,
                                            actionItem.actionName, actionItem.jobCodeName, actionItem.jobCodeRate, actionItem.plannedHours, actionItem.plannedCost,
                                            actionItem.dueDate, actionItem.dueDateOption, actionItem.actualHours, actionItem.actualCost, actionItem.notes, googleFileId, actionItem.googleDriveFileId, actionItem.isDeleted, actionItem.isRemedial, actionItem.teamMemberId, userId, ref ActionResult);
                                    }
                                    List<ActionsManage> deletedActions = taskItem.actionData.Where(x => x.isDeleted == true).ToList();
                                    if (deletedActions.Count > 0)
                                    {
                                        foreach (var actionDeletedItem in deletedActions)
                                        {
                                            string actionGoogleDriveFileId = _quotesExtensionDB.tbl_ibw_manage_actions.Where(x => x.int_ibw_manage_action_id == actionDeletedItem.actionManageId).Select(y => y.vc_google_drive_file_id).FirstOrDefault();
                                            GoogleDriveFilesRepository.DeleteFile(actionGoogleDriveFileId);
                                            // deleteGoogleDriveFilesfromList(_quotesDB.tbl_ibw_manage_actions_files.Where(x => x.int_ibw_manage_action_id == actionDeletedItem.actionManageId).ToList());
                                            long? deletedActionId = null;
                                            _quotesDB.deleteActionsAlongWithFilesInManage(actionDeletedItem.actionManageId, ref deletedActionId);

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (result > 0)
                {
                    return result;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool? setUpGoogleDriveFileForEntireQuoteStructure(long? QuoteID, long? SiteID = null, long? SOWID = null, long? TaskBudgetID = null, long? ManageActionID = null)
        {
            try
            {

                var quoteData = _quotesExtensionDB.tbl_ibw_quotes.Where(x => x.int_quote_id == QuoteID).FirstOrDefault();
                var siteData = _quotesExtensionDB.tbl_ibw_sites.Where(x => x.int_site_id == SiteID).FirstOrDefault();
                var sowData = _quotesExtensionDB.tbl_ibw_sows.Where(x => x.int_sow_id == SOWID).FirstOrDefault();
                var taskData = _quotesExtensionDB.tbl_tasks_in_sow_budgets.Where(x => x.int_task_in_sow_id == TaskBudgetID).FirstOrDefault();
                var actionData = _quotesExtensionDB.tbl_ibw_manage_actions.Where(x => x.int_ibw_manage_action_id == ManageActionID).FirstOrDefault();

                bool? returnValue = true;
                if (GoogleDriveFilesRepository.CheckFileExists(quoteData.vc_GDrive_file_id))
                {
                    returnValue = true;
                    if (SiteID != null)
                    {
                        if (GoogleDriveFilesRepository.CheckFileExists(siteData.vc_google_drive_folder_id))
                        {
                            returnValue = true;
                            if (SOWID != null)
                            {
                                if (GoogleDriveFilesRepository.CheckFileExists(sowData.vc_google_drive_folder_id))
                                {
                                    returnValue = true;
                                    if (TaskBudgetID != null)
                                    {
                                        if (GoogleDriveFilesRepository.CheckFileExists(taskData.vc_google_drive_folder_id))
                                        {
                                            returnValue = true;
                                            if (ManageActionID != null)
                                            {
                                                if (actionData.vc_google_drive_file_id != null && GoogleDriveFilesRepository.CheckFileExists(actionData.vc_google_drive_file_id))
                                                {
                                                    returnValue = true;
                                                }
                                                else
                                                {
                                                    returnValue = false;
                                                    string actionFileName = DateTime.Now.ToString("yyyyMMdd") + "-" + actionData.vc_custom_action_name + "-" + DateTime.Now.ToString("hh:mm tt");
                                                    var tblActions = _quotesExtensionDB.tbl_ibw_manage_actions.Single(x => x.int_ibw_manage_action_id == ManageActionID);
                                                    tblActions.vc_google_drive_file_id = GoogleDriveFilesRepository.CreateFolder(actionFileName, taskData.vc_google_drive_folder_id);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            returnValue = false;
                                            var tblTasks = _quotesExtensionDB.tbl_tasks_in_sow_budgets.Single(x => x.int_task_in_sow_id == TaskBudgetID);
                                            tblTasks.vc_google_drive_folder_id = GoogleDriveFilesRepository.CreateFolder(taskData.vc_task_name, sowData.vc_google_drive_folder_id);
                                            if (ManageActionID != null)
                                            {
                                                string actionFileName = DateTime.Now.ToString("yyyyMMdd") + "-" + actionData.vc_custom_action_name + "-" + DateTime.Now.ToString("hh:mm tt");
                                                var tblActions = _quotesExtensionDB.tbl_ibw_manage_actions.Single(x => x.int_ibw_manage_action_id == ManageActionID);
                                                tblActions.vc_google_drive_file_id = GoogleDriveFilesRepository.CreateFolder(actionFileName, taskData.vc_google_drive_folder_id);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    returnValue = false;
                                    var tblSOWs = _quotesExtensionDB.tbl_ibw_sows.Single(x => x.int_sow_id == SOWID);

                                    //string latestSowNumber = "";
                                    //if (sowData.vc_sow_number.Contains("#"))
                                    //{
                                    //    string[] arrayLatestSerialNum = sowData.vc_sow_number.Split('#');
                                    //    latestSowNumber = arrayLatestSerialNum[1];
                                    //}
                                    //else
                                    //{
                                    //    latestSowNumber = sowData.vc_sow_number;
                                    //}

                                    tblSOWs.vc_google_drive_folder_id = GoogleDriveFilesRepository.CreateFolder((sowData.vc_sow_number != null ? sowData.vc_sow_number : "") + " " + sowData.vc_sow_name, siteData.vc_google_drive_folder_id);
                                    if (TaskBudgetID != null)
                                    {
                                        var tblTasks = _quotesExtensionDB.tbl_tasks_in_sow_budgets.Single(x => x.int_task_in_sow_id == TaskBudgetID);
                                        tblTasks.vc_google_drive_folder_id = GoogleDriveFilesRepository.CreateFolder(taskData.vc_task_name, sowData.vc_google_drive_folder_id);
                                        if (ManageActionID != null)
                                        {
                                            string actionFileName = DateTime.Now.ToString("yyyyMMdd") + "-" + actionData.vc_custom_action_name + "-" + DateTime.Now.ToString("hh:mm tt");
                                            var tblActions = _quotesExtensionDB.tbl_ibw_manage_actions.Single(x => x.int_ibw_manage_action_id == ManageActionID);
                                            tblActions.vc_google_drive_file_id = GoogleDriveFilesRepository.CreateFolder(actionFileName, taskData.vc_google_drive_folder_id);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            returnValue = false;
                            var tblSite = _quotesExtensionDB.tbl_ibw_sites.Single(x => x.int_site_id == SiteID);
                            string SiteSerialNumber = siteData.vc_site_number == null ? siteData.vc_site_number + " " : "";

                            tblSite.vc_google_drive_folder_id = GoogleDriveFilesRepository.CreateFolder(SiteSerialNumber + siteData.vc_site_name, quoteData.vc_GDrive_file_id);
                            if (SOWID != null)
                            {
                                var tblSOWs = _quotesExtensionDB.tbl_ibw_sows.Single(x => x.int_sow_id == SOWID);
                                //string latestSowNumber = "";
                                //if (sowData.vc_sow_number.Contains("#"))
                                //{
                                //    string[] arrayLatestSerialNum = sowData.vc_sow_number.Split('#');
                                //    latestSowNumber = arrayLatestSerialNum[1];
                                //}
                                //else
                                //{
                                //    latestSowNumber = sowData.vc_sow_number;
                                //}

                                tblSOWs.vc_google_drive_folder_id = GoogleDriveFilesRepository.CreateFolder((sowData.vc_sow_number != null ? sowData.vc_sow_number : "") + " " + sowData.vc_sow_name, siteData.vc_google_drive_folder_id);
                                if (TaskBudgetID != null)
                                {
                                    var tblTasks = _quotesExtensionDB.tbl_tasks_in_sow_budgets.Single(x => x.int_task_in_sow_id == TaskBudgetID);
                                    tblTasks.vc_google_drive_folder_id = GoogleDriveFilesRepository.CreateFolder(taskData.vc_task_name, sowData.vc_google_drive_folder_id);
                                    if (ManageActionID != null)
                                    {
                                        string actionFileName = DateTime.Now.ToString("yyyyMMdd") + "-" + actionData.vc_custom_action_name + "-" + DateTime.Now.ToString("hh:mm tt");
                                        var tblActions = _quotesExtensionDB.tbl_ibw_manage_actions.Single(x => x.int_ibw_manage_action_id == ManageActionID);
                                        tblActions.vc_google_drive_file_id = GoogleDriveFilesRepository.CreateFolder(actionFileName, taskData.vc_google_drive_folder_id);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    returnValue = false;
                    var tblQuote = _quotesExtensionDB.tbl_ibw_quotes.Single(x => x.int_quote_id == QuoteID);
                    tblQuote.vc_GDrive_file_id = SaveFileInGDrive(QuoteID, quoteData.bt_project == true ? "Project" : "Quote");
                    if (SiteID != null)
                    {
                        var tblSite = _quotesExtensionDB.tbl_ibw_sites.Single(x => x.int_site_id == SiteID);
                        string SiteSerialNumber = siteData.vc_site_number == null ? siteData.vc_site_number + " " : "";

                        tblSite.vc_google_drive_folder_id = GoogleDriveFilesRepository.CreateFolder(SiteSerialNumber + siteData.vc_site_name, quoteData.vc_GDrive_file_id);
                        if (SOWID != null)
                        {
                            var tblSOWs = _quotesExtensionDB.tbl_ibw_sows.Single(x => x.int_sow_id == SOWID);
                            //string latestSowNumber = "";
                            //if (sowData.vc_sow_number.Contains("#"))
                            //{
                            //    string[] arrayLatestSerialNum = sowData.vc_sow_number.Split('#');
                            //    latestSowNumber = arrayLatestSerialNum[1];
                            //}
                            //else
                            //{
                            //    latestSowNumber = sowData.vc_sow_number;
                            //}

                            tblSOWs.vc_google_drive_folder_id = GoogleDriveFilesRepository.CreateFolder((sowData.vc_sow_number != null ? sowData.vc_sow_number : "") + " " + sowData.vc_sow_name, siteData.vc_google_drive_folder_id);
                            if (TaskBudgetID != null)
                            {
                                var tblTasks = _quotesExtensionDB.tbl_tasks_in_sow_budgets.Single(x => x.int_task_in_sow_id == TaskBudgetID);
                                tblTasks.vc_google_drive_folder_id = GoogleDriveFilesRepository.CreateFolder(taskData.vc_task_name, sowData.vc_google_drive_folder_id);
                                if (ManageActionID != null)
                                {
                                    string actionFileName = DateTime.Now.ToString("yyyyMMdd") + "-" + actionData.vc_custom_action_name + "-" + DateTime.Now.ToString("hh:mm tt");
                                    var tblActions = _quotesExtensionDB.tbl_ibw_manage_actions.Single(x => x.int_ibw_manage_action_id == ManageActionID);
                                    tblActions.vc_google_drive_file_id = GoogleDriveFilesRepository.CreateFolder(actionFileName, taskData.vc_google_drive_folder_id);
                                }
                            }
                        }
                    }
                }
                _quotesExtensionDB.SubmitChanges();
                return returnValue;
            }
            catch (Exception ex)
            {
                return null;
                throw ex;
            }

        }


        public void deleteGoogleDriveFilesfromList(List<string> filesList)
        {
            foreach (var item in filesList)
            {
                GoogleDriveFilesRepository.DeleteFile(item);
            }
        }


        //This Method is used to get the budget data in the budget as well as in Manage
        public Manage getSavedManageData(long? sowId)
        {
            var item = _quotesDB.getManageDetailsOfSOW(sowId).FirstOrDefault();
            if (item != null)
            {
                Manage manage = new Manage();
                manage.manageId = item.int_manage_sow_id;
                manage.sowId = item.int_sow_id;
                manage.totalActualCost = item.dec_actual_amount;
                manage.totalActualHours = item.dec_actual_hours;
                manage.totalPlannedCost = item.dec_planned_amount;
                manage.totalPlannedHours = item.dec_planned_hours;
                manage.totalBudgetAmount = item.dec_budget_amount;
                manage.totalBudgetHours = item.dec_budget_hrs;
                manage.totalQuotedAmount = item.dec_quoted_amount;
                manage.totalExpenseCost = _quotesDB.getAllExpensesOfSOW(item.int_sow_id).Sum(y => y.dec_total_expense_cost);
                manage.isSubmitted = item.bt_isSubmitted;
                manage.taskData = getSavedTasksInManage(manage.manageId);
                return manage;
            }
            else
            {
                return null;
            }
        }



        public List<TaskManage> getSavedTasksInManage(long? manageSowId, bool isCreate = false)
        {
            List<TaskManage> tasksList = new List<TaskManage>();
            var data = _quotesDB.getSavedTasksInManage(manageSowId).ToList();
            foreach (var item in data)
            {
                TaskManage taskItem = new TaskManage();
                taskItem.taskManageId = item.int_ibw_manage_task_id;
                taskItem.manageId = item.int_ibw_manage_sow_id;
                taskItem.taskBudgetId = item.int_ibw_budget_task_id;
                taskItem.taskId = item.int_task_id;
                taskItem.taskName = item.vc_task_name;
                taskItem.totalActualCost = item.dec_actual_amount;
                taskItem.totalPlannedCost = item.dec_planned_amount;
                taskItem.totalPlannedHours = item.dec_planned_hours;
                taskItem.totalActualHours = item.dec_actual_hours;
                taskItem.totalBudetAmount = item.dec_budget_amount;
                taskItem.totalBudgetHours = item.dec_budget_hrs;
                taskItem.totalQuotedAmount = item.dec_quoted_amount;
                if (isCreate == false)
                    taskItem.actionData = getSavedActionsInManage(taskItem.taskManageId);
                tasksList.Add(taskItem);
            }
            return tasksList;
        }

        public List<ActionsManage> getSavedActionsInManage(long? taskInSowId)
        {
            List<ActionsManage> actionsList = new List<ActionsManage>();
            var data = _quotesDB.getActionsInTasksManage(taskInSowId).ToList();
            foreach (var item in data)
            {
                ActionsManage actionItem = new ActionsManage();
                actionItem.actionManageId = item.int_ibw_manage_action_id;
                actionItem.actionId = item.int_action_id;
                actionItem.jobCodeId = item.int_job_code_id;
                actionItem.notes = item.vc_notes;
                actionItem.jobCodeName = item.vc_job_code_title;
                actionItem.jobCodeRate = item.dec_job_code_rate;
                actionItem.actionName = item.vc_custom_action_name;
                actionItem.dueDate = item.dt_due_date;
                actionItem.googleDriveFileId = item.vc_google_drive_file_id;
                actionItem.dueDateOption = item.int_due_date_lookup_id;
                actionItem.plannedCost = item.dec_planned_amount;
                actionItem.plannedHours = item.dec_planned_hours;
                actionItem.actualCost = item.dec_actual_amount;
                actionItem.actualHours = item.dec_actual_hours;
                actionItem.isRemedial = item.bt_isRemedial_action;
                actionItem.teamMemberId = item.int_team_member_id;
                actionItem.isScheduled = item.bt_scheduled;
                actionItem.isDeleted = false;
                actionItem.scheduledActionsCount = item.int_scheduled_actions_count;
                actionsList.Add(actionItem);
            }
            return actionsList;
        }

        public List<ExpensesManage> getExpensesInManage(long? sowId)
        {
            List<ExpensesManage> expensesList = new List<ExpensesManage>();
            var data = _quotesDB.getAllExpensesOfAction(sowId).Where(x => x.bt_approved == true).ToList();
            expensesList = data.Select(x => new ExpensesManage
            {
                actionLogHourExpenseId = x.int_action_log_hours_expense_id,
                actionLogHourId = x.int_action_log_hour_id,
                expenseCodeId = x.int_expense_code_id,
                expenseCodeName = x.expenseCode_vc_expense_code,
                expenseCodeIsDeleted = x.expensCode_bt_delete,
                expenseCodeStatus = x.expenseCode_bt_status,
                limitType = x.expenseCode_vc_limitType,
                expenseUnit = x.expenseCode_vc_unitName,
                expenseRate = x.dec_expense_rate,
                quantity = x.int_quanity,
                total = x.dec_total_expense_cost,
                description = x.vc_description,
                actionFolderId = x.ActionGoogleDriveFileId
            }).ToList();
            return expensesList;
        }



        #endregion

        #region Log Hours
        public List<LogHours> getTimeSheetsOfQuote(long quoteId)
        {
            List<LogHours> logHoursList = new List<LogHours>();
            var data = _schedulingDB.sp_get_user_timesheet().Where(x => x.quoteId == quoteId).ToList();

            logHoursList = data.Select(item => new LogHours
            {
                logHourId = item.timesheetId,
                isAssigned = item.isAssigned,
                projectManager = item.projectManager,
                quoteID = item.quoteId,
                siteName = item.siteName,
                sowName = item.SOWName,
                taskName = item.taskName,
                manageActionName = item.actionName,
                jobCode = item.jobCodeName,
                jobCodeTitle = item.jobCodeTitle,
                plannedHours = item.plannedHours != "-" ? Convert.ToDecimal(item.plannedHours) : Convert.ToDecimal(0),
                workedHours = item.workedHours != "-" ? Convert.ToDecimal(item.workedHours) : Convert.ToDecimal(0),
                remainingHours = item.remainingHours != "-" ? Convert.ToDecimal(item.remainingHours) : Convert.ToDecimal(0),
                dueDate = item.dueDate,
                loggedDate = item.completionDate,
                loggedBy = item.userName,
                updatedBy = item.modifiedByName,
                updatedDate = item.modifiedDate,
                logHoursStatus = item.completionStatus,
                logHourRemarks = item.notes,
                googleDriveId = item.googleFileId,
                status = item.isApproved,
                pmMessage = item.pm_message,
                siteId = item.siteId,
                sowId = item.sowId,
                notesCount = item.int_notes_count
            }).ToList();

            return logHoursList.OrderBy(x => x.loggedDate).ToList();
        }

        public List<ActionExpenses> getExpensesOfQuote(long quoteId)
        {
            var data = _schedulingDB.sp_get_pm_expenses().Where(x => x.quoteId == quoteId).OrderByDescending(x => x.expenseId).Select(item => new ActionExpenses
            {
                expenseId = item.expenseId,
                isAssigned = item.isAssigned,
                projectManager = item.projectManager,
                quoteID = item.quoteId,
                siteName = item.siteName,
                sowName = item.SOWName,
                taskName = item.taskName,
                manageActionName = item.actionName,
                jobCode = item.jobCodeName,
                jobCodeTitle = item.jobCodeTitle,
                loggedDate = item.completionDate,
                loggedBy = item.userName,
                updatedBy = item.modifiedByName,
                updatedDate = item.modifiedDate,
                logHourRemarks = item.notes,
                expenseCode = item.expenseCode,
                expenseCodeId = item.expenseId,
                limitType = item.limitType,
                expenseRate = item.expenseRate,
                quantity = item.Quantity,
                isReimbursable = item.isReimbursable == "Yes" ? true : false,
                expenseDescription = item.notes,
                status = item.isApproved,
                googleDriveId = item.googleFileId,
                pmMessage = item.pm_message,
                siteId = item.siteId,
                sowId = item.sowId,
                notesCount = item.int_notes_count,
            }).ToList();
            return data.OrderBy(x => x.loggedDate).ToList();
        }


        #endregion

        #endregion








    }


}