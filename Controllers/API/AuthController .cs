//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using MPCRS.Utilities;
//using System.Security.Claims;

//namespace MPCRS.Controllers.API
//{
//   [Authorize]
//    [ApiController]
//    [Route("api/[controller]/[action]")]
//    public class AuthController : Controller
//    {
     
//        [HttpGet]
//      //  [AllowAnonymous]
//        public IActionResult userPermissions()
//        {
//            var userGuid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            //var hasAdminPermission = User.HasClaim("Permission", "Task_Manager_Admin");
//            //var hasMemberPermission = User.HasClaim("Permission", "Task_Manager_Member");
//             var hasAdminPermission = UserData.IsAuthorized(User, Constants.UserPermissions.Task_Manager_Admin);
//            var hasMemberPermission = UserData.IsAuthorized(User, Constants.UserPermissions.Task_Manager_Memeber);

//            return Ok(new
//            {
//                success = true,
//                data = new
//                {
//                    userGuid,
//                    permissions = new
//                    {
//                        isAdmin = hasAdminPermission,
//                        isMember = hasMemberPermission
//                    }
//                }
//            });
//        }
//    }
//}
