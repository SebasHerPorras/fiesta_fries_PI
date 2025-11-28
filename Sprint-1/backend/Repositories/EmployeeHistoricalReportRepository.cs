using backend.Infrastructure;
using backend.Interfaces;
using backend.Models.Payroll;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;

namespace backend.Repositories
{
    public class EmployeeHistoricalReportRepository : IEmployeeHistoricalReportRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<EmployeeHistoricalReportRepository> _logger;

        public EmployeeHistoricalReportRepository(
            IDbConnectionFactory connectionFactory,
            ILogger<EmployeeHistoricalReportRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployeeHistoricalReportDto>> GetReportAsync(
            long employeeId,
            DateTime? startDate,
            DateTime? endDate)
        {
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var result = await connection.QueryAsync<EmployeeHistoricalReportDto>(
                    "sp_GetPayrollByEmployeeAndDates",
                    new
                    {
                        Cedula = employeeId,
                        FechaInicio = startDate,
                        FechaFin = endDate
                    },
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing stored procedure GetEmployeeHistoricalReport");
                throw;
            }
        }
    }
}
