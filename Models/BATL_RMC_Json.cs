using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class BATL_RMC_Json
{
    public int Id { get; set; }

    public string? RMC_Json { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? File_No { get; set; }
}
