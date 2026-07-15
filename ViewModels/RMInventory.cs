namespace MPCRS.ViewModels
{
    public class RMInventory
    {
        public string? TransType { get; set; }
        public int? OrderSequence { get; set; }
        public int? RowNumber { get; set; }
        public string? Receipt_Date { get; set; }
        public string? Receipt_No { get; set; }
        public string? Raw_material_Name { get; set; }
        public int? Raw_material_Dbkey { get; set; }
        public double? Qty { get; set; }
        public string? UOM { get; set; }
        public string? Outer_Dia_mm { get; set; }
        public string? Thickness { get; set; }
        public string? RMType { get; set; }
        public string? RM_identifier { get; set; }
        public string? Vendor_Name { get; set; }
        public string? IssuePurpose { get; set; }
        public double? RunningBalance { get; set; }
        public string? MMGOrderNumber { get; set; }
    }
}
