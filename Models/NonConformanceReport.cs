using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NonConformanceReport
{
    public int Id { get; set; }

    public string? NCRGuid { get; set; }

    public string? ReferenceNumber { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public int? ReceivedFrom { get; set; }

    public string? ComitteeReferred { get; set; }

    public string? ReportStatus { get; set; }

    public string? Remarks { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? FileLocation { get; set; }

    public string? OrignalFileName { get; set; }

    public string? SystemFileName { get; set; }

    public int? Engine_Part_Dbkey { get; set; }

    public int? Part_relation_dbkey { get; set; }

    public int? Vendor { get; set; }

    public string? SerialNumber { get; set; }

    public string? Revision { get; set; }

    public int? Qty { get; set; }

    public string? Module { get; set; }

    public string? Stress { get; set; }

    public string? Chair { get; set; }

    public string? Tas { get; set; }

    public int? Module_Responsibilty { get; set; }

    public string? DARno { get; set; }

    public string? JobCard { get; set; }

    public string? AssignedUserGuid { get; set; }

    public int? ModuleAssignedBy { get; set; }

    public DateTime? ModuleAssignedOn { get; set; }

    public int? CloseNCR { get; set; }

    public int? RawMaterial { get; set; }

    public string? Stage_Final { get; set; }

    public string? Inspection_Report_No { get; set; }

    public bool? UseNewWorkflow { get; set; }

    public string? ECM_TR_NO { get; set; }

    public string? ECM_No { get; set; }
}
