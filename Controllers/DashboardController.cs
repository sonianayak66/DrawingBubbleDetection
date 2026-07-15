using Microsoft.AspNetCore.Mvc;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using System.Data;
using Dapper;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;

        public DashboardController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

        // ============================================
        // Main Dashboard Page
        // ============================================
        [HttpGet]
        [ClaimRequirement(UserPermissions.Configuration_Read)]
        public IActionResult Index()
        {
            var viewModel = new DashboardViewModel();

            // Check user permissions for each module
            // Check user permissions for each module
            viewModel.UserPermissions.HasProcurement = UserData.IsAuthorized(User, UserPermissions.Demand_Read);
            viewModel.UserPermissions.HasManufacturing = UserData.IsAuthorized(User, UserPermissions.MPL_Read);
            viewModel.UserPermissions.HasQuality = UserData.IsAuthorized(User, UserPermissions.NCR_Read);
            viewModel.UserPermissions.HasSOP = UserData.IsAuthorized(User, UserPermissions.SOP_Read);
            viewModel.UserPermissions.HasConfiguration = UserData.IsAuthorized(User, UserPermissions.ACSN_Read);
            viewModel.UserPermissions.HasSystem = UserData.IsAuthorized(User, UserPermissions.Configuration_Read);

            return View(viewModel);
        }

        // ============================================
        // Load Procurement Section - Returns Partial View with Data
        // ============================================
        [HttpGet]
        [ClaimRequirement(UserPermissions.Demand_Read)]
        public async Task<IActionResult> LoadProcurementSection()
        {
            try
            {
                var vm = new ProcurementDashboardVM();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    // Call the stored procedure and read ALL 16 result sets
                    var multi = await connection.QueryMultipleAsync(
                        "Dashboard_GetProcurementMetrics",
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 120);

                    // SECTION 1: KPI Cards (Result Sets 1-4)
                    vm.UrgentActions = await multi.ReadFirstOrDefaultAsync<UrgentActionsSummary>();
                    vm.VendorAccountability = await multi.ReadFirstOrDefaultAsync<VendorAccountabilitySummary>();
                    vm.MaterialFlow = await multi.ReadFirstOrDefaultAsync<MaterialFlowSummary>();
                    vm.UntrackedDemands = await multi.ReadFirstOrDefaultAsync<UntrackedDemandsSummary>();

                    // SECTION 2: Charts (Result Sets 5-7)
                    vm.VendorPerformanceData = (await multi.ReadAsync<VendorPerformanceChart>()).ToList();
                    vm.MaterialHealthData = (await multi.ReadAsync<MaterialHealthChart>()).ToList();
                    vm.AtRiskDemands = (await multi.ReadAsync<AtRiskDemand>()).ToList();

                    // SECTION 3: Tables (Result Sets 8-12)
                    vm.ImmediateAttention = (await multi.ReadAsync<ImmediateAttentionItem>()).ToList();
                    vm.MaterialAvailability = (await multi.ReadAsync<MaterialAvailabilityItem>()).ToList();
                    vm.VendorPerformanceScorecard = (await multi.ReadAsync<VendorPerformanceScorecard>()).ToList();
                    vm.UpcomingMilestones = (await multi.ReadAsync<UpcomingMilestone>()).ToList();
                    vm.MaterialIssuedToVendors = (await multi.ReadAsync<MaterialIssuedToVendor>()).ToList();

                    // SECTION 4: Legacy (Result Sets 13-16)
                    vm.CostVarianceAnalysis = (await multi.ReadAsync<CostVarianceItem>()).ToList();
                    vm.ProcurementByStatus = (await multi.ReadAsync<ProcurementStatusDistribution>()).ToList();
                    vm.TopVendors = (await multi.ReadAsync<TopVendor>()).ToList();
                    vm.OverdueProcurements = (await multi.ReadAsync<OverdueProcurement>()).ToList();
                    vm.UntrackedDemandsList = (await multi.ReadAsync<UntrackedDemandItem>()).ToList();

                    // SECTION 5: Order Type Metrics (Result Sets 18-23)
                    vm.CastingMetrics = await multi.ReadFirstOrDefaultAsync<OrderTypeMetricsSummary>();
                    vm.ForgingMetrics = await multi.ReadFirstOrDefaultAsync<OrderTypeMetricsSummary>();
                    vm.PyroMetrics = await multi.ReadFirstOrDefaultAsync<OrderTypeMetricsSummary>();
                    vm.CastingDetails = (await multi.ReadAsync<OrderTypePartDetail>()).ToList();
                  
                    vm.ForgingDetails = (await multi.ReadAsync<OrderTypePartDetail>()).ToList();
                   
                    vm.PyroDetails = (await multi.ReadAsync<OrderTypePartDetail>()).ToList();
                }

                return PartialView("_Procurement", vm);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return StatusCode(500, "Error loading Procurement section");
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetManufacturingQualitySection()
        {
            try
            {
                var viewModel = new DashboardManufacturingQualityVM();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    using (var multi = await connection.QueryMultipleAsync("Dashboard_GetManufacturingQualityMetrics",
                        commandType: CommandType.StoredProcedure))
                    {
                        // Result Set 1: Summary Counts
                        viewModel.Summary = await multi.ReadFirstOrDefaultAsync<ManufacturingQualitySummary>();

                        // Result Set 2: Engines by Status
                        viewModel.EnginesByStatus = (await multi.ReadAsync<EngineStatusDistribution>()).ToList();

                        // Result Set 3: Active Engine Builds
                        viewModel.ActiveEngineBuilds = (await multi.ReadAsync<ActiveEngineBuild>()).ToList();

                        // Result Set 4: Recently Completed Engines
                        viewModel.CompletedEngines = (await multi.ReadAsync<CompletedEngine>()).ToList();

                        // Result Set 5: Parts by Type
                        viewModel.PartsByType = (await multi.ReadAsync<PartsByType>()).ToList();

                        // Result Set 6: Module-wise Parts Distribution
                        viewModel.ModulePartsDistribution = (await multi.ReadAsync<ModulePartsDistribution>()).ToList();

                        // Result Set 7: Drawing Status Summary
                        viewModel.DrawingStatusSummary = await multi.ReadFirstOrDefaultAsync<DrawingStatusSummary>();

                        // Result Set 8: Recent Parts Revisions
                        viewModel.RecentPartRevisions = (await multi.ReadAsync<RecentPartRevision>()).ToList();

                        // Result Set 9: Build Components Status
                        viewModel.BuildComponentsStatus = await multi.ReadFirstOrDefaultAsync<BuildComponentsStatusSummary>();

                        // Result Set 10: Build Components by Build
                        viewModel.BuildComponentsByBuild = (await multi.ReadAsync<BuildComponentsByBuild>()).ToList();

                        // Result Set 11: NCR Age Summary
                        viewModel.NCRAgeSummary = await multi.ReadFirstOrDefaultAsync<NCRAgeSummary>();

                        // Result Set 12: NCRs by Workflow Stage
                        viewModel.NCRsByStage = (await multi.ReadAsync<NCRsByStage>()).ToList();

                        // Result Set 13: NCRs Older than 30 Days
                        viewModel.OldNCRs = (await multi.ReadAsync<OldNCR>()).ToList();

                        // Result Set 14: NCRs by Module Assignment
                        viewModel.NCRsByModule = (await multi.ReadAsync<NCRsByModule>()).ToList();

                        // Result Set 15: NCRs by Received From
                        viewModel.NCRsByReceivedFrom = (await multi.ReadAsync<NCRsByReceivedFrom>()).ToList();

                        // Result Set 16: NCR Rework Items
                        viewModel.NCRReworkItems = (await multi.ReadAsync<NCRReworkItem>()).ToList();

                        // Result Set 17: Top Parts with NCRs
                        viewModel.TopPartsWithNCRs = (await multi.ReadAsync<TopPartsWithNCRs>()).ToList();

                        // Result Set 18: ACSN Records by Status
                        viewModel.ACSNByStatus = (await multi.ReadAsync<ACSNByStatus>()).ToList();

                        // Result Set 19: Pending ACSN Approvals
                        viewModel.PendingACSNs = (await multi.ReadAsync<PendingACSN>()).ToList();

                        // Result Set 20: ACSN Items by Step
                        viewModel.ACSNItemsByStep = (await multi.ReadAsync<ACSNItemsByStep>()).ToList();

                        // Result Set 21: NCR Full Distribution
                        viewModel.NCRDistribution = await multi.ReadFirstOrDefaultAsync<NCRFullDistribution>();

                        // Result Set 22: ACSN Series-wise Open/Closed
                        viewModel.ACSNBySeriesDistribution = (await multi.ReadAsync<ACSNSeriesDistribution>()).ToList();

                        // Result Set 23: ACSN Open Items by Age
                        viewModel.ACSNOpenByAge = (await multi.ReadAsync<ACSNOpenByAge>()).ToList();

                    }
                }

                return PartialView("_ManufacturingQuality", viewModel);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return StatusCode(500, "Error loading Manufacturing & Quality section");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSOPSection()
        {
            try
            {
                var viewModel = new DashboardSOPVM();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    using (var multi = await connection.QueryMultipleAsync("Dashboard_GetSOPMetrics",
                        commandType: CommandType.StoredProcedure))
                    {
                        viewModel.Summary = await multi.ReadFirstOrDefaultAsync<SOPSummary>();
                        viewModel.BuildsByStatus = (await multi.ReadAsync<SOPBuildStatusDistribution>()).ToList();
                        viewModel.EngineBuilds = (await multi.ReadAsync<SOPEngineBuildDetail>()).ToList();
                        viewModel.BuildComponentSummary = (await multi.ReadAsync<SOPBuildComponentSummary>()).ToList();
                        viewModel.SectionCompletionByBuild = (await multi.ReadAsync<SOPSectionCompletionByBuild>()).ToList();
                        viewModel.DocumentSummary = (await multi.ReadAsync<SOPDocumentSummary>()).ToList();
                        viewModel.RecentActivity = (await multi.ReadAsync<SOPRecentActivity>()).ToList();
                        viewModel.BuildsByEngine = (await multi.ReadAsync<SOPBuildsByEngine>()).ToList();
                    }
                }

                return PartialView("_SOPDashboard", viewModel);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return StatusCode(500, "Error loading SOP section");
            }
        }



    }
}
