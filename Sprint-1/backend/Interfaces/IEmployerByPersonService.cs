using backend.Models.Payroll;

namespace backend.Interfaces
{
    public interface IEmployerByPersonService
    {
        Task<IEnumerable<EmployerByPersonReportDto>> GetReportAsync(long employerId, DateTime? startDate, DateTime? endDate, String? employmentType, long? companyId, int? cedula);
    }
}
