using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TaskManager_ApplicationSetting
{
    public int SettingId { get; set; }

    public string Category { get; set; } = null!;

    public string SettingKey { get; set; } = null!;

    public string SettingValue { get; set; } = null!;

    public string? Description { get; set; }

    public string DataType { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime UpdatedDate { get; set; }
}
