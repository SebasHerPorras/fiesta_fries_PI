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
    public class EmployerByPersonReportCsvService
    {
        private readonly ILogger<EmployerByPersonReportCsvService> _logger;

        public EmployerByPersonReportCsvService(ILogger<EmployerByPersonReportCsvService> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> GenerateCsvAsync(IEnumerable<EmployerByPersonReportDto> report)
        {
            try
            {
                using var ms = new MemoryStream();
                using var writer = new StreamWriter(ms, System.Text.Encoding.UTF8);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                });

                csv.WriteField("Nombre Empresa");
                csv.WriteField("Cédula");
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
                    csv.WriteField(row.Cedula);
                    csv.WriteField(row.PeriodoPago?.ToString("yyyy-MM-dd") ?? string.Empty);
                    csv.WriteField(row.FechaPago?.ToString("yyyy-MM-dd") ?? string.Empty);
                    csv.WriteField(row.SalarioBruto.ToString());
                    csv.WriteField(row.CargasSocialesEmpleador.ToString());
                    csv.WriteField(row.DeduccionesVoluntarias.ToString());
                    csv.WriteField(row.CostoEmpleador.ToString());

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