using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ION_Template
{
    public int TemplateId { get; set; }

    public string TemplateGUID { get; set; } = null!;

    public string TemplateName { get; set; } = null!;

    public string? Description { get; set; }

    public string? GroupGUID { get; set; }

    public string? SubjectTemplate { get; set; }

    public string? IONBodyTemplate { get; set; }

    public string? ToAddressTemplate { get; set; }

    public string? CopyToTemplate { get; set; }

    public string? CommRefTemplate { get; set; }

    public string? EnclosuresTemplate { get; set; }

    public bool? IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public int UsageCount { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
