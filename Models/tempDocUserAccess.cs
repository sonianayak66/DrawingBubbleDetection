using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class tempDocUserAccess
{
    public int? UserDbkey { get; set; }

    public int? Document_Dbkey { get; set; }

    public bool ReadAccess { get; set; }

    public bool WriteAccess { get; set; }

    public bool DownloadAccess { get; set; }
}
