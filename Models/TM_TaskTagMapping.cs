using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TM_TaskTagMapping
{
    public int MappingId { get; set; }

    public Guid? MappingGuid { get; set; }

    public int? TaskId { get; set; }

    public int? TagId { get; set; }

    public bool? IsDeleted { get; set; }
}
