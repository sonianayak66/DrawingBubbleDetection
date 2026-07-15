using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Engine_Parts_Revisoin_History
{
    public int Rev_History_Dbkey { get; set; }

    public int Engine_Dbkey { get; set; }

    public int Engine_Part_Dbkey { get; set; }

    public string? Revision { get; set; }

    public DateTime? Revision_Date { get; set; }

    public string? Solid_Model_File { get; set; }

    public string? Solid_Model_File_location { get; set; }

    public string? Drawing_File { get; set; }

    public string? Drawing_File_location { get; set; }

    public string? Revision_Notes { get; set; }

    public string is_latest { get; set; } = null!;

    public int Updated_by { get; set; }

    public DateTime Updated_on { get; set; }
}
