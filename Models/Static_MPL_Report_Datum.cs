using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Static_MPL_Report_Datum
{
    public int Sl_No { get; set; }

    public int? BL_Engine_Dbkey { get; set; }

    public int? Engine_Part_Dbkey { get; set; }

    public int? Parent_id { get; set; }

    public int? Level { get; set; }

    public string? Draw_part_no { get; set; }

    public string? Description { get; set; }

    public string? Material_name { get; set; }

    public string? ParentName { get; set; }

    public string? Type_Part_Name { get; set; }

    public string? Reporting_Type { get; set; }

    public string? IsParent { get; set; }

    public int? Qty_per_Engine { get; set; }

    public string? Revision { get; set; }

    public string? ParentRevision { get; set; }

    public double? ParentQuantity { get; set; }

    public string? ParentDescription { get; set; }

    public string? ParentMaterial_name { get; set; }

    public string? ParentReporting_type { get; set; }

    public string? ParentModuleRes { get; set; }

    public string? PartModuleRes { get; set; }

    public string? Hierarchy { get; set; }

    public string? PartPath { get; set; }

    public string? ReportGroup { get; set; }

    public DateTime? Updated_On { get; set; }

    public string? engineDescription { get; set; }

    public DateTime? EngineRevisionDate { get; set; }

    public string? AssemblyReportingType { get; set; }

    public double? ReportDisplayOrder { get; set; }

    public double? Reporting_Parent { get; set; }

    public double? Part_relation_dbkey { get; set; }

    public string? Part_Remarks { get; set; }

    public string? Collaborators { get; set; }

    public string? ManufacturingComments { get; set; }

    public double? Per_VendorStatus { get; set; }

    public double? AssemblyDisplayOrder { get; set; }

    public string? ReportGroupIdentifier { get; set; }

    public string? MfgStatus { get; set; }
}
