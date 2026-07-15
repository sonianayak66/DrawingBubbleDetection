using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TM_Task
{
    public int TaskId { get; set; }

    public Guid? TaskGuid { get; set; }

    public int? ProjectId { get; set; }

    public string? TaskTitle { get; set; }

    public string? TaskDescription { get; set; }

    public int? StatusId { get; set; }

    public int? Priority { get; set; }

    public DateTime? DueDate { get; set; }

    public decimal? EstimatedHours { get; set; }

    public decimal? ActualHours { get; set; }

    public DateTime? CompletedDate { get; set; }

    public string? SourceEmailId { get; set; }

    public string? CreatedBy { get; set; }

    public bool? IsDeleted { get; set; }
}
