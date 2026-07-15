using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class RM_Qty_Vendor_Split
{
    public int ID { get; set; }

    public int Raw_material_Dbkey { get; set; }

    public int Vendor_Dbkey { get; set; }

    public double? Qty { get; set; }

    public DateTime? Updated_On { get; set; }

    public int? Updated_By { get; set; }
}
