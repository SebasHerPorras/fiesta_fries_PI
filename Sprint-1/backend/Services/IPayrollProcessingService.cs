using backend.Models.Payroll.Requests;
using backend.Models.Payroll.Results;

namespace backend.Interfaces.Services
{
    public interface IPayrollProcessingService
    {
        Task<PayrollProcessResult> ProcessPayrollAsync(PayrollProcessRequest request);
    }
}