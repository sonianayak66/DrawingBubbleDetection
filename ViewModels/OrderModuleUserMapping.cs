using MPCRS.Models;

namespace MPCRS.ViewModels
{

    public class OrdersModuleUserDetails
    {
       public CastingDetail castingDetail { get; set; }
        public List<OrderModuleUserMapping> orderModuleUserMappings {  get; set; }  
    }


    public class OrderModuleUserMapping
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public string? OrderType { get; set; }
        public string? UserGuid { get; set; }
        public string? UserName { get; set; }
		public string? DemandNumber { get; set; }
		public string? DemandDesc { get; set; }
		public string? MMGOrderNumber { get; set; }
        public string? castingGUID { get; set; }        
        public DateTime? UpdateOn { get; set; }
    }
}
