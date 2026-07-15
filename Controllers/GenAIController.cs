using Dapper;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2013.Excel;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.Utils;
using OpenXmlPowerTools;
using Org.BouncyCastle.Ocsp;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using XAct;
using static MPCRS.Utilities.Constants;
using static NuGet.Packaging.PackagingConstants;
using Folders = MPCRS.ViewModels.Folders;

namespace MPCRS.Controllers
{
    [Authorize]
	public class GenAIController : Controller
    {
        private readonly DESI_STFE_PRODContext _dbContext;
        private readonly AiAPIService _aiAPIService;
        private static string connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["MPCRS"];
        private readonly string _dbSchema;
        private readonly IConfiguration _configuration;
        private readonly MPDapperContext mPDapperContext;
		private readonly string _pythonPath;

		public GenAIController(AiAPIService aiAPIService, DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
        {
            
            _aiAPIService = aiAPIService;
            _dbContext = context;
            _configuration = configuration;
            this.mPDapperContext = mPDapperContext;
			_pythonPath = configuration["PythonPath:Path"];

		}
        [HttpGet]
        [ClaimRequirement(UserPermissions.GEN_AI)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteQuery([FromBody] QueryRequest request)
        {
            DTOResponse dTOResponse = new DTOResponse();
            DateTime starttime = DateTime.Now;
            var html = string.Empty;
            var response = string.Empty;
            var req = request.NaturalLanguageQuery;
            var Questiontype = request.QuestionType;

            int attemptCount = 0;
            const int maxAttempts = 2;

            DataTable dt = new DataTable(); 


            if (Questiontype == "Domain")
            {
                while (attemptCount < maxAttempts)
                {
                    dTOResponse = await _aiAPIService.ChatAsync(req.ToString(), Questiontype, request.DomainModel,(attemptCount + 1));

                    if (ContainsDeleteOrUpdate(dTOResponse.ResponseMessage))
                    {
                        break;
                    }
                    else
                    {
                        dt = MPGlobals.GetDataForDatalist(dTOResponse.ResponseMessage);                    
                    }

                    if (dt.Rows.Count > 0)
                    {
                        break ;
                    }

                    attemptCount++;


					using (_dbContext)
					{
						DateTime endtime = DateTime.Now;
						TimeSpan difference = endtime.Subtract(starttime);
						string customString = difference.ToString(@"hh\:mm\:ss");
						NLtoSql_Log nLtoSql_Log = new NLtoSql_Log();
						nLtoSql_Log.SqlQuery = dTOResponse.ResponseMessage;
						nLtoSql_Log.Prompt = dTOResponse.ErrorMessage;
						nLtoSql_Log.Question = req.ToString();
						nLtoSql_Log.UpdatedOn = DateTime.Now;
						nLtoSql_Log.ExecutionTime = customString;
						nLtoSql_Log.RequestedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
						nLtoSql_Log.RetryCount = attemptCount;
						nLtoSql_Log.RequestType = Questiontype;
						_dbContext.Add(nLtoSql_Log);
						_dbContext.SaveChanges();
					}


				}

            }
            else
            {
                dTOResponse = await _aiAPIService.ChatAsync(req.ToString(), Questiontype);


				using (_dbContext)
				{
					DateTime endtime = DateTime.Now;
					TimeSpan difference = endtime.Subtract(starttime);
					string customString = difference.ToString(@"hh\:mm\:ss");
					NLtoSql_Log nLtoSql_Log = new NLtoSql_Log();
					nLtoSql_Log.SqlQuery = dTOResponse.ResponseMessage;
					nLtoSql_Log.Prompt = dTOResponse.ErrorMessage;
					nLtoSql_Log.Question = req.ToString();
					nLtoSql_Log.UpdatedOn = DateTime.Now;
					nLtoSql_Log.ExecutionTime = customString;
					nLtoSql_Log.RequestedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
					nLtoSql_Log.RetryCount = attemptCount;
					nLtoSql_Log.RequestType = Questiontype;
					_dbContext.Add(nLtoSql_Log);
					_dbContext.SaveChanges();
				}
			}
 
            if (Questiontype == "Domain")
            {
                return PartialView("_dtResponse", dt);
            }
            else
            {
                html = Markdown.ToHtml(dTOResponse.ResponseMessage);
                return PartialView("_GeneralResponse", html);
            }

      
        }
 
        public bool ContainsDeleteOrUpdate(string sql)
        {
            string trimmedSql = sql.Trim().ToUpper();
            string pattern = @"\b(DELETE|UPDATE|CREATE)\b";
            return Regex.IsMatch(trimmedSql, pattern);
        }
        public class QueryRequest
        {
            public string NaturalLanguageQuery { get; set; }
            public string QuestionType { get; set; }
			public string DomainModel { get; set; }
		}


		#region Chat Document 
		[HttpGet]
		[ClaimRequirement(UserPermissions.Chat_With_Documents)]
        public IActionResult ChatWithDocument()
        {
            return View();
        }

        public string GetDocResponseLog()
        {
            var loggedInUser = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
            //string Cmdstr = $"dbo.GetNCRData @GetLog ='{getLog}'";
            //DataTable dataTable = MPGlobals.GetDataForDatalist(Cmdstr);
            List<NLtoSql_Log> nLtoSql_Log = _dbContext.NLtoSql_Logs.Where(x => x.RequestType == "Document" && x.RequestedBy == loggedInUser).ToList();
           

            foreach (var nLtoSql in nLtoSql_Log)
            {
               nLtoSql.SqlQuery = Markdown.ToHtml(nLtoSql.SqlQuery);
            }

            return JsonConvert.SerializeObject(nLtoSql_Log, Formatting.Indented);

        }
        [HttpGet]
		[ClaimRequirement(UserPermissions.Doc_Embedding_status)]
        public IActionResult DocumentDashboard()
        {
            return View();
        }
        public JsonResult GetDocumentEmbeddingStaus()
        {
            List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.DocumentationVectorStatusTree_SSP");
                documentsViewModels = db.Read<DocumentsViewModel>().ToList();
            }
            List<DocJsTreeViewModel> docJsTreeViewModel = ContructJsTreeModel(documentsViewModels);           
            return Json(docJsTreeViewModel);
        }
        private List<DocJsTreeViewModel> ContructJsTreeModel(List<DocumentsViewModel> documentsViewModels)
        {
            List<DocJsTreeViewModel> docJsTreeViewModels = new List<DocJsTreeViewModel>();
            DocJsTreeViewModel myArray = new DocJsTreeViewModel();
            myArray.id = "-1";
            myArray.text = "Root";
            myArray.icon = "fa fa-folder";
            myArray.state = new State();
          
                myArray.state.opened = true;
            
            List<Folders> flatObjects = new List<Folders>();
            foreach (var item in documentsViewModels)
            {
                Folders folders = new Folders();
                folders.id = item.Document_Dbkey.ToString();
                folders.text = item.Refrence_Title;
                folders.Parent_id = item.Parent_id;
                folders.item_type = item.Item_type;
                folders.data = item;
                flatObjects.Add(folders);
            }
            myArray.children = FillRecursive(flatObjects, 0);
            docJsTreeViewModels.Add(myArray);
            return docJsTreeViewModels;
        }
        private List<DocJsTreeViewModel> FillRecursive(List<Folders> flatObjects, int parentId)
        {
            var childrenFlatItems = flatObjects.Where(i => i.Parent_id == parentId);
            return childrenFlatItems.Select(i => new DocJsTreeViewModel
            {
                text = i.text,
                id = i.id.ToString(),
                icon = GetIcons(i.item_type),
                state = GetStates(i.data.ReadAccess),
                a_attr = Getattr(i.data.ReadAccess),
                data = i.data,
                children = FillRecursive(flatObjects, int.Parse(i.id)),
            }).ToList();
        }
        private A_attr Getattr(bool access)
        {
            A_attr a_Attr = new A_attr();

            if (access)
            {
                a_Attr.Class = "jstree-anchor jstree-checked Notupdated";
            }
            else
            {
                a_Attr.Class = "jstree-anchor Notupdated";
            }

            return a_Attr;
        }
        private State GetStates(bool access)
        {
            State state = new State();
            if (access)
            {
                state.selected = false;
            }
            else
            {
                state.selected = false;
            }

            return state;
        }

        //public JsonResult GetDocumentEmbeddingStaus()
        //{
        //    List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
        //    using (var connection = mPDapperContext.CreateConnection())
        //    {
        //        var db = connection.QueryMultiple($" dbo.DocumentationVectorStatusTree_SSP");
        //        documentsViewModels = db.Read<DocumentsViewModel>().ToList();
        //    }
        //    List<DocJsTreeViewModel> docJsTreeViewModel = ContructJsTreeModel(documentsViewModels);
           
        //    return Json(docJsTreeViewModel);
        //}
        //private List<DocJsTreeViewModel> ContructJsTreeModel(List<DocumentsViewModel> documentsViewModels)
        //{
        //    List<DocJsTreeViewModel> docJsTreeViewModels = new List<DocJsTreeViewModel>();
        //    DocJsTreeViewModel myArray = new DocJsTreeViewModel();
        //    myArray.id = "-1";
        //    myArray.text = "Root";
        //    myArray.icon = "fa fa-folder";
        //    myArray.state = new State();
           
        //    List<Folders> flatObjects = new List<Folders>();
        //    foreach (var item in documentsViewModels)
        //    {
        //        Folders folders = new Folders();
        //        folders.id = item.Document_Dbkey.ToString();
        //        folders.text = item.Refrence_Title;
        //        folders.Parent_id = item.Parent_id;
        //        folders.item_type = item.Item_type;
        //        folders.data = item;
        //        flatObjects.Add(folders);
        //    }
        //    myArray.children = FillRecursive(flatObjects, 0);
        //    docJsTreeViewModels.Add(myArray);
        //    return docJsTreeViewModels;
        //}
        //private List<DocJsTreeViewModel> FillRecursive(List<Folders> flatObjects, int parentId)
        //{
        //    var childrenFlatItems = flatObjects.Where(i => i.Parent_id == parentId);
        //    return childrenFlatItems.Select(i => new DocJsTreeViewModel
        //    {
        //        text = i.text,
        //        id = i.id.ToString(),
        //        icon = GetIcons(i.item_type),
        //        state = GetStates(i.data.Status_In_VectorDB == 1),
        //        a_attr = Getattr(i.data.Status_In_VectorDB == 1),
        //        data = i.data,
        //        children = FillRecursive(flatObjects, int.Parse(i.id)),
        //    }).ToList();
        //}
        //private A_attr Getattr(bool access)
        //{
        //    A_attr a_Attr = new A_attr();

        //    if (access)
        //    {
        //        a_Attr.Class = "jstree-anchor jstree-checked Notupdated";
        //    }
        //    else
        //    {
        //        a_Attr.Class = "jstree-anchor Notupdated";
        //    }

        //    return a_Attr;
        //}
        //private State GetStates(bool access)
        //{
        //    State state = new State();
        //    if (access)
        //    {
        //        state.selected = false;
        //    }
        //    else
        //    {
        //        state.selected = false;
        //    }

        //    return state;
        //}
        private static string GetIcons(string itemtype)
        {
            string icon = "";
            if (itemtype == "Folder")
            {
                icon = "fa fa-folder small-icon";
            }
            else if (itemtype == "File")
            {
                icon = "fas fa-file-alt small-icon";
            }            
            else
            {
                icon = "fa fa-folder";
            }
            return icon;

        }


        public string GetDocDashBoardData()
        {
            string Cmdstr = $"DocumentsVectorDbStatusList_SSP";
            DataTable dataTable = MPGlobals.GetDataForDatalist(Cmdstr);
            List<DocumentsViewModel> documents = JsonConvert.DeserializeObject<List<DocumentsViewModel>>(JsonConvert.SerializeObject(dataTable));
            return JsonConvert.SerializeObject(documents, Formatting.Indented);
        }

      
        [HttpPost]
        public IActionResult ProceedDocForEmbedding(string DocJSon)
        {
            string result = "";         
            result = SaveToVectoDB.ProccessDocumentsIntoVectorDb(DocJSon);
            return Json(new { response = result });
        }
 
              
           
        [HttpGet]
        public async Task GetDoc_Response(string userPrompt)
        {
            try
            {
                DateTime starttime = DateTime.Now;
                //get document list from database
                int UserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
                using (var connection = mPDapperContext.CreateConnection())
                {
                    var db = connection.QueryMultiple($"dbo.Get_Document_File_By_FolderID_V2 @FolderID = {-1},@parentId='{-1}',@searchtag='{userPrompt}',@userid={UserDbkey}");
                    documentsViewModels = db.Read<DocumentsViewModel>().ToList();
                }
                HashSet<string> docDbKeys = new HashSet<string>();
                if (documentsViewModels != null)
                {
                    foreach (var item in documentsViewModels)
                    {
                        docDbKeys.Add(item.Document_Dbkey.ToString());
                    }
                }


                //Get documents from chromadB (python)
                string GenerateResponsePyPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ChatWithDoc/RetrieveEmbedddings.py");
                string generated_response = "";
                string documentList = "";
                string pythonpath = _pythonPath;
                Response.ContentType = "text/event-stream";
                Response.Headers.Add("Cache-Control", "no-cache");
                Response.Headers.Add("Connection", "keep-alive");
                //Response.Headers["Transfer-Encoding"] = "chunked";

                using (var process = new Process())
                {
                    string arguments = $"\"{GenerateResponsePyPath}\" \"{userPrompt}\""; // Pass both script and userprompt as arguments

                    var psi = new ProcessStartInfo
                    {
                        FileName = System.IO.Path.Combine(pythonpath, "python.exe"),
                        Arguments = arguments, // Arguments include the script path and user input
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    process.StartInfo = psi; // Set the StartInfo to the process
                    process.Start();
                    string result;
                    using (var reader = process.StandardOutput)
                    {
                        StringBuilder outputBuilder = new StringBuilder();
                        char[] buffer = new char[16384]; // Buffer size
                        int bytesRead;

                        // Read the output in a loop until the process ends
                        while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            outputBuilder.Append(buffer, 0, bytesRead);
                            var outputLines = outputBuilder.ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            int i = 0;

                            foreach (var line in outputLines)
                            {
                                if (line.Contains("NoRelevantDocFound"))
                                {
                                    string docDetails = GetFolderName(docDbKeys);
                                    if (docDetails.Contains("[]"))
                                    {
                                        await Response.WriteAsync($"data:!No Relevant documnets found!\n\n"); // Send as JSON
                                        process.Kill();
                                        break; // Exit the loop early
                                    }
                                    string htmlTable = Utilities.HtmlTableGenerator.GenerateHtmlTable(docDetails);
                                    documentList = htmlTable;
                                    await Response.WriteAsync($"data:NoRelevantDocFoundFromPYTHON{htmlTable}\n\n"); // Send as JSON
                                    process.Kill();
                                    break; // Exit the loop early

                                }
                                if (line.StartsWith("docdbkey")) // Assuming your output format starts with a specific identifier
                                {
                                    string docDbkey_py = line;
                                    docDbkey_py = docDbkey_py.Replace("docdbkey", "");
                                   // docDbkey_py = docDbkey_py.Replace("]]", "]");
                                    docDbkey_py = docDbkey_py.Replace("'", "\"");

                                    // Parse the JSON
                                    var jsonArray = JArray.Parse(docDbkey_py);
                                    foreach (var item in jsonArray)
                                    {
                                        string docDbKey = item["docDbKey"].ToString();
                                        docDbKeys.Add(docDbKey);
                                    }
                                    string docDetails = GetFolderName(docDbKeys);
                                    string htmlTable = Utilities.HtmlTableGenerator.GenerateHtmlTable(docDetails);
                                    documentList = htmlTable;
                                    await Response.WriteAsync($"data: {htmlTable}\n\n"); // Send as JSON
                                }
                                else
                                {
                                    i++;
                                    generated_response += line;
                                }
                            }
                            if (i > 0)
                            {
                                await Response.WriteAsync($"data: {generated_response}\n\n");
                            }

                            // Clear the builder for the next read
                            outputBuilder.Clear();
                        }
                    }
                    process.WaitForExit();
                }
                DateTime endtime = DateTime.Now;
                TimeSpan difference = endtime.Subtract(starttime);
                string customString = difference.ToString(@"hh\:mm\:ss");
                NLtoSql_Log nLtoSql_Log = new NLtoSql_Log();
                nLtoSql_Log.SqlQuery = generated_response;
                nLtoSql_Log.Prompt = documentList;
                nLtoSql_Log.Question = userPrompt;
                nLtoSql_Log.UpdatedOn = DateTime.Now;
                nLtoSql_Log.ExecutionTime = customString;
                nLtoSql_Log.RequestedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                nLtoSql_Log.RequestType = "Document";
                _dbContext.Add(nLtoSql_Log);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
          
        }
        public string GetFolderName(HashSet<string> dbkeys)
        {
           List<Documentation> docs = _dbContext.Documentations.ToList();
            int UserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
            List<DocumentsViewModel> folders = new List<DocumentsViewModel>();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.Get_Document_Folders_By_Useraccess @UserID={UserDbkey}");
                folders = db.Read<DocumentsViewModel>().ToList();
            }
            string folderName = "";
            string FileName = "";
           // List<string> docLinks = new List<string>();
            string FileType = "";
            var jsonResults = new List<object>(); // List to hold results
            foreach (var dbkey in dbkeys)
            {
                try
                {
                    var docDetails = docs.Where(x => x.Document_Dbkey == Convert.ToInt32(dbkey)).FirstOrDefault();
                    if (docDetails != null) 
                    {
                        FileName = docDetails.File_Name;
                        var folderDetails = folders.Where(x => x.Document_Dbkey == docDetails.Parent_id).FirstOrDefault();
                        if(folderDetails != null)
                        {
                            folderName = folderDetails.Refrence_Title;
                            if (docDetails.File_type.Contains("pdf"))
                            {
                                FileType = "<i class='far fa-file-pdf text-danger fs-3'></i>";
                            }
                            else
                            {
                                FileType = "<i class='far fa-file text-danger fs-3'></i>";
                            }
                            string doclink = $"{FileType} <a class=\"text-primary\" href='#' onclick='getDoc({docDetails.Document_Dbkey})'>{FileName}</a>";
                            var jsonResponse = new
                            {
                                DocLink = doclink,
                                FolderName = folderName
                            };

                            jsonResults.Add(jsonResponse); // Add to the list
                        }

                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }
            }
            string jsonResult = JsonConvert.SerializeObject(jsonResults);
            return jsonResult;
        }

        // public IActionResult Pythontest()
        public async Task Pythontest()
        {
            string GenerateResponsePyPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ChatWithDoc/TestPy.py");
            //List<string> parameters = new List<string>();
            //parameters.Add("A");
            //parameters.Add("B");
            //parameters.Add("C");
            //var result = PythonInvoker.RunPythonScript(GenerateResponsePyPath, "Generate", parameters);
            //return Content(result, "text/html");

            Response.ContentType = "text/event-stream";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            Response.Headers["Transfer-Encoding"] = "chunked";


            using (var process = new Process())
            {
                process.StartInfo.FileName = "python";
             //   process.StartInfo.Arguments = @"C:\\Users\\dell\\source\\repos\\watermarktest\\watermarktest\\wwwroot\\script.py"; // Adjust the path
                process.StartInfo.Arguments = GenerateResponsePyPath; // Adjust the path
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                using (var reader = process.StandardOutput)
                {
                    string line;
                    int i = 0;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        await Response.WriteAsync($"data: Response - {i}  {DateTime.Now}\n\n");
                        await Response.Body.FlushAsync(); // Send data to the client immediately
                        Console.WriteLine($"data: Response - {i}  {DateTime.Now}\n\n"); // Log the message
                        await Task.Delay(1000); // Simulate delay between messages
                        i++;
                    }
                }

                process.WaitForExit();
            }
        }
        #endregion
    }


}
