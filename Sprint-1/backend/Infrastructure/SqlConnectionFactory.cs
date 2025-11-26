using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using backend.Interfaces;

namespace backend.Infrastructure
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        private readonly ILogger<SqlConnectionFactory> _logger;

        public SqlConnectionFactory(IConfiguration configuration, ILogger<SqlConnectionFactory> logger)
        {
            _connectionString = configuration.GetConnectionString("UserContext")
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public IDbConnection CreateConnection()
        {
            try
            {
                var connection = new SqlConnection(_connectionString);
                _logger.LogDebug("Conexión SQL creada para base de datos: {Database}", connection.Database);
                return connection;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error al crear conexión a Azure SQL Database. ErrorNumber: {ErrorNumber}, State: {State}", 
                    ex.Number, ex.State);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear conexión SQL");
                throw;
            }
        }
    }
}