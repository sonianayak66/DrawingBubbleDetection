using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
    public class SOPCustomAccessLink
    {
        [DataType(DataType.DateTime)]
        public DateTime access_validity { get; set; }
        [Required]
        public string modules { get; set; }
        public string AccessGrantedBy { get; set; }
        [Required]
        public string LinkTitle { get; set; }
        public string AccessBuildGuid { get; set; }
        [Required]
        public string Remarks { get; set; }
        public string? LinkGuid { get; set; }
        public string? Link { get; set; }
    }

    public class BuildViewModel
    {
        public BaseLineEngineVM baseLineEngineVM { get; set; }
        public EngineBuildsVM engineBuildViewModel { get; set; }
        public List<EnginePartsViewModel> enginePartsViewModel { get; set; }

    }

    public class EnginePartsViewModel
    {
        public string? PartDisplayName { get; set; }
        public int? EnginePartDbkey { get; set; }
        public int? ParentId { get; set; }
        public int? PartBuildDbkey { get; set; }
        public int? IsActive { get; set; }
        public int? QtyPerEngine { get; set; }
        public string? ParentDrawingNumber { get; set; }
        public bool? Isupdated { get; set; }
        public bool? ForSopOnly { get; set; } = false;
        public bool? IsReplaced { get; set; } 
        public bool? IsNewlyAdded { get; set; } = false;
        public bool? IsRemoved { get; set; } = false;

    }


    public  class EngineBuildComponentViewModel
    {
        public int Id { get; set; }

        public int? BuildDbkey { get; set; }

        public int? BaseLineEngineDbkey { get; set; }

        public int? EnginePartDbkey { get; set; }

        public int? ParentId { get; set; }
        public string? Revision { get; set; }
        [Required(ErrorMessage ="Required")]
        public int? QtyPerEngine { get; set; }
        [Required(ErrorMessage = "Required")]
        public string? Description { get; set; }
       
        public string? JobCard { get; set; }
        public string? ContractNumber { get; set; }
        [Required(ErrorMessage = "Required")]
        public string? SerialNumber { get; set; }

        public string? Remarks { get; set; }

        public List<IFormFile> files { get; set; } 
        public string? PartDisplayName { get; set; }
        public string? Collaborators { get; set; }
        [Required(ErrorMessage = "Required")]
        public string? DrawingNumber { get; set; }
        public string? SchemeNumber { get; set; }
        public bool IsActive { get; set; }
        public bool IsReplaced { get; set; }
		public bool IsRemoved { get; set; }
		public bool IsNewlyAdded { get; set; }
		public int? ReportingParent { get; set; }

		public string? ReportingType { get; set; }

		public string? AssemblyReportingType { get; set; }

	}


    public class SOP_AdditionalComponentVM
    {
        public int? Engine_Part_Dbkey { get; set; }
        public int? BL_Engine_Dbkey { get; set; }

        public int? Type_Dbkey { get; set; }
        [Required(ErrorMessage = "Required")]
        public string? DrawingNumber { get; set; }

        public string? Revision { get; set; }
        [Required(ErrorMessage = "Required")]
        public int? QtyPerEngine { get; set; }
        [Required(ErrorMessage = "Required")]
        public string? Description { get; set; }
 
        public int? RawMaterial { get; set; }
        public int? Module_Responsibility { get; set; }
        public int? Parent_id { get; set; }
        public int? BuildId { get; set; }

    }


	public class SOP_AdditionalInfoComponentVM
	{
		public int Id { get; set; }
		public int? ReportingParent { get; set; }

		public string? Reporting_Type { get; set; }

		public string? AssemblyReportingType { get; set; }
	}



}
