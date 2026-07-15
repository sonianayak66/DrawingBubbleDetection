namespace MPCRS.ViewModels
{
    public class ModuleWiseNCRListItemVM
    {
        public string NCRGuid { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string ReferenceNumber { get; set; }
        public string PartNo { get; set; }
        public string SerialNumber { get; set; }
        public string Revision { get; set; }
        public string VendorName { get; set; }
        public string ModuleName { get; set; }
        public string ModuleReferredOtherDeviation { get; set; }
        public string Status { get; set; }
    }
    public class ModuleWiseNCRSummaryVM
    {
        public string ModuleName { get; set; }
        public int NCRCount { get; set; }
    }

    public class ModuleWiseNCRListVM
    {
        public List<ModuleWiseNCRListItemVM> Items { get; set; } = new();
        public List<ModuleWiseNCRSummaryVM> Summary { get; set; } = new();
    }

    public class ModuleWiseNCRPopupSummaryVM
    {
        public string ModuleName { get; set; }
        public int NCRCount { get; set; }
    }

    public class ModuleWiseNCRPopupItemVM
    {
        public string NCRGuid { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string ReferenceNumber { get; set; }
        public string PartNo { get; set; }
        public string ModuleName { get; set; }
        public string FinalStatus { get; set; }
    }

    public class ModuleWiseNCRPopupVM
    {
        public ModuleWiseNCRPopupSummaryVM Summary { get; set; } = new();
        public List<ModuleWiseNCRPopupItemVM> Items { get; set; } = new();
    }
}