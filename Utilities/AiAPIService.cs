using iTextSharp.text.xml.xmp;
using Org.BouncyCastle.Ocsp;
using System.Data;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;

namespace MPCRS.Utilities
{
	public class AiAPIService
	{

		private readonly IHttpClientFactory _clientFactory;
		private readonly string _apibaseurl;
		private readonly string _aimodel;
		public AiAPIService(IConfiguration configuration, IHttpClientFactory clientFactory)
		{
			_apibaseurl = configuration["GenAI:ApiUrl"];
			_aimodel = configuration["GenAI:AIModel"];
			_clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
		}

		public async Task<DTOResponse> ChatAsync(string naturalLanguageQuery, string Questiontype, string domainmodule = "",int maxAttempts=0)
		{
            DTOResponse dTOResponse = new DTOResponse();

            string ApiUrl = _apibaseurl + "/api/chat";
			string response = string.Empty;
			var userQ = GetPromt(Questiontype, naturalLanguageQuery, domainmodule);
			var fullPrompt = userQ;
			QueryRequest queryRequest = new QueryRequest();
			queryRequest.promt = fullPrompt;
			queryRequest.AIModel = _aimodel;
            queryRequest.requesttype = Questiontype;
			queryRequest.model = "Model" + maxAttempts.ToString();
			var json = JsonSerializer.Serialize(queryRequest);
			var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
			request.Content = new StringContent(json, Encoding.UTF8, "application/json");
			var client = _clientFactory.CreateClient();
			client.Timeout = Timeout.InfiniteTimeSpan;
			var apiresponse = await client.SendAsync(request);
			if (apiresponse.IsSuccessStatusCode)
			{
				var jsonResponse = await apiresponse.Content.ReadAsStringAsync();
				var responseObject = JsonSerializer.Deserialize<APIResponse>(jsonResponse);
				var sqlquery = responseObject.data.ToString();
				response = ValidateSQLQuery(sqlquery);
				dTOResponse.ResponseMessage = response;
				dTOResponse.ErrorMessage = fullPrompt;
            }

			return dTOResponse;
		}

		private string ValidateSQLQuery(string sqlquery)
		{
            string cleanedQuery = string.Empty;
            cleanedQuery = sqlquery.Trim('`');
			return cleanedQuery;
        }



		public string GetPromt(string Questiontype, string naturalLanguageQuery, string domainmodel)
		{
			try
			{
                var prompt = string.Empty;
				// string[] synoyms = SynonymFinder.Find(naturalLanguageQuery);
				//  string dbschema = GetDatabaseSchema(synoyms);				
				if (Questiontype == "Domain")
                {
					string dbschema = GetModelDatabaseSchema(domainmodel);
					string SampleQuries = GetModelSampleSqlQuries(domainmodel);
					prompt = $@"Given the following MS sql server database schema: {dbschema},
							Task: Generate a SQL query to answer the following question: {naturalLanguageQuery}
							Instructions:You dont need to remember any previous context, Just focus on current question.
							Ensure the column names and table names are correct (make sure they are available on given schema).
							The given schema may contain additional tables that may not be related to current question since we have the complete db schema.
							Use only what is relavent based on the question. when asked about certain columns, try and provide relavent columns on select query for better understanding.
							Concentrate on table names and column names for better accuracy (refer to schema provided)and 
							Provide data only which are columns provided in schema.provide with required SQL query only,
							No explaination required and dont append any other things beginnig or end of the query.";
				 
				}
				else
                {
                    prompt =  naturalLanguageQuery;
                }
                return prompt;
            }
			catch (Exception ex)
			{

				return "some error occurred";
			}
		
		}
		public string GetDatabaseSchema(string[] synoyms)
		{
			string cmdstr = "Select distinct tableSchema from [dbo].[NLPDatabaseDef] where";
			for (int i = 0; i < synoyms.Length; i++)
			{
				string syn = synoyms[i];
				cmdstr += $"(tableName like '%{syn}%' OR tableschema like '%{syn}%')";
				if (i < synoyms.Length - 1)
				{
					cmdstr += " OR ";
				}
			}
			string schema = string.Empty;
			StringBuilder stringBuilder = new StringBuilder();
			DataTable dataTable = MPGlobals.GetDataForDatatable(cmdstr);
			foreach (DataRow item in dataTable.Rows)
			{
				stringBuilder.Append(item["tableSchema"].ToString() + ";");
				stringBuilder.Append(Environment.NewLine);

			}
			return stringBuilder.ToString();

		}
			public string GetModelDatabaseSchema(string domainmodel)
			{
				string cmdstr = $"Select Distinct tableSchema from [dbo].[NLPDatabaseDef] where DomainModel like '%{domainmodel}%'";
				string schema = string.Empty;
				StringBuilder stringBuilder = new StringBuilder();
				DataTable dataTable = MPGlobals.GetDataForDatatable(cmdstr);
				foreach (DataRow item in dataTable.Rows)
				{
					stringBuilder.Append(item["tableSchema"].ToString() + ";");
					stringBuilder.Append(Environment.NewLine);

				}
				return stringBuilder.ToString();
			}


		public string GetModelSampleSqlQuries(string domainmodel)
		{
			string cmdstr = $"SELECT [SampleQuries] from [dbo].[NLPDomainModelDef] where DomainModel like '%{domainmodel}%'";
			string schema = string.Empty;
			StringBuilder stringBuilder = new StringBuilder();
			DataTable dataTable = MPGlobals.GetDataForDatatable(cmdstr);
			foreach (DataRow item in dataTable.Rows)
			{
				stringBuilder.Append(item["SampleQuries"].ToString() + ";");
				stringBuilder.Append(Environment.NewLine);

			}
			return stringBuilder.ToString();
		}


		public class QueryRequest
		{
			public string promt { get; set; }
			public string AIModel { get; set; }
            public string requesttype { get; set; }
			public string model { get; set; }
		}

		public class APIResponse
		{
			public bool success { get; set; }
			public object data { get; set; }

		}
	}
}
