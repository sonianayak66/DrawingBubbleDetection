namespace MPCRS.ViewModels
{
	public class MaterialIssueNoteDetails
	{
		public int Issue_Dbkey { get; set; }
		public string? Demand_No { get; set; }
		public string? Order_Ref_No { get; set; }
		public DateTime Order_Ref_Date { get; set; }
		public string? PMO_Ref_No { get; set; }
		public string? Vendor_Name { get; set; }
		public string? Issue_Purpose { get; set; }
		public string? PartName { get; set; }

		public float? Qty { get; set; }
    }
}
