using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ACSN_Config
{
    public int AcsnConfigKey { get; set; }

    public string? Series { get; set; }

    public int? InititialIndex { get; set; }

    public string? Prefix { get; set; }
}
