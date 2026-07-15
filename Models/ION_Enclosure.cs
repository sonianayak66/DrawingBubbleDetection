using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ION_Enclosure
{
    public int EnclosureId { get; set; }

    public string EnclosureGUID { get; set; } = null!;

    public string IONGUID { get; set; } = null!;

    public string EnclosureDescription { get; set; } = null!;

    public int? SortOrder { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool? IsDeleted { get; set; }
}
