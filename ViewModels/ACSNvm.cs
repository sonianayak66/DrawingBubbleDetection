using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
    public class ACSNvm
    {
        public int rn { get; set; }
        public int acsnKey { get; set; }

        [DisplayName("Serial Number")]
        public int? SerialNumber { get; set; }
        [Required]
        public string Series { get; set; }
        [Required]
        [DisplayName("Received Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ReceivedDate { get; set; } = DateTime.Now;

        [DisplayName("ACSN Number")]
        public string ACSNnum { get; set; } = "Auto Generated";

        [Required]
        [DisplayName("Module Ref Number")]
        public string ModuleRefNum { get; set; }

        [DisplayName("Part Number")]
        public int? PartDbKey { get; set; }
        [Required]
        [DisplayName("Drawing Number")]
        public string DrawingNumber { get; set; }
        [DisplayName("Description")]
        public string description { get; set; }
        [Required]
        public int Module { get; set; }
        [DisplayName("Module Name")]
        public string ModuleName { get; set; }
        [Required]
        [DisplayName("Existing Revision")]
        public string existingRevision { get; set; }
        [Required]
        [DisplayName("New Revision")]
        public string NewRevision { get; set; }

        public string? Remarks { get; set; }
        [DisplayName("Status")]
        public string ACSN_Status { get; set; }

        [DisplayName("Current Step")]
        public string StepStatus { get; set; }
        public int? updatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class ACSNItemVm
    {
        public int ACSNStatusKey { get; set; }
        public Nullable<int> acsnKey { get; set; }
        public string acsnStatus { get; set; }
        public Nullable<int> acsnStepId { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }
        public string Documents { get; set; }
        public string Remarks { get; set; }
        public string StepName { get; set; }
        public string StepStatus { get; set; }
        public decimal DisplayOrder { get; set; }
        public bool isActiveStatus { get; set; } = false;
        public Nullable<int> updatedBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
    }

    public class ACSNStepSummary
    {
        public int Master_Dbkey { get; set; }
        public string Master_Name { get; set; }
        public string DisplayOrder { get; set; }
        public string? Series { get; set; }
        public int? ItemsCount { get; set; }
        public int? acsnStepId { get; set; }
    }
}
