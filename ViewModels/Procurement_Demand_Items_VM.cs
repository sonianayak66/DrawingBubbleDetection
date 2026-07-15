using Microsoft.AspNetCore.Mvc.Rendering;

namespace MPCRS.ViewModels
{
    public class Procurement_Demand_Items_VM
    {
        public int? DemandItemKey { get; set; }
        public string? Itemtype { get; set; }
        public Nullable<int> DemandDbKey { get; set; }
        public Nullable<int> ItemDbKey { get; set; }
        public string? LineItem { get; set; }
        public string? UOM { get; set; }
        public string? Outer_Dia_mm { get; set; }
        public string? height { get; set; }
        public string? Inner_Dia_mm { get; set; }
        public string?  Thickness { get; set; }
        public Nullable<double> Qty { get; set; }
        public SelectList Outer_Dia_mm_list { get; set; }
        public SelectList Thickness_list { get; set; }
        public SelectList Inner_Dia_mm_list { get; set; }
        public SelectList height_list { get; set; }
        public string? Remarks { get; set; }
        public string? Item_Code { get; set; }
        public Nullable<int> Engine_Part_Dbkey { get; set; }
        public string Item_Sub_Type { get; set; }
        public string New_Item_name { get; set; }
        public string MMGOrderNumber { get; set; }
        public Nullable<float> ShortCloseQty { get; set; }

        public double TotalAdjustment { get; set; } = 0;


    }

}
