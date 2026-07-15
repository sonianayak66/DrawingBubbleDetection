using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MPCRS.ViewModels
{
    public class PartManufacturingStatusVM
    {
        public Nullable<int> Engine_Part_Dbkey { get; set; }
        public Nullable<long> Part_relation_dbkey { get; set; }
        public string PartNumber { get; set; }
        public string PartDescription { get; set; }
        public string PartRevision { get; set; }
        public int QtyPerEngine { get; set; }
        public List<VendorComponentStatus> vendorComponentStatuses { get; set; }
        public SelectList MasterManufacturingStatus { get; set; }
        public SelectList MasterRevisions { get; set; }
        public SelectList MasterVendors { get; set; }
        public void LoadMasterLists()
        {
           
            MasterRevisions = MPCRS.Utilities.Masters.RevisionList();
            MasterManufacturingStatus = MPCRS.Utilities.Masters.GetMaster_General("ManufacturingStatus");      
            MasterVendors = MPCRS.Utilities.Masters.GetVendorsList();
        }
    }

    public class VendorComponentStatus
    {
        public int Id { get; set; }
        public Nullable<int> Engine_Part_Dbkey { get; set; }
        public Nullable<long> Part_relation_dbkey { get; set; }
        public string Revision { get; set; }
        public Nullable<int> QtyOrdered { get; set; }
        public Nullable<int> QtyReceived { get; set; }
        public Nullable<int> VendorId { get; set; }
        public Nullable<int> ManufacturingStatus { get; set; }

    }

}
