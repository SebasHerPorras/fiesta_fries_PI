using backend.Models.Payroll.Results;

namespace backend.Interfaces.Services
{
    public interface IPayrollResultBuilder
    {
        PayrollProcessResult CreateSuccessResult(int payrollId, PayrollProcessResult processingResult);
        PayrollProcessResult CreateErrorResult(string message);
    }
}