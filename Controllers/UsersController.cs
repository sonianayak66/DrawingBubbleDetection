using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MPCRS.DbJsonModels;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using System.Security.Claims;
using XAct;
using XAct.Users;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;

        public UsersController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }
         
        [ClaimRequirement(UserPermissions.User_Read)]
        public IActionResult Index()
        {
            return View();
        }

        [ClaimRequirement(UserPermissions.User_Write)]
        public IActionResult ManagePerson(string ID = "")
        {
            managePerson managePerson = new managePerson();
            try
            {
				PersonViewModel Person = new();

				if (!string.IsNullOrEmpty(ID))
				{
					using (_dbContext)
					{
						var dbPerson = _dbContext.persons_data.AsNoTracking().Where(x => x.PersonGUID == ID && string.IsNullOrEmpty(x.DataJson) == false).FirstOrDefault();
						if (dbPerson != null)
						{
							Person = JsonConvert.DeserializeObject<PersonViewModel>(dbPerson.DataJson);
						}
						if (Person != null && Person.allowLogin == true)
						{
							AspNetUser UserInfo = _dbContext.AspNetUsers.AsNoTracking().Where(x => x.PersonGuid == Person.PersonGUID).FirstOrDefault();
							if (UserInfo != null)
							{
								var assignedRoles = _dbContext.AspNetUserRoles.AsNoTracking().Where(x => x.UserId == UserInfo.Id)?.Select(x => x.RoleId).ToArray();
								managePerson.roles = assignedRoles;
							}
						}
					}
				}
				managePerson.PersonInfo = Person;
			}
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
            return View(managePerson);
        }

        [HttpPost]
        [ClaimRequirement(UserPermissions.User_Write)]
        public async Task<IActionResult> ManagePerson(managePerson managepersonModel)
        {
            //managepersonModel.roles = managepersonModel.roles == null ? new List<string>().ToArray() : managepersonModel.roles;
            //if (!ModelState.IsValid)
            //{
            //    return View(managepersonModel);
            //}
            PersonViewModel personModel = managepersonModel.PersonInfo;
            personModel.updated_on = DateTime.Now;
            personModel.updated_by = User.Identity.Name;
            var DbOperation = personModel.PersonGUID == VTSDataRecordType.New.ToString() ? dbOperation.Create : dbOperation.Update;
            var dbPerson = new persons_datum();
            try
            {
				using (_dbContext)
				{
					if (DbOperation != dbOperation.Create)
					{
						dbPerson = _dbContext.persons_data.Where(x => x.PersonGUID == personModel.PersonGUID).FirstOrDefault();
						if (dbPerson != null)
						{
							dbPerson.DataJson = JsonConvert.SerializeObject(personModel);
							_dbContext.Entry(dbPerson).State = EntityState.Modified;
						}
					}
					else if (DbOperation == dbOperation.Create || dbPerson == null)
					{
						dbPerson = new persons_datum();
						personModel.PersonGUID = Guid.NewGuid().ToString();
						dbPerson.PersonGUID = personModel.PersonGUID;
						dbPerson.DataJson = JsonConvert.SerializeObject(personModel);
						_dbContext.Add(dbPerson);
					}

					var password = MPGlobals.Encrypt_SHA256("DefaultPassword" + personModel.email_address.ToLower());
					AspNetUser UserInfo = _dbContext.AspNetUsers.Where(x => x.PersonGuid == personModel.PersonGUID).FirstOrDefault();
					if (UserInfo != null)
					{
                        UserInfo.IsActiveUser = personModel.allowLogin;
                        
                        if (managepersonModel.ResetPassword == true)
						{
                            UserInfo.PasswordHash = password;
                            UserInfo.TwoFactorEnabled = true;// Indicates that user has changed password for V2

                        }
						_dbContext.Entry(UserInfo).State = EntityState.Modified;
					}
					else if (personModel.allowLogin)
					{
						UserInfo = new();
						UserInfo.Id = Guid.NewGuid().ToString();
                        UserInfo.IsActiveUser = true;
                        
                        UserInfo.PasswordHash = password;
						UserInfo.Email = personModel.email_address.ToLower();
						UserInfo.NormalizedEmail = personModel.email_address.ToLower();
						UserInfo.EmailConfirmed = true;
						UserInfo.UserName = personModel.name;
						UserInfo.PersonGuid = personModel.PersonGUID;
						UserInfo.PhoneNumber = personModel.phone_number;
						UserInfo.PhoneNumberConfirmed = true;
						UserInfo.TwoFactorEnabled = true;
						UserInfo.LockoutEnabled = false;
						_dbContext.AspNetUsers.Add(UserInfo);
					}

					if (UserInfo != null)
					{
						List<AspNetUserRole> roleMapping = _dbContext.AspNetUserRoles.Where(x => x.UserId == UserInfo.Id).ToList();
						if (roleMapping != null && managepersonModel.roles != null)
						{
							// remove roles unavailable in model from db
							var rolesToRemove = roleMapping.Where(e => !managepersonModel.roles.Contains(e.RoleId)).ToList();
							_dbContext.AspNetUserRoles.RemoveRange(rolesToRemove);
						}
						else if (roleMapping != null && (managepersonModel.roles == null || personModel.allowLogin == false))
						{
							_dbContext.AspNetUserRoles.RemoveRange(roleMapping);
						}
						// add new roles
						if (managepersonModel.roles != null && personModel.allowLogin)
						{
							foreach (var item in managepersonModel.roles)
							{
								if (roleMapping.Where(x => x.RoleId == item).FirstOrDefault() == null)
								{
									_dbContext.AspNetUserRoles.Add(new AspNetUserRole() { UserId = UserInfo.Id, RoleId = item });
								}
							}
						}
					}
					await _dbContext.SaveChangesAsync();


					if (UserInfo.OldUserDbkey == null)
					{
						Models.User user = _dbContext.Users.Where(x => x.Email == UserInfo.Email).FirstOrDefault();
						if (user == null)
						{
                            user = new Models.User();
                            user.UserName = UserInfo.UserName;
                            user.Email = UserInfo.Email;
                            user.Department = 13;
                            user.User_Status = true;
                            user.Updated_On = DateTime.Now;
                            _dbContext.Add(user);
                            _dbContext.SaveChanges();
                        }
                     

						MPGlobals.ExceSQLNonQuery($"Update [dbo].[AspNetUsers] set OldUserDbkey = {user.UserDbkey} where Id = '{UserInfo.Id}'");
                    }
				}
				DataCaching.removeCache(CacheKeys.Persons.ToString());
            }
            catch (Exception ex) { return Json(new { success = false, msg = ex.Message }); }
            return Json(new { success = true, msg = "Saved Successfully" });
            //return RedirectToAction("Index");
        }

       

        [ClaimRequirement(UserPermissions.User_RolesWrite)]
        [HttpGet]
        public IActionResult Permissions(string roleID = "")
        {
            List<Permissions> permissions = new();
            try
            {
				foreach (UserPermissions value in Enum.GetValues(typeof(UserPermissions)))
				{
					var desc = value.GetAttributeOfType<PermissionDescription>();
					string Permission = Enum.GetName(typeof(UserPermissions), value);
					permissions.Add(new Permissions() { PermissionInfo = desc, PermissionString = Permission });
				}

				var assignedPermissions = DataCaching.getCachedRoleClaims().Where(x => x.RoleId == roleID);
				if (assignedPermissions != null)
				{
					foreach (var permission in assignedPermissions)
					{
						permissions.Where(x => x.PermissionString == permission.ClaimValue).ToList().ForEach(x => x.selected = true);
					}
				}
				using (_dbContext)
				{
					ViewBag.RoleName = _dbContext.AspNetRoles.Where(x => x.Id == roleID).FirstOrDefault()?.Name;
					ViewBag.RoleId = roleID;
				}
			}
            catch (Exception ex) { ErrorHandler.LogException(ex); }
            return View(permissions);
        }

        [ClaimRequirement(UserPermissions.User_RolesWrite)]
        [HttpPost]
        public async Task<IActionResult> PermissionsSave([FromBody] IEnumerable<AspNetRoleClaimsVM> aspNetRoleClaimsVM)
        {
            // TO-Do
            // Need to handle case where all claims are marked unchecked. in such case data will be empty.
            // If empty, no action can be taken on sql
            try
            {
				if (aspNetRoleClaimsVM.IsNullOrEmpty() == false)
				{
					string roleID = aspNetRoleClaimsVM.FirstOrDefault().RoleId;
					var claimDataJson = JsonConvert.SerializeObject(aspNetRoleClaimsVM);
					using (var connection = mPDapperContext.CreateConnection())
					{
						var parms = new DynamicParameters();
						parms.Add("@jsonData", claimDataJson);
						parms.Add("@roleID", roleID);
						var db = await connection.QueryAsync($"[dbo].[UserManagement_ManageRoleClaims] @roleID=@roleID, @jsonData=@jsonData", parms);
					}
				}
				Masters.RemoveCache(CacheKeys.RoleClaims.ToString());
			}
            catch (Exception ex) 
			{ 
				return Json(new { success = true, msg=ex.Message });
			}
            return Json(new { success = true, Msg = "Saved Succesfully" });
        }


        [HttpGet]
        [ClaimRequirement(UserPermissions.User_Read)]
        public ActionResult UserList()
        {
			Masters.RemoveCache(CacheKeys.Persons.ToString());
			List<PersonVM> UserList = DataCaching.getCachedPersonList();
            //var valuesList = JArray.FromObject(UserList).Select(x => x.Values().ToList()).ToList(); 
            //string finalRes = JsonConvert.SerializeObject(valuesList, Formatting.Indented); 
            return View(UserList);
        }
        [HttpGet]
        [ClaimRequirement(UserPermissions.User_Read)]
        public ActionResult InactiveUserList()
        {
            Masters.RemoveCache(CacheKeys.Persons.ToString());
            List<PersonVM> InactiveUserList = DataCaching.getCachedPersonListInactive();
            //var valuesList = JArray.FromObject(UserList).Select(x => x.Values().ToList()).ToList(); 
            //string finalRes = JsonConvert.SerializeObject(valuesList, Formatting.Indented); 
            return View(InactiveUserList);
        }

        [HttpGet]
        public ActionResult RoleList()
        {
            List<AspNetRole> roles = DataCaching.getCachedRole();
            return View(roles);
        }


        [ClaimRequirement(UserPermissions.User_RolesWrite)]
        public async Task<IActionResult> ManageRole(string ID = "")
        {
            AspNetRolesVM data = new AspNetRolesVM();
			try
			{
				if (!string.IsNullOrEmpty(ID))
				{
					using (_dbContext)
					{
						AspNetRole dbRole = await _dbContext.AspNetRoles.AsNoTracking().Where(x => x.Id == ID).FirstOrDefaultAsync();
						if (dbRole != null)
						{
							data = JsonConvert.DeserializeObject<AspNetRolesVM>(JsonConvert.SerializeObject(dbRole));
						}
					}
				}
			}
			catch (Exception ex) { ErrorHandler.LogException(ex); }
            return PartialView(data);
        }


        [HttpPost]
        [ClaimRequirement(UserPermissions.User_RolesWrite)]
        public async Task<IActionResult> ManageRole(AspNetRolesVM manageRoles)
        {
            var jsonModel = JsonConvert.SerializeObject(manageRoles);
			// Make Dapper Call with parameter
			try
			{
				using (var connection = mPDapperContext.CreateConnection())
				{
					var parms = new DynamicParameters();
					parms.Add("@RoleDataJson", jsonModel);
					var db = await connection.QueryAsync($"[dbo].[UserManagement_ManageRoles] @RoleDataJson=@RoleDataJson", parms);
				}
				Masters.RemoveCache(CacheKeys.AspNetRoles.ToString());
				Masters.RemoveCache(CacheKeys.RoleClaims.ToString());
			}
			catch (Exception ex) 
			{
				return Json(new { success = true, Msg = ex.Message });
			}
            
            return Json(new { success = true, Msg = "Saved Succesfully" });
        }


		[HttpGet]
		[ClaimRequirement(UserPermissions.User_Write)]
		public ActionResult BulkRoleAssignment()
		{
			var model = new BulkRoleAssignmentVM();
			try
			{
				Masters.RemoveCache(CacheKeys.Persons.ToString());
				model.Users = DataCaching.getCachedPersonList() ?? new List<PersonVM>();
				model.Roles = DataCaching.getCachedRole()?.Select(r => new RoleItem { Id = r.Id, Name = r.Name }).ToList() ?? new List<RoleItem>();

				using (_dbContext)
				{
                    model.Departments = _dbContext.MetaMasters
						.Where(x => x.MasterType == "Departments" && x.IsActive == true)
						.OrderBy(x => x.DisplayOrder)
						.Select(x => new MetaMasterItem
						{
						    MasterGUID = x.MasterGUID == null ? "" : x.MasterGUID.ToString(),
						    DisplayText = x.DisplayText == null ? "" : x.DisplayText.ToString(),
						    UseValue = x.UseValue == null ? "" : x.UseValue.ToString()
						})
						.ToList();

                    model.PersonTypes = _dbContext.MetaMasters
                        .Where(x => x.MasterType == "PersonType" && x.IsActive == true)
                        .OrderBy(x => x.DisplayOrder)
                        .Select(x => new MetaMasterItem
                        {
                            MasterGUID = x.MasterGUID == null ? "" : x.MasterGUID.ToString(),
                            DisplayText = x.DisplayText == null ? "" : x.DisplayText.ToString(),
                            UseValue = x.DisplayText == null ? "" : x.DisplayText.ToString()
                        })
                        .ToList();

                    var allUserRoles = _dbContext.AspNetUserRoles
						.Include(x => x.Role)
						.Include(x => x.User)
						.Where(x => x.User.IsActiveUser == true)
						.Select(x => new {
						    x.RoleId,
						    RoleName = x.Role.Name,
						    PersonGuid = x.User.PersonGuid ?? ""
						})
						.ToList();

                    model.UserRoleMappings = allUserRoles
						.GroupBy(x => x.PersonGuid)
						.ToDictionary(
							g => g.Key ?? "",
							g => g.Select(r => new UserRoleInfo { RoleId = r.RoleId, RoleName = r.RoleName }).ToList()
						);
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.LogException(ex);
			}
			return View(model);
		}

		[HttpPost]
		[ClaimRequirement(UserPermissions.User_Write)]
		public async Task<IActionResult> SaveBulkRoleAssignment([FromBody] string jsonData)
		{
			try
			{
				using (var connection = mPDapperContext.CreateConnection())
				{
					var parms = new DynamicParameters();
					parms.Add("@jsonData", jsonData);
					await connection.QueryAsync("[dbo].[UserManagement_BulkAssignRoles] @jsonData=@jsonData", parms);
				}
				DataCaching.removeCache(CacheKeys.Persons.ToString());
				return Json(new { success = true, msg = "Roles updated successfully" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, msg = ex.Message });
			}
		}

		[HttpGet]
		public async Task<IActionResult> ChangePassword()
		{
            ChangePasswordViewModel changePasswordViewModel = new ChangePasswordViewModel();
			string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            using (_dbContext)
			{
                var user = await _dbContext.AspNetUsers.Where(x => x.Id == userId).FirstOrDefaultAsync();
                if (user != null)
				{
                    changePasswordViewModel.Email = user.Email;
                }
            }	
            return View(changePasswordViewModel);	
		}


        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
			if (ModelState.IsValid)
			{
                using (_dbContext)
                {
                    string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    using (_dbContext)
                    {
                        var user = await _dbContext.AspNetUsers.Where(x => x.Id == userId).FirstOrDefaultAsync();
                        if (user != null)
                        {
							user.TwoFactorEnabled = true;// Indicates that user has changed password for V2
                            user.PasswordHash = MPGlobals.Encrypt_SHA256(changePasswordViewModel.Password + user.Email.ToLower());
							_dbContext.Entry(user).State = EntityState.Modified;
                            await _dbContext.SaveChangesAsync();
                        }

						Models.User userv1 = await _dbContext.Users.Where(x => x.UserDbkey == user.OldUserDbkey).FirstOrDefaultAsync();
						if (userv1 != null)
						{
                            userv1.Password = MPGlobals.Encrypt_SHA256(changePasswordViewModel.Password + userv1.UserDbkey.ToString());
                        }
                        changePasswordViewModel.ResponseMessage = "Password Changed Successfully;";
                    }
                }
			}
			return View(changePasswordViewModel);
        }

    }
}
