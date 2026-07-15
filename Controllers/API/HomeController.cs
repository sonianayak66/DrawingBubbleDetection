//using Dapper;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using MPCRS.Models;
//using MPCRS.Utilities;
//using MPCRS.ViewModels;
//using System.Data;
//using System.Security.Claims;

//namespace MPCRS.Controllers.API
//{

//    [Authorize]
//    [ApiController]
//    [Route("api/[controller]/[action]")]
//    public class HomeController : ControllerBase
//    {
//        private readonly IConfiguration _configuration;
//        private readonly string _connectionString;
//        private readonly DESI_STFE_PRODContext _dbContext;

//        public HomeController(IConfiguration configuration, DESI_STFE_PRODContext context)
//        {
//            _dbContext = context;
//            _configuration = configuration;
//            _connectionString = configuration.GetConnectionString("MPCRS");
//        }

//        [HttpGet]
//        public async Task<ApiResponse> GetTaskCounts()
//        {
//            try
//            {
//                var currentUserGuid = GetCurrentUserGuid();
//                var isAdmin = UserData.IsAuthorized(User, Constants.UserPermissions.Task_Manager_Admin);

//                For admin pass 'All', for team members pass their UserGuid

//               var userFilter = isAdmin ? "All" : currentUserGuid;

//                using (var connection = new SqlConnection(_connectionString))
//                    {
//                        var taskCounts = await connection.QueryAsync<TaskCountDto>(
//                            "sp_TM_GetTaskCounts",
//                            new { UserFilter = userFilter },
//                            commandType: CommandType.StoredProcedure
//                        );

//                        return new ApiResponse
//                        {
//                            Success = true,
//                            Data = taskCounts
//                        };
//                    }
//            }
//            catch (Exception ex)
//            {
//                return new ApiResponse
//                {
//                    Success = false,
//                    Message = "Failed to retrieve task counts",
//                    Data = null
//                };
//            }
//        }

//        private string GetCurrentUserGuid()
//        {
//            Adjust this based on how you store user info in claims
//            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value.ToString();
//        }
//        private String GetCurrentUserName()
//        {
//            var user = @User.FindFirst(System.Security.Claims.ClaimTypes.GivenName).Value;
//            return user ?? string.Empty;
//        }
//        public class TaskCountDto
//        {
//            public string StatusName { get; set; }
//            public string ColorCode { get; set; }
//            public int TaskCount { get; set; }
//            public string IconName { get; set; }
//        }

//    }
//}
