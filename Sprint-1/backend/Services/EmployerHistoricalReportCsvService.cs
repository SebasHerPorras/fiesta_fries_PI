using backend.Models;
using backend.Models.Payroll;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.IO;

namespace backend.Services
{
    public class EmployerHistoricalReportCsvService
    {
        private readonly ILogger<EmployerHistoricalReportCsvService> _logger;

        public EmployerHistoricalReportCsvService(ILogger<EmployerHistoricalReportCsvService> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> GenerateCsvAsync(IEnumerable<EmployerHistoricalReportDto> report)
        {
            try
            {
                using var ms = new MemoryStream();
                using var writer = new StreamWriter(ms, System.Text.Encoding.UTF8);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                });

                // Encabezados
                csv.WriteField("Nombre Empresa");
                csv.WriteField("Frecuencia de pago");
                csv.WriteField("Periodo de pago");
                csv.WriteField("Fecha de pago");
                csv.WriteField("Salario Bruto");
                csv.WriteField("Cargas sociales empleador");
                csv.WriteField("Deducciones voluntarias");
                csv.WriteField("Costo Empleador");
                await csv.NextRecordAsync();

                foreach (var row in report)
                {
                    csv.WriteField(row.Nombre);
                    csv.WriteField(row.FrecuenciaPago);
                    csv.WriteField(row.PeriodoPago?.ToString("yyyy-MM-dd") ?? string.Empty);
                    csv.WriteField(row.FechaPago?.ToString("yyyy-MM-dd") ?? string.Empty);

                    csv.WriteField(row.SalarioBrutoText ?? row.SalarioBruto.ToString());
                    csv.WriteField(row.CargasSocialesEmpleadorText ?? row.CargasSocialesEmpleador.ToString());
                    csv.WriteField(row.DeduccionesVoluntariasText ?? row.DeduccionesVoluntarias.ToString());
                    csv.WriteField(row.CostoEmpleadorText ?? row.CostoEmpleador.ToString());

                    await csv.NextRecordAsync();
                }

                await writer.FlushAsync();
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Employer Historical Report CSV");
                throw;
            }
        }
    }
}
