using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using backend.Interfaces;

namespace backend.Infrastructure
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("UserContext")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}