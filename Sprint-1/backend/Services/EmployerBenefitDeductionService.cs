using backend.Models;
using backend.Interfaces;
using backend.Repositories;

namespace backend.Services
{
    public class EmployerBenefitDeductionService : IEmployerBenefitDeductionService
    {
        private readonly EmployerBenefitDeductionRepository _repository;

        public EmployerBenefitDeductionService()
        {
            _repository = new EmployerBenefitDeductionRepository();
        }

        public void SaveEmployerBenefitDeductions(List<EmployerBenefitDeductionDto> deductions)
        {
            try
            {
                if (deductions == null || !deductions.Any())
                    throw new ArgumentException("La lista de deducciones no puede estar vacía");

                _repository.SaveEmployerBenefitDeductions(deductions);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar deducciones de beneficios: {ex.Message}", ex);
            }
        }
    }
}