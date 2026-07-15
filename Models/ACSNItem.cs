using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ACSNItem
{
    public int ACSNStatusKey { get; set; }

    public int? acsnKey { get; set; }

    public string? acsnStatus { get; set; }

    public int? acsnStepId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Documents { get; set; }

    public string? Remarks { get; set; }

    public bool? isActiveStatus { get; set; }

    public int? updatedBy { get; set; }

    public DateTime? updatedOn { get; set; }
}
