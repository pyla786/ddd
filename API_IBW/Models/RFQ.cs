using API_IBW.DB_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_IBW.Models
{
    public class RFQ
    {
        public string QuoteNumber { get; set; }
        public string QuoteReceivedDate { get; set; }
        public List<CommonMasterData> ProjectTypes { get; set; }
        public List<Clients> Clients { get; set; }
        public List<sp_getAllIBWUsersResult> ProjectManagers { get; set; }
        public List<sp_getAllIBWUsersResult> AccountManagers { get; set; }
    }
    public class Quote
    {
        public long? CommonMasterDataId { get; set; }
        public long? accountManagerId { get; set; }
        public long? clientId { get; set; }
        public long? contactId { get; set; }
        public long? contactTypeId { get; set; }
        public string dueDate { get; set; }
        public long? prepareQuoteInDays { get; set; }
        public string quotePreparationDueDate { get; set; }
        public long? projectManagerId { get; set; }
        public long? quoteId { get; set; }
        public string quoteNumber { get; set; }
        public string receivedDate { get; set; }
        public long? createdBy { get; set; }
        public string legacyNumber { get; set; }
        public bool isProject { get; set; }
    }
    public class QuoteAddress
    {
        public long? quoteId { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public long? countryId { get; set; }
        public long? municipalityId { get; set; }
        public string postal { get; set; }
        public long? stateId { get; set; }
        public long? createdBy { get; set; }
        [System.Web.Mvc.AllowHtml]
        public string legalDescription { get; set; }
    }
    
    public class Purpose
    {
        public long? quoteId { get; set; }
        public long? purposeOfSurveyId { get; set; }
        [System.Web.Mvc.AllowHtml]
        public string details { get; set; }
        public long? leadSourceId { get; set; }
        public string questionIds { get; set; }
        public long? createdBy { get; set; }
    }
    public class QuoteDetails
    {
        public GetQuoteDetailsByIdResult Quote { get; set; }
        public List<GetQuoteQuestionsResult> Questionnaire { get; set; }
    }
    public class UpdateQuote : GetQuoteDetailsByIdResult
    {
        public string quostionId { get; set; }
        public string reasons { get; set; }
        public string type { get; set; }
        [System.Web.Mvc.AllowHtml]
        public new string purposeDetails { get; set; }
        [System.Web.Mvc.AllowHtml]
        public string legalDescription { get; set; }
        public string gotoLink { get; set; }
    }
}