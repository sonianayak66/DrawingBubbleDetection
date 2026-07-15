using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class MPLReportView
{
    public string? Draw_part_no { get; set; }

    public string? Description { get; set; }

    public string? Material_name { get; set; }

    public int Engine_Part_Dbkey { get; set; }

    public int? Parent_id { get; set; }

    public string Revision { get; set; } = null!;

    public int? Qty_per_Engine { get; set; }

    public string? ParentName { get; set; }

    public string? Reporting_Type { get; set; }

    public string? Type_Part_Name { get; set; }

    public string? IsParent { get; set; }

    public string? ParentRevision { get; set; }

    public double? ParentQuantity { get; set; }

    public string? ParentDescription { get; set; }

    public string? ParentMaterial_name { get; set; }

    public string? ParentReporting_type { get; set; }

    public string? PartModuleRes { get; set; }

    public string? ParentModuleRes { get; set; }

    public int BL_Engine_Dbkey { get; set; }

    public string? Execution_Resp { get; set; }

    public int is_active { get; set; }

    public string? AssemblyReportingType { get; set; }

    public double ReportDisplayOrder { get; set; }

    public int? Reporting_Parent { get; set; }

    public int Part_relation_dbkey { get; set; }

    public string? Part_Remarks { get; set; }

    public string Collaborators { get; set; } = null!;

    public string? ManufacturingComments { get; set; }

    public int Per_VendorStatus { get; set; }

    public bool ForSopOnly { get; set; }
}
