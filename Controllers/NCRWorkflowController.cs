using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using System.Data;
using System.Security.Claims;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Controllers
{
    [Authorize]
    public class NCRWorkflowController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;
        private readonly string _shramLink;
        private readonly string _bccList;

        public NCRWorkflowController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            _shramLink = configuration.GetValue<string>("NCRSettings:SHRAMLink") ?? string.Empty;
            _bccList = configuration.GetValue<string>("NCRSettings:BCCList") ?? string.Empty;
            this.mPDapperContext = mPDapperContext;
        }


        [OrClaimRequirement(UserPermissions.NCR_Assignments_Admin, UserPermissions.NCR_Module_User)]
        public IActionResult AssignedNCRList()
        {
            var Permission_Assignments_Admin = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Assignments_Admin);
            var UserGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();

            using (var connection = mPDapperContext.CreateConnection())
            {
                var result = connection.Query<NCRAssignmentsVM>(
                    "NCRWorkflow_GetAssignedNCRList",
                    new
                    {
                        UserGuid = Permission_Assignments_Admin ? "All" : UserGuid,
                        IsAdmin = Permission_Assignments_Admin ? 1 : 0
                    },
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return View(result);
            }
        }

        [HttpGet]
        public IActionResult AssignModuleToNCR(string NCRGUID)
        {
            try
            {
                // Get NCR details
                var ncr = _dbContext.NonConformanceReports
                    .FirstOrDefault(x => x.NCRGuid == NCRGUID);

                if (ncr == null)
                {
                    return Json(new { success = false, msg = "NCR not found" });
                }

                // Get engine part details
                var enginePart = _dbContext.Engine_Parts_Masters
                    .FirstOrDefault(x => x.Engine_Part_Dbkey == ncr.Engine_Part_Dbkey);

                // Get modules (exclude TAS, STRESS, CHAIR for initial assignment)
                var modules = _dbContext.Master_Generals
                    .Where(x => x.Master_Type == "Module_Responsibility"
                             && x.is_active == 1
                             && !x.Master_Name.ToLower().Contains("tas")
                             && !x.Master_Name.ToLower().Contains("stress")
                             && !x.Master_Name.ToLower().Contains("chair"))
                    .OrderBy(x => x.Master_Name)
                    .ToList();

                var model = new AssignNCRWorkflowVM
                {
                    NCRGuid = NCRGUID,
                    ReferenceNumber = ncr.ReferenceNumber,
                    PartInfo = enginePart != null ? $"{enginePart.Draw_part_no} / {enginePart.Description}" : ""
                };

                ViewBag.Modules = modules;

                return PartialView(model);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetUsersForModule(int moduleId)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var users = connection.Query<UserModuleMappingVM>(  
                        "[dbo].[Get_Users_WorkflowAssignment]",
                        new { ModuleID = moduleId },
                        commandType: CommandType.StoredProcedure
                    ).ToList();

                    return Json(users);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveInitialAssignment([FromBody] SaveInitialAssignmentVM model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.NCRGuid))
                {
                    return Json(new { success = false, message = "Invalid request data" });
                }

                var userDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var userGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                // Call stored procedure
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = connection.QueryFirstOrDefault<SaveAssignmentResultVM>(
                        "[dbo].[NCRWorkflow_SaveInitialAssignment]",
                        new
                        {
                            NCRGuid = model.NCRGuid,
                            ModuleID = model.ModuleID,
                            UserGUIDs = model.UserGUIDs,  // Already comma-separated from JS
                            AssignedBy = userDbkey
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    if (result == null || result.Success == 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = result?.Message ?? "Failed to assign NCR"
                        });
                    }

                    // Get NCR details for email
                    var ncr = await _dbContext.NonConformanceReports
                        .FirstOrDefaultAsync(x => x.NCRGuid == model.NCRGuid);

                    var module = await _dbContext.Master_Generals
                        .FirstOrDefaultAsync(m => m.Master_Dbkey == model.ModuleID);

                    // Send email notifications
                    var userGuids = model.UserGUIDs.Split(',').Select(g => g.Trim()).ToList();
                    var userEmails = _dbContext.AspNetUsers
                        .Where(u => userGuids.Contains(u.Id))
                        .Select(u => u.Email)
                        .Where(email => !string.IsNullOrEmpty(email))
                        .ToArray();

                    var senderEmail = _dbContext.AspNetUsers
                        .FirstOrDefault(u => u.OldUserDbkey == userDbkey)?.Email;

                    if (userEmails.Length > 0 && ncr != null)
                    {
                        //await SendNCRNotification(
                        //    recipientEmails: userEmails,
                        //    senderEmail: senderEmail,
                        //    ncrReferenceNumber: ncr.ReferenceNumber,
                        //    moduleName: module?.Master_Name ?? "Module",
                        //    currentStatus: $"Forwarded To {result.StageName}",
                        //    actionRequired: "Please review and provide your comments",
                        //    assignedBy: userDbkey,
                        //    ncrGuid: model.NCRGuid,
                        //    serialNumbers: null
                        //);
                    }

                    return Json(new
                    {
                        success = true,
                        message = $"NCR successfully assigned to {module?.Master_Name}",
                        workflowGuid = result.WorkflowGuid
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new
                {
                    success = false,
                    message = "An error occurred: " + ex.Message
                });
            }
        }

        [OrClaimRequirement(Constants.UserPermissions.NCR_Assignments_Admin, Constants.UserPermissions.NCR_Module_User)]
        public async Task<IActionResult> AssignmentDetail(string id, string NCRGuid)
        {
            try
            {
                var NCR_Assignments_Admin = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Assignments_Admin);
                var NCR_Module_User = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Module_User);
                var UserGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value.ToString();
                var userDBKey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                ViewBag.UserGuid = UserGuid;
                ViewBag.userDBKey = userDBKey;
                ViewBag.NCR_Assignments_Admin = NCR_Assignments_Admin;
                ViewBag.NCR_Module_User = NCR_Module_User;
                ViewBag.NCR_Assignments_Admin_Delete = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Assignments_Admin_Delete);
                ViewBag.NCR_Assignments_Module_Delete = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Assignments_Module_Delete);

                var viewModel = new NCRWorkflowAssignmentDetailVM();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var multi = await connection.QueryMultipleAsync(
                        "[dbo].[NCRWorkflow_GetAssignmentDetail]",
                        new
                        {
                            NCRWorkflowGUID = id,
                            NCRGuid = NCRGuid,
                            UserGuid = NCR_Assignments_Admin ? null : UserGuid
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    // First result set - Assignment details
                    viewModel.AssignmentData = await multi.ReadFirstOrDefaultAsync<NCRWorkflowAssignmentVM>();

                    // Second result set - Items with tracking
                    viewModel.NcrItems = (await multi.ReadAsync<NCRWorkflowItemVM>()).ToList();

                    // Third result set - Rework markings
                    viewModel.ReworkMarkings = (await multi.ReadAsync<NCRItemReworkVM>()).ToList();
                }

                if (viewModel.AssignmentData == null)
                {
                    return RedirectToAction("AssignedNCRList");
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> SaveRemarks([FromBody] SaveRemarksVM model)
        {
            try
            {
                var userDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var userGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var NCR_Assignments_Admin = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Assignments_Admin);

                // Verify user has permission to edit this item
                if (!NCR_Assignments_Admin)
                {
                    // Check if user is assigned to this NCR
                    using (var connection = mPDapperContext.CreateConnection())
                    {
                        var isAssigned = await connection.QueryFirstOrDefaultAsync<int>(
                            @"SELECT COUNT(*)
                      FROM NCR_Workflow_Assignment_Users
                      WHERE NCRWorkflowGUID = (
                          SELECT NCRWorkFlowGuid 
                          FROM NCR_Item_Workflow_Tracking 
                          WHERE TrackingID = @TrackingID
                      )
                      AND UserGUID = @UserGuid",
                            new { TrackingID = model.TrackingID, UserGuid = userGuid }
                        );

                        if (isAssigned == 0)
                        {
                            return Json(new { success = false, message = "You are not assigned to this item" });
                        }
                    }
                }

                // Save remarks
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<SaveResultVM>(
                        "[dbo].[NCRWorkflow_SaveRemarks]",
                        new
                        {
                            TrackingID = model.TrackingID,
                            NCRItemKey = model.NCRItemKey,
                            StageID = model.StageID,
                            Remarks = model.Remarks,
                            UpdatedBy = userDbkey
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    if (result == null || result.Success == 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = result?.Message ?? "Failed to save remarks"
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new
                {
                    success = false,
                    message = "An error occurred: " + ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsComplete([FromBody] MarkAsCompleteVM model)
        {
            try
            {
                var userDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var userGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var NCR_Module_User = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Module_User);
                var NCR_Assignments_Admin = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Assignments_Admin);

                // Only module users can mark as complete (admins use forward functionality)
                if (!NCR_Module_User || NCR_Assignments_Admin)
                {
                    return Json(new { success = false, message = "Only module users can mark work as complete" });
                }

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<MarkCompleteResultVM>(
                        "[dbo].[NCRWorkflow_MarkAsComplete]",
                        new
                        {
                            NCRWorkflowGUID = model.NCRWorkflowGUID,
                            UserGuid = userGuid,
                            UpdatedBy = userDbkey
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    if (result == null || result.Success == 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = result?.Message ?? "Failed to mark as complete"
                        });
                    }

                    // Send email notification to admins - all data from SP
                    if (!string.IsNullOrEmpty(result.AdminEmails))
                    {
                        var adminEmails = result.AdminEmails.Split(',')
                            .Where(e => !string.IsNullOrWhiteSpace(e))
                            .ToArray();

                        if (adminEmails.Length > 0)
                        {
                            //await SendNCRNotification(
                            //    recipientEmails: adminEmails,
                            //    senderEmail: result.SenderEmail,
                            //    ncrReferenceNumber: result.ReferenceNumber,
                            //    moduleName: result.StageName,
                            //    currentStatus: "Work Marked As Complete",
                            //    actionRequired: "Please review and forward to next stage",
                            //    assignedBy: userDbkey,
                            //    ncrGuid: result.NCRGuid,
                            //    serialNumbers: null
                            //);
                        }
                    }

                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        itemsCompleted = result.ItemsCompleted
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new
                {
                    success = false,
                    message = "An error occurred: " + ex.Message
                });
            }
        }


        [HttpGet]
        public IActionResult GetForwardStagePopup(string workflowGuid, int nextStageID)
        {
            try
            {
                // Get modules based on stage
                var modules = _dbContext.Master_Generals
                    .Where(x => x.Master_Type == "Module_Responsibility" && x.is_active == 1)
                    .OrderBy(x => x.Master_Name)
                    .ToList();

                // Get stage name
                var stage = _dbContext.NCR_Workflow_Stages
                    .FirstOrDefault(s => s.StageID == nextStageID);

                ViewBag.WorkflowGuid = workflowGuid;
                ViewBag.NextStageID = nextStageID;
                ViewBag.StageName = stage?.StageName ?? "Next Stage";
                ViewBag.Modules = modules;

                return PartialView("_ForwardToNextStage");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ForwardToNextStage([FromBody] ForwardToNextStageVM model)
        {
            try
            {
                var userDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var NCR_Assignments_Admin = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Assignments_Admin);

                // Only admins can forward
                if (!NCR_Assignments_Admin)
                {
                    return Json(new { success = false, message = "Only admins can forward to next stage" });
                }

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<ForwardStageResultVM>(
                        "[dbo].[NCRWorkflow_ForwardToNextStage]",
                        new
                        {
                            NCRWorkflowGUID = model.NCRWorkflowGUID,
                            NextStageID = model.NextStageID,
                            NextModuleID = model.NextModuleID,
                            AssignedUserGUIDs = model.AssignedUserGUIDs,
                            ForwardedBy = userDbkey
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    if (result == null || result.Success == 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = result?.Message ?? "Failed to forward to next stage"
                        });
                    }

                    // Send email notification
                    if (!string.IsNullOrEmpty(result.AssignedUserEmails))
                    {
                        var userEmails = result.AssignedUserEmails.Split(',')
                            .Where(e => !string.IsNullOrWhiteSpace(e))
                            .ToArray();

                        if (userEmails.Length > 0)
                        {
                            //await SendNCRNotification(
                            //    recipientEmails: userEmails,
                            //    senderEmail: result.SenderEmail,
                            //    ncrReferenceNumber: result.ReferenceNumber,
                            //    moduleName: $"{result.NextStageName} - {result.ModuleName}",
                            //    currentStatus: $"Forwarded To {result.NextStageName}",
                            //    actionRequired: "Please review and provide your comments",
                            //    assignedBy: userDbkey,
                            //    ncrGuid: result.NCRGuid,
                            //    serialNumbers: null
                            //);
                        }
                    }

                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        newWorkflowGuid = result.NewWorkflowGuid
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new
                {
                    success = false,
                    message = "An error occurred: " + ex.Message
                });
            }
        }

        [HttpGet]
        public IActionResult GetReferItemsPopup(string workflowGuid, string ncrGuid)
        {
            try
            {
                // Get modules (exclude TAS, STRESS, CHAIR - only operational modules)
                var modules = _dbContext.Master_Generals
                    .Where(x => x.Master_Type == "Module_Responsibility"
                             && x.is_active == 1
                             && !x.Master_Name.ToLower().Contains("tas")
                             && !x.Master_Name.ToLower().Contains("stress")
                             && !x.Master_Name.ToLower().Contains("chair"))
                    .OrderBy(x => x.Master_Name)
                    .ToList();

                // Get items for this workflow
                var items = _dbContext.NonConformanceReport_Items
                    .Where(i => i.NCRGuid == ncrGuid)
                    .OrderBy(i => i.SerialNumber)
                    .ToList();

                ViewBag.WorkflowGuid = workflowGuid;
                ViewBag.NCRGuid = ncrGuid;
                ViewBag.Modules = modules;
                ViewBag.Items = items;

                return PartialView("_ReferItemsToModule");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReferItemsToModule([FromBody] ReferItemsVM model)
        {
            try
            {
                var userDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var userGuid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var NCR_Module_User = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Module_User);

                // Module users can refer items
                if (!NCR_Module_User)
                {
                    return Json(new { success = false, message = "You don't have permission to refer items" });
                }

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<ReferItemsResultVM>(
                        "[dbo].[NCRWorkflow_ReferItemsToModule]",
                        new
                        {
                            CurrentWorkflowGUID = model.CurrentWorkflowGUID,
                            ItemKeys = model.ItemKeys,
                            ReferToModuleID = model.ReferToModuleID,
                            ReferToUserGUIDs = model.ReferToUserGUIDs,
                            Remarks = model.Remarks,
                            ReferredBy = userDbkey
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    if (result == null || result.Success == 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = result?.Message ?? "Failed to refer items"
                        });
                    }

                    // Send email notification
                    if (!string.IsNullOrEmpty(result.AssignedUserEmails))
                    {
                        var userEmails = result.AssignedUserEmails.Split(',')
                            .Where(e => !string.IsNullOrWhiteSpace(e))
                            .ToArray();

                        if (userEmails.Length > 0)
                        {
                            //await SendNCRNotification(
                            //    recipientEmails: userEmails,
                            //    senderEmail: result.SenderEmail,
                            //    ncrReferenceNumber: result.ReferenceNumber,
                            //    moduleName: $"{result.StageName} - {result.ReferToModuleName}",
                            //    currentStatus: "Items Referred for Review",
                            //    actionRequired: "Please review the referred items and provide your comments",
                            //    assignedBy: userDbkey,
                            //    ncrGuid: result.NCRGuid,
                            //    serialNumbers: result.ReferredSerialNumbers
                            //);
                        }
                    }

                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        itemsReferred = result.ItemsReferred,
                        serialNumbers = result.ReferredSerialNumbers
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new
                {
                    success = false,
                    message = "An error occurred: " + ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkItemForRework([FromBody] MarkItemReworkVM model)
        {
            try
            {
                var userDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var NCR_Module_User = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Module_User);

                // Only module users can mark items
                if (!NCR_Module_User)
                {
                    return Json(new { success = false, message = "Only module users can mark items for rework" });
                }

                // Validate: Cannot mark as both
                if (model.IsRework && model.IsTrialAssembly)
                {
                    return Json(new { success = false, message = "Cannot mark as both Rework and Trial Assembly" });
                }

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<MarkReworkResultVM>(
                        "[dbo].[NCRWorkflow_MarkItemForRework]",
                        new
                        {
                            NCRItemKey = model.NCRItemKey,
                            StageID = model.StageID,
                            NCRWorkFlowGuid = model.NCRWorkFlowGuid,
                            IsRework = model.IsRework,
                            IsTrialAssembly = model.IsTrialAssembly,
                            MarkedBy = userDbkey
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    if (result == null || result.Success == 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = result?.Message ?? "Failed to mark item"
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        markedAs = result.MarkedAs
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new
                {
                    success = false,
                    message = "An error occurred: " + ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnmarkItemForRework([FromBody] UnmarkItemReworkVM model)
        {
            try
            {
                var userDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var NCR_Module_User = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Module_User);

                // Only module users can unmark items
                if (!NCR_Module_User)
                {
                    return Json(new { success = false, message = "Only module users can unmark items" });
                }

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<UnmarkReworkResultVM>(
                        "[dbo].[NCRWorkflow_UnmarkItemForRework]",
                        new
                        {
                            NCRItemKey = model.NCRItemKey,
                            StageID = model.StageID,
                            UnmarkedBy = userDbkey
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    if (result == null || result.Success == 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = result?.Message ?? "Failed to unmark item"
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new
                {
                    success = false,
                    message = "An error occurred: " + ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> StartReworkCycle([FromBody] StartReworkCycleVM model)
        {
            try
            {
                var userDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                var NCR_Assignments_Admin = UserData.IsAuthorized(User, Constants.UserPermissions.NCR_Assignments_Admin);

                // Only admins can start rework cycle
                if (!NCR_Assignments_Admin)
                {
                    return Json(new { success = false, message = "Only admins can start rework cycle" });
                }

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<StartReworkCycleResultVM>(
                        "[dbo].[NCRWorkflow_StartReworkCycle]",
                        new
                        {
                            NCRWorkflowGUID = model.NCRWorkflowGUID,
                            ModuleID = model.ModuleID,
                            AssignedUserGUIDs = model.AssignedUserGUIDs,
                            StartedBy = userDbkey
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    if (result == null || result.Success == 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = result?.Message ?? "Failed to start rework cycle"
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        itemCount = result.ItemCount
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new
                {
                    success = false,
                    message = "An error occurred: " + ex.Message
                });
            }
        }

        [HttpGet]
        public IActionResult GetStartReworkCyclePopup(string workflowGuid)
        {
            try
            {
                // Get modules for assignment
                var modules = _dbContext.Master_Generals
                    .Where(x => x.Master_Type == "Module_Responsibility" && x.is_active == 1)
                    .OrderBy(x => x.Master_Name)
                    .ToList();

                ViewBag.WorkflowGuid = workflowGuid;
                ViewBag.Modules = modules;

                return PartialView("_StartReworkCycle");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetModuleUsers(int moduleId)
        {
            try
            {
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var users = connection.Query<UserModuleMappingVM>(
                        "[dbo].[Get_Users_WorkflowAssignment]",
                        new { ModuleID = moduleId },
                        commandType: CommandType.StoredProcedure
                    ).ToList();

                    

                    return Json(users);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = ex.Message });
            } 
        }


    }
}
