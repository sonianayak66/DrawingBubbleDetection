using Microsoft.VisualBasic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
    public class EngineBuildsVM
    {
        public int Id { get; set; }

        public string? BuildGuid { get; set; }

        public int? BaseLineEngineDbkey { get; set; }
        [Required]
        [DisplayName("Build Name")]
        public string? BuildName { get; set; }
        [Required]
        [DisplayName("Build Date")]
        public DateTime? BuildDate { get; set; }
        
        public string? ReferenceNumber { get; set; }
        [Required]
        [DisplayName("Description")]
        public string? Description { get; set; }

        [DisplayName("Status")]
        public string? Status { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
        public string? BaseLineEngineName { get; set; }

        public int? ClonedFromKey { get; set; }

        public string? ClonedFrom { get; set; }


    }
}
