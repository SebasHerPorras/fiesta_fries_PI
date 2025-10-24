using backend.Models;
using backend.Repositories;
using backend.Interfaces;

namespace backend.Services
{
    public class EmployerSocialSecurityByPayrollService : IEmployerSocialSecurityByPayrollService
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