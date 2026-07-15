using Microsoft.AspNetCore.Mvc.Rendering;
using MPCRS.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
    public class NonConformanceReportVM
    {
        public int Id { get; set; }
        public string NCRGuid { get; set; }

        [Required]
        [DisplayName("Reference Number")]
        public string? ReferenceNumber { get; set; }
        [Required]
        [DisplayName("Reference Date")]
        public DateTime? ReceivedDate { get; set; }

        [Required(ErrorMessage = "Please select Received From")]
        [DisplayName("Received From")]
        [Range(1, int.MaxValue)]
        public int ReceivedFrom { get; set; }

        [Required]
        [DisplayName("Committee Referred")]
        public string? ComitteeReferred { get; set; }
        [Required]
        [DisplayName("Report Status")]
        public string? ReportStatus { get; set; }

        public string? Remarks { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public string? FileLocation { get; set; }

        public string? OrignalFileName { get; set; }

        public string? SystemFileName { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please Select Part No")]
        public int? Engine_Part_Dbkey { get; set; }

        public int? Part_relation_dbkey { get; set; }

        public int? Vendor { get; set; }

        public string? SerialNumber { get; set; }

        [Required]
        public string? Revision { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Qty")]
        public int? Qty { get; set; }

        public string? Module { get; set; }

        public string? Stress { get; set; }

        public string? Chair { get; set; }

        public string? Tas { get; set; }
        public SelectList Module_ResponsibilityList { get; set; }

        public int? Module_Responsibilty { get; set; }

        public string? JobCard { get; set; }

        public string? DARno { get; set; }
        public string? AssignedUserGuid { get; set; }
        public string? Module_Responsibilty_String { get; set; }

        //public int? ModuleAssignedBy { get; set; }
        //public DateTime? ModuleAssignedOn { get; set; }

        public int? RawMaterial { get; set; }
        [DisplayName("Stage/Final")]
        public string? Stage_Final { get; set; }
        [DisplayName("Inspection Report No")]
        public string? Inspection_Report_No { get; set; }

        public string? ECM_TR_NO { get; set; }

        public string? ECM_No { get; set; }
    }

    public class NonConformanceReport_ItemVM
    {
        public int NCRItemKey { get; set; }

        public string? NCRGuid { get; set; }

        public string? Engine { get; set; }

        public string? SerialNumber { get; set; }

        public string? Status { get; set; }

        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public string? DrawingDimension { get; set; }

        public string? ActualDimension { get; set; }

        public string? DrgZone { get; set; }

        public string? NCRWorkFlowGuid { get; set; }

        public string? Remarks { get; set; }
        public string? Module_Status { get; set; }

        public string? Module_Remarks { get; set; }

        public DateTime? Module_UpdatedOn { get; set; }

        public int? Module_UpdateBy { get; set; }

        public string? TAS_Status { get; set; }

        public string? TAS_Remarks { get; set; }

        public DateTime? TAS_UpdatedOn { get; set; }

        public int? TAS_UpdatedBy { get; set; }

        public string? Stress_Status { get; set; }

        public string? Stress_Remarks { get; set; }

        public DateTime? Stress_UpdatedOn { get; set; }

        public int? Stress_UpdatedBy { get; set; }

        public string? Chair_Status { get; set; }

        public string? Chair_Remarks { get; set; }

        public DateTime? Chair_UpdatedOn { get; set; }

        public int? Chair_UpdatedBy { get; set; }
        public string? AssignedUserGuid { get; set; }
        public string? AssigneeUserGUIDs { get; set; }
        public string? Module_Responsibilty_String { get; set; }
        public int? AssignedBy { get; set; }
        public string? remarksType { get; set; }

        public int? Rework_Status { get; set; }

		public string? Module_Rework_Status { get; set; }
		public string? Module_Rework_Remarks { get; set; }



		public string? TAS_Rework_Status { get; set; }
		public string? TAS_Rework_Remarks { get; set; }

		public string? STRESS_Rework_Status { get; set; }
	    public string? STRESS_Rework_Remarks { get; set; }

		public string? CHAIR_Rework_Status { get; set; }
		public string? CHAIR_Rework_Remarks { get; set; }

        public string? Rework_Dimension { get; set; }
        public string? STFE_PO_Remark { get; set; }

        public string? Workflow_Assignments_Status { get; set; }

        public string? Serial_No_in_Inspection_Rep { get; set; }
        public string? ReworkMarkedModuleName { get; set; }

        public string? Deviation_Reason_Analysis { get; set; }
        public string? SlNo { get; set; }

        public int? Module_ReworkType { get; set; } // new column to hold rework types for separate modules individually - coming from NonConformanceReport_Item_Rework table
        public int? TAS_ReworkType { get; set; }
        public int? STRESS_ReworkType { get; set; }
        public int? CHAIR_ReworkType { get; set; }
        public int? PO_ReworkType { get; set; }

        

    }
    public class NCRvm
    {
        public NonConformanceReportVM nonConformanceReportVM {  get; set; }
       // public List<NonConformanceReport_Item> nonConformanceReport_Items { get; set; }
        public List<NonConformanceReport_ItemVM> nonConformanceReport_Items { get; set; }
    }

}
