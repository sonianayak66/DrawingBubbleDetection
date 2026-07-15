using MPCRS.Models;
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
	public class Casting_MaterialIssue_VM
	{
		public int IssueDbKey { get; set; }

		[Required]
		public int? VendorKey { get; set; }

		[Required]
		public DateTime? IssueDate { get; set; }

		[Required]
		public string? Reference_No { get; set; }

		public string? Issue_type { get; set; }
	
		public string? Vendor_Name { get; set; }

		public int? UpdatedBy { get; set; }

		public DateTime? UpdatedOn { get; set; }
		
	}

	public class Casting_MaterialIssue_Item_VM
	{
		public int IssueItemKey { get; set; }

		public int? IssueDbKey { get; set; }

		public int? QtySplitKey { get; set; }

		[Required]
		public int? IssueQty { get; set; }
		[Required]
		public string? IssueSlNos { get; set; }

		public int? UpdatedBy { get; set; }

		public DateTime? UpdatedOn { get; set; }
		public string? ForEngine { get; set; }

		public string? JobCardNumber { get; set; }

		public string? JCFileName { get; set; }

		public string? JCFileLocation { get; set; }
	}
	public class GenericMaterialIssueVM
	{
		public Casting_MaterialIssue_VM casting_MaterialIssue_VM { get; set; }
		public List<Casting_MaterialIssue_Item_VM> casting_MaterialIssue_Items_VM { get; set; }
		public IFormFileCollection FormFiles { get; set; }

	}


}
