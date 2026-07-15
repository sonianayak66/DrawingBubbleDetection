using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Mail_Template
{
    public int Mail_Temp_ID { get; set; }

    public string? Mail_Temp_Name { get; set; }

    public string Mail_Subject { get; set; } = null!;

    public string Mail_Body { get; set; } = null!;

    public string? Parameters { get; set; }

    public string? Recipients { get; set; }

    public string? CopyTo { get; set; }

    public string? BlindCopy { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public string? Source_table_name { get; set; }

    public string? EmailTriggerDays { get; set; }
}
