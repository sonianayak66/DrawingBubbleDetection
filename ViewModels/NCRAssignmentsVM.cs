using MPCRS.Models;
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
	public class NCRAssignmentsVM
	{
		public int NCRWorkflowID { get; set; }
		public int NcrID { get; set; }
		public string? NCRGUID { get; set; }
		public string? NCRWorkflowGUID { get; set; }
		public int? ModuleID { get; set; }
		public string? ModuleName { get; set; }
		public string? AssigneeUserGUIDs { get; set; }
        public string? Status { get; set; }
		public string? Remarks { get; set; }
		public DateTime? WorkUpdatedOn { get; set; }
		public int? AssignedBy { get; set; }
		public DateTime? AssignedOn { get; set; }
		public string? Draw_part_no { get; set; }
		public string? UserName { get; set; }
		public DateTime? ReceivedDate { get; set; }
		public string? ComitteeReferred { get; set; }
		public string? ReceivedFrom { get; set; }
		public string? NCRRemarks { get; set; }
		public string? Revision { get; set; }
		public string? ReceivedFromString { get; set; }
		public string? WorkFlowAssigneeUserGuid { get; set; }
		public string? PartDescription { get; set; }
		public string? ReferenceNumber { get; set; }

		public int? CloseNCR { get; set; }
		public string? SerialNumber { get; set; }
		public int? Qty { get; set; }
		public string? DARno { get; set; }

		public string? Stage_Final { get; set; }
		public string? Inspection_Report_No { get; set; }
		public string? Raw_material_Name { get; set; }
		public string?  JobCard { get; set; }
		public string? Module_Responsibilty_String { get; set; }

		 
        // NEW PROPERTIES - Add these
        public int UseNewWorkflow { get; set; }
        public int TotalDeviations { get; set; }
        public int CompletedDeviations { get; set; }
        public int ForwardedToModule { get; set; }
        public int ForwardedToTAS { get; set; }
        public int ForwardedToSTRESS { get; set; }
        public int ForwardedToCHAIR { get; set; }
        public int ForwardedToModule_Rework { get; set; }
        public int ForwardedToTAS_Rework { get; set; }
        public int ForwardedToSTRESS_Rework { get; set; }
        public int ForwardedToCHAIR_Rework { get; set; }

        // Computed by dbo.GetNcrAssignments_Data_V2 — replaces the
        // 15-branch if/else-if ladder that used to live in
        // Views/NCR/AssignedNCRList.cshtml. Keep string values in
        // sync with the SP CASE expression.
        public string? CurrentStatusText { get; set; }

        // Master_Dbkey (Master_General) of the committee that
        // currently owns the NCR. NULL = nobody needs to act
        // (closed, or a MarkedAsComplete waiting state).
        public int? CurrentStageModuleID { get; set; }

        // 1 = the logged-in user is mapped (via NCR_ModuleToUserMapping)
        // to the committee that currently owns this NCR, so the
        // Open tab should show it. 0 = it belongs on the Completed tab.
        public int IsMyActionRequired { get; set; }
    }

    // Second result set from GetNcrAssignments_Data_V2 — one row per
    // NCR item, used to render the "Status of Deviations" column
    // inline (kills the per-row NCRStatusTable_SSP call that the
    // NCRItemstatus partial used to make).
    public class NCRItemStatusRow
    {
        public string? NCRGuid { get; set; }
        public int NCRItemKey { get; set; }
        public string? Serial_No_in_Inspection_Rep { get; set; }
        public string? SerialNumber { get; set; }
        public string? Module_Responsibilty_String { get; set; }
        public int Rework_Status { get; set; }
    }

    // Wrapper VM passed to AssignedNCRList.cshtml. Carries the
    // tab-filtered assignment list, the per-NCR item map, and
    // some page-level flags the view needs.
    public class AssignedNCRListPageVM
    {
        public List<NCRAssignmentsVM> Assignments { get; set; } = new();
        public Dictionary<string, List<NCRItemStatusRow>> ItemMap { get; set; } = new();
        public string ActiveTab { get; set; } = "PendingWithMe";
        public bool IsAdmin { get; set; }
    }

    public class NcrAssignmentLogVM
	{
		public int NCRWorkflowLogsID { get; set; }

		public string? NCRWorkflowGUID { get; set; }
		
		[Required]
		public string? Status { get; set; }

		public string? Remarks { get; set; }

		public bool? IsTransfered { get; set; }

		public string? UpdatedBy { get; set; }

		public DateTime? UpdatedOn { get; set; }
	}

	public class AssignedNCRvm
	{	
		public List<NCR_Workflow_Assignments_Log> workFlowLogs { get; set; }
		public NCRAssignmentsVM AssignmentData { get; set; }
		public List<NonConformanceReport_ItemVM> NcrItems { get; set; }
		public List<AssigneeUserName> ncrAssigneeName { get; set; }
	}

	public class AssigneeUserName
	{
		public string ModuleID { get; set; }

		public string AssigneeUserGUID { get; set; }
	}

}
