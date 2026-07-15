using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class CastingReceiptRemark_DepartmentOrder
{
    public int Id { get; set; }

    public int? DepartmentID { get; set; }

    public int? DisplayOrder { get; set; }
}
