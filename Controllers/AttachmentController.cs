using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Security.Claims;
using Attachment = MPCRS.Models.Attachment;

namespace MPCRS.Controllers
{
    public class AttachmentController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AttachmentController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult ViewAttachments(string Source_table = "", int Source_table_key = 0, string ViewFor = "MPL")
        {
            ViewBag.ViewFor = ViewFor;
            ViewBag.Source_table = Source_table;
            ViewBag.Source_table_key = Source_table_key;
            return PartialView();
        }

        public IActionResult DownloadfileWithKey(int id = 0)
        {
            try
            {
                using (_dbContext)
                {
                    string wwwrootPath = _webHostEnvironment.WebRootPath;
                    Attachment attachment = _dbContext.Attachments.Where(x => x.Attachment_Db_Key == id).FirstOrDefault();
                    string filePath = wwwrootPath + attachment.Attachment_location + attachment.Attachment_FileName;
                    var memory = new MemoryStream();
                    using (var stream = new FileStream(filePath, FileMode.Open))
                    {
                        stream.CopyTo(memory);
                    }
                    memory.Position = 0;
                    return File(memory, System.Net.Mime.MediaTypeNames.Application.Octet, attachment.Orginal_File_Name);
                }
                
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                throw;
            }
        }


        public ActionResult GetAttachments(int itemKey, string sourceTableName, bool deleteAccess = true)
        {
            attachmentData attData = new attachmentData();
            attData.sourceAttachTable = sourceTableName;
            attData.sourceAttachTableKey = itemKey;
            attData.deleteAccess = deleteAccess;
            return PartialView(attData);
        }

        public IActionResult DeleteAttachment(int id)
        {
            try
            {            
                MPGlobals.ExceSQLNonQuery($"Delete from [dbo].[Attachments] where [Attachment_Db_Key] = {id}");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
            return Json(new { success = true, msg = "File deleted successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(AttachmentVM attachmentVM)
        {
            using (_dbContext)
            {
                Models.Attachment att = new();
                var userguid = User.Identity.Name;
                string systemfilename = string.Empty;
                string filename = string.Empty;
                string SavePath = string.Empty;
                // string SaveDirectory = string.Empty;
                attachmentVM.AttachmentGUID = Guid.NewGuid().ToString();
                if (attachmentVM.uploadeddocument != null)
                {
                    filename = attachmentVM.uploadeddocument.FileName;
                    systemfilename = attachmentVM.AttachmentGUID + Path.GetExtension(attachmentVM.uploadeddocument.FileName);
                    SavePath = GetDestinationFolder(attachmentVM.Source_table, attachmentVM.Attachment_type) + systemfilename;
                    using (var stream = new FileStream(SavePath, FileMode.Create))
                    {
                        await attachmentVM.uploadeddocument.CopyToAsync(stream);
                    }
                    att.Attachment_FileName = systemfilename;
                    att.Orginal_File_Name = filename;
                    att.Attachment_location = GetDestinationPath(attachmentVM.Source_table, attachmentVM.Attachment_type);
                    att.Attachment_type = attachmentVM.Attachment_type;
                    att.Source_table_key = attachmentVM.Source_table_key;
                    att.Source_table = attachmentVM.Source_table;
                    att.Revision = attachmentVM.Revision;
                    att.File_Revision = attachmentVM.File_Revision;
                    att.AttachmentGUID = attachmentVM.AttachmentGUID;
                    att.Attachment_type = attachmentVM.Attachment_type;
                    att.File_DVD_Num = attachmentVM.File_DVD_Num;
                    att.Updated_on = DateTime.Now;
                    att.Updated_by = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    if (attachmentVM.Source_table == "Engine_Parts_Master")
                    {
                        att.Approved_status = false;
                    }
                    _dbContext.Attachments.Add(att);
                    await _dbContext.SaveChangesAsync();

                    if (attachmentVM.Source_table == "Engine_Parts_Master")
                    {
                        UpdateEnginePartMaster(att, attachmentVM.Approver);
                        Engine_Parts_Master engine_Parts_Master = _dbContext.Engine_Parts_Masters.Where(x => x.Engine_Part_Dbkey == att.Source_table_key).FirstOrDefault();
                        engine_Parts_Master.Updated_By = att.Updated_by;
                        engine_Parts_Master.Drawing_File = attachmentVM.Attachment_type;
                        Task.Run(() => MPCRS.Utilities.Notification.TriggerMPLApprovalMail(engine_Parts_Master));
                    }
                }
                return Json(new { success = true, attachment = att.Attachment_Db_Key });
            }
        }

        private void UpdateEnginePartMaster(Attachment att, string approverID)
        {
            string Cmdtsr = "";
            if (att.Attachment_type == "Drawing_File")
            {
                Cmdtsr = $"Update [dbo].[Engine_Parts_Master] set [Drawing_File_ID] = {att.Attachment_Db_Key} ,[Updated_On] = GETDATE(),[Updated_By] = {att.Updated_by},[Record_Status] = 'Drawing_File', [Approver_ID] = '{approverID}' where [Engine_Part_Dbkey] = {att.Source_table_key}";
            }
            else if (att.Attachment_type == "2D_Model_Drawing_File")
            {
                Cmdtsr = $"Update [dbo].[Engine_Parts_Master] set Drawing_File_2Dmodel_ID = {att.Attachment_Db_Key} ,[Updated_On] = GETDATE() ,[Updated_By] = {att.Updated_by},[Record_Status] = '2D_Model_Drawing_File', [Approver_ID] = '{approverID}' where [Engine_Part_Dbkey] = {att.Source_table_key}";
            }
            else if (att.Attachment_type == "3D_Model_Drawing_File")
            {
                Cmdtsr = $"Update [dbo].[Engine_Parts_Master] set [Drawing_File_3Dmodel_ID] = {att.Attachment_Db_Key} ,[Updated_On] = GETDATE() ,[Updated_By] = {att.Updated_by},[Record_Status] = '3D_Model_Drawing_File', [Approver_ID] = '{approverID}' where [Engine_Part_Dbkey] = {att.Source_table_key}";
            }
            else if (att.Attachment_type == "ACSN")
            {
                Cmdtsr = $"Update [dbo].[Engine_Parts_Master] set [Drawing_File_ACSN_ID] = {att.Attachment_Db_Key} ,[Updated_On] = GETDATE() ,[Updated_By] = {att.Updated_by} ,[Record_Status] = 'ACSN', [Approver_ID] = '{approverID}' where [Engine_Part_Dbkey] = {att.Source_table_key}";
            }
            else if (att.Attachment_type == "Casting_Forging_File")
            {
                Cmdtsr = $"Update [dbo].[Engine_Parts_Master] set [Drawing_File_Casting_Forging_ID] = {att.Attachment_Db_Key} ,[Updated_On] = GETDATE() ,[Updated_By] = {att.Updated_by} ,[Record_Status] = 'Casting_Forging_File', [Approver_ID] = '{approverID}' where [Engine_Part_Dbkey] = {att.Source_table_key}";
            }
            MPGlobals.ExceSQLNonQuery(Cmdtsr);



        }

        private string GetDestinationFolder(string SourceFileType,string Attachment_type)
        {
            string directoryname = GetDestinationPath(SourceFileType, Attachment_type);
            string SaveDirectory = string.Empty;
            SaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/" + directoryname);
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
            return SaveDirectory + "/";
        }

        private string GetDestinationPath(string SourceFileType, string Attachment_type)
        {
            string directoryname = string.Empty;
            if (SourceFileType == "ACSNItems")
            {
                directoryname = @"/Attachments/acsn/";
            }
            else if (SourceFileType == "EngineBuildComponents")
            {
                directoryname = @"/Attachments/EngineBuildComponents/";
            }
            else if (SourceFileType == "NonConformanceReport")
            {
                directoryname = @"/Attachments/NonConformanceReport/";
            }else if (SourceFileType == "ManufacturingStatus")
            {
                directoryname = @"/Attachments/ManufacturingStatus/";
            }
            else if(SourceFileType == "Procurement_Demand_Receipts")
            {
                if (Attachment_type == "Deamands_Receipt_Docs")
                {
                    directoryname = @"/Attachments/Deamands_Receipt_Docs/";
                }
            }else if (SourceFileType == "Forging_Receipt_Docs")
            {
                directoryname = @"/Attachments/Forging_Receipt_Docs/";
            }
            else if (SourceFileType == "Engine_Parts_Master")
            {
                if (Attachment_type == "Drawing_File")
                {
                    directoryname = @"/Attachments/Drawing/";
                }else if (Attachment_type == "2D_Model_Drawing_File")
                {
                    directoryname = @"/Attachments/2D/";
                }else if (Attachment_type == "3D_Model_Drawing_File")
                {
                    directoryname = @"/Attachments/3D/";
                }
                else if (Attachment_type == "Casting_Forging_File")
                {
                    directoryname = @"/Attachments/Casting_Forging_File/";
                }
            }

            return directoryname;
        }


        public ActionResult GetAttachmentsForSOP(int itemKey)
        {
            List<Models.Attachment> attData = new List<Models.Attachment>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"[dbo].[AttachmentsForSopReport_SSP] @SourceTableKey={itemKey}");
                attData = db.Read<Models.Attachment>().ToList();
            }
            return PartialView(attData);
        }


        public ActionResult GetMPLPartDocuments(int itemKey,string AllRevs = "false")
        {
            List<Models.Attachment> attData = new List<Models.Attachment>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"[dbo].[AttachmentsForSopReport_SSP] @SourceTableKey={itemKey},@AllRevs='{AllRevs}'");
                attData = db.Read<Models.Attachment>().ToList();
            }
            return PartialView(attData);
        }


        [Authorize]
        public async Task<IActionResult> UploadExcel(AttachmentVM attachmentVM)
        {
            try
            {
                DTOResponse dTOResponse = new DTOResponse();
                string filePath = string.Empty;
                string FileName = string.Empty;
                if (attachmentVM.uploadeddocument != null)
                {
                    string path = GetDestinationFolder("ManufacturingStatus","");// Server.MapPath("~/Upload_Excel_File/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    FileName = attachmentVM.uploadeddocument.FileName;
                    filePath = path + Path.GetFileName(attachmentVM.uploadeddocument.FileName);
                    string extension = Path.GetExtension(attachmentVM.uploadeddocument.FileName);
                   // attachmentVM.uploadeddocument.SaveAs(filePath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await attachmentVM.uploadeddocument.CopyToAsync(stream);
                    }

                    DataTable dataTable = ExcelFileHelper.SaveAsDatatable(filePath);
                     if (attachmentVM.Source_table == "JsonVendorComponentDetail")
                    {
                        dTOResponse = ProccessExcel_JsonVendorComponentDetail(dataTable);
                    }
                   
                    return Json(new { success = dTOResponse.Result, Msg = dTOResponse.ResponseMessage });
                }
                else
                {
                    return Json(new { success = false, Msg = "Invalid Excel File" });
                }
            }
            catch (Exception ex)
            {
                // Logger.WriteToFile(ex.Message);
                // Logger.WriteToFile(ex.StackTrace);
                return Json(new { success = false, Msg = ex.Message }); 
            } 
        }

        private DTOResponse ProccessExcel_JsonVendorComponentDetail(DataTable dataTable)
        {
            DTOResponse dTOResponse = new DTOResponse();
            int rowscount = dataTable.Rows.Count;
            if (rowscount > 0)
            {
                using (_dbContext)
                {
                    //DESI_STFE.DTO.Security.UserManagement.userdata ud = MPGlobals.GetUserDataModel();
                    //var userguid = User.Identity.Name;

                    for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                    {
                        dataTable.Columns[i].ColumnName = dataTable.Columns[i].ColumnName.Replace("\n", "").Trim(); 
                    }
                    dataTable.AcceptChanges();
                    MPGlobals.ExceSQLNonQuery($"UPDATE  [dbo].[AuditLogDisplayManager] SET [DisplayOrder] = 0 WHERE [SourceTable] = 'ExternalMfgStatus' ");
                    AuditLogDisplayManager auditLogDisplayManager = new AuditLogDisplayManager();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        auditLogDisplayManager = _dbContext.AuditLogDisplayManagers.Where(x => x.SourceTable == "ExternalMfgStatus" && x.ColumnName == column.ColumnName.Trim()).FirstOrDefault();
                        if (auditLogDisplayManager == null)
                        {
                            if (!string.IsNullOrEmpty(column.ColumnName.Trim()))
                            {
                                auditLogDisplayManager = new AuditLogDisplayManager();
                                auditLogDisplayManager.SourceTable = "ExternalMfgStatus";
                                auditLogDisplayManager.ColumnName = column.ColumnName.Trim();
                                auditLogDisplayManager.Display_ColumnName = column.ColumnName.Trim();
                                auditLogDisplayManager.DisplayData = false;
                                auditLogDisplayManager.Force_Display_Data = false;
                                auditLogDisplayManager.DataType = "varchar(max)";
                                auditLogDisplayManager.DisplayOrder = 1;
                                _dbContext.AuditLogDisplayManagers.Add(auditLogDisplayManager);
                                _dbContext.SaveChanges();
                            }
                        }
                        else
                        {
                            auditLogDisplayManager.DisplayOrder = 1;
                            _dbContext.Entry(auditLogDisplayManager).State = EntityState.Modified;
                            _dbContext.SaveChanges();
                        }
                    }

                    string Jsonstring = JsonConvert.SerializeObject(dataTable);
                    ExternalMfgStatus externalMfgStatu = new ExternalMfgStatus();
                    externalMfgStatu.Json = Jsonstring;
                    externalMfgStatu.UpdatedOn = DateTime.Now;
                   // externalMfgStatu.UploadedBy = userguid;
                    _dbContext.ExternalMfgStatuses.Add(externalMfgStatu);
                    _dbContext.SaveChanges();
                    dTOResponse.Result = true;
                    dTOResponse.ResponseMessage = "Uploaded Successfully";
                }

            }
            else
            {
                dTOResponse.Result = false;
                dTOResponse.ResponseMessage = "Invalid Excel file";
            }

            return dTOResponse;
        }
    }
}
