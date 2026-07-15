using System;
using System.Collections.Generic;

namespace MPCRS.Models;

public partial class Procurement_Demand
{
    public int DemandDbKey { get; set; }

    public string? Project_Head { get; set; }

    public string? Demand_No { get; set; }

    public string? Item_Description { get; set; }

    public decimal? Quantity { get; set; }

    public string? UOM { get; set; }

    public string? Item_Type { get; set; }

    public decimal? EstimatedCost { get; set; }

    public string? TenderMode { get; set; }

    public int? DemandingOfficer { get; set; }

    public DateTime? StatusDate { get; set; }

    public string? CurrentStatus { get; set; }

    public bool? DO_Review { get; set; }

    public DateTime? EstimatedOrderDate { get; set; }

    public int? Delivery_Schedule { get; set; }

    public DateTime? Planned_Date_of_receipt { get; set; }

    public string? Remarks { get; set; }

    public int? Updated_By { get; set; }

    public DateTime? Updated_On { get; set; }

    public int? Project_Dbkey { get; set; }

    public string? MMG_File_No { get; set; }

    public decimal? ActualCost { get; set; }

    public bool? IsActive { get; set; }

    public int? Vendor_Dbkey { get; set; }

    public bool? IsShortClosure { get; set; }

    public DateTime? ShortClosedOn { get; set; }

    public int? ShortClosedBy { get; set; }

    public string? ShortCloseReason { get; set; }

    public string? OrderNumbers { get; set; }

    public string? OrderType { get; set; }

    public decimal? AdvancePaid { get; set; }

    public decimal? PaymentMadeTillDate { get; set; }

    public virtual ICollection<Procurement_Demand_Receipt> Procurement_Demand_Receipts { get; set; } = new List<Procurement_Demand_Receipt>();
}
