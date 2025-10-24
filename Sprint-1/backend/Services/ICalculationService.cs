namespace backend.Interfaces.Services
{
    public interface ICalculationService
    {
        Task<decimal> CalculateDeductionsAsync(int employeeId, long companyId, int payrollId);
        Task<decimal> CalculateBenefitsAsync(int employeeId, long companyId, int payrollId);
        Task<decimal> CalculateIncomeTaxAsync(int employeeId, long companyId, int payrollId);
    }
}