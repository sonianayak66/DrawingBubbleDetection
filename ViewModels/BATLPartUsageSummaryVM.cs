namespace MPCRS.ViewModels
{
    //public class BATLPartUsageSummaryVM
    //{
    //    public List<string> BuildName { get; set; }

    //    public List<usageDetails> usageDetails { get; set; }
    //}

    //public class usageDetails
    //{
    //    public string? Draw_part_no { get; set; }
    //    public string? Serial_No { get; set; }
    //    public string? BuildName { get; set; }
    //    public string? MatchStatus { get; set; }
    //    public string? Used { get; set; }

    //}

    public class BATLPartUsageSummaryVM
    {
        public List<string> BuildName { get; set; } = new List<string>();
        public List<GroupedUsageDetails> GroupedUsageDetails { get; set; } = new List<GroupedUsageDetails>();
    }

    // New model for grouped usage details
    public class GroupedUsageDetails
    {
        public string Draw_part_no { get; set; }
        public string Description { get; set; }
        public string Revision { get; set; }
        public int Quantity { get; set; }
        public string SerialNumbers { get; set; }
        public string UsageSummary { get; set; }
        public int UsedCount { get; set; }
        public int TotalCount { get; set; }
    }
}
