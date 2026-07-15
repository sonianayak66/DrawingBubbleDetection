using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
    
{
    public class EmailConfigVM
    {
        public int Sl_No { get; set; }
        [Required(ErrorMessage = "Required")]
        [DisplayName("Mail ID")]
        public string? MailID { get; set; }
        [Required(ErrorMessage = "Required")]
        [DisplayName("Password")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "Required")]
        [DisplayName("SMTP HostName")]
        public string? SMTP_HostName { get; set; }
        [DisplayName("SMTP Port")]
        [Required(ErrorMessage = "Required")]
        public int? SMTP_Port { get; set; }
        public string? SSL { get; set; }
        public int? Updated_By { get; set; }

        public DateTime? Updated_On { get; set; }

          }
}
