using backend.Models;

namespace backend.Interfaces
{
    public interface IEmployeeDeductionsByPayrollService
    {
        void SaveEmployeeDeductions(List<EmployeeDeductionsByPayrollDto> deductions);
    }
}