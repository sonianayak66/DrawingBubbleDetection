using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using System.Data;
using Dapper;
using Newtonsoft.Json;
using System;
using XAct;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;
        public ProjectsController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }
        [ClaimRequirement(UserPermissions.Projects_Read)]
        public IActionResult Index()
        {
            List<ProjectVM> Projectinfo = new();
            using (_dbContext)
            {
                try
                {
                    using (var connection = mPDapperContext.CreateConnection())
                    {
                    var db = connection.QueryMultiple($"dbo.ProjectList_SP");
                    Projectinfo = db.Read<ProjectVM>().ToList();

                }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }

            return View(Projectinfo);
            }
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.Projects_Write)]
        public IActionResult CreateProject(int Id)
        {
            ProjectVM vm = new();
            using (_dbContext)
            {

                if (Id != 0)
                {
                    Project proj = _dbContext.Projects.Where(x => x.Project_Dbkey == Id).FirstOrDefault();
                    if (proj != null)
                    {
                        vm.Project_Dbkey = proj.Project_Dbkey;

                        vm.Title = proj.Title;
                        vm.Display_title = proj.Display_title;
                        vm.Description = proj.Description;
                        vm.DOS = proj.DOS;
                        vm.EDO = proj.EDO;
                        vm.Project_Number = proj.Project_Number;
                        vm.Category_Dbkey = proj.Category_Dbkey;
                        vm.Sec_Classfic_Dbkey = proj.Sec_Classfic_Dbkey;
                        vm.No_of_Engines = proj.No_of_Engines;
                        vm.Unique_Name = proj.Unique_Name;
                        vm.BL_Engine_Dbkey = proj.BL_Engine_Dbkey;
                        vm.EstimatedCost = proj.EstimatedCost;


                        return View(vm);
                    }
                }


            }
            return View(vm);
            
        }

        [HttpPost]
        [ClaimRequirement(UserPermissions.Projects_Write)]
        public IActionResult CreateProject(ProjectVM vm)
        {
            Project dbModel = new();
            using (_dbContext)
            {
                try
                {
                    dbModel = JsonConvert.DeserializeObject<Project>(JsonConvert.SerializeObject(vm));
                    if (vm.Project_Dbkey != 0)
                    {
                         Project dbmodel = _dbContext.Projects.AsNoTracking().Where(x => x.Project_Dbkey == vm.Project_Dbkey).FirstOrDefault();
                            if (dbmodel != null)
                            {
                                dbModel.is_active = 1;
                                dbModel.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                                dbModel.Updated_on = DateTime.Now;
                                _dbContext.Entry(dbModel).State = EntityState.Modified;

                                _dbContext.SaveChanges();
                            }
                            else
                            {
                                // this is to ensure it catches the next if condition
                                vm.Project_Dbkey = 0;
                            }
                      
                    }
                    if (vm.Project_Dbkey == 0)
                    {
                        if (CheckDuplicateProject(dbModel.Title) == false)
                        {

                            Project projmodel = _dbContext.Projects.Where(x => x.Project_Dbkey == dbModel.Project_Dbkey).FirstOrDefault();

                            dbModel.Project_Dbkey = vm.Project_Dbkey;

                            dbModel.Title = vm.Title;
                            dbModel.Display_title = vm.Display_title;
                            dbModel.Description = vm.Description;
                            dbModel.DOS = vm.DOS;
                            dbModel.EDO = vm.EDO;
                            dbModel.Project_Number = vm.Project_Number;
                            dbModel.Category_Dbkey = vm.Category_Dbkey;
                            dbModel.Sec_Classfic_Dbkey = vm.Sec_Classfic_Dbkey;
                            dbModel.No_of_Engines = vm.No_of_Engines;
                            dbModel.Unique_Name = vm.Unique_Name;
                            dbModel.BL_Engine_Dbkey = vm.BL_Engine_Dbkey;
                            dbModel.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                            dbModel.Updated_on = DateTime.Now;
                            dbModel.is_active = 1;
                            
                            _dbContext.Add(dbModel);
                            _dbContext.SaveChanges();                            
                        }
                    }
                    return Json(new { success = true, msg = "Saved successfully" });
                }

                catch (Exception ex) { return Json(new { success = false, msg = ex.Message }); }
            }

        }

        public static bool CheckDuplicateProject(string project_Title)
        {
            DataTable dataTable = MPGlobals.GetDataForDatalist("SELECT [Project_Dbkey] FROM [dbo].[Project] where [Title] = '" + project_Title + "'");
            if (dataTable.Rows.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    }
