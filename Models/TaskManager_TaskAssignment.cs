using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TaskManager_TaskAssignment
{
    public int AssignmentId { get; set; }

    public Guid TaskGUID { get; set; }

    public int AssignedUserDbkey { get; set; }

    public int AssignedBy { get; set; }

    public DateTime AssignedDate { get; set; }

    public bool IsDeleted { get; set; }
}
