using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace MPCRS.Utilities
{
    public static class HtmlTableGenerator
    {
        public static string GenerateHtmlTable(string jsonString)
        {
            // Parse the JSON string into a JArray
            try
            {

                //jsonString = jsonString.Replace("docdbkey[", "![");
                //jsonString = jsonString.Replace("[[", "[");
                //jsonString = jsonString.Replace("]]", "]");
                //jsonString = jsonString.Split("!")[1];
               // jsonString = jsonString.Substring(0, jsonString.Length - 1);

                var data = JArray.Parse(jsonString);
                // Start building the HTML table
                StringBuilder html = new StringBuilder();
                html.Append("<table class='table table-bordered'>");

                // Add the header row
                html.Append("<thead><tr>");
                foreach (var header in ((JObject)data[0]).Properties())
                {
                    html.Append($"<th>{header.Name}</th>");
                }
                html.Append("</tr></thead>");

                // Add the data rows
                html.Append("<tbody>");
                foreach (var item in data)
                {
                    html.Append("<tr>");
                    foreach (var property in ((JObject)item).Properties())
                    {
                        html.Append($"<td>{property.Value}</td>");
                    }
                    html.Append("</tr>");
                }
                html.Append("</tbody>");
                html.Append("</table>");

                return html.ToString();
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
                return "Error getting document list!";
            }
           

            
        }
    }
}






