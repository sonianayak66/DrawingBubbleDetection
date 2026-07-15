using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using System.Data;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Controllers
{
    [Authorize]
    public class ACSNController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;

        public ACSNController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

        [ClaimRequirement(UserPermissions.ACSN_Read)]
        public IActionResult Index()
        {
            return View();
        }

        [ClaimRequirement(UserPermissions.ACSN_Read)]
        public IActionResult AcsnList()
        {
            return View(DataCaching.getCachedACSNList());
        }


        [ClaimRequirement(UserPermissions.ACSN_Write)]
        public ActionResult ManageItem(int id = 0)
        {
            ACSNvm acsnItem = new ACSNvm();
            using (_dbContext)
            {
                var dbmodel = _dbContext.ACSNs.AsNoTracking().Where(x => x.acsnKey == id).FirstOrDefault();
                if (dbmodel != null)
                {
                    var jsonData = JsonConvert.SerializeObject(dbmodel);
                    acsnItem = JsonConvert.DeserializeObject<ACSNvm>(jsonData);
                }
            }
            return View(acsnItem);
        }


        [ClaimRequirement(UserPermissions.ACSN_Write)]
        [HttpPost]
        public ActionResult ManageItem(ACSN acsnItem)
        {
            acsnItem.UpdatedOn = DateTime.Now;
            //acsnItem.updatedBy = User.Identity.Name;
            using (_dbContext)
            {
                if (acsnItem.acsnKey != 0)
                {
                    ACSN dbRecord = _dbContext.ACSNs.AsNoTracking().Where(x => x.acsnKey == acsnItem.acsnKey).FirstOrDefault();
                    if (dbRecord != null)
                    {
                      
                        _dbContext.Entry(acsnItem).State = EntityState.Modified; 
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        // this is to ensure it catches the next if condition
                        acsnItem.acsnKey = 0;
                    }
                }

                if (acsnItem.acsnKey == 0)
                {
                    _dbContext.ACSNs.Add(acsnItem);
                    _dbContext.SaveChanges();
                    string cmd = $"EXEC dbo.ACSN_AssignSerialNumber @ascnkey ={acsnItem.acsnKey}, @Series = '{acsnItem.Series}'";
                    MPGlobals.ExceSQLNonQuery(cmd);
                }

                DataCaching.removeCache(Constants.CacheKeys.ACSNList.ToString());
                return RedirectToAction("Index");
            }

        }


        public IActionResult ValidateModuleRefNum(string moduleRefNum, string acsnKey)
        {
            // validate if the module ref number is unique using entity framework
            using (_dbContext)
            {
                var dbRecord = _dbContext.ACSNs.AsNoTracking().Where(x => x.ModuleRefNum == moduleRefNum && x.acsnKey != Convert.ToInt32(acsnKey)).FirstOrDefault();
                if (dbRecord != null)
                {
                    return Json(new { isduplicate = true });
                }
            }
            return Json(new { isduplicate = false });
        }



        [ClaimRequirement(UserPermissions.ACSN_Read)]
        [Authorize]
        public ActionResult Status(int id)
        {
            List<ACSNItemVm> acsnItems = new List<ACSNItemVm>();
            DataTable dataTable = MPGlobals.GetDataForDatalist($"dbo.Get_ACSN_items @acsnKey = {id}");
            var jsonData = JsonConvert.SerializeObject(dataTable);
            acsnItems = JsonConvert.DeserializeObject<List<ACSNItemVm>>(jsonData);
            var acsnKey = acsnItems.FirstOrDefault()?.acsnKey;
            using (_dbContext)
            {
                ACSN acsn = _dbContext.ACSNs.Where(x => x.acsnKey == acsnKey).FirstOrDefault();
                ViewBag.ACSN = acsn;
            }

            return PartialView(acsnItems);
        }

        [ClaimRequirement(UserPermissions.ACSN_Write)]
        [HttpPost]
        public ActionResult ACSNStatus()
        {
            try
            {
                var JsonData = Request.Form["ACSNStatus"];
                ACSNItem acsnItem = JsonConvert.DeserializeObject<ACSNItem>(JsonData);
                using (_dbContext)
                {
                    ACSNItem dbitem = _dbContext.ACSNItems.Where(x => x.ACSNStatusKey == acsnItem.ACSNStatusKey).FirstOrDefault();
                    if (acsnItem.isActiveStatus == true)
                    {
                        MPGlobals.ExceSQLNonQuery($"UPDATE [dbo].[ACSNItems] SET [isActiveStatus] = 0 WHERE [acsnKey] = {dbitem.acsnKey}");
                    }

                    dbitem.ACSNStatusKey = acsnItem.ACSNStatusKey;
                    dbitem.EndDate = acsnItem.EndDate;
                    dbitem.StartDate = acsnItem.StartDate;
                    dbitem.isActiveStatus = acsnItem.isActiveStatus;
                    dbitem.acsnStatus = acsnItem.acsnStatus;
                    dbitem.updatedOn = acsnItem.updatedOn;
                    dbitem.Remarks = acsnItem.Remarks;
                    _dbContext.Entry(dbitem).State = EntityState.Modified;
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }

            DataCaching.removeCache(Constants.CacheKeys.ACSNList.ToString());
            return Json(new { success = true, Msg = "Saved successfully" });
        }


        [ClaimRequirement(UserPermissions.ACSN_Read)]
        [Authorize]
        public ActionResult GetAttachments(int itemKey)
        {
            return PartialView(itemKey);
        }


        [ClaimRequirement(UserPermissions.ACSN_Delete)]
        [Authorize]
        [HttpPost]
        public ActionResult Delete(int acsnKey)
        {
            try
            {
                MPGlobals.ExceSQLNonQuery($"DELETE FROM [dbo].[ACSNItems] WHERE [acsnKey] = {acsnKey}");
                MPGlobals.ExceSQLNonQuery($"DELETE FROM [dbo].[ACSN] WHERE [acsnKey] = {acsnKey}");
                DataCaching.removeCache(Constants.CacheKeys.ACSNList.ToString());

                return Json(new { success = true, Msg = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                DataCaching.removeCache(Constants.CacheKeys.ACSNList.ToString());

                return Json(new { success = false, Msg = "Failed to delete, please check exception logs for more details" });
            }
        }


        [ClaimRequirement(UserPermissions.ACSN_Read)]
        public IActionResult Dashbord()
        {
            //return View(DataCaching.getCachedDashborad());
            return View();
        }

        public ActionResult DashboardSummary()
        {
            string cmdstr = @$"EXEC [dbo].[ACSN_OpenItemsByAge]";
            DataTable dt = MPGlobals.GetDataForDatalist(cmdstr);
            return Json(MPGlobals.GetTableAsList(dt));
            //var AcnsSeries = DataCaching.getCachedDashborad();
            //return Json(new {data= AcnsSeries });
        }

        [ClaimRequirement(UserPermissions.ACSN_Read)]
        public IActionResult ACSNStatusSummary()
        {
            DataTable dt = MPGlobals.GetDataForDatalist("EXEC [dbo].[ACSN_GetStepSummary]");
            var jsonData = JsonConvert.SerializeObject(dt);
            var acsnItemsSummary = JsonConvert.DeserializeObject<List<ACSNStepSummary>>(jsonData);
            return View(acsnItemsSummary);
        }
    }
}
