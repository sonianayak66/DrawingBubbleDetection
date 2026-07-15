using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Utilization_todo_StatusLog
{
    public int logdbkey { get; set; }

    public int? task_id { get; set; }

    public string? task_status { get; set; }

    public DateTime? status_date { get; set; }

    public string? note { get; set; }

    public string? File_location { get; set; }

    public string? attachment_file_name { get; set; }

    public string? system_file_name { get; set; }

    public DateTime? updated_on { get; set; }

    public int? updated_by { get; set; }
}
