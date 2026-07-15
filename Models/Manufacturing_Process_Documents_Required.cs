using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Manufacturing_Process_Documents_Required
{
    public int Id { get; set; }

    public int? Part_DbKey { get; set; }

    public int? AttachmentTypeKey { get; set; }

    public string? Attachment_Type { get; set; }

    public bool? Required { get; set; }
}
