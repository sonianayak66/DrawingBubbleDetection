namespace MPCRS.ViewModels
{
    // NCRWorkflowAssignmentDetailVM.cs

    public class NCRWorkflowAssignmentDetailVM
    {
        public NCRWorkflowAssignmentVM AssignmentData { get; set; }
        public List<NCRWorkflowItemVM> NcrItems { get; set; }
        public List<NCRItemReworkVM> ReworkMarkings { get; set; }
    }

    public class NCRWorkflowAssignmentVM
    {
        public string NCRWorkflowGUID { get; set; }
        public string NCRGUID { get; set; }
        public int CurrentStageID { get; set; }
        public string CurrentStageName { get; set; }
        public int CurrentStageOrder { get; set; }
        public string Status { get; set; }
        public int? AssignedBy { get; set; }
        public DateTime? AssignedOn { get; set; }
        public DateTime? WorkUpdatedOn { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public int? ReceivedFrom { get; set; }
        public string ReceivedFromString { get; set; }
        public string ComitteeReferred { get; set; }
        public string NCRRemarks { get; set; }
        public string Revision { get; set; }
        public int? Qty { get; set; }
        public string DARno { get; set; }
        public string Stage_Final { get; set; }
        public string Inspection_Report_No { get; set; }
        public string Raw_material_Name { get; set; }
        public string JobCard { get; set; }
        public int? CloseNCR { get; set; }
        public string Draw_part_no { get; set; }
        public string PartDescription { get; set; }
        public string AssignedModules { get; set; }
        public string AssigneeUserGUIDs { get; set; }
    }

    public class NCRWorkflowItemVM
    {
        public int NCRItemKey { get; set; }
        public string NCRGuid { get; set; }
        public string SerialNumber { get; set; }
        public string EngineName { get; set; }
        public string DeviationDescription { get; set; }
        public string Disposition { get; set; }

        // Tracking details
        public int TrackingID { get; set; }
        public string NCRWorkFlowGuid { get; set; }
        public int StageID { get; set; }
        public string StageName { get; set; }
        public string StageDescription { get; set; }
        public int StageOrder { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string UpdatedByUserName { get; set; }
        public int CycleNumber { get; set; }
        public int IsCurrentStage { get; set; }
        public int CanEdit { get; set; }
    }

    public class NCRItemReworkVM
    {
        public int ReworkID { get; set; }
        public int NCRItemKey { get; set; }
        public int StageID { get; set; }
        public string StageName { get; set; }
        public string NCRWorkFlowGuid { get; set; }
        public bool IsRework { get; set; }
        public bool IsTrialAssembly { get; set; }
        public string ReworkDimension { get; set; }
        public int? MarkedBy { get; set; }
        public string MarkedByUserName { get; set; }
        public DateTime? MarkedOn { get; set; }
        public bool IsUnmarked { get; set; }
        public int? UnmarkedBy { get; set; }
        public string UnmarkedByUserName { get; set; }
        public DateTime? UnmarkedOn { get; set; }
        public bool IsCleared { get; set; }
        public int? ClearedBy { get; set; }
        public string ClearedByUserName { get; set; }
        public DateTime? ClearedOn { get; set; }
        public bool IsActive { get; set; }
    }
}
