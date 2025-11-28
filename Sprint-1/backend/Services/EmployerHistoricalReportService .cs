using backend.Interfaces;
using backend.Models.Payroll;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class EmployerHistoricalReportService : IEmployerHistoricalReportService
    {
        private readonly IEmployerHistoricalReportRepository _repository;
        private readonly ILogger<EmployerHistoricalReportService> _logger;

        public EmployerHistoricalReportService(IEmployerHistoricalReportRepository repository,
            ILogger<EmployerHistoricalReportService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployerHistoricalReportDto>> GenerateReportAsync(long employerId,
            long? companyId, DateTime? startDate, DateTime? endDate)
        {
            var report = (await _repository.GetReportAsync(employerId, companyId, startDate, endDate)).ToList();

            if (!report.Any())
                return report;

            var grossSalaryTotal = report.Sum(r => r.SalarioBruto);
            var socialChargesTotal = report.Sum(r => r.CargasSocialesEmpleador);
            var voluntaryDeductionsTotal = report.Sum(r => r.DeduccionesVoluntarias);
            var employerCostTotal = report.Sum(r => r.CostoEmpleador);

            var totalsRow = new EmployerHistoricalReportDto
            {
                Nombre = string.Empty,
                FrecuenciaPago = string.Empty,
                PeriodoPago = null,
                FechaPago = null,
                SalarioBruto = 0,
                CargasSocialesEmpleador = 0,
                DeduccionesVoluntarias = 0,
                CostoEmpleador = 0,

                SalarioBrutoText = $"Total CRC {grossSalaryTotal}",
                CargasSocialesEmpleadorText = $"Total CRC {socialChargesTotal}",
                DeduccionesVoluntariasText = $"Total CRC {voluntaryDeductionsTotal}",
                CostoEmpleadorText = $"Total CRC {employerCostTotal}"
            };

            // Ensamblado de tabla
            var mappedReport = report.Select(r => new EmployerHistoricalReportDto
            {
                Nombre = r.Nombre,
                FrecuenciaPago = r.FrecuenciaPago,
                PeriodoPago = r.PeriodoPago,
                FechaPago = r.FechaPago,

                SalarioBruto = r.SalarioBruto,
                CargasSocialesEmpleador = r.CargasSocialesEmpleador,
                DeduccionesVoluntarias = r.DeduccionesVoluntarias,
                CostoEmpleador = r.CostoEmpleador,

                SalarioBrutoText = r.SalarioBruto.ToString(),
                CargasSocialesEmpleadorText = r.CargasSocialesEmpleador.ToString(),
                DeduccionesVoluntariasText = r.DeduccionesVoluntarias.ToString(),
                CostoEmpleadorText = r.CostoEmpleador.ToString()
            }).ToList();

            // Last row = totals
            mappedReport.Add(totalsRow);

            return report;
        }
    }
}
