using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TaskManager_EmailNotification
{
    public int NotificationId { get; set; }

    public Guid NotificationGUID { get; set; }

    public string NotificationType { get; set; } = null!;

    public Guid EmailGUID { get; set; }

    public Guid? RelatedTaskGUID { get; set; }

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public bool IsActionRequired { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? ReadBy { get; set; }

    public DateTime? ReadDate { get; set; }

    public bool IsDeleted { get; set; }
}
