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


namespace MPCRS.Controllers
{
    [Authorize]
    public class VendorController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;

        public VendorController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

        [ClaimRequirement(UserPermissions.Vendors_Read)]
        public IActionResult Vendors()
        {
            List<Vendors_VM> vendorinfo = new();
           try
           {
           using (var connection = mPDapperContext.CreateConnection())
               {
                   var db = connection.QueryMultiple($"dbo.VendorList_SP");
                   vendorinfo = db.Read<Vendors_VM>().ToList();
               }
           }
           catch (Exception ex)
           {
           ErrorHandler.LogException(ex);
           }

            return View(vendorinfo);
            
        }
        [HttpGet]
        [ClaimRequirement(UserPermissions.Vendors_Write)]
        public IActionResult CreateVendor(int Id)
        {
            Vendors_VM vm = new();
            using (_dbContext)
            {
              
                    if (Id != 0)
                    {
                        Vendor vend = _dbContext.Vendors.Where(x => x.Vendor_Dbkey == Id).FirstOrDefault();
                        if (vend != null)
                        {
                            vm.Vendor_Dbkey = vend.Vendor_Dbkey;
                        
                        vm.Vendor_ID_User = vend.Vendor_ID_User;
                        vm.Vendor_Name = vend.Vendor_Name;
                        vm.Vendor_Email = vend.Vendor_Email;
                        vm.Vendor_Contact = vend.Vendor_Contact;
                        vm.Vendor_Adress = vend.Vendor_Adress;
                        vm.Vendor_State = vend.Vendor_State;
                        vm.Vendor_City = vend.Vendor_City;
                        vm.Vendor_Pincode = vend.Vendor_Pincode;
                        vm.Vendor_guid = vend.vendor_GUID;
                        vm.Vendor_ID_System = vend.Vendor_ID_System;

                        return PartialView(vm);
                        }
                    }
                  
             
            }
            return PartialView(vm);
        }

        [HttpPost]
        [ClaimRequirement(UserPermissions.Vendors_Write)]
        public IActionResult CreateVendor(Vendors_VM vm)
        {
            Vendor dbModel = new();
            using (_dbContext)
            {
                try
                {
                    dbModel = JsonConvert.DeserializeObject<Vendor>(JsonConvert.SerializeObject(vm));
                    if (vm.Vendor_Dbkey != 0)
                    {
                        Vendor dbmodel = _dbContext.Vendors.AsNoTracking().Where(x => x.Vendor_Dbkey == vm.Vendor_Dbkey).FirstOrDefault();
                        if (dbmodel != null)
                        {
                            dbModel.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                            dbModel.Updated_On = DateTime.Now;
                            _dbContext.Entry(dbModel).State = EntityState.Modified;
                           
                            _dbContext.SaveChanges();
                        }
                        else
                        {
                            // this is to ensure it catches the next if condition
                            vm.Vendor_Dbkey = 0;
                        }
                    }
                    if (vm.Vendor_Dbkey == 0)
                    {

                        Vendor venmodel = _dbContext.Vendors.Where(x => x.Vendor_Dbkey == dbModel.Vendor_Dbkey).FirstOrDefault();
                        dbModel.vendor_GUID = Guid.NewGuid().ToString();
                        dbModel.Vendor_Dbkey = vm.Vendor_Dbkey;
                        dbModel.Vendor_ID_User = vm.Vendor_ID_User;
                        dbModel.Vendor_ID_System = "VED" + DateTime.Now.Year;
                        dbModel.Vendor_Name = vm.Vendor_Name;
                        dbModel.Vendor_Email = vm.Vendor_Email;
                        dbModel.Vendor_Contact = vm.Vendor_Contact;
                        dbModel.Vendor_Adress = vm.Vendor_Adress;
                        dbModel.Vendor_State = vm.Vendor_State;
                        dbModel.Vendor_City = vm.Vendor_City;
                        dbModel.Vendor_Pincode = vm.Vendor_Pincode;
                        dbModel.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                        dbModel.Updated_On = DateTime.Now;
                        _dbContext.Add(dbModel);
                        _dbContext.SaveChanges();
                    }
                    return Json(new { success = true, msg = "Saved successfully" });
                }
               
                catch (Exception ex) { return Json(new { success = false, msg = ex.Message }); }
            }
           // return RedirectToAction("Vendors");
        }

    }
}
