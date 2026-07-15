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
using System.Collections.Generic;
using XAct.Library.Settings;
using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;

namespace MPCRS.Controllers
{
	[Authorize]
	public class ConfigurationController : Controller
	{
		private readonly DESI_STFE_PRODContext _dbContext;
		private readonly IConfiguration _configuration;
		private readonly MPDapperContext mPDapperContext;
		public ConfigurationController(DESI_STFE_PRODContext context, IConfiguration configuration, MPDapperContext mPDapperContext)
		{
			_dbContext = context;
			_configuration = configuration;
			this.mPDapperContext = mPDapperContext;
		}
		[ClaimRequirement(UserPermissions.Read_Audit_Logs)]
		public IActionResult AuditLogs()
		{
			return View();
		}


		public ActionResult GetUserLoginLogs(string startDate = "2000-01-01", string endDate = "2000-01-01")
		{
			DataTable dataTable = new DataTable();
			string cmdStr = $"dbo.GetUserLoginLogs @StartDate = '{startDate}', @endDate='{endDate}' ";
			dataTable = MPGlobals.GetDataForDatalist(cmdStr);
			return Json(JsonConvert.SerializeObject(MPGlobals.GetTableAsList(dataTable)));
		}

		public ActionResult GetAuditLogs_OtherActivities(string startDate = "2000-01-01", string endDate = "2000-01-01")
		{
			DataTable dataTable = new DataTable();
			string cmdStr = $"dbo.GetUserLogs_OtherActivities @StartDate = '{startDate}', @endDate='{endDate}' ";
			dataTable = MPGlobals.GetDataForDatalist(cmdStr);
			return Json(JsonConvert.SerializeObject(MPGlobals.GetTableAsList(dataTable)));
		}



		public ActionResult GetAuditLogs(string id = "", int partID = 0, int engineDbkey = 0)
		{
			DataTable dataTable = new DataTable();
			if (partID == 0 && engineDbkey == 0)
			{
				dataTable = MPGlobals.GetDataForDatalist("dbo.Get_Audit_Logs @tatbleName = '" + id + "',@StatusID = 0, @PartID =" + partID + ",@Engine_Dbkey = " + engineDbkey + "");
			}
			else
			{
				dataTable = MPGlobals.GetDataForDatalist("dbo.Get_Audit_Logs @tatbleName = '" + id + "',@StatusID = 0, @PartID =" + partID + ",@Engine_Dbkey = " + engineDbkey + "");
			}

			//return Json(MPGlobals.GetTableAsList(dataTable));
			return Json(JsonConvert.SerializeObject(MPGlobals.GetTableAsList(dataTable)));
		}

		[ClaimRequirement(UserPermissions.Configuration_Read)]
		public IActionResult Index()
		{
			return View();
		}

		[HttpGet]
		[ClaimRequirement(UserPermissions.Configuration_Read)]
		public ActionResult EmailConfigDetail(int id = 0)
		{
			id = int.Parse(MPGlobals.GetOnedata("SELECT [Sl_No]  FROM [dbo].[Mail_Credentials]"));
			return PartialView(GetEmailConfigVM(id));
		}
		private EmailConfigVM GetEmailConfigVM(int id)
		{
			Mail_Credential dbModel = new();
			using (_dbContext)
			{

				EmailConfigVM emailConfigVM = new EmailConfigVM();
				if (id == 0)
				{
					emailConfigVM.MailID = "********";
					emailConfigVM.Password = "********";
					emailConfigVM.SMTP_HostName = "********";
					return emailConfigVM;
				}

				Mail_Credential mail_Credentials = _dbContext.Mail_Credentials.Where(x => x.Sl_No == id).FirstOrDefault();

				emailConfigVM = JsonConvert.DeserializeObject<EmailConfigVM>(JsonConvert.SerializeObject(mail_Credentials));
				return emailConfigVM;
			}
		}

		public ActionResult testEmailConfig(string emailaddress)
		{
			EmailModel emailModel = new EmailModel();
			emailModel.Recipients = emailaddress;
			emailModel.MailBody = "Test email from SHRAM app";
			emailModel.MailSubject = "Test email from SHRAM app";

			Utilities.Notification.SendEmail(emailModel);
			return Json(new { success = true });
		}
        public IActionResult SendMail_MailKit(string emailaddress)
        {
            try
            {
                DataTable dt = MPGlobals.GetDataForDatalist("Select * from Mail_Credentials");
                var message = new MimeMessage();
				string FromMail = dt.Rows[0].ItemArray[1].ToString();
				string Pswd = dt.Rows[0].ItemArray[2].ToString(); 
                message.From.Add(new MailboxAddress("stfe", FromMail ));
                message.To.Add(new MailboxAddress("test", emailaddress));
                message.Subject = "Test mail";

                message.Body = new TextPart("plain")
                {
                    Text = @"Test mail "

                };

                using (var client = new SmtpClient())
                {
					bool EnableSslValue = false;
					int PortNo = Int32.Parse(dt.Rows[0].ItemArray[4].ToString());
					string HostName = dt.Rows[0].ItemArray[3].ToString();

                    if (dt.Rows[0].ItemArray[5].ToString() == "YES")
                    {
                        EnableSslValue = true;
                    }
                    client.Connect(HostName, PortNo, EnableSslValue==true? SecureSocketOptions.StartTls : SecureSocketOptions.None);
					 

					// Note: only needed if the SMTP server requires authentication
					client.Authenticate(FromMail, Pswd);

                    client.Send(message);
                    client.Disconnect(true);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return Json(new { success = false });

            }
        }

        [HttpGet]
		[ClaimRequirement(UserPermissions.Configuration_Write)]
		public ActionResult EmailConfig(int id = 0)
		{
			return PartialView(GetEmailConfigVM(id));
		}

		[HttpPost]
		[ClaimRequirement(UserPermissions.Configuration_Write)]
		public ActionResult EmailConfig(EmailConfigVM vm)
		{
			Mail_Credential dbModel = new();
			using (_dbContext)
			{
				try
				{
					dbModel = JsonConvert.DeserializeObject<Mail_Credential>(JsonConvert.SerializeObject(vm));
					if (vm.Sl_No != 0)
					{
						Mail_Credential dbmodel = _dbContext.Mail_Credentials.AsNoTracking().Where(x => x.Sl_No == vm.Sl_No).FirstOrDefault();
						if (dbmodel != null)
						{

							dbModel.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
							dbModel.Updated_On = DateTime.Now;
							_dbContext.Entry(dbModel).State = EntityState.Modified;

							_dbContext.SaveChanges();
						}
						else
						{
							// this is to ensure it catches the next if condition
							vm.Sl_No = 0;
						}

					}
					if (vm.Sl_No == 0)
					{
						Mail_Credential mailmodel = _dbContext.Mail_Credentials.Where(x => x.Sl_No == dbModel.Sl_No).FirstOrDefault();

						dbModel.Sl_No = vm.Sl_No;

						dbModel.MailID = vm.MailID;
						dbModel.Password = vm.Password;
						dbModel.SMTP_HostName = vm.SMTP_HostName;
						dbModel.SMTP_Port = vm.SMTP_Port;
						dbModel.SSL = vm.SSL;
						dbModel.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
						dbModel.Updated_On = DateTime.Now;

						_dbContext.Add(dbModel);
						_dbContext.SaveChanges();

					}

					//return Json(new { success = true, msg = "Saved successfully" });
					return RedirectToAction("Index");

				}

				catch (Exception ex) { return Json(new { success = false, msg = ex.Message }); }
			}
		}



		[HttpGet]
		[ClaimRequirement(UserPermissions.Configuration_Read)]
		public ActionResult ApproverMailConfigDetail(string template = "Engine_Parts_Master")
		{
			return PartialView(GetApproverVM(template));
		}

		private ApproverMailConfigVM GetApproverVM(string template)
		{
			Mail_Template dbModel = new();
			using (_dbContext)
			{
				ApproverMailConfigVM approverMailConfigVM = new ApproverMailConfigVM();
				Mail_Template mail_Templates = _dbContext.Mail_Templates.Where(x=>x.Source_table_name == template).First();
				if (mail_Templates == null)
				{
					return approverMailConfigVM;
				}
				else
				{
					approverMailConfigVM = JsonConvert.DeserializeObject<ApproverMailConfigVM>(JsonConvert.SerializeObject(mail_Templates));
					return approverMailConfigVM;
				}
			}
		}
		[HttpGet]
		[ClaimRequirement(UserPermissions.Configuration_Write)]
		public ActionResult ApproverMailConfig(string template)
		{
			return PartialView(GetApproverVM(template));
		}
		[HttpPost]
		[ClaimRequirement(UserPermissions.Configuration_Write)]
		public ActionResult ApproverMailConfig(ApproverMailConfigVM vm)
		{
			Mail_Template dbModel = new();
			using (_dbContext)
			{
				try
				{
					dbModel = JsonConvert.DeserializeObject<Mail_Template>(JsonConvert.SerializeObject(vm));
					if (vm.Mail_Temp_ID != 0)
					{
						Mail_Template dbmodel = _dbContext.Mail_Templates.AsNoTracking().Where(x => x.Mail_Temp_ID == vm.Mail_Temp_ID).FirstOrDefault();
						if (dbmodel != null)
						{

							dbModel.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
							dbModel.Updated_On = DateTime.Now;
							_dbContext.Entry(dbModel).State = EntityState.Modified;
							_dbContext.SaveChanges();
						}
						else
						{
							// this is to ensure it catches the next if condition
							vm.Mail_Temp_ID = 0;
						}

					}
					if (vm.Mail_Temp_ID == 0)
					{
						Mail_Template mailmodel = _dbContext.Mail_Templates.Where(x => x.Mail_Temp_ID == dbModel.Mail_Temp_ID).FirstOrDefault();

						dbModel.Mail_Temp_ID = vm.Mail_Temp_ID;
						dbModel.Mail_Subject = vm.Mail_Subject;
						dbModel.Mail_Body = vm.Mail_Body;
						dbModel.Recipients = vm.Recipients;
						dbModel.CopyTo = vm.CopyTo;
						dbModel.BlindCopy = vm.BlindCopy;
						dbModel.Updated_By = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
						dbModel.Updated_On = DateTime.Now;
						dbModel.Mail_Temp_Name = vm.Mail_Temp_Name;
                        dbModel.Source_table_name = vm.Source_table_name;
                        _dbContext.Add(dbModel);
						_dbContext.SaveChanges();

					}

					return RedirectToAction("Index");

				}

				catch (Exception ex) { return Json(new { success = false, msg = ex.Message }); }
			}
		}

		[HttpGet]
		[ClaimRequirement(UserPermissions.Configuration_Write)]
		public ActionResult AuditLogDisplayDataConfig(string sourcetble)
		{
			List<AuditLogDisplayMangerVM> auditLogDisplayMangerVM = getauditLogDisplayManger(sourcetble);
			ViewBag.sourcetble = sourcetble;
			return PartialView(auditLogDisplayMangerVM);
		}

		private List<AuditLogDisplayMangerVM> getauditLogDisplayManger(string sourcetable)
		{
			string cmdstr = "SELECT * FROM [dbo].[AuditLogDisplayManager] where [SourceTable] = '" + sourcetable + "' and Force_Display_Data = 0 order by DisplayOrder";
			if (sourcetable == "ExternalMfgStatus")
			{
				cmdstr = "SELECT * FROM [dbo].[AuditLogDisplayManager] where [SourceTable] = '" + sourcetable + "' and Force_Display_Data = 0 and  [DisplayOrder] = 1 ";
			}
			DataTable dataTable = MPGlobals.GetDataForDatalist(cmdstr);
			return MPGlobals.ConvertDataTable<AuditLogDisplayMangerVM>(dataTable);
		}



		[HttpPost]
		[ClaimRequirement(UserPermissions.Configuration_Write)]
		public ActionResult AuditLogDisplayDataConfig()
		{
			using (_dbContext)
			{

				try
				{
					var Userarrays = Request.Form["DisplayDataArray"];
					List<AuditLogDisplayManager> auditLogDisplayManagers = JsonConvert.DeserializeObject<List<AuditLogDisplayManager>>(Userarrays);
					foreach (AuditLogDisplayManager item in auditLogDisplayManagers)
					{
						if (item.DisplayManagerKey == 0)
						{
							_dbContext.AuditLogDisplayManagers.Add(item);
						}
						else
						{
							AuditLogDisplayManager auditLogDisplayManager2 = new AuditLogDisplayManager();
							auditLogDisplayManager2 = _dbContext.AuditLogDisplayManagers.Where(x => x.DisplayManagerKey == item.DisplayManagerKey).FirstOrDefault();
							auditLogDisplayManager2.Display_ColumnName = item.Display_ColumnName;
							auditLogDisplayManager2.DisplayData = item.DisplayData;
							_dbContext.Entry(auditLogDisplayManager2).State = EntityState.Modified;
							//MPGlobals.ExceSQLNonQuery("update [dbo].[AuditLogDisplayManager] set [Display_ColumnName] ='"+ item.Display_ColumnName + "',[DisplayData] = "+ item.DisplayData + " where [DisplayManagerKey]")   
						}
					}
					_dbContext.SaveChanges();
					return Json(new { success = true, Msg = "Updated Successfully" });
				}
				catch (Exception ex) { return Json(new { success = false, msg = ex.Message }); }
			}
		}

		[HttpGet]
		[ClaimRequirement(UserPermissions.Configuration_Write)]
		public IActionResult WatermarkConfigurationDetails()
		{
			List<WatermarkConfiguration> watermarkConfiguration = _dbContext.WatermarkConfigurations.ToList();
			return PartialView(watermarkConfiguration);
		}

		[HttpPost]
		public IActionResult SaveWatermarkConfiguration([FromBody] List<WatermarkConfiguration> watermarkConfigurations)
		{
			if (watermarkConfigurations.Count > 0)
			{
				foreach (var item in watermarkConfigurations)
				{
					if(item.ConfigurationFor == "Drawings")
					{
						item.FontSize = item.FontSize / 100;
					}
					
					_dbContext.Entry(item).State = EntityState.Modified;
				}
				_dbContext.SaveChanges();
				return Json(new { success = true });
			}
			else
			{
				return Json(new { success = false });
			}
		}





        [HttpGet]
        [ClaimRequirement(UserPermissions.Configuration_Write)]
        public IActionResult ModuleUserMapping()
        {
			var masterGeneralList = _dbContext.Master_Generals.Where(x => x.is_active != 0 && x.Master_Type == "Module_Responsibility").ToList();
            var jsonstring = JsonConvert.SerializeObject(masterGeneralList);
            List<ModuleUserMapping> moduleUserMappings = JsonConvert.DeserializeObject<List<ModuleUserMapping>>(jsonstring);
			List<NCR_ModuleToUserMapping> nCR_ModuleToUserMappings = _dbContext.NCR_ModuleToUserMappings.ToList();
			foreach (var item in moduleUserMappings)
			{
				var users = nCR_ModuleToUserMappings.Where(x => x.Module_ID == item.Master_Dbkey && x.Isactive ==1).Select(x => x.UserGuid).ToArray();
				item.users = users;
            }
            return PartialView(moduleUserMappings);
        }

        [ClaimRequirement(UserPermissions.Configuration_Write)]
        public IActionResult SaveModuleUserMapping([FromBody] IEnumerable<ModuleUserMapping> moduleUserMappings)
        {
			MPGlobals.ExceSQLNonQuery("Update [dbo].[NCR_ModuleToUserMapping] set [Isactive] = 0 ");
			List<NCR_ModuleToUserMapping> ncr = _dbContext.NCR_ModuleToUserMappings.ToList();
			foreach (var item in moduleUserMappings)
			{
					foreach (var usr in item.users)
					{
					   var mapping = ncr.Where(x => x.Module_ID == item.Master_Dbkey && x.UserGuid == usr).FirstOrDefault();
						if (mapping != null)
						{
							mapping.Isactive = 1;
							_dbContext.Entry(mapping).State = EntityState.Modified;
						}
						else
						{
							NCR_ModuleToUserMapping nCR_ModuleToUserMapping = new();
							nCR_ModuleToUserMapping.Module_ID = item.Master_Dbkey;
							nCR_ModuleToUserMapping.UserGuid = usr;
							nCR_ModuleToUserMapping.Isactive = 1;
							nCR_ModuleToUserMapping.UpdatedOn = DateTime.Now;
							nCR_ModuleToUserMapping.UpdatedBy = int.Parse(User.FindFirst(ClaimTypes.Sid).Value);
							_dbContext.Add(nCR_ModuleToUserMapping);
						}
					
					}
			}
			_dbContext.SaveChanges();
			return PartialView(moduleUserMappings);
        }


  
		public IActionResult SendTestMail(int Id)
		{
			ViewBag.Id = Id;
			return PartialView();
		}

        [Authorize]
        //[ClaimRequirement(UserPermissions.Demand_Read)]
        public IActionResult EmailLogs()
        {
            DateTime today = DateTime.Today;
            string StartDate = today.AddMonths(-1).ToString("yyyy-MM-dd");
            string EndDate = today.AddDays(1).ToString("yyyy-MM-dd");
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            return View();
        }
        public string EmailLogsData(string MailType, string StartDate, string EndDate, int EmailLimit)
        {
            string cmdstr = $"dbo.Email_Log_SSP @MailType ='{MailType}',@StartDate ='{StartDate}',@EndDate ='{EndDate}' , @TopLimit= {EmailLimit} ";
            DataTable dataTable = MPGlobals.GetDataForDatalist(cmdstr);
            return JsonConvert.SerializeObject(dataTable, Formatting.Indented);

        }


    }
}
