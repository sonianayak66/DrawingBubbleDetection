using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Master_Part_Type
{
    public int Type_Dbkey { get; set; }

    public string Type_Part_Name { get; set; } = null!;

    public int is_active { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public int Hierarchy { get; set; }
}
