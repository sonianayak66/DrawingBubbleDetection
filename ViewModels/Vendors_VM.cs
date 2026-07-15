
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace MPCRS.ViewModels
{
    public class Vendors_VM
    {
        public int Vendor_Dbkey { get; set; }

        [DisplayName("Vendor ID System")]
        public string? Vendor_ID_System { get; set; }


        [Required]
        [DisplayName("Vendor ID")]
        public string? Vendor_ID_User { get; set; }

        [Required]
        [DisplayName("Vendor Name")]
        public string? Vendor_Name { get; set; }
        [Required]
        [DisplayName("Vendor Email")]
        public string? Vendor_Email { get; set; }

        [Required]
        [DisplayName("Vendor Contact")]
        public string? Vendor_Contact { get; set; }
        [Required]
        [DisplayName("Address")]
        public string? Vendor_Adress { get; set; }
        [DisplayName("State")]
        public string? Vendor_State { get; set; }
        [DisplayName("City")]
        public string? Vendor_City { get; set; }
        [DisplayName("Pincode")]
        public string? Vendor_Pincode { get; set; }

        public int Updated_by { get; set; }

        public DateTime Updated_on { get; set; }

        public string? Vendor_guid { get; set; }
    
}
}
