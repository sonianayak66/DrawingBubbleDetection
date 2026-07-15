using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class SOP_ReportTemplate_Backup
{
    public int Id { get; set; }

    public string? TemplateSectionGuid { get; set; }

    public string? SectionHeader { get; set; }

    public string? Body { get; set; }

    public double? DisplayOrder { get; set; }

    public string? AccessibleUsers { get; set; }

    public bool isActive { get; set; }

    public bool? PageBreakAfter { get; set; }

    public bool? PageBreakBefore { get; set; }

    public string? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }
}
