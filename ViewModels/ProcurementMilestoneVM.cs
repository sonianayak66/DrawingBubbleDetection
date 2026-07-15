using MPCRS.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using MPCRS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MPCRS.ViewModels
{
    public class ProcurementMilestoneVM
    {
        public int MilestoneID { get; set; }

        public int? DemandDbKey { get; set; }
       
        [Required(ErrorMessage = "Required")]
        [DisplayName(" MilestoneName")]
        public string? MilestoneName { get; set; }
        
        [Required(ErrorMessage = "Required")]
        [DisplayName(" Components")]
        public string? Components { get; set; }
        
        [Required(ErrorMessage = "Required")]
        [DisplayName(" Description")]
        public string? Description { get; set; }
       
        [Required(ErrorMessage = "Required")]
        [DisplayName("Due Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DueDate { get; set; }
        
        [Required(ErrorMessage = "Required")]
        [DisplayName("Completion Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? CompletionDate { get; set; }

        public string? Status { get; set; }

        public string? Comments { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
        public double? QtyPercentage { get; set; }

        public bool? IsLastMilestone { get; set; }

        public List<ProcurementMilestoneVM> ProcurementMilestoneVMs { get; set; }
    }
}
