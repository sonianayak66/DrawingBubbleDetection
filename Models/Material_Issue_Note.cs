using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Material_Issue_Note
{
    public int Issue_Dbkey { get; set; }

    public string? Ref_Number { get; set; }

    public string? Form_Number { get; set; }

    public string? Engine_Name { get; set; }

    public string? Demand_No { get; set; }

    public int? DemandDbKey { get; set; }

    public string? Order_Ref_No { get; set; }

    public DateTime? Order_Ref_Date { get; set; }

    public string? PMO_Ref_No { get; set; }

    public DateTime? PMO_Ref_Date { get; set; }

    public int? Demanding_Officer { get; set; }

    public int? Tech_Officer { get; set; }

    public int? Project_Director { get; set; }

    public int? Vendor { get; set; }

    public int? MR_No { get; set; }

    public int? Book_Serial_No { get; set; }

    public int? Volume_No { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public double? Total_Qty { get; set; }

    public double? Total_Cost { get; set; }

    public string? Returnable { get; set; }

    public int? Issue_Purpose { get; set; }

    public string? Job_Card { get; set; }

    public string? JobCardFileLocation { get; set; }

    public string? JobCardFileName { get; set; }

    public bool? IsActive { get; set; }

    public string? Attachment_Db_Key { get; set; }

    public virtual ICollection<Material_Issue_Item> Material_Issue_Items { get; set; } = new List<Material_Issue_Item>();
}
