using System.ComponentModel.DataAnnotations;
namespace MPCRS.ViewModels

{
    public class CreateMilestoneVM
    {
        public int MilestoneID { get; set; }

        public int? DemandDbKey { get; set; }

        [Required(ErrorMessage = "Required")]
        public string? MilestoneName { get; set; }


        [Required(ErrorMessage = "Required")]
        public DateTime? DueDate { get; set; }

        public DateTime? CompletionDate { get; set; }


        public string? Comments { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
        [Required(ErrorMessage = "Required")]
        public double? QtyPercentage { get; set; }

        public bool? IsLastMilestone { get; set; }

        public int? MilestoneNo { get; set; }
    }

    public class ExtendMileStoneVM
    {
        public string? MilestoneID { get; set; }
        public string? ExtendedDate { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
