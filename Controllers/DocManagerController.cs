using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using System.Data;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using static MPCRS.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using UAParser;
using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text.pdf;
using iTextSharp.text;
using ImageMagick;
using System.Drawing;
using XAct;
using System.Drawing.Imaging;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace MPCRS.Controllers
{
	[Authorize]
	public class DocManagerController : Controller
	{
		private readonly DESI_STFE_PRODContext _dbContext;
		private readonly IConfiguration _configuration;
		private readonly MPDapperContext mPDapperContext;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public DocManagerController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext, IHttpContextAccessor httpContextAccessor)
		{
			_dbContext = context;
			_configuration = configuration;
			this.mPDapperContext = mPDapperContext;
			_httpContextAccessor = httpContextAccessor;
		}

		#region Access Manager
		[HttpGet]
		[ClaimRequirement(UserPermissions.DOC_Write)]
		public ActionResult AccessConfiguration()
		{
			return View();
		}

		[HttpPost]
		[ClaimRequirement(UserPermissions.DOC_Write)]
		public async Task<IActionResult> SaveAccessConfiguration([FromBody] IEnumerable<Documents_Access_Config> docAccessInfos)
		{
			using (_dbContext)
			{
				int updatedUser = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
				foreach (var item in docAccessInfos)
				{
					item.Updated_By = updatedUser;
					item.Updated_On = DateTime.Now;
					if (item.doc_config_dbkey != 0)
					{
						_dbContext.Entry(item).State = EntityState.Modified;
					}
					else
					{
						_dbContext.Documents_Access_Configs.Add(item);
					}
				}
				_dbContext.SaveChanges();
			}
			return Json(new { success = true, Msg = "Saved successfully" });
		}

		[HttpGet]
		[ClaimRequirement(UserPermissions.DOC_Write)]
		public JsonResult GetUserFolderAccess(int UserDbkey)
		{
			List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"dbo.Get_User_Document_Access_Rights @UserDbkey={UserDbkey}");
				documentsViewModels = db.Read<DocumentsViewModel>().ToList();
			}
			List<DocJsTreeViewModel> docJsTreeViewModel = ContructJsTreeModel(documentsViewModels, "AccessConfiguration");
			if (UserDbkey == 0)
			{
				docJsTreeViewModel = new List<DocJsTreeViewModel>();
			}
			return Json(docJsTreeViewModel);
		}
		private List<DocJsTreeViewModel> ContructJsTreeModel(List<DocumentsViewModel> documentsViewModels, string treestructurefor = "")
		{
			List<DocJsTreeViewModel> docJsTreeViewModels = new List<DocJsTreeViewModel>();
			DocJsTreeViewModel myArray = new DocJsTreeViewModel();
			myArray.id = "-1";
			myArray.text = "Root";
			myArray.icon = "fa fa-folder";
			myArray.state = new State();
			if (treestructurefor == "AccessConfiguration")
			{
				myArray.state.opened = true;
			}
			List<Folders> flatObjects = new List<Folders>();
			foreach (var item in documentsViewModels)
			{
				Folders folders = new Folders();
				folders.id = item.Document_Dbkey.ToString();
				folders.text = item.Refrence_Title;
				folders.Parent_id = item.Parent_id;
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
				icon = "fa fa-folder",
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


		#endregion


		#region Documentation old Tree View

		[HttpGet]
		[ClaimRequirement(UserPermissions.DOC_Read)]
		public IActionResult Browser()
		{
			//string userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"];
			//string browserType = "Unknown";


			string userAgentString = HttpContext.Request.Headers["User-Agent"].ToString();
			Parser parser = Parser.GetDefault();
			ClientInfo clientInfo = parser.Parse(userAgentString);
			string browserName = clientInfo.UA.Family;

			ViewBag.isValidBrowser = (browserName == "Chrome" || browserName == "Edge") ? true : false;
            
            return View();
		}

		[HttpGet]
		[ClaimRequirement(UserPermissions.DOC_Read)]
		public JsonResult GetFolders()
		{
			int UserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
			List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"dbo.Get_Document_Folders_By_Useraccess @UserID={UserDbkey}");
				documentsViewModels = db.Read<DocumentsViewModel>().ToList();
			}
			List<DocJsTreeViewModel> docJsTreeViewModel = ContructJsTreeModel(documentsViewModels);
			return Json(docJsTreeViewModel);
		}

		[HttpGet]
		[ClaimRequirement(UserPermissions.DOC_Read)]
		public ActionResult DocsSummary()
		{
			return PartialView();
		}

		[HttpGet]
		[ClaimRequirement(UserPermissions.DOC_Read)]
		public ActionResult GetFilesList(int folderid = -1, int parentid = -1, string searchtags = "")
		{
			int UserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
			List<DocumentsViewModel> documentsViewModels = new List<DocumentsViewModel>();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var db = connection.QueryMultiple($"dbo.Get_Document_File_By_FolderID_V2 @FolderID = {folderid},@parentId='{parentid}',@searchtag='{searchtags}',@userid={UserDbkey}");
				documentsViewModels = db.Read<DocumentsViewModel>().ToList();
			}

			string foldername = string.Empty;
			using (_dbContext)
			{
				if (folderid != -1)
				{
					foldername = _dbContext.Documentations.Where(x => x.Document_Dbkey == folderid).Select(x => x.Refrence_Title).FirstOrDefault();
				}
				else if (parentid != -1)
				{
					foldername = _dbContext.Documentations.Where(x => x.Document_Dbkey == parentid).Select(x => x.Refrence_Title).FirstOrDefault();
				}
				else
				{
					foldername = "Root";
				}
			}

			ViewBag.foldername = foldername;
			return PartialView(documentsViewModels);
		}




        [HttpGet]
		[ClaimRequirement(UserPermissions.DOC_Read)]
		public IActionResult DownloadDocument(int docdbkey)
		{
			List<DocumentsViewModel> documentations = GetDocumentsdata(docdbkey.ToString(), "Document");
			string filename = documentations[0].System_File_Name;
			string UploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Attachments/MISC_DOCS", filename);
			byte[] fileBytes = System.IO.File.ReadAllBytes(UploadPath);
			string Filename = documentations[0].File_Name;
			return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, Filename);
		}


		[HttpGet]
		[ClaimRequirement(UserPermissions.DOC_Write)]
		public IActionResult AccessDetail(int docdbkey)
		{
			List<DocAccessInfo> accessinfo = new List<DocAccessInfo>();
			try
			{
				using (var connection = mPDapperContext.CreateConnection())
				{
					var db = connection.QueryMultiple($"dbo.Get_Document_AccessRights_By_Folder @FolderID={docdbkey}");
					accessinfo = db.Read<DocAccessInfo>().ToList();
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.LogException(ex);
			}

			return PartialView(accessinfo);
		}

        //[HttpPost]
        //[ClaimRequirement(UserPermissions.DOC_Write)]
        //public IActionResult SaveAccessDetail()
        //{
        //	try
        //	{
        //		var JsonData = Request.Form["usraccessdata"];
        //		List<DocAccessInfo> docAccessInfos = JsonConvert.DeserializeObject<List<DocAccessInfo>>(JsonData);
        //		using (_dbContext)
        //		{
        //			StringBuilder Cmdstrs = new StringBuilder();
        //			foreach (var item in docAccessInfos)
        //			{
        //				Documents_Access_Config documents_Access_Config = new Documents_Access_Config();
        //				documents_Access_Config.doc_config_dbkey = item.doc_config_dbkey;
        //				documents_Access_Config.UserDbkey = item.UserDbkey;
        //				documents_Access_Config.Document_Dbkey = item.Document_Dbkey;
        //				documents_Access_Config.ReadAccess = item.ReadAccess;
        //				documents_Access_Config.WriteAccess = item.ReadAccess == false ? false : item.WriteAccess;
        //				documents_Access_Config.DownloadAccess = item.DownloadAccess;
        //				documents_Access_Config.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        //				documents_Access_Config.Updated_On = DateTime.Now;

        //				if (documents_Access_Config.doc_config_dbkey == 0)
        //				{
        //					_dbContext.Documents_Access_Configs.Add(documents_Access_Config);
        //				}
        //				else
        //				{
        //					_dbContext.Entry(documents_Access_Config).State = EntityState.Modified;
        //				}

        //				if (documents_Access_Config.ReadAccess == true)
        //				{
        //					Cmdstrs.Append($"dbo.UpdateDocumentAccessRights @UserId = {item.UserDbkey}, @FolderID={documents_Access_Config.Document_Dbkey},@Accesstype = 1,@AccessRight = 1;");
        //				}
        //				if (documents_Access_Config.WriteAccess == true)
        //				{
        //					Cmdstrs.Append($"dbo.UpdateDocumentAccessRights @UserId = {item.UserDbkey}, @FolderID={documents_Access_Config.Document_Dbkey},@Accesstype = 2,@AccessRight = 1;");
        //				}
        //			}
        //			_dbContext.SaveChanges();
        //			MPGlobals.ExceSQLNonQuery(Cmdstrs.ToString());
        //		}
        //		return Json(new { success = true, Msg = "Saved successfully" });
        //	}
        //	catch (Exception ex)
        //	{
        //		return Json(new { success = false, Msg = ex.Message });
        //	}

        //}

        [HttpPost]
        [ClaimRequirement(UserPermissions.DOC_Write)]
        public IActionResult SaveAccessDetail()
        {
            try
            {
                string jsonData = Request.Form["usraccessdata"].ToString();

                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    return Json(new { success = false, Msg = "No access data found." });
                }

                int updatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                _dbContext.Database.SetCommandTimeout(180);

                var jsonParam = new SqlParameter("@JsonData", jsonData);
                var updatedByParam = new SqlParameter("@UpdatedBy", updatedBy);

                _dbContext.Database.ExecuteSqlRaw(
                    "EXEC dbo.UpdateDocumentAccessRights_v2 @JsonData, @UpdatedBy",
                    jsonParam,
                    updatedByParam
                );

                return Json(new { success = true, Msg = "Saved successfully" });
            }
            catch (Exception ex)
            {
				ErrorHandler.LogException(ex);
                return Json(new { success = false, Msg = ex.Message });
            }
        }

        [HttpGet]
		[ClaimRequirement(UserPermissions.DOC_Read)]
		public ActionResult Folder(int docdbkey = 0, String actionon = "CreateFolder")
		{
			int Userdbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
			DocumentsViewModel documentsViewModel = new DocumentsViewModel();
			using (_dbContext)
			{
				try
				{
					var hasWritePermission = UserData.IsAuthorized(User, Constants.UserPermissions.DOC_Write);
					Documents_Access_Config documents_Access_Config = _dbContext.Documents_Access_Configs.AsNoTracking().Where(x => x.Document_Dbkey == docdbkey && x.UserDbkey == Userdbkey).FirstOrDefault();

					if (hasWritePermission == false)
					{
						if (documents_Access_Config != null)
						{
							if (documents_Access_Config.WriteAccess == false || documents_Access_Config.WriteAccess == null)
							{
								return new ForbidResult();
							}
						}
					}


					Documentation documentation = _dbContext.Documentations.AsNoTracking().Where(x => x.Document_Dbkey == docdbkey).FirstOrDefault();
					if (documentation != null)
					{
						documentsViewModel.Parent = documentation.Refrence_Title;
					}
					if (actionon == "CreateFolder")
					{
						documentsViewModel.Item_type = "Folder";
						documentsViewModel.Parent_id = docdbkey == -1 ? 0 : docdbkey;
					}
					else if (actionon == "UploadFile")
					{
						documentsViewModel.Item_type = "File";
						documentsViewModel.Parent_id = docdbkey;
					}
					else if (actionon == "UpdateFolder" || actionon == "RenameFile")
					{
						documentsViewModel.Document_Dbkey = documentation.Document_Dbkey;
						documentsViewModel.Refrence_Title = documentation.Refrence_Title;
						documentsViewModel.Description = documentation.Description;
						documentsViewModel.Item_type = documentation.Item_type;
						documentsViewModel.File_Location = documentation.File_Location;
						documentsViewModel.File_Name = documentation.File_Name;
						documentsViewModel.System_File_Name = documentation.System_File_Name;
						documentsViewModel.File_Size = documentation.File_Size;
						documentsViewModel.File_type = documentation.File_type;
						documentsViewModel.Approved_Status = documentation.Approved_Status;
						documentsViewModel.Parent_id = documentation.Parent_id == -1 ? 0 : documentation.Parent_id;
						//documentsViewModel.Updated_By = documentation.Updated_By;
						//documentsViewModel.Updated_On = documentation.Updated_On;
						documentsViewModel.is_required_approve = documentation.is_required_approve ?? false;
						//documentsViewModel.Approved_by = documentation.Approved_by;
						documentsViewModel.is_active = documentation.is_active;
						documentsViewModel.SearchTags = documentation.SearchTags;
						documentsViewModel.Note = documentation.Note;
						documentsViewModel.Inherit_Parent_Access = documentation.Inherit_Parent_Access ?? false;
						documentsViewModel.Inherit_Access_From = documentation.Document_Dbkey;
						var parent = _dbContext.Documentations.AsNoTracking().Where(x => x.Document_Dbkey == documentsViewModel.Parent_id).FirstOrDefault();
						documentsViewModel.Parent = parent != null ? parent.Refrence_Title : null;
					}
				}
				catch (Exception ex)
				{

					ErrorHandler.LogException(ex);
				}

			}

			return PartialView(documentsViewModel);
		}


		[HttpPost]
		[ClaimRequirement(UserPermissions.DOC_Read)]
		public ActionResult SaveDocumentDetail(DocumentsViewModel documentsViewModel)
		{
			try
			{
                int Userdbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
                bool isEmailTriffered = true;
				int docdbkey = 0;
				using (_dbContext)
				{
					List<Documentation> documentations = new List<Documentation>();
					documentations = ConstructDocumentModel(documentsViewModel);

					foreach (var Docmodel in documentations)
					{
						if (Docmodel.Document_Dbkey == 0)
						{
							_dbContext.Documentations.Add(Docmodel);
							_dbContext.SaveChanges();

							if (Docmodel.Item_type == "File")
							{
								// Trigger mail Send only first time
								if (isEmailTriffered == false)
								{
									//SendDocumentUploadedMail(Docmodel);
								}
								isEmailTriffered = true;
							}
						}
						else
						{
							_dbContext.Entry(Docmodel).State = EntityState.Modified;
							_dbContext.SaveChanges();
						}

						if (Docmodel.Item_type == "Folder")
						{
							int InheritParent = 0;
							if (Docmodel.Inherit_Parent_Access ?? false == true)
							{
								//  MPGlobals.ExceSQLNonQuery($"dbo.Inherit_Folder_Write_Acess @FolderID={Docmodel.Document_Dbkey}");
								InheritParent = 1;
							}
							MPGlobals.ExceSQLNonQuery($"dbo.Inherit_Folder_Write_Acess_Updated @FolderID={Docmodel.Document_Dbkey}, @UserDbKey={Userdbkey}, @InheritParent={InheritParent}");
						}
						docdbkey = Docmodel.Document_Dbkey;
					}
					return Json(new { success = true, Msg = "Saved Succesully", docdbkey = docdbkey });
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, Msg = ex.Message });
			}

		}

		[HttpGet]
		[ClaimRequirement(UserPermissions.DOC_Read)]
		public ActionResult Rename(int Document_Dbkey)
		{
			DocumentsViewModel vm = new();
			if (Document_Dbkey != 0)
			{
				using (_dbContext)
				{
					try
					{
						Documentation docu = _dbContext.Documentations.AsNoTracking().Where(x => x.Document_Dbkey == Document_Dbkey).FirstOrDefault();
						if (docu != null)
						{
							vm.Document_Dbkey = docu.Document_Dbkey;
							vm.Description = docu.Description;
							vm.Refrence_Title = docu.Refrence_Title;
							vm.SearchTags = docu.SearchTags;
							//vm = JsonConvert.DeserializeObject<DocumentsViewModel>(JsonConvert.SerializeObject(docu));
						}
					}
					catch (Exception ex)
					{
						ErrorHandler.LogException(ex);
					}

				}
			}
			return PartialView(vm);
		}
		[HttpPost]
		public IActionResult Rename(DocumentsViewModel vm)
		{
			using (_dbContext)
			{
				try
				{
					Documentation docu = _dbContext.Documentations.AsNoTracking().Where(x => x.Document_Dbkey == vm.Document_Dbkey).FirstOrDefault();
					if (docu != null)
					{
						docu.Document_Dbkey = vm.Document_Dbkey;
						docu.Description = vm.Description;
						docu.Refrence_Title = vm.Refrence_Title;
						docu.SearchTags = vm.SearchTags;
						_dbContext.Entry(docu).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
						_dbContext.SaveChanges();
					}
				}
				catch (Exception ex)
				{
					ErrorHandler.LogException(ex);
					return Json(new { success = false, msg = ex.Message });
				}

			}
			return Json(new { success = true, msg = "Updated Successfully" });
		}

		[HttpPost]
		[ClaimRequirement(UserPermissions.DOC_Write)]
		public IActionResult DocDelete(int Document_Dbkey)
		{
			if (Document_Dbkey != 0)
			{
				using (_dbContext)
				{
					try
					{
						int Userdbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
						Documentation docu = _dbContext.Documentations.AsNoTracking().Where(x => x.Document_Dbkey == Document_Dbkey).FirstOrDefault();
						if (docu != null)
						{
							docu.is_active = 0;
							docu.Updated_On = DateTime.Now;
							docu.Updated_By = Userdbkey;
							_dbContext.Entry(docu).State = EntityState.Modified;
							_dbContext.SaveChanges();

						}
					}
					catch (Exception ex)
					{
						return Json(new { success = false, msg = ex.Message });
					}

				}
			}
			return Json(new { success = true, msg = "Deleted Successfully.." });
		}

		[HttpPost]
		[ClaimRequirement(UserPermissions.DOC_Delete_Folder)]
		public IActionResult FolderDelete(int Document_Dbkey)
		{
			if (Document_Dbkey != 0)
			{
				using (_dbContext)
				{
					try
					{
						int Userdbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
						Documentation docu = _dbContext.Documentations.AsNoTracking().Where(x => x.Document_Dbkey == Document_Dbkey).FirstOrDefault();
						if (docu != null)
						{
							docu.is_active = 0;
							docu.Updated_On = DateTime.Now;
							docu.Updated_By = Userdbkey;
							_dbContext.Entry(docu).State = EntityState.Modified;
							_dbContext.SaveChanges();

						}
					}
					catch (Exception ex)
					{
						return Json(new { success = false, msg = ex.Message });
					}

				}
			}
			return Json(new { success = true, msg = "Deleted Successfully.." });
		}


		[HttpPost]
		[ClaimRequirement(UserPermissions.DOC_Read)]
		public IActionResult Approve(int Document_Dbkey)
		{
			if (Document_Dbkey != 0)
			{
				using (_dbContext)
				{
					try
					{
						Documentation docu = _dbContext.Documentations.AsNoTracking().Where(x => x.Document_Dbkey == Document_Dbkey).FirstOrDefault();
						if (docu != null)
						{
							docu.Approved_Status = 1;
							_dbContext.Entry(docu).State = EntityState.Modified;
							_dbContext.SaveChanges();
						}
					}
					catch (Exception ex)
					{
						return Json(new { success = false, msg = ex.Message });
					}

				}
			}
			return Json(new { success = true, msg = "Approved Succesfully" });
		}

		[HttpPost]
		[ClaimRequirement(UserPermissions.DOC_Read)]
		public IActionResult Reject(int Document_Dbkey)
		{
			if (Document_Dbkey != 0)
			{
				using (_dbContext)
				{
					try
					{
						Documentation docu = _dbContext.Documentations.AsNoTracking().Where(x => x.Document_Dbkey == Document_Dbkey).FirstOrDefault();

						if (docu != null)
						{
							docu.Approved_Status = -1;
							_dbContext.Entry(docu).State = EntityState.Modified;
							_dbContext.SaveChanges();
						}
					}
					catch (Exception ex)
					{
						return Json(new { success = false, msg = ex.Message });
					}

				}
			}
			return Json(new { success = true, msg = "Approval Rejected Succesfully" });
		}

		private bool CheckApprvalRequired(int parent_id)
		{
			bool value;
			try
			{
				if (parent_id != 0)
				{
					value = bool.Parse(MPGlobals.GetOnedata(@"select [is_required_approve] FROM  [dbo].[Documentation]  where [Document_Dbkey]=" + parent_id + " "));
					return value;
				}
				else
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.LogException(ex);
				return false;
			}



		}

		private List<Documentation> ConstructDocumentModel(DocumentsViewModel dmvm)
		{
			List<Documentation> datamodel = new List<Documentation>();
			if (dmvm.Item_type == "Folder")
			{
				try
				{
					Documentation Docmodel = new Documentation();
					Docmodel.Document_Dbkey = dmvm.Document_Dbkey;
					Docmodel.Refrence_Title = dmvm.Refrence_Title;
					Docmodel.Description = dmvm.Description;
					Docmodel.is_required_approve = dmvm.is_required_approve;
					Docmodel.Approved_by = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
					Docmodel.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
					Docmodel.Updated_On = DateTime.Now;
					Docmodel.Item_type = dmvm.Item_type;
					Docmodel.Parent_id = dmvm.Parent_id;
					Docmodel.Inherit_Parent_Access = dmvm.Inherit_Parent_Access;
					datamodel.Add(Docmodel);
				}
				catch (Exception ex)
				{
					ErrorHandler.LogException(ex);
				}

				return datamodel;
			}
			else
			{
				if (dmvm.Files.Count != 0)
				{
					try
					{
						foreach (var item in dmvm.Files)
						{
							Documentation Docmodel = new Documentation();
							Docmodel.Document_Dbkey = dmvm.Document_Dbkey;
							Docmodel.Refrence_Title = dmvm.Refrence_Title;
							Docmodel.Description = dmvm.Description;
							Docmodel.is_required_approve = dmvm.is_required_approve;
							Docmodel.Approved_by = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
							Docmodel.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
							Docmodel.Updated_On = DateTime.Now;
							Docmodel.Item_type = dmvm.Item_type;
							Docmodel.Parent_id = dmvm.Parent_id;
							Docmodel.SearchTags = dmvm.SearchTags;
							Docmodel.Note = dmvm.Note;

							if (CheckApprvalRequired(Docmodel.Parent_id))
							{
								Docmodel.Approved_Status = 0;
							}
							else
							{
								Docmodel.Approved_Status = 1;
							}

							string path = "";
							string FileExtension = System.IO.Path.GetExtension(item.FileName);
							string FileName = Path.GetFileNameWithoutExtension(item.FileName);
							path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Attachments/MISC_DOCS");

							Regex reg = new Regex("[*'\",_&#^@]");
							var guid = Guid.NewGuid();
							FileName = reg.Replace(FileName, "_");
							string originalFileName = FileName.Trim() + FileExtension;
							string SystemFileName = guid + FileExtension;

							if (!Directory.Exists(path))
							{
								Directory.CreateDirectory(path);
							}

							path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Attachments/MISC_DOCS", SystemFileName);

							using (FileStream fs = System.IO.File.Create(path))
							{
								item.CopyTo(fs);
							}
							string File_loc = path + SystemFileName;


							Docmodel.File_Location = "/Attachments/MISC_DOCS/";
							Docmodel.File_Name = originalFileName;
							Docmodel.System_File_Name = SystemFileName;
							Docmodel.File_Size = item.Length.ToString();
							Docmodel.File_type = FileExtension;

							datamodel.Add(Docmodel);
						}
					}
					catch (Exception ex)
					{
						ErrorHandler.LogException(ex);
					}

				}
				return datamodel;
			}
		}

        [ClaimRequirement(UserPermissions.DOC_Dnd)]
        public ActionResult DndDocFolder(int id, int parent)
        {
            try
            {
                // Get current user ID
                int userId = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

                using (var connection = mPDapperContext.CreateConnection())
                {
                    // Setup parameters for stored procedure
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("DocumentID", id);
                    parameters.Add("NewParentID", parent);
                    parameters.Add("UserID", userId);

                    // Call the stored procedure
                    var result = connection.QueryFirstOrDefault<dynamic>(
                        "DndDocument",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    // Check if SP returned a result
                    if (result == null)
                    {
                        return Json(new { success = false, msg = "No response from database" });
                    }

                    // Check success status from SP
                    if (result.Success == 1)
                    {
                        return Json(new
                        {
                            success = true,
                            msg = result.Message,
                            itemType = result.ItemType,
                            itemName = result.ItemName,
                            sourceFolder = result.SourceFolder,
                            destinationFolder = result.DestinationFolder
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            msg = result.Message
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false, msg = "Error: " + ex.Message });
            }
        }

        #endregion


        #region Documentation taxanomy js tree structure
        [HttpGet]
		[ClaimRequirement(UserPermissions.DOC_Read)]
		public IActionResult Index()
		{
			return View();
		}

		[HttpGet]
		[ClaimRequirement(UserPermissions.DOC_Read)]
		public IActionResult Detail(int docdbkey)
		{
			List<DocumentsViewModel> documentations = GetDocumentsdata(docdbkey.ToString(), "Document");
			return PartialView(documentations[0]);
		}

		public async Task<string> GetDocuments(string id = "0", string type = "Parent", string searchtags = "")
		{
			List<DocumentsViewModel> documentations = GetDocumentsdata(id, type, searchtags);
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				string str = @"""results""";
				var jsonData = JsonConvert.SerializeObject(documentations);
				stringBuilder.Append("{" + str + ":");
				stringBuilder.Append(jsonData);
				stringBuilder.Append("}");
			}
			catch (Exception ex)
			{
				ErrorHandler.LogException(ex);
			}

			return stringBuilder.ToString();

		}

		private List<DocumentsViewModel> GetDocumentsdata(string id = "0", string type = "Parent", string searchtags = "")
		{
			List<DocumentsViewModel> documentations = new List<DocumentsViewModel>();
			try
			{
				int userdbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
				using (var connection = mPDapperContext.CreateConnection())
				{
					var db = connection.QueryMultiple($"[dbo].[GetDocumentsData] @docId={id},@option = '{type}',@searchtags = '{searchtags}',@requestedby={userdbkey}");
					documentations = db.Read<DocumentsViewModel>().ToList();
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.LogException(ex);
			}
			return documentations;
		}


        #endregion


        #region Load files in Tree View

        [HttpGet]
        [ClaimRequirement(UserPermissions.DOC_Read)]
        public JsonResult GetFolderChildren(int folderId)
        {
            int UserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
            List<DocumentsViewModel> filesViewModels = new List<DocumentsViewModel>();

            // Get files for this folder using existing stored procedure
            using (var connection = mPDapperContext.CreateConnection())
            {
                var db = connection.QueryMultiple($"dbo.Get_Document_File_By_FolderID_V2 @FolderID = {folderId},@parentId=-1,@searchtag='',@userid={UserDbkey}");
                filesViewModels = db.Read<DocumentsViewModel>().ToList();
            }

            // Convert to jsTree format
            List<DocJsTreeViewModel> fileNodes = filesViewModels.Select(file => new DocJsTreeViewModel
            {
                id = file.Document_Dbkey.ToString(),
                text = file.Refrence_Title,
                icon = GetFileIcon(file.File_type),
                state = new State { selected = false },
                a_attr = new A_attr { Class = "jstree-anchor file-node" },
                data = file,
                children = new List<DocJsTreeViewModel>() // Files have no children
            }).ToList();

            return Json(fileNodes);
        }

        // Helper method to get appropriate icon class based on file type
        private string GetFileIcon(string fileType)
        {
            if (string.IsNullOrEmpty(fileType))
                return "far fa-file";

            fileType = fileType.ToLower();

            if (fileType.Contains("pdf"))
                return "far fa-file-pdf text-danger";
            else if (fileType.Contains("xls") || fileType.Contains("csv"))
                return "far fa-file-excel text-success";
            else if (fileType.Contains("doc"))
                return "far fa-file-word text-primary";
            else if (fileType.Contains("ppt"))
                return "far fa-file-powerpoint text-warning";
            else if (fileType.Contains("zip") || fileType.Contains("rar"))
                return "far fa-file-archive";
            else if (fileType.Contains("jpg") || fileType.Contains("png") || fileType.Contains("gif") || fileType.Contains("jpeg"))
                return "far fa-file-image";
            else
                return "far fa-file";
        }

        #endregion

    }
}
