using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Drawing_Bubble_Inspection_Item
{
    public int Item_Dbkey { get; set; }

    public int Inspection_Dbkey { get; set; }

    public string Bubble_Number { get; set; } = null!;

    public string? Dimension { get; set; }

    public decimal Confidence { get; set; }

    public bool Needs_Review { get; set; }

    public bool Is_Manually_Added { get; set; }

    public bool Is_Corrected { get; set; }

    public int? X_Coordinate { get; set; }

    public int? Y_Coordinate { get; set; }

    public int? Radius { get; set; }

    public bool? Is_Active { get; set; }

    public string? Tolerance { get; set; }
}
