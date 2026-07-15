using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class SOP_ReportTemplate_Document
{
    public int Id { get; set; }

    public string? BuildGuid { get; set; }

    public string? UserGuid { get; set; }

    public string? FileName { get; set; }

    public string? FileLocation { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
