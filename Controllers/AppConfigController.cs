using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using System.Data;

namespace MPCRS.Controllers
{
    public class AppConfigController : Controller
    {
        public IActionResult Index()
        {
            try
            {
				MPCRS.Utilities.DataCaching.RemoveAllCache();
			}
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
            return Json(new { success = true });
        }
        public IActionResult Errors()
        {
            List<ExceptionLog> errorslist = new();
            MPDapperContext mPDapperContext = new();
            using (DESI_STFE_PRODContext db =new())
            {
                try
                {
                    string cmdstr = $"select * from ExceptionLog ORDER BY ErrorLine ASC";
                    DataTable dt = MPGlobals.GetDataForDatalist(cmdstr);
                    if (dt.Rows.Count > 0)
                    {
                        errorslist = JsonConvert.DeserializeObject<List<ExceptionLog>>(JsonConvert.SerializeObject(dt));
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }

                return View(errorslist);
            }
        }
    }
}
