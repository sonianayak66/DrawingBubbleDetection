using MPCRS.Models;
using System;

namespace MPCRS.Utilities
{
    public class ErrorHandler
    {
        public static void LogException(Exception exception)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Logs");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filepath = path + "/ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            { 
                logerror(File.CreateText(filepath), exception);      
            }
            else
            {
                logerror(File.AppendText(filepath), exception); 
            }

            try {
                using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
                {
                    ExceptionLog exceptionLog = new();
                    exceptionLog.ErrorLine = 1;
                    exceptionLog.ErrorNumber = 1;
                    exceptionLog.ErrorProcedure = exception.Message;
                    exceptionLog.DateErrorRaised = DateTime.Now;
                    if (exception.InnerException is not null)
                    {
                        exceptionLog.ErrorMessage = exception.InnerException.ToString();
                    }

                    db.ExceptionLogs.Add(exceptionLog);
                    db.SaveChanges();
                }
            } catch (Exception ex){
                
            }
      
        }


        public static void LogMesaage(string message)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Logs");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filepath = path + "/ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                logerror(File.CreateText(filepath), message);
            }
            else
            {
                logerror(File.AppendText(filepath), message);
            }

            try
            {
                using (DESI_STFE_PRODContext db = new DESI_STFE_PRODContext())
                {
                    ExceptionLog exceptionLog = new();
                    exceptionLog.ErrorLine = 1;
                    exceptionLog.ErrorNumber = 1;
                    exceptionLog.ErrorProcedure = "Custom Logs";
                    exceptionLog.DateErrorRaised = DateTime.Now;
                    exceptionLog.ErrorMessage = message;             
                    db.ExceptionLogs.Add(exceptionLog);
                    db.SaveChanges();
                }
            }
            catch
            {
            }

        }

        public static void logerror(StreamWriter sw,string message)
        {
            try
            {
                sw.WriteLine(DateTime.Now);
                sw.WriteLine(message);
                sw.WriteLine("------------------------------------------------------------------------- -------");
                sw.Dispose();
            }
            catch (Exception)
            {
            }

        }

        public static void logerror(StreamWriter sw, Exception exception)
        {
            try
            { 
                sw.WriteLine(DateTime.Now);
                sw.WriteLine(exception.Message);
                if (exception.InnerException is not null)
                {
                    sw.WriteLine(exception.InnerException);
                }
                sw.WriteLine("------------------------------------------------------------------------- -------");
                sw.Dispose();
            }
            catch (Exception)
            { 
            }
              
        }
    }
}
