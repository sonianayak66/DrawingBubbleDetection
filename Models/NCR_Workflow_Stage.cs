using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NCR_Workflow_Stage
{
    public int StageID { get; set; }

    public string StageName { get; set; } = null!;

    public string StageDescription { get; set; } = null!;

    public int StageOrder { get; set; }

    public bool? SkipForTrialAssembly { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }
}
