namespace MPCRS.ViewModels
{
    public class ReceiptCommentVM
    {
        public List<ReceiptCommentUserDetailsVM> receiptCommentUserDetailsVMs { get; set; }
        public CastingReceiptSplitVM castingReceiptSplitVM { get; set; }

    }

    public class DepartmentOrderVM
    {
        public int Id { get; set; }
        public int DepartmentID { get; set; }
        public float DisplayOrder { get; set; }
        public string? Department { get; set; }

    }
}
