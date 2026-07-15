using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class MetaMaster
{
    public int Id { get; set; }

    public string? MasterGUID { get; set; }

    public string? ParentGUID { get; set; }

    public string? MasterType { get; set; }

    public string? DisplayText { get; set; }

    public double? DisplayOrder { get; set; }

    public bool? UseValue { get; set; }

    public bool? IsActive { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
