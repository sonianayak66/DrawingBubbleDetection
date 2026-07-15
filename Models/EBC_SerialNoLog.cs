using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class EBC_SerialNoLog
{
    public int LogID { get; set; }

    public int? EBC_Id { get; set; }

    public string? Previous_SerialNo { get; set; }

    public string? Updated_serialNo { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? UpdatedBy { get; set; }
}
