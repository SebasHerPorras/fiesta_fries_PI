using System.Threading.Tasks;
using backend.Models;

namespace backend.Interfaces
{
    public interface IEmployeeDeletionService
    {
        Task<EmployeeDeletionResult> DeleteEmpleadoAsync(int personaId, long companyId);
        Task<EmployeePayrollStatus> CheckPayrollStatusAsync(int personaId);
    }
}