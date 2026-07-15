using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MPCRS.ViewModels
{
    public class MasterPartViewModel
    {
        public int Engine_Part_Dbkey { get; set; }

        public int Hierarchy { get; set; }

        public Nullable<int> Parent_id { get; set; }

        public string Engine_Title { get; set; }
        public int Engine_Dbkey { get; set; }

        public string Execution_Resp { get; set; }
        public string Reporting_Type { get; set; }

        [DisplayName("Type")]
        public int Type_Dbkey { get; set; }

        [Required(ErrorMessage = "Required")]
        [DisplayName("Drawing / Part No")]
        public string Draw_part_no { get; set; }
        [Required(ErrorMessage = "Required")]
        [DisplayName("Revision")]
        public string Revision { get; set; }

        [DisplayName("Drawing File")]
        public string Drawing_File { get; set; }

        [DisplayName("Revision File")]
        public string Revision_File { get; set; }

        [DisplayName("Solid Model")]
        public string Solid_Model { get; set; }
        [DisplayName("Solid Model #")]
        public string Solid_model_no { get; set; }
        [DisplayName("Drawing #")]
        public string Drawing_no { get; set; }

        public string Drawing_File_location { get; set; }
        public string Solid_Model_location { get; set; }

        [Required(ErrorMessage = "Required")]
        [DisplayName("Quantity Per Engine")]
        public Nullable<double> Quantity { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }
        [DisplayName("Comments")]
        public string Comments { get; set; }
        [DisplayName("Raw Material")]
        public Nullable<int> Raw_Material { get; set; }
        [DisplayName("Module Responsibility")]
        public Nullable<int> Module_Responsibility { get; set; }
        public Nullable<int> is_active { get; set; }

        [DisplayName("Manufacturing Duration")]
        [Required(ErrorMessage = "Required")]
        public Nullable<double> Manufacturing_Duration { get; set; }

        public string RevisionNotes { get; set; }
        public int RevisionApplied { get; set; }

        [DisplayName("Revision Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public System.DateTime Revision_Date { get; set; }

        public SelectList RevisionList { get; set; }
        public SelectList partstypes { get; set; }
        public SelectList Module_ResponsibilityList { get; set; }
        public SelectList Raw_MaterialList { get; set; }

        [DisplayName("is vendor material")]
        public Nullable<bool> is_vendor_material { get; set; } = false;
        [DisplayName("weight in kg")]
        public Nullable<double> weight_in_kg { get; set; }
        [DisplayName("Manufacturing Process")]
        public string FCBP { get; set; }
        [DisplayName("Bar Pipe Dia OD")]
        public Nullable<double> Bar_pipe_Dia_OD { get; set; }
        [DisplayName("Bar Pipe Length")]
        public Nullable<double> Bar_pipe_Length { get; set; }
        [DisplayName("Bar Pipe Thickness")]
        public Nullable<double> Bar_pipe_Thickness { get; set; }
        [DisplayName("Plate Width")]
        public Nullable<double> Plate_Width { get; set; }
        [DisplayName("Plate length")]
        public Nullable<double> Plate_length { get; set; }
        [DisplayName("Plate Thickness")]
        public Nullable<double> Plate_Thickness { get; set; }
        public Nullable<double> Area { get; set; }
        public Nullable<double> Density { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Required")]
        public int? Approver_ID { get; set; }

        public SelectList ReportingType { get; set; }

        public SelectList ParentParts { get; set; }

        public SelectList UserList { get; set; }

        public int Count_of_PendingApproving { get; set; }
        public bool is_rm_verified { get; set; }
        public Nullable<int> rm_verified_by { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> rm_updated_on { get; set; }

        public string AssemblyReportingType { get; set; }
        public Nullable<double> ReportDisplayOrder { get; set; }

        public int? Reporting_Parent { get; set; }
        public int? Part_relation_dbkey { get; set; }
        public string? Part_Remarks { get; set; }
        public string? Execution_Resp_additionalLevel { get; set; }
        public string? CollaboratorsId { get; set; }
        public string? Collaborators { get; set; }
        public int[] CollaboratorArr { get; set; }
        public string? ManufacturingComments { get; set; }
        public float? AssemblyDisplayOrder { get; set; }
		public SelectList manufacturingprocess { get; set; }
        public ApprovalRequestDetail approvalRequestDetail { get; set; }    
		public void LoadSelectLists()
        {
            is_active = 1;
            RevisionList = MPCRS.Utilities.Masters.RevisionList();
            Module_ResponsibilityList = MPCRS.Utilities.Masters.GetMaster_General("Module_Responsibility");
            Raw_MaterialList = MPCRS.Utilities.Masters.GetMaterialList();
            UserList = MPCRS.Utilities.Masters.GetOldUsersList();
            ReportingType = MPCRS.Utilities.Masters.ReportingTypeList();
            Revision_Date = DateTime.Now;
			manufacturingprocess = MPCRS.Utilities.Masters.ManufacturingProcessList();
        }

        public string? Module_PBS { get; set; }
        public string? PartType_PBS { get; set; }


}


    public class ApprovalRequestDetail
    {
        public string? Requested_By { get; set; }

        public DateTime? Requested_On { get; set; }

    }
}
