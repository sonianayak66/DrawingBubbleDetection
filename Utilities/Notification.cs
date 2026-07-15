using System.Data;
using System.Net.Mail;
using System.Net;
using MPCRS.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using System.Web.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace MPCRS.Utilities
{
    public class Notification
    {
        public static DTOResponse SendSMS(string phoneno, string OTP)
        {
            DTOResponse dTOResponse = new DTOResponse();
            if (MPGlobals.GetAppenvironment() != "SIT")
            {
                string sURL;
                StreamReader objReader;
                string userName = "mlacw.org";
                string password = "913341589";
                string templated_id = "1707165691413083708";
                string msgText = $"mLAC portal login OTP is {OTP} Valid for {10} minutes. -mLACW";
                sURL = $@"https://www.txtguru.in/imobile/api.php?username={userName}&password={password}&source=MLACWA&dmobile={phoneno}&dlttempid={templated_id}&message={msgText}";
                WebRequest wrGETURL;
                wrGETURL = WebRequest.Create(sURL);
                try
                {
                    dTOResponse.Result = true;
                    Stream objStream;
                    objStream = wrGETURL.GetResponse().GetResponseStream();
                    objReader = new StreamReader(objStream);
                    var res = objReader.ReadToEndAsync();
                    objReader.Close();
                    dTOResponse.Result = true;
                    dTOResponse.ResponseMessage = "OTP sent";
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                    dTOResponse.Result = false;
                    dTOResponse.ResponseMessage = ex.Message;
                }
            }
            else
            {
                dTOResponse.Result = true;
                dTOResponse.ResponseMessage = "SIT OTP";
            }

            return dTOResponse;
        }

        public static DTOResponse SendEmail(EmailModel emailmodel)
        {
            DTOResponse dTOResponse = new DTOResponse();
            try
            {
                string FromAddress = "";
                string smtpvalue = "";
                bool EnableSslValue = false;
                string Password = "";
                int portnumber = 0;

                DataTable dt = MPGlobals.GetDataForDatatable("Select * from Mail_Credentials");

                if (dt.Rows.Count == 1)
                {
                    FromAddress = dt.Rows[0].ItemArray[1].ToString();
                    Password = dt.Rows[0].ItemArray[2].ToString();
                    smtpvalue = dt.Rows[0].ItemArray[3].ToString();
                    portnumber = Int32.Parse(dt.Rows[0].ItemArray[4].ToString());
                    if (dt.Rows[0].ItemArray[5].ToString().ToLower() == "yes")
                    {
                        EnableSslValue = true;
                    }

                    using (System.Net.Mail.MailMessage mm = new System.Net.Mail.MailMessage())
                    {
                        mm.From = (new MailAddress(FromAddress));

                        if (!emailmodel.Recipients.IsNullOrEmpty())
                        {
                            
                                string[] To = emailmodel.Recipients.Split(',');
                                for (int i = 0; i < To.Length; i++)
                                {
                                    mm.To.Add(new MailAddress(To.GetValue(i).ToString()));
                                }
                            

                        }

                        if (!emailmodel.CopyTo.IsNullOrEmpty())
                        {
                           
                                string[] Cc = emailmodel.CopyTo.Split(',');
                                for (int i = 0; i < Cc.Length; i++)
                                {
                                    mm.CC.Add(new MailAddress(Cc.GetValue(i).ToString()));
                                }
                          
                        }


                        if (!emailmodel.BlindCopy.IsNullOrEmpty())
                        {
                                string[] bcc = emailmodel.BlindCopy.Split(',');
                                for (int i = 0; i < bcc.Length; i++)
                                {
                                    mm.Bcc.Add(new MailAddress(bcc.GetValue(i).ToString()));
                                }
                        }



                        mm.Subject = emailmodel.MailSubject;
                        mm.Body = emailmodel.MailBody;

                        //if (!string.IsNullOrEmpty(emailmodel.attachment))
                        //{

                        //    System.Net.Mail.Attachment file;
                        //    file = new System.Net.Mail.Attachment(emailmodel.attachment);
                        //    file.Name = emailmodel.attachmentname;
                        //    mm.Attachments.Add(file);
                        //}

                        mm.IsBodyHtml = emailmodel.IsHTML;
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = smtpvalue;
                        smtp.EnableSsl = EnableSslValue;
                        System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential(FromAddress, Password);
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = NetworkCred;
                        smtp.Port = portnumber;
                        smtp.Send(mm);
                        dTOResponse.Result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                dTOResponse.Result = false;
                ErrorHandler.LogException(ex);
            }

            return dTOResponse;
        }


        public static void TriggerMPLApprovalMail(Engine_Parts_Master engine_Parts_Master)
        {
            EmailModel emailModel = new EmailModel();
            try
            {
                using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
                {
                    Mail_Template mail_Templates = db.Mail_Templates.Where(x => x.Source_table_name == "Engine_Parts_Master").FirstOrDefault();

                    emailModel.MailSubject = mail_Templates.Mail_Subject;
                    if (!String.IsNullOrWhiteSpace(engine_Parts_Master.Drawing_File))
                    {
                        emailModel.MailSubject = mail_Templates.Mail_Subject + $" : Document(s) uploaded - {engine_Parts_Master.Drawing_File}";
                    }

                    if (mail_Templates.Recipients.ToUpper() == "USER_SELECTION")
                    {
                        emailModel.Recipients = db.AspNetUsers.Where(x => x.OldUserDbkey == engine_Parts_Master.Approver_ID).FirstOrDefault().Email;
                    }
                    else
                    {
                        emailModel.Recipients = mail_Templates.Recipients;
                    }

                    emailModel.CopyTo = mail_Templates.CopyTo;
                    emailModel.BlindCopy = mail_Templates.BlindCopy;

                    string Requested = db.AspNetUsers.Where(x => x.OldUserDbkey == engine_Parts_Master.Updated_By).FirstOrDefault().UserName;
                    emailModel.MailBody = mail_Templates.Mail_Body.Replace("#PartNumber#", engine_Parts_Master.Draw_part_no);
                    emailModel.MailBody = emailModel.MailBody.Replace("#Revision#", engine_Parts_Master.Revision);
                    emailModel.MailBody = emailModel.MailBody.Replace("#Requester#", Requested);
                    emailModel.MailBody = Regex.Replace(emailModel.MailBody, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);


                    DTOResponse dTOResponse = SendEmail(emailModel);
                    DataTable dt = MPGlobals.GetDataForDatatable("Select * from Mail_Credentials");
                    string SenderMail = "";
                    if (dt.Rows.Count == 1)
                    {
                        SenderMail = dt.Rows[0].ItemArray[1].ToString();
                    }
                    Mailer_Log mailer_Log = new Mailer_Log();
                    mailer_Log.MailFrom = SenderMail;
                    mailer_Log.MailType = "MPL_Changes_Approval_Mail";
                    mailer_Log.MailTo = emailModel.Recipients;
                    mailer_Log.Subject = emailModel.MailSubject;
                    mailer_Log.Body = emailModel.MailBody;
                    mailer_Log.TriggerStatus = dTOResponse.Result == true ? 1 : 0;
                    mailer_Log.CreatedOn = DateTime.Now;
                    mailer_Log.CreatedBy = engine_Parts_Master.Updated_By;
                    db.Mailer_Logs.Add(mailer_Log);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                ErrorHandler.LogException(ex);
            }
          
        }

        
        public static void TriggerMilestoneDueMail()
        {
            try
            {
                using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
                {
                    Mail_Template mail_Templates = db.Mail_Templates.Where(x => x.Source_table_name == "Procurement_Demand_MileStone").FirstOrDefault();
                    List<MilestoneSummaryVM> summaryInfo = new List<MilestoneSummaryVM>();
                    DataTable dataTable = MPGlobals.GetDataForDatalist("[dbo].[GetMilestoneDuedays]");
                    var jsonstring = JsonConvert.SerializeObject(dataTable);
                    summaryInfo = JsonConvert.DeserializeObject<List<MilestoneSummaryVM>>(jsonstring);
                    string[] EmailTriggersDays = mail_Templates.EmailTriggerDays.Split(",");
                    List<int> intList = EmailTriggersDays.Select(int.Parse).ToList();
                    summaryInfo = summaryInfo.Where(x => intList.Contains(x.RemainingDays)).ToList();
                    foreach (var item in summaryInfo.Where(x => x.DemandingOfficerEmail != null).ToList())
                    {

                        string itemHtmltable = GetMilestoneDemandItemDetails(item);


                        EmailModel emailModel = new EmailModel();
                        emailModel.MailSubject = mail_Templates.Mail_Subject;
                        if (mail_Templates.Recipients.ToUpper() == "DEMANDING_OFFICER")
                        {
                            emailModel.Recipients = item.DemandingOfficerEmail;
                        }
                        else
                        {
                            emailModel.Recipients = mail_Templates.Recipients;
                        }
                        emailModel.CopyTo = mail_Templates.CopyTo;
                        emailModel.BlindCopy = mail_Templates.BlindCopy;
                        emailModel.MailBody = mail_Templates.Mail_Body.Replace("#DemandingOfficer#", item.DemandingOfficerName);
                        emailModel.MailBody = emailModel.MailBody.Replace("#MilestoneName#", item.MilestoneName);
                        emailModel.MailBody = emailModel.MailBody.Replace("#VendorName#", item.VendorName);
                        emailModel.MailBody = emailModel.MailBody.Replace("#MilestoneDate#", item.DueDate.Value.ToString("dd-MM-yyyy"));
                        emailModel.MailBody = emailModel.MailBody.Replace("#DueDays#", item.RemainingDays.ToString());
                        emailModel.MailBody = emailModel.MailBody.Replace("#DemandDetail#",
                                                                            $"<br/><table style='border-collapse: collapse;border: 1px solid black'><tr><th style='border: 1px solid black'>Project</th><td style='border: 1px solid black'>{item.ProjectTitle}</td></tr>" +
                                                                            $"<tr><th style='border: 1px solid black'>MMG File Number</th><td style='border: 1px solid black'>{item.MMG_File_No}</td></tr>" +
                                                                            $"<tr><th style='border: 1px solid black'>Demand Description</th><td style='border: 1px solid black'>{item.Item_Description}</td></tr>" +
                                                                            $"<tr><th style='border: 1px solid black'>Order Number</th><td style='border: 1px solid black'>{item.OrderNumbers}</td></tr></table>" + itemHtmltable
                                                                             );

                        emailModel.IsHTML = true;


                        DTOResponse dTOResponse = SendEmail(emailModel);
                        DataTable dt = MPGlobals.GetDataForDatatable("Select * from Mail_Credentials");
                        string SenderMail = "";
                        if (dt.Rows.Count == 1)
                        {
                            SenderMail = dt.Rows[0].ItemArray[1].ToString();
                        }
                        Mailer_Log mailer_Log = new Mailer_Log();
                        mailer_Log.MailFrom = SenderMail;
                        mailer_Log.MailType = "Milestone_Due_Mail";
                        mailer_Log.MailTo = emailModel.Recipients;
                        mailer_Log.Subject = emailModel.MailSubject;
                        mailer_Log.Body = emailModel.MailBody;
                        mailer_Log.TriggerStatus = dTOResponse.Result == true ? 1 : 0;
                        mailer_Log.CreatedOn = DateTime.Now;
                        mailer_Log.CreatedBy = 1;
                        db.Mailer_Logs.Add(mailer_Log);
                    }
                    db.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
		

		}

        private static string GetMilestoneDemandItemDetails(MilestoneSummaryVM milestoneSummary)
        {
            string htmltble = $"<br/><table style='border-collapse: collapse;border: 1px solid black'>" +
                $"<thead><tr><th style='border: 1px solid black'>Items</th>" +
                $"<th style='border: 1px solid black'>Milestone Qty</th>" +
               // $"<th style='border: 1px solid black'>Balance</th></tr>" +
                $"</thead><tbody>";
            string cmdstr = $"[dbo].[Procurment_MileStone] @DemandDbKey = '{milestoneSummary.DemandDbKey}' ";
            DataTable milestoneitems = MPGlobals.GetDataForDatalist(cmdstr);
            List<Procurement_Demand_MileStoneViewModel> procurement_Demand_Mile = JsonConvert.DeserializeObject<List<Procurement_Demand_MileStoneViewModel>>(JsonConvert.SerializeObject(milestoneitems));
            foreach (var item in procurement_Demand_Mile.Select(x => new { x.DemandItemName, x.ItemDescription, x.MilestoneQty, x.MilestoneID, x.DemandItemDbKey }).Where(x => x.MilestoneID == milestoneSummary.MilestoneID))
            {
                htmltble = htmltble + $"<tr><td style='border: 1px solid black'>{@item.DemandItemName}</td>" +
                    $"<td style='border: 1px solid black'>{@item.MilestoneQty}</td>" +
                   // $"<td style='border: 1px solid black'>{@item.MilestoneQty}</td>" +
                    $"</tr>";
            }
            htmltble = htmltble + "</tbody></table>";
            return htmltble;
        }


    }
}
