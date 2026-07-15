using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class DataProtectionKey
{
    public int id { get; set; }

    public string? FriendlyName { get; set; }

    public string? Xml { get; set; }
}
