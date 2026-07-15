using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Project
{
    public int Project_Dbkey { get; set; }

    public string Title { get; set; } = null!;

    public string? Display_title { get; set; }

    public string? Description { get; set; }

    public DateTime DOS { get; set; }

    public DateTime EDO { get; set; }

    public string Project_Number { get; set; } = null!;

    public int? Category_Dbkey { get; set; }

    public int? Sec_Classfic_Dbkey { get; set; }

    public string? Attachment_Name { get; set; }

    public string? Attachment_location { get; set; }

    public double No_of_Engines { get; set; }

    public string Unique_Name { get; set; } = null!;

    public int? is_active { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_on { get; set; }

    public int BL_Engine_Dbkey { get; set; }

    public double? EstimatedCost { get; set; }
}
