using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TM_TaskStatus
{
    public int StatusId { get; set; }

    public Guid? StatusGuid { get; set; }

    public string? StatusName { get; set; }

    public string? ColorCode { get; set; }

    public int? DisplayOrder { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDefault { get; set; }

    public bool? IsDeleted { get; set; }

    public string? IconName { get; set; }
}
