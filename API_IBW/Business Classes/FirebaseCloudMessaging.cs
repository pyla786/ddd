using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static API_IBW.Models.FCMModels;

namespace API_IBW.Business_Classes
{
    public class FirebaseCloudMessaging
    {
        //public static async Task SendPushNotifications(string[] deviceTokens, string Title, string Body)
        //{
        //    var messageInformation = new Message()
        //    {
        //        notification = new Notification()
        //        {
        //            title = Title,
        //            text = Body
        //        },
        //        registration_ids = deviceTokens
        //    };
        //    //Object to JSON STRUCTURE => using Newtonsoft.Json;
        //    string jsonMessage = JsonConvert.SerializeObject(messageInformation);

        //    var request = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send");
        //    request.Headers.TryAddWithoutValidation("Authorization", "key =AAAAnNurrBQ:APA91bF3VlPDx-_nECTwP7ExzkzEwT4FkpX8k86JaJpARZ6hDTx91zKWHNj9lJ9Rsut5PfuHEkFldHqEsMj9c90S-t220R4wjHmwXhov8rYV1CAVEStP9SVYygWs_kYvp1MXrDK9d4I0");
        //    request.Content = new StringContent(jsonMessage, Encoding.UTF8, "application / json");
        //    HttpResponseMessage result;
        //    using (var client = new HttpClient())
        //    {
        //        result = await client.SendAsync(request);
        //    }
        //}

        public static long? SendNotification(long? userId, string messageTitle, string messageBody, string routerLink)
        {
            try
            {
                DB_Models.AdminDataContext _adminDB = new DB_Models.AdminDataContext();
                string SERVER_API_KEY = "AAAAnNurrBQ:APA91bF3VlPDx-_nECTwP7ExzkzEwT4FkpX8k86JaJpARZ6hDTx91zKWHNj9lJ9Rsut5PfuHEkFldHqEsMj9c90S-t220R4wjHmwXhov8rYV1CAVEStP9SVYygWs_kYvp1MXrDK9d4I0";
                var SENDER_ID = "673700359188";
                var value = new Notification
                {
                    title = messageTitle,
                    text = messageBody,
                    routerLink = routerLink
                };
                string values = JsonConvert.SerializeObject(value);

                List<string> userFCMTokens = _adminDB.tbl_ibw_user_fcm_keys.Where(x => x.int_user_id == userId).Select(y => y.vc_fcm_key).ToList();
                string sResponseFromServer = "";
                foreach (var item in userFCMTokens)
                {
                    WebRequest tRequest;
                    tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                    tRequest.Method = "post";
                    tRequest.ContentType = " application/x-www-form-urlencoded;charset=UTF-8";
                    tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));
                    tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
                    string postData = "collapse_key=score_update&time_to_live=108&delay_while_idle=1&data.message=" + values + "&data.time=" + System.DateTime.Now.ToString() + "&registration_id=" + item + "";
                    Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                    tRequest.ContentLength = byteArray.Length;
                    Stream dataStream = tRequest.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                    WebResponse tResponse = tRequest.GetResponse();
                    dataStream = tResponse.GetResponseStream();
                    StreamReader tReader = new StreamReader(dataStream);
                    sResponseFromServer = tReader.ReadToEnd();
                    tReader.Close();
                    dataStream.Close();
                    tResponse.Close();
                }
                long? alertResult = 0;
                _adminDB.insertAlerts(userId, messageTitle, messageBody, routerLink, ref alertResult);
                _adminDB.Dispose();
                return alertResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<Notification> getNotifcations(long? userId)
        {
            try
            {
                DB_Models.AdminDataContext _adminDB = new DB_Models.AdminDataContext();
                List<Notification> notificationsList = _adminDB.tbl_ibw_alerts.Where(x => x.int_user_id == userId && x.bt_is_read == false).Select(y => new Notification
                {
                    title = y.vc_alert_title,
                    text = y.vc_message,
                    routerLink = y.vc_router_link,
                    alertDate = y.dt_created_on
                }).ToList();
                return notificationsList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}