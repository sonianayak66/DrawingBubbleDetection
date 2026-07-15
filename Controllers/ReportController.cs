using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using System.Data;
using System.Drawing;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;



        public ReportController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

        [ClaimRequirement(UserPermissions.Procurement_Reports)]
        public IActionResult ProcurementReports()
        {
            return View();
        }


        #region RM Inventory Detail    
        [ClaimRequirement(UserPermissions.Procurement_Reports)]
        public IActionResult InventoryReport()
        {
            return View();
        }

        [ClaimRequirement(UserPermissions.Procurement_Reports)]
        public IActionResult RawmaterialsInventoryDetail(int Raw_material_Dbkey = 0)
        {
            ViewBag.Raw_material_Dbkey = Raw_material_Dbkey;
            return View();
        }

        [ClaimRequirement(UserPermissions.Procurement_Reports)]
        public IActionResult InventoryReportDetail(int Raw_material_Dbkey)
        {
            ViewBag.Raw_material_Dbkey = Raw_material_Dbkey;
            DataTable dataTable = MPGlobals.GetDataForDatalist($"[dbo].[Raw_Material_Inventory_Total_By_RMKey] @RMkey = {Raw_material_Dbkey}");
            return View(dataTable);
        }

        public IActionResult GetRawmaterialsInventory(int ForConsolidatedReport = 0, int Raw_material_Dbkey=0)
        {
            DataTable dataTable = MPGlobals.GetDataForDatalist($"[dbo].[Raw_Material_Inventory_Report] @ConsolidatedReport = {ForConsolidatedReport},@Raw_material_Dbkey = {Raw_material_Dbkey}");
            return Json(MPGlobals.GetTableAsList(dataTable));
        }
        public ActionResult GetRawmaterialsInventoryDetail(int Raw_material_Dbkey = 0)
        {
            DataTable dataTable = MPGlobals.GetDataForDatalist($"[dbo].[Raw_Material_Inventory_Detail_Report] @ItemDbKey = {Raw_material_Dbkey} ");
            return Json(MPGlobals.GetTableAsList(dataTable));
        }

        #endregion

        #region Demand Status Report
        public IActionResult DemandReportsView()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GetDemandReportsData()
        {
            DataTable dataTable = MPGlobals.GetDataForDatalist("dbo.Get_DemandReport_Data");
            return Json(MPGlobals.GetTableAsList(dataTable));
        }

        public IActionResult DemandDataEntryStatusDetail()
        {
            DemandDataEntryStatus demandDataEntryStatus = new DemandDataEntryStatus();  
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"Exec [dbo].DemandEntryStatus");
                demandDataEntryStatus.procurement_Demands_VM = db.Read<Procurement_Demands_VM>().ToList();
                demandDataEntryStatus.Procurement_Demand_Items_VM = db.Read<Procurement_Demand_Items_VM>().ToList();
                demandDataEntryStatus.Procurement_Demand_Receipt = db.Read<Procurement_Demand_Receipt>().ToList();
                demandDataEntryStatus.procurement_ReceiptItemSplits = db.Read<Procurement_ReceiptItemSplit>().ToList();
            }
            return View(demandDataEntryStatus);
        }

        #endregion

        #region RM Aggregate Inventory Detail
        [ClaimRequirement(UserPermissions.Procurement_Reports)]
        public ActionResult AgRMInventoryReport()
        {
            List<RMInventory> rmInventoryList = new List<RMInventory>();    
            using (var connection = mPDapperContext.CreateConnection())
            {
                    var db = connection.QueryMultiple($"Exec [dbo].[Raw_Material_Inventory_Report] @ConsolidatedReport = 0");
                    rmInventoryList = db.Read<RMInventory>().ToList();
            }
                return View(rmInventoryList);
        }



        #endregion

        [ClaimRequirement(UserPermissions.Procurement_Reports)]
        public ActionResult MaterialIssueReport()
        {
            List<ReportMaterialIssueModel> rmInventoryList = new List<ReportMaterialIssueModel>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"Exec [dbo].[Report_MaterialIssue_SSP]");
                rmInventoryList = db.Read<ReportMaterialIssueModel>().ToList();
            }
            return View(rmInventoryList);
        }


		public IActionResult DemandSplitAndDocMapStatus()
		{
            DemandEntryStatusModel demandDataEntryStatus = new DemandEntryStatusModel();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"Exec [dbo].DemandSplitAndDocMapStatus_SSP");
                demandDataEntryStatus.demandItemQtyStatus = db.Read<DemandItemQtyStatus>().ToList();
            }
			return View(demandDataEntryStatus);
		}

        [ClaimRequirement(UserPermissions.Procurement_Reports)]
        public ActionResult ConsolidatedInventoryReport()
        {
            List<ConsolidatedInventoryReportVM> reportData = new List<ConsolidatedInventoryReportVM>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"Exec [dbo].[Consolidated_Inventory_Report_SSP]");
                reportData = db.Read<ConsolidatedInventoryReportVM>().ToList();
            }
            return View(reportData);
        }
        [ClaimRequirement(UserPermissions.Procurement_Reports)]
        public ActionResult ProductionPlanningReport()
        {
            List<ProductionPlanningReportVM> reportData = new List<ProductionPlanningReportVM>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"Exec [dbo].[Production_Planning_Report_SSP]");
                reportData = db.Read<ProductionPlanningReportVM>().ToList();
            }
            return View(reportData);
        }

        [ClaimRequirement(UserPermissions.Procurement_Reports)]
        public IActionResult MaterialDetailsPopup(int rawMaterialDbkey)
        {
            try
            {
                var viewModel = new MaterialDetailsPopupVM();

                using (var connection = mPDapperContext.CreateConnection())
                
                {
                    var result = connection.QueryMultiple(
                    "[dbo].[Production_Planning_Material_Details_SSP]",
                     new { Raw_material_Dbkey = rawMaterialDbkey },
                    commandType: CommandType.StoredProcedure
                       );

                    viewModel.Demands = result.Read<DemandDetail>().ToList();
                    viewModel.Issues = result.Read<IssueDetail>().ToList();

                    

                    var materialName = _dbContext.Master_Rawmaterials
                        .Where(x => x.Raw_material_Dbkey == rawMaterialDbkey)
                        .Select(x => x.Raw_material_Name)
                        .FirstOrDefault();

                    viewModel.MaterialName = materialName ?? "Unknown Material";
                    viewModel.RawMaterialDbkey = rawMaterialDbkey;

                    viewModel.Summary = new MaterialSummary
                    {
                        TotalDemands = viewModel.Demands.Count,
                        TotalOrdered = viewModel.Demands.Sum(x => x.OrderedQty),
                        TotalReceived = viewModel.Demands.Sum(x => x.ReceivedQty),
                        TotalBalance = viewModel.Demands.Sum(x => x.Balance),

                        TotalIssues = viewModel.Issues.Count,
                        TotalIssued = viewModel.Issues.Sum(x => x.IssueQty),

                        CurrentStock = viewModel.Demands.Sum(x => x.ReceivedQty) - viewModel.Issues.Sum(x => x.IssueQty)
                    };
                }

                return PartialView(viewModel);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>Error: {ex.Message}</div>");
            }
        }

        // ReportController.cs  (ADD THIS METHOD INSIDE ReportController CLASS)
        [HttpGet]
        [ClaimRequirement(UserPermissions.Procurement_Reports)]
        public IActionResult GetRawMaterialVendorSplits(int rawMaterialDbkey)
        {
            using (var connection = mPDapperContext.CreateConnection())
            {
                var data = connection.Query(@"
            SELECT 
                v.Vendor_Name AS VendorName,
                ISNULL(s.Qty, 0) AS Qty
            FROM dbo.RM_Qty_Vendor_Split s
            INNER JOIN dbo.Vendors v ON v.Vendor_Dbkey = s.Vendor_Dbkey
            WHERE s.Raw_material_Dbkey = @rawMaterialDbkey
            ORDER BY v.Vendor_Name
        ", new { rawMaterialDbkey }).ToList();

                return Json(data);
            }
        }


        // ADD THIS — place after GetRawMaterialVendorSplits method
        [HttpGet]
        [ClaimRequirement(UserPermissions.Procurement_Reports)]
        public IActionResult GetRawMaterialDemandVendorDetails(int rawMaterialDbkey)
        {
            try
            {
                var result = new RawMaterialDemandVendorResultVM();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var multiResult = connection.QueryMultiple(
                        "[dbo].[GetRawMaterialDemandVendorDetails_SSP]",
                        new { Raw_material_Dbkey = rawMaterialDbkey },
                        commandType: System.Data.CommandType.StoredProcedure
                    );

                    result.VendorSummary = multiResult.Read<RawMaterialVendorSummaryVM>().ToList();
                    result.DemandDetails = multiResult.Read<RawMaterialDemandDetailVM>().ToList();
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }


    }
}
