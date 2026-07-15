using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ION_SerialTracking
{
    public int TrackingId { get; set; }

    public int? LastSerialNumber { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? GroupGUID { get; set; }
}
