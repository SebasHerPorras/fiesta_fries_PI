using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Interfaces
{
    public interface IEmployeeBenefitRepository
    {
        Task<List<int>> GetSelectedBenefitIdsAsync(int employeeId);
        Task<bool> CanEmployeeSelectBenefitAsync(int employeeId, int benefitId);
        Task<BeneficioModel?> GetBeneficioByIdAsync(int beneficioId);
        Task<bool> SaveSelectionAsync(EmployeeBenefit entity);
        Task<List<EmployeeBenefit>> GetSelectedByEmployeeIdAsync(int employeeId);
        int CountBeneficiosPorEmpleado(int employeeId);
    }
}