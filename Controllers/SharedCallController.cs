using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPCRS.Utilities;
using System.Data;


namespace MPCRS.Controllers
{
    [Authorize]
    public class SharedCallController : Controller
    {
        public ActionResult Audit_Logs(string id = "", int viewID = 0, int partID = 0, int engineDbkey = 0)
        {
            ViewBag.viewID = viewID;
            ViewBag.partID = partID;
            ViewBag.FilterID = id;
            ViewBag.engineDbkey = engineDbkey;
            return View();
        }

        public ActionResult GetAuditLogs(string id = "", int partID = 0, int engineDbkey = 0)
        {
            DataTable dataTable = new DataTable();
            if (partID == 0 && engineDbkey == 0)
            {
                dataTable = MPGlobals.GetDataForDatalist("dbo.Get_Audit_Logs @tatbleName = '" + id + "',@StatusID = 0, @PartID =" + partID + ",@Engine_Dbkey = " + engineDbkey + "");
            }
            else
            {
                dataTable = MPGlobals.GetDataForDatalist("dbo.Get_Audit_Logs @tatbleName = '" + id + "',@StatusID = 0, @PartID =" + partID + ",@Engine_Dbkey = " + engineDbkey + "");
            }

            return Json(MPGlobals.GetTableAsList(dataTable));
        }

        public ActionResult GetMasterJResult(string type)
        {
            DataTable Dt = Masters.GetMaster_General_Jresult(type);
            return Json(MPGlobals.GetTableAsList(Dt));
        }


        public ActionResult GetMetaMasterJResult(string type)
        {
            DataTable Dt = MPGlobals.GetDataForDatalist($"Select * from [MetaMaster] where [MasterType] ='{type}' ");
            return Json(MPGlobals.GetTableAsList(Dt));
        }

        [Authorize]
        public ActionResult GetRawmaterialParaJResult(int id, string type)
        {
            DataTable Dt = Masters.GetRawmaterialParaJResult(id, type);
            return Json(MPGlobals.GetTableAsList(Dt));
        }

        [Authorize]
        public ActionResult GetRawmaterialItemtype(int id, string type)
        {
            string items = MPGlobals.GetOnedata("SELECT (SELECT [Master_Name] FROM [dbo].[Master_General] where [Master_Dbkey] = [RM_Type]) FROM [dbo].[Master_Rawmaterials] where [Raw_material_Dbkey] = " + id + "");
            return Json(new { itemtype = items });
        }

    }
}
