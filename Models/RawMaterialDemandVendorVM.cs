namespace MPCRS.ViewModels
{
    // Summary row per vendor
    public class RawMaterialVendorSummaryVM
    {
        public string VendorName { get; set; }
        public double OrderedQty { get; set; }
        public double ReceivedQty { get; set; }
        public double BalanceQty { get; set; }
    }

    // One row per demand line
    public class RawMaterialDemandDetailVM
    {
        public string Demand_No { get; set; }
        public string MMG_File_No { get; set; }
        public string Item_Description { get; set; }
        public string CurrentStatus { get; set; }
        public int DemandDbKey { get; set; }
        public int DemandItemKey { get; set; }
        public string VendorName { get; set; }
        public double OrderedQty { get; set; }
        public double ReceivedQty { get; set; }
        public double BalanceQty { get; set; }
        public DateTime? EstimatedOrderDate { get; set; }
        public DateTime? Planned_Date_of_receipt { get; set; }
    }

    // Wrapper returned as JSON to the view
    public class RawMaterialDemandVendorResultVM
    {
        public List<RawMaterialVendorSummaryVM> VendorSummary { get; set; } = new();
        public List<RawMaterialDemandDetailVM> DemandDetails { get; set; } = new();
    }
}
