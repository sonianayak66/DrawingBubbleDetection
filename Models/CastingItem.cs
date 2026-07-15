using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class CastingItem
{
    public int CastingItemKey { get; set; }

    public int? CastingDbkey { get; set; }

    public int? EnginePartDbkey { get; set; }

    public string? PartName { get; set; }

    public string? ItemDescription { get; set; }

    public double? OrderQty { get; set; }

    public int? Vendor { get; set; }

    public string? OrderNumber { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public bool? Isdeleted { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public int? RawMaterial { get; set; }

    public string? GTREDrgNo { get; set; }

    public int? TestSpecimen { get; set; }
}
