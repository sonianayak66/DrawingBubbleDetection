using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class User_Role
{
    public int User_Role_Dbkey { get; set; }

    public string Role_name { get; set; } = null!;

    public bool is_active { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public string? LandingPage { get; set; }

    public virtual ICollection<Roles_URL_Module_map> Roles_URL_Module_maps { get; set; } = new List<Roles_URL_Module_map>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
