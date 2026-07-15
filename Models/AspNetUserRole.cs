using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class AspNetUserRole
{
    public string RoleMappingID { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string RoleId { get; set; } = null!;

    public virtual AspNetRole Role { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
