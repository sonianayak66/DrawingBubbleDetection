using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class TestDataRepository_backup
{
    public int TestdataDbKey { get; set; }

    public string? CellNo { get; set; }

    public string? EngineName { get; set; }

    public string? BuildNo { get; set; }

    public string? RunNo { get; set; }

    public string? NH { get; set; }

    public string? NL { get; set; }

    public string? AtmosphericPressure { get; set; }

    public string? RoomTemperature { get; set; }

    public string? DecuSWBuildNumber { get; set; }

    public string? UploadedFile { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? UpdateBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? CreatedBy { get; set; }
}
