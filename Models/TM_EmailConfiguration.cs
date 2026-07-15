using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TM_EmailConfiguration
{
    public int ConfigId { get; set; }

    public Guid? ConfigGuid { get; set; }

    public string? ConfigName { get; set; }

    public string? EmailProtocol { get; set; }

    public string? ServerHost { get; set; }

    public int? ServerPort { get; set; }

    public bool? UseSSL { get; set; }

    public string? Username { get; set; }

    public string? PasswordEncrypted { get; set; }

    public string? FolderName { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? LastSyncDate { get; set; }

    public bool? IsDeleted { get; set; }
}
