using backend.Models.Payroll;

namespace backend.Interfaces
{
    public interface IEmployerHistoricalReportService
    {
        Task<IEnumerable<EmployerHistoricalReportDto>> GenerateReportAsync(int employerId, int? companyId, DateTime? startDate, DateTime? endDate);
    }

}
