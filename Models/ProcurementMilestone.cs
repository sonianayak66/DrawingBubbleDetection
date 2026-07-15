using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ProcurementMilestone
{
    public int MilestoneID { get; set; }

    public int? DemandDbKey { get; set; }

    public string? MilestoneName { get; set; }

    public string? Components { get; set; }

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? CompletionDate { get; set; }

    public string? Status { get; set; }

    public string? Comments { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public double? QtyPercentage { get; set; }

    public bool? IsLastMilestone { get; set; }
}
