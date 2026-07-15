using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class User_Module
{
    public int Module_Dbkey { get; set; }

    public string Module_Name { get; set; } = null!;

    public string Url { get; set; } = null!;

    public string Page_title { get; set; } = null!;

    public int? Default_rights { get; set; }

    public int? Updated_by { get; set; }

    public DateTime? Updated_on { get; set; }

    public string? MenuGroup { get; set; }

    public double? MenuGroupOrder { get; set; }

    public string? MenuItem { get; set; }

    public double? MenuItemOrder { get; set; }
}
