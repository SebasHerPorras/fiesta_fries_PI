using backend.Models.Payroll;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;

namespace backend.Repositories
{
    public class PayrollReportRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<PayrollReportRepository> _logger;

        public PayrollReportRepository(ILogger<PayrollReportRepository> logger)
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext")
                ?? throw new InvalidOperationException("Connection string 'UserContext' not found.");

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PayrollFullReport> GetPayrollFullReportAsync(int payrollId)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                using var multi = await connection.QueryMultipleAsync(
                    "SP_GetPayrollFullReport",
                    new { PayrollId = payrollId },
                    commandType: CommandType.StoredProcedure
                );

                var header = await multi.ReadSingleOrDefaultAsync<PayrollReportHeader>();
                if (header == null)
                {
                    _logger.LogWarning("No se encontró la planilla {PayrollId}", payrollId);
                    throw new InvalidOperationException($"Planilla {payrollId} no encontrada");
                }

                var employees = (await multi.ReadAsync<EmployeeDetail>()).ToList();
                var employerCharges = (await multi.ReadAsync<ChargeDetail>()).ToList();
                var employeeDeductions = (await multi.ReadAsync<DeductionDetail>()).ToList();
                var benefits = (await multi.ReadAsync<BenefitDetail>()).ToList();

                _logger.LogInformation(
                    "Reporte completo obtenido - Planilla: {PayrollId}, Empleados: {Count}",
                    payrollId, employees.Count);

                return new PayrollFullReport
                {
                    Header = header,
                    Employees = employees,
                    EmployerCharges = employerCharges,
                    EmployeeDeductions = employeeDeductions,
                    Benefits = benefits
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reporte completo para planilla {PayrollId}", payrollId);
                throw;
            }
        }

        public async Task<List<PayrollSummary>> GetLast12PayrollsAsync(long companyId)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                const string query = @"
                    SELECT TOP 12
                        PayrollId,
                        PeriodDate,
                        TotalGrossSalary,
                        TotalEmployerCost,
                        TotalNetSalary,
                        IsCalculated,
                        LastModified
                    FROM Payroll
                    WHERE CompanyId = @CompanyId
                    ORDER BY PeriodDate DESC";

                var result = (await connection.QueryAsync<PayrollSummary>(query, new { CompanyId = companyId })).ToList();

                _logger.LogInformation(
                    "Obtenidas {Count} planillas para empresa {CompanyId}",
                    result.Count, companyId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo últimas planillas para empresa {CompanyId}", companyId);
                throw;
            }
        }
    }

    public class PayrollSummary
    {
        public int PayrollId { get; set; }
        public DateTime PeriodDate { get; set; }
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalEmployerCost { get; set; }
        public decimal TotalNetSalary { get; set; }
        public bool IsCalculated { get; set; }
        public DateTime LastModified { get; set; }
    }
}