using backend.Interfaces;
using backend.Interfaces.Repositories;
using backend.Models.Payroll;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace backend.Repositories
{
    public class PayrollPositionRepository : IPayrollPositionRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<PayrollPositionRepository> _logger;

        public PayrollPositionRepository(
            IDbConnectionFactory connectionFactory,
            ILogger<PayrollPositionRepository> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SavePositionAsync(PayrollPosition position)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string query = @"
                INSERT INTO [Fiesta_Fries_DB].[PayrollPosition] (
                    PayrollId,
                    EmployeeId,
                    CompanyId,
                    Position
                ) VALUES (
                    @PayrollId,
                    @EmployeeId,
                    @CompanyId,
                    @Position
                )";

            try
            {
                await connection.ExecuteAsync(query, position);
                _logger.LogDebug("Saved position for employee {EmployeeId} in payroll {PayrollId}", position.EmployeeId, position.PayrollId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving position for employee {EmployeeId} in payroll {PayrollId}", position.EmployeeId, position.PayrollId);
                throw;
            }
        }
    }
}
