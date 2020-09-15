using API_IBW.DB_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_IBW.Models
{
    public class UserPermissions
    {
        public long? roleMasterId { get; set; }
        public string roleMasterName { get; set; }
        public bool? status { get; set; }
        public long? usersCount { get; set; }
        public long? screensCount { get; set; }
        public long? totalScreensCount { get; set; }
        public long userId { get; set; }
        public long? createdBy { get; set; }
        public long? modifiedBy { get; set; }
        public List<sp_getAllUsersOfRoleResult> usersList { get; set; }
    }
    public class screenPermissions
    {
        public long? roleMasterId { get; set; }
        public string roleMasterName { get; set; }
        public bool? status { get; set; }
        public string screenName { get; set; }
        public string screenNameText { get; set; }
        public long? screenId { get; set; }
        public bool? isRead { get; set; }
        public bool? isWrite { get; set; }
        public bool? isDeletePermission { get; set; }
        public bool? isApproval { get; set; }
        public bool? readAll { get; set; }
        public bool? writeAll { get; set; }
        public bool? deleteAll { get; set; }
        public bool? approveAll { get; set; }
    }

    public class ScreenPermissionsList
    {
        public List<screenPermissions> screenPermissionList { get; set; }
    }

}