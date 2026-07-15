using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NonConformanceReport_Item_Rework
{
    public int RecordID { get; set; }

    public int NCRItemKey { get; set; }

    public string StageName { get; set; } = null!;

    public int ReworkType { get; set; }

    public int? MarkedBy { get; set; }

    public DateTime? MarkedOn { get; set; }

    public bool? IsActive { get; set; }

    public string? Remarks { get; set; }
}
