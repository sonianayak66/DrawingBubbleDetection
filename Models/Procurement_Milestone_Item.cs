using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Procurement_Milestone_Item
{
    public int MilestoneItemID { get; set; }

    public int MilestoneID { get; set; }

    public int DemandItemDbKey { get; set; }

    public double? Qty { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
