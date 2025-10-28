using backend.Models;

namespace backend.Interfaces
{
    public interface IEmployeeBenefitService
    {
        Task<List<int>> GetSelectedBenefitIdsAsync(int employeeId);
        Task<bool> SaveSelectionAsync(EmployeeBenefit benefit);
        Task<bool> CanEmployeeSelectBenefitAsync(int employeeId, int benefitId);
        Task<List<EmployeeBenefit>> GetSelectedByEmployeeIdAsync(int employeeId);

    }
}