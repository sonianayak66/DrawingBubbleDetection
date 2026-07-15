using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NCR_Item_Rework
{
    public int ReworkID { get; set; }

    public int NCRItemKey { get; set; }

    public int StageID { get; set; }

    public string? NCRWorkFlowGuid { get; set; }

    public bool? IsRework { get; set; }

    public bool? IsTrialAssembly { get; set; }

    public string? ReworkDimension { get; set; }

    public int? MarkedBy { get; set; }

    public DateTime? MarkedOn { get; set; }

    public bool? IsUnmarked { get; set; }

    public int? UnmarkedBy { get; set; }

    public DateTime? UnmarkedOn { get; set; }

    public bool? IsCleared { get; set; }

    public int? ClearedBy { get; set; }

    public DateTime? ClearedOn { get; set; }

    public bool? IsActive { get; set; }
}
