using backend.Models.Payroll;

namespace backend.Interfaces
{
    public interface IEmployerHistoricalReportRepository
    {
        Task<IEnumerable<EmployerHistoricalReportDto>> GetReportAsync(long employerId, long? companyId, DateTime? startDate, DateTime? endDate);
    }
}
