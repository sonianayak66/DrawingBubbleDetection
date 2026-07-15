using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Engine_Parts_Master
{
    public int Engine_Part_Dbkey { get; set; }

    public int Engine_Dbkey { get; set; }

    public int Type_Dbkey { get; set; }

    public string Draw_part_no { get; set; } = null!;

    public string? Revision { get; set; }

    public string? Drawing_no { get; set; }

    public string? Drawing_File { get; set; }

    public string? Drawing_File_location { get; set; }

    public string? Solid_model_no { get; set; }

    public string? Solid_Model { get; set; }

    public string? Solid_Model_location { get; set; }

    public int? is_active { get; set; }

    public double? Quantity { get; set; }

    public double? Manufacturing_Duration { get; set; }

    public string? Description { get; set; }

    public string? Comments { get; set; }

    public int? Raw_Material { get; set; }

    public int? Module_Responsibility { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public int? Parent_id { get; set; }

    public long? Ref_SL_NO { get; set; }

    public long? Ref_SL_NO_parent { get; set; }

    public bool? is_vendor_material { get; set; }

    public double? weight_in_kg { get; set; }

    public string? FCBP { get; set; }

    public double? Bar_pipe_Dia_OD { get; set; }

    public double? Bar_pipe_Length { get; set; }

    public double? Bar_pipe_Thickness { get; set; }

    public double? Plate_Width { get; set; }

    public double? Plate_length { get; set; }

    public double? Plate_Thickness { get; set; }

    public double? Area { get; set; }

    public double? Density { get; set; }

    public int? Approver_ID { get; set; }

    public string? Record_Status { get; set; }

    public int? Drawing_File_ID { get; set; }

    public int? Drawing_File_2Dmodel_ID { get; set; }

    public int? Drawing_File_3Dmodel_ID { get; set; }

    public int? Drawing_File_ACSN_ID { get; set; }

    public bool? is_rm_verified { get; set; }

    public int? rm_verified_by { get; set; }

    public DateTime? rm_updated_on { get; set; }

    public string? Reporting_Type { get; set; }

    public string? Execution_Resp { get; set; }

    public string? AssemblyReportingType { get; set; }

    public double? ReportDisplayOrder { get; set; }

    public int? Reporting_Parent { get; set; }

    public string? Execution_Resp_additionalLevel { get; set; }

    public double? AssemblyDisplayOrder { get; set; }

    public int? Drawing_File_Casting_Forging_ID { get; set; }

    public virtual ICollection<Engine_Parts_Usage> Engine_Parts_Usages { get; set; } = new List<Engine_Parts_Usage>();
}
