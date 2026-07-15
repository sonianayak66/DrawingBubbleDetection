using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ExternalMfgStatus
{
    public int Id { get; set; }

    public string? Json { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? UploadedBy { get; set; }
}
