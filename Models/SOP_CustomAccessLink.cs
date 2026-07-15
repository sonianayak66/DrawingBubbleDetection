using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class SOP_CustomAccessLink
{
    public int LinkdbKey { get; set; }

    public string? LinkGuid { get; set; }

    public string? LinkDataJson { get; set; }

    public string? Updated_By { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? BuildGuid { get; set; }
}
