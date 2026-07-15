using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Task_Tree_Master
{
    public int task_master_dbkey { get; set; }

    public string title { get; set; } = null!;

    public string? Description { get; set; }

    public int? Responsible_Person { get; set; }

    public decimal? Weightage_percentage { get; set; }

    public string? task_status { get; set; }

    public int? Parent_id { get; set; }

    public int? updated_by { get; set; }

    public DateTime? updated_on { get; set; }

    public decimal? Estimated_Effort { get; set; }

    public string item_type { get; set; } = null!;
}
