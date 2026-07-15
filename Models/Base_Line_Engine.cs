using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Base_Line_Engine
{
    public int BL_Engine_Dbkey { get; set; }

    public string? Engine_Title { get; set; }

    public string? Engine_Description { get; set; }

    public int? is_active { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_on { get; set; }

    public DateTime? Revision_date { get; set; }

    public string? Revision_title { get; set; }

    public string? Updated_By_UserGuid { get; set; }

    public virtual ICollection<Engine_Parts_Usage> Engine_Parts_Usages { get; set; } = new List<Engine_Parts_Usage>();

    public virtual ICollection<Engine> Engines { get; set; } = new List<Engine>();
}
