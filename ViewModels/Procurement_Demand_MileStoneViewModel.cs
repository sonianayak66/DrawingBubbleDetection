using MPCRS.Models;

namespace MPCRS.ViewModels
{
	public class Procurement_Demand_MileStoneViewModel
	{
		public int? MilestoneDbKey { get; set; }
		public int? DemandDbkey { get; set; }
		public int? DemandItemDbKey { get; set; }
		public int? Milestone { get; set; }
		public DateTime? DeliveryDate { get; set; }
		public double? MilestoneQty { get; set; }
		public double? OrderQuantity { get; set; }
		public double? ActualReceiptQty { get; set; }
		public double? running_MileStoneQty { get; set; }
		public int? Updateby { get; set; }
		public DateTime? UpdatedOn { get; set; }
		public string? DemandItemType { get; set; }
		public int? DemandItemTypeKey { get; set; }
		public string? Remarks { get; set; }
		public string? Item_Code { get; set; }
		public string? Vendor_Name { get; set; }
		public string? Vendor_Contact { get; set; }
		public string? DemandItemName { get; set; }
		public string? ItemDescription { get; set; }
		public string? ItemMilestoneStatus { get; set; }
		public string? Demand_No { get; set; }
		public int? DemandingOfficer { get; set; }
		public string? MMG_File_No { get; set; }
		public string? DemandingOfficerName { get; set; }
		public int? MilestoneID { get; set; }
		public string? MilestoneName { get;set; }
		public string? Comments { get; set; }
		public string? Status { get; set; }
		public string? Description { get; set; }
		public string? QtyPercentage { get; set; }
		public string? IsLastMilestone { get; set; }

        public DateTime? OriginalDueDate { get; set; }
    }


	public class milestoneStatusData
	{
		public int DemandDbkey { get; set; }
		public int DemandItemDbKey { get; set; }
		public int Milestone { get; set; }
		public DateTime? DeliveryDate { get; set; }
		public float RunningMilestoneQty { get; set; }
		public float OverallrptPc { get; set; }
		public float TotalReceipts { get; set; }
		public int MilestoneID { get; set; }
		public float rptPc { get; set; }
		public DateTime? ReceiptAsonDate { get; set; }
		public float RunningReceiptQty { get; set; }
		public string Status { get; set; }
	}

    //public class CompleteMilestoneData
    //{
    //    public List<Procurement_Demand_MileStone> procurement_Demand_MileStones { get; set; }
    //    public List<ProcurementMilestone> procurementMilestones { get; set; }


    //}

    public class CompleteMilestoneData
    {
        public List<Procurement_Demand_MileStone_SaveVM> procurement_Demand_MileStones { get; set; }
        public List<ProcurementMilestone_SaveVM> procurementMilestones { get; set; }
    }

    public class Procurement_Demand_MileStone_SaveVM
    {
        public int? MilestoneDbKey { get; set; }   // maps to MilestoneItemID
        public int? MilestoneID { get; set; }
        public int? DemandItemDbKey { get; set; }
        public double? Qty { get; set; }
        public double? OrderQuantity { get; set; }
    }

    public class ProcurementMilestone_SaveVM
    {
        public int MilestoneID { get; set; }
        public string? MilestoneName { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Comments { get; set; }
    }

}
