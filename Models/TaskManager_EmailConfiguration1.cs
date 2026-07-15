using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TaskManager_EmailConfiguration1
{
    public int ConfigId { get; set; }

    public Guid ConfigGUID { get; set; }

    public string ConfigName { get; set; } = null!;

    public string ImapServer { get; set; } = null!;

    public int ImapPort { get; set; }

    public bool? UseSSL { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string InboxFolder { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? LastSyncDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsDeleted { get; set; }
}
