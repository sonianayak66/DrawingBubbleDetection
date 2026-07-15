namespace MPCRS.ViewModels
{
    public class DuplicateDeviationItem
    {
        public int DuplicateGroupId { get; set; }
        public string PartNo { get; set; }          // mapped from [Part No]
        public string Description { get; set; }
        public string SerialNumber { get; set; }
        public string DrgZone { get; set; }
        public string DrawingDimension { get; set; }  // mapped from [Drawing Dimension]
        public string ActualDimension { get; set; }   // mapped from [Actual Dimension]
        public string NCRReference { get; set; }      // mapped from [NCR Reference]
        public DateTime? ReceivedDate { get; set; }    // mapped from [Received Date]
        public string CurrentStatus { get; set; }      // mapped from [Current Status]
        public int DuplicateCount { get; set; }        // mapped from [Duplicate Count]
        public string NCRGuid { get; set; }
    }
}