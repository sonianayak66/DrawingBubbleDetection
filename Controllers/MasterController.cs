using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPCRS;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using System.Data;
using static MPCRS.Utilities.Constants;

namespace HelpDesk.Controllers
{
    [Authorize]
    public class MasterController : Controller
    {

        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;

        public MasterController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

        [ClaimRequirement(UserPermissions.Masters_Read)]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetAllMasters()
         {
            List<MetaMaster> data = Masters.GetMasters();
            return Json(new { success = true, data = data });
        }

        [ClaimRequirement(UserPermissions.Masters_Write)]
        public IActionResult Master(string ID = "")
        {
            MetaMasterVM data = new MetaMasterVM();
            if (ID != "")
            {
                using (_dbContext)
                {
                    try
                    {
						MetaMaster dbMaster = _dbContext.MetaMasters.AsNoTracking().Where(x => x.MasterGUID == ID).FirstOrDefault();
						if (dbMaster != null)
						{
							data.Id = dbMaster.Id;
							data.MasterGUID = dbMaster.MasterGUID;
							data.MasterType = dbMaster.MasterType;
							data.ParentGUID = dbMaster.ParentGUID;
							data.UseValue = dbMaster.UseValue??false;
                            data.IsActive = dbMaster.IsActive ?? false;
							data.DisplayOrder = dbMaster.DisplayOrder;
							data.DisplayText = dbMaster.DisplayText;
						}
					}
                    catch (Exception ex)
                    {
                        ErrorHandler.LogException(ex);
                    } 
                }
            }

            return View(data);
        }
        [HttpPost]
        [ClaimRequirement(UserPermissions.Masters_Write)]
        public IActionResult Master(MetaMasterVM viewData)
        {
            if (ModelState.IsValid)
            {
                using (_dbContext)
                {
                    try
                    {
						MetaMaster data = _dbContext.MetaMasters.Where(x => x.MasterGUID == viewData.MasterGUID).FirstOrDefault();

						if (data != null)
						{
							data.DisplayText = viewData.DisplayText;
							data.MasterType = viewData.MasterType;
							data.DisplayOrder = viewData.DisplayOrder;
							data.UseValue = viewData.UseValue;
							data.IsActive = viewData.IsActive;
							data.UpdatedOn = DateTime.Now;
							_dbContext.Entry(data).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
							_dbContext.SaveChanges();
						}
						else
						{
							data = new MetaMaster();
							data.MasterGUID = Guid.NewGuid().ToString();
							data.DisplayText = viewData.DisplayText;
							data.MasterType = viewData.MasterType;
							data.DisplayOrder = viewData.DisplayOrder;
							data.UseValue = viewData.UseValue;
							data.IsActive = viewData.IsActive;
							data.UpdatedOn = DateTime.Now;
							_dbContext.Add(data);
							_dbContext.SaveChanges();
						}
						Masters.RemoveCache(CacheKeys.MetaMaster.ToString());
					}
                    catch (Exception ex)
                    {
						return Json(new { success = false, msg=ex.Message });
					}
                    
                    return Json(new { success = true, msg = "Saved Successfully" });
                }
            }

            return View(viewData);
        }

    }
}

