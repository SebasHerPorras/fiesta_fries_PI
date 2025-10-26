using backend.Models;

namespace backend.Interfaces
{
    public interface IEmployerSocialSecurityByPayrollService
    {
        void SaveEmployerDeductions(List<EmployerSocialSecurityByPayrollDto> deductions);
    }
}