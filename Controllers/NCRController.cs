using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.ML.Trainers;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using XAct;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Controllers
{
    [Authorize]
    public class NCRController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;
        private readonly string _shramLink;
        private readonly string _bccList;
        public NCRController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            _shramLink = configuration.GetValue<string>("NCRSettings:SHRAMLink") ?? string.Empty;
            _bccList = configuration.GetValue<string>("NCRSettings:BCCList") ?? string.Empty;
            this.mPDapperContext = mPDapperContext;
        }
        #region NCR
        [ClaimRequirement(UserPermissions.NCR_Read)]
        public ActionResult Index()
        {
            return View();
        }
            
        [ClaimRequirement(UserPermissions.NCR_Read)]
        public string GetNCRdatalist(string getLog = "No")
        {
            string Cmdstr = $"dbo.GetNCRData @GetLog ='{getLog}'";
            DataTable dataTable = MPGlobals.GetDataForDatalist(Cmdstr);
            return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
        }


        [ClaimRequirement(UserPermissions.NCR_Read)]
        public ActionResult NCRLog()
        {
            return View();
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.NCR_Write)]
        public IActionResult NCR(string NCRGuid)
        {
            NCRvm viewmodel = new();
            viewmodel.nonConformanceReport_Items = new();
            viewmodel.nonConformanceReportVM = new();
            if (!string.IsNullOrEmpty(NCRGuid))
            {
                using (_dbContext)
                {
                    NonConformanceReport nonConformanceReport = _dbContext.NonConformanceReports.Where(x => x.NCRGuid == NCRGuid).FirstOrDefault();
                    if (nonConformanceReport != null)
                    {
                        viewmodel.nonConformanceReportVM = JsonConvert.DeserializeObject<NonConformanceReportVM>(JsonConvert.SerializeObject(nonConformanceReport));
                        DataTable dt = MPGlobals.GetDataForDatatable($" [dbo].[NCR_SerialNo_status_Data_SSP] @NCRGuid ='{NCRGuid}',@assignmentID ='all'");
                        viewmodel.nonConformanceReport_Items = JsonConvert.DeserializeObject<List<NonConformanceReport_ItemVM>>(JsonConvert.SerializeObject(dt));
                        viewmodel.nonConformanceReport_Items = viewmodel.nonConformanceReport_Items == null ? new() : viewmodel.nonConformanceReport_Items;
                        viewmodel.nonConformanceReportVM.Module_Responsibilty_String = _dbContext.Master_Generals.Where(x => x.Master_Type == "Module_Responsibility" && x.is_active == 1 && x.Master_Dbkey == nonConformanceReport.Module_Responsibilty).Select(x => x.Master_Name).FirstOrDefault();
                    }
                }
            }
            viewmodel.nonConformanceReportVM.Module_ResponsibilityList = MPCRS.Utilities.Masters.GetMaster_General("Module_Responsibility");

            return View(viewmodel);
        }

        [HttpPost]
        [ClaimRequirement(UserPermissions.NCR_Write)]
        public IActionResult NCR([FromBody] NCRvm viewmodel)
        {

            //var tr = viewmodel?.nonConformanceReportVM?.ECM_TR_NO;
            //var no = viewmodel?.nonConformanceReportVM?.ECM_No;

            using (_dbContext)
            {
                try
                {
                    var loggedInUser = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    NonConformanceReport ncr = JsonConvert.DeserializeObject<NonConformanceReport>(JsonConvert.SerializeObject(viewmodel.nonConformanceReportVM));


                    if (ncr.Id == 0)
                    {
                        // Normalize the incoming reference number 
                        var refNo = (ncr.ReferenceNumber ?? string.Empty).Trim();

                        // Check if any existing NCR has the same (trimmed) reference number
                        bool referenceExists = _dbContext.NonConformanceReports
                            .Any(x => x.ReferenceNumber != null &&
                                      x.ReferenceNumber.Trim().ToUpper() == refNo.ToUpper());
                       
                        if (referenceExists)
                        {
                            return Json(new { success = false, msg = "NCR No already exists" });
                        }
                        ncr.NCRGuid = Guid.NewGuid().ToString();
                        _dbContext.Add(ncr);
                    }
                    else
                    {
                        NonConformanceReport nonConformanceReport = new NonConformanceReport();
                        nonConformanceReport = _dbContext.NonConformanceReports.Where(x => x.NCRGuid == ncr.NCRGuid).FirstOrDefault();

                        if (nonConformanceReport != null)
                        {
                            nonConformanceReport.Part_relation_dbkey = ncr.Part_relation_dbkey;
                            nonConformanceReport.ReferenceNumber = ncr.ReferenceNumber;
                            nonConformanceReport.ReceivedDate = ncr.ReceivedDate;
                            nonConformanceReport.ReceivedFrom = ncr.ReceivedFrom;
                            nonConformanceReport.ComitteeReferred = ncr.ComitteeReferred;
                            nonConformanceReport.ReportStatus = ncr.ReportStatus;
                            nonConformanceReport.Remarks = ncr.Remarks;
                            nonConformanceReport.UpdatedBy = loggedInUser;
                            nonConformanceReport.UpdatedOn = DateTime.Now;
                            nonConformanceReport.FileLocation = ncr.FileLocation;
                            nonConformanceReport.OrignalFileName = ncr.OrignalFileName;
                            nonConformanceReport.SystemFileName = ncr.SystemFileName;
                            nonConformanceReport.Engine_Part_Dbkey = ncr.Engine_Part_Dbkey;
                            nonConformanceReport.Vendor = ncr.Vendor;
                            nonConformanceReport.SerialNumber = ncr.SerialNumber;
                            nonConformanceReport.Revision = ncr.Revision;
                            nonConformanceReport.Qty = ncr.Qty;
                            nonConformanceReport.Module = ncr.Module;
                            nonConformanceReport.Stress = ncr.Stress;
                            nonConformanceReport.Tas = ncr.Tas;
                            nonConformanceReport.Chair = ncr.Chair;
                            nonConformanceReport.DARno = ncr.DARno;
                            nonConformanceReport.JobCard = ncr.JobCard;
                            nonConformanceReport.RawMaterial = ncr.RawMaterial;
                            nonConformanceReport.Stage_Final = ncr.Stage_Final;
                            nonConformanceReport.Inspection_Report_No = ncr.Inspection_Report_No;
                            nonConformanceReport.ECM_TR_NO = ncr.ECM_TR_NO;
                            nonConformanceReport.ECM_No = ncr.ECM_No;
                            
                            

                            _dbContext.Entry(nonConformanceReport).State = EntityState.Modified;
                        }
                    }
                    _dbContext.SaveChanges();
                    List<NonConformanceReport_Item> ncr_items = _dbContext.NonConformanceReport_Items.Where(x => x.NCRGuid == viewmodel.nonConformanceReportVM.NCRGuid).ToList();
                    foreach (var item in viewmodel.nonConformanceReport_Items)
                    {
                        if (item.NCRItemKey == 0)
                        {
                            var ncrItem = new NonConformanceReport_Item();
                            ncrItem.SerialNumber = item.SerialNumber;
                            ncrItem.DrawingDimension = item.DrawingDimension;
                            ncrItem.ActualDimension = item.ActualDimension;
                            ncrItem.Rework_Dimension = item.Rework_Dimension;
                            ncrItem.DrgZone = item.DrgZone;
                            ncrItem.Engine = item.Engine;
                            ncrItem.UpdatedOn = DateTime.Now;
                            ncrItem.UpdatedBy = loggedInUser;
                            ncrItem.NCRGuid = ncr.NCRGuid;
                            ncrItem.CreatedBy = loggedInUser;
                            ncrItem.CreatedOn = DateTime.Now;
                            ncrItem.Serial_No_in_Inspection_Rep = item.Serial_No_in_Inspection_Rep;
                            ncrItem.Deviation_Reason_Analysis = item.Deviation_Reason_Analysis;
                            ncrItem.SlNo = item.SlNo;
                            _dbContext.Add(ncrItem);
                        }
                        else
                        {
                            var ncrItem = ncr_items.Where(x => x.NCRItemKey == item.NCRItemKey).FirstOrDefault();
                            if (ncrItem != null)
                            {
                                ncrItem.SerialNumber = item.SerialNumber;
                                ncrItem.DrawingDimension = item.DrawingDimension;
                                ncrItem.ActualDimension = item.ActualDimension;
                                ncrItem.Rework_Dimension = item.Rework_Dimension;
                                ncrItem.DrgZone = item.DrgZone;
                                ncrItem.Engine = item.Engine;
                                ncrItem.UpdatedOn = DateTime.Now;
                                ncrItem.UpdatedBy = loggedInUser;
                                ncrItem.NCRGuid = ncr.NCRGuid;
                                ncrItem.Serial_No_in_Inspection_Rep = item.Serial_No_in_Inspection_Rep;
                                ncrItem.Deviation_Reason_Analysis = item.Deviation_Reason_Analysis;
                                ncrItem.SlNo = item.SlNo;
                                _dbContext.Entry(ncrItem).State = EntityState.Modified;
                            }
                        }
                    }
                    _dbContext.SaveChanges();

                    return Json(new { success = true, msg = "Saved successfully", ncrguid = ncr.NCRGuid });
                }
                catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
            }
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.NCR_Read)]
        public IActionResult NCRSummary()
        {
            return View();
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.NCR_Write)]
        public IActionResult deleteNCRItem(int key)
        {
            string cmdstr = $"DELETE FROM [dbo].[NonConformanceReport_Items] WHERE [NCRItemKey] = {key}";
            MPCRS.Utilities.MPGlobals.ExceSQLNonQuery(cmdstr);
            return Json(new { success = true });
        }

        [HttpPost]
        [ClaimRequirement(UserPermissions.NCR_Delete)] 
        public async Task<IActionResult> DeleteNcr(string ncrGUid)
        {
            try
            {
                if (ncrGUid.IsNullOrEmpty())
                {
                    return Json(new
                    {
                        success = false,
                        message = "NCR Guid is missing or invalid."
                    });
                }

                int UserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                // 1. Get connection string from appsettings.json 
                var connectionString = _configuration.GetConnectionString("MPCRS");

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    using (var cmd = new SqlCommand("dbo.DeleteNCR", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // 2. Pass parameters to SP
                        cmd.Parameters.AddWithValue("@NCRGuid", ncrGUid);
                        cmd.Parameters.AddWithValue("@UpdatedBy", UserDbkey);

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
        public IActionResult NCRItemstatus(string ncrGuid, string ncrWorkFlowGUID)
        {
            NCRSerialNoDataVM nCRSerialNo = new();
            nCRSerialNo.NCRGUID = ncrGuid;
            nCRSerialNo.NCRWorkflowGUID = ncrWorkFlowGUID;
            return PartialView("../ncr/ncritemstatus", nCRSerialNo);
        }

        //public async Task<IActionResult> NCREmailNotification(string[] filteredUsersEmails, string senderEmail, string ReferenceNumber, int? AssignedBy)
        //{
        //    try
        //    {
        //        string shramLink = _shramLink;
        //        string bccList = _bccList;
        //        EmailModel emailModel = new EmailModel();
        //        emailModel.Recipients = string.Join(",", filteredUsersEmails);
        //        if (!string.IsNullOrEmpty(senderEmail) && senderEmail.EndsWith("@mail.gtre.org", StringComparison.OrdinalIgnoreCase))
        //        {
        //            emailModel.CopyTo = senderEmail;
        //        }
        //        emailModel.BlindCopy = bccList;
        //        emailModel.MailSubject = "New NCR review request";
        //        StringBuilder sb = new StringBuilder();
        //        sb.Append(@"Dear User, <br> <br>");
        //        sb.Append("A new NCR review request for ");
        //        sb.Append($"NCR reference no {ReferenceNumber} has been created and assigned to you. <br>");
        //        // sb.Append($"You are requested to login to SHRAM portal - {shramLink} and take necessary action <br>");
        //        sb.Append($"You are requested to login to <a href='{shramLink}' target='_blank'>SHRAM portal</a> and take necessary action <br>");

        //        sb.Append($"-Thank you, <br> STFE");
        //        emailModel.MailBody = sb.ToString();
        //        emailModel.IsHTML = true;
        //        Utilities.Notification.SendEmail(emailModel);

        //        DataTable dt = MPGlobals.GetDataForDatatable("Select * from Mail_Credentials");
        //        string SenderMail = "";
        //        if (dt.Rows.Count == 1)
        //        {
        //            SenderMail = dt.Rows[0].ItemArray[1].ToString();
        //        }
        //        Mailer_Log mailer_Log = new Mailer_Log();
        //        mailer_Log.MailFrom = SenderMail;
        //        mailer_Log.MailType = "NCR Assignment";
        //        mailer_Log.MailTo = emailModel.Recipients;
        //        mailer_Log.Subject = emailModel.MailSubject;
        //        mailer_Log.Body = emailModel.MailBody;
        //        mailer_Log.TriggerStatus = 1;
        //        mailer_Log.CreatedOn = DateTime.Now;
        //        mailer_Log.CreatedBy = AssignedBy;
        //        _dbContext.Mailer_Logs.Add(mailer_Log);
        //        _dbContext.SaveChanges();
        //        return Json(new { success = true, msg = "Email Sent" });

        //    }
        //    catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
        //}


        public async Task<IActionResult> SendNCRNotification(
         string[] recipientEmails,
         string senderEmail,
         string ncrReferenceNumber,
         string moduleName,
         string currentStatus,
         string actionRequired,
         int? assignedBy,
         string ncrGuid,
     List<string> serialNumbers = null     )
        {
            try
            {
                string shramLink = _shramLink;
                string bccList = _bccList;

                EmailModel emailModel = new EmailModel();

                // Validate and handle empty recipients
                var validRecipients = recipientEmails?.Where(e => !string.IsNullOrEmpty(e)).ToArray() ?? new string[] { };

                if (validRecipients.Length == 0)
                {
                    // If no valid recipients, check if BCC list exists
                    if (string.IsNullOrEmpty(bccList))
                    {
                        // Log warning and return - cannot send email without any recipients
                        ErrorHandler.LogException(new Exception($"No valid recipients found for NCR notification: {ncrReferenceNumber}"));
                        return Json(new { success = false, msg = "No valid email recipients found" });
                    }
                    else
                    {
                        // Send to BCC list only by making it the primary recipient
                        emailModel.Recipients = bccList;
                        emailModel.BlindCopy = ""; // Clear BCC since we moved them to Recipients
                    }
                }
                else
                {
                    emailModel.Recipients = string.Join(",", validRecipients);
                    emailModel.BlindCopy = bccList;
                }

                if (!string.IsNullOrEmpty(senderEmail) && senderEmail.EndsWith("@mail.gtre.org", StringComparison.OrdinalIgnoreCase))
                {
                    emailModel.CopyTo = senderEmail;
                }

                DetailsForNCRMailVM details = new DetailsForNCRMailVM();
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var db = connection.QueryMultiple($" GetDetailsForNCRMail @NCRGuid  = '{ncrGuid}' ");
                    details = db.Read<DetailsForNCRMailVM>().FirstOrDefault();
                }
                emailModel.MailSubject = $"Review Request for NCR #{ncrReferenceNumber}";

                StringBuilder sb = new StringBuilder();

                // Forwarding notification template
                sb.Append("Dear User,<br><br>");
                sb.Append("A new NCR review request has been assigned to you.<br><br>");
                sb.Append("<strong>NCR Details:</strong><br>");
                sb.Append($"NCR No.: {ncrReferenceNumber}<br>");
                sb.Append($"Part No.: {details.Draw_part_no}<br>");
                sb.Append($"Part Description: {details.Description}<br>");
                sb.Append($"Module: {moduleName}<br>");
                sb.Append($"Current Status: {currentStatus}<br>");
                sb.Append($"Forwarded By: {senderEmail}<br>");
                sb.Append($"Date: {DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt")}<br><br>");

                // Show serial numbers if provided (for item-level forwarding)
                if (serialNumbers != null && serialNumbers.Count > 0)
                {
                    sb.Append("<strong>Serial Numbers Assigned:</strong><br>");
                    foreach (var sn in serialNumbers)
                    {
                        sb.Append($"- {sn}<br>");
                    }
                    sb.Append("<br>");
                }

                sb.Append($"<strong>Action Required:</strong> {actionRequired}<br><br>");
                sb.Append($"You are requested to login to <a href='{shramLink}' target='_blank'>SHRAM portal</a> and take necessary action.<br><br>");
                sb.Append("Thank you,<br>STFE");

                sb.Append("<br><br>");
                sb.Append("<hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>");
                sb.Append("<small style='color: #888; font-size: 11px;'>This is an automated system-generated email.</small>");

                emailModel.MailBody = sb.ToString();
                emailModel.IsHTML = true;

                Utilities.Notification.SendEmail(emailModel);

                // Log the email
                DataTable dt = MPGlobals.GetDataForDatatable("Select * from Mail_Credentials");
                string SenderMail = "";
                if (dt.Rows.Count == 1)
                {
                    SenderMail = dt.Rows[0].ItemArray[1].ToString();
                }

                Mailer_Log mailer_Log = new Mailer_Log();
                mailer_Log.MailFrom = SenderMail;
                mailer_Log.MailType = "NCR Assignment";
                mailer_Log.MailTo = emailModel.Recipients;
                mailer_Log.Subject = emailModel.MailSubject;
                mailer_Log.Body = emailModel.MailBody;
                mailer_Log.TriggerStatus = 1;
                mailer_Log.CreatedOn = DateTime.Now;
                mailer_Log.CreatedBy = assignedBy;
                _dbContext.Mailer_Logs.Add(mailer_Log);
                _dbContext.SaveChanges();

                return Json(new { success = true, msg = "Email Sent" });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }


        public async Task<IActionResult> SendNCRCompletionNotification(
      string[] recipientEmails,
      string senderEmail,
      string ncrReferenceNumber,
      string moduleName,
      int? completedBy,
       string ncrGuid,
      List<string> serialNumbers = null)
        {
            try
            {
                string shramLink = _shramLink;
                string bccList = _bccList;

                EmailModel emailModel = new EmailModel();

                // Validate and handle empty recipients
                var validRecipients = recipientEmails?.Where(e => !string.IsNullOrEmpty(e)).ToArray() ?? new string[] { };

                if (validRecipients.Length == 0)
                {
                    // If no valid recipients, check if BCC list exists
                    if (string.IsNullOrEmpty(bccList))
                    {
                        // Log warning and return - cannot send email without any recipients
                        ErrorHandler.LogException(new Exception($"No valid recipients found for NCR completion notification: {ncrReferenceNumber}"));
                        return Json(new { success = false, msg = "No valid email recipients found" });
                    }
                    else
                    {
                        // Send to BCC list only by making it the primary recipient
                        emailModel.Recipients = bccList;
                        emailModel.BlindCopy = ""; // Clear BCC since we moved them to Recipients
                    }
                }
                else
                {
                    emailModel.Recipients = string.Join(",", validRecipients);
                    emailModel.BlindCopy = bccList;
                }

                if (!string.IsNullOrEmpty(senderEmail) && senderEmail.EndsWith("@mail.gtre.org", StringComparison.OrdinalIgnoreCase))
                {
                    emailModel.CopyTo = senderEmail;
                }

                emailModel.MailSubject = $"Review Completed For NCR #{ncrReferenceNumber}";

                StringBuilder sb = new StringBuilder();


                DetailsForNCRMailVM details = new DetailsForNCRMailVM();
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var db = connection.QueryMultiple($" GetDetailsForNCRMail @NCRGuid  = '{ncrGuid}' ");
                    details = db.Read<DetailsForNCRMailVM>().FirstOrDefault();
                }

                // Completion notification template
                sb.Append("Dear User,<br><br>");
                sb.Append($"The NCR review has been marked as completed.<br><br>");
                sb.Append("<strong>NCR Details:</strong><br>");
                sb.Append($"NCR No.: {ncrReferenceNumber}<br>");
                sb.Append($"Part No.: {details.Draw_part_no}<br>");
                sb.Append($"Part Description.: {details.Description}<br>");
                sb.Append($"Module: {moduleName}<br>");
                sb.Append($"Status: Review Completed<br>");
                sb.Append($"Completed By: {senderEmail}<br>");
                sb.Append($"Date: {DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt")}<br><br>");

                // Show serial numbers if provided
                if (serialNumbers != null && serialNumbers.Count > 0)
                {
                    sb.Append("<strong>Serial Numbers Completed:</strong><br>");
                    foreach (var sn in serialNumbers)
                    {
                        sb.Append($"- {sn}<br>");
                    }
                    sb.Append("<br>");
                }

                sb.Append($"You can view the updated NCR in <a href='{shramLink}' target='_blank'>SHRAM portal</a><br><br>");
                sb.Append("Thank you,<br>STFE");

                sb.Append("<br><br>");
                sb.Append("<hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>");
                sb.Append("<small style='color: #888; font-size: 11px;'>This is an automated system-generated email.</small>");

                emailModel.MailBody = sb.ToString();
                emailModel.IsHTML = true;

                Utilities.Notification.SendEmail(emailModel);

                // Log the email
                DataTable dt = MPGlobals.GetDataForDatatable("Select * from Mail_Credentials");
                string SenderMail = "";
                if (dt.Rows.Count == 1)
                {
                    SenderMail = dt.Rows[0].ItemArray[1].ToString();
                }

                Mailer_Log mailer_Log = new Mailer_Log();
                mailer_Log.MailFrom = SenderMail;
                mailer_Log.MailType = "NCR Completion";
                mailer_Log.MailTo = emailModel.Recipients;
                mailer_Log.Subject = emailModel.MailSubject;
                mailer_Log.Body = emailModel.MailBody;
                mailer_Log.TriggerStatus = 1;
                mailer_Log.CreatedOn = DateTime.Now;
                mailer_Log.CreatedBy = completedBy;
                _dbContext.Mailer_Logs.Add(mailer_Log);
                _dbContext.SaveChanges();

                return Json(new { success = true, msg = "Completion Email Sent" });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

    
        public IActionResult GetUSersForAssignment(int ModuleID)
        {
            DataTable dataTable = MPGlobals.GetDataForDatalist($"[dbo].[Get_Users_WorkflowAssignment]  @ModuleID = {ModuleID}");
            List<string> users = new List<string>();
            if (dataTable != null)
            {
                foreach (DataRow dr in dataTable.Rows)
                {
                    users.Add(dr["UserGuid"].ToString());
                }
            }
            return Json(users);
        }

        #endregion

        #region ncr module user options
        public IActionResult checkifModuleAssignedforNCR(string ncrguid)
        {
            try
            {
                bool isNCRAssignedtoModule = false;
                var isNCRAssignedtoModuledata = _dbContext.NonConformanceReports.Where(x => x.NCRGuid == ncrguid && (x.Module_Responsibilty == null || x.AssignedUserGuid == null)).FirstOrDefault();
                if (isNCRAssignedtoModuledata != null)
                {
                    isNCRAssignedtoModule = true;
                }
                return Json(new { success = isNCRAssignedtoModule });
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

        }

        public IActionResult AssignModuleToNCR(string NCRGUID)
        {
            NonConformanceReportVM nonConformanceReportVM = new NonConformanceReportVM();
            var nonConformanceReport = _dbContext.NonConformanceReports.Where(x => x.NCRGuid == NCRGUID).FirstOrDefault();
            nonConformanceReportVM = JsonConvert.DeserializeObject<NonConformanceReportVM>(JsonConvert.SerializeObject(nonConformanceReport));
            var Engineparts = _dbContext.Engine_Parts_Masters.Where(x => x.Engine_Part_Dbkey == nonConformanceReport.Engine_Part_Dbkey).FirstOrDefault();
            nonConformanceReportVM.FileLocation = Engineparts.Draw_part_no + " / " + Engineparts.Description;
            return PartialView(nonConformanceReportVM);
        }

        //FORWARDING IN NCR LEVEL TO DIFFERENT MODULES(TAS,STRES ,CHAIR.) BY STFE
        // Helper method to check if item has active rework marking
        private bool HasActiveReworkMarking(int ncrItemKey)
        {
            return _dbContext.NonConformanceReport_Item_Reworks
                .Any(x => x.NCRItemKey == ncrItemKey
                       && x.IsActive == true
                       && (x.ReworkType == 1 || x.ReworkType == 2));
        }

        [HttpPost]
        public async Task<IActionResult> SaveModuleAssignedToNCR([FromBody] NonConformanceReportVM nonConformanceReportVM)
        {
            try
            {
                if (nonConformanceReportVM != null)
                {
                    NonConformanceReport nonConformanceReport = _dbContext.NonConformanceReports.Where(x => x.NCRGuid == nonConformanceReportVM.NCRGuid).FirstOrDefault();
                    int UserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    string UserGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value; ;
                    List<NonConformanceReport_Item> nonConformanceReport_Items = _dbContext.NonConformanceReport_Items.Where(X => X.NCRGuid == nonConformanceReportVM.NCRGuid).ToList();
                    if (nonConformanceReport_Items.Count == 0)
                    {
                        return Json(new { failed = true, msg = "Please add serial numbers before assigning" });
                    }
                    var ModuleName = _dbContext.Master_Generals.Where(x => x.Master_Dbkey == nonConformanceReportVM.Module_Responsibilty
                                                                            && x.Master_Type == "Module_Responsibility"
                                                                            && x.is_active == 1)
                                                                            .Select(x => x.Master_Name).FirstOrDefault();
                    if (ModuleName == null)
                    {
                        ModuleName = "";
                    }

                   // bool ReworkStatus = nonConformanceReport_Items.Any(x => (x.Rework_Status == 1 || x.Rework_Status == 2));
                    // Check if any item has rework marking in the new table
                    bool ReworkStatus = _dbContext.NonConformanceReport_Item_Reworks
                        .Any(x => nonConformanceReport_Items.Select(i => i.NCRItemKey).Contains(x.NCRItemKey)
                               && x.IsActive == true
                               && (x.ReworkType == 1 || x.ReworkType == 2));
                    bool AllStressRemarksMarked = nonConformanceReport_Items.All(x => !string.IsNullOrEmpty(x.Stress_Remarks));
                    //Rework cycle
                    if (ReworkStatus == true && AllStressRemarksMarked)
                    {
                        NCR_Workflow_Assignment nCR_Workflow_Assignment = new NCR_Workflow_Assignment();

                        if (ModuleName == "TAS")
                        {
                            nCR_Workflow_Assignment.NCRGUID = nonConformanceReportVM.NCRGuid;
                            nCR_Workflow_Assignment.ModuleID = nonConformanceReportVM.Module_Responsibilty;
                            nCR_Workflow_Assignment.AssigneeUserGUIDs = nonConformanceReportVM.AssignedUserGuid;
                            nCR_Workflow_Assignment.Status = "Forwarded To TAS For Rework/Trial Assembly Remarks";
                            nCR_Workflow_Assignment.WorkUpdatedOn = DateTime.Now;
                            nCR_Workflow_Assignment.AssignedBy = UserDbkey;
                            nCR_Workflow_Assignment.AssignedOn = DateTime.Now;
                            _dbContext.Add(nCR_Workflow_Assignment);
                            _dbContext.SaveChanges();
                            foreach (var item in nonConformanceReport_Items)
                            {
                                //if (item.Rework_Status == 1 || item.Rework_Status == 2)
                                if (HasActiveReworkMarking(item.NCRItemKey))
                                {
                                    item.NCRWorkFlowGuid = nCR_Workflow_Assignment.NCRWorkflowGUID;
                                    item.Module_Rework_Status = "Ended";
                                    item.TAS_Rework_Status = "Forwarded To TAS";
                                    item.UpdatedBy = UserDbkey;
                                    item.UpdatedOn = DateTime.Now;
                                    _dbContext.Entry(item).State = EntityState.Modified;
                                }
                                else
                                {
                                    item.NCRWorkFlowGuid = null;
                                }

                            }
                            NCR_Workflow_Assignments_Log log = new NCR_Workflow_Assignments_Log();
                            log.NCRWorkflowGUID = nCR_Workflow_Assignment.NCRWorkflowGUID;
                            log.Status = "Forwarded to TAS For Rework/Trial Assembly Remarks";
                            log.IsTransfered = true;
                            log.Status_Verbose = "Forwarded NCR to TAS For Rework/Trial Assembly Remarks";
                            log.UpdatedBy = UserGuid;
                            log.UpdatedOn = DateTime.Now;
                            log.NCRGuid = nonConformanceReportVM.NCRGuid;
                            _dbContext.Add(log);
                            _dbContext.SaveChanges();
                            var users = _dbContext.AspNetUsers.ToList();
                            var filteredUsersEmails = users.Where(user => nonConformanceReportVM.AssignedUserGuid.Contains(user.Id)).Select(x => x.Email).ToArray();
                            var senderEmail = users.Where(x => x.OldUserDbkey == nCR_Workflow_Assignment.AssignedBy).Select(x => x.Email).FirstOrDefault();

                            // NCREmailNotification(filteredUsersEmails, senderEmail, nonConformanceReport.ReferenceNumber, UserDbkey);
                            SendNCRNotification(
                                 recipientEmails: filteredUsersEmails,
                                 senderEmail: senderEmail,
                                 ncrReferenceNumber: nonConformanceReport.ReferenceNumber,
                                 moduleName: "TAS",
                                 currentStatus: "Forwarded to TAS for Rework/Trial Assembly Remarks",
                                 actionRequired: "Please update rework/trial assembly remarks",
                                 assignedBy: UserDbkey,
                                 ncrGuid: nonConformanceReportVM.NCRGuid,
                                 serialNumbers: null                                
                             );
                            return Json(new { failed = false, msg = "Successfuly Forwarded to TAS for Rework/Trial Assembly remarks" });
                        }
                        else if (ModuleName == "STRESS")
                        {
                            nCR_Workflow_Assignment.NCRGUID = nonConformanceReportVM.NCRGuid;
                            nCR_Workflow_Assignment.ModuleID = nonConformanceReportVM.Module_Responsibilty;
                            nCR_Workflow_Assignment.AssigneeUserGUIDs = nonConformanceReportVM.AssignedUserGuid;
                            nCR_Workflow_Assignment.Status = "Forwarded To STRESS For Rework/Trial Assembly Remarks";
                            nCR_Workflow_Assignment.WorkUpdatedOn = DateTime.Now;
                            nCR_Workflow_Assignment.AssignedBy = UserDbkey;
                            nCR_Workflow_Assignment.AssignedOn = DateTime.Now;
                            _dbContext.Add(nCR_Workflow_Assignment);
                            _dbContext.SaveChanges();
                            foreach (var item in nonConformanceReport_Items)
                            {
                                // if (item.Rework_Status == 1 || item.Rework_Status == 2)
                                if (HasActiveReworkMarking(item.NCRItemKey))
                                {
                                    item.NCRWorkFlowGuid = nCR_Workflow_Assignment.NCRWorkflowGUID;
                                    item.STRESS_Rework_Status = "Forwarded To STRESS";
                                    item.TAS_Rework_Status = "Ended";
                                    item.UpdatedBy = UserDbkey;
                                    item.UpdatedOn = DateTime.Now;
                                    _dbContext.Entry(item).State = EntityState.Modified;
                                }
                                else
                                {
                                    item.NCRWorkFlowGuid = null;
                                }
                            }
                            NCR_Workflow_Assignments_Log log = new NCR_Workflow_Assignments_Log();
                            log.NCRWorkflowGUID = nCR_Workflow_Assignment.NCRWorkflowGUID;
                            log.Status = "Forwarded to STRESS For Rework/Trial Assembly Remarks";
                            log.IsTransfered = true;
                            log.Status_Verbose = "Forwarded NCR to STRESS For Rework/Trial Assembly Remarks";
                            log.UpdatedBy = UserGuid;
                            log.UpdatedOn = DateTime.Now;
                            log.NCRGuid = nonConformanceReportVM.NCRGuid;
                            _dbContext.Add(log);
                            _dbContext.SaveChanges();
                            var users = _dbContext.AspNetUsers.ToList();
                            var filteredUsersEmails = users.Where(user => nonConformanceReportVM.AssignedUserGuid.Contains(user.Id)).Select(x => x.Email).ToArray();
                            var senderEmail = users.Where(x => x.OldUserDbkey == nCR_Workflow_Assignment.AssignedBy).Select(x => x.Email).FirstOrDefault();

                            // NCREmailNotification(filteredUsersEmails, senderEmail, nonConformanceReport.ReferenceNumber, UserDbkey);
                            SendNCRNotification(
                                recipientEmails: filteredUsersEmails,
                                senderEmail: senderEmail,
                                ncrReferenceNumber: nonConformanceReport.ReferenceNumber,
                                moduleName: "STRESS",
                                currentStatus: "Forwarded to STRESS for Rework/Trial Assembly Remarks",
                                actionRequired: "Please update rework/trial assembly remarks ",
                                assignedBy: UserDbkey,
                                ncrGuid: nonConformanceReportVM.NCRGuid,
                                serialNumbers: null 
                            );

                            return Json(new { failed = false, msg = "Successfuly Forwarded to STRESS For Rework/Trial Assembly Remarks" });
                        }
                        else if (ModuleName == "CHAIR")
                        {
                            nCR_Workflow_Assignment.NCRGUID = nonConformanceReportVM.NCRGuid;
                            nCR_Workflow_Assignment.ModuleID = nonConformanceReportVM.Module_Responsibilty;
                            nCR_Workflow_Assignment.AssigneeUserGUIDs = nonConformanceReportVM.AssignedUserGuid;
                            nCR_Workflow_Assignment.Status = "Forwarded To CHAIR";
                            nCR_Workflow_Assignment.WorkUpdatedOn = DateTime.Now;
                            nCR_Workflow_Assignment.AssignedBy = UserDbkey;
                            nCR_Workflow_Assignment.AssignedOn = DateTime.Now;
                            _dbContext.Add(nCR_Workflow_Assignment);
                            _dbContext.SaveChanges();
                            foreach (var item in nonConformanceReport_Items)
                            {
                                item.NCRWorkFlowGuid = nCR_Workflow_Assignment.NCRWorkflowGUID;
                                item.Chair_Status = "Forwarded To CHAIR";
                                item.TAS_Rework_Status = "Ended"; // when deviations only has Trial assembly then TAS was not getting ENDED so by default we are ending it here
                                item.STRESS_Rework_Status = "Ended";
                                item.UpdatedBy = UserDbkey;
                                item.UpdatedOn = DateTime.Now;
                                _dbContext.Entry(item).State = EntityState.Modified;
                            }
                            NCR_Workflow_Assignments_Log log = new NCR_Workflow_Assignments_Log();
                            log.NCRWorkflowGUID = nCR_Workflow_Assignment.NCRWorkflowGUID;
                            log.Status = "Forwarded to CHAIR";
                            log.IsTransfered = true;
                            log.Status_Verbose = "Forwarded NCR to CHAIR";
                            log.UpdatedBy = UserGuid;
                            log.UpdatedOn = DateTime.Now;
                            log.NCRGuid = nonConformanceReportVM.NCRGuid;
                            _dbContext.Add(log);
                            _dbContext.SaveChanges();
                            var users = _dbContext.AspNetUsers.ToList();
                            var filteredUsersEmails = users.Where(user => nonConformanceReportVM.AssignedUserGuid.Contains(user.Id)).Select(x => x.Email).ToArray();
                            var senderEmail = users.Where(x => x.OldUserDbkey == nCR_Workflow_Assignment.AssignedBy).Select(x => x.Email).FirstOrDefault();

                            // NCREmailNotification(filteredUsersEmails, senderEmail, nonConformanceReport.ReferenceNumber, UserDbkey);
                            SendNCRNotification(
                             recipientEmails: filteredUsersEmails,
                             senderEmail: senderEmail,
                             ncrReferenceNumber: nonConformanceReport.ReferenceNumber,
                               //moduleName: "CHAIR",
                               //currentStatus: "Forwarded to CHAIR",
                             moduleName: "Waiver Board Chairperson",
                             currentStatus: "Forwarded to Chairperson",
                             actionRequired: "Please review and update remarks ",
                             assignedBy: UserDbkey,
                             ncrGuid: nonConformanceReportVM.NCRGuid,
                             serialNumbers: null 
                         );
                            return Json(new { failed = false, msg = "Successfuly Forwarded to CHAIR" });
                        }
                        else
                        {
                            nCR_Workflow_Assignment.NCRGUID = nonConformanceReportVM.NCRGuid;
                            nCR_Workflow_Assignment.ModuleID = nonConformanceReportVM.Module_Responsibilty;
                            nCR_Workflow_Assignment.AssigneeUserGUIDs = nonConformanceReportVM.AssignedUserGuid;
                            nCR_Workflow_Assignment.Status = $"Forwarded To {ModuleName} For Rework/Trial Assembly Remarks";
                            nCR_Workflow_Assignment.WorkUpdatedOn = DateTime.Now;
                            nCR_Workflow_Assignment.AssignedBy = UserDbkey;
                            nCR_Workflow_Assignment.AssignedOn = DateTime.Now;
                            _dbContext.Add(nCR_Workflow_Assignment);
                            _dbContext.SaveChanges();
                            foreach (var item in nonConformanceReport_Items)
                            {
                                //  if (item.Rework_Status == 1 || item.Rework_Status == 2)
                                if (HasActiveReworkMarking(item.NCRItemKey))
                                {
                                    item.NCRWorkFlowGuid = nCR_Workflow_Assignment.NCRWorkflowGUID;
                                    item.Module_Rework_Status = "Forwarded To Module";
                                    item.UpdatedBy = UserDbkey;
                                    item.UpdatedOn = DateTime.Now;
                                    item.Stress_Status = "Ended";
                                    _dbContext.Entry(item).State = EntityState.Modified;
                                }
                                else
                                {
                                    item.NCRWorkFlowGuid = null;
                                    nonConformanceReport.AssignedUserGuid = "";
                                    item.Stress_Status = "Ended";
                                    _dbContext.Entry(nonConformanceReport).State = EntityState.Modified;
                                }
                            }
                            NCR_Workflow_Assignments_Log log = new NCR_Workflow_Assignments_Log();
                            log.NCRWorkflowGUID = nCR_Workflow_Assignment.NCRWorkflowGUID;
                            log.Status = $"Forwarded to {ModuleName} For Rework/Trial Assembly Remarks";
                            log.IsTransfered = true;
                            log.Status_Verbose = $"Forwarded NCR to {ModuleName} For Rework/Trial Assembly Remarks";
                            log.UpdatedBy = UserGuid;
                            log.UpdatedOn = DateTime.Now;
                            log.NCRGuid = nonConformanceReportVM.NCRGuid;
                            _dbContext.Add(log);

                            _dbContext.SaveChanges();

                            var users = _dbContext.AspNetUsers.ToList();
                            var filteredUsersEmails = users.Where(user => nonConformanceReportVM.AssignedUserGuid.Contains(user.Id)).Select(x => x.Email).ToArray();
                            var senderEmail = users.Where(x => x.OldUserDbkey == nonConformanceReport.ModuleAssignedBy).Select(x => x.Email).FirstOrDefault();

                            // NCREmailNotification(filteredUsersEmails, senderEmail, nonConformanceReport.ReferenceNumber, nonConformanceReport.ModuleAssignedBy);
                            SendNCRNotification(
                                  recipientEmails: filteredUsersEmails,
                                  senderEmail: senderEmail,
                                  ncrReferenceNumber: nonConformanceReport.ReferenceNumber,
                                  moduleName: ModuleName,
                                  currentStatus: $"Forwarded to {ModuleName} for Rework/Trial Assembly Remarks",
                                  actionRequired: "Please update rework/trial assembly remarks ",
                                  assignedBy: nonConformanceReport.ModuleAssignedBy,
                                  ncrGuid: nonConformanceReportVM.NCRGuid,
                                  serialNumbers: null 
                             );
                            return Json(new { failed = false, msg = $"Successfuly Forwarded to {ModuleName} For Rework/Trial Assembly Remarks" });
                        }

                    }
                    //normal cycle forawarding
                    else
                    {


                        NCR_Workflow_Assignment nCR_Workflow_Assignment = new NCR_Workflow_Assignment();
                        if (ModuleName == "TAS")
                        {
                            nCR_Workflow_Assignment.NCRGUID = nonConformanceReportVM.NCRGuid;
                            nCR_Workflow_Assignment.ModuleID = nonConformanceReportVM.Module_Responsibilty;
                            nCR_Workflow_Assignment.AssigneeUserGUIDs = nonConformanceReportVM.AssignedUserGuid;
                            nCR_Workflow_Assignment.Status = "Forwarded To TAS";
                            nCR_Workflow_Assignment.WorkUpdatedOn = DateTime.Now;
                            nCR_Workflow_Assignment.AssignedBy = UserDbkey;
                            nCR_Workflow_Assignment.AssignedOn = DateTime.Now;
                            _dbContext.Add(nCR_Workflow_Assignment);
                            _dbContext.SaveChanges();
                            foreach (var item in nonConformanceReport_Items)
                            {
                                item.NCRWorkFlowGuid = nCR_Workflow_Assignment.NCRWorkflowGUID;
                                item.TAS_Status = "Forwarded To TAS";
                                item.Module_Status = "Ended";
                                item.UpdatedBy = UserDbkey;
                                item.UpdatedOn = DateTime.Now;
                                _dbContext.Entry(item).State = EntityState.Modified;
                            }
                            NCR_Workflow_Assignments_Log log = new NCR_Workflow_Assignments_Log();
                            log.NCRWorkflowGUID = nCR_Workflow_Assignment.NCRWorkflowGUID;
                            log.Status = "Forwarded to TAS";
                            log.IsTransfered = true;
                            log.Status_Verbose = "Forwarded NCR to TAS";
                            log.UpdatedBy = UserGuid;
                            log.UpdatedOn = DateTime.Now;
                            log.NCRGuid = nonConformanceReportVM.NCRGuid;
                            _dbContext.Add(log);
                            _dbContext.SaveChanges();
                            var users = _dbContext.AspNetUsers.ToList();
                            var filteredUsersEmails = users.Where(user => nonConformanceReportVM.AssignedUserGuid.Contains(user.Id)).Select(x => x.Email).ToArray();
                            var senderEmail = users.Where(x => x.OldUserDbkey == nCR_Workflow_Assignment.AssignedBy).Select(x => x.Email).FirstOrDefault();

                            // NCREmailNotification(filteredUsersEmails, senderEmail, nonConformanceReport.ReferenceNumber, UserDbkey);

                            SendNCRNotification(
                                recipientEmails: filteredUsersEmails,
                                senderEmail: senderEmail,
                                ncrReferenceNumber: nonConformanceReport.ReferenceNumber,
                                moduleName: "TAS",
                                currentStatus: "Forwarded to TAS",
                                actionRequired: "Please review and provide your comments",
                                assignedBy: UserDbkey,
                                ncrGuid: nonConformanceReportVM.NCRGuid,
                                serialNumbers: null 
                            );
                            return Json(new { failed = false, msg = "Successfuly Forwarded to TAS" });
                        }
                        else if (ModuleName == "STRESS")
                        {
                            nCR_Workflow_Assignment.NCRGUID = nonConformanceReportVM.NCRGuid;
                            nCR_Workflow_Assignment.ModuleID = nonConformanceReportVM.Module_Responsibilty;
                            nCR_Workflow_Assignment.AssigneeUserGUIDs = nonConformanceReportVM.AssignedUserGuid;
                            nCR_Workflow_Assignment.Status = "Forwarded To STRESS";
                            nCR_Workflow_Assignment.WorkUpdatedOn = DateTime.Now;
                            nCR_Workflow_Assignment.AssignedBy = UserDbkey;
                            nCR_Workflow_Assignment.AssignedOn = DateTime.Now;
                            _dbContext.Add(nCR_Workflow_Assignment);
                            _dbContext.SaveChanges();
                            foreach (var item in nonConformanceReport_Items)
                            {
                                item.NCRWorkFlowGuid = nCR_Workflow_Assignment.NCRWorkflowGUID;
                                item.Stress_Status = "Forwarded To STRESS";
                                item.TAS_Status = "Ended";
                                item.UpdatedBy = UserDbkey;
                                item.UpdatedOn = DateTime.Now;
                                _dbContext.Entry(item).State = EntityState.Modified;
                            }
                            NCR_Workflow_Assignments_Log log = new NCR_Workflow_Assignments_Log();
                            log.NCRWorkflowGUID = nCR_Workflow_Assignment.NCRWorkflowGUID;
                            log.Status = "Forwarded to STRESS";
                            log.IsTransfered = true;
                            log.Status_Verbose = "Forwarded NCR to STRESS";
                            log.UpdatedBy = UserGuid;
                            log.UpdatedOn = DateTime.Now;
                            log.NCRGuid = nonConformanceReportVM.NCRGuid;
                            _dbContext.Add(log);
                            _dbContext.SaveChanges();
                            var users = _dbContext.AspNetUsers.ToList();
                            var filteredUsersEmails = users.Where(user => nonConformanceReportVM.AssignedUserGuid.Contains(user.Id)).Select(x => x.Email).ToArray();
                            var senderEmail = users.Where(x => x.OldUserDbkey == nCR_Workflow_Assignment.AssignedBy).Select(x => x.Email).FirstOrDefault();

                            //NCREmailNotification(filteredUsersEmails, senderEmail, nonConformanceReport.ReferenceNumber, UserDbkey);
                            SendNCRNotification(
                                 recipientEmails: filteredUsersEmails,
                                 senderEmail: senderEmail,
                                 ncrReferenceNumber: nonConformanceReport.ReferenceNumber,
                                 moduleName: "STRESS",
                                 currentStatus: "Forwarded to STRESS",
                                 actionRequired: "Please review and provide your comments",
                                 assignedBy: UserDbkey,
                                 ncrGuid: nonConformanceReportVM.NCRGuid,
                                 serialNumbers: null 
                             );
                            return Json(new { failed = false, msg = "Successfuly Forwarded to STRESS" });
                        }
                        else if (ModuleName == "CHAIR")
                        {
                            nCR_Workflow_Assignment.NCRGUID = nonConformanceReportVM.NCRGuid;
                            nCR_Workflow_Assignment.ModuleID = nonConformanceReportVM.Module_Responsibilty;
                            nCR_Workflow_Assignment.AssigneeUserGUIDs = nonConformanceReportVM.AssignedUserGuid;
                            nCR_Workflow_Assignment.Status = "Forwarded To CHAIR";
                            nCR_Workflow_Assignment.WorkUpdatedOn = DateTime.Now;
                            nCR_Workflow_Assignment.AssignedBy = UserDbkey;
                            nCR_Workflow_Assignment.AssignedOn = DateTime.Now;
                            _dbContext.Add(nCR_Workflow_Assignment);
                            _dbContext.SaveChanges();
                            foreach (var item in nonConformanceReport_Items)
                            {
                                item.NCRWorkFlowGuid = nCR_Workflow_Assignment.NCRWorkflowGUID;
                                item.Chair_Status = "Forwarded To CHAIR";
                                item.Stress_Status = "Ended";
                                item.UpdatedBy = UserDbkey;
                                item.UpdatedOn = DateTime.Now;
                                _dbContext.Entry(item).State = EntityState.Modified;
                            }
                            NCR_Workflow_Assignments_Log log = new NCR_Workflow_Assignments_Log();
                            log.NCRWorkflowGUID = nCR_Workflow_Assignment.NCRWorkflowGUID;
                            log.Status = "Forwarded to CHAIR";
                            log.IsTransfered = true;
                            log.Status_Verbose = "Forwarded NCR to CHAIR";
                            log.UpdatedBy = UserGuid;
                            log.UpdatedOn = DateTime.Now;
                            log.NCRGuid = nonConformanceReportVM.NCRGuid;
                            _dbContext.Add(log);
                            _dbContext.SaveChanges();
                            var users = _dbContext.AspNetUsers.ToList();
                            var filteredUsersEmails = users.Where(user => nonConformanceReportVM.AssignedUserGuid.Contains(user.Id)).Select(x => x.Email).ToArray();
                            var senderEmail = users.Where(x => x.OldUserDbkey == nCR_Workflow_Assignment.AssignedBy).Select(x => x.Email).FirstOrDefault();

                            //NCREmailNotification(filteredUsersEmails, senderEmail, nonConformanceReport.ReferenceNumber, UserDbkey);
                            SendNCRNotification(
                            recipientEmails: filteredUsersEmails,
                            senderEmail: senderEmail,
                            ncrReferenceNumber: nonConformanceReport.ReferenceNumber,
                            moduleName: "CHAIR",
                            currentStatus: "Forwarded to CHAIR",
                            actionRequired: "Please review and update remarks ",
                            assignedBy: UserDbkey,
                            ncrGuid: nonConformanceReportVM.NCRGuid,
                            serialNumbers: null 
                        );
                            return Json(new { failed = false, msg = "Successfuly Forwarded to CHAIR" });
                        }
                        else
                        {
                            NCR_Workflow_Assignments_Log log = new NCR_Workflow_Assignments_Log();
                            // log.NCRWorkflowGUID = nCR_Workflow_Assignment.NCRWorkflowGUID;
                            log.Status = $"Forwarded to {ModuleName}";
                            log.IsTransfered = true;
                            log.Status_Verbose = $"Forwarded NCR to {ModuleName}";
                            log.UpdatedBy = UserGuid;
                            log.UpdatedOn = DateTime.Now;
                            log.NCRGuid = nonConformanceReportVM.NCRGuid;
                            _dbContext.Add(log);
                            nonConformanceReport.Module_Responsibilty = nonConformanceReportVM.Module_Responsibilty;
                            nonConformanceReport.AssignedUserGuid = nonConformanceReportVM.AssignedUserGuid;
                            nonConformanceReport.ModuleAssignedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                            nonConformanceReport.ModuleAssignedOn = DateTime.Now;
                            _dbContext.Entry(nonConformanceReport).State = EntityState.Modified;
                            _dbContext.SaveChanges();

                            nCR_Workflow_Assignment.NCRGUID = nonConformanceReportVM.NCRGuid;
                            nCR_Workflow_Assignment.ModuleID = nonConformanceReportVM.Module_Responsibilty;
                            nCR_Workflow_Assignment.AssigneeUserGUIDs = nonConformanceReportVM.AssignedUserGuid;
                            nCR_Workflow_Assignment.Status = "Forwarded To MODULE";
                            nCR_Workflow_Assignment.WorkUpdatedOn = DateTime.Now;
                            nCR_Workflow_Assignment.AssignedBy = UserDbkey;
                            nCR_Workflow_Assignment.AssignedOn = DateTime.Now;
                            _dbContext.Add(nCR_Workflow_Assignment);
                            _dbContext.SaveChanges();
                            foreach (var item in nonConformanceReport_Items)
                            {
                                item.NCRWorkFlowGuid = nCR_Workflow_Assignment.NCRWorkflowGUID;
                                item.Module_Status = "Forwarded To MODULE";
                                item.UpdatedBy = UserDbkey;
                                item.UpdatedOn = DateTime.Now;
                                _dbContext.Entry(item).State = EntityState.Modified;
                            }
                            _dbContext.SaveChanges();
                            var users = _dbContext.AspNetUsers.ToList();
                            var filteredUsersEmails = users.Where(user => nonConformanceReportVM.AssignedUserGuid.Contains(user.Id)).Select(x => x.Email).ToArray();
                            var senderEmail = users.Where(x => x.OldUserDbkey == nonConformanceReport.ModuleAssignedBy).Select(x => x.Email).FirstOrDefault();

                            // NCREmailNotification(filteredUsersEmails, senderEmail, nonConformanceReport.ReferenceNumber, nonConformanceReport.ModuleAssignedBy);
                            SendNCRNotification(
                                 recipientEmails: filteredUsersEmails,
                                 senderEmail: senderEmail,
                                 ncrReferenceNumber: nonConformanceReport.ReferenceNumber,
                                 moduleName: ModuleName,
                                 currentStatus: $"Forwarded to {ModuleName}",
                                 actionRequired: "Please review and provide your comments",
                                 assignedBy: nonConformanceReport.ModuleAssignedBy,
                                 ncrGuid: nonConformanceReportVM.NCRGuid,
                                 serialNumbers: null 
                             );
                            return Json(new { failed = false, msg = "Assigned Successfuly" });
                        }

                    }


                }
                else
                {
                    return Json(new { failed = true, msg = "Failed to assign!" });
                }

            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { failed = true, msg = ex.Message }); }
        }

        [HttpGet]
        public IActionResult checkExistingAssignment(string NCRGUID)
        {
            string workflowGUID = "";
            var userguid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            NCR_Workflow_Assignment existingAssignment = _dbContext.NCR_Workflow_Assignments.Where(x => x.NCRGUID == NCRGUID).FirstOrDefault();
            if (existingAssignment == null)
            {
                existingAssignment = new NCR_Workflow_Assignment();
            }
            NCR_WorkflowVM vm = GetNCRWorkFlowData(NCRGUID, userguid, existingAssignment.NCRWorkflowGUID);
            bool isSerialNoPresent = true ? vm.nonConformanceReport_Items.Count() > 0 : false;
            if (existingAssignment != null)
            {
                workflowGUID = existingAssignment.NCRWorkflowGUID;
            }
            return Json(new { success = true, workflowguid = workflowGUID, isSerialNoPresent = isSerialNoPresent });

        }

        [HttpGet]
        public IActionResult NCRWorkflowAssignment(string NCRGUID, string workflowguid = "")
        {
            var UserGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
            NCR_WorkflowVM vm = GetNCRWorkFlowData(NCRGUID, UserGuid, workflowguid);
            return PartialView(vm);
        }

        private NCR_WorkflowVM GetNCRWorkFlowData(string NCRGUID, string userguid, string workflowguid = "")
        {
            try
            {
                NCR_WorkflowVM vm = new();
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var db = connection.QueryMultiple($" [dbo].[NCR_WorkFlow_SSP] @NCRGUID  ='{NCRGUID}', @assignmentID ='{workflowguid}', @userguid ='{userguid}'");
                    vm.nonConformanceReport = db.Read<NonConformanceReport>().FirstOrDefault();
                    vm.assignmentData = db.Read<NCR_Workflow_Assignment>().FirstOrDefault();
                    //vm.assignmentData = vm.assignmentData == null ? new() : vm.assignmentData;
                    vm.assignmentData = new();
                    vm.nonConformanceReport_Items = db.Read<NonConformanceReport_Item>().ToList();
                    return vm;
                }
            }

            catch (Exception ex) { ErrorHandler.LogException(ex); return new(); }
        }

        //FOWARDING IN SERIAL NUMBER LEVEL  BY MODULE USERS
        public IActionResult SaveNCRWorkflowAssignment([FromBody] NCR_Workflow_Assignment_ViewModel ncrAssignmentsVM)
        {
            try
            {
                var updatedByGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var ncrAssignments = JsonConvert.DeserializeObject<NCR_Workflow_Assignment>(JsonConvert.SerializeObject(ncrAssignmentsVM));
                var ModuleName = _dbContext.Master_Generals.Where(x => x.Master_Type == "Module_Responsibility" && x.is_active == 1 && x.Master_Dbkey == ncrAssignments.ModuleID).Select(x => x.Master_Name).FirstOrDefault();
                if (ncrAssignments != null)
                {
                    ncrAssignments.NCRGUID = ncrAssignmentsVM.NCRGUID;
                    ncrAssignments.AssignedOn = DateTime.Now;
                    ncrAssignments.AssignedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    ncrAssignments.Status = $"Forwarded to {ModuleName}";
                    _dbContext.Add(ncrAssignments);
                    _dbContext.SaveChanges();
                    string[] NCRItemKeysStr = ncrAssignmentsVM.NCRItemKeys.Split(',');
                    List<string> serialNumbersList = new List<string>();

                    foreach (var NCRItemKey in NCRItemKeysStr)
                    {

                        int NCRitemKey = int.Parse(NCRItemKey);

                        NonConformanceReport_Item nonConformanceReport_Item = _dbContext.NonConformanceReport_Items.Where(x => x.NCRItemKey == NCRitemKey).FirstOrDefault();
                        nonConformanceReport_Item.NCRWorkFlowGuid = ncrAssignments.NCRWorkflowGUID;
                        _dbContext.Entry(nonConformanceReport_Item).State = EntityState.Modified;

                        // Add serial number to list for email
                        serialNumbersList.Add(nonConformanceReport_Item.SerialNumber);

                        var log = new NCR_Workflow_Assignments_Log();
                        log.UpdatedBy = updatedByGuid;
                        log.UpdatedOn = DateTime.Now;
                        log.Status = $"Forwarded to {ModuleName}";
                        log.Status_Verbose = $"Forwarded serial number - {nonConformanceReport_Item.SerialNumber} to {ModuleName} module ";
                        log.Remarks = ncrAssignments.Remarks;
                        log.NCRWorkflowGUID = ncrAssignments.NCRWorkflowGUID;
                        log.IsTransfered = true;
                        log.NCRGuid = ncrAssignments.NCRGUID;
                        log.NCRItemKey = NCRitemKey;
                        _dbContext.Add(log);
                        // _dbContext.SaveChanges();
                    }
                    _dbContext.SaveChanges();

                    //mail info
                    var ncr = _dbContext.NonConformanceReports.Where(x => x.NCRGuid == ncrAssignments.NCRGUID).FirstOrDefault();
                    var users = _dbContext.AspNetUsers.ToList();
                    var filteredUsersEmails = users.Where(user => ncrAssignments.AssigneeUserGUIDs.Contains(user.Id)).Select(x => x.Email).ToArray();
                    var senderEmail = users.Where(x => x.Id == updatedByGuid).Select(x => x.Email).FirstOrDefault();

                    // NCREmailNotification(filteredUsersEmails, senderEmail, ncr.ReferenceNumber, ncrAssignments.AssignedBy);
                    SendNCRNotification(
                         recipientEmails: filteredUsersEmails,
                         senderEmail: senderEmail,
                         ncrReferenceNumber: ncr.ReferenceNumber,
                         moduleName: ModuleName,
                         currentStatus: $"Forwarded to {ModuleName} - Item Level Assignment",
                         actionRequired: "Please review and provide comments for the assigned serial numbers",
                         assignedBy: ncrAssignments.AssignedBy,
                         ncrAssignments.NCRGUID,
                         serialNumbers: serialNumbersList 
                     );
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { failed = true });
                }
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
        }

        [OrClaimRequirementAttribute(Constants.UserPermissions.NCR_Assignments_Admin, Constants.UserPermissions.NCR_Module_User)]
        public IActionResult AssignedNCRList(string NCRID = "All")
        {
            try
            {
                var Permission_Assignments_Admin = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Assignments_Admin);
                var Permission_Module_User = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Module_User);

                if (Permission_Assignments_Admin)
                {
                    var assignmentData = getAssignments(true, "All", NCRID);

                    return View(assignmentData);
                }

                if (Permission_Module_User)
                {
                    return View(getAssignments(false, "All", NCRID));
                }
                return RedirectToAction("UnAuthorized", "Auth"); // Redirect to Home/About action
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
        }

        /// <summary>
        /// V2 of the assignment list page — client-side 4-tab layout.
        /// Loads ALL data in one SP call; JS filters by tab instantly.
        /// Route: /NCR/AssignedNCRListV2?tab=PendingWithMe|InProgress|Closed|All
        /// </summary>
        [OrClaimRequirementAttribute(Constants.UserPermissions.NCR_Assignments_Admin, Constants.UserPermissions.NCR_Module_User)]
        public IActionResult AssignedNCRListV2(string NCRID = "All", string tab = "PendingWithMe")
        {
            try
            {
                var Permission_Assignments_Admin = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Assignments_Admin);
                var Permission_Module_User = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Module_User);

                if (!Permission_Assignments_Admin && !Permission_Module_User)
                    return RedirectToAction("UnAuthorized", "Auth");

                // Always fetch ALL rows — client-side JS handles tab filtering
                var pageData = getAssignmentsV2(Permission_Assignments_Admin, "All", NCRID, "All");

                // tab param only controls which tab is pre-selected in the UI
                if (tab != "PendingWithMe" && tab != "InProgress" && tab != "Closed" && tab != "All")
                    tab = "PendingWithMe";

                pageData.ActiveTab = tab;
                pageData.IsAdmin = Permission_Assignments_Admin;

                return View(pageData);
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
        }

        /// <summary>
        /// Assignment-list fetch for AssignedNCRListV2. Calls dbo.GetNcrAssignments_Data_V2,
        /// which returns two result sets:
        ///   1. The tab-filtered assignment rows (NCRAssignmentsVM) with
        ///      CurrentStatusText / CurrentStageModuleID / IsMyActionRequired
        ///      already computed in SQL.
        ///   2. One row per item for each NCR in set 1 (NCRItemStatusRow),
        ///      used to render the "Status of Deviations" column inline.
        ///
        /// Leaves the old getAssignments() helper untouched because
        /// AssignmentDetail and AssignedListData still depend on it.
        /// </summary>
        private AssignedNCRListPageVM getAssignmentsV2(bool admin, string NCRWorkflowGUID, string NCRGuid, string tab)
        {
            var result = new AssignedNCRListPageVM { ActiveTab = tab, IsAdmin = admin };
            try
            {
                var userGuid = admin
                    ? "All"
                    : User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var parameters = new
                    {
                        UserGuid        = userGuid ?? "All",
                        NCRGuid         = string.IsNullOrEmpty(NCRGuid)         ? "All" : NCRGuid,
                        NCRWorkflowGUID = string.IsNullOrEmpty(NCRWorkflowGUID) ? "All" : NCRWorkflowGUID,
                        Tab             = tab
                    };

                    using (var multi = connection.QueryMultiple(
                        "dbo.GetNcrAssignments_Data_V2",
                        parameters,
                        commandType: CommandType.StoredProcedure))
                    {
                        result.Assignments = multi.Read<NCRAssignmentsVM>().ToList();

                        var itemRows = multi.Read<NCRItemStatusRow>().ToList();
                        result.ItemMap = itemRows
                            .Where(x => !string.IsNullOrEmpty(x.NCRGuid))
                            .GroupBy(x => x.NCRGuid!)
                            .ToDictionary(g => g.Key, g => g.ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
            return result;
        }

        [OrClaimRequirementAttribute(Constants.UserPermissions.NCR_Assignments_Admin, Constants.UserPermissions.NCR_Module_User)]
        public IActionResult AssignmentDetail(string id, string NCRGuid)
        {
            try
            {
                var Permission_Assignments_Admin = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Assignments_Admin);
                var Permission_Module_User = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Module_User);
                var users = _dbContext.AspNetUsers.ToList();
                var UserGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
                var userDBKey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                ViewBag.UserGuid = UserGuid;
                ViewBag.userDBKey = userDBKey;
                AssignedNCRvm viewModel = new();

                List<NCRAssignmentsVM> assignmentDetail = new();
                if (Permission_Assignments_Admin)
                {
                    assignmentDetail = getAssignments(true, id, NCRGuid);
                }
                else if (Permission_Module_User)
                {
                    assignmentDetail = getAssignments(false, id, NCRGuid);
                }

                if (assignmentDetail.Count > 0)
                {
                    viewModel.AssignmentData = assignmentDetail.FirstOrDefault();
                    DataTable dt = MPGlobals.GetDataForDatatable($"[dbo].[NCR_SerialNo_status_Data_SSP] @NCRGuid ='{NCRGuid}',@assignmentID ='{id}'");
                    viewModel.NcrItems = JsonConvert.DeserializeObject<List<NonConformanceReport_ItemVM>>(JsonConvert.SerializeObject(dt));
                    viewModel.NcrItems = viewModel.NcrItems == null ? new() : viewModel.NcrItems;
                    // viewModel.workFlowLogs = _dbContext.NCR_Workflow_Assignments_Logs.Where(x => x.NCRGuid == NCRGuid && !x.Status.Contains("Deleted")).ToList();
                    viewModel.workFlowLogs = _dbContext.NCR_Workflow_Assignments_Logs.Where(x => x.NCRGuid == NCRGuid).ToList();
                    viewModel.workFlowLogs = viewModel.workFlowLogs == null ? new() : viewModel.workFlowLogs;

                    foreach (var item in viewModel.workFlowLogs)
                    {
                        var UserName = users.Where(x => x.Id == item.UpdatedBy).FirstOrDefault();
                        if (UserName != null)
                        {
                            viewModel.workFlowLogs.Where(x => x.NCRWorkflowLogsID == item.NCRWorkflowLogsID).ForEach(x => x.UpdatedBy = UserName.UserName);
                        }

                    }
                    return View(viewModel);
                }
                return View(viewModel = new());
                // return RedirectToAction("UnAuthorized", "Auth"); // Redirect to Home/About action
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

        }

        private List<NCRAssignmentsVM> getAssignments(bool admin, string NCRWorkflowGUID = "All", string NCRGuid = "All")
        {
            try
            {
                var UserGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
                List<NCRAssignmentsVM> assignmentsInfo = new List<NCRAssignmentsVM>();
                using (var connection = mPDapperContext.CreateConnection())
                {
                    //string cmdstr = $"[dbo].[GetNcrAssignments] @UserGuid = '{UserGuid}', @NCRWorkflowGUID ='{NCRWorkflowGUID}' ,@NCRGuid ='{NCRGuid}' ";
                    string cmdstr = $"[dbo].[GetNcrAssignments_Data]  @UserGuid = '{UserGuid}' , @NCRGuid = '{NCRGuid}' , @NCRWorkflowGUID = '{NCRWorkflowGUID}' ";
                    if (admin)
                    {
                        //cmdstr = $"[dbo].[GetNcrAssignments] @UserGuid = 'All', @NCRWorkflowGUID ='{NCRWorkflowGUID}', @NCRGuid ='{NCRGuid}' ";
                        cmdstr = $"[dbo].[GetNcrAssignments_Data]  @UserGuid = 'All' , @NCRGuid = '{NCRGuid}' , @NCRWorkflowGUID = '{NCRWorkflowGUID}' ";
                    }
                    var assignmentData = connection.QueryMultiple(cmdstr);
                    return assignmentData.Read<NCRAssignmentsVM>().ToList();
                }
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return new(); }
        }

        //[HttpPost]
        //public IActionResult SaveAssignmentLogs([FromBody] AssignedNCRvm assignedNCRvm)
        //{
        //    try
        //    {
        //        if (assignedNCRvm != null)
        //        {
        //            if (assignedNCRvm.NcrItems != null)
        //            {
        //                foreach (var item in assignedNCRvm.NcrItems)
        //                {
        //                    var itemrecord = _dbContext.NonConformanceReport_Items.Where(x => x.NCRItemKey == item.NCRItemKey).FirstOrDefault();
        //                    if (itemrecord != null)
        //                    {
        //                        if (!string.IsNullOrEmpty(item.Module_Remarks))
        //                        {
        //                            if (itemrecord.Module_Remarks != item.Module_Remarks)
        //                            {
        //                                itemrecord.Module_Remarks = item.Module_Remarks;
        //                                itemrecord.Module_UpdateBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //                                itemrecord.Module_UpdatedOn = DateTime.Now;
        //                                _dbContext.Entry(itemrecord).State = EntityState.Modified;
        //                            }
        //                        }
        //                        if (!string.IsNullOrEmpty(item.TAS_Remarks))
        //                        {
        //                            if (itemrecord.TAS_Remarks != item.TAS_Remarks)
        //                            {
        //                                itemrecord.TAS_Remarks = item.TAS_Remarks;
        //                                itemrecord.TAS_UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //                                itemrecord.TAS_UpdatedOn = DateTime.Now;
        //                                _dbContext.Entry(itemrecord).State = EntityState.Modified;
        //                            }
        //                        }
        //                        if (!string.IsNullOrEmpty(item.Stress_Remarks))
        //                        {
        //                            if (itemrecord.Stress_Remarks != item.Stress_Remarks)
        //                            {
        //                                itemrecord.Stress_Remarks = item.Stress_Remarks;
        //                                itemrecord.Stress_UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //                                itemrecord.Stress_UpdatedOn = DateTime.Now;
        //                                _dbContext.Entry(itemrecord).State = EntityState.Modified;
        //                            }
        //                        }
        //                        if (!string.IsNullOrEmpty(item.Chair_Remarks))
        //                        {
        //                            if (itemrecord.Chair_Remarks != item.Chair_Remarks)
        //                            {
        //                                itemrecord.Chair_Remarks = item.Chair_Remarks;
        //                                itemrecord.Chair_UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //                                itemrecord.Chair_UpdatedOn = DateTime.Now;
        //                                _dbContext.Entry(itemrecord).State = EntityState.Modified;
        //                            }
        //                        }

        //                        // adding entery to the worflow log table
        //                        NCR_Workflow_Assignments_Log workflowLog = new NCR_Workflow_Assignments_Log();
        //                        workflowLog.NCRGuid = itemrecord.NCRGuid;
        //                        workflowLog.NCRItemKey = itemrecord.NCRItemKey;
        //                        workflowLog.NCRWorkflowGUID = assignedNCRvm.AssignmentData.NCRWorkflowGUID;
        //                        workflowLog.Status = itemrecord.SerialNumber;
        //                        workflowLog.Remarks = itemrecord.Module_Remarks;
        //                        workflowLog.Status_Verbose = $"Updated Remarks for serial number {itemrecord.SerialNumber}";
        //                        workflowLog.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
        //                        workflowLog.UpdatedOn = DateTime.Now;
        //                        _dbContext.Add(workflowLog);
        //                    }
        //                }
        //            }
        //            _dbContext.SaveChanges();
        //            return Json(new { success = true });
        //        }
        //        return Json(new { success = false });
        //    }
        //    catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false }); }

        //}

       // delete complete workflow
        [ClaimRequirement(UserPermissions.NCR_Assignments_Admin_Delete)]
        public IActionResult DeleteNCRModuleAssignedData(string ncrGuid, string WorkFlowAssignmentGuid)
        {
            try
            {
                //removing workflow guid from items, if any
                List<NonConformanceReport_Item> nonConformanceReport_Item = _dbContext.NonConformanceReport_Items.Where(x => x.NCRGuid == ncrGuid).ToList();
                foreach (var ncrItems in nonConformanceReport_Item)
                {
                    if (ncrItems != null)
                    {
                        ncrItems.NCRWorkFlowGuid = null;
                        ncrItems.Status = null;

                        ncrItems.Module_Remarks = null;
                        ncrItems.Module_UpdateBy = null;
                        ncrItems.Module_UpdatedOn = null;
                        ncrItems.Module_Status = null;

                        ncrItems.TAS_Remarks = null;
                        ncrItems.TAS_Status = null;
                        ncrItems.TAS_UpdatedBy = null;
                        ncrItems.TAS_UpdatedOn = null;

                        ncrItems.Stress_Remarks = null;
                        ncrItems.Stress_Status = null;
                        ncrItems.Stress_UpdatedBy = null;
                        ncrItems.Stress_UpdatedOn = null;

                        ncrItems.Chair_Remarks = null;
                        ncrItems.Chair_Status = null;
                        ncrItems.Chair_UpdatedOn = null;
                        ncrItems.Chair_UpdatedBy = null;

                        ncrItems.Rework_Status = null;
                        ncrItems.Rework_MarkedBy = null;
                        ncrItems.Rework_MarkedModule = null;

                        ncrItems.Module_Rework_Status = null;
                        ncrItems.Module_Rework_Remarks = null;
                        ncrItems.Module_Rework_UpdatedBy = null;
                        ncrItems.Module_Rework_UpdatedOn = null;

                        ncrItems.TAS_Rework_Status = null;
                        ncrItems.TAS_Rework_Remarks = null;
                        ncrItems.TAS_Rework_UpdatedBy = null;
                        ncrItems.TAS_Rework_UpdatedOn = null;

                        ncrItems.STRESS_Rework_Status = null;
                        ncrItems.STRESS_Rework_Remarks = null;
                        ncrItems.STRESS_Rework_UpdatedBy = null;
                        ncrItems.STRESS_Rework_UpdatedOn = null;

                        ncrItems.CHAIR_Rework_Status = null;
                        ncrItems.CHAIR_Rework_Remarks = null;
                        ncrItems.CHAIR_Rework_UpdatedBy = null;
                        ncrItems.CHAIR_Rework_UpdatedOn = null;

                        ncrItems.Rework_Dimension = null;
                        ncrItems.STFE_PO_Remark = null;
                        ncrItems.STFE_PO_Remark_UpdatedBy = null;
                        ncrItems.STFE_PO_Remark_UpdatedOn = null;

                        var reworkRecords = _dbContext.NonConformanceReport_Item_Reworks
                                                      .Where(x => x.NCRItemKey == ncrItems.NCRItemKey && x.IsActive == true)
                                                      .ToList();
                        foreach (var rework in reworkRecords)
                        {
                            rework.IsActive = false;
                            _dbContext.Entry(rework).State = EntityState.Modified;
                        }

                        _dbContext.Entry(ncrItems).State = EntityState.Modified;
                    }
                }

                // deleteing workflow assignments
                List<NCR_Workflow_Assignment> nCR_Workflow_Assignments = _dbContext.NCR_Workflow_Assignments.Where(x => x.NCRGUID == ncrGuid).ToList();
                foreach (var assignments in nCR_Workflow_Assignments)
                {
                    if (assignments != null)
                    {
                        _dbContext.Remove(assignments);
                    }
                }

                // removing assigned module from ncr main table
                NonConformanceReport NCRDetails = _dbContext.NonConformanceReports.Where(x => x.NCRGuid == ncrGuid).FirstOrDefault();
                if (NCRDetails != null)
                {
                    NCRDetails.Module_Responsibilty = null;
                    NCRDetails.AssignedUserGuid = null;
                    NCRDetails.ModuleAssignedBy = null;
                    NCRDetails.ModuleAssignedOn = null;
                    NCRDetails.ReportStatus = null;
                    _dbContext.Entry(NCRDetails).State = EntityState.Modified;
                }

                //deleting logs
                List<NCR_Workflow_Assignments_Log> logs = _dbContext.NCR_Workflow_Assignments_Logs.Where(x => x.NCRGuid == ncrGuid).ToList();
                foreach (var log in logs)
                {
                    if (log != null)
                    {
                        _dbContext.Remove(log);
                    }
                }
                _dbContext.SaveChanges();

                return Json(new { success = true, msg = "Deleted Successfully" });
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false }); }

        }

        //revert transfer
        [ClaimRequirement(UserPermissions.NCR_Assignments_Module_Delete)]
        public IActionResult DeleteModuleAssignment(string ncrGuid, string WorkFlowAssignmentGuid, int ncrItemKey)
        {
            try
            {
                var UserOldDbKey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var userGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
                List<NonConformanceReport_Item> nonConformanceReport_Item = _dbContext.NonConformanceReport_Items.Where(x => x.NCRGuid == ncrGuid && x.NCRWorkFlowGuid == WorkFlowAssignmentGuid).ToList();
                NCR_Workflow_Assignment nCR_Workflow = _dbContext.NCR_Workflow_Assignments.Where(x => x.NCRGUID == ncrGuid && x.AssigneeUserGUIDs.Contains(userGuid)).FirstOrDefault();
                var revertNcrItem = nonConformanceReport_Item.Where(x => x.NCRItemKey == ncrItemKey).FirstOrDefault();
                if (revertNcrItem != null)
                {
                    if (nCR_Workflow != null)
                    {
                        revertNcrItem.NCRWorkFlowGuid = nCR_Workflow.NCRWorkflowGUID;//goes back to who assigned himself
                    }
                    else
                    {
                        revertNcrItem.NCRWorkFlowGuid = null; //  goes back to original module user
                    }
                    if (revertNcrItem.Chair_Status == "Forwarded To CHAIR")
                    {
                        revertNcrItem.Chair_Remarks = null;
                        revertNcrItem.Chair_UpdatedBy = null;
                        revertNcrItem.Chair_UpdatedOn = null;

                    }
                    else if (revertNcrItem.Stress_Status == "Forwarded To STRESS")
                    {
                        revertNcrItem.Stress_Remarks = null;
                        revertNcrItem.Stress_UpdatedBy = null;
                        revertNcrItem.Stress_UpdatedOn = null;
                    }
                    else if (revertNcrItem.TAS_Status == "Forwarded To TAS")
                    {
                        revertNcrItem.TAS_Remarks = null;
                        revertNcrItem.TAS_UpdatedBy = null;
                        revertNcrItem.TAS_UpdatedOn = null;
                    }
                    else
                    {
                        revertNcrItem.Module_Remarks = null;
                        revertNcrItem.Module_UpdateBy = null;
                        revertNcrItem.Module_UpdatedOn = null;
                    }
                    _dbContext.Entry(revertNcrItem).State = EntityState.Modified;

                    NCR_Workflow_Assignments_Log workflowLog = new NCR_Workflow_Assignments_Log();
                    workflowLog.NCRGuid = ncrGuid;
                    workflowLog.Status = "Reverted transfer";
                    workflowLog.Remarks = "Reverted transfer";
                    workflowLog.Status_Verbose = "Reverted transfer for serial number - " + revertNcrItem.SerialNumber;
                    workflowLog.NCRItemKey = revertNcrItem.NCRItemKey;
                    workflowLog.NCRWorkflowGUID = revertNcrItem.NCRWorkFlowGuid;
                    workflowLog.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
                    workflowLog.UpdatedOn = DateTime.Now;
                    _dbContext.Add(workflowLog);
                    _dbContext.SaveChanges();
                }

                //NCR_Workflow_Assignment ncrAssignments = _dbContext.NCR_Workflow_Assignments.Where(_x => _x.NCRWorkflowGUID == WorkFlowAssignmentGuid && _x.NCRGUID == ncrGuid).FirstOrDefault();
                //ncrAssignments.Status = null;
                //_dbContext.Entry(ncrAssignments).State = EntityState.Modified;

                List<NonConformanceReport_Item> updatesItems = _dbContext.NonConformanceReport_Items.Where(x => x.NCRGuid == ncrGuid && x.NCRWorkFlowGuid == WorkFlowAssignmentGuid).ToList();

                if (updatesItems.Count < 1)
                {
                    MPGlobals.ExceSQLNonQuery($"DELETE FROM [dbo].[NCR_Workflow_Assignments]      WHERE NCRWorkflowGUID = '{WorkFlowAssignmentGuid}' AND NCRGUID = '{ncrGuid}'");
                }
                return Json(new { success = true, msg = "Reverted Successfully" });

            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex); return Json(new { success = false });
            }

        }


        public IActionResult AssignedListData(string NcrGuid)
        {
            try
            {
                var assignmentData = getAssignments(false, "All", NcrGuid);
                return PartialView(assignmentData);
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false }); }
        }

        #endregion

        public async Task<IActionResult> AutoSaveRemarks(string remarksType, int ncrItemKey, string remarks)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(remarks))
                {
                    return Json(new { success = "invalid" });
                }

                var userDbKey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var userGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "[dbo].[NCR_AutoSaveRemarks]",
                        new
                        {
                            NCRItemKey = ncrItemKey,
                            RemarksType = remarksType,
                            Remarks = remarks,
                            UserDbKey = userDbKey,
                            UserGuid = userGuid
                        },
                        commandType: CommandType.StoredProcedure);

                    if (result != null)
                    {
                        string success = result.Success;
                        if (success == "true")
                            return Json(new { success = true });
                        else
                            return Json(new { success = "invalid" });
                    }
                    return Json(new { success = false });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> MarkAsAcceptedUnderConcession(string remarksType, int ncrItemKey)
        {
            try
            {
                var userDbKey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var userGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "[dbo].[NCR_MarkAsAcceptedUnderConcession]",
                        new
                        {
                            NCRItemKey = ncrItemKey,
                            RemarksType = remarksType,
                            UserDbKey = userDbKey,
                            UserGuid = userGuid
                        },
                        commandType: CommandType.StoredProcedure);

                    if (result != null && result.Success == 1)
                    {
                        return Json(new { success = true, finalRemarks = (string)result.FinalRemarks });
                    }
                    return Json(new { success = false, msg = result?.Message ?? "Failed" });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult MarkUpdatingRemarksAsCompleted([FromBody] List<NonConformanceReport_ItemVM> NcrItems)
        {
            try
            {
                if (NcrItems != null)
                {

                    var UserOldDbKey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    var userGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
                    var NcrGuid = NcrItems.Select(x => x.NCRGuid).FirstOrDefault();
                    var WorkFlowGuid = NcrItems.Where(x => x.NCRWorkFlowGuid != null).Select(x => x.NCRWorkFlowGuid).FirstOrDefault();
                    List<NonConformanceReport_Item> nonConformanceReport_Items = _dbContext.NonConformanceReport_Items.Where(x => x.NCRGuid == NcrGuid).ToList();
                    NCR_Workflow_Assignment assignment = new NCR_Workflow_Assignment();
                    NonConformanceReport ncr = _dbContext.NonConformanceReports.Where(x => x.NCRGuid == NcrGuid).FirstOrDefault();
                    //giving back to STFE
                    if (NcrItems.Count == nonConformanceReport_Items.Count
                       // || (NcrItems.Count == nonConformanceReport_Items.Count(x => x.Rework_Status == 1 || x.Rework_Status == 2)
                       || (NcrItems.Count == _dbContext.NonConformanceReport_Item_Reworks
                        .Count(x => nonConformanceReport_Items.Select(i => i.NCRItemKey).Contains(x.NCRItemKey)
                                 && x.IsActive == true
                                 && (x.ReworkType == 1 || x.ReworkType == 2))
                             && nonConformanceReport_Items.Any(x => !string.IsNullOrEmpty(x.Module_Rework_Status))
                             && nonConformanceReport_Items.All(x => string.IsNullOrEmpty(x.Chair_Status))))
                    {
                         if (ncr != null)
                        {
                            var AssigneeUserData = _dbContext.AspNetUsers.Where(x => x.OldUserDbkey == ncr.ModuleAssignedBy).FirstOrDefault();
                            assignment.NCRGUID = NcrGuid;
                            assignment.Status = "MarkedAsComplete";
                            if (AssigneeUserData != null)
                            {
                                var Module_User_Maping = _dbContext.NCR_ModuleToUserMappings.Where(x => x.UserGuid == AssigneeUserData.Id && x.Isactive == 1).FirstOrDefault();
                                assignment.AssigneeUserGUIDs = AssigneeUserData.Id;
                                assignment.AssignedBy = UserOldDbKey;
                                if (Module_User_Maping != null)
                                {
                                    assignment.ModuleID = Module_User_Maping.Module_ID;
                                }
                                else
                                {
                                    assignment.ModuleID = ncr.Module_Responsibilty;
                                }

                            }
                            assignment.AssignedOn = DateTime.Now;
                            _dbContext.Add(assignment);
                            _dbContext.SaveChanges();
                        }
                    }
                    //giving back to whoever refered
                    else
                    {
                        if (WorkFlowGuid != null)
                        {
                            NCR_Workflow_Assignment nCR_Workflow_Assignment = _dbContext.NCR_Workflow_Assignments.Where(x => x.NCRWorkflowGUID == WorkFlowGuid).FirstOrDefault();
                            if (nCR_Workflow_Assignment != null)
                            {
                                var AssigneeUserData = _dbContext.AspNetUsers.Where(x => x.OldUserDbkey == nCR_Workflow_Assignment.AssignedBy).FirstOrDefault();
                                var ReferedUSerWorkflow = _dbContext.NCR_Workflow_Assignments.Where(x => x.AssigneeUserGUIDs.Contains(AssigneeUserData.Id)).FirstOrDefault();

                                assignment.NCRGUID = NcrGuid;
                                assignment.Status = "MarkedAsComplete";
                                assignment.ModuleID = ReferedUSerWorkflow.ModuleID;
                                if (AssigneeUserData != null)
                                {
                                    assignment.AssigneeUserGUIDs = AssigneeUserData.Id.ToString();
                                    assignment.AssignedBy = AssigneeUserData.OldUserDbkey;
                                }

                                assignment.AssignedOn = DateTime.Now;
                                _dbContext.Add(assignment);
                                _dbContext.SaveChanges();
                            }
                        }
                    }
                    foreach (var item in NcrItems)
                    {
                        if (item.NCRItemKey != 0)
                        {
                            var itemToMark = nonConformanceReport_Items.Where(x => x.NCRItemKey == item.NCRItemKey).FirstOrDefault();
                            if (itemToMark != null)
                            {
                                itemToMark.NCRWorkFlowGuid = assignment.NCRWorkflowGUID;
                                if (item.remarksType == "Module_Remarks")
                                {
                                    itemToMark.Module_Status = "MarkedAsComplete";
                                }
                                else if (item.remarksType == "TAS_Remarks")
                                {
                                    itemToMark.TAS_Status = "MarkedAsComplete";
                                }
                                else if (item.remarksType == "Stress_Remarks")
                                {
                                    itemToMark.Stress_Status = "MarkedAsComplete";
                                }
                                else if (item.remarksType == "Chair_Remarks")
                                {
                                    itemToMark.Chair_Status = "MarkedAsComplete";
                                }
                                else if (item.remarksType == "Module_Rework_Remarks")
                                {
                                    itemToMark.Module_Rework_Status = "MarkedAsComplete";
                                }
                                else if (item.remarksType == "TAS_Rework_Remarks")
                                {
                                    itemToMark.TAS_Rework_Status = "MarkedAsComplete";
                                }
                                else if (item.remarksType == "STRESS_Rework_Remarks")
                                {
                                    itemToMark.STRESS_Rework_Status = "MarkedAsComplete";
                                }
                                _dbContext.Entry(itemToMark).State = EntityState.Modified;
                            }
                        }
                    }


                    NCR_Workflow_Assignments_Log workflowLog = new NCR_Workflow_Assignments_Log();
                    workflowLog.NCRGuid = NcrGuid;
                    workflowLog.Status = "Marked as Completed";
                    workflowLog.Remarks = "";
                    workflowLog.Status_Verbose = "Marked updating remarks as Completed ";
                    workflowLog.NCRWorkflowGUID = WorkFlowGuid;
                    workflowLog.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
                    workflowLog.UpdatedOn = DateTime.Now;
                    _dbContext.Add(workflowLog);
                    _dbContext.SaveChanges();

                    // Send completion notification email
                     if ( assignment.AssigneeUserGUIDs != null)
                    {
                        // Get completed serial numbers from the items we already have in loop
                        List<string> completedSerialNumbers = new List<string>();
                        foreach (var item in NcrItems)
                        {
                            var ncrItem = nonConformanceReport_Items.Where(x => x.NCRItemKey == item.NCRItemKey).FirstOrDefault();
                            if (ncrItem != null && !string.IsNullOrEmpty(ncrItem.SerialNumber))
                            {
                                completedSerialNumbers.Add(ncrItem.SerialNumber);
                            }
                        }

                        // Get recipient (the person who assigned it)
                        var users = _dbContext.AspNetUsers.ToList();
                        var assigneeUser = users.Where(x => x.Id == assignment.AssigneeUserGUIDs).FirstOrDefault();
                        string[] recipientEmails = assigneeUser != null ? new string[] { assigneeUser.Email } : new string[] { };

                        // Get sender (current user who marked as complete)
                        var senderUser = users.Where(x => x.Id == userGuid).FirstOrDefault();
                        string senderEmail = senderUser?.Email;

                        // Get module name
                        var moduleName = "Unknown";
                        if (assignment.ModuleID.HasValue)
                        {
                            moduleName = _dbContext.Master_Generals
                                .Where(x => x.Master_Dbkey == assignment.ModuleID.Value
                                         && x.Master_Type == "Module_Responsibility"
                                         && x.is_active == 1)
                                .Select(x => x.Master_Name)
                                .FirstOrDefault() ?? "Unknown";
                        }

                        if (recipientEmails.Length > 0)
                        {
                            SendNCRCompletionNotification(
                                recipientEmails: recipientEmails,
                                senderEmail: senderEmail,
                                ncrReferenceNumber: ncr.ReferenceNumber,
                                moduleName: moduleName,
                                completedBy: UserOldDbKey,
                                ncrGuid: NcrGuid,
                                serialNumbers: completedSerialNumbers
                            );
                        }
                    }


                    return Json(new { success = true });

                }

                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception ex)
            {
                {
                    ErrorHandler.LogException(ex);
                    return Json(new { success = false });

                }

            }
        }
        public IActionResult CloseNCRData(string NcrGuid)
        {
            try
            {
                var viewModel = new CloseNCRViewModel();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    using (var multi = connection.QueryMultiple(
                        "[dbo].[NCR_GetCloseNCRData]",
                        new { NCRGuid = NcrGuid },
                        commandType: CommandType.StoredProcedure))
                    {
                        viewModel.Header = multi.ReadFirstOrDefault<CloseNCRHeaderVM>();
                        viewModel.Items = multi.Read<CloseNCRItemVM>().ToList();
                    }
                }

                if (viewModel.Header == null)
                {
                    return Json(new { success = false, msg = "NCR not found" });
                }

                return View(viewModel);
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }
        }

        public IActionResult CloseNCR([FromBody] CloseNCRPostVM model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.NCRGuid))
                {
                    return Json(new { success = false, msg = "Invalid request data" });
                }

                // Validate ECM_No is always required
                if (string.IsNullOrWhiteSpace(model.ECM_No))
                {
                    return Json(new { success = false, msg = "ECM No. is required to close the NCR" });
                }

                // Check if rework/trial assembly exists and validate ECM_TR_NO
                NonConformanceReport nonConformanceReport = _dbContext.NonConformanceReports.Where(x => x.NCRGuid == model.NCRGuid).FirstOrDefault();
                if (nonConformanceReport == null)
                {
                    return Json(new { success = false, msg = "NCR not found" });
                }

                // Check rework existence for ECM_TR_NO validation
                bool hasRework = _dbContext.NonConformanceReport_Items
                    .Where(x => x.NCRGuid == model.NCRGuid)
                    .Any(x => x.Rework_Status == 1 || x.Rework_Status == 2)
                    || _dbContext.NonConformanceReport_Item_Reworks
                    .Any(x => _dbContext.NonConformanceReport_Items
                        .Where(i => i.NCRGuid == model.NCRGuid)
                        .Select(i => i.NCRItemKey)
                        .Contains(x.NCRItemKey)
                        && x.IsActive == true
                        && (x.ReworkType == 1 || x.ReworkType == 2));

                if (hasRework && string.IsNullOrWhiteSpace(model.ECM_TR_NO))
                {
                    return Json(new { success = false, msg = "ECM TR No. is required when Rework/Trial Assembly exists" });
                }

                // Validate all items have status
                if (model.Items == null || !model.Items.Any() || model.Items.Any(x => string.IsNullOrWhiteSpace(x.Status)))
                {
                    return Json(new { success = false, msg = "Select a valid status for all serial numbers" });
                }

                // Update NCR header
                nonConformanceReport.CloseNCR = 1;
                nonConformanceReport.ECM_TR_NO = model.ECM_TR_NO?.Trim();
                nonConformanceReport.ECM_No = model.ECM_No.Trim();

                // Calculate ReportStatus
                bool allRejected = model.Items.All(x => x.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase));
                bool allCleared = model.Items.All(x => x.Status.StartsWith("Cleared", StringComparison.OrdinalIgnoreCase));
                bool anyRejected = model.Items.Any(x => x.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase));

                // Use user-selected ReportStatus (auto-calculated on frontend, but editable)
                if (!string.IsNullOrWhiteSpace(model.ReportStatus))
                {
                    nonConformanceReport.ReportStatus = model.ReportStatus.Trim();
                }
                else if (allCleared)
                {
                    nonConformanceReport.ReportStatus = "Cleared";
                }
                else if (allRejected)
                {
                    nonConformanceReport.ReportStatus = "Rejected";
                }
                else if (anyRejected && !allRejected)
                {
                    nonConformanceReport.ReportStatus = "Processed - Partially Cleared";
                }

                _dbContext.Entry(nonConformanceReport).State = EntityState.Modified;

                // Update item statuses
                List<NonConformanceReport_Item> ncrItems = _dbContext.NonConformanceReport_Items
                    .Where(x => x.NCRGuid == model.NCRGuid).ToList();

                foreach (var viewModelItem in model.Items)
                {
                    var matchingItems = ncrItems.Where(x => x.SerialNumber == viewModelItem.SerialNumber);
                    foreach (var dbItem in matchingItems)
                    {
                        dbItem.Status = viewModelItem.Status;
                        dbItem.Chair_Status = "Ended";
                        _dbContext.Entry(dbItem).State = EntityState.Modified;
                    }
                }

                // Add workflow log
                NCR_Workflow_Assignments_Log workflowLog = new NCR_Workflow_Assignments_Log();
                workflowLog.NCRGuid = model.NCRGuid;
                workflowLog.Status = "NCR Closed";
                workflowLog.Remarks = $"ECM No: {model.ECM_No}" + (hasRework ? $", ECM TR No: {model.ECM_TR_NO}" : "");
                workflowLog.Status_Verbose = "NCR Closed";
                workflowLog.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
                workflowLog.UpdatedOn = DateTime.Now;
                _dbContext.Add(workflowLog);
                _dbContext.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }


        public IActionResult GetRawMaterialOfPart(int EnginePartDbkey)
        {
            DataTable dataTable = MPGlobals.GetDataForDatalist($"Select EPM.Raw_Material,isnull(EPU.Revision,EPM.Revision) as Revision  from [dbo].[Engine_Parts_Master] EPM Left Join dbo.Engine_Parts_Usage EPU on EPM.Engine_Part_Dbkey = EPU.Engine_Part_Dbkey and EPU.BL_Engine_Dbkey = (dbo.Get_Active_BL_Engine()) where EPM.Engine_Part_Dbkey = {EnginePartDbkey}");
            string jsonResult = JsonConvert.SerializeObject(dataTable);
            return Json(jsonResult);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRework(string remarksType, int NcrItemKey, int reworkType, string stage)
        {
            try
            {
                if (string.IsNullOrEmpty(stage))
                {
                    return Json(new { success = false, message = "Stage information is required" });
                }

                var userDbKey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var userGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "[dbo].[NCR_MarkAsRework]",
                        new
                        {
                            NCRItemKey = NcrItemKey,
                            ReworkType = reworkType,
                            Stage = stage,
                            UserDbKey = userDbKey,
                            UserGuid = userGuid
                        },
                        commandType: CommandType.StoredProcedure);

                    if (result != null && result.Success == 1)
                    {
                        // Send email in background — don't block the response
                        string recipientEmail = result.RecipientEmail;
                        string mailSubject = result.MailSubject;
                        string mailBody = result.MailBody;

                        if (!string.IsNullOrEmpty(recipientEmail))
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    EmailModel emailModel = new EmailModel();
                                    emailModel.Recipients = recipientEmail;
                                    emailModel.MailSubject = mailSubject;
                                    emailModel.MailBody = mailBody;
                                    emailModel.IsHTML = true;
                                    Utilities.Notification.SendEmail(emailModel);
                                }
                                catch (Exception emailEx)
                                {
                                    ErrorHandler.LogException(emailEx);
                                }
                            });
                        }

                        return Json(new { success = true });
                    }
                    return Json(new { success = false, message = result?.Message ?? "Failed" });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        //[HttpPost]
        //public IActionResult MarkAsRework(string remarksType, int NcrItemKey, int reworkType, )
        //{
        //    NonConformanceReport_Item nonConformanceReport_item = _dbContext.NonConformanceReport_Items.Where(x => x.NCRItemKey == NcrItemKey).FirstOrDefault();
        //    var userId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //    if (nonConformanceReport_item != null)
        //    {
        //        NonConformanceReport ncr = _dbContext.NonConformanceReports.Where(x => x.NCRGuid == nonConformanceReport_item.NCRGuid).FirstOrDefault();
        //        string MarkedFor = "";
        //        if (reworkType == 1)
        //        {
        //            nonConformanceReport_item.Rework_Status = 1; //marked for rework
        //            MarkedFor = "Marked for Rework";
        //        }
        //        else if (reworkType == 2)
        //        {
        //            nonConformanceReport_item.Rework_Status = 2; //marked for trial assembly
        //            MarkedFor = "Marked for Trial Assembly";
        //        }

        //        nonConformanceReport_item.Rework_MarkedBy = userId;
        //        if (nonConformanceReport_item.NCRWorkFlowGuid != null)
        //        {
        //            NCR_Workflow_Assignment assignment = _dbContext.NCR_Workflow_Assignments.Where(x => x.NCRWorkflowGUID == nonConformanceReport_item.NCRWorkFlowGuid).FirstOrDefault();
        //            if (assignment != null)
        //            {
        //                nonConformanceReport_item.Rework_MarkedModule = assignment.ModuleID;
        //            }
        //        }
        //        else
        //        {
        //            if (ncr != null)
        //            {
        //                nonConformanceReport_item.Rework_MarkedModule = ncr.Module_Responsibilty;
        //            }
        //        } 

        //        _dbContext.Entry(nonConformanceReport_item).State = EntityState.Modified;

        //        NCR_Workflow_Assignments_Log workflowLog = new NCR_Workflow_Assignments_Log();
        //        workflowLog.NCRGuid = nonConformanceReport_item.NCRGuid;
        //        workflowLog.Status = MarkedFor;
        //        workflowLog.Remarks = "";
        //        workflowLog.Status_Verbose = $"{MarkedFor} - Serial number - \"{nonConformanceReport_item.SerialNumber}\" ";
        //        workflowLog.NCRItemKey = nonConformanceReport_item.NCRItemKey;
        //        workflowLog.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
        //        workflowLog.UpdatedOn = DateTime.Now;
        //        _dbContext.Add(workflowLog);

        //        EmailModel emailModel = new EmailModel();
        //        AspNetUser userInfo = _dbContext.AspNetUsers.Where(x => x.OldUserDbkey == ncr.ModuleAssignedBy).FirstOrDefault();
        //        emailModel.Recipients = userInfo.Email;
        //        emailModel.MailSubject = $"NCR {MarkedFor}";
        //        StringBuilder sb = new StringBuilder();
        //        sb.Append(@"Dear User, <br> <br>");
        //        sb.Append($"Serial Number {nonConformanceReport_item.SerialNumber} under NCR Reference Number - {ncr.ReferenceNumber} has been {MarkedFor}. <br>");
        //        sb.Append($"You are requested to login to SHRAM portal and take necessary action <br>");
        //        sb.Append($"-Thank you, <br> STFE");
        //        emailModel.MailBody = sb.ToString();
        //        emailModel.IsHTML = true;
        //        Utilities.Notification.SendEmail(emailModel);

        //        DataTable dt = MPGlobals.GetDataForDatatable("Select * from Mail_Credentials");
        //        string SenderMail = "";
        //        if (dt.Rows.Count == 1)
        //        {
        //            SenderMail = dt.Rows[0].ItemArray[1].ToString();
        //        }
        //        Mailer_Log mailer_Log = new Mailer_Log();
        //        mailer_Log.MailFrom = SenderMail;
        //        mailer_Log.MailType = "NCR Assignment";
        //        mailer_Log.MailTo = emailModel.Recipients;
        //        mailer_Log.Subject = emailModel.MailSubject;
        //        mailer_Log.Body = emailModel.MailBody;
        //        mailer_Log.TriggerStatus = 1;
        //        mailer_Log.CreatedOn = DateTime.Now;
        //        mailer_Log.CreatedBy = userId;
        //        _dbContext.Mailer_Logs.Add(mailer_Log);
        //        _dbContext.SaveChanges();

        //    }
        //    return Json(new { success = true });
        //}

        public IActionResult PrintNCRModule(string NCRGuid)
        {

            try
            {
                var Permission_Assignments_Admin = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Assignments_Admin);
                var Permission_Module_User = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Module_User);
                var users = _dbContext.AspNetUsers.ToList();
                var UserGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
                var userDBKey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                ViewBag.UserGuid = UserGuid;
                ViewBag.userDBKey = userDBKey;
                AssignedNCRvm viewModel = new();

                List<NCRAssignmentsVM> assignmentDetail = new();
                if (Permission_Assignments_Admin)
                {
                    assignmentDetail = getAssignments(true, "All", NCRGuid);
                }
                else if (Permission_Module_User)
                {
                    assignmentDetail = getAssignments(false, "All", NCRGuid);
                }

                if (assignmentDetail.Count > 0)
                {
                    viewModel.AssignmentData = assignmentDetail.FirstOrDefault();
                    DataTable dt = MPGlobals.GetDataForDatatable($"[dbo].[NCR_SerialNo_status_Data_SSP] @NCRGuid ='{NCRGuid}',@assignmentID ='All'");
                    viewModel.NcrItems = JsonConvert.DeserializeObject<List<NonConformanceReport_ItemVM>>(JsonConvert.SerializeObject(dt));
                    viewModel.NcrItems = viewModel.NcrItems == null ? new() : viewModel.NcrItems;
                    // viewModel.workFlowLogs = _dbContext.NCR_Workflow_Assignments_Logs.Where(x => x.NCRGuid == NCRGuid && !x.Status.Contains("Deleted")).ToList();
                    viewModel.workFlowLogs = _dbContext.NCR_Workflow_Assignments_Logs.Where(x => x.NCRGuid == NCRGuid).ToList();
                    viewModel.workFlowLogs = viewModel.workFlowLogs == null ? new() : viewModel.workFlowLogs;

                    foreach (var item in viewModel.workFlowLogs)
                    {
                        var UserName = users.Where(x => x.Id == item.UpdatedBy).FirstOrDefault();
                        if (UserName != null)
                        {
                            viewModel.workFlowLogs.Where(x => x.NCRWorkflowLogsID == item.NCRWorkflowLogsID).ForEach(x => x.UpdatedBy = UserName.UserName);
                        }

                    }
                    DataTable assigneeNameDT = MPGlobals.GetDataForDatatable($"[dbo].NCR_AssignedModuleUserNames @NCRGuid ='{NCRGuid}'");
                    viewModel.ncrAssigneeName = JsonConvert.DeserializeObject<List<AssigneeUserName>>(JsonConvert.SerializeObject(assigneeNameDT));

                    return View(viewModel);
                }
                return View(viewModel = new());
                // return RedirectToAction("UnAuthorized", "Auth"); // Redirect to Home/About action
            }
            catch (Exception ex) { ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message }); }

        }

        public IActionResult UpdateRemarksAfterMarkAsComplete(string remarksType, int ncrItemKey, string remarks)
        {
            try
            {
                if (remarks.Trim().IsNullOrEmpty())
                {
                    return Json(new { success = "invalid" });
                }
                NonConformanceReport_Item ncrItem = _dbContext.NonConformanceReport_Items.Where(x => x.NCRItemKey == ncrItemKey).FirstOrDefault();
                if (ncrItem != null)
                {
                    NCR_Workflow_Assignment assignment = _dbContext.NCR_Workflow_Assignments.Where(x => x.NCRWorkflowGUID == ncrItem.NCRWorkFlowGuid).FirstOrDefault();
                    if (assignment != null)
                    {
                        Master_General master_General = _dbContext.Master_Generals.Where(x => x.Master_Dbkey == assignment.ModuleID).FirstOrDefault();
                        if (master_General != null)
                        {
                            if (remarksType == "Module_Remarks")
                            {
                                ncrItem.Module_Remarks = $" {ncrItem.Module_Remarks} <br/> {remarks} <i>- By {master_General.Master_Name}</i>";
                            }
                            else if (remarksType == "TAS_Remarks")
                            {
                                ncrItem.TAS_Remarks = $" {ncrItem.TAS_Remarks} <br/> {remarks} <i>- By {master_General.Master_Name}</i>";
                            }
                            else if (remarksType == "Stress_Remarks")
                            {
                                ncrItem.Stress_Remarks = $" {ncrItem.Stress_Remarks} <br/> {remarks} <i>- By {master_General.Master_Name}</i>";
                            }
                            else if (remarksType == "Chair_Remarks")
                            {
                                ncrItem.Chair_Remarks = $" {ncrItem.Chair_Remarks} <br/> {remarks} <i>- By {master_General.Master_Name}</i>";
                            }
                            //rework 
                            else if (remarksType == "Module_Rework_Remarks")
                            {
                                ncrItem.Module_Rework_Remarks = $" {ncrItem.Module_Rework_Remarks} <br/> {remarks} <i>- By {master_General.Master_Name}</i>";
                            }
                            else if (remarksType == "TAS_Rework_Remarks")
                            {
                                ncrItem.TAS_Rework_Remarks = $" {ncrItem.TAS_Rework_Remarks} <br/> {remarks} <i>- By {master_General.Master_Name}</i>";
                            }
                            else if (remarksType == "STRESS_Rework_Remarks")
                            {
                                ncrItem.STRESS_Rework_Remarks = $" {ncrItem.STRESS_Rework_Remarks} <br/> {remarks} <i>- By {master_General.Master_Name}</i>";
                            }
                            _dbContext.Entry(ncrItem).State = EntityState.Modified;
                            _dbContext.SaveChanges();

                        }
                    }

                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false });
            }

        }

        public IActionResult MArkSerialNoStatus([FromBody] List<NonConformanceReport_ItemVM> viewmodel)
        {
            var NcrGuid = viewmodel.FirstOrDefault().NCRGuid;
            if (!NcrGuid.IsNullOrEmpty())
            {
                NonConformanceReport nonConformanceReport = _dbContext.NonConformanceReports.Where(x => x.NCRGuid == NcrGuid).FirstOrDefault();
                if (nonConformanceReport != null)
                {
                    List<NonConformanceReport_Item> ncrItems = _dbContext.NonConformanceReport_Items.Where(x => x.NCRGuid == nonConformanceReport.NCRGuid).ToList();

                    //bool allRejected = viewmodel.All(x => x.Status != null && x.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase));
                    //bool allCleared = viewmodel.All(x => x.Status != null && x.Status.StartsWith("Cleared", StringComparison.OrdinalIgnoreCase));
                    //bool anyRejected = viewmodel.Any(x => x.Status != null && x.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase));
                    //if (allCleared)
                    //{
                    //	nonConformanceReport.ReportStatus = "Cleared";
                    //}
                    //else if (allRejected)
                    //{
                    //	nonConformanceReport.ReportStatus = "Rejected";
                    //}
                    //else if (anyRejected == true && allRejected == false)
                    //{
                    //	nonConformanceReport.ReportStatus = "Processed - Partially Cleared";
                    //}

                    foreach (var viewModelItem in viewmodel)
                    {
                        // Find matching ncrItems by SerialNumber
                        var matchingItems = ncrItems.Where(x => x.SerialNumber == viewModelItem.SerialNumber);

                        foreach (var dbItem in matchingItems)
                        {
                            dbItem.Status = viewModelItem.Status;
                            //dbItem.Chair_Status = "Ended";
                            _dbContext.Entry(dbItem).State = EntityState.Modified;
                        }
                    }

                    //	_dbContext.Entry(nonConformanceReport).State = EntityState.Modified;

                    NCR_Workflow_Assignments_Log workflowLog = new NCR_Workflow_Assignments_Log();
                    workflowLog.NCRGuid = NcrGuid;
                    workflowLog.Status = "Marked Serial Number status";
                    workflowLog.Remarks = "Marked Serial Number status";
                    workflowLog.Status_Verbose = "Status for individual serial number marked by Chair";
                    workflowLog.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
                    workflowLog.UpdatedOn = DateTime.Now;
                    _dbContext.Add(workflowLog);
                    _dbContext.SaveChanges();
                    return Json(new { success = true });
                }

            }
            return Json(new { success = false });
        }

        //[HttpPost]
        //public IActionResult UndoReworkMarking(int NcrItemKey)
        //{
        //    try
        //    {
        //        NonConformanceReport_Item nonConformanceReport_item = _dbContext.NonConformanceReport_Items
        //            .Where(x => x.NCRItemKey == NcrItemKey)
        //            .FirstOrDefault();

        //        if (nonConformanceReport_item != null)
        //        {
        //            var userId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //            var userGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

        //            // Store the status before clearing (for log message)
        //            string previousStatus = nonConformanceReport_item.Rework_Status == 1
        //                ? "Rework"
        //                : "Trial Assembly";

        //            // Clear the rework status
        //            nonConformanceReport_item.Rework_Status = null;
        //            nonConformanceReport_item.Rework_MarkedBy = null;
        //            nonConformanceReport_item.Rework_MarkedModule = null;

        //            _dbContext.Entry(nonConformanceReport_item).State = EntityState.Modified;

        //            // Add log entry
        //            NCR_Workflow_Assignments_Log log = new NCR_Workflow_Assignments_Log();
        //            log.NCRWorkflowGUID = nonConformanceReport_item.NCRWorkFlowGuid;
        //            log.Status = $"Unmarked from {previousStatus}";
        //            log.IsTransfered = false;
        //            log.Status_Verbose = $"Unmarked serial number {nonConformanceReport_item.SerialNumber} from {previousStatus}";
        //            log.UpdatedBy = userGuid;
        //            log.UpdatedOn = DateTime.Now;
        //            log.NCRGuid = nonConformanceReport_item.NCRGuid;
        //            log.NCRItemKey = NcrItemKey;

        //            _dbContext.Add(log);
        //            _dbContext.SaveChanges();

        //            return Json(new { success = true });
        //        }

        //        return Json(new { success = false, msg = "Item not found" });
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorHandler.LogException(ex);
        //        return Json(new { success = false, msg = ex.Message });
        //    }
        //}

        [HttpPost]
        public IActionResult UndoReworkMarking(int NcrItemKey, string stage)
        {
            try
            {
                // Validate stage parameter
                if (string.IsNullOrEmpty(stage))
                {
                    return Json(new { success = false, message = "Stage information is required" });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var userGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                // Find the rework record for this item and stage
                var reworkRecord = _dbContext.NonConformanceReport_Item_Reworks
                    .Where(x => x.NCRItemKey == NcrItemKey
                             && x.StageName == stage
                             && x.IsActive == true)
                    .FirstOrDefault();

                if (reworkRecord == null)
                {
                    return Json(new { success = false, message = "No rework marking found for this stage" });
                }

                // Get the NCR item for logging
                var nonConformanceReport_item = _dbContext.NonConformanceReport_Items
                    .Where(x => x.NCRItemKey == NcrItemKey)
                    .FirstOrDefault();

                if (nonConformanceReport_item == null)
                {
                    return Json(new { success = false, message = "NCR Item not found" });
                }

                // Store status before clearing
                string previousStatus = reworkRecord.ReworkType == 1 ? "Rework" : "Trial Assembly";

                // Deactivate the rework record
                reworkRecord.IsActive = false;
                _dbContext.Entry(reworkRecord).State = EntityState.Modified;

                // Log the action
                var workflowLog = new NCR_Workflow_Assignments_Log
                {
                    NCRGuid = nonConformanceReport_item.NCRGuid,
                    NCRItemKey = NcrItemKey,
                    Status = $"Unmarked from {previousStatus}",
                    Status_Verbose = $"Item unmarked from {previousStatus} - Serial number: \"{nonConformanceReport_item.SerialNumber}\"",
                    UpdatedBy = userGuid,
                    UpdatedOn = DateTime.Now
                };
                _dbContext.NCR_Workflow_Assignments_Logs.Add(workflowLog);

                _dbContext.SaveChanges();

                return Json(new { success = true, message = $"Successfully unmarked from {previousStatus}" });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, message = "An error occurred while unmarking item" });
            }
        }

        #region Serial no - engine mapping and report and summary
        public IActionResult SerialNumberEngineMapping(string NCRGUID)
        {
            List<NCR_SerialNumber_Engine_Mapping> data = new List<NCR_SerialNumber_Engine_Mapping>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($" GetNCR_SerialNumber_Engine_Mapping @NCR_GUId  = '{NCRGUID}' ");
                data = db.Read<NCR_SerialNumber_Engine_Mapping>().ToList();
            }
            return PartialView(data);
        }

        [HttpPost]
        public IActionResult SaveSerialNumberEngineMapping(string NCRGUID, string mappingData)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@NCR_GUId", NCRGUID);
                    parameters.Add("@MappingData", mappingData);
                    parameters.Add("@UpdatedBy", userId);

                    var result = connection.QueryFirstOrDefault<dynamic>(
                        "SaveNCR_SerialNumber_Engine_Mapping",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    if (result != null && result.Success == 1)
                    {
                        return Json(new { success = true, message = result.Message });
                    }
                    else
                    {
                        return Json(new { success = false, message = result?.Message ?? "Error saving mappings" });
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // =============================================
        // FILE: Controllers/NCRController.cs
        // ACTION: REPLACE the existing NCR_PartEngine_Report() method with this
        // =============================================

        public IActionResult NCR_PartEngine_Report()
        {

            var tableViewModel = new NCRPartEngineReportViewModel();
            var statusOverview = new NCRStatusOverviewVM();
            var engineDistribution = new NCREngineDistributionVM();
            var duplicateDeviations = new List<DuplicateDeviationItem>();

            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    // 1. Table View - Part-Engine pivot
                    var tableResult = connection.Query("GetNCR_PartEngine_Report",
                        new { BL_Engine_Dbkey = 0, Engine_Dbkey = 0 },
                        commandType: CommandType.StoredProcedure).ToList();

                    if (tableResult.Any())
                    {
                        var firstRow = tableResult.First() as IDictionary<string, object>;
                        tableViewModel.ColumnHeaders = firstRow.Keys.ToList();

                        foreach (var row in tableResult)
                        {
                            var rowDict = new Dictionary<string, string>();
                            var rowData = row as IDictionary<string, object>;
                            foreach (var key in rowData.Keys)
                            {
                                rowDict[key] = rowData[key]?.ToString() ?? "-";
                            }
                            tableViewModel.ReportData.Add(rowDict);
                        }
                    }

                    // 2. Status Overview (2 result sets)
                    var statusMulti = connection.QueryMultiple("GetNCR_StatusOverview",
                        commandType: CommandType.StoredProcedure);
                    statusOverview.ReportStatusCounts = statusMulti.Read<ReportStatusCount>().ToList();
                    statusOverview.WorkflowStageCounts = statusMulti.Read<WorkflowStageCount>().ToList();

                    // 3. Engine Distribution (2 result sets)
                    var engineMulti = connection.QueryMultiple("GetNCR_EngineDistribution",
                        commandType: CommandType.StoredProcedure);
                    engineDistribution.EngineStatusCounts = engineMulti.Read<EngineReportStatusCount>().ToList();
                    engineDistribution.EngineWorkflowCounts = engineMulti.Read<EngineWorkflowStageCount>().ToList();

                    // 4. Duplicate Deviations
                    duplicateDeviations = connection.Query<DuplicateDeviationItem>(
                        "GetNCR_DuplicateDeviations",
                        commandType: CommandType.StoredProcedure
                    ).ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }

            ViewBag.StatusOverview = statusOverview;
            ViewBag.EngineDistribution = engineDistribution;
            ViewBag.DuplicateDeviations = duplicateDeviations;
            

            return View(tableViewModel);
        }

        //public IActionResult NCR_PartEngine_Summary()
        //{
        //    var viewModel = new NCRSummaryViewModel();

        //    try
        //    {
        //        using (var connection = mPDapperContext.CreateConnection())
        //        {
        //            var result = connection.Query<EngineNCRCount>(
        //                "GetNCR_Summary_ByEngine",
        //                commandType: CommandType.StoredProcedure
        //            ).ToList();

        //            viewModel.EngineSummary = result;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log error if you have logging setup
        //        // For now, return empty list
        //        viewModel.EngineSummary = new List<EngineNCRCount>();
        //    }

        //    return View(viewModel);
        //}

        // =============================================
        // FILE: Controllers/NCRController.cs
        // ACTION: REPLACE the GetNCRsByWorkflowStage method with this
        // =============================================

        [HttpGet]
        public IActionResult GetNCRsByWorkflowStage(string stage, string source = "overview", string engine = null, string type = "workflow")
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = connection.Query(
                        "GetNCR_ByWorkflowStage",
                        new { Stage = stage, Source = source, Engine = engine, Type = type },
                        commandType: CommandType.StoredProcedure
                    ).ToList();

                    return Json(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, message = ex.Message });
            }
        }


        #endregion




        [ClaimRequirement(Constants.UserPermissions.NCR_Assignments_Readonly)]
        public IActionResult ReadOnlyNCRList(string NCRID = "All")
        {
            try
            {
                // Get all NCR assignments using admin privilege (to see all records)
                var assignmentData = getAssignments(true, "All", NCRID); 
                return View(assignmentData);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [ClaimRequirement(Constants.UserPermissions.NCR_Assignments_Readonly)]
        public IActionResult ViewReadOnlyNCR(string id, string NCRGuid)
        {
            try
            {
                var users = _dbContext.AspNetUsers.ToList();
                var UserGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
                var userDBKey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                ViewBag.UserGuid = UserGuid;
                ViewBag.userDBKey = userDBKey;

                AssignedNCRvm viewModel = new();

                // Get all assignments using admin privilege (to see all records)
                List<NCRAssignmentsVM> assignmentDetail = getAssignments(true, id, NCRGuid);

                if (assignmentDetail.Count > 0)
                {
                    viewModel.AssignmentData = assignmentDetail.FirstOrDefault();
                    DataTable dt = MPGlobals.GetDataForDatatable($"[dbo].[NCR_SerialNo_status_Data_SSP] @NCRGuid ='{NCRGuid}',@assignmentID ='{id}'");
                    viewModel.NcrItems = JsonConvert.DeserializeObject<List<NonConformanceReport_ItemVM>>(JsonConvert.SerializeObject(dt));
                    viewModel.NcrItems = viewModel.NcrItems == null ? new() : viewModel.NcrItems;

                    viewModel.workFlowLogs = _dbContext.NCR_Workflow_Assignments_Logs.Where(x => x.NCRGuid == NCRGuid).ToList();
                    viewModel.workFlowLogs = viewModel.workFlowLogs == null ? new() : viewModel.workFlowLogs;

                    foreach (var item in viewModel.workFlowLogs)
                    {
                        var UserName = users.Where(x => x.Id == item.UpdatedBy).FirstOrDefault();
                        if (UserName != null)
                        {
                            viewModel.workFlowLogs.Where(x => x.NCRWorkflowLogsID == item.NCRWorkflowLogsID).ForEach(x => x.UpdatedBy = UserName.UserName);
                        }
                    }
                    return View(viewModel);
                }
                return View(viewModel = new());
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.NCR_Read)]
        public IActionResult ModuleWiseNCRList()
        {
            try
            {
                ModuleWiseNCRListVM vm = new ModuleWiseNCRListVM();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var multi = connection.QueryMultiple(
                        "[dbo].[GetModuleWiseNCRList]",
                        commandType: CommandType.StoredProcedure
                    );

                    vm.Items = multi.Read<ModuleWiseNCRListItemVM>().ToList();
                    vm.Summary = multi.Read<ModuleWiseNCRSummaryVM>().ToList();
                }

                return View(vm);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.NCR_Read)]
        public IActionResult GetModuleWiseNCRPopupDetails(string moduleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(moduleName))
                {
                    return Json(new { success = false, msg = "Module name is required" });
                }

                ModuleWiseNCRPopupVM vm = new ModuleWiseNCRPopupVM();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var multi = connection.QueryMultiple(
                        "[dbo].[GetModuleWiseNCRPopupDetails]",
                        new { ModuleName = moduleName },
                        commandType: CommandType.StoredProcedure
                    );

                    vm.Summary = multi.Read<ModuleWiseNCRPopupSummaryVM>().FirstOrDefault() ?? new ModuleWiseNCRPopupSummaryVM();
                    vm.Items = multi.Read<ModuleWiseNCRPopupItemVM>().ToList();
                }

                return Json(new { success = true, data = vm });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }


    }
}
