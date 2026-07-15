using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TaskManager_Email
{
    public int EmailId { get; set; }

    public Guid EmailGUID { get; set; }

    public string MessageId { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string FromEmail { get; set; } = null!;

    public string? FromName { get; set; }

    public string? ToEmails { get; set; }

    public string? CcEmails { get; set; }

    public string? EmailBodyText { get; set; }

    public string? EmailBodyHtml { get; set; }

    public DateTime ReceivedDate { get; set; }

    public bool HasAttachments { get; set; }

    public bool IsConverted { get; set; }

    public Guid? ConvertedTaskGUID { get; set; }

    public int? ConvertedBy { get; set; }

    public DateTime? ConvertedDate { get; set; }

    public DateTime EmailProcessedDate { get; set; }

    public bool IsDeleted { get; set; }

    public string? InReplyTo { get; set; }

    public string? References { get; set; }

    public string? ThreadId { get; set; }

    public string? CleanSubject { get; set; }
}
