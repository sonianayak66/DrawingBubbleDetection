using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class InspectionReport
{
    public int Inspect_Rpt_key { get; set; }

    public int? Inspect_Rpt_Dbkey { get; set; }

    public long? File_No { get; set; }

    public string Part_relation_dbkey { get; set; } = null!;

    public string? Drawing_No { get; set; }

    public string? Serial_No { get; set; }

    public string? Job_No { get; set; }

    public string? File_Name { get; set; }

    public string? File_Location { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? BuildNumber { get; set; }

    public string? Remarks { get; set; }

    public string? Revision { get; set; }

    public string? Quantity { get; set; }

    public string? RMC_Number { get; set; }
}
