using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static MPCRS.Utilities.Constants;

namespace MPCRS.DbJsonModels
{
    public sealed class PersonViewModel
    {
        public string PersonGUID { get; set; } = VTSDataRecordType.New.ToString();

        [Required]
        [DisplayName("Name")]
        public string name { get; set; }

        [Required]
        [Phone]
        [DisplayName("Phone")]
        [RegularExpression(@"^(?!\d+[09]{4}$)\d{10}$", ErrorMessage = "Invalid Phone Number")]
        public string phone_number { get; set; }

        [Required]
        [EmailAddress]
        [DisplayName("Email")]
        public string email_address { get; set; }

        [DisplayName("Designation")]
        public string designationID { get; set; }

        [DisplayName("Person Type")]
        public string persontypeId { get; set; }

        [DisplayName("Department")]
        public string departmentId { get; set; }

        [DisplayName("Active user?")]
        public bool isactive { get; set; } = true;

        [DisplayName("Allow Login?")]
        public bool allowLogin { get; set; } = false;
        public DateTime? updated_on { get; set; }
        public string? updated_by { get; set; }
        public string? additionalInfoJson { get; set; }
    }

    public class managePerson
    {
        public PersonViewModel PersonInfo { get; set; }

        [DisplayName("Roles")]
        public string?[] roles { get; set; }
        public bool ResetPassword { get; set; } = false;
    }

}
