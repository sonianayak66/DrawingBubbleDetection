using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Procurement_Demand_MileStone
{
    public int MilestoneDbKey { get; set; }

    public int? DemandDbkey { get; set; }

    public int? DemandItemDbKey { get; set; }

    public int? Milestone { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public double? Qty { get; set; }

    public int? Updatedby { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? MilestoneID { get; set; }
}
