using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class IssueComment
{
    public int CommentId { get; set; }

    public int SlNo { get; set; }

    public string Comment { get; set; } = null!;

    public string CommentedBy { get; set; } = null!;

    public string UpdatedBy { get; set; } = null!;

    public DateTime UpdatedDate { get; set; }
}
