using backend.Infrastructure;
using backend.Interfaces;
using backend.Models.Payroll;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;

namespace backend.Repositories
{
    public class EmployerHistoricalReportRepository : IEmployerHistoricalReportRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<EmployerHistoricalReportRepository> _logger;

        public EmployerHistoricalReportRepository(IDbConnectionFactory connectionFactory,
            ILogger<EmployerHistoricalReportRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployerHistoricalReportDto>> GetReportAsync(int employerId,
            int? companyId, DateTime? startDate, DateTime? endDate)
        {
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var result = await connection.QueryAsync<EmployerHistoricalReportDto>(
                    "GetEmployerHistoricalReport",
                    new { EmployerId = employerId, CompanyId = companyId, StartDate = startDate,
                        EndDate = endDate },
                    commandType: CommandType.StoredProcedure);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing stored procedure GetEmployerHistoricalReport");
                throw;
            }
        }
    }
}
