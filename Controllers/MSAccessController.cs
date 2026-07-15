using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MPCRS.Models;
using MPCRS.Utilities;
using Newtonsoft.Json;
using System.Data;
using System.Data.OleDb;
using System.Security.Claims;
using static MPCRS.Utilities.Constants;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using MPCRS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Dapper;

namespace MPCRS.Controllers
{
    [Authorize]
    public class MSAccessController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;
     
        public MSAccessController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
        }

        [ClaimRequirement(UserPermissions.Import_Inspection_Report_Read)]
        public IActionResult Index()
        {
            return View();
        }
       
        public string GetAccessUploadList(string vendorCode = null)
        {
            string Cmdstr = string.IsNullOrEmpty(vendorCode)
                ? "InspectionReportRecords_SSP"
                : $"InspectionReportRecords_SSP @VendorCode = '{vendorCode.Replace("'", "''")}'";
            DataTable dataTable = MPGlobals.GetDataForDatalist(Cmdstr);
            return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
        }
        public string GetInspectionReportDataList(string vendorCode = null)
        {
            string Cmdstr = string.IsNullOrEmpty(vendorCode)
                ? "InspectionReportData_SSP"
                : $"InspectionReportData_SSP @VendorCode = '{vendorCode.Replace("'", "''")}'";
            DataTable dataTable = MPGlobals.GetDataForDatalist(Cmdstr);
            return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
        }

        public IActionResult GetInspectionReport(int id)
        {
            string Cmdstr = $"[InspectionReportData_SSP] @Part_relation_dbkey = " + id;
            DataTable dataTable = MPGlobals.GetDataForDatalist(Cmdstr);
           var json = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            List<InspectionReports> reports = JsonConvert.DeserializeObject<List<InspectionReports>>(json);
            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument(IFormFile document)
        {
            DataTable dataTable = new DataTable();
            try
            {
                int FileNo = 0;
                if (document != null && document.Length > 0)
                {
                    InspectionReportRecord inspectionReportRecord = new InspectionReportRecord();
                    var userguid = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    string systemfilename = string.Empty;
                    string filename = string.Empty;
                    string SavePath = string.Empty;
                    string AttachmentGUID = Guid.NewGuid().ToString();
                    filename = document.FileName;
                    systemfilename = AttachmentGUID + Path.GetExtension(filename);
                    string SaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    SavePath = SaveDirectory + "/" + systemfilename;
                    if (!Directory.Exists(SaveDirectory))
                    {
                        Directory.CreateDirectory(SaveDirectory);
                    }
                    using (var stream = new FileStream(SavePath, FileMode.Create))
                    {
                        document.CopyTo(stream);
                    }

                    // Save InspectionReportRecord first to get FileNo for linking JSON dumps
                    inspectionReportRecord.File_Name = filename;
                    inspectionReportRecord.File_SystemName = systemfilename;
                    inspectionReportRecord.File_Location = "/uploads/";
                    inspectionReportRecord.File_Updatedby = userguid;
                    inspectionReportRecord.File_UpdatedOn = DateTime.Now;
                    _dbContext.InspectionReportRecords.Add(inspectionReportRecord);
                    _dbContext.SaveChanges();
                    FileNo = inspectionReportRecord.Inspect_File_DBkey;

                    // Process ZIP file - passes FileNo so JSON dumps can be linked to this batch record
                    var (rawData, vendorCode) = await ProcessZipFileAsync(SavePath, FileNo);

                    // Update the record with VendorCode parsed from vendor_info.json
                    if (!string.IsNullOrWhiteSpace(vendorCode))
                    {
                        inspectionReportRecord.VendorCode = vendorCode;
                        _dbContext.SaveChanges();
                    }

                    int updatedUser = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                    var inspectionReports = _dbContext.InspectionReports.AsNoTracking();
                    foreach (var item in rawData)
                    {
                        item.File_No = FileNo;  //Reference Dbkey for [dbo].[InspectionReportRecord] table
                        var existingRecord = inspectionReports.Where(x => x.Inspect_Rpt_Dbkey == item.Inspect_Rpt_Dbkey).FirstOrDefault();
                        if (existingRecord != null)
                        {
                            //Inspect_Rpt_key is the primary key for table [InspectionReport]
                            //Inspect_Rpt_Dbkey is the dbkey coming from windows application
                            item.Inspect_Rpt_key = existingRecord.Inspect_Rpt_key;
                            // Preserve Part_relation_dbkey from DB if not provided in vendor data
                            if (string.IsNullOrEmpty(item.Part_relation_dbkey))
                            {
                                item.Part_relation_dbkey = existingRecord.Part_relation_dbkey;
                            }
                            _dbContext.InspectionReports.Entry(item).State = EntityState.Modified;
                        }
                        else
                        {
                            _dbContext.InspectionReports.Add(item);
                        }
                    }
                    _dbContext.SaveChanges();

                    return Json(new { success = true, msg = "Saved Successfully" });
                }
            }
            catch (Exception ex) {
                ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message });
            }


            return BadRequest();
        }

        public async Task<(List<InspectionReport> Reports, string? VendorCode)> ProcessZipFileAsync(string zipFilePath, int fileNo)
        {
            var extractedObjects = new List<InspectionReport>();
            var extractedObjectsVM = new List<InspectionReportVM>();
            string? vendorCode = null;
            var destinationFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "InspectionReportImports");
            var ncrAttachmentFolderPath = Path.Combine(destinationFolderPath, "NCR");

                // Create destination folders if they do not exist
                if (!Directory.Exists(destinationFolderPath))
                {
                    Directory.CreateDirectory(destinationFolderPath);
                }
                if (!Directory.Exists(ncrAttachmentFolderPath))
                {
                    Directory.CreateDirectory(ncrAttachmentFolderPath);
                }

                using (var zipArchive = ZipFile.OpenRead(zipFilePath))
                {
                    foreach (var entry in zipArchive.Entries)
                    {
                        if (entry.FullName.EndsWith("report_data.json", StringComparison.OrdinalIgnoreCase))
                        {
                            // Read JSON file and deserialize into List<InspectionReport>
                            using (var stream = entry.Open())
                            using (var reader = new StreamReader(stream))
                            {
                                var jsonContent = reader.ReadToEnd();

                                if (!string.IsNullOrWhiteSpace(jsonContent))
                                {
                                    extractedObjectsVM = JsonConvert.DeserializeObject<List<InspectionReportVM>>(jsonContent) ?? new List<InspectionReportVM>();

                                    // Map VMs to entities and add to extractedObjects
                                    extractedObjects.AddRange(
                                        extractedObjectsVM.Select(vm => new InspectionReport
                                        {
                                            Inspect_Rpt_key = vm.Inspect_Rpt_key,
                                            Inspect_Rpt_Dbkey = vm.Inspect_Rpt_Dbkey,
                                            File_No = vm.File_No,
                                            Part_relation_dbkey = vm.Part_relation_dbkey,
                                            Drawing_No = vm.Draw_part_no,   // <— name difference handled here
                                            Serial_No = vm.Serial_No,
                                            Job_No = vm.Job_No,
                                            File_Name = vm.File_Name,
                                            File_Location = vm.File_Location,
                                            UpdatedOn = vm.UpdatedOn,
                                            BuildNumber = vm.BuildNumber,
                                            Remarks = vm.Remarks,
                                            Revision = vm.Revision,
                                            Quantity = vm.Quantity,
                                            RMC_Number = vm.RMC_Number
                                        })
                                    );
                                }
                                else
                                {
                                    throw new InvalidDataException("The JSON content is empty.");
                                }
                            }
                        }
                    else if (entry.FullName.EndsWith("BuildAssignment.json", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var stream = entry.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            var jsonContent = reader.ReadToEnd();

                            if (!string.IsNullOrWhiteSpace(jsonContent))
                            {
                                BATL_Build_assignment_Json bATL_Build_Assignment_Json = new BATL_Build_assignment_Json();
                                bATL_Build_Assignment_Json.Build_assignment_Json = jsonContent;
                                bATL_Build_Assignment_Json.File_No = fileNo;
                                bATL_Build_Assignment_Json.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                                bATL_Build_Assignment_Json.UpdatedOn = DateTime.Now;
                                _dbContext.Add(bATL_Build_Assignment_Json);
                                _dbContext.SaveChanges();
                            }
                        }
                    }
                    else if (entry.FullName.EndsWith("RMC_Data_data.json", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var stream = entry.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            var jsonContent = reader.ReadToEnd();

                            if (!string.IsNullOrWhiteSpace(jsonContent))
                            {
                                BATL_RMC_Json rmcData = new BATL_RMC_Json();
                                rmcData.RMC_Json = jsonContent;
                                rmcData.File_No = fileNo;
                                rmcData.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                                rmcData.UpdatedOn = DateTime.Now;
                                _dbContext.Add(rmcData);
                                _dbContext.SaveChanges();
                            }
                        }
                    }
                    else if (entry.FullName.EndsWith("NCR_data.json", StringComparison.OrdinalIgnoreCase))
                    {
                        // Store NCR JSON data from vendor
                        using (var stream = entry.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            var jsonContent = reader.ReadToEnd();

                            if (!string.IsNullOrWhiteSpace(jsonContent))
                            {
                                BATL_NCR_Json ncrData = new BATL_NCR_Json();
                                ncrData.NCR_Json = jsonContent;
                                ncrData.File_No = fileNo;
                                ncrData.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                                ncrData.UpdatedOn = DateTime.Now;
                                _dbContext.Add(ncrData);
                                _dbContext.SaveChanges();
                            }
                        }
                    }
                    else if (entry.FullName.EndsWith("castingSerialMapping.json", StringComparison.OrdinalIgnoreCase))
                    {
                        // Store Casting Serial Mapping JSON data from vendor
                        using (var stream = entry.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            var jsonContent = reader.ReadToEnd();

                            if (!string.IsNullOrWhiteSpace(jsonContent))
                            {
                                BATL_CastingSerialMapping_Json mappingData = new BATL_CastingSerialMapping_Json();
                                mappingData.CastingSerialMapping_Json = jsonContent;
                                mappingData.File_No = fileNo;
                                mappingData.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                                mappingData.UpdatedOn = DateTime.Now;
                                _dbContext.Add(mappingData);
                                _dbContext.SaveChanges();
                            }
                        }
                    }
                    else if (entry.FullName.EndsWith("vendor_info.json", StringComparison.OrdinalIgnoreCase))
                    {
                        // Parse vendor identification from vendor_info.json
                        using (var stream = entry.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            var jsonContent = reader.ReadToEnd();

                            if (!string.IsNullOrWhiteSpace(jsonContent))
                            {
                                var vendorInfo = JsonConvert.DeserializeAnonymousType(jsonContent,
                                    new { vendor_code = "", generated_at = "" });
                                vendorCode = vendorInfo?.vendor_code;
                            }
                        }
                    }
                    else if (entry.FullName.StartsWith("NCR/", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(entry.Name))
                    {
                        // Extract NCR attachment files to dedicated NCR folder
                        var ncrFilePath = Path.Combine(ncrAttachmentFolderPath, entry.Name);
                        entry.ExtractToFile(ncrFilePath, overwrite: true);
                    }
                    else if (!string.IsNullOrEmpty(entry.Name))
                        {
                            // Copy other files to the destination folder
                            var destinationPath = Path.Combine(destinationFolderPath, entry.FullName);
                            var directoryPath = Path.GetDirectoryName(destinationPath);

                            if (!string.IsNullOrEmpty(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            entry.ExtractToFile(destinationPath, overwrite: true);

                    }
                }
                }

                if (extractedObjects == null || extractedObjects.Count == 0)
                {
                    //throw new InvalidDataException("No valid JSON data was found in test.json.");
                }

                return (extractedObjects, vendorCode);

        }

        //public IActionResult RMC_Data(string RMC_Number)
        //{
        //    List<ProcurementReceiptItemSplitViewModel> rmData = new List<ProcurementReceiptItemSplitViewModel>();
        //    using (var connection = mPDapperContext.CreateConnection())
        //    {
        //        var db = connection.QueryMultiple($" [dbo].[Get_RM_By_RMC_Number] @RMC_Number  ='{RMC_Number}'");
        //        rmData = db.Read<ProcurementReceiptItemSplitViewModel>().ToList();

        //    }
        //    return View(rmData);
        //}

        public IActionResult RMC_Data(string RMC_Number)
        {
            RMCDataPopupVM result = new RMCDataPopupVM();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var multi = connection.QueryMultiple(
                    "[dbo].[Get_RM_By_RMC_Number]",
                    new { RMC_Number = RMC_Number },
                    commandType: CommandType.StoredProcedure
                );
                result.Diagnostic = multi.ReadFirstOrDefault<RMCDiagnosticVM>();
                result.Splits = multi.Read<ProcurementReceiptItemSplitViewModel>().ToList();
                result.Attachments = multi.Read<RMCAttachmentVM>().ToList();
            }
            return PartialView(result);
        }

        public IActionResult GetVendorList()
        {
            var vendors = _dbContext.InspectionReportRecords
                .Where(r => r.VendorCode != null && r.VendorCode != "")
                .Select(r => r.VendorCode)
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            return Json(vendors);
        }


        public ContentResult GetRMJsonData(string vendorCode = null)
        {
            var query = from rmc in _dbContext.BATL_RMC_Jsons
                        join rec in _dbContext.InspectionReportRecords
                            on rmc.File_No equals rec.Inspect_File_DBkey into recJoin
                        from rec in recJoin.DefaultIfEmpty()
                        select new { rmc, VendorCode = rec != null ? rec.VendorCode : null };

            if (!string.IsNullOrEmpty(vendorCode))
            {
                query = query.Where(x => x.VendorCode == vendorCode);
            }

            var latestJson = query
                .OrderByDescending(x => x.rmc.UpdatedOn)
                .Select(x => x.rmc)
                .FirstOrDefault();

            if (latestJson != null && !string.IsNullOrWhiteSpace(latestJson.RMC_Json))
            {
                return Content(latestJson.RMC_Json, "application/json");
            }

            return Content("[]", "application/json");
        }


        public ContentResult GetBuildAssignmentJsonData(string vendorCode = null)
        {
            var query = from ba in _dbContext.BATL_Build_assignment_Jsons
                        join rec in _dbContext.InspectionReportRecords
                            on ba.File_No equals rec.Inspect_File_DBkey into recJoin
                        from rec in recJoin.DefaultIfEmpty()
                        where ba.Build_assignment_Json.Contains("engine_build_name")
                        select new { ba, VendorCode = rec != null ? rec.VendorCode : null };

            if (!string.IsNullOrEmpty(vendorCode))
            {
                query = query.Where(x => x.VendorCode == vendorCode);
            }

            var latestJson = query
                .OrderByDescending(x => x.ba.UpdatedOn)
                .Select(x => x.ba)
                .FirstOrDefault();

            if (latestJson != null && !string.IsNullOrWhiteSpace(latestJson.Build_assignment_Json))
            {
                return Content(latestJson.Build_assignment_Json, "application/json");
            }

            return Content("[]", "application/json");
        }


        public ContentResult GetNCRJsonData(string vendorCode = null)
        {
            var query = from ncr in _dbContext.BATL_NCR_Jsons
                        join rec in _dbContext.InspectionReportRecords
                            on ncr.File_No equals rec.Inspect_File_DBkey into recJoin
                        from rec in recJoin.DefaultIfEmpty()
                        select new { ncr, VendorCode = rec != null ? rec.VendorCode : null };

            if (!string.IsNullOrEmpty(vendorCode))
            {
                query = query.Where(x => x.VendorCode == vendorCode);
            }

            var latestJson = query
                .OrderByDescending(x => x.ncr.UpdatedOn)
                .Select(x => x.ncr)
                .FirstOrDefault();

            if (latestJson != null && !string.IsNullOrWhiteSpace(latestJson.NCR_Json))
            {
                // Enrich NCR data with part name from Engine_Parts_Usage -> Engine_Parts_Master
                var ncrList = JsonConvert.DeserializeObject<List<dynamic>>(latestJson.NCR_Json);
                if (ncrList != null)
                {
                    var partDbkeys = ncrList
                        .Where(n => n.part_relation_dbkey != null)
                        .Select(n => (int)n.part_relation_dbkey)
                        .Distinct()
                        .ToList();

                    var partNames = _dbContext.Engine_Parts_Usages
                        .Where(epu => partDbkeys.Contains(epu.Part_relation_dbkey))
                        .Select(epu => new
                        {
                            epu.Part_relation_dbkey,
                            epu.Engine_Part_DbkeyNavigation.Draw_part_no,
                            epu.Description
                        })
                        .ToDictionary(x => x.Part_relation_dbkey);

                    foreach (var ncr in ncrList)
                    {
                        int? prDbkey = ncr.part_relation_dbkey;
                        if (prDbkey.HasValue && partNames.ContainsKey(prDbkey.Value))
                        {
                            ncr.draw_part_no = partNames[prDbkey.Value].Draw_part_no;
                            ncr.part_description = partNames[prDbkey.Value].Description;
                        }
                        else
                        {
                            ncr.draw_part_no = "";
                            ncr.part_description = "";
                        }
                    }

                    var enrichedJson = JsonConvert.SerializeObject(ncrList);
                    return Content(enrichedJson, "application/json");
                }

                return Content(latestJson.NCR_Json, "application/json");
            }

            return Content("[]", "application/json");
        }


        public IActionResult PartUsageSummary()
        {
            return View();
        }


        [HttpGet]
        public IActionResult GetPartUsageSummaryJson()
        {
            var summaryData = new BATLPartUsageSummaryVM();
            using (var connection = mPDapperContext.CreateConnection())
            {
                using (var multi = connection.QueryMultiple("[dbo].[BATL_PartUsageSummary]",
                       commandType: CommandType.StoredProcedure, commandTimeout: 300))
                {
                    summaryData.BuildName = multi.Read<string>().ToList();
                    summaryData.GroupedUsageDetails = multi.Read<GroupedUsageDetails>().ToList();
                }
            }
            return Json(summaryData);
        }

        [HttpGet]
        public IActionResult DistictPartList(string buildName)
        {
            DistinctBuildAssignmentData data = new DistinctBuildAssignmentData();

            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple(
                    "[dbo].[DistinctBuildJsonParts]",
                    new { BuildName = buildName },
                    commandType: CommandType.StoredProcedure
                );

                // Read first result set (DistinctBuildNames)
                data.DistinctBuildNames = db.Read<DistinctBuildNames>().ToList();

                // Read second result set (DistinctBuildParts)  
                data.DistinctBuildParts = db.Read<DistinctBuildParts>().ToList();
            }

            return PartialView(data);
        }

        #region Download Json files
        public IActionResult DownloadMPLJsonFile()
        {
            try
            {
                DataTable dataTable = MPGlobals.GetDataForDatalist("dbo.Get_MPL_AccessDB @BL_Engine_Dbkey = " + 0 + ",@Engine_Dbkey = " + 0);
                var dataList = MPGlobals.GetTableAsList(dataTable);
                var jsonData = JsonConvert.SerializeObject(dataList);
                return File(System.Text.Encoding.UTF8.GetBytes(jsonData), "application/json", "MPL.json");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, error = ex });

            }
           
        }
        public IActionResult Download_RM_JsonFile()
        {
            try
            {
                DataTable dataTable = MPGlobals.GetDataForDatalist("dbo.Get_RM_AccessDB");
                var dataList = MPGlobals.GetTableAsList(dataTable);
                var jsonData = JsonConvert.SerializeObject(dataList);
                return File(System.Text.Encoding.UTF8.GetBytes(jsonData), "application/json", "RawMaterial_Master.json");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, error = ex });

            }

        }

        public IActionResult DownloadSOPData()
        {
            try
            {
                DataTable dataTable = MPGlobals.GetDataForDatalist("dbo.GetSOPDataForBATL");
                var dataList = MPGlobals.GetTableAsList(dataTable);

                // Parse string columns into lists
                foreach (var row in dataList)
                {
                    if (row.ContainsKey("SerialNumber") && row["SerialNumber"] is string serials)
                        row["SerialNumber"] = serials.Split(',').Select(s => s.Trim()).ToList();

                }

                var jsonData = JsonConvert.SerializeObject(dataList);
                return File(System.Text.Encoding.UTF8.GetBytes(jsonData), "application/json", "SOPBuildsData.json");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, error = ex });
            }
        }


        //public IActionResult Download_CastingData_JsonFile()
        //{
        //    try
        //    {
        //        DataTable dataTable = MPGlobals.GetDataForDatalist("dbo.Get_CastingData_AccessDB");
        //        var dataList = MPGlobals.GetTableAsList(dataTable);
        //        var jsonData = JsonConvert.SerializeObject(dataList);
        //        return File(System.Text.Encoding.UTF8.GetBytes(jsonData), "application/json", "CastingData.json");
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorHandler.LogException(ex);
        //        return Json(new { success = false, error = ex });

        //    }

        //}
        // Model Definition
       
        public class CastingData
        {
            public string ReceiptNumber { get; set; } = string.Empty;
            public string Draw_part_no { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string HeatNumber { get; set; } = string.Empty;
            public string BatchNumber { get; set; } = string.Empty;
            public string SerialNos { get; set; } = string.Empty; // Keep as string from DB, we'll parse later
        }

        // Updated Controller Method using Dapper
        public IActionResult Download_CastingData_JsonFile()
        {
            try
            {
                List<CastingData> castingDataList = new List<CastingData>();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    // Option 1: Using stored procedure
                    castingDataList = connection.Query<CastingData>("dbo.Get_CastingData_AccessDB",
                        commandType: CommandType.StoredProcedure).ToList();                 
                  
                }

                // Transform the data to have SerialNos as arrays
                var transformedData = castingDataList.Select(item => new
                {
                    item.ReceiptNumber,
                    item.Draw_part_no,
                    item.Description,
                    item.HeatNumber,
                    item.BatchNumber,
                    SerialNos = ParseSerialNumbers(item.SerialNos)
                }).ToList();

                var jsonData = JsonConvert.SerializeObject(transformedData, Formatting.Indented);
                return File(System.Text.Encoding.UTF8.GetBytes(jsonData), "application/json", "CastingData.json");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, error = ex.Message });
            }
        }

        public IActionResult Download_ForgingData_JsonFile()  ////--------------------------
        {
            try
            {
                List<CastingData> castingDataList = new List<CastingData>();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    // Option 1: Using stored procedure
                    castingDataList = connection.Query<CastingData>("dbo.Get_ForgingData_AccessDB",
                        commandType: CommandType.StoredProcedure).ToList();

                }

                // Transform the data to have SerialNos as arrays
                var transformedData = castingDataList.Select(item => new
                {
                    item.ReceiptNumber,
                    item.Draw_part_no,
                    item.Description,
                    item.HeatNumber,
                    item.BatchNumber,
                    SerialNos = ParseSerialNumbers(item.SerialNos)
                }).ToList();

                var jsonData = JsonConvert.SerializeObject(transformedData, Formatting.Indented);
                return File(System.Text.Encoding.UTF8.GetBytes(jsonData), "application/json", "ForgingData.json");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, error = ex.Message });
            }
        }

        // Helper method to parse SerialNos string into array
        private string[] ParseSerialNumbers(string serialNosString)
        {
            if (string.IsNullOrWhiteSpace(serialNosString))
            {
                return new string[0];
            }

            // Check if it's JSON format first
            if (serialNosString.Trim().StartsWith("[") && serialNosString.Trim().EndsWith("]"))
            {
                try
                {
                    return JsonConvert.DeserializeObject<string[]>(serialNosString) ?? new string[0];
                }
                catch
                {
                    // If JSON parsing fails, fall back to comma separation
                }
            }

            // Parse as comma-separated values
            return serialNosString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(s => s.Trim())
                                 .Where(s => !string.IsNullOrEmpty(s))
                                 .ToArray();
        }

        #endregion

    }
}
