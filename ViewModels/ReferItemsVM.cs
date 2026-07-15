namespace MPCRS.ViewModels
{
    public class ReferItemsVM
    {
        public string CurrentWorkflowGUID { get; set; }
        public string ItemKeys { get; set; } // Comma-separated NCRItemKeys
        public int ReferToModuleID { get; set; }
        public string ReferToUserGUIDs { get; set; } // Comma-separated
        public string Remarks { get; set; }
    }

    public class ReferItemsResultVM
    {
        public int Success { get; set; }
        public string Message { get; set; }
        public string NewReferralGuid { get; set; }
        public string ReferenceNumber { get; set; }
        public string NCRGuid { get; set; }
        public string StageName { get; set; }
        public string ReferToModuleName { get; set; }
        public int? ItemsReferred { get; set; }
        public string ReferredSerialNumbers { get; set; }
        public string AssignedUserEmails { get; set; }
        public string SenderEmail { get; set; }
    }
}
