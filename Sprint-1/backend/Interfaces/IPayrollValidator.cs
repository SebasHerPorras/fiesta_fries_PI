using backend.Models.Payroll.Requests;
using backend.Models.Payroll.Results;

namespace backend.Interfaces.Services
{
    public interface IPayrollValidator
    {
        Task<PayrollValidationResult> ValidateAsync(PayrollProcessRequest request);
    }
}