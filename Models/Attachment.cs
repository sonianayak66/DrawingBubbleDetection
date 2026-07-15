using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Attachment
{
    public int Attachment_Db_Key { get; set; }

    public string? Source_table { get; set; }

    public int? Source_table_key { get; set; }

    public string? Revision { get; set; }

    public string? Attachment_location { get; set; }

    public string? Attachment_FileName { get; set; }

    public string? Attachment_type { get; set; }

    public string? Orginal_File_Name { get; set; }

    public string? File_DVD_Num { get; set; }

    public string? File_Revision { get; set; }

    public int? Updated_by { get; set; }

    public DateTime? Updated_on { get; set; }

    public bool? Approved_status { get; set; }

    public string? AttachmentGUID { get; set; }
}
