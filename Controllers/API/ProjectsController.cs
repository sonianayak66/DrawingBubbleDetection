//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System.Data;
//using Dapper;
//using Microsoft.Data.SqlClient;
//using System.Security.Claims;
//using static MPCRS.Utilities.Constants;
//using MPCRS.ViewModels;
//using Microsoft.EntityFrameworkCore;
//using MPCRS.Models;
//using UglyToad.PdfPig.Fonts.TrueType.Names;

//namespace MPCRS.Controllers.API
//{
//    [Authorize] // Requires authentication
//    [ApiController]
//    // [AllowAnonymous]
//    [Route("api/[controller]/[action]")]
//    public class ProjectsController : ControllerBase
//    {
//        private readonly IConfiguration _configuration;
//        private readonly string _connectionString;
//        private readonly DESI_STFE_PRODContext _dbContext;


//        public ProjectsController(IConfiguration configuration, DESI_STFE_PRODContext context)
//        {
//            _dbContext = context;
//            _configuration = configuration;
//            _connectionString = configuration.GetConnectionString("MPCRS");
//        }

//        [HttpGet]
         
//        public async Task<IActionResult> GetProjects()
//        {
//            try
//            {
//                using var connection = new SqlConnection(_connectionString);

//                var projects = await connection.QueryAsync<ProjectViewModel>(
//                    "sp_TM_GetAllProjects",
//                    commandType: CommandType.StoredProcedure
//                );

//                return Ok(new ApiResponse
//                {
//                    Success = true,
//                    Message = "Success",
//                    Data = projects
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

//        [HttpPost]
        
//        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
//        {
//            try
//            {
//                using var connection = new SqlConnection(_connectionString);
//                var parameters = new DynamicParameters();

//                parameters.Add("ProjectName", dto.ProjectName);
//                parameters.Add("ProjectDescription", dto.ProjectDescription);
//                parameters.Add("ProjectCode", dto.ProjectCode);
//                parameters.Add("StartDate", dto.StartDate);
//                parameters.Add("DueDate", dto.DueDate);
//                parameters.Add("CreatedBy", GetCurrentUserGuid());
//                parameters.Add("ProjectGuid", dbType: DbType.Guid, direction: ParameterDirection.Output);
//                parameters.Add("ProjectId", dbType: DbType.Int32, direction: ParameterDirection.Output);
//                parameters.Add("Success", dbType: DbType.Boolean, direction: ParameterDirection.Output);
//                parameters.Add("Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

//                await connection.ExecuteAsync("sp_TM_CreateProject", parameters, commandType: CommandType.StoredProcedure);

//                var success = parameters.Get<bool>("Success");
//                var message = parameters.Get<string>("Message");

//                return Ok(new ApiResponse
//                {
//                    Success = success,
//                    Message = message,
//                    Data = success ? parameters.Get<Guid>("ProjectGuid") : null
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

//        [HttpPut("{projectGuid}")]
       
//        public async Task<IActionResult> UpdateProject(Guid projectGuid, [FromBody] UpdateProjectDto dto)
//        {
//            try
//            {
//                using var connection = new SqlConnection(_connectionString);

//                // Create JSON for update
//                var updateData = Newtonsoft.Json.JsonConvert.SerializeObject(new
//                {
//                    projectName = dto.ProjectName,
//                    projectDescription = dto.ProjectDescription,
//                    projectCode = dto.ProjectCode,
//                    startDate = dto.StartDate,
//                    dueDate = dto.DueDate
//                });

//                var parameters = new DynamicParameters();
//                parameters.Add("ProjectGuid", projectGuid);
//                parameters.Add("UpdateData", updateData);
//                parameters.Add("ModifiedBy", GetCurrentUserGuid());
//                parameters.Add("Result", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);

//                await connection.ExecuteAsync("sp_TM_UpdateProject", parameters, commandType: CommandType.StoredProcedure);

//                var result = parameters.Get<string>("Result");
//                var response = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse>(result);

//                return Ok(new ApiResponse
//                {
//                    Success = response.Success == true,
//                    Message = response.Success == true ? "Project updated successfully" : "Error updating data",
//                    Data = response.Success == true ? projectGuid : null
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
//        public async Task<IActionResult> GetProjectDetails(string projectGuid)
//        {
//            try
//            {
//                using var connection = new SqlConnection(_connectionString);
//                var projectDetails = await connection.QueryFirstOrDefaultAsync<ProjectDetailViewModel>(
//                    "sp_TM_GetProjectDetails",
//                    new { ProjectGuid = projectGuid },
//                    commandType: CommandType.StoredProcedure
//                );

//                if (projectDetails == null)
//                {
//                    return Ok(new ApiResponse
//                    {
//                        Success = false,
//                        Message = "Project not found",
//                        Data = null
//                    });
//                }

//                return Ok(new ApiResponse
//                {
//                    Success = true,
//                    Message = "Success",
//                    Data = projectDetails
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
//        public async Task<IActionResult> GetProjectTasks(string projectGuid)
//        {
//            try
//            {
//                using var connection = new SqlConnection(_connectionString);
//                var tasks = await connection.QueryAsync<ProjectTaskViewModel>(
//                    "sp_TM_GetProjectTasks",
//                    new { ProjectGuid = projectGuid },
//                    commandType: CommandType.StoredProcedure
//                );

//                return Ok(new ApiResponse
//                {
//                    Success = true,
//                    Message = "Success",
//                    Data = tasks
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
//        public async Task<IActionResult> GetTaskManagerUsers()
//        {
//            try
//            {
//                using var connection = new SqlConnection(_connectionString);
//                var users = await connection.QueryAsync<TaskManagerUserViewModel>(
//                    "sp_TM_GetTaskManagerUsers",
//                    commandType: CommandType.StoredProcedure
//                );

//                return Ok(new ApiResponse
//                {
//                    Success = true,
//                    Message = "Success",
//                    Data = users
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
//        public async Task<IActionResult> GetActiveTags()
//        {
//            try
//            {
//                var tags = _dbContext.TM_TaskTags
//                    .Where(t => t.IsActive == true)
//                    .Select(t => new
//                    {
//                        t.TagId,
//                        t.TagGuid,
//                        t.TagName,
//                        t.ColorCode
//                    })
//                    .ToList();

//                return Ok(new ApiResponse
//                {
//                    Success = true,
//                    Message = "Success",
//                    Data = tags
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

//        private string GetCurrentUserGuid()
//        {
//            // Adjust this based on how you store user info in claims
//            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value.ToString();
//        }
//    }
//}

//// DTOs
//// Add these DTOs at the bottom of ProjectsController.cs or in a separate file

//public class ProjectViewModel
//{
//    public int ProjectId { get; set; }
//    public Guid ProjectGuid { get; set; }
//    public string ProjectName { get; set; }
//    public string ProjectDescription { get; set; }
//    public string ProjectCode { get; set; }
//    public DateTime? StartDate { get; set; }
//    public DateTime? DueDate { get; set; }
//    public bool IsActive { get; set; }
//    public int TotalTasks { get; set; }
//    public int CompletedTasks { get; set; }
//    public int MemberCount { get; set; }
//}

//public class ProjectDetailViewModel
//{
//    public int ProjectId { get; set; }
//    public Guid ProjectGuid { get; set; }
//    public string ProjectName { get; set; }
//    public string ProjectDescription { get; set; }
//    public string ProjectCode { get; set; }
//    public DateTime? StartDate { get; set; }
//    public DateTime? DueDate { get; set; }
//    public bool IsActive { get; set; }
//    public int TotalTasks { get; set; }
//    public int TodoTasks { get; set; }
//    public int InProgressTasks { get; set; }
//    public int DoneTasks { get; set; }
//    public int BlockedTasks { get; set; }
//}

//public class ProjectTaskViewModel
//{
//    public int TaskId { get; set; }
//    public Guid TaskGuid { get; set; }
//    public string TaskTitle { get; set; }
//    public string TaskDescription { get; set; }
//    public int Priority { get; set; }
//    public DateTime? DueDate { get; set; }
//    public decimal? EstimatedHours { get; set; }
//    public decimal? ActualHours { get; set; }
//    public DateTime? CompletedDate { get; set; }
//    public string CreatedBy { get; set; }

//    // Status Information
//    public int StatusId { get; set; }
//    public string StatusName { get; set; }
//    public string StatusColor { get; set; }

//    // Assignment Information
//    public string AssignedToGuid { get; set; }
//    public string AssignedToName { get; set; }
//    public DateTime? AssignedDate { get; set; }
//    public string AssignedBy { get; set; }

//    // Tags as JSON string
//    public string Tags { get; set; }
//}
//public class CreateProjectDto
//{
//    public string ProjectName { get; set; }
//    public string ProjectDescription { get; set; }
//    public string ProjectCode { get; set; }
//    public DateTime? StartDate { get; set; }
//    public DateTime? DueDate { get; set; }
//}

//public class TaskManagerUserViewModel
//{
//    public string UserGuid { get; set; }
//    public string UserName { get; set; }
//    public string Email { get; set; }
//    public string Role { get; set; }
//    public bool IsAdmin { get; set; }
//}
//public class UpdateProjectDto
//{
//    public string ProjectName { get; set; }
//    public string ProjectDescription { get; set; }
//    public string ProjectCode { get; set; }
//    public DateTime? StartDate { get; set; }
//    public DateTime? DueDate { get; set; }
//}


