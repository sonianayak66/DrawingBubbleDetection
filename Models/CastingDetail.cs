using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class CastingDetail
{
    public int CastingDbkey { get; set; }

    public string? castingGUID { get; set; }

    public string? DemandNumber { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? Remarks { get; set; }

    public string? MMGOrderNumber { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public bool? Isdeleted { get; set; }

    public string? OrderStatus { get; set; }

    public string? OrderNumbers { get; set; }

    public int? DemandingOfficer { get; set; }

    public string? DemandDesc { get; set; }

    public string? OrderType { get; set; }
}
