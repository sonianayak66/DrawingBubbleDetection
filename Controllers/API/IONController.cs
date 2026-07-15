using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MPCRS.Models;
using MPCRS.Utilities;
using static MPCRS.Utilities.Constants;
using System.Security.Claims;
using System.Text.Json;
using Dapper;
using MPCRS.ViewModels;

namespace MPCRS.Controllers.API
{
   
    [ApiController]
    [Authorize]
    public class IONController : ControllerBase
    {

        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;

        public IONController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

        [ClaimRequirement(UserPermissions.ION_View)]
        [Route("/v3modules/ion")]
        public IActionResult Index()
        {
            return PhysicalFile(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "v3modules", "index.html"),
                "text/html"
            );
        }

        [HttpGet]
        [Route("api/ion/permissions")]
        public IActionResult GetPermissions()
        {
            try
            {
                var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                // Get user information
                var user = _dbContext.Users.FirstOrDefault(u => u.UserDbkey == currentUserDbkey);

                // Get permissions
                var permissions = UserData.GetUserPermissions(User);
                var ionPermissions = permissions.Where(x => x.ClaimValue.StartsWith("ION_"));

                return Ok(new
                {
                    user = new
                    {
                        userDbkey = user?.UserDbkey,
                        userName = user?.UserName,
                        email = user?.Email
                    },
                    permissions = ionPermissions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/ion/users")]
        public async Task<IActionResult> GetUsers()
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

        #region ION Notes CRUD

        [HttpGet]
        [Route("api/ion/notes")]
        [ClaimRequirement(UserPermissions.ION_View)]
        public async Task<IActionResult> GetIONNotes(
            string ionGuid = null,
            string status = null,
            string office = null,
            int? preparedBy = null,
            int? approvedBy = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var notes = await connection.QueryAsync<dynamic>(
                        "sp_ION_Notes_Get",
                        new
                        {
                            IONGUID = ionGuid,
                            Status = status,
                            Office = office,
                            PreparedBy = preparedBy,
                            ApprovedBy = approvedBy,
                            FromDate = fromDate,
                            ToDate = toDate
                        },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(notes);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/ion/notes/save")]
        [ClaimRequirement(UserPermissions.ION_Create)]
        [RequestSizeLimit(52428800)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10_485_760)]
        [DisableRequestSizeLimit] 
        public async Task<IActionResult> SaveIONNote([FromBody] IONNoteDto noteData)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value); 
                    
                    // Convert to dictionary for manipulation
                    var dataDict = new Dictionary<string, object>
                    {
                        ["IONId"] = noteData.IONId,
                        ["IONGUID"] = noteData.IONGUID,
                        ["IONNumber"] = noteData.IONNumber,
                        ["GroupGUID"] = noteData.GroupGUID,
                        ["IONDate"] = noteData.IONDate,
                        ["Subject"] = noteData.Subject,
                        ["CommunicationReference"] = noteData.CommunicationReference,
                        ["IONBody"] = noteData.IONBody, // HTML content allowed
                        ["ToAddress"] = noteData.ToAddress,
                        ["CopyTo"] = noteData.CopyTo,
                        ["PreparedBy"] = noteData.PreparedBy,
                        ["PreparedByDesignation"] = noteData.PreparedByDesignation,
                        ["SentThrough"] = noteData.SentThrough,
                        ["Status"] = noteData.Status,
                        ["ToRecipients"] = noteData.ToRecipients ?? new List<string>(),
                        ["CopyToRecipients"] = noteData.CopyToRecipients ?? new List<string>()
                    };

                    bool isNewRecord = string.IsNullOrEmpty(noteData.IONGUID);

                    if (isNewRecord)
                    {
                        dataDict["CreatedBy"] = currentUserDbkey;
                        if (string.IsNullOrEmpty(noteData.Status))
                        {
                            dataDict["Status"] = "Draft";
                        }
                    }
                    else
                    {
                        dataDict["UpdatedBy"] = currentUserDbkey;
                    }

                    var jsonData = JsonSerializer.Serialize(dataDict);
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_Notes_Save",
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
        [Route("api/ion/notes/delete")]
        [ClaimRequirement(UserPermissions.ION_Delete)]
        public async Task<IActionResult> DeleteIONNote([FromBody] JsonElement deleteData)
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
                        "sp_ION_Notes_Delete",
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
        [Route("api/ion/notes/approve")]
        [ClaimRequirement(UserPermissions.ION_Approve)]
        public async Task<IActionResult> ApproveIONNote([FromBody] JsonElement approvalData)
        {
            try
            {
                var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(approvalData));

                dataDict["ApprovedBy"] = currentUserDbkey;

                var jsonData = JsonSerializer.Serialize(dataDict);
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_Notes_Approve",
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

        [HttpGet]
        [Route("api/ion/notes/detail")]
        [ClaimRequirement(UserPermissions.ION_View)]
        public async Task<IActionResult> GetIONNoteDetail(string ionGuid)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var note = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_Notes_GetDetail",
                        new { IONGUID = ionGuid },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(note);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Configuration

        [HttpGet]
        [Route("api/ion/office-config")]
        [ClaimRequirement(UserPermissions.ION_View)]
        public async Task<IActionResult> GetOfficeConfig()
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var configs = await connection.QueryAsync<dynamic>(
                        "sp_ION_OfficeConfig_Get",
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(configs);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/ion/destinations")]
        [ClaimRequirement(UserPermissions.ION_View)]
        public async Task<IActionResult> GetDestinations(string destinationGuid = null)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var destinations = await connection.QueryAsync<dynamic>(
                        "sp_ION_Destinations_Get",
                        new { DestinationGUID = destinationGuid },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(destinations);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [HttpPost]
        [Route("api/ion/office-config/save")]
        [ClaimRequirement(UserPermissions.ION_Admin)]
        public async Task<IActionResult> SaveOfficeConfig([FromBody] JsonElement configData)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        JsonSerializer.Serialize(configData));

                    // Check if new or update - FIX HERE
                    bool isNew = false;
                    if (dataDict.ContainsKey("ConfigId") && dataDict["ConfigId"] != null)
                    {
                        // Try to parse the ConfigId
                        var configIdValue = dataDict["ConfigId"].ToString();
                        if (int.TryParse(configIdValue, out int configId) && configId > 0)
                        {
                            isNew = false;
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    else
                    {
                        isNew = true;
                    }

                    if (isNew)
                    {
                        dataDict["CreatedBy"] = currentUserDbkey;
                    }
                    else
                    {
                        dataDict["UpdatedBy"] = currentUserDbkey;
                    }

                    var jsonData = JsonSerializer.Serialize(dataDict);
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_OfficeConfig_Save",
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
        [Route("api/ion/office-config/delete")]
        [ClaimRequirement(UserPermissions.ION_Admin)]
        public async Task<IActionResult> DeleteOfficeConfig([FromBody] JsonElement deleteData)
        {
            try
            {
                var configId = deleteData.GetProperty("ConfigId").GetInt32();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_OfficeConfig_Delete",
                        new { ConfigId = configId },
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
        [Route("api/ion/destinations/save")]
        [ClaimRequirement(UserPermissions.ION_Admin)]
        public async Task<IActionResult> SaveDestination([FromBody] JsonElement destinationData)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        JsonSerializer.Serialize(destinationData));

                    // Check if new or update - FIX HERE
                    bool isNew = false;
                    if (dataDict.ContainsKey("DestinationId") && dataDict["DestinationId"] != null)
                    {
                        // Try to parse the DestinationId
                        var destIdValue = dataDict["DestinationId"].ToString();
                        if (int.TryParse(destIdValue, out int destId) && destId > 0)
                        {
                            isNew = false;
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    else
                    {
                        isNew = true;
                    }

                    if (isNew)
                    {
                        dataDict["CreatedBy"] = currentUserDbkey;
                    }
                    else
                    {
                        dataDict["UpdatedBy"] = currentUserDbkey;
                    }

                    var jsonData = JsonSerializer.Serialize(dataDict);
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_Destinations_Save",
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
        [Route("api/ion/destinations/delete")]
        [ClaimRequirement(UserPermissions.ION_Admin)]
        public async Task<IActionResult> DeleteDestination([FromBody] JsonElement deleteData)
        {
            try
            {
                var destinationId = deleteData.GetProperty("DestinationId").GetInt32();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_Destinations_Delete",
                        new { DestinationId = destinationId },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/ion/file-groups")]
        [ClaimRequirement(UserPermissions.ION_View)]
        public async Task<IActionResult> GetFileGroups()
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var groups = await connection.QueryAsync<dynamic>(
                        "sp_ION_FileGroups_GetAll",
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(groups);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/ion/file-groups/save")]
        [ClaimRequirement(UserPermissions.ION_Admin)]
        public async Task<IActionResult> SaveFileGroup([FromBody] JsonElement fileGroupData)
        {
            try
            {
                var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(fileGroupData));

                dataDict["UserId"] = currentUserDbkey;

                var jsonData = JsonSerializer.Serialize(dataDict);

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_FileGroups_Save",
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
        [Route("api/ion/file-groups/toggle-active")]
        [ClaimRequirement(UserPermissions.ION_Admin)]
        public async Task<IActionResult> ToggleFileGroupActive([FromBody] JsonElement toggleData)
        {
            try
            {
                var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(toggleData));

                dataDict["UserId"] = currentUserDbkey;

                var jsonData = JsonSerializer.Serialize(dataDict);

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_FileGroups_ToggleActive",
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

        // =====================================================================
        // ION Templates — shared template library used to pre-fill new ION
        // creation. Admins / SupportOperators manage; everyone with ION_View
        // can list and apply them.
        // =====================================================================

        [HttpGet]
        [Route("api/ion/templates")]
        [ClaimRequirement(UserPermissions.ION_View)]
        public async Task<IActionResult> GetIONTemplates()
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var templates = await connection.QueryAsync<dynamic>(
                        "sp_ION_Templates_GetAll",
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(templates);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/ion/templates/save")]
        [ClaimRequirement(UserPermissions.ION_Admin)]
        public async Task<IActionResult> SaveIONTemplate([FromBody] JsonElement templateData)
        {
            try
            {
                var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(templateData));

                dataDict["UserId"] = currentUserDbkey;

                var jsonData = JsonSerializer.Serialize(dataDict);

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_Templates_Save",
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
        [Route("api/ion/templates/delete")]
        [ClaimRequirement(UserPermissions.ION_Admin)]
        public async Task<IActionResult> DeleteIONTemplate([FromBody] JsonElement deleteData)
        {
            try
            {
                var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var templateGuid = deleteData.GetProperty("TemplateGUID").GetString();

                if (string.IsNullOrEmpty(templateGuid))
                    return BadRequest(new { error = "TemplateGUID is required" });

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_Templates_Delete",
                        new { TemplateGUID = templateGuid, UserId = currentUserDbkey },
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
        [Route("api/ion/templates/increment-use")]
        [ClaimRequirement(UserPermissions.ION_Create)]
        public async Task<IActionResult> IncrementIONTemplateUse([FromBody] JsonElement useData)
        {
            try
            {
                var templateGuid = useData.GetProperty("TemplateGUID").GetString();

                if (string.IsNullOrEmpty(templateGuid))
                    return BadRequest(new { error = "TemplateGUID is required" });

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_Templates_IncrementUse",
                        new { TemplateGUID = templateGuid },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/ion/internal-users")]
        [ClaimRequirement(UserPermissions.ION_View)]
        public async Task<IActionResult> GetInternalUsers()
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var users = await connection.QueryAsync<dynamic>(
                        "sp_ION_GetInternalUsers",
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(users);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/ion/search-demands")]
        [ClaimRequirement(UserPermissions.ION_View)]
        public async Task<IActionResult> SearchDemands(string query = "")
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var demands = await connection.QueryAsync<dynamic>(
                        @"SELECT TOP 15
                            DemandDbKey,
                            MMG_File_No,
                            Demand_No,
                            Item_Description
                          FROM Procurement_Demands
                          WHERE MMG_File_No IS NOT NULL
                            AND MMG_File_No <> ''
                            AND (MMG_File_No LIKE '%' + @Query + '%'
                                 OR Demand_No LIKE '%' + @Query + '%')
                            AND ISNULL(IsActive,1) = 1
                          ORDER BY MMG_File_No",
                        new { Query = query ?? "" });

                    return Ok(demands);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Enclosures

        [HttpGet]
        [Route("api/ion/enclosures")]
        [ClaimRequirement(UserPermissions.ION_View)]
        public async Task<IActionResult> GetEnclosures(string ionGuid = null, string enclosureGuid = null)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var enclosures = await connection.QueryAsync<dynamic>(
                        "sp_ION_Enclosures_Get",
                        new { IONGUID = ionGuid, EnclosureGUID = enclosureGuid },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(enclosures);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/ion/enclosures/save")]
        [ClaimRequirement(UserPermissions.ION_Edit)]
        public async Task<IActionResult> SaveEnclosure([FromBody] JsonElement enclosureData)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        JsonSerializer.Serialize(enclosureData));

                    // For new enclosures, set CreatedBy
                    if (dataDict["EnclosureGUID"] == null)
                    {
                        dataDict["CreatedBy"] = currentUserDbkey;
                    }

                    var jsonData = JsonSerializer.Serialize(dataDict);
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_Enclosures_Save",
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
        [Route("api/ion/enclosures/delete")]
        [ClaimRequirement(UserPermissions.ION_Edit)]
        public async Task<IActionResult> DeleteEnclosure([FromBody] JsonElement deleteData)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(deleteData);
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_Enclosures_Delete",
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
        [Route("api/ion/enclosures/upload")]
        [ClaimRequirement(UserPermissions.ION_Edit)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadEnclosureAttachment([FromForm] IFormFile file, [FromForm] int enclosureId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file uploaded" });

                // Create upload directory
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Attachments", "ION");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                // Generate unique filename
                var attachmentGuid = Guid.NewGuid().ToString();
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var storedFileName = $"{attachmentGuid}{fileExtension}";
                var filePath = Path.Combine(uploadDir, storedFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Insert into Attachments table
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    var attachmentId = await connection.QueryFirstOrDefaultAsync<int>(
                        @"INSERT INTO [dbo].[Attachments]
                          (Source_table, Source_table_key, Attachment_location, Attachment_FileName,
                           Orginal_File_Name, AttachmentGUID, Updated_by, Updated_on)
                          VALUES ('ION_Enclosures', @EnclosureId, @Location, @StoredFileName,
                                  @OriginalFileName, @AttachmentGUID, @UpdatedBy, GETDATE());
                          SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        new
                        {
                            EnclosureId = enclosureId,
                            Location = "/Attachments/ION/",
                            StoredFileName = storedFileName,
                            OriginalFileName = file.FileName,
                            AttachmentGUID = attachmentGuid,
                            UpdatedBy = currentUserDbkey
                        });

                    return Ok(new { success = true, attachmentId, fileName = file.FileName });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/ion/enclosures/attachments/{enclosureId}")]
        [ClaimRequirement(UserPermissions.ION_View)]
        public async Task<IActionResult> GetEnclosureAttachments(int enclosureId)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var attachments = await connection.QueryAsync<dynamic>(
                        @"SELECT a.Attachment_Db_Key, a.Orginal_File_Name, a.Attachment_FileName,
                                 a.Updated_on, u.UserName AS UploadedByName
                          FROM [dbo].[Attachments] a
                          LEFT JOIN [dbo].[Users] u ON u.UserDbkey = a.Updated_by
                          WHERE a.Source_table = 'ION_Enclosures'
                            AND a.Source_table_key = @EnclosureId
                          ORDER BY a.Updated_on DESC",
                        new { EnclosureId = enclosureId });

                    return Ok(attachments);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/ion/enclosures/download/{attachmentId}")]
        [ClaimRequirement(UserPermissions.ION_View)]
        public async Task<IActionResult> DownloadEnclosureAttachment(int attachmentId)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var attachment = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        @"SELECT Attachment_location, Attachment_FileName, Orginal_File_Name
                          FROM [dbo].[Attachments]
                          WHERE Attachment_Db_Key = @AttachmentId",
                        new { AttachmentId = attachmentId });

                    if (attachment == null)
                        return NotFound(new { error = "Attachment not found" });

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot", "Attachments", "ION", (string)attachment.Attachment_FileName);

                    if (!System.IO.File.Exists(filePath))
                        return NotFound(new { error = "File not found on disk" });

                    var memory = new MemoryStream();
                    using (var stream = new FileStream(filePath, FileMode.Open))
                    {
                        await stream.CopyToAsync(memory);
                    }
                    memory.Position = 0;

                    return File(memory, "application/octet-stream", (string)attachment.Orginal_File_Name);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/ion/enclosures/delete-attachment/{attachmentId}")]
        [ClaimRequirement(UserPermissions.ION_Edit)]
        public async Task<IActionResult> DeleteEnclosureAttachment(int attachmentId)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    // Get file info before deleting
                    var attachment = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        @"SELECT Attachment_FileName FROM [dbo].[Attachments]
                          WHERE Attachment_Db_Key = @AttachmentId
                            AND Source_table = 'ION_Enclosures'",
                        new { AttachmentId = attachmentId });

                    if (attachment == null)
                        return NotFound(new { error = "Attachment not found" });

                    // Delete from database
                    await connection.ExecuteAsync(
                        "DELETE FROM [dbo].[Attachments] WHERE Attachment_Db_Key = @AttachmentId",
                        new { AttachmentId = attachmentId });

                    // Delete physical file
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot", "Attachments", "ION", (string)attachment.Attachment_FileName);
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);

                    return Ok(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region ScannedCopy

        [HttpPost]
        [Route("api/ion/notes/upload-scanned-copy")]
        [ClaimRequirement(UserPermissions.ION_SupportOperator)]
        public async Task<IActionResult> UploadScannedCopy([FromForm] IFormFile file, [FromForm] string ionGuid, [FromForm] int? approvedBy = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file uploaded" });

                // Validate file type (PDF only)
                var allowedExtensions = new[] { ".pdf" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { error = "Only PDF files are allowed" });

                // Create upload directory if it doesn't exist
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Attachments", "ION", "ScannedCopies");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                // Generate unique filename
                var fileName = $"{ionGuid}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var fullPath = Path.Combine(uploadDir, fileName);

                // Store relative path in database
                var relativePath = $"/Attachments/ION/ScannedCopies/{fileName}";

                // Save file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Update database - upload scanned copy and mark as approved
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    await connection.ExecuteAsync(
                        @"UPDATE ION_Notes
                  SET ScannedCopyUploaded = 1,
                      ScannedCopyPath = @FilePath,
                      ScannedCopyUploadedBy = @UploadedBy,
                      ScannedCopyUploadedDate = GETDATE(),
                      Status = 'Approved',
                      ApprovedBy = @ApprovedBy,
                      ApprovedDate = GETDATE(),
                      UpdatedBy = @UpdatedBy,
                      UpdatedDate = GETDATE()
                  WHERE IONGUID = @IONGUID",
                        new
                        {
                            FilePath = relativePath,
                            UploadedBy = currentUserDbkey,
                            ApprovedBy = approvedBy ?? currentUserDbkey,
                            UpdatedBy = currentUserDbkey,
                            IONGUID = ionGuid
                        });

                    return Ok(new
                    {
                        success = true,
                        fileName = fileName,
                        message = "Scanned copy uploaded and ION approved successfully"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/ion/notes/download-scanned-copy/{ionGuid}")]
        [ClaimRequirement(UserPermissions.ION_View)]
        public async Task<IActionResult> DownloadScannedCopy(string ionGuid)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var relativePath = await connection.QueryFirstOrDefaultAsync<string>(
                        "SELECT ScannedCopyPath FROM ION_Notes WHERE IONGUID = @IONGUID",
                        new { IONGUID = ionGuid });

                    if (string.IsNullOrEmpty(relativePath))
                        return NotFound(new { error = "Scanned copy not found" });

                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                        relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                    if (!System.IO.File.Exists(fullPath))
                        return NotFound(new { error = "Scanned copy file not found on disk" });

                    var memory = new MemoryStream();
                    using (var stream = new FileStream(fullPath, FileMode.Open))
                    {
                        await stream.CopyToAsync(memory);
                    }
                    memory.Position = 0;

                    var fileName = Path.GetFileName(fullPath);
                    return File(memory, "application/pdf", fileName);
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
