namespace MPCRS.ViewModels
{
    public class RMC_VM
    {
        public int ID { get; set; }
        public string? Raw_Material_GUID { get; set; }
        public string? Raw_Material_Name { get; set; }
        public string? Heat_No { get; set; }
        public string? Batch_No { get; set; }
        public string? RMC_Number { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
