using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class EngineBuild
{
    public int Id { get; set; }

    public string? BuildGuid { get; set; }

    public int? BaseLineEngineDbkey { get; set; }

    public string? BuildName { get; set; }

    public DateTime? BuildDate { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Description { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? ClonedFromKey { get; set; }

    public string? ClonedFrom { get; set; }

    public string? Status { get; set; }
}
