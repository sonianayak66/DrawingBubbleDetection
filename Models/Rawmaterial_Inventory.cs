using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Rawmaterial_Inventory
{
    public int RM_Inventory_Dbkey { get; set; }

    public int Raw_material_Dbkey { get; set; }

    public double Quantity { get; set; }

    public DateTime Trans_Datetime { get; set; }

    public string? Description { get; set; }

    public string Trans_Type { get; set; } = null!;

    public int? Updated_By { get; set; }

    public DateTime? Updated_on { get; set; }

    public virtual Master_Rawmaterial Raw_material_DbkeyNavigation { get; set; } = null!;
}
