using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class SOP_BuildReportSection
{
    public int Id { get; set; }

    public string? SopReportSectionGUID { get; set; }

    public string BuildGuid { get; set; } = null!;

    public string? ReportTemplateSectionGUID { get; set; }

    public string? Body { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsReviewed { get; set; }

    public bool IsActive { get; set; }

    public string? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public string? ExtractedBody { get; set; }

    public int? AttachmentKey { get; set; }
}
