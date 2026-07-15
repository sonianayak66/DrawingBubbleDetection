using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Master_Activity
{
    public int Activity_Db_key { get; set; }

    public string? Activity_Name { get; set; }

    public string? Type { get; set; }

    public int? Updated_by { get; set; }

    public DateTime? Updated_on { get; set; }

    public virtual ICollection<Audit_log> Audit_logs { get; set; } = new List<Audit_log>();
}
