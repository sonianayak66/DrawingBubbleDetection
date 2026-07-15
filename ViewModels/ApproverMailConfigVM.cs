using MPCRS.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
namespace MPCRS.ViewModels
{
    public class ApproverMailConfigVM
    {
        public int Mail_Temp_ID { get; set; }

        public string? Mail_Temp_Name { get; set; }
        [Required]
        [DisplayName("email Subject")]
        public string Mail_Subject { get; set; } = null!;
        [DisplayName("email Body")]
        [Required]
        public string Mail_Body { get; set; } = null!;

        public string? Parameters { get; set; }

        public string? Recipients { get; set; }
        [DisplayName("CC to")]
        public string? CopyTo { get; set; }
        [DisplayName("BCC to")]
        public string? BlindCopy { get; set; }

        public int? Updated_By { get; set; }

        public DateTime? Updated_On { get; set; }

        public string? Source_table_name { get; set; }

		public string? EmailTriggerDays { get; set; }


	}
}
