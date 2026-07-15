using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class IssueHistory
{
    public int HistoryId { get; set; }

    public int? SlNo { get; set; }

    public string? OldStatus { get; set; }

    public string? NewStatus { get; set; }

    public string? ChangedBy { get; set; }

    public DateTime? ChangedDate { get; set; }
}
