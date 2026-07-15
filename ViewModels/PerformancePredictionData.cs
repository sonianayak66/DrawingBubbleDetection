using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
    public class PerformancePredictionData
    {
        public int predictionKey { get; set; }
        public string predectionGUID { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile inputCSV { get; set; }
        public string inputFilename { get; set; }
        public string InputDataJson { get; set; }
        public string OutputDataJson { get; set; }
        public string createdBy { get; set; }
        public DateTime createdon { get; set; }
    }
}
