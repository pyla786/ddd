using API_IBW.DB_Models;
using API_IBW.HelperMethods;
using API_IBW.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace API_IBW.Business_Classes
{
    public class Admin
    {
        private AdminDataContext _adminDB;
        private AdminExtensionDataContext _adminExtensionDB;
        private QuotesDataContext _quotesDB;
        private SchedulingDataContext _schDB;
        public Admin()
        {
            _adminDB = new AdminDataContext();
            _quotesDB = new QuotesDataContext();
            _schDB = new SchedulingDataContext();
            _adminExtensionDB = new AdminExtensionDataContext();
        }
        
        #region sree
        public long? upsertandDeleteActionsMaster(ActionsMaster model)

        {
            try
            {
                long? result = 0;
                _adminDB.upsertDeleteActionMaster(model.actionMasterId, model.actionMasterName, model.jobCodeId, model.createdBy, model.modifiedBy, model.status, model.isDeleted, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<ActionsMaster> getAllActionsInMasterData()
        {
            try
            {
                List<ActionsMaster> actionsList = new List<ActionsMaster>();
                actionsList = _adminDB.getAllActionsInMasterData().Where(x => x.bt_delete == false).Select(x =>
                      new ActionsMaster
                      {
                          actionMasterId = x.int_action_master_id,
                          actionMasterName = x.vc_action_master_name,
                          jobCodeId = x.int_job_code_id,
                          jobCode = x.vc_job_code,
                          jobCodeTitle = x.vc_job_title,
                          jobCodeRate = x.dec_chargeout_rate_per_hr,
                          status = x.bt_status,
                          projectsCount = _adminDB.getCountOfQuotesForActions(x.int_action_master_id).Where(y => y.bt_project == true).Count(),
                          quotesCount = _adminDB.getCountOfQuotesForActions(x.int_action_master_id).Where(y => y.bt_project == false).Count()
                      }).ToList();
                return actionsList.OrderBy(x => x.actionMasterName).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Sreebharath 
        //Date : 19-08-2019
        //Get all the Screen Permissions
        public List<screenPermissions> GetScreenPermissions(long roleId)
        {
            try
            {
                var screenPermissions = _adminDB.sp_getAllScreenPermissions().Where(x => x.int_role_master_id == roleId).ToList();
                var screenPermissionsEmpty = _adminDB.tbl_ibw_screen_masters.ToList();
                List<screenPermissions> screenPermissionsList = new List<screenPermissions>();

                foreach (var item in screenPermissionsEmpty)
                {
                    screenPermissions screenPermission = new screenPermissions();
                    screenPermission.screenId = item.int_screen_id;
                    screenPermission.screenName = item.vc_screen_name;
                    screenPermission.screenNameText = item.vc_screen_name_text;
                    screenPermission.isRead = item.bt_read;
                    screenPermission.isWrite = item.bt_write;
                    screenPermission.isDeletePermission = item.bt_delete_permission;
                    screenPermission.isApproval = item.bt_approvals;
                    screenPermission.roleMasterId = roleId;
                    foreach (var item2 in screenPermissions)
                    {
                        if (item.int_screen_id == item2.int_screen_id)
                        {
                            if (item2.RightName == "Read")
                            {
                                screenPermission.readAll = true;
                            }
                            if (item2.RightName == "Write")
                            {
                                screenPermission.writeAll = true;
                            }
                            if (item2.RightName == "Delete")
                            {
                                screenPermission.deleteAll = true;
                            }
                            if (item2.RightName == "Approvals")
                            {
                                screenPermission.approveAll = true;
                            }
                        }
                    }
                    screenPermissionsList.Add(screenPermission);
                }

                return screenPermissionsList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<IBWUsers> getAllReportingManagers()
        {
            try
            {
                return _adminDB.sp_getAllReportingManagers().Select(x => new IBWUsers { userId = x.int_user_id, aliasName = x.vc_alias_name, userName = x.vc_user_name }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<IBWUsers> getAllProjectManagers()
        {
            try
            {
                return _adminDB.sp_getAllProjectManagers().Select(x => new IBWUsers { userId = x.int_user_id, aliasName = x.vc_alias_name, userName = x.vc_user_name }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<IBWUsers> getProjectManagersAndSuperAdmin()
        {
            try
            {
                return _adminDB.sp_getSuperAdminsAndPMs().Select(x => new IBWUsers { userId = x.int_user_id, aliasName = x.vc_alias_name, userName = x.vc_user_name }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<IBWUsers> getReportingManagersAndSuperAdmin()
        {
            try
            {
                return _adminDB.sp_getSuperAdminsAndReportingManagers().Select(x => new IBWUsers { userId = x.int_user_id, aliasName = x.vc_alias_name, userName = x.vc_user_name + " " + x.vc_alias_name }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public long? upsertUser(IBWUsers model)
        {
            try
            {
                long? result = null;
                //Initially setting the job codes string to empty string
                model.jobCodesStringSeperated = "";
                //Initially setting the permission role string to empty string
                model.permissionLevelStringSeperated = "";
                // If roles exists then merging all the permission roles with role 
                if (model.permissionRoleIds.Count > 0)
                {
                    model.permissionLevelStringSeperated = string.Join("#", model.permissionRoleIds);
                    model.permissionLevelStringSeperated = model.permissionLevelStringSeperated + "#";
                }
                // If Job Codes exists then merging all the job codes with # seperator
                if (model.jobCodes.Count > 0)
                {
                    model.jobCodesStringSeperated = string.Join("#", model.jobCodes);
                    model.jobCodesStringSeperated = model.jobCodesStringSeperated + "#";
                }
                //Sending Parameters to User Upsert Stored Procedure
                _adminDB.sp_upsertIBWUsers(model.userId, model.userName, model.aliasName, model.email, model.phoneNumber,
                                                     model.employmentTypeId, model.reportingManagerId, model.eligibleVacationDays,
                                                     model.openingLeaveBalance, model.hoursPerDay, model.jobCodesStringSeperated, model.permissionLevelStringSeperated, model.createdBy, model.modifiedBy,model.startDate, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public long? upsertPotentialLevel(PotentialLevel model)
        {
            try
            {
                long? result = null;
                _adminDB.sp_upsertPotentialLevels(model.potentialLevelId, model.potentialLevelName, model.colorCode, model.status, model.createdBy, model.modifiedBy, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public long? insertScreenPermissions(ScreenPermissionsList model)
        {
            try
            {
                long? result = null;
                bool? deleteResult = null;

                foreach (var item in model.screenPermissionList)
                {
                    deleteResult = deleteScreenPermissions(item.roleMasterId);
                }
                foreach (var item in model.screenPermissionList)
                {
                    if (deleteResult == true)
                    {
                        if (item.readAll == true || item.writeAll == true || item.deleteAll == true || item.approveAll == true)
                        {
                            _adminDB.sp_insertScreenPermissions(item.roleMasterId, item.screenId, item.readAll, item.writeAll, item.deleteAll, item.approveAll, ref result);
                        }

                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public bool? deleteScreenPermissions(long? roleMasterId)
        {
            try
            {
                bool? result = null;
                _adminDB.sp_deleteScreenPermissions(roleMasterId, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //Author : Sreebharath 
        //Date : 19-08-2019
        //To upsert all users of a role
        public long? upsertUsersOfRole(UserPermissions model)
        {
            try
            {
                long? result = null;
                string usersListString = "";
                if (model.usersList.Count > 0)
                {
                    usersListString = string.Join("#", model.usersList);
                    usersListString = usersListString + "#";
                }
                _adminDB.sp_userRoleUpsert(usersListString, model.roleMasterId, model.modifiedBy, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //Author : Sreebharath 
        //Date : 07-08-2019
        //Get all the IBW Users
        public List<IBWUsers> GetUsers()
        {
            try
            {
                var usersList = _adminDB.sp_getAllUsersExceptSuperAdmins().ToList();
                List<IBWUsers> userData = new List<IBWUsers>();
                foreach (var item in usersList)
                {
                    IBWUsers eachUserData = new IBWUsers();
                    eachUserData.userId = item.int_user_id;
                    eachUserData.userName = item.vc_user_name;
                    eachUserData.aliasName = item.vc_alias_name;
                    eachUserData.email = item.vc_email;
                    eachUserData.phoneNumber = item.vc_phone_number;
                    eachUserData.employmentTypeId = item.int_employment_type_id;
                    eachUserData.reportingManagerId = item.int_reporting_manager_id;
                    eachUserData.isVerified = item.bt_verified;
                    eachUserData.reportingManagerName = item.ReportingManagerName;
                    // eachUserData.permissionLevelName = _adminDB.sp_getRoleOfUser().Where(x => x.int_user_id == item.int_user_id && x.bt_delete == false).Select(y => y.vc_role_name).FirstOrDefault();
                    eachUserData.jobShortCode = _adminDB.sp_getJobCodesOfUser().Where(x => x.int_user_id == item.int_user_id && x.bt_delete == false).Select(y => " " + y.vc_job_code).ToList();
                    eachUserData.jobCodeSet = _adminDB.sp_getJobCodesOfUser().Where(x => x.int_user_id == item.int_user_id && x.bt_delete == false).Select(y => new AllJobCodes { jobCode = y.vc_job_code, jobCodeId = y.int_job_code_master_id, jobCodeTitle = y.vc_job_title }).ToList();
                    eachUserData.PermissionRoles = _adminDB.sp_getRoleOfUser().Where(x => x.int_user_id == item.int_user_id && x.bt_delete == false).Select(y => new UserRoles { roleMasterId = y.int_role_master_id, roleName = y.vc_role_name }).ToList();
                    eachUserData.eligibleVacationDays = item.int_eligible_vacation_days;
                    eachUserData.hoursPerDay = item.int_hrs_per_day;
                    eachUserData.openingLeaveBalance = item.int_opening_leave_balance;
                    eachUserData.status = item.bt_status;
                    eachUserData.isSuperAdmin = item.bt_superadmin;
                    eachUserData.totalLeaveBalance = item.int_total_leave_balance;
                    eachUserData.ptoLeaveBalance = item.int_pto_leave_balance;
                    eachUserData.startDate = item.strStartDate;
                    eachUserData.leavesCount = item.int_leaves_count;
                    eachUserData.schActionsCount = item.int_sch_actions_count;
                    eachUserData.timesheetsCount = item.int_timesheets_count;
                    eachUserData.asRMCount = item.int_as_rm_count;
                    eachUserData.asPMAMCount = item.int_as_pm_am_count;
                    userData.Add(eachUserData);

                }
                return userData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //Author : Sreebharath 
        //Date : 22-08-2019
        //Get all Potential Levels
        public List<PotentialLevel> GetAllPotentialLevels()
        {
            try
            {
                var potentialLevelsList = _adminDB.sp_getAllPotentialLevels().ToList();
                List<PotentialLevel> potentialLevels = new List<PotentialLevel>();
                foreach (var item in potentialLevelsList)
                {
                    PotentialLevel eachPotentialLevel = new PotentialLevel();
                    eachPotentialLevel.potentialLevelId = item.int_RFQ_potential_level_id;
                    eachPotentialLevel.potentialLevelName = item.vc_potential_level;
                    eachPotentialLevel.colorCode = item.vc_color_code;
                    eachPotentialLevel.status = item.bt_status;
                    potentialLevels.Add(eachPotentialLevel);
                }
                return potentialLevels;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //Author : Sreebharath 
        //Date : 22-08-1996
        //Get IBW Municpalities
        public List<Municipalities> GetMunicipalities()
        {
            try
            {
                var municipalityList = _adminDB.sp_getIBWMuncipalities().ToList();
                List<Municipalities> MunicipalitiesList = new List<Municipalities>();
                foreach (var item in municipalityList)
                {
                    Municipalities eachMunicipality = new Municipalities();
                    eachMunicipality.municipalityId = item.int_municipality_id;
                    eachMunicipality.municipalityName = item.vc_municipality_name;
                    eachMunicipality.status = item.bt_status;
                    MunicipalitiesList.Add(eachMunicipality);
                }
                return MunicipalitiesList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //Author : Sreebharath
        //Date : 16-08-2019
        //To Update and Insert Permission Roles
        public long? upsertPermissionRole(UserPermissions model)
        {
            try
            {
                long? result = 0;
                _adminDB.sp_permissionRolesUpsert(model.roleMasterId, model.roleMasterName, model.status, ref result);

                return result;
            }
            catch (Exception ex) { throw ex; }
        }


        //Author : Sreebharath 
        //Date : 17-08-2019
        //To delete permission role
        public long? deletePermissionRole(UserPermissions model)
        {
            try
            {
                long? result = null;
                _adminDB.sp_deleteUserRoles(model.roleMasterId, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Sreebharath 
        //Date : 22-08-2019
        //To delete Municipality
        public long? deleteMunicipality(Municipalities model)
        {
            try
            {
                long? result = null;
                _adminDB.sp_deleteMunicipality(model.municipalityId, model.modifiedBy, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Sreebharath 
        //Date : 22-08-2019
        //To delete Potential Level
        public long? deletePotentialLevel(PotentialLevel model)
        {
            try
            {
                long? result = null;
                _adminDB.sp_deletePotential(model.potentialLevelId, model.modifiedBy, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Sreebharath 
        //Date : 17-08-2019
        //Get All User Roles With Total screen and Users Count
        public List<UserPermissions> GetAllRolesWithCount()
        {
            try
            {
                var AllUserRoles = _adminDB.sp_getAllUserRolesWithCount().Where(x => x.bt_delete == false).ToList();
                List<UserPermissions> permissionsList = new List<UserPermissions>();
                foreach (var item in AllUserRoles)
                {
                    UserPermissions eachUserPermission = new UserPermissions();
                    eachUserPermission.roleMasterId = item.int_role_master_id;
                    eachUserPermission.roleMasterName = item.vc_role_name;
                    eachUserPermission.screensCount = item.ScreensCount;
                    eachUserPermission.totalScreensCount = item.TotalScreensCount;
                    eachUserPermission.usersCount = item.NumberOfUsers;
                    eachUserPermission.usersList = _adminDB.sp_getAllUsersOfRole().Where(x => x.bt_delete == false && x.int_role_master_id == item.int_role_master_id).ToList();
                    eachUserPermission.status = item.bt_status;
                    permissionsList.Add(eachUserPermission);
                }
                return permissionsList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        //Author : Sreebharath 
        //Date : 18-08-2019
        //permission name validation
        public bool permissionNameValidation(string roleName, long? roleId)
        {
            try
            {
                List<tbl_ibw_role_master> allRolesList;
                if (roleId == null)
                {
                    allRolesList = _adminDB.tbl_ibw_role_masters.Where(x => x.vc_role_name == roleName && x.bt_delete == false).ToList();
                }
                else
                {
                    allRolesList = _adminDB.tbl_ibw_role_masters.Where(x => x.vc_role_name == roleName && x.int_role_master_id != roleId && x.bt_delete == false).ToList();
                }
                if (allRolesList.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;

                }
            }
            catch { throw; }
        }


        public long? changeStatusAndDelete(IBWUsers model)
        {
            try
            {
                long? result = null;
                var data = _adminDB.sp_changeStatusDeleteUser(model.userId, model.isDelete, model.status, model.modifiedBy, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public long? UserSuperAdminStatusChange(IBWUsers model)
        {
            try
            {
                long? result = 0;
                var data = _adminDB.sp_userSuperAdminStatusChange(model.userId, model.isSuperAdmin, model.modifiedBy, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public long? ChangeUserMode(long? userId, bool? isSuperAdminMode)
        {
            try
            {
                long? result = 0;
                var data = _adminDB.sp_userModeChange(userId, isSuperAdminMode, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public long GetUserLeaveCount(long? userId)
        {
            try
            {
                long result = 0;
                result = _adminDB.sp_getuserleaverequest(userId).Where(x => x.LeaveType == "PTO" || x.LeaveType == "Full Day" || x.LeaveType == "Multiple Days").Count();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool checkVerified(long? userId)
        {
            try
            {

                bool? isUserVerified = _adminDB.sp_getAllIBWUsers().Where(x => x.int_user_id == userId).Select(y => y.bt_verified).FirstOrDefault();
                if (isUserVerified == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Sreebharath 
        //Date : 08-08-2019
        //Get User Roles from IBW User Roles
        public List<UserRoles> GetUserRoles()
        {
            try
            {
                var ListOfRolesRaw = _adminDB.tbl_ibw_role_masters.Where(x => x.bt_delete == false).ToList();
                List<UserRoles> userRolesList = new List<UserRoles>();
                foreach (var item in ListOfRolesRaw)
                {
                    UserRoles eachUserRole = new UserRoles();
                    eachUserRole.roleMasterId = item.int_role_master_id;
                    eachUserRole.roleName = item.vc_role_name;
                    eachUserRole.status = item.bt_status;
                    userRolesList.Add(eachUserRole);
                }
                return userRolesList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Sreebharath 
        //Date : 08-08-2019
        //Get Employee Types from IBW Users
        public List<EmployeeTypes> GetEmployeeTypes()
        {
            try
            {
                //var ListOfEmployeeTypes = _adminDB.tbl_ibw_employee_types.Where(x => x.bt_delete == false).ToList();
                var ListOfEmployeeTypes = _adminDB.GetLookupOptions().Where(x => x.int_code_master_id == 1).ToList();
                List<EmployeeTypes> employeeTypesList = new List<EmployeeTypes>();
                foreach (var item in ListOfEmployeeTypes)
                {
                    EmployeeTypes eachEmployeeType = new EmployeeTypes();
                    eachEmployeeType.employeeTypeId = item.int_lookup_id;
                    eachEmployeeType.employeeTypeName = item.vc_lookup_name;
                    employeeTypesList.Add(eachEmployeeType);
                }
                return employeeTypesList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Sreebharath 
        //Date : 08-08-2019
        //Get All Job Codes
        public List<AllJobCodes> GetAllJobCodes()
        {
            try
            {
                var JobCodes = _adminDB.GetJobcodes().ToList();
                var ListOfAllJobCodes = _adminDB.tbl_ibw_job_code_masters.Where(x => x.bt_delete == false).ToList();
                List<AllJobCodes> jobCodeList = new List<AllJobCodes>();
                foreach (var item in ListOfAllJobCodes)
                {
                    AllJobCodes eachJobCodeType = new AllJobCodes();
                    eachJobCodeType.jobCodeId = item.int_job_code_master_id;
                    eachJobCodeType.jobCode = item.vc_job_code;
                    eachJobCodeType.jobCodeTitle = item.vc_job_title;
                    eachJobCodeType.status = item.bt_status;
                    eachJobCodeType.JobCodeCategoryId = item.int_job_code_category;
                    eachJobCodeType.JobCodeCategory = JobCodes.Where(x => x.int_job_code_master_id == item.int_job_code_master_id).FirstOrDefault().vc_job_code_category;
                    jobCodeList.Add(eachJobCodeType);
                }
                return jobCodeList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //Author : Sreebharath 
        //Date : 07-08-2019
        //Email id verification
        public bool EmailValidation(string vcEmail, long? userId)
        {
            try
            {
                List<tbl_ibw_user> allUsersList;
                if (userId == null)
                {
                    allUsersList = _adminDB.tbl_ibw_users.Where(x => x.vc_email == vcEmail && x.bt_delete == false).ToList();
                }
                else
                {
                    allUsersList = _adminDB.tbl_ibw_users.Where(x => x.vc_email == vcEmail && x.int_user_id != userId && x.bt_delete == false).ToList();
                }
                if (allUsersList.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;

                }
            }
            catch { throw; }
        }

        //Author : Sreebharath 
        //Date : 07-08-2019
        //Email id verification
        public bool MunicipalityNameValidation(string municipalityName, long? municipalityId)
        {
            try
            {
                List<sp_getIBWMuncipalitiesResult> allMunicipalities;
                if (municipalityId == null)
                {
                    allMunicipalities = _adminDB.sp_getIBWMuncipalities().Where(x => x.vc_municipality_name.ToLower() == municipalityName.ToLower() && x.bt_delete == false).ToList();
                }
                else
                {
                    allMunicipalities = _adminDB.sp_getIBWMuncipalities().Where(x => x.vc_municipality_name.ToLower() == municipalityName.ToLower() && x.int_municipality_id != municipalityId && x.bt_delete == false).ToList();
                }
                if (allMunicipalities.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;

                }
            }
            catch { throw; }
        }

        //Author : Sreebharath 
        //Date : 25-08-2019
        //Bulk Upload Municipalities
        public bool bulkUploadMunicipalities(Municipalities model)
        {
            try
            {
                long? result = null;

                //List<sp_getIBWMuncipalitiesResult> allMunicipalities;
                foreach (var item in model.municipalitiesList)
                {
                    if ((_adminDB.sp_getIBWMuncipalities().Where(x => x.vc_municipality_name == item && x.bt_delete == false).ToList()).Count == 0)
                    {
                        _adminDB.sp_municipalityUpsert(null, item, true, model.createdBy, model.modifiedBy, ref result);
                    }

                }
                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //Author : Sreebharath 
        //Date : 07-08-2019
        //Email id verification
        public bool potentialLevelNameandColorCodeValidation(string potentialName, long? potentialLevelId, string colorCode)
        {
            try
            {
                List<sp_getAllPotentialLevelsResult> allPotentialLevels;
                if (potentialLevelId == null)
                {
                    allPotentialLevels = _adminDB.sp_getAllPotentialLevels().Where(x => (x.vc_potential_level.Trim().ToLower() == potentialName.Trim().ToLower() || x.vc_color_code == colorCode) && x.bt_delete == false).ToList();
                }
                else
                {
                    allPotentialLevels = _adminDB.sp_getAllPotentialLevels().Where(x => (x.vc_potential_level.Trim().ToLower() == potentialName.Trim().ToLower() || x.vc_color_code == colorCode) && x.int_RFQ_potential_level_id != potentialLevelId && x.bt_delete == false).ToList();
                }
                if (allPotentialLevels.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;

                }
            }
            catch { throw; }
        }

        public long? upsertMuniciplaties(Municipalities model)
        {
            try
            {
                long? result = null;
                _adminDB.sp_municipalityUpsert(model.municipalityId, model.municipalityName, model.status, model.createdBy, model.modifiedBy, ref result);
                return result;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        #region lookup
        //Author: Mani
        //Date : 22-Aug-2019
        //Get Lookup Options

        public LookupModel GetLookups()
        {
            try
            {
                LookupModel model = new LookupModel();

                model.LookupOptions = _adminDB.GetLookupOptions().Select(x => new LookupOptions { CodeMasterId = x.int_code_master_id, CodeMasterName = x.vc_code_master_name, LookupId = x.int_lookup_id, LookupName = x.vc_lookup_name, IsSystem = x.bt_system }).OrderBy(x => x.CodeMasterName).ThenBy(x => x.LookupName).ToList();
                model.CodeMaster = _adminDB.GetCodeMaster().Select(x => new CodeMaster { CodeMasterId = x.int_code_master_id, CodeMasterName = x.vc_code_master_name }).ToList();

                return model;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public Response<string> UpsertLookup(LookupOptions lookup, long? createdBy)
        {
            Response<string> response = new Response<string>();
            List<GetLookupOptionsResult> lookups = new List<GetLookupOptionsResult>();
            lookups = _adminDB.GetLookupOptions().Where(x => x.int_lookup_id != lookup.LookupId && x.int_code_master_id == lookup.CodeMasterId && x.vc_lookup_name.ToLower() == lookup.LookupName.ToLower()).ToList();
            if (lookups.Count == 0)
            {
                long? result = 0;
                _adminDB.UpsertLookup(lookup.LookupId, lookup.CodeMasterId, lookup.LookupName, createdBy, ref result);
                if (result > 0)
                {
                    response.Status = true;
                    response.Message = lookup.LookupId > 0 ? "Lookup updated successfully" : "Lookup saved successfully";
                }
                else
                {
                    response.Status = false;
                    response.Message = "Something went wrong.";
                }
            }
            else
            {
                response.Status = false;
                response.Message = "Lookup name already exists for code master.";
            }
            return response;
        }
        public bool DeleteLookup(long? lookupId, long? createdBy)
        {
            long? result = 0;
            _adminDB.DeleteLookup(lookupId, true, createdBy, ref result);
            if (result > 0)
                return true;
            else
                return false;
        }
        #endregion


        #region Mani

        #region Tasks

        //Author : Mani
        //Date : 14-Oct-2019
        //TaskValidateUpsertAndGetResponse

        public Response<bool> TaskValidateUpsertAndGetResponse(Response<bool> response, Tasks model, long? headerUserId)
        {
            try
            {
                //storing data in a variable, not to hit database multiple times.
                var Data = GetTasks();

                if (Data.Exists(L => L.TaskId != model.TaskId && L.TaskName.ToLower() == model.TaskName.ToLower()))
                {
                    response.Status = false;
                    response.Message = "Task already exists";
                    //Event logging
                    // Logging.EventLog(headerUserId, "Attempt to add new leave reason");
                }
                else if (UpsertTaskCursor(model) > 0)
                {
                    if (model.TaskId == 0)
                    {
                        response.Status = true;
                        response.Message = "Task added successfully";
                        //Event logging
                        //Logging.EventLog(headerUserId, "Attempt to add new leave reason");
                        // Logging.EventLog(headerUserId, "Leave reason added successfully");
                    }
                    else
                    {
                        response.Status = true;
                        response.Message = "Task updated successfully";
                        //Event logging
                        // Logging.EventLog(headerUserId, "Attempt to update leave reason");
                        //  Logging.EventLog(headerUserId, "Leave reason updated successfully");
                    }
                }
                else
                {
                    if (model.TaskId == 0)
                    {
                        response.Status = false;
                        response.Message = "Adding task failed";
                        //Event logging
                        //Logging.EventLog(headerUserId, "Attempt to add new leave reason");

                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Updating task failed";
                        //Event logging
                        //Logging.EventLog(headerUserId, "Attempt to update leave reason");

                    }
                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        //author:Sreebharath
        //date: 05/11/2019
        //change the Sequential Order for all the tasks
        public long? changeSequentialOrderTasks(long? fromPosition, long? toPosition)
        {
            try
            {
                long? result = 0;
                var data = _adminDB.changePostionForTasks(fromPosition, toPosition, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        //author:Sreebharath
        //date: 05/11/2019
        //to insert the job Codes with notes
        public long? UpsertTaskCursor(Tasks model)
        {
            try
            {
                long? result = null;
                DataTable dt = new DataTable();
                DataTable documnetTypedt = new DataTable();
                documnetTypedt.Columns.AddRange(new DataColumn[1] { new DataColumn("int_document_lookup_id", typeof(int)) });
                dt.Columns.AddRange(new DataColumn[2] { new DataColumn("JobCodeId", typeof(int)), new DataColumn("Notes", typeof(string)) });
                foreach (var row in model.jobCodeDetails)
                {
                    int JobCodeId = Convert.ToInt32(row.JobCodeId);
                    string Notes = row.Notes;
                    dt.Rows.Add(JobCodeId, Notes);
                }

                using (var con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    using (var cmd = new SqlCommand("upsertTaskWithNotesCursor", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        if (model.TaskId != null)
                            cmd.Parameters.AddWithValue("@intTaskId", model.TaskId);
                        else
                            cmd.Parameters.AddWithValue("@intTaskId", DBNull.Value);
                        cmd.Parameters.AddWithValue("@vcTaskName", model.TaskName);
                        cmd.Parameters.AddWithValue("@array_jobCode_details", dt);
                        if (model.documentTypes != null)
                        {
                            foreach (var documentTypeLookupId in model.documentTypes)
                            {
                                int int_document_lookup_id = Convert.ToInt32(documentTypeLookupId);
                                documnetTypedt.Rows.Add(int_document_lookup_id);
                            }
                            cmd.Parameters.AddWithValue("@array_document_types", documnetTypedt);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@array_document_types", documnetTypedt);
                        }
                        cmd.Parameters.AddWithValue("@intUserId", model.UserId);
                        //Add the output parameter to the command object
                        SqlParameter outPutParameter = new SqlParameter();
                        outPutParameter.ParameterName = "@result";
                        outPutParameter.SqlDbType = System.Data.SqlDbType.Int;
                        outPutParameter.Direction = System.Data.ParameterDirection.Output;
                        cmd.Parameters.Add(outPutParameter);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        result = Convert.ToInt32(outPutParameter.Value);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        //author:Mani
        //date: 14-Oct-2019
        //Get Leave Reasons
        public List<Tasks> GetTasks()
        {
            try
            {
                var Tasks = _adminDB.GetTasks().ToList();
                var data = Tasks.GroupBy(t => new { t.int_task_id, t.vc_task_name, t.int_position_index, t.bt_status });

                List<Tasks> TasksData = data.Select(x => new Tasks
                {
                    TaskId = x.Key.int_task_id,
                    TaskName = x.Key.vc_task_name,
                    JobCodeIds = x.Select(j => j.int_job_code_id).ToList(),
                    JobCodes = x.Select(jc => jc.vc_job_code).ToList(),
                    jobCodeDetails = x.Select(j => new JobCodes { JobCodeId = j.int_job_code_id, JobCode = j.vc_job_code, Notes = j.vc_notes }).ToList(),
                    documentTypes = _adminDB.GetAllDocumentsTypeOfTask(x.Key.int_task_id).Select(doc => doc.int_document_lookup_id).ToList(),
                    documentTypeNames = _adminDB.GetAllDocumentsTypeOfTask(x.Key.int_task_id).Select(doc => " " + doc.vc_lookup_name).ToList(),
                    positionIndex = x.Key.int_position_index,
                    IsActive = x.Key.bt_status
                }).ToList();


                return TasksData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //Author        : Siddhant Chawade
        //Date          : 28th Nov 2019
        //Description   : To update task Status
        public long? UpdateTaskStatus(long? taskId, bool? status)
        {
            try
            {
                long? result = 0;
                _adminDB.UpdateTaskStaus(taskId, status, ref result);
                return result;
            }
            catch { throw; }
        }





        //author:Mani
        //date: 14-Oct-2019
        //delete leave reasons
        public long? DeleteTask(long? TaskId, long? userId)
        {
            try
            {
                long? result = 0;
                _adminDB.DeleteTask(TaskId, userId, ref result);
                return result;
            }
            catch { throw; }
        }

        #endregion
        #region Job Codes
        //Author        : Siddhant Chawade
        //Date          : 20th Jan 2020
        //Description   : To get the list of category wise job codes
        public List<AllJobCodesWithCategory> GetJobCodesWithCategory(bool? isUserSpecific, long? userId)
        {
            try
            {
                var categories = _adminDB.GetLookupOptions().Where(x => x.vc_code_master_name == "Job Code Category").ToList();
                List<AllJobCodesWithCategory> list = new List<AllJobCodesWithCategory>();
                var JobCodes = _adminDB.GetJobcodes().ToList();
                if (isUserSpecific == true && userId != 1)
                {
                    JobCodes = _adminDB.sp_getJobCodesOfUser().Where(x => x.int_user_id == userId && x.bt_delete == false).AsEnumerable().Select(x => new GetJobcodesResult
                    {
                        int_job_code_master_id = Convert.ToInt64(x.int_job_code_master_id),
                        vc_job_code = x.vc_job_code,
                        vc_job_title = x.vc_job_title,
                        vc_job_code_category = x.vc_job_code_category,
                        int_job_code_category = x.int_job_code_category
                    }).ToList();
                }

                foreach (var cat in categories)
                {
                    AllJobCodesWithCategory jcCat = new AllJobCodesWithCategory();
                    List<AllJobCodes> jcs = new List<Models.AllJobCodes>();
                    jcCat.JobCodeCategoryId = cat.int_lookup_id;
                    jcCat.JobCodeCategory = cat.vc_lookup_name;

                    foreach (var job in JobCodes.Where(x => x.vc_job_code_category == cat.vc_lookup_name))
                    {
                        AllJobCodes jc = new Models.AllJobCodes();
                        jc.jobCodeId = job.int_job_code_master_id;
                        jc.jobCode = job.vc_job_code;
                        jc.jobCodeTitle = job.vc_job_title;

                        jcs.Add(jc);
                    }
                    jcCat.JobCodes = jcs;
                    if (jcCat.JobCodes.Count > 0)
                        list.Add(jcCat);
                }
                if (JobCodes.Where(x => x.vc_job_code_category == "-").Count() > 0)
                {
                    AllJobCodesWithCategory jcCat1 = new AllJobCodesWithCategory();
                    List<AllJobCodes> jcs1 = new List<Models.AllJobCodes>();
                    jcCat1.JobCodeCategoryId = null;
                    jcCat1.JobCodeCategory = "-";

                    foreach (var job in JobCodes.Where(x => x.vc_job_code_category == "-"))
                    {
                        AllJobCodes jc = new Models.AllJobCodes();
                        jc.jobCodeId = job.int_job_code_master_id;
                        jc.jobCode = job.vc_job_code;
                        jc.jobCodeTitle = job.vc_job_title;

                        jcs1.Add(jc);
                    }
                    jcCat1.JobCodes = jcs1;
                    if (jcCat1.JobCodes.Count > 0)
                        list.Add(jcCat1);
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //Author : Mani
        //Date : 11-Sep-2019
        //JobCodeValidateUpsertAndGetResponse

        public Response<bool> JobCodeValidateUpsertAndGetResponse(Response<bool> response, JobCodes model, long? headerUserId)
        {
            try
            {
                var data = GetJobCodes().ToList();

                if ((model.JobCodeId == 0 && data.Exists(jobcode => jobcode.JobCode.Trim().ToLower() == model.JobCode.Trim().ToLower()))
                   ||
                   data.Where(x => x.JobCodeId != model.JobCodeId).ToList().Exists(jobcode => jobcode.JobCode.Trim().ToLower() == model.JobCode.Trim().ToLower())
                   )
                {
                    response.Status = false;
                    response.Message = "Job code already exists";

                    //Logging event
                    Logging.EventLog(headerUserId, "Attempt to add new job code");


                }

                else if ((model.JobCodeId == 0 && data.Exists(jobcode => jobcode.JobCodeTitle.Trim().ToLower() == model.JobCodeTitle.Trim().ToLower()))
                    ||
                    data.Where(x => x.JobCodeId != model.JobCodeId).ToList().Exists(jobcode => jobcode.JobCodeTitle.Trim().ToLower() == model.JobCodeTitle.Trim().ToLower())
                    )
                {
                    response.Status = false;
                    response.Message = "Job code title already exists";

                    //Logging event
                    Logging.EventLog(headerUserId, "Attempt to add new job code");

                }

                else if (UpsertJobCode(model) > 0)
                {
                    if (model.JobCodeId == 0)
                    {
                        response.Status = true;
                        response.Message = "Job code saved successfully";

                        //Logging event
                        Logging.EventLog(headerUserId, "Attempt to add new job code");
                        Logging.EventLog(headerUserId, "New job code added successfully");
                    }
                    else
                    {
                        response.Status = true;
                        response.Message = "Job code updated successfully";

                        //Logging event
                        Logging.EventLog(headerUserId, "Attempt to update job code details");
                        Logging.EventLog(headerUserId, "Job code detail updated successfully");
                    }
                }
                else
                {
                    if (model.JobCodeId == 0)
                    {
                        response.Status = false;
                        response.Message = "Saving job code failed";

                        //Logging event
                        Logging.EventLog(headerUserId, "Attempt to add new job code");

                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Updating job code failed";

                        //Logging event
                        Logging.EventLog(headerUserId, "Attempt to update job code details");
                    }
                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //Author : Mani
        //Date : 08-Aug-2019
        //Job Code details insert and update
        public long? UpsertJobCode(JobCodes model)
        {
            try
            {
                long? result = 0;
                _adminDB.UpsertJobcode(model.JobCodeId, model.JobCodeCategoryId, model.JobCode, model.JobCodeTitle, model.ChargeoutRate, model.Notes, model.IsActive, model.UserId, ref result);
                return result;
            }
            catch { throw; }
        }

        //Author : Mani
        //Date : 08-Aug-2019
        //Get Job Code(s) details
        public List<JobCodes> GetJobCodes()
        {
            try
            {
                var JobCodes = _adminDB.GetJobcodes().ToList();

                List<JobCodes> JobCodesData = JobCodes.Select(x => new JobCodes
                {
                    JobCodeId = x.int_job_code_master_id,
                    JobCode = x.vc_job_code,
                    JobCodeTitle = x.vc_job_title,
                    ChargeoutRate = x.dec_chargeout_rate_per_hr,
                    AssociatedUsersCount = x.int_associated_users_count,
                    Notes = x.vc_notes,
                    IsActive = x.bt_status,
                    UserId = x.int_created_by,
                    JobCodeCategory = x.vc_job_code_category,
                    JobCodeCategoryId = x.int_job_code_category
                }).ToList();


                return JobCodesData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Mani
        //Date : 08-Aug-2019
        //Job Code status update
        public long? UpdateJobCodeStatus(long? jobCodeId, bool? status)
        {
            try
            {
                long? result = 0;
                _adminDB.UpdateJobcodeStaus(jobCodeId, status, ref result);
                return result;
            }
            catch { throw; }
        }


        //Author : Mani
        //Date : 08-Aug-2019
        //Delete Job Code
        public long? DeleteJobCode(long? jobCodeId)
        {
            try
            {
                long? result = 0;
                _adminDB.DeleteJobcode(jobCodeId, ref result);
                return result;
            }
            catch { throw; }
        }

        #endregion

        #region Expense Codes


        //Author : Mani
        //Date : 11-Sep-2019
        //JobCodeValidateUpsertAndGetResponse

        public Response<bool> ExpenseCodeValidateUpsertAndGetResponse(Response<bool> response, ExpenseCodes model, long? headerUserId)
        {
            try
            {

                var Data = GetExpenseCodes();

                if ((model.ExpenseCodeId == 0 && Data.
                    Exists(expensecode => expensecode.ExpenseCode.Trim().ToLower() == model.ExpenseCode.Trim().ToLower()))

                    ||

                    Data.Where(x => x.ExpenseCodeId != model.ExpenseCodeId).ToList().
                    Exists(expensecode => expensecode.ExpenseCode.Trim().ToLower() == model.ExpenseCode.Trim().ToLower())

                    )
                {
                    response.Status = false;
                    response.Message = "Expense code already exists";

                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to add new expense code");

                }
                else if (UpsertExpenseCode(model) > 0)
                {
                    if (model.ExpenseCodeId == 0)
                    {
                        response.Status = true;
                        response.Message = "Expense code saved successfully";

                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to add new expense code");
                        Logging.EventLog(headerUserId, "New expense code added successfully");
                    }
                    else
                    {
                        response.Status = true;
                        response.Message = "Expense code updated successfully";

                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update expense code details");
                        Logging.EventLog(headerUserId, "Expense code detail updated successfully");
                    }
                }
                else
                {
                    if (model.ExpenseCodeId == 0)
                    {
                        response.Status = false;
                        response.Message = "Saving expense code failed";

                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to add new expense code");
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Updating expense code failed";

                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update expense code details");
                    }
                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //Author : Mani
        //Date : 20-Aug-2019
        //Expense Code details insert and update
        public long? UpsertExpenseCode(ExpenseCodes model)
        {
            try
            {
                long? result = 0;
                _adminDB.UpsertExpenseCode(model.ExpenseCodeId, model.ExpenseCode, model.ExpenseUnitId, model.ExpenseLimitTypeId, model.ExpenseLimitAmount, model.Rate, model.isDefault, model.isReimbursable, model.AttachmentRequired, model.UserId, ref result);
                return result;
            }
            catch { throw; }
        }

        //Author : Mani
        //Date : 20-Aug-2019
        //Get Expense Code(s) details
        public List<ExpenseCodes> GetExpenseCodes()
        {
            try
            {
                List<GetExpenseCodeResult> ExpenseCodesList = _adminDB.GetExpenseCode().ToList();

                List<ExpenseCodes> ExpenseCodesData = ExpenseCodesList.Select(x => new ExpenseCodes
                {
                    ExpenseCodeId = x.int_expense_code_master_id,
                    ExpenseCode = x.vc_expense_code,
                    ExpenseUnitId = x.int_expense_unit_id,
                    ExpenseUnit = x.vc_expense_unit,
                    ExpenseLimitTypeId = x.int_expense_limit_type_id,
                    ExpenseLimitType = x.vc_expense_limit_type,
                    ExpenseLimitAmount = x.dec_expense_limit_amout,
                    isDefault = x.bt_isDefault,
                    Rate = x.dec_rate,
                    IsActive = x.bt_status,
                    isReimbursable = x.bt_isReimbursable,
                    AttachmentRequired = x.bt_attachment_required,
                    UserId = x.int_created_by
                }).ToList();
                return ExpenseCodesData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //Author : Mani
        //Date : 20-Aug-2019
        //Expense Code status update
        public long? UpdateExpenseCodeStatus(long? expenseCodeId, bool? status)
        {
            try
            {
                long? result = 0;
                _adminDB.UpdateExpenseCodeStaus(expenseCodeId, status, ref result);
                return result;
            }
            catch { throw; }
        }


        //Author : Mani
        //Date : 20-Aug-2019
        //Delete Expense Code
        public long? DeleteExpenseCode(long? expenseCodeId)
        {
            try
            {
                long? result = 0;
                _adminDB.DeleteExpenseCode(expenseCodeId, ref result);
                return result;
            }
            catch { throw; }
        }

        #endregion

        #region Settings

        //Author        : Vamsi
        //Date          : 17th Mar 2020
        //Description   : To get the list of country specific states
        public List<SettingsCountries> GetCountriesWithStates()
        {
            try
            {
                var countries = _adminDB.sp_getIBWCountries().ToList();
                List<SettingsCountries> lstcountries = new List<SettingsCountries>();
                SettingsCountries lstcountry;
                foreach (var country in countries)
                {
                    lstcountry = new SettingsCountries();
                    lstcountry.int_country_id = country.int_county_id;
                    lstcountry.vc_country_name = country.vc_country_name;
                    List<States> lstStates = new List<States>();
                    States lststate;
                    var states = _adminDB.sp_getIBWStates(country.int_county_id).ToList();
                    foreach (var state in states)
                    {
                        lststate = new States();
                        lststate.stateId = state.int_state_id;
                        lststate.stateName = state.vc_state_name;
                        lststate.countryID = state.int_county_id;
                        lstStates.Add(lststate);
                    }
                    lstcountry.states = lstStates;
                    lstcountries.Add(lstcountry);
                }
                return lstcountries;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //Author : Mani
        //Date : 17-08-2019
        //Get Settings

        public List<Settings> GetSettings()
        {
            try
            {
                List<Settings> SettingsData = _adminDB.GetSettings().Select(x => new Settings
                {
                    SettingId = x.int_setting_id,
                    SettingName = x.vc_setting_name,
                    Description = x.vc_description,
                    InputType = x.vc_input_type,
                    Value = x.vc_value
                }).ToList();

                return SettingsData;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        //Author : Mani
        //Date : 17-08-2019
        //Update Settings

        public long? UpdateSettings(long? settingId, string newValue, string InputType)
        {
            try
            {
                long? result = 0;
                string UpdatedValue = newValue;
                if (InputType == "date" && newValue != "")
                {
                    UpdatedValue = newValue.Substring(0, 10);
                }

                return _adminDB.UpdateSettings(settingId, UpdatedValue, ref result).FirstOrDefault().UpdatedRows;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion

        #region Configure Kanban

        //Author : Mani
        //Date : 11-Sep-2019
        //KanbanValidateUpsertAndGetResponse

        public Response<bool> KanbanValidateUpsertAndGetResponse(Response<bool> response, ConfigureKanban model, long? headerUserId)
        {
            try
            {
                var data = GetKanbanStages().Where(x => x.KanbanTypeName != null).ToList();

                if (((model.ConfigureKanbanId == null || model.ConfigureKanbanId == 0) &&
                     data.Exists(K => K.KanbanTypeName.Trim().ToLower() == model.KanbanTypeName.Trim().ToLower() &&
                 K.KanbanStageName.Trim().ToLower() == model.KanbanStageName.Trim().ToLower()))

                 ||

                (data.Where(x => x.ConfigureKanbanId != model.ConfigureKanbanId).ToList().
                     Exists(K => K.KanbanTypeName.Trim().ToLower() == model.KanbanTypeName.Trim().ToLower() &&
                 K.KanbanStageName.Trim().ToLower() == model.KanbanStageName.Trim().ToLower()))
                 )
                {

                    switch (model.KanbanTypeName.ToLower())
                    {
                        case "quotes":
                            //Event logging
                            Logging.EventLog(headerUserId, "Attempt to add new quote stage");
                            break;
                        case "projects":
                            //Event logging
                            Logging.EventLog(headerUserId, "Attempt to add new project stage");
                            break;
                        case "sites":
                            //Event logging
                            Logging.EventLog(headerUserId, "Attempt to add new site stage");
                            break;
                        case "sows":
                            //Event logging
                            Logging.EventLog(headerUserId, "Attempt to add new SOW stage");
                            break;
                        case "tasks":
                            //Event logging
                            Logging.EventLog(headerUserId, "Attempt to add new task stage");
                            break;
                        default:

                            break;
                    }



                    response.Status = false;
                    response.Message = model.KanbanTypeName + " kanban stage already exists";

                }
                else if (UpsertKanbanStage(model) > 0)
                {
                    if (model.ConfigureKanbanId == 0 || model.ConfigureKanbanId == null)
                    {

                        switch (model.KanbanTypeName.ToLower())
                        {
                            case "quotes":
                                //Event logging
                                Logging.EventLog(headerUserId, "New quote stage has been added successfully");
                                break;
                            case "projects":
                                //Event logging
                                Logging.EventLog(headerUserId, "New project stage has been added successfully");
                                break;
                            case "sites":
                                //Event logging
                                Logging.EventLog(headerUserId, "New site stage has been added successfully");
                                break;
                            case "sow":
                                //Event logging
                                Logging.EventLog(headerUserId, "New SOW stage has been added successfully");
                                break;
                            case "tasks":
                                //Event logging
                                Logging.EventLog(headerUserId, "New task stage has been added successfully");
                                break;
                            default:

                                break;
                        }


                        response.Status = true;
                        response.Message = model.KanbanTypeName + " kanban stage added successfully";


                    }
                    else
                    {
                        switch (model.KanbanTypeName.ToLower())
                        {
                            case "quotes":
                                //Event logging
                                Logging.EventLog(headerUserId, "Quote stage details updated successfully");
                                break;
                            case "projects":
                                //Event logging
                                Logging.EventLog(headerUserId, "Project stage details updated successfully");
                                break;
                            case "sites":
                                //Event logging
                                Logging.EventLog(headerUserId, "Site stage details updated successfully");
                                break;
                            case "sow":
                                //Event logging
                                Logging.EventLog(headerUserId, "SOW stage details updated successfully");
                                break;
                            case "tasks":
                                //Event logging
                                Logging.EventLog(headerUserId, "Task stage details updated successful");
                                break;
                            default:

                                break;
                        }
                        response.Status = true;
                        response.Message = model.KanbanTypeName + " kanban stage updated successfully";
                    }


                }
                else
                {
                    if (model.ConfigureKanbanId == 0)
                    {
                        response.Status = false;
                        response.Message = "Saving " + model.KanbanTypeName + " kanban stage failed";

                        switch (model.KanbanTypeName.ToLower())
                        {
                            case "quotes":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to add new quote stage");
                                break;
                            case "projects":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to add new project stage");
                                break;
                            case "sites":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to add new site stage");
                                break;
                            case "sow":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to add new SOW stage");
                                break;
                            case "tasks":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to add new task stage");
                                break;
                            default:

                                break;
                        }
                    }
                    else
                    {
                        switch (model.KanbanTypeName.ToLower())
                        {
                            case "quotes":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to update quote stage");
                                break;
                            case "projects":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to update project stage details");
                                break;
                            case "sites":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to update site stage details");
                                break;
                            case "sow":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to update SOW stage details");
                                break;
                            case "tasks":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to update task stage details");
                                break;
                            default:

                                break;
                        }

                        response.Status = false;
                        response.Message = "Updating " + model.KanbanTypeName + " kanban stage failed";


                    }

                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        //Author : Mani
        //Date : 18-Aug-2019
        //Kanban step details insert and update
        public long? UpsertKanbanStage(ConfigureKanban model)
        {
            try
            {
                long? result = 0;


                _adminDB.UpsertKanbanStage(model.ConfigureKanbanId, model.KanbanStageName, model.KanbanTypeName.ToLower(), model.ColorCode, model.UserId, ref result);

                return result;
            }
            catch { throw; }
        }

        //Author : Mani
        //Date : 18-Aug-2019
        //Get KanbanStep(s) details
        public List<ConfigureKanban> GetKanbanStages()
        {
            try
            {
                List<ConfigureKanban> KanbanStagesData = _adminDB.GetKanbanStages().Where(x => x.bt_delete == false).Select(x => new ConfigureKanban
                {
                    ConfigureKanbanId = x.int_configure_kanban_id,
                    KanbanStageName = x.vc_kanban_stage_name,
                    KanbanTypeName = x.vc_kanban_type,
                    IsActive = x.bt_status,
                    ColorCode = x.vc_color_code,
                    UserId = x.int_created_by,
                    IsSystem = Convert.ToBoolean(x.bt_system),
                    OptionCount = x.OptionCount,
                    StageAssignedCount = x.StageAssignedCount
                }).ToList();

                return KanbanStagesData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Mani
        //Date : 18-Aug-2019
        //Update Kanban stage status update
        public long? UpdateKanbanStageStatus(long? ConfigureKanbanId, bool? status)
        {
            try
            {
                long? result = 0;
                _adminDB.UpdateKanbanStageStaus(ConfigureKanbanId, status, ref result);
                return result;
            }
            catch { throw; }
        }


        //Author : Mani
        //Date : 18-Aug-2019
        //Delete Kanban Step
        public long? DeleteKanbanStage(long? jobCodeId)
        {
            try
            {
                long? result = 0;
                _adminDB.DeleteKanbanStage(jobCodeId, ref result);
                return result;
            }
            catch { throw; }
        }

        #endregion

        #region Common Master Data Tabs

        //Author : Mani
        //Date : 11-Sep-2019
        //CMDTabsValidateUpsertAndGetResponse

        public Response<bool> CMDTabsValidateUpsertAndGetResponse(Response<bool> response, CommonMasterData model, long? headerUserId)
        {
            try
            {


                var Data = GetCommonMasterData();

                if ((model.CommonMasterDataId == 0 && Data.Exists(CM => CM.CommonMasterDataCategory.ToLower() == model.CommonMasterDataCategory.ToLower()
                && CM.CommonMasterDataName.Trim().ToLower() == model.CommonMasterDataName.Trim().ToLower()))
                ||
                Data.Where(x => x.CommonMasterDataId != model.CommonMasterDataId).ToList().Exists(CM => CM.CommonMasterDataCategory.ToLower() == model.CommonMasterDataCategory.ToLower()
                  && CM.CommonMasterDataName.Trim().ToLower() == model.CommonMasterDataName.Trim().ToLower())
                )
                {
                    response.Status = false;
                    response.Message = model.CommonMasterDataCategory.Remove(model.CommonMasterDataCategory.Length - 1, 1) + " already exists";

                    switch (model.CommonMasterDataCategory.ToLower())
                    {
                        case "client types":
                            //Event logging
                            Logging.EventLog(headerUserId, "Attempt to add new client type");
                            break;
                        case "project types":
                            //Event logging
                            Logging.EventLog(headerUserId, "Attempt to add new project type");
                            break;
                        case "lead sources":
                            //Event logging
                            Logging.EventLog(headerUserId, "Attempt to add new lead source");
                            break;
                        case "delay reasons":
                            //Event logging
                            Logging.EventLog(headerUserId, "Attempt to add new delay reason");
                            break;
                        case "survey purposes":
                            //Event logging
                            Logging.EventLog(headerUserId, "Attempt to add new survey purpose");
                            break;
                        //case "actions":
                        //    //Event logging
                        //    Logging.EventLog(headerUserId, "Attempt to add new action");
                        //    break;
                        default:

                            break;
                    }

                }
                else if (UpsertCommonMasterData(model) > 0)
                {
                    if (model.CommonMasterDataId == 0)
                    {

                        switch (model.CommonMasterDataCategory.ToLower())
                        {
                            case "client types":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to add new client type");
                                break;
                            case "project types":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to add new project type");
                                break;
                            case "lead sources":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to add new lead source");
                                break;
                            case "delay reasons":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to add new delay reason");
                                break;
                            case "survey purposes":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to add new survey purpose");
                                break;
                            //case "actions":
                            //    //Event logging
                            //    Logging.EventLog(headerUserId, "Attempt to add new action");
                            //    break;
                            default:

                                break;
                        }

                        response.Status = true;
                        response.Message = model.CommonMasterDataCategory.Remove(model.CommonMasterDataCategory.Length - 1, 1) + " saved successfully";

                        switch (model.CommonMasterDataCategory.ToLower())
                        {
                            case "client types":
                                //Event logging
                                Logging.EventLog(headerUserId, "Client type added successfully");
                                break;
                            case "project types":
                                //Event logging
                                Logging.EventLog(headerUserId, "Project type added successfully");
                                break;
                            case "lead sources":
                                //Event logging
                                Logging.EventLog(headerUserId, "Lead source added successfully");
                                break;
                            case "delay reasons":
                                //Event logging
                                Logging.EventLog(headerUserId, "Delay reason added successfully");
                                break;
                            case "survey purposes":
                                //Event logging
                                Logging.EventLog(headerUserId, "Survey purpose added successfully");
                                break;
                            //case "actions":
                            //    //Event logging
                            //    Logging.EventLog(headerUserId, "Action added successfully");
                            //    break;
                            default:

                                break;
                        }

                    }
                    else
                    {

                        switch (model.CommonMasterDataCategory.ToLower())
                        {
                            case "client types":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to update client types");
                                break;
                            case "project types":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to update project types");
                                break;
                            case "lead sources":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to update lead source");
                                break;
                            case "delay reasons":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to update delay reason");
                                break;
                            case "survey purposes":
                                //Event logging
                                Logging.EventLog(headerUserId, "Attempt to update survey purpose");
                                break;
                            //case "actions":
                            //    //Event logging
                            //    Logging.EventLog(headerUserId, "Attempt to update action");
                            //    break;
                            default:

                                break;
                        }


                        response.Status = true;
                        response.Message = model.CommonMasterDataCategory.Remove(model.CommonMasterDataCategory.Length - 1, 1) + " updated successfully";

                        switch (model.CommonMasterDataCategory.ToLower())
                        {
                            case "client types":
                                //Event logging
                                Logging.EventLog(headerUserId, "Client type details updated successfully");
                                break;
                            case "project types":
                                //Event logging
                                Logging.EventLog(headerUserId, "Project type details updated successfully");
                                break;
                            case "lead sources":
                                //Event logging
                                Logging.EventLog(headerUserId, "Lead source details updated successfully");
                                break;
                            case "delay reasons":
                                //Event logging
                                Logging.EventLog(headerUserId, "Delay reason details updated successfully");
                                break;
                            case "survey purposes":
                                //Event logging
                                Logging.EventLog(headerUserId, "Survey purpose updated successfully");
                                break;
                            //case "actions":
                            //    //Event logging
                            //    Logging.EventLog(headerUserId, "Action updated successfully");
                            //    break;
                            default:

                                break;
                        }

                    }
                }
                else
                {
                    if (model.CommonMasterDataId == 0)
                    {
                        response.Status = false;
                        response.Message = "Saving" + model.CommonMasterDataCategory.Remove(model.CommonMasterDataCategory.Length - 1, 1) + "failed";

                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Updating" + model.CommonMasterDataCategory.Remove(model.CommonMasterDataCategory.Length - 1, 1) + "failed";

                    }

                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        //Author : Mani
        //Date : 23-Aug-2019
        //Upsert Common Master Data
        public long? UpsertCommonMasterData(CommonMasterData model)
        {
            try
            {
                long? result = 0;

                _adminDB.UpsertCommonMasterData(model.CommonMasterDataId, model.CommonMasterDataCategory, model.CommonMasterDataName, model.UserId, ref result);

                return result;
            }
            catch (Exception ex) { throw; }
        }

        //Author : Mani
        //Date : 23-Aug-2019
        //Get KanbanStep(s) details
        public List<CommonMasterData> GetCommonMasterData()
        {
            try
            {


                List<CommonMasterData> CommonMasterData = _adminExtensionDB.GetCommonMasterData().Where(x => x.bt_delete == false).Select(x => new CommonMasterData
                {
                    CommonMasterDataId = x.int_common_master_data_id,
                    CommonMasterDataCategory = x.vc_category_name,
                    CommonMasterDataName = x.vc_common_master_data_name,
                    AssociatedClientsCount = x.int_associated_clients_count,
                    AssociatedQuestionsCount = x.int_associated_config_questions_count,
                    IsActive = x.bt_valid,
                    UserId = x.int_created_by,
                    dataTypeAssignedCount = x.dataTypeAssignedCount
                }).ToList();

                return CommonMasterData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Mani
        //Date : 23-Aug-2019
        //Update Common Master Data Status
        public long? UpdateCommonMasterDataStatus(long? ConfigureKanbanId, long? userId, bool? status)
        {
            try
            {
                long? result = 0;
                _adminDB.UpdateCommonMasterDataStaus(ConfigureKanbanId, status, userId, ref result);
                return result;
            }
            catch { throw; }
        }


        //Author : Mani
        //Date : 23-Aug-2019
        //Delete common master data
        public long? DeleteCommonMasterData(long? commonMasterDataId, long? userId)
        {
            try
            {
                long? result = 0;
                _adminDB.DeleteCommonMasterData(commonMasterDataId, userId, ref result);
                return result;
            }
            catch { throw; }
        }

        #endregion

        #region Asset Categories


        //Author : Mani
        //Date : 11-Sep-2019
        //AssetCategoryValidateUpsertAndGetResponse

        public Response<bool> AssetCategoryValidateUpsertAndGetResponse(Response<bool> response, AssetCategories model, long? headerUserId)
        {
            try
            {
                //storing data in a variable, not to hit database multiple times.
                var Data = GetAssetCategories();

                // condition to validate the asset category name uniqueness
                if ((model.AssetCategoryId == 0 && Data.
                    Exists(assetCategory => assetCategory.AssetCategoryName.Trim().ToLower() == model.AssetCategoryName.Trim().ToLower()))

                    ||

                    Data.Where(x => x.AssetCategoryId != model.AssetCategoryId).ToList().
                    Exists(assetCategory => assetCategory.AssetCategoryName.Trim().ToLower() == model.AssetCategoryName.Trim().ToLower())
                    )
                {
                    response.Status = false;
                    response.Message = "Asset category already exists";

                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to add new asset category");
                }

                // condition to validate the asset category prefix uniqueness
                else if ((model.AssetCategoryId == 0 && Data.
                    Exists(assetCategory => assetCategory.AssetCategoryPrefix.Trim().ToLower() == model.AssetCategoryPrefix.Trim().ToLower()))

                    ||

                    Data.Where(x => x.AssetCategoryId != model.AssetCategoryId).ToList().
                    Exists(assetCategory => assetCategory.AssetCategoryPrefix.Trim().ToLower() == model.AssetCategoryPrefix.Trim().ToLower())
                    )
                {
                    response.Status = false;
                    response.Message = "Asset category prefix already exists";

                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to add new asset category");
                }
                else if (UpsertAssetCategory(model) > 0)
                {
                    if (model.AssetCategoryId == 0)
                    {
                        response.Status = true;
                        response.Message = "Asset category saved successfully";

                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to add new asset category");
                        Logging.EventLog(headerUserId, "New asset category added successfully");
                    }
                    else
                    {
                        response.Status = true;
                        response.Message = "Asset category updated successfully";

                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update asset category details");
                        Logging.EventLog(headerUserId, "Asset category detail updated successfully");
                    }
                }
                else
                {
                    if (model.AssetCategoryId == 0)
                    {
                        response.Status = false;
                        response.Message = "Saving asset category failed";

                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to add new asset category");


                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Updating asset category failed";

                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update asset category details");
                    }
                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }



        //author:Mani
        //date: 24-Aug-2019
        //Asset Category Upsert
        public long? UpsertAssetCategory(AssetCategories model)
        {
            try
            {
                long? result = 0;
                _adminDB.UpsertAssetCategory(model.AssetCategoryId, model.AssetCategoryName, model.AssetCategoryPrefix, model.UserId, ref result);
                return result;
            }
            catch (Exception ex) { throw; }
        }

        //author:Mani
        //date: 24-Aug-2019
        //Asset Category Get
        public List<AssetCategories> GetAssetCategories()
        {
            try
            {
                var AssetCategories = _adminDB.GetAssetCategory().ToList();

                List<AssetCategories> AssetCategoriesData = AssetCategories.Select(x => new AssetCategories
                {
                    AssetCategoryId = x.int_asset_category_id,
                    AssetCategoryName = x.vc_asset_category,
                    AssetCategoryPrefix = x.vc_asset_category_prefix,
                    IsActive = x.bt_status,
                    UserId = x.int_created_by
                }).ToList();


                return AssetCategoriesData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //author:Mani
        //date: 24-Aug-2019
        //Update Asset Category Status
        public long? UpdateAssetCategoryStatus(long? assetCategoryId, bool? status)
        {
            try
            {
                long? result = 0;
                _adminDB.UpdateAssetCategoryStaus(assetCategoryId, status, ref result);
                return result;
            }
            catch { throw; }
        }


        //author:Mani
        //date: 24-Aug-2019
        //delete Asset Category
        public long? DeleteAssetCategory(long? assetCategoryId)
        {
            try
            {
                long? result = 0;
                _adminDB.DeleteAssetCategory(assetCategoryId, ref result);
                return result;
            }
            catch { throw; }
        }

        #endregion

        #region Configure Questionnaire

        //Author : Mani
        //Date : 11-Sep-2019
        //CQValidateUpsertAndGetResponse

        public Response<bool> CQValidateUpsertAndGetResponse(Response<bool> response, ConfigQuestions model, long? headerUserId)
        {
            try
            {
                //storing data in a variable, not to hit database multiple times.
                var Data = GetConfigQuestions();

                if (Data.Exists(Q => Q.SurveyPurposeId == model.SurveyPurposeId && Q.ConfigQuestion.ToLower() == model.ConfigQuestion.ToLower() && Q.ConfigQuestionId != model.ConfigQuestionId))
                {
                    response.Status = false;
                    response.Message = "Qestion already exists for survey type";

                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to add new question");
                }
                else if (UpsertConfigQuestion(model) > 0)
                {
                    if (model.ConfigQuestionId == 0)
                    {
                        response.Status = true;
                        response.Message = "Question configured successfully";

                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to add new question");
                        Logging.EventLog(headerUserId, "New question added successfully");
                    }
                    else
                    {
                        response.Status = true;
                        response.Message = "Question configuration updated successfully";
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update question");
                        Logging.EventLog(headerUserId, "Question updated successfully");
                    }
                }
                else
                {
                    if (model.ConfigQuestionId == 0)
                    {
                        response.Status = false;
                        response.Message = "Configuring question failed";
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to add new question");
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Updating question configuration failed";
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update question");
                    }
                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //author:Mani
        //date: 25-Aug-2019
        //Configure Questionnaire Upsert
        public long? UpsertConfigQuestion(ConfigQuestions model)
        {
            try
            {
                long? result = 0;
                _adminDB.UpsertConfigQuestion(model.ConfigQuestionId, model.SurveyPurposeId, model.ConfigQuestion, model.UserId, ref result);
                return result;
            }
            catch (Exception ex) { throw; }
        }

        //author:Mani
        //date: 25-Aug-2019
        //Configured Questionns Get
        public List<ConfigQuestions> GetConfigQuestions()
        {
            try
            {
                var ConfigQuestions = _adminDB.GetConfigQuestions().ToList();

                List<ConfigQuestions> ConfigQuestionsData = ConfigQuestions.Select(x => new ConfigQuestions
                {
                    ConfigQuestionId = x.int_configure_question_id,
                    SurveyPurposeId = x.int_survey_purpose_id,
                    ConfigQuestion = x.vc_configure_question,
                    SurveyPurposeName = x.vc_survey_purpose_name,
                    IsActive = x.bt_valid,
                    UserId = x.int_created_by
                }).ToList();


                return ConfigQuestionsData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //author:Mani
        //date: 25-Aug-2019
        //Configured Questionns Get
        public List<SelectedQuestions> GetConfigQuestions(long? quoteId, long? SurveyPurposeId)
        {
            try
            {
                List<SelectedQuestions> selectedQuestions = new List<SelectedQuestions>();
                List<GetConfigQuestionsResult> ConfigQuestions = new List<GetConfigQuestionsResult>();
                List<long?> selectedQuestionIds = new List<long?>();
                ConfigQuestions = _adminDB.GetConfigQuestions().Where(x=>x.int_survey_purpose_id == SurveyPurposeId).ToList();
                selectedQuestionIds = _quotesDB.GetQuoteQuestions(quoteId).Select(x=>x.questionId).ToList();
                foreach (var item in ConfigQuestions)
                {
                    SelectedQuestions selectedQuestion = new SelectedQuestions();
                    selectedQuestion.ConfigQuestionId = item.int_configure_question_id;
                    selectedQuestion.SurveyPurposeId = item.int_survey_purpose_id;
                    selectedQuestion.ConfigQuestion = item.vc_configure_question;
                    selectedQuestion.SurveyPurposeName = item.vc_survey_purpose_name;
                    selectedQuestion.IsActive = item.bt_valid;
                    selectedQuestion.UserId = item.int_created_by;
                    selectedQuestion.selected = selectedQuestionIds.Contains(item.int_configure_question_id) ? true : false;
                    selectedQuestions.Add(selectedQuestion);
                }
                return selectedQuestions;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //author:Mani
        //date: 25-Aug-2019
        //delete Configure Question
        public long? DeleteConfigQuestion(long? configQuestionId, long? userId)
        {
            try
            {
                long? result = 0;
                _adminDB.DeleteConfigQuestion(configQuestionId, userId, ref result);
                return result;
            }
            catch { throw; }
        }

        #endregion

        #region Leave Reasons

        //Author : Mani
        //Date : 11-Sep-2019
        //LRValidateUpsertAndGetResponse

        public Response<bool> LeaveReasonsValidateUpsertAndGetResponse(Response<bool> response, LeaveReasons model, long? headerUserId)
        {
            try
            {
                //storing data in a variable, not to hit database multiple times.
                var Data = GetLeaveReasons();
                var lookups = _adminDB.GetLookupOptions();
                model.TypeOfAbsence = lookups.Where(x => x.int_lookup_id == model.TypeOfAbsenceId).FirstOrDefault().vc_lookup_name;
                //model.TypeOfAbsence = Data.Where(x => x.TypeOfAbsenceId == model.TypeOfAbsenceId).Select(y => y.TypeOfAbsence).FirstOrDefault();

                if (Data.Exists(L => L.TypeOfAbsenceId == model.TypeOfAbsenceId && L.LeaveReason.ToLower() == model.LeaveReason.ToLower() && L.LeaveReasonId != model.LeaveReasonId))
                {
                    response.Status = false;
                    response.Message = "Leave reason already exists for absence type";
                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to add new leave reason");
                }
                else if (model.TypeOfAbsence != null && model.TypeOfAbsence != "" && model.TypeOfAbsence == "Working Remotely" && model.LeaveReason.ToLower() == "pto")
                {
                    response.Status = false;
                    response.Message = "PTO cannot be added as leave reason under Working Remotely";
                    //Event logging
                    Logging.EventLog(headerUserId, "Attempt to add new leave reason");
                }
                else
                {
                    long? result = UpsertLeaveReason(model);
                    if (result > 0)
                    {
                        if (model.LeaveReasonId == 0)
                        {
                            response.Status = true;
                            response.Message = "Leave reason added successfully";
                            //Event logging
                            Logging.EventLog(headerUserId, "Attempt to add new leave reason");
                            Logging.EventLog(headerUserId, "Leave reason added successfully");
                        }
                        else
                        {
                            response.Status = true;
                            response.Message = "Leave reason updated successfully";
                            //Event logging
                            Logging.EventLog(headerUserId, "Attempt to update leave reason");
                            Logging.EventLog(headerUserId, "Leave reason updated successfully");
                        }
                    }
                    else if (result == -1)
                    {
                        response.Status = false;
                        response.Message = "Leave reason already exists";
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to add new leave reason");
                    }
                    else if (model.LeaveReasonId == 0)
                    {
                        response.Status = false;
                        response.Message = "Adding leave reason failed";
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to add new leave reason");

                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Updating leave reason failed";
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to update leave reason");

                    }
                }
                //else
                //{
                //    if (model.LeaveReasonId == 0)
                //    {
                //        response.Status = false;
                //        response.Message = "Adding leave reason failed";
                //        //Event logging
                //        Logging.EventLog(headerUserId, "Attempt to add new leave reason");

                //    }
                //    else
                //    {
                //        response.Status = false;
                //        response.Message = "Updating leave reason failed";
                //        //Event logging
                //        Logging.EventLog(headerUserId, "Attempt to update leave reason");

                //    }
                //}
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        //author:Mani
        //date: 29-Aug-2019
        //Leave Reason Upsert
        public long? UpsertLeaveReason(LeaveReasons model)
        {
            try
            {
                if (model.LeaveReason.ToLower() == "pto")
                    model.LeaveReason = model.LeaveReason.ToUpper();

                long? result = 0;
                _adminDB.UpsertLeaveReason(model.LeaveReasonId, model.TypeOfAbsenceId, model.LeaveReason, model.UserId, ref result);
                return result;
            }
            catch (Exception ex) { throw; }
        }

        //author:Mani
        //date: 29-Aug-2019
        //Get Leave Reasons
        public List<LeaveReasons> GetLeaveReasons()
        {
            try
            {
                var LeaveReasons = _adminDB.GetLeaveReasons().ToList();

                List<LeaveReasons> LeaveReasonsData = LeaveReasons.Select(x => new LeaveReasons
                {
                    LeaveReasonId = x.int_leave_reason_id,
                    TypeOfAbsenceId = x.int_type_of_absence_id,
                    LeaveReason = x.vc_leave_reason,
                    TypeOfAbsence = x.vc_absence_type,
                    IsActive = x.bt_valid,
                    UserId = x.int_created_by
                }).ToList();


                return LeaveReasonsData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //author:Mani
        //date: 29-Aug-2019
        //delete leave reasons
        public long? DeleteLeaveReason(long? leaveReasonId, long? userId)
        {
            try
            {
                long? result = 0;
                _adminDB.DeleteLeaveReason(leaveReasonId, userId, ref result);
                return result;
            }
            catch { throw; }
        }

        #endregion


        #region Leave Requests
        //Author Name   : Siddhant Chawade
        //Created Date  : 13th Nov 2019
        //Description   : Get Leaves Activity Log data by filter
        public GetLeaveRequests GetLeaveActivityLogFilter(long UserId, long? AbsenceTypeId, long? LeaveReasonId, long? ActivityTypeId, long? UserNameId, DateTime? FromDate, DateTime? ToDate)
        {
            try
            {
                bool isReportingManager = false;
                var usersList = GetUsers();
                List<long?> AssociateduserIds = usersList.Where(x => x.reportingManagerId == UserId).Select(u => u.userId).ToList();
                isReportingManager = AssociateduserIds.Count > 0 ? true : false;
                //var LeaveActivityLog = _adminDB.GetLeavesActivityLog(UserId).OrderByDescending(x => x.dt_created_on).ToList();
                var LeaveActivityLog = _adminDB.GetLeavesActivityLog(UserId).ToList();
                GetLeaveRequests LeavesActivityLogData = new GetLeaveRequests();
                LeavesActivityLogData.ReportingManager = _adminDB.getProjectManagerofUser(UserId).Select(x => x.vc_user_name).FirstOrDefault();
                LeavesActivityLogData.isReportingManager = isReportingManager;
                var lookupOptions = _adminDB.GetLookupOptions().Select(x => new LookupOptions { CodeMasterName = x.vc_code_master_name, LookupId = x.int_lookup_id, LookupName = x.vc_lookup_name }).OrderBy(x => x.LookupName).ToList();
                LeavesActivityLogData.AbsenceTypeList = lookupOptions.Where(x => x.CodeMasterName == "Absence Types").ToList();
                LeavesActivityLogData.ActivityTypeList = lookupOptions.Where(x => x.CodeMasterName == "Leave_Activity_Log_Activities").ToList();
                var users = LeaveActivityLog.AsEnumerable().Select(x => new IBWUsers { userId = x.int_leave_user_id, userName = x.vc_leave_user_name }).ToList();
                users = users.Distinct().ToList();
                var userList = new List<IBWUsers>();
                var userIds = new List<long?>();
                foreach (var user in users)
                {
                    if (!userIds.Contains(user.userId))
                    {
                        userIds.Add(user.userId);
                        userList.Add(user);
                    }
                }
                if (users.Count == 0)
                {
                    var currentUser = usersList.Where(x => x.userId == UserId).Select(x => new IBWUsers { userId = x.userId, userName = x.userName }).FirstOrDefault();
                    if (currentUser != null)
                        userList.Add(currentUser);
                }
                LeavesActivityLogData.UserNameList = userList;
                if (AbsenceTypeId != null && AbsenceTypeId != 0)
                {
                    LeaveActivityLog = LeaveActivityLog.Where(x => x.int_absence_type_id == AbsenceTypeId).ToList();
                }
                if (LeaveReasonId != null && LeaveReasonId != 0)
                {
                    LeaveActivityLog = LeaveActivityLog.Where(x => x.int_leave_reason_id == LeaveReasonId).ToList();
                }
                if (ActivityTypeId != null && ActivityTypeId != 0)
                {
                    LeaveActivityLog = LeaveActivityLog.Where(x => x.int_activity_type_id == ActivityTypeId).ToList();
                }
                if (UserNameId != null && UserNameId != 0)
                {
                    LeaveActivityLog = LeaveActivityLog.Where(x => x.int_leave_user_id == UserNameId).ToList();
                }
                if (FromDate != null && FromDate != Convert.ToDateTime("1/1/1111"))
                {
                    LeaveActivityLog = LeaveActivityLog.Where(x => Convert.ToDateTime(x.dt_from_date) >= FromDate).ToList();
                }
                if (ToDate != null && ToDate != Convert.ToDateTime("1/1/1111"))
                {
                    LeaveActivityLog = LeaveActivityLog.Where(x => Convert.ToDateTime(x.dt_from_date) <= ToDate).ToList();
                }

                List<LeaveRequests> LeaveActivityLogList = LeaveActivityLog.Select(x => new LeaveRequests
                {
                    LeaveRequestId = Convert.ToInt64(x.int_leave_request_id),
                    CreatedDateString = getDateTimeString(x.dt_created_on),
                    ActivityType = x.vc_activity_type,
                    TypeOfAbsence = x.vc_absence_type,
                    LeaveReason = x.vc_leave_reason_type,
                    FromDateString = x.dt_from_date,
                    ToDateString = x.dt_to_date,
                    ActualPaidHours = x.int_actual_paid_leaves,
                    userName = x.vc_user_name,
                    FromUserName = x.vc_from_user_name,
                    ToUserName = x.vc_to_user_name,
                    Remarks = x.vc_remarks,
                    LeaveUserName = x.vc_leave_user_name,
                    Notes = x.vc_notes,
                    //Using IsSelected for is_read property
                    IsSelected = (x.bt_is_read == null || x.bt_is_read == false) ? false : true
                }).ToList();

                if (LeaveActivityLogList.Count > 0)
                {
                    long? leaveRequestId = LeaveActivityLogList[0].LeaveRequestId;
                    string currentColor = "white";
                    foreach (var item in LeaveActivityLogList)
                    {
                        if (leaveRequestId != item.LeaveRequestId)
                        {
                            currentColor = currentColor == "white" ? "gray" : "white";
                        }
                        item.Color = currentColor;
                        leaveRequestId = item.LeaveRequestId;
                    }
                }
                LeavesActivityLogData.ActivityLeaveRequestsList = LeaveActivityLogList;
                return LeavesActivityLogData;
            }
            catch { throw; }
        }
        //Method to check existence for Full Day and PTO
        public bool FullDayPTOExistenceCheck(GetLeaveRequests Data, LeaveRequests model)
        {
            try
            {
                return Data.ActivityLeaveRequestsList.Exists(x => x.LeaveRequestId != model.LeaveRequestId &&
                             (x.TypeOfAbsence == "Full Day" || x.TypeOfAbsence == "Multiple Days")
                             &&
                              ((x.TypeOfAbsence == "Full Day")

                               ?
                                DateTime.Parse(x.FromDate.Value.ToString("MM/dd/yyyy")) == DateTime.Parse(model.FromDate.Value.ToString("MM/dd/yyyy"))
                               :
                                (DateTime.Parse(x.FromDate.Value.ToString("MM/dd/yyyy")) <= DateTime.Parse(model.FromDate.Value.ToString("MM/dd/yyyy"))
                                 &&
                                 DateTime.Parse(x.ToDate.Value.ToString("MM/dd/yyyy")) >= DateTime.Parse(model.FromDate.Value.ToString("MM/dd/yyyy"))
                                 )
                                 )
                                 );
            }
            catch (Exception)
            {

                throw;
            }

        }

        //Method to check existence for multiple days
        public bool MultipleDaysExistenceCheck(GetLeaveRequests Data, LeaveRequests model)
        {
            try
            {
                return Data.ActivityLeaveRequestsList.Exists(x => x.LeaveRequestId != model.LeaveRequestId &&

                             (x.TypeOfAbsence == "Full Day" || x.TypeOfAbsence == "Multiple Days")
                             &&
                             (x.TypeOfAbsence == "Multiple Days"
                             ?
                             (
                             (DateTime.Parse(x.FromDate.Value.ToString("MM/dd/yyyy")) >= DateTime.Parse(model.FromDate.Value.ToString("MM/dd/yyyy"))
                             &&
                             DateTime.Parse(x.FromDate.Value.ToString("MM/dd/yyyy")) <= DateTime.Parse(model.ToDate.Value.ToString("MM/dd/yyyy"))
                             )
                             ||
                             (DateTime.Parse(x.ToDate.Value.ToString("MM/dd/yyyy")) >= DateTime.Parse(model.FromDate.Value.ToString("MM/dd/yyyy"))
                             &&
                             DateTime.Parse(x.ToDate.Value.ToString("MM/dd/yyyy")) <= DateTime.Parse(model.ToDate.Value.ToString("MM/dd/yyyy"))
                             )
                             )

                             :

                             (DateTime.Parse(x.FromDate.Value.ToString("MM/dd/yyyy")) >= DateTime.Parse(model.FromDate.Value.ToString("MM/dd/yyyy"))
                             &&
                             DateTime.Parse(x.FromDate.Value.ToString("MM/dd/yyyy")) <= DateTime.Parse(model.ToDate.Value.ToString("MM/dd/yyyy"))
                             )
                             )
                              );
            }

            finally { }
        }


        //Author : Mani
        //Date : 11-Sep-2019
        //LRValidateUpsertAndGetResponse

        public Response<bool> LeaveRequestValidateUpsertAndGetResponse(Response<bool> response, LeaveRequests model, long? headerUserId)
        {
            try
            {
                //storing data in a variable, not to hit database multiple times.
                var Data = GetLeaveRequests(headerUserId, true, null, null, null, null, null);
                bool IsAlreadyApplied = false;

                //  validation block for Full day and PTO 
                if (model.TypeOfAbsence == "Full Day" && Data.ActivityLeaveRequestsList.Count() > 0 &&
                    ((
                    (Data.ActivityLeaveRequestsList.Exists(x => x.LeaveRequestId == model.LeaveRequestId))
                     && FullDayPTOExistenceCheck(Data, model)
                    )
                    ||
                    (
                    model.LeaveRequestId == 0 && FullDayPTOExistenceCheck(Data, model)
                    ))
                   )
                {

                    //  block for to get dates on which leave is already applied
                    foreach (var leave in Data.ActivityLeaveRequestsList)
                    {
                        IsAlreadyApplied = false;

                        if (leave.TypeOfAbsence == "Full Day" || leave.TypeOfAbsence == "Multiple Days")
                        {
                            if (leave.TypeOfAbsence == "Full Day"
                                &&
                                DateTime.Parse(leave.FromDate.Value.ToString("yyyy/MM/dd")) == DateTime.Parse(model.FromDate.Value.ToString("yyyy/MM/dd")))
                            {
                                response.Message = "You have already applied leave on " + leave.FromDate.Value.ToString("yyyy/MM/dd");
                                IsAlreadyApplied = true;
                            }
                            else if (leave.TypeOfAbsence == "Multiple Days" && (DateTime.Parse(leave.FromDate.Value.ToString("yyyy/MM/dd")) <= DateTime.Parse(model.FromDate.Value.ToString("yyyy/MM/dd"))
                                     &&
                                        DateTime.Parse(leave.ToDate.Value.ToString("yyyy/MM/dd")) >= DateTime.Parse(model.FromDate.Value.ToString("yyyy/MM/dd")))
                                     )
                            {
                                response.Message = "You have already applied for leave between " + leave.FromDate.Value.ToString("yyyy/MM/dd") + " To " + leave.ToDate.Value.ToString("yyyy/MM/dd");
                                IsAlreadyApplied = true;
                            }
                        }
                        if (IsAlreadyApplied)
                            break;
                    }


                    response.Status = false;

                }

                //Block to validate multiple days 
                else if (model.TypeOfAbsence == "Multiple Days" && Data.ActivityLeaveRequestsList.Count() > 0 &&
                         ((
                         model.LeaveRequestId == 0 && MultipleDaysExistenceCheck(Data, model)
                         )
                        ||
                        (
                        Data.ActivityLeaveRequestsList.Exists(x => x.LeaveRequestId == model.LeaveRequestId)
                        && MultipleDaysExistenceCheck(Data, model)
                        ))
                       )

                {
                    //  block for to get dates on which leave is already applied
                    foreach (var leave in Data.ActivityLeaveRequestsList)
                    {
                        IsAlreadyApplied = false;

                        if (leave.TypeOfAbsence == "Full Day" || leave.TypeOfAbsence == "Multiple Days")
                        {
                            if (leave.TypeOfAbsence == "Full Day"
                                &&
                                 (DateTime.Parse(leave.FromDate.Value.ToString("yyyy/MM/dd")) >= DateTime.Parse(model.FromDate.Value.ToString("yyyy/MM/dd"))
                                    &&
                                    DateTime.Parse(leave.FromDate.Value.ToString("yyyy/MM/dd")) <= DateTime.Parse(model.ToDate.Value.ToString("yyyy/MM/dd"))
                                    )

                                )
                            {
                                response.Message = "You have already applied leave on " + leave.FromDate.Value.ToString("yyyy/MM/dd");
                                IsAlreadyApplied = true;
                            }
                            else if (leave.TypeOfAbsence == "Multiple Days" &&
                                 ((
                DateTime.Parse(leave.FromDate.Value.ToString("yyyy/MM/dd")) >= DateTime.Parse(model.FromDate.Value.ToString("yyyy/MM/dd"))
                &&
                DateTime.Parse(leave.FromDate.Value.ToString("yyyy/MM/dd")) <= DateTime.Parse(model.ToDate.Value.ToString("yyyy/MM/dd"))
                )
                ||
                (DateTime.Parse(leave.ToDate.Value.ToString("yyyy/MM/dd")) >= DateTime.Parse(model.FromDate.Value.ToString("yyyy/MM/dd"))
                &&
                DateTime.Parse(leave.ToDate.Value.ToString("yyyy/MM/dd")) <= DateTime.Parse(model.ToDate.Value.ToString("yyyy/MM/dd"))
                       )
                )
                                )
                            {
                                response.Message = "You have already applied for leave between " + leave.FromDate.Value.ToString("yyyy/MM/dd") + " To " + leave.ToDate.Value.ToString("yyyy/MM/dd");
                                IsAlreadyApplied = true;
                            }
                        }
                        if (IsAlreadyApplied)
                            break;
                    }

                    response.Status = false;
                    //response.Message = "You have already applied leave for a day(s) in this range please check and reapply";
                }

                //Block to check Actual paid hours conditions
                else if ((model.TypeOfAbsence == "Full Day" || model.TypeOfAbsence == "Multiple Days") && ActualPaidHoursFailChecking(model, response))
                {
                    response.Message = "Actual paid hours cannot be greater than defined hours per day.";
                    IsAlreadyApplied = false;
                }
                //Block to upsert the leave request
                else if (UpsertLeaveRequest(model, headerUserId) > 0)
                {
                    if (model.LeaveRequestId == 0)
                    {
                        response.Status = true;
                        response.Message = "Leave request added successfully";
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to make a leave request");
                        Logging.EventLog(headerUserId, "Leave request has been made successfully");
                    }
                    else
                    {
                        response.Status = true;
                        response.Message = "Leave request updated successfully";
                    }
                }
                else
                {
                    if (model.LeaveRequestId == 0)
                    {
                        response.Status = false;
                        response.Message = "Adding leave request failed";
                        //Event logging
                        Logging.EventLog(headerUserId, "Attempt to make a leave request");
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Updating leave request failed";
                    }
                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;

                response.Status = false;
                response.Message = "Something went wrong please contact administrator";
            }
        }

        public List<sp_getAllIBWUsersResult> GetAllIBWUsers()
        {
            try
            {
                return _adminDB.sp_getAllIBWUsers().ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool ActualPaidHoursFailChecking(LeaveRequests model, Response<bool> response)
        {
            try
            {
                int duration = 0;
                model.ActualPaidHours = model.ActualPaidHours == null ? 0 : model.ActualPaidHours;
                bool IsHoliday = false;
                decimal? UserHoursPerDay = GetAllIBWUsers().Where(x => x.int_user_id == model.UserId).Select(y => y.int_hrs_per_day).FirstOrDefault();

                if (model.TypeOfAbsence == "Full Day")
                {

                    duration = 1;
                    var holidaysList = _adminDB.GetHolidays().Select(x => x.holidayDate).ToList();
                    if (holidaysList.Exists(x => x == model.FromDate))
                    {
                        duration = 0;

                        IsHoliday = true;
                        model.ActualPaidHours = 0;
                    }

                    if (model.FromDate.Value.DayOfWeek == DayOfWeek.Sunday || model.FromDate.Value.DayOfWeek == DayOfWeek.Saturday)
                        duration = 0;

                }
                else if (model.TypeOfAbsence == "Multiple Days")
                {

                    DateTime start = (DateTime)model.FromDate;
                    DateTime end = (DateTime)model.ToDate;

                    var Dateslist1 = Enumerable.Range(0, 1 + end.Subtract(start).Days).Select(day => start.AddDays(day)).ToList();
                    List<DateTime> WorkingDates = new List<DateTime>();
                    List<DateTime> ValidDates = new List<DateTime>();
                    var holidaysList = _adminDB.GetHolidays().Select(x => x.holidayDate).ToList();


                    WorkingDates = Dateslist1.Where(x => x.DayOfWeek != DayOfWeek.Sunday && x.DayOfWeek != DayOfWeek.Saturday).ToList();


                    foreach (DateTime date in WorkingDates)
                    {
                        if (!holidaysList.Exists(x => x.Value.Date == date.Date))
                            ValidDates.Add(date);
                    }

                    duration = ValidDates.Count();
                    //var AppliedLeaves = GetLeaveRequests(model.UserId).ActivityLeaveRequestsList.Where(x => x.FromDate == (DateTime)model.FromDate
                    //|| (x.FromDate >= (DateTime)model.FromDate && x.ToDate <= (DateTime)model.ToDate)).ToList();

                }
                if (IsHoliday == true)
                {
                    return false;
                }
                else
                {
                    return ((duration * UserHoursPerDay) < model.ActualPaidHours);
                }
            }
            catch (Exception ex)
            {

                throw ex;

                response.Status = false;
                response.Message = "Something went wrong please contact administrator";
            }
        }

        //author:Mani
        //date: 30-Aug-2019
        //Leave Request Upsert
        public long? UpsertLeaveRequest(LeaveRequests model, long? userId)
        {
            try
            {
                long? result = 0;

                //  if(model.IsHoliday)


                _adminDB.UpsertLeaveRequest(model.LeaveRequestId, model.TypeOfAbsenceId, model.TypeOfAbsence, model.LeavesCount, model.LeaveReasonId,
                    model.FromDate, model.ActualPaidHours, model.ToDate, model.FromTime,
                model.ToTime, model.IsFullDay, model.UserId, userId, model.AuthorisedById, model.Remarks, model.valuesUpdated, model.Notes, ref result);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //author:Mani
        //date: 01-Sep-2019
        //Review leave request
        public long? ReviewLeaveRequest(List<LeaveRequests> model)
        {
            try
            {
                long? result = 0;
                foreach (var item in model)
                {
                    _adminDB.ReviewLeaveRequest(item.LeaveRequestId, item.UserId, item.ActualPaidHours
                        , item.AuthorisedById, item.IsApproved, item.IsDeclined, item.Remarks, item.LeaveReason, ref result);
                }

                //_adminDB.ReviewLeaveRequest(model.LeaveRequestId, model.UserId, model.LeavesCount, model.AuthorisedById, model.IsApproved, 
                //    model.IsDeclined, model.TypeOfAbsence, ref result);

                return result;
            }
            catch { throw; }
        }

        //author:Mani
        //date: 30-Aug-2019
        //Get Leave Requests
        public GetLeaveRequests GetLeaveRequests(long? UserId, bool? isSuperAdminThere, long? filterRMId, long? filterUserId, DateTime? filterFromDate, DateTime? filterToDate, int? type)
        {
            try
            {
                GetLeaveRequests LeaveRequests = new Models.GetLeaveRequests();
                var usersList = GetUsers();
                List<IBWUsers> RMs = new List<IBWUsers>();
                List<long?> RMIds = new List<long?>();
                //All leave requests
                List<GetLeaveRequestsResult> AllLeaveRequests = _adminDB.GetLeaveRequests().ToList();

                //getting list of reporting managers
                if (type == 1)
                    RMIds = AllLeaveRequests.Where(x => x.is_approved == false && x.is_declined == false).Select(x => x.int_rm_id).Distinct().ToList();
                else if (type == 2)
                    RMIds = AllLeaveRequests.Where(x => x.is_approved == false && x.is_declined == true).Select(x => x.int_rm_id).Distinct().ToList();
                else if (type == 3)
                    RMIds = AllLeaveRequests.Where(x => x.is_approved == true && x.is_declined == false).Select(x => x.int_rm_id).Distinct().ToList();
                else
                    RMIds = AllLeaveRequests.Select(x => x.int_rm_id).Distinct().ToList();
                foreach (var id in RMIds)
                {
                    if (id != null && id != 1)
                    {
                        var rm = usersList.Where(x => x.userId == id).FirstOrDefault();
                        RMs.Add(rm);
                    }
                }

                //getting list of users
                var users = new List<IBWUsers>();
                if (type == 1)
                    users = AllLeaveRequests.Where(x => x.is_approved == false && x.is_declined == false).AsEnumerable().Select(x => new IBWUsers { userId = x.int_created_by, userName = x.vc_user_name }).ToList();
                else if (type == 2)
                    users = AllLeaveRequests.Where(x => x.is_approved == false && x.is_declined == true).AsEnumerable().Select(x => new IBWUsers { userId = x.int_created_by, userName = x.vc_user_name }).ToList();
                else if (type == 3)
                    users = AllLeaveRequests.Where(x => x.is_approved == true && x.is_declined == false).AsEnumerable().Select(x => new IBWUsers { userId = x.int_created_by, userName = x.vc_user_name }).ToList();
                else
                    users = AllLeaveRequests.AsEnumerable().Select(x => new IBWUsers { userId = x.int_created_by, userName = x.vc_user_name }).ToList();
                users = users.Distinct().ToList();
                var userList = new List<IBWUsers>();
                var userIds = new List<long?>();
                foreach (var user in users)
                {
                    if (!userIds.Contains(user.userId))
                    {
                        if (user.userId != 1)
                        {
                            userIds.Add(user.userId);
                            userList.Add(user);
                        }
                    }
                }
                LeaveRequests.UserNameList = userList.OrderBy(x => x.userName).ToList();
                LeaveRequests.RMs = RMs.OrderBy(x => x.userName).ToList();

                //applying filter
                if (filterRMId != null)
                {
                    AllLeaveRequests = AllLeaveRequests.Where(x => x.int_rm_id == filterRMId).ToList();
                }
                if (filterUserId != null)
                {
                    AllLeaveRequests = AllLeaveRequests.Where(x => x.int_created_by == filterUserId).ToList();
                }
                if (filterFromDate != null && filterFromDate != Convert.ToDateTime("1/1/1111"))
                {
                    AllLeaveRequests = AllLeaveRequests.Where(x => Convert.ToDateTime(x.dt_from_date) >= filterFromDate).ToList();
                }
                if (filterToDate != null && filterToDate != Convert.ToDateTime("1/1/1111"))
                {
                    AllLeaveRequests = AllLeaveRequests.Where(x => Convert.ToDateTime(x.dt_from_date) <= filterToDate).ToList();
                }
                if (isSuperAdminThere == true)
                {
                    bool IsManager = false;
                    bool IsSuperAdmin = true;
                    List<LeaveRequests> AssociateUsersLeaveRequestsList = new List<LeaveRequests>();

                    //Logged in user leave requests
                    List<GetLeaveRequestsResult> ActivityLeaveRequests = AllLeaveRequests.Where(x => x.int_created_by == UserId).ToList();

                    var AllUserJobCodes = _adminDB.sp_getJobCodesOfUser().ToList();

                    // If logged in user is a manger, we will get all the reporting users leave requests
                    if (IsSuperAdmin)
                    {
                        //getting all the associated userIds of the logged in manager
                        List<long?> AssociateduserIds = GetUsers().Where(x => x.reportingManagerId == UserId).Select(u => u.userId).ToList();
                        //AssociateduserIds.Add(UserId);

                        //getting all the leave requests of the associated users
                        List<GetLeaveRequestsResult> AssociatedUsersLeaveRequests = new List<GetLeaveRequestsResult>();
                        if (IsSuperAdmin)
                        {
                            AssociatedUsersLeaveRequests = AllLeaveRequests;
                            IsManager = true;
                        }

                        //Preparing associated users leave requests list of logged in user if manager status is true 
                        AssociateUsersLeaveRequestsList = AssociatedUsersLeaveRequests.Select(x => new LeaveRequests
                        {
                            UserJobIds = AllUserJobCodes.Where(j => j.int_user_id == x.int_created_by).Select(s => s.int_job_code_master_id).ToList(),
                            LeaveRequestId = x.int_leave_request_id,
                            TypeOfAbsenceId = x.int_type_of_absence_id,
                            TypeOfAbsence = x.vc_absence_type,
                            LeaveReasonId = x.int_leave_reason_id,
                            LeaveReason = x.vc_reason,
                            FromDate = x.dt_from_date,
                            ToDate = x.dt_to_date,
                            FromTime = x.tm_from_time,
                            ToTime = x.tm_to_time,
                            IsApproved = x.is_approved,
                            IsDeclined = x.is_declined,
                            IsFullDay = x.is_full_day,
                            AuthorisedById = x.int_authorized_by,
                            TotalLeaveBalance = x.intTotalLeaveBalance,
                            TimeDuration = string.Format("{0:00}:{1:00}", (x.timeDuration / 60), (x.timeDuration % 60)),
                            ActualPaidHours = x.int_duration,
                            PTOLeaveBalance = x.intPtoLeaveBalance,
                            userName = x.vc_user_name,
                            UserId = x.int_created_by,
                            ReportingManager = x.vc_reporting_manager_name,
                            CreatedDateTime = x.dt_created_on,
                            Remarks = x.vc_leave_activity_log_remarks,
                            PreLeaveBalance = x.int_pre_leave_balance,
                            PrePTOBalance = x.int_pre_pto_balance,
                            Notes = x.vc_notes
                        }).ToList();
                    }

                    //Preparing logged in user leave requests list
                    List<LeaveRequests> ActivityLeaveRequestsList = ActivityLeaveRequests.Select(x => new LeaveRequests
                    {
                        UserJobIds = AllUserJobCodes.Where(j => j.int_user_id == x.int_created_by).Select(s => s.int_job_code_master_id).ToList(),
                        LeaveRequestId = x.int_leave_request_id,
                        TypeOfAbsenceId = x.int_type_of_absence_id,
                        TypeOfAbsence = x.vc_absence_type,
                        LeaveReasonId = x.int_leave_reason_id,
                        LeaveReason = x.vc_reason,
                        FromDate = x.dt_from_date,
                        ToDate = x.dt_to_date,
                        FromTime = x.tm_from_time,
                        ToTime = x.tm_to_time,
                        IsApproved = x.is_approved,
                        IsDeclined = x.is_declined,
                        IsFullDay = x.is_full_day,
                        TotalLeaveBalance = x.intTotalLeaveBalance,
                        PTOLeaveBalance = x.intPtoLeaveBalance,
                        AuthorisedById = x.int_authorized_by,
                        TimeDuration = string.Format("{0:00}:{1:00}", (x.timeDuration / 60), (x.timeDuration % 60)),
                        ActualPaidHours = x.int_duration,
                        userName = x.vc_user_name,
                        UserId = x.int_created_by,
                        ReportingManager = x.vc_reporting_manager_name,
                        CreatedDateTime = x.dt_created_on,
                        Remarks = x.vc_leave_activity_log_remarks,
                        PreLeaveBalance = x.int_pre_leave_balance,
                        PrePTOBalance = x.int_pre_pto_balance,
                        Notes = x.vc_notes
                    }).ToList();

                    //Preparing main list for entire leave requests


                    DateTime startOfWeek = DateTime.Today;
                    int delta = DayOfWeek.Monday - startOfWeek.DayOfWeek;
                    startOfWeek = startOfWeek.AddDays(delta);

                    DateTime endOfWeek = startOfWeek.AddDays(7);

                    DateTime now = DateTime.Now;



                    LeaveRequests.IsManager = IsManager;
                    LeaveRequests.ReportingManager = _adminDB.getProjectManagerofUser(UserId).Select(x => x.vc_user_name).FirstOrDefault();
                    LeaveRequests.ActivityLeaveRequestsList = ActivityLeaveRequestsList.OrderByDescending(x => x.CreatedDateTime).ToList();
                    LeaveRequests.AssociateUsersLeaveRequestsList = AssociateUsersLeaveRequestsList.OrderByDescending(x => x.CreatedDateTime).ToList();

                    LeaveRequests.TodayAssociateUsersLeaveRequestsList = LeaveRequests.AssociateUsersLeaveRequestsList.Where(x => checkFromandToDateToday(Convert.ToDateTime(x.FromDate), Convert.ToDateTime(x.ToDate)) == true && x.IsDelete == false).ToList();
                    LeaveRequests.TomorrowAssociateUsersLeaveRequestsList = LeaveRequests.AssociateUsersLeaveRequestsList.Where(x => checkFromandToDateTomorrow(Convert.ToDateTime(x.FromDate), Convert.ToDateTime(x.ToDate)) == true && x.IsDelete == false).ToList();
                    LeaveRequests.ThisWeeksAssociateUsersLeaveRequestsList = LeaveRequests.AssociateUsersLeaveRequestsList.Where(x => checkFromandToDateThisWeek(Convert.ToDateTime(x.FromDate), Convert.ToDateTime(x.ToDate)) == true && x.IsDelete == false).ToList();
                    LeaveRequests.ThisMonthsAssociateUsersLeaveRequestsList = LeaveRequests.AssociateUsersLeaveRequestsList.Where(x => checkFromandToDateThisMonth(Convert.ToDateTime(x.FromDate), Convert.ToDateTime(x.ToDate)) == true && x.IsDelete == false).ToList();
                    LeaveRequests.AllLeavesForCalendar = AssociateUsersLeaveRequestsList;
                    return LeaveRequests;
                }

                else
                {
                    bool IsManager = false;
                    bool IsSuperAdmin = false;
                    List<LeaveRequests> AllLeavesForCalendar = new List<Models.LeaveRequests>();
                    List<LeaveRequests> AssociateUsersLeaveRequestsList = new List<Models.LeaveRequests>();

                    //All leave requests
                    //List<GetLeaveRequestsResult> AllLeaveRequests = _adminDB.GetLeaveRequests().ToList();

                    //Logged in user leave requests
                    List<GetLeaveRequestsResult> ActivityLeaveRequests = AllLeaveRequests.Where(x => x.int_created_by == UserId).ToList();

                    //manager status checking

                    IsManager = getAllReportingManagers().Exists(m => m.userId == UserId);

                    //checking SuperAdmin status
                    IsSuperAdmin = _adminDB.sp_getRoleOfUser().Where(s => s.int_user_id == UserId).Select(x => x.vc_role_name).FirstOrDefault() == "Super Admin";

                    var AllUserJobCodes = _adminDB.sp_getJobCodesOfUser().ToList();

                    // If logged in user is a manger, we will get all the reporting users leave requests
                    if (IsManager || IsSuperAdmin)
                    {
                        //getting all the associated userIds of the logged in manager
                        List<long?> AssociateduserIds = GetUsers().Where(x => x.reportingManagerId == UserId).Select(u => u.userId).ToList();
                        //AssociateduserIds.Add(UserId);

                        //getting all the leave requests of the associated users
                        List<GetLeaveRequestsResult> AssociatedUsersLeaveRequests = new List<GetLeaveRequestsResult>();

                        if (IsSuperAdmin)
                        {
                            AssociatedUsersLeaveRequests = AllLeaveRequests;
                            IsManager = true;
                        }
                        else
                        {
                            AssociatedUsersLeaveRequests = AllLeaveRequests;//.Where(x => AssociateduserIds.Contains(x.int_created_by)).ToList();
                            IsManager = true;
                        }


                        //Preparing associated users leave requests list of logged in user if manager status is true 
                        AllLeavesForCalendar = AssociateUsersLeaveRequestsList = AssociatedUsersLeaveRequests.Select(x => new LeaveRequests
                        {
                            UserJobIds = AllUserJobCodes.Where(j => j.int_user_id == x.int_created_by).Select(s => s.int_job_code_master_id).ToList(),
                            LeaveRequestId = x.int_leave_request_id,
                            TypeOfAbsenceId = x.int_type_of_absence_id,
                            TypeOfAbsence = x.vc_absence_type,
                            LeaveReasonId = x.int_leave_reason_id,
                            LeaveReason = x.vc_reason,
                            FromDate = x.dt_from_date,
                            ToDate = x.dt_to_date,
                            FromTime = x.tm_from_time,
                            ToTime = x.tm_to_time,
                            IsApproved = x.is_approved,
                            IsDeclined = x.is_declined,
                            IsFullDay = x.is_full_day,
                            AuthorisedById = x.int_authorized_by,
                            TotalLeaveBalance = x.intTotalLeaveBalance,
                            TimeDuration = string.Format("{0:00}:{1:00}", (x.timeDuration / 60), (x.timeDuration % 60)),
                            ActualPaidHours = x.int_duration,
                            PTOLeaveBalance = x.intPtoLeaveBalance,
                            userName = x.vc_user_name,
                            UserId = x.int_created_by,
                            ReportingManager = x.vc_reporting_manager_name,
                            CreatedDateTime = x.dt_created_on,
                            Remarks = x.vc_leave_activity_log_remarks,
                            PreLeaveBalance = x.int_pre_leave_balance,
                            PrePTOBalance = x.int_pre_pto_balance,
                            Notes = x.vc_notes

                        }).ToList();
                        if (!IsSuperAdmin)
                        {
                            AssociateUsersLeaveRequestsList = AssociateUsersLeaveRequestsList.Where(x => AssociateduserIds.Contains(x.UserId)).ToList();
                            IsManager = true;
                        }
                    }
                    //Preparing logged in user leave requests list
                    List<LeaveRequests> ActivityLeaveRequestsList = ActivityLeaveRequests.Select(x => new LeaveRequests
                    {
                        UserJobIds = AllUserJobCodes.Where(j => j.int_user_id == x.int_created_by).Select(s => s.int_job_code_master_id).ToList(),
                        LeaveRequestId = x.int_leave_request_id,
                        TypeOfAbsenceId = x.int_type_of_absence_id,
                        TypeOfAbsence = x.vc_absence_type,
                        LeaveReasonId = x.int_leave_reason_id,
                        LeaveReason = x.vc_reason,
                        FromDate = x.dt_from_date,
                        ToDate = x.dt_to_date,
                        FromTime = x.tm_from_time,
                        ToTime = x.tm_to_time,
                        IsApproved = x.is_approved,
                        IsDeclined = x.is_declined,
                        IsFullDay = x.is_full_day,
                        TotalLeaveBalance = x.intTotalLeaveBalance,
                        PTOLeaveBalance = x.intPtoLeaveBalance,
                        AuthorisedById = x.int_authorized_by,
                        TimeDuration = string.Format("{0:00}:{1:00}", (x.timeDuration / 60), (x.timeDuration % 60)),
                        ActualPaidHours = x.int_duration,
                        userName = x.vc_user_name,
                        UserId = x.int_created_by,
                        ReportingManager = x.vc_reporting_manager_name,
                        CreatedDateTime = x.dt_created_on,
                        Remarks = x.vc_leave_activity_log_remarks,
                        PreLeaveBalance = x.int_pre_leave_balance,
                        PrePTOBalance = x.int_pre_pto_balance,
                        Notes = x.vc_notes
                    }).ToList();

                    //Preparing main list for entire leave requests


                    DateTime startOfWeek = DateTime.Today;
                    int delta = DayOfWeek.Monday - startOfWeek.DayOfWeek;
                    startOfWeek = startOfWeek.AddDays(delta);

                    DateTime endOfWeek = startOfWeek.AddDays(7);

                    DateTime now = DateTime.Now;
                    LeaveRequests.ReportingManager = _adminDB.getProjectManagerofUser(UserId).Select(x => x.vc_user_name).FirstOrDefault();
                    LeaveRequests.IsManager = IsManager;
                    LeaveRequests.ActivityLeaveRequestsList = ActivityLeaveRequestsList.OrderByDescending(x => x.CreatedDateTime).ToList();
                    LeaveRequests.AssociateUsersLeaveRequestsList = AssociateUsersLeaveRequestsList.OrderByDescending(x => x.CreatedDateTime).ToList();

                    LeaveRequests.TodayAssociateUsersLeaveRequestsList = LeaveRequests.AssociateUsersLeaveRequestsList.Where(x => checkFromandToDateToday(Convert.ToDateTime(x.FromDate), Convert.ToDateTime(x.ToDate)) == true && x.IsDelete == false).ToList();
                    LeaveRequests.TomorrowAssociateUsersLeaveRequestsList = LeaveRequests.AssociateUsersLeaveRequestsList.Where(x => checkFromandToDateTomorrow(Convert.ToDateTime(x.FromDate), Convert.ToDateTime(x.ToDate)) == true && x.IsDelete == false).ToList();
                    LeaveRequests.ThisWeeksAssociateUsersLeaveRequestsList = LeaveRequests.AssociateUsersLeaveRequestsList.Where(x => checkFromandToDateThisWeek(Convert.ToDateTime(x.FromDate), Convert.ToDateTime(x.ToDate)) == true && x.IsDelete == false).ToList();
                    LeaveRequests.ThisMonthsAssociateUsersLeaveRequestsList = LeaveRequests.AssociateUsersLeaveRequestsList.Where(x => checkFromandToDateThisMonth(Convert.ToDateTime(x.FromDate), Convert.ToDateTime(x.ToDate)) == true && x.IsDelete == false).ToList();
                    LeaveRequests.AllLeavesForCalendar = AllLeavesForCalendar;
                    return LeaveRequests;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public bool checkFromandToDateToday(DateTime fromDate, DateTime toDate)
        //{
        //    if (fromDate.Year != 1 && toDate.Year != 1)
        //    {
        //        if (fromDate.Date >= DateTime.Now.Date && toDate.Date <= toDate.Date)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else if (fromDate.Year != 1)
        //    {
        //        if (fromDate.Date == DateTime.Now.Date)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public bool checkFromandToDateTomorrow(DateTime fromDate, DateTime toDate)
        //{
        //    if (fromDate.Year != 1 && toDate.Year != 1)
        //    {
        //        if (fromDate.Date >= DateTime.Now.AddDays(1).Date && toDate.Date <= toDate.Date)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else if (fromDate.Year != 1)
        //    {
        //        if (fromDate.Date == DateTime.Now.AddDays(1).Date)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}


        //author:Mani
        //date: 30-Aug-2019
        //delete leave request


        public bool checkFromandToDateToday(DateTime fromDate, DateTime toDate)
        {
            if (fromDate.Year != 1 && toDate.Year != 1)
            {
                if (fromDate.Date >= DateTime.Now.Date && toDate.Date <= DateTime.Now.Date)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (fromDate.Year != 1)
            {
                if (fromDate.Date == DateTime.Now.Date)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool checkFromandToDateTomorrow(DateTime fromDate, DateTime toDate)
        {
            if (fromDate.Year != 1 && toDate.Year != 1)
            {
                if (fromDate.Date >= DateTime.Now.AddDays(1).Date && toDate.Date <= DateTime.Now.AddDays(1).Date)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (fromDate.Year != 1)
            {

                if (fromDate.Date == DateTime.Now.AddDays(1).Date)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool checkFromandToDateThisWeek(DateTime fromDate, DateTime toDate)
        {
            DateTime startOfWeek = DateTime.Today;
            int delta = DayOfWeek.Monday - startOfWeek.DayOfWeek;
            startOfWeek = startOfWeek.AddDays(delta);

            DateTime endOfWeek = startOfWeek.AddDays(7);
            if (fromDate.Year != 1 && toDate.Year != 1)
            {
                if (startOfWeek <= fromDate && toDate <= endOfWeek)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (fromDate.Year != 1)
            {
                if (startOfWeek <= fromDate && fromDate <= endOfWeek)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool checkFromandToDateThisMonth(DateTime fromDate, DateTime toDate)
        {
            if (fromDate.Year != 1 && toDate.Year != 1)
            {
                if (fromDate.Month == DateTime.Now.Month || toDate.Month == DateTime.Now.Month)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (fromDate.Year != 1)
            {

                if (fromDate.Month == DateTime.Now.Month)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public long? DeleteLeaveRequest(LeaveRequests model)
        {
            try
            {
                long? result = 0;
                _adminDB.DeleteLeaveRequest(model.LeaveRequestId,
                    model.TypeOfAbsenceId, model.LeaveReasonId, model.FromDate,
                    model.ToDate, model.ActualPaidHours, model.AuthorisedById, model.Remarks, model.UserId, ref result);
                return result;
            }
            catch { throw; }
        }

        ///author:Mani
        //date: 24-Oct-2019
        //Get Leaves Activity Log
        public GetLeaveRequests GetLeaveActivityLog(long UserId)
        {
            try
            {
                bool isReportingManager = false;
                List<long?> AssociateduserIds = GetUsers().Where(x => x.reportingManagerId == UserId).Select(u => u.userId).ToList();
                isReportingManager = AssociateduserIds.Count > 0 ? true : false;

                var LeaveActivityLog = _adminDB.GetLeavesActivityLog(UserId).OrderByDescending(x => x.dt_created_on).ToList();

                List<LeaveRequests> LeaveActivityLogList = LeaveActivityLog.Select(x => new LeaveRequests
                {
                    CreatedDateString = getDateTimeString(x.dt_created_on),
                    ActivityType = x.vc_activity_type,
                    TypeOfAbsence = x.vc_absence_type,
                    LeaveReason = x.vc_leave_reason_type,
                    FromDateString = x.dt_from_date,
                    ToDateString = x.dt_to_date,
                    ActualPaidHours = x.int_actual_paid_leaves,
                    userName = x.vc_user_name,
                    FromUserName = x.vc_from_user_name,
                    ToUserName = x.vc_to_user_name,
                    Remarks = x.vc_remarks,
                    LeaveUserName = x.vc_leave_user_name,
                    //Using IsSelected for is_read property
                    IsSelected = (x.bt_is_read == null || x.bt_is_read == false) ? false : true,
                    CreatedDateTime = Convert.ToDateTime(x.dt_created_on)
                }).ToList();

                var actionNotifications = _schDB.sp_get_scheduled_action_notifications(UserId).ToList();
                if (actionNotifications != null && actionNotifications.Count > 0)
                {
                    foreach (var noti in actionNotifications)
                    {
                        LeaveRequests notification = new LeaveRequests();
                        //for created date
                        notification.CreatedDateString = Convert.ToDateTime(noti.dt_created_date).ToString("yyyy-MM-dd, hh:mm tt");
                        //for ref number
                        notification.TypeOfAbsence = noti.vc_quote_number;
                        //for status
                        notification.ActivityType = noti.vc_status;
                        //for assigned hours
                        notification.LeaveReason = noti.int_assigned_hours.ToString();
                        //for scheduled date
                        notification.FromDateString = Convert.ToDateTime(noti.dt_scheduled_date).ToString("yyyy-MM-dd");
                        //for from user name
                        notification.FromUserName = noti.vc_from_user_name;
                        //for read status
                        notification.IsSelected = Convert.ToBoolean(noti.bt_read);
                        notification.CreatedDateTime = noti.dt_created_date;

                        LeaveActivityLogList.Add(notification);
                    }
                }

                LeaveActivityLogList = LeaveActivityLogList.OrderBy(x => x.CreatedDateTime).ToList();

                GetLeaveRequests LeavesActivityLogData = new GetLeaveRequests();
                LeavesActivityLogData.ReportingManager = _adminDB.getProjectManagerofUser(UserId).Select(x => x.vc_user_name).FirstOrDefault();
                LeavesActivityLogData.ActivityLeaveRequestsList = LeaveActivityLogList;
                LeavesActivityLogData.isReportingManager = isReportingManager;
                return LeavesActivityLogData;
            }
            catch { throw; }
        }
        public string getDateTimeString(string dateTimeString)
        {
            try
            {
                if (dateTimeString != "")
                {
                    string date = dateTimeString.Split(' ')[0];
                    string time = dateTimeString.Split(' ')[1];
                    string hours = "";
                    string MeridiemType = "";
                    hours = time.Split(':')[0];
                    if (Convert.ToInt16(hours) > 12)
                    {
                        hours = (Convert.ToInt16(hours) - 12).ToString();
                        MeridiemType = "PM";
                    }
                    else
                    {
                        MeridiemType = "AM";
                    }
                    time = hours + ':' + time.Split(':')[1] + " " + MeridiemType;
                    dateTimeString = date + ',' + " " + time;
                }


                return dateTimeString;
            }
            catch (Exception)
            {

                throw;
            }
        }

        //author:Mani
        //date: 25-Oct-2019
        //Update Activity Log Read status
        public long? UpdateActivityLogReadstatus(long? UserId)
        {
            try
            {
                long? result = 0;
                _adminDB.UpdateUserLeaveActivityLogReadStatus(UserId, ref result);
                return result;
            }
            catch { throw; }
        }




        #endregion



        #endregion

        #region Clients
        public bool VerifyClientName(long? clientId, string clientName)
        {
            bool isExists = false;
            List<VerifyClientNameResult> clients = new List<VerifyClientNameResult>();
            clientId = clientId == null ? 0 : clientId;
            clients = _adminDB.VerifyClientName(clientId, clientName).ToList();
            if (clients != null && clients.Count > 0) {
                isExists = true;
            }
            return isExists;
        }
        //Author : Shyam 
        //Date : 15-08-2019
        //Upsert Client
        public long? upsertClient(Clients model)
        {
            try
            {
                long? result = null;
                _adminDB.sp_upsertIBWClients(model.clientID, model.clientName, model.clientEmail, model.clientPhone, model.clientTypeID,
                                             model.clientStreetAddress, model.clientCity, model.clientZip, model.clientCountryID, model.clientStateID, model.clientMuncipalityID, model.createdBy, model.modifiedBy, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //Author : Shyam 
        //Date : 17-08-2019
        //Get all the IBW Clients
        public List<Clients> GetClients()
        {
            try
            {
                var clientsList = _adminDB.sp_getAllIBWClients().ToList();
                var contactsList = _adminDB.sp_getAllIBWContacts().ToList();
                List<Clients> clientData = clientsList.AsEnumerable().Select(x => new Clients
                {
                    clientID = x.int_client_id,
                    clientName = x.vc_client_name,
                    clientCode = x.vc_clientCode,
                    clientEmail = x.vc_client_email,
                    clientPhone = x.vc_client_phone_number,
                    clientType = x.vc_Client_Type,
                    clientTypeID = x.int_client_type_id,
                    clientCountry = x.vc_Country_Name,
                    clientCountryID = x.int_county_id,
                    clientState = x.vc_State_Name,
                    clientStateID = x.int_state_id,
                    clientMuncipality = x.vc_Muncipality_Name,
                    clientMuncipalityID = x.int_municipality_id,
                    clientCity = x.vc_city,
                    clientZip = x.vc_zip_or_postal,
                    clientStreetAddress = x.vc_site_street_address,
                    clientbtFlag = x.bt_flag,
                    clientbtStatus = x.bt_status,
                    createdDate = x.int_created_on,
                    activityDate = x.dt_activity_on,
                    quotesCount = x.QuotesCount,
                    projectsCount = x.ProjectsCount,
                    clientContacts = contactsList.Where(p => p.int_client_id == x.int_client_id).ToList().Count

                }).ToList();

                return clientData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Clients> GetClientById(long? clientId)
        {
            try
            {
                var clientsList = _adminDB.sp_getAllIBWClients().Where(x=>x.int_client_id == clientId).ToList();
                var contactsList = _adminDB.sp_getAllIBWContacts().Where(x => x.int_client_id == clientId).ToList();
                List<Clients> clientData = clientsList.AsEnumerable().Select(x => new Clients
                {
                    clientID = x.int_client_id,
                    clientName = x.vc_client_name,
                    clientCode = x.vc_clientCode,
                    clientEmail = x.vc_client_email,
                    clientPhone = x.vc_client_phone_number,
                    clientType = x.vc_Client_Type,
                    clientTypeID = x.int_client_type_id,
                    clientCountry = x.vc_Country_Name,
                    clientCountryID = x.int_county_id,
                    clientState = x.vc_State_Name,
                    clientStateID = x.int_state_id,
                    clientMuncipality = x.vc_Muncipality_Name,
                    clientMuncipalityID = x.int_municipality_id,
                    clientCity = x.vc_city,
                    clientZip = x.vc_zip_or_postal,
                    clientStreetAddress = x.vc_site_street_address,
                    clientbtFlag = x.bt_flag,
                    clientbtStatus = x.bt_status,
                    createdDate = x.int_created_on,
                    activityDate = x.dt_activity_on,
                    clientContacts = contactsList.Where(p => p.int_client_id == x.int_client_id).ToList().Count

                }).ToList();

                return clientData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //Author : Shyam 
        //Date : 17-08-2019
        //Get all the IBW Clients
        public long? ChangeClientState(Clients model)
        {
            try
            {
                long? result = null;
                var data = _adminDB.sp_ChangeStatusClient(model.clientID, model.clientbtDelete, model.clientbtStatus, model.clientbtFlag, model.modifiedBy, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Shyam 
        //Date : 18-08-2019
        //Get Client Types
        public List<ClientTypes> GetClientTypes()
        {
            try
            {
                var clientTypes = _adminDB.sp_getIBWClientTypes().ToList();
                List<ClientTypes> clientTypesList = clientTypes.AsEnumerable().Select(x => new ClientTypes
                {
                    clientTypeId = x.int_client_type_id,
                    clientTypeName = x.vc_client_type
                }).ToList();

                return clientTypesList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Shyam 
        //Date : 18-08-2019
        //Get Countries
        public List<Countries> GetCountries()
        {
            try
            {
                var countries = _adminDB.sp_getIBWCountries().ToList();
                List<Countries> countriesList = countries.AsEnumerable().Select(x => new Countries
                {
                    countryId = x.int_county_id,
                    countryName = x.vc_country_name
                }).ToList();

                return countriesList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Shyam 
        //Date : 18-08-2019
        //Get States
        public List<States> GetStates(long? countryID)
        {
            try
            {
                var states = _adminDB.sp_getIBWStates(countryID).ToList();
                List<States> statesList = states.AsEnumerable().Select(x => new States
                {
                    stateId = x.int_state_id,
                    stateName = x.vc_state_name,
                    countryID = x.int_county_id
                }).ToList();

                return statesList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Shyam 
        //Date : 18-08-2019
        //Get Muncipalities
        public List<Muncipalities> GetMuncipalities()
        {
            try
            {
                var muncipalities = _adminDB.sp_getIBWMuncipalities().ToList();
                List<Muncipalities> muncipalitiesList = muncipalities.AsEnumerable().Select(x => new Muncipalities
                {
                    muncipalityId = x.int_municipality_id,
                    muncipalityName = x.vc_municipality_name,
                    stateID = x.int_state_id
                }).ToList();

                return muncipalitiesList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Shyam 
        //Date : 18-08-2019
        //Save Grid Columns
        public long? SaveGridColumns(List<string> array)
        {
            try
            {
                long? result = null;
                if (array != null && array.Count > 0)
                {
                    long? userID = (long)Convert.ToInt32(array[array.Count - 1]);
                    //array = array.Where(val => val != array[array.Length - 1]).ToArray();
                    array.RemoveAt(array.Count - 1);
                    string screenName = array[array.Count - 1];
                    //array = array.Where(val => val != array[array.Length - 1]).ToArray();
                    array.RemoveAt(array.Count - 1);
                    string arrayString = String.Join(",", array.ToArray());
                    //arrayString += ",";
                    _adminDB.sp_Save_Grid_Columns(arrayString, userID, screenName, ref result);
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //Author : Shyam 
        //Date : 18-08-2019
        //Get Muncipalities
        public List<string> GetGridColumns(long? id, string screenId)
        {
            try
            {
                List<string> stringList = new List<string>();
                var gridColumns = _adminDB.GetGridColumns(id, screenId).FirstOrDefault();
                if (gridColumns != null)
                {
                    stringList = gridColumns.vc_preferred_grid_columns.Split(',').ToList();
                }
                return stringList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region contacts
        //Author : Shyam 
        //Date : 15-08-2019
        //Upsert Client
        public long? upsertContact(Contacts model)
        {
            try
            {
                long? result = null;
                _adminDB.sp_upsertIBWContacts(model.contactID, model.clientID, model.contactName, model.contactJobTitle, model.contactEmail, model.contactPhone, model.contactStreetAddress,
                                              model.contactCity, model.contactZip, model.contactCountryID, model.contactStateID, model.contactMuncipalityID, model.contactIsPrimaryContact, model.contactIsBillingContact, model.isAddressCopiedFromClient, model.createdBy, model.modifiedBy, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //Author : Shyam 
        //Date : 18-09-2019
        //Contact Email id verification
        public bool ContactEmailValidation(string vcEmail, long? clientId, long? contactID)
        {
            try
            {
                List<Contacts> contactsList = new List<Contacts>();
                if (clientId != null)
                {
                    if (contactID == null || contactID == 0)
                    {
                        contactsList = GetContacts().Where(x => x.contactEmail == vcEmail && x.clientID == clientId).ToList();
                    }
                    else
                    {
                        contactsList = GetContacts().Where(x => x.contactEmail == vcEmail && x.clientID == clientId && x.contactID != contactID).ToList();
                    }
                }
                if (contactsList.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch { throw; }
        }
        //Author : Shyam 
        //Date : 17-08-2019
        //Get all the IBW Clients
        public List<Contacts> GetContacts()
        {
            try
            {
                var contactsList = _adminDB.sp_getAllIBWContacts().ToList();
                List<Contacts> contactsData = contactsList.AsEnumerable().Select(x => new Contacts
                {
                    contactID = x.int_contact_id,
                    clientID = x.int_client_id,
                    clientName = x.vc_client_name,
                    contactName = x.vc_contact_name,
                    contactJobTitle = x.vc_job_title,
                    contactEmail = x.vc_contact_email,
                    contactPhone = x.vc_contact_phone_number,
                    contactCountry = x.vc_Country_Name,
                    contactCountryID = x.int_county_id,
                    contactState = x.vc_State_Name,
                    contactStateID = x.int_state_id,
                    contactMuncipalityID = x.int_municipality_id,
                    contactMuncipality = x.vc_Muncipality_Name,
                    contactCity = x.vc_city,
                    contactZip = x.vc_zip_or_postal,
                    contactStreetAddress = x.vc_site_street_address,
                    contactIsPrimaryContact = x.bt_primary_contact,
                    contactIsBillingContact = x.bt_billing_contact,
                    contactbtStatus = x.bt_status,
                    createdDate = x.int_created_on,
                    activityDate = x.dt_activity_on,
                    isAddressCopiedFromClient = x.bt_client_address_copied,
                    quotesCount = x.QuotesCount
                }).ToList();

                return contactsData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Author : Shyam 
        //Date : 27-08-2019
        //Get all the IBW Contacts
        public long? ChangeContactState(Contacts model)
        {
            try
            {
                long? result = null;
                var data = _adminDB.sp_ChangeStatusContact(model.contactID, model.contactbtDelete, model.contactbtStatus, model.modifiedBy, ref result);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Asset
        public List<GetAssetsResult> GetAssets()
        {
            return _adminDB.GetAssets().ToList();
        }
        public List<GetAssetCategoriesResult> GetAssetCategoriesForMaster()
        {
            return _adminDB.GetAssetCategories().Where(x => x.bt_delete != true && x.bt_status != false).ToList();
        }
        public string GetAssetCategoriesSerial(long? categoryId)
        {
            string serialNum = "";
            List<GetAssetCategoriesSerialResult> categories = new List<GetAssetCategoriesSerialResult>();
            categories = _adminDB.GetAssetCategoriesSerial(categoryId).ToList();
            if (categories.Count > 0)
            {
                string latestSerialNum = categories.OrderByDescending(x => x.int_asset_master_id).Select(x => x.vc_asset_number).FirstOrDefault();
                string[] arrayLatestSerialNum = latestSerialNum.Split('-');
                int incrementedNum = Convert.ToInt32(arrayLatestSerialNum[1]) + 1;
                serialNum = arrayLatestSerialNum[0] + '-' + incrementedNum.ToString("D" + 4);
            }
            else
            {
                string prefix = _adminDB.GetAssetCategories().Where(x => x.int_asset_category_id == categoryId).Select(x => x.vc_asset_category_prefix).FirstOrDefault();
                serialNum = prefix + "-0001";
            }
            return serialNum;
        }
        public Response<bool> UpsertAsset(GetAssetsResult asset, long? createdBy)
        {
            Response<bool> response = new Response<bool>();
            List<GetAssetsResult> assets = new List<GetAssetsResult>();
            List<GetAssetsResult> assetNumbers = new List<GetAssetsResult>();
            List<GetAssetsResult> assetSerialNumber = new List<GetAssetsResult>();
            assets = _adminDB.GetAssets().ToList();
            assetNumbers = assets.Where(x => x.SerialNumber == asset.SerialNumber && x.assetId != asset.assetId).ToList();
            assetSerialNumber = assets.Where(x => x.AssetNumberNew == asset.AssetNumberNew && x.assetId != asset.assetId).ToList();
            if (assetSerialNumber.Count == 0 || asset.AssetNumberNew == null || asset.AssetNumberNew == "")
            {
                if (assetNumbers.Count == 0)
                {
                    long? result = 0;
                    _adminDB.UpsertAsset(asset.assetId, asset.CategoryId, asset.Name, asset.Number, asset.Make, asset.Model, asset.SerialNumber, asset.Dealer, asset.assetDescription, asset.strPurchaseDate, asset.PurchasePrice, asset.AssetNumberNew, createdBy, ref result);
                    if (result > 0)
                    {
                        response.Status = true;
                        response.Message = asset.assetId != 0 ? "Asset saved successfully" : "Asset updated successfully";
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Operation failed";
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = "Serial Number already exists";
                }
            }
            else
            {
                response.Status = false;
                response.Message = "Asset Number already exists";
            }
            return response;
        }
        public bool BulkInsertAssets(List<GetAssetsResult> assets, long? createdBy)
        {
            long? result = 0;
            foreach (var asset in assets)
            {
                string assetNumber = GetAssetCategoriesSerial(asset.CategoryId);
                _adminDB.UpsertAsset(asset.assetId, asset.CategoryId, asset.Name, assetNumber, asset.Make, asset.Model, asset.SerialNumber, asset.Dealer, asset.assetDescription, asset.strPurchaseDate, asset.PurchasePrice, asset.AssetNumberNew, createdBy, ref result);
            }
            if (result > 0)
                return true;
            else
                return false;
        }
        public bool ChangeAssetStatus(GetAssetsResult asset, long? createdBy)
        {
            long? result = 0;
            _adminDB.AssetDeleteAndStatusChange(asset.assetId, asset.bt_status, null, createdBy, ref result);
            if (result > 0)
                return true;
            else
                return false;
        }
        public bool DeleteAsset(long? assetId, long? createdBy)
        {
            long? result = 0;
            _adminDB.AssetDeleteAndStatusChange(assetId, null, true, createdBy, ref result);
            if (result > 0)
                return true;
            else
                return false;
        }
        #endregion
        #region Holidays
        public List<GetHolidaysResult> GetHolidays()
        {
            List<GetHolidaysResult> holidays = new List<GetHolidaysResult>();
            holidays = _adminDB.GetHolidays().ToList();
            return holidays;
        }
        public bool SaveHolidays(List<GetHolidaysResult> model, long? createdBy)
        {
            long? result = 0;
            long? insertResult = 0;
            _adminDB.DeleteHoliday(null, createdBy, ref result);
            foreach (var item in model)
            {
                insertResult = 0;
                _adminDB.InsertHoliday(item.holidayId, item.holidayFor, item.strHolidayDate, createdBy, ref insertResult);
            }
            if (insertResult > 0)
                return true;
            else
                return false;
        }
        public bool DeleteHoliday(long? holidayId, long? updatedBy)
        {
            long? result = 0;
            _adminDB.DeleteHoliday(holidayId, updatedBy, ref result);
            if (result > 0)
                return true;
            else
                return false;
        }
        #endregion
        #region EventAndErrorLog
        public List<GetEventLogResult> GetEventLog()
        {
            return _adminDB.GetEventLog().ToList();
        }
        public List<GetErrorLogResult> GetErrorLog()
        {
            return _adminDB.GetErrorLog().ToList();
        }
        #endregion
        ~Admin()
        {
            _adminDB.Dispose();
        }


    }


}

