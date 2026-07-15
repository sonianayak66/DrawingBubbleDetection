using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ION_InwardNote
{
    public int InwardNoteId { get; set; }

    public string InwardIONGUID { get; set; } = null!;

    public DateTime ReceivedDate { get; set; }

    public DateTime? IONDate { get; set; }

    public string? IONReferenceNumber { get; set; }

    public string FromDepartment { get; set; } = null!;

    public string? FromPersonNameWithDesignation { get; set; }

    public string Subject { get; set; } = null!;

    public string AddressedTo { get; set; } = null!;

    public string? CopyTo { get; set; }

    public string? Remarks { get; set; }

    public bool AcknowledgmentSent { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
