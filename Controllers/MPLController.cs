// Ignore Spelling: Json

using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPCRS.Models;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using OfficeOpenXml.Table;
using OfficeOpenXml;
using System.Data;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using static MPCRS.Utilities.Constants;
using Microsoft.AspNetCore.Http.Json;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using WebGrease.Css;
using ImageMagick;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.Data.SqlClient;
using System.Management.Automation.Language;
using System.Net.Mail;
using Newtonsoft.Json.Linq;



namespace MPCRS.Controllers
{
	[Authorize]
	public class MPLController : Controller
	{
		private readonly DESI_STFE_PRODContext _dbContext;
		private readonly IConfiguration _configuration;
		private readonly MPDapperContext mPDapperContext;

		public MPLController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
		{
			_dbContext = context;
			_configuration = configuration;
			this.mPDapperContext = mPDapperContext;

		}

		[HttpGet]
		[ClaimRequirement(UserPermissions.MPL_Read)]
		public IActionResult MasterPartList(int BaselineEngineKey = 0)
		{
			ViewBag.BaselineEngineKey = BaselineEngineKey;
			return View();
		}

		public JsonResult GetMplDetailList(int BaselineEngineKey = 0, int EngineKey = 0)
		{
			DataTable dataTable = MPGlobals.GetDataForDatalist("dbo.Get_MPL @BL_Engine_Dbkey = " + BaselineEngineKey + ",@Engine_Dbkey = " + EngineKey + "");
			return Json(MPGlobals.GetTableAsList(dataTable));
		}


		public JsonResult GetMasterPartList(int BaselineEngineKey = 0, int isactive = 1)
		{
			List<MplJsTreeViewModel> mplJsTrees = GetJstreeList(BaselineEngineKey, isactive);
			return Json(mplJsTrees);
		}



		private List<MplJsTreeViewModel> GetJstreeList(int BaselineEngineKey, int isactive = 1)
		{
			List<BaseLineEngineVM> baseLineEngineVMs = new List<BaseLineEngineVM>();
			List<MplTreeViewModel> mplTreeViewModels = new List<MplTreeViewModel>();
			BaseLineEngineVM baselineformpl = new BaseLineEngineVM();
			using (var connection = mPDapperContext.CreateConnection())
			{
				var baselineengine = connection.QueryMultiple($"dbo.GetBaseLineEngines");
				baseLineEngineVMs = baselineengine.Read<BaseLineEngineVM>().ToList();
				if (BaselineEngineKey == 0)
				{
					baselineformpl = baseLineEngineVMs.Where(x => x.is_active == "Active").FirstOrDefault();
				}
				else
				{
					baselineformpl = baseLineEngineVMs.Where(x => x.BL_Engine_Dbkey == BaselineEngineKey).FirstOrDefault();
				}
				var mpl = connection.QueryMultiple($"[dbo].[Get_MPL_TreeData] @BL_Engine_Db_key={BaselineEngineKey},@isactive = {isactive}");
				mplTreeViewModels = mpl.Read<MplTreeViewModel>().ToList();
			}
			List<MplJsTreeViewModel> mplJsTrees = ContructMPLJsTreeModel(baselineformpl, mplTreeViewModels);
			return mplJsTrees;
		}

		private List<MplJsTreeViewModel> ContructMPLJsTreeModel(BaseLineEngineVM baseLineEngineVM, List<MplTreeViewModel> mplTreeViewModels)
		{
			List<MplJsTreeViewModel> masterparts_Jstrees = new List<MplJsTreeViewModel>();
			MplJsTreeViewModel myArray = new MplJsTreeViewModel();
			myArray.id = "0" + "_" + "0";
			myArray.text = baseLineEngineVM.Engine_Title.ToString();
			myArray.icon = "fa fa-fighter-jet";
			myArray.state = new State();
			myArray.state.opened = true;
			List<Category> flatObjects = new List<Category>();

			foreach (MplTreeViewModel enginePartsViewModel in mplTreeViewModels)
			{
				Category category = new Category();
				category.id = enginePartsViewModel.Engine_Part_Dbkey.ToString() + "_" + enginePartsViewModel.Part_relation_dbkey.ToString();
				category.text = enginePartsViewModel.Type_Part_Name;
				category.isactive = enginePartsViewModel.is_active ?? 0;
				category.Parent_id = enginePartsViewModel.Parent_id;
				category.data = enginePartsViewModel;
				flatObjects.Add(category);
			}
			myArray.children = FillRecursive(flatObjects, 0);
			masterparts_Jstrees.Add(myArray);
			return masterparts_Jstrees;
		}

		private static List<MplJsTreeViewModel> FillRecursive(List<Category> flatObjects, int? parentId = null, int id = 0)
		{
			var childrenFlatItems = flatObjects.Where(i => i.Parent_id == parentId);

			return childrenFlatItems.Select(i => new MplJsTreeViewModel
			{
				text = i.text,
				id = i.id.ToString(),
				icon = "fa fa-cogs",
				state = GetStates(i.isactive),
				a_attr = Getattr(i.isactive, i.Isupdated, i.ForSopOnly),
                data = i.data,     
				children = FillRecursive(flatObjects, int.Parse(i.id.Split("_")[0]), id),
            }).ToList();

		}

		private static A_attr Getattr(int isactive, bool? isupdated, bool? soppartonly)
		{
			A_attr a_Attr = new A_attr();
			try
			{
				if (isactive == 0)
				{
					a_Attr.Class = "jstree-anchor";
				}
				else
				{
					a_Attr.Class = "jstree-anchor jstree-clicked";
				}
			}
			catch (Exception ex) { ErrorHandler.LogException(ex); }
			return a_Attr;
		}

		private static State GetStates(int id)
		{
			State state = new State();
			state.opened = true;
			try
			{
				if (id == 0)
				{
					state.selected = false;
				}
				else
				{
					state.selected = true;
				}
			}

			catch (Exception ex) { ErrorHandler.LogException(ex); }
			return state;
		}

		[HttpGet]
		[ClaimRequirement(UserPermissions.MPL_Report_Read)]
		public ActionResult Report(string key = "0")
		{
			ReportData reportData = new ReportData();
			try
			{
				using (var connection = mPDapperContext.CreateConnection())
				{
					var mplreport = connection.QueryMultiple($"[dbo].[MPLReport_New_Beta] @BL_Engine_Dbkey={key}");
					reportData.mPLReportDatas = mplreport.Read<MPLReportData>().ToList();

					var approvers = connection.QueryMultiple($"[dbo].[Get_BL_Engine_Approvers]");
					reportData.bL_Engine_Approvers = approvers.Read<BL_Engine_Approvers>().ToList();
				}
				ViewBag.key = key;
				return View(reportData);
			}
			catch (Exception ex)
			{
				ErrorHandler.LogException(ex);
				return View(reportData);
			}

		}
		// BAseLine Additional info (for MPL report) AddApprover 

		[HttpGet]
		[ClaimRequirement(UserPermissions.BaseLineEngine_Write)]
		public IActionResult AddApprovers(int BlApproversDbKey, int BlEngineDbKey)
		{

			BaseLineEngineApproversVm VM = new BaseLineEngineApproversVm();
			if (BlApproversDbKey == 0)
			{
				VM.BL_Approvers_Dbkey = BlApproversDbKey;
				VM.BL_Engine_Dbkey = BlEngineDbKey;
			}
			if (BlApproversDbKey != 0)
			{
				using (_dbContext)
				{
					var baseLineEnginesApproverModel = _dbContext.Base_Line_Engines_Approvers.AsNoTracking().Where(x => x.BL_Approvers_Dbkey == BlApproversDbKey).FirstOrDefault();
					try
					{
						if (baseLineEnginesApproverModel != null)
						{
							VM = JsonConvert.DeserializeObject<BaseLineEngineApproversVm>(JsonConvert.SerializeObject(baseLineEnginesApproverModel));
						}
					}
					catch (Exception ex)
					{
						ErrorHandler.LogException(ex);
					}
				}
			}
			return View(VM);

		}

		[HttpPost]
		[ClaimRequirement(UserPermissions.BaseLineEngine_Write)]
		public IActionResult AddApprovers(BaseLineEngineApproversVm VM)
		{
			DTOResponse dTOResponse = new DTOResponse();
			int? dbkey;
			//if (ModelState.IsValid)
			//{
			using (DESI_STFE_PRODContext db = new())
			{
				try
				{
					Base_Line_Engines_Approver bdmodel = new Base_Line_Engines_Approver();
					bdmodel.BL_Approvers_Dbkey = VM.BL_Approvers_Dbkey;
					bdmodel.BL_Engine_Dbkey = VM.BL_Engine_Dbkey;
					bdmodel.Role_Dbkey = VM.Role_Dbkey;
					bdmodel.Module_Dbkey = VM.Module_Dbkey;
					bdmodel.Person_Dbkey = VM.Person_Dbkey;
					bdmodel.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
					bdmodel.Updated_On = DateTime.Now;
					//bdmodel.Updated_By_UserGuid = (User.FindFirst(ClaimTypes.NameIdentifier)?.Value).ToString();
					if (bdmodel.BL_Approvers_Dbkey == 0)
					{
						db.Base_Line_Engines_Approvers.Add(bdmodel);
						db.SaveChanges();
						dTOResponse.Result = true;

						dTOResponse.ResponseMessage = "Saved Successfully";
					}
					else
					{
						db.Entry(bdmodel).State = EntityState.Modified;
						db.SaveChanges();
						dTOResponse.Result = true;

						dTOResponse.ResponseMessage = "Updated Successfully";
					}
					dbkey = bdmodel.BL_Engine_Dbkey;
				}
				catch (Exception ex)
				{
					//Logger.WriteToFile(ex.Message);
					//Logger.WriteToFile(ex.StackTrace);
					throw;
				}
			}

			return Json(new { success = true, Msg = "Saved Successfully", Dbkey = dbkey });
			// return RedirectToAction("ApproverList", new { BlApproversDbKey = dbkey });
			//}
			//else
			//{
			//    //return Json(new { success = false, Msg = "Invalid Model state" }, JsonRequestBehavior.AllowGet);
			//    return BadRequest(new { success = false, Msg = "Invalid Model state" });
			//}

		}

		[HttpGet]
		[ClaimRequirement(UserPermissions.BaseLineEngine_Read)]
		public IActionResult ApproverList(int BlEngineDbKey = 0)
		{

			ViewBag.BlEngineDbkey = BlEngineDbKey;


			if (BlEngineDbKey == 0)
			{
				return RedirectToAction("/SOPManagement/BaseLineEngines");
			}
			BaseLineEngineApproversVm VM = new BaseLineEngineApproversVm();
			VM.BL_Engine_Dbkey = BlEngineDbKey;
			string cmd = @"dbo.Get_BL_Engine_Approvers @BL_Engine_Dbkey=" + BlEngineDbKey;
			DataTable dt = MPGlobals.GetDataForDatalist(cmd);
			List<BaseLineEngineApproversVm> ApproversVm = MPGlobals.ConvertDataTable<BaseLineEngineApproversVm>(dt);
			if (ApproversVm.Count != 0)
			{
				ApproversVm[0].BaseLineEngine = MPGlobals.ConvertDataTable<BaseLineEngineApproversVm>(dt);
				return View(ApproversVm[0]);
			}
			else
			{
				VM.BaseLineEngine = new List<BaseLineEngineApproversVm>();
				return View(VM);
			}
		}


		[HttpPost]
		[ClaimRequirement(UserPermissions.BaseLineEngine_Write)]
		public IActionResult RemoveApprover(int BlApproversDbKey = 0)
		{
			MPGlobals.ExceSQLNonQuery("Delete FROM  [dbo].[Base_Line_Engines_Approvers] where [BL_Approvers_Dbkey]= '" + BlApproversDbKey + "'");
			return Json(new { success = true, message = "Removed Successfully" });
			//return Ok(new { success = true, message = "Removed Successfully" });
		}


		[ClaimRequirement(UserPermissions.MPL_Report_Read)]
		public ActionResult ExcelExportMPL(int id = 0, string colbs = "")
		{

			List<MPLReportData> Model = new List<MPLReportData>();
			Model = MPGlobals.ConvertDataTable<MPLReportData>(MPGlobals.GetDataForDatalist("[dbo].[MPLReport_New_Beta]"));
			var memorystream = new MemoryStream();
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
			using (var Excelpackage = new ExcelPackage(memorystream))
			{


				var ReportGroupIdentifiers = Model.Where(x => x.IsParent != "yes" && x.ReportGroupIdentifier != null && x.ReportGroupIdentifier != "_0").Select(x => x.ReportGroupIdentifier).Distinct().OrderBy(x => x);
				List<MPLReportData> groups = new List<MPLReportData>();
				foreach (var group in ReportGroupIdentifiers)
				{
					MPLReportData orderitem = new MPLReportData();
					orderitem.ReportGroupIdentifier = group;
					orderitem.ReportDisplayOrder = 0;
					if (group.Contains("Direct") == false)
					{
						int thisParent = int.Parse(group.Split('_')[group.Split('_').Length - 1]);
						orderitem.ReportDisplayOrder = Model.Where(x => x.Engine_Part_Dbkey == thisParent).Select(x => x.ReportDisplayOrder).FirstOrDefault();
					}
					groups.Add(orderitem);
				}

				foreach (var groupItem in groups.OrderBy(x => x.ReportDisplayOrder))
				{
					// Entire Table Contruction;
					int reducer = 0;
					int MaXlevel = Model.Where(x => x.ReportGroupIdentifier == groupItem.ReportGroupIdentifier).Select(x => x.Level).Max();
					string PreviousGroup = "";
					string PageHeading = "PW00000 - DIRECT PARTS";
					string filterParents = "Yes";
					string headingMainAssembly = "PW00000";
					bool filterDirctParts = true;

					if (groupItem.ReportGroupIdentifier.Contains("Direct") == false)
					{
						filterDirctParts = false;
						MaXlevel = MaXlevel - 1;
						reducer = 1;
						filterParents = "No";
						int AssemblyParentID = int.Parse(groupItem.ReportGroupIdentifier.Split('_')[groupItem.ReportGroupIdentifier.Split('_').Length - 1]);
						PageHeading = Model.Where(x => x.Engine_Part_Dbkey == AssemblyParentID).Select(x => (x.Draw_part_no + " - " + x.Description)).FirstOrDefault();
						headingMainAssembly = Model.Where(x => x.Engine_Part_Dbkey == AssemblyParentID).Select(x => (x.Draw_part_no)).FirstOrDefault();
						string collaborators = Model.Where(x => x.Engine_Part_Dbkey == AssemblyParentID).Select(x => (x.Collaborators)).FirstOrDefault();
					}

					// Creating Excel Sheet for for Direct Parts and each Assembly 
					var WorkSheet = Excelpackage.Workbook.Worksheets.Add(headingMainAssembly);
					DataTable dtAssy = new DataTable();

					for (int i = 0; i < MaXlevel; i++)
					{
						dtAssy.Columns.Add("Drawing No L" + (i + 1), typeof(string));
					}

					dtAssy.Columns.Add("Description", typeof(string));
					dtAssy.Columns.Add("Part/Assy", typeof(string));
					dtAssy.Columns.Add("Revision", typeof(string));
					dtAssy.Columns.Add("Quantity", typeof(string));
					dtAssy.Columns.Add("Raw Material", typeof(string));
					dtAssy.Columns.Add("Module  Responsibility", typeof(string));
					dtAssy.Columns.Add("Comments", typeof(string));
					dtAssy.Columns.Add("Manufacturing Comments", typeof(string));
					dtAssy.Columns.Add("Collaborators", typeof(string));
					dtAssy.Columns.Add("Mfg-Vendor", typeof(string));
					dtAssy.Columns.Add("Mfg-Qty_Engine", typeof(string));
					dtAssy.Columns.Add("Mfg-RV_Qty", typeof(string));
					dtAssy.Columns.Add("Mfg-Remarks", typeof(string));


					string parentID = String.Empty;
					if (groupItem.ReportGroupIdentifier.Contains("Direct"))
					{
						parentID = Model.Where(x => x.Draw_part_no == "PW00000").Select(x => x.Engine_Part_Dbkey).FirstOrDefault().ToString();
					}
					else
					{
						parentID = groupItem.ReportGroupIdentifier.Split('_')[groupItem.ReportGroupIdentifier.Split('_').Length - 1];
					}

					int parentPartID = int.Parse(parentID.Trim());
					MPLReportData mainParent = Model.Where(x => x.Engine_Part_Dbkey == parentPartID).FirstOrDefault();
					mainParent.MaxLevel = MaXlevel;
					mainParent.MainAssesmblyHeading = headingMainAssembly;
					mainParent.reducer = reducer;
					DataRow row = dtAssy.NewRow();
					row = GetPartsRows(mainParent, row);
					dtAssy.Rows.Add(row);
					List<MPLReportData> childLevel1 = new List<MPLReportData>();



					if (filterDirctParts)
					{
						childLevel1 = MPGlobals.GetDirectPartsMPL(Model.Where(x => x.ReportGroupIdentifier == groupItem.ReportGroupIdentifier).ToList(), parentPartID);
					}
					else
					{
						//  childLevel1 = MPGlobals.GetAllChildren(Model.Where(x => x.ReportGroupIdentifier == item1).ToList(),parentPartID);
						childLevel1 = MPGlobals.GetAllChildrenMPL(Model.ToList(), parentPartID);
					}


					foreach (var item in childLevel1.OrderBy(x => x.Reporting_Parent).ThenBy(x => x.ReportDisplayOrder).ThenBy(x => x.Draw_part_no))
					{
						// rows for each assembly level wise
						row = dtAssy.NewRow();
						item.MaxLevel = MaXlevel;
						item.MainAssesmblyHeading = headingMainAssembly;
						item.reducer = reducer;
						row = GetPartsRows(item, row);
						dtAssy.Rows.Add(row);
						if (item.IsParent == "Yes")
						{
							List<MPLReportData> childLevel2 = MPGlobals.GetAllChildrenMPL(Model.ToList(), item.Engine_Part_Dbkey);
							foreach (var child2 in childLevel2.OrderBy(x => x.Reporting_Parent).ThenBy(x => x.ReportDisplayOrder))
							{
								row = dtAssy.NewRow();
								child2.MaxLevel = MaXlevel;
								child2.MainAssesmblyHeading = headingMainAssembly;
								child2.reducer = reducer;
								row = GetPartsRows(child2, row);
								dtAssy.Rows.Add(row);
								List<MPLReportData> childLevel3 = MPGlobals.GetAllChildrenMPL(Model.ToList(), child2.Engine_Part_Dbkey);
								foreach (var child3 in childLevel3.OrderBy(x => x.Reporting_Parent).ThenBy(x => x.ReportDisplayOrder))
								{
									row = dtAssy.NewRow();
									child3.MaxLevel = MaXlevel;
									child3.MainAssesmblyHeading = headingMainAssembly;
									child3.reducer = reducer;
									row = GetPartsRows(child3, row);
									dtAssy.Rows.Add(row);
									if (child3.IsParent == "Yes")
									{
										List<MPLReportData> childLevel4 = MPGlobals.GetAllChildrenMPL(Model.ToList(), child3.Engine_Part_Dbkey);
										foreach (var child4 in childLevel4.OrderBy(x => x.Reporting_Parent).ThenBy(x => x.ReportDisplayOrder))
										{
											row = dtAssy.NewRow();
											child4.MaxLevel = MaXlevel;
											child4.MainAssesmblyHeading = headingMainAssembly;
											child4.reducer = reducer;
											row = GetPartsRows(child4, row);
											dtAssy.Rows.Add(row);
											if (child4.IsParent == "Yes")
											{
												List<MPLReportData> childLevel5 = MPGlobals.GetAllChildrenMPL(Model.ToList(), child4.Engine_Part_Dbkey);
												foreach (var child5 in childLevel5.OrderBy(x => x.Reporting_Parent).ThenBy(x => x.ReportDisplayOrder))
												{
													row = dtAssy.NewRow();
													child5.MaxLevel = MaXlevel;
													child5.MainAssesmblyHeading = headingMainAssembly;
													child5.reducer = reducer;
													row = GetPartsRows(child5, row);
													dtAssy.Rows.Add(row);
													if (child5.IsParent == "Yes")
													{
														List<MPLReportData> childLevel6 = MPGlobals.GetAllChildrenMPL(Model.ToList(), child5.Engine_Part_Dbkey);
														foreach (var child6 in childLevel6.OrderBy(x => x.Reporting_Parent).ThenBy(x => x.ReportDisplayOrder))
														{
															row = dtAssy.NewRow();
															child6.MaxLevel = MaXlevel;
															child6.MainAssesmblyHeading = headingMainAssembly;
															child6.reducer = reducer;
															row = GetPartsRows(child6, row);
															dtAssy.Rows.Add(row);
															if (child6.IsParent == "Yes")
															{
																List<MPLReportData> childLevel7 = MPGlobals.GetAllChildrenMPL(Model.ToList(), child6.Engine_Part_Dbkey);
																foreach (var child7 in childLevel7.OrderBy(x => x.Reporting_Parent).ThenBy(x => x.ReportDisplayOrder))
																{
																	row = dtAssy.NewRow();
																	child7.MaxLevel = MaXlevel;
																	child7.MainAssesmblyHeading = headingMainAssembly;
																	child7.reducer = reducer;
																	row = GetPartsRows(child7, row);
																	dtAssy.Rows.Add(row);
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}

					}


					DataTable filteredTbl = new DataTable();
					string[] colbfilter;
					if (colbs != "")
					{
						colbfilter = colbs.Split(',');

						if (colbfilter.Length > 0)
						{
							foreach (var item in colbfilter)
							{
								try
								{
									var filter = dtAssy.AsEnumerable()
									.Where(r => r.Field<string>("Collaborators").Contains(item.Trim()));

									if (filter.Any())
									{
										var currentFilterdata = filter.CopyToDataTable();
										filteredTbl.Merge(currentFilterdata);

									}

								}
								catch (Exception)
								{
									continue;
								}
							}
						}
					}
					else
					{
						filteredTbl = dtAssy;
					}

					if (filteredTbl.Rows.Count > 0)
					{
						WorkSheet.Cells["A1"].LoadFromDataTable(filteredTbl, true, TableStyles.None);
						WorkSheet.Cells["A1:Q1"].Style.Font.Bold = true;
						//WorkSheet.Cells["A1:I1"].Style.Font.Color.SetColor(System.Drawing.Color.White);
						//WorkSheet.Cells["A1:Q1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
						//WorkSheet.Cells["A1:I1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkBlue);
						WorkSheet.Cells["A1:Q1"].AutoFitColumns();
					}


				}


				byte[] data = Excelpackage.GetAsByteArray();
				return File(data, "application/octet-stream", "MPLReport.xlsx");
			}
		}


		private DataRow GetPartsRows(MPLReportData item, DataRow row)
		{

			for (int j = 0; j < item.MaxLevel; j++)
			{
				if (item.Level - item.reducer == (j + 1))
				{
					row[j] = item.Draw_part_no;
				}
			}
			row[item.MaxLevel] = item.Description;
			row[item.MaxLevel + 1] = item.Type_Part_Name;
			row[item.MaxLevel + 2] = item.Revision;
			row[item.MaxLevel + 3] = item.Qty_per_Engine;
			row[item.MaxLevel + 4] = item.Material_name;
			row[item.MaxLevel + 5] = item.PartModuleRes;
			row[item.MaxLevel + 6] = item.Part_Remarks;
			row[item.MaxLevel + 7] = item.ManufacturingComments;
			row[item.MaxLevel + 8] = item.Collaborators;
			row[item.MaxLevel + 9] = item.MfgStatus_Vendor;
			row[item.MaxLevel + 10] = item.MfgStatus_Qty_Engine;
			row[item.MaxLevel + 11] = item.MfgStatus_RVQty;
			row[item.MaxLevel + 12] = item.MfgStatus_Remarks;

			return row;
		}

		[ClaimRequirement(UserPermissions.MPL_Report_Write)]
		public ActionResult UpdateMPLAssemblyDisplayOrder()
		{
			try
			{
				var MPL = Request.Form["MPLDisplayOrders"];
				List<MPLReportData> mPLReportDatas = JsonConvert.DeserializeObject<List<MPLReportData>>(MPL);
				StringBuilder sb = new StringBuilder();
				foreach (var item in mPLReportDatas)
				{
					if (item.Part_Remarks == "AssyDisplayOrder")
					{
						sb.Append($"Update [dbo].[Engine_Parts_Master] set AssemblyDisplayOrder = {item.AssemblyDisplayOrder} where Engine_Part_Dbkey = {item.Engine_Part_Dbkey};");
					}
					else
					{
						sb.Append($"Update [dbo].[Engine_Parts_Master] set ReportDisplayOrder = {item.AssemblyDisplayOrder} where Engine_Part_Dbkey = {item.Engine_Part_Dbkey};");
						sb.Append($"Update [dbo].[Engine_Parts_Usage] set ReportDisplayOrder = {item.AssemblyDisplayOrder} where [Part_relation_dbkey] = {item.Part_relation_dbkey};");
					}

				}
				MPGlobals.ExceSQLNonQuery("DISABLE TRIGGER [dbo].[A_IU_MPL_Audit_Log] ON [dbo].[Engine_Parts_Master];");
				MPGlobals.ExceSQLNonQuery("DISABLE TRIGGER [dbo].[After_update_EngineParts] ON [dbo].[Engine_Parts_Usage];");
				MPGlobals.ExceSQLNonQuery(sb.ToString());
				MPGlobals.ExceSQLNonQuery("ENABLE TRIGGER [dbo].[A_IU_MPL_Audit_Log] ON [dbo].[Engine_Parts_Master];");
				MPGlobals.ExceSQLNonQuery("ENABLE TRIGGER [dbo].[After_update_EngineParts] ON [dbo].[Engine_Parts_Usage];");

				return Json(new { success = true, Msg = "Updated Successfully" });
			}
			catch (Exception ex)
			{
				MPGlobals.ExceSQLNonQuery("ENABLE TRIGGER [dbo].[A_IU_MPL_Audit_Log] ON [dbo].[Engine_Parts_Master];");
				MPGlobals.ExceSQLNonQuery("ENABLE TRIGGER [dbo].[After_update_EngineParts] ON [dbo].[Engine_Parts_Usage];");
				return Json(new { success = true, Msg = "Updated Successfully" });
			}

		}

		[ClaimRequirement(UserPermissions.MPL_Report_Write)]
		public ActionResult UploadManufacturingStatus(AttachmentVM attachmentVM)
		{
			try
			{
				if (attachmentVM.uploadeddocument != null)
				{
					string uploadpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ExcelUploads");
					if (!Directory.Exists(uploadpath))
					{
						Directory.CreateDirectory(uploadpath);
					}
					string FileName = attachmentVM.uploadeddocument.FileName;
					uploadpath = uploadpath + FileName;

					using (var stream = new FileStream(uploadpath, FileMode.Create))
					{
						attachmentVM.uploadeddocument.CopyToAsync(stream);
					}

					DataTable dataTable = MPGlobals.ExceltoDatatable(uploadpath);

					int rowscount = dataTable.Rows.Count;
					if (rowscount > 0)
					{
						using (_dbContext)
						{
							for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
							{
								dataTable.Columns[i].ColumnName = dataTable.Columns[i].ColumnName.Replace("\n", "").Trim();
							}
							dataTable.AcceptChanges();
							MPGlobals.ExceSQLNonQuery($"UPDATE  [dbo].[AuditLogDisplayManager] SET [DisplayOrder] = 0 WHERE [SourceTable] = 'ExternalMfgStatus' ");
							AuditLogDisplayManager auditLogDisplayManager = new AuditLogDisplayManager();
							foreach (DataColumn column in dataTable.Columns)
							{
								auditLogDisplayManager = _dbContext.AuditLogDisplayManagers.Where(x => x.SourceTable == "ExternalMfgStatus" && x.ColumnName == column.ColumnName.Trim()).FirstOrDefault();
								if (auditLogDisplayManager == null)
								{
									if (!string.IsNullOrEmpty(column.ColumnName.Trim()))
									{
										auditLogDisplayManager = new AuditLogDisplayManager();
										auditLogDisplayManager.SourceTable = "ExternalMfgStatus";
										auditLogDisplayManager.ColumnName = column.ColumnName.Trim();
										auditLogDisplayManager.Display_ColumnName = column.ColumnName.Trim();
										auditLogDisplayManager.DisplayData = false;
										auditLogDisplayManager.Force_Display_Data = false;
										auditLogDisplayManager.DataType = "varchar(max)";
										auditLogDisplayManager.DisplayOrder = 1;
										_dbContext.AuditLogDisplayManagers.Add(auditLogDisplayManager);
									}
								}
								else
								{
									auditLogDisplayManager.DisplayOrder = 1;
									_dbContext.Entry(auditLogDisplayManager).State = EntityState.Modified;
								}
							}
							_dbContext.SaveChanges();
							string Jsonstring = JsonConvert.SerializeObject(dataTable);
							ExternalMfgStatus externalMfgStatu = new ExternalMfgStatus();
							externalMfgStatu.Json = Jsonstring;
							externalMfgStatu.UpdatedOn = DateTime.Now;
							externalMfgStatu.UploadedBy = 1;
							_dbContext.ExternalMfgStatuses.Add(externalMfgStatu);
							_dbContext.SaveChanges();
						}

					}
					else
					{
						return Json(new { success = false, Msg = "Uploaded Successfully" });
					}
				}
				return Json(new { success = true, Msg = "Uploaded Successfully" });
			}
			catch (Exception ex)
			{
				ErrorHandler.LogException(ex);
				return Json(new { success = false, Msg = "Server Error" });
			}
		}


		[Authorize]
		[ClaimRequirement(UserPermissions.MPL_DocumentStatus_Read)]
		public ActionResult MPLDocumentStatus(int id = 0, string sourcetable = "")
		{
			return View();
		}

		[Authorize]
		public string GetMPLDocumentStatus()
		{
			DataTable dataTable = MPGlobals.GetDataForDatalist("EXEC dbo.GetMPLDocumentStatus");
			// return Json(MPGlobals.GetTableAsList(dataTable));
			var jsonData = JsonConvert.SerializeObject(dataTable);
			return jsonData;
		}




		[HttpGet]
		[ClaimRequirement(UserPermissions.MPL_Revision_Read)]
		public IActionResult Revision(int BaselineEngineKey = 0)
		{
			ViewBag.BaselineEngineKey = BaselineEngineKey;
			return View();
		}

		[HttpGet]
		[ClaimRequirement(UserPermissions.MPL_Revision_Write)]
		public IActionResult MasterEnginePart(string PartDbkey_PartRelationKey, string formAction, string displayType)
		{
			ViewBag.DisplayType = displayType;
			using (_dbContext)
			{
				MasterPartViewModel masterPartViewModel = new MasterPartViewModel();
				masterPartViewModel.LoadSelectLists();
				int PartDbkey = int.Parse(PartDbkey_PartRelationKey.Split("_")[0]);
				int PartRelationKey = int.Parse(PartDbkey_PartRelationKey.Split("_")[1]);
				masterPartViewModel.partstypes = Masters.GetMasterpartTypes(PartDbkey);
				Engine_Parts_Master engine_Parts_Master = _dbContext.Engine_Parts_Masters.Where(x => x.Engine_Part_Dbkey == PartDbkey).FirstOrDefault();
				masterPartViewModel.Approver_ID = 0;
				masterPartViewModel.Part_relation_dbkey = PartRelationKey;
				masterPartViewModel.is_vendor_material = false;

				if (formAction == "Create" || formAction == "Create_With_Existing_Part")
				{
					masterPartViewModel.Engine_Dbkey = PartDbkey;
					masterPartViewModel.Parent_id = PartDbkey;
					masterPartViewModel.Engine_Title = GetEnginepartsChildName(PartDbkey);
					if (formAction == "Create_With_Existing_Part")
					{
						return PartialView("CreateMasterParts_UseExistParts", masterPartViewModel);
					}
				}
				else if (formAction == "Edit")
				{
					var ModelJson = JsonConvert.SerializeObject(engine_Parts_Master);
					masterPartViewModel = JsonConvert.DeserializeObject<MasterPartViewModel>(ModelJson);
					masterPartViewModel.Part_relation_dbkey = PartRelationKey;
					masterPartViewModel.Parent_id = engine_Parts_Master.Parent_id;
					masterPartViewModel.Engine_Title = GetEnginepartsChildName(engine_Parts_Master.Engine_Part_Dbkey);
					masterPartViewModel.partstypes = Masters.GetMasterpartTypes(engine_Parts_Master.Engine_Part_Dbkey);
					masterPartViewModel.Count_of_PendingApproving = int.Parse(MPGlobals.GetOnedata($"SELECT  count([Log_Db_Key])  FROM [dbo].[Audit_logs] where [Approval_Status] = 8 and table_name = 'Engine_Parts_Master' and [Primary_key]  = {engine_Parts_Master.Engine_Part_Dbkey}"));
					if (masterPartViewModel.Count_of_PendingApproving != 0)
					{
						masterPartViewModel.approvalRequestDetail = GetApprovalRequesterDetail(engine_Parts_Master.Engine_Part_Dbkey);

					}
					else
					{
						masterPartViewModel.approvalRequestDetail = new ApprovalRequestDetail();

                    }
					Engine_Parts_Usage engine_Parts_Usage = _dbContext.Engine_Parts_Usages.Where(x => x.Part_relation_dbkey == PartRelationKey).FirstOrDefault();
					if (engine_Parts_Usage != null)
					{
						masterPartViewModel.Description = engine_Parts_Usage.Description;
						masterPartViewModel.Revision = engine_Parts_Usage.Revision;
						masterPartViewModel.Quantity = engine_Parts_Usage.Qty_per_Engine;
						masterPartViewModel.Module_Responsibility = engine_Parts_Usage.Module_Responsibility;
					}
					masterPartViewModel.is_vendor_material = masterPartViewModel.is_vendor_material;
					masterPartViewModel.LoadSelectLists();
				}
				return PartialView(masterPartViewModel);
			}
		}

		private ApprovalRequestDetail GetApprovalRequesterDetail(int engine_Part_Dbkey)
		{
			ApprovalRequestDetail approvalRequestDetail = new ApprovalRequestDetail();
            DataTable dt = MPGlobals.GetDataForDatalist(@"SELECT top 1 au.UserName, [Updated_On]
														FROM  [dbo].[Audit_logs] al
														join AspNetUsers au on al.Updated_By = au.OldUserDbkey
														where al.[Approval_Status] = 8  and al.table_name = 'Engine_Parts_Master' and al.Primary_key ="+ engine_Part_Dbkey + " order by [Log_Db_Key] desc");
            approvalRequestDetail.Requested_By = dt.Rows[0][0].ToString();
            approvalRequestDetail.Requested_On = DateTime.Parse(dt.Rows[0][1].ToString());

            return approvalRequestDetail;

        }


        [HttpGet]
		[ClaimRequirement(UserPermissions.MPL_Read)]
		public IActionResult ViewMasterEnginePart(string PartDbkey_PartRelationKey, string formAction, string displayType)
		{
			ViewBag.DisplayType = displayType;
			using (_dbContext)
			{
				MasterPartViewModel masterPartViewModel = new MasterPartViewModel();
				masterPartViewModel.LoadSelectLists();
				int PartDbkey = int.Parse(PartDbkey_PartRelationKey.Split("_")[0]);
				int PartRelationKey = int.Parse(PartDbkey_PartRelationKey.Split("_")[1]);
				masterPartViewModel.partstypes = Masters.GetMasterpartTypes(PartDbkey);
				Engine_Parts_Master engine_Parts_Master = _dbContext.Engine_Parts_Masters.Where(x => x.Engine_Part_Dbkey == PartDbkey).FirstOrDefault();
				masterPartViewModel.Approver_ID = 0;
				masterPartViewModel.Part_relation_dbkey = PartRelationKey;
				masterPartViewModel.is_vendor_material = false;

				if (formAction == "Create" || formAction == "Create_With_Existing_Part")
				{
					masterPartViewModel.Engine_Dbkey = PartDbkey;
					masterPartViewModel.Parent_id = PartDbkey;
					masterPartViewModel.Engine_Title = GetEnginepartsChildName(PartDbkey);
					if (formAction == "Create_With_Existing_Part")
					{
						return PartialView("CreateMasterParts_UseExistParts", masterPartViewModel);
					}
				}
				else if (formAction == "Edit")
				{
					var ModelJson = JsonConvert.SerializeObject(engine_Parts_Master);
					masterPartViewModel = JsonConvert.DeserializeObject<MasterPartViewModel>(ModelJson);
					masterPartViewModel.Part_relation_dbkey = PartRelationKey;
					masterPartViewModel.Parent_id = engine_Parts_Master.Parent_id;
					masterPartViewModel.Engine_Title = GetEnginepartsChildName(engine_Parts_Master.Engine_Part_Dbkey);
					masterPartViewModel.partstypes = Masters.GetMasterpartTypes(engine_Parts_Master.Engine_Part_Dbkey);
					masterPartViewModel.Count_of_PendingApproving = int.Parse(MPGlobals.GetOnedata($"SELECT  count([Log_Db_Key])  FROM [dbo].[Audit_logs] where [Approval_Status] = 8 and [Primary_key]  = {engine_Parts_Master.Engine_Part_Dbkey}"));
					Engine_Parts_Usage engine_Parts_Usage = _dbContext.Engine_Parts_Usages.Where(x => x.Part_relation_dbkey == PartRelationKey).FirstOrDefault();
					if (engine_Parts_Usage != null)
					{
						masterPartViewModel.Description = engine_Parts_Usage.Description;
						masterPartViewModel.Revision = engine_Parts_Usage.Revision;
						masterPartViewModel.Quantity = engine_Parts_Usage.Qty_per_Engine;
						masterPartViewModel.Module_Responsibility = engine_Parts_Usage.Module_Responsibility;
					}
					masterPartViewModel.is_vendor_material = masterPartViewModel.is_vendor_material;
					masterPartViewModel.LoadSelectLists();
				}
				return PartialView("MasterEnginePart", masterPartViewModel);
			}
		}


		[HttpPost]
		[ClaimRequirement(UserPermissions.MPL_Revision_Write)]
		public ActionResult MasterEnginePart(MasterPartViewModel masterPartViewModel)
		{
			bool triggermail = false;
            DTOResponse dTOResponse = new DTOResponse();
			using (_dbContext)
			{
				try
				{
					Engine_Parts_Master engine_Parts_Master = new Engine_Parts_Master();
					var ViewModelJson = JsonConvert.SerializeObject(masterPartViewModel);
					engine_Parts_Master = JsonConvert.DeserializeObject<Engine_Parts_Master>(ViewModelJson);
					engine_Parts_Master.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
					engine_Parts_Master.Updated_On = DateTime.Now;

					if (engine_Parts_Master.Engine_Part_Dbkey == 0)
					{
						_dbContext.Add(engine_Parts_Master);
						_dbContext.SaveChanges();
						if (engine_Parts_Master.Draw_part_no != "NA")
						{
							MPGlobals.ExceSQLNonQuery($"Delete FROM  [dbo].[Engine_Parts_Master] where [Engine_Part_Dbkey] ={engine_Parts_Master.Engine_Part_Dbkey} ");
						}
						dTOResponse.ResponseMessage = "Saved Successfully";
					}
					else
					{
						if (DeepCompare(masterPartViewModel))
						{
							triggermail = true;
                            engine_Parts_Master.Solid_Model = masterPartViewModel.Part_relation_dbkey?.ToString(); // Solid_Model column(string) used to store Part_relation_dbkey as reference to update only that part
                            _dbContext.Entry(engine_Parts_Master).State = EntityState.Modified;
							_dbContext.SaveChanges();
						}
						dTOResponse.ResponseMessage = "Requested for approval";
					}

					if (triggermail)
					{
                        Task.Run(() => MPCRS.Utilities.Notification.TriggerMPLApprovalMail(engine_Parts_Master));
                    }
				}
				catch (Exception ex)
				{
					ErrorHandler.LogException(ex);
					dTOResponse.ResponseMessage = ex.Message;
				}
			}
			return Json(new { success = true, Msg = dTOResponse.ResponseMessage });
		}
 

        private static bool DeepCompare(MasterPartViewModel engine_PartsVM)
		{

			using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
			{
				Engine_Parts_Master engine_Parts = db.Engine_Parts_Masters.Where(x => x.Engine_Part_Dbkey == engine_PartsVM.Engine_Part_Dbkey).FirstOrDefault();
				Engine_Parts_Usage engine_Parts_Usage = db.Engine_Parts_Usages.Where(x => x.Part_relation_dbkey == engine_PartsVM.Part_relation_dbkey).FirstOrDefault();
				if (engine_Parts.Type_Dbkey != engine_PartsVM.Type_Dbkey)
				{
					return true;
				}
				else if (engine_Parts.Draw_part_no != engine_PartsVM.Draw_part_no)
				{
					return true;
				}
				else if (engine_Parts_Usage.Revision != engine_PartsVM.Revision)
				{
					return true;
				}
				else if (engine_Parts.Quantity != engine_PartsVM.Quantity)
				{
					return true;
				}
				else if (engine_Parts.Manufacturing_Duration != engine_PartsVM.Manufacturing_Duration)
				{
					return true;
				}
				else if (engine_Parts.Module_Responsibility != engine_PartsVM.Module_Responsibility)
				{
					return true;
				}
				else if (engine_Parts.Raw_Material != engine_PartsVM.Raw_Material)
				{
					return true;
				}
				else if (engine_Parts_Usage.Description != engine_PartsVM.Description)
				{
					return true;
				}
				else if (engine_Parts_Usage.Comments != engine_PartsVM.Comments)
				{
					return true;
				}
				else if (engine_Parts.FCBP != engine_PartsVM.FCBP)
				{
					return true;
				}
				else if (engine_Parts.is_vendor_material != engine_PartsVM.is_vendor_material)
				{
					return true;
				}
				else if (engine_Parts.Bar_pipe_Dia_OD != engine_PartsVM.Bar_pipe_Dia_OD)
				{
					return true;
				}
				else if (engine_Parts.Bar_pipe_Length != engine_PartsVM.Bar_pipe_Length)
				{
					return true;
				}
				else if (engine_Parts.Bar_pipe_Thickness != engine_PartsVM.Bar_pipe_Thickness)
				{
					return true;
				}
				else if (engine_Parts.Plate_length != engine_PartsVM.Plate_length)
				{
					return true;
				}
				else if (engine_Parts.Plate_Width != engine_PartsVM.Plate_Width)
				{
					return true;
				}
				else if (engine_Parts.Plate_Thickness != engine_PartsVM.Plate_Thickness)
				{
					return true;
				}
				else if (engine_Parts.weight_in_kg != engine_PartsVM.weight_in_kg)
				{
					return true;
				}
				else if (engine_Parts.Density != engine_PartsVM.Density)
				{
					return true;
				}
				return false;

			}

		}


		[HttpPost]
		[ClaimRequirement(UserPermissions.MPL_Revision_Write)]
		public ActionResult CreateEnginepartsWithExisting(MasterPartViewModel engine_PartsVM)
		{
			using (_dbContext)
			{
				Engine_Parts_Usage db_engine_Parts_Usage = _dbContext.Engine_Parts_Usages.Where(x => x.Part_relation_dbkey == engine_PartsVM.Part_relation_dbkey).FirstOrDefault();
				Engine_Parts_Usage engine_Parts_Usage = new Engine_Parts_Usage();
				engine_Parts_Usage.BL_Engine_Dbkey = db_engine_Parts_Usage.BL_Engine_Dbkey;
				engine_Parts_Usage.Engine_Dbkey = 0;
				engine_Parts_Usage.Engine_Part_Dbkey = engine_PartsVM.Engine_Part_Dbkey;
				engine_Parts_Usage.Qty_per_Engine = db_engine_Parts_Usage.Qty_per_Engine;
				engine_Parts_Usage.Parent_id = engine_PartsVM.Parent_id;
				engine_Parts_Usage.Revision = db_engine_Parts_Usage.Revision;
				engine_Parts_Usage.is_active = 1;
				engine_Parts_Usage.Description = db_engine_Parts_Usage.Description;
				engine_Parts_Usage.Updated_by = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
				engine_Parts_Usage.Updated_on = DateTime.Now;
				_dbContext.Engine_Parts_Usages.Add(engine_Parts_Usage);
				_dbContext.SaveChanges();
				MPGlobals.ExceSQLNonQuery($"Delete from [dbo].[Engine_Parts_Usage] where [Part_relation_dbkey] ={engine_Parts_Usage.Part_relation_dbkey} ");
				//Engine_Parts_Master engine_Parts_Master = _dbContext.Engine_Parts_Masters.Where(x => x.Engine_Part_Dbkey == engine_PartsVM.Engine_Part_Dbkey).FirstOrDefault();
				//engine_Parts_Master.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
				//Task.Run(() => MPCRS.Utilities.Notification.TriggerMPLApprovalMail(engine_Parts_Master));
                return Json(new { success = true, Msg = "Saved Successfully" });
			}
		}

		public static string GetEnginepartsChildName(int PartDbkey)
		{
			DataTable dataTable = MPGlobals.GetDataForDatalist("dbo.GetEngineParts_parent @partdbkey = " + PartDbkey + "");
			string allparentnames = "";

			for (int i = dataTable.Rows.Count; i > 0; i--)
			{
				allparentnames = allparentnames + " " + dataTable.Rows[i - 1]["NAME"].ToString() + " <i class='fa fa-arrow-right'> </i> ";
			}
			return allparentnames + " <i class='fa fa-arrow-right'> </i> ";
		}



		[HttpGet]
		[ClaimRequirement(UserPermissions.MPL_Report_Read)]
		public ActionResult MPL_Comaprision()
		{
			return View();
		}

		[HttpGet]
		[ClaimRequirement(UserPermissions.MPL_Report_Read)]
		public string GetMPL_Comaparision(int BL1, int BL2, bool ischangeOnly, int ExecResp)
		{
			int filterrequest = ischangeOnly == true ? 1 : 0;
			DataTable dataTable = new DataTable();
			dataTable = MPGlobals.GetDataForDatalist($"dbo.MPL_Comparision_New @BL_Engine_Dbkey1 ={BL1},@BL_Engine_Dbkey2 = {BL2},@IsChangeOnly ={filterrequest},@ExecResponsibility ={ExecResp}");
			//	return Json(MPGlobals.GetTableAsList(dataTable));
			return JsonConvert.SerializeObject(dataTable, Formatting.Indented);
		}


		[ClaimRequirement(UserPermissions.BaseLineEngine_Write)]
		public ActionResult CloneBLEngine(int id = 0)
		{
			try
			{
				MPGlobals.ExceSQLNonQuery("Exec dbo.Clone_BL_Engine @BL_Engine_Dbkey =" + id);
				return Json(new { success = true, Msg = "Success" });
			}
			catch (Exception ex)
			{

				return Json(new { success = false, Msg = "Clone Failed !" });
			}

		}

		public ActionResult DownloadMplJsonFiles(int BL_Engine_Dbkey = 0)
		{
			List<MplJsTreeViewModel> myArrays = GetJstreeList(BL_Engine_Dbkey);
			string MPLFileName = "MPLTree.js";
			var jsondata = "var treeData =";
			jsondata = jsondata + JsonConvert.SerializeObject(myArrays);
			string directoryname = @"/Attachments/MPLJsonFiles/";
			string folderpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/" + directoryname);
			if (!Directory.Exists(folderpath))
			{
				Directory.CreateDirectory(folderpath);
			}
			System.IO.File.WriteAllText(folderpath + MPLFileName, jsondata);
			byte[] fileBytes = System.IO.File.ReadAllBytes(folderpath + MPLFileName);
			return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, MPLFileName);
		}


		public ActionResult DownloadMplDocJsonFiles(int BL_Engine_Dbkey = 0)
		{
			Guid guid = Guid.NewGuid();
			string MPLFileName = "MplDocumentData.js";
			string Cmdstr = @"dbo.DownloadMPLDocsJson_SSP";
			List<AttachmentVM> myArrays = new List<AttachmentVM>();
			DataTable dataTable = MPGlobals.GetDataForDatalist(Cmdstr);
			var jsondata = "var attachmentData =";
			jsondata = jsondata + JsonConvert.SerializeObject(dataTable, Formatting.Indented);
			string directoryname = @"/Attachments/MPLJsonFiles/";
			string folderpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/" + directoryname);
			if (!Directory.Exists(folderpath))
			{
				Directory.CreateDirectory(folderpath);
			}
			System.IO.File.WriteAllText(folderpath + MPLFileName, jsondata);
			byte[] fileBytes = System.IO.File.ReadAllBytes(folderpath + MPLFileName);
			return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, MPLFileName);
		}


		[Authorize]
		public ActionResult MaterialIssues(int Engine_Part_Dbkey)
		{
			ViewBag.Engine_Part_Dbkey = Engine_Part_Dbkey;
			return PartialView();
		}

		[Authorize]
		[HttpGet]
		[ClaimRequirement(UserPermissions.BaseLineEngine_Write)]
		public ActionResult ManageBaselineEngineParts(int BaselineEngineDbkey)
		{
			using (_dbContext)
			{
				DataTable dataTable = MPGlobals.GetDataForDatalist($"SELECT  [Part_relation_dbkey] FROM  [dbo].[Engine_Parts_Usage] where [BL_Engine_Dbkey] = {BaselineEngineDbkey} and [Engine_Dbkey] = 0 ");
				if (dataTable.Rows.Count == 0)
				{
					MPGlobals.ExceSQLNonQuery($"dbo.Clone_Engine_parts @BL_Engine_Dbkey = {BaselineEngineDbkey}");
				}

				ViewBag.BaselineEngineKey = BaselineEngineDbkey;
				ViewBag.EngineDetail = _dbContext.Base_Line_Engines.Where(x => x.BL_Engine_Dbkey == BaselineEngineDbkey).FirstOrDefault().Engine_Title;
				return View();
			}

		}

		[Authorize]
		[HttpPost]
		[ClaimRequirement(UserPermissions.BaseLineEngine_Write)]
		public ActionResult ManageBaselineEngineParts()
		{
			try
			{
				var MPL = Request.Form["MPL"];
				int BL_Engine_Dbkey = int.Parse(Request.Form["BL_Engine_Dbkey"]);
				int Engine_Dbkey = int.Parse(Request.Form["Engine_Dbkey"]);

				int UserDbkey = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);

				string[] ArrayOfPartsChanges = JsonConvert.DeserializeObject<string[]>(MPL);

				if (ArrayOfPartsChanges.Length != 0)
				{
					for (int i = 0; i < ArrayOfPartsChanges.Length; i++)
					{
						int EnginePartDbkey = int.Parse(ArrayOfPartsChanges[i].Split(';')[0]);
						int PartRelationKey = int.Parse(ArrayOfPartsChanges[i].Split(';')[1]);
						bool State = bool.Parse(ArrayOfPartsChanges[i].Split(';')[2]);
						int isactiveState = State == true ? 1 : 0;

							Engine_Parts_Usage engine_Parts_Usage = new Engine_Parts_Usage();
							List<Engine_Parts_Usage> engine_Parts_UsageList = new List<Engine_Parts_Usage>();

							if (Engine_Dbkey == 0)
							{
								engine_Parts_UsageList = _dbContext.Engine_Parts_Usages.Where(x => x.Part_relation_dbkey == PartRelationKey).ToList();
							}
							else
							{
								engine_Parts_UsageList = _dbContext.Engine_Parts_Usages.Where(x => x.Part_relation_dbkey == PartRelationKey).ToList();
							}

							if (engine_Parts_UsageList.Count() != 0)
							{
								foreach (var item in engine_Parts_UsageList)
								{
									if (item.is_active != isactiveState)
									{
										item.is_active = isactiveState;
										item.Updated_by = UserDbkey;
										item.Updated_on = DateTime.Now;

										_dbContext.Entry(item).State = EntityState.Modified;
										_dbContext.SaveChanges();
										 MPGlobals.ExceSQLNonQuery($"DISABLE TRIGGER [dbo].[After_update_EngineParts] ON [dbo].[Engine_Parts_Usage];");
										item.is_active = isactiveState == 0 ? 1 : 0;
										item.Updated_by = UserDbkey;
										item.Updated_on = DateTime.Now;
										_dbContext.Entry(item).State = EntityState.Modified;
										_dbContext.SaveChanges();
										MPGlobals.ExceSQLNonQuery($"Enable TRIGGER [dbo].[After_update_EngineParts] ON [dbo].[Engine_Parts_Usage];");

									}
								}
								// Need to enable Approval Management
								engine_Parts_Usage = engine_Parts_UsageList[0];
							}
							else
							{
								if (BL_Engine_Dbkey == 0)
								{
									BL_Engine_Dbkey = int.Parse(MPGlobals.GetOnedata($"SELECT [BL_Engine_Dbkey] FROM [dbo].[Engine_Parts_Usage] where [Engine_Dbkey] ={Engine_Dbkey} "));
									engine_Parts_Usage = _dbContext.Engine_Parts_Usages.Where(x => x.BL_Engine_Dbkey == BL_Engine_Dbkey && x.Engine_Part_Dbkey == EnginePartDbkey && x.Engine_Dbkey == 0).FirstOrDefault();
									Engine_Parts_Master dbmaster = _dbContext.Engine_Parts_Masters.Where(x => x.Engine_Part_Dbkey == EnginePartDbkey).FirstOrDefault();
									Engine_Parts_Usage engine_Parts = new Engine_Parts_Usage();
									engine_Parts.Part_relation_dbkey = 0;
									engine_Parts.Engine_Dbkey = Engine_Dbkey;
									engine_Parts.BL_Engine_Dbkey = BL_Engine_Dbkey;
									engine_Parts.Qty_per_Engine = engine_Parts_Usage.Qty_per_Engine;
									engine_Parts.Revision = engine_Parts_Usage.Revision;
									engine_Parts.is_active = 1;
									engine_Parts.Engine_Part_Dbkey = engine_Parts_Usage.Engine_Part_Dbkey;
									engine_Parts.Parent_id = engine_Parts_Usage.Parent_id;
									engine_Parts.Updated_by = UserDbkey;
									engine_Parts.Updated_on = DateTime.Now;
									engine_Parts.Comments = dbmaster.Comments;
									engine_Parts.Description = dbmaster.Description;
									_dbContext.Engine_Parts_Usages.Add(engine_Parts);
									_dbContext.SaveChanges();
									MPGlobals.ExceSQLNonQuery($"Delete FROM [dbo].[Engine_Parts_Usage]  where [Part_relation_dbkey] = {engine_Parts.Part_relation_dbkey}");
								}
								else
								{
									Engine_Parts_Master dbmaster = _dbContext.Engine_Parts_Masters.Where(x => x.Engine_Part_Dbkey == EnginePartDbkey).FirstOrDefault();
									if (dbmaster != null)
									{
										Engine_Parts_Usage engine_Parts = new Engine_Parts_Usage();
										engine_Parts.Part_relation_dbkey = 0;
										engine_Parts.Engine_Dbkey = Engine_Dbkey;
										engine_Parts.BL_Engine_Dbkey = BL_Engine_Dbkey;
										engine_Parts.Qty_per_Engine = Convert.ToInt32(dbmaster.Quantity);
										engine_Parts.Revision = dbmaster.Revision;
										engine_Parts.is_active = isactiveState;
										engine_Parts.Engine_Part_Dbkey = dbmaster.Engine_Part_Dbkey;
										engine_Parts.Parent_id = dbmaster.Parent_id;
										engine_Parts.Updated_by = UserDbkey;
										engine_Parts.Updated_on = DateTime.Now;
										engine_Parts.Comments = dbmaster.Comments;
										engine_Parts.Description = dbmaster.Description;
										_dbContext.Engine_Parts_Usages.Add(engine_Parts);
										_dbContext.SaveChanges();
										MPGlobals.ExceSQLNonQuery($"Delete FROM [dbo].[Engine_Parts_Usage]  where [Part_relation_dbkey] = {engine_Parts.Part_relation_dbkey}");
									}

								}
						

							//Engine_PartsVM engine_PartsVM = DESI_STFE.Areas.Engines.DTO.Engines.GetMPLData(engine_Parts_Usage.Engine_Part_Dbkey);
							//string engineName = DESI_STFE.Areas.Engines.DTO.Engines.GetEngineData(engine_Parts_Usage.Engine_Dbkey);
							//Logger.WriteToFile("Begin Send Approver Mail - Engine Level");
							//eMailVM eMailVM = new eMailVM();
							//eMailVM.PartNumber = engine_PartsVM.Draw_part_no;
							//eMailVM.Reivsion = engine_Parts_Usage.Revision;
							////eMailVM.Approver = ApproverDbkey;
							//eMailVM.Requested = ud.UserDbkey;
							//eMailVM.sourceTable = "Engine_Parts_Usage";
							//Task.Run(() => DESI_STFE.DTO.Notifications.Sendmail(eMailVM));
						}


					}
				}

				return Json(new { success = true, Msg = "Updated Successfully" });
			}
			catch (Exception ex)
			{
				MPGlobals.ExceSQLNonQuery($"Enable TRIGGER [dbo].[After_update_EngineParts] ON [dbo].[Engine_Parts_Usage];");

				return Json(new { success = false, Msg = ex.ToString() });
			}


		}


		[HttpGet]
		[ClaimRequirement(UserPermissions.MPL_Write)]
		public ActionResult UpdatePartAdditionalInfo(int PartDbkey, int PartRelationkey)
		{
			MasterPartViewModel vm = new MasterPartViewModel();
			vm.LoadSelectLists();
			using (_dbContext)
			{
				Engine_Parts_Master dbData = _dbContext.Engine_Parts_Masters.Where(x => x.Engine_Part_Dbkey == PartDbkey).FirstOrDefault();
				Engine_Parts_Usage engine_Parts_Usage = _dbContext.Engine_Parts_Usages.Where(x => x.Part_relation_dbkey == PartRelationkey).FirstOrDefault();
				//var ModelJson = JsonConvert.SerializeObject(dbData);
				//vm = JsonConvert.DeserializeObject<MasterPartViewModel>(ModelJson);
				vm.Engine_Part_Dbkey = dbData.Engine_Part_Dbkey;
				vm.Part_relation_dbkey = PartRelationkey;
				vm.Draw_part_no = dbData.Draw_part_no;
				vm.Description = engine_Parts_Usage.Description;
				vm.ParentParts = Masters.GetMPLParts();
				vm.Parent_id = engine_Parts_Usage.Parent_id;
				vm.Reporting_Parent = engine_Parts_Usage.Reporting_Parent ?? dbData.Reporting_Parent ?? 0;
				vm.Part_Remarks = engine_Parts_Usage.Part_Remarks;
				vm.ManufacturingComments = engine_Parts_Usage.ManufacturingComments;
				vm.ReportDisplayOrder = engine_Parts_Usage.ReportDisplayOrder > 0 ? engine_Parts_Usage.ReportDisplayOrder : 100;
				vm.CollaboratorArr = GetEngineCollabrators(engine_Parts_Usage.CollaboratorsId);
				vm.Execution_Resp = engine_Parts_Usage.Execution_Resp;
				vm.Execution_Resp_additionalLevel = engine_Parts_Usage.Execution_Resp_additionalLevel;
				vm.Reporting_Type = dbData.Reporting_Type;
				vm.AssemblyReportingType = dbData.AssemblyReportingType;
			}
			return PartialView(vm);
		}


		[HttpPost]
		[ClaimRequirement(UserPermissions.MPL_Write)]
		public ActionResult SaveAdditionalPartInfo(MasterPartViewModel engine_PartsVM)
		{
			string cmdstr = string.Empty;
			try
			{
				string ColbsIdBeforeupdate = "";
				using (_dbContext)
				{
					Engine_Parts_Usage engine_Parts_Usage = _dbContext.Engine_Parts_Usages.Where(x => x.Part_relation_dbkey == engine_PartsVM.Part_relation_dbkey).FirstOrDefault();
					ColbsIdBeforeupdate = engine_Parts_Usage.CollaboratorsId;
				}


				cmdstr = "DISABLE TRIGGER dbo.A_IU_MPL_Audit_Log ON dbo.Engine_Parts_Master; ";
				cmdstr = cmdstr + "DISABLE TRIGGER dbo.After_update_EngineParts ON dbo.Engine_Parts_Usage; ";

				cmdstr = cmdstr + @" UPDATE [dbo].[Engine_Parts_Master] 
                                  SET [Execution_Resp] = " + engine_PartsVM.Execution_Resp + @", 
                                      [Reporting_Type] = '" + engine_PartsVM.Reporting_Type + @"',
                                      [AssemblyReportingType] = '" + engine_PartsVM.AssemblyReportingType + @"',
                                      [ReportDisplayOrder] = '" + engine_PartsVM.ReportDisplayOrder + @"',
                                      [Parent_id] = '" + engine_PartsVM.Parent_id + @"',
                                      [Reporting_Parent] = '" + engine_PartsVM.Reporting_Parent + @"',
                                      [Execution_Resp_additionalLevel] = '" + engine_PartsVM.Execution_Resp_additionalLevel + @"'
                                  WHERE [Engine_Part_Dbkey] =" + engine_PartsVM.Engine_Part_Dbkey + "; ";

				cmdstr = cmdstr + @" UPDATE [dbo].[Engine_Parts_Usage]
                                  SET [Parent_id] = " + engine_PartsVM.Parent_id + @"  ,
                                      [Reporting_Parent] = '" + engine_PartsVM.Reporting_Parent + @"',
                                      [Part_Remarks] = '" + engine_PartsVM.Part_Remarks + @"',
                                      [ManufacturingComments] = '" + engine_PartsVM.ManufacturingComments + @"',
                                      [ReportDisplayOrder] = '" + engine_PartsVM.ReportDisplayOrder + @"',
                                      [Execution_Resp] = '" + engine_PartsVM.Execution_Resp + @"',
                                      [Execution_Resp_additionalLevel] = '" + engine_PartsVM.Execution_Resp_additionalLevel + @"',
                                      [CollaboratorsId] = '" + engine_PartsVM.CollaboratorsId + @"',
                                      [Collaborators] = '" + engine_PartsVM.Collaborators + @"'
                                  WHERE [Engine_Part_Dbkey] =" + engine_PartsVM.Engine_Part_Dbkey + " and   Part_relation_dbkey =" + engine_PartsVM.Part_relation_dbkey + " ;";


				cmdstr = cmdstr + " ENABLE TRIGGER dbo.A_IU_MPL_Audit_Log ON dbo.Engine_Parts_Master; ";
				cmdstr = cmdstr + " ENABLE TRIGGER dbo.After_update_EngineParts ON dbo.Engine_Parts_Usage;";

				MPGlobals.ExceSQLNonQuery(cmdstr);

				//if (ColbsIdBeforeupdate != engine_PartsVM.CollaboratorsId)
				//{
				MPGlobals.ExceSQLNonQuery($"[dbo].[Update_Collaborators] @PartRelationID = {engine_PartsVM.Part_relation_dbkey}");
				//}

			}
			catch (Exception)
			{
				cmdstr = " ENABLE TRIGGER dbo.A_IU_MPL_Audit_Log ON dbo.Engine_Parts_Master; ";
				cmdstr = cmdstr + " ENABLE TRIGGER dbo.After_update_EngineParts ON dbo.Engine_Parts_Usage;";
				MPGlobals.ExceSQLNonQuery(cmdstr);
				return Json(new { success = true, Msg = "Something went wrong. Please try later" });
			}

			return Json(new { success = true, Msg = "Saved Successfully" });
		}


		private int[] GetEngineCollabrators(string collaboratorsId)
		{
			int[] collids = new int[] { };
			if (string.IsNullOrEmpty(collaboratorsId))
			{
				return collids;
			}
			else
			{
				string[] Colitems = collaboratorsId.Split(',');
				collids = Array.ConvertAll(Colitems, s => int.Parse(s));
				return collids;
			}
		}

		[HttpGet]
		[Authorize]
		public ActionResult ViewMfgStatus(int PartRelationKey)
		{
			using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
			{

				PartManufacturingStatusVM partManufacturingStatusVM = new PartManufacturingStatusVM();
				Engine_Parts_Usage engine_Parts_Usage = db.Engine_Parts_Usages.Where(x => x.Part_relation_dbkey == PartRelationKey).FirstOrDefault();
				Engine_Parts_Master engine_Parts_Master = db.Engine_Parts_Masters.Where(x => x.Engine_Part_Dbkey == engine_Parts_Usage.Engine_Part_Dbkey).FirstOrDefault();
				partManufacturingStatusVM.PartNumber = engine_Parts_Master.Draw_part_no;
				partManufacturingStatusVM.PartDescription = engine_Parts_Master.Description;
				partManufacturingStatusVM.PartRevision = engine_Parts_Master.Revision;
				partManufacturingStatusVM.Part_relation_dbkey = engine_Parts_Usage.Part_relation_dbkey;
				partManufacturingStatusVM.Engine_Part_Dbkey = engine_Parts_Usage.Engine_Part_Dbkey;
				partManufacturingStatusVM.QtyPerEngine = engine_Parts_Usage.Qty_per_Engine ?? 0;
				return PartialView(partManufacturingStatusVM);
			}

		}

		[HttpGet]
		public ActionResult ManufacturingProcessRequiredDocuments(int enginePartDbkey)
		{
			using (_dbContext)
			{
				ViewBag.enginePartDbkey = enginePartDbkey;
				List<Master_General> master_Generals = new List<Master_General>();
				master_Generals = _dbContext.Master_Generals.Where(x => x.Master_Type == "Procurement_Doument_Type").ToList();
				List<Manufacturing_Process_Documents_Required> requiredDoc = _dbContext.Manufacturing_Process_Documents_Requireds.Where(x => x.Part_DbKey == enginePartDbkey).ToList();

				if (requiredDoc == null)
				{
					requiredDoc = new List<Manufacturing_Process_Documents_Required>();

				}
				foreach (var item in master_Generals)
				{
					if (requiredDoc.Where(x => x.AttachmentTypeKey == item.Master_Dbkey).FirstOrDefault() == null)
					{
						requiredDoc.Add(new Manufacturing_Process_Documents_Required { AttachmentTypeKey = item.Master_Dbkey, Attachment_Type = item.Master_Name, Required = false });
					}
				}
				return PartialView(requiredDoc);
			}
		}

		[HttpPost]
		public ActionResult ManufacturingProcessRequiredDocuments([FromBody] List<Manufacturing_Process_Documents_Required> documents_Required)
		{
			foreach (var item in documents_Required)
			{
				if (item.Id != 0)
				{
					_dbContext.Entry(item).State = EntityState.Modified;
				}
				else
				{
					_dbContext.Add(item);
				}

				_dbContext.SaveChanges();
			}

			return Json(new { success = true });
		}

		[ClaimRequirement(UserPermissions.MPLDocuments_View)]
		public IActionResult MPLDocuments()
		{
			return View();
		}


		public string Get_MPLDocuments(int latest)
		{
			DataTable dataTable = MPGlobals.GetDataForDatalist("EXEC dbo.Get_MPLDocuments @latest="+latest);
			// return Json(MPGlobals.GetTableAsList(dataTable));
			var jsonData = JsonConvert.SerializeObject(dataTable);
			return jsonData;
		}

		//[HttpGet]
		//[ClaimRequirement(UserPermissions.MPLDocuments_View)]
		//public ActionResult ViewWatermarkedDocument(int docid, bool download = false)
		//{

		//	Attachment attachment = _dbContext.Attachments.AsNoTracking().Where(x => x.Attachment_Db_Key == docid).FirstOrDefault();
		//	DocumentsViewModel documentsViewModel = new();
		//	documentsViewModel.Document_Dbkey = attachment.Attachment_Db_Key;
		//	documentsViewModel.File_type = Path.GetExtension(attachment.Orginal_File_Name);

		//	if (documentsViewModel.File_type.ToLower().Contains(".pdf") && (documentsViewModel.File_type.ToLower().EndsWith(".zip") == false))
		//	{

		//		documentsViewModel.System_File_Name = AddWatermark(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + attachment.Attachment_location), attachment.Attachment_FileName, attachment.Orginal_File_Name);
		//	}
		//	else if ((documentsViewModel.File_type.ToLower().Contains(".tiff") || documentsViewModel.File_type.ToLower().Contains(".tif")) && (documentsViewModel.File_type.ToLower().EndsWith(".zip") == false))
		//	{
		//		documentsViewModel.TiffToPngConvertedLocation = AddWatermarkToTiff(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + attachment.Attachment_location), attachment.Attachment_FileName, attachment.Orginal_File_Name);
		//              if (download)
		//              {
		//                  List<string> pngFiles = new List<string>();
		//			string Extension = Path.GetExtension(attachment.Orginal_File_Name);
		//                  string downloadfolder = "/Attachments/Downloads/";
		//                  downloadfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + downloadfolder);

		//                  foreach (var item in documentsViewModel.TiffToPngConvertedLocation)
		//                  {
		//                      pngFiles.Add(Path.Combine(downloadfolder, item.Item1).Replace("\\", "/"));

		//                  }

		//                  if (Extension.ToLower().Contains(".tiff") == false)
		//                  {
		//                      var tiffPath = ConvertToMultiPageTiff(pngFiles, ".tif");
		//                      var fileName = Path.GetFileName(attachment.Orginal_File_Name);
		//                      var fileBytes = System.IO.File.ReadAllBytes(tiffPath);
		//                      var contentType = "application/octet-stream";

		//                      return File(fileBytes, contentType, fileName);
		//                  }
		//                  else
		//                  {
		//                      var tiffPath = ConvertToMultiPageTiff(pngFiles, "tiff");
		//                      var fileName = Path.GetFileName(attachment.Orginal_File_Name);
		//                      var fileBytes = System.IO.File.ReadAllBytes(tiffPath);
		//                      var contentType = "application/octet-stream";

		//                      return File(fileBytes, contentType, fileName);
		//                  }


		//              }
		//          }
		//	return PartialView(documentsViewModel);

		//}

		//      public string ConvertToMultiPageTiff(List<string> pngFilePaths, string filetype)
		//      {
		//          try
		//          {
		//              // Create a MagickImageCollection
		//              var collection = new MagickImageCollection();

		//              // Read each PNG file and add it to the collection
		//              foreach (var pngFilePath in pngFilePaths)
		//              {
		//                  var image = new MagickImage(pngFilePath);
		//                  collection.Add(image);
		//              }

		//              // Write the collection as a multi-page TIFF
		//              var tiffFilePath = Path.ChangeExtension(pngFilePaths[0], filetype);
		//              collection.Write(tiffFilePath);

		//              // Dispose of the collection to release resources
		//              collection.Dispose();

		//              return tiffFilePath;
		//          }
		//          catch (Exception ex)
		//          {
		//              // Handle exceptions
		//              throw new Exception($"An error occurred while converting to multi-page TIFF: {ex.Message}");
		//          }
		//      }

		//public string AddWatermark(string path, string fileName, string originalFileName)
		//{

		//	string extension = System.IO.Path.GetExtension(fileName);
		//	if (extension == ".pdf")
		//	{
		//		var pdfBytes = System.IO.File.ReadAllBytes(Path.Combine(path, fileName));
		//		var oldFile = new iTextSharp.text.pdf.PdfReader(pdfBytes);

		//		string username = User.FindFirst(ClaimTypes.GivenName).Value;
		//		string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

		//		Guid guid = Guid.NewGuid();
		//		string watermarkedFileName = guid.ToString() + fileName;
		//		string downloadfolder = "/Attachments/Downloads/";
		//		downloadfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + downloadfolder);
		//		string watermarkedFilePath = System.IO.Path.Combine(downloadfolder, watermarkedFileName);

		//		if (!Directory.Exists(downloadfolder))
		//		{
		//			Directory.CreateDirectory(downloadfolder);
		//		}


		//		//	Response.ContentType = "application/pdf";
		//		originalFileName = originalFileName == "" ? fileName : originalFileName;
		//		//Response.Headers.Add("Content-Disposition", "attachment; filename=" + originalFileName);
		//		using (FileStream outputStream = new FileStream(watermarkedFilePath, FileMode.Create))
		//		{

		//			using (var ms = new MemoryStream())
		//			{
		//				// Setup PdfStamper
		//				using (var stamper = new PdfStamper(oldFile, ms))
		//				{
		//					var font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
		//					var brush = new PdfGState();
		//					brush.FillOpacity = 0.3f;

		//					// Iterate through the pages in the original file
		//					for (var i = 1; i <= oldFile.NumberOfPages; i++)
		//					{
		//						var page = stamper.GetOverContent(i);
		//						page.BeginText();
		//						page.SetFontAndSize(font, 12);
		//						page.SetGState(brush);


		//						var width = oldFile.GetPageSize(i).Width;
		//						var height = oldFile.GetPageSize(i).Height;

		//						// Calculate the position for the first text (30% from the top)
		//						var x1 = width / 2;
		//						//var y1 = height * 0.7f; // 30% from the top
		//						var y1 = height / 2;
		//						page.SetTextMatrix(1, 0, 0, 1, x1, y1);
		//						page.ShowTextAligned(Element.ALIGN_CENTER, $"Downloaded by: {username}, Date: {timestamp}", x1, y1, 30);


		//						// Calculate the position for the second text (60% from the top)
		//						//var x2 = width / 2;
		//						//var y2 = height * 0.4f; // 60% from the top
		//						//page.SetTextMatrix(1, 0, 0, 1, x2, y2);
		//						//page.ShowTextAligned(Element.ALIGN_CENTER, $"Downloaded by: {username}, Date: {timestamp}", x2, y2, 30);

		//						page.EndText();
		//					}
		//				}


		//				// Create a copy of the MemoryStream
		//				var copy = new MemoryStream(ms.ToArray());
		//				copy.CopyTo(outputStream);
		//				outputStream.Close();

		//				// Write the copy of the MemoryStream directly to the response body
		//				//copy.CopyTo(Response.Body);
		//			}

		//		}
		//		return watermarkedFileName;
		//	}
		//	else
		//	{
		//		return string.Empty;
		//	}
		//}

		//public List<(string, int, int)> AddWatermarkToTiff(string path, string fileName, string originalFileName)
		//{
		//	string extension = System.IO.Path.GetExtension(fileName);
		//	string inputFilePath = Path.Combine(path, fileName);
		//	string UserName = User.FindFirst(ClaimTypes.GivenName).Value;
		//	string text = $"Downloaded by: {UserName}, Date: {DateTime.Now}";
		//	string watermarkedFileName = "";
		//	string downloadfolder = "/Attachments/Downloads/";
		//	downloadfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + downloadfolder);
		//	string watermarkedFilePath = Path.Combine(downloadfolder, watermarkedFileName);
		//	if (!Directory.Exists(downloadfolder))
		//	{
		//		Directory.CreateDirectory(downloadfolder);
		//	}
		//	originalFileName = ((originalFileName == "") ? fileName : originalFileName);

		//	List<(string, int, int)> tiffConvertedToImage = new List<(string, int, int)>();


		//	using (MagickImageCollection images = new MagickImageCollection(inputFilePath))
		//	{
		//		foreach (MagickImage image in images)
		//		{
		//			// Get the dimensions of the image
		//			int width = image.Width;
		//			int height = image.Height;

		//			// Calculate the position to center the text
		//			int textX = width / 2;
		//			int textY = height / 2;
		//			// Annotate the text on the image

		//			// Set the text properties
		//			image.Settings.FontPointsize = height * 0.045; // Increase text size to 36
		//			image.Settings.FillColor = new MagickColor(MagickColors.Gray) { A = (ushort)(Quantum.Max / 3) }; // Set text color to red with 50% opacity
		//			image.Settings.TextGravity = Gravity.Center; // Set text gravity to center
		//			image.Settings.TextInterlineSpacing = 10; // Set text interline spacing

		//			image.Annotate(text, new MagickGeometry(textX, (int)(height * 0.5), 20, 20), Gravity.Center, 30);
		//			// image.Annotate(text, new MagickGeometry(textX, (int)(height * 0.66), 20, 20), Gravity.Center, 30);

		//			// Write the modified images to the output file
		//			string pngName = Guid.NewGuid().ToString() + ".png";
		//			image.Write(downloadfolder + pngName);
		//			//tiffConvertedToImage.Add((pngName+"-"+counter+".png", width, height));
		//			tiffConvertedToImage.Add((pngName, width, height));

		//		}
		//	}

		//	// Read the modified image into a byte array
		//	// byte[] fileBytes = System.IO.File.ReadAllBytes(watermarkedFilePath);

		//	// Set the content type for TIFF files
		//	//  string contentType = "image/tiff";

		//	// Return the modified image in the response body
		//	return tiffConvertedToImage;

		//}


		[HttpGet]
		public IActionResult MPLView()
		{
			return View();
		}

		public JsonResult MPLJsonData(int BaselineEngineKey = 0)
		{
            List<MplJsTreeViewModel> mplJsTrees = GetJsTreeWithTblList(BaselineEngineKey);
            return Json(mplJsTrees);

        } 

        private List<MplJsTreeViewModel> GetJsTreeWithTblList(int BaselineEngineKey)
        {
            List<BaseLineEngineVM> baseLineEngineVMs = new List<BaseLineEngineVM>();
            List<MplTreeViewModel> mplTreeViewModels = new List<MplTreeViewModel>();		
            BaseLineEngineVM baselineformpl = new BaseLineEngineVM();
            using (var connection = mPDapperContext.CreateConnection())
            {
                var baselineengine = connection.QueryMultiple($"dbo.GetBaseLineEngines");
                baseLineEngineVMs = baselineengine.Read<BaseLineEngineVM>().ToList();
                if (BaselineEngineKey == 0)
                {
                    baselineformpl = baseLineEngineVMs.Where(x => x.is_active == "Active").FirstOrDefault();
                }
                else
                {
                    baselineformpl = baseLineEngineVMs.Where(x => x.BL_Engine_Dbkey == BaselineEngineKey).FirstOrDefault();
                }
                var mpl = connection.QueryMultiple($"[dbo].[Get_MPL_JSTree_tbl_data] @BL_Engine_Db_key={BaselineEngineKey}");
                mplTreeViewModels = mpl.Read<MplTreeViewModel>().ToList();
            }
            List<MplJsTreeViewModel> mplJsTrees = ContructMPLJsTreeModel(baselineformpl, mplTreeViewModels);
            return mplJsTrees;
        }


        public JsonResult GetUniqueMplPartList(int BaselineEngineKey = 0, int EngineKey = 0)
        {
            DataTable dataTable = MPGlobals.GetDataForDatalist("dbo.[Get_Unique_MPL_Parts] @BL_Engine_Dbkey = " + BaselineEngineKey + ",@Engine_Dbkey = " + EngineKey + "");
            return Json(MPGlobals.GetTableAsList(dataTable));
        }


        #region Part Breakdown Structure

        public IActionResult PBSInfo(int PartDbkey, int PartRelationkey)
		{
            MasterPartViewModel vm = new MasterPartViewModel();
            vm.LoadSelectLists();
          
                Engine_Parts_Master dbData = _dbContext.Engine_Parts_Masters.Where(x => x.Engine_Part_Dbkey == PartDbkey).FirstOrDefault();
                Engine_Parts_Usage engine_Parts_Usage = _dbContext.Engine_Parts_Usages.Where(x => x.Part_relation_dbkey == PartRelationkey).FirstOrDefault();
              
				vm.Engine_Part_Dbkey = dbData.Engine_Part_Dbkey;
                vm.Part_relation_dbkey = PartRelationkey;
                vm.Draw_part_no = dbData.Draw_part_no;
                vm.Description = engine_Parts_Usage.Description;
				vm.Module_PBS = engine_Parts_Usage.Module_PBS;
				vm.PartType_PBS = engine_Parts_Usage.PartType_PBS;
            return PartialView(vm);
        }

        [HttpPost]
        public JsonResult SavePBSInfo(int PartDbkey, int PartRelationDbkey, string ModulePBS, string PartTypePBS)
        {
            try
            {
                SavePBSResult result = new SavePBSResult();

                using (var connection = mPDapperContext.CreateConnection())
                {
                    var data = connection.QueryMultiple($"[dbo].[SavePBSInfo] @PartRelationDbkey={PartRelationDbkey}, @ModulePBS='{ModulePBS}', @PartTypePBS='{PartTypePBS}', @UpdatedBy={GetCurrentUserId()}, @ApplyHierarchy=1");
                    result = data.Read<SavePBSResult>().FirstOrDefault();
                }

                if (result != null && result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        updatedPartsCount = result.UpdatedPartsCount
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result?.Message ?? "Unknown error occurred"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Result class for SP return values
        public class SavePBSResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public int UpdatedPartsCount { get; set; }
        }

        // Helper method - replace with your actual user ID logic
        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
        }

        public IActionResult PBSView()
        {
            var pbsData = GetPBSDataFromSP();

            // Add row and column information for layout
            int modulesPerRow = 4; // 4 modules per row on large screens
            for (int i = 0; i < pbsData.Count; i++)
            {
                pbsData[i].RowIndex = i / modulesPerRow;
                pbsData[i].ColumnIndex = i % modulesPerRow;
                pbsData[i].ModuleIndex = i;
            }

            return View(pbsData);
        }

        private List<PBSModuleSummary> GetPBSDataFromSP()
        {
            List<PBSModuleSummary> pbsSummaryDate = new List<PBSModuleSummary>();

            using (var connection = mPDapperContext.CreateConnection())
            {
                var data = connection.QueryMultiple($"[dbo].[GetPBSSummary]");
                pbsSummaryDate = data.Read<PBSModuleSummary>().ToList();
            }

            return pbsSummaryDate;
        }

        // Enhanced model class with layout information
        public class PBSModuleSummary
        {
            public int ModuleId { get; set; }
            public string ModuleName { get; set; }
            public int DisplayOrder { get; set; }
            public int AssemblyCount { get; set; }
            public int SubAssemblyCount { get; set; }
            public int PartCount { get; set; }
            public int TotalCount { get; set; }

            // New properties for layout
            public int RowIndex { get; set; }
            public int ColumnIndex { get; set; }
            public int ModuleIndex { get; set; }
        }
        #endregion
    }
}
