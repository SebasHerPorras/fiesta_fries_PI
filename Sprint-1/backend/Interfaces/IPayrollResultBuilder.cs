using backend.Models.Payroll.Results;

namespace backend.Interfaces.Services
{
    public interface IPayrollResultBuilder
    {
        PayrollProcessResult CreateSuccessResult(int payrollId, decimal totalAmount, int processedEmployees);
        PayrollProcessResult CreateErrorResult(string message);
        PayrollProcessResult CreatePreviewResult(
           decimal totalAmount,
           int processedEmployees,
           decimal totalGrossSalary,
           decimal totalEmployeeDeductions,
           decimal totalEmployerDeductions,
           decimal totalBenefits,
           decimal totalNetSalary,
           decimal totalEmployerCost);
    }
}