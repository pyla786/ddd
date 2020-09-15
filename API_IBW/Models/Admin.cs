using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_IBW.Models
{
    public class SettingsCountries
    {
        public long? int_country_id { get; set; }
        public string vc_country_name { get; set; }
        public List<States> states { get; set; }
    }
    public class FCMTokenModel
    {
        public long? UserID { get; set; }
        public string FCMToken { get; set; }
        public List<string> FCMTokens { get; set; }
        //For Primary Key Id in Table
        public long FCMTokenID { get; set; }
    }
    public class Settings
    {
        public long? SettingId { get; set; }
        public string SettingName { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
        public string InputType { get; set; }
    }
    
    public class ConfigureKanban
    {
        public long? ConfigureKanbanId { get; set; }
        public string KanbanStageName { get; set; }
        public string KanbanTypeName { get; set; }
        public bool? IsActive { get; set; }
        public long? UserId { get; set; }
        public bool IsSystem { get; set; }
        public long? OptionCount { get; set; }
        public string ColorCode { get; set; }
        public long? StageAssignedCount { get; set; }
    }
    public class CodeMaster
    {
        public long? CodeMasterId { get; set; }
        public string CodeMasterName { get; set; }
    }
    public class LookupOptions: CodeMaster
    {
        public long? LookupId { get; set; }
        public string LookupName { get; set; }
        public bool? IsSystem { get; set; }
    }
    public class LookupModel
    {
        public List<LookupOptions> LookupOptions { get; set; }
        public List<CodeMaster> CodeMaster { get; set; }
    }
}