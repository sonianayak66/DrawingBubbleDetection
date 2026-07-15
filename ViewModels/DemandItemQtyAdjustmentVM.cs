namespace MPCRS.ViewModels
{
    public class DemandItemQtyAdjustmentVM
    {

        public int DemandItemKey { get; set; }
        public string ItemName { get; set; }
        public double OriginalQty { get; set; }
        public string UOM { get; set; }

        // Adjustment fields
        public int AdjustmentDbkey { get; set; }  // 0 if new, otherwise existing adjustment ID
        public double AdjustmentQty { get; set; }  // 0 if no adjustment exists
        public string AdjustmentRemarks { get; set; }  // Empty if no adjustment exists

        // Helper flag
        public bool HasExistingAdjustment => AdjustmentDbkey > 0;

    }
}
