using backend.Models;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace backend.Repositories
{
    public class EmployeeDeletionRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<EmployeeDeletionRepository> _logger;

        public EmployeeDeletionRepository(ILogger<EmployeeDeletionRepository> logger)
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext")
                ?? throw new InvalidOperationException("Connection string 'UserContext' not found.");

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Valida que el empleado existe y pertenece a la empresa
        /// </summary>
        public async Task<bool> ValidateEmployeeExistsAsync(int personaId, long companyId)
        {
            using var connection = new SqlConnection(_connectionString);

            const string query = @"
                SELECT COUNT(1)
                FROM Empleado e
                WHERE e.id = @PersonaId 
                  AND e.idCompny = @CompanyId
                  AND (e.IsDeleted = 0 OR e.IsDeleted IS NULL)";

            var count = await connection.ExecuteScalarAsync<int>(query, new
            {
                PersonaId = personaId,
                CompanyId = companyId
            });

            return count > 0;
        }

        /// <summary>
        /// Valida que el empleado no sea dueño de empresa
        /// </summary>
        public async Task<bool> IsCompanyOwnerAsync(int personaId)
        {
            using var connection = new SqlConnection(_connectionString);

            const string query = @"
                SELECT COUNT(1)
                FROM Empresa
                WHERE DueñoEmpresa = @PersonaId";

            var count = await connection.ExecuteScalarAsync<int>(query, new
            {
                PersonaId = personaId
            });

            return count > 0;
        }

        /// <summary>
        /// Verifica si el empleado tiene pagos registrados en planilla
        /// </summary>
        public async Task<EmployeePayrollStatus> CheckPayrollStatusAsync(int personaId)
        {
            using var connection = new SqlConnection(_connectionString);

            const string query = @"
                SELECT 
                    CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS HasPayments,
                    COUNT(*) AS PaymentCount,
                    MIN(PaymentDate) AS FirstPaymentDate,
                    MAX(PaymentDate) AS LastPaymentDate
                FROM PayrollPayment
                WHERE EmployeeId = @PersonaId";

            try
            {
                var result = await connection.QuerySingleOrDefaultAsync<EmployeePayrollStatus>(
                    query, new { PersonaId = personaId });

                return result ?? new EmployeePayrollStatus
                {
                    HasPayments = false,
                    PaymentCount = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando estado de pagos para empleado {PersonaId}", personaId);
                throw;
            }
        }

        /// <summary>
        /// Ejecuta el borrado LÓGICO mediante Stored Procedure
        /// Llama a SP_DeleteEmployee_Logical (archivo 027)
        /// </summary>
        public async Task<bool> ExecuteLogicalDeleteAsync(int personaId)
        {
            using var connection = new SqlConnection(_connectionString);

            const string query = "EXEC SP_DeleteEmployee_Logical @PersonaId";

            try
            {
                var result = await connection.QuerySingleOrDefaultAsync<SpResult>(
                    query, new { PersonaId = personaId });

                if (result?.Success == 1)
                {
                    _logger.LogInformation(
                        "Borrado lógico exitoso para empleado {PersonaId}", personaId);
                    return true;
                }

                var message = result?.Message ?? "Sin mensaje";
                _logger.LogWarning(
                    "SP_DeleteEmployee_Logical retornó Success=0 para empleado {PersonaId}. Mensaje: {Message}",
                    personaId, message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error ejecutando SP_DeleteEmployee_Logical para empleado {PersonaId}", personaId);
                throw;
            }
        }

        /// <summary>
        /// Ejecuta el borrado FÍSICO mediante Stored Procedure
        /// Llama a SP_DeleteEmployee_Physical (archivo 028)
        /// </summary>
        public async Task<bool> ExecutePhysicalDeleteAsync(int personaId)
        {
            using var connection = new SqlConnection(_connectionString);

            const string query = "EXEC SP_DeleteEmployee_Physical @PersonaId";

            try
            {
                var result = await connection.QuerySingleOrDefaultAsync<SpResult>(
                    query, new { PersonaId = personaId });

                if (result?.Success == 1)
                {
                    _logger.LogInformation(
                        "Borrado físico exitoso para empleado {PersonaId}", personaId);
                    return true;
                }

                var message = result?.Message ?? "Sin mensaje";
                _logger.LogWarning(
                    "SP_DeleteEmployee_Physical retornó Success=0 para empleado {PersonaId}. Mensaje: {Message}",
                    personaId, message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error ejecutando SP_DeleteEmployee_Physical para empleado {PersonaId}", personaId);
                throw;
            }
        }

        /// <summary>
        /// Clase auxiliar para mapear resultado de Stored Procedures
        /// </summary>
        private class SpResult
        {
            public int Success { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }
}