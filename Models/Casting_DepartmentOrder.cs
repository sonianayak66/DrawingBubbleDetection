using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Casting_DepartmentOrder
{
    public int Id { get; set; }

    public int? DepartmentID { get; set; }

    public double? DisplayOrder { get; set; }
}
