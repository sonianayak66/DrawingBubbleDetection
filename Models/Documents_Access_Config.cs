using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Documents_Access_Config
{
    public int doc_config_dbkey { get; set; }

    public int? UserDbkey { get; set; }

    public int? Document_Dbkey { get; set; }

    public bool? ReadAccess { get; set; }

    public bool? WriteAccess { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public bool? DownloadAccess { get; set; }
}
