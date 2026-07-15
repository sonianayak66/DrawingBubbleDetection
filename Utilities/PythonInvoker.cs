using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace MPCRS.Utilities
{
    public class PythonInvoker
    {

		public static string RunPythonScript(string scriptPath,string scriptType, List<string> parameters)
        {
            try
            {
				string pythonpath = @"C:\Users\Administrator\AppData\Local\Programs\Python\Python312";
				//string pythonpath = @"C:\Users\dell\AppData\Local\Programs\Python\Python312";
				//string pythonpath = _pythonPath;

                string tempFilePath = Path.GetTempFileName();
				var arguments = string.Empty;	
				if (scriptType != "Response")
				{
					var args = new
					{
						file_name = parameters[0],
						doc_id = parameters[1],
						texts = parameters.Skip(2).ToList()
					};
					File.WriteAllText(tempFilePath, JsonConvert.SerializeObject(args));
				}
				else
				{
					var args = new
					{
						file_name = "",
						doc_id = "",
						texts = parameters[0],
					};
					File.WriteAllText(tempFilePath, JsonConvert.SerializeObject(args));
				}
				
					arguments = $"\"{scriptPath}\" \"{tempFilePath}\"";


				//foreach (var item in parameters)
				//{
				//	arguments += $" \"{item}\"";  
				//}
				var psi = new ProcessStartInfo
				{
					FileName = Path.Combine(pythonpath, "python.exe"),
					//FileName = "python",
					// Arguments = $"\"{scriptPath}\" \"{methodTOCall}\"",

					Arguments = arguments,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				};

				using (var process = Process.Start(psi))
				{
					process.WaitForExit();

					var output = process.StandardOutput.ReadToEnd();
					var error = process.StandardError.ReadToEnd();
					var error2 = error;


					if (!string.IsNullOrWhiteSpace(output))
					{
						return output;
					}


					if (!string.IsNullOrWhiteSpace(error))
					{
						ErrorHandler.LogException(new Exception($"Python script error: {error}"));
						return error;
					}

					return output;
				}
			}
            catch (Exception ex)
            {
				ErrorHandler.LogException(ex);
				return ex.ToString();

			}
		
        }

 
    }
}
