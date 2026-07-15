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
using System.Web.Helpers;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics.Metrics;
using System.Linq;
using NUglify.JavaScript;
using System.Net.Mail;
using System.Text;
using OfficeOpenXml.Table.PivotTable;
using DocumentFormat.OpenXml.Office2010.Excel;
using static Microsoft.ML.Data.DataDebuggerPreview;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;



namespace MPCRS.Controllers
{
    [Authorize]
    public class DemandManagementController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;
        public DemandManagementController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

        #region DemandDetails
        public IActionResult DemandDetails(int Id)
        {
            return View();
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.Demand_Write)]
        public IActionResult CreateDemand(int Id, string Viewtype = "Readonly")
        {
            ViewBag.FormView = Viewtype;
            Procurement_Demands_VM vm = new();
            using (_dbContext)
            {
                if (Id != 0)
                {
                    try
                    {
                        Procurement_Demand prodemand = _dbContext.Procurement_Demands.Where(x => x.DemandDbKey == Id).FirstOrDefault();
                        if (prodemand != null)
                        {
                            var Json = JsonConvert.SerializeObject(prodemand);
                            vm = JsonConvert.DeserializeObject<Procurement_Demands_VM>(Json);
                            //if(vm.DO_Review == null)
                            //{
                            //    vm.DO_Review = 0;
                            //}
                            return PartialView(vm);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.LogException(ex);
                    }
                }
            }
            return View(vm);
        }
        [HttpPost]
        [ClaimRequirement(UserPermissions.Demand_Write)]
        public IActionResult CreateDemand(Procurement_Demands_VM vm)
        {
            Procurement_Demand dbModel = new();
            using (_dbContext)
            {
                try
                {
                    dbModel = JsonConvert.DeserializeObject<Procurement_Demand>(JsonConvert.SerializeObject(vm));
                    dbModel.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    dbModel.Updated_On = DateTime.Now;
                    if (vm.DemandDbKey != 0)
                    {
                        _dbContext.Entry(dbModel).State = EntityState.Modified;
                    }
                    else
                    {
                        dbModel.Project_Head = _dbContext.Projects.Where(x => x.Project_Dbkey == vm.Project_Dbkey).Select(x => x.Project_Number).FirstOrDefault();
                        _dbContext.Add(dbModel);
                    }
                    _dbContext.SaveChanges();

                    // Update demand time line
                    Procurement_Demands_History demands_History = _dbContext.Procurement_Demands_Histories.Where(x => x.DemandDbKey == dbModel.DemandDbKey && x.ActionStatus == dbModel.CurrentStatus && x.Do_Review == dbModel.DO_Review).FirstOrDefault();

                    if (demands_History == null)
                    {
                        demands_History = new Procurement_Demands_History();
                    }

                    demands_History.DemandDbKey = dbModel.DemandDbKey;
                    demands_History.ActionDate = dbModel.StatusDate;
                    demands_History.ActionStatus = dbModel.CurrentStatus;
                    demands_History.Do_Review = dbModel.DO_Review;
                    demands_History.Remarks = dbModel.Remarks;
                    demands_History.Updated_By = dbModel.Updated_By;
                    demands_History.Updated_On = dbModel.Updated_On;
                    if (demands_History.Demand_Procurement_History_Key == 0)
                    {
                        _dbContext.Procurement_Demands_Histories.Add(demands_History);
                    }
                    else
                    {
                        _dbContext.Entry(demands_History).State = EntityState.Modified;
                    }
                    _dbContext.SaveChanges();
                    return Json(new { success = true, msg = "Saved successfully", demandid = demands_History.DemandDbKey });
                }
                catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
            }
        }


        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Read)]
        public IActionResult ViewDemands(int Id)
        {
            Procurement_Demands_VM vm = new();
            using (_dbContext)
            {
                try
                {
                    if (Id != 0)
                    {
                        Procurement_Demand prodemand = _dbContext.Procurement_Demands.Where(x => x.DemandDbKey == Id).FirstOrDefault();
                        if (prodemand != null)
                        {
                            var Json = JsonConvert.SerializeObject(prodemand);
                            vm = JsonConvert.DeserializeObject<Procurement_Demands_VM>(Json);
                            MPCRS.Models.Project project = _dbContext.Projects.Where(x => x.Project_Dbkey == vm.Project_Dbkey).FirstOrDefault();
                            User DemandingOfficer = _dbContext.Users.Where(x => x.UserDbkey == vm.DemandingOfficer).FirstOrDefault();
                            Vendor vendor = _dbContext.Vendors.Where(x => x.Vendor_Dbkey == vm.Vendor_Dbkey).FirstOrDefault();
                            if (project != null)
                            {
                                ViewBag.ProjectName = project.Title;
                            }
                            if (DemandingOfficer != null)
                            {
                                ViewBag.DemandingOfficer = DemandingOfficer.UserName;
                            }
                            if (vendor != null)
                            {
                                ViewBag.vendor = vendor.Vendor_Name;
                            }
                            return PartialView(vm);
                        }
                    }
                }
                catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
            }
            return View(vm);
        }
        #endregion

        [HttpGet]
        [ClaimRequirement(UserPermissions.Demand_Read)]
        public IActionResult DemandTree(int id = 0)
        {
            ViewBag.DemandID = id;
            return View(id);
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.Demand_Read)]
        public IActionResult DemandStatusBadge(int id = 0)
        {
            Procurement_Demands_VM DemandData = GetDemand(id, "", 0, "");
            List<Procurement_Demands_History> History = GetDemandHistory(id);
            ProcurementStatusIndicator HisData = new ProcurementStatusIndicator();
            HisData.DemandData = DemandData;
            HisData.DemandHistory = History;
            return View(HisData);
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.Demand_Read)]
        public IActionResult DemandReceiptDocuments(int id = 0)
        {
            ViewBag.demandKey = id;
            List<Procurement_Demad_ReceiptDocumentsVM> docInfo = new List<Procurement_Demad_ReceiptDocumentsVM>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var docData = connection.QueryMultiple($"[dbo].GetDemandReceiptDocuments @demandDbKey = {id}");
                docInfo = docData.Read<Procurement_Demad_ReceiptDocumentsVM>().ToList();
            }
            return View(docInfo);
        }


        [Authorize]
        [HttpGet]
        [ClaimRequirement(UserPermissions.Demand_Write)]
        public ActionResult EditDemandHistory(int id = 0)
        {
            Procurement_Demands_History History = _dbContext.Procurement_Demands_Histories.Where(x => x.Demand_Procurement_History_Key == id).FirstOrDefault();
            Procurement_Demand_History_VM procurement_Demand_History_VM = JsonConvert.DeserializeObject<Procurement_Demand_History_VM>(JsonConvert.SerializeObject(History));
            return View(procurement_Demand_History_VM);

        }

        [Authorize]
        [HttpPost]
        [ClaimRequirement(UserPermissions.Demand_Write)]
        public ActionResult EditDemandHistory(Procurement_Demands_History model)
        {
            try
            {
                Procurement_Demands_History History = _dbContext.Procurement_Demands_Histories.Where(x => x.Demand_Procurement_History_Key == model.Demand_Procurement_History_Key).FirstOrDefault();
                History.Remarks = model.Remarks;
                History.ActionStatus = model.ActionStatus;
                History.ActionDate = model.ActionDate;
                History.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                History.Do_Review = model.Do_Review;
                History.Updated_On = DateTime.Now;
                _dbContext.Entry(History).State = EntityState.Modified;
                _dbContext.SaveChanges();
                return Json(new { success = true, demandID = History.DemandDbKey });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.Demand_Read)]
        public IActionResult DemandStatusTable(int id = 0)
        {
            Procurement_Demands_VM DemandData = GetDemand(id, "", 0, "");
            List<Procurement_Demands_History> History = GetDemandHistory(id);
            ProcurementStatusIndicator HisData = new ProcurementStatusIndicator();
            HisData.DemandData = DemandData;
            HisData.DemandHistory = History;
            return View(HisData);
        }

        public Procurement_Demands_VM GetDemand(int id, string prhead, int prjid, string itemtype)
        {
            try
            {
                Procurement_Demands_VM demandVM = new Procurement_Demands_VM();
                if (id == 0)
                {
                    demandVM.Project_Head = prhead;
                    demandVM.Project_Dbkey = prjid;
                    demandVM.Item_Type = itemtype;
                    return demandVM;
                }
                else
                {
                    Procurement_Demand Demand = _dbContext.Procurement_Demands.Where(x => x.DemandDbKey == id).FirstOrDefault();
                    demandVM = JsonConvert.DeserializeObject<Procurement_Demands_VM>(JsonConvert.SerializeObject(Demand));
                    return demandVM;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<Procurement_Demands_History> GetDemandHistory(int DemandId)
        {
            try
            {
                List<Procurement_Demands_History> DemandHistory = new List<Procurement_Demands_History>();
                if (DemandId == 0)
                {
                    return DemandHistory;
                }
                else
                {
                    DemandHistory = _dbContext.Procurement_Demands_Histories.Where(x => x.DemandDbKey == DemandId).OrderByDescending(x => x.ActionDate).ThenByDescending(x => x.Demand_Procurement_History_Key).ToList<Procurement_Demands_History>();
                    return DemandHistory;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public JsonResult GetDemandJsTreeData()
        {
            List<DemandJsTreeViewModel> mplJsTrees = GetDemandJstreeList();
            return Json(mplJsTrees);
        }

        private List<DemandJsTreeViewModel> GetDemandJstreeList()
        {
            List<DemandJsTreeViewModel> demandJsTreeViewModels = new List<DemandJsTreeViewModel>();
            List<DemandTreeViewModel> demandTreeViewModels = new List<DemandTreeViewModel>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var demandtree = connection.QueryMultiple($"[dbo].[DemandDetailsTree]");
                demandTreeViewModels = demandtree.Read<DemandTreeViewModel>().ToList();
            }
            demandJsTreeViewModels = ContructDemandJsTreeModel(demandTreeViewModels);
            return demandJsTreeViewModels;
        }

        private List<DemandJsTreeViewModel> ContructDemandJsTreeModel(List<DemandTreeViewModel> demandTreeViewModels)
        {
            List<DemandJsTreeViewModel> demandJsTreeViewModels = new List<DemandJsTreeViewModel>();
            DemandJsTreeViewModel myArray = new DemandJsTreeViewModel();
            myArray.id = "0" + "_" + "0";
            myArray.text = "STFE-DEMAND's";
            myArray.icon = "fa fa-fighter-jet";
            myArray.state = GetStates("Project");
            List<DemandNodeCategory> flatObjects = new List<DemandNodeCategory>();
            foreach (DemandTreeViewModel enginePartsViewModel in demandTreeViewModels)
            {
                DemandNodeCategory category = new DemandNodeCategory();
                category.id = enginePartsViewModel.id;
                category.text = enginePartsViewModel.RecordType + "-" + enginePartsViewModel.Nodetext;
                category.isactive = 1;
                category.Parent_id = enginePartsViewModel.Parent_id;
                flatObjects.Add(category);
            }

            myArray.children = FillRecursive(flatObjects, "0");
            demandJsTreeViewModels.Add(myArray);
            return demandJsTreeViewModels;

        }

        private List<DemandJsTreeViewModel> FillRecursive(List<DemandNodeCategory> flatObjects, string parentId = "0", string id = "0")
        {
            var childrenFlatItems = flatObjects.Where(i => i.Parent_id == parentId);
            return childrenFlatItems.Select(i => new DemandJsTreeViewModel
            {
                text = i.text,
                id = i.id.ToString(),
                icon = GetIcons(i.text.Split("-")[0]),
                state = GetStates(i.text.Split("-")[0]),
                a_attr = Getattr(i.text.Split("-")[0]),
                children = FillRecursive(flatObjects, i.id, id),
            }).ToList();
            throw new NotImplementedException();
        }


        private static string GetIcons(string itemtype)
        {
            string icon = "";
            if (itemtype == "Project")
            {
                icon = "fa fa-cogs small-icon";
            }
            else if (itemtype == "Demand")
            {
                icon = "fa fa-pie-chart small-icon";
            }
            else if (itemtype == "DemandItems")
            {
                icon = "fa fa-list small-icon";
            }
            else if (itemtype == "DemandReceipts")
            {
                icon = "fa fa-indent small-icon";
            }
            else if (itemtype == "Split")
            {
                icon = "fa fa-arrow-right small-icon";
            }
            else
            {
                icon = "fa fa-cogs";
            }
            return icon;

        }

        private static A_attr_DemandNode Getattr(string itemtype)
        {
            A_attr_DemandNode a_Attr = new A_attr_DemandNode();
            try
            {
                if (itemtype == "Project")
                {
                    a_Attr.Class = "jstree-anchor jstree-anchor-project";
                }
                else if (itemtype == "Demand")
                {
                    a_Attr.Class = "jstree-anchor jstree-anchor-demand";
                }
                else if (itemtype == "DemandItems")
                {
                    a_Attr.Class = "jstree-anchor jstree-anchor-demanditems";
                }
                else if (itemtype == "DemandReceipts")
                {
                    a_Attr.Class = "jstree-anchor jstree-anchor-demandreceipts";
                }
                else if (itemtype == "Item")
                {
                    a_Attr.Class = "jstree-anchor jstree-anchor-demandItems";
                }
                else if (itemtype == "Receipt")
                {
                    a_Attr.Class = "jstree-anchor jstree-anchor-demandreceiptItems";
                }
                else if (itemtype == "Split")
                {
                    a_Attr.Class = "jstree-anchor jstree-anchor-split";
                }
                else
                {
                    a_Attr.Class = "jstree-anchor jstree-anchor-otherItems";
                }
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); }
            return a_Attr;
        }

        private static DemandNodeState GetStates(string itemtype)
        {
            DemandNodeState state = new DemandNodeState();
            try
            {
                if (itemtype == "Project")
                {
                    state.opened = true;
                }
                else if (itemtype == "Demand")
                {
                    state.opened = false;
                }
                else
                {
                    state.opened = false;
                }

                state.disabled = false;
                state.selected = false;
                state.Checked = false;

            }
            catch (Exception ex) { ErrorHandler.LogException(ex); }
            return state;
        }

        #region DemandItems

        [ClaimRequirement(UserPermissions.Demand_Write)]
        public ActionResult DemandItems(int Id, string Viewtype = "Readonly")
        {
            ViewBag.FormView = Viewtype;
            ProcurementDemandInfo procurementDemandInfo = new ProcurementDemandInfo();
            using (_dbContext)
            {
                try
                {
                    Procurement_Demand procurement_Demand = _dbContext.Procurement_Demands.Where(x => x.DemandDbKey == Id).FirstOrDefault();
                    var Json = JsonConvert.SerializeObject(procurement_Demand);
                    procurementDemandInfo.DemandData = JsonConvert.DeserializeObject<Procurement_Demands_VM>(Json);

                    List<Procurement_Demand_Item> procurement_Demand_Items = _dbContext.Procurement_Demand_Items.Where(x => x.DemandDbKey == Id).ToList();
                    var jsonItems = JsonConvert.SerializeObject(procurement_Demand_Items);
                    procurementDemandInfo.procurement_Demand_Items_VMs = JsonConvert.DeserializeObject<List<Procurement_Demand_Items_VM>>(jsonItems);

                    List<Master_Rawmaterial> master_Rawmaterials = _dbContext.Master_Rawmaterials.ToList();
                    foreach (var item in procurementDemandInfo.procurement_Demand_Items_VMs)
                    {
                        if (item.ItemDbKey != null)
                        {
                            item.Thickness_list = Masters.GetRawMaterial_ParameterList(item.ItemDbKey, "Thickness", master_Rawmaterials);
                            item.Outer_Dia_mm_list = Masters.GetRawMaterial_ParameterList(item.ItemDbKey, "Outer_Dia", master_Rawmaterials);
                        }
                    }
                }
                catch (Exception ex) { ErrorHandler.LogException(ex); }
            }
            return PartialView(procurementDemandInfo);
        }


        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Delete)]
        public ActionResult DeleteDemandItem(int id = 0)
        {
            int RecieptCount = int.Parse(MPGlobals.GetOnedata("SELECT count([Receipt_dbkey]) FROM [dbo].[Procurement_Demand_Receipts] where [DemandItemKey] =" + id + " "));
            if (RecieptCount == 0)
            {
                // Need introduce Inactive coolumn to demand items table ;
                MPGlobals.ExceSQLNonQuery($"Delete FROM [dbo].[Procurement_Demand_Items] where [DemandItemKey] ={id} ");
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }

        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Write)]
        public ActionResult SaveDemandItems()
        {
            using (_dbContext)
            {
                try
                {
                    var demanditems = Request.Form["demanditems"];
                    List<Procurement_Demand_Item> procurement_Demand_Item = JsonConvert.DeserializeObject<List<Procurement_Demand_Item>>(demanditems);

                    //    ErrorHandler.LogMesaage("Before Entered loop");
                    //    ErrorHandler.LogMesaage(JsonConvert.SerializeObject(procurement_Demand_Item));
                    foreach (var item in procurement_Demand_Item)
                    {
                        if (item.DemandItemKey == 0)
                        {
                            item.Updated_On = DateTime.Now;
                            item.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                            _dbContext.Procurement_Demand_Items.Add(item);
                            //            ErrorHandler.LogMesaage("Create _dbContext.Procurement_Demand_Items.Add(item);");
                            //            ErrorHandler.LogMesaage(JsonConvert.SerializeObject(item));
                        }
                        else
                        {
                            item.Updated_On = DateTime.Now;
                            item.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                            _dbContext.Entry(item).State = EntityState.Modified;
                            //           ErrorHandler.LogMesaage("Edit _dbContext.Procurement_Demand_Items.Add(item);");
                            //           ErrorHandler.LogMesaage(JsonConvert.SerializeObject(item));
                        }
                    }
                    //    ErrorHandler.LogMesaage("Before dbContext.SaveChanges();");
                    _dbContext.SaveChanges();
                    //    ErrorHandler.LogMesaage("After dbContext.SaveChanges();");
                }
                catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

            }
            return Json(new { success = true });
        }

        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Read)]
        public IActionResult ViewDemandItems(int Id, string Viewtype = "Readonly")
        
        {
            ProcurementDemandInfo procurementDemandInfo = new ProcurementDemandInfo();
            //Procurement_Demands_VM procurement_Demands_VM = new Procurement_Demands_VM();
            ViewBag.ViewType = Viewtype;
            ViewBag.HasManyOrderNumbers = false;
            using (_dbContext)
            {
                try
                {
                    Procurement_Demand procurement_Demand = _dbContext.Procurement_Demands.Where(x => x.DemandDbKey == Id).FirstOrDefault();
                    var Json = JsonConvert.SerializeObject(procurement_Demand);
                    procurementDemandInfo.DemandData = JsonConvert.DeserializeObject<Procurement_Demands_VM>(Json);

                    if (procurementDemandInfo.DemandData.OrderNumbers != null)
                    {
                        if (procurementDemandInfo.DemandData.OrderNumbers.Split(",").Length > 1)
                        {
                            ViewBag.HasManyOrderNumbers = true;
                        }
                    }

                    string cmdstr = $"EXEC [dbo].[GetDemandItem_SSP] @DemandDbKey = '{procurementDemandInfo.DemandData.DemandDbKey}' ";
                    DataTable dataTable = MPGlobals.GetDataForDatalist(cmdstr);
                    var jsonData = JsonConvert.SerializeObject(dataTable);
                    procurementDemandInfo.procurement_Demand_Items_VMs = JsonConvert.DeserializeObject<List<Procurement_Demand_Items_VM>>(jsonData);
                    procurementDemandInfo.procurement_Demand_Receipts = _dbContext.Procurement_Demand_Receipts.Where(x => x.DemandDbKey == Id).ToList();
                }
                catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
            }
            return PartialView(procurementDemandInfo);
        }

        [Authorize]
        [HttpPost]
        [ClaimRequirement(UserPermissions.Demand_Write_Receipts)]
        public IActionResult SaveDemandItemsReceipt([FromBody] IEnumerable<Procurement_Demand_Receipt_VM> procurement_Demand_Receipts)
        {
            ProcurementDemandInfo procurementDemandInfo = new ProcurementDemandInfo();
            using (_dbContext)
            {
                try
                {
                    int ReceiptIndexNumber = procurement_Demand_Receipts.Select(x => x.Index_No).FirstOrDefault();

                    foreach (var item in procurement_Demand_Receipts)
                    {
                       
                        Procurement_Demand_Receipt procurement_Demand_Receipt = new Procurement_Demand_Receipt();
                        if (item.Receiving_inventory < 0)
                        {
                            item.Receiving_inventory = 0;
                        }
                        procurement_Demand_Receipt.Receipt_dbkey = item.Receipt_dbkey ?? 0;
                        procurement_Demand_Receipt.Receipt_Date = item.Receipt_Date ?? DateTime.Now;
                        procurement_Demand_Receipt.Receipt_No = item.Receipt_No;
                        procurement_Demand_Receipt.DemandDbKey = item.DemandDbKey ?? 0;
                        procurement_Demand_Receipt.DemandItemKey = item.DemandItemKey ?? 0;
                        procurement_Demand_Receipt.Receiving_inventory = item.Receiving_inventory ?? 0;
                        procurement_Demand_Receipt.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                        procurement_Demand_Receipt.Updated_on = DateTime.Now;
                        procurement_Demand_Receipt.Index_No = item.Index_No;

                        if(procurement_Demand_Receipt.Receiving_inventory == 0 &&  procurement_Demand_Receipt.Receipt_dbkey != 0 )
                        {
                            _dbContext.Procurement_Demand_Receipts.Remove(procurement_Demand_Receipt);
                        }
                        else if (procurement_Demand_Receipt.Receipt_dbkey != 0)
                        {
                            _dbContext.Procurement_Demand_Receipts.Entry(procurement_Demand_Receipt).State = EntityState.Modified;
                        }
                        else if (procurement_Demand_Receipt.Receipt_dbkey == 0 && procurement_Demand_Receipt.Index_No == 0)
                        {
                            if(procurement_Demand_Receipt.Receiving_inventory == 0)
                            {
                                continue;
                            }
                            procurement_Demand_Receipt.Index_No = GetDemandReceiptIndexNumber(procurement_Demand_Receipt);
                            _dbContext.Add(procurement_Demand_Receipt);
                        }
                        else
                        {
                            //procurement_Demand_Receipt.Index_No = GetDemandReceiptIndexNumber(procurement_Demand_Receipt);
                            _dbContext.Add(procurement_Demand_Receipt);
                        }
                    }
                    _dbContext.SaveChanges();
                    return Json(new { success = true, msg = "Successfully Saved" });
                }
                catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
            }
        }

        private int GetDemandReceiptIndexNumber(Procurement_Demand_Receipt procurement_Demand_Receipt)
        {

            int index = 0;
            DataTable dataTable = MPGlobals.GetDataForDatalist("SELECT TOP (1) isnull(Max([Index_No]),0) FROM  [dbo].[Procurement_Demand_Receipts]where [DemandDbKey] = " + procurement_Demand_Receipt.DemandDbKey + " ");
            index = int.Parse(dataTable.Rows[0][0].ToString());
            if (dataTable.Rows.Count == 0)
            {
                index = 1;
            }
            else
            {
                index = int.Parse(dataTable.Rows[0][0].ToString()) + 1;
            }
            return index;
        }

        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Read)]
        public IActionResult DemandReceiptsDocs(int Receipt_dbkey = 0, int readmode = 0)    // used to show attachments and splits
        {
            ViewBag.Receipt_dbkey = Receipt_dbkey;
            ViewBag.readmode = readmode;
            return PartialView();
        }


        [Authorize]
        [HttpGet]
        [ClaimRequirement(UserPermissions.Demand_Read)]
        public IActionResult ReceiptItemSplits(int id)
        {
            List<ProcurementReceiptItemSplitViewModel> receiptItemSplits = new List<ProcurementReceiptItemSplitViewModel>();
            DataTable dt = MPGlobals.GetDataForDatalist($"[dbo].[GetProcurement_ReceiptItemSplit] @Receipt_dbkey ={id}");
            receiptItemSplits = MPGlobals.ConvertDataTable<ProcurementReceiptItemSplitViewModel>(dt);
            var reciptSplitSum = _dbContext.Procurement_ReceiptItemSplits.Where(x => x.Receipt_dbkey == id).Sum(x => x.Weight);
            ViewBag.ReceiptSplitWeight = reciptSplitSum;
            ViewBag.Receipt_dbkey = id;
            return PartialView(receiptItemSplits);
        }


        private string GetDestinationFolder(string SourceFileType, string Attachment_type)
        {
            string directoryname = @"/Attachments/Deamands_Receipt_Docs/";
            string SaveDirectory = string.Empty;
            SaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/" + directoryname);
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
            return SaveDirectory + "/";
        }

        [Authorize]
        [HttpGet]
        [ClaimRequirement(UserPermissions.Demand_Read)]
        public IActionResult ReceiptItemSplitsModel(int Receipt_dbkey, int splitKey)
        {
            List<ProcurementReceiptItemSplitViewModel> receiptItemSplits = new List<ProcurementReceiptItemSplitViewModel>();
            DataTable dt = MPGlobals.GetDataForDatalist($"[dbo].[GetProcurement_ReceiptItemSplit] @Receipt_dbkey ={Receipt_dbkey}, @receiptItemKey = {splitKey}");
            receiptItemSplits = MPGlobals.ConvertDataTable<ProcurementReceiptItemSplitViewModel>(dt);

            ViewBag.Receipt_dbkey = Receipt_dbkey;
            List<Models.Attachment> attachments = new List<Models.Attachment>();

            if (receiptItemSplits.Count > 0 && receiptItemSplits.FirstOrDefault().Attachment_Db_Key != null && splitKey != -1)
            {
                attachments = _dbContext.Attachments.Where(x => x.Source_table == "Procurement_Demand_Receipts" && x.Source_table_key == Receipt_dbkey).ToList();
                var attachmentKeys = receiptItemSplits.FirstOrDefault().Attachment_Db_Key.Split(",").ToList();
                List<int> intList = attachmentKeys.Select(s => int.Parse(s)).ToList();
                attachments = attachments.Where(x => intList.Contains(x.Attachment_Db_Key)).ToList();
            }

            if (splitKey == -1)
            {
                ProcurementReceiptItemSplitViewModel procurementReceiptItemSplitViewModel_ = receiptItemSplits.FirstOrDefault();
                procurementReceiptItemSplitViewModel_.SplitId = 0;
                procurementReceiptItemSplitViewModel_.Measurement = 0;
                procurementReceiptItemSplitViewModel_.Weight = 0;
                procurementReceiptItemSplitViewModel_.Batch_No = "";
                procurementReceiptItemSplitViewModel_.Material_Reference_No = "";
                procurementReceiptItemSplitViewModel_.Heat_No = "";
                procurementReceiptItemSplitViewModel_.Attachment_Db_Key = "";
                procurementReceiptItemSplitViewModel_.Measurement_breadth = 0;

                receiptItemSplits = new List<ProcurementReceiptItemSplitViewModel>();
                receiptItemSplits.Add(procurementReceiptItemSplitViewModel_);
            }


            var myTuple = new Tuple<List<ProcurementReceiptItemSplitViewModel>, List<Models.Attachment>>(receiptItemSplits, attachments);
            return View(myTuple);
        }



        [Authorize]
        [HttpPost]
        [ClaimRequirement(UserPermissions.Demand_Write_Receipts)]
        public async Task<IActionResult> SaveReceiptItemSplitsModelAsync([FromForm] UploadViewModel model)
        {
            try
            {
                List<AttachmentVM> attach = JsonConvert.DeserializeObject<List<AttachmentVM>>(model.filesData);
                ProcurementReceiptItemSplitViewModel splitData = JsonConvert.DeserializeObject<ProcurementReceiptItemSplitViewModel>(model.JsonData);
                string AttachmentDbKey = "";

                //Hemanth, why did you write as below? this line was erasing all attachments upon edit!
                //if (splitData.Attachment_Db_Key_Data_String != null)
                //{ 
                //    AttachmentDbKey = splitData.Attachment_Db_Key_Data_String;
                //}  

                if (splitData.Attachment_Db_Key != null)
                {
                    AttachmentDbKey = splitData.Attachment_Db_Key;
                }

                int counter = 0;
                foreach (var item in attach)
                {
                    var userguid = User.Identity.Name;
                    string systemfilename = string.Empty;
                    string filename = string.Empty;
                    string SavePath = string.Empty;
                    item.uploadeddocument = model.files[counter];
                    // string SaveDirectory = string.Empty;
                    item.AttachmentGUID = Guid.NewGuid().ToString();
                    if (item.uploadeddocument != null)
                    {
                        Models.Attachment att = new();
                        filename = item.uploadeddocument.FileName;
                        systemfilename = item.AttachmentGUID + Path.GetExtension(item.uploadeddocument.FileName);
                        SavePath = GetDestinationFolder(item.Source_table, item.Attachment_type) + systemfilename;
                        using (var stream = new FileStream(SavePath, FileMode.Create))
                        {
                            item.uploadeddocument.CopyTo(stream);
                        }
                        att.Attachment_FileName = systemfilename;
                        att.Orginal_File_Name = filename;
                        att.Attachment_location = @"/Attachments/Deamands_Receipt_Docs/";
                        att.Attachment_type = item.Attachment_type;
                        att.Source_table_key = splitData.Receipt_dbkey;
                        att.Source_table = item.Source_table;
                        att.Revision = item.Revision;
                        att.File_Revision = item.File_Revision;
                        att.AttachmentGUID = item.AttachmentGUID;
                        att.Attachment_type = item.Attachment_type;
                        att.File_DVD_Num = item.File_DVD_Num;
                        att.Updated_on = DateTime.Now;
                        att.Updated_by = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                        _dbContext.Attachments.Add(att);
                        _dbContext.SaveChanges();
                        if (AttachmentDbKey == "")
                        {
                            AttachmentDbKey = att.Attachment_Db_Key.ToString();
                        }
                        else
                        {
                            AttachmentDbKey = AttachmentDbKey + "," + att.Attachment_Db_Key;
                        }

                    }
                    counter++;
                }

                Procurement_ReceiptItemSplit procurement_ReceiptItemSplit = _dbContext.Procurement_ReceiptItemSplits.Where(x => x.SplitId == splitData.SplitId).FirstOrDefault();

                if (procurement_ReceiptItemSplit == null)
                {
                    procurement_ReceiptItemSplit = new Procurement_ReceiptItemSplit();
                }



                procurement_ReceiptItemSplit.SplitId = splitData.SplitId;
                procurement_ReceiptItemSplit.Receipt_dbkey = splitData.Receipt_dbkey;
                procurement_ReceiptItemSplit.Measurement = splitData.Measurement;
                procurement_ReceiptItemSplit.Revision = splitData.Revision;
                procurement_ReceiptItemSplit.Measurement_breadth = splitData.Measurement_breadth;
                procurement_ReceiptItemSplit.Material_Reference_No = splitData.Material_Reference_No;
                procurement_ReceiptItemSplit.Heat_No = splitData.Heat_No;
                procurement_ReceiptItemSplit.Batch_No = splitData.Batch_No;
                procurement_ReceiptItemSplit.Weight = splitData.Weight;
                procurement_ReceiptItemSplit.UOM = splitData.UOM;
                procurement_ReceiptItemSplit.Attachment_Db_Key = AttachmentDbKey;
                procurement_ReceiptItemSplit.Updated_on = DateTime.Now;
                procurement_ReceiptItemSplit.Updated_by = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                if (procurement_ReceiptItemSplit.SplitId != 0)
                {
                    _dbContext.Procurement_ReceiptItemSplits.Entry(procurement_ReceiptItemSplit).State = EntityState.Modified;
                }
                else
                {
                    _dbContext.Add(procurement_ReceiptItemSplit);
                }

                _dbContext.SaveChanges();
                return Json(new { success = true, msg = "Successfully Saved" });
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

        }


        [Authorize]
        [HttpPost]
        [ClaimRequirement(UserPermissions.Demand_Write_Receipts)]
        public IActionResult SaveReceiptItemSplitsModel_Duplicate()
        {
            try
            {
                var procurement_ReceiptItemSplitForm = Request.Form["Procurement_Demand_Items_Split"];
                //  var AttachFiles = Request.Form[""];
                var UploadedFiles = Request.Form.Files;
                List<ProcurementReceiptItemSplitViewModel> procurement_ReceiptItemSplitVM = JsonConvert.DeserializeObject<List<ProcurementReceiptItemSplitViewModel>>(procurement_ReceiptItemSplitForm);
                int counter = 1;
                foreach (var item in procurement_ReceiptItemSplitVM)
                {
                    //foreach (var item in Attachment)
                    //{

                    //}
                    // Attachment fileSavedInfo = UploadDemandReciptDoc(UploadedFiles, counter, item.Receipt_dbkey, item.SplitId);
                    Procurement_ReceiptItemSplit procurement_ReceiptItemSplit = new Procurement_ReceiptItemSplit();
                    procurement_ReceiptItemSplit.SplitId = item.SplitId;
                    procurement_ReceiptItemSplit.Receipt_dbkey = item.Receipt_dbkey;
                    procurement_ReceiptItemSplit.Measurement = item.Measurement;
                    procurement_ReceiptItemSplit.Measurement_breadth = item.Measurement_breadth;
                    procurement_ReceiptItemSplit.Material_Reference_No = item.Material_Reference_No;
                    procurement_ReceiptItemSplit.Heat_No = item.Heat_No;
                    procurement_ReceiptItemSplit.Batch_No = item.Batch_No;
                    procurement_ReceiptItemSplit.Weight = item.Weight;
                    procurement_ReceiptItemSplit.UOM = item.UOM;
                    if (item.Attachment_Db_Key_Data != null)
                    {
                        var AttachmentDbKey = string.Join(",", item.Attachment_Db_Key_Data);
                        procurement_ReceiptItemSplit.Attachment_Db_Key = AttachmentDbKey;
                    }

                    procurement_ReceiptItemSplit.Updated_on = DateTime.Now;
                    procurement_ReceiptItemSplit.Updated_by = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    if (procurement_ReceiptItemSplit.SplitId != 0)
                    {
                        _dbContext.Procurement_ReceiptItemSplits.Entry(procurement_ReceiptItemSplit).State = EntityState.Modified;
                    }
                    else
                    {
                        _dbContext.Add(procurement_ReceiptItemSplit);
                    }
                }
                _dbContext.SaveChanges();
                return Json(new { success = true, msg = "Successfully Saved" });
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

        }


        [Authorize]
        [HttpPost]
        [ClaimRequirement(UserPermissions.Demand_Write_Receipts)]
        public IActionResult SaveReceiptItemSplits()
        {
            try
            {
                var procurement_ReceiptItemSplitForm = Request.Form["Procurement_Demand_Items_Split"];
                //  var AttachFiles = Request.Form[""];
                var UploadedFiles = Request.Form.Files;
                List<ProcurementReceiptItemSplitViewModel> procurement_ReceiptItemSplitVM = JsonConvert.DeserializeObject<List<ProcurementReceiptItemSplitViewModel>>(procurement_ReceiptItemSplitForm);
                int counter = 1;
                foreach (var item in procurement_ReceiptItemSplitVM)
                {
                    //foreach (var item in Attachment)
                    //{

                    //}
                    // Attachment fileSavedInfo = UploadDemandReciptDoc(UploadedFiles, counter, item.Receipt_dbkey, item.SplitId);
                    Procurement_ReceiptItemSplit procurement_ReceiptItemSplit = new Procurement_ReceiptItemSplit();
                    procurement_ReceiptItemSplit.SplitId = item.SplitId;
                    procurement_ReceiptItemSplit.Receipt_dbkey = item.Receipt_dbkey;
                    procurement_ReceiptItemSplit.Measurement = item.Measurement;
                    procurement_ReceiptItemSplit.Measurement_breadth = item.Measurement_breadth;
                    procurement_ReceiptItemSplit.Material_Reference_No = item.Material_Reference_No;
                    procurement_ReceiptItemSplit.Heat_No = item.Heat_No;
                    procurement_ReceiptItemSplit.Batch_No = item.Batch_No;
                    procurement_ReceiptItemSplit.Weight = item.Weight;
                    procurement_ReceiptItemSplit.UOM = item.UOM;
                    if (item.Attachment_Db_Key_Data != null)
                    {
                        var AttachmentDbKey = string.Join(",", item.Attachment_Db_Key_Data);
                        procurement_ReceiptItemSplit.Attachment_Db_Key = AttachmentDbKey;
                    }

                    procurement_ReceiptItemSplit.Updated_on = DateTime.Now;
                    procurement_ReceiptItemSplit.Updated_by = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    if (procurement_ReceiptItemSplit.SplitId != 0)
                    {
                        _dbContext.Procurement_ReceiptItemSplits.Entry(procurement_ReceiptItemSplit).State = EntityState.Modified;
                    }
                    else
                    {
                        _dbContext.Add(procurement_ReceiptItemSplit);
                    }
                }
                _dbContext.SaveChanges();
                return Json(new { success = true, msg = "Successfully Saved" });
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

        }

        //private Attachment UploadDemandReciptDoc(IFormFileCollection uploadedFiles, int counter, int receipt_dbkey, int splitId)
        //{
        //    throw new NotImplementedException();
        //}

        [HttpGet]
        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Delete)]
        public IActionResult DeleteReceiptItemSplits(int id)
        {
            try
            {
                MPGlobals.ExceSQLNonQuery($"Delete FROM [dbo].[Procurement_ReceiptItemSplit] where [SplitId] ={id} ");
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
            return Json(new { success = true });
        }


        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Read)]
        public IActionResult AdditionalInfo(int Receipt_dbkey = 0, int SplitID = 0)
        {
            List<ProcurementAdditionalInfo> model = new List<ProcurementAdditionalInfo>();
            try
            {
                using (_dbContext)
                {
                    var SplitData = _dbContext.Procurement_ReceiptItemSplits.Where(x => x.SplitId == SplitID).FirstOrDefault();
                    if (SplitData != null)
                    {
                        if (SplitData.AdditionalinfoJson != null)
                        {
                            try
                            {
                                List<ProcurementAdditionalInfo> procurementAdditionalInfos = JsonConvert.DeserializeObject<List<ProcurementAdditionalInfo>>(SplitData.AdditionalinfoJson);
                                model = procurementAdditionalInfos;
                            }
                            catch (Exception)
                            {
                                // to match documents that are coming as int array from V1 entries
                                List<ProcurementAdditionalInfo> procurementAdditionalInfos = new List<ProcurementAdditionalInfo>();
                                List<ProcurementAdditionalInfoTest> procurementAdditionalInfosTest = JsonConvert.DeserializeObject<List<ProcurementAdditionalInfoTest>>(SplitData.AdditionalinfoJson);

                                foreach (var item in procurementAdditionalInfosTest)
                                {
                                    ProcurementAdditionalInfo lineItem = new ProcurementAdditionalInfo();

                                    if (lineItem.documents != null)
                                    {
                                        lineItem.documents = string.Join(",", item.documents);
                                    }

                                    lineItem.Item_Part = item.Item_Part;
                                    lineItem.item_SerialNumber = item.item_SerialNumber;
                                    lineItem.refNos = item.refNos;
                                    lineItem.parentKey = item.parentKey;
                                    lineItem.recordGUID = item.recordGUID;
                                    lineItem.remarks = item.remarks;
                                    lineItem.updatedBy = item.updatedBy;
                                    procurementAdditionalInfos.Add(lineItem);
                                }
                                model = procurementAdditionalInfos;
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                model = new List<ProcurementAdditionalInfo>();
            }
            ViewBag.Receipt_dbkey = Receipt_dbkey;
            ViewBag.parentKey = SplitID;
            return PartialView(model);
        }

        [Authorize]
        [HttpPost]
        [ClaimRequirement(UserPermissions.Demand_Write_Receipts)]
        public IActionResult SaveAdditionalInfo()
        {
            try
            {
                var additionalInfoJson = Request.Form["AdnlInfoData"];
                List<ProcurementAdditionalInfo> additionalInfoItems = JsonConvert.DeserializeObject<List<ProcurementAdditionalInfo>>(additionalInfoJson);
                additionalInfoItems.ForEach(x => x.recordGUID = (x.recordGUID.ToLower() == "new" ? Guid.NewGuid().ToString() : x.recordGUID));
                int SplitID = additionalInfoItems.FirstOrDefault().parentKey;
                using (_dbContext)
                {
                    var SplitData = _dbContext.Procurement_ReceiptItemSplits.Where(x => x.SplitId == SplitID).FirstOrDefault();
                    if (SplitData != null)
                    {
                        SplitData.AdditionalinfoJson = JsonConvert.SerializeObject(additionalInfoItems);
                        _dbContext.Procurement_ReceiptItemSplits.Entry(SplitData).State = EntityState.Modified;
                        _dbContext.SaveChanges();
                    }
                }
                return Json(new { success = true, Msg = "Saved Successfully" });
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
        }



        #endregion



        #region Documents

        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Read)]
        public IActionResult Demand_Documents(int Id = 0)
        {
            try
            {
                Upload_DocumentVM vm = new Upload_DocumentVM();
                using (_dbContext)
                {
                    Procurement_Demand Demands = _dbContext.Procurement_Demands.Where(x => x.DemandDbKey == Id).FirstOrDefault();
                    ViewBag.Demand_No = Demands.Demand_No;
                }
                string cmd = @"SELECT dbo.Master_General.Master_Name, dbo.Demand_Document.Document_Name, dbo.Demand_Document.Document_Location,dbo.Demand_Document.Updated_On as Updated_On, dbo.Demand_Document.Remarks, 
                         dbo.Demand_Document.DemandDbKey, dbo.Demand_Document.DocumentID
                         FROM            dbo.Demand_Document LEFT OUTER JOIN
                         dbo.Master_General ON dbo.Demand_Document.Master_Dbkey = dbo.Master_General.Master_Dbkey
                         WHERE(dbo.Demand_Document.DemandDbKey = '" + Id + "')";
                DataTable dt = MPGlobals.GetDataForDatalist(cmd);
                List<Upload_DocumentVM> DocumentVM = MPGlobals.ConvertDataTable<Upload_DocumentVM>(dt);
                if (DocumentVM.Count != 0)
                {
                    DocumentVM[0].UploadDocumentVM = MPGlobals.ConvertDataTable<Upload_DocumentVM>(dt);
                    return View(DocumentVM[0]);
                }
                else
                {
                    vm.UploadDocumentVM = new List<Upload_DocumentVM>();
                    return View(vm);
                }
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
        }


        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Write_Documents)]
        public IActionResult UploadDemandDocument(int Id = 0)
        {
            Upload_DocumentVM vm = new Upload_DocumentVM();
            vm.DemandDbKey = Id;
            vm.DocumentType = Masters.GetMaster_Demand_DocumentType();
            return View(vm);
        }


        [ClaimRequirement(UserPermissions.Demand_Write_Documents)]
        public IActionResult SaveDemandDocument(Upload_DocumentVM DocumentVM)
        {
            try
            {
                int? keyId;
                Demand_Document document = new Demand_Document();

                if (DocumentVM.DocumentFile != null)
                {
                    IFormFile postedFile = DocumentVM.DocumentFile;
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Attachments", "DemandDOC");
                    string ext = Path.GetExtension(postedFile.FileName);
                    string filename = DocumentVM.DemandDbKey + postedFile.FileName;

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    string filePath = Path.Combine(path, filename);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        postedFile.CopyTo(stream);
                    }

                    document.DocumentID = DocumentVM.DocumentID;
                    document.DemandDbKey = DocumentVM.DemandDbKey;
                    document.Master_Dbkey = DocumentVM.Document_Type;
                    document.Remarks = DocumentVM.Remarks;
                    document.Document_Name = filename;
                    document.Document_Location = "Attachments/DemandDOC/";
                    document.Updated_by = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    document.Updated_On = DateTime.Now;

                    using (var db = _dbContext) // Assuming DESI_STFEEntities is your DbContext
                    {
                        if (document.DocumentID == 0)
                        {
                            db.Demand_Documents.Add(document);
                        }
                        else
                        {
                            db.Entry(document).State = EntityState.Modified;
                        }

                        db.SaveChanges();
                    }
                }

                keyId = document.DemandDbKey;
                return Json(new { success = true, Msg = "Saved Successfully", key = keyId });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message });
            }
        }

        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Delete)]
        public IActionResult DeleteDemandDoc(int id = 0)
        {
            try
            {
                using (_dbContext) // Assuming DESI_STFEEntities is your DbContext
                {
                    var demandDocument = _dbContext.Demand_Documents.FirstOrDefault(x => x.DocumentID == id);
                    if (demandDocument != null)
                    {
                        _dbContext.Demand_Documents.Remove(demandDocument);
                        _dbContext.SaveChanges();
                        return Json(new { success = true, message = "Removed Successfully" });
                    }
                }
            }
            catch (Exception ex)
            {

                ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message });
            }
            return Json(new { success = false, message = "Document not found" });
        }

        public IActionResult Downloadfile(int Id)
        {
            try
            {
                using (var db = _dbContext)
                {
                    var demandDocument = db.Demand_Documents.FirstOrDefault(x => x.DocumentID == Id);
                    if (demandDocument != null)
                    {
                        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Attachments", "DemandDOC");
                        string docName = demandDocument.Document_Name.ToLower().Contains(".pdf.pdf") ? demandDocument.Document_Name.Replace(".pdf.pdf", ".pdf") : demandDocument.Document_Name;
                        string filePath = Path.Combine(uploadPath, docName);
                        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                        return File(fileBytes, "application/octet-stream", docName);
                        //if (System.IO.File.Exists(filePath))
                        //{
                        //    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(demandDocument.Document_Name);
                        //    string fileExtension = Path.GetExtension(demandDocument.Document_Name);
                        //    string correctedFileName = fileNameWithoutExtension;
                        //    if (fileNameWithoutExtension.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                        //    {
                        //        correctedFileName = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.Length - fileExtension.Length);
                        //    }
                        //    string correctedFilePath = Path.Combine(uploadPath, correctedFileName + fileExtension);
                        //    byte[] fileBytes = System.IO.File.ReadAllBytes(correctedFilePath);
                        //    return File(fileBytes, "application/octet-stream", correctedFileName + fileExtension);
                        //}

                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                throw;
            }
            return NotFound(); // Return appropriate result if document not found
        }
        #endregion

        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Delete)]
        public IActionResult DeleteDemand(int id)
        {
            return Json(new { success = true, message = "Removed Successfully" });
        }


        #region Dashboard and Milestone
        [Authorize]
        public IActionResult Dashboard(int ProjectDbkey, int DemandingofficerDbKey = 0)
        {
            ViewBag.DemandingOfficerDbkey = DemandingofficerDbKey;
            using (_dbContext)
            {
                try
                {
                    Project project = new Project();
                    if (ProjectDbkey != 0)
                    {
                        project = _dbContext.Projects.Where(x => x.Project_Dbkey == ProjectDbkey).FirstOrDefault();
                    }
                    else
                    {
                        project = new Project();
                        project.Project_Number = "-";
                        project.Display_title = "All";
                        project.Project_Dbkey = 0;
                    }

                    return PartialView(project);
                }
                catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
            }
        }

        [HttpGet]
        public ActionResult GetDemandNumbersByStatus(string id = "", int prjtID = 0, int DemandingOfficerDbkey = 0)
        {
            try
            {
                DataTable dataTable = MPGlobals.GetDataForDatalist(@"Select SummaryData.* from 
            (SELECT Project_Dbkey,DemandDbKey, Demand_No,Item_Description, DO_Review,[CurrentStatus],DemandingOfficer, CASE WHEN [DO_Review] <> 0 THEN[CurrentStatus] + '(DO Review)' ELSE[CurrentStatus] END as DemandStatus
            FROM[dbo].[Procurement_Demands]  where ISNULL(IsActive,1)=1)
            SummaryData where SummaryData.Project_Dbkey = (case when " + prjtID + " = 0 then SummaryData.Project_Dbkey else " + prjtID + " end)  AND SummaryData.DemandStatus = '" + id + "' AND " +
            "SummaryData.DemandingOfficer = ( case when " + DemandingOfficerDbkey + "= 0 then SummaryData.DemandingOfficer else " + DemandingOfficerDbkey + " end) ");
                return PartialView(dataTable);
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
        }

        //[ClaimRequirement(UserPermissions.Demand_Write)]
        //public ActionResult ProcurementMilestone(int Id)
        //{
        //    ProcurementMilestoneVM procurementmilestoneVM = new ProcurementMilestoneVM();
        //    procurementmilestoneVM.ProcurementMilestoneVMs = new List<ProcurementMilestoneVM>();
        //    using (_dbContext)
        //    {
        //        try
        //        {

        //            List<ProcurementMilestone> procurement_ms = _dbContext.ProcurementMilestones.Where(x => x.DemandDbKey == Id).ToList();
        //            var jsonItems = JsonConvert.SerializeObject(procurement_ms);
        //            procurementmilestoneVM.ProcurementMilestoneVMs = JsonConvert.DeserializeObject<List<ProcurementMilestoneVM>>(jsonItems);
        //        }
        //        catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
        //    }
        //    return PartialView(procurementmilestoneVM);
        //}

        [ClaimRequirement(UserPermissions.Demand_Write)]
        public ActionResult ProcurementMilestone(int Id)
        {
            ProcurementMilestoneVM procurementmilestoneVM = new ProcurementMilestoneVM();
            procurementmilestoneVM.ProcurementMilestoneVMs = new List<ProcurementMilestoneVM>();
            
                try
                {
                    List<Procurement_Milestone> procurement_ms = _dbContext.Procurement_Milestones
                        .Where(x => x.DemandDbKey == Id && x.Status != "Merged" && x.Status != "Cancelled")
                        .ToList();

                    // Map new model to existing VM
                    procurementmilestoneVM.ProcurementMilestoneVMs = procurement_ms.Select(x => new ProcurementMilestoneVM
                    {
                        MilestoneID = x.MilestoneID,
                        DemandDbKey = x.DemandDbKey,
                        MilestoneName = x.MilestoneName,
                        Components = null,          // No longer used in new schema
                        Description = null,         // No longer used in new schema
                        DueDate = x.CurrentDueDate,
                        CompletionDate = x.CompletionDate,
                        Status = x.Status,
                        Comments = x.Comments,
                        UpdatedBy = x.UpdatedBy,
                        UpdatedOn = x.UpdatedOn,
                        QtyPercentage = x.QtyPercentage,
                        IsLastMilestone = x.IsLastMilestone
                    }).ToList();
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                    return Json(new { success = false, msg = ex.Message });
                } 
            return PartialView(procurementmilestoneVM);
        }



        //[ClaimRequirement(UserPermissions.Demand_Read)]
        //public ActionResult ViewProcurementMilestone(int Id)
        //{
        //    ProcurementMilestoneVM procurementmilestoneVM = new ProcurementMilestoneVM();
        //    procurementmilestoneVM.ProcurementMilestoneVMs = new List<ProcurementMilestoneVM>();
        //    using (_dbContext)
        //    {
        //        try
        //        {
        //            List<ProcurementMilestone> procurement_ms = _dbContext.ProcurementMilestones.Where(x => x.DemandDbKey == Id).ToList();
        //            var jsonItems = JsonConvert.SerializeObject(procurement_ms);
        //            procurementmilestoneVM.ProcurementMilestoneVMs = JsonConvert.DeserializeObject<List<ProcurementMilestoneVM>>(jsonItems);
        //        }

        //        catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
        //    }
        //    return PartialView(procurementmilestoneVM);
        //}
        [ClaimRequirement(UserPermissions.Demand_Read)]
        public ActionResult ViewProcurementMilestone(int Id)
        {
            ProcurementMilestoneVM procurementmilestoneVM = new ProcurementMilestoneVM();
            procurementmilestoneVM.ProcurementMilestoneVMs = new List<ProcurementMilestoneVM>();
            using (_dbContext)
            {
                try
                {
                    List<Procurement_Milestone> procurement_ms = _dbContext.Procurement_Milestones
                        .Where(x => x.DemandDbKey == Id && x.Status != "Merged" && x.Status != "Cancelled")
                        .ToList();

                    procurementmilestoneVM.ProcurementMilestoneVMs = procurement_ms.Select(x => new ProcurementMilestoneVM
                    {
                        MilestoneID = x.MilestoneID,
                        DemandDbKey = x.DemandDbKey,
                        MilestoneName = x.MilestoneName,
                        Components = null,
                        Description = null,
                        DueDate = x.CurrentDueDate,
                        CompletionDate = x.CompletionDate,
                        Status = x.Status,
                        Comments = x.Comments,
                        UpdatedBy = x.UpdatedBy,
                        UpdatedOn = x.UpdatedOn,
                        QtyPercentage = x.QtyPercentage,
                        IsLastMilestone = x.IsLastMilestone
                    }).ToList();
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                    return Json(new { success = false, msg = ex.Message });
                }
            }
            return PartialView(procurementmilestoneVM);
        }



        //[Authorize]
        //[ClaimRequirement(UserPermissions.Demand_Delete)]
        //public ActionResult DeleteMilestoneItem(int id = 0)
        //{
        //    try
        //    {
        //        MPGlobals.ExceSQLNonQuery($"Delete FROM [dbo].[ProcurementMilestones] where [MilestoneID] ={id} ");
        //    }
        //    catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
        //    return Json(new { success = true });
        //}
        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Delete)]
        public ActionResult DeleteMilestoneItem(int id = 0)
        {
            try
            {
                MPGlobals.ExceSQLNonQuery($"EXEC [dbo].[DeleteMilestone_DSP] @MilestoneID = {id}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        //[Authorize]
        //[ClaimRequirement(UserPermissions.Demand_Write)]
        //public ActionResult SaveProcurementMileStone([FromBody] IEnumerable<ProcurementMilestone> procurementMilestone)
        //{
        //    using (_dbContext)
        //    {
        //        try
        //        {
        //            foreach (var item in procurementMilestone)
        //            {
        //                if (item.MilestoneID == 0)
        //                {

        //                    item.UpdatedOn = DateTime.Now;
        //                    item.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //                    _dbContext.ProcurementMilestones.Add(item);
        //                }
        //                else
        //                {
        //                    item.UpdatedOn = DateTime.Now;
        //                    item.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //                    _dbContext.Entry(item).State = EntityState.Modified;
        //                }
        //            }
        //            _dbContext.SaveChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(new { success = false });
        //        }

        //    }
        //    return Json(new { success = true });
        //}

        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Write)]
        public ActionResult SaveProcurementMileStone([FromBody] IEnumerable<ProcurementMilestoneVM> procurementMilestone)
        { 
                    try
                    {
                        foreach (var item in procurementMilestone)
                        {
                            if (item.MilestoneID == 0)
                            {
                                // This path shouldn't normally be hit from the ProcurementMilestone view
                                // but keeping for safety
                                var newMilestone = new Procurement_Milestone();
                                newMilestone.DemandDbKey = item.DemandDbKey ?? 0;
                                newMilestone.MilestoneName = item.MilestoneName;
                                newMilestone.CurrentDueDate = item.DueDate ?? DateTime.Now;
                                newMilestone.OriginalDueDate = item.DueDate ?? DateTime.Now;
                                newMilestone.CompletionDate = item.CompletionDate;
                                newMilestone.Comments = item.Comments;
                                newMilestone.Status = "Active";
                                newMilestone.UpdatedOn = DateTime.Now;
                                newMilestone.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                                _dbContext.Procurement_Milestones.Add(newMilestone);
                            }
                            else
                            {
                                var existing = _dbContext.Procurement_Milestones
                                    .Where(x => x.MilestoneID == item.MilestoneID)
                                    .FirstOrDefault();

                                if (existing != null)
                                {
                                    existing.MilestoneName = item.MilestoneName;
                                    existing.CurrentDueDate = item.DueDate ?? existing.CurrentDueDate;
                                    existing.CompletionDate = item.CompletionDate;
                                    existing.Comments = item.Comments;
                                    existing.UpdatedOn = DateTime.Now;
                                    existing.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                                    _dbContext.Entry(existing).State = EntityState.Modified;
                                }
                            }
                        }
                        _dbContext.SaveChanges();
                        return Json(new { success = true });
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false });
                    } 
        }

        #endregion

        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Delete)]
        public ActionResult DeleteReceiptDocument(int documentId = 0, int SplitId = 0)
        {
            try
            { 
                    bool validtodelete = true;
                    List<string> stringList = new List<string>();
                    Procurement_ReceiptItemSplit procurement_ReceiptItemSplits = _dbContext.Procurement_ReceiptItemSplits.Where(x => x.SplitId == SplitId && x.Attachment_Db_Key != null).FirstOrDefault();
                    if (procurement_ReceiptItemSplits != null)
                    {
                        string[] attachments = procurement_ReceiptItemSplits.Attachment_Db_Key.Split(",");
                        for (int i = 0; i < attachments.Count(); i++)
                        {
                            if (attachments[i].ToString() != documentId.ToString())
                            {
                                stringList.Add(attachments[i].ToString());
                            }
                        }
                    }
                    if (validtodelete)
                    {
                        MPGlobals.ExceSQLNonQuery($"Update Procurement_ReceiptItemSplit set Attachment_Db_Key = '{string.Join(",", stringList)}' where SplitId ={SplitId} ");
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false });
                    }
                 
            }
            catch (Exception ex) { 
                ErrorHandler.LogException(ex); 
                return Json(new { success = false, msg = ex.Message });
            }

        }


        public JsonResult GetDemandList(int ProjectId, int demandingofficerId = 0)
        {
            //List<Procurement_Demand> procurement_Demands = _dbContext.Procurement_Demands.Where(x => x.Project_Dbkey == ProjectId && x.IsActive != false).ToList();

            //if (demandingofficerId != 0)
            //{
            //    procurement_Demands = procurement_Demands.Where(x => x.DemandingOfficer == demandingofficerId).ToList();
            //}
            //if (ProjectId == 0 && demandingofficerId != 0)
            //{
            //    List<Procurement_Demand> procurement_Demands_DO = _dbContext.Procurement_Demands.Where(x => x.DemandingOfficer == demandingofficerId && x.IsActive != false).ToList();
            //    return Json(procurement_Demands_DO);
            //}
            //return Json(procurement_Demands);
            List<Procurement_Demand> procurement_Demands = new List<Procurement_Demand>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var demandDetails = connection.QueryMultiple($"[dbo].[DemandListForSelectList]  @ProjectDbkey = {ProjectId} ,@DemandingOfficerKey = {demandingofficerId}");
                procurement_Demands = demandDetails.Read<Procurement_Demand>().ToList();
                return Json(procurement_Demands);
            }
        }

        public JsonResult DemandItemBalance(int DemandDbkey)
        {
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.DemandItemBalance_SSP @DemandDbkey={DemandDbkey}");
                var demandItemBalance = db.Read<dynamic>().ToList();
                return Json(demandItemBalance);
            }
        }


        [HttpPost]
        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Short_Closure)]
        public ActionResult DemandShortClose()
        {
            try
            {
                int userkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                Procurement_Demands_VM model = new Procurement_Demands_VM();
                model.revertshortclose = int.Parse(Request.Form["revertshortclose"]);
                model.ShortClosedBy = userkey;
                model.DemandDbKey = int.Parse(Request.Form["demanddbkey"]);

                if (model.revertshortclose == 0)
                {
                    model.ShortClosedOn = DateTime.Parse(Request.Form["ShortClosedate"]);
                    model.ShortCloseReason = Request.Form["Remarks"];
                }



                string json = JsonConvert.SerializeObject(model);
                MPGlobals.ExceSQLNonQuery($"dbo.ShortCloseDemand_USP @JsonData = '{json}'");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
            return Json(new { success = true });
        }



        [Authorize]
        public ActionResult UpdateReceiptDocType(int documentId = 0, int doctype = 0)
        {
            try
            {
                using (_dbContext)
                {
                    MPGlobals.ExceSQLNonQuery($"Update Attachments set File_DVD_Num = {doctype} where  Attachment_Db_Key = {documentId}");
                    return Json(new { success = true });
                }
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

        }

        #region Demand Receipt history
        [Authorize]
        [HttpGet]
        [ClaimRequirement(UserPermissions.Demand_Read)]
        public IActionResult DemandReceiptsHistory(int id = 0)
        {
            ViewBag.DemandID = id;
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult RMData()
        {
            return PartialView();
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetDemandReceiptsHistoryJResult(int id = 0)
        {
            List<ProcurementDemandReceiptSummaryVM> res = new();
            // return Json(MPGlobals.GetTableAsList(Dt), JsonRequestBehavior.AllowGet);
            try
            {
                DataTable Dt = MPGlobals.GetDataForDatalist("dbo.DemandReceiptSummary_SSP @demandDbkey=" + id);
                res = JsonConvert.DeserializeObject<List<ProcurementDemandReceiptSummaryVM>>(JsonConvert.SerializeObject(Dt));
            }
            catch (Exception ex)
            {
                res = new List<ProcurementDemandReceiptSummaryVM>();
            }
            return Json(res);
        }

        [Authorize]
        [HttpGet]
        public IActionResult PartsDataView()
        {
            return PartialView();
        }

        [Authorize]
        [HttpGet]
        public IActionResult BOIDataView()
        {
            return PartialView();
        }

        [Authorize]
        [HttpGet]
        public IActionResult LRUDataView()
        {
            return PartialView();
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetPartsData(int id = 0, string partsType = "Parts")
        {
            List<ProcurementPartsSummaryVM> res = new();
            // return Json(MPGlobals.GetTableAsList(Dt), JsonRequestBehavior.AllowGet);
            //try
            //{
            //    string sqlCommand = $"dbo.PartReceiptSummary_SSP @demandDbKey={id},@item_type={type}";
            //    DataTable Dt = MPGlobals.GetDataForDatalist(sqlCommand);
            //    res = JsonConvert.DeserializeObject<List<ProcurementPartsSummaryVM>>(JsonConvert.SerializeObject(Dt));
            //}
            //catch (Exception ex)
            //{
            //    res = new List<ProcurementPartsSummaryVM>();
            //}
            //return Json(res);

            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.PartReceiptSummary_SSP @demandDbKey={id}, @item_type={partsType}");
                res = db.Read<ProcurementPartsSummaryVM>().ToList();
            }
            return Json(res);
        }


        #endregion


        #region Procurment Demand Receipt Milestone

        [Authorize]
        [HttpGet]
        public IActionResult procurementDemandMilestone(int id, string IsEditMode = "false")
        {
            List<Procurement_Demand_MileStoneViewModel> procurement_Demand_Mile = new();
            try
            {
                string cmdstr = $"[dbo].[Procurment_MileStone] @DemandDbKey = '{id}' ";
                DataTable dataTable = MPGlobals.GetDataForDatalist(cmdstr);
                procurement_Demand_Mile = JsonConvert.DeserializeObject<List<Procurement_Demand_MileStoneViewModel>>(JsonConvert.SerializeObject(dataTable));

                ViewBag.IsEditMode = IsEditMode;
                return PartialView(procurement_Demand_Mile);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex); throw;
            }

        }

        public IActionResult procurmentMileStoneDetails(int id)
        {
            return View();
        }

        //[HttpGet]
        //public IActionResult CreateMilestone(int DemandDbkey, string EstimatedOrderDate)
        //{

        //    ViewBag.DemandDbKey = DemandDbkey;
        //    if (DemandDbkey != 0)
        //    {
        //        DateTime parsedDate = DateTime.Parse(EstimatedOrderDate);
        //        string OrderDate = parsedDate.ToString("yyyy-MM-dd");
        //        var highestMilestone = _dbContext.Procurement_Demand_MileStones
        //                                 .Where(x => x.DemandDbkey == DemandDbkey)
        //                                 .OrderByDescending(x => x.Milestone)
        //                                 .Select(x => x.Milestone)
        //                                 .FirstOrDefault();

        //        if (highestMilestone == null)
        //        {
        //            highestMilestone = 1;
        //        }
        //        else
        //        {
        //            highestMilestone += 1;
        //        }
        //        ViewBag.MilestoneNumber = highestMilestone;
        //    }

        //    return PartialView();
        //}

        [HttpGet]
        public IActionResult CreateMilestone(int DemandDbkey, string EstimatedOrderDate)
        {
            ViewBag.DemandDbKey = DemandDbkey;
            if (DemandDbkey != 0)
            {
                DateTime parsedDate = DateTime.Parse(EstimatedOrderDate);
                string OrderDate = parsedDate.ToString("yyyy-MM-dd");

                var highestMilestone = _dbContext.Procurement_Milestones
                                         .Where(x => x.DemandDbKey == DemandDbkey)
                                         .OrderByDescending(x => x.MilestoneNumber)
                                         .Select(x => (int?)x.MilestoneNumber)
                                         .FirstOrDefault();

                if (highestMilestone == null)
                {
                    highestMilestone = 1;
                }
                else
                {
                    highestMilestone += 1;
                }
                ViewBag.MilestoneNumber = highestMilestone;
            }

            return PartialView();
        }


        //[HttpPost]
        //public IActionResult CreateMilestone([FromBody] CreateMilestoneVM milestoneVM)
        //{
        //    int UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //    DateTime UpdatedOn = DateTime.Now;
        //    if (milestoneVM != null)
        //    {

        //        ProcurementMilestone procurementMilestone = new ProcurementMilestone();
        //        procurementMilestone.DemandDbKey = milestoneVM.DemandDbKey;
        //        procurementMilestone.MilestoneName = milestoneVM.MilestoneName;
        //        procurementMilestone.DueDate = milestoneVM.DueDate;
        //        procurementMilestone.QtyPercentage = milestoneVM.QtyPercentage;
        //        procurementMilestone.Comments = milestoneVM.Comments;
        //        procurementMilestone.IsLastMilestone = milestoneVM.IsLastMilestone;
        //        procurementMilestone.UpdatedBy = UpdatedBy;
        //        procurementMilestone.UpdatedOn = UpdatedOn;
        //        _dbContext.Add(procurementMilestone);
        //        _dbContext.SaveChanges();

        //        List<Procurement_Demand_Item> procurement_Demand_Items = _dbContext.Procurement_Demand_Items.Where(x => x.DemandDbKey == milestoneVM.DemandDbKey).ToList();
        //        int counter = 0;
        //        foreach (var item in procurement_Demand_Items)
        //        {

        //            Procurement_Demand_MileStone procurement_Demand_MileStone = new Procurement_Demand_MileStone();
        //            //if (Qty_Procurement_Demand_MileStone == 0)
        //            //{

        //            //    float CalculatedQty = (float)((item.Qty * milestoneVM.QtyPercentage) / 100);
        //            //    float roundedQty = (float)Math.Round(CalculatedQty, 2);
        //            //    if (CalculatedQty > 0)
        //            //    {
        //            //        procurement_Demand_MileStone.Qty = roundedQty;
        //            //    }
        //            //    else
        //            //    {
        //            //        procurement_Demand_MileStone.Qty = item.Qty;
        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    var DemandItemQty = item.Qty;
        //            //    DemandItemQty = DemandItemQty -  Qty_Procurement_Demand_MileStone;
        //            //    float CalculatedQty = (float)((DemandItemQty * milestoneVM.QtyPercentage) / 100);
        //            //    if (CalculatedQty > 0)
        //            //    {
        //            //        procurement_Demand_MileStone.Qty = CalculatedQty;
        //            //    }
        //            //    else
        //            //    {
        //            //        procurement_Demand_MileStone.Qty = DemandItemQty;
        //            //    }
        //            //}
        //            var Qty_Procurement_Demand_MileStone = _dbContext.Procurement_Demand_MileStones.Where(x => x.DemandDbkey == milestoneVM.DemandDbKey && x.DemandItemDbKey == item.DemandItemKey)
        //                                                                                           .Select(x => x.Qty).Sum();
        //            if (milestoneVM.IsLastMilestone != true)
        //            {

        //                float CalculatedQty = (float)((item.Qty * milestoneVM.QtyPercentage) / 100);
        //                //float roundedQty = (float)Math.Round(CalculatedQty, 2);
        //                float roundedQty = (float)Math.Round(CalculatedQty, 2, MidpointRounding.AwayFromZero); // Ensure correct rounding
        //                //if (CalculatedQty > 0 && item.Qty > Qty_Procurement_Demand_MileStone)
        //                //{
        //                //    procurement_Demand_MileStone.Qty = roundedQty;
        //                //}
        //                //else
        //                //{

        //                //    procurement_Demand_MileStone.Qty = item.Qty - Qty_Procurement_Demand_MileStone;
        //                //}
        //                if ((Qty_Procurement_Demand_MileStone + roundedQty) < item.Qty)
        //                {
        //                    procurement_Demand_MileStone.Qty = roundedQty;
        //                }
        //                else
        //                {
        //                    procurement_Demand_MileStone.Qty = item.Qty - Qty_Procurement_Demand_MileStone;
        //                    if (item.Qty - Qty_Procurement_Demand_MileStone == 0)
        //                    {
        //                        counter = counter + 1;
        //                        if (counter == procurement_Demand_Items.Count())
        //                        {
        //                            return Json(new { success = "allZeros" });

        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                procurement_Demand_MileStone.Qty = item.Qty - Qty_Procurement_Demand_MileStone;
        //            }

        //            procurement_Demand_MileStone.DemandDbkey = item.DemandDbKey;
        //            procurement_Demand_MileStone.DemandItemDbKey = item.DemandItemKey;
        //            procurement_Demand_MileStone.Milestone = milestoneVM.MilestoneNo;
        //            procurement_Demand_MileStone.DeliveryDate = milestoneVM.DueDate;
        //            procurement_Demand_MileStone.Updatedby = UpdatedBy;
        //            procurement_Demand_MileStone.UpdatedOn = UpdatedOn;
        //            procurement_Demand_MileStone.MilestoneID = procurementMilestone.MilestoneID;

        //            _dbContext.Add(procurement_Demand_MileStone);

        //        }
        //        _dbContext.SaveChanges();

        //        return Json(new { success = true });
        //    }
        //    return Json(new { success = false });

        //}

        [HttpPost]
        public IActionResult CreateMilestone([FromBody] CreateMilestoneVM milestoneVM)
        {
            int UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
            DateTime UpdatedOn = DateTime.Now;
            if (milestoneVM != null)
            {
                // Insert into new Procurement_Milestones table
                Procurement_Milestone procurementMilestone = new Procurement_Milestone();
                procurementMilestone.DemandDbKey = milestoneVM.DemandDbKey ?? 0;
                procurementMilestone.MilestoneNumber = milestoneVM.MilestoneNo ?? 1;
                procurementMilestone.MilestoneName = milestoneVM.MilestoneName;
                procurementMilestone.OriginalDueDate = milestoneVM.DueDate ?? DateTime.Now;
                procurementMilestone.CurrentDueDate = milestoneVM.DueDate ?? DateTime.Now;
                procurementMilestone.QtyPercentage = milestoneVM.QtyPercentage;
                procurementMilestone.Comments = milestoneVM.Comments;
                procurementMilestone.IsLastMilestone = milestoneVM.IsLastMilestone;
                procurementMilestone.Status = "Active";
                procurementMilestone.CreatedBy = UpdatedBy;
                procurementMilestone.CreatedOn = UpdatedOn;
                procurementMilestone.UpdatedBy = UpdatedBy;
                procurementMilestone.UpdatedOn = UpdatedOn;
                _dbContext.Procurement_Milestones.Add(procurementMilestone);
                _dbContext.SaveChanges();

                // Insert item rows into new Procurement_Milestone_Items table
                List<Procurement_Demand_Item> procurement_Demand_Items = _dbContext.Procurement_Demand_Items
                    .Where(x => x.DemandDbKey == milestoneVM.DemandDbKey).ToList();

                var activeMilestoneIds = _dbContext.Procurement_Milestones
                        .Where(m => m.DemandDbKey == milestoneVM.DemandDbKey && m.Status == "Active")
                        .Select(m => m.MilestoneID)
                        .ToList();


                int counter = 0;
                foreach (var item in procurement_Demand_Items)
                {
                    Procurement_Milestone_Item milestoneItem = new Procurement_Milestone_Item();

                    // Calculate qty based on existing milestone allocations

                    var existingQtySum = _dbContext.Procurement_Milestone_Items
                        .Where(x => activeMilestoneIds.Contains(x.MilestoneID)
                                && x.DemandItemDbKey == item.DemandItemKey)
                        .Sum(x => (double?)x.Qty) ?? 0;

                    if (milestoneVM.IsLastMilestone != true)
                    {
                        float CalculatedQty = (float)((item.Qty * milestoneVM.QtyPercentage) / 100);
                        float roundedQty = (float)Math.Round(CalculatedQty, 2, MidpointRounding.AwayFromZero);

                        if ((existingQtySum + roundedQty) < item.Qty)
                        {
                            milestoneItem.Qty = roundedQty;
                        }
                        else
                        {
                            milestoneItem.Qty = item.Qty - existingQtySum;
                            if (item.Qty - existingQtySum == 0)
                            {
                                counter = counter + 1;
                                if (counter == procurement_Demand_Items.Count())
                                {
                                    return Json(new { success = "allZeros" });
                                }
                            }
                        }
                    }
                    else
                    {
                        milestoneItem.Qty = item.Qty - existingQtySum;
                    }

                    milestoneItem.MilestoneID = procurementMilestone.MilestoneID;
                    milestoneItem.DemandItemDbKey = item.DemandItemKey;
                    milestoneItem.UpdatedBy = UpdatedBy;
                    milestoneItem.UpdatedOn = UpdatedOn;

                    _dbContext.Procurement_Milestone_Items.Add(milestoneItem);
                }
                _dbContext.SaveChanges();

                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        //[Authorize]
        //[ClaimRequirement(UserPermissions.Demand_Delete)]
        //public ActionResult DeleteMilestoneColumn(int MilestoneId)
        //{
        //    try
        //    {
        //        MPGlobals.ExceSQLNonQuery($"DELETE FROM [dbo].[Procurement_Demand_MileStone] WHERE [MilestoneID] = {MilestoneId} " +
        //                                  $"DELETE FROM [dbo].[ProcurementMilestones] WHERE [MilestoneID] = {MilestoneId} ");

        //        return Json(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorHandler.LogException(ex);
        //        return Json(new { success = false });
        //    }

        //}

        [Authorize]
        [ClaimRequirement(UserPermissions.Demand_Delete)]
        public ActionResult DeleteMilestoneColumn(int MilestoneId)
        {
            try
            {
                MPGlobals.ExceSQLNonQuery($"EXEC [dbo].[DeleteMilestone_DSP] @MilestoneID = {MilestoneId}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false });
            }
        }


        //[HttpPost]
        //public IActionResult SaveProcurementDemandMilestone([FromBody] CompleteMilestoneData completeMilestoneData)
        //{
        //    if (completeMilestoneData.procurement_Demand_MileStones != null)
        //    {
        //        foreach (var milestone in completeMilestoneData.procurement_Demand_MileStones)
        //        {
        //            // Assuming you have a way to identify the existing entity, e.g., DemandDbkey and DemandItemDbKey
        //            //var existingEntity = _dbContext.Procurement_Demand_MileStones
        //            //                    .Where(m => m.DemandDbkey == milestone.DemandDbkey && m.DemandItemDbKey == milestone.DemandItemDbKey && m.Milestone == milestone.Milestone).FirstOrDefault();

        //            if (milestone.MilestoneDbKey != 0)
        //            {
        //                milestone.Updatedby = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //                milestone.UpdatedOn = DateTime.Now;
        //                _dbContext.Entry(milestone).State = EntityState.Modified;
        //            }
        //            else
        //            {
        //                Procurement_Demand_MileStone procurement_Demand_MileStone = new Procurement_Demand_MileStone();
        //                procurement_Demand_MileStone.DemandDbkey = milestone.DemandDbkey;
        //                procurement_Demand_MileStone.DemandItemDbKey = milestone.DemandItemDbKey;
        //                procurement_Demand_MileStone.Milestone = milestone.Milestone;
        //                procurement_Demand_MileStone.Qty = milestone.Qty;
        //                procurement_Demand_MileStone.DeliveryDate = milestone.DeliveryDate;
        //                procurement_Demand_MileStone.Updatedby = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //                procurement_Demand_MileStone.UpdatedOn = DateTime.Now;
        //                //ProcurementMilestone procurementMilestone = _dbContext.ProcurementMilestones.Where(x => x.DemandDbKey == milestone.DemandDbkey && x.MilestoneName == "Milestone - " + milestone.Milestone).FirstOrDefault();
        //                ProcurementMilestone procurementMilestone = _dbContext.ProcurementMilestones.Where(x => x.MilestoneID == milestone.MilestoneID).FirstOrDefault();
        //                if (procurementMilestone != null)
        //                {
        //                    procurement_Demand_MileStone.MilestoneID = procurementMilestone.MilestoneID;
        //                }
        //                _dbContext.Add(procurement_Demand_MileStone);
        //            }

        //        }
        //        foreach (var procurementMilestone in completeMilestoneData.procurementMilestones)
        //        {
        //            if (procurementMilestone.MilestoneID != 0)
        //            {
        //                ProcurementMilestone procurementMilestone1 = _dbContext.ProcurementMilestones.Where(x => x.MilestoneID == procurementMilestone.MilestoneID).FirstOrDefault();
        //                if (procurementMilestone1 != null)
        //                {
        //                    procurementMilestone1.MilestoneName = procurementMilestone.MilestoneName;
        //                    procurementMilestone1.DueDate = procurementMilestone.DueDate;
        //                    procurementMilestone1.Comments = procurementMilestone.Comments;
        //                    procurementMilestone1.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //                    procurementMilestone1.UpdatedOn = DateTime.Now;

        //                }
        //                _dbContext.Entry(procurementMilestone1).State = EntityState.Modified;
        //            }

        //        }
        //        _dbContext.SaveChanges();
        //        return Json(new { success = true });
        //    }
        //    return Json(new { success = false });

        //}

        //[HttpPost]
        //public IActionResult SaveProcurementDemandMilestone([FromBody] CompleteMilestoneData completeMilestoneData)
        //{
        //    if (completeMilestoneData.procurement_Demand_MileStones != null)
        //    {
        //        foreach (var milestone in completeMilestoneData.procurement_Demand_MileStones)
        //        {
        //            if (milestone.MilestoneDbKey != 0)
        //            {
        //                // Update existing item row in new table
        //                var existingItem = _dbContext.Procurement_Milestone_Items
        //                    .Where(x => x.MilestoneItemID == milestone.MilestoneDbKey)
        //                    .FirstOrDefault();

        //                if (existingItem != null)
        //                {
        //                    existingItem.Qty = milestone.Qty;
        //                    existingItem.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //                    existingItem.UpdatedOn = DateTime.Now;
        //                    _dbContext.Entry(existingItem).State = EntityState.Modified;
        //                }
        //            }
        //            else
        //            {
        //                // Insert new item row
        //                Procurement_Milestone_Item newItem = new Procurement_Milestone_Item();
        //                newItem.MilestoneID = milestone.MilestoneID ?? 0;
        //                newItem.DemandItemDbKey = milestone.DemandItemDbKey ?? 0;
        //                newItem.Qty = milestone.Qty;
        //                newItem.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //                newItem.UpdatedOn = DateTime.Now;
        //                _dbContext.Procurement_Milestone_Items.Add(newItem);
        //            }
        //        }

        //        // Update milestone headers if provided
        //        if (completeMilestoneData.procurementMilestones != null)
        //        {
        //            foreach (var procurementMilestone in completeMilestoneData.procurementMilestones)
        //            {
        //                if (procurementMilestone.MilestoneID != 0)
        //                {
        //                    var existingMilestone = _dbContext.Procurement_Milestones
        //                        .Where(x => x.MilestoneID == procurementMilestone.MilestoneID)
        //                        .FirstOrDefault();

        //                    if (existingMilestone != null)
        //                    {
        //                        existingMilestone.MilestoneName = procurementMilestone.MilestoneName;
        //                        existingMilestone.CurrentDueDate = procurementMilestone.DueDate ?? existingMilestone.CurrentDueDate;
        //                        existingMilestone.Comments = procurementMilestone.Comments;
        //                        existingMilestone.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //                        existingMilestone.UpdatedOn = DateTime.Now;
        //                        _dbContext.Entry(existingMilestone).State = EntityState.Modified;
        //                    }
        //                }
        //            }
        //        }

        //        _dbContext.SaveChanges();
        //        return Json(new { success = true });
        //    }
        //    return Json(new { success = false });
        //}

        [HttpPost]
        public IActionResult SaveProcurementDemandMilestone([FromBody] CompleteMilestoneData completeMilestoneData)
        {
            if (completeMilestoneData == null)
                return Json(new { success = false, msg = "No data received" });

            try
            {
                if (completeMilestoneData.procurement_Demand_MileStones != null)
                {
                    foreach (var milestone in completeMilestoneData.procurement_Demand_MileStones)
                    {
                        // Skip rows with no item (empty milestone placeholder rows)
                        if (milestone.DemandItemDbKey == null || milestone.DemandItemDbKey == 0)
                            continue;

                        if (milestone.MilestoneDbKey != null && milestone.MilestoneDbKey != 0)
                        {
                            // Update existing item row
                            var existingItem = _dbContext.Procurement_Milestone_Items
                                .Where(x => x.MilestoneItemID == milestone.MilestoneDbKey)
                                .FirstOrDefault();

                            if (existingItem != null)
                            {
                                existingItem.Qty = milestone.Qty;
                                existingItem.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                                existingItem.UpdatedOn = DateTime.Now;
                                _dbContext.Entry(existingItem).State = EntityState.Modified;
                            }
                        }
                        else
                        {
                            // Insert new item row
                            Procurement_Milestone_Item newItem = new Procurement_Milestone_Item();
                            newItem.MilestoneID = milestone.MilestoneID ?? 0;
                            newItem.DemandItemDbKey = milestone.DemandItemDbKey ?? 0;
                            newItem.Qty = milestone.Qty;
                            newItem.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                            newItem.UpdatedOn = DateTime.Now;
                            _dbContext.Procurement_Milestone_Items.Add(newItem);
                        }
                    }
                }

                // Update milestone headers
                if (completeMilestoneData.procurementMilestones != null)
                {
                    foreach (var procurementMilestone in completeMilestoneData.procurementMilestones)
                    {
                        if (procurementMilestone.MilestoneID != 0)
                        {
                            var existingMilestone = _dbContext.Procurement_Milestones
                                .Where(x => x.MilestoneID == procurementMilestone.MilestoneID)
                                .FirstOrDefault();

                            if (existingMilestone != null)
                            {
                                existingMilestone.MilestoneName = procurementMilestone.MilestoneName;
                                existingMilestone.CurrentDueDate = procurementMilestone.DueDate ?? existingMilestone.CurrentDueDate;
                                existingMilestone.Comments = procurementMilestone.Comments;
                                existingMilestone.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                                existingMilestone.UpdatedOn = DateTime.Now;
                                _dbContext.Entry(existingMilestone).State = EntityState.Modified;
                            }
                        }
                    }
                }

                _dbContext.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }


        //public IActionResult ExtendMilestone(int demandDBkey, int MilestoneID)
        //{
        //    List<ProcurementMilestone> MileStoneData = _dbContext.ProcurementMilestones.Where(x => x.DemandDbKey == demandDBkey && x.Status != "Extended").ToList();

        //    ViewBag.UserID = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //    ViewBag.MilestoneId = MilestoneID;

        //    return PartialView(MileStoneData);
        //}

        public IActionResult ExtendMilestone(int demandDBkey, int MilestoneID)
        {
            List<Procurement_Milestone> MileStoneData = _dbContext.Procurement_Milestones
                .Where(x => x.DemandDbKey == demandDBkey && x.Status != "Merged" && x.Status != "Cancelled")
                .ToList();

            ViewBag.UserID = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
            ViewBag.MilestoneId = MilestoneID;

            return PartialView(MileStoneData);
        }

        [HttpPost]
        public IActionResult SaveExtendedMileStone([FromBody] List<ExtendMileStoneVM> extendedmilestoneVM)
        {
            try
            {
                var extendedJSon = JsonConvert.SerializeObject(extendedmilestoneVM);
                MPGlobals.ExceSQLNonQuery($" dbo.ExtendProcurmentDemandMileStone_IUSP @json= {extendedJSon} ");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult MilestoneReport()
        {
            return View();
        }


        [HttpGet]
        public IActionResult MilestoneStatusReport(int demandingOfficerkey = 0, string status = "All", string duedate = "All", int project = 0, int demandDbkey = 0, string viewMode = "DemandScreen")
        {
            ViewBag.viewMode = viewMode;
            List<MilestoneSummaryVM> summaryInfo = new List<MilestoneSummaryVM>();
            List<Procurement_Demand> demandsDorSelectlist = new List<Procurement_Demand>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var docData = connection.QueryMultiple($"[dbo].[GetMilestoneDuedays] @DemandingOfficerDbkey =  {demandingOfficerkey}");
                summaryInfo = docData.Read<MilestoneSummaryVM>().ToList();
                if (status != "All" && summaryInfo != null)
                {
                    summaryInfo = summaryInfo.Where(x => x.MilestoneStatus == status).ToList();
                }

                if (duedate != "All" && summaryInfo != null)
                {
                    if (duedate == "<0")
                    {
                        summaryInfo = summaryInfo.Where(x => x.RemainingDays <= 0).ToList();
                    }
                    else
                    {
                        summaryInfo = summaryInfo.Where(x => x.RemainingDays >= 0 && x.RemainingDays <= int.Parse(duedate)).ToList();
                    }
                }

                if (project != 0 && summaryInfo != null)
                {
                    summaryInfo = summaryInfo.Where(x => x.ProjectDbkey == project).ToList();
                }
                //            if (summaryInfo != null) {
                //	List<SelectListItem> selectListItems = new List<SelectListItem>();
                //	selectListItems.Add(new SelectListItem() { Text = "Select", Value = "0" });
                //	foreach (var item in summaryInfo)
                //	{
                //		selectListItems.Add(new SelectListItem() { Text = item.MMG_File_No + "-" + item.Item_Description, Value = item.DemandDbKey.ToString() }); 
                //	}
                //                summaryInfo.demands = selectListItems;
                //}

                if (demandDbkey != 0 && summaryInfo != null)
                {
                    summaryInfo = summaryInfo.Where(x => x.DemandDbKey == demandDbkey).ToList();
                }

            }
            return View(summaryInfo);
        }

         
        [HttpGet]
        public IActionResult MilestoneItemsForSummaryTable(int DemandDbkey, int MilestoneId)
        {
            List<Procurement_Demand_MileStoneViewModel> procurement_Demand_Mile = new();
            using (_dbContext)
            {
                ViewBag.MilestoneId = MilestoneId;
                string cmdstr = $"[dbo].[Procurment_MileStone] @DemandDbKey = '{DemandDbkey}' ";
                DataTable dataTable = MPGlobals.GetDataForDatalist(cmdstr);
                procurement_Demand_Mile = JsonConvert.DeserializeObject<List<Procurement_Demand_MileStoneViewModel>>(JsonConvert.SerializeObject(dataTable));
                return PartialView(procurement_Demand_Mile);
            }
        }
        #endregion

        [Authorize]
        public ActionResult ReceiptDocumentMappingToSplit(int ReceiptDbkey, bool forceupdate, string password)
        {
            int userkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
            string encryptpassword = MPGlobals.Encrypt_SHA256(password);
            DapperExecDbResponse dapperExecDbResponse = new DapperExecDbResponse();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var docData = connection.QueryMultiple($"[dbo].[MapReceiptDocumentToSplit_IUSP] @ReceiptDbkey = {ReceiptDbkey},@ForceUpdate={forceupdate},@password='{encryptpassword}',@Userid={userkey}");
                dapperExecDbResponse = docData.Read<DapperExecDbResponse>().FirstOrDefault();
            }
            return Json(dapperExecDbResponse);
        }

        [HttpGet]
        public IActionResult DemandDetailsInTableFormat(int ProjectDbkey = 0, int DemandingOfficerKey = 0)
        {
            List<Procurement_Demands_VM> procurement_Demands_VM = new List<Procurement_Demands_VM>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var demandDetails = connection.QueryMultiple($"[dbo].[Get_ListofDemandDetailsForTable]  @Project_Dbkey = {ProjectDbkey} ,@DemandingOfficerDbKey = {DemandingOfficerKey}");
                procurement_Demands_VM = demandDetails.Read<Procurement_Demands_VM>().ToList();
                return PartialView(procurement_Demands_VM);
            }
        }

        [HttpGet]
        public IActionResult Project_DemandingOfficer_Report(int ProjectDbkey = 0, int DemandingOfficerDbKey = 0)
        {

            using (var connection = mPDapperContext.CreateConnection())
            {
                //var details = connection.QueryMultiple($"[dbo].[Project_DO_Details]  @projectDbkey = {ProjectDbkey}, @demandinfofficerDbkey = {DemandingOfficerDbKey} ");
                //reportDetails = details.Read<Project_Do_Report>().ToList();
                //return PartialView(reportDetails);
                ViewBag.DemandingOfficerDbKey = DemandingOfficerDbKey;
                var details = connection.QueryMultiple($"[dbo].[GetDemandSummaryTable] @projectkey = {ProjectDbkey} ,@DemandingOfficerDbKey ={DemandingOfficerDbKey} ");
                List<demandSummaryData> reportDetails = details.Read<demandSummaryData>().ToList();
                return PartialView(reportDetails);
            }

        }

        [HttpGet]
        public IActionResult DemandsList(int DemadingOfficerKey, int ProjectDbkey, string CurrrentStatus)
        {
            if (CurrrentStatus.Contains("DO Review"))
            {
                CurrrentStatus = CurrrentStatus.Replace("(DO Review)", "").Trim();
            }
            using (var connection = mPDapperContext.CreateConnection())
            {
                var details = connection.QueryMultiple($"[dbo].[StatusFilteredDemandList] @DemandsingOfficerDbKey = {DemadingOfficerKey} ,@ProjectDbkey ={ProjectDbkey} ,@CurrrentStatus = '{CurrrentStatus}' ");
                List<Procurement_Demands_VM> demandDetails = details.Read<Procurement_Demands_VM>().ToList();
                return PartialView(demandDetails);
            }
            //List<Procurement_Demand> demands = new();

            //if (DemadingOfficerKey != 0)
            //         {
            //	 demands = _dbContext.Procurement_Demands.Where(x => x.DemandingOfficer == DemadingOfficerKey && x.Project_Dbkey == ProjectDbkey).ToList();
            //	return PartialView(demands);
            //}
            //return PartialView(demands);
        }

       

        [HttpGet]
        public IActionResult ExecutiveSummary(int ProjectDbkey = 0, int DemandingOfficerKey = 0, string filter = "All")
        {
            List<Procurement_Demands_VM> procurement_Demands_VM = new List<Procurement_Demands_VM>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var demandDetails = connection.QueryMultiple($"[dbo].[DemandExecutiveSummary]  @Project_Dbkey = {ProjectDbkey} ,@DemandingOfficerDbKey = {DemandingOfficerKey} ,@filter = '{filter}'");
                procurement_Demands_VM = demandDetails.Read<Procurement_Demands_VM>().ToList();
                return PartialView(procurement_Demands_VM);
            }
        }

        public IActionResult DashboardSummary(string itemType)
        {
            ViewBag.Engines = MPGlobals.GetDataForDatalist("Select [DataJson] from [dbo].[AppSettings] where [AppSettingType] = 'Engines'").Rows[0][0].ToString();
            List<DemandSummaryDashboard> demandSummary = new List<DemandSummaryDashboard>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.[DemandSummaryDashboard_SSP] @itemType = '{itemType}' ");
                demandSummary = db.Read<DemandSummaryDashboard>().ToList();
            }
            return PartialView(demandSummary);
        }

        [HttpGet]
        public IActionResult DemandItemQtyAdjustment(int demandItemKey)
        {
            try
            {
                DemandItemQtyAdjustmentVM viewModel = new DemandItemQtyAdjustmentVM();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    string cmdstr = $"EXEC [dbo].[GetDemandItemAdjustmentDetail_SSP] @DemandItemKey = {demandItemKey}";
                    var result = connection.QueryFirstOrDefault<DemandItemQtyAdjustmentVM>(cmdstr);

                    if (result != null)
                    {
                        viewModel = result;
                    }
                    else
                    {
                        // Item not found
                        ViewBag.ErrorMessage = "Demand item not found";
                    }
                }

                return PartialView(viewModel);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                ViewBag.ErrorMessage = ex.Message;
                return PartialView(new DemandItemQtyAdjustmentVM());
            }
        }

        [HttpPost]
        public IActionResult SaveDemandItemAdjustment(int demandItemKey, int adjustmentDbkey, double adjustmentQty, string adjustmentRemarks)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(adjustmentRemarks))
                {
                    return Json(new { success = false, message = "Remarks are required" });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                if (adjustmentDbkey == 0)
                {
                    // INSERT
                    var newAdjustment = new Procurement_Demand_Item_Adjustment
                    {
                        DemandItemKey = demandItemKey,
                        Adjustment_Qty = adjustmentQty,
                        Adjustment_Remarks = adjustmentRemarks,
                        Adjusted_By = userId,
                        Adjusted_On = DateTime.Now
                    };

                    _dbContext.Procurement_Demand_Item_Adjustments.Add(newAdjustment);
                    _dbContext.SaveChanges();

                    // CHANGED: do not update milestone item quantities
                    // RecalculateMilestoneItemsForAdjustedDemandItem(demandItemKey);

                    return Json(new
                    {
                        success = true,
                        message = "Adjustment saved successfully",
                        adjustmentDbkey = newAdjustment.Adjustment_Dbkey
                    });
                }
                else
                {
                    // UPDATE
                    var existingAdjustment = _dbContext.Procurement_Demand_Item_Adjustments
                        .FirstOrDefault(x => x.Adjustment_Dbkey == adjustmentDbkey);

                    if (existingAdjustment == null)
                    {
                        return Json(new { success = false, message = "Adjustment not found" });
                    }

                    existingAdjustment.Adjustment_Qty = adjustmentQty;
                    existingAdjustment.Adjustment_Remarks = adjustmentRemarks;
                    existingAdjustment.Adjusted_By = userId;
                    existingAdjustment.Adjusted_On = DateTime.Now;

                    _dbContext.Entry(existingAdjustment).State = EntityState.Modified;
                    _dbContext.SaveChanges();

                    // CHANGED: do not update milestone item quantities
                    // RecalculateMilestoneItemsForAdjustedDemandItem(demandItemKey);

                    return Json(new
                    {
                        success = true,
                        message = "Adjustment updated successfully",
                        adjustmentDbkey = existingAdjustment.Adjustment_Dbkey
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, message = ex.Message });
            }
        }




        [HttpPost]
        [ClaimRequirement(UserPermissions.Demand_Receipt_Delete)]        
        public async Task<IActionResult> DeleteDemandReceipts(int demandDbKey, int indexNo)
        {
            try
            {
                if (demandDbKey <= 0 || indexNo <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "DemandDbKey or IndexNo is missing or invalid."
                    });
                }

                // 1. Get connection string from appsettings.json ("DefaultConnection" is just an example)
                var connectionString = _configuration.GetConnectionString("MPCRS");

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    using (var cmd = new SqlCommand("dbo.DeleteDemandReceipt", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // 2. Pass parameters to SP
                        cmd.Parameters.AddWithValue("@DemandDbKey", demandDbKey);
                        cmd.Parameters.AddWithValue("@Index_No", indexNo);

                        // 3. Execute and read the single row returned by the SP
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Columns come from: SELECT @Success AS Success, @Message AS Message
                                bool success = reader.GetBoolean(reader.GetOrdinal("Success"));
                                string message = reader.GetString(reader.GetOrdinal("Message"));

                                return Json(new { success, message });
                            }
                            else
                            {
                                // SP didn’t return anything (shouldn’t normally happen)
                                return Json(new
                                {
                                    success = false,
                                    message = "Unexpected error: stored procedure returned no result."
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimRequirement(UserPermissions.Demand_Financial_Read)]
        public IActionResult BulkEditFinancialDetails(int ProjectDbkey = 0, int DemandingOfficerKey = 0)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    // Load all rows once. Project filter will be client-side in DataTable.
                    var demandList = connection.Query<Procurement_Demands_VM>(
                        "[dbo].[Get_ListofDemandDetailsForTable] @Project_Dbkey = @Project_Dbkey, @DemandingOfficerDbKey = @DemandingOfficerDbKey",
                        new
                        {
                            Project_Dbkey = 0,
                            DemandingOfficerDbKey = DemandingOfficerKey
                        }).ToList();

                    var vm = new Procurement_Demand_FinancialBulkEditPage_VM
                    {
                        ProjectDbkey = ProjectDbkey,
                        DemandingOfficerKey = DemandingOfficerKey,
                        Items = demandList.Select(x => new Procurement_Demand_FinancialBulkEdit_VM
                        {
                            DemandDbKey = x.DemandDbKey,
                            Project_Dbkey = x.Project_Dbkey,
                            DemandingOfficerKey = x.DemandingOfficer,
                            MMG_File_No = x.MMG_File_No,
                            Item_Description = x.Item_Description,
                            ActualCost = x.ActualCost,
                            ProjectRunningBalance = x.ProjectRunningBalance,
                            AdvancePaid = x.AdvancePaid,
                            PaymentMadeTillDate = x.PaymentMadeTillDate,
                            BalanceOrderValue = x.BalanceOrderValue,
                            CurrentStatus = x.CurrentStatus,
                            Remarks = x.Remarks,
                            StatusDate = x.StatusDate,
                            Updated_On = x.Updated_On
                        }).ToList()
                    };

                    
                    ViewBag.Projects = MPCRS.Utilities.MPGlobals.GetDataForDatalist("dbo.ProjectList_SP");

                    // Add Demanding Officer list
                    ViewBag.DemandingOfficers = MPCRS.Utilities.MPGlobals.GetDataForDatalist("dbo.DemandingOfficerList_SP");


                     
                   
                    return View(vm);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("DemandTree");
            }
        }

        [HttpPost]
        [ClaimRequirement(UserPermissions.Demand_Financial_Edit)]
        public IActionResult BulkEditFinancialDetails(string bulkEditJson, int ProjectDbkey = 0, int DemandingOfficerKey = 0)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bulkEditJson))
                {
                    TempData["ErrorMessage"] = "No demand data found to save.";
                    return RedirectToAction("BulkEditFinancialDetails", new { ProjectDbkey, DemandingOfficerKey });
                }

                var model = JsonConvert.DeserializeObject<List<Procurement_Demand_FinancialBulkEdit_VM>>(bulkEditJson);

                if (model == null || !model.Any())
                {
                    TempData["ErrorMessage"] = "No demand data found to save.";
                    return RedirectToAction("BulkEditFinancialDetails", new { ProjectDbkey, DemandingOfficerKey });
                }

                int userId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                DateTime now = DateTime.Now;

                var demandKeys = model
                    .Where(x => x != null)
                    .Select(x => x.DemandDbKey)
                    .Distinct()
                    .ToList();

                var dbDemands = _dbContext.Procurement_Demands
                    .Where(x => demandKeys.Contains(x.DemandDbKey))
                    .ToList();

                var dbDemandMap = dbDemands.ToDictionary(x => x.DemandDbKey, x => x);

                var dbHistories = _dbContext.Procurement_Demands_Histories
                    .Where(x => x.DemandDbKey.HasValue && demandKeys.Contains(x.DemandDbKey.Value))
                    .ToList();

                foreach (var item in model)
                {
                    if (item == null)
                        continue;

                    if (!dbDemandMap.TryGetValue(item.DemandDbKey, out var dbModel))
                        continue;

                    dbModel.ActualCost = item.ActualCost;
                    dbModel.AdvancePaid = item.AdvancePaid;
                    dbModel.PaymentMadeTillDate = item.PaymentMadeTillDate;
                    dbModel.CurrentStatus = item.CurrentStatus;
                    dbModel.Remarks = item.Remarks;
                    dbModel.StatusDate = item.StatusDate;
                    dbModel.Updated_By = userId;
                    dbModel.Updated_On = now;

                    var history = dbHistories.FirstOrDefault(x =>
                        x.DemandDbKey.HasValue &&
                        x.DemandDbKey.Value == dbModel.DemandDbKey &&
                        x.ActionStatus == dbModel.CurrentStatus &&
                        x.Do_Review == dbModel.DO_Review);

                    if (history == null)
                    {
                        history = new Procurement_Demands_History
                        {
                            DemandDbKey = dbModel.DemandDbKey,
                            ActionDate = dbModel.StatusDate,
                            ActionStatus = dbModel.CurrentStatus,
                            Do_Review = dbModel.DO_Review,
                            Remarks = dbModel.Remarks,
                            Updated_By = dbModel.Updated_By,
                            Updated_On = dbModel.Updated_On
                        };

                        _dbContext.Procurement_Demands_Histories.Add(history);
                        dbHistories.Add(history);
                    }
                    else
                    {
                        history.ActionDate = dbModel.StatusDate;
                        history.ActionStatus = dbModel.CurrentStatus;
                        history.Do_Review = dbModel.DO_Review;
                        history.Remarks = dbModel.Remarks;
                        history.Updated_By = dbModel.Updated_By;
                        history.Updated_On = dbModel.Updated_On;
                    }
                }

                _dbContext.SaveChanges();

                TempData["SuccessMessage"] = "Financial details updated successfully.";
                return RedirectToAction("BulkEditFinancialDetails", new { ProjectDbkey = 0, DemandingOfficerKey });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("BulkEditFinancialDetails", new { ProjectDbkey, DemandingOfficerKey });
            }
        }
    }
}
