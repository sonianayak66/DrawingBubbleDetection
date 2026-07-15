using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPCRS.Utilities;
using Newtonsoft.Json;
using System.Data;
using System.Security.Claims;
using static MPCRS.Utilities.Constants;


namespace MPCRS.Controllers
{
    [Authorize]
    public class ApprovalManagementController : Controller
    {

        [ClaimRequirement(UserPermissions.Approval_Management_Read_Requests)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

       
        [Authorize]
        public string GetAuditLogsforApprovals(int id = 0)
        {
            var Approve_Management_Approve_Permisson = UserData.IsAuthorized(User, Constants.UserPermissions.Approval_Management_Approve_Requests);
            int userid = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
            DataTable dataTable = MPGlobals.GetDataForDatalist("dbo.Get_Audit_Logs @tatbleName = '',@StatusID = " + id + ", @PartID = 0, @Engine_Dbkey = 0,@userid = "+ userid + "");
            //return Json(MPGlobals.GetTableAsList(dataTable));
            var jsonData = JsonConvert.SerializeObject(dataTable);
            return jsonData ;
        }

        [ClaimRequirement(UserPermissions.Approval_Management_Approve_Requests)]
        [HttpGet]
        public ActionResult CompareAuditChanges(int Log_Db_Key)
        {
            ViewBag.Log_Db_Key = Log_Db_Key;
            DataTable dataTable = MPGlobals.GetDataForDatalist("SELECT [Approval_Status],[Event_Description] FROM [dbo].[Audit_logs] where [Log_Db_Key] =" + Log_Db_Key + " ");
            ViewBag.Status = dataTable.Rows[0]["Approval_Status"];
            ViewBag.Event = dataTable.Rows[0]["Event_Description"];
            return PartialView();
        }

        [Authorize]
        [HttpGet]
        public string GetComparisionData(int Log_Db_Key)
        {
            DataTable dataTable1 = MPGlobals.GetDataForDatalist($"[dbo].[Get_AuditLogData] @logDbKey = {Log_Db_Key}");
            //return Json(MPGlobals.GetTableAsList(dataTable1));
            return JsonConvert.SerializeObject(dataTable1);
        }

        [Authorize]
        public ActionResult SaveApprovalRequest(int Log_Db_Key, int status)
        {
            string Message = "Something went wrong. Please try later";
            try
            {                
                int userID = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                DataTable dataTable = MPGlobals.GetDataForDatalist($"dbo.ValidateApprovalAction_SSp @Log_Db_Key = {Log_Db_Key}");

                if (dataTable.Rows.Count > 0)
                {
                    Message = dataTable.Rows[0][0].ToString();
                    return Json(new { success = false, msg = Message });
                }


                if (status == 9)
                {
                    Message = "Approved Successfully";
                }
                else
                {
                    Message = "Rejected Successfully";
                }

                MPGlobals.ExceSQLNonQuery("dbo.UpdateAuditLogStatus @Log_Db_Key =" + Log_Db_Key + ",@status = " + status + ",@user = " + userID + " ");
                return Json(new { success = true, msg = Message });
            }
            catch (Exception ex)
            { 
                return Json(new { success = false, msg = Message });
            } 
        }

        [Authorize]
        [HttpGet]
        public string AuditLogDisplayManager(string SourceTable)
        {
            DataTable dataTable1 = MPGlobals.GetDataForDatalist($"SELECT [ColumnName],[Display_ColumnName],[DisplayData],[DisplayOrder],[Force_Display_Data] FROM [dbo].[AuditLogDisplayManager] where [SourceTable] = '{SourceTable}'");           
            var jsonData = JsonConvert.SerializeObject(dataTable1);
            return jsonData;
        }
    }
}
