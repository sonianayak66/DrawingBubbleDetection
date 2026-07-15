using Microsoft.Data.SqlClient;
using System.Data;

namespace MPCRS.Models
{
    public class MPDapperContext
    {

        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public MPDapperContext()
        {
        }

        public MPDapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("MPCRS");
        }
        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
