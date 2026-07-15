using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NonConformanceReport_Item
{
    public int NCRItemKey { get; set; }

    public string? NCRGuid { get; set; }

    public string? Engine { get; set; }

    public string? SerialNumber { get; set; }

    public string? Status { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? CreatedBy { get; set; }

    public string? DrawingDimension { get; set; }

    public string? ActualDimension { get; set; }

    public string? DrgZone { get; set; }

    public string? NCRWorkFlowGuid { get; set; }

    public string? Remarks { get; set; }

    public string? Module_Status { get; set; }

    public string? Module_Remarks { get; set; }

    public DateTime? Module_UpdatedOn { get; set; }

    public int? Module_UpdateBy { get; set; }

    public string? TAS_Status { get; set; }

    public string? TAS_Remarks { get; set; }

    public DateTime? TAS_UpdatedOn { get; set; }

    public int? TAS_UpdatedBy { get; set; }

    public string? Stress_Status { get; set; }

    public string? Stress_Remarks { get; set; }

    public DateTime? Stress_UpdatedOn { get; set; }

    public int? Stress_UpdatedBy { get; set; }

    public string? Chair_Status { get; set; }

    public string? Chair_Remarks { get; set; }

    public DateTime? Chair_UpdatedOn { get; set; }

    public int? Chair_UpdatedBy { get; set; }

    public int? Rework_Status { get; set; }

    public int? Rework_MarkedBy { get; set; }

    public int? Rework_MarkedModule { get; set; }

    public string? Module_Rework_Remarks { get; set; }

    public int? Module_Rework_UpdatedBy { get; set; }

    public DateTime? Module_Rework_UpdatedOn { get; set; }

    public string? TAS_Rework_Remarks { get; set; }

    public int? TAS_Rework_UpdatedBy { get; set; }

    public DateTime? TAS_Rework_UpdatedOn { get; set; }

    public string? STRESS_Rework_Remarks { get; set; }

    public int? STRESS_Rework_UpdatedBy { get; set; }

    public DateTime? STRESS_Rework_UpdatedOn { get; set; }

    public string? CHAIR_Rework_Remarks { get; set; }

    public int? CHAIR_Rework_UpdatedBy { get; set; }

    public DateTime? CHAIR_Rework_UpdatedOn { get; set; }

    public string? Rework_Dimension { get; set; }

    public string? STFE_PO_Remark { get; set; }

    public int? STFE_PO_Remark_UpdatedBy { get; set; }

    public DateTime? STFE_PO_Remark_UpdatedOn { get; set; }

    public string? Module_Rework_Status { get; set; }

    public string? TAS_Rework_Status { get; set; }

    public string? STRESS_Rework_Status { get; set; }

    public string? CHAIR_Rework_Status { get; set; }

    public string? Serial_No_in_Inspection_Rep { get; set; }

    public string? Deviation_Reason_Analysis { get; set; }

    public string? SlNo { get; set; }
}
