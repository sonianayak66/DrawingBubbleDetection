using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Procurement_Demand_Item_Adjustment
{
    public int Adjustment_Dbkey { get; set; }

    public int DemandItemKey { get; set; }

    public double Adjustment_Qty { get; set; }

    public string Adjustment_Remarks { get; set; } = null!;

    public int? Adjusted_By { get; set; }

    public DateTime Adjusted_On { get; set; }
}
