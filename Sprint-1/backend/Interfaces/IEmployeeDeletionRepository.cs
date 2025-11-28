using System.Threading.Tasks;
using backend.Models;

namespace backend.Interfaces
{
    public interface IEmployeeDeletionRepository
    {
        Task<bool> ValidateEmployeeExistsAsync(int personaId, long companyId);
        Task<bool> IsCompanyOwnerAsync(int personaId);
        Task<EmployeePayrollStatus> CheckPayrollStatusAsync(int personaId);
        Task<bool> ExecuteLogicalDeleteAsync(int personaId);
        Task<bool> ExecutePhysicalDeleteAsync(int personaId);
    }
}