using Dapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using System.Data;
using System.Diagnostics;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;

        public HomeController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext, ILogger<HomeController> logger)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy(string buidguid)
        {
            return View();
        }

        public IActionResult batchsearch(string searchText)
        {
            ViewBag.batchnumber = searchText?.Trim();
            return View();
        }

        public IActionResult RawMaterialSearch(string searchText)
        {
            ViewBag.rawmaterial = searchText?.Trim();
            return View();
        }

        public IActionResult MMGNumberSearch(string searchText)
        {
            ViewBag.mmgnumber = searchText?.Trim();
            return View();
        }
        
        public IActionResult Search(int partnumber)
        {
            using (var dbContext = _dbContext) { 
                Engine_Parts_Master engine_Parts_Master = dbContext.Engine_Parts_Masters.FirstOrDefault(x => x.Engine_Part_Dbkey == partnumber);
                if (engine_Parts_Master != null)
                {
                    ViewBag.PartNumber = engine_Parts_Master.Draw_part_no;
                    HttpContext.Items["Title"] ="-" + engine_Parts_Master.Draw_part_no;
                }
                else
                {
                    ViewBag.PartNumber = "NOT FOUND";
                }
        
            }
         
            return View();
        }

        public IActionResult JobcardSearch (string searchText)
        {
            ViewBag.JobCardNumber = searchText?.Trim();
            return View();
        }


        [HttpGet]
        public IActionResult VendorRawMaterialDetails(int vendorDbkey, int rawMaterialDbkey)
        {
            try
            {
                VendorMaterialDetailsPopupVM viewModel = new VendorMaterialDetailsPopupVM();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = connection.QueryMultiple(
                        "[dbo].[VendorRawMaterialDetails_SSP]",
                        new
                        {
                            Vendor_Dbkey = vendorDbkey,
                            Raw_material_Dbkey = rawMaterialDbkey
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    // Read names (first result set)
                    var names = result.ReadFirstOrDefault<dynamic>();

                    // Read summary (second result set)
                    var summary = result.ReadFirstOrDefault<VendorMaterialSummary>();

                    // Read procurement demands (third result set)
                    var demands = result.Read<DemandDetail>().ToList();

                    // Read material issues (fourth result set)
                    var issues = result.Read<IssueDetail>().ToList();

                    // Build viewmodel
                    viewModel.VendorName = names?.VendorName ?? "Unknown Vendor";
                    viewModel.MaterialName = names?.MaterialName ?? "Unknown Material";
                    viewModel.VendorDbkey = vendorDbkey;
                    viewModel.RawMaterialDbkey = rawMaterialDbkey;
                    viewModel.Demands = demands ?? new List<DemandDetail>();
                    viewModel.Issues = issues ?? new List<IssueDetail>();

                    // Use summary from SP
                    viewModel.Summary = summary ?? new VendorMaterialSummary
                    {
                        TotalDemands = 0,
                        TotalOrdered = 0,
                        TotalReceived = 0,
                        TotalBalance = 0,
                        TotalIssues = 0,
                        TotalIssued = 0,
                        CurrentStock = 0
                    };
                }

                return PartialView(viewModel);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Content($"<div class='alert alert-danger'>Error loading vendor details: {ex.Message}</div>");
            }
        }


        [HttpGet]
        public IActionResult JobCardDetails(string jcNo)
        {
            var model = jcNo; 

            return PartialView("_JobcardDetails", model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

		[ClaimRequirement(UserPermissions.Exception_Logs_Read)]
		public IActionResult ExceptionLogs()
        {
            using(_dbContext)
            {
                List<ExceptionLog> exceptionLog = _dbContext.ExceptionLogs.OrderByDescending(x=>x.id).Take(20).ToList();
                return View(exceptionLog);
            }
        }
    }
}