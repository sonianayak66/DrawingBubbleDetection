using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Order_ModuleUserMapping
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public string? OrderType { get; set; }

    public string? UserGuid { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? UpdateOn { get; set; }
}
