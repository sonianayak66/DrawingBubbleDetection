namespace MPCRS.ViewModels
{
	public class GenericMaterialIssueSummary_VM
	{
		public string? Issue_type { get; set; }
		public int? QtySplitKey { get; set; }
		public int? Engine_Part_Dbkey { get; set; }
		public string? Draw_part_no { get; set; }
		public string? Description { get; set; }
		public int? TotalQty { get; set; }
		public int? IssuedQty { get; set; }
		public int? RemaningQty { get; set; }

	}
}
