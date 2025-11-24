using backend.Models.Payroll;

namespace backend.Interfaces
{
    public interface IEmployerHistoricalReportRepository
    {
        Task<IEnumerable<EmployerHistoricalReportDto>> GetReportAsync(int employerId, int? companyId, DateTime? startDate, DateTime? endDate);
    }
}
