using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class CastingReceiptsItemSplit
{
    public int Id { get; set; }

    public string? SerialNumber { get; set; }

    public string? BatchNumber { get; set; }

    public string? HeatNumber { get; set; }

    public string? Attachments { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? Revision { get; set; }

    public string? Status { get; set; }

    public bool? Isdeleted { get; set; }

    public string? ReceiptGuid { get; set; }

    public int? OrderItemKey { get; set; }

    public string? ReceiptNumber { get; set; }

    public DateTime? ReceiptDate { get; set; }

    public string? Remarks { get; set; }

    public string? VendorDrawingNo { get; set; }
}
