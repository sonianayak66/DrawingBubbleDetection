using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TaskManager_TaskComment
{
    public int CommentId { get; set; }

    public Guid CommentGUID { get; set; }

    public Guid TaskGUID { get; set; }

    public Guid? ParentCommentGUID { get; set; }

    public string CommentText { get; set; } = null!;

    public string CommentType { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsDeleted { get; set; }
}
