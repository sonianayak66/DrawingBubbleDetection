namespace MPCRS.ViewModels
{
    public class MaterialDetailsPopupVM
    {
        public string MaterialName { get; set; }
        public int RawMaterialDbkey { get; set; }
        // Summary
        public MaterialSummary Summary { get; set; }

        // Details
        public List<DemandDetail> Demands { get; set; }
        public List<IssueDetail> Issues { get; set; }
    }

    public class MaterialSummary
    {
        public int TotalDemands { get; set; }
        public double TotalOrdered { get; set; }
        public double TotalReceived { get; set; }
        public double TotalBalance { get; set; }

        public int TotalIssues { get; set; }
        public double TotalIssued { get; set; }

        public double CurrentStock { get; set; }  // Received - Issued
    }

    public class DemandDetail
    {
        public int DemandDbKey { get; set; }
        public string MMGFileNo { get; set; }
        public string ItemDescription { get; set; }
        public string VendorName { get; set; }
        public double OrderedQty { get; set; }
        public double ReceivedQty { get; set; }
        public double Balance { get; set; }
        public string CurrentStatus { get; set; }
        public DateTime? PlannedDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
    }

    public class IssueDetail
    {
        public DateTime? IssueDate { get; set; }
        public string ReferenceNo { get; set; }
        public string VendorName { get; set; }
        public double IssueQty { get; set; }
        public string ForEngine { get; set; }
        public string SerialNumbers { get; set; }
        public string JobCardNumber { get; set; }
        
        public string PartName { get; set; } //Add
    }
    


}