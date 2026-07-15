namespace MPCRS.ViewModels
{
    public class NCRPartEngineReportViewModel
    {
        public List<string> ColumnHeaders { get; set; } = new List<string>();
        public List<Dictionary<string, string>> ReportData { get; set; } = new List<Dictionary<string, string>>();
    }
}
