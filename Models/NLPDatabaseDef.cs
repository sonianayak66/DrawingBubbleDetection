using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NLPDatabaseDef
{
    public int tableKey { get; set; }

    public string? tableName { get; set; }

    public string? tableSchema { get; set; }

    public string? AdditionalNotes { get; set; }

    public string? DomainModel { get; set; }
}
