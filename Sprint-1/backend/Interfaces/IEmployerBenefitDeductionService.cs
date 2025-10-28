using backend.Models;

namespace backend.Interfaces
{
    public interface IEmployerBenefitDeductionService
    {
        void SaveEmployerBenefitDeductions(List<EmployerBenefitDeductionDto> deductions);
    }
}