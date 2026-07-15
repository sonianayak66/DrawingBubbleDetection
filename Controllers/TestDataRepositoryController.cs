using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using MPCRS.Models;
using MPCRS.Utilities;
using Newtonsoft.Json;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MPCRS.ViewModels;
using System.IO;
using System.Globalization;
using Dapper;
using NUglify.Helpers;
using NUglify.JavaScript.Syntax;
using XAct.Library.Settings;
using static MPCRS.Utilities.Constants;
using System.Net.Mail;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using XAct;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using System.Text;
using CsvHelper.Configuration.Attributes;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;



namespace MPCRS.Controllers
{
    [Authorize]
    public class TestDataRepositoryController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly ILogger<TestDataRepositoryController> _logger;
        private readonly MPDapperContext mPDapperContext;
        private readonly IConfiguration _configuration;

        public TestDataRepositoryController(DESI_STFE_PRODContext context, ILogger<TestDataRepositoryController> logger, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _logger = logger;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

		[ClaimRequirement(UserPermissions.Test_Data_Repository_Read)]
		public IActionResult Index()
        {
            using (_dbContext)
            {
                List<TestDataRepository> testDataRepo = _dbContext.TestDataRepositories.AsNoTracking().ToList();
                // List<TestDataRepositoryVM> testDataRepository = JsonConvert.DeserializeObject<List<TestDataRepositoryVM>>(JsonConvert.SerializeObject(testDataRepo));
                return View(testDataRepo);
            }
        }

        [HttpGet]
        [ClaimRequirement(UserPermissions.Performance_Prediction_Read)]
        public IActionResult ManageExcelPerdiction(int Id)
        {
            TestDataRepositoryVM testDataRepository = new TestDataRepositoryVM();
            if (Id != 0)
            {
                using (_dbContext)
                {
                    var dbTestDataRepo = _dbContext.TestDataRepositories.AsNoTracking().Where(x => x.TestdataDbKey == Id).FirstOrDefault();
                    try
                    {
                        if (dbTestDataRepo != null)
                        {
                            testDataRepository = JsonConvert.DeserializeObject<TestDataRepositoryVM>(JsonConvert.SerializeObject(dbTestDataRepo));
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.LogException(ex);
                    }
                }
            }

            return View(testDataRepository);
        }

        [HttpPost]
        [ClaimRequirement(UserPermissions.Performance_Prediction_Write)]
        public IActionResult ManageExcelPerdiction(TestDataRepositoryVM inputData)
        {
            TestDataRepository dbTestData = new TestDataRepository();
            int dbkey = 0;
            try
            {

                using (_dbContext)
                {
                    dbTestData.TestdataDbKey = inputData.TestdataDbKey;
                    dbTestData.CellNo = inputData.CellNo;
                    dbTestData.EngineName = inputData.EngineName;
                    dbTestData.BuildNo = inputData.BuildNo;
                    dbTestData.RunNo = inputData.RunNo;
                    dbTestData.NH = inputData.NH;
                    dbTestData.NL = inputData.NL;
                    dbTestData.AtmosphericPressure = inputData.AtmosphericPressure;
                    dbTestData.RoomTemperature = inputData.RoomTemperature;
                    dbTestData.DecuSWBuildNumber = inputData.DecuSWBuildNumber;
                    // dbTestData.UploadedFile = inputData.UploadedFile; // NEED TO ASK CLARIFICATION
                    dbTestData.UpdateBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    dbTestData.UpdatedOn = DateTime.Now;

                    if (inputData.TestdataDbKey == 0)
                    {
                        dbTestData.CreatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                        dbTestData.CreatedOn = DateTime.Now;
                        _dbContext.Add(dbTestData);
                        _dbContext.SaveChanges();

                    }
                    else
                    {
                        _dbContext.Entry(dbTestData).State = EntityState.Modified;
                        _dbContext.SaveChanges();
                    }

                    dbkey = dbTestData.TestdataDbKey;
                    return Json(new { success = true, msg = "Saved Successfully", saveddbkey = dbkey });

                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);

            }
            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult UploadExcelForPrediction(AttachmentVM attachmentVM)
        {
            using (_dbContext)
            {
                if (attachmentVM != null)
                {
                    var TestdataDbKey = attachmentVM.Source_table_key;
                    var UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    // var UpdatedOn = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

                    string systemfilename = string.Empty;
                    string filename = string.Empty;
                    string SavePath = string.Empty;
                    filename = attachmentVM.uploadeddocument.FileName;
                    systemfilename = Guid.NewGuid().ToString() + Path.GetExtension(attachmentVM.uploadeddocument.FileName);
                    SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Attachments/PreformancePrediction");
                    if (!Directory.Exists(SavePath))
                    {
                        Directory.CreateDirectory(SavePath);
                    }
                    SavePath = SavePath + "/" + systemfilename;
                    using (var stream = new FileStream(SavePath, FileMode.Create))
                    {
                        attachmentVM.uploadeddocument.CopyTo(stream);
                    }

                    DataTable excelTable = ExcelFileHelper.SaveAsDatatable(SavePath);

                    List<string> columnNames = excelTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList();


                    foreach (string columnName in columnNames)
                    {

                        string checkColumnQuery = $"IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TestDataValue' AND COLUMN_NAME = '{columnName}') BEGIN ALTER TABLE TestDataValue ADD [{columnName}] VARCHAR(50) END";
                        MPGlobals.ExceSQLNonQuery(checkColumnQuery);



                    }

                    foreach (DataRow row in excelTable.Rows)
                    {
                        var columns = string.Join(", ", excelTable.Columns.Cast<DataColumn>().Select(c => $"[{c.ColumnName}]"));
                        var values = string.Join(", ", excelTable.Columns.Cast<DataColumn>().Select(c => $"'{row[c.ColumnName].ToString().Replace("'", "''")}'"));

                        string insertQuery = $"INSERT INTO TestDataValue ([TestdataDbKey],[UpdatedBy],[UpdatedOn],{columns}) VALUES ('{TestdataDbKey}','{UpdatedBy}',getdate(),{values})";
                        MPGlobals.ExceSQLNonQuery(insertQuery);


                    }
                    string jsonData = JsonConvert.SerializeObject(excelTable, Formatting.Indented);

                    
                    // to savepath in _dbcontext only as testdatarepojson is taking to much time to load

                    TestDataRepository testDataRepository = _dbContext.TestDataRepositories.Where(x => x.TestdataDbKey == attachmentVM.Source_table_key).FirstOrDefault();
                    testDataRepository.UploadedFile = "/Attachments/PreformancePrediction/" + systemfilename.ToString();
                    _dbContext.TestDataRepositories.Entry(testDataRepository).State = EntityState.Modified;


                }
                _dbContext.SaveChanges();
            }
            //return View();
            return Json(new { success = true, msg = "Saved Successfully" });
        }
        [HttpGet]
        public IActionResult DownloadTestDataFile(string datakeys)
        {
            if (!datakeys.IsNullOrEmpty())     
            {   

                //string query = $"SELECT * FROM dbo.TestDataValue WHERE TestdataDbKey IN ({datakeys})";
                string query = $" Dbo.GetTestDataValue @testDataKeys  = '{datakeys}'";
                DataTable dataTable = MPGlobals.GetDataForDatalist(query);
                //Hiding 4 column
                HideColumns(dataTable, "TestDataValDbKey", "TestdataDbKey", "UpdatedOn", "UpdatedBy");

                DataTable modifiedTable = CreateModifiedDataTable(dataTable);

                string csvContent = DataTableToCsv(modifiedTable);
                // Set response headers
                var contentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "exported_data.csv"
                };
                Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                // Return CSV as FileResult
                return File(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent)), "text/csv");
            }
            return Json(new { success = false });
        }
       

        // Convert DataTable to CSV string
        private string DataTableToCsv(DataTable dataTable)
        {
            StringWriter csvString = new StringWriter();
            // Write header
            foreach (DataColumn column in dataTable.Columns)
            {
                csvString.Write($"{column.ColumnName},");
            }
            csvString.WriteLine();

            // Write data
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (object item in row.ItemArray)
                {
                    csvString.Write($"{item},");
                }
                csvString.WriteLine();
            }

            return csvString.ToString();
        }

        static void HideColumns(DataTable dataTable, params string[] columnsToHide)
        {
            foreach (string columnName in columnsToHide)
            {
                if (dataTable.Columns.Contains(columnName))
                {
                    dataTable.Columns[columnName].ColumnMapping = MappingType.Hidden;
                }
            }
        }
        // Method to create a new DataTable with modified columns
        static DataTable CreateModifiedDataTable(DataTable originalTable)
        {
            DataTable modifiedTable = new DataTable();

            foreach (DataColumn originalColumn in originalTable.Columns)
            {
                if (originalColumn.ColumnMapping != MappingType.Hidden)
                {
                    modifiedTable.Columns.Add(originalColumn.ColumnName, originalColumn.DataType);
                }
            }
            foreach (DataRow originalRow in originalTable.Rows)
            {
                DataRow newRow = modifiedTable.NewRow();

                foreach (DataColumn column in modifiedTable.Columns)
                {
                    newRow[column.ColumnName] = originalRow[column.ColumnName];
                }

                modifiedTable.Rows.Add(newRow);
            }
            return modifiedTable;
        }

        public IActionResult DeleteTestDataRepo(int TestDataDbKey)
        {
            MPGlobals.ExceSQLNonQuery($"Delete from dbo.TestDataRepository where [TestdataDbKey]= {TestDataDbKey}");
            MPGlobals.ExceSQLNonQuery($"Delete from dbo.TestDataValue where [TestdataDbKey]= {TestDataDbKey}");
            return Json(new { success = true });
        }

    }
}
