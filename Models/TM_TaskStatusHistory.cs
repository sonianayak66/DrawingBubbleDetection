using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TM_TaskStatusHistory
{
    public int HistoryId { get; set; }

    public Guid? HistoryGuid { get; set; }

    public int? TaskId { get; set; }

    public int? FromStatusId { get; set; }

    public int? ToStatusId { get; set; }

    public DateTime? ChangedDate { get; set; }

    public string? ChangedBy { get; set; }

    public string? Comments { get; set; }
}
