using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using MPCRS.Models;


namespace MPCRS.ViewModels
{


    public class Procurement_Demands_VM
    {
        public int DemandDbKey { get; set; }
        [Required]
        [DisplayName("Project Head")]
        public string? Project_Head { get; set; }

        [Required]
        [DisplayName("Demand No")]
        public string Demand_No { get; set; }

        [Required]
        [DisplayName("Demand Description")]
        public string? Item_Description { get; set; }

        [Required]
        public decimal? Quantity { get; set; }
        [Required]
        public string? UOM { get; set; }

        [Required]
        [DisplayName("Item Type")]
        public string? Item_Type { get; set; }

        [Required]
        [DisplayName("Estimated Cost")]
        public decimal? EstimatedCost { get; set; }
        [Required]
        [DisplayName("Tender Mode")]
        public string? TenderMode { get; set; }

        [Required]
        [DisplayName("Demanding Officer")]
        public int? DemandingOfficer { get; set; }


        [DisplayName("Status as On")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? StatusDate { get; set; }

        [Required]
        [DisplayName("Current Status")]
        public string? CurrentStatus { get; set; }


        [DisplayName("Under DO Review?")]
        public bool? DO_Review { get; set; }

        [DisplayName("Estimated Order Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EstimatedOrderDate { get; set; }

        [DisplayName("Delivery Schedule")]
        public int? Delivery_Schedule { get; set; }

        [DisplayName("Planned Date of receipt")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Planned_Date_of_receipt { get; set; }

        public string? Remarks { get; set; }

        public int? Updated_By { get; set; }

        public DateTime? Updated_On { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Required")]
        public int? Project_Dbkey { get; set; }
        [Required]
        [DisplayName("MMG File No")]
        public string MMG_File_No { get; set; }

        [DisplayName("Actual Cost")]
        public decimal? ActualCost { get; set; }

        [DisplayName("Payment Made Till Date")]
        public decimal? PaymentMadeTillDate { get; set; }

        public decimal? BalanceOrderValue { get; set; }
        public bool? IsActive { get; set; }
        [Required]
        [DisplayName("Vendor")]
        public int? Vendor_Dbkey { get; set; }

        public bool? IsShortClosure { get; set; } = false;

        public DateTime? ShortClosedOn { get; set; }

        public int? ShortClosedBy { get; set; }

        public string ShortCloseReason { get; set; }

        public int? revertshortclose { get; set; }
        public string? OrderNumbers { get; set; }
        public string? DemandOfficerName { get; set; }

        public string? OrderType { get; set; }

        public string? VendorName { get; set; }
        public decimal? ProjectRunningBalance { get; set; }
        public decimal? ProjectSanctionedCost { get; set; }
        public string? ProjectTitle { get; set; }
        public string? MilestoneName { get; set; }
        public string? MilestoneStatus { get; set; }

        public DateTime? DueDate { get; set; }

        [Required]
        [DisplayName("Advance Paid")]
        public decimal? AdvancePaid { get; set; }

    }

    public class ProcurementStatusIndicator
    {
        public Procurement_Demands_VM DemandData { get; set; }
        public List<Procurement_Demands_History> DemandHistory { get; set; }
    }

    public class ProcurementDemandInfo
    {
        public Procurement_Demands_VM DemandData { get; set; }

        public List<Procurement_Demand_Items_VM> procurement_Demand_Items_VMs { get; set; }

        public List<Procurement_Demand_Receipt> procurement_Demand_Receipts { get; set; }

    }


    public partial class Procurement_Demand_Receipt_VM
    {
        public int? Receipt_dbkey { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Receipt_Date { get; set; }

        public string? Receipt_No { get; set; }

        public int? DemandDbKey { get; set; }

        public int? DemandItemKey { get; set; }
        public double? Receiving_inventory { get; set; }
        public int Index_No { get; set; }
    }

    public class Procurement_Demand_History_VM
    {
        public int Demand_Procurement_History_Key { get; set; }
        public Nullable<int> DemandDbKey { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> ActionDate { get; set; }
        public string ActionStatus { get; set; }
        public Nullable<bool> Do_Review { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> Updated_By { get; set; }
        public Nullable<System.DateTime> Updated_On { get; set; }
    }


    public class Procurement_Demad_ReceiptDocumentsVM
    {
        public int RowNumber { get; set; }
        public int Attachment_Db_Key { get; set; }
        public string Source_table { get; set; }
        public string Source_table_key { get; set; }
        public string Attachment_location { get; set; }
        public string Attachment_FileName { get; set; }
        public string Orginal_File_Name { get; set; }
        public string File_DVD_Num { get; set; }
        public string Master_Name { get; set; }
        public string Receipt_No { get; set; }
        public string ItemName { get; set; }
        public string Demand_No { get; set; }
        public string Item_Description { get; set; }
        public int? DemandItemKey { get; set; }
        public int DemandDbKey { get; set; }
        public DateTime? Receipt_Date { get; set; }
        public int? Receiving_inventory { get; set; }
        public string Attachments { get; set; }

    }

    public class Procurement_DemandReceiptHistoryVM
    {
        public int DemandDbKey { get; set; }
        public string? Demand_No { get; set; }
        public string? MMG_File_No { get; set; }
        public DateTime Receipt_Date { get; set; }
        public string? Receipt_No { get; set; }
        public string? Material_name { get; set; }
        public double Qty { get; set; }
        public double Physical_inventory { get; set; }
        public double Receiving_inventory { get; set; }
        public int Index_No { get; set; }
        public int Receipt_dbkey { get; set; }
        public string? Outer_Dia_mm { get; set; }
        // public string? Inner_Dia_mm  { get; set; }
        public string? Thickness { get; set; }
        public string? height { get; set; }
        public string? Master_Name { get; set; }
    }

    public class ProcurementDemandReceiptSummaryVM
    {
        public int DemandDbKey { get; set; }
        public string Demand_No { get; set; }
        public string MMG_File_No { get; set; }
        public int RawMaterialKey { get; set; }
        public string Raw_material_Name { get; set; }
        public float OrderQty { get; set; }
        public DateTime Receipt_Date { get; set; }
        public string Receipt_No { get; set; }
        public float ReceiptQty { get; set; }
        public int Receipt_dbkey { get; set; }

    }

    public class ProcurementPartsSummaryVM
    {
        public int DemandDbKey { get; set; }
        public string Demand_No { get; set; }
        public string MMG_File_No { get; set; }
        public string Item_Code { get; set; }
        public string Description { get; set; }
        public float OrderedQty { get; set; }
        public DateTime Receipt_Date { get; set; }
        public string Receipt_No { get; set; }
        public float ReceiptQty { get; set; }
        public int Receipt_dbkey { get; set; }

    }
}
