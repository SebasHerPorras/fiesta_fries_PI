using backend.Models.Payroll;

public interface IPayrollRepository
{
    Task<Payroll?> GetByPeriodAndCompanyAsync(DateTime periodDate, long companyId);
    Task<int> CreatePayrollAsync(Payroll payroll);
    Task UpdatePayrollAsync(Payroll payroll);
    Task<List<PayrollPayment>> CreatePayrollPaymentsAsync(List<PayrollPayment> payments);
    Task<List<PayrollPayment>> GetPayrollDetailsAsync(int payrollId);
    Task<List<Payroll>> GetPayrollsByCompanyAsync(string companyId);
    Task<Payroll?> GetLatestPayrollAsync(string companyId);
}