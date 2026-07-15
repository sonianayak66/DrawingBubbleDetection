using System;
using System.Collections.Generic;

namespace MPCRS.ViewModels;

public  class NLtoSql_LogVM
{
    public int Id { get; set; }

    public string? Question { get; set; }

    public string? SqlQuery { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? ExecutionTime { get; set; }

    public int? RequestedBy { get; set; }

    public string? Prompt { get; set; }

    public string? RequestType { get; set; }

    public int? RetryCount { get; set; }
}

public class DocumentStatusVM
{
    public int TotalDoc { get; set; }
    public int DocVectored { get; set; }
    public int DocNotVectored { get; set; }
}

