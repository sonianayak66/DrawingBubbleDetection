using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class WatermarkConfiguration
{
    public int WatermarkConfigurationID { get; set; }

    public string? ConfigurationFor { get; set; }

    public double? FontSize { get; set; }

    public double? FontOpacity { get; set; }

    public double? Rotation { get; set; }

    public long? Compression { get; set; }
}
