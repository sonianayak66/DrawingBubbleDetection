using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ION_OfficeConfig
{
    public int ConfigId { get; set; }

    public string Office { get; set; } = null!;

    public string RefNoPrefix { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }
}
