using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TaskManager_TaskChecklist
{
    public int ChecklistId { get; set; }

    public Guid ChecklistGUID { get; set; }

    public Guid TaskGUID { get; set; }

    public string ItemText { get; set; } = null!;

    public bool IsCompleted { get; set; }

    public int SortOrder { get; set; }

    public int? CompletedBy { get; set; }

    public DateTime? CompletedDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool IsDeleted { get; set; }
}
