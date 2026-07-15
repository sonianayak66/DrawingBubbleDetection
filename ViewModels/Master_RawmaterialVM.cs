using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace MPCRS.ViewModels
{
    public class Master_RawmaterialVM
    {
        public int Raw_material_Dbkey { get; set; }
        public string RawmaterialGuid { get; set; }

        [Required]
        [DisplayName("Material Name")]
        public string Material_name { get; set; } = null!;

        [DisplayName("Outer Diameter (MM)")]
        public string? Dia_mm { get; set; }

        [DisplayName("Thickness (MM)")]
        public string? Thick_mm { get; set; }
        [Required]
        public string? Remarks { get; set; }
        [DisplayName("Is Active")]
        public bool is_active { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_on { get; set; }
        [Required]
        [DisplayName("Raw Material Type")]
        [Range(1, int.MaxValue, ErrorMessage = "Please Select Raw Material Type")]
        public string? RM_Type { get; set; }
        [Required]
        [DisplayName("Unit Of Measurement")]
        [Range(1, int.MaxValue, ErrorMessage = "Please Select Unit Of Measurement")]
        public string? RM_UOM { get; set; }

        [DisplayName("Density")]
        public int? Density { get; set; }
        [DisplayName("Inner Diameter (MM)")]
        public string? inner_Dia_mm { get; set; }
        [DisplayName("Height")]
        public string? height { get; set; }
        public string? Raw_material_Name { get; set; }

        [DisplayName("RM Qty Per Engine")]
        public double? RMQtyPerEngine { get; set; }

        [DisplayName("Min Inventory Threshold")]
        public double? MinInventoryThreshold { get; set; }

        public bool IsUsedInDemand { get; set; }

        
    }

    
}
