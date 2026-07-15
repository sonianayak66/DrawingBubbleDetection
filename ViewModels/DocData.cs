namespace MPCRS.ViewModels
{
    public class DocDisplayData
    {
        public int? parent { get; set; }
        public string[] docData { get; set; }
    }



    public class DocData
    {
        public int? Document_Dbkey { get; set; }
        public string? Refrence_Title { get; set; }
        public string? Item_type { get; set; }
        public int? Approved_Status { get; set; }
        public int? Parent_id { get; set; }
        public int? Updated_By { get; set; }
        public DateTime? Updated_On { get; set; }
        public bool? is_required_approve { get; set; }
        public int? Approved_by { get; set; }
        public bool? Inherit_Parent_Access { get; set; }
        public string? Description { get; set; }
        public string? File_Location { get; set; }
        public string? File_Name { get; set; }
        public string? System_File_Name { get; set; }
        public string? File_Size { get; set; }
        public string? File_type { get; set; }
        public string? SearchTags { get; set; }
    }

}
