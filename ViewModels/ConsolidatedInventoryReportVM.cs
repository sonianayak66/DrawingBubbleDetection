namespace MPCRS.ViewModels
{
    public class ConsolidatedInventoryReportVM
    {
        public int Raw_material_Dbkey { get; set; }
        public string? MaterialName { get; set; }
        public string? UOM { get; set; }
        public double TotalOrdered { get; set; }
        public double TotalReceived { get; set; }
        public double ProcurementBalance { get; set; }
        public double TotalIssued { get; set; }
        public double InventoryBalance { get; set; }
        public double? RequiredPerEngine { get; set; }
        public string? JobCardFileNames { get; set; }
        public string? JobCardNumber { get; set; }
        public string? JobCardFileLocations { get; set; }
        public string? RMType { get; set; }
        public string? ExecutionResponsibility { get; set; }
        public string? DemandNumbers { get; set; }
        public string? MMGFileNumbers { get; set; }
        public string? MMGDemandMapping { get; set; }  // ← ADD THIS LINE
        public string? DemandReceiptNumbers { get; set; }
        public string? VendorNames { get; set; }
        public string? VendorDbkeys { get; set; }
    }
}
