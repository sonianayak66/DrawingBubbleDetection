using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NCR_Workflow_Assignment_User
{
    public int AssignmentUserID { get; set; }

    public string NCRWorkflowGUID { get; set; } = null!;

    public int ModuleID { get; set; }

    public string UserGUID { get; set; } = null!;
}
