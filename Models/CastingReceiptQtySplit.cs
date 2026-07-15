using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class CastingReceiptQtySplit
{
    public int QtySplitKey { get; set; }

    public int? ReceiptsItemSplitKey { get; set; }

    public int? SplitQty { get; set; }

    public string? StatusRemarks { get; set; }

    public string? Remarks { get; set; }

    public string? SerialNos { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
