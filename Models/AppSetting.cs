using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class AppSetting
{
    public Guid AppSettingKey { get; set; }

    public string? AppSettingType { get; set; }

    public string? DataJson { get; set; }
}
