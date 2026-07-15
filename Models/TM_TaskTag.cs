using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TM_TaskTag
{
    public int TagId { get; set; }

    public Guid? TagGuid { get; set; }

    public string? TagName { get; set; }

    public string? ColorCode { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }
}
