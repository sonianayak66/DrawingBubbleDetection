namespace MPCRS.ViewModels
{
    public class AttachmentVM
    {
        public int? Attachment_Db_Key { get; set; }
        public string? AttachmentGUID { get; set; }
        public int? Source_table_key { get; set; }
        public string? Source_table { get; set; }
        public string? Orginal_File_Name { get; set; }
        public string? Attachment_FileName { get; set; }
        public string? Attachment_location { get; set; }
        public string? Attachment_type { get; set; }
        public IFormFile? uploadeddocument { set; get; }
        public DateTime? Updated_on { get; set; }

        public string? Revision { get; set; }
        public string? File_Revision { get; set; }
        public string? File_DVD_Num { get; set; }

        public string? Approver { get; set; }
        public string? Part_number { get; set; }
    }

    public class attachmentData
    {
        public int sourceAttachTableKey { get; set; }
        public string sourceAttachTable { get; set; }
        public bool deleteAccess { get; set; } = true;
    }


    public class UploadViewModel
    {
        public string JsonData { get; set; }
        public string qtySplits { get; set; }
        public string filesData { get; set; }
        public List<IFormFile> files { get; set; } 
    }


}
