using backend.Models.Payroll.Results;

namespace backend.Interfaces.Services
{
    public interface IPayrollResultBuilder
    {
        PayrollProcessResult CreateSuccessResult(int payrollId, decimal totalAmount, int processedEmployees);
        PayrollProcessResult CreateErrorResult(string message);
    }
}