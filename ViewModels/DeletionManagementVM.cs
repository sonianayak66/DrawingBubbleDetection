using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
    public class DeletionManagementVM
    {
        public int DeletionKey { get; set; }

        [Required]
        public string? SourceTableName { get; set; }

        [Required]
        public int? SourceTableKey { get; set; }

        [Required]
        public string? SourceDisplayName { get; set; }

        [Required]
        public string? ReasonForDeletion { get; set; }

        public string? InitiatedBy { get; set; }

        public DateTime? InitiatedOn { get; set; }

        public string? ApprovalStatus { get; set; }

        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedOn { get; set; }

        public string? InitiatedUserName { get; set; }
        public string? ApprovedByUserName { get; set; }
        public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }

    }


}
