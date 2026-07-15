using System.ComponentModel.DataAnnotations;
using static MPCRS.Utilities.Constants;

namespace MPCRS.ViewModels
{
    public class AspNetRolesVM
    {

        public string Id { get; set; } = VTSDataRecordType.New.ToString();
        [Required]
        public string? Name { get; set; }

        public string? NormalizedName { get; set; }

        public string? ConcurrencyStamp { get; set; }

        public string? ClonePermissionsFromRole { get; set; }
    }

}
