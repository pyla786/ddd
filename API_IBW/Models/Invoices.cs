using API_IBW.DB_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_IBW.Models
{
    public class Invoices
    {
        public long invoice_id { get; set; }
        public string invoice_number { get; set; }
        public string manual_invoice_number { get; set; }
        public string invoice_google_drive_file_id { get; set; }
        public string invoice_access_link { get; set; }
        public long rowspan { get; set; }
        public decimal? invoice_total { get; set; }
        public decimal? balance_amount { get; set; }
        public DateTime? invoice_date { get; set; }
        public string invoice_status { get; set; }
        public long? days_old { get; set; }
        public string sent_to { get; set; }
        public string sent_date { get; set; }
        public string sent_by { get; set; }
        public List<InvSites> sites { get; set; }
    }
    public class InvSitescontactsQuoteDetails
    {
        public InvQuoteDetails quotedetails { get; set; }
        public List<InvSites> sites { get; set; }
        public List<ClientContacts> contacts { get; set; }
    }
    public class InvSites
    {
        public string name { get; set; }
        public long rowspan { get; set; }
        public List<InvSows> sows { get; set; }
    }
    public class InvSows
    {
        public string name { get; set; }
        public decimal? budgetAmount { get; set; }
        public decimal? quotedAmount { get; set; }
        public string feestructure { get; set; }
        public long rowspan { get; set; }
        public List<InvTasks> tasks { get; set; }
    }
    public class InvTasks
    {
        public string name { get; set; }
        public long rowspan { get; set; }
        public List<InvActions> actions { get; set; }
    }
    public class InvActions
    {
        public long? int_invoice_item_id { get; set; }
        public long? int_action_log_hour_id { get; set; }
        public long? int_action_expense_id { get; set; }
        public long? int_timesheet_id { get; set; }
        public long? int_expense_id { get; set; }
        public long? int_budget_sow_id { get; set; }
        public string name { get; set; }
        public string jobcode { get; set; }
        public string jobtitle { get; set; }
        public string expensecode { get; set; }
        public decimal? worked { get; set; }
        public decimal? total { get; set; }
        public decimal? expenses { get; set; }
        public Boolean isselect { get; set; }
        public string username { get; set; }
        public DateTime? date { get; set; }
        public long percentinvoiced { get; set; }
        public decimal? invoiced { get; set; }
        public decimal? billed { get; set; }
        public decimal? remainingunbilled { get; set; }
        public decimal? totalinvoiced { get; set; }
        public decimal? totalremainingunbilled { get; set; }
    }
    public class SelectedActions
    {
        public List<long?> actionloghourids { get; set; }
        public List<long?> actionexpenseids { get; set; }
        public List<long?> timesheetids { get; set; }
        public List<long?> expensesids { get; set; }
        public List<long?> budgetsowids { get; set; }
    }
    public class InvQuoteDetails
    {
        public long? int_quote_id { get; set; }
        public long? int_client_id { get; set; }
        public string client_name { get; set; }
        public string client_email { get; set; }
        public string client_phone { get; set; }
        public string project_manager_name { get; set; }
        public string account_manager_name { get; set; }
        public long? int_contact_id { get; set; }
        public DateTime? dt_invoice_date { get; set; }
        public bool? is_existing_client { get; set; }
        public long? int_invoice_id { get; set; }
        public string auto_invoice_number { get; set; }
        public string manual_invoice_number { get; set; }
        public decimal? invoice_total { get; set; }
        public decimal? actual_total { get; set; }
        public string description { get; set; }
        public string purchase_order_number { get; set; }
    }
    public class ClientContacts
    {
        public long int_contact_id { get; set; }
        public string contact_name { get; set; }
        public string job_title { get; set; }
        public string contact_email { get; set; }
        public string contact_phone { get; set; }
    }
    public class InvoiceItems
    {
        public long? int_invoice_item_id{get; set;}
        public long? int_action_log_hour_id { get; set; }
        public long? int_action_expense_id { get; set; }
        public long? int_timesheet_id { get; set; }
        public long? int_expense_id { get; set; }
        public long? int_budget_sow_id { get; set; }
        public decimal? dec_invoiced_percent { get; set; }
        public decimal? dec_invoiced_total { get; set; }
    }
    public class UpsertInvoiceParameters
    {
        public InvQuoteDetails quotedetails { get; set; }
        public List<InvSites> sitesdetails { get; set; }
    }
    public class InvoiceTermsandConditions
    {
        public string termsandconditions { get; set; }
    }
    public class InvoicepaymentDetails
    {
        public string ProjectNumber { get; set; }
        public long InvoiceId { get; set; }
        public string vc_invoice_number { get; set; }
        public string vc_manual_invoice_number { get; set; }
        public decimal dec_invoice_total { get; set; }
        public string vc_invoice_date { get; set; }
        public long? PaymentTypeId { get; set; }
        public decimal? Amount { get; set; }
        public string Comments { get; set; }
        public string Attachment { get; set; }
        public string InvoiceFileId { get; set; }
        public string ClientName { get; set; }
        public string ContactName { get; set; }
        public string InvoiceStatus { get; set; }
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public string subject { get; set; }
        public string email { get; set; }
        public string description { get; set; }
        public string vc_access_link { get; set; }
        public string vc_project_manager_email { get; set; }
        public string vc_financial_email { get; set; }
        public string vc_sent_to { get; set; }
        public string vc_sent_by { get; set; }
        public long? UserId { get; set; }
    }
    public class InvoicePaymentHistory
    {
        public string vc_posted_date { get; set; }
        public string vc_posted_by { get; set; }
        public string vc_payment_type { get; set; }
        public decimal? Amount { get; set; }
        public string Comments { get; set; }
        public string vc_access_link { get; set; }
    }
    public class InvoicePaymentdetailsandPaymentHistory
    {
        public InvoicepaymentDetails InvoicepaymentDetails { get; set; }
        public List<InvoicePaymentHistory> InvoicePaymentHistory { get; set; }
    }
}