using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Forging_Receipt_Item
{
    public int forging_item_dbkey { get; set; }

    public int forging_recp_dbkey { get; set; }

    public int Engine_Part_Dbkey { get; set; }

    public string? GTRE_Drawing_No { get; set; }

    public string? HAL_Drawing_No { get; set; }

    public double? Physical_Inventory { get; set; }

    public double? Receiving_Inventory { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public virtual ICollection<Forging_Split> Forging_Splits { get; set; } = new List<Forging_Split>();

    public virtual Forging_Receipt forging_recp_dbkeyNavigation { get; set; } = null!;
}
