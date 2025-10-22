using backend.Models;
using backend.Repositories;

namespace backend.Services
{
    public class EmployerSocialSecurityByPayrollService
    {
        private readonly EmployerSocialSecurityByPayrollRepository _repository;

        public EmployerSocialSecurityByPayrollService()
        {
            _repository = new EmployerSocialSecurityByPayrollRepository();
        }

        public void SaveEmployerDeductions(List<EmployerSocialSecurityByPayrollDto> deductions)
        {
            if (deductions == null || !deductions.Any())
                throw new ArgumentException("La lista de deducciones no puede estar vacía");

            _repository.InsertDeductions(deductions);
        }
    }
}