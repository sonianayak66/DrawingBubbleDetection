using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class InspectionReportRecord
{
    public int Inspect_File_DBkey { get; set; }

    public string? File_Name { get; set; }

    public string? File_Location { get; set; }

    public string? File_SystemName { get; set; }

    public string? File_Updatedby { get; set; }

    public DateTime? File_UpdatedOn { get; set; }

    public string? VendorCode { get; set; }
}
