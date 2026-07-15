using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Procurement_Milestone_Extension
{
    public int ExtensionID { get; set; }

    public int MilestoneID { get; set; }

    public int ExtensionNumber { get; set; }

    public DateTime? PreviousDueDate { get; set; }

    public DateTime NewDueDate { get; set; }

    public string ExtensionType { get; set; } = null!;

    public string? Reason { get; set; }

    public int ExtendedBy { get; set; }

    public DateTime ExtendedOn { get; set; }
}
