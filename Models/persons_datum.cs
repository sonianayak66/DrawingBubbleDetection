using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class persons_datum
{
    public int personDbKey { get; set; }

    public string? PersonGUID { get; set; }

    public string? DataJson { get; set; }
}
