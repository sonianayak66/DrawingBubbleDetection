using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Forging_Receipt
{
    public int forging_recp_dbkey { get; set; }

    public string Receipt_Number { get; set; } = null!;

    public DateTime Receipt_Date { get; set; }

    public string? MMG_File_No { get; set; }

    public double? Total_Qty { get; set; }

    public int? Issue_Item_Dbkey { get; set; }

    public int? Raw_material_Dbkey { get; set; }

    public int? Vendor_Dbkey { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public virtual ICollection<Forging_Receipt_Item> Forging_Receipt_Items { get; set; } = new List<Forging_Receipt_Item>();
}
