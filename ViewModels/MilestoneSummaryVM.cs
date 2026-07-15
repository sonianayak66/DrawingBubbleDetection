using System.Web.Mvc;

namespace MPCRS.ViewModels
{
	public class MilestoneSummaryVM
	{
		public string? DemandingOfficerName { get; set; }
		public string? DemandingOfficerEmail { get; set; }
		public string? Project { get; set; }
		public string? MMG_File_No { get; set; }
        public string? Demand_No { get; set; }
        public int? DemandDbKey { get; set; }
        public int? ProjectDbkey { get; set; }
		public string? Item_Description { get; set; }
		public int? MilestoneID { get; set; }
		public string? MilestoneName { get; set; }
		public DateTime? DueDate { get; set; }
		public int RemainingDays { get; set; }

		public string? VendorName { get; set; }
		public string? MilestoneStatus { get; set; }
        public string? OrderNumbers { get; set; }

		public string? ProjectTitle { get; set;}
		public string? MilestoneRemarks { get; set;}

        public DateTime? OriginalDueDate { get; set; }

        //public  SelectList? demandsSelectList { get; set; }


    }
}
