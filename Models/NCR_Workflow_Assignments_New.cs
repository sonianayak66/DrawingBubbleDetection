using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NCR_Workflow_Assignments_New
{
    public string NCRWorkflowGUID { get; set; } = null!;

    public string NCRGUID { get; set; } = null!;

    public int StageID { get; set; }

    public string? Status { get; set; }

    public int? AssignedBy { get; set; }

    public DateTime? AssignedOn { get; set; }

    public DateTime? WorkUpdatedOn { get; set; }

    public bool? IsActive { get; set; }
}
