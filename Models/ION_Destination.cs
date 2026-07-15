using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ION_Destination
{
    public int DestinationId { get; set; }

    public string DestinationGUID { get; set; } = null!;

    public string DestinationName { get; set; } = null!;

    public string DestinationCode { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsDeleted { get; set; }
}
