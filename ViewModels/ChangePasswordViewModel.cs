using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
    public class ChangePasswordViewModel
    {
        public string? Id { get; set; }
        public string? Email  { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password),ErrorMessage = "The password and confirmation password do not match.")]
        public string confimationPassword { get; set; }
        public string? ResponseMessage { get; set; }
    }
}
