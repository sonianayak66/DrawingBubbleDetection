using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Material_IssueItems_Consolidation
{
    public int split_issue_id { get; set; }

    public int? Issue_Item_Dbkey { get; set; }

    public int? Issue_Dbkey { get; set; }

    public int? Consolidated_dbkey { get; set; }

    public int? Updated_by { get; set; }

    public DateTime? Updated_on { get; set; }

    public virtual Rawmaterial_Consolidation? Consolidated_dbkeyNavigation { get; set; }
}
