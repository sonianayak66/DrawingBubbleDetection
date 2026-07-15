using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class ExceptionLog
{
    public int id { get; set; }

    public int? ErrorLine { get; set; }

    public string? ErrorMessage { get; set; }

    public int? ErrorNumber { get; set; }

    public string? ErrorProcedure { get; set; }

    public int? ErrorSeverity { get; set; }

    public int? ErrorState { get; set; }

    public DateTime? DateErrorRaised { get; set; }
}
