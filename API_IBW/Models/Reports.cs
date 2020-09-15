using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_IBW.Models
{
    public class PayrollBiWeekly
    {
        public List<IBWUsers> Users { get; set; }
        public List<string> Headers { get; set; }
        public List<string> Summary { get; set; }
        public List<UserPayroll> UserPayrolls { get; set; }
        public List<IBWUsers> ReportingManagers { get; set; }
    }

    public class UserPayroll
    {
        public long? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Regular { get; set; }
        public List<string> Holiday { get; set; }
        public List<string> Leave { get; set; }
        public List<string> Total { get; set; }
    }
}