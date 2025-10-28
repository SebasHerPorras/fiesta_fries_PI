using backend.Models.Payroll.Requests;
using backend.Models.Payroll.Results;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Interfaces.Services
{
    public interface IPayrollProcessingService
    {
        Task<PayrollProcessResult> ProcessPayrollAsync(PayrollProcessRequest request);
        Task<List<PayrollSummaryResult>> GetPayrollsByCompanyAsync(string companyId);
    }
}