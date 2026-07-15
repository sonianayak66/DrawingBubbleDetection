using System;
using System.Collections.Generic;

namespace MPCRS.ViewModels
{
    // Summary per module — from Result Set 1 of the SP
    public class DataEntryTrackingSummaryVM
    {
        public int ConfigId { get; set; }
        public string ModuleName { get; set; }
        public int TotalRecords { get; set; }
        public int GreenCount { get; set; }
        public int AmberCount { get; set; }
        public int RedCount { get; set; }
        public int BlockedCount { get; set; }
    }

    // Record-level detail — from Result Set 2 of the SP
    public class DataEntryTrackingDetailVM
    {
        public int ConfigId { get; set; }
        public string ModuleName { get; set; }
        public string SourceTable { get; set; }
        public int SourceRecordKey { get; set; }
        public string DisplayValuesJson { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public int UpdateFrequencyDays { get; set; }
        public int DaysSinceUpdate { get; set; }
        public string TrackingStatus { get; set; }  // "Green", "Amber", "Red", "NoUpdate"
        // Remark fields
        public int? RemarkId { get; set; }
        public string RemarkText { get; set; }
        public bool IsBlocked { get; set; }
        public int? RemarkBy { get; set; }
        public string RemarkByName { get; set; }
        public DateTime? RemarkOn { get; set; }
    }

    // Combined dashboard model — passed to the view
    public class DataEntryTrackingDashboardVM
    {
        public List<DataEntryTrackingSummaryVM> Summary { get; set; } = new List<DataEntryTrackingSummaryVM>();
        public List<DataEntryTrackingDetailVM> Details { get; set; } = new List<DataEntryTrackingDetailVM>();
        public string ActiveFilter { get; set; }  // Current status filter if any
        public int? ActiveConfigId { get; set; }   // Current module filter if any
    }

    // For saving/updating remarks via AJAX
    public class DataEntryRemarkPostVM
    {
        public int ConfigId { get; set; }
        public int SourceRecordKey { get; set; }
        public string RemarkText { get; set; }
        public bool NoUpdateNeeded { get; set; }
        public int? RemarkId { get; set; }  // NULL for new, has value for update
    }

    // Config page — for admin to manage tracked modules
    public class DataEntryTrackingConfigVM
    {
        public int ConfigId { get; set; }
        public string ModuleName { get; set; }
        public string SourceTable { get; set; }
        public string PrimaryKeyColumn { get; set; }
        public string DisplayColumns { get; set; }  // JSON string
        public string UpdatedOnColumn { get; set; }
        public int UpdateFrequencyDays { get; set; }
        public int? AmberThresholdDays { get; set; }
        public string ExclusionCondition { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}