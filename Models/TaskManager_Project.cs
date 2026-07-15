using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TaskManager_Project
{
    public int ProjectId { get; set; }

    public Guid ProjectGUID { get; set; }

    public string ProjectName { get; set; } = null!;

    public string? ProjectDescription { get; set; }

    public string ProjectStatus { get; set; } = null!;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Priority { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsDeleted { get; set; }
}
