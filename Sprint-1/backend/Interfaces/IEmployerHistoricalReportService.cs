using backend.Models.Payroll;

namespace backend.Interfaces
{
    public interface IEmployerHistoricalReportService
    {
        Task<IEnumerable<EmployerHistoricalReportDto>> GenerateReportAsync(long employerId, long? companyId, DateTime? startDate, DateTime? endDate);
    }

}
