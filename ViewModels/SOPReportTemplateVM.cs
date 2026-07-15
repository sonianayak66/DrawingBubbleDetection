using Microsoft.AspNetCore.Mvc.Rendering;
using MPCRS.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
	public class SOPReportTemplateVM
	{
		public int Id { get; set; }
		public string? TemplateSectionGuid { get; set; }

		[Required]
		[DisplayName("Section Header")]
		public string? SectionHeader { get; set; }
		[Required]
		[DisplayName("Body")]
		public string Body { get; set; }
        [Required]
        [DisplayName("Display Order")]
		public double? DisplayOrder { get; set; }
		public bool isActive { get; set; }

        [Required]
        [DisplayName("Accessible Users")]
		public string[] AccessibleUsers { get; set; }
		[DisplayName("Page Break After")]
		public bool PageBreakAfter { get; set; }
		[DisplayName("Page Break Before")]
		public bool PageBreakBefore { get; set; }
		public string? UsersList { get; set; }

		
        public string? Updated_By { get; set; }

		public DateTime? Updated_On { get; set; }

    }
}
