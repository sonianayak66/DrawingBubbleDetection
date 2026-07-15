using ExcelDataReader;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using MPCRS.Models;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using XSystem.Security.Cryptography;

namespace MPCRS.Utilities
{
    public class MPGlobals
    {

        private static string connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["MPCRS"];
        private static string DefaultDateFormat = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["DefaultDateFormat"];
        private static string DefaultDateTimeFormat = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["DefaultDateTimeFormat"];


        public static string GetAppenvironment()
        {
            //  var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development;
            string appenvironment = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["AppEnvironment"];
            return appenvironment;
        }



        public static string Encrypt_SHA256(string input)
        {
            byte[] bytes = new UTF8Encoding(true).GetBytes(input);
            SHA256Managed managed = new SHA256Managed();
            return BitConverter.ToString(managed.ComputeHash(bytes));
        }

        public static List<Models.Attachment> GetAttachmentsData(string sourceTable, int sourceTableKey)
        {
            List<Models.Attachment> AttachmentsData = new List<Models.Attachment>();
            if(sourceTableKey == 0)
            {
                return AttachmentsData;
            }
            string cmdstr = $"select * from [dbo].[Attachments] where [Source_table_key] = {sourceTableKey} AND [Source_table] = '{sourceTable}'";
            DataTable dt = GetDataForDatalist(cmdstr);
            if (dt.Rows.Count > 0)
            {
                AttachmentsData = JsonConvert.DeserializeObject<List<Models.Attachment>>(JsonConvert.SerializeObject(dt));
            }
            return AttachmentsData;
        }


      

        public static string GetFormattedDate(DateTime? dateTime)
        {
            if (dateTime != null)
            {
                return dateTime.Value.ToString(DefaultDateFormat);
            }
            return string.Empty;
        }

        public static string GetFormattedDateTime(DateTime? dateTime)
        {
            if (dateTime != null)
            {
                return dateTime.Value.ToString(DefaultDateTimeFormat);
            }
            return string.Empty;
        }

        public static string GetISOFormatDate(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return dateTime.Value.ToString("yyyy-MM-dd");
            }
            return string.Empty;
        }
        public static DataSet GetDataForDataSet(string CmdStr)
        {
            DataSet dt = new DataSet();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter ad = new SqlDataAdapter(CmdStr, conn);
                ad.Fill(dt);
                return dt;
            }
        }

        public static DataTable GetDataForDatatable(string CmdStr)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter ad = new SqlDataAdapter(CmdStr, conn);
                ad.Fill(dt);
                return dt;
            }
        }

        public static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    T item = GetItem<T>(row);
                    data.Add(item);
                }
            }
            catch (Exception ex)
            {
                //
            }
            return data;
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        if (dr[column.ColumnName].ToString() != "")
                        {
                            pro.SetValue(obj, dr[column.ColumnName], null);
                        }

                        else
                            continue;
                }
            }
            return obj;
        }

        public static void ExceSQLNonQuery(string Cmd)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(Cmd, conn);
                int result = executeSQLAsync(conn, command).Result;
            }
        }

        public static List<Dictionary<string, object>> GetTableAsList(DataTable dt)
        {
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>(); //creating a list to hold the rows of datatable  
            Dictionary<string, object> rowelement; //Initialise a dictionary because it will contain columnName and Column Value and the key is column Name

            if (dt.Rows.Count > 0) //if data is there in dt(dataTable)  
            {
                foreach (DataRow dr in dt.Rows)
                {
                    rowelement = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        rowelement.Add(col.ColumnName, dr[col]); //adding columnn  
                    }
                    rows.Add(rowelement);
                }
            }
            return rows;
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties by using reflection   
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names  
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static DataTable GetDataForDatalist(string CmdStr)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlDataAdapter ad = new SqlDataAdapter(CmdStr, conn);
                    ad.SelectCommand.CommandTimeout = 600;  
                    ad.Fill(dt); 
                    return dt;
                }
            }
            catch (Exception ex)
            {
                return new DataTable();
            }
        }

        public static async Task<DataTable> getSQLDataAsync(string CmdStr)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    SqlCommand command = new SqlCommand(CmdStr, conn);
                    var reader = await command.ExecuteReaderAsync();
                    dt.Load(reader);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                return new DataTable();
            }
        }

        static async Task<int> executeSQLAsync(SqlConnection conn, SqlCommand cmd)
        {
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return 1;
        }

        public static string GetOnedata(string CmdStr)
        {
            DataTable dt = GetDataForDatalist(CmdStr);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0][0].ToString();
            }
            else
            {
                return "0";
            }
        }

        public static DataSet GetDataForDataset(string cmdStr)
        {
            var ds = new DataSet();
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(cmdStr, conn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;   // because cmdStr is a full text command (EXEC ...)
                da.Fill(ds);                          // fills ds.Tables[0], ds.Tables[1], ...
            }
            return ds;
        }

        public static List<EngineBuildComponents> GetAllChildren(List<EngineBuildComponents> source, int partNumber)
        {
            List<EngineBuildComponents> data = source.Where(x => x.Parent_id == partNumber).ToList();

            if (data != null)
            {
                return data.ToList();
            }
            else
            {
                return new List<EngineBuildComponents>();
            }
        }


        public static List<MPLReportData> GetAllChildrenMPL(List<MPLReportData> source, int partNumber)
        {
            List<MPLReportData> data = source.Where(x => x.Parent_id == partNumber).ToList();

            if (data != null)
            {
                return data.ToList();
            }
            else
            {
                return new List<MPLReportData>();
            }
        }


        public static List<EngineBuildComponents> GetDirectParts(List<EngineBuildComponents> source, int partNumber)
        {
            try
            {
                List<EngineBuildComponents> data = source.Where(x => x.IsParent != "Yes").ToList();
                List<EngineBuildComponents> retData = new List<EngineBuildComponents>();
                var parentsList = data.Where(x => x.Level == 3).Select(x => x.Parent_id).Distinct();

				//var parentsList = data.Where(x => x.Level == 2 && x.IsParent == "Yes").Select(x => x.Engine_Part_Dbkey).Distinct();
				foreach (var item in parentsList)
                {
                    var parentItem = source.Where(x => x.Engine_Part_Dbkey == item).FirstOrDefault();
                    if (parentItem != null)
                    {
                        retData.Add(parentItem);
                    }
                }

                foreach (var item in data.Where(x => x.Parent_id == partNumber))
                {
                    retData.Add(item);
                }

                if (retData != null)
                {
                    return retData.ToList();
                }
                else
                {
                    return new List<EngineBuildComponents>();
                }
            }
            catch (Exception ex)
            {
                return new List<EngineBuildComponents>();
            }

        }

        public static List<MPLReportData> GetDirectPartsMPL(List<MPLReportData> source, int partNumber)
        {
            try
            {
                List<MPLReportData> data = source.Where(x => x.IsParent != "Yes" ).ToList();
               
                // List<MPLReportData> data = source;
                List<MPLReportData> retData = new List<MPLReportData>();
                var parentsList = data.Where(x => x.Level == 3).Select(x => x.Parent_id).Distinct(); 

                foreach (var item in parentsList)
                {
                    var parentItem = source.Where(x => x.Engine_Part_Dbkey == item).FirstOrDefault();
                    if (parentItem != null)
                    {
                        retData.Add(parentItem);
                    }
                }


                var parentsList1 = source.Where(x => x.Level == 3 && x.IsParent == "Yes").Select(x => x.Parent_id).Distinct();

                foreach (var item in parentsList1)
                {
                    //old logic
                    //var parentItem = source.Where(x => x.Engine_Part_Dbkey == item).FirstOrDefault();
                    //if (parentItem != null)
                    //{
                    //    retData.Add(parentItem);
                    //}

                    // CHECK IF ALREADY ADDED
                    if (!retData.Any(x => x.Engine_Part_Dbkey == item))
                    {
                        var parentItem = source.Where(x => x.Engine_Part_Dbkey == item).FirstOrDefault();
                        if (parentItem != null)
                        {
                            retData.Add(parentItem);
                        }
                    }
                }


                foreach (var item in data.Where(x => x.Parent_id == partNumber))
                {
                    retData.Add(item);
                }

                if (retData != null)
                {
                    return retData.ToList();
                }
                else
                {
                    return new List<MPLReportData>();
                }
            }
            catch (Exception ex)
            {
                return new List<MPLReportData>();
            }

        }


        public static DataTable ExceltoDatatable(string excelFilePath)
        {
            DataTable dt = new DataTable();

            using (var stream = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IExcelDataReader reader = null;
                if (excelFilePath.EndsWith(".xls"))
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if (excelFilePath.EndsWith(".xlsx"))
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }

                if (reader == null)
                    return dt;

                var ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true,
                        FilterColumn = (rowReader, columnIndex) =>
                        {
                            return rowReader[columnIndex] != null;
                        }
                    }
                });

                dt = ds.Tables[0];

                return dt;
            }
        }



		public static void SendEmail_withVM(EmailModel emailmodel)
		{
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            try
			{
                string FromAddress = "";
				string smtpvalue = "";
				bool EnableSslValue = false;
				string Password = "";
				int portnumber = 0;

                DataTable dt = GetDataForDatalist("Select * from Mail_Credentials");
			
				if (dt.Rows.Count == 1)
				{
					FromAddress = dt.Rows[0].ItemArray[1].ToString();
					smtpvalue = dt.Rows[0].ItemArray[3].ToString();
					if (dt.Rows[0].ItemArray[5].ToString() == "YES")
					{
						EnableSslValue = true;
					}
					Password = dt.Rows[0].ItemArray[2].ToString();
					portnumber = Int32.Parse(dt.Rows[0].ItemArray[4].ToString());



					using (System.Net.Mail.MailMessage mm = new System.Net.Mail.MailMessage())
					{
						mm.From = (new MailAddress(FromAddress));

						if (emailmodel.Recipients != null)
						{
							if (emailmodel.Recipients != "")
							{
								string[] To = emailmodel.Recipients.Split(',');
								for (int i = 0; i < To.Length; i++)
								{
									mm.To.Add(new MailAddress(To.GetValue(i).ToString()));
								}
							}

						}

						if (emailmodel.CopyTo != null)
						{
							if (emailmodel.CopyTo != " ")
							{
								string[] Cc = emailmodel.CopyTo.Split(',');
								for (int i = 0; i < Cc.Length; i++)
								{
									mm.CC.Add(new MailAddress(Cc.GetValue(i).ToString()));
								}
							}

						}

						mm.Subject = emailmodel.MailSubject;
						mm.Body = emailmodel.MailBody;

						//if (attachment != "")
						//{

						//	System.Net.Mail.Attachment file;
						//	file = new System.Net.Mail.Attachment(attachment);
						//	file.Name = attachmentname;
						//	mm.Attachments.Add(file);
						//}

						mm.IsBodyHtml = true;
						SmtpClient smtp = new SmtpClient();
						smtp.Host = smtpvalue;
						smtp.EnableSsl = EnableSslValue;
						System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential(FromAddress, Password);
						smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
						smtp.UseDefaultCredentials = false;
                  
						smtp.Credentials = NetworkCred;
						smtp.Port = portnumber;
						smtp.Send(mm);
					}
				}
			}
			catch (Exception ex)
			{
                
                ErrorHandler.LogException(ex);
                //   Logger.WriteToFile("Mail sender Exception" + ex.Message + " " + ex.StackTrace);
            }



		}


        public static DataSet GetDataSet(string CmdStr)
        {
            try
            {
                DataSet ds = new DataSet();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlDataAdapter ad = new SqlDataAdapter(CmdStr, conn);
                    ad.SelectCommand.CommandTimeout = 600;
                    ad.Fill(ds);
                    return ds;
                }
            }
            catch (Exception ex)
            {
                return new DataSet();
            }
        }
    }
}
