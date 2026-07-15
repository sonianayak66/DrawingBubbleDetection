using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class RecordsForDeletion
{
    public int DeletionKey { get; set; }

    public string? SourceTableName { get; set; }

    public int? SourceTableKey { get; set; }

    public string? SourceDisplayName { get; set; }

    public string? ReasonForDeletion { get; set; }

    public string? InitiatedBy { get; set; }

    public DateTime? InitiatedOn { get; set; }

    public string? ApprovalStatus { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }
}
