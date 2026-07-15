using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class MaterialIssueDocument
{
    public int MaterialIssueDocumentKey { get; set; }

    public int? IssueDbKey { get; set; }

    public int? IssueItemDbKey { get; set; }

    public string? FileLocation { get; set; }

    public string? FileName { get; set; }

    public int? uploadedBy { get; set; }

    public DateTime? uploadedOn { get; set; }
}
