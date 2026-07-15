using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class DataEntry_Tracking_History
{
    public int HistoryId { get; set; }

    public int RemarkId { get; set; }

    public string Action { get; set; } = null!;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public int? ActionBy { get; set; }

    public DateTime? ActionOn { get; set; }

    public virtual DataEntry_Tracking_Remark Remark { get; set; } = null!;
}
