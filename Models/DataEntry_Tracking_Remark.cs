using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class DataEntry_Tracking_Remark
{
    public int RemarkId { get; set; }

    public int ConfigId { get; set; }

    public int SourceRecordKey { get; set; }

    public string? RemarkText { get; set; }

    public bool NoUpdateNeeded { get; set; }

    public int? RemarkBy { get; set; }

    public DateTime? RemarkOn { get; set; }

    public virtual DataEntry_Tracking_Config Config { get; set; } = null!;

    public virtual ICollection<DataEntry_Tracking_History> DataEntry_Tracking_Histories { get; set; } = new List<DataEntry_Tracking_History>();
}
