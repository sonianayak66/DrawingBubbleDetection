using System.ComponentModel.DataAnnotations;
using MPCRS.Models;

namespace MPCRS.ViewModels
{
	public class NCR_WorkflowVM
	{
		public NCR_Workflow_Assignment assignmentData { get; set; }

		public NonConformanceReport nonConformanceReport { get; set; }
        
		public List<NonConformanceReport_Item> nonConformanceReport_Items { get; set; }

	}

    public partial class NCR_Workflow_Assignment_ViewModel
    {
        public int NCRWorkflowID { get; set; }

        public string? NCRWorkflowGUID { get; set; }

        public string? NCRGUID { get; set; }
        [Required(ErrorMessage ="Required")]
        public int? ModuleID { get; set; }
        [Required(ErrorMessage = "Required")]
        public string? AssigneeUserGUIDs { get; set; }

        public string? Status { get; set; }

        public string? Remarks { get; set; }

        public DateTime? WorkUpdatedOn { get; set; }

        public int? AssignedBy { get; set; }

        public DateTime? AssignedOn { get; set; }
        public string? NCRItemKeys { get; set; }
    }

    public class NCRSerialNoDataVM
    {
        public string? NCRWorkflowGUID { get; set; }
        public string? NCRGUID { get; set; }
    }
}
