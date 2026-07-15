using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class RecordDeletionLog
{
    public int Id { get; set; }

    public string? SourceTable { get; set; }

    public int? DeletionKey { get; set; }

    public string? JsonData { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
