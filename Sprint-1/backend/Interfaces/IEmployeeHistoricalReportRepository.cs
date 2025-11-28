using backend.Models.Payroll;

namespace backend.Interfaces
{
    public interface IEmployeeHistoricalReportRepository
    {
        Task<IEnumerable<EmployeeHistoricalReportDto>> GetReportAsync(
            long employeeId,
            DateTime? startDate,
            DateTime? endDate);
    }
}
