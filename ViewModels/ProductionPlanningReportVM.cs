namespace MPCRS.ViewModels
{
    public class ProductionPlanningReportVM
    {
        public int Raw_material_Dbkey { get; set; }
        public string? MaterialName { get; set; }
        public string? RMType { get; set; }

        // Engine Production Details
        public double RequiredPerEngine { get; set; }

        public string? EngineList { get; set; }


        // Procurement Details
        public double TotalOrdered { get; set; }
        public double TotalReceived { get; set; }
        public double ProcurementBalance { get; set; }

        // Issue Details
        public double TotalIssued { get; set; }

        // Availability vs Requirement
        public double CurrentStock { get; set; }
        public double ShortageOrSurplus { get; set; }
        public double EngineRMAvailability { get; set; }
       
    }
}