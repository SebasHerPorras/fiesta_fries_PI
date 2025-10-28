using backend.Models;

namespace backend.Interfaces
{
    public interface ICalculatorBenefitsService
    {
        Task<decimal> CalculateBenefitsAsync(EmployeeCalculationDto employee, int reportId, long cedulaJuridicaEmpresa);
        List<BenefitDto> GetBenefitsList(long cedulaJuridicaEmpresa);
    }
}