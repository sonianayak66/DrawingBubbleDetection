using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using System.Data;
using Dapper;
using Newtonsoft.Json;
using System.Security.Claims;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Controllers
{
    [Authorize]
    public class DataEntryTrackingController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;

        public DataEntryTrackingController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

        private int GetCurrentUserDbKey()
        {
            var sid = User.FindFirst(ClaimTypes.Sid)?.Value;
            return int.TryParse(sid, out int key) ? key : 0;
        }

        #region Dashboard

        [ClaimRequirement(UserPermissions.DataEntry_Tracking_Read)]
        public IActionResult Dashboard(int? configId = null, string statusFilter = null)
        {
            var model = new DataEntryTrackingDashboardVM();
            model.ActiveConfigId = configId;
            model.ActiveFilter = statusFilter;

            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var parms = new DynamicParameters();
                    parms.Add("@ConfigId", configId);
                    parms.Add("@StatusFilter", statusFilter);

                    var result = connection.QueryMultiple(
                        "[dbo].[DataEntry_GetTrackingDashboard]",
                        parms,
                        commandType: CommandType.StoredProcedure
                    );

                    model.Summary = result.Read<DataEntryTrackingSummaryVM>().ToList();
                    model.Details = result.Read<DataEntryTrackingDetailVM>().ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }

            return View(model);
        }

        // AJAX endpoint — returns filtered detail data as JSON
        [ClaimRequirement(UserPermissions.DataEntry_Tracking_Read)]
        [HttpGet]
        public IActionResult GetDashboardData(int? configId = null, string statusFilter = null)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var parms = new DynamicParameters();
                    parms.Add("@ConfigId", configId);
                    parms.Add("@StatusFilter", statusFilter);

                    var result = connection.QueryMultiple(
                        "[dbo].[DataEntry_GetTrackingDashboard]",
                        parms,
                        commandType: CommandType.StoredProcedure
                    );

                    var summary = result.Read<DataEntryTrackingSummaryVM>().ToList();
                    var details = result.Read<DataEntryTrackingDetailVM>().ToList();

                    return Json(new { success = true, summary = summary, details = details });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        #endregion

        #region Remarks

        // Save or update a remark from the dashboard
        [ClaimRequirement(UserPermissions.DataEntry_Tracking_Read)]
        [HttpPost]
        public IActionResult SaveRemark([FromBody] DataEntryRemarkPostVM model)
        {
            try
            {
                int userDbKey = GetCurrentUserDbKey();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            if (model.RemarkId.HasValue && model.RemarkId.Value > 0)
                            {
                                // UPDATE existing remark

                                // Fetch old values for audit
                                var oldRemark = connection.QueryFirstOrDefault<DataEntryTrackingDetailVM>(
                                    @"SELECT RemarkText, NoUpdateNeeded FROM [dbo].[DataEntry_Tracking_Remarks] 
                                      WHERE RemarkId = @RemarkId",
                                    new { model.RemarkId },
                                    transaction
                                );

                                // Update remark
                                connection.Execute(
                                    @"UPDATE [dbo].[DataEntry_Tracking_Remarks] 
                                      SET RemarkText = @RemarkText, 
                                          NoUpdateNeeded = @NoUpdateNeeded, 
                                          RemarkBy = @RemarkBy, 
                                          RemarkOn = GETDATE()
                                      WHERE RemarkId = @RemarkId",
                                    new
                                    {
                                        model.RemarkText,
                                        model.NoUpdateNeeded,
                                        RemarkBy = userDbKey,
                                        model.RemarkId
                                    },
                                    transaction
                                );

                                // Audit history
                                string action = "Updated";
                                if (oldRemark != null && oldRemark.IsBlocked != model.NoUpdateNeeded)
                                    action = "Toggled NoUpdate";

                                connection.Execute(
                                    @"INSERT INTO [dbo].[DataEntry_Tracking_History] 
                                      (RemarkId, [Action], OldValue, NewValue, ActionBy, ActionOn)
                                      VALUES (@RemarkId, @Action, @OldValue, @NewValue, @ActionBy, GETDATE())",
                                    new
                                    {
                                        model.RemarkId,
                                        Action = action,
                                        OldValue = oldRemark?.RemarkText,
                                        NewValue = model.RemarkText,
                                        ActionBy = userDbKey
                                    },
                                    transaction
                                );
                            }
                            else
                            {
                                // INSERT new remark
                                var newRemarkId = connection.QuerySingle<int>(
                                    @"INSERT INTO [dbo].[DataEntry_Tracking_Remarks] 
                                      (ConfigId, SourceRecordKey, RemarkText, NoUpdateNeeded, RemarkBy, RemarkOn)
                                      VALUES (@ConfigId, @SourceRecordKey, @RemarkText, @NoUpdateNeeded, @RemarkBy, GETDATE());
                                      SELECT SCOPE_IDENTITY();",
                                    new
                                    {
                                        model.ConfigId,
                                        model.SourceRecordKey,
                                        model.RemarkText,
                                        model.NoUpdateNeeded,
                                        RemarkBy = userDbKey
                                    },
                                    transaction
                                );

                                // Audit history
                                connection.Execute(
                                    @"INSERT INTO [dbo].[DataEntry_Tracking_History] 
                                      (RemarkId, [Action], OldValue, NewValue, ActionBy, ActionOn)
                                      VALUES (@RemarkId, 'Added', NULL, @NewValue, @ActionBy, GETDATE())",
                                    new
                                    {
                                        RemarkId = newRemarkId,
                                        NewValue = model.RemarkText,
                                        ActionBy = userDbKey
                                    },
                                    transaction
                                );
                            }

                            transaction.Commit();
                            return Json(new { success = true, msg = "Remark saved successfully." });
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        // Get remark history for a specific remark
        [ClaimRequirement(UserPermissions.DataEntry_Tracking_Read)]
        [HttpGet]
        public IActionResult GetRemarkHistory(int remarkId)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var history = connection.Query<dynamic>(
                        @"SELECT h.HistoryId, h.[Action], h.OldValue, h.NewValue, 
                                 h.ActionOn, u.UserName AS ActionByName
                          FROM [dbo].[DataEntry_Tracking_History] h
                          LEFT JOIN [dbo].[Users] u ON h.ActionBy = u.UserDbkey
                          WHERE h.RemarkId = @RemarkId
                          ORDER BY h.ActionOn DESC",
                        new { RemarkId = remarkId }
                    ).ToList();

                    return Json(new { success = true, data = history });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        #endregion

        #region Config Management

        [ClaimRequirement(UserPermissions.DataEntry_Tracking_Config)]
        public IActionResult Config()
        {
            var configs = new List<DataEntryTrackingConfigVM>();
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    configs = connection.Query<DataEntryTrackingConfigVM>(
                        @"SELECT * FROM [dbo].[DataEntry_Tracking_Config] ORDER BY ModuleName"
                    ).ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }

            return View(configs);
        }

        [ClaimRequirement(UserPermissions.DataEntry_Tracking_Config)]
        [HttpPost]
        public IActionResult SaveConfig([FromBody] DataEntryTrackingConfigVM model)
        {
            try
            {
                int userDbKey = GetCurrentUserDbKey();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    if (model.ConfigId > 0)
                    {
                        // UPDATE
                        connection.Execute(
                            @"UPDATE [dbo].[DataEntry_Tracking_Config] 
                              SET ModuleName = @ModuleName,
                                  SourceTable = @SourceTable,
                                  PrimaryKeyColumn = @PrimaryKeyColumn,
                                  DisplayColumns = @DisplayColumns,
                                  UpdatedOnColumn = @UpdatedOnColumn,
                                  UpdateFrequencyDays = @UpdateFrequencyDays,
                                  AmberThresholdDays = @AmberThresholdDays,
                                  ExclusionCondition = @ExclusionCondition,
                                  IsActive = @IsActive,
                                  UpdatedBy = @UpdatedBy,
                                  UpdatedOn = GETDATE()
                              WHERE ConfigId = @ConfigId",
                            new
                            {
                                model.ModuleName,
                                model.SourceTable,
                                model.PrimaryKeyColumn,
                                model.DisplayColumns,
                                model.UpdatedOnColumn,
                                model.UpdateFrequencyDays,
                                model.AmberThresholdDays,
                                model.ExclusionCondition,
                                model.IsActive,
                                UpdatedBy = userDbKey,
                                model.ConfigId
                            }
                        );
                    }
                    else
                    {
                        // INSERT
                        connection.Execute(
                            @"INSERT INTO [dbo].[DataEntry_Tracking_Config] 
                              (ModuleName, SourceTable, PrimaryKeyColumn, DisplayColumns,
                               UpdatedOnColumn, UpdateFrequencyDays, AmberThresholdDays,
                               ExclusionCondition, IsActive, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn)
                              VALUES 
                              (@ModuleName, @SourceTable, @PrimaryKeyColumn, @DisplayColumns,
                               @UpdatedOnColumn, @UpdateFrequencyDays, @AmberThresholdDays,
                               @ExclusionCondition, @IsActive, @CreatedBy, GETDATE(), @UpdatedBy, GETDATE())",
                            new
                            {
                                model.ModuleName,
                                model.SourceTable,
                                model.PrimaryKeyColumn,
                                model.DisplayColumns,
                                model.UpdatedOnColumn,
                                model.UpdateFrequencyDays,
                                model.AmberThresholdDays,
                                model.ExclusionCondition,
                                model.IsActive,
                                CreatedBy = userDbKey,
                                UpdatedBy = userDbKey
                            }
                        );
                    }

                    return Json(new { success = true, msg = "Config saved successfully." });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [ClaimRequirement(UserPermissions.DataEntry_Tracking_Config)]
        [HttpPost]
        public IActionResult ToggleConfig(int configId, bool isActive)
        {
            try
            {
                int userDbKey = GetCurrentUserDbKey();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    connection.Execute(
                        @"UPDATE [dbo].[DataEntry_Tracking_Config] 
                          SET IsActive = @IsActive, UpdatedBy = @UpdatedBy, UpdatedOn = GETDATE()
                          WHERE ConfigId = @ConfigId",
                        new { IsActive = isActive, UpdatedBy = userDbKey, ConfigId = configId }
                    );
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        #endregion
    }
}