using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class NLPDomainModelDef
{
    public int Id { get; set; }

    public string? DomainModel { get; set; }

    public string? ModelDescription { get; set; }

    public string? SampleQuries { get; set; }
}
