namespace MPCRS.ViewModels
{
    public class BATL_IssueSummary
    {
        public string? Draw_part_no { get; set; }
        public string? parent_partno { get; set; }
        public string? Type_Part_Name { get; set; }
        public string? Description { get; set; }
        public int? Quantity { get; set; }
        public int? Qty_For_44Eng { get; set; }
        public string? Execution_Resp { get; set; }
        public string? JobCardFileName { get; set; }
        public string? JobCardFileLocation { get; set; }
        public int? Total_Issued_Qty { get; set; }

    }
}
