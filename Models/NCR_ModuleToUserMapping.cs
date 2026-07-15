using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NCR_ModuleToUserMapping
{
    public int Id { get; set; }

    public int? Module_ID { get; set; }

    public string? UserGuid { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? Isactive { get; set; }
}
