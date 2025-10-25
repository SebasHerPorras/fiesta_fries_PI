namespace backend.Interfaces.Services
{
    public interface ICalculationService
    {
        Task<decimal> CalculateDeductionsAsync(EmployeeCalculationDto empleado, long companyId, int payrollId);
        Task<decimal> CalculateBenefitsAsync(EmployeeCalculationDto empleado, long companyId, int payrollId);
        Task<decimal> CalculateIncomeTaxAsync(EmployeeCalculationDto empleado, long companyId, int payrollId);
    }
}