using backend.Interfaces;
using backend.Models.Payroll;

namespace backend.Services
{
    public class EmployerByPersonReportService : IEmployerByPersonReportService
    {
        private readonly IEmployerByPersonReportRepository _repository;
        private readonly ILogger<EmployerByPersonReportService> _logger;

        public EmployerByPersonReportService(IEmployerByPersonReportRepository repository,
            ILogger<EmployerByPersonReportService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployerByPersonReportDto>> GetReportAsync(long employerId, DateTime? startDate,
            DateTime? endDate, String? employmentType, long? companyId, int? cedula)
        {
            var report = (await _repository.GetReportAsync( employerId,  startDate,
             endDate,  employmentType,  companyId,  cedula)).ToList();

            if (!report.Any())
                return report;

            var grossSalaryTotal = report.Sum(r => r.SalarioBruto);
            var socialChargesTotal = report.Sum(r => r.CargasSocialesEmpleador);
            var voluntaryDeductionsTotal = report.Sum(r => r.DeduccionesVoluntarias);
            var employerCostTotal = report.Sum(r => r.CostoEmpleador);

            var totalsRow = new EmployerByPersonReportDto
            {
                Nombre = string.Empty,
                Cedula = 0,
                PeriodoPago = null,
                FechaPago = null,
                SalarioBruto = grossSalaryTotal,
                CargasSocialesEmpleador = socialChargesTotal,
                DeduccionesVoluntarias = voluntaryDeductionsTotal,
                CostoEmpleador = employerCostTotal,
            };

            // Ensamblado de tabla
            var mappedReport = report.Select(r => new EmployerByPersonReportDto
            {
                Nombre = r.Nombre,
                Cedula = r.Cedula,
                PeriodoPago = r.PeriodoPago,
                FechaPago = r.FechaPago,

                SalarioBruto = r.SalarioBruto,
                CargasSocialesEmpleador = r.CargasSocialesEmpleador,
                DeduccionesVoluntarias = r.DeduccionesVoluntarias,
                CostoEmpleador = r.CostoEmpleador,
            }).ToList();

            // Last row = totals
            mappedReport.Add(totalsRow);

            return report;
        }
    }

}

