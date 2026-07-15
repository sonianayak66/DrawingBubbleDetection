using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Procurement_Milestone_Merge_Source
{
    public int MergeSourceID { get; set; }

    public int ExtensionID { get; set; }

    public int SourceMilestoneID { get; set; }

    public DateTime? SourceDueDate { get; set; }

    public double? SourceQtyTotal { get; set; }
}
