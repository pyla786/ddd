using API_IBW.DB_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_IBW.Models
{
    public static class Logging
    {

        public static void EventLog(long? userId, string strEvent)
        {
            using (AdminDataContext adminDB = new AdminDataContext())
            {
                long? result = 0;
                adminDB.InsertEvent(userId, strEvent, ref result);
            }
            
        }
        public static void ErrorLog(long? userId, string error, string url)
        {
            using (AdminDataContext adminDB = new AdminDataContext())
            {
                long? result = 0;
                adminDB.InsertErrors(userId, error, url, ref result);
            }
        }
    }
}