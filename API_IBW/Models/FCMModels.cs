using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_IBW.Models
{
    public class FCMModels
    {
        public class Message
        {
            public string[] registration_ids { get; set; }
            public string FCMToken { get; set; }
            public Notification notification { get; set; }
            public object data { get; set; }
        }
        public class Notification
        {
            public string title { get; set; }
            public string text { get; set; }
            public string routerLink { get; set; }
            public DateTime alertDate { get; set; }
        }
    }
}