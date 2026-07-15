namespace MPCRS.ViewModels
{
    public class VendorRawMaterialDetails
    {
        public int Vendor_Dbkey { get; set; }
        public string Vendor_Name { get; set; }

        public int Raw_material_Dbkey { get; set; }
        public string Raw_material_Name { get; set; }

        // Add these only if your SSP returns them
        public double? Qty { get; set; }
        public double? TotalIssuedKg { get; set; }
        public double? BalanceKg { get; set; }

        // Optional: if you are returning badge html
        public string IssueStatusBadge { get; set; }
    }
}
