
using backend.Repositories;
using Microsoft.Extensions.Logging;
using backend.Repositories;

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
    }
}