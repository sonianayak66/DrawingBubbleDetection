using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Mailer_Log
{
    public int Id { get; set; }

    public string? MailType { get; set; }

    public string? MailFrom { get; set; }

    public string? MailTo { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public int? TriggerStatus { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? CreatedBy { get; set; }
}
