using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class UserSession
{
    public Guid SessionGuid { get; set; }

    public string? UserId { get; set; }

    public string? ClientIP { get; set; }

    public DateTime? SessionStart { get; set; }

    public DateTime? SessionEnd { get; set; }

    public string? BrowserInfo { get; set; }
}
