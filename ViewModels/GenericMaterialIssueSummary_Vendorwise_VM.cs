namespace MPCRS.ViewModels
{
	public class GenericMaterialIssueSummary_Vendorwise_VM
	{
		public int? QtySplitKey { get; set; }
		public DateTime? IssueDate { get; set; }
		public string? Vendor_Name { get; set; }
		public string? Reference_No { get; set; }
		public string? IssueSlNos { get; set; }
		public string? Draw_part_no { get; set; }
		public string? Description { get; set; }
		public int TotalQty { get; set; }
		public int IssuedQty { get; set; }
		public int? RemaningQty { get; set; }

		public string? ForEngine { get; set; }


	}
}
