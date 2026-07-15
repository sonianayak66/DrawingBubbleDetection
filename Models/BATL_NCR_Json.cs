using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class BATL_NCR_Json
{
    public int Id { get; set; }

    public string? NCR_Json { get; set; }

    public int? File_No { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
