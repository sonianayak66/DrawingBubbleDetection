namespace MPCRS.ViewModels
{
    public class NCRSummaryViewModel
    {
           /// List of engines with their NCR counts 
        public List<EngineNCRCount> EngineSummary { get; set; } = new List<EngineNCRCount>();

       
        /// NCR count per current status 
        public List<StatusNCRCount> StatusDistribution { get; set; } = new List<StatusNCRCount>();
    }

    public class EngineNCRCount
    {
        public string Engine { get; set; }
        public int NCRCount { get; set; }
    }

    public class StatusNCRCount
    {
        public string CurrentStatus { get; set; }
        public int NCRCount { get; set; }
    }
}

