using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Material_Issue_Item
{
    public int Issue_Item_Dbkey { get; set; }

    public int Issue_Dbkey { get; set; }

    public int Raw_material_Dbkey { get; set; }

    public string? Drawing_no { get; set; }

    public string? Description { get; set; }

    public int? Engine_Part_Dbkey { get; set; }

    public double Qty { get; set; }

    public string? Size { get; set; }

    public string? Denom { get; set; }

    public double Qty_Issue { get; set; }

    public string? Heat_No { get; set; }

    public double? Weight_Kg { get; set; }

    public double Amount { get; set; }

    public DateTime? Updated_On { get; set; }

    public int? Updated_By { get; set; }

    public string? outer_dia { get; set; }

    public string? thickness { get; set; }

    public string? JobCardNumber { get; set; }

    public string? JCFileName { get; set; }

    public string? JCFileLocation { get; set; }

    public string? height { get; set; }

    public bool? IsActive { get; set; }

    public string? SerialNo { get; set; }

    public int? Vendor_Dbkey { get; set; }

    public double? PartQty_EngineWise { get; set; }

    public string? EngineLevel { get; set; }

    public virtual Material_Issue_Note Issue_DbkeyNavigation { get; set; } = null!;

    public virtual Master_Rawmaterial Raw_material_DbkeyNavigation { get; set; } = null!;
}
