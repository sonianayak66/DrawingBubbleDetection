namespace MPCRS.Utilities
{
    public class EmailModel
    {
        public string MailSubject { get; set; }
        public string MailBody { get; set; }
        public string Parameters { get; set; }
        public string Recipients { get; set; }
        public string CopyTo { get; set; }
        public string BlindCopy { get; set; }
        public string emailAttachment { get; set; }
        public string emailAttachmentname { get; set; }
        public bool IsHTML { get; set; }
    }
}
