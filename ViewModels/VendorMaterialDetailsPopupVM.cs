namespace MPCRS.ViewModels
{
    public class VendorMaterialDetailsPopupVM
    {
        public string VendorName { get; set; }
        public int VendorDbkey { get; set; }

        public string MaterialName { get; set; }
        public int RawMaterialDbkey { get; set; }

        // Summary
        public VendorMaterialSummary Summary { get; set; }

        // Details
        public List<DemandDetail> Demands { get; set; }
        public List<IssueDetail> Issues { get; set; }
    }

    public class VendorMaterialSummary
    {
        public int TotalDemands { get; set; }
        public double TotalOrdered { get; set; }
        public double TotalReceived { get; set; }
        public double TotalBalance { get; set; }

        public int TotalIssues { get; set; }
        public double TotalIssued { get; set; }

        public double CurrentStock { get; set; }  // Received - Issued
    }
}