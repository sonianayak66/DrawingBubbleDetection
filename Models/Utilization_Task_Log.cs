using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Utilization_Task_Log
{
    public int Utli_Log_Dbkey { get; set; }

    public int? Session_ID { get; set; }

    public DateTime? Login_datetime { get; set; }

    public string? Task { get; set; }

    public string? Activity { get; set; }

    public string? SuperVicer { get; set; }

    public string? Remarks { get; set; }

    public DateTime? Task_Start { get; set; }

    public DateTime? Task_End { get; set; }

    public DateTime? Updated_On { get; set; }

    public int? Updated_By { get; set; }

    public string? Task_Status { get; set; }

    public string? Task_note { get; set; }
}
