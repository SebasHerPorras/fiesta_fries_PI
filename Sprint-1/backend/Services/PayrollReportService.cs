using backend.Repositories;
using Microsoft.Extensions.Logging;
using backend.Models.Payroll.Results;

namespace backend.Services
{
    public class PayrollReportService
    {
        private readonly PayrollReportRepository _repository;
        private readonly ILogger<PayrollReportService> _logger;

        public PayrollReportService(PayrollReportRepository repository, ILogger<PayrollReportService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public Task<List<TopEmployeeDto>> GetTop12EmployeesByCompanyAsync(long companyId)
        {
            _logger.LogDebug("Solicitando Top12 empleados para empresa {CompanyId}", companyId);
            return _repository.GetTop12EmployeesByCompanyAsync(companyId);
        }

        public async Task<List<EmployeeLastPaymentsResult>> GetLast12PaymentsByEmployeeAsync(int employeeId)
        {
            return await _repository.GetLast12PaymentsByEmployeeAsync(employeeId);
        }
    }
}