namespace MPCRS.ViewModels
{
    public class ReportMaterialIssueModel
    {
        public int? Issue_Dbkey { get; set; }
        public string? Order_Ref_No { get; set; }
        public DateTime? Order_Ref_Date { get; set; }
        public int? Raw_material_Dbkey { get; set; }
        public string? Drawing_no { get; set; }
        public string? Description { get; set; }
        public int? Qty { get; set; }
        public int? Qty_Issue { get; set; }
        public double? Weight_Kg { get; set; }
        public string? outer_dia { get; set; }
        public string? thickness { get; set; }
        public string? PartNumber { get; set; }
        public string? Raw_material_Name { get; set; }
        public int? QtyperEngine { get; set; }
        public string? Vendor_Name { get; set; }
        public string? Issue_Purpose { get; set; }      
        public string? Part_Name { get; set; }
    }

    public class DemandItemQtyStatus
    {
        public string? Project { get; set; }
        public int? DemandDbKey { get; set; }
        public string? Demand_No { get; set; }
        public string? Item_Type { get; set; }
        public string? MMG_File_No { get; set; }
        public string? Receipt_No { get; set; }
        public DateTime? Receipt_Date { get; set; }
        public string? Item { get; set; }
        public int? Receiving_inventory { get; set; }
        public float? SumOfSplitQty { get; set; }
        public float? QtyVariance { get; set; }
        public string? SplitStatus { get; set; }
        public string? DocumentStatus { get; set; }
        public string? DocAvaililty { get; set; }
        public int? Receipt_dbkey { get; set; }

    }

    public class DemandEntryStatusModel
    {
         public List<DemandItemQtyStatus> demandItemQtyStatus { get; set; }   
    }

}
