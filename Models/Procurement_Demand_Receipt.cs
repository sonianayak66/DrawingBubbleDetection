using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Procurement_Demand_Receipt
{
    public int Receipt_dbkey { get; set; }

    public DateTime Receipt_Date { get; set; }

    public string? Receipt_No { get; set; }

    public int DemandDbKey { get; set; }

    public int DemandItemKey { get; set; }

    public double Physical_inventory { get; set; }

    public double Receiving_inventory { get; set; }

    public int Updated_By { get; set; }

    public DateTime Updated_on { get; set; }

    public int Index_No { get; set; }

    public int? Created_By { get; set; }

    public DateTime? Created_On { get; set; }

    public decimal? length { get; set; }

    public decimal? breadth { get; set; }

    public virtual Procurement_Demand DemandDbKeyNavigation { get; set; } = null!;

    public virtual Procurement_Demand_Item DemandItemKeyNavigation { get; set; } = null!;
}
