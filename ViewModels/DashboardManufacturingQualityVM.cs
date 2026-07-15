using System;
using System.Collections.Generic;

namespace MPCRS.ViewModels
{
    // =============================================
    // Main Container ViewModel
    // =============================================
    public class DashboardManufacturingQualityVM
    {
        // Result Set 1: Summary Counts
        public ManufacturingQualitySummary Summary { get; set; }

        // Result Set 2: Engines by Status
        public List<EngineStatusDistribution> EnginesByStatus { get; set; }

        // Result Set 3: Active Engine Builds
        public List<ActiveEngineBuild> ActiveEngineBuilds { get; set; }

        // Result Set 4: Recently Completed Engines
        public List<CompletedEngine> CompletedEngines { get; set; }

        // Result Set 5: Parts by Type (FCBP)
        public List<PartsByType> PartsByType { get; set; }

        // Result Set 6: Module-wise Parts Distribution
        public List<ModulePartsDistribution> ModulePartsDistribution { get; set; }

        // Result Set 7: Drawing Status Summary
        public DrawingStatusSummary DrawingStatusSummary { get; set; }

        // Result Set 8: Recent Parts Revisions
        public List<RecentPartRevision> RecentPartRevisions { get; set; }

        // Result Set 9: Build Components Status
        public BuildComponentsStatusSummary BuildComponentsStatus { get; set; }

        // Result Set 10: Build Components by Build
        public List<BuildComponentsByBuild> BuildComponentsByBuild { get; set; }

        // Result Set 11: Active NCRs Count (detailed)
        public NCRAgeSummary NCRAgeSummary { get; set; }

        // Result Set 12: NCRs by Workflow Stage
        public List<NCRsByStage> NCRsByStage { get; set; }

        // Result Set 13: NCRs Older than 30 Days
        public List<OldNCR> OldNCRs { get; set; }

        // Result Set 14: NCRs by Module Assignment
        public List<NCRsByModule> NCRsByModule { get; set; }

        // Result Set 15: NCRs by Received From
        public List<NCRsByReceivedFrom> NCRsByReceivedFrom { get; set; }

        // Result Set 16: NCR Rework Items
        public List<NCRReworkItem> NCRReworkItems { get; set; }

        // Result Set 17: Top Parts with NCRs
        public List<TopPartsWithNCRs> TopPartsWithNCRs { get; set; }

        // Result Set 18: ACSN Records by Status
        public List<ACSNByStatus> ACSNByStatus { get; set; }

        // Result Set 19: Pending ACSN Approvals
        public List<PendingACSN> PendingACSNs { get; set; }

        // Result Set 20: ACSN Items by Step
        public List<ACSNItemsByStep> ACSNItemsByStep { get; set; }

        // Result Set 21: NCR Full Distribution
        public NCRFullDistribution NCRDistribution { get; set; }

        // Result Set 22: ACSN Series-wise Open/Closed
        public List<ACSNSeriesDistribution> ACSNBySeriesDistribution { get; set; }

        // Result Set 23: ACSN Open Items by Age
        public List<ACSNOpenByAge> ACSNOpenByAge { get; set; }
    }

    // =============================================
    // Individual ViewModels for each result set
    // =============================================

    // Result Set 1
    public class ManufacturingQualitySummary
    {
        public int ActiveEngineBuildsCount { get; set; }
        public int CompletedEngineBuildsCount { get; set; }
        public int TotalEngineBuildsCount { get; set; }
        public int ActiveNCRsCount { get; set; }
        public int PendingACSNCount { get; set; }
        public int ClosedACSNCount { get; set; }
        public int TotalACSNCount { get; set; }
    }

    // Result Set 2
    public class EngineStatusDistribution
    {
        public string Status { get; set; }
        public int EngineCount { get; set; }
    }

    // Result Set 3
    public class ActiveEngineBuild
    {
        public int Id { get; set; }
        public string BuildName { get; set; }
        public string BuildGuid { get; set; }
        public DateTime? BuildDate { get; set; }
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Status { get; set; }
    }

    // Result Set 4
    public class CompletedEngine
    {
        public int Id { get; set; }
        public string BuildName { get; set; }
        public string BuildGuid { get; set; }
        public DateTime? BuildDate { get; set; }
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; }
    }

    // Result Set 5
    public class PartsByType
    {
        public string PartType { get; set; }
        public int PartsCount { get; set; }
    }

    // Result Set 6
    public class ModulePartsDistribution
    {
        public string ModuleName { get; set; }
        public int PartsCount { get; set; }
    }

    // Result Set 7
    public class DrawingStatusSummary
    {
        public int Parts_With_Drawing { get; set; }
        public int Parts_With_2D_Model { get; set; }
        public int Parts_With_3D_Model { get; set; }
        public int Parts_With_ACSN { get; set; }
        public int Total_Active_Parts { get; set; }
    }

    // Result Set 8
    public class RecentPartRevision
    {
        public int Rev_History_Dbkey { get; set; }
        public string Draw_part_no { get; set; }
        public string Description { get; set; }
        public string Revision { get; set; }
        public DateTime? Revision_Date { get; set; }
        public string Revision_Notes { get; set; }
        public DateTime Updated_on { get; set; }
    }

    // Result Set 9
    public class BuildComponentsStatusSummary
    {
        public int Active_Components { get; set; }
        public int Newly_Added { get; set; }
        public int Updated_Components { get; set; }
        public int Replaced_Components { get; set; }
        public int Removed_Components { get; set; }
        public int Total_Components { get; set; }
    }

    // Result Set 10
    public class BuildComponentsByBuild
    {
        public string BuildName { get; set; }
        public int ComponentCount { get; set; }
        public int NewlyAdded { get; set; }
        public int Updated { get; set; }
    }

    // Result Set 11
    public class NCRAgeSummary
    {
        public int TotalActiveNCRs { get; set; }
        public int NCRs_0_30_Days { get; set; }
        public int NCRs_31_60_Days { get; set; }
        public int NCRs_Over_60_Days { get; set; }
    }

    // Result Set 12
    public class NCRsByStage
    {
        public string Stage { get; set; }
        public int NCRCount { get; set; }
    }

    // Result Set 13
    public class OldNCR
    {
        public string ReferenceNumber { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public int DaysPending { get; set; }
        public string ReportStatus { get; set; }
        public string Remarks { get; set; }
    }

    // Result Set 14
    public class NCRsByModule
    {
        public string ModuleName { get; set; }
        public int NCRCount { get; set; }
    }

    // Result Set 15
    public class NCRsByReceivedFrom
    {
        public string ReceivedFromName { get; set; }
        public int NCRCount { get; set; }
    }

    // Result Set 16
    public class NCRReworkItem
    {
        public string ReferenceNumber { get; set; }
        public string SerialNumber { get; set; }
        public string Engine { get; set; }
        public string Draw_part_no { get; set; }
        public string PartDescription { get; set; }
        public int? Rework_Status { get; set; }
        public DateTime? ReceivedDate { get; set; }
    }

    // Result Set 17
    public class TopPartsWithNCRs
    {
        public string Draw_part_no { get; set; }
        public string Description { get; set; }
        public int NCRCount { get; set; }
        public string NCR_References { get; set; }
    }

    // Result Set 18
    public class ACSNByStatus
    {
        public string ACSN_Status { get; set; }
        public int RecordCount { get; set; }
    }

    // Result Set 19
    public class PendingACSN
    {
        public string ACSNnum { get; set; }
        public string DrawingNumber { get; set; }
        public string description { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public int DaysPending { get; set; }
        public string existingRevision { get; set; }
        public string NewRevision { get; set; }
        public string ModuleName { get; set; }
    }

    // Result Set 20
    public class ACSNItemsByStep
    {
        public string StepName { get; set; }
        public int ItemCount { get; set; }
        public int ActiveItems { get; set; }
    }

    // Result Set 21
    public class NCRFullDistribution
    {
        public int TotalNCRs { get; set; }
        public int InProgress_L1 { get; set; }
        public int InProgress_L2 { get; set; }
        public int PartiallyCleared { get; set; }
        public int Cleared { get; set; }
        public int Rejected { get; set; }
    }

    // Result Set 22
    public class ACSNSeriesDistribution
    {
        public string Series { get; set; }
        public int TotalCount { get; set; }
        public int OpenCount { get; set; }
        public int ClosedCount { get; set; }
    }

    // Result Set 23
    public class ACSNOpenByAge
    {
        public string AgeBucket { get; set; }
        public int ACSNCount { get; set; }
    }
}