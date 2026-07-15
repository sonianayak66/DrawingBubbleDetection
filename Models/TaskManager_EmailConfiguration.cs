using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TaskManager_EmailConfiguration
{
    public int ConfigId { get; set; }

    public string ConfigKey { get; set; } = null!;

    public string ConfigValue { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }
}
