using backend.Models.Payroll;
using backend.Models.Payroll.Requests;
using backend.Models.Payroll.Results;
using System.Collections.Generic;
using System.Threading.Tasks;
using static backend.Controllers.PayrollController;

namespace backend.Interfaces.Services
{
    public interface IPayrollProcessingService
    {
        Task<PayrollProcessResult> ProcessPayrollAsync(PayrollProcessRequest request);
        Task<List<PayrollSummaryResult>> GetPayrollsByCompanyAsync(string companyId);
        Task<PayrollPeriod> GetNextPayrollPeriodAsync(string companyId);
        Task<List<PayrollPeriod>> GetPendingPeriodsAsync(string companyId, int months = 6);
        Task<List<PayrollPeriod>> GetOverduePeriodsAsync(string companyId);
        Task<PayrollProcessResult> PreviewPayrollAsync(PayrollPreviewRequest request);
    }
}