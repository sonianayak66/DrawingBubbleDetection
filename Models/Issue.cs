using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Issue
{
    public int SlNo { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Reporter { get; set; } = null!;

    public string? Developer { get; set; }

    public string IssueType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public string Section { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? Solution_Description { get; set; }

    public string? Solution_Placeholder { get; set; }
}
