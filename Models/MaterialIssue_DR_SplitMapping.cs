using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class MaterialIssue_DR_SplitMapping
{
    public int split_issue_id { get; set; }

    public int? Issue_Item_Dbkey { get; set; }

    public int? Issue_Dbkey { get; set; }

    public int? DR_Item_SplitId { get; set; }

    public int? Updated_by { get; set; }

    public DateTime? Updated_On { get; set; }
}
