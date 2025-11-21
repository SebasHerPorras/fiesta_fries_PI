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

                // Calcular porcentaje: para "Impuesto sobre la Renta" usar regla de 3 basada en SalarioBruto y DeductionAmount,
                // para el resto multiplicar por 100 (fallback si SalarioBruto == 0)
                if (employeeDeductions != null && employeeDeductions.Count > 0)
                {
                    foreach (var d in employeeDeductions)
                    {
                        if (header.SalarioBruto > 0)
                        {
                            // Si el nombre sugiere impuesto sobre la renta, calcular porcentaje = (deducción / bruto) * 100
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
                            // Salario bruto 0: mantener comportamiento anterior (multiplicar por 100) para evitar division por cero
                            d.Percentage = d.Percentage * 100m;
                        }
                    }
                }

                var totalEmployeeDeductions = await multi.ReadSingleOrDefaultAsync<decimal>();
                var employerBenefits = (await multi.ReadAsync<EmployerBenefitItem>()).ToList();

                // Para beneficios mantenemos la multiplicación por 100 (no regla de 3 por ahora)
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