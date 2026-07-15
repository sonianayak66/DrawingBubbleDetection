using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class EngineBuildComponent
{
    public int Id { get; set; }

    public int? BuildDbkey { get; set; }

    public int? BaseLineEngineDbkey { get; set; }

    public int? PartRelationKey { get; set; }

    public int? EnginePartDbkey { get; set; }

    public int? ParentId { get; set; }

    public int? IsActive { get; set; }

    public string? Revision { get; set; }

    public int? QtyPerEngine { get; set; }

    public string? Description { get; set; }

    public string? JobCard { get; set; }

    public string? ContractNumber { get; set; }

    public string? SerialNumber { get; set; }

    public string? Remarks { get; set; }

    public string? DrawingNumber { get; set; }

    public string? SchemeNumber { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsReplaced { get; set; }

    public bool? IsUpdated { get; set; }

    public bool? IsRemoved { get; set; }

    public bool? IsNewlyAdded { get; set; }

    public int? ReportingParent { get; set; }

    public string? ReportingType { get; set; }

    public string? AssemblyReportingType { get; set; }

    public bool? SyncedFromBATL { get; set; }
}
