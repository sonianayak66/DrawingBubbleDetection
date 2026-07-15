using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TaskTracker
{
    public int TaskId { get; set; }

    public int? ProjectId { get; set; }

    public string? TaskName { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Remarks { get; set; }

    public int? Vendorid { get; set; }

    public int? ResponsibleUnit { get; set; }

    public int? Dependancy { get; set; }

    public int? Category { get; set; }

    public string? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }
}
