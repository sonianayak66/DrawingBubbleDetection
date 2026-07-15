using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class BATL_Build_assignment_Json
{
    public int Id { get; set; }

    public string? Build_assignment_Json { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? File_No { get; set; }
}
