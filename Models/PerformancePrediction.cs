using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class PerformancePrediction
{
    public int predictionKey { get; set; }

    public string? predectionGUID { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? inputFilename { get; set; }

    public string? InputDataJson { get; set; }

    public string? OutputDataJson { get; set; }

    public string? InputFileLoc { get; set; }

    public string? createdBy { get; set; }

    public DateTime? createdon { get; set; }
}
