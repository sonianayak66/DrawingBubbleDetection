using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Engine_Parts_Usage
{
    public int Part_relation_dbkey { get; set; }

    public int BL_Engine_Dbkey { get; set; }

    public int? Engine_Dbkey { get; set; }

    public int Engine_Part_Dbkey { get; set; }

    public int? Parent_id { get; set; }

    public int is_active { get; set; }

    public string Revision { get; set; } = null!;

    public int? Qty_per_Engine { get; set; }

    public int? Updated_by { get; set; }

    public DateTime? Updated_on { get; set; }

    public string? Description { get; set; }

    public string? Comments { get; set; }

    public int? Reporting_Parent { get; set; }

    public string? Part_Remarks { get; set; }

    public double? ReportDisplayOrder { get; set; }

    public int? Module_Responsibility { get; set; }

    public int? Raw_Material { get; set; }

    public string? Execution_Resp { get; set; }

    public string? Execution_Resp_additionalLevel { get; set; }

    public string? CollaboratorsId { get; set; }

    public string? Collaborators { get; set; }

    public string? ManufacturingComments { get; set; }

    public bool? ForSopOnly { get; set; }

    public string? Module_PBS { get; set; }

    public string? PartType_PBS { get; set; }

    public virtual Base_Line_Engine BL_Engine_DbkeyNavigation { get; set; } = null!;

    public virtual Engine_Parts_Master Engine_Part_DbkeyNavigation { get; set; } = null!;
}
