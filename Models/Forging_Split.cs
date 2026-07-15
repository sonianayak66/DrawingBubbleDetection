using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Forging_Split
{
    public int forging_item_split_dbkey { get; set; }

    public int? forging_item_dbkey { get; set; }

    public string? part_name { get; set; }

    public string? GTRE_Drawing_No { get; set; }

    public string? Batch_Number { get; set; }

    public string? Heat_Number { get; set; }

    public string? Sl_No_Forging { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public string? Attachment_Db_Key { get; set; }

    public virtual Forging_Receipt_Item? forging_item_dbkeyNavigation { get; set; }
}
