using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MPCRS.ViewModels
{
    public class AuthenticateByEmailViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string? ReturnUrl { get; set; }
        public string? ClientIP { get; set; }
        public string? Browser { get; set; }
        public string? AuthenticateBy { get; set; }
        public string? ResponseMessage { get; set; }
        public bool IsAuthenticated { get; set; }
        public string? UserType { get; set; }
        public bool RememberMe { get; set; } = false;   
    }


    public class AuthenticateUser
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool IsAuthenticated { get; set; }
        public string UserSessionGuid { get; set; }
        public string UserGuid { get; set; }
		public int UserDbkey { get; set; }

	}


    public partial class UserRoles
    {
        public string? RoleGuid { get; set; }

        public string? RoleName { get; set; }
 
    }
}
