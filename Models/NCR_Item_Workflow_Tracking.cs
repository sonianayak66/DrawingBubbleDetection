using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NCR_Item_Workflow_Tracking
{
    public int TrackingID { get; set; }

    public int NCRItemKey { get; set; }

    public string? NCRWorkFlowGuid { get; set; }

    public int StageID { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }
}
