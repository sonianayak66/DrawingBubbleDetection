using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class CastingReceipt
{
    public int CastingReceiptDbkey { get; set; }

    public string? ReceiptGuid { get; set; }

    public int? CastingDbkey { get; set; }

    public int? CastingItemKey { get; set; }

    public string? ReceiptNumber { get; set; }

    public DateTime? ReceiptDate { get; set; }

    public double? Qty { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? Isdeleted { get; set; }
}
