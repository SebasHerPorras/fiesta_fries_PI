using backend.Models.Payroll;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace backend.Services
{
    public class PayrollCsvService
    {
        private readonly ILogger<PayrollCsvService> _logger;

        public PayrollCsvService(ILogger<PayrollCsvService> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> GeneratePayrollCsvAsync(PayrollFullReport report)
        {
            try
            {
                using var ms = new MemoryStream();
                using var writer = new StreamWriter(ms, System.Text.Encoding.UTF8);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ","
                });

                // ENCABEZADO
                await writer.WriteLineAsync($"Empresa,{report.Header.NombreEmpresa}");
                await writer.WriteLineAsync($"Empleador,{report.Header.NombreEmpleador}");
                await writer.WriteLineAsync($"Período,{report.Header.PeriodDate:yyyy-MM-dd}");
                await writer.WriteLineAsync($"Frecuencia Pago,{report.Header.FrecuenciaPago}");
                await writer.WriteLineAsync($"Día de Pago,{report.Header.DiaPago}");
                await writer.WriteLineAsync();

                // RESUMEN TOTALES
                await writer.WriteLineAsync("RESUMEN GENERAL");
                await writer.WriteLineAsync("Concepto,Monto");
                await writer.WriteLineAsync($"Total Salarios Brutos,{report.Header.TotalGrossSalary:N2}");
                await writer.WriteLineAsync($"Total Deducciones Empleado,{report.Header.TotalEmployeeDeductions:N2}");
                await writer.WriteLineAsync($"Total Deducciones Empleador,{report.Header.TotalEmployerDeductions:N2}");
                await writer.WriteLineAsync($"Total Beneficios,{report.Header.TotalBenefits:N2}");
                await writer.WriteLineAsync($"Total Salarios Netos,{report.Header.TotalNetSalary:N2}");
                await writer.WriteLineAsync($"COSTO TOTAL EMPLEADOR,{report.Header.TotalEmployerCost:N2}");
                await writer.WriteLineAsync();

                // DETALLE POR EMPLEADO
                await writer.WriteLineAsync("DETALLE POR EMPLEADO");
                csv.WriteHeader<EmployeeDetail>();
                await csv.NextRecordAsync();
                await csv.WriteRecordsAsync(report.Employees);
                await writer.WriteLineAsync();

                // CARGAS PATRONALES
                await writer.WriteLineAsync("CARGAS PATRONALES");
                csv.WriteHeader<ChargeDetail>();
                await csv.NextRecordAsync();
                await csv.WriteRecordsAsync(report.EmployerCharges);
                await writer.WriteLineAsync();

                // DEDUCCIONES EMPLEADO
                await writer.WriteLineAsync("DEDUCCIONES EMPLEADO");
                csv.WriteHeader<DeductionDetail>();
                await csv.NextRecordAsync();
                await csv.WriteRecordsAsync(report.EmployeeDeductions);
                await writer.WriteLineAsync();

                // BENEFICIOS
                await writer.WriteLineAsync("BENEFICIOS");
                csv.WriteHeader<BenefitDetail>();
                await csv.NextRecordAsync();
                await csv.WriteRecordsAsync(report.Benefits);

                await writer.FlushAsync();

                _logger.LogInformation("CSV generado exitosamente - Planilla: {PayrollId}", report.Header.PayrollId);

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando CSV para planilla {PayrollId}", report.Header.PayrollId);
                throw;
            }
        }
        public async Task<byte[]> GeneratePayrollEmployeeCsvAsync(PayrollEmployeeReport report)
        {
            try
            {
                using var ms = new MemoryStream();
                using var writer = new StreamWriter(ms, System.Text.Encoding.UTF8);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ","
                });

                // ENCABEZADO
                await writer.WriteLineAsync($"Empresa,{report.Header.NombreEmpresa}");
                await writer.WriteLineAsync($"Empleado,{report.Header.NombreEmpleado}");
                await writer.WriteLineAsync($"Tipo Empleado,{report.Header.TipoEmpleado}");
                await writer.WriteLineAsync($"Fecha Pago,{report.Header.FechaPago:yyyy-MM-dd}");
                await writer.WriteLineAsync($"Salario Bruto,{report.Header.SalarioBruto:N2}");
                await writer.WriteLineAsync();

                // DEDUCCIONES OBLIGATORIAS
                await writer.WriteLineAsync("DEDUCCIONES OBLIGATORIAS");
                csv.WriteHeader<EmployeeDeductionItem>();
                await csv.NextRecordAsync();
                await csv.WriteRecordsAsync(report.EmployeeDeductions);
                await writer.WriteLineAsync();
                await writer.WriteLineAsync($"Total Deducciones Obligatorias,{report.TotalEmployeeDeductions:N2}");
                await writer.WriteLineAsync();

                // BENEFICIOS / DEDUCCIONES VOLUNTARIAS
                await writer.WriteLineAsync("BENEFICIOS / DEDUCCIONES VOLUNTARIAS");
                csv.WriteHeader<EmployerBenefitItem>();
                await csv.NextRecordAsync();
                await csv.WriteRecordsAsync(report.EmployerBenefits);
                await writer.WriteLineAsync();
                await writer.WriteLineAsync($"Total Beneficios Voluntarios,{report.TotalEmployerBenefits:N2}");
                await writer.WriteLineAsync();

                // TOTALES FINALES
                await writer.WriteLineAsync("TOTALES FINALES");
                await writer.WriteLineAsync("Concepto,Monto");
                await writer.WriteLineAsync($"Total Deducciones Obligatorias,{report.Totals.TotalDeduccionesObligatorias:N2}");
                await writer.WriteLineAsync($"Total Beneficios Voluntarios,{report.Totals.TotalBeneficiosVoluntarios:N2}");
                await writer.WriteLineAsync($"Total Deducciones,{report.Totals.TotalDeducciones:N2}");
                await writer.WriteLineAsync($"Pago Neto,{report.Totals.PagoNeto:N2}");

                await writer.FlushAsync();

                _logger.LogInformation("CSV de empleado generado exitosamente - Payroll: {PayrollId}, Employee: {EmployeeId}",
                    report.Header.CompanyId, report.Header.EmployeeId);

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando CSV para reporte por empleado - Payroll: {PayrollId}, Employee: {EmployeeId}",
                    report.Header.CompanyId, report.Header.EmployeeId);
                throw;
            }
        }
    }
}