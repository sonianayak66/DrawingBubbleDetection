using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NCR_Workflow_Assignments_Log
{
    public int NCRWorkflowLogsID { get; set; }

    public string? NCRWorkflowGUID { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public bool? IsTransfered { get; set; }

    public string? Status_Verbose { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? NCRGuid { get; set; }

    public int? NCRItemKey { get; set; }
}
