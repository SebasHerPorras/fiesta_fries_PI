using backend.Models;
using backend.Repositories;
using backend.Interfaces;

namespace backend.Services
{
    public class EmployeeDeductionsByPayrollService : IEmployeeDeductionsByPayrollService
    {
        private readonly EmployeeDeductionsByPayrollRepository _repository;

        public EmployeeDeductionsByPayrollService()
        {
            _repository = new EmployeeDeductionsByPayrollRepository();
        }

        public void SaveEmployeeDeductions(List<EmployeeDeductionsByPayrollDto> deductions)
        {
            try
            {
                if (deductions == null || !deductions.Any())
                    throw new ArgumentException("La lista de deducciones no puede estar vacía");

                _repository.SaveEmployeeDeductions(deductions);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar las deducciones de empleado: {ex.Message}", ex);
            }
        }
    }
}