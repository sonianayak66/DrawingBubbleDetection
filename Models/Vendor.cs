using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Vendor
{
    public int Vendor_Dbkey { get; set; }

    public string? Vendor_ID_System { get; set; }

    public string? Vendor_ID_User { get; set; }

    public string? Vendor_Name { get; set; }

    public string? Vendor_Email { get; set; }

    public string? Vendor_Contact { get; set; }

    public string? Vendor_Adress { get; set; }

    public string? Vendor_State { get; set; }

    public string? Vendor_City { get; set; }

    public string? Vendor_Pincode { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public string? vendor_GUID { get; set; }
}
