using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class CastingReceiptsComment
{
    public int CastingReceiptsCommentsKey { get; set; }

    public int? CastingReceiptsItemSplitKey { get; set; }

    public string? Comments { get; set; }

    public int? DepartmentID { get; set; }

    public string? UserGuid { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? MappingKey { get; set; }
}
