using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Demand_Document
{
    public int DocumentID { get; set; }

    public int? DemandDbKey { get; set; }

    public int? Master_Dbkey { get; set; }

    public string? Document_Name { get; set; }

    public string? Document_Location { get; set; }

    public string? Remarks { get; set; }

    public int? Updated_by { get; set; }

    public DateTime? Updated_On { get; set; }
}
