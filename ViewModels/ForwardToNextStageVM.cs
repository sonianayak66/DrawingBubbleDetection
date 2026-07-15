namespace MPCRS.ViewModels
{
    public class ForwardToNextStageVM
    {
        public string NCRWorkflowGUID { get; set; }
        public int NextStageID { get; set; }
        public int NextModuleID { get; set; }
        public string AssignedUserGUIDs { get; set; } // Comma-separated
    }

    public class ForwardStageResultVM
    {
        public int Success { get; set; }
        public string Message { get; set; }
        public string NewWorkflowGuid { get; set; }
        public string NextStageName { get; set; }
        public string ReferenceNumber { get; set; }
        public string NCRGuid { get; set; }
        public string AssignedUserEmails { get; set; }
        public string SenderEmail { get; set; }
        public string ModuleName { get; set; }
        public int? TotalItems { get; set; }
    }
}
