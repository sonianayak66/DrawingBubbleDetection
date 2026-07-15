using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Master_General
{
    public int Master_Dbkey { get; set; }

    public string? Master_Name { get; set; }

    public string? Master_Type { get; set; }

    public int? is_active { get; set; }

    public int? Updated_by { get; set; }

    public DateTime? Updated_on { get; set; }

    public string? Misc { get; set; }

    public virtual ICollection<User_Addtional_Role_Map> User_Addtional_Role_Maps { get; set; } = new List<User_Addtional_Role_Map>();
}
