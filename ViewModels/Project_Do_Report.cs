namespace MPCRS.ViewModels
{
	public class Project_Do_Report
	{
		public int? Project_Dbkey { get; set; }
		public string? CurrentStatus { get; set; }
		public int? DO_Review { get; set; }
		public string? Display_title { get; set; }
		public int? Demands { get; set; }
		public int? Misc { get; set; }
		public string? DemandStatus { get; set; }
		public int? TotalDemands { get; set; }
		public int? closedDemands { get; set; }

	}


	public class demandSummaryData
	{
		public int DemandCount { get; set; }
		public string CurrentStatus { get; set; }
		public string DemandingOfficer { get; set; }
		public string Display_title { get; set; }
		public string statusDisplayOrder { get; set; }
		public string statusCategory { get; set; }
		public int Project_Dbkey { get; set; }
		public int DemandingOfficerKey { get; set; }
	}

}
