using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Procurement_Demands_History
{
    public int Demand_Procurement_History_Key { get; set; }

    public int? DemandDbKey { get; set; }

    public DateTime? ActionDate { get; set; }

    public string? ActionStatus { get; set; }

    public bool? Do_Review { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public string? Remarks { get; set; }
}
