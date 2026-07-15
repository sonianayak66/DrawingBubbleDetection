using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Master_Rawmaterial
{
    public int Raw_material_Dbkey { get; set; }

    public string Material_name { get; set; } = null!;

    public string? Dia_mm { get; set; }

    public string? Thick_mm { get; set; }

    public string? Remarks { get; set; }

    public bool? is_active { get; set; }

    public int Updated_by { get; set; }

    public DateTime Updated_on { get; set; }

    public int? RM_Type { get; set; }

    public int? RM_UOM { get; set; }

    public int? Density { get; set; }

    public string? inner_Dia_mm { get; set; }

    public string? height { get; set; }

    public string? RawmaterialGuid { get; set; }

    public string? Raw_material_Name { get; set; }

    public double? RMQtyPerEngine { get; set; }

    public double? MinInventoryThreshold { get; set; }

    public virtual ICollection<Material_Issue_Item> Material_Issue_Items { get; set; } = new List<Material_Issue_Item>();

    public virtual ICollection<Rawmaterial_Inventory> Rawmaterial_Inventories { get; set; } = new List<Rawmaterial_Inventory>();
}
