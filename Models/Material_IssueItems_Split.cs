using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Material_IssueItems_Split
{
    public int split_issue_id { get; set; }

    public int? Issue_Item_Dbkey { get; set; }

    public int? Issue_Dbkey { get; set; }

    public int? SplitId { get; set; }

    public int? Updated_by { get; set; }

    public DateTime? Updated_on { get; set; }

    public virtual Procurement_ReceiptItemSplit? Split { get; set; }
}
