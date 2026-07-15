using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class InspectionReport_Updated
{
    public string Part_relation_dbkey { get; set; } = null!;

    public string? Drawing_No { get; set; }

    public string? Serial_No { get; set; }

    public string? Job_No { get; set; }

    public string? File_Name { get; set; }

    public string? File_Location { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
