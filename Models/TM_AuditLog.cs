using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TM_AuditLog
{
    public int AuditId { get; set; }

    public string? TableName { get; set; }

    public int? RecordId { get; set; }

    public Guid? RecordGuid { get; set; }

    public string? Action { get; set; }

    public string? ChangedBy { get; set; }

    public DateTime? ChangedDate { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }
}
