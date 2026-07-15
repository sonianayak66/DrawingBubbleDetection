using MPCRS.Models;

namespace MPCRS.ViewModels
{
    public class DemandDataEntryStatus
    {

        public List<Procurement_Demands_VM> procurement_Demands_VM { get; set; }
        public List<Procurement_Demand_Items_VM> Procurement_Demand_Items_VM { get; set; }
        public List<Procurement_Demand_Receipt> Procurement_Demand_Receipt { get; set; }
        public List<Procurement_ReceiptItemSplit> procurement_ReceiptItemSplits { get; set; }

    }
}
