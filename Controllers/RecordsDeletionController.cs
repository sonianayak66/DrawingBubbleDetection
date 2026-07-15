using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPCRS.Models;
using MPCRS.ViewModels;
using System.Security.Claims;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Controllers
{
    [Authorize]
    public class RecordsDeletionController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;
        public RecordsDeletionController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }
        [ClaimRequirement(UserPermissions.Record_Deletion)]
        public IActionResult Index()
        { 
            List<DeletionManagementVM> castingViewModel = new List<DeletionManagementVM>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.DeletionRequests_SSP");
                castingViewModel = db.Read<DeletionManagementVM>().ToList();
            }
            return View(castingViewModel); 
        }

    
        [HttpPost]
        public IActionResult CreateApprovalRequest([FromBody] DeletionManagementVM requestData)
        {
            return View(requestData);
        }
 
        [HttpPost]
        public IActionResult SubmitApprovalRequest([FromBody] RecordsForDeletion requestData)
        {
            var existingRecord = _dbContext.RecordsForDeletions.Where(x => x.SourceTableName == requestData.SourceTableName
                                                && x.SourceTableKey == requestData.SourceTableKey && x.ApprovalStatus == "Requested").FirstOrDefault();

            requestData.InitiatedBy = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            requestData.InitiatedOn = DateTime.Now;
            requestData.ApprovalStatus = "Requested";

            if (existingRecord == null)
            {
                _dbContext.RecordsForDeletions.Add(requestData);
                _dbContext.SaveChanges();
            }
            return Json(new { success = true, msg = "Submitted Successfully" });
        }

        [ClaimRequirement(UserPermissions.Record_Deletion)]
        [HttpGet]
        public IActionResult RequestAction(int DeletionKey = 0, string UserAction = "")
        {

            var existingRecord = _dbContext.RecordsForDeletions.Where(x => x.DeletionKey == DeletionKey).FirstOrDefault();
            if(existingRecord != null)
            {
                string userGUID = User.FindFirst(ClaimTypes.NameIdentifier).Value; ;
                string cmdStr = $"EXEC dbo.RecordDeletionAction  @DeletionKey={DeletionKey}, @Action='{UserAction}', @userGUID='{userGUID}'";
                MPCRS.Utilities.MPGlobals.ExceSQLNonQuery(cmdStr);
            }
            return Json(new { success = true, msg = "Deleted Successfully" });
        }




    }
}
