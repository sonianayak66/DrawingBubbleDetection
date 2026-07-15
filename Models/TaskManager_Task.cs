using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TaskManager_Task
{
    public int TaskId { get; set; }

    public Guid TaskGUID { get; set; }

    public Guid ProjectGUID { get; set; }

    public Guid? BucketGUID { get; set; }

    public string TaskTitle { get; set; } = null!;

    public string? TaskDescription { get; set; }

    public string? Priority { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? StartDateTime { get; set; }

    public DateTime? EndDateTime { get; set; }

    public string? Tags { get; set; }

    public int? ProgressPercentage { get; set; }

    public decimal? EstimatedHours { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsPrivate { get; set; }
}
