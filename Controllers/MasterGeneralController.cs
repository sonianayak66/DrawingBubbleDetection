using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using System.Data;
using Dapper;
using Newtonsoft.Json;
using System;
using XAct;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using static MPCRS.Utilities.Constants;
using XAct.Library.Settings;

namespace MPCRS.Controllers
{
    [Authorize]
    public class MasterGeneralController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;
        public MasterGeneralController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
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

        [HttpGet]
        public IActionResult MasterGeneral(string Type = "")
        {
            Master_GeneralVM master_GeneralVM = new Master_GeneralVM();
            master_GeneralVM.master_Generals = getGeneralmasters(Type);
            ViewBag.mastertype = Type;
            ViewBag.masterheader = Type.Replace("_", " ");
            return PartialView(master_GeneralVM);
        }

        public static List<Master_General> getGeneralmasters(string type)
        {
            using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
            {
                {
                    List<Master_General> master_Generals = db.Master_Generals.Where(x => x.Master_Type == type).ToList();
                    return master_Generals;
                }
            }
        }


        [HttpPost]
        [ClaimRequirement(UserPermissions.Configuration_Write)]
        public IActionResult MasterGeneral(string Name = "", int ID = 0, string type = "", bool delete = false, string Miscvalue = "0")
        {
            using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
            {
                try
                {
                    Master_General master_General = db.Master_Generals.Where(x => x.Master_Name == Name && x.Master_Type == type && x.Misc == Miscvalue && x.Master_Dbkey != ID).FirstOrDefault();

                    if (master_General != null)
                    {
                        return Json(new { success = false, msg = $"The {type} {Name} already exists" });                       
                    }

                    Master_General master_General1 = new Master_General();

                    master_General1.Master_Dbkey = ID;
                    master_General1.Master_Name = Name;
                    master_General1.Master_Type = type;
                    master_General1.Misc = Miscvalue;
                    master_General1.is_active = 1;
                    master_General1.Updated_by = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    master_General1.Updated_on = DateTime.Now;

                    if (master_General1.Master_Dbkey == 0)
                    {
                        db.Master_Generals.Add(master_General1);
                        db.SaveChanges();                      
                    }
                    else
                    {                       
                        db.Entry(master_General1).State = EntityState.Modified;
                        db.SaveChanges();                     
                    }
                    return Json(new { success = true, Msg = "Saved Successfully" });
                }
                catch (Exception ex) { return Json(new { success = false, msg = ex.Message }); }              
        
            }

        }

        [ClaimRequirement(UserPermissions.Configuration_Write)]
        public IActionResult ChangeMasterStatus(int dbkey, int state)
        {
           try
            {
                int activation = state == 0 ? 1 : 0;
                MPGlobals.ExceSQLNonQuery($"update  [dbo].[Master_General] set [is_active] = {activation},[Updated_by] = {int.Parse(User.FindFirst(ClaimTypes.Sid).Value)}, [Updated_on] = GETDATE() where  [Master_Dbkey] = {dbkey}");
                return Json(new { success = true, Msg = "Updated Successfully" });                               
            }
            catch (Exception ex) { return Json(new { success = false, msg = ex.Message }); }           
        }



    }
}
