namespace MPCRS.ViewModels
{
	public class ReceiptCommentUserDetailsVM
	{
		public int OrderId { get; set; }
		public int CastingReceiptsItemSplitKey { get; set; }
		public string UserGuid { get; set; }
		public string UserName { get; set; }
		public string? Comments { get; set; }
		public string? Department { get; set; }
		public int? DepartmentId { get; set; }
		public int? CastingReceiptsCommentsKey { get; set; }
		public string? Designation { get; set; }

	}
}
