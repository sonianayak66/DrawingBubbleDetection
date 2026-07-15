//using Dapper;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using MPCRS.Models;
//using MPCRS.Utilities;
//using MPCRS.ViewModels;
//using Newtonsoft.Json;
//using System.Data;
//using System.Security.Claims;

//namespace MPCRS.Controllers.API
//{

//    [Authorize]
//    [ApiController]
//    [Route("api/[controller]/[action]")]
//    public class TasksController : ControllerBase
//    {

//        private readonly IConfiguration _configuration;
//        private readonly string _connectionString;
//        private readonly DESI_STFE_PRODContext _dbContext;


//        public TasksController(IConfiguration configuration, DESI_STFE_PRODContext context)
//        {
//            _dbContext = context;
//            _configuration = configuration;
//            _connectionString = configuration.GetConnectionString("MPCRS");
//        }

//        [HttpPost]
//        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
//        {
//            try
//            {

//                using var connection = new SqlConnection(_connectionString);

//                var parameters = new
//                {
//                    ProjectGuid = request.ProjectGuid,
//                    TaskTitle = request.TaskTitle,
//                    TaskDescription = request.TaskDescription,
//                    Priority = request.Priority,
//                    DueDate = request.DueDate,
//                    EstimatedHours = request.EstimatedHours,
//                    AssignedTo = string.IsNullOrEmpty(request.AssignedTo) ? null : request.AssignedTo,
//                    CreatedBy = GetCurrentUserGuid(),
//                    Tags = request.Tags?.Count > 0 ? JsonConvert.SerializeObject(request.Tags) : null
//                };

//                var result = await connection.ExecuteAsync(
//                    "sp_TM_CreateTask",
//                    parameters,
//                    commandType: CommandType.StoredProcedure
//                );

//                return Ok(new ApiResponse
//                {
//                    Success = true,
//                    Message = "Task created successfully",
//                    Data = result
//                });
//            }
//            catch (Exception ex)
//            {
//                return Ok(new ApiResponse
//                {
//                    Success = false,
//                    Message = ex.Message,
//                    Data = null
//                });
//            }
//        }

//        [HttpPut("{taskGuid}")]
//        public async Task<IActionResult> UpdateTask(string taskGuid, [FromBody] UpdateTaskRequest request)
//        {
//            try
//            {
//                if (!Guid.TryParse(taskGuid, out var taskGuidParsed))
//                {
//                    return Ok(new ApiResponse
//                    {
//                        Success = false,
//                        Message = "Invalid task GUID format",
//                        Data = null
//                    });
//                }



//                using var connection = new SqlConnection(_connectionString);

//                var parameters = new
//                {
//                    TaskGuid = taskGuidParsed,
//                    TaskTitle = request.TaskTitle,
//                    TaskDescription = request.TaskDescription,
//                    Priority = request.Priority,
//                    DueDate = request.DueDate,
//                    EstimatedHours = request.EstimatedHours,
//                    AssignedTo = string.IsNullOrEmpty(request.AssignedTo) ? null : request.AssignedTo,
//                    UpdatedBy = GetCurrentUserGuid(),
//                    Tags = request.Tags?.Count > 0 ? JsonConvert.SerializeObject(request.Tags) : null
//                };

//                var result = await connection.ExecuteAsync(
//                    "sp_TM_UpdateTask",
//                    parameters,
//                    commandType: CommandType.StoredProcedure
//                );

//                return Ok(new ApiResponse
//                {
//                    Success = true,
//                    Message = "Task updated successfully",
//                    Data = result
//                });
//            }
//            catch (Exception ex)
//            {
//                return Ok(new ApiResponse
//                {
//                    Success = false,
//                    Message = ex.Message,
//                    Data = null
//                });
//            }
//        }

//        [HttpGet]
//        public async Task<ApiResponse> GetTasks(string filterType = "User", string filterValue = "All")
//        {
//            try
//            {
//                If filterType is User and filterValue is not explicitly set, determine based on user role
//                if (filterType == "User" && filterValue == "All")
//                {
//                    var isAdmin = UserData.IsAuthorized(User, Constants.UserPermissions.Task_Manager_Admin);
//                    if (!isAdmin)
//                    {
//                        filterValue = GetCurrentUserGuid();
//                    }
//                }

//                using (var connection = new SqlConnection(_connectionString))
//                {
//                    var tasks = await connection.QueryAsync<TaskDto>(
//                        "sp_TM_GetTasks",
//                        new
//                        {
//                            FilterType = filterType,
//                            FilterValue = filterValue
//                        },
//                        commandType: CommandType.StoredProcedure
//                    );

//                    return new ApiResponse
//                    {
//                        Success = true,
//                        Data = tasks.ToList()
//                    };
//                }
//            }
//            catch (Exception ex)
//            {
//                return new ApiResponse
//                {
//                    Success = false,
//                    Message = "Failed to retrieve tasks",
//                    Data = null
//                };
//            }
//        }
//        private string GetCurrentUserGuid()
//        {
//            Adjust this based on how you store user info in claims
//            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value.ToString();
//        }
//    }

//    public class CreateTaskRequest
//    {
//        public Guid ProjectGuid { get; set; }
//        public string TaskTitle { get; set; }
//        public string TaskDescription { get; set; }
//        public int Priority { get; set; } = 2;
//        public DateTime? DueDate { get; set; }
//        public decimal? EstimatedHours { get; set; }
//        public string AssignedTo { get; set; }
//        public List<int> Tags { get; set; } = new List<int>();
//    }

//    public class UpdateTaskRequest
//    {
//        public string TaskTitle { get; set; }
//        public string TaskDescription { get; set; }
//        public int Priority { get; set; } = 2;
//        public DateTime? DueDate { get; set; }
//        public decimal? EstimatedHours { get; set; }
//        public string AssignedTo { get; set; }
//        public List<int> Tags { get; set; } = new List<int>();
//    }

//    public class TaskDto
//    {
//        public int TaskId { get; set; }
//        public Guid TaskGuid { get; set; }
//        public string? TaskTitle { get; set; }
//        public string? TaskDescription { get; set; }
//        public int Priority { get; set; }
//        public DateTime? DueDate { get; set; }
//        public decimal? EstimatedHours { get; set; }
//        public decimal? ActualHours { get; set; }
//        public DateTime? CompletedDate { get; set; }
//        public string? CreatedBy { get; set; }
//        public string? ProjectName { get; set; }
//        public Guid ProjectGuid { get; set; }
//        public string? StatusName { get; set; }
//        public int StatusId { get; set; }
//        public string? StatusColor { get; set; }
//        public string? AssignedTo { get; set; }
//        public DateTime? AssignedDate { get; set; }
//        public string? AssignedBy { get; set; }

//        public string? UserName { get; set; }
//    }
//}

