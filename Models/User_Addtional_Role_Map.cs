using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class User_Addtional_Role_Map
{
    public int User_Adt_role_Dbkey { get; set; }

    public int UserDbkey { get; set; }

    public int Master_Dbkey { get; set; }

    public bool Is_Active { get; set; }

    public int? Updated_by { get; set; }

    public DateTime? Updated_On { get; set; }

    public virtual Master_General Master_DbkeyNavigation { get; set; } = null!;

    public virtual User UserDbkeyNavigation { get; set; } = null!;
}
