using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
	public class DocumentsViewModel
	{
		public int Document_Dbkey { get; set; }
        [Required(ErrorMessage = "Required")]
        [DisplayName("Title")]
        public string Refrence_Title { get; set; } = null!;
		public string? Description { get; set; }
		public string Item_type { get; set; } = null!;
		public string? File_Location { get; set; }
        [DisplayName("File")]
        public string? File_Name { get; set; }
		public string? System_File_Name { get; set; }
        [DisplayName("File Size")]
        public string? File_Size { get; set; }
        [DisplayName("File Type")]
        public string? File_type { get; set; }
        [DisplayName("IsApproved")]
        public bool is_required_approve { get; set; } = false;
        public string? Approved_Status_text { get; set; }
        public Nullable<int> Approved_Status { get; set; }
        public int Parent_id { get; set; }
		public string? Updated_By { get; set; }
		public string? Updated_On { get; set; }
		public string? is_required_approve_text { get; set; }
		public string? Approved_by { get; set; }
		public int? is_active { get; set; }
        [DisplayName("Search Tags")]
        public string? SearchTags { get; set; }
		public string? Note { get; set; }
        [DisplayName("Inhertit Parent Access")]
        public bool Inherit_Parent_Access { get; set; } = false;
		public string? Inherit_Parent_Access_text { get; set; }
		public int? Inherit_Access_From { get; set; }
		public string? Parent { get; set; }
		public string? Updated_By_UserName { get; set; }
		public string? Approved_by_UserName { get; set; }
        public List<IFormFile> Files { get; set; }
        public bool ReadAccess { get; set; } = false;
        public bool WriteAccess { get; set; } = false;
        public bool DownloadAccess { get; set; } = false;
        public string? ReadAccesstree { get; set; } 
        public string? WriteAccesstree { get; set; } 
        public string? DownloadAccesstree { get; set; }
        public string? DisplayTitle { get; set; }
        public string? FolderName { get; set; }
        public int? SuperUser { get; set; }
        public int? Status_In_VectorDB { get; set; }
       
        public string? VectorExecutionStartTime { get; set; }

        public string? VectorExecutionEndTime { get; set; }

       public string? TotalFileCount { get; set; }
        public string? EmbeddedFileCount { get; set; }
        //used to store location,width,height of tiff to png converted images for viewing
        public List<(string, int, int)>? TiffToPngConvertedLocation { get; set; }
    }

}
