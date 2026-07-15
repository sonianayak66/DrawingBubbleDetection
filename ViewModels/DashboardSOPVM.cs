using System;
using System.Collections.Generic;

namespace MPCRS.ViewModels
{
    // =============================================
    // SOP Dashboard Main Container
    // =============================================
    public class DashboardSOPVM
    {
        // Result Set 1: Summary KPIs
        public SOPSummary Summary { get; set; }

        // Result Set 2: Build Status Distribution
        public List<SOPBuildStatusDistribution> BuildsByStatus { get; set; }

        // Result Set 3: All Engine Builds
        public List<SOPEngineBuildDetail> EngineBuilds { get; set; }

        // Result Set 4: Build Component Summary
        public List<SOPBuildComponentSummary> BuildComponentSummary { get; set; }

        // Result Set 5: SOP Section Completion by Build
        public List<SOPSectionCompletionByBuild> SectionCompletionByBuild { get; set; }

        // Result Set 6: Document Upload Summary
        public List<SOPDocumentSummary> DocumentSummary { get; set; }

        // Result Set 7: Recent Activity
        public List<SOPRecentActivity> RecentActivity { get; set; }

        // Result Set 8: Builds by Baseline Engine
        public List<SOPBuildsByEngine> BuildsByEngine { get; set; }
    }

    // Result Set 1
    public class SOPSummary
    {
        public int TotalBuilds { get; set; }
        public int InProgressBuilds { get; set; }
        public int CompletedBuilds { get; set; }
        public int TotalTemplateSections { get; set; }
        public int CompletedSections { get; set; }
        public int ReviewedSections { get; set; }
        public int TotalBuildSections { get; set; }
        public int TotalDocuments { get; set; }
        public int RecentActivityCount { get; set; }
    }

    // Result Set 2
    public class SOPBuildStatusDistribution
    {
        public string Status { get; set; }
        public int BuildCount { get; set; }
    }

    // Result Set 3
    public class SOPEngineBuildDetail
    {
        public int Id { get; set; }
        public string BuildName { get; set; }
        public string BuildGuid { get; set; }
        public DateTime? BuildDate { get; set; }
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string Status { get; set; }
        public string BaselineEngine { get; set; }
        public int ComponentCount { get; set; }
    }

    // Result Set 4
    public class SOPBuildComponentSummary
    {
        public int BuildId { get; set; }
        public string BuildName { get; set; }
        public string Status { get; set; }
        public int TotalComponents { get; set; }
        public int ActiveComponents { get; set; }
        public int NewlyAdded { get; set; }
        public int Updated { get; set; }
        public int Replaced { get; set; }
        public int Removed { get; set; }
    }

    // Result Set 5
    public class SOPSectionCompletionByBuild
    {
        public string BuildName { get; set; }
        public string BuildGuid { get; set; }
        public string BuildStatus { get; set; }
        public int TotalSections { get; set; }
        public int CompletedSections { get; set; }
        public int ReviewedSections { get; set; }
        public int PendingSections { get; set; }
    }

    // Result Set 6
    public class SOPDocumentSummary
    {
        public string BuildName { get; set; }
        public string BuildGuid { get; set; }
        public int DocumentCount { get; set; }
        public DateTime? LastUploadDate { get; set; }
    }

    // Result Set 7
    public class SOPRecentActivity
    {
        public int Log_Db_Key { get; set; }
        public string TableName { get; set; }
        public string Event_Description { get; set; }
        public string Remarks { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime? Updated_On { get; set; }
        public string Activity_Name { get; set; }
    }

    // Result Set 8
    public class SOPBuildsByEngine
    {
        public string EngineName { get; set; }
        public int BuildCount { get; set; }
    }
}