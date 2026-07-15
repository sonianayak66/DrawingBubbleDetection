using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NCR_Workflow_Assignment
{
    public int NCRWorkflowID { get; set; }

    public string? NCRWorkflowGUID { get; set; }

    public string? NCRGUID { get; set; }

    public int? ModuleID { get; set; }

    public string? AssigneeUserGUIDs { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public DateTime? WorkUpdatedOn { get; set; }

    public int? AssignedBy { get; set; }

    public DateTime? AssignedOn { get; set; }
}
