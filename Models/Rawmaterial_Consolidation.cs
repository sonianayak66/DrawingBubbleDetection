using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Rawmaterial_Consolidation
{
    public int Consolidated_dbkey { get; set; }

    public int? Receipt_dbkey { get; set; }

    public double? Measurement { get; set; }

    public string? UOM { get; set; }

    public string? Material_Reference_No { get; set; }

    public string? Heat_No { get; set; }

    public string? Batch_No { get; set; }

    public int? Updated_by { get; set; }

    public DateTime? Updated_on { get; set; }

    public int? Attachment_Db_Key { get; set; }

    public double? Measurement_breadth { get; set; }

    public double? Weight { get; set; }

    public string? AdditionalinfoJson { get; set; }

    public virtual ICollection<Material_IssueItems_Consolidation> Material_IssueItems_Consolidations { get; set; } = new List<Material_IssueItems_Consolidation>();
}
