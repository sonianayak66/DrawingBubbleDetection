using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ION_FileGroup
{
    public int GroupId { get; set; }

    public string GroupGUID { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public int FileNo { get; set; }

    public string ReferenceNo { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
