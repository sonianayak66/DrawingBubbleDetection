using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Job_Card
{
    public int JobCard_Dbkey { get; set; }

    public string? JobCard_No { get; set; }

    public string? Engine { get; set; }

    public string? Request_No { get; set; }

    public DateTime? JobCard_Date { get; set; }

    public string? Tech_Development_Type { get; set; }

    public string? Nomenclature { get; set; }

    public string? Drawing_No { get; set; }

    public string? Issue_No { get; set; }

    public string? Module { get; set; }

    public int? Quantity { get; set; }

    public string? Material_Details { get; set; }

    public string? Technology_Description { get; set; }

    public string? Scope_Of_Work { get; set; }

    public DateTime? JC_Opened_On { get; set; }

    public DateTime? JC_Closed_On { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }
}
