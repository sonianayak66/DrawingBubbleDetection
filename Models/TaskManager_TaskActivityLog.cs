using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TaskManager_TaskActivityLog
{
    public int ActivityId { get; set; }

    public Guid ActivityGUID { get; set; }

    public Guid TaskGUID { get; set; }

    public string ActivityType { get; set; } = null!;

    public string? FieldName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Description { get; set; }

    public string? TargetName { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsDeleted { get; set; }
}
