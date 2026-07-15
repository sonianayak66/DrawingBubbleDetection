using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
	public class DemandVerificationVM
	{

		public int Verification_Id { get; set; }

		[Required(ErrorMessage ="Required")]
		public string? Demand_No { get; set; }
		[Required(ErrorMessage ="Required")]
		public string? Demand_Desc { get; set; }
		[Required(ErrorMessage = "Required")]
		public string? Demanding_Officer { get; set; }
		[Required(ErrorMessage = "Required")]
		public string? Project { get; set; }

		public bool? Items { get; set; }

		public bool? Receipt { get; set; }

		public bool? Receipt_Docs { get; set; }

		public string? Remarks { get; set; }

		public bool? Verified { get; set; }

		public string? Verified_By { get; set; }
	}
}
