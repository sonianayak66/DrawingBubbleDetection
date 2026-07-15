using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Drawing_Bubble_Inspection
{
    public int Inspection_Dbkey { get; set; }

    public int Engine_Part_Dbkey { get; set; }

    public string Draw_part_no { get; set; } = null!;

    public string Revision { get; set; } = null!;

    public string Original_File_Name { get; set; } = null!;

    public string Original_File_Path { get; set; } = null!;

    public string? Annotated_File_Name { get; set; }

    public string? Annotated_File_Path { get; set; }

    public int Total_Bubbles { get; set; }

    public int Needs_Review_Count { get; set; }

    public string Status { get; set; } = null!;

    public DateTime Processed_On { get; set; }

    public int Processed_By { get; set; }

    public bool? Is_Active { get; set; }

    public string? Detection_Method { get; set; }
}
