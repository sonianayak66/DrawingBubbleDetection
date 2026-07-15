using Dapper;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using System.Data;
using System.Security.Claims;
using static MPCRS.Utilities.Constants;
using System.Collections.Generic;
using XAct.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json;
using System.Reflection;
using XAct;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MPCRS.Controllers
{
    [Authorize]
    public class SOPManagementController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;


        public SOPManagementController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ClaimRequirement(UserPermissions.BaseLineEngine_Read)]
        public IActionResult BaseLineEngines()
        {
            List<BaseLineEngineVM> accessinfo = new List<BaseLineEngineVM>();
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var db = connection.QueryMultiple($"dbo.GetBaseLineEngines");
                    accessinfo = db.Read<BaseLineEngineVM>().ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }

            return PartialView(accessinfo);
        }

        [HttpGet]
        public IActionResult CreateBaseLineEngine(int BlEngineDbKey)       // to Edit Base Line Engine
        {
            Base_Line_EngineViewModel VM = new Base_Line_EngineViewModel();

            if (BlEngineDbKey != 0)
            {
                using (_dbContext)
                {
                    var baseLineEnginesVModel = _dbContext.Base_Line_Engines.AsNoTracking().Where(x => x.BL_Engine_Dbkey == BlEngineDbKey).FirstOrDefault();
                    try
                    {
                        if (baseLineEnginesVModel != null)
                        {
                            VM = JsonConvert.DeserializeObject<Base_Line_EngineViewModel>(JsonConvert.SerializeObject(baseLineEnginesVModel));
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.LogException(ex);
                    }
                }
            }
            return View(VM);
        }

        [HttpPost]
        public IActionResult CreateBaseLineEngine(Base_Line_EngineViewModel VM)       // to Edit Base Line Engine
        {
            DTOResponse dTOResponse = new DTOResponse();
            int? dbkey;
            //if (ModelState.IsValid)
            //{
            using (DESI_STFE_PRODContext db = new())
            {
                try
                {
                    if (VM.is_active == true)
                    {
                        MPGlobals.ExceSQLNonQuery($"Disable trigger [A_IUD_AuditLog] on dbo.Base_Line_Engines;\r\n  Update [dbo].[Base_Line_Engines] set is_active = 0 ;\r\n  Enable trigger [A_IUD_AuditLog] on dbo.Base_Line_Engines;");
                    }
                    Base_Line_Engine base_Line_Engine = new Base_Line_Engine();
                    base_Line_Engine.BL_Engine_Dbkey = VM.BL_Engine_Dbkey;
                    base_Line_Engine.Engine_Title = VM.Engine_Title;
                    base_Line_Engine.Engine_Description = VM.Engine_Description;
                    base_Line_Engine.is_active = VM.is_active == true ? 1 : 0;
                    base_Line_Engine.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    base_Line_Engine.Revision_date = DateTime.Now;
                    base_Line_Engine.Revision_title = VM.Revision_title;
                    if (VM.BL_Engine_Dbkey == 0)
                    {
                        base_Line_Engine.Updated_on = DateTime.Now;
                        db.Base_Line_Engines.Add(base_Line_Engine);
                        db.SaveChanges();
                        dTOResponse.ResponseMessage = "Saved Successfully";
                        dTOResponse.Result = true;

                    }
                    else
                    {
                        db.Entry(base_Line_Engine).State = EntityState.Modified;
                        db.SaveChanges();
                        dTOResponse.ResponseMessage = "Updated Successfully";
                        dTOResponse.Result = true;

                    }
                    //dbkey = base_Line_Engine.BL_Engine_Dbkey;
                }
                catch (Exception ex)
                {
                    
                    throw ;
                }
                return Json(new { success = true, Msg = dTOResponse.ResponseMessage });
            }
        }


        [HttpGet]
        [ClaimRequirement(UserPermissions.SOP_Read)]
        public IActionResult EngineBuilds(int id = 0)
        {
            List<EngineBuildsVM> accessinfo = new List<EngineBuildsVM>();
            using (_dbContext)
            {
                try
                {
                    Base_Line_Engine blemodel = _dbContext.Base_Line_Engines.Where(x => x.BL_Engine_Dbkey == id).FirstOrDefault();
                    if (blemodel != null)
                    {
                        ViewBag.EngineTitle = blemodel.Engine_Title;
                        ViewBag.ID = blemodel.BL_Engine_Dbkey;
                    }
                    using (var connection = mPDapperContext.CreateConnection())
                    {
                        var db = connection.QueryMultiple($"dbo.EngineBuilds_SSP @BaseLineDBKeyID={id}");
                        accessinfo = db.Read<EngineBuildsVM>().ToList();
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }

                return PartialView(accessinfo);
            }
        }


        [HttpGet]
        [ClaimRequirement(UserPermissions.SOP_Read)]
        public IActionResult Builds()
        {
            List<EngineBuildsVM> accessinfo = new List<EngineBuildsVM>();
            using (_dbContext)
            {
                try
                {
                    using (var connection = mPDapperContext.CreateConnection())
                    {
                        var db = connection.QueryMultiple($"dbo.EngineBuilds_SSP @BaseLineDBKeyID=0");
                        accessinfo = db.Read<EngineBuildsVM>().ToList();

                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }

                return View(accessinfo);
            }
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.SOP_Write)]
        public IActionResult CreateEngineBuild(int Id, string? Etitle)
        {
            EngineBuildsVM vm = new();
            using (_dbContext)
            {
                try
                {
                    if (Id != 0 && Etitle != null)
                    {
                        Base_Line_Engine blemodel = _dbContext.Base_Line_Engines.Where(x => x.BL_Engine_Dbkey == Id).FirstOrDefault();
                        if (blemodel != null)
                        {
                            vm.ClonedFrom = "Base_Line_Engine";
                            vm.ClonedFromKey = blemodel.BL_Engine_Dbkey;
                            vm.BaseLineEngineDbkey = blemodel.BL_Engine_Dbkey;
                            ViewBag.Etitle = blemodel.Engine_Title;
                            return View(vm);
                        }
                    }
                    else
                    {
                        EngineBuild dbmodel = _dbContext.EngineBuilds.Where(x => x.Id == Id).FirstOrDefault();
                        Base_Line_Engine blemodel = _dbContext.Base_Line_Engines.Where(x => x.BL_Engine_Dbkey == dbmodel.BaseLineEngineDbkey).FirstOrDefault();
                        if (dbmodel != null)
                        {
                            ViewBag.Etitle = blemodel.Engine_Title;
                            vm = JsonConvert.DeserializeObject<EngineBuildsVM>(JsonConvert.SerializeObject(dbmodel));
                        }
                    }
                }
                catch (Exception ex) { ErrorHandler.LogException(ex); }
            }
            return View(vm);
        }

        [HttpPost]
        [ClaimRequirement(UserPermissions.SOP_Write)]
        public IActionResult CreateEngineBuild(EngineBuild vm)
        {
            EngineBuild dbModel = new();
            using (_dbContext)
            {
                try
                {
                    dbModel = JsonConvert.DeserializeObject<EngineBuild>(JsonConvert.SerializeObject(vm));
                    if (vm.Id != 0)
                    {
                        EngineBuild dbmodel = _dbContext.EngineBuilds.AsNoTracking().Where(x => x.Id == vm.Id).FirstOrDefault();
                        if (dbmodel != null)
                        {
                            dbmodel.UpdatedBy = User.Identity.Name;
                            dbmodel.UpdatedOn = DateTime.Now;
                            _dbContext.Entry(dbModel).State = EntityState.Modified;
                            _dbContext.SaveChanges();
                        }
                        else
                        {
                            // this is to ensure it catches the next if condition
                            vm.Id = 0;
                        }
                    }
                    if (vm.Id == 0)
                    {

                        Base_Line_Engine blemodel = _dbContext.Base_Line_Engines.Where(x => x.BL_Engine_Dbkey == dbModel.BaseLineEngineDbkey).FirstOrDefault();
                        dbModel.BuildGuid = Guid.NewGuid().ToString();
                        dbModel.UpdatedBy = User.Identity.Name;
                        dbModel.CreatedBy = User.Identity.Name;
                        dbModel.UpdatedOn = DateTime.Now;
                        dbModel.CreatedOn = DateTime.Now;
                        _dbContext.Add(dbModel);
                        _dbContext.SaveChanges();
                    }
                }
                catch (Exception ex) { ErrorHandler.LogException(ex); }
            }
            return RedirectToAction("Builds");
        }


        public IActionResult BuildComponents(string buidguid)
        {
            ViewBag.customAccess = "false";
            BuildViewModel buildViewModel = GetBuildViewModel(buidguid);
            return View(buildViewModel);
        }


        [HttpGet]
        [ClaimRequirement(UserPermissions.SOP_Write)]
        public IActionResult CreateSOPAccess(string LinkGuid = "")
        {
            using (_dbContext)
            {
                SOPCustomAccessLink sOPCustomAccessLink = new SOPCustomAccessLink();
                sOPCustomAccessLink.access_validity = DateTime.Now.AddDays(1);
                if (string.IsNullOrEmpty(LinkGuid))
                {
                    return PartialView(sOPCustomAccessLink);
                }
                else
                {
                    SOP_CustomAccessLink sOP_CustomAccessLink = _dbContext.SOP_CustomAccessLinks.Where(x=>x.LinkGuid == LinkGuid).FirstOrDefault();
                    var jsonstring = sOP_CustomAccessLink.LinkDataJson;
                    sOPCustomAccessLink = JsonConvert.DeserializeObject<SOPCustomAccessLink>(jsonstring);
                    sOPCustomAccessLink.LinkGuid = LinkGuid;
                    return PartialView(sOPCustomAccessLink);
                }
            }
        }


        [HttpPost]
        [ClaimRequirement(UserPermissions.SOP_Write)]
        public IActionResult CreateSOPAccess(SOPCustomAccessLink sOPCustomAccessLink)
        {
            using (_dbContext)
            {
                AppSetting appSettings = _dbContext.AppSettings.Where(x => x.AppSettingType == "SOPCustomAccessLink").FirstOrDefault();
                SOP_CustomAccessLink sOP_CustomAccessLink_db = new SOP_CustomAccessLink();
                sOP_CustomAccessLink_db.BuildGuid = sOPCustomAccessLink.AccessBuildGuid;
                sOP_CustomAccessLink_db.UpdatedOn = DateTime.Now;
                sOP_CustomAccessLink_db.Updated_By = User.FindFirst(ClaimTypes.NameIdentifier).Value; 
                sOPCustomAccessLink.AccessGrantedBy = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (string.IsNullOrEmpty(sOPCustomAccessLink.LinkGuid))
                {
                    Guid newGuid = Guid.NewGuid();
                    sOP_CustomAccessLink_db.LinkGuid = newGuid.ToString();
                    sOPCustomAccessLink.Link = appSettings.DataJson + sOP_CustomAccessLink_db.LinkGuid;
                    sOPCustomAccessLink.LinkGuid = sOP_CustomAccessLink_db.LinkGuid;
                    sOP_CustomAccessLink_db.LinkDataJson = JsonConvert.SerializeObject(sOPCustomAccessLink);
                    _dbContext.Add(sOP_CustomAccessLink_db);
                }
                else
                {
                    sOP_CustomAccessLink_db = _dbContext.SOP_CustomAccessLinks.Where(x => x.LinkGuid == sOPCustomAccessLink.LinkGuid).FirstOrDefault();
                    sOPCustomAccessLink.Link = appSettings.DataJson + sOP_CustomAccessLink_db.LinkGuid;
                    sOP_CustomAccessLink_db.LinkDataJson = JsonConvert.SerializeObject(sOPCustomAccessLink);
                    _dbContext.Entry(sOP_CustomAccessLink_db).State = EntityState.Modified;
                }
                _dbContext.SaveChanges();
                return Json(new {success = true, linkguid = sOPCustomAccessLink.Link });
            }
        }

        public IActionResult GetListOfSopAccessLinks(string BuildGuid)
        {
            List<SOPCustomAccessLink> sOPCustomAccessLinksList = new();
            using (_dbContext)
            {
                List<SOP_CustomAccessLink> sOP_CustomAccessLink = _dbContext.SOP_CustomAccessLinks.Where(x=>x.BuildGuid == BuildGuid).ToList();
                foreach (var item in sOP_CustomAccessLink)
                {
                    SOPCustomAccessLink sOPCustomAccessLinks = JsonConvert.DeserializeObject<SOPCustomAccessLink>(item.LinkDataJson);
                    sOPCustomAccessLinksList.Add(sOPCustomAccessLinks);

                }
                return View(sOPCustomAccessLinksList);
            }
 
        }


        [AllowAnonymous]
        public IActionResult SOPCustomAccess(string accessToken = "")
        {
            using (_dbContext)
            {
                ViewBag.customAccess = "true";
                ViewBag.accessExpired = "false";
                SOPCustomAccessLink linkData = new();
                SOP_CustomAccessLink sOP_CustomAccessLink = _dbContext.SOP_CustomAccessLinks.Where(x=>x.LinkGuid == accessToken).FirstOrDefault();
                if (sOP_CustomAccessLink != null)
                {
                    linkData = JsonConvert.DeserializeObject<SOPCustomAccessLink>(sOP_CustomAccessLink.LinkDataJson);

                    ViewBag.customLinkData = sOP_CustomAccessLink.LinkDataJson;
                    if (linkData.access_validity < DateTime.Now)
                    {
                        ViewBag.accessExpired = "true";
                        return View();
                    }

                    if (User.Identity.IsAuthenticated == false)
                    {
                        TemporaryImpersonate(linkData.AccessGrantedBy);
                        return View();
                    }
                    BuildViewModel buildViewModel = GetBuildViewModel(linkData.AccessBuildGuid, "All", "Active", sOP_CustomAccessLink.LinkDataJson);
                    return View("BuildComponents", buildViewModel);
                }
                else
                {
                    ViewBag.accessExpired = "true";
                    return View();
                }
            } 
        }

        public bool TemporaryImpersonate(string userGUID)
        {
            AuthenticateByEmailViewModel authenticateByEmailViewModel = new AuthenticateByEmailViewModel();
            authenticateByEmailViewModel.ClientIP = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            authenticateByEmailViewModel.Browser = Request.Headers["User-Agent"];
            authenticateByEmailViewModel.AuthenticateBy = userGUID;

            string JsonPara = JsonConvert.SerializeObject(authenticateByEmailViewModel);
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.Authenticate_User @Params='{JsonPara}'");
                AuthenticateUser authenticateUser = db.Read<AuthenticateUser>().FirstOrDefault();
                try
                {
                    if (authenticateUser is not null)
                    {
                        if (authenticateUser.IsAuthenticated == true)
                        {
                            authenticateByEmailViewModel.ReturnUrl = "/Home/Index";
                            authenticateByEmailViewModel.IsAuthenticated = authenticateUser.IsAuthenticated;
                            List<UserRoles> userRoles = db.Read<UserRoles>().ToList();
                            var claims = new List<Claim>();
                            claims.Add(new Claim(ClaimTypes.Name, authenticateUser.UserSessionGuid));
                            claims.Add(new Claim(ClaimTypes.NameIdentifier, authenticateUser.UserGuid));
                            claims.Add(new Claim(ClaimTypes.GivenName, authenticateUser.DisplayName));
                            claims.Add(new Claim(ClaimTypes.Email, authenticateUser.Email));
                            claims.Add(new Claim(ClaimTypes.Sid, authenticateUser.UserDbkey.ToString()));
                            foreach (var item in userRoles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, item.RoleGuid));
                            }
                            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                            //claims.Add(new Claim(ClaimTypes.Upn, GenerateJwtToken(identity)));
                            //identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var principal = new ClaimsPrincipal(identity);
                            var props = new AuthenticationProperties();
                            //props.IsPersistent = model.RememberMe;
                            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props).Wait();
                           return User.Identity.IsAuthenticated;
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                    return false;
                } 
            }
        }


        public JsonResult GetBuildJsTreeData(string buidguid, string filter = "All",string activestatus = "Active", string customAccessJsonData = "")
        {
            BuildViewModel buildViewModel = GetBuildViewModel(buidguid, filter, activestatus);
            List<MplJsTreeViewModel> myArrays = ContructJsTreeModel(buildViewModel, filter, customAccessJsonData);
            return Json(myArrays);
        }
        public IActionResult DeleteAttachment(int id)
        {
            try
            {
                MPGlobals.ExceSQLNonQuery($"Delete from [dbo].[Attachments] where [Attachment_Db_Key] = {id}");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
            return Json(new { success = true, msg = "File deleted successfully" });
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.SOP_Write)]
        public ActionResult UpdatePartDetail(int Id,string customAccess = "false")
        {
            EngineBuildComponentViewModel engineBuildComponentViewModel = new EngineBuildComponentViewModel();

            if (customAccess == "true")
            {
                ViewBag.customAccess = "true";
            }
            else
            {
				ViewBag.customAccess = "false";
			}

            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var db = connection.QueryMultiple($"[dbo].[SOP_EngineIndividualPartComponent_SSP] @Id={Id}");
                    engineBuildComponentViewModel = db.Read<EngineBuildComponentViewModel>().FirstOrDefault();
                }

                engineBuildComponentViewModel = engineBuildComponentViewModel ?? new EngineBuildComponentViewModel();
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); }
            return PartialView(engineBuildComponentViewModel);
        }

        public ActionResult GetAttachments(int itemKey)
        {
            return PartialView(itemKey);
        }

        [HttpPost]
        [ClaimRequirement(UserPermissions.SOP_Write)]
        public ActionResult UpdatePartDetail(EngineBuildComponentViewModel engineBuildComponentViewModel)
        {
            using (_dbContext)
            {
                EngineBuildComponent dbmodel = _dbContext.EngineBuildComponents.Where(x => x.Id == engineBuildComponentViewModel.Id).FirstOrDefault();
                try
                {
                    // Compare Reporting parent is active;
                    engineBuildComponentViewModel.BuildDbkey = dbmodel.BuildDbkey;
					engineBuildComponentViewModel.EnginePartDbkey = dbmodel.EnginePartDbkey;
					DTOResponse response = ValidateReportingParentStatus(engineBuildComponentViewModel);
					if (response.Result == false)
					{
						return Json(new { success = false, id = dbmodel.Id, msg = response.ResponseMessage });
					}
 
					bool isactive_bool = dbmodel.IsActive == 1 ? true : false;
                    bool ChangesOnlyInReportingParent = false;

                    bool DataChanges = engineBuildComponentViewModel.DrawingNumber == dbmodel.DrawingNumber
                        && engineBuildComponentViewModel.Revision == dbmodel.Revision
                        && engineBuildComponentViewModel.QtyPerEngine == dbmodel.QtyPerEngine
                        && engineBuildComponentViewModel.Description == dbmodel.Description
                        && engineBuildComponentViewModel.JobCard == dbmodel.JobCard
                        && engineBuildComponentViewModel.SerialNumber == dbmodel.SerialNumber
                        && engineBuildComponentViewModel.Remarks == dbmodel.Remarks
                        && engineBuildComponentViewModel.SchemeNumber == dbmodel.SchemeNumber;


                    if (DataChanges
                        && engineBuildComponentViewModel.IsActive == isactive_bool
						//&& engineBuildComponentViewModel.ReportingParent == dbmodel.ReportingParent
						&& (engineBuildComponentViewModel.IsRemoved) ==(dbmodel.IsRemoved == null ? false : dbmodel.IsRemoved) 
                        && engineBuildComponentViewModel.IsReplaced == (dbmodel.IsReplaced == null ? false : dbmodel.IsReplaced))
                    {
      //                  int? DbmodelRepParent = dbmodel.ReportingParent == 0 ? dbmodel.ParentId : dbmodel.ReportingParent;

						//if (engineBuildComponentViewModel.ReportingParent == DbmodelRepParent)
      //                  {
							return Json(new { success = true, id = dbmodel.Id, msg = "No changes to update" });
						//}
      //                  else
      //                  {
						//	dbmodel.ReportingParent = engineBuildComponentViewModel.ReportingParent;
						//	_dbContext.Entry(dbmodel).State = EntityState.Modified;
						//	_dbContext.SaveChanges();
						//	return Json(new { success = true, id = dbmodel.Id, msg = "Updated Successfully" });
						//}
                    }
 
					dbmodel.IsRemoved = false;
					dbmodel.IsReplaced = false;
					dbmodel.IsUpdated = false;

					int isactive_int = engineBuildComponentViewModel.IsActive == true ? 1 : 0;

					if (dbmodel.IsRemoved != engineBuildComponentViewModel.IsRemoved)
					{
                        dbmodel.IsRemoved = engineBuildComponentViewModel.IsRemoved;
						dbmodel.IsNewlyAdded = false; 
					}
                    else if ((dbmodel.IsReplaced = (dbmodel.IsReplaced == null ? false : dbmodel.IsReplaced)) != engineBuildComponentViewModel.IsReplaced)
                    {
						dbmodel.IsReplaced = engineBuildComponentViewModel.IsReplaced;
						dbmodel.IsNewlyAdded = false;
					}
                    else if (dbmodel.IsActive != isactive_int)
                    {
                        dbmodel.IsActive = isactive_int;
						dbmodel.IsNewlyAdded = true;
					}
                    else if(dbmodel.IsNewlyAdded == true)
                    {
						dbmodel.IsNewlyAdded = true;
					}
                    else
                    {          
                        
						dbmodel.IsUpdated = !DataChanges;
					}


					dbmodel.DrawingNumber = engineBuildComponentViewModel.DrawingNumber;
                    dbmodel.SchemeNumber = engineBuildComponentViewModel.SchemeNumber;
                    dbmodel.SerialNumber = engineBuildComponentViewModel.SerialNumber;
                    dbmodel.ContractNumber = engineBuildComponentViewModel.ContractNumber;
                    dbmodel.QtyPerEngine = engineBuildComponentViewModel.QtyPerEngine;
                    dbmodel.Revision = engineBuildComponentViewModel.Revision;
                    dbmodel.Remarks = engineBuildComponentViewModel.Remarks;
                    dbmodel.JobCard = engineBuildComponentViewModel.JobCard;
                    dbmodel.UpdatedBy = User.Identity.Name;
                    dbmodel.UpdatedOn = DateTime.Now;
                    dbmodel.Description = engineBuildComponentViewModel.Description;
                    //if (engineBuildComponentViewModel.ReportingParent != dbmodel.ParentId)
                    //{
                    //    dbmodel.ReportingParent = engineBuildComponentViewModel.ReportingParent;
                    //}
                        
					_dbContext.Entry(dbmodel).State = EntityState.Modified;
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, id = dbmodel.Id, msg = ex.Message });
                }

                return Json(new { success = true, id = dbmodel.Id, msg = "Updated Successfully" });
            }

        }


        private DTOResponse ValidateReportingParentStatus(EngineBuildComponentViewModel engineBuildComponentViewModel)
        {
			DTOResponse dTOResponse = new DTOResponse();
            dTOResponse.Result = true;
			DataTable dataTable;

		
		      dataTable = MPGlobals.GetDataForDatatable($"SOP_ValidateReportingParent_SSP @PartID={engineBuildComponentViewModel.ReportingParent},@buildId = {engineBuildComponentViewModel.BuildDbkey} , @ValidationCase = 1");
            
               if (dataTable.Rows[0][0].ToString() == "0")
               {
                   dTOResponse.Result = false;
		    	  dTOResponse.ResponseMessage = dataTable.Rows[0][1].ToString();
		       }


            if (engineBuildComponentViewModel.IsRemoved)
            {
				dataTable = MPGlobals.GetDataForDatatable($"SOP_ValidateReportingParent_SSP @PartID={engineBuildComponentViewModel.EnginePartDbkey},@buildId = {engineBuildComponentViewModel.BuildDbkey} , @ValidationCase = 2");

				if (dataTable.Rows[0][0].ToString() == "0")
				{
					dTOResponse.Result = false;
					dTOResponse.ResponseMessage = dataTable.Rows[0][1].ToString();
				}
			}

            return dTOResponse;
		}




		[HttpGet]
        [ClaimRequirement(UserPermissions.SOP_Write)]
        public ActionResult AdditionalSopBuildComponent(int Id = 0,int parentid = 0)
        {
            //parentid is ID from EngineBuildComponents table
            SOP_AdditionalComponentVM engineBuildComponentViewModel = new SOP_AdditionalComponentVM();

            if (Id == 0)
            {
                EngineBuildComponentViewModel engineBuildComponent_parentdetail = new EngineBuildComponentViewModel();
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var db = connection.QueryMultiple($"[dbo].[SOP_EngineIndividualPartComponent_SSP] @Id={parentid}");
                    engineBuildComponent_parentdetail = db.Read<EngineBuildComponentViewModel>().FirstOrDefault();
                }
                engineBuildComponentViewModel.Parent_id = engineBuildComponent_parentdetail.EnginePartDbkey;
                engineBuildComponentViewModel.BL_Engine_Dbkey = engineBuildComponent_parentdetail.BaseLineEngineDbkey;
                engineBuildComponentViewModel.BuildId = engineBuildComponent_parentdetail.BuildDbkey;
                ViewBag.ParentPart = engineBuildComponent_parentdetail.DrawingNumber;   
            }
            else
            {
                //using (var connection = mPDapperContext.CreateConnection())
                //{
                //    var db = connection.QueryMultiple($"[dbo].[SOP_EngineIndividualPartComponent_SSP] @Id={Id}");
                //    engineBuildComponentViewModel = db.Read<EngineBuildComponentViewModel>().FirstOrDefault();
                //}
                ViewBag.ParentPart = engineBuildComponentViewModel.DrawingNumber;   
            }

            return PartialView(engineBuildComponentViewModel);
        }
        [HttpPost]
        [ClaimRequirement(UserPermissions.SOP_Write)]
        public ActionResult AdditionalSopBuildComponent(SOP_AdditionalComponentVM sOP_AdditionalComponentVM)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("Engine_Part_Dbkey", sOP_AdditionalComponentVM.Engine_Part_Dbkey);
                    parameters.Add("BuildId", sOP_AdditionalComponentVM.BuildId);
                    parameters.Add("Type_Dbkey", sOP_AdditionalComponentVM.Type_Dbkey);
                    parameters.Add("BL_Engine_Dbkey", sOP_AdditionalComponentVM.BL_Engine_Dbkey);
                    parameters.Add("Parent_id", sOP_AdditionalComponentVM.Parent_id);
                    parameters.Add("DrawingNumber", sOP_AdditionalComponentVM.DrawingNumber);
                    parameters.Add("Revision", sOP_AdditionalComponentVM.Revision);
                    parameters.Add("QtyPerEngine", sOP_AdditionalComponentVM.QtyPerEngine);
                    parameters.Add("Description", sOP_AdditionalComponentVM.Description);
                    parameters.Add("RawMaterial", sOP_AdditionalComponentVM.RawMaterial);
                    parameters.Add("Module_Responsibility", sOP_AdditionalComponentVM.Module_Responsibility);
                    parameters.Add("UpdateBy", User.Identity.Name);
                    parameters.Add("UpdatedOn", DateTime.Now);
                    var results = connection.Query("SOP_AdditionalBuilComponent_IUSP", parameters, commandType: CommandType.StoredProcedure).ToList();

                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
  
            return Json(new { success = true, msg = "Updated Successfully" });
        }

         
        private BuildViewModel GetBuildViewModel(string buidguid, string filter = "All", string activestatus = "Active", string CustomAccessData ="")
        {
            BuildViewModel buildViewModel = new BuildViewModel();
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var db = connection.QueryMultiple($"dbo.SOP_EnginePartsComponents_SSP @BuidGuid='{buidguid}', @filter='{filter}',@ActiveStatus = '{activestatus}'");
                    BaseLineEngineVM baseLineEngine = db.Read<BaseLineEngineVM>().FirstOrDefault();
                    EngineBuildsVM engineBuildViewModel = db.Read<EngineBuildsVM>().FirstOrDefault();
                    List<EnginePartsViewModel> enginePartsViewModel = db.Read<EnginePartsViewModel>().ToList();
                    buildViewModel.baseLineEngineVM = baseLineEngine;
                    buildViewModel.engineBuildViewModel = engineBuildViewModel;
                    buildViewModel.enginePartsViewModel = enginePartsViewModel;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
            return buildViewModel;
        }

        private static List<MplJsTreeViewModel> ContructJsTreeModel(BuildViewModel buildViewModel,string filter = "All", string customAccessJsonData = "")
        {
            List<MplJsTreeViewModel> masterparts_Jstrees = new List<MplJsTreeViewModel>();
            try
            {
                MplJsTreeViewModel myArray = new MplJsTreeViewModel();
                myArray.id = buildViewModel.baseLineEngineVM.BL_Engine_Dbkey.ToString();
                myArray.text = buildViewModel.baseLineEngineVM.Engine_Title.ToString();
                myArray.icon = "fa fa-fighter-jet";
                myArray.state = new State();
                myArray.state.opened = true;
                var customAccessID_List = new List<string>();
                if (customAccessJsonData != "")
                {
                    // Parse the JSON string
                    var jsonDoc = JsonDocument.Parse(customAccessJsonData);

                    // Extract the 'modules' value
                    string customAccessID_csv = jsonDoc.RootElement.GetProperty("modules").GetString();
                     customAccessID_List = customAccessID_csv.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                
                List<Category> flatObjects = new List<Category>();
                foreach (EnginePartsViewModel enginePartsViewModel in buildViewModel.enginePartsViewModel)
                {
                    Category category = new Category();
                    category.id = enginePartsViewModel.EnginePartDbkey.ToString() + "_" + enginePartsViewModel.PartBuildDbkey.ToString();
                    category.isactive = enginePartsViewModel.IsActive ?? 0;
                    category.Parent_id = enginePartsViewModel.ParentId;
                    category.text = enginePartsViewModel.PartDisplayName;
                    category.Isupdated = enginePartsViewModel.Isupdated;
                    category.ForSopOnly = enginePartsViewModel.ForSopOnly;
                    category.IsReplaced = enginePartsViewModel.IsReplaced;
                    category.IsNewlyAdded = enginePartsViewModel.IsNewlyAdded;
					category.IsRemoved = enginePartsViewModel.IsRemoved;
					flatObjects.Add(category);
                }
                myArray.children = FillRecursive(flatObjects, 0,0, customAccessID_List);
                masterparts_Jstrees.Add(myArray);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
            return masterparts_Jstrees;
        }

        private static List<MplJsTreeViewModel> FillRecursive(List<Category> flatObjects, int? parentId = null, int id = 0, List<string> customAccessCompId = null )
        {
            var childrenFlatItems = flatObjects.Where(i => i.Parent_id == parentId);
            customAccessCompId ??= new List<string>();
            return childrenFlatItems.Select(i => new MplJsTreeViewModel
            {
                text = i.text,
                id = i.id.ToString(),
              //  icon = "fa fa-cogs",
                icon = GetIcon(i.id,customAccessCompId),
                state = GetStates(i.isactive),
                a_attr = Getattr(i.isactive, i.Isupdated,i.ForSopOnly,i.IsReplaced,i.IsNewlyAdded,i.IsRemoved),
                children = FillRecursive(flatObjects, int.Parse(i.id.Split("_")[0]), id, customAccessCompId),
            }).ToList();

        }

        private static A_attr Getattr(int isactive, bool? isupdated,bool? soppartonly, bool? isreplaced, bool? IsNewlyAdded, bool? IsRemoved)
        {
            A_attr a_Attr = new A_attr();
            try
            {
                a_Attr.Class = "jstree-anchor Notupdated";

                if (isupdated ?? false)
                {
                    a_Attr.Class = "jstree-anchor Isupdated";
                }

                if (IsNewlyAdded??false)
                {
                    a_Attr.Class = "jstree-anchor soppartonly"; // Newly addded
                }

                if (isreplaced??false)
                {
                    a_Attr.Class = "jstree-anchor IsReplaced";
                }


                if (isactive == 0)
                {
                    a_Attr.Class = "jstree-anchor IsInactivated";
                }

                if (IsRemoved??false)
                {
                    a_Attr.Class = "jstree-anchor IsInactivated";
                }

            



                //if (isactive == 0)
                //{
                //    a_Attr.Class = "jstree-anchor";
                //}
                //else
                //{
                //    a_Attr.Class = "jstree-anchor";
                //}
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); }
            return a_Attr;
        }

        private static State GetStates(int id)
        {
            State state = new State();
            state.opened = true;
            try
            {
                if (id == 0)
                {
                    state.selected = false;
                }
                else
                {
                    state.selected = false;
                }
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); }
            return state;
        }

        private static string GetIcon(string id, List<string> customAccessCompId)
        {
            string icon = "fa fa-cogs";
            foreach (var item in customAccessCompId)
            {
                if (id == item)
                {
                    icon = "fa fa-edit";
                }
            }
            return icon;
           
        }

        [ClaimRequirement(UserPermissions.SOP_Write)]
        public IActionResult CloneSOPEngineBuild( int id)
        {
            string UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            try
            {
                MPGlobals.ExceSQLNonQuery($"Exec [dbo].[Clone_SOP_Engine_SSP] @Id ='{id}' , @userId='{UserId}'");
                return Json(new { success = true, Msg = "Success" });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, Msg = "Clone Failed !" });
            }
        }

        [ClaimRequirement(UserPermissions.SOP_Delete)]
        public IActionResult DeleteBuild(int id)
        {
            string UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            try
            {
                MPGlobals.ExceSQLNonQuery($"Exec [dbo].[DeleteSOPBuild] @Id ='{id}' , @userid='{UserId}'");
                return Json(new { success = true, Msg = "Success" });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, Msg = "Delete Failed !" });
            }
        }

        [HttpGet]
        public IActionResult SOPComparison()
        {
            return View();
        }

        [HttpGet]
        public string GetSOPComparison(int BL1, int BL2, bool ischangeOnly)
        {
            int filterrequest = ischangeOnly == true ? 1 : 0;
            DataTable dataTable = new DataTable();
            dataTable = MPGlobals.GetDataForDatalist($"dbo.SOP_SOP_Comparsion @BuildId1 ={BL1},@BuildId2 = {BL2},@ShowOnlyChanges ={filterrequest}");
            return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
        }

		[HttpPost]
		[ClaimRequirement(UserPermissions.SOP_Write)]
		public ActionResult SubmitSOPAdditionalInfo([FromBody] SOP_AdditionalInfoComponentVM sOP_AdditionalInfoComponentVM) 
        {
			try
			{
				using (var connection = mPDapperContext.CreateConnection())
				{
					DynamicParameters parameters = new DynamicParameters();
					parameters.Add("Id", sOP_AdditionalInfoComponentVM.Id);
					parameters.Add("ReportingParent", sOP_AdditionalInfoComponentVM.ReportingParent);
					parameters.Add("Reporting_Type", sOP_AdditionalInfoComponentVM.Reporting_Type);
					parameters.Add("AssemblyReportingType", sOP_AdditionalInfoComponentVM.AssemblyReportingType);
					var results = connection.Query("SOP_AdditionalPartInfo_ISP", parameters, commandType: CommandType.StoredProcedure).ToList();
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.LogException(ex);
				return Json(new { success = false, msg = ex.Message });
			}

			return Json(new { success = true, msg = "Updated Successfully" });
		}


        public IActionResult BuildsSerialNoCorrection(string BuildGUID = "LatestBuild")
        {
            if (string.IsNullOrEmpty(BuildGUID))
            {
                BuildGUID = "LatestBuild";
            }
            ViewBag.BuildGUID = BuildGUID;
            return View();
        }

        public string BuildSerialNumberDetails(string BuildGUID = "LatestBuild")
        {
            if (string.IsNullOrEmpty(BuildGUID))
            {
                BuildGUID = "LatestBuild";
            }
            DataTable dataTable = MPGlobals.GetDataForDatalist($"EXEC dbo.Get_SOP_DocumentStatus @BuildGUID='{BuildGUID}'");
            // return Json(MPGlobals.GetTableAsList(dataTable));
            var jsonData = JsonConvert.SerializeObject(dataTable);
            return jsonData;
        }

        [HttpPost]
        public IActionResult SaveSerialNo(int EBC_Id , string newSlno)
        {
            try
            {
                EngineBuildComponent EBC = _dbContext.EngineBuildComponents.Where(x => x.Id == EBC_Id).FirstOrDefault();
                if (EBC != null)
                {
                    EBC_SerialNoLog log = new EBC_SerialNoLog();
                    log.EBC_Id = EBC_Id;
                    log.Previous_SerialNo = EBC.SerialNumber;
                    log.Updated_serialNo = newSlno;
                    log.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    log.UpdatedOn = DateTime.Now;
                    _dbContext.Add(log);
                    EBC.SerialNumber = newSlno;
                    _dbContext.Entry(EBC).State = EntityState.Modified;
                    _dbContext.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false });

            }
            catch (Exception ex )
            {
                ErrorHandler.LogException( ex );
                return Json(new { success = false });
            }
        }

        public IActionResult GetColnedBuild(int BuildId)
        {
            EngineBuild buildDetails = _dbContext.EngineBuilds.Where(x => x.ClonedFromKey == BuildId && x.ClonedFrom == "EngineBuilds").FirstOrDefault();
            if(buildDetails == null)
            {
                buildDetails = new EngineBuild();
            }
            return Json(new { buildId = buildDetails.Id, buildName = buildDetails.BuildName });
        }


        public IActionResult AllBuildComparison()
        {
            return View();
        }


        [HttpGet]
        public string GetComparisonData(int BL1, int BL2, bool ischangeOnly)
        {
            int filterrequest = ischangeOnly == true ? 1 : 0;
            DataTable dataTable = new DataTable();
            dataTable = MPGlobals.GetDataForDatalist($"dbo.AllBuildComparisionV2 @BuildId1 ={BL1},@BuildId2 = {BL2},@ShowOnlyChanges ={filterrequest}");
            return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
        }

        [HttpPost]
        public IActionResult SyncBATLData(string buildGuid)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    int userid = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    var parameters = new DynamicParameters();
                    parameters.Add("@BuildGUID", buildGuid, DbType.String, size: 50);
                    parameters.Add("@UpdatedBy", userid , DbType.Int64, size: 128);

                    // if SP returns some info rows (like counts), fetch them
                    var result = connection.QueryFirstOrDefault(
                        "dbo.Update_EBC_FromLatestJson_RowByRow",
                        parameters,
                        commandType: CommandType.StoredProcedure,
                          commandTimeout: 600
                    );

                    return Json(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult BATLPartsUpdationReport()
        {
            return View();
        }

        public IActionResult GEtBATLPartsUpdationData(int buildDbkey, bool showOnlyUnique = false)
        {
            DataSet dataSet = new DataSet();
            int uniqueFlag = showOnlyUnique ? 1 : 0;
            dataSet = MPGlobals.GetDataSet($"BATLPartsUpdationReport @buildDbKey = {buildDbkey}, @ShowOnlyUnique = {uniqueFlag}");

            // First table contains the main data
            DataTable dataTable = dataSet.Tables[0];

            // Second table contains the counts
            DataTable countsTable = dataSet.Tables[1];

            var response = new
            {
                data = dataTable,
                counts = new
                {
                    total = countsTable.Rows[0]["TotalCount"],
                    available = countsTable.Rows[0]["AvailableCount"],
                    pending = countsTable.Rows[0]["PendingCount"]
                }
            };

            var jsonData = JsonConvert.SerializeObject(response);
            return Content(jsonData);
        }

        [HttpPost]
        [ClaimRequirement(UserPermissions.SOP_Write)]
        public ActionResult RefreshComponentFromMPL(int componentId)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("ComponentId", componentId);

                    var result = connection.QueryFirstOrDefault<dynamic>(
                        "RefreshSOPComponentFromMPL",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    if (result != null && result.Success == 1)
                    {
                        return Json(new
                        {
                            success = true,
                            msg = result.Message,
                            data = new
                            {
                                drawingNumber = result.DrawingNumber,
                                description = result.Description,
                                parentId = result.ParentId,
                                qtyPerEngine = result.QtyPerEngine,
                                reportingParent = result.ReportingParent,
                                reportingType = result.ReportingType,
                                assemblyReportingType = result.AssemblyReportingType
                            }
                        });
                    }
                    else
                    {
                        string errorMsg = result?.Message ?? "Component not found";
                        return Json(new { success = false, msg = errorMsg });
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = "Error refreshing component: " + ex.Message });
            }
        }


        [HttpGet]
        [ClaimRequirement(UserPermissions.SOP_Write)]
        public ActionResult AddComponentWithExistingPart(int parentid = 0)
        {
            // parentid is ID from EngineBuildComponents table
            SOP_AddWithExistingPartVM viewModel = new SOP_AddWithExistingPartVM();

            if (parentid > 0)
            {
                // Get parent component details
                EngineBuildComponentViewModel parentDetail = new EngineBuildComponentViewModel();
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var db = connection.QueryMultiple($"[dbo].[SOP_EngineIndividualPartComponent_SSP] @Id={parentid}");
                    parentDetail = db.Read<EngineBuildComponentViewModel>().FirstOrDefault();
                }

                // Set context from parent
                viewModel.Parent_id = parentDetail.EnginePartDbkey;
                viewModel.BL_Engine_Dbkey = parentDetail.BaseLineEngineDbkey;
                viewModel.BuildId = parentDetail.BuildDbkey;
                ViewBag.ParentPart = parentDetail.DrawingNumber;
            }

            // Get all parts data using stored procedure (pass the baseline engine key)
            using (var connection = mPDapperContext.CreateConnection())
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("BL_Engine_Dbkey", viewModel.BL_Engine_Dbkey ?? 0);

                var allParts = connection.Query<dynamic>(
                    "[dbo].[SOP_GetAllPartsForSelection_SSP]",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                ViewBag.AllPartsData = JsonConvert.SerializeObject(allParts);
            }

            return PartialView(viewModel);
        }


        [HttpPost]
        [ClaimRequirement(UserPermissions.SOP_Write)]
        public ActionResult AddComponentWithExistingPart(SOP_AddWithExistingPartVM viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, Msg = "Please fill all required fields" });
                }

                using (var connection = mPDapperContext.CreateConnection())
                {
                    // First, get the Part_relation_dbkey from the stored procedure data
                    // We need to find the Part_relation_dbkey for the selected Engine_Part_Dbkey
                    var partRelationQuery = @"
                SELECT Part_relation_dbkey 
                FROM Engine_Parts_Usage 
                WHERE Engine_Part_Dbkey = @EnginePartDbkey 
                    AND BL_Engine_Dbkey = @BL_Engine_Dbkey 
                    AND Engine_Dbkey = 0
                    AND ISNULL(is_active, 1) = 1";

                    var partRelationKey = connection.QueryFirstOrDefault<int?>(partRelationQuery,
                        new
                        {
                            EnginePartDbkey = viewModel.SelectedEnginePartDbkey,
                            BL_Engine_Dbkey = viewModel.BL_Engine_Dbkey
                        });

                    if (partRelationKey == null || partRelationKey == 0)
                    {
                        return Json(new { success = false, Msg = "Part relation key not found for selected part" });
                    }

                    // Call the stored procedure
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("BuildDbkey", viewModel.BuildId);
                    parameters.Add("BaseLineEngineDbkey", viewModel.BL_Engine_Dbkey);
                    parameters.Add("EnginePartDbkey", viewModel.SelectedEnginePartDbkey);
                    parameters.Add("ParentId", viewModel.Parent_id);
                    parameters.Add("PartRelationKey", partRelationKey);
                    parameters.Add("UpdatedBy", User.FindFirst(ClaimTypes.NameIdentifier).Value);

                    var result = connection.QueryFirstOrDefault<dynamic>(
                        "[dbo].[SOP_AddComponentWithExistingPart_ISP]",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    if (result != null && result.Result == "Success")
                    {
                        return Json(new { success = true, Msg = result.Message });
                    }
                    else
                    {
                        return Json(new { success = false, Msg = "Failed to add component" });
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, Msg = "Failed to add component: " + ex.Message });
            }
        }


    }
}
