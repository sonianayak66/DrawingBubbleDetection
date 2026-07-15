using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NCR_Workflow
{
    public int NCRWorkflowID { get; set; }

    public string? NCRGUID { get; set; }

    public int? ModuleID { get; set; }

    public string? AssigneeUserGUIDs { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public DateTime? WorkUpdatedOn { get; set; }

    public string? AssignedBy { get; set; }

    public DateTime? AssignedOn { get; set; }
}
