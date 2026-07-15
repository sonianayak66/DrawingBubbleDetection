using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TM_TaskAssignment
{
    public int AssignmentId { get; set; }

    public Guid? AssignmentGuid { get; set; }

    public int? TaskId { get; set; }

    public string? AssignedTo { get; set; }

    public DateTime? AssignedDate { get; set; }

    public string? AssignedBy { get; set; }

    public bool? IsDeleted { get; set; }
}
