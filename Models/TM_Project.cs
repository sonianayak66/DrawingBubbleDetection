using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TM_Project
{
    public int ProjectId { get; set; }

    public Guid? ProjectGuid { get; set; }

    public string? ProjectName { get; set; }

    public string? ProjectDescription { get; set; }

    public string? ProjectCode { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }
}
