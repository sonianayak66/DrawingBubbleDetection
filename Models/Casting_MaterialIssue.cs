using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Casting_MaterialIssue
{
    public int IssueDbKey { get; set; }

    public int? VendorKey { get; set; }

    public DateTime? IssueDate { get; set; }

    public string? Reference_No { get; set; }

    public string? Issue_type { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
