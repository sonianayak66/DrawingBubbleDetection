using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Procurement_Milestone
{
    public int MilestoneID { get; set; }

    public int DemandDbKey { get; set; }

    public int MilestoneNumber { get; set; }

    public string? MilestoneName { get; set; }

    public DateTime OriginalDueDate { get; set; }

    public DateTime CurrentDueDate { get; set; }

    public double? QtyPercentage { get; set; }

    public bool? IsLastMilestone { get; set; }

    public string Status { get; set; } = null!;

    public string? Comments { get; set; }

    public DateTime? CompletionDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
