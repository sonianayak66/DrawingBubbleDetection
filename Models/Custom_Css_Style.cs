using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Custom_Css_Style
{
    public int Id { get; set; }

    public string? Page { get; set; }

    public string? Style_Name { get; set; }

    public string? Style_data { get; set; }

    public int? Updated_by { get; set; }

    public DateTime? Updated_On { get; set; }

    public bool? is_active_injected { get; set; }
}
