using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace API_IBW.HelperMethods
{
    public class Mails
    {
        public async Task<bool> SendMail(string fromAddress, string toAddress, string userName, string body, string subject)
        {
            //var apiKey = Literals.SendGridApiKey;
            //var client = new SendGridClient(apiKey);
            //string senderEmail = string.IsNullOrEmpty(fromAddress) ? Literals.EmailSendingFrom : fromAddress;
            //string senderName = Literals.EmailSenderName;
            //var from = new EmailAddress(senderEmail, senderName);
            //var to = new EmailAddress(toAddress, userName);
            //var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);
            //var response = await client.SendEmailAsync(msg);
            //if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            //    return true;
            //else
            //    return false;

            string sendGridApi = Literals.SendGridApiKey;
            SendGridAPIClient sg = new SendGridAPIClient(sendGridApi);
            string senderEmail = string.IsNullOrEmpty(fromAddress) ? Literals.EmailSendingFrom : fromAddress;
            string senderName = Literals.EmailSenderName;
            Email from = new Email(senderEmail, senderName);
            Email to = new Email(toAddress, userName);
            Content content = new Content("text/html", body.ToString());
            Mail mail = new Mail(from, subject, to, content);
            SendGrid.CSharp.HTTP.Client.Response response = await sg.client.mail.send.post(requestBody: mail.Get());
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                return true;
            }
            else
            {
                int ErrorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                var Request = System.Web.HttpContext.Current.Request;
                return false;
            }
        }
        public bool SendInvoice(string subject, string body, string description, List<string> toEmail, string FileBase64String, string Fromemail, string Filename, List<Email> CCs)
        {
            try
            {
                long emailsentcount = 0;
                bool issucceeded = false;
                List<string> uniqueemails = new List<string>();
                string sendGridApi = Literals.SendGridApiKey;
                string outgoingEmail = Fromemail;
                dynamic sg = new SendGridAPIClient(sendGridApi);
                Email from = new Email(outgoingEmail);
                Content bodycontent;
                if (!string.IsNullOrEmpty(description))
                    bodycontent = new Content("text/html", body.ToString() + "<br/><br/>" + description);
                else
                    bodycontent = new Content("text/html", body.ToString());
                string Contenttype = "application/pdf";
                var attachment = new Attachment
                {
                    Filename = Filename,
                    Type = Contenttype,
                    Content = FileBase64String
                };
                foreach(var toemail in toEmail)
                {
                    Email to = new Email(toemail);
                    Mail mail = new Mail(from, subject, to, bodycontent);
                    mail.AddAttachment(attachment);
                    if (CCs != null && CCs.Count() > 0)
                    {
                        uniqueemails.AddRange(toEmail);
                        var personalization = new Personalization();
                        for (var i = 0; i < CCs.Count(); i++)
                        {
                            if (!uniqueemails.Contains(CCs[i].Address))
                            {
                                uniqueemails.Add(CCs[i].Address);
                                mail.Personalization[0].AddCc(CCs[i]);
                            }
                        }
                    }
                    SendGrid.CSharp.HTTP.Client.Response response = sg.client.mail.send.post(requestBody: mail.Get());
                    if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                    {
                        emailsentcount++;
                    }
                    else
                    {
                        int ErrorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                        var Request = System.Web.HttpContext.Current.Request;
                    }
                }
                if (emailsentcount == toEmail.Count())
                    issucceeded = true;
                return issucceeded;
            }
            catch (Exception ex)
            {
                int ErrorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                var Request = System.Web.HttpContext.Current.Request;
                //Methods.Errorlog(ex.Message, "Send Grid", ErrorCode, Request.Url.AbsolutePath, null, userTypeId);
                return false;
            }
        }


    }
}