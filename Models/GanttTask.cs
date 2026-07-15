using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class GanttTask
{
    public int pID { get; set; }

    public string? pName { get; set; }

    public DateTime? pStart { get; set; }

    public DateTime? pEnd { get; set; }

    public DateTime? pPlanStart { get; set; }

    public DateTime? pPlanEnd { get; set; }

    public string? pClass { get; set; }

    public string? pLink { get; set; }

    public int? pMile { get; set; }

    public string? pRes { get; set; }

    public int? pComp { get; set; }

    public int? pGroup { get; set; }

    public int pParent { get; set; }

    public int? pOpen { get; set; }

    public string? pDepend { get; set; }

    public string? pCaption { get; set; }

    public string? pNotes { get; set; }

    public string? category { get; set; }

    public string? sector { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public bool? Isactive { get; set; }
}
