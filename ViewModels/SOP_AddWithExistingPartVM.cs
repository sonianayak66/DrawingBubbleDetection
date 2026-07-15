using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
    public class SOP_AddWithExistingPartVM
    {

        // Existing Part Selection
        [Required(ErrorMessage = "Please select an existing part")]
        public int? SelectedEnginePartDbkey { get; set; }

        // Build Context (will be set from parent)
        public int? BuildId { get; set; }
        public int? BL_Engine_Dbkey { get; set; }
        public int? Parent_id { get; set; }

        // From Selected Part (will be auto-populated, read-only in UI)
        public int? Type_Dbkey { get; set; }
        public string? DrawingNumber { get; set; }
        public string? Description { get; set; }
        public string? Revision { get; set; }
        public int? RawMaterial { get; set; }
        public int? Module_Responsibility { get; set; }

        // User can modify these
        [Required(ErrorMessage = "Quantity is required")]
        public int? QtyPerEngine { get; set; }
        public string? JobCard { get; set; }
        public string? ContractNumber { get; set; }
        public string? SerialNumber { get; set; }
        public string? Remarks { get; set; }

        // For dropdown population
        public List<SelectListItem>? AvailableParts { get; set; }
        public List<SelectListItem>? PartTypesList { get; set; }
        public List<SelectListItem>? RawMaterialList { get; set; }
        public List<SelectListItem>? ModuleResponsibilityList { get; set; }
    }
}
