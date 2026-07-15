namespace MPCRS.ViewModels
{
    // SaveRemarksVM.cs

    public class SaveRemarksVM
    {
        public int TrackingID { get; set; }
        public int NCRItemKey { get; set; }
        public int StageID { get; set; }
        public string Remarks { get; set; } 
    }

    public class SaveResultVM
    {
        public int Success { get; set; }
        public string Message { get; set; }
    }
}
