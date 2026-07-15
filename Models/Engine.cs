using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Engine
{
    public int Engine_Dbkey { get; set; }

    public int Project_Dbkey { get; set; }

    public int BL_Engine_Dbkey { get; set; }

    public string? Engine_Name_varient { get; set; }

    public string? Engine_Description { get; set; }

    public string? Engine_Number { get; set; }

    public int? Category_Dbkey { get; set; }

    public int? Sec_Classfic_Dbkey { get; set; }

    public string? Attachment_Name { get; set; }

    public string? Attachment_location { get; set; }

    public string? Unique_Name { get; set; }

    public string? Priority { get; set; }

    public int? is_active { get; set; }

    public int? Updated_by { get; set; }

    public DateTime? Updated_on { get; set; }

    public virtual Base_Line_Engine BL_Engine_DbkeyNavigation { get; set; } = null!;
}
