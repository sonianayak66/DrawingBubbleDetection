namespace MPCRS.ViewModels
{
    public class MarkCompleteResultVM
    {
        public int Success { get; set; }
        public string Message { get; set; }
        public int? ItemsCompleted { get; set; }

        // Email notification details
        public string ReferenceNumber { get; set; }
        public string NCRGuid { get; set; }
        public string StageName { get; set; }
        public int? StageID { get; set; }
        public string AdminEmails { get; set; }
        public string SenderEmail { get; set; }
    }
    public class MarkAsCompleteVM
    {
        public string NCRWorkflowGUID { get; set; }
    }
}
