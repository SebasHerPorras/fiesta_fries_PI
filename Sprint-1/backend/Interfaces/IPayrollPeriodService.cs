using backend.Models.Payroll;

namespace backend.Interfaces.Services
{
    public interface IPayrollPeriodService
    {
        Task<PayrollPeriod> CalculateNextPeriodAsync(string companyId);
        Task<PayrollPeriod> CalculateNextPendingPeriodAsync(string companyId); 
        Task<List<PayrollPeriod>> GetOverduePeriodsAsync(string companyId); 
        Task<PayrollPeriod> CalculateCurrentPeriodAsync(string companyId);
        Task<List<PayrollPeriod>> GetPendingPeriodsAsync(string companyId, int months = 6);
        Task<bool> IsPeriodProcessedAsync(string companyId, DateTime period);
    }
}