using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MPCRS.Controllers
{
    public class PythonController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task ExecuteScript()
        {
            string GenerateResponsePyPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ChatWithDoc/TestPy.py");

            Response.ContentType = "text/event-stream";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            //Response.Headers["Transfer-Encoding"] = "chunked";


            using (var process = new Process())
            {
                process.StartInfo.FileName = "python";
                process.StartInfo.Arguments = GenerateResponsePyPath;
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
                        i++;
                    }
                }

                process.WaitForExit();
            }
        }


        public async Task GetPythontest()
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            for (int i = 0; i < 10; i++)
            {
                await Response.WriteAsync($"data: Message {i}\n\n");
                await Response.Body.FlushAsync();
                await Task.Delay(1000); // Simulate delay between messages
            }
        }
    }
}
