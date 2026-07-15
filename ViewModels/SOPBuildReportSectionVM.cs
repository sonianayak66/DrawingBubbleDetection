using Microsoft.Build.Framework;
using System.ComponentModel;

namespace MPCRS.ViewModels
{
    public class SOPBuildReportSectionVM
    {
        public int Id { get; set; }

        public string SopReportSectionGUID { get; set; } = null!;

        public string BuildGuid { get; set; } = null!;

        public string? ReportTemplateSectionGUID { get; set; }

        [Required]
        [DisplayName("Body")]
        public string? Body { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsReviewed { get; set; }

        public bool IsActive { get; set; }

        public string? Updated_By { get; set; }

        public DateTime? Updated_On { get; set; }

        [Required]
        [DisplayName("Section Header")]

        public string? SectionHeader { get; set; }
        public IFormFile File { get; set; }

    }
    public class BuildReportSectionList
    {
        public string TemplateSectionGuid { get; set; }
        public string SectionHeader { get; set; }
        public float DisplayOrder { get; set; }
        public string UsersList { get; set; }
        public string BuildGuid { get; set; }
        public string? SopReportSectionGUID { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsReviewed { get; set; }
        public bool IsActive { get; set; }
        public string BuildName { get; set; }
    }
}




