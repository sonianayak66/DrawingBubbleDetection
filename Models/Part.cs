using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Part
{
    public int ID { get; set; }

    public int? ParentID { get; set; }

    public string? Part1 { get; set; }

    public string? Status { get; set; }
}
