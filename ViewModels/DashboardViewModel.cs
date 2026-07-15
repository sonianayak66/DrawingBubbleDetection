using System;
using System.Collections.Generic;

namespace MPCRS.ViewModels
{
    // ============================================
    // MAIN DASHBOARD VIEWMODEL
    // ============================================
    public class DashboardViewModel
    {
        public UserPermissionsVM UserPermissions { get; set; }

        public DashboardViewModel()
        {
            UserPermissions = new UserPermissionsVM();
        }
    }

    public class UserPermissionsVM
    {
        public bool HasProcurement { get; set; }
        public bool HasManufacturing { get; set; }
        public bool HasQuality { get; set; }
        public bool HasSOP { get; set; }
        public bool HasConfiguration { get; set; }
        public bool HasSystem { get; set; }
    }

    // ============================================
    // PROCUREMENT DASHBOARD VIEWMODEL
    // ============================================
    public class ProcurementDashboardVM
    {
        // SECTION 1: KPI Cards (Result Sets 1-4)
        public UrgentActionsSummary UrgentActions { get; set; }
        public VendorAccountabilitySummary VendorAccountability { get; set; }
        public MaterialFlowSummary MaterialFlow { get; set; }
        public UntrackedDemandsSummary UntrackedDemands { get; set; }

        // SECTION 2: Charts (Result Sets 5-7)
        public List<VendorPerformanceChart> VendorPerformanceData { get; set; }
        public List<MaterialHealthChart> MaterialHealthData { get; set; }
        public List<AtRiskDemand> AtRiskDemands { get; set; }

        // SECTION 3: Tables (Result Sets 8-12)
        public List<ImmediateAttentionItem> ImmediateAttention { get; set; }
        public List<MaterialAvailabilityItem> MaterialAvailability { get; set; }
        public List<VendorPerformanceScorecard> VendorPerformanceScorecard { get; set; }
        public List<UpcomingMilestone> UpcomingMilestones { get; set; }
        public List<MaterialIssuedToVendor> MaterialIssuedToVendors { get; set; }

        // SECTION 4: Legacy (Result Sets 13-16)
        public List<CostVarianceItem> CostVarianceAnalysis { get; set; }
        public List<ProcurementStatusDistribution> ProcurementByStatus { get; set; }
        public List<TopVendor> TopVendors { get; set; }
        public List<OverdueProcurement> OverdueProcurements { get; set; }

        public List<UntrackedDemandItem> UntrackedDemandsList { get; set; }

        // SECTION 5: Order Type Metrics (Result Sets 18-23)
        public OrderTypeMetricsSummary CastingMetrics { get; set; }
        public List<OrderTypePartDetail> CastingDetails { get; set; }
        public OrderTypeMetricsSummary ForgingMetrics { get; set; }
        public List<OrderTypePartDetail> ForgingDetails { get; set; }
        public OrderTypeMetricsSummary PyroMetrics { get; set; }
        public List<OrderTypePartDetail> PyroDetails { get; set; }

    }

    // ============================================
    // DATA CLASSES FOR EACH RESULT SET
    // ============================================

    // Result Set 1
    public class UrgentActionsSummary
    {
        // Demands (unique)
        public int DemandsDueIn7Days { get; set; }
        public int DemandsOverdue { get; set; }
        public int TotalUrgentDemands { get; set; }

        // Milestone items (can be multiple per demand)
        public int MilestonesDueIn7Days { get; set; }
        public int MilestonesOverdue { get; set; }
        public int TotalUrgentMilestones { get; set; }

        // Financial impact
        public decimal ValueDueIn7Days { get; set; }
        public decimal ValueOverdue { get; set; }
        public decimal TotalValueAtRisk { get; set; }
    }

    // Result Set 2
    public class VendorAccountabilitySummary
    {
        public int VendorsBehindSchedule { get; set; }
        public int TotalActiveVendors { get; set; }   
        public int TotalOverdueMilestones { get; set; }
        public double AvgDelayDays { get; set; }
    }

    // Result Set 3
    public class MaterialFlowSummary
    {
        public int TargetEngineCount { get; set; }
        public int MinEnginesCanProduce { get; set; }
        public int MaterialsInShortage { get; set; }
        public int MaterialsSufficient { get; set; }
        public int CriticalMaterials { get; set; }
        public int TotalActiveMaterials { get; set; }
    }

    // Result Set 4
    public class UntrackedDemandsSummary
    {
        public int TotalUntrackedDemands { get; set; }
        public int OverdueIncomplete { get; set; }
        public int DueSoon { get; set; }
        public int CompletedUntracked { get; set; }
        public decimal TotalUntrackedValue { get; set; }
    }

    // =============================================
    // SECTION 2: Chart ViewModels
    // =============================================

    // Result Set 5
    public class VendorPerformanceChart
    {
        public string Vendor_Name { get; set; }
        public int TotalMilestones { get; set; }
        public int CompletedOnTime { get; set; }
        public int CurrentlyOverdue { get; set; }
        public int DueSoon { get; set; }
        public double CompletionRate { get; set; }
        public double AvgDelayDays { get; set; }
      
    }

    // Result Set 6
    public class MaterialHealthChart
    {
        public string HealthCategory { get; set; }
        public int MaterialCount { get; set; }
        public decimal Percentage { get; set; }
    }

    // Result Set 7
    public class AtRiskDemand
    {
        public string Demand_No { get; set; }
        public string Item_Description { get; set; }
        public int OverdueMilestonesCount { get; set; }
        public int DaysOverdue { get; set; }
        public decimal EstimatedValue { get; set; }
        public string Vendor_Name { get; set; }
    }

    // =============================================
    // SECTION 3: Table ViewModels
    // =============================================

    // Result Set 8
    public class ImmediateAttentionItem
    {
        public int DemandDbkey { get; set; }
        public string MMG_File_No { get; set; } 
        public string Demand_No { get; set; } 
        public string Vendor_Name { get; set; }
        public string MilestoneName { get; set; }
        public DateTime? DueDate { get; set; }
        public int DaysUntilDue { get; set; }
        public double QtyRequired { get; set; }
        public double QtyReceived { get; set; }
        public double Balance { get; set; }
        public string Status { get; set; }
        public decimal ValueAtRisk { get; set; }
        public decimal EstimatedCost { get; set; }
    }

    // Result Set 9
    public class MaterialAvailabilityItem
    {
        public string Material_Name { get; set; }
        public string UOM { get; set; }
        public double TotalOrdered { get; set; }
        public double TotalReceived { get; set; }
        public double ProcurementBalance { get; set; }
        public double TotalIssued { get; set; }
        public double InventoryBalance { get; set; }
        public double Threshold { get; set; }
        public double HealthPercentage { get; set; }
        public string HealthStatus { get; set; }
        public string RMType { get; set; }
        public string VendorNames { get; set; }
        public DateTime? LastReceiptDate { get; set; }
        public string ExecutionResponsibility { get; set; }
    }

    // Result Set 10
    public class VendorPerformanceScorecard
    {
        public string Vendor_Name { get; set; }
        public int TotalMilestones { get; set; }
        public int CompletedOnTime { get; set; }
        public int OverdueCount { get; set; }
        public double CompletionRate { get; set; }
        public double AvgDelayDays { get; set; }
        public decimal TotalOrderValue { get; set; }
        
    }

    // Result Set 11
    public class UpcomingMilestone
    {
        public string Demand_No { get; set; }
        public string Vendor_Name { get; set; }
        public string MilestoneName { get; set; }
        public DateTime? DueDate { get; set; }
        public int DaysUntilDue { get; set; }
        public double QtyRequired { get; set; }
        public double CurrentProgressPercentage { get; set; }
        public string Status { get; set; }
        public string Item_Description { get; set; }
        public double VendorHistoricalOnTimeRate { get; set; }
    }

    // Result Set 12
    public class MaterialIssuedToVendor
    {
        public string Vendor_Name { get; set; }
        public string Material_Name { get; set; }
        public double IssuedQty { get; set; }
        public string UOM { get; set; }
        public DateTime? IssueDate { get; set; }
        public int DaysWithVendor { get; set; }
        public string JobCardNo { get; set; }
        public string Purpose { get; set; }
        public string ExecutionResponsibility { get; set; }
        public string Status { get; set; }
    }

    // =============================================
    // SECTION 4: Legacy ViewModels
    // =============================================

    // Result Set 13
    public class CostVarianceItem
    {
        public string MMG_File_No { get; set; }
        public string Item_Description { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal ActualCost { get; set; }
        public decimal Variance { get; set; }
        public double VariancePercent { get; set; }
        public string Vendor_Name { get; set; }
    }

    // Result Set 14
    public class ProcurementStatusDistribution
    {
        public string CurrentStatus { get; set; }
        public int DemandCount { get; set; }
    }

    // Result Set 15
    public class TopVendor
    {
        public string Vendor_Name { get; set; }
        public decimal TotalOrderValue { get; set; }
        public int TotalOrders { get; set; }
    }

    // Result Set 16
    public class OverdueProcurement
    {
        public string Demand_No { get; set; }
        public string Item_Description { get; set; }
        public DateTime? Planned_Date_of_receipt { get; set; }
        public int DaysOverdue { get; set; }
        public double PendingQty { get; set; }
        public double OrderedQty { get; set; }
        public string Vendor_Name { get; set; }
        public string CurrentStatus { get; set; }
        public decimal EstimatedCost { get; set; }
    }

    // Result Set 17
    public class UntrackedDemandItem
    {
        public int DemandDbKey { get; set; }
        public string MMG_File_No { get; set; }
        public string Item_Description { get; set; }
        public DateTime? Planned_Date_of_receipt { get; set; }
        public string CurrentStatus { get; set; }
        public string Vendor_Name { get; set; }
        public decimal EstimatedCost { get; set; }
        public double TotalOrdered { get; set; }
        public double TotalReceived { get; set; }
        public double TotalBalance { get; set; }
        public double FulfillmentPercentage { get; set; }
        public string TrackingStatus { get; set; }
        public int DaysFromPlanned { get; set; }
    }
    // ============================================
    // CASTING/FORGING/PYRO ORDER METRICS
    // ============================================
    public class OrderTypeMetricsSummary
    {
        public string OrderType { get; set; }
        public int TotalEnginesPossible { get; set; }
        public string BottleneckPart { get; set; }
        public int TotalAcceptedQty { get; set; }
        public int TotalIssuedQty { get; set; }
        public int TotalAvailableQty { get; set; }
        public int TotalParts { get; set; }
        public int PartsWithNegativeStock { get; set; }
        public int PartsNotReceived { get; set; }
    }

    public class OrderTypePartDetail
    {
        public int Engine_Part_Dbkey { get; set; }
        public string Draw_part_no { get; set; }
        public string Description { get; set; }
        public int QuantityPerEngine { get; set; }
        public int AcceptedQty { get; set; }
        public int IssuedQty { get; set; }
        public int AvailableQty { get; set; }
        public int EnginesPossibleForThisPart { get; set; }
        public int IsBottleneck { get; set; }
        public string StockStatus { get; set; }
    }

}
