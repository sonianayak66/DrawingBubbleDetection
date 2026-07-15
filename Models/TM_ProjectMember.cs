using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TM_ProjectMember
{
    public int ProjectMemberId { get; set; }

    public Guid? ProjectMemberGuid { get; set; }

    public int? ProjectId { get; set; }

    public string? UserGuid { get; set; }

    public string? RoleName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }
}
