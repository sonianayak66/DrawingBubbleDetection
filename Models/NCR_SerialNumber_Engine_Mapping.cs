using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NCR_SerialNumber_Engine_Mapping
{
    public int Id { get; set; }

    public string NCR_GUId { get; set; } = null!;

    public string SerialNumber { get; set; } = null!;

    public string Engine { get; set; } = null!;

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
