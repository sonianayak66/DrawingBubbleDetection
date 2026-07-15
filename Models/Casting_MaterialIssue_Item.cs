using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Casting_MaterialIssue_Item
{
    public int IssueItemKey { get; set; }

    public int? IssueDbKey { get; set; }

    public int? QtySplitKey { get; set; }

    public int? IssueQty { get; set; }

    public string? IssueSlNos { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? ForEngine { get; set; }

    public string? JobCardNumber { get; set; }

    public string? JCFileName { get; set; }

    public string? JCFileLocation { get; set; }
}
