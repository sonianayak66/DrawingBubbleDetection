using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPCRS.Models;
using static MPCRS.Utilities.Constants;
using Dapper;
//using Newtonsoft.Json;
using System.Security.Claims;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using MPCRS.Services;


namespace MPCRS.Controllers.API
{

    [Authorize]
    public class TaskPlannerController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;

        public TaskPlannerController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

        [ClaimRequirement(UserPermissions.TaskPlanner_View)]
        [Route("/v3modules")]
        public IActionResult Index()
        {
            return PhysicalFile(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "v3modules", "index.html"),
                "text/html"
            );
        }

    


        [HttpGet]
        [Route("api/taskplanner/permissions")]
        public IActionResult GetPermissions()
        {
            try
            {
                var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                // Get user information
                var user = _dbContext.Users.FirstOrDefault(u => u.UserDbkey == currentUserDbkey);

                // Get permissions
                var permissions = UserData.GetUserPermissions(User);
                var taskPlannerPermissions = permissions.Where(x => x.ClaimValue.StartsWith("TaskPlanner_")
                                  || x.ClaimValue.StartsWith("ION_"));


                return Ok(new
                {
                    user = new
                    {
                        userDbkey = user?.UserDbkey,
                        userName = user?.UserName,
                        email = user?.Email
                    },
                    permissions = taskPlannerPermissions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [HttpGet]
        [Route("api/taskplanner/getUsers")]
        public async Task<IActionResult> getUsers()
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var users = await connection.QueryAsync<dynamic>(
                    "sp_TaskManager_GetUsersList",
                     commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(users);

                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #region Projects

        [HttpGet]
        [Route("api/taskplanner/projects")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Projects_Read)]
        public async Task<IActionResult> GetProjects(Guid? projectGuid = null)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var projects = await connection.QueryAsync<dynamic>(
                    "sp_TaskManager_Projects_Get",
                    new { ProjectGUID = projectGuid });

                    return Ok(projects);

                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }



        [HttpPost]
        [Route("api/taskplanner/projects/save")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Projects_Write)]
        public async Task<IActionResult> SaveProject([FromBody] JsonElement projectData)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    Console.WriteLine($"Incoming data: {projectData}");

                    // Convert to dictionary for manipulation
                    var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(projectData.GetRawText());

                    // Check if ProjectGUID is null
                    bool isNewProject = !dataDict.ContainsKey("ProjectGUID") ||
                                       dataDict["ProjectGUID"] == null ||
                                       dataDict["ProjectGUID"].ToString() == "";

                    if (isNewProject)
                    {
                        dataDict["ProjectGUID"] = Guid.NewGuid();
                        dataDict["CreatedBy"] = currentUserDbkey;
                    }
                    else
                    {
                        dataDict["UpdatedBy"] = currentUserDbkey;
                    }

                    var jsonData = JsonSerializer.Serialize(dataDict);
                    Console.WriteLine($"Final JSON: {jsonData}");

                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "dbo.sp_TaskManager_Projects_Save",
                        new { JsonData = jsonData },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/taskplanner/projects/delete")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Projects_Delete)]
        public async Task<IActionResult> DeleteProject([FromBody] object deleteData)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        JsonSerializer.Serialize(deleteData));

                    dataDict["UpdatedBy"] = currentUserDbkey; // Just add user ID

                    var jsonData = JsonSerializer.Serialize(dataDict);
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "dbo.sp_TaskManager_Projects_Delete",
                        new { JsonData = jsonData });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }



        #endregion

        #region ProjectBuckets

        [HttpGet]
        [Route("api/taskplanner/buckets")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Projects_Read)]
        public async Task<IActionResult> GetGlobalBuckets(Guid? bucketGuid = null)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var buckets = await connection.QueryAsync<dynamic>(
                        "sp_TaskManager_GlobalBuckets_Get",
                        new { BucketGUID = bucketGuid },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(buckets);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [HttpPost]
        [Route("api/taskplanner/buckets/save")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Projects_Write)]
        public async Task<IActionResult> SaveGlobalBucket([FromBody] JsonElement bucketData)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(bucketData.GetRawText());

                    // Remove ProjectGUID if it exists (no longer needed)
                    if (dataDict.ContainsKey("ProjectGUID"))
                    {
                        dataDict.Remove("ProjectGUID");
                    }

                    // Override only the secure fields
                    if (dataDict["BucketGUID"] == null)
                    {
                        dataDict["BucketGUID"] = Guid.NewGuid();
                        dataDict["CreatedBy"] = currentUserDbkey;
                    }
                    else
                    {
                        dataDict["UpdatedBy"] = currentUserDbkey;
                    }

                    var jsonData = JsonSerializer.Serialize(dataDict);

                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "dbo.sp_TaskManager_GlobalBuckets_Save",
                        new { JsonData = jsonData },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [HttpPost]
        [Route("api/taskplanner/buckets/delete")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Projects_Delete)]
        public async Task<IActionResult> DeleteGlobalBucket([FromBody] JsonElement deleteData)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        JsonSerializer.Serialize(deleteData));

                    dataDict["UpdatedBy"] = currentUserDbkey;

                    var jsonData = JsonSerializer.Serialize(dataDict);
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "dbo.sp_TaskManager_GlobalBuckets_Delete",
                        new { JsonData = jsonData });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
         

        #endregion


        #region Tasks

        [HttpGet]
        [Route("api/taskplanner/tasks")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Tasks_Read)]
        public async Task<IActionResult> GetTasks(Guid? projectGuid = null, Guid? bucketGuid = null, Guid? taskGuid = null)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    // Determine user's permission level
                    string userPermissionLevel = "TaskPlanner_TaskList_UserTasks"; // Default

                    if (UserData.IsAuthorized(User, UserPermissions.TaskPlanner_TaskList_AllTasks))
                    {
                        userPermissionLevel = "TaskPlanner_TaskList_AllTasks";
                    }
                    else if (UserData.IsAuthorized(User, UserPermissions.TaskPlanner_TaskList_UserTasks))
                    {
                        userPermissionLevel = "TaskPlanner_TaskList_UserTasks";
                    }

                    var tasks = await connection.QueryAsync<dynamic>(
                        "sp_TaskManager_Tasks_Get",
                        new
                        {
                            ProjectGUID = projectGuid,
                            BucketGUID = bucketGuid,
                            TaskGUID = taskGuid,
                            CurrentUserDbkey = currentUserDbkey,
                            UserPermissionLevel = userPermissionLevel
                        },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(tasks);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [HttpPost]
        [Route("api/taskplanner/tasks/save")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Tasks_Write)]
        public async Task<IActionResult> SaveTask([FromBody] JsonElement taskData)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        JsonSerializer.Serialize(taskData));

                    // Handle IsPrivate field
                    bool isPrivate = false;
                    if (dataDict.ContainsKey("IsPrivate") && dataDict["IsPrivate"] != null)
                    {
                        bool.TryParse(dataDict["IsPrivate"].ToString(), out isPrivate);
                    }
                    dataDict["IsPrivate"] = isPrivate;
                     

                    // Override only the secure fields
                    if (dataDict["TaskGUID"] == null)
                    {
                        dataDict["TaskGUID"] = Guid.NewGuid();
                        dataDict["CreatedBy"] = currentUserDbkey;
                    }
                    else
                    {
                        dataDict["UpdatedBy"] = currentUserDbkey;


                        if (isPrivate)
                        {
                            // If making task private, remove all assignments
                            using (var assignmentConnection = mPDapperContext.CreateConnection())
                            {
                                await assignmentConnection.ExecuteAsync(
                                    "UPDATE TaskManager_TaskAssignments SET IsDeleted = 1, [AssignedBy] = @UpdatedBy, [AssignedDate] = GETDATE() WHERE TaskGUID = CAST(@TaskGUID AS UNIQUEIDENTIFIER) AND IsDeleted = 0",
                                    new
                                    {
                                        TaskGUID = dataDict["TaskGUID"].ToString(),
                                        UpdatedBy = currentUserDbkey
                                    }
                                );
                            }
                        }
                    }

                    var jsonData = JsonSerializer.Serialize(dataDict);
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "dbo.sp_TaskManager_Tasks_Save",
                        new { JsonData = jsonData },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/taskplanner/tasks/delete")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Tasks_Delete)]
        public async Task<IActionResult> DeleteTask([FromBody] object deleteData)
        {
            try
            {


                var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(deleteData));

                dataDict["UpdatedBy"] = currentUserDbkey;

                var jsonData = JsonSerializer.Serialize(dataDict);

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                                        "sp_TaskManager_Tasks_Delete",
                                        new { JsonData = jsonData });

                    return Ok(result);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Task Assignments

        [HttpGet]
        [Route("api/taskplanner/assignments")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Tasks_Read)]
        public async Task<IActionResult> GetTaskAssignments(Guid? taskGuid = null, int? assignedUserDbkey = null)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var assignments = await connection.QueryAsync<dynamic>(
                 "sp_TaskManager_TaskAssignments_Get",
                 new { TaskGUID = taskGuid, AssignedUserDbkey = assignedUserDbkey }, commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(assignments);

                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [HttpPost]
        [Route("api/taskplanner/assignments/save")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Tasks_Assign)]
        public async Task<IActionResult> SaveTaskAssignment([FromBody] JsonElement assignmentData)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        JsonSerializer.Serialize(assignmentData));

                    // For assignments, always set AssignedBy to current user
                    dataDict["AssignedBy"] = currentUserDbkey;

                    var jsonData = JsonSerializer.Serialize(dataDict);
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "dbo.sp_TaskManager_TaskAssignments_Save",
                        new { JsonData = jsonData }, commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }



        [HttpPost]
        [Route("api/taskplanner/assignments/delete")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Tasks_Assign)]
        public async Task<IActionResult> DeleteTaskAssignment([FromBody] JsonElement deleteData)
        {
            try
            {
                var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(deleteData));

                dataDict["UpdatedBy"] = currentUserDbkey;

                var jsonData = JsonSerializer.Serialize(dataDict);
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                   "sp_TaskManager_TaskAssignments_Delete",
                   new { JsonData = jsonData }, commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(result);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Task Comments

        [HttpGet]
        [Route("api/taskplanner/comments")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Comments_Read)]
        public async Task<IActionResult> GetTaskComments(Guid? taskGuid = null, Guid? commentGuid = null)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var comments = await connection.QueryAsync<dynamic>(
                    "sp_TaskManager_TaskComments_Get",
                    new { TaskGUID = taskGuid, CommentGUID = commentGuid }, commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(comments);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/taskplanner/comments/save")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Comments_Write)]
        public async Task<IActionResult> SaveTaskComment([FromBody] JsonElement commentData)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        JsonSerializer.Serialize(commentData));

                    // Override only the secure fields
                    if (dataDict["CommentGUID"] == null)
                    {
                        dataDict["CommentGUID"] = Guid.NewGuid();
                        dataDict["CreatedBy"] = currentUserDbkey;
                    }
                    else
                    {
                        dataDict["UpdatedBy"] = currentUserDbkey;
                    }

                    var jsonData = JsonSerializer.Serialize(dataDict);
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "dbo.sp_TaskManager_TaskComments_Save",
                        new { JsonData = jsonData }, commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [HttpPost]
        [Route("api/taskplanner/comments/delete")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Comments_Delete)]
        public async Task<IActionResult> DeleteTaskComment([FromBody] JsonElement deleteData)
        {
            try
            {
                var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(deleteData));

                dataDict["UpdatedBy"] = currentUserDbkey;

                var jsonData = JsonSerializer.Serialize(dataDict);
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                                        "sp_TaskManager_TaskComments_Delete",
                                        new { JsonData = jsonData }, commandType: System.Data.CommandType.StoredProcedure);
                    return Ok(result);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Task Checklists

        [HttpGet]
        [Route("api/taskplanner/checklists")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Tasks_Read)]
        public async Task<IActionResult> GetTaskChecklists(Guid? taskGuid = null, Guid? checklistGuid = null)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var checklists = await connection.QueryAsync<dynamic>(
                    "sp_TaskManager_TaskChecklists_Get",
                    new { TaskGUID = taskGuid, ChecklistGUID = checklistGuid }, commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(checklists);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/taskplanner/checklists/save")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Tasks_Write)]
        public async Task<IActionResult> SaveTaskChecklist([FromBody] JsonElement checklistData)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        JsonSerializer.Serialize(checklistData));

                    // Override only the secure fields
                    if (dataDict["ChecklistGUID"] == null)
                    {
                        dataDict["ChecklistGUID"] = Guid.NewGuid();
                        dataDict["CreatedBy"] = currentUserDbkey;
                    }

                    // For checklists, if completed, set CompletedBy
                    if (dataDict.ContainsKey("IsCompleted") && dataDict["IsCompleted"].ToString().ToLower() == "true")
                    {
                        dataDict["CompletedBy"] = currentUserDbkey;
                    }

                    var jsonData = JsonSerializer.Serialize(dataDict);
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "dbo.sp_TaskManager_TaskChecklists_Save",
                        new { JsonData = jsonData }, commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }



        [HttpPost]
        [Route("api/taskplanner/checklists/delete")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Tasks_Write)]
        public async Task<IActionResult> DeleteTaskChecklist([FromBody] JsonElement deleteData)
        {
            try
            {
                var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(deleteData));

                dataDict["UpdatedBy"] = currentUserDbkey;

                var jsonData = JsonSerializer.Serialize(dataDict);
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                   "sp_TaskManager_TaskChecklists_Delete",
                   new { JsonData = jsonData }, commandType: System.Data.CommandType.StoredProcedure);
                    return Ok(result);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Task Activities

        [HttpGet]
        [Route("api/taskplanner/activities")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Tasks_Read)]
        public async Task<IActionResult> GetTaskActivities(Guid? taskGuid = null, Guid? activityGuid = null)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var activities = await connection.QueryAsync<dynamic>(
                        "sp_TaskManager_TaskActivityLog_Get",
                        new { TaskGUID = taskGuid, ActivityGUID = activityGuid },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(activities);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/taskplanner/activities/save")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Tasks_Write)]
        public async Task<IActionResult> SaveTaskActivity([FromBody] JsonElement activityData)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    // Parse the JSON data with proper type handling
                    var taskGuid = activityData.TryGetProperty("TaskGUID", out var taskGuidProp)
                        ? Guid.Parse(taskGuidProp.GetString())
                        : throw new ArgumentException("TaskGUID is required");

                    var activityType = activityData.TryGetProperty("ActivityType", out var activityTypeProp)
                        ? activityTypeProp.GetString()
                        : throw new ArgumentException("ActivityType is required");

                    var fieldName = activityData.TryGetProperty("FieldName", out var fieldNameProp)
                        ? fieldNameProp.GetString()
                        : null;

                    // Enhanced value parsing to handle different data types
                    var oldValue = GetJsonElementAsString(activityData, "OldValue");
                    var newValue = GetJsonElementAsString(activityData, "NewValue");

                    var description = activityData.TryGetProperty("Description", out var descriptionProp)
                        ? descriptionProp.GetString()
                        : null;

                    var targetName = activityData.TryGetProperty("TargetName", out var targetNameProp)
                        ? targetNameProp.GetString()
                        : null;

                    // Create the JSON data for the stored procedure
                    var jsonData = JsonSerializer.Serialize(new
                    {
                        ActivityGUID = (Guid?)null, // Let stored procedure generate
                        TaskGUID = taskGuid,
                        ActivityType = activityType,
                        FieldName = fieldName,
                        OldValue = oldValue,
                        NewValue = newValue,
                        Description = description,
                        TargetName = targetName,
                        CreatedBy = currentUserDbkey
                    });

                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_TaskManager_TaskActivityLog_Save",
                        new { JsonData = jsonData },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Helper method to safely convert JsonElement to string regardless of type
        private string GetJsonElementAsString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var prop))
                return null;

            switch (prop.ValueKind)
            {
                case JsonValueKind.String:
                    return prop.GetString();

                case JsonValueKind.Number:
                    return prop.GetDecimal().ToString();

                case JsonValueKind.True:
                    return "true";

                case JsonValueKind.False:
                    return "false";

                case JsonValueKind.Null:
                    return null;

                case JsonValueKind.Undefined:
                    return null;

                default:
                    // For objects, arrays, etc., serialize to string
                    return prop.GetRawText();
            }
        }

        #endregion


        #region Email Integration

        [HttpGet]
        [Route("api/taskplanner/emails")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Emails_Read)]
        public async Task<IActionResult> GetEmails(
            Guid? emailGuid = null,
            bool? isConverted = null,
            Guid? convertedTaskGuid = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageSize = 50,
            int pageNumber = 1)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var emails = await connection.QueryMultipleAsync(
                        "sp_TaskManager_Emails_Get",
                        new
                        {
                            EmailGUID = emailGuid,
                            IsConverted = isConverted,
                            ConvertedTaskGUID = convertedTaskGuid,
                            FromDate = fromDate,
                            ToDate = toDate,
                            PageSize = pageSize,
                            PageNumber = pageNumber
                        },
                        commandType: System.Data.CommandType.StoredProcedure);

                    var emailList = await emails.ReadAsync<dynamic>();
                    var totalCount = await emails.ReadFirstAsync<dynamic>();

                    return Ok(new
                    {
                        Data = emailList,
                        TotalCount = totalCount.TotalCount,
                        PageSize = pageSize,
                        PageNumber = pageNumber
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }



        [HttpPost]
        [Route("api/taskplanner/emails/process-relationships")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Admin)]
        public async Task<IActionResult> ProcessEmailRelationships()
        {
            try
            {
                var emailService = HttpContext.RequestServices.GetRequiredService<IEmailService>();

                // Make ProcessRecentEmailsForRelationships public in EmailService
                await ((EmailService)emailService).ProcessRecentEmailsForRelationships();

                return Ok(new { success = true, message = "Email relationships processed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }


        [HttpPost]
        [Route("api/taskplanner/emails/convert")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Tasks_Write)]
        public async Task<IActionResult> ConvertEmailToTask([FromBody] JsonElement conversionData)
        {
            try
            {
                var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var emailGuid = conversionData.GetProperty("EmailGUID").GetGuid();

                // Check if TaskGUID is provided (linking existing task) or create new task
                Guid? existingTaskGuid = null;
                if (conversionData.TryGetProperty("TaskGUID", out var taskGuidElement))
                {
                    existingTaskGuid = taskGuidElement.GetGuid();
                }

                using (var connection = mPDapperContext.CreateConnection())
                {
                    dynamic result;

                    if (existingTaskGuid.HasValue)
                    {
                        // Link email to existing task
                        result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                            "UPDATE TaskManager_Emails SET IsConverted = 1, ConvertedTaskGUID = @TaskGUID, ConvertedBy = @ConvertedBy, ConvertedDate = GETDATE() WHERE EmailGUID = @EmailGUID; SELECT 'SUCCESS' AS Result, @TaskGUID AS TaskGUID;",
                            new
                            {
                                EmailGUID = emailGuid,
                                TaskGUID = existingTaskGuid.Value,
                                ConvertedBy = currentUserDbkey
                            });

                        // **NEW: Create the "Converted" relationship for linked emails too**
                        await connection.ExecuteAsync(@"
                    INSERT INTO TaskManager_EmailTaskRelationships 
                    (EmailGUID, TaskGUID, RelationshipType, ConfidenceScore, IsConfirmed, CreatedBy, CreatedDate)
                    VALUES (@EmailGUID, @TaskGUID, 'Converted', 1.0, 1, @CreatedBy, GETDATE())",
                            new
                            {
                                EmailGUID = emailGuid,
                                TaskGUID = existingTaskGuid.Value,
                                CreatedBy = currentUserDbkey
                            });
                    }
                    else
                    {
                        // Use existing stored procedure to create new task (now includes relationship creation)
                        var taskDataJson = JsonSerializer.Serialize(conversionData.GetProperty("TaskData"));
                        result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                            "sp_TaskManager_Emails_ConvertToTask",
                            new
                            {
                                EmailGUID = emailGuid,
                                TaskData = taskDataJson,
                                ConvertedBy = currentUserDbkey
                            },
                            commandType: System.Data.CommandType.StoredProcedure);
                    }

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Email Configuration

        [HttpGet]
        [Route("api/taskplanner/email-configs")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Admin)] // Assuming admin-only access
        public async Task<IActionResult> GetEmailConfigurations(Guid? configGuid = null, bool? isActive = null)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var configs = await connection.QueryAsync<dynamic>(
                        "sp_TaskManager_EmailConfigurations_Get",
                        new { ConfigGUID = configGuid, IsActive = isActive },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(configs);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/taskplanner/email-configs/save")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Admin)] // Assuming admin-only access
        public async Task<IActionResult> SaveEmailConfiguration([FromBody] JsonElement configData)
        {
            try
            {
                var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(configData));

                // Override only the secure fields
                if (dataDict["ConfigGUID"] == null || dataDict["ConfigGUID"].ToString() == "00000000-0000-0000-0000-000000000000")
                {
                    dataDict["CreatedBy"] = currentUserDbkey;
                }
                else
                {
                    dataDict["UpdatedBy"] = currentUserDbkey;
                }

                var jsonData = JsonSerializer.Serialize(dataDict);

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_TaskManager_EmailConfigurations_Save",
                        new { JsonData = jsonData },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }



        [HttpPost]
        [Route("api/taskplanner/email-configs/test")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Admin)]
        public async Task<IActionResult> TestEmailConfiguration([FromBody] JsonElement testData)
        {
            try
            {
                var server = testData.GetProperty("ImapServer").GetString();
                var port = testData.GetProperty("ImapPort").GetInt32();
                var useSSL = testData.GetProperty("UseSSL").GetBoolean();
                var username = testData.GetProperty("Username").GetString();
                var password = testData.GetProperty("Password").GetString();

                var emailService = HttpContext.RequestServices.GetRequiredService<IEmailService>();

                var connectionResult = await emailService.TestConnectionAsync(server, port, useSSL, username, password);

                if (connectionResult)
                {
                    return Ok(new { success = true, message = "Email configuration test successful!" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Email configuration test failed. Please check your settings." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Test failed: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("api/taskplanner/emails/sync-now")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Admin)]
        public async Task<IActionResult> TriggerEmailSync()
        {
            try
            {
                var emailService = HttpContext.RequestServices.GetRequiredService<IEmailService>();
                await emailService.SyncEmailsAsync();

                return Ok(new { success = true, message = "Email sync completed successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Email sync failed: {ex.Message}" });
            }
        }




        #endregion

        #region Email Notifications


        [HttpGet]
        [Route("api/taskplanner/notifications")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Emails_Read)]
        public async Task<IActionResult> GetEmailNotifications(
    string notificationType = null,
    bool? isRead = null,
    bool? isActionRequired = null,
    int pageSize = 50,
    int pageNumber = 1)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var notifications = await connection.QueryAsync<dynamic>(@"
                SELECT 
                    n.NotificationGUID,
                    n.NotificationType,
                    n.Message,
                    n.IsRead,
                    n.IsActionRequired,
                    n.CreatedDate,
                    n.RelatedTaskGUID,
                    e.EmailGUID,
                    e.Subject as EmailSubject,
                    e.FromEmail,
                    e.FromName,
                    e.ReceivedDate,
                    t.TaskTitle as RelatedTaskTitle,
                    t.TaskGUID as TaskGUID
                FROM TaskManager_EmailNotifications n
                INNER JOIN TaskManager_Emails e ON n.EmailGUID = e.EmailGUID
                LEFT JOIN TaskManager_Tasks t ON n.RelatedTaskGUID = t.TaskGUID AND t.IsDeleted = 0
                WHERE n.IsDeleted = 0 AND e.IsDeleted = 0
                    AND (@NotificationType IS NULL OR n.NotificationType = @NotificationType)
                    AND (@IsRead IS NULL OR n.IsRead = @IsRead)
                    AND (@IsActionRequired IS NULL OR n.IsActionRequired = @IsActionRequired)
                ORDER BY n.CreatedDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
                        new
                        {
                            NotificationType = notificationType,
                            IsRead = isRead,
                            IsActionRequired = isActionRequired,
                            Offset = (pageNumber - 1) * pageSize,
                            PageSize = pageSize
                        });

                    var totalCount = await connection.QueryFirstAsync<int>(@"
                SELECT COUNT(*) 
                FROM TaskManager_EmailNotifications n
                INNER JOIN TaskManager_Emails e ON n.EmailGUID = e.EmailGUID
                WHERE n.IsDeleted = 0 AND e.IsDeleted = 0
                    AND (@NotificationType IS NULL OR n.NotificationType = @NotificationType)
                    AND (@IsRead IS NULL OR n.IsRead = @IsRead)
                    AND (@IsActionRequired IS NULL OR n.IsActionRequired = @IsActionRequired)",
                        new
                        {
                            NotificationType = notificationType,
                            IsRead = isRead,
                            IsActionRequired = isActionRequired
                        });

                    return Ok(new
                    {
                        Data = notifications,
                        TotalCount = totalCount,
                        PageSize = pageSize,
                        PageNumber = pageNumber
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }



        [HttpPost]
        [Route("api/taskplanner/notifications/mark-read")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Emails_Read)]
        public async Task<IActionResult> MarkNotificationAsRead([FromBody] JsonElement notificationData)
        {
            try
            {
                var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var notificationGuid = notificationData.GetProperty("NotificationGUID").GetGuid();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    await connection.ExecuteAsync(@"
                UPDATE TaskManager_EmailNotifications 
                SET IsRead = 1, ReadBy = @ReadBy, ReadDate = GETDATE()
                WHERE NotificationGUID = @NotificationGUID AND IsDeleted = 0",
                        new
                        {
                            NotificationGUID = notificationGuid,
                            ReadBy = currentUserDbkey
                        });

                    return Ok(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/taskplanner/notifications/summary")]
        [ClaimRequirement(UserPermissions.TaskPlanner_Emails_Read)]
        public async Task<IActionResult> GetNotificationSummary()
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var summary = await connection.QueryFirstAsync<dynamic>(@"
                SELECT 
                    COUNT(*) as TotalNotifications,
                    SUM(CASE WHEN IsRead = 0 THEN 1 ELSE 0 END) as UnreadCount,
                    SUM(CASE WHEN IsActionRequired = 1 AND IsRead = 1 THEN 1 ELSE 0 END) as ActionRequiredCount,
                    SUM(CASE WHEN NotificationType = 'NewEmail' THEN 1 ELSE 0 END) as NewEmailCount,
                    SUM(CASE WHEN NotificationType = 'RelatedEmail' THEN 1 ELSE 0 END) as RelatedEmailCount
                FROM TaskManager_EmailNotifications 
                WHERE IsDeleted = 0
                  AND (
                    IsRead = 0 
                    OR (IsActionRequired = 1 AND IsRead = 1)
                    OR NOT EXISTS (
                        SELECT 1 FROM TaskManager_EmailTaskRelationships r 
                        WHERE r.EmailGUID = TaskManager_EmailNotifications.EmailGUID 
                        AND r.IsConfirmed = 1 AND r.IsDeleted = 0
                    )
                  )");

                    return Ok(summary);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

    }
}


