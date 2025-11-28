using backend.Models.Payroll;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.IO;

namespace backend.Services
{
    public class EmployeeHistoricalReportCsvService
    {
        private readonly ILogger<EmployeeHistoricalReportCsvService> _logger;

        public EmployeeHistoricalReportCsvService(
            ILogger<EmployeeHistoricalReportCsvService> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> GenerateCsvAsync(IEnumerable<EmployeeHistoricalReportDto> report)
        {
            try
            {
                using var ms = new MemoryStream();
                using var writer = new StreamWriter(ms, System.Text.Encoding.UTF8);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                });
                csv.WriteField("Tipo de Empleo");
                csv.WriteField("Puesto");
                csv.WriteField("Fecha de Pago");
                csv.WriteField("Salario");
                csv.WriteField("Deducciones");
                csv.WriteField("Beneficios");
                csv.WriteField("Salario Neto");
                await csv.NextRecordAsync();

                foreach (var row in report)
                {
                    csv.WriteField(row.EmploymentType);
                    csv.WriteField(row.Position);
                    csv.WriteField(row.PaymentDate?.ToString("yyyy-MM-dd") ?? string.Empty);
                    csv.WriteField(row.Salary);
                    csv.WriteField(row.DeductionsAmount);
                    csv.WriteField(row.BenefitsAmount);
                    csv.WriteField(row.NetSalary);

                    await csv.NextRecordAsync();
                }

                await writer.FlushAsync();
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Employee Historical Report CSV");
                throw;
            }
        }
    }
}