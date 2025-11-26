using backend.Models.Payroll;
using backend.Models.Payroll.Results;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using Microsoft.Data.SqlClient;

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
                    FROM [Fiesta_Fries_DB].[Payroll] WHERE CompanyId = @CompanyId
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

        public async Task<PayrollEmployeeReport> GetPayrollEmployeeReportAsync(int payrollId, int employeeId)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                using var multi = await connection.QueryMultipleAsync(
                    "SP_GetPayrollEmployeeReport",
                    new { PayrollId = payrollId, EmployeeId = employeeId },
                    commandType: CommandType.StoredProcedure
                );

                var header = await multi.ReadSingleOrDefaultAsync<EmployeeReportHeader>();
                if (header == null)
                {
                    _logger.LogWarning("No se encontró reporte empleado - Payroll: {PayrollId}, Employee: {EmployeeId}", payrollId, employeeId);
                    throw new InvalidOperationException($"Reporte para planilla {payrollId}, empleado {employeeId} no encontrado");
                }

                var employeeDeductions = (await multi.ReadAsync<EmployeeDeductionItem>()).ToList();

                if (employeeDeductions != null && employeeDeductions.Count > 0)
                {
                    foreach (var d in employeeDeductions)
                    {
                        if (header.SalarioBruto > 0)
                        {
                            if (!string.IsNullOrWhiteSpace(d.DeductionName) &&
                                (d.DeductionName.IndexOf("impuesto", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 d.DeductionName.IndexOf("renta", StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                d.Percentage = (d.DeductionAmount / header.SalarioBruto) * 100m;
                            }
                            else
                            {
                                d.Percentage = d.Percentage * 100m;
                            }
                        }
                        else
                        {
                            d.Percentage = d.Percentage * 100m;
                        }
                    }
                }

                var totalEmployeeDeductions = await multi.ReadSingleOrDefaultAsync<decimal>();
                var employerBenefits = (await multi.ReadAsync<EmployerBenefitItem>()).ToList();

                if (employerBenefits != null && employerBenefits.Count > 0)
                {
                    foreach (var b in employerBenefits)
                    {
                        b.Percentage = b.Percentage * 100m;
                    }
                }

                var totalEmployerBenefits = await multi.ReadSingleOrDefaultAsync<decimal>();
                var totals = await multi.ReadSingleOrDefaultAsync<EmployeeReportTotals>();

                _logger.LogInformation("Reporte por empleado obtenido - Payroll: {PayrollId}, Employee: {EmployeeId}, Deducciones: {DCount}, Beneficios: {BCount}",
                    payrollId, employeeId, employeeDeductions.Count, employerBenefits.Count);

                return new PayrollEmployeeReport
                {
                    Header = header,
                    EmployeeDeductions = employeeDeductions,
                    TotalEmployeeDeductions = totalEmployeeDeductions,
                    EmployerBenefits = employerBenefits,
                    TotalEmployerBenefits = totalEmployerBenefits,
                    Totals = totals ?? new EmployeeReportTotals()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reporte por empleado - Payroll: {PayrollId}, Employee: {EmployeeId}", payrollId, employeeId);
                throw;
            }
        }

        public async Task<List<TopEmployeeDto>> GetTop12EmployeesByCompanyAsync(long companyId)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                const string lastPayrollQuery = @"
                    SELECT TOP 1 PayrollId
                    FROM [Fiesta_Fries_DB].[Payroll] WHERE CompanyId = @CompanyId
                    ORDER BY PeriodDate DESC";

                var payrollId = await connection.ExecuteScalarAsync<int?>(lastPayrollQuery, new { CompanyId = companyId });

                if (payrollId == null)
                {
                    _logger.LogInformation("No hay planillas para la empresa {CompanyId}", companyId);
                    return new List<TopEmployeeDto>();
                }

                const string query = @"
                    SELECT TOP 12
                        pp.EmployeeId,
                        p.firstName + ' ' + p.secondName AS NombreEmpleado,
                        pp.GrossSalary
                    FROM [Fiesta_Fries_DB].[PayrollPayment] pp
                    INNER JOIN [Fiesta_Fries_DB].[Persona] p ON pp.EmployeeId = p.id
                    WHERE pp.PayrollId = @PayrollId
                    ORDER BY pp.GrossSalary DESC";

                var result = (await connection.QueryAsync<TopEmployeeDto>(query, new { PayrollId = payrollId.Value })).ToList();

                _logger.LogInformation("Top {Count} empleados obtenidos para empresa {CompanyId} (Payroll {PayrollId})",
                    result.Count, companyId, payrollId.Value);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo Top12 empleados para empresa {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<DateTime?> GetLastPeriodAsync(int benefitId)
        {
            using var connection = new SqlConnection(_connectionString);
            try
            {
                const string query = @"
                SELECT TOP 1 p.PeriodDate
                FROM [Fiesta_Fries_DB].[EmployerBenefitDeductions] ebd
                INNER JOIN [Fiesta_Fries_DB].[Payroll] p ON ebd.ReportId = p.PayrollId
                WHERE ebd.BenefitId = @BenefitId
                ORDER BY p.PeriodDate DESC";

                var lastPeriod = await connection.ExecuteScalarAsync<DateTime?>(query, new { BenefitId = benefitId });

                _logger.LogInformation("Último periodo para beneficio {BenefitId}: {LastPeriod}", benefitId, lastPeriod);

                return lastPeriod;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error obteniendo último periodo para beneficio {benefitId}", benefitId);
                throw;
            }
        }

        public async Task<List<EmployeeLastPaymentsResult>> GetLast12PaymentsByEmployeeAsync(int employeeId)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                var query = @"
            SELECT TOP 12
                pp.PayrollId AS ReportId,
                FORMAT(py.PeriodDate, 'yyyy-MM-dd') AS Periodo,
                pp.GrossSalary AS SalarioBruto,
                pp.NetSalary AS SalarioNeto
            FROM [Fiesta_Fries_DB].[PayrollPayment] pp
            INNER JOIN [Fiesta_Fries_DB].[Payroll] py ON pp.PayrollId = py.PayrollId
            WHERE pp.EmployeeId = @employeeId
            ORDER BY py.PeriodDate DESC
        ";

                var result = (await connection.QueryAsync<EmployeeLastPaymentsResult>(query, new { employeeId })).ToList();

                _logger.LogInformation("Últimos 12 pagos obtenidos para empleado {EmployeeId}: {Count}", employeeId, result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo últimos 12 pagos para empleado {EmployeeId}", employeeId);
                throw;
            }
        }
    }

    public class TopEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public decimal GrossSalary { get; set; }
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
