using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class PartManufactureStatus
{
    public int Id { get; set; }

    public int? Engine_Part_Dbkey { get; set; }

    public long? Part_relation_dbkey { get; set; }

    public string? Revision { get; set; }

    public int? QtyOrdered { get; set; }

    public int? QtyReceived { get; set; }

    public int? VendorId { get; set; }

    public int? ManufacturingStatus { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
