using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Mail_Credential
{
    public int Sl_No { get; set; }

    public string? MailID { get; set; }

    public string? Password { get; set; }

    public string? SMTP_HostName { get; set; }

    public int? SMTP_Port { get; set; }

    public string? SSL { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }
}
