using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Controllers
{
    [Authorize]
    public class PerformancePredictionController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;

        public PerformancePredictionController(DESI_STFE_PRODContext context)
        {
            _dbContext = context;
        }

        [ClaimRequirement(UserPermissions.Performance_Prediction_Read)]
        public IActionResult Index()
        {
            DataTable dataTable = new DataTable();
            try
            {
				dataTable = MPGlobals.GetDataForDatatable(@"select [predictionKey]
              ,[predectionGUID]
              ,[Title]
              ,[Description]
              ,[inputFilename]
              ,[InputFileLoc]
              ,[createdBy]
              ,[createdon] from PerformancePrediction where Len([OutputDataJson])>50");
			}
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
			var json = JsonConvert.SerializeObject(dataTable);
			var data = JsonConvert.DeserializeObject<List<PerformancePrediction>>(json);  
          
                return View(data);
            
        }

        [ClaimRequirement(UserPermissions.Performance_Prediction_Write)]
        public IActionResult ManagePrediction(string id)
        {
            PerformancePredictionData predectionData = new PerformancePredictionData();
            if (!string.IsNullOrEmpty(id))
            {
                using (_dbContext)
                {
                    var dbPerformnaceData = _dbContext.PerformancePredictions.AsNoTracking().Where(x => x.predectionGUID == id).FirstOrDefault();
                    try
                    {
						if (dbPerformnaceData != null)
						{
							predectionData = JsonConvert.DeserializeObject<PerformancePredictionData>(JsonConvert.SerializeObject(dbPerformnaceData));
						}
					}
                    catch (Exception ex)
                    {
                        ErrorHandler.LogException(ex);
                    }
                }
            }
            return View(predectionData);
        }


        [HttpPost]
        [ClaimRequirement(UserPermissions.Performance_Prediction_Write)]
        public IActionResult ManagePrediction(PerformancePredictionData inputData)
        {
			try
			{
				if (inputData.inputCSV.Length > 0)
                {
				    List<PredectiveEnginePerformance.ModelOutput> predictionOutput = new();
				    var filename = inputData.inputCSV.FileName;
                    var systemfilename = Guid.NewGuid().ToString() + Path.GetExtension(inputData.inputCSV.FileName);
                    var savepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Attachments/PreformancePrediction", systemfilename);
                
					    using (var stream = new FileStream(savepath, FileMode.Create))
					    {
						    inputData.inputCSV.CopyTo(stream);
					    }
					    var inputCSV = ReadCSVFile(savepath);

					    foreach (var item in inputCSV)
					    {
						    var outputItem = predictPerformance(item);
						    predictionOutput.Add(outputItem);
					    }

					    using (_dbContext)
					    {
						    var dbperformanceData = new PerformancePrediction();
						    dbperformanceData.predectionGUID = Guid.NewGuid().ToString();
						    dbperformanceData.Title = inputData.Title;
						    dbperformanceData.Description = inputData.Description;
						    dbperformanceData.inputFilename = filename;
						    dbperformanceData.InputFileLoc = systemfilename;
						    //  dbperformanceData.InputDataJson = JsonConvert.SerializeObject(inputCSV);
						    dbperformanceData.OutputDataJson = JsonConvert.SerializeObject(predictionOutput);
						    // dbperformanceData.createdBy = systemfilename;
						    dbperformanceData.createdon = DateTime.Now;
						    _dbContext.Add(dbperformanceData);
						    _dbContext.SaveChanges();
					    }
					    return Json(new { success = true, inputData = inputCSV, outputData = predictionOutput });
				}   
            }
			catch (Exception ex)
			{
				ErrorHandler.LogException(ex);
				return Json(new { success = false });
			}
			return Json(new { success = false });
		}

        public PredectiveEnginePerformance.ModelOutput predictPerformance(PredectiveEnginePerformance.ModelInput input)
        {
            //Load model and predict output
            var result = PredectiveEnginePerformance.Predict(input);
            return result;
        }

        [ClaimRequirement(UserPermissions.Performance_Prediction_Read)]
        public IActionResult Results(string id)
        {
            PerformancePredictionData predectionData = new PerformancePredictionData();
            if (!string.IsNullOrEmpty(id))
            {
                using (_dbContext)
                {
                    var dbPerformnaceData = _dbContext.PerformancePredictions.AsNoTracking().Where(x => x.predectionGUID == id).FirstOrDefault();
                    try
                    {
						if (dbPerformnaceData != null)
						{
							predectionData = JsonConvert.DeserializeObject<PerformancePredictionData>(JsonConvert.SerializeObject(dbPerformnaceData));
						}
					}
                    catch (Exception ex)
                    {
                        ErrorHandler.LogException(ex);
                    }
                }
            }
            return View(predectionData);
        }


        public List<PredectiveEnginePerformance.ModelInput> ReadCSVFile(string location)
        {
            List<PredectiveEnginePerformance.ModelInput> csvData = new();
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    IgnoreBlankLines = false,
                };
                using (var reader = new StreamReader(location))
                {
                    using (var csv = new CsvReader(reader, config))
                    {
                        var record = new List<PredectiveEnginePerformance.ModelInput>();
                        // var records = csv.GetRecords(CSVDataVM);
                        var records = csv.GetRecords<PredectiveEnginePerformance.ModelInput>();
                        foreach (var item in records)
                        {
                            csvData.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
            return csvData;
        }


    }
}
