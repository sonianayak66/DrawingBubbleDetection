using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Material_Issue_Items_Part
{
    public int Material_Issue_Items_Parts_Dbkey { get; set; }

    public int? Issue_Dbkey { get; set; }

    public int? Issue_Item_Dbkey { get; set; }

    public int? Engine_Part_Dbkey { get; set; }

    public string? Part_Name { get; set; }
}
