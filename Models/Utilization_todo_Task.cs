using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Utilization_todo_Task
{
    public int todo_task_dbkey { get; set; }

    public string? task_name { get; set; }

    public string? task_description { get; set; }

    public DateTime? task_due_date { get; set; }

    public string? File_location { get; set; }

    public string? attachment_file_name { get; set; }

    public string? system_file_name { get; set; }

    public int? task_assigned_by { get; set; }

    public int? task_assigned_to { get; set; }

    public DateTime? Updated_on { get; set; }

    public int? Updated_by { get; set; }

    public int? report_to { get; set; }

    public int? task_master_dbkey { get; set; }
}
