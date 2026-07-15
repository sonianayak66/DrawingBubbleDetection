using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Procurement_Demand_Item
{
    public int DemandItemKey { get; set; }

    public int? DemandDbKey { get; set; }

    public int? ItemDbKey { get; set; }

    public string? LineItem { get; set; }

    public string? UOM { get; set; }

    public double? Qty { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public string? Outer_Dia_mm { get; set; }

    public string? Inner_Dia_mm { get; set; }

    public string? Thickness { get; set; }

    public string? Remarks { get; set; }

    public string? Item_Code { get; set; }

    public int? Engine_Part_Dbkey { get; set; }

    public string? Item_Sub_Type { get; set; }

    public string? height { get; set; }

    public double? ShortCloseQty { get; set; }

    public string? MMGOrderNumber { get; set; }

    public virtual ICollection<Procurement_Demand_Receipt> Procurement_Demand_Receipts { get; set; } = new List<Procurement_Demand_Receipt>();
}
