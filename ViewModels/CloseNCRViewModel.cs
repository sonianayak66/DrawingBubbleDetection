namespace MPCRS.ViewModels
{
    public class CloseNCRViewModel
    {
        public CloseNCRHeaderVM Header { get; set; }
        public List<CloseNCRItemVM> Items { get; set; }
    }

    public class CloseNCRHeaderVM
    {
        public string NCRGuid { get; set; }
        public string ReferenceNumber { get; set; }
        public int? CloseNCR { get; set; }
        public string ReportStatus { get; set; }
        public string ECM_TR_NO { get; set; }
        public string ECM_No { get; set; }
        public int HasReworkOrTrialAssembly { get; set; }
    }

    public class CloseNCRItemVM
    {
        public int NCRItemKey { get; set; }
        public string SerialNumber { get; set; }
        public string Status { get; set; }
        public string DrawingDimension { get; set; }
        public string ActualDimension { get; set; }
        public string Module_Status { get; set; }
        public string TAS_Status { get; set; }
        public string Stress_Status { get; set; }
        public string Chair_Status { get; set; }
        public int? Rework_Status { get; set; }
        public string CurrentWorkflowStatus { get; set; }
    }

    // For the POST request from JS
    public class CloseNCRPostVM
    {
        public string NCRGuid { get; set; }
        public string ECM_TR_NO { get; set; }
        public string ReportStatus { get; set; }
        public string ECM_No { get; set; }
        public List<CloseNCRItemStatusVM> Items { get; set; }
    }

    public class CloseNCRItemStatusVM
    {
        public int NCRItemKey { get; set; }
        public string SerialNumber { get; set; }
        public string Status { get; set; }
    }
}