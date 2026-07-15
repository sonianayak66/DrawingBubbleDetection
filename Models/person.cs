using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Person
{
    public int Person_Dbkey { get; set; }

    public string Person_Name { get; set; } = null!;

    public int? Designation { get; set; }

    public int? Section { get; set; }

    public DateTime? Updated_on { get; set; }

    public int? Updated_by { get; set; }
}
