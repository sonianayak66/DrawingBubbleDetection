namespace MPCRS.ViewModels
{
    public class DemandSummaryDashboard
    {
        public int? Engine_Part_Dbkey { get; set; }
        public string? Draw_part_no { get; set; }
        public string? Description { get; set; }
        public string? Vendor_Name { get; set; }
        public int? OrderQty { get; set; }
        public int? RecievedQty { get; set; }
        public string? Remarks { get; set; }
        public int? QuantityPerEngine { get; set; }
        public int? IssuedQty { get; set; }
    }
}
