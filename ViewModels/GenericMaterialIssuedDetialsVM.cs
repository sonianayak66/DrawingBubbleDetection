namespace MPCRS.ViewModels
{
	public class GenericMaterialIssuedDetialsVM
	{
		public int? IssueDbKey { get; set; }
		public int? IssueItemKey { get; set; }
		public int? QtySplitKey { get; set; }
		public string? ReferenceNo {get; set;}
		public DateTime? IssueDate { get; set; }
		public string? Vendor_Name { get; set; }
		public string? Draw_part_no { get; set; }
		public string? Description { get; set; }
		public int? IssueQty { get; set; }
		public string? IssueSlNos { get; set; }

		public string? ForEngine { get; set; }

	}
}
