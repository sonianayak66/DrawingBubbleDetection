using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using static MPCRS.Utilities.Constants;
using System.Security.Claims;
using System.Text.Json;
using Dapper;

namespace MPCRS.Controllers.API
{
    [ApiController]
    [Authorize]
    public class InwardIONController : ControllerBase
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;

        public InwardIONController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

        // ============================================================
        // SAVE (create or update) - one endpoint, decided by GUID
        // ============================================================
        [HttpPost]
        [Route("api/inwardion/save")]
        [ClaimRequirement(UserPermissions.ION_Inward_Create)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> SaveInwardNote([FromBody] InwardIONNoteDto noteData)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    var dataDict = new Dictionary<string, object>
                    {
                        ["InwardNoteId"] = noteData.InwardNoteId,
                        ["InwardIONGUID"] = noteData.InwardIONGUID,
                        ["ReceivedDate"] = noteData.ReceivedDate,
                        ["IONDate"] = noteData.IONDate,
                        ["IONReferenceNumber"] = noteData.IONReferenceNumber,
                        ["FromDepartment"] = noteData.FromDepartment,
                        ["FromPersonNameWithDesignation"] = noteData.FromPersonNameWithDesignation,
                        ["Subject"] = noteData.Subject,
                        ["AddressedTo"] = noteData.AddressedTo,
                        ["CopyTo"] = noteData.CopyTo,
                        ["Remarks"] = noteData.Remarks,
                        ["AcknowledgmentSent"] = noteData.AcknowledgmentSent
                    };

                    bool isNewRecord = string.IsNullOrEmpty(noteData.InwardIONGUID);
                    if (isNewRecord)
                    {
                        dataDict["CreatedBy"] = currentUserDbkey;
                    }
                    else
                    {
                        dataDict["UpdatedBy"] = currentUserDbkey;
                    }

                    var jsonData = JsonSerializer.Serialize(dataDict);
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_InwardNote_Save",
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

        // ============================================================
        // LIST with filters and pagination (50/page via SP)
        // ============================================================
        [HttpPost]
        [Route("api/inwardion/list")]
        [ClaimRequirement(UserPermissions.ION_Inward_View)]
        public async Task<IActionResult> GetInwardNotes([FromBody] InwardIONListRequestDto request)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var dataDict = new Dictionary<string, object>
                    {
                        ["PageNumber"] = request?.PageNumber ?? 1,
                        ["SearchText"] = request?.SearchText,
                        ["FromDepartment"] = request?.FromDepartment,
                        ["AddressedTo"] = request?.AddressedTo,
                        ["DateFrom"] = request?.DateFrom,
                        ["DateTo"] = request?.DateTo
                    };

                    var jsonData = JsonSerializer.Serialize(dataDict);
                    var rows = await connection.QueryAsync<dynamic>(
                        "sp_ION_InwardNote_Get",
                        new { JsonData = jsonData },
                        commandType: System.Data.CommandType.StoredProcedure);

                    return Ok(rows);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================================
        // GET single detail
        // ============================================================
        [HttpPost]
        [Route("api/inwardion/detail")]
        [ClaimRequirement(UserPermissions.ION_Inward_View)]
        public async Task<IActionResult> GetInwardNoteDetail([FromBody] JsonElement requestData)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(requestData);
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_InwardNote_GetDetail",
                        new { JsonData = jsonData },
                        commandType: System.Data.CommandType.StoredProcedure);

                    if (result == null)
                        return NotFound(new { error = "Inward note not found" });

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================================
        // DELETE - physically removes files from disk + DB rows
        // ============================================================
        [HttpPost]
        [Route("api/inwardion/delete")]
        [ClaimRequirement(UserPermissions.ION_Inward_Delete)]
        public async Task<IActionResult> DeleteInwardNote([FromBody] JsonElement deleteData)
        {
            try
            {
                var inwardIONGUID = deleteData.TryGetProperty("InwardIONGUID", out var guidProp)
                    ? guidProp.GetString()
                    : null;

                if (string.IsNullOrEmpty(inwardIONGUID))
                    return BadRequest(new { error = "InwardIONGUID is required" });

                using (var connection = mPDapperContext.CreateConnection())
                {
                    // Step 1: Fetch the note id and all attachment file names before deleting
                    var noteRow = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        @"SELECT InwardNoteId FROM [dbo].[ION_InwardNote] WHERE InwardIONGUID = @Guid",
                        new { Guid = inwardIONGUID });

                    if (noteRow == null)
                        return NotFound(new { error = "Inward note not found" });

                    int inwardNoteId = (int)noteRow.InwardNoteId;

                    var attachments = await connection.QueryAsync<dynamic>(
                        @"SELECT Attachment_FileName FROM [dbo].[Attachments]
                          WHERE Source_table = 'ION_InwardNote' AND Source_table_key = @Id",
                        new { Id = inwardNoteId });

                    // Step 2: Best-effort physical file deletion
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot", "Attachments", "ION_Inward");

                    foreach (var att in attachments)
                    {
                        try
                        {
                            string storedFileName = (string)att.Attachment_FileName;
                            if (!string.IsNullOrEmpty(storedFileName))
                            {
                                var filePath = Path.Combine(uploadDir, storedFileName);
                                if (System.IO.File.Exists(filePath))
                                {
                                    System.IO.File.Delete(filePath);
                                }
                            }
                        }
                        catch
                        {
                            // Best-effort: log would go here; do not block the delete
                        }
                    }

                    // Step 3: Call SP to purge DB rows (note + attachment metadata in a transaction)
                    var jsonData = JsonSerializer.Serialize(new { InwardIONGUID = inwardIONGUID });
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        "sp_ION_InwardNote_Delete",
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

        // ============================================================
        // ATTACHMENTS — upload
        // ============================================================
        [HttpPost]
        [Route("api/inwardion/attachment/upload")]
        [ClaimRequirement(UserPermissions.ION_Inward_Edit)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadInwardAttachment([FromForm] IFormFile file, [FromForm] int inwardNoteId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file uploaded" });

                if (inwardNoteId <= 0)
                    return BadRequest(new { error = "Invalid inwardNoteId" });

                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(),
                    "wwwroot", "Attachments", "ION_Inward");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var attachmentGuid = Guid.NewGuid().ToString();
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var storedFileName = $"{attachmentGuid}{fileExtension}";
                var filePath = Path.Combine(uploadDir, storedFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var currentUserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                    var attachmentId = await connection.QueryFirstOrDefaultAsync<int>(
                        @"INSERT INTO [dbo].[Attachments]
                          (Source_table, Source_table_key, Attachment_location, Attachment_FileName,
                           Orginal_File_Name, AttachmentGUID, Updated_by, Updated_on)
                          VALUES ('ION_InwardNote', @InwardNoteId, @Location, @StoredFileName,
                                  @OriginalFileName, @AttachmentGUID, @UpdatedBy, GETDATE());
                          SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        new
                        {
                            InwardNoteId = inwardNoteId,
                            Location = "/Attachments/ION_Inward/",
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

        // ============================================================
        // ATTACHMENTS — list for a given inward note
        // ============================================================
        [HttpGet]
        [Route("api/inwardion/attachment/list/{inwardNoteId}")]
        [ClaimRequirement(UserPermissions.ION_Inward_View)]
        public async Task<IActionResult> GetInwardAttachments(int inwardNoteId)
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
                          WHERE a.Source_table = 'ION_InwardNote'
                            AND a.Source_table_key = @InwardNoteId
                          ORDER BY a.Updated_on DESC",
                        new { InwardNoteId = inwardNoteId });

                    return Ok(attachments);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================================
        // ATTACHMENTS — download
        // ============================================================
        [HttpGet]
        [Route("api/inwardion/attachment/download/{attachmentId}")]
        [ClaimRequirement(UserPermissions.ION_Inward_View)]
        public async Task<IActionResult> DownloadInwardAttachment(int attachmentId)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var attachment = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        @"SELECT Attachment_location, Attachment_FileName, Orginal_File_Name
                          FROM [dbo].[Attachments]
                          WHERE Attachment_Db_Key = @AttachmentId
                            AND Source_table = 'ION_InwardNote'",
                        new { AttachmentId = attachmentId });

                    if (attachment == null)
                        return NotFound(new { error = "Attachment not found" });

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot", "Attachments", "ION_Inward", (string)attachment.Attachment_FileName);

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

        // ============================================================
        // ATTACHMENTS — delete single attachment (file + DB row)
        // ============================================================
        [HttpPost]
        [Route("api/inwardion/attachment/delete")]
        [ClaimRequirement(UserPermissions.ION_Inward_Edit)]
        public async Task<IActionResult> DeleteInwardAttachment([FromBody] JsonElement deleteData)
        {
            try
            {
                var attachmentId = deleteData.TryGetProperty("AttachmentId", out var idProp)
                    ? idProp.GetInt32()
                    : 0;

                if (attachmentId <= 0)
                    return BadRequest(new { error = "AttachmentId is required" });

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var attachment = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        @"SELECT Attachment_FileName FROM [dbo].[Attachments]
                          WHERE Attachment_Db_Key = @AttachmentId
                            AND Source_table = 'ION_InwardNote'",
                        new { AttachmentId = attachmentId });

                    if (attachment == null)
                        return NotFound(new { error = "Attachment not found" });

                    // Best-effort physical file delete
                    try
                    {
                        string storedFileName = (string)attachment.Attachment_FileName;
                        if (!string.IsNullOrEmpty(storedFileName))
                        {
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(),
                                "wwwroot", "Attachments", "ION_Inward", storedFileName);
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }
                    }
                    catch
                    {
                        // Swallow — DB row still needs to be removed
                    }

                    // Remove DB row
                    await connection.ExecuteAsync(
                        @"DELETE FROM [dbo].[Attachments]
                          WHERE Attachment_Db_Key = @AttachmentId
                            AND Source_table = 'ION_InwardNote'",
                        new { AttachmentId = attachmentId });

                    return Ok(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
