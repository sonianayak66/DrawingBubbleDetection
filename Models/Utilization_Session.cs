using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Utilization_Session
{
    public int Session_ID { get; set; }

    public int? UserDbkey { get; set; }

    public DateTime? Login_datetime { get; set; }

    public DateTime? LogOut_datetime { get; set; }
}
