namespace MPCRS.ViewModels
{
    public class GenericMaterialIssueSummary_IssueHistory_VM
    {
        public DateTime? IssueDate { get; set; }
        public string? Reference_No { get; set; }
        public string? Vendor_Name { get; set; }
        public string? ForEngine { get; set; }
        public int IssuedQty { get; set; }
        public int TotalProcuredQty { get; set; }
        public int CumulativeIssued { get; set; }
        public int AvailableQty { get; set; }
    }
}