using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
    public class MetaMasterVM
    {
        public int Id { get; set; } = 0;
        public string? MasterGUID { get; set; }
        public string? ParentGUID { get; set; }
        [Required]
        [DisplayName("Master Type")]
        public string? MasterType { get; set; }
        [Required]
        [DisplayName("Display Text")]
        public string? DisplayText { get; set; }
        [DisplayName("Display Order")]
        public double? DisplayOrder { get; set; }
        [DisplayName("Use Value")]
        public bool UseValue { get; set; } = true;
        [DisplayName("Is Active")]
        public bool IsActive { get; set; } = true;

    }
}
