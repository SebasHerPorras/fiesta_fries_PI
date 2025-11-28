using backend.Infrastructure;
using backend.Interfaces;
using backend.Models.Payroll;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
namespace backend.Repositories

{
    public class EmployerByPersonReportRepository : IEmployerByPersonReportRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<EmployerHistoricalReportRepository> _logger;

        public EmployerByPersonReportRepository(IDbConnectionFactory connectionFactory,
            ILogger<EmployerHistoricalReportRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployerByPersonReportDto>> GetReportAsync(long employerId, DateTime? startDate,
            DateTime? endDate, String? employmentType, long? companyId, int? cedula)
        {
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var result = await connection.QueryAsync<EmployerByPersonReportDto>(
                    "GetEmployerReport",
                    new
                    {
                        CedulaEmpleador = employerId,
                        StartDate = startDate,
                        EndDate = endDate,
                        EmploymentType = employmentType,
                        CompanyId = companyId,
                        Cedula = cedula
                    },
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

