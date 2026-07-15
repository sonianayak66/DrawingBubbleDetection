using MPCRS.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;


namespace MPCRS.ViewModels
{
    public class ProjectVM
    {
        public int Project_Dbkey { get; set; }
        [DisplayName("Project Title")]
        [Required]
        public string Title { get; set; } = null!;
        [DisplayName("Display Title")]
        [Required]
        public string? Display_title { get; set; }
        [DisplayName("Description")]
        [Required]
        public string? Description { get; set; }
        [DisplayName("Date of Sanction")]
        [Required(ErrorMessage = "Required")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DOS { get; set; }=DateTime.Now;
        [DisplayName("Expected DOC")]
        [Required(ErrorMessage = "Required")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EDO { get; set; } = DateTime.Now;
        [DisplayName("Project Head")]
        [Required]
        public string Project_Number { get; set; } = null!;
        [DisplayName("Category")]
        public int? Category_Dbkey { get; set; }
        [DisplayName("Security classfication")]
        public int? Sec_Classfic_Dbkey { get; set; }
        [DisplayName("Attachment")]
        public string? Attachment_Name { get; set; }
      
        public string? Attachment_location { get; set; }
      
        [DisplayName("No of Engines")]
        [Required(ErrorMessage = "Required")]
        public double No_of_Engines { get; set; }
        [DisplayName("Unique Name")]
        public string Unique_Name { get; set; } = null!;

        public int? is_active { get; set; }

        public int? Updated_By { get; set; }

        public DateTime? Updated_on { get; set; }
        [Required]
        [DisplayName("Base Line Engine")]
        public int BL_Engine_Dbkey { get; set; }
        [DisplayName("Estimated Cost")]
        public double? EstimatedCost { get; set; }
        public string Engine_Title { get; set; } = null!;
        


        //public ProjectVM()
        //{
        //    Sec_Classfic_list = Masters.DTO.Masters.GetMaster_General("Security");
        //    Category_list = Masters.DTO.Masters.GetMaster_General("Category");
        //    Unique_NameList = Masters.DTO.Masters.GetAplhabetList();
        //    PriorityList = Masters.DTO.Masters.PriorityList();
        //    BaseLineEngineList = Masters.DTO.Masters.BaseLineEngineLists();
        //}
    }

    public class ProjectViewModel
    {
        List<ProjectVM>? projectslist { get; set; }
        ProjectVM? projectVM { get; set; }

    }
}
