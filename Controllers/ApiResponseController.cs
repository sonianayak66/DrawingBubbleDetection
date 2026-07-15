using Microsoft.AspNetCore.Mvc;
using MPCRS.Utilities;
using MPCRS.ViewModels;
using System.Text.Json;
using MPCRS.Utilities;
using System.Net.Http.Headers;
using System.Text;


namespace MPCRS.Controllers
{
    public class ApiResponseController : Controller
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CallResponse(APIResponseVM aPIResponseVM)
        {
            try
            {
                var url = aPIResponseVM.url;
                if (IsValidUrl(url) == false)
                {
                    return Json(new { response = "Invalid URL" });
                }

                var responseData = await CallApiAsync(aPIResponseVM.url, aPIResponseVM.Content,aPIResponseVM.type, aPIResponseVM.FormatedType);
               // var jsonDeserialzeData = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<JsonElement>(responseData));
               // var jsonData = JsonConvert.SerializeObject(jsonDeserialzeData, new JsonSerializerSettings { Formatting = Formatting.Indented });

                return Json(new { response = responseData });
            }            
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex); return Json(new { success = false, msg = ex.Message });
            }
        }

        public async Task<string> CallApiAsync(string url,
            string? httpContent,
            string HttpType,
             bool formatOutput = true
            )
        {
            var responsedata ="" ;
            HttpContent? contect = httpContent != null? new StringContent(httpContent):null;
            Constants.HttpAction action ;
            if (Constants.HttpAction.TryParse(HttpType, true, out action))
            {
                responsedata = await CallApiAsync(url, contect, action, formatOutput);               
            }
            return responsedata;
        }
        public async Task<string> CallApiAsync(string url,           
            HttpContent? httpContent = null,
            Constants.HttpAction action = Constants.HttpAction.GET,
             bool formatOutput = true
            )
        {
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();

                var JsonConverter = JsonSerializer.Deserialize<JsonElement>(json); 

                string prettyJson = JsonSerializer.Serialize(JsonConverter, new JsonSerializerOptions { WriteIndented =  true });

                return prettyJson;
            }
            else
            {
                return $"Error : {response.StatusCode}";
            }
        }

        public bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }
            bool output = Uri.TryCreate(url, UriKind.Absolute, out Uri uriOutput) || (uriOutput.Scheme == Uri.UriSchemeHttp) || (uriOutput.Scheme == Uri.UriSchemeHttps);
            return output;
        }



        [HttpPost]
        public async Task<IActionResult> OllamaTest(string url, string prompt)
        {
            var requestBody = new
            {
                prompt = prompt
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var response = await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, error = $"Error: {response.StatusCode}, Details: {errorContent}" });
                }

                var result = await response.Content.ReadAsStringAsync();
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging framework)
                Console.WriteLine($"An error occurred: {ex.Message}");
                return Json(new { success = false, error = "Internal server error" });
            }
        }

        private static void LogError(Exception ex)
        {
            // You can replace this with your logging framework of choice
            Console.WriteLine("An error occurred:");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

    }
}
