using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class User_session_manager
{
    public int Session_Dbkey { get; set; }

    public string Session_ID { get; set; } = null!;

    public int UserDbkey { get; set; }

    public DateTime? Session_Start { get; set; }

    public DateTime? Session_End { get; set; }

    public string? Browser_Type { get; set; }

    public string? Device_Type { get; set; }

    public string? IP_address { get; set; }
}
