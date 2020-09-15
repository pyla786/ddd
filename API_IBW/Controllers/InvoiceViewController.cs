using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using API_IBW.Models;
using API_IBW.Business_Classes;
using Rotativa;
using Rotativa.Options;
using System.Globalization;
using API_IBW.DB_Models;
using System.Configuration;

using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using System.Net;
using SendGrid.Helpers.Mail;

namespace API_IBW.Controllers
{
    public class InvoiceViewController : Controller
    {
        // GET: InvoiceView
        public ActionResult Invoice(long? invoiceId)

        {
            Invoice invoiceMethods = new Invoice();
            Admin adminMethods = new Admin();
            Quotes quotesMethods = new Quotes();
            List<getinvoicefeedetailsResult> feedetails= invoiceMethods.GetInvoiceFeeDetails(invoiceId).ToList();
            var quotedetails = (feedetails!=null && feedetails.Count()>0) ? quotesMethods.GetQuoteDetails(feedetails.Select(x=>x.int_quote_id).FirstOrDefault()).Quote:null;
            var clientdetails = (feedetails != null && feedetails.Count() > 0) ? invoiceMethods.GetClientDetailsbyClientId(feedetails.Select(x => x.int_client_id).FirstOrDefault()) : null;
            ViewBag.feedetails = feedetails;
            string TandC = invoiceMethods.GetInvoiceTandC().termsandconditions;
            var SettingsDetails = adminMethods.GetSettings().ToList();
            
            string GSTNo = SettingsDetails.Where(x => x.SettingName == "GST No.").Select(x => x.Value).FirstOrDefault();
            string Terms = SettingsDetails.Where(x => x.SettingName == "Terms (in Days)").Select(x => x.Value).FirstOrDefault();
            decimal? HSTpercent = feedetails.Select(x=>x.dec_HST_percent).FirstOrDefault();
            decimal invoiceamount = (feedetails != null && feedetails.Count() > 0)? Convert.ToDecimal(feedetails.Select(x => x.dec_invoice_total).FirstOrDefault()):0;
            TempData["ClientName"] = clientdetails != null ? clientdetails.vc_client_name : "-";
            TempData["ClientAddress"] = clientdetails != null ? clientdetails.vc_site_street_address : "-";
            TempData["ClientCity"] = clientdetails != null ? clientdetails.vc_city : "-";
            TempData["ClientProvince"] = clientdetails != null ? clientdetails.vc_State_Name : "-";
            TempData["ClientPostalCode"] = clientdetails != null ? clientdetails.vc_zip_or_postal : "-";
            TempData["ContactName"] = (feedetails != null && feedetails.Count() > 0 && feedetails.Select(x => x.vc_contact_name).FirstOrDefault()!=null) ? feedetails.Select(x => x.vc_contact_name).FirstOrDefault() : "-";
            TempData["InvoiceDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            TempData["InvoiceNumber"] = (feedetails != null && feedetails.Count() > 0) ? string.IsNullOrEmpty(feedetails.Select(x => x.vc_manual_invoice_number).FirstOrDefault())? feedetails.Select(x => x.vc_invoice_number).FirstOrDefault(): feedetails.Select(x => x.vc_manual_invoice_number).FirstOrDefault() : "-";
            TempData["ManualInvoiceNumber"] = (feedetails != null && feedetails.Count() > 0) ? string.IsNullOrEmpty(feedetails.Select(x => x.vc_manual_invoice_number).FirstOrDefault()) ? feedetails.Select(x => x.vc_invoice_number).FirstOrDefault() : feedetails.Select(x => x.vc_invoice_number).FirstOrDefault() + " | "+feedetails.Select(x => x.vc_manual_invoice_number).FirstOrDefault() : "-";
            TempData["PurchaseOrderNumber"] = (feedetails != null && feedetails.Count() > 0 && feedetails.Select(x => x.vc_purchase_order_number).FirstOrDefault() != null) ? feedetails.Select(x => x.vc_purchase_order_number).FirstOrDefault() : "-";
            TempData["InvoiceAmount"] = string.Format(new CultureInfo("en-US"), "{0:c}", invoiceamount);
            TempData["GSTorHSTNo."] = string.IsNullOrEmpty(GSTNo) ? "-" : GSTNo;
            TempData["Terms"] = string.IsNullOrEmpty(Terms) ? "-" : Terms;
            TempData["HSTPercent"] = HSTpercent==null ? Convert.ToDecimal(0.00) : Convert.ToDecimal(HSTpercent);
            TempData["HSTTotal"] = string.Format(new CultureInfo("en-US"), "{0:c}", Convert.ToDecimal(Convert.ToDecimal(invoiceamount / 100) * Convert.ToDecimal(HSTpercent)));
            TempData["Total"] = string.Format(new CultureInfo("en-US"), "{0:c}", Convert.ToDecimal(invoiceamount + ((invoiceamount / 100) * Convert.ToDecimal(HSTpercent))));
            CultureInfo ci = new CultureInfo("en-US");
            ci.NumberFormat.CurrencyNegativePattern = 1;
            TempData["PaymentorCredits"] = string.Format(ci, "{0:c}", feedetails.Select(x=>x.dec_balance_amount).FirstOrDefault());
            TempData["BalanceDue"] = string.Format(new CultureInfo("en-US"), "{0:c}", ((invoiceamount + Convert.ToDecimal(Convert.ToDecimal(invoiceamount / 100) * Convert.ToDecimal(HSTpercent))))-feedetails.Select(x=>x.dec_balance_amount).FirstOrDefault());
            TempData["TermsandConditions"] = string.IsNullOrEmpty(TandC) ?"-": TandC;
            TempData["ProjectDetails"] = quotedetails != null ? quotedetails.quoteNumber + " (" + quotedetails.projectStreetAddress + ", " + quotedetails.projectCity + ")":"-";
            TempData["Services"] = (feedetails != null && feedetails.Count() > 0) ? feedetails.Select(x => x.vc_description).FirstOrDefault() : "-";
            
            return View();
        }

        //To render header for pdf
        [AllowAnonymous]
        public ActionResult Invoicepdfheader()
        {
            return View();
        }
        //To render footer for pdf
        [AllowAnonymous]
        public ActionResult Invoicepdffooter()
        {
            Admin adminMethods = new Admin();
            var SettingsDetails = adminMethods.GetSettings().ToList();
            string company = SettingsDetails.Where(x => x.SettingName == "Company Name").Select(x => x.Value).FirstOrDefault();
            string address = SettingsDetails.Where(x => x.SettingName == "Address").Select(x => x.Value).FirstOrDefault();
            string city = SettingsDetails.Where(x => x.SettingName == "City").Select(x => x.Value).FirstOrDefault();
            string province = SettingsDetails.Where(x => x.SettingName == "Province").Select(x => x.Value).FirstOrDefault();
            string postalcode = SettingsDetails.Where(x => x.SettingName == "Postal Code").Select(x => x.Value).FirstOrDefault();
            string tollfree = SettingsDetails.Where(x => x.SettingName == "Toll Free").Select(x => x.Value).FirstOrDefault();
            string phone = SettingsDetails.Where(x => x.SettingName == "Regular Phone Number").Select(x => x.Value).FirstOrDefault();
            string fax = SettingsDetails.Where(x => x.SettingName == "Fax Number").Select(x => x.Value).FirstOrDefault();
            string financeemail = SettingsDetails.Where(x => x.SettingName == "Finance Email").Select(x => x.Value).FirstOrDefault();
            TempData["Company"] = string.IsNullOrEmpty(company) ? "-" : company;
            TempData["Address"] = string.IsNullOrEmpty(address) ? "-" : string.Join(" ", address.Reverse().Reverse());
            TempData["City"] = string.IsNullOrEmpty(city) ? "-" : string.Join(" ", city.Reverse().Reverse());
            TempData["Province"] = string.IsNullOrEmpty(province) ? "-" : string.Join(" ", province.Reverse().Reverse());
            TempData["PostalCode"] = string.IsNullOrEmpty(postalcode) ? "-" : string.Join(" ", postalcode.Reverse().Reverse());
            TempData["TollFree"] = string.IsNullOrEmpty(tollfree) ? "-" : string.Join(" ", tollfree.Reverse().Reverse()).Replace('-', '.');
            TempData["PhoneNo"] = string.IsNullOrEmpty(phone) ? "-" : string.Join(" ", phone.Reverse().Reverse()).Replace('-', '.');
            TempData["Fax"] = string.IsNullOrEmpty(fax) ? "-" : string.Join(" ", fax.Reverse().Reverse()).Replace('-', '.');
            TempData["FinanceEmail"] = string.IsNullOrEmpty(financeemail) ? "-" : string.Join(" ", financeemail.Reverse().Reverse());
            return View();
        }

        public Byte[] getinvoicePDFdatainbytes(long? invoiceid)
        {
            string authority = System.Web.HttpContext.Current.Request.Url.Authority;
            var url = "http://" + authority + "/InvoiceView/Invoice?invoiceId=" + invoiceid;
            //var url = "http://localhost:56113/InvoiceView/Invoice?invoiceId="+invoiceid;
            string customSwitches = string.Format("--footer-html  \"{0}\" " + "--header-html \"{1}\" ", "http://"+ authority +"/InvoiceView/Invoicepdffooter", "http://"+ authority +"/InvoiceView/Invoicepdfheader");
            var actionResult = new Rotativa.UrlAsPdf(url)
            {
                FileName = "invoice_pdf",
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                PageSize = Rotativa.Options.Size.A4,
                PageMargins = new Margins(25, 5, 25, 5),
                CustomSwitches = customSwitches
            };
            byte[] invoicePDFData = actionResult.BuildPdf(this.ControllerContext);
            return invoicePDFData;
        }
    }
}