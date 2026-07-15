using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class DataEntry_Tracking_Config
{
    public int ConfigId { get; set; }

    public string ModuleName { get; set; } = null!;

    public string SourceTable { get; set; } = null!;

    public string PrimaryKeyColumn { get; set; } = null!;

    public string? DisplayColumns { get; set; }

    public string UpdatedOnColumn { get; set; } = null!;

    public int UpdateFrequencyDays { get; set; }

    public int? AmberThresholdDays { get; set; }

    public string? ExclusionCondition { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual ICollection<DataEntry_Tracking_Remark> DataEntry_Tracking_Remarks { get; set; } = new List<DataEntry_Tracking_Remark>();
}
