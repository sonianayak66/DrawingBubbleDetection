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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MPCRS.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;


        public InventoryController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }
        [ClaimRequirement(UserPermissions.Raw_Materials_Read)]
        public IActionResult RawMaterials()
        {
            List<Master_RawmaterialVM> accessinfo = new();
            using (_dbContext)
            {
                try
                {
                    using (var connection = mPDapperContext.CreateConnection())
                    {
                        var db = connection.QueryMultiple($"dbo.Rawmaterial_SSP");
                        accessinfo = db.Read<Master_RawmaterialVM>().ToList();

                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }

                return View(accessinfo);
            }
        }
        [ClaimRequirement(UserPermissions.Raw_Materials_Write)]
        public IActionResult CreateRawmaterial(string? RMGuid)
        {
            Master_RawmaterialVM vm = new();
            
            if (!string.IsNullOrEmpty(RMGuid))
            {
                using (_dbContext)
                {
                    Master_Rawmaterial MRmodel = _dbContext.Master_Rawmaterials.Where(x => x.RawmaterialGuid == RMGuid).FirstOrDefault();
                    if (MRmodel != null)
                    {
                        vm = JsonConvert.DeserializeObject<Master_RawmaterialVM>(JsonConvert.SerializeObject(MRmodel));

                        // Check if material is used in a demand
                        Procurement_Demand_Item demand_Receipt = _dbContext.Procurement_Demand_Items.Where(x => x.ItemDbKey == MRmodel.Raw_material_Dbkey).FirstOrDefault();
                        if (demand_Receipt != null)
                        {
                            vm.IsUsedInDemand = true;
                        }
                        
                        Master_General MGTmodel = _dbContext.Master_Generals.Where(x => x.Master_Dbkey == MRmodel.RM_Type).FirstOrDefault();
                        if (MGTmodel != null)
                        {
                            vm.RM_Type = MGTmodel.Master_Dbkey.ToString();
                        }
                        Master_General MGUmodel = _dbContext.Master_Generals.Where(x => x.Master_Dbkey == MRmodel.RM_UOM).FirstOrDefault();
                        if (MGUmodel != null)
                        {
                            vm.RM_UOM = MGUmodel.Master_Dbkey.ToString();
                        }
                        var splits = _dbContext.RM_Qty_Vendor_Splits.Where(x => x.Raw_material_Dbkey == MRmodel.Raw_material_Dbkey).ToList();
                        
                    }
                }
            }
            return PartialView(vm);
        }
        [HttpPost]
        [ClaimRequirement(UserPermissions.Raw_Materials_Write)]
        public IActionResult CreateRawmaterial(Master_RawmaterialVM vm)
        {
            using (_dbContext)
            {
                try
                {
                    Master_Rawmaterial MRmodel = new();

                    MRmodel.RawmaterialGuid = vm.RawmaterialGuid;
                    MRmodel.Raw_material_Dbkey = vm.Raw_material_Dbkey;
                    MRmodel.Material_name = vm.Material_name;
                    MRmodel.Dia_mm = vm.Dia_mm;
                    MRmodel.inner_Dia_mm = vm.inner_Dia_mm;
                    MRmodel.height = vm.height;
                    MRmodel.Density = vm.Density;
                    MRmodel.Thick_mm = vm.Thick_mm;
                    MRmodel.Remarks = vm.Remarks;
                    MRmodel.is_active = vm.is_active;
                    MRmodel.MinInventoryThreshold = vm.MinInventoryThreshold;
                    MRmodel.RMQtyPerEngine = vm.RMQtyPerEngine ?? 0;

                    MRmodel.Updated_on = DateTime.Now;
                    MRmodel.Updated_by = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    // Type & UOM
                    Master_General MGTmodel = _dbContext.Master_Generals
                        .FirstOrDefault(x => x.Master_Dbkey == vm.RM_Type.ToInt32());
                    if (MGTmodel != null)
                        MRmodel.RM_Type = MGTmodel.Master_Dbkey;

                    Master_General MGUmodel = _dbContext.Master_Generals
                        .FirstOrDefault(x => x.Master_Dbkey == vm.RM_UOM.ToInt32());
                    if (MGUmodel != null)
                        MRmodel.RM_UOM = MGUmodel.Master_Dbkey;

                    // Generate Raw_material_Name
                    string Raw_Material_Name = $"{vm.Material_name}";
                    Raw_Material_Name += vm.Dia_mm != null ? $" {vm.Dia_mm} mm dia" : " ";
                    if (vm.Dia_mm != null && vm.inner_Dia_mm != null)
                        Raw_Material_Name += " ";
                    Raw_Material_Name += vm.inner_Dia_mm != null ? $"{vm.inner_Dia_mm} mm dia" : " ";
                    if ((vm.Dia_mm != null || vm.inner_Dia_mm != null) && vm.Thick_mm != null)
                        Raw_Material_Name += " ";
                    Raw_Material_Name += vm.Thick_mm != null ? $"{vm.Thick_mm} mm thick " : " ";
                    Raw_Material_Name += $"{MGTmodel?.Master_Name ?? ""}";
                    MRmodel.Raw_material_Name = Raw_Material_Name.Trim();

                    // INSERT / UPDATE
                    if (string.IsNullOrEmpty(vm.RawmaterialGuid))
                    {
                        MRmodel.RawmaterialGuid = Guid.NewGuid().ToString();
                        _dbContext.Add(MRmodel);
                    }
                    else
                    {
                        _dbContext.Entry(MRmodel).State = EntityState.Modified;
                    }

                    _dbContext.SaveChanges();

                    return Json(new { success = true, msg = "Saved successfully" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, msg = ex.Message });
                }
            }
        }
    }
}
