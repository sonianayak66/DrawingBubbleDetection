using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ACSN
{
    public int acsnKey { get; set; }

    public int? SerialNumber { get; set; }

    public string? Series { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public string? ModuleRefNum { get; set; }

    public int? PartDbKey { get; set; }

    public string? DrawingNumber { get; set; }

    public string? description { get; set; }

    public int? Module { get; set; }

    public string? existingRevision { get; set; }

    public string? NewRevision { get; set; }

    public string? Remarks { get; set; }

    public int? updatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? ACSNnum { get; set; }

    public string? ACSN_Status { get; set; }
}
