using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Demand_Verification
{
    public int Verification_Id { get; set; }

    public string? Demand_No { get; set; }

    public string? Demand_Desc { get; set; }

    public string? Demanding_Officer { get; set; }

    public string? Project { get; set; }

    public bool? Items { get; set; }

    public bool? Receipt { get; set; }

    public bool? Receipt_Docs { get; set; }

    public string? Remarks { get; set; }

    public bool? Verified { get; set; }

    public string? Verified_By { get; set; }
}
