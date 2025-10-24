using backend.Models;

namespace backend.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetEmployeesByCompanyAsync(long companyId);
        Task<Employee?> GetEmployeeByIdAsync(int employeeId);
    }
}