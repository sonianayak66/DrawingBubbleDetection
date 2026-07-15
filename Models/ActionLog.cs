using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ActionLog
{
    public long Id { get; set; }

    public string? ActionName { get; set; }

    public string? Method { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? ActionData { get; set; }

    public string? Comments { get; set; }

    public int? PkTable { get; set; }

    public int? DMLEvent { get; set; }

    public bool? IsActive { get; set; }

    public bool? Isdeleted { get; set; }

    public string? MetaCode { get; set; }
}
