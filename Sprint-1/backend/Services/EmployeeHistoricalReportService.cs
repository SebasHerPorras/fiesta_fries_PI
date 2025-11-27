using backend.Interfaces;
using backend.Models.Payroll;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class EmployeeHistoricalReportService : IEmployeeHistoricalReportService
    {
        private readonly IEmployeeHistoricalReportRepository _repository;
        private readonly ILogger<EmployeeHistoricalReportService> _logger;

        public EmployeeHistoricalReportService(
            IEmployeeHistoricalReportRepository repository,
            ILogger<EmployeeHistoricalReportService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployeeHistoricalReportDto>> GenerateReportAsync(
            long employeeId,
            DateTime? startDate,
            DateTime? endDate)
        {
            var report = (await _repository.GetReportAsync(employeeId, startDate, endDate)).ToList();

            if (!report.Any())
                return report;

            var totalSalary = report.Sum(r => r.Salary);
            var totalDeductions = report.Sum(r => r.DeductionsAmount);
            var totalBenefits = report.Sum(r => r.BenefitsAmount);
            var totalNetSalary = report.Sum(r => r.NetSalary);

            var totalsRow = new EmployeeHistoricalReportDto
            {
                EmploymentType = string.Empty,
                Position = string.Empty,
                PaymentDate = null,

                Salary = totalSalary,
                DeductionsAmount = totalDeductions,
                BenefitsAmount = totalBenefits,
                NetSalary = totalNetSalary
            };

            report.Add(totalsRow);
            return report;
        }
    }
}
