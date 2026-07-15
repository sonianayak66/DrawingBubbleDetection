// =============================================
// FILE: ViewModels/NCRReportOverviewVM.cs
// ACTION: CREATE new file
// =============================================

namespace MPCRS.ViewModels
{
    /// <summary>
    /// ViewModel for Tab 1: Status Overview
    /// </summary>
    public class NCRStatusOverviewVM
    {
        public List<ReportStatusCount> ReportStatusCounts { get; set; } = new();
        public List<WorkflowStageCount> WorkflowStageCounts { get; set; } = new();
    }

    public class ReportStatusCount
    {
        public string ReportStatus { get; set; }
        public int NCRCount { get; set; }
    }

    public class WorkflowStageCount
    {
        public string WorkflowStage { get; set; }
        public int NCRCount { get; set; }
        public int StageStep { get; set; }// Added
    }

    /// <summary>
    /// ViewModel for Tab 2: Engine Distribution
    /// </summary>
    public class NCREngineDistributionVM
    {
        public List<EngineReportStatusCount> EngineStatusCounts { get; set; } = new();
        public List<EngineWorkflowStageCount> EngineWorkflowCounts { get; set; } = new();
    }

    public class EngineReportStatusCount
    {
        public string Engine { get; set; }
        public string ReportStatus { get; set; }
        public int NCRCount { get; set; }
    }

    public class EngineWorkflowStageCount
    {
        public string Engine { get; set; }
        public string WorkflowStage { get; set; }
        public int NCRCount { get; set; }
        public int StageStep { get; set; }   //  ADD
    }
}