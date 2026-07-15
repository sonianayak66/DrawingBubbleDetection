using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Roles_URL_Module_map
{
    public int Module_Map_ID { get; set; }

    public int User_Role_Dbkey { get; set; }

    public int Module_Dbkey { get; set; }

    public bool ReadAccess { get; set; }

    public bool WriteAccess { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public virtual User_Role User_Role_DbkeyNavigation { get; set; } = null!;
}
