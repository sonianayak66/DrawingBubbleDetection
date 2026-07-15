using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TestDataRepoJson
{
    public int TestDataJSonDbKey { get; set; }

    public int? TestDataDbKey { get; set; }

    public string? JsonData { get; set; }
}
