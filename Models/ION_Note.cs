using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ION_Note
{
    public int IONId { get; set; }

    public string IONGUID { get; set; } = null!;

    public string IONNumber { get; set; } = null!;

    public string? Office { get; set; }

    public DateTime IONDate { get; set; }

    public int? DestinationId { get; set; }

    public string Subject { get; set; } = null!;

    public string? CommunicationReference { get; set; }

    public string IONBody { get; set; } = null!;

    public string ToAddress { get; set; } = null!;

    public int PreparedBy { get; set; }

    public string? PreparedByDesignation { get; set; }

    public int? SentThrough { get; set; }

    public string? Status { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public string? RejectionReason { get; set; }

    public bool? ScannedCopyUploaded { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsDeleted { get; set; }

    public string? ScannedCopyPath { get; set; }

    public int? ScannedCopyUploadedBy { get; set; }

    public DateTime? ScannedCopyUploadedDate { get; set; }

    public string? GroupGUID { get; set; }

    public string? CopyTo { get; set; }
}
