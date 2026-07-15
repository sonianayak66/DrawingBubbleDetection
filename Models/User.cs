using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class User
{
    public int UserDbkey { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public int? Department { get; set; }

    public int? User_Role_Dbkey { get; set; }

    public bool User_Status { get; set; }

    public DateTime Updated_On { get; set; }

    public int? Updated_By { get; set; }

    public string? Vendor { get; set; }

    public bool? Is_Outside_Source { get; set; }

    public string? User_type { get; set; }

    public virtual ICollection<User_Addtional_Role_Map> User_Addtional_Role_Maps { get; set; } = new List<User_Addtional_Role_Map>();

    public virtual User_Role? User_Role_DbkeyNavigation { get; set; }
}
