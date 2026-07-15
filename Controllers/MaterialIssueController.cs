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
using Microsoft.AspNetCore.Hosting;
using System.Net.Mail;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;
using System.Security.Cryptography.Xml;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Text;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace MPCRS.Controllers
{
    [Authorize]
    public class MaterialIssueController : Controller
    {

        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;
        public MaterialIssueController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.MaterialIssue_Read)]
        public IActionResult MaterialIssueSummary()
        {
            //List<MaterialIssueSummaryVM> materialIssueSummary = new List<MaterialIssueSummaryVM>();
            //using (var connection = mPDapperContext.CreateConnection())
            //{
            //    var demandtree = connection.QueryMultiple($"[dbo].[Get_Rawmaterial_Issues]");
            //    materialIssueSummary = demandtree.Read<MaterialIssueSummaryVM>().ToList();
            //}

            return View();
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.MaterialIssue_Read)]
        public IActionResult MaterialIssueDetails()
        {
            List<MaterialIssueSummaryVM> materialIssueSummary = new List<MaterialIssueSummaryVM>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var demandtree = connection.QueryMultiple($"[dbo].[Get_Rawmaterial_Issues]");
                materialIssueSummary = demandtree.Read<MaterialIssueSummaryVM>().ToList();
            }

            return Json(materialIssueSummary);
        }

        [HttpGet]
        public IActionResult NewMaterialIssue(int id = 0, int part = 0)
        {
            MaterialIssueVM materialIssueVM = new MaterialIssueVM();
            using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
            {
                materialIssueVM.material_Issue_Items = new List<Material_Issue_Items_VM>();
                ViewBag.part = part;
                ViewBag.EngineOptions = GetEngineOptions();

                if (id == 0)
                {
                    materialIssueVM.Engine_Name = "STFE";
                    ViewBag.PartDetail = "";

                    if (part > 0)
                    {
                        Engine_Parts_Master partsmaster = db.Engine_Parts_Masters
                            .Where(x => x.Engine_Part_Dbkey == part)
                            .FirstOrDefault();

                        if (partsmaster != null)
                        {
                            ViewBag.PartDetail = partsmaster.Draw_part_no + "/" + partsmaster.Description;
                        }
                    }

                    return PartialView(materialIssueVM);
                }
                else
                {
                    var model = GetMaterialIssue(id);
                    ViewBag.EngineOptions = GetEngineOptions();
                    return PartialView(model);
                }
            }
        }

        private List<SelectListItem> GetEngineOptions()
        {
            var engineStr = _dbContext.AppSettings
                .Where(x => x.AppSettingType == "Engines")
                .Select(x => x.DataJson)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(engineStr))
                return new List<SelectListItem>();

            return engineStr
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x) && x != "GroundTest")
                .Select(x => new SelectListItem
                {
                    Value = x,
                    Text = x
                })
                .ToList();
        }

        [HttpGet]
        public JsonResult GetDemandDetails(int id)
        {
            var demand = _dbContext.Procurement_Demands.FirstOrDefault(x => x.DemandDbKey == id);
            if (demand == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                orderNumbers = demand.OrderNumbers ?? "",
                vendorDbkey = demand.Vendor_Dbkey ?? 0,
                orderDate = demand.EstimatedOrderDate?.ToString("yyyy-MM-dd") ?? "",
                demandNo = demand.Demand_No ?? ""
            });
        }

        [HttpGet]
        public JsonResult GetHeatBatchNumbers(int rawMaterialDbKey)
        {
            using (var connection = mPDapperContext.CreateConnection())
            {
                var sql = @"SELECT DISTINCT Value FROM (
                    SELECT prs.Heat_No AS Value
                    FROM Procurement_ReceiptItemSplit prs
                    INNER JOIN Procurement_Demand_Receipts pdr ON prs.Receipt_dbkey = pdr.Receipt_dbkey
                    INNER JOIN Procurement_Demand_Items pdi ON pdr.DemandItemKey = pdi.DemandItemKey
                    WHERE pdi.ItemDbKey = @RawMaterialDbKey AND prs.Heat_No IS NOT NULL AND prs.Heat_No != ''
                    UNION
                    SELECT prs.Batch_No AS Value
                    FROM Procurement_ReceiptItemSplit prs
                    INNER JOIN Procurement_Demand_Receipts pdr ON prs.Receipt_dbkey = pdr.Receipt_dbkey
                    INNER JOIN Procurement_Demand_Items pdi ON pdr.DemandItemKey = pdi.DemandItemKey
                    WHERE pdi.ItemDbKey = @RawMaterialDbKey AND prs.Batch_No IS NOT NULL AND prs.Batch_No != ''
                ) Combined ORDER BY Value";

                var values = connection.Query<string>(sql, new { RawMaterialDbKey = rawMaterialDbKey }).ToList();
                return Json(values);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetPartNoJson()
        {
            DataTable dt = MPGlobals.GetDataForDatalist(@"SELECT Engine_Part_Dbkey as value,[Draw_part_no] + '/' + isnull([Description],'') as label
           FROM[dbo].[Engine_Parts_Master]");
            return Json(MPGlobals.GetTableAsList(dt));

        }
        [Authorize]
        public ActionResult DeleteIssueItem(int id = 0)
        {
            //DataTable dataTable = MPGlobals.GetDataForDatalist($"Select [forging_recp_dbkey]  FROM [dbo].[Forging_Receipts] where [Issue_Item_Dbkey] ={id} ");
            //if (dataTable.Rows.Count == 0)
            //{
            MPGlobals.ExceSQLNonQuery($"Delete  FROM [dbo].[Material_Issue_Items] where [Issue_Item_Dbkey] ={id} ");
            MPGlobals.ExceSQLNonQuery($"Delete from [dbo].[Material_IssueItems_Consolidation]  where [Issue_Item_Dbkey] = {id} ");
            MPGlobals.ExceSQLNonQuery($"Delete from [dbo].[Material_Issue_Items_Parts]  where [Issue_Item_Dbkey] = {id} ");
            return Json(new { success = true });
            //}
            //else
            //{
            //    return Json(new { success = false });
            //}

        }

        [HttpPost]
        [Authorize]
        public ActionResult SaveMaterialIssue()
        {

            try
            {
                var loggedInUser = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var Material_Issue_mainArray = Request.Form["MaterialIssuemain"];
                var Material_Issue_ItemsArray = Request.Form["MaterialIssueItems"];

                var UploadedFiles = Request.Form.Files;
                Material_Issue_Note MaterialIssueNote = JsonConvert.DeserializeObject<List<Material_Issue_Note>>(Material_Issue_mainArray).FirstOrDefault();
                List<Material_Issue_Items_VM> MaterialIssueItems = JsonConvert.DeserializeObject<List<Material_Issue_Items_VM>>(Material_Issue_ItemsArray);


                Material_Issue_Note dbMaterialIssueNote = _dbContext.Material_Issue_Notes.Where(x => x.Issue_Dbkey == MaterialIssueNote.Issue_Dbkey).FirstOrDefault();

                dbMaterialIssueNote = dbMaterialIssueNote == null ? new() : dbMaterialIssueNote;
                dbMaterialIssueNote.Ref_Number = dbMaterialIssueNote.Issue_Dbkey == 0 ? GetMaterialRefNo() : MaterialIssueNote.Ref_Number;
                dbMaterialIssueNote.Form_Number = MaterialIssueNote.Form_Number;
                dbMaterialIssueNote.Engine_Name = MaterialIssueNote.Engine_Name;
                dbMaterialIssueNote.Demand_No = MaterialIssueNote.Demand_No;
                dbMaterialIssueNote.DemandDbKey = MaterialIssueNote.DemandDbKey;
                dbMaterialIssueNote.Order_Ref_No = MaterialIssueNote.Order_Ref_No;
                dbMaterialIssueNote.Order_Ref_Date = MaterialIssueNote.Order_Ref_Date;
                dbMaterialIssueNote.PMO_Ref_No = MaterialIssueNote.PMO_Ref_No;
                dbMaterialIssueNote.PMO_Ref_Date = MaterialIssueNote.PMO_Ref_Date;
                dbMaterialIssueNote.Demanding_Officer = MaterialIssueNote.Demanding_Officer;
                dbMaterialIssueNote.Tech_Officer = MaterialIssueNote.Tech_Officer;
                dbMaterialIssueNote.Project_Director = MaterialIssueNote.Project_Director;
                dbMaterialIssueNote.Vendor = MaterialIssueNote.Vendor;
                dbMaterialIssueNote.MR_No = MaterialIssueNote.MR_No;
                dbMaterialIssueNote.Book_Serial_No = MaterialIssueNote.Book_Serial_No;
                dbMaterialIssueNote.Volume_No = MaterialIssueNote.Volume_No;
                dbMaterialIssueNote.Updated_By = loggedInUser;
                dbMaterialIssueNote.Updated_On = DateTime.Now;
                dbMaterialIssueNote.Total_Qty = MaterialIssueNote.Total_Qty;
                dbMaterialIssueNote.Total_Cost = MaterialIssueNote.Total_Cost;
                dbMaterialIssueNote.Returnable = MaterialIssueNote.Returnable;
                dbMaterialIssueNote.Issue_Purpose = MaterialIssueNote.Issue_Purpose;
                dbMaterialIssueNote.Job_Card = MaterialIssueNote.Job_Card;
                dbMaterialIssueNote.JobCardFileLocation = MaterialIssueNote.JobCardFileLocation;
                dbMaterialIssueNote.JobCardFileName = MaterialIssueNote.JobCardFileName;
                dbMaterialIssueNote.IsActive = MaterialIssueNote.IsActive;
                dbMaterialIssueNote.Attachment_Db_Key = MaterialIssueNote.Attachment_Db_Key;

                if (dbMaterialIssueNote.Issue_Dbkey == 0)
                {
                    _dbContext.Material_Issue_Notes.Add(dbMaterialIssueNote);
                    _dbContext.SaveChanges();
                }
                else
                {
                    _dbContext.Entry(dbMaterialIssueNote).State = EntityState.Modified;

                }

                // var PartSmater = _dbContext.Engine_Parts_Masters.ToList();


                int counter = 1;
                foreach (var item in MaterialIssueItems)
                {
                    item.Issue_Dbkey = dbMaterialIssueNote.Issue_Dbkey;
                    Material_Issue_Item fileSavedInfo = UploadJobCardDocument(UploadedFiles, counter, (int)item.Issue_Item_Dbkey, dbMaterialIssueNote.Issue_Dbkey);
                    // convert VM to db Model
                    //  Material_Issue_Item MaterialIssueItem = JsonConvert.DeserializeObject<Material_Issue_Item>(JsonConvert.SerializeObject(item));

                    var MaterialIssueItem = new Material_Issue_Item
                    {
                        Issue_Dbkey = dbMaterialIssueNote.Issue_Dbkey,
                        Qty = (double)item.Qty,
                        Raw_material_Dbkey = (int)item.Raw_material_Dbkey,
                        Vendor_Dbkey = item.Vendor_Dbkey,
                        Size = item.Size,
                        Denom = item.Denom,
                        Qty_Issue = (double)item.Qty_Issue,
                        Heat_No = item.Heat_No,
                        EngineLevel = item.EngineLevel,   // NEW
                        Weight_Kg = item.Weight_Kg,
                        Amount = (double)item.Amount,
                        SerialNo = item.SerialNo,
                        JobCardNumber = item.JobCardNumber,
                        JCFileName = item?.JCFileName,
                        JCFileLocation = item?.JCFileLocation,
                        Updated_On = DateTime.Now,
                        Updated_By = loggedInUser,
                        Issue_Item_Dbkey = (int)item.Issue_Item_Dbkey
                    };

                    if (fileSavedInfo != null)
                    {
                        MaterialIssueItem.JCFileName = fileSavedInfo.JCFileName;
                        MaterialIssueItem.JCFileLocation = fileSavedInfo.JCFileLocation;
                    }

                    MaterialIssueItem.Issue_Dbkey = dbMaterialIssueNote.Issue_Dbkey;
                    MaterialIssueItem.Updated_On = DateTime.Now;
                    MaterialIssueItem.Updated_By = loggedInUser;

                    if (MaterialIssueItem.Issue_Item_Dbkey == 0)
                    {
                        _dbContext.Material_Issue_Items.Add(MaterialIssueItem);
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        _dbContext.Material_Issue_Items.Entry(MaterialIssueItem).State = EntityState.Modified;

                    }
                    // below item is used for saving part keys onto another table
                    item.Issue_Dbkey = MaterialIssueItem.Issue_Dbkey;
                    item.Issue_Item_Dbkey = MaterialIssueItem.Issue_Item_Dbkey;
                    saveIssueItemParts(item);

                    counter++;
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


        public void saveIssueItemParts(Material_Issue_Items_VM issueItem)
        {

            var IssueItemKey = issueItem.Issue_Item_Dbkey;
            var PartKeys = issueItem.PartNumberKey;
            var MI_Items_Parts = _dbContext.Material_Issue_Items_Parts.Where(x => x.Issue_Item_Dbkey == IssueItemKey).ToList();
            if (MI_Items_Parts != null)
            {
                var PartKeysToRemove = MI_Items_Parts.Where(x => !PartKeys.Contains(x.Engine_Part_Dbkey)).ToList();
                _dbContext.Material_Issue_Items_Parts.RemoveRange(PartKeysToRemove);
                _dbContext.SaveChanges();
            }
            if (PartKeys != null)
            {
                foreach (var item in PartKeys)
                {
                    if (MI_Items_Parts.Where(x => x.Engine_Part_Dbkey == item).FirstOrDefault() == null)
                    {
                        var PartName = _dbContext.Engine_Parts_Masters.Where(x => x.Engine_Part_Dbkey == item).FirstOrDefault();
                        _dbContext.Material_Issue_Items_Parts.Add(new Material_Issue_Items_Part()
                        {
                            Issue_Dbkey = issueItem.Issue_Dbkey,
                            Issue_Item_Dbkey = IssueItemKey,
                            Engine_Part_Dbkey = item,
                            Part_Name = PartName.Draw_part_no + '/' + PartName.Description
                        });

                    }
                }
                //	_dbContext.SaveChanges();
            }

        }
        private Material_Issue_Item UploadJobCardDocument(IFormFileCollection UploadedFiles, int counter, int Issue_Item_Dbkey, int Issue_Dbkey)
        {
            Material_Issue_Item material_Issue_Item = new Material_Issue_Item();

            try
            {
                if (UploadedFiles != null && UploadedFiles.Count > 0)
                {

                    var UploadedFile = UploadedFiles.FirstOrDefault(x => x.Name == counter.ToString());
                    if (UploadedFile != null)
                    {
                        string systemfilename = string.Empty;
                        string filename = string.Empty;
                        string SavePath = string.Empty;
                        filename = UploadedFile.FileName;
                        systemfilename = Guid.NewGuid().ToString() + Path.GetExtension(UploadedFile.FileName);
                        SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Attachments/MaterialIssue");

                        Regex reg = new Regex("[*'\",_&#^@ ()!`~{};:<>?/+-]");
                        filename = reg.Replace(filename, "_");
                        string originalFileName = filename.Trim();
                        if (!Directory.Exists(SavePath))
                        {
                            Directory.CreateDirectory(SavePath);
                        }

                        SavePath = Path.Combine(SavePath, systemfilename);

                        using (var stream = new FileStream(SavePath, FileMode.Create))
                        {
                            UploadedFile.CopyTo(stream);
                        }

                        material_Issue_Item.JCFileName = originalFileName;
                        material_Issue_Item.JCFileLocation = "/Attachments/MaterialIssue/" + systemfilename;
                        return material_Issue_Item;
                        //MaterialIssueDocument issuedDocument = new MaterialIssueDocument();
                        //issuedDocument.IssueDbKey = Issue_Dbkey;
                        //issuedDocument.IssueItemDbKey = Issue_Item_Dbkey;
                        //issuedDocument.FileLocation = "/Attachments/MaterialIssue/" + systemfilename;
                        //issuedDocument.uploadedOn = DateTime.Now;
                        //issuedDocument.FileName = systemfilename;
                        //issuedDocument.uploadedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                        //_dbContext.MaterialIssueDocuments.Add(issuedDocument);
                        //_dbContext.SaveChanges();
                    }

                }

            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
            return null;
        }


        public IActionResult MaterialIssueDocument(int id, string Type)
        {
            using (_dbContext)
            {
                List<AttachmentVM> attachmentsVM = new();
                ViewBag.IssueDbKey = id;
                ViewBag.Type = Type;
                List<Models.Attachment> attachments = _dbContext.Attachments.Where(x => x.Source_table_key == id && x.Source_table == "Material_Issue_Note").ToList();
                if (attachments.Count != 0)
                {
                    attachmentsVM = JsonConvert.DeserializeObject<List<AttachmentVM>>(JsonConvert.SerializeObject(attachments));
                }
                return View(attachmentsVM);
            }

        }


        [HttpPost]
        public async Task<IActionResult> SaveMaterialIssueDocument([FromForm] UploadViewModel model)
        {
            try
            {
                List<AttachmentVM> attachments = JsonConvert.DeserializeObject<List<AttachmentVM>>(model.filesData);
                MaterialIssueVM materialIssue = new();
                string AttachmentDbKey = "";
                if (materialIssue.Attachment_Db_Key != null)
                {
                    AttachmentDbKey = materialIssue.Attachment_Db_Key;
                }
                int counter = 0;
                foreach (var item in attachments)
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
                        SavePath = GetDestinationFolder() + systemfilename;
                        using (var stream = new FileStream(SavePath, FileMode.Create))
                        {
                            item.uploadeddocument.CopyTo(stream);
                        }
                        att.Attachment_FileName = systemfilename;
                        att.Orginal_File_Name = filename;
                        att.Attachment_location = @"/Attachments/Material_Issue_Note/";
                        att.Attachment_type = "Material_Issue_Doc";
                        att.Source_table_key = item.Source_table_key;
                        att.Source_table = item.Source_table;
                        att.Revision = item.Revision;
                        att.File_Revision = item.File_Revision;
                        att.AttachmentGUID = item.AttachmentGUID;
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

                Material_Issue_Note material_Issue_Note = _dbContext.Material_Issue_Notes.Where(x => x.Issue_Dbkey == attachments.FirstOrDefault().Source_table_key).FirstOrDefault();
                if (material_Issue_Note != null)
                {
                    material_Issue_Note.Attachment_Db_Key = AttachmentDbKey;
                    _dbContext.Material_Issue_Notes.Entry(material_Issue_Note).State = EntityState.Modified;
                    _dbContext.SaveChanges();
                    return Json(new { success = true, msg = "Successfully Saved" });
                }
                else
                {
                    return Json(new { success = false, msg = "Failed to save" });
                }

            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

        }

        private string GetDestinationFolder()
        {
            string directoryname = @"/Attachments/Material_Issue_Note/";
            string SaveDirectory = string.Empty;
            SaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/" + directoryname);
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
            return SaveDirectory + "/";
        }



        [Authorize]
        [ClaimRequirement(UserPermissions.MaterialIssue_Delete)]
        public ActionResult DeleteMaterialIssueDocument(int documentId = 0)
        {
            try
            {
                using (_dbContext)
                {
                    bool validtodelete = true;
                    if (validtodelete)
                    {
                        MPGlobals.ExceSQLNonQuery($"Delete from [dbo].[Attachments] where Attachment_Db_Key ={documentId}");
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false });
                    }
                }
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

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

        [HttpGet]
        [Authorize]
        public ActionResult PrintMaterialIssue(int id)
        {
            DataTable dataTable = MPGlobals.GetDataForDatalist($"dbo.Print_Material_Issue @Issue_Dbkey = {id}");
            PrintMaterialIssueVM printMaterialIssueVM = new PrintMaterialIssueVM();
            printMaterialIssueVM.materialIssues = MPGlobals.ConvertDataTable<MaterialIssue>(dataTable);
            return PartialView(printMaterialIssueVM);
        }

        private static int GetIssueDbKey(Material_Issue_Note item)
        {
            using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
            {
                Material_Issue_Note Updateitem = item.Issue_Dbkey != 0 ? item : db.Material_Issue_Notes.Where(x => x.Order_Ref_No == item.Order_Ref_No && x.Order_Ref_Date == item.Order_Ref_Date).FirstOrDefault();

                if (Updateitem == null)
                {
                    return 0;
                }
                else
                {
                    return Updateitem.Issue_Dbkey;
                }
            }
        }
        private static string GetMaterialRefNo()
        {
            string refno = MPGlobals.GetOnedata("SELECT (isnull(Max([Ref_Number]),0) + 1) FROM  [dbo].[Material_Issue_Note]");
            return refno;
        }

        //public static MaterialIssueVM GetMaterialIssue(int id)
        //{
        //    MaterialIssueVM materialIssueVM = new MaterialIssueVM();
        //    using (DESI_STFE_PRODContext db = new())
        //    {
        //        Material_Issue_Note material_Issue_Note = db.Material_Issue_Notes.Where(x => x.Issue_Dbkey == id).FirstOrDefault();
        //        materialIssueVM = JsonConvert.DeserializeObject<MaterialIssueVM>(JsonConvert.SerializeObject(material_Issue_Note));
        //        String Cmdstr = "select * from Material_Issue_Items where Issue_Dbkey=" + id;
        //        materialIssueVM.material_Issue_Items = MPGlobals.ConvertDataTable<Material_Issue_Items_VM>(MPGlobals.GetDataForDatalist(Cmdstr));
        //        List<Master_Rawmaterial> master_Rawmaterials = db.Master_Rawmaterials.ToList();
        //        materialIssueVM.attachments = db.Attachments.Where(x => x.Source_table_key == id && x.Source_table == "Material_Issue_Note").ToList();
        //        List<Material_Issue_Items_Part> part = db.Material_Issue_Items_Parts.Where(x => x.Issue_Dbkey == id).ToList();

        //        foreach (var item in materialIssueVM.material_Issue_Items)
        //        {
        //            item.Thickness_list = Masters.GetRawMaterial_ParameterList(item.Raw_material_Dbkey, "Thickness", master_Rawmaterials);
        //            item.Outer_Dia_mm_list = Masters.GetRawMaterial_ParameterList(item.Raw_material_Dbkey, "Outer_Dia", master_Rawmaterials);
        //            item.PartNumberKey = part.Where(x => x.Issue_Item_Dbkey == item.Issue_Item_Dbkey).Select(x => x.Engine_Part_Dbkey).ToArray();
        //            item.VendorsList = Masters.GetVendorsList();
        //        }



        //        return materialIssueVM;
        //    }
        //}

        public static MaterialIssueVM GetMaterialIssue(int id)
        {
            MaterialIssueVM materialIssueVM = new MaterialIssueVM();
            using (DESI_STFE_PRODContext db = new())
            {
                Material_Issue_Note material_Issue_Note = db.Material_Issue_Notes
                    .Where(x => x.Issue_Dbkey == id).FirstOrDefault();

                materialIssueVM = JsonConvert.DeserializeObject<MaterialIssueVM>(
                    JsonConvert.SerializeObject(material_Issue_Note));

                // Single SP call — returns items with all display data ready
                DataTable dt = MPGlobals.GetDataForDatalist(
                    $"EXEC [dbo].[Get_MaterialIssue_ForEdit] @Issue_Dbkey = {id}");
                materialIssueVM.material_Issue_Items =
                    MPGlobals.ConvertDataTable<Material_Issue_Items_VM>(dt);

                materialIssueVM.attachments = db.Attachments
                    .Where(x => x.Source_table_key == id && x.Source_table == "Material_Issue_Note")
                    .ToList();

                return materialIssueVM;
            }
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.MaterialIssue_Read)]
        public ActionResult MaterialIssueTree()
        {
            using (var connection = mPDapperContext.CreateConnection())
            {
                var MInote = connection.QueryMultiple($"[dbo].[Material_Issue_Note_Details]");
                List<MaterialIssueNoteDetails> materialIssueNoteDetails = MInote.Read<MaterialIssueNoteDetails>().ToList();
                return View(materialIssueNoteDetails);
            }

        }

        public JsonResult GetMaterialIssueJsTreeData()
        {
            List<MaterialIssueJsTreeViewModel> mplJsTrees = GetMaterialIssueJstreeList();
            return Json(mplJsTrees);
        }

        private List<MaterialIssueJsTreeViewModel> GetMaterialIssueJstreeList()
        {
            List<MaterialIssueJsTreeViewModel> materialIssueJsTreeViewModels = new List<MaterialIssueJsTreeViewModel>();
            List<MasterialIssueTreeViewModel> materialIssueJsTreeViews = new List<MasterialIssueTreeViewModel>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var demandtree = connection.QueryMultiple($"[dbo].[MaterialIssueTreeDetail_SSP]");
                materialIssueJsTreeViews = demandtree.Read<MasterialIssueTreeViewModel>().ToList();
            }
            materialIssueJsTreeViewModels = ContructMaterialIssueJsTreeModel(materialIssueJsTreeViews);
            return materialIssueJsTreeViewModels;
        }

        private List<MaterialIssueJsTreeViewModel> ContructMaterialIssueJsTreeModel(List<MasterialIssueTreeViewModel> materialIssueJsTreeViews)
        {
            List<MaterialIssueJsTreeViewModel> materialIssueJsTreeViewModels = new List<MaterialIssueJsTreeViewModel>();
            MaterialIssueJsTreeViewModel myArray = new MaterialIssueJsTreeViewModel();
            myArray.id = "0" + "_" + "0";
            myArray.text = "Material Issue";
            myArray.icon = "fa fa-fighter-jet";
            myArray.state = new MaterialIssueNodeState();
            myArray.state.opened = true;
            List<MasterialIssueNodeCategory> flatObjects = new List<MasterialIssueNodeCategory>();
            foreach (MasterialIssueTreeViewModel enginePartsViewModel in materialIssueJsTreeViews)
            {
                MasterialIssueNodeCategory category = new MasterialIssueNodeCategory();
                category.id = enginePartsViewModel.id;
                category.text = enginePartsViewModel.RecordType + "-" + enginePartsViewModel.Nodetext;
                category.isactive = 1;
                category.Parent_id = enginePartsViewModel.Parent_id;
                flatObjects.Add(category);
            }

            myArray.children = FillRecursive(flatObjects, "0");
            materialIssueJsTreeViewModels.Add(myArray);
            return materialIssueJsTreeViewModels;
        }

        private List<MaterialIssueJsTreeViewModel> FillRecursive(List<MasterialIssueNodeCategory> flatObjects, string parentId = "0", string id = "0")
        {
            var childrenFlatItems = flatObjects.Where(i => i.Parent_id == parentId);
            return childrenFlatItems.Select(i => new MaterialIssueJsTreeViewModel
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
            if (itemtype == "MaterialIssue")
            {
                icon = "fa fa-cogs small-icon";
            }
            else if (itemtype == "MaterialIssue")
            {
                icon = "fa fa-pie-chart small-icon";
            }
            else if (itemtype == "IssueItems")
            {
                icon = "fa fa-list small-icon";
            }
            else if (itemtype == "Receipts")
            {
                icon = "fa fa-indent small-icon";
            }
            else if (itemtype == "Item")
            {
                icon = "fa fa-arrow-right small-icon";
            }
            else
            {
                icon = "fa fa-cogs";
            }
            return icon;

        }

        private static A_attr_MaterialIssueNode Getattr(string itemtype)
        {
            A_attr_MaterialIssueNode a_Attr = new A_attr_MaterialIssueNode();
            try
            {
                if (itemtype == "MaterialIssue")
                {
                    a_Attr.Class = "jstree-anchor jstree-anchor-MaterialIssue";
                }
                else if (itemtype == "IssueItems")
                {
                    a_Attr.Class = "jstree-anchor jstree-anchor-IssueItems";
                }
                else if (itemtype == "Receipts")
                {
                    a_Attr.Class = "jstree-anchor jstree-anchor-Receipts";
                }
                else if (itemtype == "Item")
                {
                    a_Attr.Class = "jstree-anchor jstree-anchor-Item";
                }
                else if (itemtype == "ForgingReceipt")
                {
                    a_Attr.Class = "jstree-anchor jstree-anchor-ForgingReceipt";
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

        private static MaterialIssueNodeState GetStates(string itemtype)
        {
            MaterialIssueNodeState state = new MaterialIssueNodeState();
            try
            {
                if (itemtype == "MaterialIssue")
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


        public ActionResult IssueDetail()
        {
            return View();
        }

        public ActionResult ViewMaterialIssueDetail(int Id)
        {
            MaterialIssueVM materialIssueVM = new MaterialIssueVM();
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var parts = _dbContext.Material_Issue_Items_Parts.Where(x => x.Issue_Dbkey == Id).GroupBy(y => y.Issue_Item_Dbkey).Select(group => new { PartName = string.Join(',', group.Select(z => z.Part_Name)) }).ToList();

                    var db = connection.QueryMultiple($"dbo.MaterialIssueDetail_SSP @Dbkey = {Id}");
                    materialIssueVM = db.Read<MaterialIssueVM>().First();
                    materialIssueVM.material_Issue_Items = db.Read<Material_Issue_Items_VM>().ToList();
                    var counter = 0;
                    foreach (var item in materialIssueVM.material_Issue_Items)
                    {
                        item.Drawing_no = parts[counter].PartName;
                        counter++;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
            ViewBag.Id = Id;
            return View(materialIssueVM);
        }

        public ActionResult ViewForgingReceipts(int Id)
        {
            MaterialIssueVM materialIssueVM = new MaterialIssueVM();
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var db = connection.QueryMultiple($"dbo.MaterialIssueDetail_SSP @Dbkey = {Id}");
                    materialIssueVM = db.Read<MaterialIssueVM>().First();
                    materialIssueVM.material_Issue_Items = db.Read<Material_Issue_Items_VM>().ToList();
                    materialIssueVM.forgingReceipts = db.Read<ForgingReceipts>().ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
            return View(materialIssueVM);
        }

        public ActionResult SaveForgingReceipt([FromBody] IEnumerable<ForgingReceipts> forgingReceipts)
        {
            using (_dbContext)
            {
                foreach (var item in forgingReceipts)
                {
                    Forging_Receipt forging_Receipt = new Forging_Receipt();
                    Forging_Receipt_Item forging_Receipt_Item = new Forging_Receipt_Item();

                    Material_Issue_Item material_Issue_Items = _dbContext.Material_Issue_Items.Where(x => x.Issue_Item_Dbkey == item.Issue_Item_Dbkey).FirstOrDefault();
                    Material_Issue_Note material_Issue_Note = _dbContext.Material_Issue_Notes.Where(x => x.Issue_Dbkey == material_Issue_Items.Issue_Dbkey).FirstOrDefault();

                    forging_Receipt.forging_recp_dbkey = item.forging_recp_dbkey ?? 0;
                    forging_Receipt.Receipt_Number = item.Receipt_Number;
                    forging_Receipt.Receipt_Date = item.Receipt_Date;
                    forging_Receipt.MMG_File_No = material_Issue_Note.Order_Ref_No;
                    forging_Receipt.Total_Qty = item.Receiving_Inventory;
                    forging_Receipt.Issue_Item_Dbkey = item.Issue_Item_Dbkey;
                    forging_Receipt.Raw_material_Dbkey = material_Issue_Items.Raw_material_Dbkey;
                    forging_Receipt.Vendor_Dbkey = material_Issue_Note.Vendor;
                    forging_Receipt.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    forging_Receipt.Updated_On = DateTime.Now;
                    if (forging_Receipt.forging_recp_dbkey == 0)
                    {
                        _dbContext.Add(forging_Receipt);
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        _dbContext.Entry(forging_Receipt).State = EntityState.Modified; _dbContext.SaveChanges();
                    }


                    forging_Receipt_Item.forging_item_dbkey = item.forging_item_dbkey ?? 0;
                    forging_Receipt_Item.forging_recp_dbkey = forging_Receipt.forging_recp_dbkey;
                    forging_Receipt_Item.Engine_Part_Dbkey = 0;
                    forging_Receipt_Item.GTRE_Drawing_No = material_Issue_Items.Drawing_no.Split("/")[0];
                    forging_Receipt_Item.HAL_Drawing_No = item.HAL_Drawing_No;
                    forging_Receipt_Item.Receiving_Inventory = item.Receiving_Inventory;
                    forging_Receipt_Item.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    forging_Receipt_Item.Updated_On = DateTime.Now;

                    if (forging_Receipt_Item.forging_item_dbkey == 0)
                    {
                        _dbContext.Add(forging_Receipt_Item);
                    }
                    else
                    {
                        _dbContext.Entry(forging_Receipt_Item).State = EntityState.Modified;
                    }
                }
                _dbContext.SaveChanges();
            }
            return Json(new { success = true, msg = "Saved Successfully" });
        }

        public ActionResult ReceiptsDocs(int forging_item_dbkey = 0)
        {
            ViewBag.forging_item_dbkey = forging_item_dbkey;
            return PartialView();
        }

        public ActionResult ReceiptItemSplits(int forging_item_dbkey = 0)
        {
            List<ForgingSplitsVM> Rcdr = new List<ForgingSplitsVM>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.Get_Forging_Items_Split @forging_item_dbkey = {forging_item_dbkey}");
                Rcdr = db.Read<ForgingSplitsVM>().ToList();
            }
            ViewBag.forging_item_dbkey = forging_item_dbkey;
            return PartialView(Rcdr);
        }

        //will continue after splits
        public ActionResult DeleteForgingRcpDocument(int documentId = 0, int receiptDbkey = 0)
        {
            try
            {
                using (_dbContext)
                {
                    bool validtodelete = true;
                    List<Procurement_ReceiptItemSplit> procurement_ReceiptItemSplits = _dbContext.Procurement_ReceiptItemSplits.Where(x => x.Receipt_dbkey == receiptDbkey && x.Attachment_Db_Key != null).ToList();
                    foreach (var item in procurement_ReceiptItemSplits)
                    {
                        string[] attachments = item.Attachment_Db_Key.Split(",");
                        for (int i = 0; i < attachments.Count(); i++)
                        {
                            if (attachments[i].ToString() == documentId.ToString())
                            {
                                validtodelete = false;
                                break;
                            }
                        }
                    }

                    if (validtodelete)
                    {
                        MPGlobals.ExceSQLNonQuery($"Delete from [dbo].[Attachments] where Attachment_Db_Key ={documentId}");
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false });
                    }
                }
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
        }

        public ActionResult SaveForgingItemSplits()
        {
            var procurement_ReceiptItemSplitForm = Request.Form["ForgingSplits"];
            List<ForgingSplitsVM> procurement_ReceiptItemSplitVM = JsonConvert.DeserializeObject<List<ForgingSplitsVM>>(procurement_ReceiptItemSplitForm);
            foreach (var item in procurement_ReceiptItemSplitVM)
            {
                Forging_Split forging_Split = new Forging_Split();
                forging_Split.forging_item_split_dbkey = item.forging_item_split_dbkey;
                forging_Split.forging_item_dbkey = item.forging_item_dbkey;
                forging_Split.part_name = item.part_name;
                forging_Split.GTRE_Drawing_No = item.GTRE_Drawing_No;
                forging_Split.Batch_Number = item.Batch_Number;
                forging_Split.Heat_Number = item.Heat_Number;
                forging_Split.Sl_No_Forging = item.Sl_No_Forging;
                forging_Split.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                forging_Split.Updated_On = DateTime.Now;
                if (item.Attachment_Db_Key_Data != null)
                {
                    var AttachmentDbKey = string.Join(",", item.Attachment_Db_Key_Data);
                    forging_Split.Attachment_Db_Key = AttachmentDbKey;
                }

                if (forging_Split.forging_item_split_dbkey == 0)
                {
                    _dbContext.Add(forging_Split);
                }
                else
                {
                    _dbContext.Entry(forging_Split).State = EntityState.Modified;
                }
            }
            _dbContext.SaveChanges();
            return Json(new { success = true });
        }

        public ActionResult GetSplitMapping(int issueitemkey, int OnlyMappedItem = 0)
        {
            List<MaterialIssueSplitMapping> materialIssueSplitMappings = new List<MaterialIssueSplitMapping>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                string cmdstr = "";
                if (OnlyMappedItem == 0)
                {
                    cmdstr = $"dbo.DR_Splits_For_MaterialIssue_SSP @Issue_Item_Dbkey = {issueitemkey}";
                }
                else
                {
                    cmdstr = $"dbo.DR_Splits_For_MaterialIssue_MappedDetail_SSP @Issue_Item_Dbkey = {issueitemkey}";
                }

                var db = connection.QueryMultiple(cmdstr);
                materialIssueSplitMappings = db.Read<MaterialIssueSplitMapping>().ToList();
                ViewBag.ISdetailsplitInfo = OnlyMappedItem;
                return PartialView(materialIssueSplitMappings);
            }
        }

        public ActionResult SaveMaterialIssueItemSplitMapping()
        {
            try
            {
                var MainTableData = Request.Form["MaterialIssueConsolidation"];
                List<MaterialIssue_DR_SplitMapping> rawmaterial_Consolidations = JsonConvert.DeserializeObject<List<MaterialIssue_DR_SplitMapping>>(MainTableData);

                using (_dbContext)
                {
                    MPGlobals.ExceSQLNonQuery("Delete FROM [dbo].[Material_IssueItems_Consolidation] where [Issue_Item_Dbkey] = " + rawmaterial_Consolidations.FirstOrDefault().Issue_Item_Dbkey);
                    MPGlobals.ExceSQLNonQuery("Delete FROM  [dbo].[MaterialIssue_DR_SplitMapping] where [Issue_Item_Dbkey] = " + rawmaterial_Consolidations.FirstOrDefault().Issue_Item_Dbkey);

                    foreach (MaterialIssue_DR_SplitMapping item in rawmaterial_Consolidations)
                    {
                        item.Updated_by = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                        item.Updated_On = DateTime.Now;
                        item.split_issue_id = 0;
                        _dbContext.MaterialIssue_DR_SplitMappings.Add(item);
                        _dbContext.SaveChanges();
                    }

                    return Json(new { success = true, Msg = "Saved Successfully" });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, Msg = ex.Message });
            }
        }


        [ClaimRequirement(UserPermissions.MaterialIssue_Delete)]
        public ActionResult DeleteForgingSplitItem(int Id = 0)
        {
            MPGlobals.ExceSQLNonQuery($"Delete from [dbo].[Forging_Splits] where forging_item_split_dbkey ={Id}");
            return Json(new { success = true, msg = "Deleted Successfully" });
        }

        public ActionResult DeleteForgingSplitDocument(int Attachment_Db_Key = 0, int forging_item_split_dbkey = 0)
        {
            try
            {
                using (_dbContext)
                {
                    bool validtodelete = true;
                    List<string> stringList = new List<string>();
                    Forging_Split procurement_ReceiptItemSplits = _dbContext.Forging_Splits.Where(x => x.forging_item_split_dbkey == forging_item_split_dbkey && x.Attachment_Db_Key != null).FirstOrDefault();
                    if (procurement_ReceiptItemSplits != null)
                    {
                        string[] attachments = procurement_ReceiptItemSplits.Attachment_Db_Key.Split(",");
                        for (int i = 0; i < attachments.Count(); i++)
                        {
                            if (attachments[i].ToString() != Attachment_Db_Key.ToString())
                            {
                                stringList.Add(attachments[i].ToString());
                            }
                        }
                    }
                    if (validtodelete)
                    {
                        MPGlobals.ExceSQLNonQuery($"Update Forging_Splits set Attachment_Db_Key = '{string.Join(",", stringList)}' where forging_item_split_dbkey ={forging_item_split_dbkey} ");
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false });
                    }
                }
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

        }



        [HttpGet]
        public ActionResult EditForgingSplitRow(int forging_recp_dbkey, int forging_split_dbkey)
        {
            using (_dbContext)
            {

                Forging_Split forging_Split = _dbContext.Forging_Splits.Where(x => x.forging_item_split_dbkey == forging_split_dbkey).FirstOrDefault();
                List<Models.Attachment> attachments = new List<Models.Attachment>();
                if (forging_Split != null && forging_split_dbkey != 0)
                {
                    if (!string.IsNullOrEmpty(forging_Split.Attachment_Db_Key))
                    {
                        attachments = _dbContext.Attachments.Where(x => x.Source_table == "Forging_Receipt_Items" && x.Source_table_key == forging_split_dbkey).ToList();
                        attachments = _dbContext.Attachments.Where(x => x.Source_table == "Forging_Receipt_Items" && x.Source_table_key == forging_Split.forging_item_dbkey).ToList();  // Foring Item DB key is used in Source_table_key
                        var attachmentKeys = forging_Split.Attachment_Db_Key.Split(",").ToList();
                        List<int> intList = attachmentKeys.Select(s => int.Parse(s)).ToList();
                        attachments = attachments.Where(x => intList.Contains(x.Attachment_Db_Key)).ToList();
                    }

                }
                if (forging_split_dbkey == 0)
                {
                    Forging_Receipt_Item forging_Receipt_Item = _dbContext.Forging_Receipt_Items.Where(x => x.forging_recp_dbkey == forging_recp_dbkey).FirstOrDefault();
                    Forging_Receipt forging_Receipt = _dbContext.Forging_Receipts.Where(x => x.forging_recp_dbkey == forging_recp_dbkey).FirstOrDefault();
                    Material_Issue_Item material_Issue_Item = _dbContext.Material_Issue_Items.Where(x => x.Issue_Item_Dbkey == forging_Receipt.Issue_Item_Dbkey).FirstOrDefault();
                    Forging_Split forging_Split1 = new();
                    forging_Split1.Attachment_Db_Key = "";
                    forging_Split1.forging_item_split_dbkey = 0;
                    forging_Split1.forging_item_dbkey = forging_Receipt_Item.forging_item_dbkey;
                    forging_Split1.Heat_Number = "";
                    forging_Split1.part_name = material_Issue_Item.Drawing_no;
                    forging_Split1.GTRE_Drawing_No = forging_Receipt_Item.GTRE_Drawing_No;
                    forging_Split1.Batch_Number = "";
                    forging_Split = forging_Split1;
                }
                ViewBag.forging_recp_dbkey = forging_recp_dbkey;
                var myTuple = new Tuple<Forging_Split, List<Models.Attachment>>(forging_Split, attachments);
                return PartialView(myTuple);

            }
        }


        [Authorize]
        [ClaimRequirement(UserPermissions.MaterialIssue_ForgingReceipts)]
        public IActionResult SaveForgingReceiptSplitModel([FromForm] UploadViewModel model)
        {
            try
            {
                List<AttachmentVM> attach = JsonConvert.DeserializeObject<List<AttachmentVM>>(model.filesData);
                ForgingSplitsVM splitData = JsonConvert.DeserializeObject<ForgingSplitsVM>(model.JsonData);
                Forging_Split forging_Split = new();

                string AttachmentDbKey = "";
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
                        att.Attachment_location = @"/Attachments/Forging_Receipt_Docs/";
                        att.Attachment_type = item.Attachment_type;
                        att.Source_table_key = item.Source_table_key;
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


                forging_Split.forging_item_split_dbkey = splitData.forging_item_split_dbkey;
                forging_Split.forging_item_dbkey = splitData.forging_item_dbkey;
                forging_Split.part_name = splitData.part_name;
                forging_Split.GTRE_Drawing_No = splitData.GTRE_Drawing_No;
                forging_Split.Batch_Number = splitData.Batch_Number;
                forging_Split.Heat_Number = splitData.Heat_Number;
                forging_Split.Sl_No_Forging = splitData.Sl_No_Forging;
                forging_Split.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                forging_Split.Updated_On = DateTime.Now;
                forging_Split.Attachment_Db_Key = AttachmentDbKey;

                if (forging_Split.forging_item_split_dbkey != 0)
                {
                    _dbContext.Forging_Splits.Entry(forging_Split).State = EntityState.Modified;
                }
                else
                {
                    _dbContext.Add(forging_Split);
                }

                _dbContext.SaveChanges();
                return Json(new { success = true, msg = "Successfully Saved" });
            }
            catch (Exception ex)
            {

                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }

        }



        private string GetDestinationFolder(string SourceFileType, string Attachment_type)
        {
            string directoryname = @"/Attachments/Forging_Receipt_Docs/";
            string SaveDirectory = string.Empty;
            SaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/" + directoryname);
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
            return SaveDirectory + "/";
        }


        [Authorize]
        public ActionResult UpdateForgeReceiptDocType(int ForgeID = 0, int doctype = 0)
        {
            try
            {
                using (_dbContext)
                {
                    MPGlobals.ExceSQLNonQuery($"Update Attachments set File_DVD_Num = {doctype} where  Attachment_Db_Key = {ForgeID}");
                    return Json(new { success = true });
                }
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

        }


        public IActionResult ViewForgingReceiptsDocuments(int IssueDbkey = 0)
        {
            List<ForgingReceiptDocumentsVM> docInfo = new List<ForgingReceiptDocumentsVM>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var docData = connection.QueryMultiple($"[dbo].ForgingReceiptDocuments_SSP @IssueDbkey = {IssueDbkey}");
                docInfo = docData.Read<ForgingReceiptDocumentsVM>().ToList();
            }
            return View(docInfo);
        }


        [Authorize]
        public IActionResult ForgingReceiptSummary()
        {
            return View();
        }


        [Authorize]
        [HttpGet]
        public ActionResult GetReceiptsHistory()
        {
            DataTable dt = MPGlobals.GetDataForDatalist("dbo.Get_Forging_Receipts");
            return Json(MPGlobals.GetTableAsList(dt));
        }

        [Authorize]
        [ClaimRequirement(UserPermissions.MaterialIssue_Delete)]
        public ActionResult DeleteMaterialIssue(int IssueDbkey = 0)
        {
            try
            {
                using (_dbContext)
                {
                    MPGlobals.ExceSQLNonQuery($"DELETE FROM [dbo].[Material_Issue_Items] WHERE [Issue_Dbkey] = {IssueDbkey} ");
                    MPGlobals.ExceSQLNonQuery($"DELETE FROM [dbo].[Material_Issue_Note] WHERE [Issue_Dbkey] = {IssueDbkey} ");

                    return Json(new { success = true });
                }
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
        }

        //[HttpGet]
        //public IActionResult VendorRawMaterialDetails_Partial(string searchText = "")
        //{
        //    ViewBag.SearchText = searchText;
        //   // return PartialView("_VendorRawMaterialDetails");
        //}

        public IActionResult BATL_IssuesSummary()
        {
            List<BATL_IssueSummary> bATL_Issues = new List<BATL_IssueSummary>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var docData = connection.QueryMultiple($"[dbo].BATL_IssueSummary ");
                bATL_Issues = docData.Read<BATL_IssueSummary>().ToList();
            }
            return View(bATL_Issues);

        }

        [HttpGet]
        [Authorize]
        public ActionResult SearchParts(string term = "")
        {
            using (var connection = mPDapperContext.CreateConnection())
            {
                var parts = connection.Query<dynamic>(
                    "[dbo].[Search_Parts_For_MaterialIssue]",
                    new { SearchTerm = term },
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();

                return Json(parts);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult GetPartsByKeys([FromBody] int[] keys)
        {
            if (keys == null || keys.Length == 0)
                return Json(new List<object>());

            using (var connection = mPDapperContext.CreateConnection())
            {
                string sql = @"SELECT Engine_Part_Dbkey AS [value],
                              Draw_part_no + ' / ' + ISNULL([Description], '') AS [text]
                       FROM [dbo].[Engine_Parts_Master]
                       WHERE Engine_Part_Dbkey IN @Keys";

                var parts = connection.Query<dynamic>(sql, new { Keys = keys }).ToList();
                return Json(parts);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetPartSmartData(int partDbKey)
        {
            using (var connection = mPDapperContext.CreateConnection())
            {
                var result = connection.QueryMultiple(
                    "[dbo].[Get_PartSmartData_For_MaterialIssue]",
                    new { Engine_Part_Dbkey = partDbKey },
                    commandType: System.Data.CommandType.StoredProcedure
                );

                var partData = result.Read<dynamic>().FirstOrDefault();
                var vendors = result.Read<dynamic>().ToList();

                return Json(new
                {
                    part = partData,
                    vendors = vendors,
                    rmQtyPerPart = (double?)partData?.Quantity,        // pieces per part (e.g. 10 bars, 2 sheets)
                    rmWeightPerPart = (double?)partData?.weight_in_kg, // weight in kg per part
                    qtyPerEngine = (int?)partData?.Qty_per_Engine      // parts needed per engine
                });
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult SearchRawMaterials(string term = "")
        {
            using (var connection = mPDapperContext.CreateConnection())
            {
                string sql = @"SELECT TOP 20 
                          MR.Raw_material_Dbkey AS [value],
                          Raw_material_Name AS [text]
                       FROM [dbo].[Master_Rawmaterials] MR 
                       WHERE  
                         MR.is_active = 1
                         AND (@Term = '' 
                              OR MR.Material_name LIKE '%' + @Term + '%'
                              OR MR.Raw_material_Name LIKE '%' + @Term + '%')
                       ORDER BY MR.Material_name";

                var materials = connection.Query<dynamic>(sql, new { Term = term }).ToList();
                return Json(materials);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult SearchVendors(string term = "", int rawMaterialDbKey = 0)
        {
            using (var connection = mPDapperContext.CreateConnection())
            {
                string sql;

                // No RM filter — search all vendors
                sql = @"SELECT TOP 20
                        Vendor_Dbkey AS [value],
                        Vendor_Name AS [text]
                    FROM [dbo].[Vendors]
                    WHERE   (@Term = '' OR Vendor_Name LIKE '%' + @Term + '%')
                    ORDER BY Vendor_Name";

                var vendors = connection.Query<dynamic>(sql, new { Term = term, RmKey = rawMaterialDbKey }).ToList();
                return Json(vendors);
            }
        }

        [HttpPost]
        [Authorize]
        [ClaimRequirement(UserPermissions.MaterialIssue_Write)]
        public ActionResult SaveAllNewItems(List<Material_Issue_Items_VM> items)
        {
            try
            {
                int loggedInUser = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var results = new List<object>();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        string partKeys = item.PartKeys ?? "";

                        // Handle file upload if present (keyed as jobCardFile_0, jobCardFile_1, etc.)
                        var fileKey = "jobCardFile_" + i;
                        var jobCardFile = Request.Form.Files[fileKey];
                        if (jobCardFile != null && jobCardFile.Length > 0)
                        {
                            string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Attachments/MaterialIssue");
                            if (!Directory.Exists(savePath))
                                Directory.CreateDirectory(savePath);

                            string originalFileName = Regex.Replace(jobCardFile.FileName.Trim(), "[*'\",_&#^@ ()!`~{};:<>?/+-]", "_");
                            string systemFileName = Guid.NewGuid().ToString() + Path.GetExtension(jobCardFile.FileName);
                            string fullPath = Path.Combine(savePath, systemFileName);

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                jobCardFile.CopyTo(stream);
                            }

                            item.JCFileName = originalFileName;
                            item.JCFileLocation = "/Attachments/MaterialIssue/" + systemFileName;
                        }

                        var result = connection.QueryFirstOrDefault<dynamic>(
                            "[dbo].[Save_MaterialIssue_SingleItem]",
                            new
                            {
                                Issue_Dbkey = item.Issue_Dbkey,
                                Issue_Item_Dbkey = item.Issue_Item_Dbkey ?? 0,
                                Raw_material_Dbkey = item.Raw_material_Dbkey ?? 0,
                                Qty = item.Qty ?? 0,
                                Size = item.Size,
                                Qty_Issue = item.Qty_Issue ?? 0,
                                Heat_No = item.Heat_No,
                                EngineLevel = item.EngineLevel,   // NEW
                                Weight_Kg = item.Weight_Kg,
                                Amount = item.Amount ?? 0,
                                SerialNo = item.SerialNo,
                                Vendor_Dbkey = item.Vendor_Dbkey,
                                JobCardNumber = item.JobCardNumber,
                                JCFileName = item.JCFileName,
                                JCFileLocation = item.JCFileLocation,
                                PartKeys = partKeys,
                                Updated_By = loggedInUser,
                                PartQty_EngineWise = item.PartQty_EngineWise
                            },
                            commandType: System.Data.CommandType.StoredProcedure
                        );

                        results.Add(new { success = true, data = result });
                    }
                }

                return Json(new { success = true, results = results });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        [ClaimRequirement(UserPermissions.MaterialIssue_Write)]
        public ActionResult SaveSingleItem([FromForm] Material_Issue_Items_VM item, IFormFile jobCardFile)
        {
            try
            {
                int loggedInUser = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                string partKeys = item.PartKeys ?? "";

                // Handle file upload if present
                if (jobCardFile != null && jobCardFile.Length > 0)
                {
                    string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Attachments/MaterialIssue");
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }

                    string originalFileName = Regex.Replace(jobCardFile.FileName.Trim(), "[*'\",_&#^@ ()!`~{};:<>?/+-]", "_");
                    string systemFileName = Guid.NewGuid().ToString() + Path.GetExtension(jobCardFile.FileName);
                    string fullPath = Path.Combine(savePath, systemFileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        jobCardFile.CopyTo(stream);
                    }

                    item.JCFileName = originalFileName;
                    item.JCFileLocation = "/Attachments/MaterialIssue/" + systemFileName;
                }

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = connection.QueryFirstOrDefault<dynamic>(
                        "[dbo].[Save_MaterialIssue_SingleItem]",
                        new
                        {
                            Issue_Dbkey = item.Issue_Dbkey,
                            Issue_Item_Dbkey = item.Issue_Item_Dbkey ?? 0,
                            Raw_material_Dbkey = item.Raw_material_Dbkey ?? 0,
                            Qty = item.Qty ?? 0,
                            Size = item.Size,
                            Qty_Issue = item.Qty_Issue ?? 0,
                            Heat_No = item.Heat_No,
                            EngineLevel = item.EngineLevel,
                            Weight_Kg = item.Weight_Kg,
                            Amount = item.Amount ?? 0,
                            SerialNo = item.SerialNo,
                            Vendor_Dbkey = item.Vendor_Dbkey,
                            JobCardNumber = item.JobCardNumber,
                            JCFileName = item.JCFileName,
                            JCFileLocation = item.JCFileLocation,
                            PartKeys = partKeys,
                            Updated_By = loggedInUser,
                            PartQty_EngineWise = item.PartQty_EngineWise
                        },
                        commandType: System.Data.CommandType.StoredProcedure
                    );

                    return Json(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        [ClaimRequirement(UserPermissions.MaterialIssue_Write)]
        public ActionResult SaveHeader([FromBody] MaterialIssueVM header)
        {
            try
            {
                int loggedInUser = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = connection.QueryFirstOrDefault<dynamic>(
                        "[dbo].[Save_MaterialIssue_Header]",
                        new
                        {
                            Issue_Dbkey = header.Issue_Dbkey,
                            Engine_Name = header.Engine_Name,
                            DemandDbKey = header.DemandDbKey,
                            Demand_No = header.Demand_No,
                            Order_Ref_No = header.Order_Ref_No,
                            Order_Ref_Date = header.Order_Ref_Date,
                            Vendor = header.Vendor,
                            Returnable = header.Returnable,
                            Issue_Purpose = header.Issue_Purpose,
                            Updated_By = loggedInUser
                        },
                        commandType: System.Data.CommandType.StoredProcedure
                    );

                    return Json(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        [ClaimRequirement(UserPermissions.MaterialIssue_Delete)]
        public ActionResult DeleteSingleItem([FromBody] Material_Issue_Items_VM request)
        {
            try
            {
                int issueItemDbkey = request.Issue_Item_Dbkey ?? 0;

                if (issueItemDbkey <= 0)
                {
                    return Json(new { success = false, msg = "Invalid Issue_Item_Dbkey" });
                }

                using (var connection = mPDapperContext.CreateConnection())
                {
                    connection.Execute(
                        "DELETE FROM [dbo].[Material_Issue_Items_Parts] WHERE Issue_Item_Dbkey = @Key",
                        new { Key = issueItemDbkey });

                    connection.Execute(
                        "DELETE FROM [dbo].[Material_Issue_Items] WHERE Issue_Item_Dbkey = @Key",
                        new { Key = issueItemDbkey });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }
    }
}